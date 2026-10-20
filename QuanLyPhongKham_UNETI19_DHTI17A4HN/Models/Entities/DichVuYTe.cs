// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Entity DichVuYTe - danh mục dịch vụ y tế và đơn giá.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities
{
    public class DichVuYTe
    {
        [Key]
        [Display(Name = "Mã dịch vụ")]
        public int MaDichVu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
        [StringLength(150, ErrorMessage = "Tên dịch vụ tối đa 150 ký tự")]
        [Display(Name = "Tên dịch vụ")]
        public string TenDichVu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
        [StringLength(30)]
        [Display(Name = "Đơn vị tính")]
        public string DonViTinh { get; set; } = "Lần";

        [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
        [Range(0, 100_000_000, ErrorMessage = "Đơn giá từ 0 đến 100.000.000")]
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Đơn giá (VNĐ)")]
        public decimal DonGia { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public ICollection<ChiTietDichVu> ChiTietDichVus { get; set; } = new List<ChiTietDichVu>();
    }
}
