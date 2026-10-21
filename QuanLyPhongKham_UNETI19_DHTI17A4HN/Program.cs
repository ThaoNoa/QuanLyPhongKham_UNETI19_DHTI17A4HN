// Họ và tên: Trần Phương Thảo (Nhóm trưởng)
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Cấu hình ứng dụng - đăng ký DbContext (SQL Server), route, thông báo lỗi tiếng Việt,
// định dạng vi-VN, tự động áp dụng Migration và tạo dữ liệu mẫu khi chạy lần đầu.
// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Cấu hình Session phục vụ đăng nhập, phân quyền.
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Thông báo lỗi Model Binding bằng tiếng Việt
    var m = options.ModelBindingMessageProvider;
    m.SetValueMustNotBeNullAccessor(_ => "Vui lòng nhập giá trị.");
    m.SetAttemptedValueIsInvalidAccessor((giaTri, _) => $"Giá trị '{giaTri}' không hợp lệ.");
    m.SetValueIsInvalidAccessor(giaTri => $"Giá trị '{giaTri}' không hợp lệ.");
    m.SetValueMustBeANumberAccessor(truong => $"{truong} phải là số.");
    m.SetMissingKeyOrValueAccessor(() => "Vui lòng nhập giá trị.");
    m.SetMissingBindRequiredValueAccessor(truong => $"Vui lòng nhập {truong}.");
    m.SetUnknownValueIsInvalidAccessor(truong => $"Giá trị của {truong} không hợp lệ.");
    m.SetNonPropertyAttemptedValueIsInvalidAccessor(giaTri => $"Giá trị '{giaTri}' không hợp lệ.");
    m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Giá trị không hợp lệ.");
    m.SetNonPropertyValueMustBeANumberAccessor(() => "Giá trị phải là số.");
});

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

// Tự động tạo/cập nhật CSDL theo Migration và thêm dữ liệu mẫu
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhongKhamDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Định dạng ngày, số theo tiếng Việt (dd/MM/yyyy, 1.000.000)
var vi = new CultureInfo("vi-VN");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(vi),
    SupportedCultures = new[] { vi },
    SupportedUICultures = new[] { vi }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
