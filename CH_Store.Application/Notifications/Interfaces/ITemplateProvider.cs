using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Notifications.Interfaces
{
     public interface ITemplateProvider
     {
          string GetFormatedContent(string orderId);
     }
}
