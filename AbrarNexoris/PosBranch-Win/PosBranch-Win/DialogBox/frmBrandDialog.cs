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
    public partial class frmBrandDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        public frmBrandDialog()
        {
            InitializeComponent();
        }

        private void frmBrandDialog_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "GETALL";
            BrandDDLGrid brand = drop.getBrandDDL();
            brand.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = brand.List;
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
            UltraGridCell Id = this.ultraGrid1.ActiveRow.Cells["Id"];
            UltraGridCell BrandName = this.ultraGrid1.ActiveRow.Cells["BrandName"];

            ItemMaster.txt_Brand.Text = BrandName.Value.ToString();
            ItemMaster.lblBrandId.Text = Id.Value.ToString();
            this.Close();

        }
    }
}
