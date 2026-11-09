// Họ và tên: Trần Phương Thảo (Nhóm trưởng)
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Trang chủ - chuyên khoa nổi bật, bác sĩ tiêu biểu, số liệu tổng quan, ô tìm kiếm nhanh; trang lỗi.
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Controllers
{
    public class HomeController : Controller
    {
        private readonly PhongKhamDbContext _context;

        public HomeController(PhongKhamDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new TrangChuViewModel
            {
                ChuyenKhoas = await _context.ChuyenKhoas.AsNoTracking()
                    .Where(c => c.TrangThai)
                    .OrderBy(c => c.MaChuyenKhoa)
                    .Select(c => new ChuyenKhoaItemViewModel
                    {
                        MaChuyenKhoa = c.MaChuyenKhoa,
                        TenChuyenKhoa = c.TenChuyenKhoa,
                        SoBacSi = c.BacSis.Count(b => b.TrangThai)
                    })
                    .Take(8).ToListAsync(),

                // Bác sĩ tiêu biểu: bác sĩ đang hoạt động có nhiều lượt khám nhất
                BacSiTieuBieu = await _context.BacSis.AsNoTracking()
                    .Include(b => b.ChuyenKhoa)
                    .Where(b => b.TrangThai)
                    .OrderByDescending(b => b.LichKhams.Count(l => l.TrangThai == TrangThaiLichKham.DaKham))
                    .ThenByDescending(b => b.HocVi)
                    .Take(4).ToListAsync(),

                TongBacSi = await _context.BacSis.CountAsync(b => b.TrangThai),
                TongChuyenKhoa = await _context.ChuyenKhoas.CountAsync(c => c.TrangThai),
                TongBenhNhan = await _context.BenhNhans.CountAsync(),
                TongLuotKham = await _context.LichKhams.CountAsync(l => l.TrangThai == TrangThaiLichKham.DaKham)
            };

            var ma = vm.BacSiTieuBieu.Select(b => b.MaBacSi).ToList();
            vm.SoLuotKham = await _context.LichKhams
                .Where(l => ma.Contains(l.MaBacSi) && l.TrangThai == TrangThaiLichKham.DaKham)
                .GroupBy(l => l.MaBacSi)
                .Select(g => new { g.Key, SoLuot = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.SoLuot);
            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
