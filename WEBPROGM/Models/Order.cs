    using System;

    namespace Back.Models
    {
        public class Order
        {
            public int OrderId { get; set; }
            public int UserId { get; set; }
            public int[] ProductIds { get; set; }
            public DateTime OrderDate { get; set; }
        }
    }