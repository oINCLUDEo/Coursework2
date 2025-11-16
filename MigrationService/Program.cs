using Microsoft.EntityFrameworkCore;
using MigrationService.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".FlightSchool.Session";
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
});

// Configure DB context
builder.Services.AddDbContext<FlightSchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FlightSchoolConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Old route removed
// Initialize FlightSchool database and seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var db = services.GetRequiredService<FlightSchoolDbContext>();
    db.Database.EnsureCreated();

    try
    {
        var sqlRoot = Path.Combine(env.ContentRootPath, "Data");
        var udfSpPath = Path.Combine(sqlRoot, "FlightSchool.sql");
        if (File.Exists(udfSpPath))
        {
            var sql = File.ReadAllText(udfSpPath);
            db.Database.ExecuteSqlRaw(sql);
        }

        if (!db.Students.Any())
        {
            // Используем новый полный файл заполнения
            var seedPath = Path.Combine(sqlRoot, "FlightSchoolSeed_Complete.sql");
            if (File.Exists(seedPath))
            {
                var seed = File.ReadAllText(seedPath);
                db.Database.ExecuteSqlRaw(seed);
            }
            else
            {
                // Fallback на старый файл, если новый не найден
                var oldSeedPath = Path.Combine(sqlRoot, "FlightSchoolSeed.sql");
                if (File.Exists(oldSeedPath))
                {
                    var seed = File.ReadAllText(oldSeedPath);
                    db.Database.ExecuteSqlRaw(seed);
                }
            }
        }
    }
    catch (Exception)
    {
        // Swallow seeding errors in dev; for production, log properly
    }
}

app.Run();