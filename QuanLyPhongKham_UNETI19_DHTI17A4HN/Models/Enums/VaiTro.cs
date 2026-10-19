// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Khai báo các vai trò người dùng dùng cho đăng nhập và phân quyền.
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums
{
    public enum VaiTro
    {
        [Display(Name = "Quản trị viên")] Admin = 1,
        [Display(Name = "Nhân viên")] NhanVien = 2,
        [Display(Name = "Bác sĩ")] BacSi = 3,
        [Display(Name = "Bệnh nhân")] BenhNhan = 4
    }
}
