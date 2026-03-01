using Application.DBContext;
using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Services
{
     public abstract class PaymentService
     {
          public abstract IPaymentProcessor Create();
          protected readonly PaymentContext? _context;

          public void Pay(double amount)
          {
               var processor = Create();
               processor.Process(amount);
               Console.WriteLine("Tranzacție finalizată cu succes în sistemul magazinului.");
          }

          public void Pay(double amount, PaymentType type)
          {
               var processor = Create();
               processor.Process(amount);

               // SALVARE ÎN BAZA DE DATE
               var transaction = new PaymentData
               {
                    Amount = amount,
                    Method = type,
                    Status = "Success"
               };

               _context.Transactions.Add(transaction);
               _context.SaveChanges();
          }
     }
}
