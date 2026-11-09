// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: ViewModel danh sách chuyên khoa (tìm kiếm, lọc, sắp xếp, phân trang) và trang chuyên khoa công khai.
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels
{
    // Một dòng chuyên khoa kèm số bác sĩ (dùng Select của LINQ để tính trên SQL Server)
    public class ChuyenKhoaItemViewModel
    {
        public int MaChuyenKhoa { get; set; }
        public string TenChuyenKhoa { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int SoPhong { get; set; }
        public bool TrangThai { get; set; }
        public int SoBacSi { get; set; }
    }

    public class ChuyenKhoaIndexViewModel
    {
        public string? TuKhoa { get; set; }
        public bool? TrangThai { get; set; }
        public string? SapXep { get; set; }
        public PaginatedList<ChuyenKhoaItemViewModel> DanhSach { get; set; } = null!;
    }
}
