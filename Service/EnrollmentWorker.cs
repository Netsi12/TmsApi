
using Microsoft.Extensions.DependencyInjection;
using TmsApi.Services;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ProcessBatchAsync(
        CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();

        var enrollmentService =
            scope.ServiceProvider
                .GetRequiredService<IEnrollmentService>();

        var enrollments =
            await enrollmentService.GetAllAsync(ct);

        Console.WriteLine(
            $"Processing {enrollments.Count} enrollments for scholarship recalculation");

        foreach (var enrollment in enrollments)
        {
            Console.WriteLine(
                $"Recalculating scholarship for student " +
                $"{enrollment.StudentId} in course " +
                $"{enrollment.CourseId}");
        }
    }
}


