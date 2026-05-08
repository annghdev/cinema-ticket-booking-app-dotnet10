using System.Text;
using System.Linq;
using CinemaTicketBooking.Application;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Common.Auth;
using Microsoft.AspNetCore.Http;
using CinemaTicketBooking.Infrastructure.Notifications;
using CinemaTicketBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CinemaTicketBooking.Infrastructure.Auth;

/// <summary>
/// Registers Identity, JWT, external login, refresh tokens, and user context.
/// </summary>
public static class AuthDependencyInjection
{
    /// <summary>
    /// Adds authentication and authorization primitives for the cinema API.
    /// </summary>
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));
        services.Configure<TestAuthOptions>(configuration.GetSection(TestAuthOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext>(sp =>
        {
            var accessor = sp.GetRequiredService<IHttpContextAccessor>();
            return accessor.HttpContext is null
                ? new SystemUserContext()
                : new HttpUserContext(accessor);
        });

        services.AddScoped<IAccountCustomerLinker, AccountCustomerLinker>();
        services.AddScoped<IIdentityAuthService, IdentityAuthService>();
        services.AddScoped<IEmailSender, LogEmailSender>();

        services
            .AddIdentity<Account, Role>(options =>
            {
                var accountOptions = configuration.GetSection(AccountOptions.SectionName).Get<AccountOptions>();
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = accountOptions.RequiredLength;
                options.Password.RequireDigit = accountOptions.RequireDigit;
                options.Password.RequireLowercase = accountOptions.RequireLowercase;
                options.Password.RequireUppercase = accountOptions.RequireUppercase;
                options.Password.RequireNonAlphanumeric = accountOptions.RequireNonAlphanumeric;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.LoginPath = "/Auth/Login";
            options.AccessDeniedPath = "/Auth/AccessDenied";
        });

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

        var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        if (!string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
                options.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        if (!string.IsNullOrWhiteSpace(configuration["Authentication:Facebook:AppId"]))
        {
            authBuilder.AddFacebook(options =>
            {
                options.AppId = configuration["Authentication:Facebook:AppId"]!;
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"]!;
                options.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        services.AddAuthorization(options =>
        {
            var permissionFields = typeof(Permissions)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string));

            foreach (var field in permissionFields)
            {
                var permissionValue = (string)field.GetValue(null)!;
                options.AddPolicy(permissionValue, p => 
                {
                    p.RequireAssertion(context => 
                        context.User.IsInRole(RoleNames.SystemAdmin) ||
                        context.User.HasClaim(c => c.Type == AuthClaimTypes.Permission && c.Value == permissionValue));
                });
            }
        });

        return services;
    }
}
