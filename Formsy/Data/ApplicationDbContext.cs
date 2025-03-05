using Formsy.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Formsy.Data
{
    // ApplicationDbContext is the EF Core context class for managing the application's data
    // Inherits from IdentityDbContext to support ASP.NET Identity for user management
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor to pass options to the base IdentityDbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets represent tables in the database for different models
        public DbSet<Template> Templates { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Answer> Answers { get; set; }

        // OnModelCreating is used to configure relationships and other aspects of the models
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Template's relationship with ApplicationUser
            modelBuilder.Entity<Template>()
                .HasOne(t => t.CreatedBy) // Each Template has one created by a User
                .WithMany(u => u.Templates) // One User can have many Templates
                .OnDelete(DeleteBehavior.NoAction); // No action when the User is deleted

            // Configure Form's relationship with ApplicationUser
            modelBuilder.Entity<Form>()
                .HasOne(f => f.SubmittedBy) // Each Form has one submitted by a User
                .WithMany(u => u.Forms) // One User can submit many Forms
                .OnDelete(DeleteBehavior.NoAction); // No action when the User is deleted

            // Configure Form's relationship with Template
            modelBuilder.Entity<Form>()
                .HasOne(f => f.Template) // Each Form has one Template
                .WithMany(t => t.Forms) // One Template can have many Forms
                .OnDelete(DeleteBehavior.Cascade); // Delete associated Forms if the Template is deleted

            // Configure Answer's relationship with Form
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Form) // Each Answer belongs to one Form
                .WithMany(f => f.Answers) // One Form can have many Answers
                .OnDelete(DeleteBehavior.Cascade); // Delete associated Answers if the Form is deleted

            // Configure Answer's relationship with Question
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question) // Each Answer belongs to one Question
                .WithMany() // A Question can have many Answers (no navigation property on Question)
                .OnDelete(DeleteBehavior.Cascade); // Delete associated Answers if the Question is deleted
        }
    }
}
