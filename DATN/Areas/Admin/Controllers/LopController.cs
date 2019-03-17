using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DATN.Areas.Admin.Models;
using DATN.Models;
using Newtonsoft.Json;

namespace DATN.Areas.Admin.Controllers
{
    public class LopController : Controller
    {
		DataTVUDataContext db = new DataTVUDataContext();
		// GET: Admin/Lop
		public ActionResult Index()
        {
			var ListBat = new List<Bat>
			{
				new Bat{ maBat = "DH", tenBat = "Đại học"},
				new Bat{ maBat = "CA", tenBat = "Cao đẳng"},
			};
			ViewBag.ListBat = new SelectList(ListBat, "maBat", "tenBat");
			ViewBag.ListCTDT = new SelectList(db.CTDTs.ToList(), "Ma_CTDT", "Ten_CTDT");
			ViewBag.ListLop = db.LOPs.ToList();
			return View();
        }

		[HttpPost]
		public ActionResult AddLop(LOP _lops)
		{
			if (_lops.Ten_Lop == null)
			{
				TempData["LOP_ERR"] = "Tên lớp không được bỏ trống.";
				return RedirectToAction("index", "Lop");
			}
			var KT_Ma_Lop = db.LOPs.Where(x => x.Ma_Lop == _lops.Ma_Lop).FirstOrDefault();
			if (KT_Ma_Lop != null)
			{
				TempData["LOP_ERR"] = "Tên Lớp đã tồn tại.";
				return RedirectToAction("index", "Lop");
			}
			else
			{
				LOP _lop = new LOP();
				_lop.Ma_Lop = _lops.Ma_Lop;
				_lop.Ten_Lop = _lops.Ten_Lop;
				_lop.Bat = _lops.Bat;
				_lop.Ma_CTDT = _lops.Ma_CTDT;

				db.LOPs.InsertOnSubmit(_lops);
				db.SubmitChanges();
				TempData["LOP_SUCCESS"] = "Thêm thành công.";
			}
			return RedirectToAction("index", "Lop");
		}


		public JsonResult showLop(string idLop)
		{
			show_Lop _lop = db.LOPs.Where(x => x.Ma_Lop == idLop).Select(x => new show_Lop { _ma_lop = x.Ma_Lop, _ten_lop = x.Ten_Lop, _bat = x.Bat, _CTDT = x.Ma_CTDT }).FirstOrDefault();
			string value = string.Empty;

			value = JsonConvert.SerializeObject(_lop, Formatting.Indented, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			});


			return Json(value, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Xem_CTDT()
		{
			string _Ma_CTDT = Url.RequestContext.RouteData.Values["id"].ToString();
			ViewBag.CTDT = _Ma_CTDT;
			ViewBag.DetailLop = db.MONs.Where(x => x.Ma_CTDT == _Ma_CTDT).ToList();
			return View("Details");
		}


		public ActionResult EditLop(LOP _lops)
		{
			var _lop = db.LOPs.Where(x => x.Ma_Lop == _lops.Ma_Lop).FirstOrDefault();
			_lop.Ten_Lop = _lops.Ten_Lop;
			_lop.Bat = _lops.Bat;
			_lop.Ma_CTDT = _lops.Ma_CTDT;
			db.SubmitChanges();
			TempData["LOP_SUCCESS"] = "Sửa thành công.";
			return RedirectToAction("index", "Lop");
		}

		public ActionResult GetSinhVien()
		{
			string _Ma_Lop = Url.RequestContext.RouteData.Values["id"].ToString();
			ViewBag.GetSinhVien = db.SINHVIENs.Where(x => x.Ma_Lop == _Ma_Lop).ToList();
			ViewBag.MaLop = _Ma_Lop;
			return View("SinhVien");
		}

		public ActionResult Xem_Diem()
		{
			string _Ma_SV = Url.RequestContext.RouteData.Values["id"].ToString();
			ViewBag.GetDiem = db.DIEMs.Where(x => x.Ma_SV == _Ma_SV).Select(x => new Mons { MaMon = x.MON.Ma_Mon, TenMon = x.MON.Ten_Mon, HocKi = x.MON.HOCKI.Ma_HK, TinChi = x.MON.So_TC, ThiL1 = x.Thi_L1, ThiL2 = x.Thi_L2, ThiL3 = x.Thi_L3, TK10 = x.TK10, TKCH = x.TKCH, KQ1 = x.KQ1, KQ = x.KQ, GhiChu = x.Ghi_Chu }).ToList();
			ViewBag.MaSV = _Ma_SV;
			return View("Diem");
		}
	}
}