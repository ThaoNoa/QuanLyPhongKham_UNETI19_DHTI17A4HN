// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Khai báo các trạng thái của lịch khám.
// Luồng: Chờ xác nhận -> Đã xác nhận -> Đã khám; Chờ xác nhận -> Đã hủy.
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums
{
    public enum TrangThaiLichKham
    {
        [Display(Name = "Chờ xác nhận")] ChoXacNhan = 0,
        [Display(Name = "Đã xác nhận")] DaXacNhan = 1,
        [Display(Name = "Đã khám")] DaKham = 2,
        [Display(Name = "Đã hủy")] DaHuy = 3
    }
}
