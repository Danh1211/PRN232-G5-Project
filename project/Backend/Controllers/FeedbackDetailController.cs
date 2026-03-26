using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/feedback-detail")]
public class FeedbackDetailController : ControllerBase
{
    private readonly AppDbContext _context;

    public FeedbackDetailController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(FeedbackDetail model)
    {
        var exists = await _context.FeedbackDetails
            .AnyAsync(f => f.OrderId == model.OrderId);

        if (exists)
            return BadRequest("Already feedback");

        model.CreatedAt = DateTime.Now;
        model.IsUpdated = false;

        _context.Add(model);
        await _context.SaveChangesAsync();

        return Ok(model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, FeedbackDetail updated)
    {
        var feedback = await _context.FeedbackDetails.FindAsync(id);

        if (feedback == null)
            return NotFound();

        if (feedback.IsUpdated)
            return BadRequest("Only update once");

        feedback.Type = updated.Type;
        feedback.Comment = updated.Comment;
        feedback.ItemAsDescribed = updated.ItemAsDescribed;
        feedback.Communication = updated.Communication;
        feedback.ShippingTime = updated.ShippingTime;
        feedback.ShippingCost = updated.ShippingCost;
        feedback.IsUpdated = true;

        await _context.SaveChangesAsync();

        return Ok(feedback);
    }
}