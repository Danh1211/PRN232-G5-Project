var builder = WebApplication.CreateBuilder(args);

// =========================
// ADD SERVICES
// =========================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =========================
// MIDDLEWARE
// =========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ❗ không cần auth nếu chưa dùng
app.UseAuthorization();

// =========================
// ROUTING
// =========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Feedback}/{action=Index}/{id?}");

app.Run();