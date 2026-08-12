namespace TmsApi.Dtos;

public record UpdateCourseRequest(
    string Code,
    string Title,
    int MaxCapacity);