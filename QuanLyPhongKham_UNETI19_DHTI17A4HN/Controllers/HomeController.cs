// Họ và tên: Trần Phương Thảo (Nhóm trưởng)
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Controller trang chủ (khởi tạo từ mẫu ASP.NET Core MVC).
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
