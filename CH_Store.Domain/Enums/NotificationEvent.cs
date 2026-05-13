namespace CH_Store.Domain.Enums
{
     public enum NotificationEvent
     {
          // ─── Comenzi ───────────────────────────────
          OrderAccepted    = 1,   // Comanda a fost inregistrata si acceptata
          OrderProcessing  = 2,   // Comanda este in curs de procesare
          OrderShipped     = 3,   // Comanda a fost expediata
          OrderDelivered   = 4,   // Comanda a fost livrata
          OrderCancelled   = 5,   // Comanda a fost anulata

          // ─── Promotii / Marketing ──────────────────
          PromotionAvailable = 10, // Promotie/reducere disponibila
          SeasonalSale       = 11, // Vanzare sezoniera (ex: Black Friday)

          // ─── Cont utilizator ──────────────────────
          Welcome        = 20,    // Bun venit la inregistrare
          PasswordReset  = 21,    // Solicitare resetare parola
     }
}
