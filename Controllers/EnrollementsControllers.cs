using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    IEnrollmentService enrollmentService,
    ICourseService courseService) : ControllerBase
{
    // GET /api/enrollments/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(
    typeof(EnrollmentResponseDto),
    StatusCodes.Status200OK)]
    [ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrolment for a course")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        return BadRequest(
            "Use GET /api/courses/{courseId}/enrollments/{id}.");
    }


    // GET /api/courses/{courseId}/enrollments
    [HttpGet(
        "/api/courses/{courseId:int}/enrollments",
        Name = "ListCourseEnrollments")]
        [ProducesResponseType(
    typeof(IReadOnlyList<EnrollmentResponseDto>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrolments for a course")]
    public async Task<IActionResult> GetByCourseId(
        int courseId,
        CancellationToken ct)
    {
        // Check that the course exists
        var course = await courseService.GetByIdAsync(
            courseId,
            ct);

        if (course is null)
            return NotFound();

        // Get enrollments for this course
        var enrollments = await enrollmentService.GetByCourseIdAsync(
            courseId,
            ct);

        return Ok(enrollments);
    }


    // GET /api/courses/{courseId}/enrollments/{id}
    [HttpGet(
        "/api/courses/{courseId:int}/enrollments/{id:int}",
        Name = "GetEnrollment")]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        // Check that the course exists
        var course = await courseService.GetByIdAsync(
            courseId,
            ct);

        if (course is null)
            return NotFound();

        // Get the enrollment
        var enrollment = await enrollmentService.GetByIdAsync(
            courseId,
            id,
            ct);

        if (enrollment is null)
            return NotFound();

        return Ok(enrollment);
    }
}



