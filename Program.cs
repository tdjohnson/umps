using umps.Hubs;

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


app.Run();
