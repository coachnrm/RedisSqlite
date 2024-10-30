using Microsoft.EntityFrameworkCore;
using RedisSqlite.Data;
using RedisSqlite.Services;
using RedisSqlite.Services.Caching;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// builder.Services.AddStackExchangeRedisCache(option =>
// {
//     option.Configuration = builder.Configuration.GetConnectionString("Redis");
//     option.InstanceName = "Cars_";
// });

string? connStr = builder.Configuration.GetConnectionString("Redis");
if(connStr != null)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(connStr)
    );
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
