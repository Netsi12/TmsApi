public class CourseWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    // SOLUTION: Inject the scope factory, not the scoped service
    public CourseWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        // SOLUTION: Create a short-lived scope for this operation
        using var scope = _scopeFactory.CreateScope();

        // SOLUTION: Get the scoped service from the temporary scope
        var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();

        // Now use the service safely
        var courses = courseService.GetAllAsync().Result;
        Console.WriteLine($"Processing {courses.Count} courses for scholarship recalculation");

        foreach (var course in courses)
        {
            Console.WriteLine($"Recalculating scholarship impact for course {course.Title} (Code: {course.Code})");
        }

        // The 'using' automatically disposes the scope and its services
    }
}
