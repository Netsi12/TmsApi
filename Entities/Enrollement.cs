
namespace TmsApi.Entities;
public class Enrollment
{
public int Id { get; set; }
public int StudentId { get; set; }
public Student Student { get; set; } = null!;
public int CourseId { get; set; }
public Course Course { get; set; } = null!;
public decimal? Grade { get; set; }
 // Nullable, as student may be currently enrolled
public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
// Navigation properties back to entities
public int Year { get; set; }


}