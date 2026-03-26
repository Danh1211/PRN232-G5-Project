using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_BE.DTOs.Product;
using PRN232_BE.Models;

namespace PRN232_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly CloneEbayDb1Context _context;
        public ProductController(CloneEbayDb1Context context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllProduct()
        {
            var products = await _context.Products.
                Select(p => new ProductResponse
                {
                    Id = p.Id,
                    SellerId = p.SellerId,
                    StoreId = p.StoreId,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CreatedAt = p.CreatedAt,
                    ThumbnailUrl = p.ThumbnailUrl
                })
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var p = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(new ProductResponse
            {
                Id = p.Id,
                SellerId = p.SellerId,
                StoreId = p.StoreId,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CreatedAt = p.CreatedAt,
                ThumbnailUrl = p.ThumbnailUrl
            });
        }

        [HttpGet("GetBySellerId/{sellerId}")]
        public async Task<IActionResult> GetProductBySellerId(int sellerId)
        {
            var products = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    SellerId = p.SellerId,
                    StoreId = p.StoreId,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CreatedAt = p.CreatedAt,
                    ThumbnailUrl = p.ThumbnailUrl
                })
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("GetByOther/{sellerId}")]
        public async Task<IActionResult> GetByOther(int sellerId)
        {
            var products = await _context.Products
                .Where(p => p.SellerId != sellerId)
                .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    SellerId = p.SellerId,
                    StoreId = p.StoreId,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CreatedAt = p.CreatedAt,
                    ThumbnailUrl = p.ThumbnailUrl
                })
                .ToListAsync();
            return Ok(products);
        }

        [HttpPost("Add/{sellerId}")]
        public async Task<IActionResult> AddProduct(int sellerId, ProductCreateRequest request)
        {
            var totalReputation = await _context.FeedbackDetails
            .Where(f => _context.Feedbacks
                .Where(fb => fb.Id == f.FeedbackId && fb.SellerId == sellerId)
                .Any())
            .SumAsync(f => (int?)f.Type) ?? 0;
            if (totalReputation < 0)
            {
                return BadRequest("Your reputation is too low to add product");
            }

            var store = await _context.Stores.FirstOrDefaultAsync(s => s.SellerId == sellerId);
            if (store == null)
            {
                return BadRequest("Create store first");
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("Category does not exist");
            }

            var product = new Product
            {
                SellerId = sellerId,
                StoreId = store.Id,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                IsAuction = false,
                AuctionEndTime = null,
                CreatedAt = DateTime.UtcNow,
                ThumbnailUrl = request.ThumbnailUrl
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new ProductResponse
            {
                Id = product.Id,
                SellerId = sellerId,
                StoreId = store.Id,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                CreatedAt = product.CreatedAt,
                ThumbnailUrl = product.ThumbnailUrl
            });
        }
    }
}
