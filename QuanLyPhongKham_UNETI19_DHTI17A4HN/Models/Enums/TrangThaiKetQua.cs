// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Khai báo trạng thái của kết quả khám.
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums
{
    public enum TrangThaiKetQua
    {
        [Display(Name = "Đang cập nhật")] DangCapNhat = 0,
        [Display(Name = "Hoàn thành")] HoanThanh = 1
    }
}
