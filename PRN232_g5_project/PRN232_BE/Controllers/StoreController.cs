using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_BE.DTOs.Store;
using PRN232_BE.Models;

namespace PRN232_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly CloneEbayDb1Context _context;
        public StoreController(CloneEbayDb1Context context)
        {
            _context = context;
        }

        [HttpGet("check/{userId}")]
        public async Task<IActionResult> HasStore(int userId)
        {
            var hasStore = await _context.Stores.AnyAsync(s => s.SellerId == userId);
            return Ok(new { HasStore = hasStore });
        }

        [HttpGet("{sellerId}")]
        public async Task<IActionResult> GetStoreBySellerId(int sellerId)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.SellerId == sellerId);
            if (store == null)
            {
                return NotFound();
            }

            var totalReputation = await _context.FeedbackDetails
                .Where(f => _context.Feedbacks
                    .Where(fb => fb.Id == f.FeedbackId && fb.SellerId == sellerId)
                    .Any())
                .SumAsync(f => (int?)f.Type) ?? 0;

            return Ok(new StoreResponse
            {
                SellerId = sellerId,
                StoreName = store.StoreName,
                Description = store.Description,
                BannerImageUrl = store.BannerImageUrl,
                ReputationScore = totalReputation
            });
        }

        [HttpPost("add/{sellerId}")]
        public async Task<IActionResult> AddStore(int sellerId, StoreCreateRequest request)
        {
            var hasStore = await _context.Stores.AnyAsync(s => s.SellerId == sellerId);
            if(hasStore)
            {
                return BadRequest("Seller already has a store");
            }

            var store = new Store
            {
                SellerId = sellerId,
                StoreName = request.StoreName,
                Description = request.Description,
                BannerImageUrl = request.BannerImageUrl
            };
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();
            return Ok(new StoreResponse
            {
                SellerId = sellerId,
                StoreName = request.StoreName,
                Description = request.Description,
                BannerImageUrl = request.BannerImageUrl,
                ReputationScore = 0
            });
        }
    }
}
