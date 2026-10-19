// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Entity TaiKhoan - lưu thông tin đăng nhập, vai trò và trạng thái tài khoản.
using System.ComponentModel.DataAnnotations;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities
{
    public class TaiKhoan
    {
        [Key]
        [Display(Name = "Mã tài khoản")]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập từ 4 đến 50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm, gạch dưới")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        // Lưu chuỗi mật khẩu đã được mã hóa (băm) bằng PasswordHasher, không lưu mật khẩu gốc
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Vai trò")]
        public VaiTro VaiTro { get; set; } = VaiTro.BenhNhan;

        // true: Hoạt động, false: Bị khóa (không được đăng nhập)
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation: TaiKhoan (1) - (0..1) BenhNhan, TaiKhoan (1) - (0..1) BacSi
        public BenhNhan? BenhNhan { get; set; }
        public BacSi? BacSi { get; set; }
    }
}
