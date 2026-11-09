// Họ và tên: Đỗ Huy Tùng
// Mã sinh viên: 23103100296
// Nội dung thực hiện: Kiểm tra luồng chuyển trạng thái lịch khám và kiểm tra lại trùng lịch khi xác nhận
// (phần tiếp theo của lớp LichKhamService - partial class).
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Services
{
    public partial class LichKhamService
    {
        // ================== Module 4: luồng trạng thái ==================

        /// <summary>
        /// Chờ xác nhận -> Đã xác nhận | Đã hủy;  Đã xác nhận -> Đã khám.
        /// Không cho chuyển trực tiếp Chờ xác nhận -> Đã khám, không thay đổi lịch đã khám/đã hủy.
        /// </summary>
        public static bool ChoPhepChuyen(TrangThaiLichKham hienTai, TrangThaiLichKham moi)
        {
            return (hienTai, moi) switch
            {
                (TrangThaiLichKham.ChoXacNhan, TrangThaiLichKham.DaXacNhan) => true,
                (TrangThaiLichKham.ChoXacNhan, TrangThaiLichKham.DaHuy) => true,
                (TrangThaiLichKham.DaXacNhan, TrangThaiLichKham.DaKham) => true,
                _ => false
            };
        }

        /// <summary>
        /// Khi xác nhận: kiểm tra lại bác sĩ còn hoạt động và không trùng với lịch ĐÃ XÁC NHẬN/ĐÃ KHÁM khác.
        /// </summary>
        public async Task<string?> KiemTraKhiXacNhanAsync(LichKham lich)
        {
            var bacSi = await _context.BacSis.FindAsync(lich.MaBacSi);
            if (bacSi == null || !bacSi.TrangThai)
                return "Bác sĩ hiện không hoạt động, không thể xác nhận lịch.";

            bool trungBacSi = await _context.LichKhams.AnyAsync(l =>
                l.MaLichKham != lich.MaLichKham
                && l.MaBacSi == lich.MaBacSi
                && l.NgayKham == lich.NgayKham
                && (l.TrangThai == TrangThaiLichKham.DaXacNhan || l.TrangThai == TrangThaiLichKham.DaKham)
                && l.GioBatDau < lich.GioKetThuc && lich.GioBatDau < l.GioKetThuc);
            if (trungBacSi)
                return "Bác sĩ đã có lịch được xác nhận trùng khung giờ này.";

            bool trungBenhNhan = await _context.LichKhams.AnyAsync(l =>
                l.MaLichKham != lich.MaLichKham
                && l.MaBenhNhan == lich.MaBenhNhan
                && l.NgayKham == lich.NgayKham
                && (l.TrangThai == TrangThaiLichKham.DaXacNhan || l.TrangThai == TrangThaiLichKham.DaKham)
                && l.GioBatDau < lich.GioKetThuc && lich.GioBatDau < l.GioKetThuc);
            if (trungBenhNhan)
                return "Bệnh nhân đã có lịch được xác nhận trùng khung giờ này.";

            return null;
        }
    }
}
