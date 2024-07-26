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
        public IActionResult Index()
        {
            var yeuthich = db.YeuThiches.AsQueryable();
            var result = yeuthich.Select(p => new WishListVM
            {
                MaYT = p.MaYt,
                MaHH = (int)p.MaHh,
                NgayChon = DateTime.Now,
            });
            return Json(result);
        }
        public async Task<IActionResult> AddToWishList(WishListVM model)
        {
            var yeuthich = new YeuThich
            {
                MaYt = model.MaYT,
                MaHh = model.MaHH,
                MaKh = User.Identity.Name,
                NgayChon = DateTime.Now,
            };
            db.Add(yeuthich);
            db.SaveChanges();
            return Json(yeuthich);
        }
        public async Task<IActionResult> RemoveWishList(int id)
        {
            var yeuthichremove = await db.YeuThiches.FirstOrDefaultAsync(p => p.MaYt == id);
            if (yeuthichremove != null)
            {
                return null;
            }
            db.YeuThiches.Remove(yeuthichremove);
            await db.SaveChangesAsync();
            return Json(yeuthichremove);
        }
    }
}
