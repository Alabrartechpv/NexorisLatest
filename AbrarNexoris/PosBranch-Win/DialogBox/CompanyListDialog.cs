using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository.MasterRepositry;
using ModelClass.Master;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.DialogBox
{
    public partial class CompanyListDialog : Form
    {
        private CompanyRepo _companyRepo;

        // Properties to expose selected company
        public int SelectedCompanyID { get; private set; }
        public string SelectedCompanyName { get; private set; }

        public CompanyListDialog()
        {
            InitializeComponent();
            _companyRepo = new CompanyRepo();

            this.Load += CompanyListDialog_Load;

            // Add event handlers for selection
            this.ultraGridCountry.DoubleClickRow += UltraGridCompanies_DoubleClickRow;
            this.ultraGridCountry.KeyDown += UltraGridCompanies_KeyDown;
        }

        private void CompanyListDialog_Load(object sender, EventArgs e)
        {
            // Load companies data
            LoadCompanies();

            // Set focus to the grid
            this.ultraGridCountry.Focus();

            // If there are companies, select the first one
            if (this.ultraGridCountry.Rows.Count > 0)
            {
                this.ultraGridCountry.ActiveRow = this.ultraGridCountry.Rows[0];
                this.ultraGridCountry.Selected.Rows.Clear();
                this.ultraGridCountry.Selected.Rows.Add(this.ultraGridCountry.Rows[0]);
            }
        }

        private void LoadCompanies()
        {
            try
            {
                // Get companies from repository
                var companies = _companyRepo.GetCompanyDropdownList();

                // Set as grid data source
                this.ultraGridCountry.DataSource = companies;

                // Configure grid columns if needed
                if (this.ultraGridCountry.DisplayLayout.Bands.Count > 0)
                {
                    if (this.ultraGridCountry.DisplayLayout.Bands[0].Columns.Exists("CompanyName"))
                        this.ultraGridCountry.DisplayLayout.Bands[0].Columns["CompanyName"].Width = 300;
                    if (this.ultraGridCountry.DisplayLayout.Bands[0].Columns.Exists("CompanyID"))
                        this.ultraGridCountry.DisplayLayout.Bands[0].Columns["CompanyID"].Width = 80;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading companies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGridCompanies_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectCompany();
        }

        private void UltraGridCompanies_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCompany();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void SelectCompany()
        {
            if (ultraGridCountry.ActiveRow != null)
            {
                try
                {
                    UltraGridCell nameCell = ultraGridCountry.ActiveRow.Cells["CompanyName"];
                    UltraGridCell idCell = ultraGridCountry.ActiveRow.Cells["CompanyID"];
                    if (nameCell != null && idCell != null && nameCell.Value != null && idCell.Value != null)
                    {
                        // Store selected company info
                        SelectedCompanyName = nameCell.Value.ToString();
                        SelectedCompanyID = Convert.ToInt32(idCell.Value.ToString());

                        // Return success and close dialog
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error selecting company: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
