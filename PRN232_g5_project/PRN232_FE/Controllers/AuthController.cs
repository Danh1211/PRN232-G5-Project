using Microsoft.AspNetCore.Mvc;
using PRN232_FE.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace PRN232_FE.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            var requestBody = new
            {
                UsernameOrEmail = model.UsernameOrEmail,
                Password = model.Password
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{baseUrl}/api/Auth/login", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString, options);

                    if (result != null && result.TryGetValue("token", out var tokenElement))
                    {
                        var token = tokenElement.ToString()!.Trim('"');
                        HttpContext.Session.SetString("JwtToken", token);
                        
                        if (result.TryGetValue("userId", out var userIdElement))
                        {
                            if (int.TryParse(userIdElement.ToString(), out var userId))
                            {
                                HttpContext.Session.SetInt32("UserId", userId);
                            }
                        }

                        if (result.TryGetValue("username", out var usernameElement))
                        {
                            HttpContext.Session.SetString("Username", usernameElement.ToString()!);
                        }
                        
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var errorResult = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString, options);
                    
                    if (errorResult != null && errorResult.TryGetValue("message", out var messageElement))
                    {
                        ViewBag.ErrorMessage = messageElement.ToString();
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid username/email or password.";
                    }
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Could not connect to the authentication server. Please try again later.";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";

            // Map Firstname + Lastname to Username for backend
            var username = $"{model.Firstname.Trim()}{model.Lastname.Trim()}_{Guid.NewGuid().ToString().Substring(0, 4)}";

            var requestBody = new
            {
                Username = username,
                Email = model.Email,
                Password = model.Password
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{baseUrl}/api/Auth/register", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Auto-login to get the Token and UserId
                    var loginContent = new StringContent(JsonSerializer.Serialize(new { UsernameOrEmail = username, Password = model.Password }), Encoding.UTF8, "application/json");
                    var loginResponse = await client.PostAsync($"{baseUrl}/api/Auth/login", loginContent);
                    if (loginResponse.IsSuccessStatusCode)
                    {
                        var loginResponseStr = await loginResponse.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var loginResult = JsonSerializer.Deserialize<Dictionary<string, object>>(loginResponseStr, options);

                        string token = string.Empty;
                        int userId = 0;

                        if (loginResult != null && loginResult.TryGetValue("token", out var tokenElement))
                        {
                            token = tokenElement.ToString()!.Trim('"');
                            HttpContext.Session.SetString("JwtToken", token);
                        }

                        if (loginResult != null && loginResult.TryGetValue("userId", out var userIdElement))
                        {
                            if (int.TryParse(userIdElement.ToString(), out var parsedId))
                            {
                                userId = parsedId;
                                HttpContext.Session.SetInt32("UserId", userId);
                            }
                        }

                        if (loginResult != null && loginResult.TryGetValue("username", out var usernameElement))
                        {
                            HttpContext.Session.SetString("Username", usernameElement.ToString()!);
                        }

                        // Create Address using the token and userId
                        if (!string.IsNullOrEmpty(token) && userId > 0)
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            var addressBody = new
                            {
                                Phone = model.Phone,
                                Street = model.Street,
                                City = model.City,
                                Country = model.Country,
                                IsDefault = true
                            };
                            var addrContent = new StringContent(JsonSerializer.Serialize(addressBody), Encoding.UTF8, "application/json");
                            await client.PostAsync($"{baseUrl}/api/Address/AddAddress/{userId}", addrContent);
                        }

                        // Redirect to Home now that they are logged in and have an address
                        return RedirectToAction("Index", "Home");
                    }

                    TempData["SuccessMessage"] = "Registration successful! Please sign in.";
                    return RedirectToAction("SignIn");
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var errorResult = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString, options);
                    
                    if (errorResult != null && errorResult.TryGetValue("message", out var messageElement))
                    {
                        ViewBag.ErrorMessage = messageElement.ToString();
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Registration failed. Please check your inputs.";
                    }
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Could not connect to the authentication server. Please try again later.";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (userId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn");
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var addressResponse = await client.GetAsync($"{baseUrl}/api/Address/GetAddressByUserId/{userId}");
            var addresses = new List<AddressViewModel>();
            if (addressResponse.IsSuccessStatusCode)
            {
                var addressString = await addressResponse.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                addresses = JsonSerializer.Deserialize<List<AddressViewModel>>(addressString, options) ?? new List<AddressViewModel>();
            }

            return View(addresses);
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(AddressViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (userId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn");
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var addressBody = new
            {
                Phone = model.Phone,
                Street = model.Street,
                City = model.City,
                Country = model.Country,
                IsDefault = true // Newly added address from profile gets marked as default
            };
            var content = new StringContent(JsonSerializer.Serialize(addressBody), Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync($"{baseUrl}/api/Address/AddAddress/{userId}", content);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully added new address!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add address.";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> TopUp()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("SignIn");
            
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";
            var token = HttpContext.Session.GetString("JwtToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.PostAsync($"{baseUrl}/api/Balance/add-funds/{userId}", null);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully added $1000 to your wallet! Try placing your order again.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to top up balance.";
            }
            
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
