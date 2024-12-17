using Back.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WEBPROGM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemCard>>> GetAlbums()
        {
            var albums = await _albumService.GetAlbumsAsync();
            return Ok(albums);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<ItemCard>>> GetAlbumById(int id)
        {
            var albums = await _albumService.GetAlbumByIdAsync(id);
            return Ok(albums);
        }

        [HttpPost]
        public async Task<ActionResult<ItemCard>> CreateAlbum(ItemCard album)
        {
            var createdAlbum = await _albumService.CreateAlbumAsync(album);
            return CreatedAtAction(nameof(GetAlbums), new { productId = createdAlbum.ProductId }, createdAlbum);
        }

        [HttpPatch("{productId}")]
        public async Task<IActionResult> UpdateStock(int productId, [FromBody] int stock)
        {
            try
            {
                await _albumService.UpdateStockAsync(productId, stock);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}
