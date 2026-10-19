// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Entity LichKham - lịch hẹn khám giữa bệnh nhân và bác sĩ.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities
{
    public class LichKham
    {
        [Key]
        [Display(Name = "Mã lịch khám")]
        public int MaLichKham { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        [Display(Name = "Bệnh nhân")]
        public int MaBenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        [Display(Name = "Bác sĩ")]
        public int MaBacSi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày khám")]
        public DateTime NgayKham { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giờ bắt đầu")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan GioBatDau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giờ kết thúc")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ kết thúc")]
        public TimeSpan GioKetThuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do khám")]
        [StringLength(500, ErrorMessage = "Lý do khám tối đa 500 ký tự")]
        [Display(Name = "Lý do khám")]
        public string LyDoKham { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public TrangThaiLichKham TrangThai { get; set; } = TrangThaiLichKham.ChoXacNhan;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(MaBenhNhan))]
        public BenhNhan? BenhNhan { get; set; }

        [ForeignKey(nameof(MaBacSi))]
        public BacSi? BacSi { get; set; }

        // LichKham (1) - (0..1) KetQuaKham
        public KetQuaKham? KetQuaKham { get; set; }
    }
}
