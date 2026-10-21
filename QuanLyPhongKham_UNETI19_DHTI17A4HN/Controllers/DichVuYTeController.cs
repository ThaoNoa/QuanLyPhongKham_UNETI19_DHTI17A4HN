// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Quản lý dịch vụ y tế - tìm kiếm, lọc trạng thái, sắp xếp theo tên/đơn giá, phân trang,
// thêm, sửa, xem (số lần sử dụng, doanh thu), xóa (không xóa dịch vụ đã sử dụng).
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
    [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
    public class DichVuYTeController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly PhongKhamDbContext _context;

        public DichVuYTeController(PhongKhamDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai, string? sapXep, int trang = 1)
        {
            var query = _context.DichVuYTes.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(d => d.TenDichVu.Contains(tk) || (d.MoTa != null && d.MoTa.Contains(tk)));
            }
            if (trangThai.HasValue) query = query.Where(d => d.TrangThai == trangThai.Value);

            query = sapXep switch
            {
                "gia" => query.OrderBy(d => d.DonGia),
                "gia_desc" => query.OrderByDescending(d => d.DonGia),
                "ten_desc" => query.OrderByDescending(d => d.TenDichVu),
                _ => query.OrderBy(d => d.TenDichVu)
            };

            var vm = new DichVuYTeIndexViewModel
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                SapXep = sapXep,
                DanhSach = await PaginatedList<DichVuYTe>.CreateAsync(query, trang, KichThuocTrang)
            };
            // Số lần mỗi dịch vụ được sử dụng (GroupBy + Sum)
            var maDv = vm.DanhSach.Select(d => d.MaDichVu).ToList();
            vm.SoLanSuDung = await _context.ChiTietDichVus
                .Where(c => maDv.Contains(c.MaDichVu))
                .GroupBy(c => c.MaDichVu)
                .Select(g => new { g.Key, SoLan = g.Sum(c => c.SoLuong) })
                .ToDictionaryAsync(x => x.Key, x => x.SoLan);
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var dv = await _context.DichVuYTes.AsNoTracking().FirstOrDefaultAsync(d => d.MaDichVu == id);
            if (dv == null) return NotFound();
            ViewBag.SoLan = await _context.ChiTietDichVus.Where(c => c.MaDichVu == id).SumAsync(c => c.SoLuong);
            ViewBag.DoanhThu = await _context.ChiTietDichVus.Where(c => c.MaDichVu == id).SumAsync(c => c.SoLuong * c.DonGia);
            return View(dv);
        }

        public IActionResult Create() => View(new DichVuYTe());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDichVu,DonViTinh,DonGia,MoTa,TrangThai")] DichVuYTe dichVu)
        {
            dichVu.TenDichVu = dichVu.TenDichVu?.Trim() ?? string.Empty;
            if (await _context.DichVuYTes.AnyAsync(d => d.TenDichVu == dichVu.TenDichVu))
                ModelState.AddModelError(nameof(DichVuYTe.TenDichVu), "Tên dịch vụ đã tồn tại.");
            if (!ModelState.IsValid) return View(dichVu);

            _context.DichVuYTes.Add(dichVu);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã thêm dịch vụ \"{dichVu.TenDichVu}\".";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dv = await _context.DichVuYTes.FindAsync(id);
            if (dv == null) return NotFound();
            return View(dv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDichVu,TenDichVu,DonViTinh,DonGia,MoTa,TrangThai")] DichVuYTe model)
        {
            if (id != model.MaDichVu) return NotFound();
            model.TenDichVu = model.TenDichVu?.Trim() ?? string.Empty;
            if (await _context.DichVuYTes.AnyAsync(d => d.TenDichVu == model.TenDichVu && d.MaDichVu != id))
                ModelState.AddModelError(nameof(DichVuYTe.TenDichVu), "Tên dịch vụ đã tồn tại.");
            if (!ModelState.IsValid) return View(model);

            var dv = await _context.DichVuYTes.FindAsync(id);
            if (dv == null) return NotFound();
            dv.TenDichVu = model.TenDichVu;
            dv.DonViTinh = model.DonViTinh;
            dv.DonGia = model.DonGia;      // Giá mới chỉ áp dụng cho lần sử dụng sau (chi tiết cũ đã lưu đơn giá riêng)
            dv.MoTa = model.MoTa;
            dv.TrangThai = model.TrangThai;
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Cập nhật dịch vụ thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dv = await _context.DichVuYTes.FindAsync(id);
            if (dv == null) return NotFound();
            if (await _context.ChiTietDichVus.AnyAsync(c => c.MaDichVu == id))
            {
                TempData["Loi"] = $"Dịch vụ \"{dv.TenDichVu}\" đã được sử dụng trong kết quả khám, không thể xóa. Hãy chuyển sang ngừng áp dụng.";
                return RedirectToAction(nameof(Index));
            }
            _context.DichVuYTes.Remove(dv);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã xóa dịch vụ \"{dv.TenDichVu}\".";
            return RedirectToAction(nameof(Index));
        }
    }
}
