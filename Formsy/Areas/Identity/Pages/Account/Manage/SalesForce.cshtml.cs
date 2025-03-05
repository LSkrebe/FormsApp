using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;

namespace Formsy.Areas.Identity.Pages.Account.Manage
{
    public class SalesForceModel : PageModel
    {
        private readonly string _accessToken;
        private readonly string _instanceUrl;

        [BindProperty]
        public SalesForce SalesForce { get; set; }
        public string StatusMessage { get; set; }
        public bool AccountExists { get; set; } = false;

        public SalesForceModel()
        {
            try
            {
                string configFilePath = "secrets.json";
                string jsonString = System.IO.File.ReadAllText(configFilePath);
                dynamic config = JsonConvert.DeserializeObject(jsonString);

                _accessToken = config.Salesforce.AccessToken;
                _instanceUrl = config.Salesforce.InstanceUrl;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error reading Salesforce configuration: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    StatusMessage = "There are some validation errors.";
                    return Page();
                }

                string existingAccountId = await GetExistingAccountId(SalesForce.Name);

                if (existingAccountId != null)
                {
                    AccountExists = true;
                    return Page();
                }

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                    var accountData = new
                    {
                        Name = SalesForce.Name,
                        Phone = SalesForce.Phone,
                        Website = SalesForce.Website
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(accountData), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{_instanceUrl}/services/data/v63.0/sobjects/Account", content);

                    if (response.IsSuccessStatusCode)
                    {
                        AccountExists = true;
                        StatusMessage = $"Account '{SalesForce.Name}' created successfully in Salesforce!";
                    }
                    else
                    {
                        StatusMessage = "Failed to create account.";
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Exception during POST operation: {ex.Message}";
                return Page();
            }
        }

        private async Task<string> GetExistingAccountId(string accountName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                    string queryUrl = $"{_instanceUrl}/services/data/v63.0/query?q=SELECT+Id+FROM+Account+WHERE+Name='{Uri.EscapeDataString(accountName)}'";

                    var response = await client.GetAsync(queryUrl);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);

                    if (jsonResponse.totalSize > 0)
                    {
                        return jsonResponse.records[0].Id;
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Exception while checking existing account: {ex.Message}";
            }

            return null;
        }
    }
}
