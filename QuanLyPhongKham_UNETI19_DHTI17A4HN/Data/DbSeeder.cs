// File dùng chung của nhóm - mỗi thành viên chuẩn bị dữ liệu mẫu cho Module của mình:
// Họ và tên: Trần Phương Thảo (Nhóm trưởng) - Mã sinh viên: 23103100226
// Nội dung thực hiện: Khung tạo dữ liệu mẫu (chạy khi CSDL còn trống), tạo sẵn tài khoản quản trị để đăng nhập.
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data
{
    /// <summary>
    /// Tạo dữ liệu mẫu khi CSDL còn trống (chạy 1 lần lúc khởi động ứng dụng).
    /// Dữ liệu mẫu chi tiết của từng Module được các thành viên bổ sung ở cuối dự án.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(PhongKhamDbContext db)
        {
            if (await db.TaiKhoans.AnyAsync()) return; // Đã có dữ liệu

            // Tài khoản quản trị mặc định: admin / Admin@123
            db.TaiKhoans.Add(new TaiKhoan
            {
                TenDangNhap = "admin",
                MatKhau = MatKhauHelper.MaHoa("Admin@123"),
                HoTen = "Quản trị hệ thống",
                Email = "admin@phongkhamcare.vn",
                VaiTro = VaiTro.Admin
            });
            await db.SaveChangesAsync();
        }
    }
}
