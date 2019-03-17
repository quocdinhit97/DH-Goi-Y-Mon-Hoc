using DATN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DATN.Areas.Admin.Controllers
{
    public class DiemController : Controller
    {
		DataTVUDataContext db = new DataTVUDataContext();
        // GET: Admin/Diem
        public ActionResult Index()
        {
			ViewBag.Diem = db.DIEMs.Select(x => new DanhSachDiem { MaMon = x.MON.Ma_Mon, MaSV = x.SINHVIEN.Ma_SV,Lop = x.SINHVIEN.LOP.Ma_Lop, TenMon = x.MON.Ten_Mon, HocKi = x.MON.HOCKI.Ma_HK, TinChi = x.MON.So_TC, ThiL1 = x.Thi_L1, ThiL2 = x.Thi_L2, ThiL3 = x.Thi_L3, TK10 = x.TK10, TKCH = x.TKCH, KQ1 = x.KQ1, KQ = x.KQ, GhiChu = x.Ghi_Chu }).ToList();
			return View();
        }
    }
}