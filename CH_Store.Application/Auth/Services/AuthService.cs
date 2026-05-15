using CH_Store.Application.Auth.Interfaces;
using CH_Store.Application.DBContext;
using CH_Store.Domain.DTOs.Auth;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CH_Store.Application.Auth.Services
{
     /// <summary>
     /// Gestioneaza autentificarea si inregistrarea utilizatorilor.
     ///
     /// Parole: SHA-256 → Base64 (44 caractere, se incadreaza in StringLength(50))
     /// Token: JWT HS256, claims: userId, username, email, role (URole.ToString())
     /// RoleClaimType: "role" — compatibil cu [Authorize(Roles = "Admin")]
     /// </summary>
     public class AuthService : IAuthService
     {
          private readonly AppDbContext    _db;
          private readonly IConfiguration _config;

          public AuthService(AppDbContext db, IConfiguration config)
          {
               _db     = db;
               _config = config;
          }

          // ── Login ─────────────────────────────────────────────────────────────

          public async Task<AuthResponse?> LoginAsync(LoginRequest req)
          {
               var hashed = HashPassword(req.Password);

               // Incearca mai intai parola hashata (fluxul normal)
               var user = await _db.Users.FirstOrDefaultAsync(u =>
                    u.Username == req.Username && u.Password == hashed);

               // Fallback: parola plain-text (useri creati inainte de sistemul de hash)
               // Daca se potriveste, migreaza automat parola la hash
               if (user == null)
               {
                    var legacyUser = await _db.Users.FirstOrDefaultAsync(u =>
                         u.Username == req.Username && u.Password == req.Password);

                    if (legacyUser != null)
                    {
                         legacyUser.Password = hashed;   // migrare automata
                         user = legacyUser;
                    }
               }

               if (user == null) return null;

               user.LastLoginDateTime = DateTime.UtcNow;
               await _db.SaveChangesAsync();

               return BuildToken(user);
          }

          // ── Register ──────────────────────────────────────────────────────────

          public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
          {
               if (await _db.Users.AnyAsync(u => u.Username == req.Username))
                    throw new InvalidOperationException("Username-ul este deja folosit.");

               if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                    throw new InvalidOperationException("Email-ul este deja inregistrat.");

               var user = new UDbTable
               {
                    Username             = req.Username,
                    Email                = req.Email,
                    Password             = HashPassword(req.Password),
                    Phone                = req.Phone   ?? "",
                    Address              = req.Address ?? "",
                    UserIP               = "",
                    Level                = URole.User,
                    RegistartionDateTime = DateTime.UtcNow,
                    LastLoginDateTime    = DateTime.UtcNow
               };

               _db.Users.Add(user);
               await _db.SaveChangesAsync();

               return BuildToken(user);
          }

          // ── Helpers ───────────────────────────────────────────────────────────

          /// <summary>SHA-256 → Base64 (44 chars). Incape in StringLength(50).</summary>
          private static string HashPassword(string password)
               => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

          private AuthResponse BuildToken(UDbTable user)
          {
               var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
               var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
               var expires = DateTime.UtcNow.AddDays(
                    double.TryParse(_config["Jwt:ExpiryDays"], out var d) ? d : 7);

               var claims = new[]
               {
                    new Claim("userId",   user.Id.ToString()),
                    new Claim("username", user.Username),
                    new Claim("email",    user.Email),
                    // "role" = RoleClaimType setat in Program.cs → [Authorize(Roles="Admin")] functioneaza
                    new Claim("role",     user.Level.ToString()),
               };

               var token = new JwtSecurityToken(
                    issuer:             _config["Jwt:Issuer"],
                    audience:           _config["Jwt:Audience"],
                    claims:             claims,
                    expires:            expires,
                    signingCredentials: creds);

               return new AuthResponse
               {
                    Token     = new JwtSecurityTokenHandler().WriteToken(token),
                    UserId    = user.Id,
                    Username  = user.Username,
                    Email     = user.Email,
                    Role      = user.Level.ToString(),
                    ExpiresAt = expires
               };
          }
     }
}
