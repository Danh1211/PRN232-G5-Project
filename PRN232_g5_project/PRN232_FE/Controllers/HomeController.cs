using Microsoft.AspNetCore.Mvc;
using PRN232_FE.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Diagnostics;

namespace PRN232_FE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(token);
            
            var viewModel = new HomeViewModel();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            try
            {
                var catResponse = await client.GetAsync($"{baseUrl}/api/Category");
                if (catResponse.IsSuccessStatusCode)
                {
                    var catString = await catResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    viewModel.Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catString, options) ?? new List<CategoryViewModel>();
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                string productUrl = $"{baseUrl}/api/Product/GetAll";
                if (userId != null && userId > 0)
                {
                    productUrl = $"{baseUrl}/api/Product/GetByOther/{userId}";
                }

                var prodResponse = await client.GetAsync(productUrl);
                if (prodResponse.IsSuccessStatusCode)
                {
                    var prodString = await prodResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    viewModel.Products = JsonSerializer.Deserialize<List<ProductViewModel>>(prodString, options) ?? new List<ProductViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from API");
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
