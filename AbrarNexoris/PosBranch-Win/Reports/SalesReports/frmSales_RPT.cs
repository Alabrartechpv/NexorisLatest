using AngleSharp.Io;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSales_RPT : Form
    {
        public SalesReportRepository report = new SalesReportRepository();
        Requestfrm request = new Requestfrm();
        public frmSales_RPT()
        {
            InitializeComponent();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public void GetSales()
        {
            request.FromDate = Convert.ToDateTime( ultraDateTimeEditor1.Value.ToString());
            request.ToDate = Convert.ToDateTime(ultraDateTimeEditor2.Value.ToString());
            request.BranchId = SessionContext.BranchId;
            List<Sales_Daily> getSales = this.report.getSales(request);
            if (getSales != null)
            {
                getSales.ToString();
                DataGridTableStyle ts1 = new DataGridTableStyle();
                DataGridColumnStyle datagrid = new DataGridBoolColumn();
                //datagrid.Width = 400;
                this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                ts1.GridColumnStyles.Add(datagrid);
                ultraGrid1.DataSource = getSales;
            }
        }

        private void btn_Submit_Click(object sender, EventArgs e)
        {
            this.GetSales();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {

        }
        public void Print()
        {

        }
    }
}
