// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Lớp xử lý nghiệp vụ đặt lịch - kiểm tra thời gian hợp lệ, bác sĩ hoạt động,
// trùng lịch bác sĩ và trùng lịch bệnh nhân bằng EF Core/LINQ.
// Phần luồng trạng thái (Module 4) nằm ở file LichKhamService.TrangThai.cs.
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Services
{
    public partial class LichKhamService
    {
        // Giờ làm việc của phòng khám
        public static readonly TimeSpan GioMoCua = new(7, 0, 0);
        public static readonly TimeSpan GioDongCua = new(17, 30, 0);

        private readonly PhongKhamDbContext _context;

        public LichKhamService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ================== Module 3: kiểm tra khi đặt lịch ==================

        /// <summary>
        /// Kiểm tra toàn bộ điều kiện của một lịch khám. Trả về danh sách lỗi (key = tên thuộc tính).
        /// </summary>
        public async Task<List<KeyValuePair<string, string>>> KiemTraLichAsync(LichKham lich, int? boQuaMaLich = null)
        {
            var loi = new List<KeyValuePair<string, string>>();

            // 1. Thời gian kết thúc phải sau thời gian bắt đầu
            if (lich.GioKetThuc <= lich.GioBatDau)
                loi.Add(new(nameof(LichKham.GioKetThuc), "Giờ kết thúc phải sau giờ bắt đầu."));

            // 2. Trong giờ làm việc
            if (lich.GioBatDau < GioMoCua || lich.GioKetThuc > GioDongCua)
                loi.Add(new(nameof(LichKham.GioBatDau), $"Phòng khám làm việc từ {GioMoCua:hh\\:mm} đến {GioDongCua:hh\\:mm}."));

            // 3. Không đặt lịch vào thời điểm đã qua
            if (lich.NgayKham.Date.Add(lich.GioBatDau) <= DateTime.Now)
                loi.Add(new(nameof(LichKham.NgayKham), "Không thể đặt lịch vào thời điểm đã qua."));

            // 4. Phòng khám nghỉ Chủ nhật, chỉ nhận đặt trước tối đa 60 ngày
            if (lich.NgayKham.DayOfWeek == DayOfWeek.Sunday)
                loi.Add(new(nameof(LichKham.NgayKham), "Phòng khám nghỉ Chủ nhật, vui lòng chọn ngày khác."));
            if (lich.NgayKham.Date > DateTime.Today.AddDays(60))
                loi.Add(new(nameof(LichKham.NgayKham), "Chỉ nhận đặt lịch trước tối đa 60 ngày."));

            // 5. Bác sĩ phải tồn tại và đang hoạt động
            var bacSi = await _context.BacSis.Include(b => b.ChuyenKhoa)
                                             .FirstOrDefaultAsync(b => b.MaBacSi == lich.MaBacSi);
            if (bacSi == null)
                loi.Add(new(nameof(LichKham.MaBacSi), "Bác sĩ không tồn tại."));
            else if (!bacSi.TrangThai || bacSi.ChuyenKhoa == null || !bacSi.ChuyenKhoa.TrangThai)
                loi.Add(new(nameof(LichKham.MaBacSi), "Bác sĩ này hiện không nhận lịch khám."));

            // Chỉ kiểm tra trùng khi thời gian hợp lệ
            if (lich.GioKetThuc > lich.GioBatDau)
            {
                if (await TrungLichBacSiAsync(lich, boQuaMaLich))
                    loi.Add(new(nameof(LichKham.GioBatDau), "Bác sĩ đã có lịch khám trùng khung giờ này. Vui lòng chọn giờ khác."));

                if (await TrungLichBenhNhanAsync(lich, boQuaMaLich))
                    loi.Add(new(nameof(LichKham.GioBatDau), "Bệnh nhân đã có lịch khám khác trùng khung giờ này."));
            }

            return loi;
        }

        /// <summary>
        /// Hai khoảng thời gian [A1, A2) và [B1, B2) giao nhau khi A1 &lt; B2 và B1 &lt; A2.
        /// Chỉ xét các lịch chưa bị hủy, cùng ngày, cùng bác sĩ.
        /// </summary>
        public Task<bool> TrungLichBacSiAsync(LichKham lich, int? boQuaMaLich = null)
        {
            return _context.LichKhams.AnyAsync(l =>
                l.MaBacSi == lich.MaBacSi
                && l.NgayKham == lich.NgayKham.Date
                && l.TrangThai != TrangThaiLichKham.DaHuy
                && (boQuaMaLich == null || l.MaLichKham != boQuaMaLich)
                && l.GioBatDau < lich.GioKetThuc
                && lich.GioBatDau < l.GioKetThuc);
        }

        public Task<bool> TrungLichBenhNhanAsync(LichKham lich, int? boQuaMaLich = null)
        {
            return _context.LichKhams.AnyAsync(l =>
                l.MaBenhNhan == lich.MaBenhNhan
                && l.NgayKham == lich.NgayKham.Date
                && l.TrangThai != TrangThaiLichKham.DaHuy
                && (boQuaMaLich == null || l.MaLichKham != boQuaMaLich)
                && l.GioBatDau < lich.GioKetThuc
                && lich.GioBatDau < l.GioKetThuc);
        }

        /// <summary>
        /// Lấy các khung giờ đã có người đặt của bác sĩ trong ngày (để giao diện làm mờ khung giờ đó).
        /// </summary>
        public async Task<List<(TimeSpan BatDau, TimeSpan KetThuc)>> LayKhungGioDaDatAsync(int maBacSi, DateTime ngay)
        {
            var ds = await _context.LichKhams
                .Where(l => l.MaBacSi == maBacSi && l.NgayKham == ngay.Date && l.TrangThai != TrangThaiLichKham.DaHuy)
                .OrderBy(l => l.GioBatDau)
                .Select(l => new { l.GioBatDau, l.GioKetThuc })
                .ToListAsync();
            return ds.Select(x => (x.GioBatDau, x.GioKetThuc)).ToList();
        }
    }
}
