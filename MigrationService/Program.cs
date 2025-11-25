using Microsoft.EntityFrameworkCore;
using MigrationService.Binders;
using MigrationService.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});
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

// Initialize FlightSchool database and seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<FlightSchoolDbContext>();
    db.Database.EnsureCreated();

    try
    {   
        var sqlRoot = Path.Combine(env.ContentRootPath, "Data");
        var udfSpPath = Path.Combine(sqlRoot, "FlightSchool.sql");
        if (File.Exists(udfSpPath))
        {
            
            logger.LogInformation("Выполнение скрипта создания функций и хранимых процедур...");
            var sql = File.ReadAllText(udfSpPath);
            ExecuteSqlScript(db, sql, logger, skipCleanup: false);
            logger.LogInformation("Функции и хранимые процедуры созданы.");
        }

        if (!db.Students.Any())
        {
            var seedPath = Path.Combine(sqlRoot, "FlightSchoolSeed_Complete.sql");
            if (File.Exists(seedPath))
            {
                var seed = File.ReadAllText(seedPath);
                ExecuteSqlScript(db, seed, logger, skipCleanup: true);
                logger.LogInformation("База данных успешно заполнена тестовыми данными.");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при инициализации базы данных: {Message}", ex.Message);
    }
    
}

// Вспомогательный метод для выполнения SQL-скриптов с командами GO
static void ExecuteSqlScript(FlightSchoolDbContext db, string sqlScript, ILogger logger, bool skipCleanup = false)
{
    // Удаляем комментарии и разбиваем скрипт на команды
    var lines = sqlScript.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    var commands = new List<string>();
    var currentCommand = new System.Text.StringBuilder();
    
    foreach (var line in lines)
    {
        var trimmedLine = line.Trim();
        
        // Пропускаем пустые строки и комментарии
        if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("--"))
        {
            continue;
        }
        
        // Если встретили GO, сохраняем текущую команду и начинаем новую
        if (trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase))
        {
            if (currentCommand.Length > 0)
            {
                commands.Add(currentCommand.ToString().Trim());
                currentCommand.Clear();
            }
            continue;
        }
        
        // Пропускаем команды, которые не нужны через EF Core или при первом запуске
        if (trimmedLine.StartsWith("USE ", StringComparison.OrdinalIgnoreCase) ||
            trimmedLine.StartsWith("SET NOCOUNT", StringComparison.OrdinalIgnoreCase) ||
            trimmedLine.StartsWith("PRINT ", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }
        
        // Пропускаем команды очистки и сброса счетчиков при первом запуске
        if (skipCleanup)
        {
            if (trimmedLine.StartsWith("DELETE FROM", StringComparison.OrdinalIgnoreCase) ||
                trimmedLine.StartsWith("DBCC CHECKIDENT", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
        }
        
        // Добавляем строку к текущей команде
        if (currentCommand.Length > 0)
        {
            currentCommand.AppendLine();
        }
        currentCommand.Append(trimmedLine);
    }
    
    // Добавляем последнюю команду, если она есть
    if (currentCommand.Length > 0)
    {
        commands.Add(currentCommand.ToString().Trim());
    }

    // Выполняем команды
    foreach (var command in commands)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            continue;
        }
        
        try
        {
            db.Database.ExecuteSqlRaw(command);
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но продолжаем выполнение остальных команд
            var commandPreview = command.Length > 100 ? command.Substring(0, 100) + "..." : command;
            logger.LogWarning(ex, "Ошибка при выполнении SQL-команды: {Command}", commandPreview);
        }
    }
}

app.Run();