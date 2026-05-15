namespace CH_Store.Domain.DTOs.Auth
{
     /// <summary>
     /// Raspuns returnat dupa Login/Register.
     /// Token-ul JWT se pastreaza in localStorage si se trimite ca Bearer la fiecare request.
     /// </summary>
     public class AuthResponse
     {
          public string   Token     { get; set; } = "";
          public int      UserId    { get; set; }
          public string   Username  { get; set; } = "";
          public string   Email     { get; set; } = "";
          public string   Role      { get; set; } = "";    // "None","User","Moderator","Admin"
          public DateTime ExpiresAt { get; set; }
     }
}
