using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;
namespace DATN.Areas.Admin.Controllers
{
    public class CTDTController : Controller
    {
		DataTVUDataContext db = new DataTVUDataContext();
        // GET: Admin/CTDT
        public ActionResult Index()
        {
			List<MON> CTDT = db.MONs.ToList();
			ViewBag.CTDT = CTDT;
            return View();
        }

		[HttpPost]
		public ActionResult Create(CTDT _CTDT)
		{
			if (_CTDT.Ma_CTDT == null)
			{
				TempData["CTDT_ERR"] = "Mã chương trình đào tạo không được trống";
				return RedirectToAction("index", "CTDT");
			}
			HttpPostedFileBase excelfile = Request.Files["CTDT_excelfile"];
			if (excelfile.ContentLength == 0 || excelfile == null)
			{
				TempData["CTDT_ERR"] = "Bạn chưa chọn file";
				return RedirectToAction("index", "CTDT");
			}
			else
			{
				if (!excelfile.FileName.EndsWith("xls") && !excelfile.FileName.EndsWith("xlsx"))
				{
					TempData["CTDT_ERR"] = "File không đúng định dạng ";
					return RedirectToAction("index", "CTDT");
				}
			}


			var _KTMon = db.CTDTs.Where(x => x.Ma_CTDT == _CTDT.Ma_CTDT).FirstOrDefault();

			if (_KTMon != null)
			{
				TempData["CTDT_ERR"] = "Mã chương trình đào tạo đã tồn tại";
				return RedirectToAction("index", "CTDT");
			}
			else
			{
				CTDT daotao = new CTDT();
				daotao.Ma_CTDT = _CTDT.Ma_CTDT;
				daotao.Ten_CTDT = _CTDT.Ten_CTDT;

				db.CTDTs.InsertOnSubmit(daotao);
				db.SubmitChanges();


				if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
				{
					int _tc = 0;
					int _tclt = 0;
					int _tcth = 0;

					string path = Server.MapPath("~/Content/" + excelfile.FileName);
					if (System.IO.File.Exists(path))
					{
						System.IO.File.Delete(path);
					}
					excelfile.SaveAs(path);

					//read data from excel file
					Excel.Application application = new Excel.Application();
					Excel.Workbook workbook = application.Workbooks.Open(path);
					Excel.Worksheet worksheet = workbook.ActiveSheet;
					Excel.Range range = worksheet.UsedRange;

					try
					{
						for (int row = 2; row <= range.Rows.Count; row++)
						{
							MON _mon = new MON();
							_mon.Ma_HK = ((Excel.Range)range.Cells[row, 1]).Text;
							_mon.Ma_Mon = ((Excel.Range)range.Cells[row, 2]).Text;
							_mon.Ten_Mon = ((Excel.Range)range.Cells[row, 3]).Text;

							int.TryParse(((Excel.Range)range.Cells[row, 4]).Text, out _tc);
							_mon.So_TC = _tc;

							int.TryParse(((Excel.Range)range.Cells[row, 5]).Text, out _tclt);
							_mon.TC_LyThuyet = _tclt;

							int.TryParse(((Excel.Range)range.Cells[row, 6]).Text, out _tcth);
							_mon.TC_ThucHanh = _tcth;

							_mon.Ghi_Chu = ((Excel.Range)range.Cells[row, 7]).Text;
							_mon.Ma_CTDT = _CTDT.Ma_CTDT;

							if (_mon.Ma_HK == null || _mon.Ma_Mon == null || _mon.Ten_Mon == null /*|| !IsNumber(((Excel.Range)range.Cells[row, 4]).ToString()) || !IsNumber(((Excel.Range)range.Cells[row, 5]).Text) || !IsNumber(((Excel.Range)range.Cells[row, 6]).Text)*/)
							{
								var DelCTDT = db.CTDTs.Where(x => x.Ma_CTDT == _CTDT.Ma_CTDT).FirstOrDefault();
								db.CTDTs.DeleteOnSubmit(DelCTDT);
								db.SubmitChanges();
								TempData["CTDT_ERR"] = "Không thể đọc được file.";
								break;
							}
							db.MONs.InsertOnSubmit(_mon);

							db.SubmitChanges();
						}
						workbook.Close(false);
						application.Quit();

					}
					catch (Exception)
					{
						return RedirectToAction("index", "CTDT");
					}


				}
			}
			return RedirectToAction("index", "CTDT");
		}
		public bool IsNumber(string pValue)
		{
			foreach (Char c in pValue)
			{
				if (!Char.IsDigit(c))
					return false;
			}
			return true;
		}
	}
}