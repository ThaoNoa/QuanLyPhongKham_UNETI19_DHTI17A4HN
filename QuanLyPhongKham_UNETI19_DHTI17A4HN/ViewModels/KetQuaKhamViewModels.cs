// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: ViewModel kết quả khám (kèm danh sách dịch vụ sử dụng), danh sách kết quả, danh sách dịch vụ.
using System.ComponentModel.DataAnnotations;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels
{
    public class DichVuChonViewModel
    {
        public int MaDichVu { get; set; }
        public string TenDichVu { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public bool DaChon { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng từ 1 đến 100")]
        public int SoLuong { get; set; } = 1;
    }

    public class KetQuaKhamFormViewModel
    {
        public int MaKetQua { get; set; }
        public int MaLichKham { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán")]
        [StringLength(1000)]
        [Display(Name = "Chẩn đoán")]
        public string ChanDoan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập kết luận")]
        [StringLength(1000)]
        [Display(Name = "Kết luận")]
        public string KetLuan { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Đơn thuốc")]
        public string? DonThuoc { get; set; }

        [StringLength(1000)]
        [Display(Name = "Lời dặn")]
        public string? LoiDan { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiKetQua TrangThai { get; set; } = TrangThaiKetQua.HoanThanh;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public List<DichVuChonViewModel> DichVus { get; set; } = new();

        // Thông tin lịch khám để hiển thị
        public LichKham? LichKham { get; set; }
    }

    public class KetQuaKhamIndexViewModel
    {
        public string? TuKhoa { get; set; }
        public TrangThaiKetQua? TrangThai { get; set; }
        public string? SapXep { get; set; }
        public PaginatedList<KetQuaKham> DanhSach { get; set; } = null!;
        public List<LichKham> LichChoNhapKetQua { get; set; } = new();

        public Dictionary<string, string?> ThamSo() => new()
        {
            ["tuKhoa"] = TuKhoa,
            ["trangThai"] = TrangThai?.ToString(),
            ["sapXep"] = SapXep
        };
    }

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
