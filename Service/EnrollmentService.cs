
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Find the student
        var student = await context.Students
            .FirstOrDefaultAsync(
                s => s.Id == request.StudentId,
                ct);

        if (student is null)
        {
            throw new KeyNotFoundException(
                $"Student '{request.StudentId}' was not found.");
        }

        // Find the course
        var course = await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == courseId,
                ct);

        if (course is null)
        {
            throw new KeyNotFoundException(
                $"Course '{courseId}' was not found.");
        }

        // Check duplicate enrollment
        var existing = await context.Enrollments
            .FirstOrDefaultAsync(
                e =>
                    e.StudentId == request.StudentId &&
                    e.CourseId == courseId,
                ct);

        if (existing is not null)
        {
            logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} in course {CourseId}",
                request.StudentId,
                courseId);

            return new EnrollmentResponseDto(
                existing.Id,
                existing.StudentId,
                existing.CourseId,
                existing.EnrolledAt);
        }

        // Create enrollment
        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Enrolled student {StudentId} in course {CourseId}, enrollment {EnrollmentId}",
            request.StudentId,
            courseId,
            enrollment.Id);

        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.CourseId,
            enrollment.EnrolledAt);
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e =>
                    e.Id == id &&
                    e.CourseId == courseId,
                ct);

        if (enrollment is null)
        {
            return null;
        }

        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.CourseId,
            enrollment.EnrolledAt);
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.StudentId,
                e.CourseId,
                e.EnrolledAt))
            .ToListAsync(ct);
    }
}



