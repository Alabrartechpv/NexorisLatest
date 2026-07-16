using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
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
    public partial class frmUnitDialogItem : Form
    {
        Dropdowns dp = new Dropdowns();
        public frmUnitDialogItem()
        {
            InitializeComponent();
        }

        private void frmUnitDialogItem_Load(object sender, EventArgs e)
        {
            UnitDDlGrid unit = dp.getUnitDDl();
            unit.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = unit.List;
        }
    }
}
