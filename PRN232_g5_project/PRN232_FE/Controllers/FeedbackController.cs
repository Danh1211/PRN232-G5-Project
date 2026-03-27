using Microsoft.AspNetCore.Mvc;

namespace PRN232_FE.Controllers
{
    public class FeedbackController : Controller
    {
        // GET: /Feedback
        public IActionResult Index() => View();

        // GET: /Feedback/Create
        public IActionResult Create() => View();

        // GET: /Feedback/Edit/5  (uses query or route id)
        public IActionResult Edit() => View();

        // GET: /Feedback/Details/5
        public IActionResult Details() => View();
    }
}