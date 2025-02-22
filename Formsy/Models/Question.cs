namespace Formsy.Models
{
    public class Question
    {
        // Unique identifier for the question
        public int Id { get; set; }

        // The text or content of the question
        public string? Text { get; set; }

        // The type of the question (e.g., Single Line, Multi Line, Number, Checkbox)
        public string? Type { get; set; }

        // The Template to which this question belongs
        public Template? Template { get; set; }
    }
}
