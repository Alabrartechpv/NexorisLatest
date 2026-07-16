using Infragistics.Win.UltraWinGrid;
using ModelClass;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmSalesPersonDial : Form
    {
        Dropdowns dp = new Dropdowns();
        public frmSalesPersonDial()
        {
            InitializeComponent();
        }

        private void frmSalesPersonDial_Load(object sender, EventArgs e)
        {
            SalesPersonDDlGrid cs = dp.GetSalesPerson();
            cs.List.ToString();        
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = cs.List;
            // ultraGridItem.Focus();
        }
    }
}
