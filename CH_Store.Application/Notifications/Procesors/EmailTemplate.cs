using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Notifications.Procesors
{
     /// <summary>
     /// Concrete Product B1 — genereaza email HTML structurat pentru fiecare eveniment.
     /// </summary>
     public class EmailTemplate : ITemplateProvider
     {
          public string GetSubject(NotificationData data) => data.Event switch
          {
               NotificationEvent.OrderAccepted    => $"✅ Comanda #{data.OrderId} acceptata — CH Store",
               NotificationEvent.OrderProcessing  => $"⚙️ Comanda #{data.OrderId} se proceseaza — CH Store",
               NotificationEvent.OrderShipped     => $"🚚 Comanda #{data.OrderId} expediata — CH Store",
               NotificationEvent.OrderDelivered   => $"📦 Comanda #{data.OrderId} livrata — CH Store",
               NotificationEvent.OrderCancelled   => $"❌ Comanda #{data.OrderId} anulata — CH Store",
               NotificationEvent.PromotionAvailable => $"🎁 Oferta speciala pentru tine — CH Store",
               NotificationEvent.SeasonalSale     => $"🔥 Vanzare sezoniera — CH Store",
               NotificationEvent.Welcome          => $"👋 Bun venit la CH Store!",
               NotificationEvent.PasswordReset    => $"🔒 Resetare parola — CH Store",
               _                                  => "Notificare CH Store"
          };

          public string GetContent(NotificationData data)
          {
               string body = data.Event switch
               {
                    NotificationEvent.OrderAccepted => $@"
                         <h2 style=""color:#2e7d32;"">Comanda #{data.OrderId} a fost acceptata!</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>Comanda dvs. in valoare de <strong>{data.OrderTotal:F2} MDL</strong>
                            a fost inregistrata si este in curs de procesare.</p>
                         <p>Va vom notifica imediat ce comanda va fi expediata.</p>",

                    NotificationEvent.OrderProcessing => $@"
                         <h2 style=""color:#1565c0;"">Comanda #{data.OrderId} se proceseaza</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>Echipa noastra pregateste comanda dvs. Veti primi o confirmare de expediere in curand.</p>",

                    NotificationEvent.OrderShipped => $@"
                         <h2 style=""color:#1565c0;"">Comanda #{data.OrderId} a fost expediata!</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>Comanda dvs. este pe drum catre adresa indicata.</p>
                         <p>Livrare estimata: <strong>{data.EstimatedDelivery:dd.MM.yyyy}</strong>.</p>
                         <p>Curierul va va contacta inainte de livrare.</p>",

                    NotificationEvent.OrderDelivered => $@"
                         <h2 style=""color:#2e7d32;"">Comanda #{data.OrderId} a fost livrata!</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>Speram ca sunteti multumit de achizitie. Lasati-ne o recenzie pe site!</p>",

                    NotificationEvent.OrderCancelled => $@"
                         <h2 style=""color:#c62828;"">Comanda #{data.OrderId} a fost anulata</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>Comanda dvs. a fost anulata. Daca aveti intrebari, contactati-ne la
                            <a href=""mailto:support@chstore.md"">support@chstore.md</a>.</p>",

                    NotificationEvent.PromotionAvailable => $@"
                         <h2 style=""color:#e65100;"">Oferta speciala pentru dvs.!</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>{data.PromoDescription}</p>
                         <table style=""margin:16px 0;"">
                           <tr><td><strong>Reducere:</strong></td><td>{data.DiscountPercent}%</td></tr>
                           <tr><td><strong>Cod promo:</strong></td><td><code style=""background:#f5f5f5;padding:2px 6px;"">{data.PromoCode}</code></td></tr>
                         </table>
                         <p>Oferta este valabila pana la epuizarea stocului.</p>",

                    NotificationEvent.SeasonalSale => $@"
                         <h2 style=""color:#e65100;"">Vanzare sezoniera CH Store!</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>{data.PromoDescription}</p>
                         <p>Reduceri de pana la <strong>{data.DiscountPercent}%</strong>
                            la materiale de constructii si articole home!</p>",

                    NotificationEvent.Welcome => $@"
                         <h2 style=""color:#2e7d32;"">Bun venit la CH Store!</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>Contul dvs. a fost creat cu succes. Aveti acces la catalogul complet
                            de materiale de constructii si articole home.</p>",

                    NotificationEvent.PasswordReset => $@"
                         <h2>Resetare parola</h2>
                         <p>Buna ziua, <strong>{data.RecipientName}</strong>!</p>
                         <p>Ati solicitat resetarea parolei. Apasati butonul de mai jos:</p>
                         <p style=""text-align:center;margin:24px 0;"">
                           <a href=""{data.ResetLink}""
                              style=""background:#f4a01c;color:#fff;padding:12px 24px;text-decoration:none;border-radius:4px;"">
                             Reseteaza parola
                           </a>
                         </p>
                         <p style=""font-size:12px;color:#999;"">Linkul este valabil 30 de minute.
                            Daca nu ati solicitat resetarea, ignorati acest mesaj.</p>",

                    _ => $"<p>Notificare pentru {data.RecipientName}.</p>"
               };

               return $@"<!DOCTYPE html>
<html lang=""ro"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
  <title>CH Store</title>
</head>
<body style=""margin:0;padding:0;background:#f5f5f5;font-family:Arial,sans-serif;color:#333;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
    <tr>
      <td align=""center"" style=""padding:24px 0;"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0""
               style=""background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.1);"">
          <!-- Header -->
          <tr>
            <td style=""background:#f4a01c;padding:20px 32px;"">
              <h1 style=""margin:0;color:#fff;font-size:24px;"">CH Store</h1>
              <p style=""margin:4px 0 0;color:#fff;opacity:.85;font-size:13px;"">Materiale de constructii &amp; Home</p>
            </td>
          </tr>
          <!-- Body -->
          <tr>
            <td style=""padding:32px;"">
              {body}
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td style=""background:#f5f5f5;padding:16px 32px;font-size:12px;color:#888;text-align:center;"">
              © {DateTime.UtcNow.Year} CH Store &nbsp;|&nbsp;
              <a href=""mailto:support@chstore.md"" style=""color:#f4a01c;"">support@chstore.md</a>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
          }
     }
}
