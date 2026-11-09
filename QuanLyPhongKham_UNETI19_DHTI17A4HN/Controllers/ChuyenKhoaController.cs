// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Quản lý chuyên khoa (Admin) - tìm kiếm, lọc, sắp xếp, phân trang, thêm, sửa, xem, xóa;
// trang danh sách chuyên khoa công khai cho bệnh nhân.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Filters;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Controllers
{
    public class ChuyenKhoaController : Controller
    {
        private const int KichThuocTrang = 8;
        private readonly PhongKhamDbContext _context;

        public ChuyenKhoaController(PhongKhamDbContext context)
        {
            _context = context;
        }

        // GET: /ChuyenKhoa/DanhSach - trang công khai
        public async Task<IActionResult> DanhSach()
        {
            var ds = await _context.ChuyenKhoas.AsNoTracking()
                .Where(c => c.TrangThai)
                .OrderBy(c => c.TenChuyenKhoa)
                .Select(c => new ChuyenKhoaItemViewModel
                {
                    MaChuyenKhoa = c.MaChuyenKhoa,
                    TenChuyenKhoa = c.TenChuyenKhoa,
                    MoTa = c.MoTa,
                    SoPhong = c.SoPhong,
                    TrangThai = c.TrangThai,
                    SoBacSi = c.BacSis.Count(b => b.TrangThai)
                })
                .ToListAsync();
            return View(ds);
        }

        // GET: /ChuyenKhoa
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai, string? sapXep, int trang = 1)
        {
            var query = _context.ChuyenKhoas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(c => c.TenChuyenKhoa.Contains(tk) || (c.MoTa != null && c.MoTa.Contains(tk)));
            }
            if (trangThai.HasValue) query = query.Where(c => c.TrangThai == trangThai.Value);

            // Chiếu sang ViewModel, đếm số bác sĩ của mỗi khoa (COUNT trên SQL Server)
            var duLieu = query.Select(c => new ChuyenKhoaItemViewModel
            {
                MaChuyenKhoa = c.MaChuyenKhoa,
                TenChuyenKhoa = c.TenChuyenKhoa,
                MoTa = c.MoTa,
                SoPhong = c.SoPhong,
                TrangThai = c.TrangThai,
                SoBacSi = c.BacSis.Count()
            });

            duLieu = sapXep switch
            {
                "ten_desc" => duLieu.OrderByDescending(c => c.TenChuyenKhoa),
                "phong" => duLieu.OrderBy(c => c.SoPhong),
                "bacsi_desc" => duLieu.OrderByDescending(c => c.SoBacSi).ThenBy(c => c.TenChuyenKhoa),
                _ => duLieu.OrderBy(c => c.TenChuyenKhoa)
            };

            return View(new ChuyenKhoaIndexViewModel
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                SapXep = sapXep,
                DanhSach = await PaginatedList<ChuyenKhoaItemViewModel>.CreateAsync(duLieu, trang, KichThuocTrang)
            });
        }

        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Details(int id)
        {
            var khoa = await _context.ChuyenKhoas.AsNoTracking()
                .Include(c => c.BacSis)
                .FirstOrDefaultAsync(c => c.MaChuyenKhoa == id);
            if (khoa == null) return NotFound();
            return View(khoa);
        }

        [PhanQuyen(VaiTro.Admin)]
        public IActionResult Create() => View(new ChuyenKhoa());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Create([Bind("TenChuyenKhoa,MoTa,SoPhong,TrangThai")] ChuyenKhoa chuyenKhoa)
        {
            chuyenKhoa.TenChuyenKhoa = chuyenKhoa.TenChuyenKhoa?.Trim() ?? string.Empty;
            // Tên chuyên khoa không trùng
            if (await _context.ChuyenKhoas.AnyAsync(c => c.TenChuyenKhoa == chuyenKhoa.TenChuyenKhoa))
                ModelState.AddModelError(nameof(ChuyenKhoa.TenChuyenKhoa), "Tên chuyên khoa đã tồn tại.");

            if (!ModelState.IsValid) return View(chuyenKhoa);

            _context.ChuyenKhoas.Add(chuyenKhoa);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã thêm chuyên khoa \"{chuyenKhoa.TenChuyenKhoa}\".";
            return RedirectToAction(nameof(Index));
        }

        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var khoa = await _context.ChuyenKhoas.FindAsync(id);
            if (khoa == null) return NotFound();
            return View(khoa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Edit(int id, [Bind("MaChuyenKhoa,TenChuyenKhoa,MoTa,SoPhong,TrangThai")] ChuyenKhoa chuyenKhoa)
        {
            if (id != chuyenKhoa.MaChuyenKhoa) return NotFound();
            chuyenKhoa.TenChuyenKhoa = chuyenKhoa.TenChuyenKhoa?.Trim() ?? string.Empty;

            if (await _context.ChuyenKhoas.AnyAsync(c => c.TenChuyenKhoa == chuyenKhoa.TenChuyenKhoa && c.MaChuyenKhoa != id))
                ModelState.AddModelError(nameof(ChuyenKhoa.TenChuyenKhoa), "Tên chuyên khoa đã tồn tại.");

            if (!ModelState.IsValid) return View(chuyenKhoa);

            var khoa = await _context.ChuyenKhoas.FindAsync(id);
            if (khoa == null) return NotFound();
            khoa.TenChuyenKhoa = chuyenKhoa.TenChuyenKhoa;
            khoa.MoTa = chuyenKhoa.MoTa;
            khoa.SoPhong = chuyenKhoa.SoPhong;
            khoa.TrangThai = chuyenKhoa.TrangThai;
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Cập nhật chuyên khoa thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khoa = await _context.ChuyenKhoas.FindAsync(id);
            if (khoa == null) return NotFound();

            // Không xóa chuyên khoa đang có bác sĩ (ràng buộc khóa ngoại Restrict)
            if (await _context.BacSis.AnyAsync(b => b.MaChuyenKhoa == id))
            {
                TempData["Loi"] = $"Không thể xóa \"{khoa.TenChuyenKhoa}\" vì còn bác sĩ thuộc chuyên khoa. Hãy chuyển sang trạng thái ngừng hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            _context.ChuyenKhoas.Remove(khoa);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã xóa chuyên khoa \"{khoa.TenChuyenKhoa}\".";
            return RedirectToAction(nameof(Index));
        }
    }
}
