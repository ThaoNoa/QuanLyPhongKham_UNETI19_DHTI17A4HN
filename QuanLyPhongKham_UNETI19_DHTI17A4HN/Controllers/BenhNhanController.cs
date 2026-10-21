// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Quản lý bệnh nhân (Admin/Nhân viên) - tìm kiếm, lọc, sắp xếp, phân trang, thêm, sửa, xem, xóa;
// Hồ sơ cá nhân của bệnh nhân (bệnh nhân chỉ xem/sửa hồ sơ của chính mình, lấy theo Session).
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
    public class BenhNhanController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly PhongKhamDbContext _context;

        public BenhNhanController(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ======================= QUẢN TRỊ =======================

        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
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

        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Details(int id)
        {
            var benhNhan = await _context.BenhNhans.AsNoTracking()
                .Include(b => b.TaiKhoan)
                .FirstOrDefaultAsync(b => b.MaBenhNhan == id);
            if (benhNhan == null) return NotFound();

            var lich = await _context.LichKhams.AsNoTracking()
                .Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                .Include(l => l.KetQuaKham)
                .Where(l => l.MaBenhNhan == id)
                .OrderByDescending(l => l.NgayKham).ThenByDescending(l => l.GioBatDau)
                .ToListAsync();

            return View(new BenhNhanChiTietViewModel { BenhNhan = benhNhan, LichKhams = lich });
        }

        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public IActionResult Create() => View(new BenhNhan { NgaySinh = new DateTime(1990, 1, 1) });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Create([Bind("HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,SoBHYT,TrangThai,GhiChu")] BenhNhan benhNhan)
        {
            await KiemTraNghiepVuAsync(benhNhan);
            if (!ModelState.IsValid) return View(benhNhan);

            benhNhan.HoTen = benhNhan.HoTen.Trim();
            benhNhan.NgayDangKy = DateTime.Today;
            _context.BenhNhans.Add(benhNhan);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã thêm bệnh nhân {benhNhan.HoTen}.";
            return RedirectToAction(nameof(Details), new { id = benhNhan.MaBenhNhan });
        }

        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Edit(int id)
        {
            var benhNhan = await _context.BenhNhans.FindAsync(id);
            if (benhNhan == null) return NotFound();
            return View(benhNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Edit(int id, [Bind("MaBenhNhan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,SoBHYT,TrangThai,GhiChu")] BenhNhan model)
        {
            if (id != model.MaBenhNhan) return NotFound();
            await KiemTraNghiepVuAsync(model);
            if (!ModelState.IsValid) return View(model);

            var benhNhan = await _context.BenhNhans.FindAsync(id);
            if (benhNhan == null) return NotFound();
            CapNhatThongTin(benhNhan, model);
            benhNhan.TrangThai = model.TrangThai;
            benhNhan.GhiChu = model.GhiChu;
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Cập nhật thông tin bệnh nhân thành công.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhans.FindAsync(id);
            if (benhNhan == null) return NotFound();
            if (await _context.LichKhams.AnyAsync(l => l.MaBenhNhan == id))
            {
                TempData["Loi"] = $"Không thể xóa bệnh nhân {benhNhan.HoTen} vì đã có lịch khám. Hãy chuyển sang trạng thái ngừng theo dõi.";
                return RedirectToAction(nameof(Index));
            }
            _context.BenhNhans.Remove(benhNhan);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã xóa bệnh nhân {benhNhan.HoTen}.";
            return RedirectToAction(nameof(Index));
        }

        // ======================= HỒ SƠ CÁ NHÂN (BỆNH NHÂN) =======================

        // Lấy hồ sơ theo Mã tài khoản trong Session - không nhận mã từ URL nên không xem được hồ sơ người khác
        private Task<BenhNhan?> LayHoSoCuaToiAsync()
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            return _context.BenhNhans.Include(b => b.TaiKhoan).FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);
        }

        [PhanQuyen(VaiTro.BenhNhan)]
        public async Task<IActionResult> HoSo()
        {
            var benhNhan = await LayHoSoCuaToiAsync();
            if (benhNhan == null) return NotFound();
            ViewBag.SoLichKham = await _context.LichKhams.CountAsync(l => l.MaBenhNhan == benhNhan.MaBenhNhan);
            ViewBag.SoDaKham = await _context.LichKhams.CountAsync(l => l.MaBenhNhan == benhNhan.MaBenhNhan && l.TrangThai == TrangThaiLichKham.DaKham);
            return View(benhNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.BenhNhan)]
        public async Task<IActionResult> HoSo([Bind("HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,SoBHYT")] BenhNhan model)
        {
            var benhNhan = await LayHoSoCuaToiAsync();
            if (benhNhan == null) return NotFound();

            model.MaBenhNhan = benhNhan.MaBenhNhan;
            await KiemTraNghiepVuAsync(model);
            if (!ModelState.IsValid)
            {
                model.NgayDangKy = benhNhan.NgayDangKy;
                model.TaiKhoan = benhNhan.TaiKhoan;
                ViewBag.SoLichKham = await _context.LichKhams.CountAsync(l => l.MaBenhNhan == benhNhan.MaBenhNhan);
                ViewBag.SoDaKham = await _context.LichKhams.CountAsync(l => l.MaBenhNhan == benhNhan.MaBenhNhan && l.TrangThai == TrangThaiLichKham.DaKham);
                return View(model);
            }

            CapNhatThongTin(benhNhan, model);
            // Đồng bộ họ tên, email sang tài khoản
            if (benhNhan.TaiKhoan != null)
            {
                benhNhan.TaiKhoan.HoTen = benhNhan.HoTen;
                if (!string.IsNullOrEmpty(benhNhan.Email)) benhNhan.TaiKhoan.Email = benhNhan.Email;
            }
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString(SessionKeys.HoTen, benhNhan.HoTen);

            TempData["ThanhCong"] = "Đã lưu hồ sơ cá nhân.";
            return RedirectToAction(nameof(HoSo));
        }

        private static void CapNhatThongTin(BenhNhan dich, BenhNhan nguon)
        {
            dich.HoTen = nguon.HoTen.Trim();
            dich.NgaySinh = nguon.NgaySinh;
            dich.GioiTinh = nguon.GioiTinh;
            dich.SoDienThoai = nguon.SoDienThoai;
            dich.Email = nguon.Email;
            dich.DiaChi = nguon.DiaChi;
            dich.SoBHYT = string.IsNullOrWhiteSpace(nguon.SoBHYT) ? null : nguon.SoBHYT.Trim().ToUpper();
        }

        private async Task KiemTraNghiepVuAsync(BenhNhan b)
        {
            if (b.NgaySinh.Date > DateTime.Today || b.NgaySinh.Year < 1900)
                ModelState.AddModelError(nameof(BenhNhan.NgaySinh), "Ngày sinh không hợp lệ.");

            if (!string.IsNullOrWhiteSpace(b.SoBHYT))
            {
                var bhyt = b.SoBHYT.Trim().ToUpper();
                if (await _context.BenhNhans.AnyAsync(x => x.SoBHYT == bhyt && x.MaBenhNhan != b.MaBenhNhan))
                    ModelState.AddModelError(nameof(BenhNhan.SoBHYT), "Số BHYT đã được dùng cho bệnh nhân khác.");
            }
        }
    }
}
