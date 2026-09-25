using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AdsConnect.api.Utils
{
    /// <summary>
    /// The resolved JWT configuration, built once in Program.cs and registered as a
    /// singleton so the JwtBearer validation parameters and <see cref="JwtService"/>
    /// are guaranteed to agree on the signing key.
    /// </summary>
    public sealed class JwtSettings
    {
        /// <summary>HMAC-SHA256 needs at least 256 bits of key material.</summary>
        private const int MinimumKeyLength = 32;

        private JwtSettings(string key, string issuer, string audience, TimeSpan lifetime)
        {
            SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            Issuer = issuer;
            Audience = audience;
            Lifetime = lifetime;
        }

        public SymmetricSecurityKey SigningKey { get; }

        public string Issuer { get; }

        public string Audience { get; }

        public TimeSpan Lifetime { get; }

        /// <summary>
        /// Reads Jwt:Key / Jwt:Issuer / Jwt:Audience from configuration.
        ///
        /// appsettings.json declares Jwt:Key but leaves it EMPTY on purpose - that file
        /// is committed and a signing key is a credential. Supply it through user-secrets
        /// (<c>dotnet user-secrets set "Jwt:Key" "..."</c>) or the environment
        /// (<c>Jwt__Key</c>).
        ///
        /// Outside Development a missing key is fatal. In Development it falls back to a
        /// random per-process key, which keeps `bun run api` working with nothing to
        /// configure at the cost of invalidating every issued token when the API
        /// restarts - you simply sign in again.
        /// </summary>
        public static JwtSettings Resolve(IConfiguration configuration, IHostEnvironment environment, ILogger logger)
        {
            var key = configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key))
            {
                if (!environment.IsDevelopment())
                {
                    throw new InvalidOperationException(string.Join(Environment.NewLine,
                        "'Jwt:Key' is not configured, so access tokens cannot be signed.",
                        "",
                        @"CLI: dotnet user-secrets set ""Jwt:Key"" ""<at least 32 random characters>""",
                        "Environment: Jwt__Key=<at least 32 random characters>",
                        "",
                        "Do not put the key in appsettings.json - that file is committed."));
                }

                key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                logger.LogWarning(
                    "'Jwt:Key' is not configured. Signing development tokens with a random key " +
                    "generated for this process - every token becomes invalid when the API restarts. " +
                    "Set it with: dotnet user-secrets set \"Jwt:Key\" \"<at least 32 random characters>\"");
            }
            else if (key.Length < MinimumKeyLength)
            {
                throw new InvalidOperationException(
                    $"'Jwt:Key' is {key.Length} characters; HMAC-SHA256 needs at least {MinimumKeyLength}.");
            }

            return new JwtSettings(
                key,
                configuration["Jwt:Issuer"] ?? "http://localhost:5036/",
                configuration["Jwt:Audience"] ?? "http://localhost:5036/",
                TimeSpan.FromHours(configuration.GetValue("Jwt:TokenExpiryTimeInHour", 24)));
        }
    }
}
