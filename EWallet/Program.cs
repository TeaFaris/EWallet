using EWallet.Configuration;
using EWallet.Controllers;
using EWallet.Data;
using EWallet.Middlewares;
using EWallet.Services.Repositories.WalletRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
builder.Services
    .Configure<WalletPresets>(builder.Configuration.GetSection(nameof(WalletPresets)));

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

// Wallet Presets add to database
using (var provider = app.Services.CreateScope())
{
    var dbContext = provider.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var walletPresets = provider.ServiceProvider.GetRequiredService<IOptions<WalletPresets>>().Value;

    if (walletPresets.DropDatabase)
    {
        await dbContext.Database.EnsureDeletedAsync();
    }

    if (walletPresets.Enable)
    {
        await dbContext.Database.EnsureCreatedAsync();

        var walletRepository = provider.ServiceProvider.GetRequiredService<IWalletRepository>();

        await walletRepository.AddRangeAsync(walletPresets.Wallets);
        await walletRepository.SaveAsync();
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
