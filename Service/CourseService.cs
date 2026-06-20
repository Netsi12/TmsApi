public interface ICourseService
{
    Task<CourseRecord> AddAsync(string code, string title);
    Task<CourseRecord?> GetByCodeAsync(string code);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}
public class CourseService : ICourseService
{
    private readonly Dictionary<string, CourseRecord> _store = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<CourseRecord> AddAsync(string code, string title)
    {
        var record = new CourseRecord(code, title, DateTime.UtcNow);
        _store[code] = record;

        _logger.LogInformation(
            "Added course {CourseCode} with title {CourseTitle}",
            code, title);

        return Task.FromResult(record);
    }

    public Task<CourseRecord?> GetByCodeAsync(string code)
    {
        _store.TryGetValue(code, out var record);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        IReadOnlyList<CourseRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string code)
    {
        if (_store.Remove(code))
        {
            _logger.LogInformation("Deleted course {CourseCode}", code);
            return Task.FromResult(true);
        }
        else
        {
            _logger.LogWarning("Attempted to delete non-existent course {CourseCode}", code);
            return Task.FromResult(false);
        }
    }
}
public record CourseRecord(
    string Code,
    string Title,
    DateTime CreatedAt);