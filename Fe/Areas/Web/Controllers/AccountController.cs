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

        // GET: /Web/Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Web/Account/Login
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


        // GET: /Web/Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Web/Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(AccountRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _clientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_config["ApiSettings:BaseUrl"]}accounts/register", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Registration successful, please log in!";
                return RedirectToAction("Login");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Registration error: {error}");
            return View(model);
        }

        // GET: /Web/Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST: /Web/Account/ForgotPassword
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

        //Reset Password
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



        // GET: /Web/Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWT");
            HttpContext.Session.Remove("DisplayName");
            HttpContext.Session.Remove("Role");
            return RedirectToAction("Login");
        }

        // POST: /Web/Account/GoogleCallback
        [HttpPost]
        public async Task<IActionResult> GoogleCallback([FromBody] GoogleLoginDto model)
        {
            var client = _clientFactory.CreateClient();

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_config["ApiSettings:BaseUrl"]}accounts/google-login", content);

            if (!response.IsSuccessStatusCode)
                return Unauthorized("Google login failed");

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var jwt = doc.RootElement.GetProperty("token").GetString();

            HttpContext.Session.SetString("JWT", jwt);

            // Gọi /me
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
    }
}
