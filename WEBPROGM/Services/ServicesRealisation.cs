using Back.Models;
using Npgsql;
using RabbitMQ.Client;
using System.Text;

public class UserService : IUserService
{
    private readonly PasswordService _passwordService;
    private readonly string _connectionString;
    private readonly IMessageBroker _messageBroker;

    public UserService(PasswordService passwordService, IConfiguration configuration, IMessageBroker messageBroker)
    {
        _passwordService = passwordService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _messageBroker = messageBroker;
    }

    public async Task<Client> RegisterUser(Client newUser)
    {
     
        newUser.Password = _passwordService.HashPassword(newUser.Password);

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(
                "INSERT INTO client (username, email, password, shoppingcart) VALUES (@username, @email, @password, @shoppingcart) RETURNING user_id", conn))
            {
                cmd.Parameters.AddWithValue("username", newUser.Username);
                cmd.Parameters.AddWithValue("email", newUser.Email);
                cmd.Parameters.AddWithValue("password", newUser.Password);
                cmd.Parameters.AddWithValue("shoppingcart", new int[0]);

                newUser.UserId = (int)await cmd.ExecuteScalarAsync();
            }
        }

 
        var message = $"User {newUser.Username} (ID: {newUser.UserId}) successfully registered.";
        _messageBroker.Publish("user_notifications", message);

        return newUser;
    }

    public async Task<Client> Login(string username, string password)
    {
        Client user = null;

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT * FROM client WHERE username = @username", conn))
            {
                cmd.Parameters.AddWithValue("username", username);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user = new Client
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            Password = reader.GetString(3),
                            ShoppingCart = reader.GetFieldValue<int[]>(4).ToList()
                        };
                    }
                }
            }
        }

        if (user == null || !_passwordService.VerifyPassword(user.Password, password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

    
        var message = $"User {user.Username} (ID: {user.UserId}) successfully logged in.";
        _messageBroker.Publish("user_notifications", message);

        return user;
    }

    public async Task<Client> GetUserById(int userId)
    {
        Client user = null;

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT * FROM client WHERE user_id = @userId", conn))
            {
                cmd.Parameters.AddWithValue("userId", userId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user = new Client
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            Password = reader.GetString(3),
                            ShoppingCart = reader.GetFieldValue<int[]>(4).ToList()
                        };
                    }
                }
            }
        }

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return user;
    }

    public async Task AddToShoppingCart(int userId, int productId)
    {
        Client user = await GetUserById(userId);

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            user.ShoppingCart.Add(productId);
            using (var cmd = new NpgsqlCommand("UPDATE client SET shoppingcart = @shoppingcart WHERE user_id = @userId", conn))
            {
                cmd.Parameters.AddWithValue("shoppingcart", user.ShoppingCart.ToArray());
                cmd.Parameters.AddWithValue("userId", userId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

       
        var message = $"Product {productId} added to User {userId}'s shopping cart.";
        _messageBroker.Publish("user_notifications", message);
    }
}


public class AlbumService : IAlbumService
{
    private readonly string _connectionString;

    public AlbumService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<ItemCard>> GetAlbumsAsync()
    {
        var albums = new List<ItemCard>();

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT * FROM item_card", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    albums.Add(new ItemCard
                    {
                        ProductId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Price = reader.GetDecimal(2),
                        Description = reader.GetString(3),
                        Category = reader.GetString(4),
                        ImageUrl = reader.GetString(5),
                        Stock = reader.GetInt32(6)
                    });
                }
            }
        }

        return albums;
    }

    public async Task<ItemCard> GetAlbumByIdAsync(int productId)
    {
        ItemCard album = null;

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT * FROM item_card WHERE product_id = @productId", conn))
            {
                cmd.Parameters.AddWithValue("productId", productId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        album = new ItemCard
                        {
                            ProductId = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            Description = reader.GetString(3),
                            Category = reader.GetString(4),
                            ImageUrl = reader.GetString(5),
                            Stock = reader.GetInt32(6)
                        };
                    }
                }
            }
        }

        return album;
    }

    public async Task<ItemCard> CreateAlbumAsync(ItemCard album)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(
                "INSERT INTO item_card (title, price, description, category, image_url, stock) VALUES (@title, @price, @description, @category, @imageUrl, @stock) RETURNING product_id", conn))
            {
                cmd.Parameters.AddWithValue("title", album.Title);
                cmd.Parameters.AddWithValue("price", album.Price);
                cmd.Parameters.AddWithValue("description", album.Description);
                cmd.Parameters.AddWithValue("category", album.Category);
                cmd.Parameters.AddWithValue("imageUrl", album.ImageUrl);
                cmd.Parameters.AddWithValue("stock", album.Stock);

                album.ProductId = (int)await cmd.ExecuteScalarAsync();
            }
        }

        return album;
    }

    public async Task<bool> UpdateStockAsync(int productId, int stock)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("UPDATE item_card SET stock = @stock WHERE product_id = @productId", conn))
            {
                cmd.Parameters.AddWithValue("productId", productId);
                cmd.Parameters.AddWithValue("stock", stock);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }
}


public class OrderService : IOrderService
{
    private readonly string _connectionString;
    private readonly IMessageBroker _messageBroker;

    public OrderService(IConfiguration configuration, IMessageBroker messageBroker)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _messageBroker = messageBroker;
    }

    public async Task<Order> CreateOrderAsync(Order newOrder)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("INSERT INTO orders (user_id, product_ids, order_date) VALUES (@userId, @productIds, @orderDate) RETURNING order_id", conn))
            {
                cmd.Parameters.AddWithValue("userId", newOrder.UserId);
                cmd.Parameters.AddWithValue("productIds", newOrder.ProductIds);
                cmd.Parameters.AddWithValue("orderDate", newOrder.OrderDate);

                newOrder.OrderId = (int)await cmd.ExecuteScalarAsync();
            }
        }

  
        var message = $"Order {newOrder.OrderId} created for User {newOrder.UserId}";
        _messageBroker.Publish("order_notifications", message);

        return newOrder;
    }

    public async Task<Order> GetOrderByIdAsync(int orderId)
    {
        Order order = null;

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT * FROM orders WHERE order_id = @orderId", conn))
            {
                cmd.Parameters.AddWithValue("orderId", orderId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        order = new Order
                        {
                            OrderId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            ProductIds = reader.GetFieldValue<int[]>(2),
                            OrderDate = reader.GetDateTime(3)
                        };
                    }
                }
            }
        }

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        }

        return order;
    }
}








public class NotificationHandler : IHostedService
{
    private readonly IMessageBroker _messageBroker;

    public NotificationHandler(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _messageBroker.Subscribe("user_notifications", message =>
        {
            Console.WriteLine($"[Notification] {message}");
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
