using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI + Scalar
builder.Services.AddOpenApi(); // Required for MapOpenApi and MapScalarApiReference

// Controllers
builder.Services.AddControllers();

// DbContext
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging());

// Authentication / Authorization
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

// Services
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddSingleton<StudentWorker>();
// builder.Services.AddSingleton<CourseWorker>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ProblemDetails
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TmsApi", Version = "v1" });
});

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));
builder.Services.AddControllers();

builder.Services.AddScoped<ICourseService, CourseService>();
var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.Run();

// var app = builder.Build();

// Middleware order
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TmsApi v1");
        c.RoutePrefix = string.Empty; // serve at root
    });
}

// OpenAPI + Scalar
app.MapOpenApi();
app.MapScalarApiReference();

// Example endpoints
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

//m5-exercise 2- add logging and sensitive data to db context step-2
// Seed test data at startup
using (var scope = app.Services.CreateScope())
{
var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact
if (!context.Students.Any())
{
var students = new List<Student>
{
new() { RegistrationNumber = "TMS-2026-0001", Name = "AliceSmith", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true }
};
context.Students.AddRange(students);
var courses = new List<Course>
{
    new() { Code = "CS-101", Title = "Introduction to ComputerScience" },
    new() { Code = "CS-201", Title = "Data Structures and Algorithms" },
    new() { Code = "MAT-101", Title = "Calculus I" }
};
context.Courses.AddRange(courses);
context.SaveChanges();
var enrollments = new List<Enrollment>
{
new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
};
context.Enrollments.AddRange(enrollments);
context.SaveChanges();
}
}
app.MapControllers();
app.Run();

public class PaymentOptions { }










