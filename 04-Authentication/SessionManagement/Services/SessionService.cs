using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using SessionManagement.Data;
using SessionManagement.DTOs;
using SessionManagement.Models;

namespace SessionManagement.Services;

public interface ISessionService
{
    Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, AuthResponse? Response, string? Error)> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent);
    Task<(bool Success, AuthResponse? Response, string? Error)> RefreshAsync(string refreshToken, string? ipAddress);
    Task<bool> RevokeSessionAsync(Guid sessionId, int requestingUserId, string reason);
    Task RevokeAllSessionsAsync(int userId, string reason, Guid? exceptSessionId = null);
    Task<List<SessionInfo>> GetActiveSessionsAsync(int userId, Guid currentSessionId);
    Task<bool> ValidateSessionAsync(Guid sessionId);
    Task UpdateLastActivityAsync(Guid sessionId);
}

public class SessionService : ISessionService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IDatabase _redis;
    private readonly int _maxSessionsPerUser;
    private readonly int _refreshTokenExpiryDays;
    private readonly int _accessTokenExpiryMinutes;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    // ── Redis key helpers ───────────────────────────────────────────────────
    private static string SessionKey(Guid sessionId) => $"session:{sessionId}";
    private static string UserSessionsKey(int userId) => $"user_sessions:{userId}";
    private static string RefreshKey(string tokenHash) => $"refresh:{tokenHash}";

    public SessionService(AppDbContext db, ITokenService tokenService, IConfiguration configuration, IConnectionMultiplexer redis)
    {
        _db = db;
        _tokenService = tokenService;
        _redis = redis.GetDatabase();
        _maxSessionsPerUser = int.Parse(configuration["Session:MaxSessionsPerUser"] ?? "5");
        _refreshTokenExpiryDays = int.Parse(configuration["Session:RefreshTokenExpiryDays"] ?? "7");
        _accessTokenExpiryMinutes = int.Parse(configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15");
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private async Task SaveSessionAsync(Session session)
    {
        string json = JsonSerializer.Serialize(session, JsonOptions);
        TimeSpan ttl = session.ExpiresAt - DateTime.UtcNow;
        if (ttl > TimeSpan.Zero)
            await _redis.StringSetAsync(SessionKey(session.Id), json, ttl);
    }

    private async Task<Session?> GetSessionAsync(Guid sessionId)
    {
        RedisValue value = await _redis.StringGetAsync(SessionKey(sessionId));
        if (!value.HasValue)
            return null;
        return JsonSerializer.Deserialize<Session>(value.ToString(), JsonOptions);
    }

    // ── Public operations ───────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return (false, "Username already taken");

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return (false, "Email already registered");

        _db.Users.Add(new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        });

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> LoginAsync(
        LoginRequest request, string? ipAddress, string? userAgent)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (false, null, "Invalid credentials");

        if (!user.IsActive)
            return (false, null, "Account is inactive");

        // Enforce max concurrent sessions — revoke the oldest when the limit is reached
        RedisValue[] existingIds = await _redis.SetMembersAsync(UserSessionsKey(user.Id));
        List<Session> activeSessions = new List<Session>();

        foreach (RedisValue sid in existingIds)
        {
            if (Guid.TryParse(sid.ToString(), out Guid parsedId))
            {
                Session? s = await GetSessionAsync(parsedId);
                if (s is not null && !s.IsRevoked)
                    activeSessions.Add(s);
            }
        }

        activeSessions.Sort((a, b) => a.LastActivityAt.CompareTo(b.LastActivityAt));

        if (activeSessions.Count >= _maxSessionsPerUser)
        {
            Session oldest = activeSessions[0];
            oldest.IsRevoked = true;
            oldest.RevokedAt = DateTime.UtcNow;
            oldest.RevokedReason = "Exceeded maximum concurrent session limit";
            await SaveSessionAsync(oldest);
            await _redis.KeyDeleteAsync(RefreshKey(oldest.RefreshTokenHash));
        }

        string refreshToken = _tokenService.GenerateRefreshToken();
        string refreshHash = _tokenService.HashToken(refreshToken);
        Guid sessionId = Guid.NewGuid();
        DateTime expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        Session session = new Session
        {
            Id = sessionId,
            UserId = user.Id,
            RefreshTokenHash = refreshHash,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            DeviceName = request.DeviceName,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            LastActivityAt = DateTime.UtcNow
        };

        await SaveSessionAsync(session);
        await _redis.SetAddAsync(UserSessionsKey(user.Id), sessionId.ToString());
        await _redis.StringSetAsync(RefreshKey(refreshHash), sessionId.ToString(), TimeSpan.FromDays(_refreshTokenExpiryDays));

        string accessToken = _tokenService.GenerateAccessToken(user, sessionId);

        return (true, new AuthResponse(
            SessionId: sessionId,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresIn: _accessTokenExpiryMinutes * 60,
            SessionExpiresAt: expiresAt
        ), null);
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> RefreshAsync(
        string refreshToken, string? ipAddress)
    {
        string hash = _tokenService.HashToken(refreshToken);
        RedisValue sessionIdValue = await _redis.StringGetAsync(RefreshKey(hash));

        if (!sessionIdValue.HasValue || !Guid.TryParse(sessionIdValue.ToString(), out Guid sessionId))
            return (false, null, "Invalid refresh token");

        Session? session = await GetSessionAsync(sessionId);
        if (session is null)
            return (false, null, "Session has expired");

        if (session.IsRevoked)
            return (false, null, "Session has been revoked");

        User? user = await _db.Users.FindAsync(session.UserId);
        if (user is null || !user.IsActive)
            return (false, null, "Account is inactive");

        // Rotate refresh token on every use — prevents replay attacks
        string newRefreshToken = _tokenService.GenerateRefreshToken();
        string newHash = _tokenService.HashToken(newRefreshToken);

        await _redis.KeyDeleteAsync(RefreshKey(hash));

        session.RefreshTokenHash = newHash;
        session.LastActivityAt = DateTime.UtcNow;
        if (ipAddress is not null)
            session.IPAddress = ipAddress;

        await SaveSessionAsync(session);

        TimeSpan remaining = session.ExpiresAt - DateTime.UtcNow;
        await _redis.StringSetAsync(RefreshKey(newHash), sessionId.ToString(), remaining > TimeSpan.Zero ? remaining : TimeSpan.FromSeconds(1));

        string accessToken = _tokenService.GenerateAccessToken(user, sessionId);

        return (true, new AuthResponse(
            SessionId: sessionId,
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            ExpiresIn: _accessTokenExpiryMinutes * 60,
            SessionExpiresAt: session.ExpiresAt
        ), null);
    }

    public async Task<bool> RevokeSessionAsync(Guid sessionId, int requestingUserId, string reason)
    {
        Session? session = await GetSessionAsync(sessionId);

        if (session is null || session.UserId != requestingUserId)
            return false;

        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedReason = reason;

        await SaveSessionAsync(session);
        await _redis.KeyDeleteAsync(RefreshKey(session.RefreshTokenHash));
        return true;
    }

    public async Task RevokeAllSessionsAsync(int userId, string reason, Guid? exceptSessionId = null)
    {
        RedisValue[] sessionIds = await _redis.SetMembersAsync(UserSessionsKey(userId));

        foreach (RedisValue sid in sessionIds)
        {
            if (!Guid.TryParse(sid.ToString(), out Guid parsedId))
                continue;

            if (exceptSessionId.HasValue && parsedId == exceptSessionId.Value)
                continue;

            Session? session = await GetSessionAsync(parsedId);
            if (session is null || session.IsRevoked)
                continue;

            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = reason;

            await SaveSessionAsync(session);
            await _redis.KeyDeleteAsync(RefreshKey(session.RefreshTokenHash));
        }
    }

    public async Task<List<SessionInfo>> GetActiveSessionsAsync(int userId, Guid currentSessionId)
    {
        RedisValue[] sessionIds = await _redis.SetMembersAsync(UserSessionsKey(userId));
        List<SessionInfo> result = new List<SessionInfo>();

        foreach (RedisValue sid in sessionIds)
        {
            if (!Guid.TryParse(sid.ToString(), out Guid parsedId))
                continue;

            Session? session = await GetSessionAsync(parsedId);
            if (session is null || session.IsRevoked)
                continue;

            result.Add(new SessionInfo(
                SessionId: session.Id,
                DeviceName: session.DeviceName,
                IPAddress: session.IPAddress,
                UserAgent: session.UserAgent,
                CreatedAt: session.CreatedAt,
                LastActivityAt: session.LastActivityAt,
                ExpiresAt: session.ExpiresAt,
                IsCurrent: session.Id == currentSessionId
            ));
        }

        result.Sort((a, b) => b.LastActivityAt.CompareTo(a.LastActivityAt));
        return result;
    }

    public async Task<bool> ValidateSessionAsync(Guid sessionId)
    {
        Session? session = await GetSessionAsync(sessionId);
        return session is not null && !session.IsRevoked;
    }

    public async Task UpdateLastActivityAsync(Guid sessionId)
    {
        Session? session = await GetSessionAsync(sessionId);
        if (session is not null)
        {
            session.LastActivityAt = DateTime.UtcNow;
            await SaveSessionAsync(session);
        }
    }
}
