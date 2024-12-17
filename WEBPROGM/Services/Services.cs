using Back.Models;
using Microsoft.AspNetCore.Identity;
using Npgsql;

public class PasswordService
{
    private readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(null, password);
    }

    public bool VerifyPassword(string hashedPassword, string inputPassword)
    {
        var result = _hasher.VerifyHashedPassword(null, hashedPassword, inputPassword);
        return result == PasswordVerificationResult.Success;
    }
}

public interface IUserService
{
    Task<Client> RegisterUser(Client newUser);
    Task AddToShoppingCart(int userId, int productId);

    Task<Client> Login(string username, string password);
    Task<Client> GetUserById(int userId);
}



public interface IOrderService
{
    Task<Order> CreateOrderAsync(Order newOrder);
    Task<Order> GetOrderByIdAsync(int orderId);
}

public interface IAlbumService
{
    Task<IEnumerable<ItemCard>> GetAlbumsAsync();
    Task<ItemCard> GetAlbumByIdAsync(int productId);
    Task<ItemCard> CreateAlbumAsync(ItemCard album);
    Task<bool> UpdateStockAsync(int productId, int stock);
}


public interface IMessageBroker
{
    void Publish(string queueName, string message);
    void Subscribe(string queueName, Action<string> handler);
}

