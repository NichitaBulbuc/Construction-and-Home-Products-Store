using Application.DBContext;
using CH_Store.Application.Admin.Interfaces;
using CH_Store.Application.Admin.Services;
using CH_Store.Application.Auth.Interfaces;
using CH_Store.Application.Auth.Services;
using CH_Store.Application.DBContext;
using CH_Store.Application.DbRepo;
using CH_Store.Application.Notifications;
using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Application.Notifications.Services;
using CH_Store.Application.Cart.Command;
using CH_Store.Application.Cart.Interfaces;
using CH_Store.Application.Cart.Services;
using CH_Store.Application.Order.Chain;
using CH_Store.Application.Order.Chain.Handlers;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Observer;
using CH_Store.Application.Order.Strategy.Payment;
using NotificationService = CH_Store.Application.Notifications.Services.NotificationService;
using CH_Store.Application.Order.Services;
using OrderTemplateService = CH_Store.Application.Order.Services.OrderTemplateService;
using CH_Store.Application.PaymentAdapter.API;
using CH_Store.Application.Payments.Services;
using CH_Store.Application.Product.Interfaces;
using CH_Store.Application.Product.Proxy;
using CH_Store.Application.Product.Services;
using CatalogService = CH_Store.Application.Product.Services.CatalogService;
using CH_Store.Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ─── JWT Authentication ──────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? "CH_Store_JWT_SecretKey_Min32Chars_2026!!Secure";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
         // Dezactiveaza remapping-ul automat al claim-urilor
         // (implicit, "role" → ClaimTypes.Role URI, "email" → ClaimTypes.Email URI etc.)
         // Cu MapInboundClaims = false, claim-urile raman cu numele exact din JWT.
         opts.MapInboundClaims = false;

         opts.TokenValidationParameters = new TokenValidationParameters
         {
              ValidateIssuer           = true,
              ValidIssuer              = builder.Configuration["Jwt:Issuer"],
              ValidateAudience         = true,
              ValidAudience            = builder.Configuration["Jwt:Audience"],
              ValidateIssuerSigningKey = true,
              IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
              ValidateLifetime         = true,
              ClockSkew                = TimeSpan.Zero,
              // Cu MapInboundClaims=false, claim "role" din JWT ramane "role"
              // si [Authorize(Roles = "Admin")] il gaseste corect
              RoleClaimType            = "role",
              NameClaimType            = "username"
         };
    });

builder.Services.AddAuthorization();

// ─── Auth Service ────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

// ─── Baza de date principala (SQL Server) ───────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Repository pentru comenzi ───────────────────────────────────────────────
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IOrderTemplateService, OrderTemplateService>();

// ─── Command Pattern ─────────────────────────────────────────────────────────
builder.Services.AddSingleton<ICommandInvoker, CommandInvoker>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderCommandService, OrderCommandService>();

// ─── Observer Pattern ────────────────────────────────────────────────────────
builder.Services.AddScoped<IOrderObserver, CustomerNotificationObserver>();
builder.Services.AddScoped<IOrderObserver, StockObserver>();
builder.Services.AddScoped<IOrderObserver, AdminDashboardObserver>();
builder.Services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();

// ─── Strategy Pattern ────────────────────────────────────────────────────────
builder.Services.AddScoped<IPaymentStrategyResolver, PaymentStrategyResolver>();

// ─── Chain of Responsibility ─────────────────────────────────────────────────
builder.Services.AddScoped<StockValidationHandler>();
builder.Services.AddScoped<CreditLimitHandler>();
builder.Services.AddScoped<DiscountApprovalHandler>();
builder.Services.AddScoped<FraudDetectionHandler>();
builder.Services.AddScoped<IOrderApprovalChain, OrderApprovalChain>();
builder.Services.AddScoped<IOrderFacade, OrderFacade>();

// ─── Contexte InMemory ────────────────────────────────────────────────────────
builder.Services.AddDbContext<PaymentContext>(opt =>
    opt.UseInMemoryDatabase("CH_StoreDb"));

builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseInMemoryDatabase("CH_StoreDb"));

