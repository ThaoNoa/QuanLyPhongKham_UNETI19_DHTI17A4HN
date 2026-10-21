// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Quản lý bệnh nhân (Admin/Nhân viên) - danh sách bệnh nhân: tìm kiếm, lọc, sắp xếp, phân trang.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Controllers
{
    public class BenhNhanController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly PhongKhamDbContext _context;

        public BenhNhanController(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ======================= QUẢN TRỊ =======================

        // GET: /BenhNhan?tuKhoa=&gioiTinh=&trangThai=&sapXep=&trang=
        public async Task<IActionResult> Index(string? tuKhoa, string? gioiTinh, bool? trangThai, string? sapXep, int trang = 1)
        {
            var query = _context.BenhNhans.AsNoTracking().AsQueryable();

            // Tìm theo họ tên, số điện thoại, số BHYT
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(b => b.HoTen.Contains(tk) || b.SoDienThoai.Contains(tk)
                                      || (b.SoBHYT != null && b.SoBHYT.Contains(tk)));
            }
            if (!string.IsNullOrEmpty(gioiTinh)) query = query.Where(b => b.GioiTinh == gioiTinh);
            if (trangThai.HasValue) query = query.Where(b => b.TrangThai == trangThai.Value);

            query = sapXep switch
            {
                "ten_desc" => query.OrderByDescending(b => b.HoTen),
                "ngaydk_desc" => query.OrderByDescending(b => b.NgayDangKy),
                "ngaydk" => query.OrderBy(b => b.NgayDangKy),
                "tuoi" => query.OrderByDescending(b => b.NgaySinh),
                _ => query.OrderBy(b => b.HoTen)
            };

            return View(new BenhNhanIndexViewModel
            {
                TuKhoa = tuKhoa,
                GioiTinh = gioiTinh,
                TrangThai = trangThai,
                SapXep = sapXep,
                DanhSach = await PaginatedList<BenhNhan>.CreateAsync(query, trang, KichThuocTrang)
            });
        }
    }
}
