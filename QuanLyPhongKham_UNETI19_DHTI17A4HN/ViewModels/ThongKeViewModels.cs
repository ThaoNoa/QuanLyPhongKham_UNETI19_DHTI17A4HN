// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: ViewModel cho Dashboard và trang thống kê (kết quả các truy vấn LINQ).
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels
{
    // Một dòng thống kê dạng (Tên, Giá trị)
    public class DongThongKe
    {
        public string Ten { get; set; } = string.Empty;
        public string? PhuDe { get; set; }
        public int SoLuong { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class DashboardViewModel
    {
        public int Nam { get; set; }
        public int TongChuyenKhoa { get; set; }
        public int TongBacSi { get; set; }
        public int TongBenhNhan { get; set; }
        public int SoLichChoXacNhan { get; set; }
        public int SoLichDaXacNhan { get; set; }
        public int SoLichDaKham { get; set; }
        public int SoLichDaHuy { get; set; }
        public int SoLichHomNay { get; set; }
        public decimal DoanhThuNam { get; set; }

        public int[] BenhNhanTheoThang { get; set; } = new int[12];
        public decimal[] DoanhThuTheoThang { get; set; } = new decimal[12];
        public List<DongThongKe> BacSiNhieuLichNhat { get; set; } = new();
        public List<LichKham> LichChoXuLy { get; set; } = new();
    }

    public class ThongKeViewModel
    {
        public int Nam { get; set; }
        public List<int> CacNam { get; set; } = new();

        public List<DongThongKe> BenhNhanTheoKhoa { get; set; } = new();
        public List<DongThongKe> DoanhThuTheoNam { get; set; } = new();
        public decimal TrungBinhDoanhThuNam { get; set; }
        public decimal TongDoanhThuNamChon { get; set; }
        public decimal TrungBinhThangNamChon { get; set; }
        public List<DongThongKe> DoanhThuTheoBacSi { get; set; } = new();
        public List<DongThongKe> DoanhThuTheoChuyenKhoa { get; set; } = new();
        public DongThongKe? KhoaDongNhat { get; set; }
        public List<DongThongKe> DichVuDungNhieu { get; set; } = new();
        public decimal[] DoanhThuTheoThang { get; set; } = new decimal[12];
    }

    public class TrangChuViewModel
    {
        public List<ChuyenKhoaItemViewModel> ChuyenKhoas { get; set; } = new();
        public List<BacSi> BacSiTieuBieu { get; set; } = new();
        public Dictionary<int, int> SoLuotKham { get; set; } = new();
        public int TongBacSi { get; set; }
        public int TongChuyenKhoa { get; set; }
        public int TongBenhNhan { get; set; }
        public int TongLuotKham { get; set; }
    }
}
