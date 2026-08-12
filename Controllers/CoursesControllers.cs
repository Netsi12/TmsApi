using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    IEnrollmentService enrollmentService,
    LinkGenerator linkGenerator) : ControllerBase
{
    // GET /api/courses
    // Returns paginated courses
    [HttpGet]
[ProducesResponseType(
    typeof(PagedResponse<CourseResponseDto>),
    StatusCodes.Status200OK)]
[EndpointSummary("List courses with pagination")]
[EndpointDescription(
    "Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(
            request,
            ct);

        return Ok(result);
    }


    // GET /api/courses/{id}
    // Returns one course with HATEOAS links
   [HttpGet("{id:int}", Name = nameof(GetCourseById))]
[ProducesResponseType(
    typeof(CourseDetailDto),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("Get a course by ID")]
[EndpointDescription(
    "Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<ActionResult<CourseDetailDto>> GetCourseById(
        int id,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(
            id,
            ct);

        if (course is null)
            return NotFound();


        // HATEOAS links
      var links = new List<LinkDto>
{
    // Self
    new(
        linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetCourseById),
            new { id })!,
        "self",
        "GET"),

    // Update
    new(
        linkGenerator.GetPathByName(
            HttpContext,
            nameof(Update),
            new { id })!,
        "update",
        "PUT"),

    // Delete
    new(
        linkGenerator.GetPathByName(
            HttpContext,
            nameof(Delete),
            new { id })!,
        "delete",
        "DELETE")
};


        // GET /api/courses/{courseId}/enrollments
        var enrollmentsUrl = linkGenerator.GetPathByName(
            HttpContext,
            "ListCourseEnrollments",
            new { courseId = id });

        if (enrollmentsUrl is not null)
        {
            links.Add(new LinkDto(
                enrollmentsUrl,
                "enrollments",
                "GET"));
        }


        // POST /api/courses/{courseId}/enrollments
        // Only include when there is available capacity.
        var createEnrollmentUrl = linkGenerator.GetPathByName(
            HttpContext,
            "CreateEnrollment",
            new { courseId = id });

        if (course.EnrollmentCount < course.MaxCapacity &&
            createEnrollmentUrl is not null)
        {
            links.Add(new LinkDto(
                createEnrollmentUrl,
                "create-enrollment",
                "POST"));
        }


        var response = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(response);
    }


    // POST /api/courses
    // Creates a new course
   [HttpPost]
[ProducesResponseType(
    typeof(CourseResponseDto),
    StatusCodes.Status201Created)]
[ProducesResponseType(
    typeof(ValidationProblemDetails),
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status409Conflict)]
[EndpointSummary("Create a new course")]
[EndpointDescription(
    "Creates a course with a unique code. Returns 409 if the course code already exists.")]
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

        var result = await courseService.CreateAsync(
            request,
            ct);

        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }


    // POST /api/courses/{courseId}/enrollments
    // Enrolls a student in a course
    [HttpPost(
    "{courseId:int}/enrollments",
    Name = "CreateEnrollment")]
[ProducesResponseType(
    typeof(EnrollmentResponseDto),
    StatusCodes.Status201Created)]
[ProducesResponseType(
    typeof(ValidationProblemDetails),
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status409Conflict)]
[EndpointSummary("Enrol a student in a course")]
[EndpointDescription(
    "Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        [FromBody] EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Find the course first
        var course = await courseService.GetByIdAsync(
            courseId,
            ct);

        if (course is null)
        {
            return NotFound();
        }


        // Check capacity
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


        // Create enrollment
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
    // PUT /api/courses/{id}
[HttpPut("{id:int}", Name = nameof(Update))]
[ProducesResponseType(
    typeof(CourseResponseDto),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("Update a course")]
[EndpointDescription("Updates the details of an existing course.")]
public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateCourseRequest request,
    CancellationToken ct)
{
    var result = await courseService.UpdateAsync(
        id,
        request,
        ct);

    if (result is null)
        return NotFound();

    return Ok(result);
}

// DELETE /api/courses/{id}
[HttpDelete("{id:int}", Name = nameof(Delete))]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("Delete a course")]
[EndpointDescription("Deletes an existing course.")]
public async Task<IActionResult> Delete(
    int id,
    CancellationToken ct)
{
    var deleted = await courseService.DeleteAsync(
        id,
        ct);

    if (!deleted)
        return NotFound();

    return NoContent();
}
}


