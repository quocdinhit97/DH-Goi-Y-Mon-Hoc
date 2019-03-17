using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DATN.Models;
using Excel = Microsoft.Office.Interop.Excel;
namespace DATN.Controllers
{
    public class ImportDataController : Controller
    {
		DataTVUDataContext db = new DataTVUDataContext();
        // GET: ImportData
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult uploadDiem(ClassTemp upload)
		{
			if (db.SINHVIENs.Where(x => x.Ma_SV == upload.masv).Count() > 0)
			{
				ViewBag.maso = upload.masv+": "+"Đã tồn tại.";
			}
			else
			{
				SINHVIEN sv = new SINHVIEN();
				sv.Ma_SV = upload.masv;
				sv.Ma_Lop = upload.lops;
				sv.Ten_SV = upload.masv;
				Session["lop"] = upload.lops;
				Session["email"] = upload.masv;
				Session["name"] = upload.masv;
				Session["diachi"] = "";
				Session["sdt"] = "";
				Session["avata"] = "";
				db.SINHVIENs.InsertOnSubmit(sv);
				db.SubmitChanges();

				HttpPostedFileBase excelfile = Request.Files["excelfile"];
				if (excelfile.ContentLength == 0 || excelfile == null)
				{
					TempData["file"] = "<label style='color: #ff4a4a'>Bạn chưa chọn file điểm.</label>";
					return RedirectToAction("index", "importdata");
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

			}


			ViewBag.thongbao = "thành công";
			return View("index");
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
					string test = ((Excel.Range)range.Cells[row, 11]).Text;
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
				if (diem.ID_Diem != idDiem)
				{
					if (diem.ID_Mon == IdMon && diem.TK10 < kq)
					{
						//var DiemNho = db.DIEMs.Where(x => x.ID_Mon == diem.ID_Mon).FirstOrDefault();
						db.DIEMs.DeleteOnSubmit(diem);
						db.SubmitChanges();
					}
					else if (diem.ID_Mon == IdMon && diem.TK10 > kq)
					{
						var DiemNho = db.DIEMs.Where(x => x.ID_Diem == idDiem).FirstOrDefault();
						db.DIEMs.DeleteOnSubmit(DiemNho);
						db.SubmitChanges();
					}

					if (diem.ID_Mon == IdMon && diem.TK10 == kq)
					{
						if (diem.TKCH != "")
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