using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTaoCSDL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChuyenKhoa",
                columns: table => new
                {
                    MaChuyenKhoa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChuyenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoPhong = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenKhoa", x => x.MaChuyenKhoa);
                });

            migrationBuilder.CreateTable(
                name: "DichVuYTe",
                columns: table => new
                {
                    MaDichVu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDichVu = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichVuYTe", x => x.MaDichVu);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "BacSi",
                columns: table => new
                {
                    MaBacSi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuyenKhoa = table.Column<int>(type: "int", nullable: false),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ChuyenMon = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HocVi = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacSi", x => x.MaBacSi);
                    table.ForeignKey(
                        name: "FK_BacSi_ChuyenKhoa_MaChuyenKhoa",
                        column: x => x.MaChuyenKhoa,
                        principalTable: "ChuyenKhoa",
                        principalColumn: "MaChuyenKhoa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BacSi_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BenhNhan",
                columns: table => new
                {
                    MaBenhNhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SoBHYT = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    NgayDangKy = table.Column<DateTime>(type: "date", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenhNhan", x => x.MaBenhNhan);
                    table.ForeignKey(
                        name: "FK_BenhNhan_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LichKham",
                columns: table => new
                {
                    MaLichKham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBenhNhan = table.Column<int>(type: "int", nullable: false),
                    MaBacSi = table.Column<int>(type: "int", nullable: false),
                    NgayKham = table.Column<DateTime>(type: "date", nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    LyDoKham = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichKham", x => x.MaLichKham);
                    table.ForeignKey(
                        name: "FK_LichKham_BacSi_MaBacSi",
                        column: x => x.MaBacSi,
                        principalTable: "BacSi",
                        principalColumn: "MaBacSi",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichKham_BenhNhan_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhan",
                        principalColumn: "MaBenhNhan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaKham",
                columns: table => new
                {
                    MaKetQua = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLichKham = table.Column<int>(type: "int", nullable: false),
                    ChanDoan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    KetLuan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DonThuoc = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LoiDan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaKham", x => x.MaKetQua);
                    table.ForeignKey(
                        name: "FK_KetQuaKham_LichKham_MaLichKham",
                        column: x => x.MaLichKham,
                        principalTable: "LichKham",
                        principalColumn: "MaLichKham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDichVu",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKetQua = table.Column<int>(type: "int", nullable: false),
                    MaDichVu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDichVu", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietDichVu_DichVuYTe_MaDichVu",
                        column: x => x.MaDichVu,
                        principalTable: "DichVuYTe",
                        principalColumn: "MaDichVu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietDichVu_KetQuaKham_MaKetQua",
                        column: x => x.MaKetQua,
                        principalTable: "KetQuaKham",
                        principalColumn: "MaKetQua",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BacSi_HoTen",
                table: "BacSi",
                column: "HoTen");

            migrationBuilder.CreateIndex(
                name: "IX_BacSi_MaChuyenKhoa",
                table: "BacSi",
                column: "MaChuyenKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_BacSi_MaTaiKhoan",
                table: "BacSi",
                column: "MaTaiKhoan",
                unique: true,
                filter: "[MaTaiKhoan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhan_MaTaiKhoan",
                table: "BenhNhan",
                column: "MaTaiKhoan",
                unique: true,
                filter: "[MaTaiKhoan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDichVu_MaDichVu",
                table: "ChiTietDichVu",
                column: "MaDichVu");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDichVu_MaKetQua",
                table: "ChiTietDichVu",
                column: "MaKetQua");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenKhoa_TenChuyenKhoa",
                table: "ChuyenKhoa",
                column: "TenChuyenKhoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DichVuYTe_TenDichVu",
                table: "DichVuYTe",
                column: "TenDichVu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaKham_MaLichKham",
                table: "KetQuaKham",
                column: "MaLichKham",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichKham_MaBacSi_NgayKham",
                table: "LichKham",
                columns: new[] { "MaBacSi", "NgayKham" });

            migrationBuilder.CreateIndex(
                name: "IX_LichKham_MaBenhNhan_NgayKham",
                table: "LichKham",
                columns: new[] { "MaBenhNhan", "NgayKham" });

            migrationBuilder.CreateIndex(
                name: "IX_LichKham_TrangThai",
                table: "LichKham",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDichVu");

            migrationBuilder.DropTable(
                name: "DichVuYTe");

            migrationBuilder.DropTable(
                name: "KetQuaKham");

            migrationBuilder.DropTable(
                name: "LichKham");

            migrationBuilder.DropTable(
                name: "BacSi");

            migrationBuilder.DropTable(
                name: "BenhNhan");

            migrationBuilder.DropTable(
                name: "ChuyenKhoa");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
