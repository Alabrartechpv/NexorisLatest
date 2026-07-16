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
    public partial class frmTaxTypeDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        public frmTaxTypeDialog()
        {
            InitializeComponent();
        }

        private void frmTaxTypeDialog_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "GETTAXPER";
            TaxTypeDDLGrid taxType = drop.GetTaxType();
            taxType.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = taxType.List;
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
            UltraGridCell ID = this.ultraGrid1.ActiveRow.Cells["ID"];
            UltraGridCell TaxType = this.ultraGrid1.ActiveRow.Cells["TaxType"];
            ItemMaster.txt_TaxType.Text = TaxType.Value.ToString();
            ItemMaster.lbl_taxper.Text = ID.Value.ToString();
            this.Close();
        }
    }
}
