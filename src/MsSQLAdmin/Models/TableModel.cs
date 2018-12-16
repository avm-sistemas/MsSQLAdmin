using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsSQLAdmin.Models
{
	public class TableModel
	{
		public DatabaseModel Database { get; set; }
		public string Name { get; set; }
		public IEnumerable<TableColumnModel> TableColumns { get; set; }
	}
}
