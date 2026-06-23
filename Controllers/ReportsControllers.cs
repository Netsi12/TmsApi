using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Controllers
{
    [ApiController]
    [Route("api/reports")]
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
        // TODO1:Pagination OrderBy, Skip((page- 1) * pageSize), Take(pageSize), ToListAsync with CancellationToken.
        [HttpGet("students")]
     public async Task<IActionResult> GetStudents(int page = 1, int pageSize = 5, CancellationToken cancellationToken = default)
  {
    var students = await _context.Students
        .OrderBy(s => s.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

     return Ok(new { Page = page, PageSize = pageSize, Data = students });
  }

       // TODO2:Top5coursesby enrollment GroupBy, order by count, Take(5).
       [HttpGet("top-5-courses")]
       public async Task<IActionResult> GetTop5Courses(CancellationToken cancellationToken = default)
       {
           var top5Courses = await _context.Courses
               .Select(c => new { c.Title, EnrollmentCount = c.Enrollments.Count })
               .OrderByDescending(x => x.EnrollmentCount)
               .Take(5)
               .ToListAsync(cancellationToken);

           return Ok(top5Courses);
       }
   }
}




