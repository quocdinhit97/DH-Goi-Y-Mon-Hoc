using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DATN.Models;
using Excel = Microsoft.Office.Interop.Excel;
namespace DATN.Controllers
{
	public class HomesController : Controller
	{
		DataTVUDataContext db = new DataTVUDataContext();
		// GET: Homes
		public ActionResult demo()
		{
			return View("demo");
		}
		public ActionResult Index()
		{
			if(Session["email"] == null)
			{
				return RedirectToAction("index", "Login");
			}

			string MaSV = Session["email"].ToString();

			List<int> TK_Diem = new List<int>();
			List<string> LB = new List<string>();

			List<string> lb2 = new List<string>();
			List<int> diem = new List<int>();

			var t = db.DIEMs.Where(x=>x.Ma_SV == MaSV).GroupBy(x => x.TKCH).Select(x => new TK_Diem { Dlable = x.Key, Dcount = x.Select(y => y.TKCH).Count() });
			var t2 = db.DIEMs.Where(x => x.Ma_SV == MaSV).GroupBy(x => x.KQ).Select(x => new TK_CD { lb = x.Key, diem = x.Select(y => y.KQ).Count() });

			if(t.Count() > 0)
			{
				foreach (var item in t)
				{
					if (item.Dlable.Trim() == "")
					{
						item.Dlable = "Chưa đạt";
					}
					TK_Diem.Add(item.Dcount);
					LB.Add(item.Dlable);
				}

				foreach (var item2 in t2)
				{
					if (item2.lb == "X")
					{
						item2.lb = "Chưa đạt";
					}

					lb2.Add(item2.lb);
					diem.Add(item2.diem);
				}


				ViewBag.lable = LB;
				ViewBag.count = TK_Diem;

				ViewBag.lb = lb2;
				ViewBag.diem = diem;
			}
			else
			{
				TempData["Diem_ERR"] = "Bạn chưa cập nhật bản điểm";
			}

			return View();
		}

		//View ctdt
		public ActionResult CTDT()
		{
			if (Session["lop"] == null)
			{
				return RedirectToAction("index", "login");
			}
			string MaLop = Session["lop"].ToString();
			string Ma_CTDT = db.LOPs.Where(x => x.Ma_Lop == MaLop).Select(x => x.Ma_CTDT).FirstOrDefault();
			ViewBag.CTDT = db.MONs.Where(x => x.Ma_CTDT == Ma_CTDT).ToList();
			return View();
		}

		//View điểm
		public ActionResult Diem()
		{
			if(Session["email"] == null)
			{
				return RedirectToAction("index", "login");
			}
			string maSV = Session["email"].ToString();
			ViewBag.Diem = db.DIEMs.Where(x => x.Ma_SV == maSV).Select(x => new Mons {ID_Mon= x.ID_Mon ,MaMon = x.MON.Ma_Mon, TenMon = x.MON.Ten_Mon, HocKi = x.MON.HOCKI.Ma_HK, TinChi = x.MON.So_TC, ThiL1 = x.Thi_L1, ThiL2 = x.Thi_L2, ThiL3 = x.Thi_L3, TK10 = x.TK10, TKCH = x.TKCH, KQ1 = x.KQ1, KQ = x.KQ, GhiChu = x.Ghi_Chu }).ToList();
			return View();
		}

		[HttpPost]
		public ActionResult EditProfile(SINHVIEN SV)
		{
			var _SV = db.SINHVIENs.Where(x => x.Ma_SV == Session["email"].ToString()).FirstOrDefault();

			_SV.Ten_SV = SV.Ten_SV;
			_SV.Dia_Chi = SV.Dia_Chi;
			_SV.SDT = SV.SDT;
			_SV.Ma_Lop = SV.Ma_Lop;

			db.SubmitChanges();
			Session["diachi"] = SV.Dia_Chi;
			Session["sdt"] = SV.SDT;
			Session["name"] = SV.Ten_SV;
			Session["lop"] = SV.Ma_Lop;
			return RedirectToAction("index", "Homes");
		}
		public ActionResult Profile()
		{
			return View();
		}
		//THêm điểm
		[HttpPost]
		public ActionResult Create()
		{
			HttpPostedFileBase excelfile = Request.Files["excelfile"];
			if (excelfile.ContentLength == 0 || excelfile == null)
			{
				TempData["file"] = "<label style='color: #ff4a4a'>Bạn chưa chọn file điểm.</label>";
				return RedirectToAction("index", "Homes");
			}
			else
			{
				if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
				{
					string path = Server.MapPath("~/Content/" + excelfile.FileName);
					if (System.IO.File.Exists(path))
					{
						System.IO.File.Delete(path);
					}
					excelfile.SaveAs(path);
					Session["path"] = path;
					//read data from excel file

					ReadExcel(path);

				}
				else
				{
					TempData["file"] = "<label style='color: #ff4a4a'>File không đúng định dạng</label>";
				}
			}

			return RedirectToAction("index", "Homes");
		}

		//Đăng xuất
		public ActionResult Logout()
		{
			Session.RemoveAll();
			return RedirectToAction("index","Login");
		}


