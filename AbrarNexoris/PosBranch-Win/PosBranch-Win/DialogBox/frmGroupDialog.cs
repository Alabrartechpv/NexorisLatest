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
    public partial class frmGroupDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        public frmGroupDialog()
        {
            InitializeComponent();
        }

        private void frmGroupDialog_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "GETALL";
            GroupDDlGrid group = drop.getGroupDDl();
            group.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.dgv_Grop.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            dgv_Grop.DataSource = group.List;
        }

        private void dgv_Grop_KeyPress(object sender, KeyPressEventArgs e)
        {
            ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
            UltraGridCell Id = this.dgv_Grop.ActiveRow.Cells["Id"];
            UltraGridCell GroupName = this.dgv_Grop.ActiveRow.Cells["GroupName"];
            ItemMaster.txt_Group.Text = GroupName.Value.ToString();
            ItemMaster.lblGroupId.Text = Id.Value.ToString();
            this.Close();
        }
    }
}
