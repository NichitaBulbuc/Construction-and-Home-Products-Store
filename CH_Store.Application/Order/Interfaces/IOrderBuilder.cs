using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Order.Interfaces
{
     public interface IOrderBuilder
     {
          void Reset();
          void AddItem(string name, double price, int quantity);
          void SetDelivery(string address);
          void AddInstallation();
          void EnablePriority();
     }
}
