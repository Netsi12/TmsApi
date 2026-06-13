using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

// var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddControllers();

// builder.Services
//     .AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
// builder.Services.AddAuthorization();
// // builder.Services.AddOpenApi();


// builder.Services.AddSingleton<EnrollmentWorker>();         
// builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>(); 


// builder.Services.AddOptions<PaymentOptions>()
//     .BindConfiguration("Payments")
//     .ValidateDataAnnotations()
//     .ValidateOnStart(); 
// // Configure ProblemDetails (use default settings)
// builder.Services.AddProblemDetails();

// builder.Host.UseDefaultServiceProvider(options =>
// {
//     options.ValidateScopes = true;
//     options.ValidateOnBuild = true;
// });

// var app = builder.Build();


// app.UseMiddleware<RequestLoggingMiddleware>(); // FIRST - outer wrapper
// app.UseExceptionHandler();                    // Exception handling
// app.UseStatusCodePages();                    // ProblemDetails for empty-body status codes
// app.UseHttpsRedirection();                    // HTTPS redirect
// app.UseRouting();                            // Routing
// app.UseAuthentication();                     // Authentication
// app.UseAuthorization();                      // Authorization


// // Map the protected endpoint
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001", 
//     letterGrade = "A"
// })).RequireAuthorization();

// // EXERCISE 2: Test route to trigger the captive dependency
// app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
// {
//     worker.ProcessBatch();
//     return Results.Ok("processed");
// });

// // EXERCISE 4: Test endpoints for logging
// app.MapPost("/api/enrollments/test", async (IEnrollmentService service) =>
// {
//     Console.WriteLine("=== TESTING DUPLICATE ENROLLMENT ===");
    
//     // First enrollment - should succeed
//     Console.WriteLine("1. First enrollment attempt...");
//     var enrollment1 = await service.EnrollAsync("S-001", "CS-101");
    
//     // Second enrollment - should show duplicate warning
//     Console.WriteLine("2. Second enrollment attempt (same student, same course)...");
//     var enrollment2 = await service.EnrollAsync("S-001", "CS-101");
    
//     Console.WriteLine("=== TESTING MISSING RECORDS ===");
    
//     // Test existing record
//     Console.WriteLine("3. Looking for existing record...");
//     var found = await service.GetByIdAsync(enrollment1.Id);
    
//     // Test missing record  
//     Console.WriteLine("4. Looking for non-existent record...");
//     var notFound = await service.GetByIdAsync("nonexistent");
    
//     Console.WriteLine("=== TESTING DELETE ===");
    
//     // First delete - should succeed
//     Console.WriteLine("5. First delete attempt...");
//     var deleted = await service.DeleteAsync(enrollment1.Id);
    
//     // Second delete - should show not found warning
//     Console.WriteLine("6. Second delete attempt (already deleted)...");
//     var deletedAgain = await service.DeleteAsync(enrollment1.Id);
    
//     return Results.Ok("Logging test completed - check console for structured logs");
// });

// app.MapGet("/api/error", () =>
// {
//     throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
// });

// app.MapControllers();
// app.Run();

// public class PaymentOptions
// {
// }

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


// var builder = WebApplication.CreateBuilder(args);

// ... your existing service registrations ...
// builder.Services.AddControllers();





