namespace Formsy.Models
{
    public class Answer
    {
        // The unique identifier for the answer
        public int Id { get; set; }

        // The response given by the user, could be text, number, or other types based on the question type
        public string? Response { get; set; }

        // The associated Question that this answer is for
        public Question? Question { get; set; }

        // The Form that this answer belongs to
        public Form? Form { get; set; }
    }
}
