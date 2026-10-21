// Họ và tên: Lê Quang Trung
// Mã sinh viên: 23103100270
// Nội dung thực hiện: ViewModel cho thanh phân trang dùng chung - giữ nguyên điều kiện tìm kiếm/lọc/sắp xếp khi chuyển trang.
namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels
{
    public class PhanTrangViewModel
    {
        public int TrangHienTai { get; set; }
        public int TongSoTrang { get; set; }
        public int TongSoBanGhi { get; set; }
        public string Action { get; set; } = "Index";
        public string? Controller { get; set; }

        // Các tham số tìm kiếm/lọc/sắp xếp hiện tại, sẽ được gắn lại vào link của từng trang
        public Dictionary<string, string?> ThamSo { get; set; } = new();

        public Dictionary<string, string?> ThamSoTrang(int trang)
        {
            var ds = new Dictionary<string, string?>(ThamSo) { ["trang"] = trang.ToString() };
            return ds;
        }
    }
}
