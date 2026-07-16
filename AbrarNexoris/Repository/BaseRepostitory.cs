using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Repository
{
    public class BaseRepostitory : IDisposable
    {
        public IDbConnection DataConnection;
        bool disposed = false;
        DataBase db = new DataBase();
        public BaseRepostitory()
        //test sibi ---
        //hgjhgjhgjhhj
        //change by Shaji
        //change by Ashlydd
        {
            DataBase.Status = "Local";
            if (DataBase.Status == "Local")
            {
                string txtpath = @"C:\Connection\Config.txt";
                try
                {
                    if (!File.Exists(txtpath))
                    {
                        throw new FileNotFoundException("Database configuration file is missing.", txtpath);
                    }

                    using (StreamReader sr = new StreamReader(txtpath))
                    {
                        string ss = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(ss))
                        {
                            throw new InvalidOperationException("Database configuration file is empty.");
                        }

                        string[] txtsplit = ss.Split(';');
                        if (txtsplit.Length < 4)
                        {
                            throw new FormatException("Database configuration file is malformed. It must contain at least 4 semicolon-separated parameters.");
                        }

                        string server = txtsplit[0].ToString();
                        string DataBase = txtsplit[1].ToString();
                        string userid = txtsplit[2].ToString(); // user id
                        string password = txtsplit[3].ToString(); // password
                        string Local = server + ';' + DataBase + ';' + userid + ';' + password + ';';

                        DataConnection = new SqlConnection(Local);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error initializing connection: {ex.Message}", ex);
                }
            }
            else
            {
                throw new NotSupportedException($"Database status '{DataBase.Status}' is not supported in production. Only local configuration is allowed.");
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (DataConnection != null)
                    {
                        if (DataConnection.State == ConnectionState.Open)
                        {
                            DataConnection.Close();
                        }
                        DataConnection.Dispose();
                        DataConnection = null;
                    }
                }
                disposed = true;
            }
        }
         

        ~BaseRepostitory()
        {
            Dispose(false);
        }
    }
}
