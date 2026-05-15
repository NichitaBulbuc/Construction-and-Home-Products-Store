using CH_Store.Domain.DTOs.Auth;

namespace CH_Store.Application.Auth.Interfaces
{
     public interface IAuthService
     {
          /// <summary>
          /// Autentifica utilizatorul cu username + parola.
          /// Returneaza null daca credentialele sunt invalide.
          /// </summary>
          Task<AuthResponse?> LoginAsync(LoginRequest request);

          /// <summary>
          /// Creeaza un cont nou si returneaza token-ul JWT.
          /// Arunca InvalidOperationException daca username/email sunt deja folosite.
          /// </summary>
          Task<AuthResponse> RegisterAsync(RegisterRequest request);
     }
}
