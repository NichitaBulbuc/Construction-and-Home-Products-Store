namespace CH_Store.Application.Notifications.Interfaces
{
     /// <summary>
     /// Abstract Product A — contractul de livrare a mesajului.
     /// </summary>
     public interface IMessageSender
     {
          /// <param name="to">Destinatarul: adresa email sau numarul de telefon.</param>
          /// <param name="subject">Subiectul mesajului. Folosit de Email; ignorat de SMS.</param>
          /// <param name="content">Continutul generat de ITemplateProvider.</param>
          void Send(string to, string subject, string content);
     }
}
