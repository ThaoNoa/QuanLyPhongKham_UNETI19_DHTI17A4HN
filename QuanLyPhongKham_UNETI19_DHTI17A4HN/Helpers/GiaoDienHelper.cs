// Họ và tên: Trần Phương Thảo (Nhóm trưởng)
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Hàm tiện ích giao diện dùng chung - màu trạng thái, biểu tượng chuyên khoa, chữ cái đại diện.
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers
{
    public static class GiaoDienHelper
    {
        // Lớp CSS hiển thị trạng thái lịch khám
        public static string CssTrangThai(this TrangThaiLichKham trangThai) => trangThai switch
        {
            TrangThaiLichKham.ChoXacNhan => "status status-pending",
            TrangThaiLichKham.DaXacNhan => "status status-confirmed",
            TrangThaiLichKham.DaKham => "status status-done",
            TrangThaiLichKham.DaHuy => "status status-cancel",
            _ => "status"
        };

        public static string CssTrangThai(this TrangThaiKetQua trangThai) =>
            trangThai == TrangThaiKetQua.HoanThanh ? "status status-done" : "status status-pending";

        // Biểu tượng + tông màu cho từng chuyên khoa (dựa theo tên)
        public static (string Icon, string Tone) GiaoDienChuyenKhoa(string? tenChuyenKhoa)
        {
            var ten = (tenChuyenKhoa ?? string.Empty).ToLower();
            if (ten.Contains("răng")) return ("tooth", "tone-cyan");
            if (ten.Contains("mắt")) return ("eye", "tone-indigo");
            if (ten.Contains("tai") || ten.Contains("họng")) return ("ear", "tone-purple");
            if (ten.Contains("nhi")) return ("baby", "tone-orange");
            if (ten.Contains("sản") || ten.Contains("phụ")) return ("mother", "tone-pink");
            if (ten.Contains("ngoại")) return ("syringe", "tone-teal");
            if (ten.Contains("nội")) return ("stethoscope", "tone-blue");
            if (ten.Contains("da")) return ("skin", "tone-red");
            return ("heart", "tone-blue");
        }

        // Lấy chữ cái đầu của tên (từ cuối) để làm ảnh đại diện mặc định
        public static string ChuCaiDau(string? hoTen)
        {
            if (string.IsNullOrWhiteSpace(hoTen)) return "?";
            var tu = hoTen.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return tu.Length == 1
                ? tu[0][..1].ToUpper()
                : (tu[^2][..1] + tu[^1][..1]).ToUpper();
        }

        public static string TenThu(DateTime ngay) => ngay.DayOfWeek switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            _ => "Chủ nhật"
        };
    }
}
