using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PRN232_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CloneEbayDb1Context _context;
        public CategoryController(CloneEbayDb1Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory()
        {
            var categories = await _context.Categories
            .Select(c => new
            {
                c.Id,
                c.Name
            })
            .ToListAsync();

            return Ok(categories);
        }
    }
}
