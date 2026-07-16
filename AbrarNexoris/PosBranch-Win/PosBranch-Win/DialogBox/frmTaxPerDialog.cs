using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Master;
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
    public partial class frmTaxPerDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        public frmTaxPerDialog()
        {
            InitializeComponent();
        }

        private void frmTaxPerDialog_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "GETTAXPER";
            TaxPerDDlGrid taxper = drop.GetTaxPer();
            taxper.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = taxper.List;
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
            UltraGridCell ID = this.ultraGrid1.ActiveRow.Cells["Id"];
            UltraGridCell TaxPer = this.ultraGrid1.ActiveRow.Cells["TaxPer"];
            ItemMaster.txt_TaxPer.Text = TaxPer.Value.ToString();
            ItemMaster.lblTaxtypeId.Text = ID.Value.ToString();
            this.Close();
        }
    }
}
