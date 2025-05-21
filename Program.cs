using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using CursWork.Services;
using CursWork.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ���������
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// ���������� ��������
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = null; // ��������� ����������� ������
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment(); // ������� � ������ ����������
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // ��������� PascalCase
    });

// ������������ DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<CursWorkContext>(options =>
    options.UseSqlServer(connectionString));

// ������������ �������������� �� �����������
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login.html"; // ��������������� �� ������� �����
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // ��� 䳿 cookie
        options.SlidingExpiration = true; // ��������� ���� 䳿 ��� ���������
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ��������� ������
builder.Services.AddScoped<TransactionService>();

// ������������ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://localhost:7211", "http://localhost:5500") // �������� ���� ���������
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // ���������� credentials ��� ��������������
    });
});

var app = builder.Build();

// ������������ middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ������ middleware ��� �������������� �� �����������
app.UseAuthentication();
app.UseAuthorization();

// ������������� CORS
app.UseCors("AllowSpecificOrigins");
app.Logger.LogInformation("CORS middleware applied with policy 'AllowSpecificOrigins'.");

app.MapControllers();

// ��������������� ��������� ����� �� login.html
app.MapGet("/", () => Results.Redirect("~/login.html"));

// �������� ����������� � ���� ������
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CursWorkContext>();
        context.Database.EnsureCreated(); // ������� ����, ���� ��� �� ����������
        app.Logger.LogInformation("Database connection established successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while connecting to the database.");
    }
}

// ����������� ��������� ������
app.Logger.LogInformation("Application started successfully. Listening on {Url}", app.Urls);

app.Run();