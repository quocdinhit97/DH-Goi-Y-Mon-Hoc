using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DATN.Areas.Admin.Controllers
{
    public class AdminLoginController : Controller
    {
		DataTVUDataContext db = new DataTVUDataContext();

        // GET: Admin/AdminLogin
        public ActionResult Index()
        {
			Session.RemoveAll();
            return View("index");
        }

		public ActionResult Login(Account acc)
		{
			var AccountLogin = db.Accounts.Where(x => x.Email == acc.Email && x.Pass == acc.Pass).FirstOrDefault();
			if(AccountLogin == null)
			{
				TempData["Login_ERR"] = "Tài khoản hoặc mật khẩu không đúng.";
				return RedirectToAction("index", "AdminLogin");
			}

			Session["email"] = AccountLogin.Email;
			Session["name"] = AccountLogin.Name;
			Session["Role"] = AccountLogin.Roles;

			return RedirectToAction("index", "Home");
		}

		public ActionResult Logout()
		{
			Session.RemoveAll();
			return RedirectToAction("index", "AdminLogin");
		}
    }
}