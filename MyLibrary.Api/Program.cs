using Microsoft.EntityFrameworkCore;
using MyLibrary;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseCors();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/index.html");
});


app.MapControllers();

app.Run();