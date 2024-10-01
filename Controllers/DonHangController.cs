using Microsoft.AspNetCore.Mvc;
using System;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Controllers
{
    public class DonHangController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Hshop2023Context db;
        public DonHangController(ILogger<HomeController> logger, Hshop2023Context context)
        {
            _logger = logger;
            db = context;

        }
        public IActionResult Index()
        {
            var MaKh = User.Identity.Name;
            // Truy vấn hóa đơn từ database
            var hoaDons = db.HoaDons.AsQueryable();

            // Nếu MaKh không null hoặc rỗng, lọc theo khách hàng
            if (!string.IsNullOrEmpty(MaKh))
            {
                hoaDons = hoaDons.Where(hd => hd.MaKh == MaKh);
            }

            // Lấy chi tiết hóa đơn liên kết với các hóa đơn vừa truy vấn
            var result = hoaDons.Select(hd => new DonHangVM
            {
                MaHD = hd.MaHd,
                MaKH = hd.MaKh,
                NgayDat = hd.NgayDat,
                //NgayCan = hd.NgayCan,
                //NgayGiao = hd.NgayGiao,
                HoTen = hd.HoTen,
                DiaChi = hd.DiaChi,
                CachThanhToan = hd.CachThanhToan,
                CachVanChuyen = hd.CachVanChuyen,
                PhiVanChuyen = (int)hd.PhiVanChuyen,
                MaTrangThai = hd.MaTrangThai,
                DienThoai = hd.DienThoai,
                TrangThai = hd.MaTrangThaiNavigation.TenTrangThai,
                ChiTietHds = db.ChiTietHds
                    .Where(ct => ct.MaHd == hd.MaHd)
                    .Select(ct => new ChiTietHoaDonMD
                    {
                        MaCT = ct.MaCt,
                        MaHD = ct.MaHd,
                        MaHH = ct.MaHh,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia,
                        TenHangHoa = db.HangHoas.FirstOrDefault(hh => hh.MaHh == ct.MaHh).TenHh,
                        MaGiamGia = (int)ct.MaGiamGia,
                        HinhAnh = db.HangHoas.FirstOrDefault(hh => hh.MaHh == ct.MaHh).Hinh
                    }).ToList()
            }).ToList();

            // Trả về kết quả cho View
            return View(result);
        }
    }
}
