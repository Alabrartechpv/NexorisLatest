using System;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmSalesOptions : Form
    {
        string itemname;
        int qty;
        string unit;
        int unitId;
        int Index;
        public frmSalesOptions(string ItemName, int Qty, string Unit, int UnitId, int index)
        {
            InitializeComponent();
            itemname = ItemName;
            qty = Qty;
            unit = Unit;
            unitId = UnitId;
            Index = index;


        }

        private void frmSalesOptions_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            textBox1.Text = itemname;

        }

        private void frmSalesOptions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
