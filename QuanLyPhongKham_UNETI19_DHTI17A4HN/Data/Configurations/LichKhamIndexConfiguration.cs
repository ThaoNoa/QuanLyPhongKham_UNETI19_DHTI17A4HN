// Họ và tên: Đỗ Huy Tùng
// Mã sinh viên: 23103100296
// Nội dung thực hiện: Index cho bảng LichKham phục vụ tìm kiếm, lọc lịch theo bác sĩ, bệnh nhân, ngày và trạng thái.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data.Configurations
{
    public class LichKhamIndexConfiguration : IEntityTypeConfiguration<LichKham>
    {
        public void Configure(EntityTypeBuilder<LichKham> e)
        {
            e.HasIndex(l => new { l.MaBacSi, l.NgayKham });     // lịch của bác sĩ theo ngày (kiểm tra trùng lịch)
            e.HasIndex(l => new { l.MaBenhNhan, l.NgayKham });  // lịch của bệnh nhân theo ngày
            e.HasIndex(l => l.TrangThai);                       // lọc theo trạng thái
        }
    }
}