		//Tìm kiếm
		public ActionResult Search()
		{
			string maMon = Request["search"].ToString();
			ViewBag.Lich = db.Liches.Where(x => x.Ma_MH == maMon).ToList();
			ViewBag.Friend  = db.DIEMs.Where(x => x.MON.Ma_Mon == maMon && x.TKCH == "D" && x.Ma_SV != Session["email"].ToString() || ( x.MON.Ma_Mon == maMon && x.TKCH == "F" && x.Ma_SV != Session["email"].ToString())).Select(x => new TimBanHoc { Ma_SV = x.SINHVIEN.Ma_SV, Ma_Mon = x.MON.Ma_Mon, Ho_Ten = x.SINHVIEN.Ten_SV, Lop = x.SINHVIEN.Ma_Lop, SDT = x.SINHVIEN.SDT, Diem_KT = x.TKCH }).ToList();
			ViewBag.search = Request["search"];
			return View("Search");
		}

		//Đọc excel
		public void ReadExcel(string path)
		{
			double l1 = 0;
			double l2 = 0;
			double l3 = 0;
			double he10 = 0;
			int ID = 0;

			string MaSV = Session["email"].ToString();
			while (true)
			{
				var temp = db.DIEMs.Where(x => x.Ma_SV == MaSV).FirstOrDefault();
				if (temp == null)
				{
					break;
				}

				db.DIEMs.DeleteOnSubmit(temp);
				db.SubmitChanges();	
			}
			

			Excel.Application application = new Excel.Application();
			Excel.Workbook workbook = application.Workbooks.Open(path);
			Excel.Worksheet worksheet = workbook.ActiveSheet;
			Excel.Range range = worksheet.UsedRange;


			try
			{
				for (int row = 3; row <= range.Rows.Count; row++)
				{

					if (((Excel.Range)range.Cells[row, 11]).Text != "")
					{
						string MaLop = Session["lop"].ToString();
						string Ma_CTDT = db.LOPs.Where(x => x.Ma_Lop == MaLop).Select(x => x.Ma_CTDT).FirstOrDefault();
						string mamon = ((Excel.Range)range.Cells[row, 2]).Text;

						ID = db.MONs.Where(x => x.Ma_Mon == mamon && x.Ma_CTDT == Ma_CTDT).Select(x => x.ID_Mon).FirstOrDefault();
						//Kiễm tra môn có nằm trong chương trình đào tạo không
						// Nếu k có bỏ qua (id == 0)
						if (ID == 0)
						{
							continue;
						}


						DIEM _scores = new DIEM();
						_scores.ID_Mon = ID;
						_scores.PT_KT = Convert.ToInt32(((Excel.Range)range.Cells[row, 5]).Text);
						_scores.PT_Thi = Convert.ToInt32(((Excel.Range)range.Cells[row, 6]).Text);

						double.TryParse(((Excel.Range)range.Cells[row, 7]).Text, out l1);
						_scores.Thi_L1 = l1;

						double.TryParse(((Excel.Range)range.Cells[row, 8]).Text, out l2);
						_scores.Thi_L2 = l2;

						double.TryParse(((Excel.Range)range.Cells[row, 9]).Text, out l3);
						_scores.Thi_L3 = l3;

						double.TryParse(((Excel.Range)range.Cells[row, 10]).Text, out he10);
						_scores.TK10 = he10;

						_scores.TKCH = ((Excel.Range)range.Cells[row, 11]).Text;
						_scores.KQ1 = ((Excel.Range)range.Cells[row, 12]).Text;
						_scores.KQ = ((Excel.Range)range.Cells[row, 13]).Text;
						_scores.Ghi_Chu = ((Excel.Range)range.Cells[row, 14]).Text;
						_scores.Ma_SV = MaSV;

						db.DIEMs.InsertOnSubmit(_scores);
						db.SubmitChanges();

						// xóa bỏ những môn học lại và đã đạt
						LocDiemTrung(ID, he10);
					}

				}

				workbook.Close(false);

				application.Quit();

			}
			catch (Exception)
			{
				TempData["file"] = "<label style='color: #ff4a4a'>Không thể đọc File.</label>";
				Response.Redirect("/Home/index");
			}

		}



		//Lọc điểm trùng
		void LocDiemTrung(int IdMon, double kq)
		{
			string maSV = Session["email"].ToString();
			int idDiem = db.DIEMs.Where(x => x.Ma_SV == maSV).Max(x => x.ID_Diem);

			//lấy ra danh sách điểm của sinh viên
			var DanhSachDiem = db.DIEMs.Where(x => x.Ma_SV == maSV).ToList();

			foreach (var diem in DanhSachDiem)
			{
				if(diem.ID_Diem != idDiem)
				{
					if (diem.ID_Mon == IdMon && diem.TK10 < kq)
					{
						//var DiemNho = db.DIEMs.Where(x => x.ID_Mon == diem.ID_Mon).FirstOrDefault();
						db.DIEMs.DeleteOnSubmit(diem);
						db.SubmitChanges();
					}
					else if(diem.ID_Mon == IdMon && diem.TK10 > kq)
					{
						var DiemNho = db.DIEMs.Where(x => x.ID_Diem == idDiem).FirstOrDefault();
						db.DIEMs.DeleteOnSubmit(DiemNho);
						db.SubmitChanges();
					}

					if(diem.ID_Mon == IdMon && diem.TK10 == kq)
					{
						if(diem.TKCH != "")
						{
							var DiemNho = db.DIEMs.Where(x => x.ID_Diem == idDiem).FirstOrDefault();
							db.DIEMs.DeleteOnSubmit(DiemNho);
							db.SubmitChanges();
						}
						else
						{
							db.DIEMs.DeleteOnSubmit(diem);
							db.SubmitChanges();
						}
					}
				}
			}
		}
	}
}