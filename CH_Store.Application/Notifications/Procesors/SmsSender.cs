using CH_Store.Application.Notifications.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Notifications.Procesors
{
     public class SmsSender : IMessageSender
     {
          public void Send(string content)
          {
               Console.WriteLine($"[SMS Gateway] Trimitere SMS scurt: {content}");
          }
     }
}
