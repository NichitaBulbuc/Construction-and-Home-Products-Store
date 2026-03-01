using CH_Store.Application.Notifications.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Notifications.Procesors
{
     public class SmsTemplate : ITemplateProvider
     {
          public string GetFormatedContent(string orderId)
          {
               return $"Comanda #{orderId} a fost expediata!";
          }
     }
}
