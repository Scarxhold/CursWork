using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using CursWork.Services;
using CursWork.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Логування
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Добавление сервисов
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = null; // Отключаем циклические ссылки
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment(); // Отступы в режиме разработки
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Сохраняем PascalCase
    });

// Налаштування DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<CursWorkContext>(options =>
    options.UseSqlServer(connectionString));

// Налаштування автентифікації та авторизації
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login.html"; // Перенаправлення на сторінку логіну
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Час дії cookie
        options.SlidingExpiration = true; // Оновлення часу дії при активності
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Реєстрація сервісів
builder.Services.AddScoped<TransactionService>();

// Налаштування CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://localhost:7211", "http://localhost:5500") // Добавлен порт фронтенда
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Дозволяємо credentials для автентифікації
    });
});

var app = builder.Build();

// Налаштування middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Додаємо middleware для автентифікації та авторизації
app.UseAuthentication();
app.UseAuthorization();

// Використовуємо CORS
app.UseCors("AllowSpecificOrigins");
app.Logger.LogInformation("CORS middleware applied with policy 'AllowSpecificOrigins'.");

app.MapControllers();

// Перенаправлення корневого шляху на login.html
app.MapGet("/", () => Results.Redirect("~/login.html"));

// Проверка подключения к базе данных
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CursWorkContext>();
        context.Database.EnsureCreated(); // Создает базу, если она не существует
        app.Logger.LogInformation("Database connection established successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while connecting to the database.");
    }
}

// Логирование успешного старта
app.Logger.LogInformation("Application started successfully. Listening on {Url}", app.Urls);

app.Run();