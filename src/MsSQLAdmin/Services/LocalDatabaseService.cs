using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using MsSQLAdmin.Models;
using System.IO;

namespace MsSQLAdmin.Services
{
    /// <summary>
    /// SQLite Local Database
    /// </summary>
    public class LocalDatabaseService
    {
        private string _database = @"Data Source=.\Data\MsSQLAdmin.s3db";
        private SQLiteConnection _conn = null;

        /// <summary>
        /// Constructor create database and connections table
        /// </summary>
        public LocalDatabaseService()
        {
            if (!System.IO.Directory.Exists(".\\Data\\"))
            {
                System.IO.Directory.CreateDirectory(".\\Data\\");
                _conn = _conn ?? new SQLiteConnection(_database);
                if (!System.IO.File.Exists(".\\Data\\MsSQLAdmin.s3db"))
                    CreateLocalTables();
            }
            _conn = _conn ?? new SQLiteConnection(_database);
        }

        /// <summary>
        /// Destructor closes database
        /// </summary>
        ~LocalDatabaseService()
        {
            if (_conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }
        
        /// <summary>
        /// Create connections table
        /// </summary>
        private void CreateLocalTables()
        {
            using (SQLiteConnection conn = new SQLiteConnection(this._conn))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "CREATE TABLE connections(server char(100),username char(100),password char(100),database char(100))";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Get ALL connections
        /// </summary>
        /// <returns></returns>
        public DataTable GetConnections()
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "SELECT server,username,password,database FROM connections";
                SQLiteDataAdapter da = new SQLiteDataAdapter(sql, _conn);
                da.Fill(dt);
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
                return dt;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Insert NEW Connection
        /// </summary>
        /// <param name="dcm"></param>
        /// <returns></returns>
        public int InsertConnection(DatabaseConnectionModel dcm)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(this._conn))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "INSERT INTO conexoes(server,username,password,database) VALUES (@server,@username,@password,@database)";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@server", dcm.Server);
                    cmd.Parameters.AddWithValue("@username", dcm.Username);
                    cmd.Parameters.AddWithValue("@password", dcm.Password);
                    cmd.Parameters.AddWithValue("@database", dcm.Database);
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return result;
        }
    }
}
