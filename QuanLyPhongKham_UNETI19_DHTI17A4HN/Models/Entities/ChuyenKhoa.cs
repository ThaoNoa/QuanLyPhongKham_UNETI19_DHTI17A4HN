// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Entity ChuyenKhoa - quản lý chuyên khoa của phòng khám.
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities
{
    public class ChuyenKhoa
    {
        [Key]
        [Display(Name = "Mã chuyên khoa")]
        public int MaChuyenKhoa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chuyên khoa")]
        [StringLength(100, ErrorMessage = "Tên chuyên khoa tối đa 100 ký tự")]
        [Display(Name = "Tên chuyên khoa")]
        public string TenChuyenKhoa { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số phòng")]
        [Range(0, 1000, ErrorMessage = "Số phòng phải từ 0 đến 1000 (không âm)")]
        [Display(Name = "Số phòng")]
        public int SoPhong { get; set; }

        // true: Hoạt động, false: Ngừng hoạt động
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation: ChuyenKhoa (1) - (n) BacSi
        public ICollection<BacSi> BacSis { get; set; } = new List<BacSi>();
    }
}
