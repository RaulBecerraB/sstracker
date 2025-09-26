var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "SS Tracker API",
        Version = "v1",
        Description = "API for managing schedule tracking and advisor availability"
    });
    c.EnableAnnotations();
    
    // Configure servers for Swagger to show clean paths
    c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
    {
        Url = "/api/v1",
        Description = "SS Tracker API v1"
    });
    
    // Remove /api/v1 prefix from paths shown in Swagger
    c.DocumentFilter<RemoveVersionFromPathsFilter>();
});

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for this demo app
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SS Tracker API v1");
    c.RoutePrefix = "swagger";
});

// Enable CORS
app.UseCors("AllowReactApp");

app.UseRouting();

//app.UseHttpsRedirection();

//app.UseAuthorization();

// Serve static files from wwwroot
app.UseStaticFiles();

// Map API controllers
app.MapControllers();

// Serve React app on /calendar path but allow static files to pass through
app.MapWhen(context => 
    context.Request.Path.StartsWithSegments("/calendar") && 
    !context.Request.Path.StartsWithSegments("/calendar/static"), 
    calendarApp =>
{
    calendarApp.Run(async context =>
    {
        var webRootPath = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        var indexPath = Path.Combine(webRootPath, "index.html");
        
        if (File.Exists(indexPath))
        {
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexPath);
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"Calendar app not found. Looking for: {indexPath}. WebRootPath: {webRootPath}");
        }
    });
});

// Serve static files for /calendar/static/* by mapping them to /static/*
app.MapWhen(context => context.Request.Path.StartsWithSegments("/calendar/static"), staticApp =>
{
    staticApp.Run(async context =>
    {
        var requestPath = context.Request.Path.Value;
        var staticPath = requestPath?.Replace("/calendar/static", "/static");
        var webRootPath = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        var filePath = Path.Combine(webRootPath, staticPath?.TrimStart('/') ?? "");
        
        if (File.Exists(filePath))
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".js" => "application/javascript",
                ".css" => "text/css",
                ".map" => "application/json",
                _ => "application/octet-stream"
            };
            
            context.Response.ContentType = contentType;
            await context.Response.SendFileAsync(filePath);
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"Static file not found: {filePath}");
        }
    });
});

// Root endpoint - show available endpoints (excluded from Swagger)
app.MapGet("/", () => Results.Json(new
{
    message = "SS Tracker API",
    version = "v1",
    endpoints = new
    {
        schedule = "/api/v1/schedule",
        availableAdvisors = "/api/v1/schedule/advisors/available",
        swagger = "/swagger/index.html",
        calendar = "/calendar",
        health = "/api/v1/health"
    }
})).ExcludeFromDescription();

app.Run();

// Document filter to clean paths in Swagger UI
public class RemoveVersionFromPathsFilter : Microsoft.OpenApi.Models.IDocumentFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiDocument swaggerDoc, Swashbuckle.AspNetCore.SwaggerGen.DocumentFilterContext context)
    {
        var pathsToUpdate = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiPathItem>();
        
        foreach (var path in swaggerDoc.Paths)
        {
            var newKey = path.Key.Replace("/api/v1", "");
            if (string.IsNullOrEmpty(newKey))
                newKey = "/";
            pathsToUpdate[newKey] = path.Value;
        }
        
        swaggerDoc.Paths.Clear();
        foreach (var path in pathsToUpdate)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}
