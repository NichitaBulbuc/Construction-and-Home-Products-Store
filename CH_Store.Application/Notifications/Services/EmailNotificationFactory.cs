using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Application.Notifications.Procesors;
using Microsoft.Extensions.Options;

namespace CH_Store.Application.Notifications.Services
{
     /// <summary>
     /// Concrete Factory 1 — creeaza familia de produse Email.
     ///
     /// Primeste SmtpSettings din DI si le paseaza la EmailSender.
     /// EmailTemplate nu are dependente externe — se instantiaza direct.
     /// </summary>
     public class EmailNotificationFactory : INotificationFactory
     {
          private readonly SmtpSettings _smtpSettings;

          public EmailNotificationFactory(IOptions<SmtpSettings> smtpOptions)
          {
               _smtpSettings = smtpOptions.Value;
          }

          /// <summary>Creeaza EmailSender configurat cu setarile SMTP din appsettings.</summary>
          public IMessageSender CreateSender()
               => new EmailSender(_smtpSettings);

          /// <summary>Creeaza EmailTemplate — genereaza HTML complet cu subiect per eveniment.</summary>
          public ITemplateProvider CreateTemplate()
               => new EmailTemplate();
     }
}
