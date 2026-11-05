using LearningCrud.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("LearningCrud")
    ?? throw new InvalidOperationException(
        "The ConnectionStrings:LearningCrud setting is required.");

builder.Services.AddDbContext<LearningDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LearningItems}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }
