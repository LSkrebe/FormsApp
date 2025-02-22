using Formsy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Formsy.Data;
using Microsoft.AspNetCore.Authorization;

namespace Formsy.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to initialize context and user manager
        public TemplateController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Displays all public templates with related data
        public async Task<IActionResult> Index()
        {
            var templates = await _context.Templates
                .Where(t => t.IsPublic)
                .Include(t => t.CreatedBy)
                .Include(t => t.Questions)
                .Include(t => t.Forms)
                .ToListAsync();

            return View(templates);
        }

        // GET: Displays details of a specific template
        [HttpGet("Template/Details/{templateId}")]
        public async Task<IActionResult> Details(int templateId)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.Forms)
                    .ThenInclude(f => f.Answers)
                .Include(t => t.Forms)
                    .ThenInclude(f => f.SubmittedBy)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return NotFound();

            return View("Create", template);
        }

        // GET: Displays the view for creating a new template (Authorized users only)
        [Authorize]
        public IActionResult Create()
        {
            var model = new Template();

            return View(model);
        }

        // POST: Creates or updates a template (Authorized users only)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? templateId, Template model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(model);

            Template? template;
            if (templateId.HasValue && templateId > 0)
            {
                // Editing an existing template
                template = await _context.Templates
                    .Include(t => t.Questions)
                    .Include(t => t.Forms)
                        .ThenInclude(f => f.Answers)
                    .FirstOrDefaultAsync(t => t.Id == templateId.Value);

                if (template == null)
                    return NotFound();

                // Update template properties
                template.Title = model.Title;
                template.Description = model.Description;
                template.Topic = model.Topic;
                template.IsPublic = model.IsPublic;

                // Map existing questions by ID and update or add them
                var existingQuestions = template.Questions.ToDictionary(q => q.Id, q => q);
                var updatedQuestions = new List<Question>();

                foreach (var newQuestion in model.Questions)
                {
                    if (newQuestion.Id > 0 && existingQuestions.TryGetValue(newQuestion.Id, out var existingQuestion))
                    {
                        // Update existing question
                        existingQuestion.Text = newQuestion.Text;
                        existingQuestion.Type = newQuestion.Type;
                        updatedQuestions.Add(existingQuestion);
                    }
                    else
                    {
                        // Add new question
                        newQuestion.Template = template;
                        updatedQuestions.Add(newQuestion);
                    }
                }

                // Identify removed questions
                var removedQuestions = template.Questions
                    .Where(q => !updatedQuestions.Contains(q))
                    .ToList();

                // Remove associated answers of deleted questions
                foreach (var removedQuestion in removedQuestions)
                {
                    var relatedAnswers = _context.Answers.Where(a => a.Question.Id == removedQuestion.Id);
                    _context.Answers.RemoveRange(relatedAnswers);
                }

                _context.Questions.RemoveRange(removedQuestions);
                template.Questions = updatedQuestions;

                // Update all submitted forms to reflect question changes
                foreach (var form in template.Forms)
                {
                    var updatedAnswers = new List<Answer>();

                    foreach (var question in template.Questions)
                    {
                        var existingAnswer = form.Answers.FirstOrDefault(a => a.Question.Id == question.Id);
                        if (existingAnswer != null)
                        {
                            // Update existing answer for changed question type if necessary
                            // Handle based on question type
                            switch (question.Type)
                            {
                                case "Single Line":
                                case "Multi Line":
                                    existingAnswer.Response = existingAnswer.Response ?? string.Empty;
                                    break;

                                case "Number":
                                    if (!int.TryParse(existingAnswer.Response, out var numericResponse))
                                    {
                                        existingAnswer.Response = "";
                                    }
                                    break;

                                case "Checkbox":
                                    bool checkboxResponse = existingAnswer.Response?.ToLower() == "true";
                                    existingAnswer.Response = checkboxResponse ? "true" : "false";
                                    break;
                            }
                            updatedAnswers.Add(existingAnswer);
                        }
                        else
                        {
                            updatedAnswers.Add(new Answer
                            {
                                Question = question,
                                Response = question.Type switch
                                {
                                    "Single Line" => string.Empty,
                                    "Multi Line" => string.Empty,
                                    "Number" => "",
                                    "Checkbox" => "false",
                                    _ => string.Empty,
                                }
                            });
                        }
                    }

                    form.Answers = updatedAnswers;
                }

                _context.Templates.Update(template);
            }
            else
            {
                // Creating a new template
                template = new Template
                {
                    Title = model.Title,
                    Description = model.Description,
                    Topic = model.Topic,
                    IsPublic = model.IsPublic,
                    CreatedBy = user,
                    Questions = model.Questions.Select(q => new Question
                    {
                        Text = q.Text,
                        Type = q.Type
                    }).ToList()
                };

                _context.Templates.Add(template);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Form");
        }

        // POST: Deletes a template (Authorized users only)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Template/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.Templates
                .Include(t => t.Forms)
                .ThenInclude(f => f.Answers)
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return NotFound();

            // Remove related Questions, Forms, and the Template itself
            _context.Questions.RemoveRange(template.Questions);
            _context.Forms.RemoveRange(template.Forms);
            _context.Templates.Remove(template);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Form");
        }

        // GET: Loads public templates with pagination support
        public async Task<IActionResult> LoadTemplates(int page = 1)
        {
            int pageSize = 9;

            var templates = await _context.Templates
                .Where(t => t.IsPublic)
                .Include(t => t.CreatedBy)
                .Include(t => t.Questions)
                .Include(t => t.Forms)
                .OrderBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Json(templates);
        }
    }
}
