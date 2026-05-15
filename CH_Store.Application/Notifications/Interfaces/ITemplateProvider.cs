using CH_Store.Domain.Models;

namespace CH_Store.Application.Notifications.Interfaces
{
     /// <summary>
     /// Abstract Product B — genereaza continutul mesajului pe baza contextului evenimentului.
     /// </summary>
     public interface ITemplateProvider
     {
          /// <summary>
          /// Subiectul mesajului.
          /// Email: linie de subiect clara. SMS: returneaza string gol (nu se foloseste).
          /// </summary>
          string GetSubject(NotificationData data);

          /// <summary>
          /// Corpul mesajului.
          /// Email: HTML complet. SMS: text scurt (max ~160 caractere).
          /// </summary>
          string GetContent(NotificationData data);
     }
}
