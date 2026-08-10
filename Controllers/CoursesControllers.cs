
using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(
        int id,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);

        return course is not null
            ? Ok(course)
            : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var exists = await courseService.CodeExistsAsync(
            request.Code,
            ct);

        if (exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail =
                    $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await courseService.CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }

    // POST /api/courses/{courseId}/enrollments
    [HttpPost("{courseId:int}/enrollments")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        [FromBody] EnrollStudentRequest request,
        CancellationToken ct)
    {
        // 1. Find the course first
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
        {
            return NotFound();
        }

        // 2. Check capacity
        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Status = StatusCodes.Status409Conflict,
                Detail =
                    $"Course '{course.Code}' has reached its maximum capacity of {course.MaxCapacity}."
            });
        }

        // 3. Create enrollment in the database
        var enrollment = await enrollmentService.CreateAsync(
            courseId,
            request,
            ct);

        return CreatedAtAction(
            nameof(EnrollmentsController.GetById),
            "Enrollments",
            new { id = enrollment.Id },
            enrollment);
    }
}


