using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AdvancedAuthSystem.Data;
using AdvancedAuthSystem.Services;
using AdvancedAuthSystem.Authorization.Policies;
using AdvancedAuthSystem.Authorization.Requirements;
using AdvancedAuthSystem.Authorization.Handlers;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AdvancedAuthDb"));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddHttpContextAccessor();

// Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization Policies
builder.Services.AddAuthorizationBuilder()
    // Age-based policies
    .AddPolicy(PolicyNames.MinimumAge18, policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)))
    .AddPolicy(PolicyNames.MinimumAge21, policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(21)))
    
    // Department policies
    .AddPolicy(PolicyNames.ITDepartment, policy =>
        policy.Requirements.Add(new DepartmentRequirement("IT")))
    .AddPolicy(PolicyNames.SalesDepartment, policy =>
        policy.Requirements.Add(new DepartmentRequirement("Sales")))
    .AddPolicy(PolicyNames.HRDepartment, policy =>
        policy.Requirements.Add(new DepartmentRequirement("HR")))
    
    // Level policies
    .AddPolicy(PolicyNames.SeniorLevel, policy =>
        policy.Requirements.Add(new ClaimRequirement("Level", "Senior")))
    .AddPolicy(PolicyNames.JuniorLevel, policy =>
        policy.Requirements.Add(new ClaimRequirement("Level", "Junior")))
    
    // Time-based policies
    .AddPolicy(PolicyNames.WorkingHours, policy =>
        policy.Requirements.Add(new TimeBasedRequirement(
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0))))
    .AddPolicy(PolicyNames.WeekdaysOnly, policy =>
        policy.Requirements.Add(new TimeBasedRequirement(
            new TimeSpan(0, 0, 0),
            new TimeSpan(23, 59, 59),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })))
    
    // Resource policies
    .AddPolicy(PolicyNames.ResourceOwner, policy =>
        policy.Requirements.Add(new ResourceOwnerRequirement()))
    
    // Combined policies
    .AddPolicy(PolicyNames.AdminOrManager, policy =>
        policy.RequireRole("Admin", "Manager"))
    .AddPolicy(PolicyNames.CanManageUsers, policy =>
        policy.Requirements.Add(new PermissionRequirement("users.write")));

// Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, MinimumAgeHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DepartmentHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ResourceOwnerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TimeBasedHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ClaimHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Advanced Auth System API",
        Version = "v1",
        Description = "Sistema completo de autenticação e autorização com JWT, 2FA, RBAC e ABAC"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Advanced Auth System API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
