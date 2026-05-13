using CH_Store.Application.Notifications.Interfaces;

namespace CH_Store.Application.Notifications.Procesors
{
     /// <summary>
     /// Concrete Product A2 — livreaza mesajul prin SMS.
     /// Subiectul este ignorat (SMS nu are camp de subiect).
     /// In productie: se integreaza cu Twilio / SMSC / gateway local.
     /// </summary>
     public class SmsSender : IMessageSender
     {
          public void Send(string to, string subject, string content)
          {
               Console.WriteLine("─────────────────────────────────────────────");
               Console.WriteLine($"[SMS - SIMULAT]");
               Console.WriteLine($"  Catre  : {to}");
               Console.WriteLine($"  Mesaj  : {content}");
               Console.WriteLine("─────────────────────────────────────────────");
          }
     }
}
