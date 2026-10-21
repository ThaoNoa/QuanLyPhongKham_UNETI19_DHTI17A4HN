// Họ và tên: Đỗ Huy Tùng
// Mã sinh viên: 23103100296
// Nội dung thực hiện: Tiếp nhận lịch khám - danh sách lịch cần xử lý: tìm theo bệnh nhân/bác sĩ,
// lọc theo chuyên khoa, trạng thái, khoảng ngày khám.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Controllers
{
    public class TiepNhanController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly PhongKhamDbContext _context;

        public TiepNhanController(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ======================= DANH SÁCH LỊCH CẦN XỬ LÝ =======================

        // GET: /TiepNhan?tuKhoa=&maChuyenKhoa=&trangThai=&tuNgay=&denNgay=
        public async Task<IActionResult> Index(string? tuKhoa, int? maChuyenKhoa, TrangThaiLichKham? trangThai,
            DateTime? tuNgay, DateTime? denNgay)
        {
            IQueryable<LichKham> query = _context.LichKhams.AsNoTracking();

            // Tìm theo tên bệnh nhân, số điện thoại bệnh nhân hoặc tên bác sĩ
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(l => l.BenhNhan!.HoTen.Contains(tk) || l.BenhNhan.SoDienThoai.Contains(tk) || l.BacSi!.HoTen.Contains(tk));
            }
            // Lọc theo chuyên khoa (thông qua bác sĩ)
            if (maChuyenKhoa.HasValue) query = query.Where(l => l.BacSi!.MaChuyenKhoa == maChuyenKhoa.Value);
            // Lọc theo khoảng ngày khám
            if (tuNgay.HasValue) query = query.Where(l => l.NgayKham >= tuNgay.Value.Date);
            if (denNgay.HasValue) query = query.Where(l => l.NgayKham <= denNgay.Value.Date);

            // Lọc theo trạng thái
            if (trangThai.HasValue) query = query.Where(l => l.TrangThai == trangThai.Value);

            // Lịch mới nhất lên đầu
            query = query.OrderByDescending(l => l.NgayKham).ThenByDescending(l => l.GioBatDau);

            query = query.Include(l => l.BenhNhan).Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa);

            var vm = new TiepNhanIndexViewModel
            {
                TuKhoa = tuKhoa,
                MaChuyenKhoa = maChuyenKhoa,
                TrangThai = trangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                DanhSach = await PaginatedList<LichKham>.CreateAsync(query, 1, KichThuocTrang),
                ChuyenKhoas = new SelectList(await _context.ChuyenKhoas.AsNoTracking().OrderBy(c => c.TenChuyenKhoa).ToListAsync(),
                    nameof(ChuyenKhoa.MaChuyenKhoa), nameof(ChuyenKhoa.TenChuyenKhoa), maChuyenKhoa)
            };
            return View(vm);
        }
    }
}
