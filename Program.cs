using Microsoft.EntityFrameworkCore;
using Trackify.Api.Data;
using Trackify.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Config Sql server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger pour tester facilement
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
