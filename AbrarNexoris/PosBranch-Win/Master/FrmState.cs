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
    public partial class FrmState : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        State state = new State();
        Dropdowns drop = new Dropdowns();
        StateRepository operations = new StateRepository();
        int Id;
        public FrmState()
        {
            InitializeComponent();

            // Initialize button states explicitly
            btnUpdate.Visible = false;
            btnSave.Visible = true;
        }

        private void FrmState_Load(object sender, EventArgs e)
        {
            this.FormatGrid();
            this.RefreshCountry();
            KeyPreview = true;

            CountryDDLGrid cp = drop.CountryDDl();
            if (cp != null && cp.List != null)
            {
                ultraGrid1.DataSource = cp.List;
            }
            
            // Ensure proper button state on load
            SetButtonMode(false);
            this.RefreshState();
        }

        private void FormatGrid()
        {
            try
            {
                // Configure main grid appearance with frmSalesInvoice styling
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Configure header appearance with modern gradient look (exactly like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Reset appearance settings (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

                // Set key navigation and selection properties (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.TabNavigation = Infragistics.Win.UltraWinGrid.TabNavigation.NextCell;

                // Set basic row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;

                // Configure active row highlighting (professional blue like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.Black;

                // Remove active cell highlighting (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = System.Drawing.Color.Empty;
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = System.Drawing.Color.Black;

                // Configure scrolling and layout (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;

                // Configure spacing and expansion behavior (like frmSalesInvoice)
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
                    
                    // StateID column - styled like frmSalesInvoice SlNo column
                    if (band.Columns.Exists("StateID"))
                    {
                        band.Columns["StateID"].Header.Caption = "ID";
                        band.Columns["StateID"].Width = 60;
                        band.Columns["StateID"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        band.Columns["StateID"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["StateID"].MinWidth = 50;
                        
                        // Reset any appearance settings for the ID column specifically (like frmSalesInvoice)
                        band.Columns["StateID"].CellAppearance.BackColor = System.Drawing.Color.Empty;
                        band.Columns["StateID"].CellAppearance.BackColor2 = System.Drawing.Color.Empty;
                        band.Columns["StateID"].CellAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    }
                    
                    // StateName column - styled like frmSalesInvoice ItemName column
                    if (band.Columns.Exists("StateName"))
                    {
                        band.Columns["StateName"].Header.Caption = "State Name";
                        band.Columns["StateName"].Width = 350;
                        band.Columns["StateName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["StateName"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["StateName"].MinWidth = 200;
                    }
                    
                    // Hide any other columns that may not be needed for state display
                    // This follows the pattern from frmSalesInvoice of hiding unnecessary columns
                    foreach (var column in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
                    {
                        if (column.Key != "StateID" && column.Key != "StateName")
                        {
                            // You can choose to hide other columns or configure them as needed
                            // For now, we'll keep them visible but set minimum width
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

        public void RefreshCountry()
        {
            CountryDDLGrid grid = drop.CountryDDl();
            ultraComboCountry.DataSource = grid.List;
            ultraComboCountry.DisplayMember = "CountryName";
            ultraComboCountry.ValueMember = "CountryID";
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            state.StateID = Id;
            state.CountryID = Convert.ToInt32(ultraComboCountry.Value);

            state.StateName = ultraTextStateName.Text;
            state.StateCode = Convert.ToInt32(ultraTextStateCode.Text);
            state._Operation = "Update";

            State message = operations.UpdateState(state);
            MessageBox.Show("State Update Success", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.RefreshState();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            this.SaveState();
            this.RefreshState();
           
        }
        public void SaveState()
        {
            try
            {
                state.StateID = 0;
                state.CountryID = Convert.ToInt32(ultraComboCountry.Value);
                state.StateName = ultraTextStateName.Text;
                state.StateCode = Convert.ToInt32(ultraTextStateCode.Text);
                state._Operation = "Create";

                string message = operations.SaveState(state);

                // Show success dialog like FrmBranch
                frmSuccesMsg msg = new frmSuccesMsg();
                msg.ShowDialog();
                
                // Clear form after successful save
                btnClear_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving state: {ex.Message}\n\nPlease check your input and try again.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in SaveState: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }
        public void RefreshState()
        {
            try
            {
                StateDDlGrid stgrid = drop.getStateDDl();
                if (stgrid != null && stgrid.List != null)
                {
                    ultraGrid1.DataSource = stgrid.List;
                }
                else
                {
                    ultraGrid1.DataSource = null;
                }
                
                // Configure columns after data is loaded
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing state: {ex.Message}");
            }
        }

        private void ultraGrid1_ClickCell(object sender, Infragistics.Win.UltraWinGrid.ClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Row != null)
                {
                    int selectedId = Convert.ToInt32(e.Cell.Row.Cells["StateID"].Value);
                    State st = operations.GetByIdState(selectedId);

                    if (st != null)
                    {
                        Id = st.StateID;
                        ultraTextStateName.Text = st.StateName;
                        ultraTextStateCode.Text = Convert.ToString(st.StateCode);
                        ultraComboCountry.Value = st.CountryID;
                        
                        // Switch to edit mode
                        SetButtonMode(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting state: {ex.Message}\n\nPlease try again or contact system administrator.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in ultraGrid1_ClickCell: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear all input fields
            ultraTextStateName.Clear();
            ultraTextStateCode.Clear();
            ultraComboCountry.Value = null;
            
            // Reset to new entry mode
            SetButtonMode(false);
            
            // Reset Id for new entry
            Id = 0;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
            {
                    int selectedId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["StateID"].Value);
                State st = operations.DeleteState(selectedId);

                if (st != null)
                {
                    MessageBox.Show("Record deleted successfully.");
                    
                    // Clear the form after deletion
                    ultraTextStateName.Clear();
                    ultraTextStateCode.Clear();
                    ultraComboCountry.Value = null;
                    Id = 0;
                }
                else
                {
                    MessageBox.Show("Error deleting record.");
                }

                // Reset to new entry mode after deletion
                SetButtonMode(false);
            }
            else
            {
                MessageBox.Show("Please select a record to delete.");
                }
                this.RefreshState();
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
                StateDDlGrid stgrid = operations.SearchRecords(ultraTextSearch.Text);
                ultraGrid1.DataSource = stgrid.List;
                
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

        private void DisplayRowValues(UltraGridRow row)
        {
            try
            {
                if (row != null && row.Cells.Count > 0)
                {
                    ultraTextStateCode.Text = row.Cells[0].Value?.ToString() ?? "";
                    ultraTextStateName.Text = row.Cells[1].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error displaying row values: {ex.Message}");
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



        private void FrmState_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                SaveState();
                this.RefreshState();
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
        }

        private void TxtStateName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ultraTextStateCode.Focus();
            }
        }

        private void TxtStateCd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ultraComboCountry.Focus();
            }
        }

        private void cbBoxCountry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSave.Focus();
            }
        }
    }
}

