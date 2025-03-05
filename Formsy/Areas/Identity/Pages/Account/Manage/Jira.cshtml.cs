using Formsy.Data;
using Formsy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Formsy.Areas.Identity.Pages.Account.Manage
{
    public class JiraModel : PageModel
    {
        private readonly string _token;
        private readonly string _key;
        private readonly string _username;
        private readonly string _url;
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public Jira Jira { get; set; }

        public string StatusMessage { get; set; }
        public List<Template> Templates { get; set; } = new List<Template>();

        public JiraModel(ApplicationDbContext context)
        {
            try
            {
                string configFilePath = "secrets.json";
                string jsonString = System.IO.File.ReadAllText(configFilePath);
                dynamic config = JsonConvert.DeserializeObject(jsonString);

                _token = config.Jira.Token;
                _key = config.Jira.Key;
                _username = config.Jira.Username;
                _url = config.Jira.Url;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error reading Jira configuration: {ex.Message}";
            }

            _context = context;
        }

        public async Task OnGetAsync()
        {
            await LoadTemplates();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Jira.Title) || string.IsNullOrEmpty(Jira.Description))
                {
                    StatusMessage = "Title and Description are required fields.";

                    await LoadTemplates();

                    return Page();
                }

                var jiraUrl = $"{_url}/rest/api/3/issue";

                var issueData = new
                {
                    fields = new
                    {
                        project = new { key = _key },
                        summary = Jira.Title,
                        description = new
                        {
                            type = "doc",
                            version = 1,
                            content = new[]
                            {
                                new
                                {
                                    type = "paragraph",
                                    content = new[]
                                    {
                                        new
                                        {
                                            type = "text",
                                            text = Jira.Description
                                        }
                                    }
                                }
                            }
                        },
                        issuetype = new { name = "Bug" },
                        customfield_10036 = Jira.TemplateId
                    }
                };

                using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_token}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    var json = JsonConvert.SerializeObject(issueData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(jiraUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        StatusMessage = "Ticket created successfully!";
                    }
                    else
                    {
                        StatusMessage = $"Error creating ticket: {response.ReasonPhrase}";
                    }

                    await LoadTemplates();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Exception during POST operation: {ex.Message}";
            }

            return Page();
        }
        private async Task LoadTemplates()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                Templates = await _context.Templates
                    .Where(f => f.CreatedBy != null && f.CreatedBy.Id == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error fetching templates: {ex.Message}";
            }
        }

    }
}
