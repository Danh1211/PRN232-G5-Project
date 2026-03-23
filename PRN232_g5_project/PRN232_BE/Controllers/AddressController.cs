using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_BE.DTOs.Address;
using PRN232_BE.Models;

namespace PRN232_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly CloneEbayDb1Context _context;
        public AddressController(CloneEbayDb1Context context)
        {
            _context = context;
        }

        [HttpGet("GetAddressByUserId/{userId}")]
        public async Task<IActionResult> GetAddressByUserId(int userId)
        {
            var address = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
            if (address == null)
            {
                return NotFound();
            }
            return Ok(address);
        }

        [HttpPost("AddAddress/{userId}")]
        public async Task<IActionResult> AddAddress(int userId, AddressCreateRequest request)
        {
            var hasAddress = await _context.Addresses.AnyAsync(a => a.UserId == userId);
            if (!hasAddress)
            {
                request.IsDefault = true;
            }
            if (request.IsDefault)
            {
                var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

                foreach (var addr in addresses)
                    addr.IsDefault = false;
            }
            var address = new Address
            {
                UserId = userId,
                Phone = request.Phone,
                Street = request.Street,
                City = request.City,
                Country = request.Country,
                IsDefault = request.IsDefault
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return Ok(new AddressResponse
            {
                UserId = address.UserId,
                Phone = address.Phone,
                Street = address.Street,
                City = address.City,
                Country = address.Country,
                IsDefault = address.IsDefault
            });
        }

        [HttpPut("SetDefault/{userId}/{addressId}")]
        public async Task<IActionResult> SetDefault(int userId, int addressId)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
            if (address == null)
            {
                return NotFound();
            }
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();
            foreach (var addr in addresses)
                addr.IsDefault = false;
            address.IsDefault = true;
            await _context.SaveChangesAsync();
            return Ok(new AddressResponse
            {
                UserId = address.UserId,
                Phone = address.Phone,
                Street = address.Street,
                City = address.City,
                Country = address.Country,
                IsDefault = address.IsDefault
            });
        }
    }
}
