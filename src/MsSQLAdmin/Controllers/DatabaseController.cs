using Microsoft.AspNetCore.Mvc;
using MsSQLAdmin.Services;
using MsSQLAdmin.Models;
using System.Threading.Tasks;
using System;

namespace MsSQLAdmin.Controllers {
    public class DatabaseController : Controller {
        private DatabaseService ServiceDatabase { get; set; }
        private ConnectionService ServiceConnection { get; set; }

        public DatabaseController(DatabaseService serviceDatabase, ConnectionService serviceConnection) {
            this.ServiceDatabase = serviceDatabase;
            this.ServiceConnection = serviceConnection;
        }

        [HttpGet("Server/{serveur}")]
        public async Task<IActionResult> Index([FromRoute]string serveur) {
            var model = this.ServiceConnection.GetDatabaseConnection();
            var models = await this.ServiceDatabase.GetDatabasesListAsync(model.ConnectionString);
            this.ServiceConnection.SetServeur(serveur);

            return View(models);
        }

        [HttpGet("Server/{serveur}/{database}")]
        public async Task<IActionResult> Tables([FromRoute]string serveur, [FromRoute]string database) {
            var model = this.ServiceConnection.GetDatabaseConnection();
            model.Database = database;

            this.ServiceConnection.SetDatabase(database);

            var models = await this.ServiceDatabase.GetDatabaseTablesListAsync(model.ConnectionString, database);
            return View(models);
        }

        [ResponseCache(Duration = 60)]//, VaryByQueryKeys = new string[] { "serveur", "database", "table" }
        [HttpGet("Server/{serveur}/{database}/{table}")]
        public async Task<IActionResult> Table([FromRoute]string serveur, [FromRoute] string database, [FromRoute]string table) {
            var connection = this.ServiceConnection.GetDatabaseConnection();
            connection.Database = database;

            this.ServiceConnection.SetDatabase(database);
            this.ServiceConnection.SetTable(table);

            TableViewModel model = new TableViewModel() {
                TableColumns = await this.ServiceDatabase.GetDatabaseTableDetailAsync(connection.ConnectionString, table)
            };

            return View(model);
        }

        [HttpPost("Server/{serveur}/Sql", Order = 1)]
        [HttpPost("Server/{serveur}/{database}/{table}", Order = 0)]
        public async Task<IActionResult> Data([FromRoute]string serveur, [FromRoute] string database, [FromRoute] string table, [FromForm] string sql) {
            var connection = this.ServiceConnection.GetDatabaseConnection();
            TableViewModel model = null;

            connection.Database = database;

            this.ServiceConnection.SetDatabase(database);
            this.ServiceConnection.SetTable(table);

            try {
                model = await this.ServiceDatabase.GetDataAsync(connection.ConnectionString, sql);
            } catch (Exception e) {
                model = new TableViewModel() { ErrorMessage = e.Message };
            }

            return PartialView("_Data", model);
        }

        [HttpGet("Server/{serveur}/Sql")]
        public IActionResult Sql([FromRoute]string serveur) {
            this.ServiceConnection.SetServeur(serveur);
            return View();
        }

		[HttpGet("Server/{serveur}/Create")]
		public IActionResult Create()
		{			
			return View();
		}

		[HttpPost("Server/{serveur}/Create")]
		public async Task<IActionResult> Create(DatabaseModel model)
		{
			var connection = this.ServiceConnection.GetDatabaseConnection();

			this.ServiceConnection.SetDatabase("master");

			string sql = string.Format("CREATE DATABASE {0}", model.Name);

			var k  = await this.ServiceDatabase.GetDataAsync(connection.ConnectionString, sql);
			
			return PartialView("_Create", k.DDLResult.ToString());
		}

		[HttpGet("Server/{serveur}/CreateTable")]
		public IActionResult CreateTable()
		{
			return View();
		}

		[HttpPost("Server/{serveur}/CreateTable")]
		public async Task<IActionResult> CreateTable(TableModel model)
		{
			if (model != null && model.TableColumns.GetEnumerator().Current != null)
			{
				string sql = string.Format("CREATE TABLE {0} (", model.Name);

				foreach (var item in model.TableColumns)
				{
					sql += string.Format("{0} {1} ({2}),", item.Name, item.Type, item.Precision);
				}
				sql = sql.Substring(0, sql.Length - 2);
				sql += ")";

				var connection = this.ServiceConnection.GetDatabaseConnection();

				var k = await this.ServiceDatabase.GetDataAsync(connection.ConnectionString, sql);

				return PartialView("_CreateTable", new { msg = "Creation success." });
			}
			return PartialView("_CreateTable", new { msg = "Creation failed." });
		}

	}
}