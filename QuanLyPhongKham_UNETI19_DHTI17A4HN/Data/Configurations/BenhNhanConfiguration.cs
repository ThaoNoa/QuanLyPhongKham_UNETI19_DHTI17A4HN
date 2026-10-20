// Họ và tên: Trần Anh Đức
// Mã sinh viên: 23103100284
// Nội dung thực hiện: Cấu hình Fluent API cho BenhNhan - TaiKhoan (1) - (0..1) BenhNhan.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data.Configurations
{
    public class BenhNhanConfiguration : IEntityTypeConfiguration<BenhNhan>
    {
        public void Configure(EntityTypeBuilder<BenhNhan> e)
        {
            e.ToTable("BenhNhan");
            e.Property(b => b.NgaySinh).HasColumnType("date");
            e.Property(b => b.NgayDangKy).HasColumnType("date");

            // TaiKhoan (1) - (0..1) BenhNhan
            e.HasOne(b => b.TaiKhoan)
             .WithOne(t => t.BenhNhan)
             .HasForeignKey<BenhNhan>(b => b.MaTaiKhoan)
             .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
