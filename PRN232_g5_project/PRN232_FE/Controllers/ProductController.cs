using Microsoft.AspNetCore.Mvc;
using PRN232_FE.Models;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;

namespace PRN232_FE.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ProductController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            var response = await client.GetAsync($"{baseUrl}/api/Product/GetById/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var product = JsonSerializer.Deserialize<ProductViewModel>(responseString, options);

            return View(product);
        }

        public async Task<IActionResult> Checkout(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (userId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Auth"); // Must be logged in to checkout
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            // Fetch Product
            var productResponse = await client.GetAsync($"{baseUrl}/api/Product/GetById/{id}");
            if (!productResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var productString = await productResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var product = JsonSerializer.Deserialize<ProductViewModel>(productString, options);

            // Fetch Address
            var addressResponse = await client.GetAsync($"{baseUrl}/api/Address/GetAddressByUserId/{userId}");
            var addresses = new List<AddressViewModel>();
            if (addressResponse.IsSuccessStatusCode)
            {
                var addressString = await addressResponse.Content.ReadAsStringAsync();
                addresses = JsonSerializer.Deserialize<List<AddressViewModel>>(addressString, options) ?? new List<AddressViewModel>();
            }

            // Fetch Profile for Balance
            var profileResponse = await client.GetAsync($"{baseUrl}/api/Auth/profile");
            decimal balance = 0;
            if (profileResponse.IsSuccessStatusCode)
            {
                var profileStr = await profileResponse.Content.ReadAsStringAsync();
                var profileDict = JsonSerializer.Deserialize<Dictionary<string, object>>(profileStr, options);
                if (profileDict != null && profileDict.TryGetValue("balance", out var balStr))
                {
                    if (decimal.TryParse(balStr.ToString(), out var parsedBal)) balance = parsedBal;
                }
            }

            var viewModel = new CheckoutViewModel
            {
                Product = product,
                Addresses = addresses,
                SelectedAddressId = addresses.FirstOrDefault(a => a.IsDefault)?.Id ?? addresses.FirstOrDefault()?.Id ?? 0,
                UserBalance = balance
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int productId, int addressId, string paymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (userId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            if (addressId == 0)
            {
                TempData["ErrorMessage"] = "Please select or add a shipping address.";
                return RedirectToAction("Checkout", new { id = productId });
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            var orderBody = new
            {
                ProductId = productId,
                Quantity = 1,
                AddressId = addressId,
                PaymentMethod = paymentMethod
            };

            var content = new StringContent(JsonSerializer.Serialize(orderBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/api/Order", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Order placed successfully!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var responseString = await response.Content.ReadAsStringAsync();
                
                // If it's empty, API returned NO text body, let's explicitly debug this.
                if (string.IsNullOrWhiteSpace(responseString))
                    responseString = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                
                TempData["ErrorMessage"] = "Failed to place order. " + responseString;
                return RedirectToAction("Checkout", new { id = productId });
            }
        }
    }
}
