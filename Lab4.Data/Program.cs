using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Lab4.Data;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
        // Setup EF Core based on environment variable
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("RestTestDb"));
        }
            
        builder.Services.AddScoped<StudentRepository>();
        builder.Services.AddScoped<IValidator<CreateStudentRequest>, CreateStudentRequestValidator>();
        builder.Services.AddScoped<IValidator<UpdateStudentRequest>, UpdateStudentRequestValidator>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        app.MapControllers();

        app.Run();
    }
}

