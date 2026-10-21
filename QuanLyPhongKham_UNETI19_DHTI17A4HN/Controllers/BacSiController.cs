// Họ và tên: Lê Quang Trung
// Mã sinh viên: 23103100270
// Nội dung thực hiện: Quản lý bác sĩ - danh sách: tìm kiếm theo họ tên/chuyên môn, lọc theo chuyên khoa/trạng thái,
// sắp xếp theo tên/học vị, phân trang (Skip/Take) và giữ điều kiện lọc khi chuyển trang; chi tiết bác sĩ.
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
    public class BacSiController : Controller
    {
        private const int KichThuocTrang = 8;

        private readonly PhongKhamDbContext _context;

        public BacSiController(PhongKhamDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Truy vấn dữ liệu -> Tìm kiếm -> Lọc -> Sắp xếp. (Phân trang thực hiện sau bằng PaginatedList)
        /// </summary>
        private IQueryable<BacSi> TaoTruyVan(string? tuKhoa, int? maChuyenKhoa, bool? trangThai, string? sapXep)
        {
            IQueryable<BacSi> query = _context.BacSis.AsNoTracking().Include(b => b.ChuyenKhoa);

            // Tìm theo họ tên hoặc chuyên môn
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(b => b.HoTen.Contains(tk) || b.ChuyenMon.Contains(tk));
            }

            // Lọc theo chuyên khoa
            if (maChuyenKhoa.HasValue)
                query = query.Where(b => b.MaChuyenKhoa == maChuyenKhoa.Value);

            // Lọc theo trạng thái
            if (trangThai.HasValue)
                query = query.Where(b => b.TrangThai == trangThai.Value);

            // Sắp xếp theo tên hoặc học vị (học vị lưu dạng số tăng dần theo cấp bậc)
            query = sapXep switch
            {
                "ten_desc" => query.OrderByDescending(b => b.HoTen),
                "hocvi" => query.OrderBy(b => b.HocVi).ThenBy(b => b.HoTen),
                "hocvi_desc" => query.OrderByDescending(b => b.HocVi).ThenBy(b => b.HoTen),
                _ => query.OrderBy(b => b.HoTen)
            };
            return query;
        }

        private async Task<Dictionary<int, int>> DemLuotKhamAsync(IEnumerable<int> maBacSis)
        {
            var ds = maBacSis.ToList();
            return await _context.LichKhams
                .Where(l => ds.Contains(l.MaBacSi) && l.TrangThai == TrangThaiLichKham.DaKham)
                .GroupBy(l => l.MaBacSi)
                .Select(g => new { MaBacSi = g.Key, SoLuot = g.Count() })
                .ToDictionaryAsync(x => x.MaBacSi, x => x.SoLuot);
        }

        private SelectList TaoSelectChuyenKhoa(int? chon = null, bool chiHoatDong = false)
        {
            var ds = _context.ChuyenKhoas.AsNoTracking()
                .Where(c => !chiHoatDong || c.TrangThai)
                .OrderBy(c => c.TenChuyenKhoa).ToList();
            return new SelectList(ds, nameof(ChuyenKhoa.MaChuyenKhoa), nameof(ChuyenKhoa.TenChuyenKhoa), chon);
        }

        // ============================ QUẢN TRỊ ============================

        // GET: /BacSi?tuKhoa=&maChuyenKhoa=&trangThai=&sapXep=&trang=
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Index(string? tuKhoa, int? maChuyenKhoa, bool? trangThai, string? sapXep, int trang = 1)
        {
            var query = TaoTruyVan(tuKhoa, maChuyenKhoa, trangThai, sapXep);
            var vm = new BacSiIndexViewModel
            {
                TuKhoa = tuKhoa,
                MaChuyenKhoa = maChuyenKhoa,
                TrangThai = trangThai,
                SapXep = sapXep,
                DanhSach = await PaginatedList<BacSi>.CreateAsync(query, trang, KichThuocTrang),
                ChuyenKhoas = TaoSelectChuyenKhoa(maChuyenKhoa)
            };
            vm.SoLuotKham = await DemLuotKhamAsync(vm.DanhSach.Select(b => b.MaBacSi));
            return View(vm);
        }

        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Details(int id)
        {
            var bacSi = await _context.BacSis.AsNoTracking()
                .Include(b => b.ChuyenKhoa)
                .Include(b => b.TaiKhoan)
                .FirstOrDefaultAsync(b => b.MaBacSi == id);
            if (bacSi == null) return NotFound();

            var vm = new BacSiChiTietViewModel
            {
                BacSi = bacSi,
                SoLuotKham = await _context.LichKhams.CountAsync(l => l.MaBacSi == id && l.TrangThai == TrangThaiLichKham.DaKham),
                SoLichSapToi = await _context.LichKhams.CountAsync(l => l.MaBacSi == id && l.NgayKham >= DateTime.Today
                    && (l.TrangThai == TrangThaiLichKham.ChoXacNhan || l.TrangThai == TrangThaiLichKham.DaXacNhan))
            };
            return View(vm);
        }
    }
}
