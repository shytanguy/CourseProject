using System.ComponentModel.DataAnnotations;

namespace Back.Models
{ 
    public class ItemCard
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public int Stock { get; set; }
    }

    public class Client
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<int> ShoppingCart { get; set; } = new List<int>();
    }

    public class BankCard
    {
        public int BankCardId { get; set; }
        public int UserId { get; set; }
        public string CardNumber { get; set; }
        public string CardholderName { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}