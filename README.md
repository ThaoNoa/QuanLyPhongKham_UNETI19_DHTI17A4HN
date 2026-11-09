# QuanLyPhongKham_UNETI19_DHTI17A4HN

**Đề tài 16 – Hệ thống quản lý đặt lịch khám phòng khám (PhongKhamCare)**
Bài tập lớn môn Thực hành lập trình .NET – Nhóm 19 – Lớp DHTI17A4HN

| Thành viên | Mã SV | Module |
|---|---|---|
| Trần Hoài Nam | 23103100240 | Module 1 – Tài khoản, đăng nhập, phân quyền, chuyên khoa |
| Lê Quang Trung | 23103100270 | Module 2 – Quản lý bác sĩ, tìm kiếm, lọc, sắp xếp, phân trang |
| Trần Anh Đức | 23103100284 | Module 3 – Bệnh nhân, hồ sơ cá nhân, đặt lịch, theo dõi lịch |
| Đỗ Huy Tùng | 23103100296 | Module 4 – Tiếp nhận lịch, xác nhận/hủy, trạng thái khám |
| Trần Phương Thảo (nhóm trưởng) | 23103100226 | Module 5 – Kết quả khám, dịch vụ, Dashboard, thống kê |

## Công nghệ

.NET 10 · ASP.NET Core 10 MVC · Entity Framework Core 10 (Code First, Migration) · SQL Server (LocalDB) · LINQ · Razor View · Bootstrap 5 · Chart.js · ClosedXML (xuất Excel)

## Cài đặt và chạy

**Yêu cầu:** Visual Studio 2026 (workload *ASP.NET and web development*, có sẵn .NET 10 SDK và SQL Server LocalDB).
Visual Studio 2022 không build được dự án .NET 10.

1. Mở `QuanLyPhongKham_UNETI19_DHTI17A4HN.sln`.
2. Chuỗi kết nối nằm ở `appsettings.json` (mặc định `(localdb)\MSSQLLocalDB`, CSDL `QuanLyPhongKham_UNETI19`).
3. Mở **Tools → NuGet Package Manager → Package Manager Console**, chạy:
   ```
   Add-Migration KhoiTaoCSDL
   Update-Database
   ```
   (Nếu repo đã có thư mục `Migrations` thì chỉ cần `Update-Database`.)
4. Nhấn **F5**. Lần chạy đầu, ứng dụng tự áp dụng Migration và tạo dữ liệu mẫu.

Dùng dòng lệnh thay cho Visual Studio:
```
dotnet tool install --global dotnet-ef
cd QuanLyPhongKham_UNETI19_DHTI17A4HN
dotnet ef migrations add KhoiTaoCSDL
dotnet ef database update
dotnet run
```

Tạo lại CSDL từ đầu: `Drop-Database` rồi `Update-Database`.

## Tài khoản kiểm thử

| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Admin | `admin`, `admin2` | `Admin@123` |
| Nhân viên | `nhanvien1`, `nhanvien2` | `NhanVien@123` |
| Nhân viên (đã bị khóa) | `nhanvien3` | `NhanVien@123` |
| Bác sĩ | `bs01` … `bs15` (`bs15` đang tạm ngừng) | `BacSi@123` |
| Bệnh nhân | `bn01` … `bn20` | `BenhNhan@123` |

Mật khẩu được mã hóa bằng `PasswordHasher` (PBKDF2), không lưu mật khẩu gốc.

## Dữ liệu mẫu (`Data/DbSeeder.cs`)

8 chuyên khoa · 15 bác sĩ · 30 bệnh nhân (20 có tài khoản) · 2 Admin · 3 nhân viên · 12 dịch vụ ·
48 lịch khám ở đủ 4 trạng thái (có 2 lịch hôm nay của `bs01`) · 20 kết quả khám kèm dịch vụ.

## Luồng nghiệp vụ chính

```
Bệnh nhân đặt lịch ──► Chờ xác nhận ──(Nhân viên xác nhận, kiểm tra lại trùng lịch)──► Đã xác nhận ──(Bác sĩ/NV ghi nhận)──► Đã khám ──► Nhập kết quả + dịch vụ
                              └──(Nhân viên hủy / bệnh nhân tự hủy)──► Đã hủy
```

- Không đặt lịch vào thời điểm đã qua, vào Chủ nhật hay ngoài giờ làm việc (07:00–17:30); giờ kết thúc phải sau giờ bắt đầu.
- Chỉ bác sĩ đang hoạt động mới nhận lịch. Không trùng lịch bác sĩ, không trùng lịch bệnh nhân (kiểm tra bằng LINQ `AnyAsync`).
- Không chuyển thẳng Chờ xác nhận → Đã khám. Chỉ lịch Đã khám mới được nhập kết quả.
- Bệnh nhân chỉ xem và sửa dữ liệu của chính mình. Đổi mã trên URL sang lịch hoặc kết quả của người khác sẽ trả về 404.

