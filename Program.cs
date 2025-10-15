using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Legacy route for XtalHtml.html
app.MapGet("/XtalHtml.html", () => Results.Redirect("/"));

// Fallback for any unmatched routes - serve static files
app.MapFallback(async (HttpContext context) =>
{
    var path = context.Request.Path.Value?.TrimStart('/');
    if (string.IsNullOrEmpty(path))
    {
        return Results.Redirect("/");
    }
    
    var filePath = Path.Combine(app.Environment.WebRootPath, path);
    
    if (File.Exists(filePath))
    {
        var content = await File.ReadAllBytesAsync(filePath);
        var contentType = GetContentType(path);
        return Results.Bytes(content, contentType);
    }
    
    return Results.NotFound();
});

app.Run();

static string GetContentType(string path)
{
    var extension = Path.GetExtension(path).ToLowerInvariant();
    return extension switch
    {
        ".html" => "text/html",
        ".css" => "text/css",
        ".js" => "application/javascript",
        ".json" => "application/json",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".ico" => "image/x-icon",
        ".mp4" => "video/mp4",
        ".mp3" => "audio/mpeg",
        ".wav" => "audio/wav",
        ".ogg" => "audio/ogg",
        _ => "application/octet-stream"
    };
}
