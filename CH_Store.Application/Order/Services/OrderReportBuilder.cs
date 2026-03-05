using CH_Store.Application.Order.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Order.Services
{
     public class OrderReportBuilder: IOrderBuilder
     {
          private StringBuilder _report = new StringBuilder();

          public OrderReportBuilder()
          {
               Reset();
          }

          public void Reset()
          {
               _report = new StringBuilder();
               _report.AppendLine("ORDER REPORT");
               _report.AppendLine("----------------------");
          }

          public void AddItem(string name, double price, int quantity)
          {
               _report.AppendLine($"Product: {name}");
               _report.AppendLine($"Price: {price}");
               _report.AppendLine($"Quantity: {quantity}");
               _report.AppendLine();
          }

          public void SetDelivery(string type)
          {
               _report.AppendLine($"Delivery: {type}");
          }

          public void AddInstallation()
          {
               _report.AppendLine("Includes installation service");
          }

          public void EnablePriority()
          {
               _report.AppendLine("Priority processing enabled");
          }

          public string GetResult()
          {
               return _report.ToString();
          }
     }
}
