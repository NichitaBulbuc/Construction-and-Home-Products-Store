using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Application.Notifications.Procesors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Notifications.Services
{
     public class SmsNotificationFactory : INotificationFactory
     {
          public IMessageSender CreateSender()
          {
               return new SmsSender();
          }

          public ITemplateProvider CreateTemplate()
          {
               return new SmsTemplate();
          }
     }
}
