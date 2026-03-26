using Microsoft.AspNetCore.Mvc;
using PRN232_FE.Models;
using System.Text.Json;
using System.Text;

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
                        var token = tokenElement.ToString();
                        HttpContext.Session.SetString("JwtToken", token!);
                        // Also store user info if needed
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
                    // Success, redirect to login
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
    }
}
