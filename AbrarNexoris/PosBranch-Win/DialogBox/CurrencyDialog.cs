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
    public partial class CurrencyDialog : Form
    {
        Dropdowns drop = new Dropdowns();

        // Add properties to store selected currency information
        public int SelectedCurrencyID { get; private set; }
        public string SelectedCurrencyName { get; private set; }

        public CurrencyDialog()
        {
            InitializeComponent();
            this.Load += CurrencyDialog_Load;

            // Add event handlers for selection
            ultraGridCurrency.DoubleClickRow += UltraGridCurrency_DoubleClickRow;
            ultraGridCurrency.KeyDown += UltraGridCurrency_KeyDown;
        }

        private void CurrencyDialog_Load(object sender, EventArgs e)
        {
            CurrencyDDLGRID curObj = drop.getCurrency();
            ultraGridCurrency.DataSource = curObj.List;
        }

        private void UltraGridCurrency_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectCurrency();
        }

        private void UltraGridCurrency_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCurrency();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void SelectCurrency()
        {
            if (ultraGridCurrency.ActiveRow != null)
            {
                try
                {
                    UltraGridCell nameCell = ultraGridCurrency.ActiveRow.Cells["CurrencyName"];
                    UltraGridCell idCell = ultraGridCurrency.ActiveRow.Cells["CurrencyID"];
                    if (nameCell != null && idCell != null && nameCell.Value != null && idCell.Value != null)
                    {
                        string name = nameCell.Value.ToString();
                        int id = Convert.ToInt32(idCell.Value.ToString());

                        // Set the properties to expose selected currency
                        SelectedCurrencyName = name;
                        SelectedCurrencyID = id;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting currency: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
