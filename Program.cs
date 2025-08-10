using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Trackify.Api.Data;
using Trackify.Api.DataProviders;
using Trackify.Api.DataProviders.EntityProviders;
using Trackify.Api.DataProviders.EntityProviders.Interfaces;
using Trackify.Api.DataProviders.ReleaseProviders;
using Trackify.Api.DataProviders.ReleaseProviders.Interfaces;
using Trackify.Api.Endpoints;
using Trackify.Api.Mappers;
using Trackify.Api.Mappers.Interfaces;
using Trackify.Api.Services;
using Trackify.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Config Sql server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger pour tester facilement
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();

builder.Services.AddScoped<IExternalBookEntityProvider, BooksEntityProvider>();
builder.Services.AddScoped<IExternalMusicEntityProvider, MusicEntityProvider>();
builder.Services.AddScoped<IExternalGameCompanyEntityProvider, GameCompanyEntityProvider>();


builder.Services.AddScoped<IBookReleaseProvider, BookReleaseProvider>();
builder.Services.AddScoped<IMusicReleaseProvider, MusicReleaseProvider>();
builder.Services.AddScoped<IGameCompanyReleaseProvider, GameCompanyReleaseProvider>();


builder.Services.AddScoped<IBooksMapper, BooksMapper>();
builder.Services.AddScoped<IMusicMapper, MusicMapper>();
builder.Services.AddScoped<IGameCompanyMapper, GameCompanyMapper>();

builder.Services.AddScoped<BooksSearchService>();
builder.Services.AddScoped<MusicSearchService>();
builder.Services.AddScoped<GameCompanySearchService>();



builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        var keyString = builder.Configuration["Jwt:Secret"] ?? builder.Configuration["JwtSecret"] ?? builder.Configuration["JwtKey"];
        var issuer = builder.Configuration["Jwt:Issuer"] ?? builder.Configuration["JwtIssuer"];
        var audience = builder.Configuration["Jwt:Audience"] ?? builder.Configuration["JwtAudience"];

        Console.WriteLine($"[JWT] secretLen={keyString?.Length ?? 0} issuer={issuer} audience={audience}");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", () => "API Notifications en ligne !");


app.MapAuthEndpoints();
app.MapUpdatesEndpoints();
app.MapPreferencesEndpoints();
app.MapDataEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
}

app.Run();
