using Application.DBContext;
using CH_Store.Application.Payments.Factories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<PaymentContext>
     (opt => opt.UseInMemoryDatabase("CH_StoreDb"));

// Înregistrăm Factory-ul ca Singleton (sau Scoped)
builder.Services.AddSingleton<PaymentProvider>();

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
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
