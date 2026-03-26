using Microsoft.AspNetCore.Mvc;
using PRN232_FE.Models;
using System.Text.Json;
using System.Net.Http.Headers;

namespace PRN232_FE.Controllers
{
    public class StoreController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public StoreController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> MyStore()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (userId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            var viewModel = new StoreDashboardViewModel();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Fetch Store Info
            var storeResponse = await client.GetAsync($"{baseUrl}/api/Store/{userId}");
            if (storeResponse.IsSuccessStatusCode)
            {
                var storeString = await storeResponse.Content.ReadAsStringAsync();
                var storeData = JsonSerializer.Deserialize<Dictionary<string, object>>(storeString, options);
                
                if (storeData != null)
                {
                    viewModel.SellerId = userId.Value;
                    if (storeData.TryGetValue("storeName", out var snElement)) viewModel.StoreName = snElement.ToString() ?? "";
                    if (storeData.TryGetValue("description", out var descElement)) viewModel.Description = descElement.ToString() ?? "";
                    if (storeData.TryGetValue("bannerImageUrl", out var bannerElement)) viewModel.BannerImageUrl = bannerElement.ToString() ?? "";
                    if (storeData.TryGetValue("reputationScore", out var repElement))
                    {
                        if (int.TryParse(repElement.ToString(), out var parsedRep)) viewModel.ReputationScore = parsedRep;
                    }
                }
            }
            else
            {
                // Unlikely to happen since they had the button, but just in case
                TempData["ErrorMessage"] = "Could not load store details.";
                return RedirectToAction("Profile", "Auth");
            }

            // Fetch Products
            var productsResponse = await client.GetAsync($"{baseUrl}/api/Product/GetBySellerId/{userId}");
            if (productsResponse.IsSuccessStatusCode)
            {
                var productsString = await productsResponse.Content.ReadAsStringAsync();
                viewModel.Products = JsonSerializer.Deserialize<List<ProductViewModel>>(productsString, options) ?? new List<ProductViewModel>();
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (userId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            // Bring Categories
            var catResponse = await client.GetAsync($"{baseUrl}/api/Category");
            var categories = new List<CategoryViewModel>();
            if (catResponse.IsSuccessStatusCode)
            {
                var catStr = await catResponse.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catStr, options) ?? new List<CategoryViewModel>();
            }

            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductCreateRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (userId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/Product/Add/{userId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction("MyStore");
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorBody) ? "Failed to add product." : errorBody;
                // Redirect back to GET so categories are repopulated
                return RedirectToAction("AddProduct");
            }
        }
    }
}
