// Họ và tên: Đỗ Huy Tùng
// Mã sinh viên: 23103100296
// Nội dung thực hiện: Tiếp nhận lịch khám - danh sách lịch cần xử lý (tìm theo bệnh nhân/bác sĩ,
// lọc chuyên khoa/trạng thái/ngày, sắp xếp theo ngày khám, phân trang, đếm số lịch theo trạng thái); chi tiết lịch khám.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Filters;
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

        // GET: /TiepNhan?tuKhoa=&maChuyenKhoa=&trangThai=&tuNgay=&denNgay=&sapXep=&trang=
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Index(string? tuKhoa, int? maChuyenKhoa, TrangThaiLichKham? trangThai,
            DateTime? tuNgay, DateTime? denNgay, string? sapXep, int trang = 1)
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

            // Đếm số lịch theo từng trạng thái (trước khi lọc trạng thái) để hiển thị trên các tab
            var soLuong = await query.GroupBy(l => l.TrangThai)
                .Select(g => new { g.Key, SoLuong = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.SoLuong);

            // Lọc theo trạng thái
            if (trangThai.HasValue) query = query.Where(l => l.TrangThai == trangThai.Value);

            // Sắp xếp theo ngày khám (kèm giờ bắt đầu)
            query = sapXep switch
            {
                "ngay" => query.OrderBy(l => l.NgayKham).ThenBy(l => l.GioBatDau),
                "ngaydat_desc" => query.OrderByDescending(l => l.NgayTao),
                _ => query.OrderByDescending(l => l.NgayKham).ThenByDescending(l => l.GioBatDau)
            };

            query = query.Include(l => l.BenhNhan).Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa);

            var vm = new TiepNhanIndexViewModel
            {
                TuKhoa = tuKhoa,
                MaChuyenKhoa = maChuyenKhoa,
                TrangThai = trangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                SapXep = sapXep,
                SoLuongTheoTrangThai = soLuong,
                DanhSach = await PaginatedList<LichKham>.CreateAsync(query, trang, KichThuocTrang),
                ChuyenKhoas = new SelectList(await _context.ChuyenKhoas.AsNoTracking().OrderBy(c => c.TenChuyenKhoa).ToListAsync(),
                    nameof(ChuyenKhoa.MaChuyenKhoa), nameof(ChuyenKhoa.TenChuyenKhoa), maChuyenKhoa)
            };
            return View(vm);
        }

        // GET: /TiepNhan/ChiTiet/5
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien, VaiTro.BacSi)]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var lich = await _context.LichKhams.AsNoTracking()
                .Include(l => l.BenhNhan)
                .Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                .Include(l => l.KetQuaKham)
                .FirstOrDefaultAsync(l => l.MaLichKham == id);
            if (lich == null) return NotFound();

            // Bác sĩ chỉ xem lịch của chính mình
            if (HttpContext.Session.LayVaiTro() == VaiTro.BacSi && lich.BacSi?.MaTaiKhoan != HttpContext.Session.LayMaTaiKhoan())
                return RedirectToAction("TuChoiTruyCap", "TaiKhoan");

            // Lịch sử khám trước đây của bệnh nhân
            ViewBag.LichSu = await _context.LichKhams.AsNoTracking()
                .Include(l => l.BacSi)
                .Where(l => l.MaBenhNhan == lich.MaBenhNhan && l.MaLichKham != id && l.TrangThai == TrangThaiLichKham.DaKham)
                .OrderByDescending(l => l.NgayKham).Take(5).ToListAsync();
            return View(lich);
        }
    }
}
