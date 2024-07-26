using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList;
using System.Drawing.Printing;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TDProjectMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly Hshop2023Context db;

        public ProductController(Hshop2023Context context)
        {
            db = context;
        }
        public IActionResult Index(string? hang, int? loai, int? page, int? pagesize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pagesize == null)
            {
                pagesize = 9;
            }
            ViewBag.PageSize = pagesize;

            var hangHoas = db.HangHoas.AsQueryable();

            if (loai.HasValue)
            {
                hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
            }
            if (hang != null)
            {
                hangHoas = hangHoas.Where(p => p.MaNcc == hang);
            }
            var result = hangHoas.Select(p => new HangHoaVM
            {
                MaHH = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai,
                GiamGia = p.GiamGia,
                MaNCC = p.MaNccNavigation.TenCongTy,
                ML = p.MaLoai,
                NCC = p.MaNcc,
            });

            int totalItems = result.Count();
            decimal totalPages = Math.Ceiling((decimal)((decimal)totalItems / pagesize));
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.Loai = loai; // Truyền tham số "loại" vào ViewBag
            ViewBag.hang = hang;

            return View(result.ToPagedList((int)page, (int)pagesize));
        }


        [HttpGet]
        public IActionResult Search(string? query)
        {
            var hangHoas = db.HangHoas.AsQueryable();
            if (query != null)
            {
                hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
            }
            var result = hangHoas.Select(p => new HangHoaVM
            {
                MaHH = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai

            });
            return View(result);
        }
        public IActionResult Detail(int id)
        {
            var data = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .Include(p => p.MaNccNavigation)
                .SingleOrDefault(p => p.MaHh == id);

            if (data == null)
            {
                TempData["Message"] = $"Không tìm thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }

            data.SoLanXem += 1;
            db.SaveChanges();

            var imageUrls = new List<string>();
            if (!string.IsNullOrEmpty(data.Hinh))
            {
                imageUrls = data.Hinh.Split(',').ToList();
            }

            var result = new HangHoaVM
            {
                MaHH = data.MaHh,
                TenHH = data.TenHh,
                MoTa = data.MoTa,
                MaNCC = data.MaNccNavigation != null ? data.MaNccNavigation.TenCongTy : "", // Kiểm tra xem MaNccNavigation có khác null không
                DonGia = data.DonGia ?? 0,
                Hinh = data.Hinh ?? "",
                MoTaNgan = data.MoTaDonVi ?? "",
                TenLoai = data.MaLoaiNavigation.TenLoai,
                ML = data.MaLoai,
                NCC = data.MaNcc,
                SoLuong = 10,
                DiemDanhGia = 5,
                ImageUrls = imageUrls

            };
            //var database = db.DanhGia
            //    //.Include(p => p.HangHoaNavigation)
            //    .Include(p => p.MaKhNavigation)
            //    .SingleOrDefault(p => p.MaHh == id);
            //var danhgia = new DanhGiaVM()
            //{
            //    MaKH = database.MaKhNavigation.TenKhachHang,
            //    NoiDung = database.NoiDung,
            //    DanhGia = database.DanhGia,
            //    Ngay = database.Ngay,
            //};
            return View(result);
        }
    }
}
