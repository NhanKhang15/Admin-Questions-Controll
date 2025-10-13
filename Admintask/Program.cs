using AdminPortal.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<FloriaDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("FloriaDb"))
    // .EnableSensitiveDataLogging() // mở khi cần debug
);

var app = builder.Build();

// Error pages & HSTS (prod)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Nếu có auth sau này:
// app.UseAuthentication();
app.UseAuthorization();

// Attribute-routed controllers (ví dụ QuestionSetController có [Route("question-set")])
app.MapControllers();

// Conventional route cho phần còn lại
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=ManageQuestions}/{id?}");

app.Run();
