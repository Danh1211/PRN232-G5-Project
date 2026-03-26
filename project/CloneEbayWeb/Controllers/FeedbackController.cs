using Microsoft.AspNetCore.Mvc;

public class FeedbackController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}