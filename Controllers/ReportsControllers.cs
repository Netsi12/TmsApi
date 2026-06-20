using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly TmsDbContext _context;

        public ReportsController(TmsDbContext context)
        {
            _context = context;
        }

        // 1. Active students with GPA >= 3.0
        [HttpGet("active-students-count")]
        public async Task<IActionResult> GetActiveStudentsCount()
        {
            var count = await _context.Students
                .Where(s => s.IsActive && s.GPA >= 3.0m)
                .CountAsync();

            return Ok(count);
        }

        // 2. Courses by enrollment count
        [HttpGet("courses-by-enrollments")]
        public async Task<IActionResult> GetCoursesByEnrollments()
        {
            var list = await _context.Courses
                .Select(c => new { c.Title, EnrollmentCount = c.Enrollments.Count })
                .OrderByDescending(x => x.EnrollmentCount)
                .ToListAsync();

            return Ok(list);
        }

        // 3. Average GPA per course
        [HttpGet("average-gpa-per-course")]
        public async Task<IActionResult> GetAverageGpaPerCourse()
        {
            var list = await _context.Enrollments
                .GroupBy(e => e.Course.Title)
                .Select(g => new
                {
                    Course = g.Key,
                    AverageGPA = g.Average(e => e.Student.GPA)
                })
                .ToListAsync();

            return Ok(list);
        }

        // 4A. Students with zero enrollments (NOT EXISTS)
        [HttpGet("students-without-enrollments-a")]
        public async Task<IActionResult> GetStudentsWithoutEnrollmentsA()
        {
            var list = await _context.Students
                .Where(s => !s.Enrollments.Any())
                .Select(s => s.Name)
                .ToListAsync();

            return Ok(list);
        }

        // 4B. Students with zero enrollments (LEFT JOIN)
        [HttpGet("students-without-enrollments-b")]
        public async Task<IActionResult> GetStudentsWithoutEnrollmentsB()
        {
            var list = await _context.Students
                .LeftJoin(_context.Enrollments,
                          s => s.Id,
                          e => e.StudentId,
                          (s, e) => new { s, e })
                .Where(x => x.e == null)
                .Select(x => x.s.Name)
                .ToListAsync();

            return Ok(list);
        }
    }
}




