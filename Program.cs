var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Add services for controllers
builder.Services.AddEndpointsApiExplorer(); // Add services for API explorer
builder.Services.AddSwaggerGen(); // Add services for Swagger

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSwagger(); // Enable middleware to serve generated Swagger as a JSON endpoint
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

app.MapRazorPages();
app.MapControllers();

app.Run();
