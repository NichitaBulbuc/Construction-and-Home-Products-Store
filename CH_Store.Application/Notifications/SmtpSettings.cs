namespace CH_Store.Application.Notifications
{
     public class SmtpSettings
     {
          /// <summary>Adresa serverului SMTP (ex: smtp.gmail.com, sandbox.smtp.mailtrap.io).</summary>
          public string Host        { get; set; } = "";

          /// <summary>Portul SMTP. Standard: 587 (TLS) sau 465 (SSL).</summary>
          public int    Port        { get; set; } = 587;

          public string Username    { get; set; } = "";
          public string Password    { get; set; } = "";

          /// <summary>Adresa expeditorului afisata in campul "From".</summary>
          public string FromAddress { get; set; } = "noreply@chstore.md";

          /// <summary>Numele afisat in campul "From" (ex: "CH Store").</summary>
          public string FromName    { get; set; } = "CH Store";

          /// <summary>
          /// true  → trimite email real prin SMTP (necesita credentiale valide).
          /// false → simuleaza trimiterea, afiseaza doar in consola (modul dev/test).
          /// </summary>
          public bool UseSmtp { get; set; } = false;
     }
}
