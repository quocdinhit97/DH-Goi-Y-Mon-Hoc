using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DATN.Models
{
	public class Mons
	{
		public int? ID_Mon { get; set; }
		public string MaMon { get; set; }
		public string TenMon { get; set; }
		public string HocKi { get; set; }
		public int? TinChi { get; set; }
		public double? ThiL1 { get; set; }
		public double? ThiL2 { get; set; }
		public double? ThiL3 { get; set; }
		public double? TK10 { get; set; }
		public string TKCH { get; set; }
		public string KQ1 { get; set; }
		public string KQ { get; set; }
		public string GhiChu { get; set; }
	}
}