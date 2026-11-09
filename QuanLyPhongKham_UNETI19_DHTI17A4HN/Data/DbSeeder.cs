// File dùng chung của nhóm - mỗi thành viên chuẩn bị dữ liệu mẫu cho Module của mình:
// Họ và tên: Trần Phương Thảo (Nhóm trưởng) - Mã sinh viên: 23103100226
// Nội dung thực hiện: Khung tạo dữ liệu mẫu; dịch vụ y tế, kết quả khám và chi tiết dịch vụ.
// Họ và tên: Trần Hoài Nam - Mã sinh viên: 23103100240
// Nội dung thực hiện: Dữ liệu mẫu tài khoản Admin, Nhân viên và chuyên khoa.
// Họ và tên: Lê Quang Trung - Mã sinh viên: 23103100270
// Nội dung thực hiện: Dữ liệu mẫu 15 bác sĩ (kèm tài khoản bác sĩ).
// Họ và tên: Trần Anh Đức - Mã sinh viên: 23103100284
// Nội dung thực hiện: Dữ liệu mẫu 30 bệnh nhân và các lịch khám (không trùng lịch).
// Họ và tên: Đỗ Huy Tùng - Mã sinh viên: 23103100296
// Nội dung thực hiện: Phân bổ trạng thái lịch khám (chờ xác nhận, đã xác nhận, đã khám, đã hủy).
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data
{
    /// <summary>
    /// Tạo dữ liệu mẫu khi CSDL còn trống (chạy 1 lần lúc khởi động ứng dụng).
    /// Mật khẩu mẫu: Admin@123, NhanVien@123, BacSi@123, BenhNhan@123
    /// </summary>
    public static class DbSeeder
    {
        // Ngày đặt lịch: trước ngày khám 1-6 ngày nhưng không vượt quá thời điểm hiện tại
        private static DateTime TaoNgayDat(DateTime ngayKham, Random rnd)
        {
            var ngayDat = ngayKham.AddDays(-rnd.Next(1, 7)).AddHours(rnd.Next(7, 20));
            return ngayDat > DateTime.Now ? DateTime.Now.AddHours(-rnd.Next(1, 72)) : ngayDat;
        }

        public static async Task SeedAsync(PhongKhamDbContext db)
        {
            if (await db.TaiKhoans.AnyAsync()) return; // Đã có dữ liệu

            var rnd = new Random(19); // cố định seed để lần nào tạo cũng giống nhau
            var homNay = DateTime.Today;

            // ===================== Module 1: Tài khoản quản trị + Chuyên khoa =====================
            string mkAdmin = MatKhauHelper.MaHoa("Admin@123");
            string mkNhanVien = MatKhauHelper.MaHoa("NhanVien@123");
            string mkBacSi = MatKhauHelper.MaHoa("BacSi@123");
            string mkBenhNhan = MatKhauHelper.MaHoa("BenhNhan@123");

            var quanTri = new List<TaiKhoan>
            {
                new() { TenDangNhap = "admin", MatKhau = mkAdmin, HoTen = "Quản trị hệ thống", Email = "admin@phongkhamcare.vn", VaiTro = VaiTro.Admin },
                new() { TenDangNhap = "admin2", MatKhau = mkAdmin, HoTen = "Nguyễn Quốc Bảo", Email = "bao.nq@phongkhamcare.vn", VaiTro = VaiTro.Admin },
                new() { TenDangNhap = "nhanvien1", MatKhau = mkNhanVien, HoTen = "Phạm Thu Hà", Email = "ha.pt@phongkhamcare.vn", VaiTro = VaiTro.NhanVien },
                new() { TenDangNhap = "nhanvien2", MatKhau = mkNhanVien, HoTen = "Vũ Minh Khang", Email = "khang.vm@phongkhamcare.vn", VaiTro = VaiTro.NhanVien },
                new() { TenDangNhap = "nhanvien3", MatKhau = mkNhanVien, HoTen = "Đặng Thị Hồng", Email = "hong.dt@phongkhamcare.vn", VaiTro = VaiTro.NhanVien, TrangThai = false }, // tài khoản bị khóa để kiểm thử
            };
            db.TaiKhoans.AddRange(quanTri);

            var khoa = new List<ChuyenKhoa>
            {
                new() { TenChuyenKhoa = "Nội khoa", SoPhong = 101, MoTa = "Khám và điều trị các bệnh lý nội khoa: tim mạch, tiêu hóa, hô hấp, nội tiết." },
                new() { TenChuyenKhoa = "Ngoại khoa", SoPhong = 102, MoTa = "Khám, tư vấn và tiểu phẫu các bệnh lý ngoại khoa, chấn thương chỉnh hình." },
                new() { TenChuyenKhoa = "Sản phụ khoa", SoPhong = 201, MoTa = "Khám thai, theo dõi thai kỳ, khám và điều trị bệnh phụ khoa." },
                new() { TenChuyenKhoa = "Nhi khoa", SoPhong = 202, MoTa = "Khám sức khỏe, dinh dưỡng và điều trị bệnh cho trẻ em." },
                new() { TenChuyenKhoa = "Tai Mũi Họng", SoPhong = 203, MoTa = "Khám, nội soi và điều trị bệnh lý tai, mũi, họng." },
                new() { TenChuyenKhoa = "Răng Hàm Mặt", SoPhong = 301, MoTa = "Khám răng, cạo vôi, trám răng và tư vấn chỉnh nha." },
                new() { TenChuyenKhoa = "Da liễu", SoPhong = 302, MoTa = "Khám và điều trị các bệnh về da, tóc, móng; tư vấn chăm sóc da." },
                new() { TenChuyenKhoa = "Mắt", SoPhong = 303, MoTa = "Đo thị lực, khám và điều trị các bệnh lý về mắt." },
            };
            db.ChuyenKhoas.AddRange(khoa);
            await db.SaveChangesAsync();

            // ===================== Module 2: 15 bác sĩ (kèm tài khoản) =====================
            var dsBacSi = new (string Ten, string GioiTinh, HocVi HocVi, int Khoa, string ChuyenMon, int NamSinh)[]
            {
                ("Nguyễn Văn Minh", "Nam", HocVi.TienSi, 0, "Tim mạch, tăng huyết áp", 1972),
                ("Hoàng Thị Thu", "Nữ", HocVi.ThacSi, 0, "Tiêu hóa, gan mật", 1985),
                ("Lê Quang Huy", "Nam", HocVi.ThacSi, 1, "Chấn thương chỉnh hình", 1983),
                ("Bùi Đức Long", "Nam", HocVi.BacSiCKII, 1, "Ngoại tổng quát, tiểu phẫu", 1976),
                ("Trần Thị Mai", "Nữ", HocVi.BacSiCKI, 2, "Theo dõi thai kỳ, siêu âm sản khoa", 1986),
                ("Ngô Thị Hạnh", "Nữ", HocVi.PhoGiaoSu, 2, "Vô sinh hiếm muộn, phụ khoa", 1968),
                ("Phạm Thị Lan", "Nữ", HocVi.BacSi, 3, "Dinh dưỡng và hô hấp trẻ em", 1990),
                ("Đinh Văn Tài", "Nam", HocVi.BacSiCKI, 3, "Sơ sinh, tiêm chủng", 1984),
                ("Vương Minh Tuấn", "Nam", HocVi.TienSi, 4, "Nội soi tai mũi họng", 1975),
                ("Lý Thị Ngọc", "Nữ", HocVi.BacSi, 4, "Viêm xoang, dị ứng", 1992),
                ("Cao Văn Sơn", "Nam", HocVi.ThacSi, 5, "Nha khoa tổng quát, chỉnh nha", 1987),
                ("Đoàn Thị Yến", "Nữ", HocVi.BacSiCKI, 5, "Phục hình răng sứ", 1989),
                ("Tạ Minh Châu", "Nữ", HocVi.ThacSi, 6, "Da liễu thẩm mỹ, mụn trứng cá", 1988),
                ("Hồ Văn Phúc", "Nam", HocVi.BacSiCKII, 7, "Khúc xạ, đục thủy tinh thể", 1974),
                ("Mai Xuân Trường", "Nam", HocVi.BacSi, 0, "Nội tổng quát", 1993),   // bác sĩ tạm ngừng để kiểm thử
            };

            var bacSis = new List<BacSi>();
            for (int i = 0; i < dsBacSi.Length; i++)
            {
                var d = dsBacSi[i];
                var tk = new TaiKhoan
                {
                    TenDangNhap = $"bs{i + 1:00}",
                    MatKhau = mkBacSi,
                    HoTen = d.Ten,
                    Email = $"bs{i + 1:00}@phongkhamcare.vn",
                    VaiTro = VaiTro.BacSi
                };
                var bs = new BacSi
                {
                    HoTen = d.Ten,
                    GioiTinh = d.GioiTinh,
                    HocVi = d.HocVi,
                    MaChuyenKhoa = khoa[d.Khoa].MaChuyenKhoa,
                    ChuyenMon = d.ChuyenMon,
                    NgaySinh = new DateTime(d.NamSinh, rnd.Next(1, 13), rnd.Next(1, 28)),
                    SoDienThoai = $"09{rnd.Next(10000000, 99999999)}",
                    Email = $"bs{i + 1:00}@phongkhamcare.vn",
                    TrangThai = i != 14,
                    GhiChu = i == 14 ? "Đang nghỉ phép dài hạn" : null,
                    TaiKhoan = tk
                };
                bacSis.Add(bs);
            }
            db.BacSis.AddRange(bacSis);
            await db.SaveChangesAsync();

            // ===================== Module 3: 30 bệnh nhân =====================
            string[] ho = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương" };
            string[] demNam = { "Văn", "Minh", "Đức", "Quang", "Hữu", "Thành" };
            string[] demNu = { "Thị", "Ngọc", "Thu", "Thanh", "Phương", "Mai" };
            string[] tenNam = { "An", "Bình", "Cường", "Dũng", "Hải", "Hiếu", "Khoa", "Nam", "Phong", "Quân", "Sơn", "Tùng" };
            string[] tenNu = { "Anh", "Chi", "Dung", "Giang", "Hoa", "Linh", "Ly", "Nhung", "Quỳnh", "Trang", "Vân", "Yến" };
            string[] quan = { "Long Biên", "Hai Bà Trưng", "Hoàn Kiếm", "Đống Đa", "Cầu Giấy", "Hoàng Mai", "Thanh Xuân", "Gia Lâm" };

            var benhNhans = new List<BenhNhan>();
            for (int i = 0; i < 30; i++)
            {
                bool nam = i % 2 == 0;
                string hoTen = $"{ho[rnd.Next(ho.Length)]} {(nam ? demNam[rnd.Next(demNam.Length)] : demNu[rnd.Next(demNu.Length)])} {(nam ? tenNam[rnd.Next(tenNam.Length)] : tenNu[rnd.Next(tenNu.Length)])}";
                var bn = new BenhNhan
                {
                    HoTen = hoTen,
                    GioiTinh = nam ? "Nam" : "Nữ",
                    NgaySinh = new DateTime(rnd.Next(1955, 2020), rnd.Next(1, 13), rnd.Next(1, 28)),
                    SoDienThoai = $"0{(new[] { 3, 7, 8, 9 })[rnd.Next(4)]}{rnd.Next(10000000, 99999999)}",
                    Email = $"benhnhan{i + 1:00}@gmail.com",
                    DiaChi = $"{rnd.Next(1, 200)} phố {rnd.Next(1, 30)}, {quan[rnd.Next(quan.Length)]}, Hà Nội",
                    SoBHYT = i % 3 == 2 ? null : $"DN401{rnd.Next(10000, 99999)}{rnd.Next(10000, 99999)}",
                    // ngày đăng ký rải đều trong 14 tháng gần nhất
                    NgayDangKy = homNay.AddDays(-rnd.Next(30, 420)),
                    TrangThai = true
                };
                // 20 bệnh nhân đầu có tài khoản đăng nhập, 10 bệnh nhân đăng ký tại quầy
                if (i < 20)
                {
                    bn.TaiKhoan = new TaiKhoan
                    {
                        TenDangNhap = $"bn{i + 1:00}",
                        MatKhau = mkBenhNhan,
                        HoTen = hoTen,
                        Email = bn.Email!,
                        VaiTro = VaiTro.BenhNhan,
                        NgayTao = bn.NgayDangKy
                    };
                }
                benhNhans.Add(bn);
            }
            db.BenhNhans.AddRange(benhNhans);
            await db.SaveChangesAsync();

            // ===================== Module 5: Dịch vụ y tế =====================
            var dichVus = new List<DichVuYTe>
            {
                new() { TenDichVu = "Khám tổng quát", DonViTinh = "Lần", DonGia = 150000, MoTa = "Khám lâm sàng tổng quát với bác sĩ" },
                new() { TenDichVu = "Khám chuyên khoa", DonViTinh = "Lần", DonGia = 200000, MoTa = "Khám với bác sĩ chuyên khoa" },
                new() { TenDichVu = "Xét nghiệm công thức máu", DonViTinh = "Lần", DonGia = 120000 },
                new() { TenDichVu = "Xét nghiệm nước tiểu", DonViTinh = "Lần", DonGia = 80000 },
                new() { TenDichVu = "Siêu âm ổ bụng", DonViTinh = "Lần", DonGia = 250000 },
                new() { TenDichVu = "Chụp X-quang", DonViTinh = "Phim", DonGia = 200000 },
                new() { TenDichVu = "Điện tim (ECG)", DonViTinh = "Lần", DonGia = 150000 },
                new() { TenDichVu = "Nội soi tai mũi họng", DonViTinh = "Lần", DonGia = 300000 },
                new() { TenDichVu = "Đo thị lực", DonViTinh = "Lần", DonGia = 50000 },
                new() { TenDichVu = "Cạo vôi răng", DonViTinh = "Hàm", DonGia = 250000 },
                new() { TenDichVu = "Siêu âm thai 4D", DonViTinh = "Lần", DonGia = 450000 },
                new() { TenDichVu = "Tiêm phòng (công tiêm)", DonViTinh = "Mũi", DonGia = 30000, TrangThai = false },
            };
            db.DichVuYTes.AddRange(dichVus);
            await db.SaveChangesAsync();

            // ===================== Module 3 + 4: 48 lịch khám, không trùng =====================
            string[] lyDo =
            {
                "Đau đầu, chóng mặt kéo dài", "Ho, sốt nhẹ 3 ngày", "Đau bụng vùng thượng vị", "Khám sức khỏe định kỳ",
                "Đau khớp gối khi vận động", "Khám thai định kỳ", "Trẻ biếng ăn, chậm tăng cân", "Viêm họng, khó nuốt",
                "Nghẹt mũi, chảy nước mũi", "Đau răng hàm dưới", "Nổi mẩn ngứa ở tay", "Mắt mờ, nhức mắt", "Tái khám theo hẹn"
            };
            TimeSpan[] khungGio =
            {
                new(8, 0, 0), new(8, 30, 0), new(9, 0, 0), new(9, 30, 0), new(10, 0, 0), new(10, 30, 0),
                new(13, 30, 0), new(14, 0, 0), new(14, 30, 0), new(15, 0, 0), new(15, 30, 0), new(16, 0, 0)
            };

            // Kế hoạch: (số lượng, khoảng ngày so với hôm nay, trạng thái)
            var keHoach = new (int SoLuong, int TuNgay, int DenNgay, TrangThaiLichKham TrangThai)[]
            {
                (20, -400, -2, TrangThaiLichKham.DaKham),
                (8, -200, 10, TrangThaiLichKham.DaHuy),
                (6, 1, 14, TrangThaiLichKham.DaXacNhan),
                (12, 1, 14, TrangThaiLichKham.ChoXacNhan),
            };

            var daDung = new HashSet<string>(); // khóa: bác sĩ|ngày|giờ và bệnh nhân|ngày|giờ
            var bacSiHoatDong = bacSis.Where(b => b.TrangThai).ToList();
            var lichKhams = new List<LichKham>();
            foreach (var kh in keHoach)
            {
                int daTao = 0;
                while (daTao < kh.SoLuong)
                {
                    var bs = bacSiHoatDong[rnd.Next(bacSiHoatDong.Count)];
                    var bn = benhNhans[rnd.Next(benhNhans.Count)];
                    var ngay = homNay.AddDays(rnd.Next(kh.TuNgay, kh.DenNgay + 1));
                    if (ngay.DayOfWeek == DayOfWeek.Sunday) continue;
                    var gio = khungGio[rnd.Next(khungGio.Length)];
                    string khoaBs = $"BS{bs.MaBacSi}|{ngay:yyyyMMdd}|{gio}";
                    string khoaBn = $"BN{bn.MaBenhNhan}|{ngay:yyyyMMdd}|{gio}";
                    if (daDung.Contains(khoaBs) || daDung.Contains(khoaBn)) continue;
                    daDung.Add(khoaBs);
                    daDung.Add(khoaBn);

                    lichKhams.Add(new LichKham
                    {
                        MaBacSi = bs.MaBacSi,
                        MaBenhNhan = bn.MaBenhNhan,
                        NgayKham = ngay,
                        GioBatDau = gio,
                        GioKetThuc = gio.Add(TimeSpan.FromMinutes(30)),
                        LyDoKham = lyDo[rnd.Next(lyDo.Length)],
                        TrangThai = kh.TrangThai,
                        GhiChu = kh.TrangThai == TrangThaiLichKham.DaHuy ? "Bệnh nhân báo bận, hủy lịch" : null,
                        NgayTao = TaoNgayDat(ngay, rnd)
                    });
                    daTao++;
                }
            }
            // 2 lịch đã xác nhận trong ngày hôm nay của bác sĩ bs01 (để demo ghi nhận Đã khám, nhập kết quả)
            if (homNay.DayOfWeek != DayOfWeek.Sunday)
            {
                foreach (var (gio, bn) in new[] { (new TimeSpan(8, 0, 0), benhNhans[0]), (new TimeSpan(9, 0, 0), benhNhans[1]) })
                {
                    if (daDung.Contains($"BS{bacSis[0].MaBacSi}|{homNay:yyyyMMdd}|{gio}") || daDung.Contains($"BN{bn.MaBenhNhan}|{homNay:yyyyMMdd}|{gio}")) continue;
                    lichKhams.Add(new LichKham
                    {
                        MaBacSi = bacSis[0].MaBacSi, MaBenhNhan = bn.MaBenhNhan, NgayKham = homNay,
                        GioBatDau = gio, GioKetThuc = gio.Add(TimeSpan.FromMinutes(30)),
                        LyDoKham = "Khám sức khỏe định kỳ", TrangThai = TrangThaiLichKham.DaXacNhan,
                        NgayTao = homNay.AddDays(-2).AddHours(9)
                    });
                }
            }
            db.LichKhams.AddRange(lichKhams);
            await db.SaveChangesAsync();

            // ===================== Module 5: 20 kết quả khám + chi tiết dịch vụ =====================
            string[] chanDoan =
            {
                "Tăng huyết áp độ 1", "Viêm dạ dày cấp", "Viêm họng cấp", "Viêm mũi dị ứng", "Thai 20 tuần phát triển bình thường",
                "Thoái hóa khớp gối", "Sâu răng hàm số 6", "Viêm da cơ địa", "Cận thị nhẹ hai mắt", "Rối loạn tiêu hóa"
            };
            var dvHoatDong = dichVus.Where(d => d.TrangThai).ToList();
            foreach (var lich in lichKhams.Where(l => l.TrangThai == TrangThaiLichKham.DaKham))
            {
                var kq = new KetQuaKham
                {
                    MaLichKham = lich.MaLichKham,
                    ChanDoan = chanDoan[rnd.Next(chanDoan.Length)],
                    KetLuan = "Tình trạng ổn định, điều trị ngoại trú theo đơn.",
                    DonThuoc = "Paracetamol 500mg x 10 viên - uống khi sốt/đau\nVitamin C 500mg x 10 viên - ngày 1 viên",
                    LoiDan = "Uống nhiều nước, nghỉ ngơi hợp lý. Tái khám sau 2 tuần hoặc khi có dấu hiệu bất thường.",
                    NgayCapNhat = lich.NgayKham.Add(lich.GioKetThuc),
                    TrangThai = TrangThaiKetQua.HoanThanh
                };
                // Mỗi kết quả: 1 lượt khám + 0..2 dịch vụ cận lâm sàng
                kq.ChiTietDichVus.Add(new ChiTietDichVu { MaDichVu = dichVus[1].MaDichVu, SoLuong = 1, DonGia = dichVus[1].DonGia });
                int soDv = rnd.Next(0, 3);
                foreach (var dv in dvHoatDong.Skip(2).OrderBy(_ => rnd.Next()).Take(soDv))
                    kq.ChiTietDichVus.Add(new ChiTietDichVu { MaDichVu = dv.MaDichVu, SoLuong = 1, DonGia = dv.DonGia });
                db.KetQuaKhams.Add(kq);
            }
            await db.SaveChangesAsync();
        }
    }
}
