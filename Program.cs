using umps.Hubs;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
        builder =>
        {
            builder.AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetIsOriginAllowed((host) => true)
                   .AllowCredentials();
        }));

builder.Services.AddSignalR();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var server = new Server
{
    id = Guid.NewGuid().ToString(),
    name = "Default",
};

app.UseCors("CorsPolicy");

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "/static"
});

app.UseRouting();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

app.MapRazorPages();
app.MapControllers();
app.MapHub<ControlHub>("/controlHub");

app.MapGet("/config.js", async context =>
{
    await context.Response.SendFileAsync(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "config.js"));
});

app.MapGet("/umps.module.js", async context =>
{
    await context.Response.SendFileAsync(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "umps.module.js"));
});

app.Run();
