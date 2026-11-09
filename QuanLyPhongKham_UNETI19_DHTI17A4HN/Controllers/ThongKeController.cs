// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Dashboard và thống kê bằng LINQ (Count, Sum, GroupBy, OrderBy, OrderByDescending, Any):
// tổng chuyên khoa/bác sĩ/bệnh nhân, số lịch theo trạng thái, bệnh nhân theo tháng, bác sĩ nhiều lịch nhất,
// doanh thu theo tháng/năm/bác sĩ/chuyên khoa, khoa đông bệnh nhân nhất; xuất báo cáo Excel.
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Filters;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.ViewModels;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Controllers
{
    [PhanQuyen(VaiTro.Admin, VaiTro.NhanVien)]
    public class ThongKeController : Controller
    {
        private readonly PhongKhamDbContext _context;

        public ThongKeController(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ============================ DASHBOARD ============================

        public async Task<IActionResult> Dashboard()
        {
            int nam = DateTime.Today.Year;
            var vm = new DashboardViewModel { Nam = nam };

            // Tổng số chuyên khoa, bác sĩ, bệnh nhân - Count()
            vm.TongChuyenKhoa = await _context.ChuyenKhoas.CountAsync();
            vm.TongBacSi = await _context.BacSis.CountAsync();
            vm.TongBenhNhan = await _context.BenhNhans.CountAsync();

            // Số lịch theo từng trạng thái - GroupBy + Count()
            var theoTrangThai = await _context.LichKhams
                .GroupBy(l => l.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToListAsync();
            int Dem(TrangThaiLichKham t) => theoTrangThai.FirstOrDefault(x => x.TrangThai == t)?.SoLuong ?? 0;
            vm.SoLichChoXacNhan = Dem(TrangThaiLichKham.ChoXacNhan);
            vm.SoLichDaXacNhan = Dem(TrangThaiLichKham.DaXacNhan);
            vm.SoLichDaKham = Dem(TrangThaiLichKham.DaKham);
            vm.SoLichDaHuy = Dem(TrangThaiLichKham.DaHuy);

            vm.SoLichHomNay = await _context.LichKhams.CountAsync(l => l.NgayKham == DateTime.Today && l.TrangThai != TrangThaiLichKham.DaHuy);

            // Số bệnh nhân theo tháng (số bệnh nhân khác nhau có lịch khám, không tính lịch đã hủy)
            var bnTheoThang = await _context.LichKhams
                .Where(l => l.NgayKham.Year == nam && l.TrangThai != TrangThaiLichKham.DaHuy)
                .GroupBy(l => l.NgayKham.Month)
                .Select(g => new { Thang = g.Key, SoBenhNhan = g.Select(l => l.MaBenhNhan).Distinct().Count() })
                .ToListAsync();
            foreach (var x in bnTheoThang) vm.BenhNhanTheoThang[x.Thang - 1] = x.SoBenhNhan;

            // Doanh thu dịch vụ theo tháng - GroupBy + Sum(SoLuong * DonGia)
            var dtTheoThang = await DoanhThuTheoThangAsync(nam);
            vm.DoanhThuTheoThang = dtTheoThang;
            vm.DoanhThuNam = dtTheoThang.Sum();

            // Bác sĩ có nhiều lịch nhất - GroupBy + Count + OrderByDescending + Take
            vm.BacSiNhieuLichNhat = await _context.LichKhams
                .Where(l => l.TrangThai != TrangThaiLichKham.DaHuy)
                .GroupBy(l => new { l.MaBacSi, l.BacSi!.HoTen, l.BacSi.ChuyenKhoa!.TenChuyenKhoa })
                .Select(g => new DongThongKe { Ten = g.Key.HoTen, PhuDe = g.Key.TenChuyenKhoa, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong).ThenBy(x => x.Ten)
                .Take(5)
                .ToListAsync();

            // Lịch chờ xác nhận gần nhất
            vm.LichChoXuLy = await _context.LichKhams.AsNoTracking()
                .Include(l => l.BenhNhan).Include(l => l.BacSi)
                .Where(l => l.TrangThai == TrangThaiLichKham.ChoXacNhan)
                .OrderBy(l => l.NgayKham).ThenBy(l => l.GioBatDau)
                .Take(6).ToListAsync();

            return View(vm);
        }

        private async Task<decimal[]> DoanhThuTheoThangAsync(int nam)
        {
            var ketQua = new decimal[12];
            var ds = await _context.ChiTietDichVus
                .Where(c => c.KetQuaKham!.LichKham!.NgayKham.Year == nam)
                .GroupBy(c => c.KetQuaKham!.LichKham!.NgayKham.Month)
                .Select(g => new { Thang = g.Key, DoanhThu = g.Sum(c => c.SoLuong * c.DonGia) })
                .ToListAsync();
            foreach (var x in ds) ketQua[x.Thang - 1] = x.DoanhThu;
            return ketQua;
        }

        // ============================ THỐNG KÊ ============================

        public async Task<IActionResult> Index(int? nam)
        {
            return View(await TaoThongKeAsync(nam ?? DateTime.Today.Year));
        }

        private async Task<ThongKeViewModel> TaoThongKeAsync(int nam)
        {
            var vm = new ThongKeViewModel { Nam = nam };

            // Danh sách năm có dữ liệu lịch khám (Distinct + OrderByDescending)
            vm.CacNam = await _context.LichKhams.Select(l => l.NgayKham.Year).Distinct().OrderByDescending(y => y).ToListAsync();
            if (!vm.CacNam.Contains(nam)) vm.CacNam.Insert(0, nam);

            // 1. Số bệnh nhân theo khoa (đếm bệnh nhân khác nhau đã đăng ký lịch, không tính lịch hủy)
            vm.BenhNhanTheoKhoa = await _context.LichKhams
                .Where(l => l.NgayKham.Year == nam && l.TrangThai != TrangThaiLichKham.DaHuy)
                .GroupBy(l => l.BacSi!.ChuyenKhoa!.TenChuyenKhoa)
                .Select(g => new DongThongKe
                {
                    Ten = g.Key,
                    SoLuong = g.Select(l => l.MaBenhNhan).Distinct().Count(),   // số bệnh nhân
                    DoanhThu = g.Count()                                          // số lượt đăng ký
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            // 2. Khoa có số bệnh nhân đăng ký nhiều nhất
            vm.KhoaDongNhat = vm.BenhNhanTheoKhoa.OrderByDescending(x => x.SoLuong).ThenByDescending(x => x.DoanhThu).FirstOrDefault();

            // 3. Doanh thu theo năm và trung bình doanh thu một năm
            vm.DoanhThuTheoNam = await _context.ChiTietDichVus
                .GroupBy(c => c.KetQuaKham!.LichKham!.NgayKham.Year)
                .Select(g => new DongThongKe { Ten = g.Key.ToString(), SoLuong = g.Select(c => c.MaKetQua).Distinct().Count(), DoanhThu = g.Sum(c => c.SoLuong * c.DonGia) })
                .OrderBy(x => x.Ten)
                .ToListAsync();
            vm.TrungBinhDoanhThuNam = vm.DoanhThuTheoNam.Any() ? vm.DoanhThuTheoNam.Average(x => x.DoanhThu) : 0;

            vm.DoanhThuTheoThang = await DoanhThuTheoThangAsync(nam);
            vm.TongDoanhThuNamChon = vm.DoanhThuTheoThang.Sum();
            vm.TrungBinhThangNamChon = vm.TongDoanhThuNamChon / 12;

            // 4. Doanh thu theo từng bác sĩ
            vm.DoanhThuTheoBacSi = await _context.ChiTietDichVus
                .Where(c => c.KetQuaKham!.LichKham!.NgayKham.Year == nam)
                .GroupBy(c => new { c.KetQuaKham!.LichKham!.MaBacSi, c.KetQuaKham.LichKham.BacSi!.HoTen, c.KetQuaKham.LichKham.BacSi.ChuyenKhoa!.TenChuyenKhoa })
                .Select(g => new DongThongKe
                {
                    Ten = g.Key.HoTen,
                    PhuDe = g.Key.TenChuyenKhoa,
                    SoLuong = g.Select(c => c.MaKetQua).Distinct().Count(),
                    DoanhThu = g.Sum(c => c.SoLuong * c.DonGia)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToListAsync();

            // 5. Doanh thu theo chuyên khoa
            vm.DoanhThuTheoChuyenKhoa = await _context.ChiTietDichVus
                .Where(c => c.KetQuaKham!.LichKham!.NgayKham.Year == nam)
                .GroupBy(c => c.KetQuaKham!.LichKham!.BacSi!.ChuyenKhoa!.TenChuyenKhoa)
                .Select(g => new DongThongKe { Ten = g.Key, SoLuong = g.Select(c => c.MaKetQua).Distinct().Count(), DoanhThu = g.Sum(c => c.SoLuong * c.DonGia) })
                .OrderByDescending(x => x.DoanhThu)
                .ToListAsync();

            // 6. Dịch vụ được sử dụng nhiều nhất
            vm.DichVuDungNhieu = await _context.ChiTietDichVus
                .Where(c => c.KetQuaKham!.LichKham!.NgayKham.Year == nam)
                .GroupBy(c => c.DichVuYTe!.TenDichVu)
                .Select(g => new DongThongKe { Ten = g.Key, SoLuong = g.Sum(c => c.SoLuong), DoanhThu = g.Sum(c => c.SoLuong * c.DonGia) })
                .OrderByDescending(x => x.SoLuong)
                .Take(6)
                .ToListAsync();

            return vm;
        }

        // ============================ XUẤT EXCEL ============================

        // GET: /ThongKe/XuatExcel?nam=2026
        public async Task<IActionResult> XuatExcel(int? nam)
        {
            int namChon = nam ?? DateTime.Today.Year;
            var tk = await TaoThongKeAsync(namChon);
            var lichKhams = await _context.LichKhams.AsNoTracking()
                .Include(l => l.BenhNhan).Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa)
                .Where(l => l.NgayKham.Year == namChon)
                .OrderBy(l => l.NgayKham).ThenBy(l => l.GioBatDau)
                .ToListAsync();

            using var wb = new XLWorkbook();

            // Sheet 1: Doanh thu theo tháng
            var ws = wb.Worksheets.Add("Doanh thu theo tháng");
            TieuDe(ws, $"BÁO CÁO DOANH THU DỊCH VỤ NĂM {namChon}", 3);
            DongTieuDe(ws, 3, "Tháng", "Doanh thu (VNĐ)", "Tỷ trọng");
            for (int i = 0; i < 12; i++)
            {
                ws.Cell(4 + i, 1).Value = $"Tháng {i + 1}";
                ws.Cell(4 + i, 2).Value = (double)tk.DoanhThuTheoThang[i];
                ws.Cell(4 + i, 3).Value = tk.TongDoanhThuNamChon == 0 ? 0d : (double)(tk.DoanhThuTheoThang[i] / tk.TongDoanhThuNamChon);
                ws.Cell(4 + i, 3).Style.NumberFormat.Format = "0.0%";
            }
            ws.Cell(16, 1).Value = "Tổng cộng";
            ws.Cell(16, 2).Value = (double)tk.TongDoanhThuNamChon;
            ws.Cell(17, 1).Value = "Trung bình / tháng";
            ws.Cell(17, 2).Value = (double)tk.TrungBinhThangNamChon;
            ws.Cell(18, 1).Value = "Trung bình doanh thu một năm (mọi năm)";
            ws.Cell(18, 2).Value = (double)tk.TrungBinhDoanhThuNam;
            ws.Range(16, 1, 18, 2).Style.Font.Bold = true;
            ws.Range(4, 2, 18, 2).Style.NumberFormat.Format = "#,##0";
            ws.Columns().AdjustToContents();

            // Sheet 2: Theo bác sĩ
            ws = wb.Worksheets.Add("Theo bác sĩ");
            TieuDe(ws, $"DOANH THU THEO BÁC SĨ NĂM {namChon}", 4);
            DongTieuDe(ws, 3, "Bác sĩ", "Chuyên khoa", "Số lượt khám", "Doanh thu (VNĐ)");
            int r = 4;
            foreach (var x in tk.DoanhThuTheoBacSi)
            {
                ws.Cell(r, 1).Value = x.Ten;
                ws.Cell(r, 2).Value = x.PhuDe ?? "";
                ws.Cell(r, 3).Value = (double)x.SoLuong;
                ws.Cell(r, 4).Value = (double)x.DoanhThu;
                ws.Cell(r, 4).Style.NumberFormat.Format = "#,##0";
                r++;
            }
            ws.Columns().AdjustToContents();

            // Sheet 3: Theo chuyên khoa
            ws = wb.Worksheets.Add("Theo chuyên khoa");
            TieuDe(ws, $"THỐNG KÊ THEO CHUYÊN KHOA NĂM {namChon}", 4);
            DongTieuDe(ws, 3, "Chuyên khoa", "Số bệnh nhân", "Số lượt khám có dịch vụ", "Doanh thu (VNĐ)");
            r = 4;
            foreach (var k in tk.BenhNhanTheoKhoa)
            {
                var dt = tk.DoanhThuTheoChuyenKhoa.FirstOrDefault(x => x.Ten == k.Ten);
                ws.Cell(r, 1).Value = k.Ten;
                ws.Cell(r, 2).Value = (double)k.SoLuong;
                ws.Cell(r, 3).Value = (double)(dt?.SoLuong ?? 0);
                ws.Cell(r, 4).Value = (double)(dt?.DoanhThu ?? 0);
                ws.Cell(r, 4).Style.NumberFormat.Format = "#,##0";
                r++;
            }
            ws.Columns().AdjustToContents();

            // Sheet 4: Danh sách lịch khám
            ws = wb.Worksheets.Add("Lịch khám");
            TieuDe(ws, $"DANH SÁCH LỊCH KHÁM NĂM {namChon}", 7);
            DongTieuDe(ws, 3, "Mã lịch", "Ngày khám", "Giờ", "Bệnh nhân", "Bác sĩ", "Chuyên khoa", "Trạng thái");
            r = 4;
            foreach (var l in lichKhams)
            {
                ws.Cell(r, 1).Value = $"LK{l.MaLichKham:00000}";
                ws.Cell(r, 2).Value = l.NgayKham;
                ws.Cell(r, 2).Style.NumberFormat.Format = "dd/mm/yyyy";
                ws.Cell(r, 3).Value = $"{l.GioBatDau:hh\\:mm} - {l.GioKetThuc:hh\\:mm}";
                ws.Cell(r, 4).Value = l.BenhNhan?.HoTen ?? "";
                ws.Cell(r, 5).Value = l.BacSi?.HoTen ?? "";
                ws.Cell(r, 6).Value = l.BacSi?.ChuyenKhoa?.TenChuyenKhoa ?? "";
                ws.Cell(r, 7).Value = l.TrangThai.TenHienThi();
                r++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCao_PhongKham_{namChon}.xlsx");
        }

        private static void TieuDe(IXLWorksheet ws, string tieuDe, int soCot)
        {
            ws.Cell(1, 1).Value = tieuDe;
            ws.Range(1, 1, 1, soCot).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static void DongTieuDe(IXLWorksheet ws, int dong, params string[] cot)
        {
            for (int i = 0; i < cot.Length; i++)
                ws.Cell(dong, i + 1).Value = cot[i];
            var range = ws.Range(dong, 1, dong, cot.Length);
            range.Style.Font.Bold = true;
            range.Style.Font.FontColor = XLColor.White;
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#1D5FE6");
        }
    }
}
