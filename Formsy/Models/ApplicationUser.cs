using System.Collections.Generic;
using Formsy.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // A collection of Forms that have been submitted by the user
    public ICollection<Form> Forms { get; set; } = new List<Form>();

    // A collection of Templates that have been created by the user
    public ICollection<Template> Templates { get; set; } = new List<Template>();
}
