using Microsoft.AspNetCore.Mvc;

namespace PRN232_FE.Controllers
{
    public class SellerController : Controller
    {
        // GET: /Seller/Reply
        public IActionResult Reply() => View();

        // GET: /Seller/SendToBuyer
        public IActionResult SendToBuyer() => View();
    }
}