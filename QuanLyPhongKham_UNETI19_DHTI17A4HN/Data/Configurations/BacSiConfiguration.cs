// Họ và tên: Lê Quang Trung
// Mã sinh viên: 23103100270
// Nội dung thực hiện: Cấu hình Fluent API cho BacSi - ChuyenKhoa (1) - (n) BacSi, TaiKhoan (1) - (0..1) BacSi.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data.Configurations
{
    public class BacSiConfiguration : IEntityTypeConfiguration<BacSi>
    {
        public void Configure(EntityTypeBuilder<BacSi> e)
        {
            e.ToTable("BacSi");
            e.Property(b => b.NgaySinh).HasColumnType("date");

            // ChuyenKhoa (1) - (n) BacSi: không cho xóa chuyên khoa khi còn bác sĩ
            e.HasOne(b => b.ChuyenKhoa)
             .WithMany(c => c.BacSis)
             .HasForeignKey(b => b.MaChuyenKhoa)
             .OnDelete(DeleteBehavior.Restrict);

            // TaiKhoan (1) - (0..1) BacSi
            e.HasOne(b => b.TaiKhoan)
             .WithOne(t => t.BacSi)
             .HasForeignKey<BacSi>(b => b.MaTaiKhoan)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(b => b.HoTen);
        }
    }
}
