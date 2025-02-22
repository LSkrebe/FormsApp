using Formsy.Data;
using Formsy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Formsy.Controllers
{
    [Authorize]
    public class FormController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor to initialize the context
        public FormController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Displays forms submitted by the current user and templates created by them
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve forms submitted by the user
            var submittedForms = await _context.Forms
                .Include(f => f.Template)
                .Include(f => f.Answers)
                .Include(f => f.SubmittedBy)
                .Include(f => f.Template.Questions)
                .Where(f => f.SubmittedBy != null && f.SubmittedBy.Id == userId)
                .ToListAsync();

            // Retrieve templates created by the user
            var createdTemplates = await _context.Templates
                .Include(f => f.Questions)
                .Include(f => f.Forms)
                .Include(f => f.CreatedBy)
                .Where(f => f.CreatedBy != null && f.CreatedBy.Id == userId)
                .ToListAsync();

            // Pass data to the view using ViewBag
            ViewBag.CreatedTemplates = createdTemplates ?? new List<Template>();
            ViewBag.SubmittedForms = submittedForms ?? new List<Form>();

            return View();
        }

        // GET: Display the form filling page based on template
        [HttpGet("Form/Fill/{templateId}")]
        public async Task<IActionResult> Fill(int templateId)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return NotFound();

            return View(template);
        }

        // GET: View the details of a specific form
        [HttpGet("Form/Details/{formId}")]
        public async Task<IActionResult> Details(int formId)
        {
            var form = await _context.Forms
                .Include(f => f.Template)
                .Include(f => f.Template.Questions)
                .Include(f => f.Answers)
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null || form.Template == null)
                return NotFound();

            // Pass form data to the view
            ViewBag.Form = form;
            return View("Fill", form.Template);
        }

        // POST: Submit the form and save the responses or update an existing form
        [HttpPost]
        public async Task<IActionResult> Submit(int templateId, int? formId, Dictionary<int, string> answers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            // Retrieve the template to ensure it exists
            var template = await _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return NotFound();

            // Ensure answers are provided
            if (answers == null || !answers.Any())
                return BadRequest("Answers cannot be empty.");

            var user = await _context.Users.FindAsync(userId);

            Form? form;

            // Check if this is an existing form or a new submission
            if (formId.HasValue)
            {
                form = await _context.Forms
                    .Include(f => f.Answers)
                    .FirstOrDefaultAsync(f => f.Id == formId.Value && f.SubmittedBy.Id == userId);

                if (form == null)
                    return NotFound();

                // Update the answers in the existing form
                foreach (var answer in form.Answers)
                {
                    if (answers.ContainsKey(answer.Question.Id))
                    {
                        answer.Response = answers[answer.Question.Id];
                    }
                }

                form.SubmittedAt = DateTime.UtcNow;
            }
            else
            {
                // Create a new form with the provided answers
                form = new Form
                {
                    Template = template,
                    SubmittedBy = user,
                    SubmittedAt = DateTime.UtcNow,
                    Answers = answers.Select(a => new Answer
                    {
                        Question = _context.Questions.Find(a.Key),
                        Response = a.Value
                    }).ToList()
                };

                _context.Forms.Add(form);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect to the index after submission
            return RedirectToAction("Index");
        }

    }
}
