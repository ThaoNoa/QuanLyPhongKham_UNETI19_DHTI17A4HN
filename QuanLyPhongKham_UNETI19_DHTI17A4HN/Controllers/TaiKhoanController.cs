// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Đăng nhập, đăng xuất, đăng ký (bệnh nhân), đổi mật khẩu, lưu Session;
// quản lý tài khoản (Admin): tìm kiếm, lọc, sắp xếp, phân trang, thêm, sửa, xem, xóa, khóa/mở khóa.
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
    public class TaiKhoanController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly PhongKhamDbContext _context;

        public TaiKhoanController(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ======================= ĐĂNG NHẬP / ĐĂNG XUẤT =======================

        // GET: /TaiKhoan/DangNhap
        public IActionResult DangNhap(string? returnUrl = null)
        {
            if (HttpContext.Session.DaDangNhap())
                return ChuyenTrangTheoVaiTro(HttpContext.Session.LayVaiTro(), returnUrl);
            return View(new DangNhapViewModel { ReturnUrl = returnUrl });
        }

        // POST: /TaiKhoan/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap.Trim());

            // Tài khoản không tồn tại
            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại.");
                return View(model);
            }

            // Sai mật khẩu
            if (!MatKhauHelper.KiemTra(taiKhoan.MatKhau, model.MatKhau))
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
                return View(model);
            }

            // Tài khoản bị khóa không được đăng nhập
            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            // Lưu Mã tài khoản, Họ tên, Vai trò vào Session
            HttpContext.Session.SetInt32(SessionKeys.MaTaiKhoan, taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString(SessionKeys.HoTen, taiKhoan.HoTen);
            HttpContext.Session.SetInt32(SessionKeys.VaiTro, (int)taiKhoan.VaiTro);

            TempData["ThanhCong"] = $"Xin chào {taiKhoan.HoTen}! Đăng nhập thành công.";
            return ChuyenTrangTheoVaiTro(taiKhoan.VaiTro, model.ReturnUrl);
        }

        // POST: /TaiKhoan/DangXuat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            TempData["ThanhCong"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }

        private IActionResult ChuyenTrangTheoVaiTro(VaiTro? vaiTro, string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return vaiTro switch
            {
                VaiTro.Admin or VaiTro.NhanVien => RedirectToAction("Dashboard", "ThongKe"),
                VaiTro.BacSi => RedirectToAction("LichCuaBacSi", "TiepNhan"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult TuChoiTruyCap()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // ======================= ĐĂNG KÝ (BỆNH NHÂN) =======================

        public IActionResult DangKy()
        {
            if (HttpContext.Session.DaDangNhap()) return RedirectToAction("Index", "Home");
            return View(new DangKyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            // Tên đăng nhập không trùng
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");

            if (model.NgaySinh.HasValue && model.NgaySinh.Value.Date > DateTime.Today)
                ModelState.AddModelError(nameof(model.NgaySinh), "Ngày sinh không hợp lệ.");

            if (!ModelState.IsValid) return View(model);

            // Tạo tài khoản + hồ sơ bệnh nhân (TaiKhoan 1 - 0..1 BenhNhan)
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap.Trim(),
                MatKhau = MatKhauHelper.MaHoa(model.MatKhau),
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                VaiTro = VaiTro.BenhNhan,
                TrangThai = true,
                BenhNhan = new BenhNhan
                {
                    HoTen = model.HoTen.Trim(),
                    Email = model.Email.Trim(),
                    SoDienThoai = model.SoDienThoai,
                    NgaySinh = model.NgaySinh!.Value,
                    GioiTinh = model.GioiTinh,
                    NgayDangKy = DateTime.Today,
                    TrangThai = true
                }
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Đăng ký thành công! Vui lòng đăng nhập để đặt lịch khám.";
            return RedirectToAction(nameof(DangNhap));
        }

        // ======================= ĐỔI MẬT KHẨU =======================

        [PhanQuyen]
        public IActionResult DoiMatKhau()
        {
            return View(new DoiMatKhauViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var taiKhoan = await _context.TaiKhoans.FindAsync(HttpContext.Session.LayMaTaiKhoan());
            if (taiKhoan == null) return NotFound();

            if (!MatKhauHelper.KiemTra(taiKhoan.MatKhau, model.MatKhauCu))
            {
                ModelState.AddModelError(nameof(model.MatKhauCu), "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            taiKhoan.MatKhau = MatKhauHelper.MaHoa(model.MatKhauMoi);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(DoiMatKhau));
        }

        // ======================= QUẢN LÝ TÀI KHOẢN (ADMIN) =======================

        // GET: /TaiKhoan?tuKhoa=..&vaiTro=..&trangThai=..&sapXep=..&trang=..
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Index(string? tuKhoa, VaiTro? vaiTro, bool? trangThai, string? sapXep, int trang = 1)
        {
            var query = _context.TaiKhoans.AsNoTracking().AsQueryable();

            // Tìm kiếm theo tên đăng nhập, họ tên, email
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(t => t.TenDangNhap.Contains(tk) || t.HoTen.Contains(tk) || t.Email.Contains(tk));
            }
            // Lọc theo vai trò, trạng thái
            if (vaiTro.HasValue) query = query.Where(t => t.VaiTro == vaiTro.Value);
            if (trangThai.HasValue) query = query.Where(t => t.TrangThai == trangThai.Value);

            // Sắp xếp
            query = sapXep switch
            {
                "ten_desc" => query.OrderByDescending(t => t.HoTen),
                "ten" => query.OrderBy(t => t.HoTen),
                "ngay_desc" => query.OrderByDescending(t => t.NgayTao),
                "vaitro" => query.OrderBy(t => t.VaiTro).ThenBy(t => t.HoTen),
                _ => query.OrderBy(t => t.MaTaiKhoan)
            };

            var vm = new TaiKhoanIndexViewModel
            {
                TuKhoa = tuKhoa,
                VaiTro = vaiTro,
                TrangThai = trangThai,
                SapXep = sapXep,
                DanhSach = await PaginatedList<TaiKhoan>.CreateAsync(query, trang, KichThuocTrang)
            };
            return View(vm);
        }

        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Details(int id)
        {
            var taiKhoan = await _context.TaiKhoans.AsNoTracking()
                .Include(t => t.BenhNhan)
                .Include(t => t.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        [PhanQuyen(VaiTro.Admin)]
        public IActionResult Create()
        {
            return View(new TaiKhoanFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Create(TaiKhoanFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MatKhau))
                ModelState.AddModelError(nameof(model.MatKhau), "Vui lòng nhập mật khẩu.");
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap.Trim(),
                MatKhau = MatKhauHelper.MaHoa(model.MatKhau!),
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                VaiTro = model.VaiTro,
                TrangThai = model.TrangThai
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã tạo tài khoản \"{taiKhoan.TenDangNhap}\".";
            return RedirectToAction(nameof(Index));
        }

        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var t = await _context.TaiKhoans.FindAsync(id);
            if (t == null) return NotFound();
            return View(new TaiKhoanFormViewModel
            {
                MaTaiKhoan = t.MaTaiKhoan,
                TenDangNhap = t.TenDangNhap,
                HoTen = t.HoTen,
                Email = t.Email,
                VaiTro = t.VaiTro,
                TrangThai = t.TrangThai
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Edit(int id, TaiKhoanFormViewModel model)
        {
            if (id != model.MaTaiKhoan) return NotFound();
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap && t.MaTaiKhoan != id))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");

            // Không cho Admin tự khóa hoặc tự hạ quyền chính mình
            bool laChinhMinh = id == HttpContext.Session.LayMaTaiKhoan();
            if (laChinhMinh && (!model.TrangThai || model.VaiTro != VaiTro.Admin))
                ModelState.AddModelError(string.Empty, "Bạn không thể tự khóa hoặc thay đổi vai trò của chính mình.");

            if (!ModelState.IsValid) return View(model);

            taiKhoan.TenDangNhap = model.TenDangNhap.Trim();
            taiKhoan.HoTen = model.HoTen.Trim();
            taiKhoan.Email = model.Email.Trim();
            taiKhoan.VaiTro = model.VaiTro;
            taiKhoan.TrangThai = model.TrangThai;
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
                taiKhoan.MatKhau = MatKhauHelper.MaHoa(model.MatKhau);

            await _context.SaveChangesAsync();
            if (laChinhMinh) HttpContext.Session.SetString(SessionKeys.HoTen, taiKhoan.HoTen);
            TempData["ThanhCong"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /TaiKhoan/KhoaMo/5 - khóa hoặc mở khóa nhanh
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> KhoaMo(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();
            if (id == HttpContext.Session.LayMaTaiKhoan())
            {
                TempData["Loi"] = "Bạn không thể tự khóa tài khoản của mình.";
                return RedirectToAction(nameof(Index));
            }
            taiKhoan.TrangThai = !taiKhoan.TrangThai;
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = taiKhoan.TrangThai ? $"Đã mở khóa \"{taiKhoan.TenDangNhap}\"." : $"Đã khóa \"{taiKhoan.TenDangNhap}\".";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.BenhNhan).Include(t => t.BacSi)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            if (id == HttpContext.Session.LayMaTaiKhoan())
            {
                TempData["Loi"] = "Bạn không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            // Hồ sơ bệnh nhân / bác sĩ được giữ lại, chỉ gỡ liên kết tài khoản
            if (taiKhoan.BenhNhan != null) taiKhoan.BenhNhan.MaTaiKhoan = null;
            if (taiKhoan.BacSi != null) taiKhoan.BacSi.MaTaiKhoan = null;

            _context.TaiKhoans.Remove(taiKhoan);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã xóa tài khoản \"{taiKhoan.TenDangNhap}\".";
            return RedirectToAction(nameof(Index));
        }
    }
}
