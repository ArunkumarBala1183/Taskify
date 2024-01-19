//Project Title         : Taskify
//Author                : Arunkumar B
//Created At            : 15/02/2023
//Last Modified date    : 06/03/2023
//Reviewed by           : Anitha Manogoran
//Reviewed Date         : 22/02/2023

using Microsoft.EntityFrameworkCore;
using Task_Management.Models.DatabaseConnection;
using Task_Management.Models.DatabaseOperations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Task_Management.Models.JWT;
using Microsoft.Extensions.Logging;
using Serilog;
using Task_Management.Models.Notification;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("connection1")));
builder.Services.AddDbContext<ProjectOpertions>();
builder.Services.AddDbContext<EmployeeOperations>();
builder.Services.AddDbContext<TaskOperations>();
builder.Services.AddDbContext<ChatOperations>();

builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IDatabaseOperations, ProjectOpertions>());
builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IDatabaseOperations, EmployeeOperations>());
builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IDatabaseOperations, TaskOperations>());
builder.Services.AddSingleton<JWTOperations>();
builder.Services.AddTransient<ManagerOperations>();
builder.Services.AddSingleton<EmailService>();

builder.Services.AddSession();
builder.Services.AddMvc();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.UseAuthentication();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
