using Fe.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;

        public AccountController(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _clientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_config["ApiSettings:BaseUrl"]}accounts/login", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                if (errorMessage.Contains("does not support password login", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("", "This email was registered using Google. Please log in using Google instead.");
                }
                else
                {
                    ModelState.AddModelError("", $"Login failed: {errorMessage}");
                }

                return View(model);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            var token = jsonDoc.RootElement.GetProperty("token").GetString();

            HttpContext.Session.SetString("JWT", token);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var meResponse = await client.GetAsync($"{_config["ApiSettings:BaseUrl"]}accounts/me");

            if (meResponse.IsSuccessStatusCode)
            {
                var meBody = await meResponse.Content.ReadAsStringAsync();
                var user = JsonDocument.Parse(meBody).RootElement;

                HttpContext.Session.SetString("DisplayName", user.GetProperty("displayName").GetString());
                HttpContext.Session.SetString("Role", user.GetProperty("role").GetString());
            }

            TempData["Success"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(AccountRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _clientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_config["ApiSettings:BaseUrl"]}accounts/register", content);

            var errorContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Registration successful. Please log in.";
                TempData["MessageType"] = "success";
                return RedirectToAction("Login");
            }

            try
            {
                var errorJson = JsonDocument.Parse(errorContent);
                if (errorJson.RootElement.TryGetProperty("errors", out var errors))
                {
                    foreach (var error in errors.EnumerateObject())
                    {
                        var field = error.Name;
                        foreach (var msg in error.Value.EnumerateArray())
                        {
                            ModelState.AddModelError(field, msg.GetString());
                        }
                    }
                }
                else if (errorJson.RootElement.TryGetProperty("message", out var message))
                {
                    ModelState.AddModelError("", message.GetString());
                }
                else
                {
                    ModelState.AddModelError("", "Registration failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
            }

            return View(model);
        }




        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _clientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_config["ApiSettings:BaseUrl"]}accounts/forgot-password", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Reset link sent to your email!";
                return View();
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Reset error: {error}");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is missing");

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid || model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            var client = _clientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                token = model.Token,
                newPassword = model.NewPassword
            }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_config["ApiSettings:BaseUrl"]}accounts/reset-password", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Password has been reset successfully!";
                return RedirectToAction("Login");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Reset failed: {error}");
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWT");
            HttpContext.Session.Remove("DisplayName");
            HttpContext.Session.Remove("Role");
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> GoogleCallback([FromBody] GoogleLoginDto model)
        {
            var client = _clientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_config["ApiSettings:BaseUrl"]}accounts/google-login", content);
            if (!response.IsSuccessStatusCode) return Unauthorized("Google login failed");

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var jwt = doc.RootElement.GetProperty("token").GetString();

            HttpContext.Session.SetString("JWT", jwt);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            var meResponse = await client.GetAsync($"{_config["ApiSettings:BaseUrl"]}accounts/me");

            if (meResponse.IsSuccessStatusCode)
            {
                var meJson = await meResponse.Content.ReadAsStringAsync();
                var user = JsonDocument.Parse(meJson).RootElement;

                HttpContext.Session.SetString("DisplayName", user.GetProperty("displayName").GetString());
                HttpContext.Session.SetString("Role", user.GetProperty("role").GetString());
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> MyAccount()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_config["ApiSettings:BaseUrl"]}accounts/me");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Login");

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<AccountProfileViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = HttpContext.Session.GetString("JWT");
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var data = new
            {
                currentPassword = model.CurrentPassword,
                newPassword = model.NewPassword
            };

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_config["ApiSettings:BaseUrl"]}accounts/change-password", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("MyAccount");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Change password failed: {error}");
            return View(model);
        }
    }
}
