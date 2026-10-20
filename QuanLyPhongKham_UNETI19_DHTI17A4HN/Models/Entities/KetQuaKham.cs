// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Entity KetQuaKham - kết quả khám của một lịch khám đã khám.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities
{
    public class KetQuaKham
    {
        [Key]
        [Display(Name = "Mã kết quả")]
        public int MaKetQua { get; set; }

        [Display(Name = "Lịch khám")]
        public int MaLichKham { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán")]
        [StringLength(1000, ErrorMessage = "Chẩn đoán tối đa 1000 ký tự")]
        [Display(Name = "Chẩn đoán")]
        public string ChanDoan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập kết luận")]
        [StringLength(1000, ErrorMessage = "Kết luận tối đa 1000 ký tự")]
        [Display(Name = "Kết luận")]
        public string KetLuan { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Đơn thuốc tối đa 2000 ký tự")]
        [Display(Name = "Đơn thuốc")]
        public string? DonThuoc { get; set; }

        [StringLength(1000, ErrorMessage = "Lời dặn tối đa 1000 ký tự")]
        [Display(Name = "Lời dặn")]
        public string? LoiDan { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public TrangThaiKetQua TrangThai { get; set; } = TrangThaiKetQua.DangCapNhat;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey(nameof(MaLichKham))]
        public LichKham? LichKham { get; set; }

        public ICollection<ChiTietDichVu> ChiTietDichVus { get; set; } = new List<ChiTietDichVu>();

        // Tổng tiền dịch vụ (không lưu vào CSDL)
        [NotMapped]
        [Display(Name = "Tổng tiền")]
        public decimal TongTien => ChiTietDichVus.Sum(c => c.SoLuong * c.DonGia);
    }
}
