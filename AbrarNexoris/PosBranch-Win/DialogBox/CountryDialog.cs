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
    public partial class CountryDialog : Form
    {
        Dropdowns drop = new Dropdowns();

        // Add properties to store selected country information
        public int SelectedCountryID { get; private set; }
        public string SelectedCountryName { get; private set; }

        public CountryDialog()
        {
            InitializeComponent();
            this.Load += CountryDialog_Load;

            // Add event handlers for selection
            ultraGridCountry.DoubleClickRow += UltraGridCountry_DoubleClickRow;
            ultraGridCountry.KeyDown += UltraGridCountry_KeyDown;
        }

        private void CountryDialog_Load(object sender, EventArgs e)
        {
            CountryDDLGrid countryObj = drop.CountryDDl();
            ultraGridCountry.DataSource = countryObj.List;
        }

        private void ultraGridCountry_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {
            // Initialize layout if needed
        }

        private void UltraGridCountry_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectCountry();
        }

        private void UltraGridCountry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCountry();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void SelectCountry()
        {
            if (ultraGridCountry.ActiveRow != null)
            {
                try
                {
                    UltraGridCell nameCell = ultraGridCountry.ActiveRow.Cells["CountryName"];
                    UltraGridCell idCell = ultraGridCountry.ActiveRow.Cells["CountryID"];
                    if (nameCell != null && idCell != null && nameCell.Value != null && idCell.Value != null)
                    {
                        string name = nameCell.Value.ToString();
                        int id = Convert.ToInt32(idCell.Value.ToString());

                        // Set the properties to expose selected country
                        SelectedCountryName = name;
                        SelectedCountryID = id;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting country: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
