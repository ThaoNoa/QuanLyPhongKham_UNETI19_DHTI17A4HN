// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Đặt lịch khám (bệnh nhân tự đặt / nhân viên đặt hộ) - kiểm tra ngày quá khứ, bác sĩ hoạt động,
// trùng lịch bác sĩ và bệnh nhân; lịch mới ở trạng thái Chờ xác nhận. Theo dõi lịch của tôi, xem chi tiết,
// bệnh nhân tự hủy lịch đang chờ xác nhận; API lấy khung giờ đã được đặt.
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
    public class LichKhamController : Controller
    {
        private const int KichThuocTrang = 6;
        private readonly PhongKhamDbContext _context;
        private readonly LichKhamService _lichKhamService;

        public LichKhamController(PhongKhamDbContext context, LichKhamService lichKhamService)
        {
            _context = context;
            _lichKhamService = lichKhamService;
        }

        // Lấy mã bệnh nhân của tài khoản đang đăng nhập (null nếu không phải bệnh nhân)
        private async Task<int?> LayMaBenhNhanCuaToiAsync()
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            return await _context.BenhNhans
                .Where(b => b.MaTaiKhoan == maTaiKhoan)
                .Select(b => (int?)b.MaBenhNhan)
                .FirstOrDefaultAsync();
        }

        // ======================= ĐẶT LỊCH =======================

        // GET: /LichKham/DatLich?maChuyenKhoa=1&maBacSi=2
        [PhanQuyen(VaiTro.BenhNhan, VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> DatLich(int? maChuyenKhoa, int? maBacSi, int? maBenhNhan)
        {
            var vm = new DatLichViewModel
            {
                MaChuyenKhoa = maChuyenKhoa,
                MaBacSi = maBacSi,
                MaBenhNhan = maBenhNhan,
                NgayKham = DateTime.Today.AddDays(DateTime.Today.DayOfWeek == DayOfWeek.Saturday ? 2 : 1)
            };
            if (maBacSi.HasValue)
            {
                var bs = await _context.BacSis.AsNoTracking().FirstOrDefaultAsync(b => b.MaBacSi == maBacSi);
                if (bs != null) vm.MaChuyenKhoa = bs.MaChuyenKhoa;
            }
            await NapDuLieuDatLichAsync(vm);
            return View(vm);
        }

        // POST: /LichKham/DatLich
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.BenhNhan, VaiTro.Admin, VaiTro.NhanVien)]
        public async Task<IActionResult> DatLich(DatLichViewModel vm)
        {
            bool laNhanVien = HttpContext.Session.LaQuanTri();

            // Xác định bệnh nhân: bệnh nhân tự đặt -> lấy từ Session; nhân viên -> chọn trên form
            int? maBenhNhan = laNhanVien ? vm.MaBenhNhan : await LayMaBenhNhanCuaToiAsync();
            if (maBenhNhan == null)
                ModelState.AddModelError(nameof(vm.MaBenhNhan), laNhanVien ? "Vui lòng chọn bệnh nhân." : "Không tìm thấy hồ sơ bệnh nhân của bạn.");
            else if (!await _context.BenhNhans.AnyAsync(b => b.MaBenhNhan == maBenhNhan && b.TrangThai))
                ModelState.AddModelError(nameof(vm.MaBenhNhan), "Bệnh nhân không tồn tại hoặc đã ngừng theo dõi.");

            if (ModelState.IsValid)
            {
                var lich = new LichKham
                {
                    MaBenhNhan = maBenhNhan!.Value,
                    MaBacSi = vm.MaBacSi!.Value,
                    NgayKham = vm.NgayKham!.Value.Date,
                    GioBatDau = vm.GioBatDau!.Value,
                    GioKetThuc = vm.GioKetThuc!.Value,
                    LyDoKham = vm.LyDoKham.Trim(),
                    GhiChu = vm.GhiChu,
                    TrangThai = TrangThaiLichKham.ChoXacNhan,   // Lịch mới luôn ở trạng thái Chờ xác nhận
                    NgayTao = DateTime.Now
                };

                // Kiểm tra nghiệp vụ: quá khứ, giờ kết thúc > bắt đầu, bác sĩ hoạt động, trùng lịch
                var loi = await _lichKhamService.KiemTraLichAsync(lich);
                foreach (var l in loi) ModelState.AddModelError(string.Empty, l.Value);

                if (ModelState.IsValid)
                {
                    _context.LichKhams.Add(lich);
                    await _context.SaveChangesAsync();
                    TempData["ThanhCong"] = "Đặt lịch thành công! Lịch đang chờ phòng khám xác nhận.";
                    return laNhanVien
                        ? RedirectToAction("ChiTiet", "TiepNhan", new { id = lich.MaLichKham })
                        : RedirectToAction(nameof(ChiTiet), new { id = lich.MaLichKham });
                }
            }

            await NapDuLieuDatLichAsync(vm);
            return View(vm);
        }

        private async Task NapDuLieuDatLichAsync(DatLichViewModel vm)
        {
            vm.LaNhanVien = HttpContext.Session.LaQuanTri();
            vm.ChuyenKhoas = new SelectList(
                await _context.ChuyenKhoas.AsNoTracking().Where(c => c.TrangThai).OrderBy(c => c.TenChuyenKhoa).ToListAsync(),
                nameof(ChuyenKhoa.MaChuyenKhoa), nameof(ChuyenKhoa.TenChuyenKhoa), vm.MaChuyenKhoa);

            // Chỉ bác sĩ đang hoạt động mới được nhận lịch
            var bacSis = await _context.BacSis.AsNoTracking()
                .Where(b => b.TrangThai && b.ChuyenKhoa!.TrangThai && (!vm.MaChuyenKhoa.HasValue || b.MaChuyenKhoa == vm.MaChuyenKhoa))
                .OrderBy(b => b.HoTen).ToListAsync();
            vm.BacSis = new SelectList(bacSis.Select(b => new { b.MaBacSi, Ten = $"{b.HocVi.TenVietTat()} {b.HoTen}" }), "MaBacSi", "Ten", vm.MaBacSi);

            if (vm.MaBacSi.HasValue)
                vm.BacSiDaChon = await _context.BacSis.AsNoTracking().Include(b => b.ChuyenKhoa).FirstOrDefaultAsync(b => b.MaBacSi == vm.MaBacSi);

            if (vm.LaNhanVien)
            {
                var benhNhans = await _context.BenhNhans.AsNoTracking().Where(b => b.TrangThai).OrderBy(b => b.HoTen)
                    .Select(b => new { b.MaBenhNhan, Ten = b.HoTen + " - " + b.SoDienThoai }).ToListAsync();
                vm.BenhNhans = new SelectList(benhNhans, "MaBenhNhan", "Ten", vm.MaBenhNhan);
            }
        }

        // GET: /LichKham/KhungGioDaDat?maBacSi=1&ngay=2026-10-20 - JSON khung giờ đã có lịch của bác sĩ
        [PhanQuyen]
        public async Task<IActionResult> KhungGioDaDat(int maBacSi, DateTime ngay)
        {
            var ds = await _lichKhamService.LayKhungGioDaDatAsync(maBacSi, ngay);
            return Json(ds.Select(k => new { batDau = k.BatDau.ToString(@"hh\:mm"), ketThuc = k.KetThuc.ToString(@"hh\:mm") }));
        }

        // ======================= THEO DÕI LỊCH CỦA TÔI =======================

        // GET: /LichKham/LichCuaToi?trangThai=ChoXacNhan&trang=1
        [PhanQuyen(VaiTro.BenhNhan)]
        public async Task<IActionResult> LichCuaToi(TrangThaiLichKham? trangThai, int trang = 1)
        {
            var maBenhNhan = await LayMaBenhNhanCuaToiAsync();
            if (maBenhNhan == null) return RedirectToAction("HoSo", "BenhNhan");

            // Chỉ lấy lịch của chính bệnh nhân đang đăng nhập
            var lichCuaToi = _context.LichKhams.AsNoTracking().Where(l => l.MaBenhNhan == maBenhNhan);

            // Đếm số lịch theo từng trạng thái (GroupBy + Count)
            var thongKe = await lichCuaToi.GroupBy(l => l.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToDictionaryAsync(x => x.TrangThai, x => x.SoLuong);

            var query = lichCuaToi
                .Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                .Include(l => l.KetQuaKham)
                .AsQueryable();

            if (trangThai.HasValue) query = query.Where(l => l.TrangThai == trangThai.Value);
            query = query.OrderByDescending(l => l.NgayKham).ThenByDescending(l => l.GioBatDau);

            return View(new LichCuaToiViewModel
            {
                TrangThai = trangThai,
                ThongKe = thongKe,
                DanhSach = await PaginatedList<LichKham>.CreateAsync(query, trang, KichThuocTrang)
            });
        }

        // GET: /LichKham/ChiTiet/5 - bệnh nhân chỉ xem được lịch của mình
        [PhanQuyen(VaiTro.BenhNhan)]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var maBenhNhan = await LayMaBenhNhanCuaToiAsync();
            var lich = await _context.LichKhams.AsNoTracking()
                .Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                .Include(l => l.BenhNhan)
                .Include(l => l.KetQuaKham)
                .FirstOrDefaultAsync(l => l.MaLichKham == id && l.MaBenhNhan == maBenhNhan);

            // Đổi mã trên URL sang lịch của người khác -> không tìm thấy
            if (lich == null) return NotFound();
            return View(lich);
        }

        // POST: /LichKham/HuyLich/5 - bệnh nhân hủy lịch của mình khi còn Chờ xác nhận
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.BenhNhan)]
        public async Task<IActionResult> HuyLich(int id, string? lyDo)
        {
            var maBenhNhan = await LayMaBenhNhanCuaToiAsync();
            var lich = await _context.LichKhams.FirstOrDefaultAsync(l => l.MaLichKham == id && l.MaBenhNhan == maBenhNhan);
            if (lich == null) return NotFound();

            if (!LichKhamService.ChoPhepChuyen(lich.TrangThai, TrangThaiLichKham.DaHuy))
            {
                TempData["Loi"] = "Chỉ có thể hủy lịch đang ở trạng thái Chờ xác nhận. Vui lòng liên hệ phòng khám.";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            lich.TrangThai = TrangThaiLichKham.DaHuy;
            lich.GhiChu = "Bệnh nhân tự hủy" + (string.IsNullOrWhiteSpace(lyDo) ? "" : ": " + lyDo.Trim());
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã hủy lịch khám.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }
    }
}
