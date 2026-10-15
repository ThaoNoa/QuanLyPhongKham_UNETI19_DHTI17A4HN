// Họ và tên: Lê Quang Trung
// Mã sinh viên: 23103100270
// Nội dung thực hiện: Khai báo học vị bác sĩ. Giá trị số tăng dần theo cấp bậc để sắp xếp theo học vị.
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums
{
    public enum HocVi
    {
        [Display(Name = "Bác sĩ", ShortName = "BS.")] BacSi = 1,
        [Display(Name = "Bác sĩ CKI", ShortName = "BSCKI.")] BacSiCKI = 2,
        [Display(Name = "Thạc sĩ", ShortName = "ThS. BS")] ThacSi = 3,
        [Display(Name = "Bác sĩ CKII", ShortName = "BSCKII.")] BacSiCKII = 4,
        [Display(Name = "Tiến sĩ", ShortName = "TS. BS")] TienSi = 5,
        [Display(Name = "Phó giáo sư", ShortName = "PGS. TS")] PhoGiaoSu = 6
    }
}
