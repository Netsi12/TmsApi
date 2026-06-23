using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EnrolledAt)
                .IsRequired();

            builder.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .IsRequired();

            builder.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .IsRequired();
                
    // TODO: Choose OnDelete behavior and express it in Fluent API.
// builder.HasMany(...).WithOne(...).OnDelete(DeleteBehavior.Restrict);
        
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Grade)
                .IsRequired();

            builder.Property(e => e.EnrolledAt)
                .IsRequired();

            // ✅ One-to-many: Student → Enrollment
            builder.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                // Prevent deleting a student if enrollments exist
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ One-to-many: Course → Enrollment
            builder.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                // Prevent deleting a course if enrollments exist
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
