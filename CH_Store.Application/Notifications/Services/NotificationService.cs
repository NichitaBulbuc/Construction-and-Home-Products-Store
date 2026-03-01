using CH_Store.Application.Notifications.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Notifications.Services
{
     public class NotificationService
     {
          private readonly IMessageSender _sender;
          private readonly ITemplateProvider _template;

          public NotificationService(INotificationFactory factory, string orderId)
          {
               _sender = factory.CreateSender();
               _template = factory.CreateTemplate();
          }

          public void NotifyClient(string orderId) 
          {
               string message = _template.GetFormatedContent(orderId);
               _sender.Send(message);
          }
     }
}
