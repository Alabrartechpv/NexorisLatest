using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ModelClass;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class ReportViewer : Form 
    {
        public string ReportName { get; set; }
        public dynamic reportData { get; set; }
        public DataTable tablerpt = new DataTable();
        SalesRepository sales = new SalesRepository();
        BaseRepostitory cn = new BaseRepostitory();
        ReportDocument rpt = new ReportDocument();
        public ReportViewer()
        {
            InitializeComponent();
        }

        public void setReportConnection(ReportDocument reportDoc)
        {
            ConnectionInfo connectionInfo = new ConnectionInfo
            {
                ServerName = "192.168.1.232\\SQLEXPRESS",
                DatabaseName = "RambaiTest",
                UserID = "sa",
                Password = "Abrar@123"
            };

            foreach (Table tbl in reportDoc.Database.Tables)
            {
                TableLogOnInfo tableLogOnInfo = tbl.LogOnInfo;
                tableLogOnInfo.ConnectionInfo = connectionInfo;
                tbl.ApplyLogOnInfo(tableLogOnInfo);
            }
        }

        public void PrintBill(Int64 BillNo)
        {
            DataTable dt = new DataTable();
                rpt.Load(@"C:\Users\user\Source\Repos\PosBranch-Win2\PosBranch-Win\bin\Debug\Reports\SalesInvoicePrint.rpt");//Application.ExecutablePath

                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_GetBill, (SqlConnection)cn.DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BillNo", BillNo);
                        cmd.Parameters.AddWithValue("@BranchId",SessionContext.BranchId);
                        cmd.Parameters.AddWithValue("@_Operations", "GETBILL");
                        using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            ds.Tables.Add(dt);
                            adapt.Fill(ds);
                            rpt.SetDataSource(ds);

                        }
                    }
                 // rpt.SetDataSource(reportData);
                    rpt.SetParameterValue("@BillNo", BillNo);
                    rpt.SetParameterValue("@BranchId", SessionContext.BranchId);
                    rpt.SetParameterValue("@_Operations", "GETBILL");
                    setReportConnection(rpt);
                    crystalReportViewer1.ReportSource = rpt;
                    //crystalReportViewer1.RefreshReport();
                    rpt.PrintToPrinter(1, false, 0, 0);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (cn.DataConnection.State == ConnectionState.Open)
                        cn.DataConnection.Close();
                }
            }
            }       
}
