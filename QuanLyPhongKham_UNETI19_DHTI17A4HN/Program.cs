// Họ và tên: Trần Phương Thảo (Nhóm trưởng)
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Khởi tạo project ASP.NET Core 10 MVC, đăng ký DbContext (SQL Server).
// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Cấu hình Session phục vụ đăng nhập, phân quyền.
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Entity Framework Core 10 + SQL Server
builder.Services.AddDbContext<PhongKhamDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PhongKhamConnection")));

// Session: lưu Mã tài khoản, Họ tên, Vai trò sau khi đăng nhập
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".PhongKhamCare.Session";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
