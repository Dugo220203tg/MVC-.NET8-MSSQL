using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TDProjectMVC.Models;


namespace TDProjectMVC.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly Hshop2023Context db;

        private readonly IMapper _mapper;

        public KhachHangController(Hshop2023Context context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }
        #region ---Login---
        [HttpGet]
        public IActionResult DangKy()
        {
            return View(new RegisterVM());
        }
        [HttpPost]
        public async Task<IActionResult> DangKyAsync(RegisterVM model, IFormFile Hinh)
        {
            //if (ModelState.IsValid)
            //{
            try
            {
                var khachHangexit = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == model.MaKh);

                if (khachHangexit != null)
                {
                    ModelState.AddModelError("loi", "Username da duoc su dung");
                }
                else
                {
                    var khachHang = _mapper.Map<KhachHang>(model);
                    khachHang.RandomKey = MyUtil.GenerateRamdomKey();
                    khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
                    khachHang.HieuLuc = true; //sẽ xử lý khi dùng Mail để active
                    khachHang.VaiTro = 0;

                    if (Hinh != null)
                    {
                        khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                    }

                    db.Add(khachHang);
                    await db.SaveChangesAsync();

                    return RedirectToAction("DangNhap", "KhachHang");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Loi", ex.Message);
            }
            //}
            return View(model);
        }

        #endregion

        #region ---DangNhap---
        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                var khachHang = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == model.UserName);
                if (khachHang == null)
                {
                    ModelState.AddModelError("Lỗi", "Không có khách hàng này");
                }
                else
                {
                    if (khachHang.VaiTro == 1)
                    {
                        ModelState.AddModelError("Lỗi ", "Sai quyền đăng nhập");

                    }
                    else
                    if (!khachHang.HieuLuc)
                    {
                        ModelState.AddModelError("Lỗi", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
                    }
                    else
                    {
                        if (khachHang.MatKhau != model.Password.ToMd5Hash(khachHang.RandomKey))
                        {
                            ModelState.AddModelError("Lỗi", "Sai thông tin đăng nhập");
                        }
                        else
                        {
                            var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, khachHang.Email),
                                new Claim(ClaimTypes.Name, khachHang.HoTen),
                                new Claim("CustomerID", khachHang.MaKh),

								//claim - role động
								new Claim(ClaimTypes.Role, "Customer")
                            };
                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimsPrincipal);

                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                }
            }
            return View();
        }
        #endregion

        #region ---DangXuat---
        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
        #endregion

        #region ---Chuc Nang---

        [Authorize]
        public IActionResult Profile()
        {
            // Lấy mã khách hàng từ claim 'CustomerID'
            var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;

            if (customerId != null)
            {
                // Thực hiện truy vấn để lấy thông tin khách hàng từ cơ sở dữ liệu
                var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKh == customerId);

                if (khachHang != null)
                {
                    // Đã tìm thấy thông tin khách hàng, bạn có thể sử dụng nó để hiển thị trên trang web
                    ViewBag.CustomerName = khachHang.HoTen;
                    ViewBag.CustomerDienThoai = khachHang.DienThoai;
                    ViewBag.CustomerEmail = khachHang.Email;
                    ViewBag.CustomerAddress = khachHang.DiaChi;
                    // Bạn có thể thêm các thông tin khác tùy ý từ đối tượng khachHang.

                    return View();
                }
            }
            // Nếu không tìm thấy thông tin khách hàng, bạn có thể xử lý theo ý của mình, ví dụ: hiển thị thông báo lỗi.
            ViewBag.ErrorMessage = "Không tìm thấy thông tin khách hàng.";
            return View("Error"); // Thay thế "Error" bằng tên View chứa trang thông báo lỗi của bạn.
        }
        [Authorize]
        [HttpPost]
        public IActionResult UpdateProfile(string customerName, string customerEmail, string customerAddress)
        {
            // Lấy mã khách hàng từ claim 'CustomerID'
            var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;

            if (customerId != null)
            {
                // Thực hiện truy vấn để lấy thông tin khách hàng từ cơ sở dữ liệu
                var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKh == customerId);

                if (khachHang != null)
                {
                    // Cập nhật thông tin khách hàng
                    khachHang.HoTen = customerName;
                    khachHang.Email = customerEmail;
                    khachHang.DiaChi = customerAddress;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    // Chuyển hướng người dùng đến trang Profile sau khi cập nhật thành công
                    return RedirectToAction("Profile");
                }
            }

            // Nếu không tìm thấy thông tin khách hàng, bạn có thể xử lý theo ý của mình, ví dụ: hiển thị thông báo lỗi.
            ViewBag.ErrorMessage = "Không tìm thấy thông tin khách hàng.";
            return View("Error"); // Thay thế "Error" bằng tên View chứa trang thông báo lỗi của bạn.
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(int MaHH, string noiDung, int sao)
        {
            if (sao < 1 || sao > 5)
            {
                return Json(new { success = false, message = "Invalid rating value." });
            }

            if (string.IsNullOrWhiteSpace(noiDung))
            {
                return Json(new { success = false, message = "Comment content cannot be empty." });
            }

            try
            {
                var userId = User.Identity.Name; // Assuming the user is authenticated and Name is used as the user ID

                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var danhgia = new DanhGiaSp
                {
                    MaHh = MaHH,
                    MaKh = userId,
                    Ngay = DateTime.Now,
                    NoiDung = noiDung,
                    Sao = sao,
                    TrangThai = 1 // Assuming this means active
                };

                db.Add(danhgia);
                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Comment submitted successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception here using your logging mechanism (like Serilog, NLog, etc.)
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #endregion
    }
}
