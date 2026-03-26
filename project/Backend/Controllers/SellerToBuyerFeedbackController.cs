using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/seller-feedback")]
public class SellerToBuyerFeedbackController : ControllerBase
{
    private readonly AppDbContext _context;

    public SellerToBuyerFeedbackController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(SellerToBuyerFeedback model)
    {
        var exists = await _context.SellerToBuyerFeedbacks
            .AnyAsync(f => f.OrderId == model.OrderId);

        if (exists)
            return BadRequest("Already feedback");

        model.CreatedAt = DateTime.Now;

        _context.Add(model);
        await _context.SaveChangesAsync();

        return Ok(model);
    }
}