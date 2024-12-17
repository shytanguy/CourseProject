using Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WEBPROGM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly string _connectionString;

        public InventoryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemCard>>> GetInventory()
        {
            var items = new List<ItemCard>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM item_card", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new ItemCard
                        {
                            ProductId = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            Description = reader.GetString(3),
                            Category = reader.GetString(4),
                            ImageUrl = reader.GetString(5)
                        });
                    }
                }
            }

            return items;
        }

        [HttpPost]
        public async Task<ActionResult<ItemCard>> AddToInventory(ItemCard item)
        {
           
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("INSERT INTO item_card (title, price, description, category, image_url) VALUES (@title, @price, @description, @category, @imageUrl) RETURNING product_id", conn))
                {
                    cmd.Parameters.AddWithValue("title", item.Title);
                    cmd.Parameters.AddWithValue("price", item.Price);
                    cmd.Parameters.AddWithValue("description", item.Description);
                    cmd.Parameters.AddWithValue("category", item.Category);
                    cmd.Parameters.AddWithValue("imageUrl", item.ImageUrl);
                    cmd.Parameters.AddWithValue("stock", item.Stock);
                    item.ProductId = (int)await cmd.ExecuteScalarAsync();
                }
            }

            return CreatedAtAction(nameof(GetInventory), new { productId = item.ProductId }, item);
        }
    }
}