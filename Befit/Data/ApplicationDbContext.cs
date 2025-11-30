using BeFit.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<ExerciseType> ExerciseTypes { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<PerformedExercise> PerformedExercises { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ExerciseType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            builder.Entity<Workout>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.UserId).IsRequired();
            });

            builder.Entity<PerformedExercise>(entity =>
            {
                entity.HasKey(pe => pe.Id);
            });
        }
    }
}