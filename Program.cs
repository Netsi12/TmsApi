using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi(); // Required for MapOpenApi and MapScalarApiReference

builder.Services.AddControllers();

// Authentication / Authorization
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

// Services
builder.Services.AddSingleton<EnrollmentWorker>();         
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>(); 

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart(); 

// ProblemDetails registration
builder.Services.AddProblemDetails();

builder.Services.AddProblemDetails();
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();

// Middleware order
app.UseMiddleware<RequestLoggingMiddleware>(); // outer wrapper
// UseProblemDetails() is provided by external packages (e.g. Hellang.Middleware.ProblemDetails).
// If that package/using is not available, remove the call and rely on ExceptionHandler/StatusCodePages.
// app.UseProblemDetails();                        // format errors/status codes as JSON
app.UseExceptionHandler();                      // catch exceptions
app.UseStatusCodePages();                       // wrap empty-body codes
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
 app.MapOpenApi(); 
app.MapScalarApiReference();
 // Required for MapOpenApi and MapScalarApiReference  
// Endpoints
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001", 
    letterGrade = "A"
})).RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

app.MapPost("/api/enrollments/test", async (IEnrollmentService service) =>
{
    // … your test logic …
    return Results.Ok("Logging test completed - check console for structured logs");
});

// Test route that throws
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

app.MapControllers();
app.Run();

public class PaymentOptions
 {
 }








