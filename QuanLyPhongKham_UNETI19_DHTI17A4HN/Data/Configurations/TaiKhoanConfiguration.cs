// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Cấu hình Fluent API cho TaiKhoan - tên đăng nhập không trùng.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data.Configurations
{
    public class TaiKhoanConfiguration : IEntityTypeConfiguration<TaiKhoan>
    {
        public void Configure(EntityTypeBuilder<TaiKhoan> e)
        {
            e.ToTable("TaiKhoan");
            e.HasIndex(t => t.TenDangNhap).IsUnique();   // Tên đăng nhập không trùng
            e.Property(t => t.VaiTro).HasConversion<int>();
        }
    }
}
