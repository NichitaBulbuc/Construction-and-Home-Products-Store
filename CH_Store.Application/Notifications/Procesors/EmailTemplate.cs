using CH_Store.Application.Notifications.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Notifications.Procesors
{
     internal class EmailTemplate : ITemplateProvider
     {
          public string GetFormatedContent(string orderId)
          {
               return $"<html><body><h1>Comanda #{orderId}</h1><p>Materialele dvs. sunt pe drum!</p></body></html>";
          }
     }
}
