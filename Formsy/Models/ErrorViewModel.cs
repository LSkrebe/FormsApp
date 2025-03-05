namespace Formsy.Models
{
    public class ErrorViewModel
    {
        // Represents the unique request identifier for tracking purposes
        public string? RequestId { get; set; }

        // Property that returns true if the RequestId is not null or empty, used to determine if the request ID should be shown in the error view
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