## Cấu trúc thư mục

```
Controllers/   TaiKhoan, ChuyenKhoa, BacSi, BenhNhan, LichKham, TiepNhan, KetQuaKham, DichVuYTe, ThongKe, Home
Models/Entities  TaiKhoan, ChuyenKhoa, BacSi, BenhNhan, LichKham, KetQuaKham, DichVuYTe, ChiTietDichVu
Models/Enums     VaiTro, HocVi, TrangThaiLichKham, TrangThaiKetQua
ViewModels/    ViewModel cho từng màn hình
Data/          PhongKhamDbContext (Fluent API), DbSeeder (dữ liệu mẫu)
Services/      LichKhamService (kiểm tra trùng lịch, luồng trạng thái)
Filters/       PhanQuyenAttribute (phân quyền theo Session)
Helpers/       SessionHelper, MatKhauHelper, PaginatedList, EnumHelper, GiaoDienHelper
Views/         Razor View, _Layout (bệnh nhân), _AdminLayout (quản trị)
wwwroot/       css/site.css, js/site.js, lib (bootstrap, bootstrap-icons, chart.js, font Be Vietnam Pro)
```

## Phân công file theo Module

| Module | File chính |
|---|---|
| **M1 – Nam** | `Models/Entities/TaiKhoan.cs`, `ChuyenKhoa.cs`, `Models/Enums/VaiTro.cs`, `Controllers/TaiKhoanController.cs`, `ChuyenKhoaController.cs`, `ViewModels/TaiKhoanViewModels.cs`, `ChuyenKhoaViewModels.cs`, `Filters/PhanQuyenAttribute.cs`, `Helpers/SessionHelper.cs`, `MatKhauHelper.cs`, `Views/TaiKhoan/*`, `Views/ChuyenKhoa/*`, `Views/Shared/_IconChuyenKhoa.cshtml` |
| **M2 – Trung** | `Models/Entities/BacSi.cs`, `Models/Enums/HocVi.cs`, `Controllers/BacSiController.cs`, `ViewModels/BacSiViewModels.cs`, `PhanTrangViewModel.cs`, `Helpers/PaginatedList.cs`, `Views/BacSi/*`, `Views/Shared/_PhanTrang.cshtml`, `_AnhBacSi.cshtml` |
| **M3 – Đức** | `Models/Entities/BenhNhan.cs`, `LichKham.cs`, `Models/Enums/TrangThaiLichKham.cs`, `Controllers/BenhNhanController.cs`, `LichKhamController.cs`, `ViewModels/BenhNhanViewModels.cs`, `Services/LichKhamService.cs` (phần đặt lịch), `Views/BenhNhan/*`, `Views/LichKham/*` |
| **M4 – Tùng** | `Controllers/TiepNhanController.cs`, `ViewModels/LichKhamViewModels.cs`, `Services/LichKhamService.cs` (phần luồng trạng thái), `Views/TiepNhan/*`, `Views/LichKham/_TienTrinh.cshtml` |
| **M5 – Thảo** | `Models/Entities/KetQuaKham.cs`, `DichVuYTe.cs`, `ChiTietDichVu.cs`, `Models/Enums/TrangThaiKetQua.cs`, `Controllers/KetQuaKhamController.cs`, `DichVuYTeController.cs`, `ThongKeController.cs`, `ViewModels/KetQuaKhamViewModels.cs`, `ThongKeViewModels.cs`, `Views/KetQuaKham/*`, `Views/DichVuYTe/*`, `Views/ThongKe/*` |
| **Chung (nhóm trưởng tạo, mỗi người bổ sung phần mình)** | `Program.cs`, `Data/PhongKhamDbContext.cs`, `Data/DbSeeder.cs`, `Views/Shared/_Layout.cshtml`, `_AdminLayout.cshtml`, `wwwroot/css/site.css`, `wwwroot/js/site.js`, `Controllers/HomeController.cs`, `Views/Home/Index.cshtml` |

Mỗi file có ghi Họ tên, Mã sinh viên và Nội dung thực hiện ở đầu file.

## Ảnh trang chủ

Đặt ảnh bác sĩ (PNG nền trong suốt, cao khoảng 900–1000px) tại `wwwroot/images/hero-doctor.png`. Trang chủ sẽ tự dùng ảnh này thay cho hình minh họa.
Ảnh đại diện bác sĩ được upload trong mục **Quản lý bác sĩ → Sửa**.
