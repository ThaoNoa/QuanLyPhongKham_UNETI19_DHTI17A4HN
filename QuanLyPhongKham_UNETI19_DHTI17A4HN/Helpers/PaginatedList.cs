// Họ và tên: Lê Quang Trung
// Mã sinh viên: 23103100270
// Nội dung thực hiện: Lớp phân trang dùng chung - phân trang ngay trên truy vấn EF Core bằng Skip(), Take().
using Microsoft.EntityFrameworkCore;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int TrangHienTai { get; }
        public int TongSoTrang { get; }
        public int TongSoBanGhi { get; }
        public int KichThuocTrang { get; }

        public PaginatedList(List<T> items, int tongSoBanGhi, int trang, int kichThuocTrang)
        {
            TongSoBanGhi = tongSoBanGhi;
            KichThuocTrang = kichThuocTrang;
            TongSoTrang = Math.Max(1, (int)Math.Ceiling(tongSoBanGhi / (double)kichThuocTrang));
            TrangHienTai = trang;
            AddRange(items);
        }

        public bool CoTrangTruoc => TrangHienTai > 1;
        public bool CoTrangSau => TrangHienTai < TongSoTrang;

        // Thứ tự chỉ số bản ghi đầu tiên của trang (phục vụ cột STT)
        public int ChiSoBatDau => (TrangHienTai - 1) * KichThuocTrang;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> nguon, int trang, int kichThuocTrang)
        {
            // Đếm tổng số bản ghi thỏa điều kiện (COUNT trên SQL Server)
            var tongSo = await nguon.CountAsync();
            var tongSoTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)kichThuocTrang));
            trang = Math.Clamp(trang, 1, tongSoTrang);

            // Chỉ lấy đúng các bản ghi của trang hiện tại (OFFSET ... FETCH trên SQL Server)
            var items = await nguon.Skip((trang - 1) * kichThuocTrang)
                                   .Take(kichThuocTrang)
                                   .ToListAsync();
            return new PaginatedList<T>(items, tongSo, trang, kichThuocTrang);
        }
    }
}
