using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using DATN.Models;

namespace DATN.Areas.Admin.Controllers
{
    public class LichController : Controller
    {
		DataTVUDataContext db = new DataTVUDataContext();
        // GET: Admin/Lich
        public ActionResult Index()
        {
			ViewBag.Lich = db.Liches.ToList();
            return View();
        }

		public ActionResult Upload()
		{
			HttpPostedFileBase excelfile = Request.Files["excelfilelich"];
			if (excelfile.ContentLength == 0 || excelfile == null)
			{
				TempData["file_lich"] = "Bạn chưa chọn file điểm.";
				return RedirectToAction("index", "Lich");
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

					ReadExcelLich(path);

				}
				else
				{
					TempData["file_lich"] = "File không đúng định dạng.";
					return RedirectToAction("index", "Lich");
				}
			}
			return RedirectToAction("index", "Lich");
		}

		void ReadExcelLich(string path)
		{
			//Xóa toàn bộ lịch
			string query = "DELETE FROM Lich";
			db.ExecuteCommand(query);
			db.SubmitChanges();

			Excel.Application application = new Excel.Application();
			Excel.Workbook workbook = application.Workbooks.Open(path);
			Excel.Worksheet worksheet = workbook.ActiveSheet;
			Excel.Range range = worksheet.UsedRange;

			try
			{
				for (int row = 14; row <= range.Rows.Count; row++)
				{
					if(((Excel.Range)range.Cells[row, 2]).Text == "" || ((Excel.Range)range.Cells[row, 37]).Text == "" || ((Excel.Range)range.Cells[row, 2]).Text == "Mã MH")
					{
						continue;
					}	
					else
					{
						Lich _lich = new Lich();

						_lich.Ma_MH = ((Excel.Range)range.Cells[row, 2]).Text;
						_lich.Ten_MH = ((Excel.Range)range.Cells[row, 7]).Text;
						_lich.Lop = ((Excel.Range)range.Cells[row, 22]).Text;
						_lich.Thu = ((Excel.Range)range.Cells[row, 27]).Text;
						_lich.Phong = ((Excel.Range)range.Cells[row, 35]).Text;
						_lich.Thoi_Gian = ((Excel.Range)range.Cells[row, 37]).Text;

						db.Liches.InsertOnSubmit(_lich);
						db.SubmitChanges();
					}
						
					

				}

				workbook.Close(false);

				application.Quit();

			}
			catch (Exception)
			{
				TempData["file_lich"] = "Không thể đọc File.";
				Response.Redirect("/Admin/Lich");
			}


		}
		
		public JsonResult ShowLich(string Ma_Mon)
		{

			var lich = db.Liches.Where(x => x.Ma_MH == Ma_Mon).ToList();
			//string value = string.Empty;
			//value = JsonConvert.SerializeObject(lich.Phong, Formatting.Indented, new JsonSerializerSettings
			//{
			//	ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			//});
			var value = JsonConvert.SerializeObject(lich);

			return Json(value, JsonRequestBehavior.AllowGet);
		}

		public JsonResult ShowFriend(int id)
		{
			var Friend = db.DIEMs.Where(x => x.ID_Mon == id && x.TKCH == "D" && x.Ma_SV != Session["email"].ToString() || (x.ID_Mon == id && x.TKCH == "F" && x.Ma_SV != Session["email"].ToString())).Select(x=> new TimBanHoc { Ma_SV = x.SINHVIEN.Ma_SV,Ma_Mon = x.MON.Ma_Mon, Ho_Ten = x.SINHVIEN.Ten_SV, Lop = x.SINHVIEN.Ma_Lop, SDT = x.SINHVIEN.SDT, Diem_KT = x.TKCH, Ten_Mon = x.MON.Ten_Mon}).ToList();
			var value = JsonConvert.SerializeObject(Friend);

			return Json(value, JsonRequestBehavior.AllowGet);
		}
	}
}