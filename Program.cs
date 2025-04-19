using MerchantTransactionProcessing.Data;
using MerchantTransactionProcessing.Middlewares;
using MerchantTransactionProcessing.Repositories.MerchantRepository;
using MerchantTransactionProcessing.Repositories.PaymentMethodRepository;
using MerchantTransactionProcessing.Repositories.TransactionRepository;
using MerchantTransactionProcessing.Services.BackgroundServices;
using MerchantTransactionProcessing.Services.CacheService;
using MerchantTransactionProcessing.Services.MerchantService;
using MerchantTransactionProcessing.Services.PaymentGateway;
using MerchantTransactionProcessing.Services.TransactionService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options=>options.UseInMemoryDatabase("MerchantTransactionDb"));
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPaymentGateway, MockPaymentGateway>();


builder.Services.AddHostedService<ProcessTransactionsBackgroundService>();

// Add caching services
if (builder.Configuration.GetValue<bool>("UseRedisCache"))
{
    // Register Redis cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        options.InstanceName = builder.Configuration["RedisCache:InstanceName"] ?? "MTP_";
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    // Register in-memory cache as fallback
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
}
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = builder.Configuration["RedisCache:InstanceName"];
});

var app = builder.Build();

// seed data
await SeedData.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseExceptionMiddlware();
app.UseHttpLoggingMiddleware();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
