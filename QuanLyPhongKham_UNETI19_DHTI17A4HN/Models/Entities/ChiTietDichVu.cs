// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Entity hỗ trợ ChiTietDichVu - các dịch vụ đã sử dụng trong một kết quả khám,
// dùng để tính doanh thu dịch vụ theo tháng, theo bác sĩ, theo chuyên khoa.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities
{
    public class ChiTietDichVu
    {
        [Key]
        public int MaChiTiet { get; set; }

        [Display(Name = "Kết quả khám")]
        public int MaKetQua { get; set; }

        [Display(Name = "Dịch vụ")]
        public int MaDichVu { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng từ 1 đến 100")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; } = 1;

        // Lưu lại đơn giá tại thời điểm sử dụng (giá dịch vụ có thể thay đổi sau này)
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [ForeignKey(nameof(MaKetQua))]
        public KetQuaKham? KetQuaKham { get; set; }

        [ForeignKey(nameof(MaDichVu))]
        public DichVuYTe? DichVuYTe { get; set; }
    }
}
