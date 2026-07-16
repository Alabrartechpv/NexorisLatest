using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository;
using ModelClass.Master;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.DialogBox
{
    public partial class TaxDialog : Form
    {
        Dropdowns drop = new Dropdowns();

        // Add properties to store selected tax information
        public int SelectedTaxID { get; private set; }
        public string SelectedTaxName { get; private set; }

        public TaxDialog()
        {
            InitializeComponent();
            this.Load += TaxDialog_Load;

            // Add event handlers for selection
            ultraGridTaxType.DoubleClickRow += UltraGridTaxType_DoubleClickRow;
            ultraGridTaxType.KeyDown += UltraGridTaxType_KeyDown;
        }

        private void TaxDialog_Load(object sender, EventArgs e)
        {
            TaxTypeDDLGrid taxObj = drop.TaxTypeDDL();
            ultraGridTaxType.DataSource = taxObj.List;
        }

        private void UltraGridTaxType_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectTax();
        }

        private void UltraGridTaxType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectTax();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void SelectTax()
        {
            if (ultraGridTaxType.ActiveRow != null)
            {
                try
                {
                    UltraGridCell nameCell = ultraGridTaxType.ActiveRow.Cells["TaxType"];
                    UltraGridCell idCell = ultraGridTaxType.ActiveRow.Cells["ID"];
                    if (nameCell != null && idCell != null && nameCell.Value != null && idCell.Value != null)
                    {
                        string name = nameCell.Value.ToString();
                        int id = Convert.ToInt32(idCell.Value.ToString());

                        // Set the properties to expose selected tax
                        SelectedTaxName = name;
                        SelectedTaxID = id;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting tax type: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
