using System.ComponentModel.DataAnnotations;

namespace Formsy.Areas.Identity.Pages.Account.Manage
{
    public class SalesForce
    {
        [Required]
        public string? Name { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Url]
        public string? Website { get; set; }
    }
}
