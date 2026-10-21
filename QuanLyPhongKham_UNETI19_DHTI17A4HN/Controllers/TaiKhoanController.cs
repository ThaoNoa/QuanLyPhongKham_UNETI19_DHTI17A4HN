// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Đăng nhập (kiểm tra tài khoản không tồn tại, sai mật khẩu, bị khóa), lưu Session,
// chuyển trang theo vai trò, đăng xuất; trang từ chối truy cập.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Controllers
{
    public class TaiKhoanController : Controller
    {
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
                VaiTro.Admin or VaiTro.NhanVien => RedirectToAction("Index", "TiepNhan"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult TuChoiTruyCap()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}
