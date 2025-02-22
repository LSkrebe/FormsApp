using System;
using System.Collections.Generic;

namespace Formsy.Models
{
    public class Form
    {
        // Unique identifier for the form
        public int Id { get; set; }

        // The date and time the form was submitted
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // The user who submitted the form (optional relationship)
        public ApplicationUser? SubmittedBy { get; set; }

        // The template associated with the form (optional relationship)
        public Template? Template { get; set; }

        // A collection of answers provided in the form
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
