// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Mã hóa (băm) và kiểm tra mật khẩu bằng PasswordHasher có sẵn của ASP.NET Core.
using Microsoft.AspNetCore.Identity;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers
{
    public static class MatKhauHelper
    {
        private static readonly PasswordHasher<TaiKhoan> _hasher = new();

        // Trả về chuỗi băm (PBKDF2 + salt ngẫu nhiên) để lưu vào cột MatKhau
        public static string MaHoa(string matKhau) => _hasher.HashPassword(null!, matKhau);

        // So sánh mật khẩu người dùng nhập với chuỗi băm trong CSDL
        public static bool KiemTra(string matKhauDaMaHoa, string matKhauNhap)
        {
            try
            {
                var ketQua = _hasher.VerifyHashedPassword(null!, matKhauDaMaHoa, matKhauNhap);
                return ketQua != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                // Chuỗi trong CSDL không phải dạng băm hợp lệ
                return false;
            }
        }
    }
}
