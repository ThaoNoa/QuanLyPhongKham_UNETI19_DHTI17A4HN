// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Entity BenhNhan - hồ sơ bệnh nhân, liên kết 0..1 với tài khoản đăng nhập.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities
{
    public class BenhNhan
    {
        [Key]
        [Display(Name = "Mã bệnh nhân")]
        public int MaBenhNhan { get; set; }

        // Bệnh nhân do nhân viên tạo tại quầy có thể chưa có tài khoản
        [Display(Name = "Tài khoản")]
        public int? MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } = "Nam";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(15, ErrorMessage = "Số BHYT tối đa 15 ký tự")]
        [RegularExpression(@"^[A-Za-z]{2}\d{13}$", ErrorMessage = "Số BHYT gồm 2 chữ in hoa và 13 chữ số (VD: DN4010123456789)")]
        [Display(Name = "Số BHYT")]
        public string? SoBHYT { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày đăng ký")]
        public DateTime NgayDangKy { get; set; } = DateTime.Today;

        // true: Hoạt động, false: Ngừng theo dõi
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan? TaiKhoan { get; set; }

        public ICollection<LichKham> LichKhams { get; set; } = new List<LichKham>();
    }
}
