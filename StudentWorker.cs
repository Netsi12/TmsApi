using Microsoft.Extensions.DependencyInjection;
public class StudentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    // SOLUTION: Inject the scope factory, not the scoped service
    public StudentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        // SOLUTION: Create a short-lived scope for this operation
        using var scope = _scopeFactory.CreateScope();

        // SOLUTION: Get the scoped service from the temporary scope
        var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();

        // Now use the service safely
        var students = studentService.GetAllAsync().Result;
        Console.WriteLine($"Processing {students.Count} students for scholarship recalculation");

        foreach (var student in students)
        {
            Console.WriteLine($"Recalculating scholarship for student {student.Name} (ID: {student.Id})");
        }

        // The 'using' automatically disposes the scope and its services
    }
}
