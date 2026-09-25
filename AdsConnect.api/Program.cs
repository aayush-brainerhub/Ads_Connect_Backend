using AdsConnect.api.Utils;
using AdsConnect.data;
using AdsConnect.data.Interface;
using AdsConnect.data.MappingClass;
using AdsConnect.data.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger needs the bearer scheme declared before its "Authorize" button appears,
// otherwise every protected endpoint is untestable from /swagger.
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// appsettings.json declares the key but leaves the value EMPTY on purpose - that
// file is committed. The real credential comes from user-secrets (Visual Studio:
// right click AdsConnect.api > Manage User Secrets) or from the environment.
var connectionString = builder.Configuration.GetConnectionString("PostgresConnectionString");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(string.Join(Environment.NewLine,
        "Connection string 'PostgresConnectionString' is not configured.",
        "",
        "Visual Studio: right click AdsConnect.api > Manage User Secrets, then add:",
        @"  { ""ConnectionStrings"": { ""PostgresConnectionString"": ""Host=localhost;Port=5432;Database=Ads_Connect;Username=postgres;Password=..."" } }",
        "",
        @"CLI: dotnet user-secrets set ""ConnectionStrings:PostgresConnectionString"" ""...""",
        "",
        "Or start the API with `bun run api` from the frontend, which supplies it from .env.",
        "Do not put the password in appsettings.json - that file is committed."));
}

builder.Services.AddDbContext<AdsConnectContext>(options => options
    .UseNpgsql(connectionString)
    // Every endpoint is read-only today; skipping change tracking avoids
    // building a snapshot per row.
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddTransient<IReferenceService, ReferenceService>();
builder.Services.AddTransient<IProviderService, ProviderService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IProfileService, ProfileService>();
builder.Services.AddTransient<ICampaignService, CampaignService>();
builder.Services.AddTransient<IRequestService, RequestService>();
builder.Services.AddTransient<IInventoryService, InventoryService>();
builder.Services.AddTransient<IMessagingService, MessagingService>();
builder.Services.AddTransient<IAdminService, AdminService>();
builder.Services.AddTransient<IFinanceService, FinanceService>();
builder.Services.AddTransient<IBookingService, BookingService>();
builder.Services.AddTransient<IReviewService, ReviewService>();
builder.Services.AddTransient<IAuditService, AuditService>();

// Resolved once so the JwtBearer validation parameters below and JwtService are
// provably using the same signing key. See JwtSettings.Resolve for where the key
// comes from - it is never read from the committed appsettings.json.
var jwtSettings = JwtSettings.Resolve(
    builder.Configuration,
    builder.Environment,
    LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger("Jwt"));

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ITokenService>(provider => provider.GetRequiredService<JwtService>());
builder.Services.AddSingleton<ProfileImageStore>();

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = jwtSettings.SigningKey,
        // The default five-minute grace period would keep expired tokens working
        // long past the lifetime the token itself advertises.
        ClockSkew = TimeSpan.Zero,
    };
});

const string FrontendCors = "frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:8080" };

builder.Services.AddCors(options => options.AddPolicy(
    FrontendCors,
    policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Only redirect where a TLS endpoint actually exists; the dev "http" profile
    // has none, and redirecting there would break the frontend's API calls.
    app.UseHttpsRedirection();
}

// Serves uploaded avatars out of wwwroot. This is the one thing the browser
// fetches from the API directly - every data call still goes through the
// frontend's server functions. An avatar URL carries a random GUID and no
// session, so it is a capability URL rather than an authenticated endpoint.
app.UseStaticFiles();


app.UseCors(FrontendCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();  