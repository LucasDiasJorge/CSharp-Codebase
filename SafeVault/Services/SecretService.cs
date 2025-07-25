using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Models.DTOs;
using SafeVault.Security;

namespace SafeVault.Services;

public interface ISecretService
{
    Task<SecretResponseDto?> GetSecretByIdAsync(int id, int userId);
    Task<IEnumerable<SecretResponseDto>> GetAllSecretsByUserIdAsync(int userId);
    Task<(bool Success, int? SecretId)> CreateSecretAsync(SecretCreateDto createDto, int userId);
    Task<bool> UpdateSecretAsync(int id, SecretUpdateDto updateDto, int userId);
    Task<bool> DeleteSecretAsync(int id, int userId);
}

public class SecretService : ISecretService
{
    private readonly ISecretRepository _secretRepository;
    private readonly IInputValidator _inputValidator;
    
    public SecretService(ISecretRepository secretRepository, IInputValidator inputValidator)
    {
        _secretRepository = secretRepository;
        _inputValidator = inputValidator;
    }

    public async Task<SecretResponseDto?> GetSecretByIdAsync(int id, int userId)
    {
        var secret = await _secretRepository.GetByIdAsync(id, userId);
        
        if (secret == null)
            return null;
        
        // Map to DTO to control what data is exposed
        return new SecretResponseDto
        {
            Id = secret.Id,
            Title = secret.Title,
            Content = secret.Content,
            CreatedAt = secret.CreatedAt,
            UpdatedAt = secret.UpdatedAt
        };
    }

    public async Task<IEnumerable<SecretResponseDto>> GetAllSecretsByUserIdAsync(int userId)
    {
        var secrets = await _secretRepository.GetAllByUserIdAsync(userId);
        
        // Map to DTOs
        return secrets.Select(s => new SecretResponseDto
        {
            Id = s.Id,
            Title = s.Title,
            Content = s.Content,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        });
    }

    public async Task<(bool Success, int? SecretId)> CreateSecretAsync(SecretCreateDto createDto, int userId)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(createDto.Title) || string.IsNullOrWhiteSpace(createDto.Content))
        {
            return (false, null);
        }
        
        // Check for potential SQL injection
        if (_inputValidator.ContainsSqlInjection(createDto.Title) || _inputValidator.ContainsSqlInjection(createDto.Content))
        {
            return (false, null);
        }
        
        // Sanitize HTML to prevent XSS
        var title = _inputValidator.SanitizeHtml(createDto.Title);
        var content = _inputValidator.SanitizeHtml(createDto.Content);
        
        // Create new secret
        var secret = new Secret
        {
            UserId = userId,
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var id = await _secretRepository.CreateAsync(secret);
        
        return (id > 0, id);
    }

    public async Task<bool> UpdateSecretAsync(int id, SecretUpdateDto updateDto, int userId)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(updateDto.Title) || string.IsNullOrWhiteSpace(updateDto.Content))
        {
            return false;
        }
        
        // Check for potential SQL injection
        if (_inputValidator.ContainsSqlInjection(updateDto.Title) || _inputValidator.ContainsSqlInjection(updateDto.Content))
        {
            return false;
        }
        
        // First verify secret exists and belongs to user
        var existingSecret = await _secretRepository.GetByIdAsync(id, userId);
        if (existingSecret == null)
        {
            return false;
        }
        
        // Sanitize HTML to prevent XSS
        existingSecret.Title = _inputValidator.SanitizeHtml(updateDto.Title);
        existingSecret.Content = _inputValidator.SanitizeHtml(updateDto.Content);
        existingSecret.UpdatedAt = DateTime.UtcNow;
        
        return await _secretRepository.UpdateAsync(existingSecret);
    }

    public async Task<bool> DeleteSecretAsync(int id, int userId)
    {
        // Verify secret exists and belongs to user before attempting delete
        var existingSecret = await _secretRepository.GetByIdAsync(id, userId);
        if (existingSecret == null)
        {
            return false;
        }
        
        return await _secretRepository.DeleteAsync(id, userId);
    }
}
