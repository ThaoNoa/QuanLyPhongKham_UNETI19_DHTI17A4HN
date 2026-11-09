// Họ và tên: Đỗ Huy Tùng
// Mã sinh viên: 23103100296
// Nội dung thực hiện: Tiếp nhận và xử lý lịch khám - danh sách lịch cần xử lý (tìm theo bệnh nhân/bác sĩ,
// lọc chuyên khoa/trạng thái/ngày, sắp xếp theo ngày khám, phân trang); xác nhận / hủy lịch (kiểm tra lại trùng lịch);
// quản lý luồng trạng thái Chờ xác nhận -> Đã xác nhận -> Đã khám, Chờ xác nhận -> Đã hủy; lịch làm việc của bác sĩ.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Filters;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Services;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Controllers
{
    public class TiepNhanController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly PhongKhamDbContext _context;
        private readonly LichKhamService _lichKhamService;

        public TiepNhanController(PhongKhamDbContext context, LichKhamService lichKhamService)
        {
            _context = context;
            _lichKhamService = lichKhamService;
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

        // ======================= XỬ LÝ TRẠNG THÁI =======================

        // POST: /TiepNhan/XacNhan/5 - Chờ xác nhận -> Đã xác nhận (bệnh nhân không được tự xác nhận)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> XacNhan(int id, string? returnUrl)
        {
            var lich = await _context.LichKhams.FindAsync(id);
            if (lich == null) return NotFound();

            if (!LichKhamService.ChoPhepChuyen(lich.TrangThai, TrangThaiLichKham.DaXacNhan))
            {
                TempData["Loi"] = $"Chỉ lịch Chờ xác nhận mới được xác nhận (lịch hiện ở trạng thái {lich.TrangThai.TenHienThi()}).";
                return QuayLai(returnUrl, id);
            }
            if (lich.NgayKham.Date.Add(lich.GioBatDau) < DateTime.Now)
            {
                TempData["Loi"] = "Lịch đã quá giờ hẹn, không thể xác nhận. Vui lòng hủy lịch.";
                return QuayLai(returnUrl, id);
            }

            // Kiểm tra lại điều kiện trùng lịch tại thời điểm xác nhận
            var loi = await _lichKhamService.KiemTraKhiXacNhanAsync(lich);
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return QuayLai(returnUrl, id);
            }

            lich.TrangThai = TrangThaiLichKham.DaXacNhan;
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã xác nhận lịch #LK{id:00000}.";
            return QuayLai(returnUrl, id);
        }

        // POST: /TiepNhan/Huy/5 - Chờ xác nhận -> Đã hủy
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> Huy(int id, string? lyDo, string? returnUrl)
        {
            var lich = await _context.LichKhams.FindAsync(id);
            if (lich == null) return NotFound();

            if (!LichKhamService.ChoPhepChuyen(lich.TrangThai, TrangThaiLichKham.DaHuy))
            {
                TempData["Loi"] = $"Chỉ lịch Chờ xác nhận mới được hủy (lịch hiện ở trạng thái {lich.TrangThai.TenHienThi()}).";
                return QuayLai(returnUrl, id);
            }

            lich.TrangThai = TrangThaiLichKham.DaHuy;
            lich.GhiChu = "Phòng khám hủy" + (string.IsNullOrWhiteSpace(lyDo) ? "" : ": " + lyDo.Trim());
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã hủy lịch #LK{id:00000}.";
            return QuayLai(returnUrl, id);
        }

        // POST: /TiepNhan/DaKham/5 - Đã xác nhận -> Đã khám (không cho chuyển thẳng từ Chờ xác nhận)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien, VaiTro.BacSi)]
        public async Task<IActionResult> DaKham(int id, string? returnUrl)
        {
            var lich = await _context.LichKhams.Include(l => l.BacSi).FirstOrDefaultAsync(l => l.MaLichKham == id);
            if (lich == null) return NotFound();

            if (HttpContext.Session.LayVaiTro() == VaiTro.BacSi && lich.BacSi?.MaTaiKhoan != HttpContext.Session.LayMaTaiKhoan())
                return RedirectToAction("TuChoiTruyCap", "TaiKhoan");

            if (!LichKhamService.ChoPhepChuyen(lich.TrangThai, TrangThaiLichKham.DaKham))
            {
                TempData["Loi"] = lich.TrangThai == TrangThaiLichKham.ChoXacNhan
                    ? "Lịch chưa được xác nhận, không thể chuyển trực tiếp sang Đã khám."
                    : $"Không thể chuyển lịch từ {lich.TrangThai.TenHienThi()} sang Đã khám.";
                return QuayLai(returnUrl, id);
            }
            if (lich.NgayKham.Date > DateTime.Today)
            {
                TempData["Loi"] = "Chưa đến ngày khám, không thể ghi nhận Đã khám.";
                return QuayLai(returnUrl, id);
            }

            lich.TrangThai = TrangThaiLichKham.DaKham;
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã ghi nhận bệnh nhân đã khám. Hãy cập nhật kết quả khám.";
            return RedirectToAction("Create", "KetQuaKham", new { maLichKham = id });
        }

        private IActionResult QuayLai(string? returnUrl, int id)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        // ======================= LỊCH LÀM VIỆC CỦA BÁC SĨ =======================

        // GET: /TiepNhan/LichCuaBacSi?ngay=2026-10-20
        [PhanQuyen(VaiTro.BacSi)]
        public async Task<IActionResult> LichCuaBacSi(DateTime? ngay)
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            var bacSi = await _context.BacSis.AsNoTracking().Include(b => b.ChuyenKhoa)
                .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);
            if (bacSi == null)
            {
                TempData["Loi"] = "Tài khoản chưa được gán hồ sơ bác sĩ. Vui lòng liên hệ quản trị viên.";
                return RedirectToAction("Index", "Home");
            }

            var ngayXem = (ngay ?? DateTime.Today).Date;
            var vm = new LichBacSiViewModel
            {
                BacSi = bacSi,
                Ngay = ngayXem,
                LichTrongNgay = await _context.LichKhams.AsNoTracking()
                    .Include(l => l.BenhNhan).Include(l => l.KetQuaKham)
                    .Where(l => l.MaBacSi == bacSi.MaBacSi && l.NgayKham == ngayXem && l.TrangThai != TrangThaiLichKham.DaHuy)
                    .OrderBy(l => l.GioBatDau).ToListAsync(),
                LichSapToi = await _context.LichKhams.AsNoTracking()
                    .Include(l => l.BenhNhan)
                    .Where(l => l.MaBacSi == bacSi.MaBacSi && l.NgayKham > ngayXem && l.NgayKham <= ngayXem.AddDays(7)
                             && (l.TrangThai == TrangThaiLichKham.DaXacNhan || l.TrangThai == TrangThaiLichKham.ChoXacNhan))
                    .OrderBy(l => l.NgayKham).ThenBy(l => l.GioBatDau).ToListAsync(),
                SoChuaNhapKetQua = await _context.LichKhams.CountAsync(l => l.MaBacSi == bacSi.MaBacSi && l.TrangThai == TrangThaiLichKham.DaKham && l.KetQuaKham == null)
            };
            return View(vm);
        }
    }
}
