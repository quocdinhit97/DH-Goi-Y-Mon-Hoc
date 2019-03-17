using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DATN.Models;
using ASPSnippets.GoogleAPI;
using System.Web.Script.Serialization;

namespace DATN.Controllers
{
    public class LoginController : Controller
    {
		DataTVUDataContext db = new DataTVUDataContext();
		public static string lop = "";
        // GET: Login
        public ActionResult Index()
        {
			ViewBag.Lop = new SelectList(db.LOPs.ToList(), "Ma_Lop", "Ten_Lop");
			return View();
        }


		[HttpPost]
		[ValidateAntiForgeryToken]
		public void LoginWithGoogle(ClassTemp _lop)
		{
			lop = _lop.lops;
			GoogleConnect.ClientId = "520808312902-8b27bsq7b88uvjvse8da3svlbue2sb1v.apps.googleusercontent.com";
			GoogleConnect.ClientSecret = "frdbgU0wC677SMYGcxF2zmmP";
			GoogleConnect.RedirectUri = Request.Url.AbsoluteUri.Split('?')[0];
			GoogleConnect.Authorize("profile", "email");
		}

		[ActionName("LoginWithGoogle")]
		public ActionResult LoginWithGooglePlusConfirmed()
		{
			
			if (!string.IsNullOrEmpty(Request.QueryString["code"]))
            {
				string Name;
				string code = Request.QueryString["code"];
                string json = GoogleConnect.Fetch("me", code);
                GoogleProfile profile = new JavaScriptSerializer().Deserialize<GoogleProfile>(json);

				//Kiễm tra gmail sinh viên
				string Email = profile.Emails.Find(email => email.Type == "account").Value;
				string ExtensionEmail = Email.Substring(Email.IndexOf("@"));
				Email = Email.Substring(0, Email.IndexOf("@"));
				Name = profile.DisplayName;
				string avata = profile.Image.Url;
				if (ExtensionEmail != "@sv.tvu.edu.vn")
				{
					TempData["Login_ERR"] = "<label style='color: #ff4a4a'>Vui lòng dùng gmail sinh viên.</label>";
					return RedirectToAction("index", "Login");
				}
				else if(db.SINHVIENs.Where(x => x.Ma_SV == Email).Count() > 0)
				{
					var SV = db.SINHVIENs.Where(x => x.Ma_SV == Email).FirstOrDefault();
					Session["lop"] = SV.Ma_Lop;
					Session["email"] = Email;
					Session["name"] = SV.Ten_SV;
					Session["avata"] = avata;
					Session["diachi"] = SV.Dia_Chi;
					Session["sdt"] = SV.SDT;
					Session["listLop"] = new SelectList(db.LOPs.ToList(), "Ma_Lop", "Ten_Lop", SV.Ma_Lop.ToString());
				}
				else
				{
					SINHVIEN _SV = new SINHVIEN();
					_SV.Ma_Lop = lop;
					_SV.Ma_SV = Email;
					_SV.Ten_SV = Name;
					Session["lop"] = lop;
					Session["email"] = Email;
					Session["name"] = Name;
					Session["diachi"] = "";
					Session["sdt"] = "";
					Session["avata"] = avata;
					db.SINHVIENs.InsertOnSubmit(_SV);
					db.SubmitChanges();
				}
				

            }
            if (Request.QueryString["error"] == "access_denied")
            {
                return Content("access_denied");
            }
            return RedirectToAction("index", "Homes");
		}

		public class GoogleProfile
		{
			public string Id { get; set; }
			public string DisplayName { get; set; }
			public Image Image { get; set; }
			public List<Email> Emails { get; set; }
			public string Gender { get; set; }
			public string ObjectType { get; set; }
		}

		public class Email
		{
			public string Value { get; set; }
			public string Type { get; set; }
		}

		public class Image
		{
			public string Url { get; set; }
		}
	}
}