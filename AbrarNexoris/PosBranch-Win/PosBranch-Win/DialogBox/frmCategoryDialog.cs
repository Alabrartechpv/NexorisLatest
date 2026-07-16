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
   
    public partial class frmCategoryDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        public frmCategoryDialog()
        {
            InitializeComponent();
        }

        private void frmCategoryDialog_Load(object sender, EventArgs e)
        {
            
            DataBase.Operations = "Category";
            CategoryDDlGrid category = drop.getCategoryDDl(txt_Search.Text);
            category.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = category.List;
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
            UltraGridCell Id = this.ultraGrid1.ActiveRow.Cells["Id"];
            UltraGridCell CategoryName = this.ultraGrid1.ActiveRow.Cells["CategoryName"];
            ItemMaster.txt_Category.Text = CategoryName.Value.ToString();
            ItemMaster.lblCategoryId.Text = Id.Value.ToString();
            this.Close();
        }

        public void getdata(string search)
        {
            DataBase.Operations = "Category";
            CategoryDDlGrid category = drop.getCategoryDDl(txt_Search.Text);
          if(  category.List.Count() > 0)
            {
                category.List.ToString();
                DataGridTableStyle ts1 = new DataGridTableStyle();
                DataGridColumnStyle datagrid = new DataGridBoolColumn();
                //datagrid.Width = 400;
                this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                ts1.GridColumnStyles.Add(datagrid);
                ultraGrid1.DataSource = category.List;
            }
           
        }

        private void txt_Search_TextChanged(object sender, EventArgs e)
        {
            this.getdata(txt_Search.Text);
        }

        private void txt_Search_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Down)
            {
                ultraGrid1.Focus();
            }
        }
    }
}
