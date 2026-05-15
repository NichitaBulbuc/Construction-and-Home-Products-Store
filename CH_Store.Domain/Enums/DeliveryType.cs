namespace CH_Store.Domain.Enums
{
     public enum DeliveryType
     {
          Standard = 0,   // 3-5 zile lucratoare, gratuit
          Express  = 1,   // 1-2 zile lucratoare, +100 MDL
          SameDay  = 2,   // Aceeasi zi (comanda pana la 12:00), +250 MDL
          Pickup   = 3    // Ridicare din depozit, gratuit
     }
}
