// Họ và tên: Đỗ Huy Tùng
// Mã sinh viên: 23103100296
// Nội dung thực hiện: ViewModel danh sách lịch cần xử lý (tìm kiếm, lọc chuyên khoa/trạng thái/ngày, sắp xếp, phân trang)
// và lịch làm việc của bác sĩ.
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels
{
    public class TiepNhanIndexViewModel
    {
        public string? TuKhoa { get; set; }
        public int? MaChuyenKhoa { get; set; }
        public TrangThaiLichKham? TrangThai { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string? SapXep { get; set; }

        public PaginatedList<LichKham> DanhSach { get; set; } = null!;
        public SelectList? ChuyenKhoas { get; set; }
        public Dictionary<TrangThaiLichKham, int> SoLuongTheoTrangThai { get; set; } = new();

        public Dictionary<string, string?> ThamSo(bool boTrangThai = false) => new()
        {
            ["tuKhoa"] = TuKhoa,
            ["maChuyenKhoa"] = MaChuyenKhoa?.ToString(),
            ["trangThai"] = boTrangThai ? null : TrangThai?.ToString(),
            ["tuNgay"] = TuNgay?.ToString("yyyy-MM-dd"),
            ["denNgay"] = DenNgay?.ToString("yyyy-MM-dd"),
            ["sapXep"] = SapXep
        };
    }

    public class LichBacSiViewModel
    {
        public BacSi BacSi { get; set; } = null!;
        public DateTime Ngay { get; set; }
        public List<LichKham> LichTrongNgay { get; set; } = new();
        public List<LichKham> LichSapToi { get; set; } = new();
        public int SoChuaNhapKetQua { get; set; }
    }
}
