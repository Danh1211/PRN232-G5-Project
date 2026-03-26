using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_BE.DTOs;
using PRN232_BE.Models;
using System;

namespace PRN232_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BalanceController : ControllerBase
	{
		private readonly CloneEbayDb1Context _context;

		public BalanceController(CloneEbayDb1Context context)
		{
			_context = context;
		}

		[HttpPost("add-funds/{userId}")]
		public async Task<IActionResult> UpdateBalance(int userId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			user.Balance += 1000;

			_context.Users.Update(user);
			await _context.SaveChangesAsync();

			return Ok(new { Message = "Balance updated successfully.", NewBalance = user.Balance });
		}
		[Authorize]
		[HttpPost("deduct/{userId}")]
		public async Task<IActionResult> DeductBalance(int userId, [FromBody] DeductRequest request)
		{
			if (request.Amount <= 0)
			{
				return BadRequest(new { Message = "Amount must be greater than zero." });
			}

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound(new { Message = "User not found." });
			}

			if (user.Balance < request.Amount)
			{
				return BadRequest(new { Message = "Insufficient balance. Please add more funds." });
			}

			user.Balance -= request.Amount;

			_context.Users.Update(user);
			await _context.SaveChangesAsync();

			return Ok(new
			{
				Message = "Payment successful.",
				DeductedAmount = request.Amount,
				RemainingBalance = user.Balance
			});
		}
	}
}