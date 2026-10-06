using Asp.Versioning;
using BibliotecaApi.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc();

builder.Services.AddOpenApi();

var app = builder.Build();

// Aplica as migrations pendentes ao iniciar (cria o banco SQLite se não existir)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Tratamento global de erros: exceções não tratadas viram ProblemDetails (500)
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();             // /openapi/v1.json
app.MapScalarApiReference(); // /scalar/v1

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapControllers();

app.Run();
