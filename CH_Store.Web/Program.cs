using Application.DBContext;
using CH_Store.Application.DBContext;
using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Application.Notifications.Services;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Services;
using CH_Store.Application.Payments.Services;
using CH_Store.Application.Product.Services;
using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<PaymentContext>
     (opt => opt.UseInMemoryDatabase("CH_StoreDb"));

// Înregistrăm Factory-ul ca Singleton (sau Scoped)
builder.Services.AddScoped<PaymentProvider>();

//Configurare In-Memory Database
builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseInMemoryDatabase("CH_StoreDb"));
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseInMemoryDatabase("CH_StoreDb"));
builder.Services.AddScoped<IOrderFacade, OrderFacade>();
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseInMemoryDatabase("CH_StoreDb"));

// Înregistrare Fabrici (Abstract Factory)
builder.Services.AddTransient<EmailNotificationFactory>();
builder.Services.AddTransient<SmsNotificationFactory>();

// Resolver pentru a alege fabrica la runtime
builder.Services.AddTransient<Func<string, INotificationFactory>>(serviceProvider => key =>
{
     return key.ToLower() switch
     {
          "sms" => serviceProvider.GetRequiredService<SmsNotificationFactory>(),
          "email" => serviceProvider.GetRequiredService<EmailNotificationFactory>(),
          _ => serviceProvider.GetRequiredService<EmailNotificationFactory>()
     };
});


// 2. Configurare Prototype Registry ca Singleton
var registry = new ProductRegistry();

// Populăm registrul cu date "default" (șabloane)
registry.AddItem("construction", new ConstructionProduct(new ProductPrototypeData
{
     Name = "Ciment Standard",
     Price = 45.0,
     Weight = 20.0
}));

registry.AddItem("home", new HomeProduct(new ProductPrototypeData
{
     Name = "Televizor Smart",
     Price = 2500.0,
     EnergyClass = "A++"
}));
builder.Services.AddSingleton(registry);

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
         // Transformă Enum-urile în String-uri în JSON (ex: "Card" în loc de 0)
         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

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
