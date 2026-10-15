// Họ và tên: Lê Quang Trung
// Mã sinh viên: 23103100270
// Nội dung thực hiện: Entity BacSi - thông tin bác sĩ, thuộc một chuyên khoa, có thể gắn với một tài khoản đăng nhập.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities
{
    public class BacSi
    {
        [Key]
        [Display(Name = "Mã bác sĩ")]
        public int MaBacSi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chuyên khoa")]
        [Display(Name = "Chuyên khoa")]
        public int MaChuyenKhoa { get; set; }

        // Tài khoản đăng nhập của bác sĩ (không bắt buộc)
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

        [Required(ErrorMessage = "Vui lòng nhập chuyên môn")]
        [StringLength(200, ErrorMessage = "Chuyên môn tối đa 200 ký tự")]
        [Display(Name = "Chuyên môn")]
        public string ChuyenMon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Học vị")]
        public HocVi HocVi { get; set; } = HocVi.BacSi;

        // true: Đang hoạt động (được nhận lịch), false: Tạm ngừng
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Đường dẫn ảnh đại diện (upload)
        [StringLength(255)]
        [Display(Name = "Ảnh đại diện")]
        public string? HinhAnh { get; set; }

        // Navigation properties
        [ForeignKey(nameof(MaChuyenKhoa))]
        public ChuyenKhoa? ChuyenKhoa { get; set; }

        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan? TaiKhoan { get; set; }

        public ICollection<LichKham> LichKhams { get; set; } = new List<LichKham>();
    }
}
