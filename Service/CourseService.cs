using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger) : ICourseService
{
    public Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created course {CourseId} ({Code})",
            course.Id,
            course.Code);

        return (await GetByIdAsync(course.Id, ct))!;
    }
    public async Task<CourseResponseDto?> UpdateAsync(
    int id,
    UpdateCourseRequest request,
    CancellationToken ct)
{
    var course = await context.Courses
        .FirstOrDefaultAsync(c => c.Id == id, ct);

    if (course is null)
        return null;

    course.Code = request.Code;
    course.Title = request.Title;
    course.MaxCapacity = request.MaxCapacity;

    await context.SaveChangesAsync(ct);

    logger.LogInformation(
        "Updated course {CourseId} ({Code})",
        course.Id,
        course.Code);

    return await GetByIdAsync(course.Id, ct);
}


public async Task<bool> DeleteAsync(
    int id,
    CancellationToken ct)
{
    var course = await context.Courses
        .FirstOrDefaultAsync(c => c.Id == id, ct);

    if (course is null)
        return false;

    context.Courses.Remove(course);

    await context.SaveChangesAsync(ct);

    logger.LogInformation(
        "Deleted course {CourseId} ({Code})",
        course.Id,
        course.Code);

    return true;
}

    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct)
    {
        return context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        // TODO 1: Start with a no-tracking IQueryable
        var query = context.Courses
            .AsNoTracking();

        // TODO 2: Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{search}%") ||
                EF.Functions.ILike(c.Code, $"%{search}%"));
        }

        // TODO 3: Count BEFORE Skip/Take
        var totalCount = await query.CountAsync(ct);

        // TODO 4: Apply safe sorting
        IQueryable<Entities.Course> sortedQuery;

        switch (request.OrderBy)
        {
            case "Code":
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.Code)
                    : query.OrderBy(c => c.Code);
                break;

            case "MaxCapacity":
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.MaxCapacity)
                    : query.OrderBy(c => c.MaxCapacity);
                break;

            case "Title":
            default:
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.Title)
                    : query.OrderBy(c => c.Title);
                break;
        }

        // TODO 5: Apply pagination and projection
        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToListAsync(ct);

        // TODO 6: Return paged response
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}