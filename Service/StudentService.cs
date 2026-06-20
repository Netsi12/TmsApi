public interface IStudentService
{
    Task<StudentRecord> AddAsync(string id, string name);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
public class StudentService : IStudentService
{
    private readonly Dictionary<string, StudentRecord> _store = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public Task<StudentRecord> AddAsync(string id, string name)
    {
        var record = new StudentRecord(id, name, DateTime.UtcNow);
        _store[id] = record;

        _logger.LogInformation(
            "Added student {StudentId} with name {StudentName}",
            id, name);

        return Task.FromResult(record);
    }

    public Task<StudentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        IReadOnlyList<StudentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        if (_store.Remove(id))
        {
            _logger.LogInformation("Deleted student {StudentId}", id);
            return Task.FromResult(true);
        }
        else
        {
            _logger.LogWarning("Attempted to delete non-existent student {StudentId}", id);
            return Task.FromResult(false);
        }
    }
}
public record StudentRecord(
    string Id,
    string Name,
    DateTime CreatedAt);

