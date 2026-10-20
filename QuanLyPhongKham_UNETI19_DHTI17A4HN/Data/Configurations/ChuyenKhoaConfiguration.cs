// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Cấu hình Fluent API cho ChuyenKhoa - tên chuyên khoa không trùng.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data.Configurations
{
    public class ChuyenKhoaConfiguration : IEntityTypeConfiguration<ChuyenKhoa>
    {
        public void Configure(EntityTypeBuilder<ChuyenKhoa> e)
        {
            e.ToTable("ChuyenKhoa");
            e.HasIndex(c => c.TenChuyenKhoa).IsUnique(); // Tên chuyên khoa không trùng
        }
    }
}
