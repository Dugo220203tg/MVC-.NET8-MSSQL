using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Controllers
{
    public class WishListController : Controller
    {
        private readonly Hshop2023Context db;
        public WishListController(Hshop2023Context context)
        {
            db = context;
        }

        public async Task<IActionResult> Index()
        {
            var yeuthich = await db.YeuThiches
                .Select(p => new WishListVM
                {
                    MaYT = p.MaYt,
                    MaHH = (int)p.MaHh,
                    TenHH = p.MaHhNavigation.TenHh,
                    DonGia = (double)p.MaHhNavigation.DonGia,
                    Hinh = p.MaHhNavigation.Hinh
                })
                .ToListAsync();
            return Json(yeuthich);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishList(int MaHH)
        {
            var userId = User.Identity.Name; // Assuming the user is authenticated
            var existingItem = await db.YeuThiches.FirstOrDefaultAsync(y => y.MaHh == MaHH && y.MaKh == userId);

            if (existingItem != null)
            {
                return Json(new { success = false, message = "Product is already in wishlist" });
            }

            var yeuthich = new YeuThich
            {
                MaHh = MaHH,
                MaKh = userId,
                NgayChon = DateTime.Now,
            };

            db.Add(yeuthich);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Product added to wishlist" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWishList(int id)
        {
            try
            {
                Console.WriteLine($"Received ID: {id}"); // Log received ID

                var yeuthichremove = await db.YeuThiches.FirstOrDefaultAsync(p => p.MaYt == id);
                if (yeuthichremove == null)
                {
                    Console.WriteLine("Product not found in wishlist"); // Log if not found
                    return Json(new { success = false, message = "Product not found in wishlist" });
                }

                db.YeuThiches.Remove(yeuthichremove);
                await db.SaveChangesAsync();
                Console.WriteLine("Product removed from wishlist"); // Log successful removal
                return Json(new { success = true, message = "Product removed from wishlist" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing product from wishlist: {ex.Message}"); // Log the exception
                Console.WriteLine($"Stack Trace: {ex.StackTrace}"); // Log the stack trace for more details
                return StatusCode(500, new { success = false, message = "An error occurred while removing the product from the wishlist.", details = ex.Message });
            }
        }

    }
}
