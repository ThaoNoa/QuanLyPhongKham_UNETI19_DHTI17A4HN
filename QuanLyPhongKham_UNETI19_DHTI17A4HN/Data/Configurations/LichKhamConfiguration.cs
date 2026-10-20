// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Cấu hình Fluent API cho LichKham - BenhNhan (1) - (n) LichKham, BacSi (1) - (n) LichKham.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data.Configurations
{
    public class LichKhamConfiguration : IEntityTypeConfiguration<LichKham>
    {
        public void Configure(EntityTypeBuilder<LichKham> e)
        {
            e.ToTable("LichKham");
            e.Property(l => l.NgayKham).HasColumnType("date");
            e.Property(l => l.TrangThai).HasConversion<int>();

            // BenhNhan (1) - (n) LichKham
            e.HasOne(l => l.BenhNhan)
             .WithMany(b => b.LichKhams)
             .HasForeignKey(l => l.MaBenhNhan)
             .OnDelete(DeleteBehavior.Restrict);

            // BacSi (1) - (n) LichKham
            e.HasOne(l => l.BacSi)
             .WithMany(b => b.LichKhams)
             .HasForeignKey(l => l.MaBacSi)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
