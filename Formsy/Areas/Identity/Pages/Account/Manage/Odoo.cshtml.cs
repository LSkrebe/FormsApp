using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace Formsy.Areas.Identity.Pages.Account.Manage
{
    public class OdooModel : PageModel
    {
        private readonly string _adminToken;
        private readonly string _odooUrl;
        private readonly string _odooDb;
        private readonly string _odooUsername;
        private readonly string _odooPassword;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public string? ApiKey { get; set; }
        public string OdooLink { get; private set; }
        public string StatusMessage { get; set; }

        public OdooModel(UserManager<ApplicationUser> userManager)
        {
            try
            {
                string configFilePath = "secrets.json";
                string jsonString = System.IO.File.ReadAllText(configFilePath);
                dynamic config = JsonConvert.DeserializeObject(jsonString);

                _adminToken = config.Odoo.AdminToken;
                _odooUrl = config.Odoo.Url;
                _odooDb = config.Odoo.Db;
                _odooUsername = config.Odoo.Username;
                _odooPassword = config.Odoo.Password;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error reading Odoo configuration: {ex.Message}";
            }

            _userManager = userManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Email == null) return RedirectToPage();

            ApiKey = await GenerateOdooApiKey(user.Email);
            if(ApiKey == null) return RedirectToPage();

            OdooLink = $"https://formsy.odoo.com?api_token={ApiKey}";

            return Page();
        }

        private async Task<string?> GenerateOdooApiKey(string userEmail)
        {


            var authRequest = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    service = "common",
                    method = "authenticate",
                    args = new object[] { _odooDb, _odooUsername, _odooPassword, new { email = userEmail } }
                },
                id = 1
            };

            using (var client = new HttpClient())
            {
                var jsonContent = JsonConvert.SerializeObject(authRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_odooUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(jsonResponse);
                    var userId = jsonObject["result"]?.ToString();

                    if (userId != null)
                    {
                        var apiKeyRequest = new
                        {
                            jsonrpc = "2.0",
                            method = "call",
                            @params = new
                            {
                                service = "object",
                                method = "execute",
                                args = new object[]
                                {
                            _odooDb,
                            userId,
                            _odooPassword,
                            "user.api.key",
                            "create",
                            new { user_id = userId }
                                }
                            },
                            id = 2
                        };

                        var apiKeyResponse = await client.PostAsync(_odooUrl, new StringContent(JsonConvert.SerializeObject(apiKeyRequest), Encoding.UTF8, "application/json"));

                        if (apiKeyResponse.IsSuccessStatusCode)
                        {
                            var apiKeyResponseBody = await apiKeyResponse.Content.ReadAsStringAsync();
                            var apiKeyJsonObject = JObject.Parse(apiKeyResponseBody);
                            var apiKey = apiKeyJsonObject["result"]?.ToString();
                            return apiKey ?? null;
                        }
                    }
                }
            }

            return null;
        }

    }
}