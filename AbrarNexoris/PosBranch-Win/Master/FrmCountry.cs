using ModelClass;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Master
{
    public partial class FrmCountry : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Country cntry = new Country();
        Dropdowns drop = new Dropdowns();
        CountryRepository operations = new CountryRepository();
        int Id;
        public FrmCountry()
        {
            InitializeComponent();

            // Initialize button states explicitly
            btnUpdate.Visible = false;
            btnSave.Visible = true;
        }

        private void FrmCountry_Load(object sender, EventArgs e)
        {
            this.FormatGrid();
            this.RefreshTax();
            KeyPreview = true;

            TaxTypeDDLGrid taxgrid = drop.TaxTypeDDL();
            taxgrid.List.ToString();
            
            // Set data source for UltraGrid
            ultraGrid1.DataSource = taxgrid.List;
            btnUpdate.Visible = false;
            this.RefreshCountry();
        }

        private void FormatGrid()
        {
            try
            {
                // Configure main grid appearance with modern styling
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Configure header appearance with modern gradient look
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Reset appearance settings
                ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

                // Set key navigation and selection properties
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.TabNavigation = Infragistics.Win.UltraWinGrid.TabNavigation.NextCell;

                // Set basic row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;

                // Configure active row highlighting
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.Black;

                // Remove active cell highlighting
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = System.Drawing.Color.Empty;
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = System.Drawing.Color.Black;

                // Configure scrolling and layout
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;

                // Configure spacing and expansion behavior
                ultraGrid1.DisplayLayout.InterBandSpacing = 10;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                
                // Set row sizing to fixed height for compact appearance
                ultraGrid1.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.Fixed;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 25;
                ultraGrid1.DisplayLayout.Override.CellPadding = 2;

                // Configure columns after data is loaded
                ConfigureGridColumns();
                
                // Connect grid events
                ConnectGridEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting grid: {ex.Message}");
            }
        }

        private void ConfigureGridColumns()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    var band = ultraGrid1.DisplayLayout.Bands[0];
                    
                    // CountryID column
                    if (band.Columns.Exists("CountryID"))
                    {
                        band.Columns["CountryID"].Header.Caption = "ID";
                        band.Columns["CountryID"].Width = 60;
                        band.Columns["CountryID"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        band.Columns["CountryID"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["CountryID"].MinWidth = 50;
                        
                        // Reset any appearance settings for the ID column
                        band.Columns["CountryID"].CellAppearance.BackColor = System.Drawing.Color.Empty;
                        band.Columns["CountryID"].CellAppearance.BackColor2 = System.Drawing.Color.Empty;
                        band.Columns["CountryID"].CellAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    }
                    
                    // CountryName column
                    if (band.Columns.Exists("CountryName"))
                    {
                        band.Columns["CountryName"].Header.Caption = "Country Name";
                        band.Columns["CountryName"].Width = 350;
                        band.Columns["CountryName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["CountryName"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["CountryName"].MinWidth = 200;
                    }
                    
                    // Hide any other columns that may not be needed for display
                    foreach (var column in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
                    {
                        if (column.Key != "CountryID" && column.Key != "CountryName")
                        {
                            if (!column.Hidden)
                            {
                                column.MinWidth = 50;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring grid columns: {ex.Message}");
            }
        }

        private void ConnectGridEvents()
        {
            try
            {
                // Add keyboard event handler for navigation
                ultraGrid1.KeyDown += UltraGrid1_KeyDown;
                
                // Add row selection event
                ultraGrid1.AfterRowActivate += UltraGrid1_AfterRowActivate;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting grid events: {ex.Message}");
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
                {
                    // Handle Enter key press
                    DisplayRowValues(ultraGrid1.ActiveRow);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in grid key handler: {ex.Message}");
            }
        }

        private void UltraGrid1_AfterRowActivate(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Handle row activation for selection
                    DisplayRowValues(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in row activate handler: {ex.Message}");
            }
        }
        public void RefreshTax()
        {
            TaxTypeDDLGrid grid = drop.TaxTypeDDL();
            ultraComboTax.DataSource = grid.List;
            ultraComboTax.DisplayMember = "TaxType";
            ultraComboTax.ValueMember = "ID";
        }
        public void RefreshCountry()
        {
            try
        {
            CountryDDLGrid ctgrid = drop.getCountryDDl();
                ultraGrid1.DataSource = ctgrid.List;
                
                // Configure columns after data is loaded
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing country: {ex.Message}");
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SaveCountry();
            this.RefreshCountry();
        }
        public void SaveCountry()
        {
            try
            {
                cntry.CountryID = 0;
                cntry.CountryName = ultraTextName.Text;
                cntry.FinYearFrom = Convert.ToDateTime(ultraDateFinYearFrom.Text);
                cntry.FinYearTo = Convert.ToDateTime(ultraDateFinYearTo.Text);
                cntry.TaxTypeId = Convert.ToInt32(ultraComboTax.Value);
                cntry.BookFrom = Convert.ToDateTime(ultraDateBookFrom.Text);
                cntry.BookTo = Convert.ToDateTime(ultraDateBookTo.Text);
                cntry._Operation = "CREATE";
                string message = operations.SaveCountry(cntry);

                // Show success dialog like FrmBranch
                frmSuccesMsg msg = new frmSuccesMsg();
                msg.ShowDialog();
                
                // Clear form after successful save
                btnClear_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving country: {ex.Message}\n\nPlease check your input and try again.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in SaveCountry: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ultraTextName.Clear();
            ultraComboTax.Value = null;
            ultraDateFinYearFrom.Value = null;
            ultraDateFinYearTo.Value = null;
            ultraDateBookFrom.Value = null;
            ultraDateBookTo.Value = null;
            
            // Reset to new entry mode
            SetButtonMode(false);
            
            // Reset Id for new entry
            Id = 0;
        }

        private void ultraGrid1_ClickCell(object sender, Infragistics.Win.UltraWinGrid.ClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Row != null)
                {
                    int selectedId = Convert.ToInt32(e.Cell.Row.Cells["CountryID"].Value);
                    Country cont = operations.GetByIdCountry(selectedId);

                    if (cont != null)
                    {
                        Id = cont.CountryID;
                        ultraTextName.Text = cont.CountryName;
                        ultraComboTax.Value = cont.TaxTypeId;
                        ultraDateFinYearFrom.Text = Convert.ToString(cont.FinYearFrom);
                        ultraDateFinYearTo.Text = Convert.ToString(cont.FinYearTo);
                        ultraDateBookFrom.Text = Convert.ToString(cont.BookFrom);
                        ultraDateBookTo.Text = Convert.ToString(cont.BookTo);
                        
                        // Switch to edit mode
                        SetButtonMode(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting country: {ex.Message}\n\nPlease try again or contact system administrator.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in ultraGrid1_ClickCell: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            cntry.CountryID = Id;
            cntry.CountryName = ultraTextName.Text;
            cntry.FinYearFrom = Convert.ToDateTime(ultraDateFinYearFrom.Text);
            cntry.FinYearTo = Convert.ToDateTime(ultraDateFinYearTo.Text);
            cntry.TaxTypeId = Convert.ToInt32(ultraComboTax.Value);
            cntry.BookFrom = Convert.ToDateTime(ultraDateBookFrom.Text);
            cntry.BookTo = Convert.ToDateTime(ultraDateBookTo.Text);
            cntry._Operation = "UPDATE";
            Country message = operations.UpdateCountry(cntry);
            MessageBox.Show("Country Update Success", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.RefreshCountry();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
            {
                    int selectedId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["CountryID"].Value);
               Country ct = operations.DeleteCountry(selectedId);

                if (ct != null)
                {
                    Id = ct.CountryID;
                        ultraTextName.Text = ct.CountryName;
                        ultraComboTax.Value = ct.TaxTypeId;
                        ultraDateFinYearFrom.Text = Convert.ToString(ct.FinYearFrom);
                        ultraDateFinYearTo.Text = Convert.ToString(ct.FinYearTo);
                        ultraDateBookFrom.Text = Convert.ToString(ct.BookFrom);
                        ultraDateBookTo.Text = Convert.ToString(ct.BookTo);
                    
                    MessageBox.Show("Record deleted successfully.");
                }
                else
                {
                    MessageBox.Show("Error deleting record.");
                }
                btnSave.Visible = false;
                btnUpdate.Visible = true;
            }
            else
            {
                MessageBox.Show("Please select a record to delete.");
                }
                this.RefreshCountry();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CountryDDLGrid cntygrid = drop.SearchCountry(ultraTextSearch.Text);
                ultraGrid1.DataSource = cntygrid.List;
                
                // Configure columns after search results are loaded
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in search: {ex.Message}");
            }
        }

        private void ultraGrid1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UltraGridRow selectedRow = ultraGrid1.ActiveRow;
                if (selectedRow != null)
                {
                DisplayRowValues(selectedRow);
            }
        }
        }
        private void DisplayRowValues(UltraGridRow row)
        {
            try
            {
                if (row != null && row.Cells.Count > 0)
                {
                    ultraTextName.Text = row.Cells[0].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error displaying row values: {ex.Message}");
            }
        }

        private void FrmCountry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                SaveCountry();
                this.RefreshCountry();
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[0].Cells[0];
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[0].Cells[0];
                    ultraGrid1.Focus();
                }
            }
        }

        private void TxtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ultraComboTax.Focus();
            }
        }

        private void cbBoxTax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ultraDateFinYearFrom.Focus();
            }
        }

        private void ultraDateFinYearFrom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ultraDateFinYearTo.Focus();
            }
        }

        private void ultraDateFinYearTo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ultraDateBookFrom.Focus();
            }
        }

        private void ultraDateBookFrom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ultraDateBookTo.Focus();
            }
        }

        private void ultraDateBookTo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSave.Focus();
            }
        }

        private void SetButtonMode(bool isEditMode)
        {
            if (isEditMode)
            {
                btnSave.Visible = false;
                btnUpdate.Visible = true;
            }
            else
            {
                btnSave.Visible = true;
                btnUpdate.Visible = false;
            }
        }
    }
}
