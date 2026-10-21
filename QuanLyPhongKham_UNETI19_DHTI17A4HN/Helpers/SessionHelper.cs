// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Khai báo khóa Session và các hàm tiện ích đọc thông tin người dùng đang đăng nhập.
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers
{
    public static class SessionKeys
    {
        public const string MaTaiKhoan = "MaTaiKhoan";
        public const string HoTen = "HoTen";
        public const string VaiTro = "VaiTro";
    }

    public static class SessionHelper
    {
        public static int? LayMaTaiKhoan(this ISession session) => session.GetInt32(SessionKeys.MaTaiKhoan);

        public static string LayHoTen(this ISession session) => session.GetString(SessionKeys.HoTen) ?? string.Empty;

        public static VaiTro? LayVaiTro(this ISession session)
        {
            var giaTri = session.GetInt32(SessionKeys.VaiTro);
            return giaTri.HasValue ? (VaiTro)giaTri.Value : null;
        }

        public static bool DaDangNhap(this ISession session) => session.LayMaTaiKhoan().HasValue;

        public static bool LaQuanTri(this ISession session)
        {
            var vaiTro = session.LayVaiTro();
            return vaiTro == VaiTro.Admin || vaiTro == VaiTro.NhanVien;
        }

        // Admin, Nhân viên, Bác sĩ dùng giao diện quản trị; Bệnh nhân / khách dùng giao diện công khai
        public static string LayoutTheoVaiTro(this ISession session) =>
            session.CoVaiTro(VaiTro.Admin, VaiTro.NhanVien, VaiTro.BacSi) ? "_AdminLayout" : "_Layout";

        public static bool CoVaiTro(this ISession session, params VaiTro[] vaiTros)
        {
            var vaiTro = session.LayVaiTro();
            return vaiTro.HasValue && vaiTros.Contains(vaiTro.Value);
        }
    }
}
