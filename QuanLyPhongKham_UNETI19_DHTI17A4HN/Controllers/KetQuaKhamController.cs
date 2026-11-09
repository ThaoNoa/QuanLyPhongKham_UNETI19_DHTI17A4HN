// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Kết quả khám - danh sách (tìm kiếm, lọc, sắp xếp, phân trang), nhập/sửa kết quả kèm dịch vụ sử dụng,
// xem/in kết quả; chỉ lịch Đã khám mới được cập nhật kết quả; bác sĩ chỉ thao tác lịch của mình,
// bệnh nhân chỉ xem kết quả của chính mình.
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
    public class KetQuaKhamController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly PhongKhamDbContext _context;

        public KetQuaKhamController(PhongKhamDbContext context)
        {
            _context = context;
        }

        private bool LaBacSi => HttpContext.Session.LayVaiTro() == VaiTro.BacSi;

        // Mã bác sĩ của tài khoản đang đăng nhập (nếu là bác sĩ)
        private async Task<int?> LayMaBacSiCuaToiAsync()
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            return await _context.BacSis.Where(b => b.MaTaiKhoan == maTaiKhoan).Select(b => (int?)b.MaBacSi).FirstOrDefaultAsync();
        }

        // ======================= DANH SÁCH =======================

        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien, VaiTro.BacSi)]
        public async Task<IActionResult> Index(string? tuKhoa, TrangThaiKetQua? trangThai, string? sapXep, int trang = 1)
        {
            IQueryable<KetQuaKham> query = _context.KetQuaKhams.AsNoTracking();
            IQueryable<LichKham> lichChoNhap = _context.LichKhams.AsNoTracking()
                .Where(l => l.TrangThai == TrangThaiLichKham.DaKham && l.KetQuaKham == null);

            // Bác sĩ chỉ thấy kết quả của bệnh nhân mình khám
            if (LaBacSi)
            {
                var maBacSi = await LayMaBacSiCuaToiAsync();
                query = query.Where(k => k.LichKham!.MaBacSi == maBacSi);
                lichChoNhap = lichChoNhap.Where(l => l.MaBacSi == maBacSi);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(k => k.LichKham!.BenhNhan!.HoTen.Contains(tk) || k.LichKham.BacSi!.HoTen.Contains(tk) || k.ChanDoan.Contains(tk));
            }
            if (trangThai.HasValue) query = query.Where(k => k.TrangThai == trangThai.Value);

            query = sapXep switch
            {
                "ngay" => query.OrderBy(k => k.LichKham!.NgayKham),
                "capnhat_desc" => query.OrderByDescending(k => k.NgayCapNhat),
                _ => query.OrderByDescending(k => k.LichKham!.NgayKham).ThenByDescending(k => k.MaKetQua)
            };

            query = query.Include(k => k.LichKham).ThenInclude(l => l!.BenhNhan)
                         .Include(k => k.LichKham).ThenInclude(l => l!.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                         .Include(k => k.ChiTietDichVus);

            return View(new KetQuaKhamIndexViewModel
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                SapXep = sapXep,
                DanhSach = await PaginatedList<KetQuaKham>.CreateAsync(query, trang, KichThuocTrang),
                LichChoNhapKetQua = await lichChoNhap.Include(l => l.BenhNhan).Include(l => l.BacSi)
                    .OrderBy(l => l.NgayKham).Take(10).ToListAsync()
            });
        }

        // ======================= NHẬP KẾT QUẢ =======================

        /// <summary>
        /// Kiểm tra lịch có được phép cập nhật kết quả: phải tồn tại, ở trạng thái Đã khám,
        /// và nếu là bác sĩ thì phải là lịch của mình. Trả về thông báo lỗi hoặc null.
        /// </summary>
        private async Task<string?> KiemTraLichHopLeAsync(LichKham? lich)
        {
            if (lich == null) return "Không tìm thấy lịch khám.";
            if (lich.TrangThai != TrangThaiLichKham.DaKham)
                return $"Lịch đang ở trạng thái \"{lich.TrangThai.TenHienThi()}\". Chỉ lịch Đã khám mới được cập nhật kết quả.";
            if (LaBacSi && lich.MaBacSi != await LayMaBacSiCuaToiAsync())
                return "Bạn chỉ được cập nhật kết quả cho lịch khám của mình.";
            return null;
        }

        private Task<LichKham?> LayLichAsync(int maLichKham) =>
            _context.LichKhams.AsNoTracking()
                .Include(l => l.BenhNhan).Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                .FirstOrDefaultAsync(l => l.MaLichKham == maLichKham);

        private async Task NapDichVuAsync(KetQuaKhamFormViewModel vm, ICollection<ChiTietDichVu>? daChon = null)
        {
            // Dịch vụ đang hoạt động + dịch vụ đã dùng trong kết quả (kể cả khi đã ngừng)
            var maDaChon = daChon?.Select(c => c.MaDichVu).ToList() ?? new List<int>();
            var dsDichVu = await _context.DichVuYTes.AsNoTracking()
                .Where(d => d.TrangThai || maDaChon.Contains(d.MaDichVu))
                .OrderBy(d => d.TenDichVu).ToListAsync();

            // Giữ lại lựa chọn người dùng đã nhập (khi form lỗi) hoặc dữ liệu đã lưu (khi sửa)
            var chonTruoc = vm.DichVus.Where(d => d.DaChon).ToDictionary(d => d.MaDichVu, d => d.SoLuong);
            if (daChon != null && !chonTruoc.Any())
                chonTruoc = daChon.ToDictionary(c => c.MaDichVu, c => c.SoLuong);

            vm.DichVus = dsDichVu.Select(d => new DichVuChonViewModel
            {
                MaDichVu = d.MaDichVu,
                TenDichVu = d.TenDichVu,
                DonViTinh = d.DonViTinh,
                DonGia = daChon?.FirstOrDefault(c => c.MaDichVu == d.MaDichVu)?.DonGia ?? d.DonGia,
                DaChon = chonTruoc.ContainsKey(d.MaDichVu),
                SoLuong = chonTruoc.GetValueOrDefault(d.MaDichVu, 1)
            }).ToList();
        }

        // GET: /KetQuaKham/Create?maLichKham=5
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien, VaiTro.BacSi)]
        public async Task<IActionResult> Create(int maLichKham)
        {
            var lich = await LayLichAsync(maLichKham);
            var loi = await KiemTraLichHopLeAsync(lich);
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction(nameof(Index));
            }
            var daCo = await _context.KetQuaKhams.Where(k => k.MaLichKham == maLichKham).Select(k => (int?)k.MaKetQua).FirstOrDefaultAsync();
            if (daCo != null) return RedirectToAction(nameof(Edit), new { id = daCo });

            var vm = new KetQuaKhamFormViewModel { MaLichKham = maLichKham, LichKham = lich };
            await NapDichVuAsync(vm);
            // Mặc định chọn sẵn dịch vụ "Khám chuyên khoa"
            var khamCk = vm.DichVus.FirstOrDefault(d => d.TenDichVu == "Khám chuyên khoa");
            if (khamCk != null) khamCk.DaChon = true;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien, VaiTro.BacSi)]
        public async Task<IActionResult> Create(KetQuaKhamFormViewModel vm)
        {
            var lich = await LayLichAsync(vm.MaLichKham);
            var loi = await KiemTraLichHopLeAsync(lich);
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction(nameof(Index));
            }
            if (await _context.KetQuaKhams.AnyAsync(k => k.MaLichKham == vm.MaLichKham))
                ModelState.AddModelError(string.Empty, "Lịch khám này đã có kết quả.");

            if (!ModelState.IsValid)
            {
                vm.LichKham = lich;
                await NapDichVuAsync(vm);
                return View(vm);
            }

            var ketQua = new KetQuaKham { MaLichKham = vm.MaLichKham };
            GanDuLieu(ketQua, vm);
            var giaDichVu = await _context.DichVuYTes.ToDictionaryAsync(d => d.MaDichVu, d => d.DonGia);
            foreach (var dv in vm.DichVus.Where(d => d.DaChon && giaDichVu.ContainsKey(d.MaDichVu)))
                ketQua.ChiTietDichVus.Add(new ChiTietDichVu { MaDichVu = dv.MaDichVu, SoLuong = dv.SoLuong, DonGia = giaDichVu[dv.MaDichVu] });

            _context.KetQuaKhams.Add(ketQua);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã lưu kết quả khám.";
            return RedirectToAction(nameof(Details), new { id = ketQua.MaKetQua });
        }

        // GET: /KetQuaKham/Edit/5
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien, VaiTro.BacSi)]
        public async Task<IActionResult> Edit(int id)
        {
            var ketQua = await _context.KetQuaKhams.AsNoTracking().Include(k => k.ChiTietDichVus).FirstOrDefaultAsync(k => k.MaKetQua == id);
            if (ketQua == null) return NotFound();
            var lich = await LayLichAsync(ketQua.MaLichKham);
            var loi = await KiemTraLichHopLeAsync(lich);
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction(nameof(Index));
            }

            var vm = new KetQuaKhamFormViewModel
            {
                MaKetQua = ketQua.MaKetQua,
                MaLichKham = ketQua.MaLichKham,
                ChanDoan = ketQua.ChanDoan,
                KetLuan = ketQua.KetLuan,
                DonThuoc = ketQua.DonThuoc,
                LoiDan = ketQua.LoiDan,
                TrangThai = ketQua.TrangThai,
                GhiChu = ketQua.GhiChu,
                LichKham = lich
            };
            await NapDichVuAsync(vm, ketQua.ChiTietDichVus);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien, VaiTro.BacSi)]
        public async Task<IActionResult> Edit(int id, KetQuaKhamFormViewModel vm)
        {
            if (id != vm.MaKetQua) return NotFound();
            var ketQua = await _context.KetQuaKhams.Include(k => k.ChiTietDichVus).FirstOrDefaultAsync(k => k.MaKetQua == id);
            if (ketQua == null) return NotFound();

            var lich = await LayLichAsync(ketQua.MaLichKham);
            var loi = await KiemTraLichHopLeAsync(lich);
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                vm.MaLichKham = ketQua.MaLichKham;
                vm.LichKham = lich;
                await NapDichVuAsync(vm, ketQua.ChiTietDichVus);
                return View(vm);
            }

            GanDuLieu(ketQua, vm);

            // Cập nhật danh sách dịch vụ: giữ đơn giá cũ của dịch vụ đã có, dịch vụ mới lấy giá hiện tại
            var giaDichVu = await _context.DichVuYTes.ToDictionaryAsync(d => d.MaDichVu, d => d.DonGia);
            var chon = vm.DichVus.Where(d => d.DaChon && giaDichVu.ContainsKey(d.MaDichVu)).ToList();
            foreach (var ct in ketQua.ChiTietDichVus.ToList())
            {
                var moi = chon.FirstOrDefault(d => d.MaDichVu == ct.MaDichVu);
                if (moi == null) _context.ChiTietDichVus.Remove(ct);
                else ct.SoLuong = moi.SoLuong;
            }
            foreach (var dv in chon.Where(d => ketQua.ChiTietDichVus.All(c => c.MaDichVu != d.MaDichVu)))
                _context.ChiTietDichVus.Add(new ChiTietDichVu { MaKetQua = ketQua.MaKetQua, MaDichVu = dv.MaDichVu, SoLuong = dv.SoLuong, DonGia = giaDichVu[dv.MaDichVu] });

            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã cập nhật kết quả khám.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private static void GanDuLieu(KetQuaKham k, KetQuaKhamFormViewModel vm)
        {
            k.ChanDoan = vm.ChanDoan.Trim();
            k.KetLuan = vm.KetLuan.Trim();
            k.DonThuoc = vm.DonThuoc;
            k.LoiDan = vm.LoiDan;
            k.TrangThai = vm.TrangThai;
            k.GhiChu = vm.GhiChu;
            k.NgayCapNhat = DateTime.Now;
        }

        // ======================= XEM KẾT QUẢ =======================

        // GET: /KetQuaKham/Details/5 - mọi vai trò, nhưng chỉ xem được dữ liệu được phép
        [PhanQuyen]
        public async Task<IActionResult> Details(int id)
        {
            var ketQua = await _context.KetQuaKhams.AsNoTracking()
                .Include(k => k.LichKham).ThenInclude(l => l!.BenhNhan)
                .Include(k => k.LichKham).ThenInclude(l => l!.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                .Include(k => k.ChiTietDichVus).ThenInclude(c => c.DichVuYTe)
                .FirstOrDefaultAsync(k => k.MaKetQua == id);
            if (ketQua == null) return NotFound();

            var vaiTro = HttpContext.Session.LayVaiTro();
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            // Bệnh nhân chỉ xem kết quả của mình; bác sĩ chỉ xem kết quả mình khám
            if (vaiTro == VaiTro.BenhNhan && ketQua.LichKham?.BenhNhan?.MaTaiKhoan != maTaiKhoan) return NotFound();
            if (vaiTro == VaiTro.BacSi && ketQua.LichKham?.BacSi?.MaTaiKhoan != maTaiKhoan)
                return RedirectToAction("TuChoiTruyCap", "TaiKhoan");

            return View(ketQua);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ketQua = await _context.KetQuaKhams.Include(k => k.ChiTietDichVus).FirstOrDefaultAsync(k => k.MaKetQua == id);
            if (ketQua == null) return NotFound();
            _context.ChiTietDichVus.RemoveRange(ketQua.ChiTietDichVus);
            _context.KetQuaKhams.Remove(ketQua);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã xóa kết quả khám.";
            return RedirectToAction(nameof(Index));
        }
    }
}
