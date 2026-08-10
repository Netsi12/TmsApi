
using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;

namespace TmsApi.Controllers;

using Microsoft.AspNetCore.Mvc;



[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase

{
    // GET /api/enrollments/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        // The courseId is required by the service.
        // This endpoint is not used by Step 5.
        return BadRequest(
            "Use GET /api/courses/{courseId}/enrollments/{id}.");
    }
}




