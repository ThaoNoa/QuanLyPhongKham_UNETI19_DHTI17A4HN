// Họ và tên: Lê Quang Trung
// Mã sinh viên: 23103100270
// Nội dung thực hiện: Quản lý bác sĩ - CRUD, validation, upload ảnh đại diện;
// tìm kiếm theo họ tên/chuyên môn, lọc theo chuyên khoa/trạng thái, sắp xếp theo tên/học vị, phân trang (Skip/Take)
// và giữ điều kiện lọc khi chuyển trang; trang danh sách/chi tiết bác sĩ công khai cho bệnh nhân.
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
        private const long DungLuongAnhToiDa = 2 * 1024 * 1024; // 2MB
        private static readonly string[] DuoiAnhHopLe = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly PhongKhamDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BacSiController(PhongKhamDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public IActionResult Create()
        {
            var vm = new BacSiFormViewModel { BacSi = new BacSi { NgaySinh = new DateTime(1985, 1, 1) } };
            NapDuLieuForm(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Create(BacSiFormViewModel vm)
        {
            await KiemTraNghiepVuAsync(vm);
            if (!ModelState.IsValid)
            {
                NapDuLieuForm(vm);
                return View(vm);
            }

            var bacSi = vm.BacSi;
            bacSi.HoTen = bacSi.HoTen.Trim();
            if (vm.AnhDaiDien != null) bacSi.HinhAnh = await LuuAnhAsync(vm.AnhDaiDien);

            _context.BacSis.Add(bacSi);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã thêm bác sĩ {bacSi.HoTen}.";
            return RedirectToAction(nameof(Index));
        }

        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Edit(int id)
        {
            var bacSi = await _context.BacSis.AsNoTracking().FirstOrDefaultAsync(b => b.MaBacSi == id);
            if (bacSi == null) return NotFound();
            var vm = new BacSiFormViewModel { BacSi = bacSi };
            NapDuLieuForm(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Edit(int id, BacSiFormViewModel vm)
        {
            if (id != vm.BacSi.MaBacSi) return NotFound();
            var bacSi = await _context.BacSis.FindAsync(id);
            if (bacSi == null) return NotFound();

            await KiemTraNghiepVuAsync(vm);
            if (!ModelState.IsValid)
            {
                vm.BacSi.HinhAnh = bacSi.HinhAnh;
                NapDuLieuForm(vm);
                return View(vm);
            }

            // Cập nhật từng trường (không ghi đè dữ liệu không có trên form)
            bacSi.HoTen = vm.BacSi.HoTen.Trim();
            bacSi.MaChuyenKhoa = vm.BacSi.MaChuyenKhoa;
            bacSi.MaTaiKhoan = vm.BacSi.MaTaiKhoan;
            bacSi.NgaySinh = vm.BacSi.NgaySinh;
            bacSi.GioiTinh = vm.BacSi.GioiTinh;
            bacSi.ChuyenMon = vm.BacSi.ChuyenMon;
            bacSi.SoDienThoai = vm.BacSi.SoDienThoai;
            bacSi.Email = vm.BacSi.Email;
            bacSi.HocVi = vm.BacSi.HocVi;
            bacSi.TrangThai = vm.BacSi.TrangThai;
            bacSi.GhiChu = vm.BacSi.GhiChu;
            if (vm.AnhDaiDien != null)
            {
                XoaAnh(bacSi.HinhAnh);
                bacSi.HinhAnh = await LuuAnhAsync(vm.AnhDaiDien);
            }

            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã cập nhật thông tin bác sĩ {bacSi.HoTen}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bacSi = await _context.BacSis.FindAsync(id);
            if (bacSi == null) return NotFound();

            // Bác sĩ đã có lịch khám thì không xóa (giữ lịch sử), chỉ cho tạm ngừng
            if (await _context.LichKhams.AnyAsync(l => l.MaBacSi == id))
            {
                TempData["Loi"] = $"Không thể xóa bác sĩ {bacSi.HoTen} vì đã có lịch khám. Hãy chuyển trạng thái sang Tạm ngừng.";
                return RedirectToAction(nameof(Index));
            }

            XoaAnh(bacSi.HinhAnh);
            _context.BacSis.Remove(bacSi);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã xóa bác sĩ {bacSi.HoTen}.";
            return RedirectToAction(nameof(Index));
        }

        // Validation nghiệp vụ không thể dùng Data Annotation
        private async Task KiemTraNghiepVuAsync(BacSiFormViewModel vm)
        {
            var b = vm.BacSi;

            // Bác sĩ phải thuộc một chuyên khoa hợp lệ
            if (!await _context.ChuyenKhoas.AnyAsync(c => c.MaChuyenKhoa == b.MaChuyenKhoa))
                ModelState.AddModelError("BacSi.MaChuyenKhoa", "Chuyên khoa không hợp lệ.");

            int tuoi = DateTime.Today.Year - b.NgaySinh.Year;
            if (b.NgaySinh > DateTime.Today || tuoi < 22 || tuoi > 80)
                ModelState.AddModelError("BacSi.NgaySinh", "Tuổi bác sĩ phải từ 22 đến 80.");

            // Tài khoản gán cho bác sĩ phải là tài khoản vai trò Bác sĩ và chưa gán cho bác sĩ khác
            if (b.MaTaiKhoan.HasValue)
            {
                var tk = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == b.MaTaiKhoan);
                if (tk == null || tk.VaiTro != VaiTro.BacSi)
                    ModelState.AddModelError("BacSi.MaTaiKhoan", "Tài khoản phải có vai trò Bác sĩ.");
                else if (await _context.BacSis.AnyAsync(x => x.MaTaiKhoan == b.MaTaiKhoan && x.MaBacSi != b.MaBacSi))
                    ModelState.AddModelError("BacSi.MaTaiKhoan", "Tài khoản này đã được gán cho bác sĩ khác.");
            }

            // Kiểm tra file ảnh
            if (vm.AnhDaiDien != null)
            {
                var duoi = Path.GetExtension(vm.AnhDaiDien.FileName).ToLowerInvariant();
                if (!DuoiAnhHopLe.Contains(duoi))
                    ModelState.AddModelError(nameof(vm.AnhDaiDien), "Chỉ chấp nhận ảnh .jpg, .jpeg, .png, .webp.");
                else if (vm.AnhDaiDien.Length > DungLuongAnhToiDa)
                    ModelState.AddModelError(nameof(vm.AnhDaiDien), "Ảnh tối đa 2MB.");
            }
        }

        private void NapDuLieuForm(BacSiFormViewModel vm)
        {
            vm.ChuyenKhoas = TaoSelectChuyenKhoa(vm.BacSi.MaChuyenKhoa);
            // Tài khoản vai trò Bác sĩ chưa gán cho ai (hoặc đang gán cho chính bác sĩ này)
            var taiKhoans = _context.TaiKhoans.AsNoTracking()
                .Where(t => t.VaiTro == VaiTro.BacSi && (t.BacSi == null || t.BacSi.MaBacSi == vm.BacSi.MaBacSi))
                .OrderBy(t => t.TenDangNhap)
                .Select(t => new { t.MaTaiKhoan, Ten = t.TenDangNhap + " - " + t.HoTen })
                .ToList();
            vm.TaiKhoans = new SelectList(taiKhoans, "MaTaiKhoan", "Ten", vm.BacSi.MaTaiKhoan);
        }

        private async Task<string> LuuAnhAsync(IFormFile file)
        {
            var thuMuc = Path.Combine(_env.WebRootPath, "uploads", "bacsi");
            Directory.CreateDirectory(thuMuc);
            var tenFile = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            await using var stream = new FileStream(Path.Combine(thuMuc, tenFile), FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/bacsi/{tenFile}";
        }

        private void XoaAnh(string? duongDan)
        {
            if (string.IsNullOrEmpty(duongDan) || !duongDan.StartsWith("/uploads/")) return;
            var file = Path.Combine(_env.WebRootPath, duongDan.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
        }

        // ============================ CÔNG KHAI ============================

        // GET: /BacSi/DanhSach - bệnh nhân tìm bác sĩ (chỉ bác sĩ đang hoạt động)
        public async Task<IActionResult> DanhSach(string? tuKhoa, int? maChuyenKhoa, string? sapXep, int trang = 1)
        {
            var query = TaoTruyVan(tuKhoa, maChuyenKhoa, true, sapXep)
                .Where(b => b.ChuyenKhoa!.TrangThai);
            var vm = new BacSiIndexViewModel
            {
                TuKhoa = tuKhoa,
                MaChuyenKhoa = maChuyenKhoa,
                SapXep = sapXep,
                DanhSach = await PaginatedList<BacSi>.CreateAsync(query, trang, KichThuocTrang),
                ChuyenKhoas = TaoSelectChuyenKhoa(maChuyenKhoa, chiHoatDong: true)
            };
            vm.SoLuotKham = await DemLuotKhamAsync(vm.DanhSach.Select(b => b.MaBacSi));
            return View(vm);
        }

        // GET: /BacSi/ChiTiet/5 - hồ sơ bác sĩ công khai
        public async Task<IActionResult> ChiTiet(int id)
        {
            var bacSi = await _context.BacSis.AsNoTracking()
                .Include(b => b.ChuyenKhoa)
                .FirstOrDefaultAsync(b => b.MaBacSi == id && b.TrangThai);
            if (bacSi == null) return NotFound();

            var vm = new BacSiChiTietViewModel
            {
                BacSi = bacSi,
                SoLuotKham = await _context.LichKhams.CountAsync(l => l.MaBacSi == id && l.TrangThai == TrangThaiLichKham.DaKham),
                BacSiCungKhoa = await _context.BacSis.AsNoTracking().Include(b => b.ChuyenKhoa)
                    .Where(b => b.MaChuyenKhoa == bacSi.MaChuyenKhoa && b.MaBacSi != id && b.TrangThai)
                    .OrderByDescending(b => b.HocVi).Take(3).ToListAsync()
            };
            return View(vm);
        }

        // GET: /BacSi/LayTheoChuyenKhoa?maChuyenKhoa=1 - trả JSON cho ô chọn bác sĩ ở trang đặt lịch
        public async Task<IActionResult> LayTheoChuyenKhoa(int? maChuyenKhoa)
        {
            var ds = await _context.BacSis.AsNoTracking()
                .Where(b => b.TrangThai && b.ChuyenKhoa!.TrangThai && (!maChuyenKhoa.HasValue || b.MaChuyenKhoa == maChuyenKhoa))
                .OrderBy(b => b.HoTen)
                .Select(b => new { b.MaBacSi, b.HoTen, b.HocVi, b.HinhAnh, b.ChuyenMon, TenChuyenKhoa = b.ChuyenKhoa!.TenChuyenKhoa })
                .ToListAsync();
            return Json(ds.Select(b => new
            {
                maBacSi = b.MaBacSi,
                hoTen = $"{b.HocVi.TenVietTat()} {b.HoTen}",
                chuyenKhoa = b.TenChuyenKhoa,
                chuyenMon = b.ChuyenMon,
                hinhAnh = b.HinhAnh,
                chuCai = GiaoDienHelper.ChuCaiDau(b.HoTen)
            }));
        }
    }
}
