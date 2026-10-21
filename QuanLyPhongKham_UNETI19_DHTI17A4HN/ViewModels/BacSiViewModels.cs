// Họ và tên: Lê Quang Trung
// Mã sinh viên: 23103100270
// Nội dung thực hiện: ViewModel danh sách bác sĩ (tìm kiếm, lọc, sắp xếp, phân trang), form bác sĩ có upload ảnh, chi tiết bác sĩ.
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels
{
    public class BacSiIndexViewModel
    {
        // Điều kiện tìm kiếm / lọc / sắp xếp (được giữ lại khi chuyển trang)
        public string? TuKhoa { get; set; }
        public int? MaChuyenKhoa { get; set; }
        public bool? TrangThai { get; set; }
        public string? SapXep { get; set; }

        public PaginatedList<BacSi> DanhSach { get; set; } = null!;
        public SelectList? ChuyenKhoas { get; set; }

        // Số lượt đã khám của từng bác sĩ trên trang hiện tại
        public Dictionary<int, int> SoLuotKham { get; set; } = new();

        public Dictionary<string, string?> ThamSo() => new()
        {
            ["tuKhoa"] = TuKhoa,
            ["maChuyenKhoa"] = MaChuyenKhoa?.ToString(),
            ["trangThai"] = TrangThai?.ToString(),
            ["sapXep"] = SapXep
        };
    }

    public class BacSiFormViewModel
    {
        public BacSi BacSi { get; set; } = new();
        public IFormFile? AnhDaiDien { get; set; }
        public SelectList? ChuyenKhoas { get; set; }
        public SelectList? TaiKhoans { get; set; }
    }

    public class BacSiChiTietViewModel
    {
        public BacSi BacSi { get; set; } = null!;
        public int SoLuotKham { get; set; }
        public int SoLichSapToi { get; set; }
        public List<BacSi> BacSiCungKhoa { get; set; } = new();
    }
}
