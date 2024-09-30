using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.Services;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IVnPayService _vnPayservice;


        public CartController(Hshop2023Context context, IVnPayService vnPayservice)
        {
            db = context;
            _vnPayservice = vnPayservice;

        }
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public IActionResult Index()
        {
            return View(Cart);
        }
        #region Thêm cart
        [HttpPost]
        public IActionResult AddToCart(int id, int quantity = 1, string type = "Normal")
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == id);

            if (item == null)
            {
                var hanghoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hanghoa == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy hàng hóa có mã {id}" });
                }

                item = new CartItem
                {
                    MaHH = hanghoa.MaHh,
                    TenHH = hanghoa.TenHh,
                    DonGia = hanghoa.DonGia ?? 0,
                    Hinh = hanghoa.Hinh ?? String.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            return Json(new { success = true, cartCount = gioHang.Sum(i => i.SoLuong) });
        }
        //Hiển thị dữ liệu trong giỏ hàng
        public IActionResult GetCartData()
        {
            var cartData = new
            {
                CardProducts = Cart.Select(p => new
                {
                    p.MaHH,
                    p.TenHH,
                    p.SoLuong,
                    p.DonGia,
                    Hinh = p.Hinh?.Split(',').FirstOrDefault()?.Trim() ?? ""
                }),
                TotalQuantity = Cart.Sum(p => p.SoLuong),
                TotalAmount = Cart.Sum(p => p.SoLuong * p.DonGia)
            };
            return Json(cartData);
        }
        //public IActionResult RemoveCart(int id)
        //{
        //    var gioHang = Cart;
        //    var item = gioHang.SingleOrDefault(p => p.MaHH == id);
        //    if (item != null)
        //    {
        //        gioHang.Remove(item);
        //        HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
        //    }
        //    return RedirectToAction("Index");
        //}
        public JsonResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }
        }

        public IActionResult IncreaseQuantity(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == id);
            if (item != null)
            {
                item.SoLuong++; // Tăng số lượng lên 1 đơn vị
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }
        public IActionResult MinusQuantity(int id)
        {
            var gioHang = Cart; // Giả sử Cart là một thuộc tính hoặc một biến mà bạn đã khai báo ở nơi khác trong controller
            var item = gioHang.SingleOrDefault(p => p.MaHH == id); // Tìm kiếm sản phẩm trong giỏ hàng với id đã được truyền vào
            if (item != null)
            {
                item.SoLuong--; // Giảm số lượng đi 1 đơn vị
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang); // Lưu lại giỏ hàng vào session
            }
            return RedirectToAction("Index"); // Chuyển hướng người dùng đến trang Index
        }

        #endregion
        #region Thanh Toan (checkOut)
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart.Count == 0)
            {
                return Redirect("/");
            }

            return View(Cart);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckOutVM model, string payment = "COD")
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = Cart.Sum(p => p.ThanhTien),
                        CreatedDate = DateTime.Now,
                        Description = $"{model.HoTen} {model.DienThoai}",
                        FullName = model.HoTen,
                        OrderId = new Random().Next(1000, 100000)
                    };
                    return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
                }

                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
                var khachHang = new KhachHang();
                if (model.GiongKhachHang)
                {
                    khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
                }
                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi = model.DiaChi ?? khachHang.DiaChi,
                    DienThoai = model.DienThoai ?? khachHang.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "GRAB",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };
                db.Database.BeginTransaction();
                try
                {
                    db.Add(hoadon);
                    db.SaveChanges();
                    var cthds = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        cthds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            MaHh = item.MaHH,
                            DonGia = item.DonGia,
                            SoLuong = item.SoLuong,
                            MaGiamGia = item.GiamGia,
                        });
                    }
                    db.AddRange(cthds);
                    db.SaveChanges();
                    db.Database.CommitTransaction();
                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                    return View("Success");
                }
                catch
                {
                    db.Database.RollbackTransaction();
                }
            }
            return View(Cart);
        }
        #endregion
        //#region Paypal payment
        //[Authorize]
        //[HttpPost("/Cart/create-paypal-order")]
        //public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        //{
        //	// Thông tin đơn hàng gửi qua Paypal
        //	var tongTien = Cart.Sum(p => p.ThanhTien).ToString();
        //	var donViTienTe = "USD";
        //	var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

        //	try
        //	{
        //		var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

        //		return Ok(response);
        //	}
        //	catch (Exception ex)
        //	{
        //		var error = new { ex.GetBaseException().Message };
        //		return BadRequest(error);
        //	}
        //}

        //[Authorize]
        //[HttpPost("/Cart/capture-paypal-order")]
        //public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        //{
        //	try
        //	{
        //		var response = await _paypalClient.CaptureOrder(orderID);

        //		// Lưu database đơn hàng của mình

        //		return Ok(response);
        //	}
        //	catch (Exception ex)
        //	{
        //		var error = new { ex.GetBaseException().Message };
        //		return BadRequest(error);
        //	}
        //}

        //#endregion
        public IActionResult PaymentFail()
        {
            return View();
        }
        public IActionResult PaymentSuccess()
        {
            return View("Success");

        }

        public IActionResult VNPayCallBack(CheckOutVM model)
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            var khachHang = new KhachHang();
            // Lấy thông tin từ dữ liệu trả về của VNPay
            var orderId = response.OrderId;
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;

            // Lưu đơn hàng vào cơ sở dữ liệu
            var hoadon = new HoaDon
            {
                MaKh = customerId,
                HoTen = model.HoTen ?? khachHang.HoTen,
                DiaChi = model.DiaChi ?? khachHang.DiaChi,
                DienThoai = model.DienThoai ?? khachHang.DienThoai,
                NgayDat = DateTime.Now,
                CachThanhToan = "VnPay",
                CachVanChuyen = "VnExpress",
                MaTrangThai = 0,
                GhiChu = model.GhiChu
            };

            // Tiến hành lưu đơn hàng và chi tiết đơn hàng vào cơ sở dữ liệu trong một giao dịch
            db.Database.BeginTransaction();
            try
            {
                db.Add(hoadon);
                db.SaveChanges(); // Lưu thông tin đơn hàng

                // Lưu thông tin chi tiết đơn hàng
                var cthds = new List<ChiTietHd>();
                foreach (var item in Cart)
                {
                    cthds.Add(new ChiTietHd
                    {
                        MaHd = hoadon.MaHd,
                        MaHh = item.MaHH,
                        DonGia = item.DonGia,
                        SoLuong = item.SoLuong,
                        MaGiamGia = item.GiamGia,
                    });
                }
                db.AddRange(cthds);
                db.SaveChanges(); // Lưu thông tin chi tiết đơn hàng

                db.Database.CommitTransaction(); // Xác nhận giao dịch

                // Xóa giỏ hàng trong session
                TempData["Message"] = $"Thanh toán VNPay thành công";
                HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                return View("Success");
            }
            catch (Exception ex)
            {
                db.Database.RollbackTransaction(); // Hoàn tác giao dịch nếu có lỗi
                TempData["Message"] = $"Lỗi khi lưu đơn hàng: {ex.Message}";
                return RedirectToAction("PaymentFail");
            }
        }
    }
}

