using EWallet.Configuration;
using EWallet.Controllers;
using EWallet.Data;
using EWallet.Middlewares;
using EWallet.Services.Repositories.WalletRepositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddApplicationPart(typeof(WalletController).Assembly);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Services
    .Configure<HMACConfiguration>(builder.Configuration.GetSection(nameof(HMACConfiguration)));

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

// Middlewares
builder.Services
    .AddTransient<HMACAuthenticationMiddleware>();

var app = builder.Build();

// Auto-migration
using (var provider = app.Services.CreateScope())
{
    var context = provider.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<HMACAuthenticationMiddleware>();

app.Run();