builder.Services.AddDbContext<ProductContext>(options =>
    options.UseInMemoryDatabase("CH_StoreDb"));

// ─── Payment ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IStripeExternalApi, StripeExternalApi>();
builder.Services.AddScoped<PaymentProvider>();

// ─── SMTP ─────────────────────────────────────────────────────────────────────
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// ─── Notifications (Abstract Factory) ────────────────────────────────────────
builder.Services.AddTransient<EmailNotificationFactory>();
builder.Services.AddTransient<SmsNotificationFactory>();
builder.Services.AddTransient<Func<string, INotificationFactory>>(sp => key =>
    key.ToLower() switch
    {
         "sms"   => sp.GetRequiredService<SmsNotificationFactory>(),
         "email" => sp.GetRequiredService<EmailNotificationFactory>(),
         _       => sp.GetRequiredService<EmailNotificationFactory>()
    });

builder.Services.AddScoped<INotificationService, NotificationService>();

// ─── Product ──────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductDbService>();
builder.Services.AddScoped<IProductRepo>(provider =>
{
     var realService = provider.GetRequiredService<ProductDbService>();
     var cache       = provider.GetRequiredService<IMemoryCache>();
     return new ProductRemoteProxy(realService, cache);
});

// ─── Admin Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IAdminProductService,   AdminProductService>();
builder.Services.AddScoped<IAdminOrderService,     AdminOrderService>();
builder.Services.AddScoped<IAdminUserService,      AdminUserService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// ─── Catalog (Composite Pattern) ─────────────────────────────────────────────
builder.Services.AddScoped<ICatalogService, CatalogService>();

// ─── Prototype Registry (Singleton) ─────────────────────────────────────────
var registry = new ProductRegistry();
registry.AddItem("construction", new ConstructionProduct(new ProductPrototypeData
{
     Name = "Ciment Standard", Price = 45.0, Weight = 20.0
}));
registry.AddItem("home", new HomeProduct(new ProductPrototypeData
{
     Name = "Televizor Smart", Price = 2500.0, EnergyClass = "A++"
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

// ─── Swagger cu suport JWT Bearer ────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo
     {
          Title   = "CH Store API",
          Version = "v1",
          Description = "Construction & Home Store — API complet cu autentificare JWT"
     });
     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
     {
          Name        = "Authorization",
          Type        = SecuritySchemeType.Http,
          Scheme      = "bearer",
          BearerFormat = "JWT",
          In          = ParameterLocation.Header,
          Description = "Introdu token-ul JWT (fara 'Bearer ' prefix — Swagger il adauga automat)"
     });
     c.AddSecurityRequirement(new OpenApiSecurityRequirement
     {
          {
               new OpenApiSecurityScheme
               {
                    Reference = new OpenApiReference
                    {
                         Type = ReferenceType.SecurityScheme,
                         Id   = "Bearer"
                    }
               },
               Array.Empty<string>()
          }
     });
});

// ─── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
     var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
     if (!context.Products.Any())
     {
          context.Products.AddRange(
              new ProductPrototypeData { Id = 1, Name = "Ciment Holcim 40kg",           Price = 95.0,   Weight = 40,  Description = "Ideal pentru fundatii"  },
              new ProductPrototypeData { Id = 2, Name = "Bormasina Bosch Professional",  Price = 1200.0, EnergyClass = "A+",         Description = "Acumulator inclus" },
              new ProductPrototypeData { Id = 3, Name = "Vopsea Lavabila Alba 15L",      Price = 450.0,  Weight = 20,  Description = "Acoperire mare"          }
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
app.UseDefaultFiles();

// HTML si JS nu se cache-uiesc — modificarile se vad imediat fara Ctrl+F5
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var ext = Path.GetExtension(ctx.File.Name).ToLowerInvariant();
        if (ext is ".html" or ".js")
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"]        = "no-cache";
            ctx.Context.Response.Headers["Expires"]       = "0";
        }
    }
});

app.UseAuthentication();   // ← inainte de Authorization
app.UseAuthorization();
app.MapControllers();
app.Run();
