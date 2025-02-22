using Formsy.Models;
using System.ComponentModel.DataAnnotations;

namespace Formsy.Models
{
    public class Template
    {
        // Unique identifier for the template
        public int Id { get; set; }

        // The title of the template (required and limited to 255 characters)
        [Required, MaxLength(255)]
        public string? Title { get; set; }

        // A description of the template (optional)
        public string? Description { get; set; }

        // The topic or category of the template (optional)
        public string? Topic { get; set; }

        // Determines if the template is publicly accessible (defaults to true)
        public bool IsPublic { get; set; } = true;

        // Reference to the user who created this template
        public ApplicationUser? CreatedBy { get; set; }

        // Collection of questions associated with this template
        public ICollection<Question> Questions { get; set; } = new List<Question>();

        // Collection of forms that have been submitted using this template
        public ICollection<Form> Forms { get; set; } = new List<Form>();
    }
}
