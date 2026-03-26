using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/feedback-reply")]
public class FeedbackReplyController : ControllerBase
{
    private readonly AppDbContext _context;

    public FeedbackReplyController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(FeedbackReply model)
    {
        var exists = await _context.FeedbackReplies
            .AnyAsync(r => r.FeedbackDetailId == model.FeedbackDetailId);

        if (exists)
            return BadRequest("Already replied");

        model.CreatedAt = DateTime.Now;

        _context.Add(model);
        await _context.SaveChangesAsync();

        return Ok(model);
    }
}