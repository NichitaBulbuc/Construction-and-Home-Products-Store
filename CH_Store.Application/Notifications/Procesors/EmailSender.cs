using CH_Store.Application.Notifications.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CH_Store.Application.Notifications.Procesors
{
     /// <summary>
     /// Concrete Product A1 — livreaza mesajul prin email.
     ///
     /// Daca SmtpSettings.UseSmtp = true  → trimite email real prin MailKit.
     /// Daca SmtpSettings.UseSmtp = false → simuleaza trimiterea (log in consola).
     /// </summary>
     public class EmailSender : IMessageSender
     {
          private readonly SmtpSettings _settings;

          public EmailSender(SmtpSettings settings)
          {
               _settings = settings;
          }

          public void Send(string to, string subject, string content)
          {
               if (!_settings.UseSmtp)
               {
                    // ── Mod simulare (dev / test) ───────────────────────────
                    Console.WriteLine("─────────────────────────────────────────────");
                    Console.WriteLine($"[EMAIL - SIMULAT]");
                    Console.WriteLine($"  De la    : {_settings.FromName} <{_settings.FromAddress}>");
                    Console.WriteLine($"  Catre    : {to}");
                    Console.WriteLine($"  Subiect  : {subject}");
                    Console.WriteLine($"  Continut : {TruncateHtml(content, 120)}");
                    Console.WriteLine("─────────────────────────────────────────────");
                    return;
               }

               // ── Mod SMTP real (MailKit) ─────────────────────────────────
               try
               {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
                    message.To.Add(MailboxAddress.Parse(to));
                    message.Subject = subject;
                    message.Body    = new TextPart("html") { Text = content };

                    using var client = new SmtpClient();

                    // StartTls pe portul 587 (standard modern); SSL implicit pe 465
                    var socketOptions = _settings.Port == 465
                         ? SecureSocketOptions.SslOnConnect
                         : SecureSocketOptions.StartTls;

                    client.Connect(_settings.Host, _settings.Port, socketOptions);

                    if (!string.IsNullOrWhiteSpace(_settings.Username))
                         client.Authenticate(_settings.Username, _settings.Password);

                    client.Send(message);
                    client.Disconnect(quit: true);

                    Console.WriteLine($"[EMAIL] Trimis catre {to} | Subiect: {subject}");
               }
               catch (Exception ex)
               {
                    // Nu crapa aplicatia — logheaza eroarea si continua
                    Console.WriteLine($"[EMAIL-ERROR] Trimitere catre {to} esuata: {ex.Message}");
               }
          }

          /// <summary>Extrage textul vizibil din HTML pentru preview in consola.</summary>
          private static string TruncateHtml(string html, int maxLen)
          {
               // Eliminam tagurile HTML pentru preview
               var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
               text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
               return text.Length > maxLen ? text[..maxLen] + "..." : text;
          }
     }
}
