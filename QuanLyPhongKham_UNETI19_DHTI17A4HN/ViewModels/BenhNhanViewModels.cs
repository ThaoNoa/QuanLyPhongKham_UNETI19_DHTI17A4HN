// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: ViewModel danh sách bệnh nhân, chi tiết bệnh nhân, đặt lịch khám, lịch khám của tôi.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels
{
    public class BenhNhanIndexViewModel
    {
        public string? TuKhoa { get; set; }
        public string? GioiTinh { get; set; }
        public bool? TrangThai { get; set; }
        public string? SapXep { get; set; }
        public PaginatedList<BenhNhan> DanhSach { get; set; } = null!;

        public Dictionary<string, string?> ThamSo() => new()
        {
            ["tuKhoa"] = TuKhoa,
            ["gioiTinh"] = GioiTinh,
            ["trangThai"] = TrangThai?.ToString(),
            ["sapXep"] = SapXep
        };
    }

    public class BenhNhanChiTietViewModel
    {
        public BenhNhan BenhNhan { get; set; } = null!;
        public List<LichKham> LichKhams { get; set; } = new();
    }

    public class DatLichViewModel
    {
        // Nhân viên đặt lịch hộ thì chọn bệnh nhân; bệnh nhân tự đặt thì lấy từ Session
        [Display(Name = "Bệnh nhân")]
        public int? MaBenhNhan { get; set; }

        [Display(Name = "Chuyên khoa")]
        public int? MaChuyenKhoa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        [Display(Name = "Bác sĩ")]
        public int? MaBacSi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày khám")]
        public DateTime? NgayKham { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan? GioBatDau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ kết thúc")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ kết thúc")]
        public TimeSpan? GioKetThuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do khám / triệu chứng")]
        [StringLength(500, ErrorMessage = "Tối đa 500 ký tự")]
        [Display(Name = "Lý do khám / triệu chứng")]
        public string LyDoKham { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Dữ liệu cho các ô select
        public SelectList? ChuyenKhoas { get; set; }
        public SelectList? BacSis { get; set; }
        public SelectList? BenhNhans { get; set; }
        public BacSi? BacSiDaChon { get; set; }
        public bool LaNhanVien { get; set; }
    }

    public class LichCuaToiViewModel
    {
        public TrangThaiLichKham? TrangThai { get; set; }
        public PaginatedList<LichKham> DanhSach { get; set; } = null!;
        public Dictionary<TrangThaiLichKham, int> ThongKe { get; set; } = new();
    }
}
