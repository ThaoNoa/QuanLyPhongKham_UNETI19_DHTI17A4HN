// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: ViewModel danh sách dịch vụ y tế.
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels
{
    public class DichVuYTeIndexViewModel
    {
        public string? TuKhoa { get; set; }
        public bool? TrangThai { get; set; }
        public string? SapXep { get; set; }
        public PaginatedList<DichVuYTe> DanhSach { get; set; } = null!;
        public Dictionary<int, int> SoLanSuDung { get; set; } = new();

        public Dictionary<string, string?> ThamSo() => new()
        {
            ["tuKhoa"] = TuKhoa,
            ["trangThai"] = TrangThai?.ToString(),
            ["sapXep"] = SapXep
        };
    }
}
