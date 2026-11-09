// Họ và tên: Trần Phương Thảo (Nhóm trưởng)
// Mã sinh viên: 23103100226
// Nội dung thực hiện: ViewModel trang lỗi (mẫu mặc định của ASP.NET Core MVC).
namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
