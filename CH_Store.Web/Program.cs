using Application.DBContext;
using CH_Store.Application.DBContext;
using CH_Store.Application.DbRepo;
using CH_Store.Application.Notifications;
using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Application.Notifications.Services;
using CH_Store.Application.Order.Interfaces;
using NotificationService = CH_Store.Application.Notifications.Services.NotificationService;
using CH_Store.Application.Order.Services;
using OrderTemplateService = CH_Store.Application.Order.Services.OrderTemplateService;
using CH_Store.Application.Payments.Services;
using CH_Store.Application.Product.Interfaces;
using CH_Store.Application.Product.Proxy;
using CH_Store.Application.Product.Services;
using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ─── Baza de date principala (SQL Server) ───────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Repository pentru comenzi (citire/scriere in AppDbContext) ─────────────
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IOrderFacade, OrderFacade>();
builder.Services.AddScoped<IOrderTemplateService, OrderTemplateService>();

// ─── Contexte InMemory pentru modulele existente (Payment, Notification, Product) ─
builder.Services.AddDbContext<PaymentContext>(opt =>
    opt.UseInMemoryDatabase("CH_StoreDb"));

builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseInMemoryDatabase("CH_StoreDb"));

builder.Services.AddDbContext<ProductContext>(options =>
    options.UseInMemoryDatabase("CH_StoreDb"));

// ─── Payment ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<PaymentProvider>();

// ─── SMTP Settings (binding din appsettings.json → IOptions<SmtpSettings>) ───
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// ─── Notifications (Abstract Factory) ────────────────────────────────────────
builder.Services.AddTransient<EmailNotificationFactory>();
builder.Services.AddTransient<SmsNotificationFactory>();

// Resolver: alege Concrete Factory-ul pe baza canalului ("email" / "sms")
builder.Services.AddTransient<Func<string, INotificationFactory>>(serviceProvider => key =>
{
     return key.ToLower() switch
     {
          "sms"   => serviceProvider.GetRequiredService<SmsNotificationFactory>(),
          "email" => serviceProvider.GetRequiredService<EmailNotificationFactory>(),
          _       => serviceProvider.GetRequiredService<EmailNotificationFactory>()
     };
});

builder.Services.AddScoped<INotificationService, NotificationService>();

// ─── Product ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IProductRepo>(provider =>
{
     var realService = provider.GetRequiredService<ProductService>();
     return new ProductRemoteProxy(realService);
});

// ─── Prototype Registry (Singleton) ─────────────────────────────────────────
var registry = new ProductRegistry();

registry.AddItem("construction", new ConstructionProduct(new ProductPrototypeData
{
     Name   = "Ciment Standard",
     Price  = 45.0,
     Weight = 20.0
}));

registry.AddItem("home", new HomeProduct(new ProductPrototypeData
{
     Name        = "Televizor Smart",
     Price       = 2500.0,
     EnergyClass = "A++"
}));

builder.Services.AddSingleton(registry);

// ─── Controllers + JSON ──────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
         options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
         options.JsonSerializerOptions.WriteIndented    = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─── Seed data InMemory (Product demo) ──────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
     var context = scope.ServiceProvider.GetRequiredService<ProductContext>();

     if (!context.Products.Any())
     {
          context.Products.AddRange(
              new ProductPrototypeData { Id = 1, Name = "Ciment Holcim 40kg",          Price = 95.0,   Weight = 40,  Description = "Ideal pentru fundatii"  },
              new ProductPrototypeData { Id = 2, Name = "Bormasina Bosch Professional", Price = 1200.0, EnergyClass = "A+",         Description = "Acumulator inclus" },
              new ProductPrototypeData { Id = 3, Name = "Vopsea Lavabila Alba 15L",     Price = 450.0,  Weight = 20,  Description = "Acoperire mare"          }
          );
          context.SaveChanges();
     }
}

// ─── Middleware pipeline ──────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
     app.UseSwagger();
     app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
