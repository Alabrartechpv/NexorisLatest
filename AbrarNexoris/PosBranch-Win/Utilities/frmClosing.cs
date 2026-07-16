using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using ModelClass;
using Repository;
using Infragistics.Win.UltraWinGrid;
using PosBranch_Win.DialogBox;

namespace PosBranch_Win.Utilities
{
    public partial class frmClosing : Form
    {
        private ClosingRepo _repo;
        private List<CashDetail> _cashDetails;
        private ClosingModel _model;
        private SalesDataSummary _salesData;
        private CustomerReceiptSummary _receiptData;

        public frmClosing()
        {
            InitializeComponent();
            _repo = new ClosingRepo();
            _model = new ClosingModel();
            ApplyModernLayout();
            Resize += FrmClosing_Resize;
            LoadData();
        }

        private void LoadData()
        {
            // Initialize denomination grid
            _cashDetails = _repo.GetDefaultDenominations();
            gridCash.DataSource = _cashDetails;

            // IMPORTANT: Configure grid immediately after binding to hide unwanted columns
            ConfigureGrid();

            // Calculate Total
            CalculateTotal();

            // Event handlers
            gridCash.BeforeCellUpdate += GridCash_BeforeCellUpdate;
            gridCash.AfterCellUpdate += GridCash_AfterCellUpdate;
            gridCash.KeyDown += GridCash_KeyDown;
            gridCash.InitializeLayout += GridCash_InitializeLayout;

            // Add closing history button click event
            btnClosingHistory.Click += BtnClosingHistory_Click;

            ApplyResponsiveLayout();

            // Set default values
            dtpDate.Value = DateTime.Now;
            cboReportSelection.Text = "Shift Collection";

            // Load counter from session
            txtCounter.Text = !string.IsNullOrWhiteSpace(SessionContext.CounterName)
                ? SessionContext.CounterName
                : (SessionContext.CounterId > 0 ? $"COUNTER{SessionContext.CounterId}" : "Counter-1");

            // Populate report selection dropdown
            PopulateReportSelection();

            _salesData = null;
            _receiptData = null;

            Shown += FrmClosing_Shown;
        }

        private void FrmClosing_Shown(object sender, EventArgs e)
        {
            FocusQuantityCell(0, true);
        }

        private void PopulateReportSelection()
        {
            cboReportSelection.Items.Clear();
            cboReportSelection.Items.Add("Shift Collection");
            cboReportSelection.Items.Add("Day End Closing");
            cboReportSelection.Items.Add("Mid Day Closing");
            cboReportSelection.SelectedIndex = 0;
        }

        private void ApplyModernLayout()
        {
            Font = new Font("Segoe UI", 9F);
            Text = "Counter Closing";
            BackColor = Color.FromArgb(246, 248, 251);
            ultraPanel1.Appearance.BackColor = Color.FromArgb(246, 248, 251);

            txtPurchaseNo.Text = SessionContext.CounterSessionId > 0
                ? SessionContext.CounterSessionId.ToString()
                : string.Empty;
            lblDocNo.Text = "Session:";

            StyleField(txtCounter);
            StyleField(txtPurchaseNo);
            StyleField(txtTotal);
            cboReportSelection.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            cboReportSelection.Appearance.BackColor = Color.White;
            cboReportSelection.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            dtpDate.Enabled = true;
            dtpDate.ReadOnly = true;
            dtpDate.TabStop = false;
            dtpDate.Appearance.BackColor = Color.White;
            dtpDate.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            dtpDate.Appearance.BorderColor = Color.FromArgb(203, 213, 225);
            dtpDate.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            dtpDate.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            lblTotal.Text = "Counted Cash:";
            lblTotal.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            lblCounter.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            lblDate.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            lblReportSelection.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            lblDocNo.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            btnClosingHistory.Visible = CanViewClosingHistory();
        }

        private void FrmClosing_Resize(object sender, EventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void ApplyResponsiveLayout()
        {
            if (ultraPanel1 == null || grpCashCalculation == null ||
                lblTotal == null || txtTotal == null)
                return;

            int margin = 28;
            int top = 118;
            int bottomBarHeight = 58;
            int availableWidth = Math.Max(900, ultraPanel1.ClientSize.Width - (margin * 2));
            int availableHeight = Math.Max(300, ultraPanel1.ClientSize.Height - top - bottomBarHeight - 14);
            int gridWidth = availableWidth;

            grpCashCalculation.Location = new Point(margin, top);
            grpCashCalculation.Size = new Size(gridWidth, availableHeight);
            ResizeGridColumns();

            int bottomY = top + availableHeight + 14;
            lblTotal.Location = new Point(margin, bottomY + 8);
            txtTotal.Location = new Point(margin + 150, bottomY);
            txtTotal.Size = new Size(190, 35);

        }

        private void StyleField(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            editor.Appearance.BorderColor = Color.FromArgb(203, 213, 225);
            editor.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            editor.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
        }

        public void Save()
        {
            btnSave_Click(this, EventArgs.Empty);
        }

        public void RibbonClear()
        {
            var result = MessageBox.Show("Are you sure you want to clear all data?",
                "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ClearForm();
            }
        }

        private bool CanViewClosingHistory()
        {
            string userLevel = SessionContext.UserLevel ?? string.Empty;
            return userLevel.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   userLevel.IndexOf("manager", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   userLevel.IndexOf("supervisor", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ConfigureGrid()
        {
            if (gridCash.DisplayLayout.Bands.Count == 0)
                return;

            var band = gridCash.DisplayLayout.Bands[0];

            // Configure columns
            if (band.Columns.Exists("No"))
            {
                band.Columns["No"].Width = 70;
                band.Columns["No"].Header.Caption = "#";
                StyleGridHeader(band.Columns["No"]);
                band.Columns["No"].CellActivation = Activation.NoEdit;
                band.Columns["No"].CellAppearance.BackColor = Color.FromArgb(241, 245, 249);
                band.Columns["No"].CellAppearance.ForeColor = Color.FromArgb(51, 65, 85);
                band.Columns["No"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                band.Columns["No"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            }

            if (band.Columns.Exists("Denomination"))
            {
                band.Columns["Denomination"].Width = 230;
                band.Columns["Denomination"].Header.Caption = "Denomination (₹)";
                StyleGridHeader(band.Columns["Denomination"]);
                band.Columns["Denomination"].Format = "0.00";
                band.Columns["Denomination"].CellActivation = Activation.NoEdit;
                band.Columns["Denomination"].CellAppearance.BackColor = Color.FromArgb(248, 250, 252);
                band.Columns["Denomination"].CellAppearance.ForeColor = Color.FromArgb(15, 23, 42);
                band.Columns["Denomination"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                band.Columns["Denomination"].CellAppearance.FontData.SizeInPoints = 11;
                band.Columns["Denomination"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            if (band.Columns.Exists("Quantity"))
            {
                band.Columns["Quantity"].Width = 260;
                band.Columns["Quantity"].Header.Caption = "Quantity";
                StyleGridHeader(band.Columns["Quantity"]);
                band.Columns["Quantity"].CellActivation = Activation.AllowEdit;
                band.Columns["Quantity"].CellAppearance.BackColor = Color.FromArgb(255, 251, 235);
                band.Columns["Quantity"].CellAppearance.ForeColor = Color.FromArgb(30, 64, 175);
                band.Columns["Quantity"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                band.Columns["Quantity"].CellAppearance.FontData.SizeInPoints = 11;
                band.Columns["Quantity"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                band.Columns["Quantity"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.IntegerPositive;
            }

            if (band.Columns.Exists("Amount"))
            {
                band.Columns["Amount"].Width = 280;
                band.Columns["Amount"].Header.Caption = "Amount (₹)";
                StyleGridHeader(band.Columns["Amount"]);
                band.Columns["Amount"].Format = "#,##0.00";
                band.Columns["Amount"].CellActivation = Activation.NoEdit;
                band.Columns["Amount"].CellAppearance.BackColor = Color.FromArgb(220, 252, 231);
                band.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(20, 83, 45);
                band.Columns["Amount"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                band.Columns["Amount"].CellAppearance.FontData.SizeInPoints = 11;
                band.Columns["Amount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            // Hide unwanted columns
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Key != "No" && col.Key != "Denomination" && col.Key != "Quantity" && col.Key != "Amount")
                {
                    col.Hidden = true;
                }
            }

            // Grid styling - quiet, fast cashier-entry table
            gridCash.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            gridCash.DisplayLayout.Appearance.BackColor = Color.White;
            band.ColHeadersVisible = true;
            band.HeaderVisible = false;
            gridCash.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
            gridCash.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
            gridCash.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(219, 234, 254);
            gridCash.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(219, 234, 254);
            gridCash.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
            gridCash.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.FromArgb(15, 23, 42);
            gridCash.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            gridCash.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 10F;
            gridCash.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            gridCash.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            gridCash.DisplayLayout.Override.HeaderAppearance.BorderColor = Color.FromArgb(96, 165, 250);
            gridCash.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
            gridCash.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
            gridCash.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.Select;
            gridCash.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            gridCash.DisplayLayout.Override.RowAppearance.ForeColor = Color.FromArgb(15, 23, 42);
            gridCash.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);
            gridCash.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(238, 242, 255);
            gridCash.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(15, 23, 42);
            gridCash.DisplayLayout.Override.ActiveCellAppearance.BackColor = Color.FromArgb(29, 78, 216);
            gridCash.DisplayLayout.Override.ActiveCellAppearance.ForeColor = Color.White;
            gridCash.DisplayLayout.Override.SelectedCellAppearance.BackColor = Color.FromArgb(29, 78, 216);
            gridCash.DisplayLayout.Override.SelectedCellAppearance.ForeColor = Color.White;
            gridCash.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(238, 242, 255);
            gridCash.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.FromArgb(15, 23, 42);
            gridCash.DisplayLayout.Override.CellPadding = 8;
            gridCash.DisplayLayout.Override.RowSizing = RowSizing.Fixed;
            gridCash.DisplayLayout.Override.DefaultRowHeight = 36;
            gridCash.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            gridCash.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            gridCash.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            gridCash.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;

            // Add grid lines for better visual separation
            gridCash.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            gridCash.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            gridCash.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(148, 163, 184);
            gridCash.DisplayLayout.Override.RowAppearance.BorderColor = Color.FromArgb(148, 163, 184);
            gridCash.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

            // Remove any existing summaries to keep the grid clean
            if (band.Summaries.Count > 0)
            {
                band.Summaries.Clear();
            }

            ResizeGridColumns();
        }

        private void StyleGridHeader(UltraGridColumn column)
        {
            column.Header.Appearance.BackColor = Color.FromArgb(219, 234, 254);
            column.Header.Appearance.BackColor2 = Color.FromArgb(219, 234, 254);
            column.Header.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
            column.Header.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            column.Header.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            column.Header.Appearance.FontData.SizeInPoints = 10F;
            column.Header.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;
            column.Header.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            column.Header.Appearance.BorderColor = Color.FromArgb(96, 165, 250);
            column.Header.Appearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
        }

        private void GridCash_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            ConfigureGrid();
            e.Layout.Override.AllowAddNew = AllowAddNew.No;
            e.Layout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
            e.Layout.Override.SelectTypeRow = SelectType.Single;
            e.Layout.Override.CellClickAction = CellClickAction.EditAndSelectText;
        }

        private void ResizeGridColumns()
        {
            if (gridCash.DisplayLayout.Bands.Count == 0)
                return;

            var band = gridCash.DisplayLayout.Bands[0];
            int width = Math.Max(760, gridCash.ClientSize.Width - 24);

            if (band.Columns.Exists("No"))
                band.Columns["No"].Width = 70;
            if (band.Columns.Exists("Denomination"))
                band.Columns["Denomination"].Width = Math.Max(190, (int)(width * 0.27));
            if (band.Columns.Exists("Quantity"))
                band.Columns["Quantity"].Width = Math.Max(210, (int)(width * 0.30));
            if (band.Columns.Exists("Amount"))
                band.Columns["Amount"].Width = Math.Max(250, width - 70
                    - (band.Columns.Exists("Denomination") ? band.Columns["Denomination"].Width : 0)
                    - (band.Columns.Exists("Quantity") ? band.Columns["Quantity"].Width : 0));
        }

        private void CommitGridEdits()
        {
            try
            {
                gridCash.UpdateData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error committing grid edits: {ex.Message}");
            }
        }

        private void FocusQuantityCell(int rowIndex, bool enterEditMode)
        {
            if (gridCash.Rows.Count == 0)
                return;

            int targetIndex = Math.Max(0, Math.Min(rowIndex, gridCash.Rows.Count - 1));
            var row = gridCash.Rows[targetIndex];
            if (!row.Cells.Exists("Quantity"))
                return;

            gridCash.Focus();
            row.Activate();
            row.Cells["Quantity"].Activate();
            gridCash.ActiveCell = row.Cells["Quantity"];

            if (enterEditMode)
            {
                Action enterEdit = () =>
                {
                    try
                    {
                        gridCash.PerformAction(UltraGridAction.EnterEditMode);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error entering quantity edit mode: {ex.Message}");
                    }
                };

                if (IsHandleCreated)
                    BeginInvoke(enterEdit);
                else
                    enterEdit();
            }
        }

        private void MoveQuantityFocus(int delta)
        {
            CommitGridEdits();

            var activeRow = gridCash.ActiveRow ?? gridCash.ActiveCell?.Row;
            int currentIndex = activeRow != null ? activeRow.Index : 0;
            int targetIndex = Math.Max(0, Math.Min(currentIndex + delta, gridCash.Rows.Count - 1));

            if (activeRow != null)
                UpdateRowAmount(activeRow);

            FocusQuantityCell(targetIndex, true);
        }

        private void GridCash_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                MoveQuantityFocus(1);
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                MoveQuantityFocus(-1);
            }
            else if (e.KeyCode == Keys.Home)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                FocusQuantityCell(0, true);
            }
            else if (e.KeyCode == Keys.End)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                FocusQuantityCell(gridCash.Rows.Count - 1, true);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F8)
            {
                Save();
                return true;
            }

            if (keyData == Keys.F1)
            {
                RibbonClear();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void GridCash_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (e.Cell.Column.Key == "Quantity")
            {
                UpdateRowAmount(e.Cell.Row);
            }
        }

        private void GridCash_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            if (e.Cell.Column.Key != "Quantity")
            {
                return;
            }

            if (e.NewValue == null || e.NewValue == DBNull.Value || string.IsNullOrWhiteSpace(e.NewValue.ToString()))
            {
                e.Cancel = true;
                ResetQuantityCell(e.Cell);
                return;
            }

            if (!int.TryParse(e.NewValue.ToString(), out int quantity) || quantity < 0)
            {
                e.Cancel = true;
                ResetQuantityCell(e.Cell);
            }
        }

        private void ResetQuantityCell(UltraGridCell cell)
        {
            if (cell == null)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                cell.Value = 0;
                UpdateRowAmount(cell.Row);
            }));
        }

        private void UpdateRowAmount(UltraGridRow row)
        {
            try
            {
                decimal denomination = Convert.ToDecimal(row.Cells["Denomination"].Value);
                int quantity = 0;

                if (row.Cells["Quantity"].Value != null && row.Cells["Quantity"].Value != DBNull.Value)
                {
                    int.TryParse(row.Cells["Quantity"].Value.ToString(), out quantity);
                }

                decimal amount = denomination * quantity;
                row.Cells["Amount"].Value = amount;

                int rowIndex = row.Index;
                if (rowIndex < _cashDetails.Count)
                {
                    _cashDetails[rowIndex].Quantity = quantity;
                }

                CalculateTotal();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating row amount: {ex.Message}");
            }
        }

        private void CalculateTotal()
        {
            decimal total = _cashDetails.Sum(x => x.Amount);
            txtTotal.Text = $"₹{total:#,##0.00}";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime closingDate = dtpDate.Value != null ? (DateTime)dtpDate.Value : DateTime.Now;

                // Validation
                if (string.IsNullOrWhiteSpace(txtCounter.Text))
                {
                    MessageBox.Show("Please enter Counter name.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCounter.Focus();
                    return;
                }

                // Check if any cash counted
                if (_cashDetails.Sum(x => x.Quantity) == 0)
                {
                    var result = MessageBox.Show("No cash denominations entered. Do you want to continue?",
                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                        return;
                }

                // Set default reason (no variance check needed)
                _model.DifferenceReason = "Closing completed";

                // Confirmation before save
                var confirmResult = MessageBox.Show("Do you want to save?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmResult == DialogResult.No)
                    return;

                Cursor = Cursors.WaitCursor;
                var salesData = _repo.GetSalesDataSummary(closingDate, txtCounter.Text) ?? new SalesDataSummary();
                var receiptData = _repo.GetCustomerReceiptSummary(closingDate) ?? new CustomerReceiptSummary();
                _salesData = salesData;
                _receiptData = receiptData;

                // Populate model with sales data
                _model.TotalGrossSales = salesData.TotalGrossSales;
                _model.TotalDiscount = salesData.TotalDiscount;
                _model.TotalReturn = salesData.TotalReturn;
                _model.NetSales = salesData.NetSales;
                _model.CashSale = salesData.CashSale;
                _model.CardSale = salesData.CardSale;
                _model.UpiSale = salesData.UpiSale;
                _model.CreditSale = salesData.CreditSale;
                _model.CustomerReceipt = receiptData.CashReceipt;
                _model.TotalCollection = salesData.TotalCollection + receiptData.TotalReceipt;
                _model.TotalBills = salesData.TotalBills;
                _model.CashBills = salesData.CashBills;
                _model.CardBills = salesData.CardBills;
                _model.UpiBills = salesData.UpiBills;

                // Calculate System Expected Cash internally after the cashier enters physical cash.
                _model.SystemExpectedCash = _model.CashSale + _model.CustomerReceipt
                                           - _model.CashRefundAdjusted - _model.MidDayCashSkim;

                _model.PhysicalCashCounted = _cashDetails.Sum(x => x.Amount);
                _model.CashDifference = _model.PhysicalCashCounted - _model.SystemExpectedCash;

                // Populate model
                _model.Counter = txtCounter.Text;
                _model.TransactionDate = dtpDate.Value != null ? (DateTime)dtpDate.Value : DateTime.Now;
                _model.ReportSelection = cboReportSelection.Text;
                _model.CashDetails = _cashDetails.Where(x => x.Quantity > 0).ToList();
                _model.Status = "Closed";
                _model.CompanyId = SessionContext.CompanyId;
                _model.BranchId = SessionContext.BranchId;
                _model.FinYearId = SessionContext.FinYearId;
                _model.UserId = SessionContext.UserId;

                // Save
                bool success = _repo.SaveClosing(_model);

                if (success)
                {
                    MessageBox.Show("Saved Successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Auto-trigger Database Backup on Day End Closing
                    if (_model.ReportSelection == "Day End Closing")
                    {
                        TriggerDayEndBackup();
                    }

                    SessionContext.CounterSessionId = 0;
                    SessionContext.RequiresClosing = false;

                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to save closing.\n\n" +
                        "Possible reasons:\n" +
                        "• Shift already closed for today\n" +
                        "• Database connection error\n" +
                        "• Insufficient permissions\n\n" +
                        "Please check and try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving closing:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearForm()
        {
            foreach (var detail in _cashDetails)
            {
                detail.Quantity = 0;
            }

            gridCash.DataSource = null;
            gridCash.DataSource = _cashDetails;

            // IMPORTANT: Reconfigure grid after rebinding to maintain styling
            ConfigureGrid();

            CalculateTotal();

            txtCounter.Text = !string.IsNullOrWhiteSpace(SessionContext.CounterName)
                ? SessionContext.CounterName
                : (SessionContext.CounterId > 0 ? $"COUNTER{SessionContext.CounterId}" : "Counter-1");
            dtpDate.Value = DateTime.Now;

            _model = new ClosingModel();
            _salesData = null;
            _receiptData = null;

            // Focus on first quantity cell
            if (gridCash.Rows.Count > 0)
            {
                gridCash.ActiveCell = gridCash.Rows[0].Cells["Quantity"];
            }
        }

        /// <summary>
        /// Print the closing report
        /// </summary>
        private void PrintClosingReport()
        {
            try
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = "Closing Report";
                printDocument.PrintPage += PrintDocument_PrintPage;

                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDocument;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Print page event handler
        /// </summary>
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                Font titleFont = new Font("Arial", 14, FontStyle.Bold);
                Font headerFont = new Font("Arial", 10, FontStyle.Bold);
                Font dataFont = new Font("Arial", 9);
                Font totalFont = new Font("Arial", 11, FontStyle.Bold);

                float yPosition = 40;
                float leftMargin = 40;
                float pageWidth = e.PageBounds.Width - 80;

                // Print title
                string title = "SHIFT CLOSING REPORT";
                SizeF titleSize = e.Graphics.MeasureString(title, titleFont);
                e.Graphics.DrawString(title, titleFont, Brushes.Black, (pageWidth - titleSize.Width) / 2 + leftMargin, yPosition);
                yPosition += 35;

                // Draw line
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 15;

                // Print counter and date info
                e.Graphics.DrawString($"Counter: {_model.Counter}", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Date: {_model.TransactionDate:dd-MMM-yyyy HH:mm}", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"User: {SessionContext.UserName}", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Report Type: {_model.ReportSelection}", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 25;

                // Draw line
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 15;

                // Print cash denominations header
                e.Graphics.DrawString("CASH DENOMINATIONS", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 25;

                // Column headers
                float col1 = leftMargin;
                float col2 = leftMargin + 120;
                float col3 = leftMargin + 220;
                float col4 = leftMargin + 320;

                e.Graphics.DrawString("#", headerFont, Brushes.Black, col1, yPosition);
                e.Graphics.DrawString("Denomination", headerFont, Brushes.Black, col2, yPosition);
                e.Graphics.DrawString("Quantity", headerFont, Brushes.Black, col3, yPosition);
                e.Graphics.DrawString("Amount", headerFont, Brushes.Black, col4, yPosition);
                yPosition += 20;

                // Draw line under headers
                e.Graphics.DrawLine(Pens.Gray, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 10;

                // Print each denomination row (only those with quantity > 0)
                int rowNum = 1;
                foreach (var detail in _cashDetails.Where(x => x.Quantity > 0))
                {
                    e.Graphics.DrawString(rowNum.ToString(), dataFont, Brushes.Black, col1, yPosition);
                    e.Graphics.DrawString($"₹{detail.Denomination:N2}", dataFont, Brushes.Black, col2, yPosition);
                    e.Graphics.DrawString(detail.Quantity.ToString(), dataFont, Brushes.Black, col3, yPosition);
                    e.Graphics.DrawString($"₹{detail.Amount:N2}", dataFont, Brushes.Black, col4, yPosition);
                    yPosition += 18;
                    rowNum++;
                }

                yPosition += 10;

                // Draw line before total
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 15;

                // Print total
                decimal totalAmount = _cashDetails.Sum(x => x.Amount);
                e.Graphics.DrawString("TOTAL CASH:", totalFont, Brushes.Black, col2, yPosition);
                e.Graphics.DrawString($"₹{totalAmount:N2}", totalFont, Brushes.Black, col4, yPosition);
                yPosition += 30;

                // Draw double line
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 3;
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 20;

                // Footer
                e.Graphics.DrawString($"Printed on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}", dataFont, Brushes.Gray, leftMargin, yPosition);
            }
            catch (Exception ex)
            {
                e.Graphics.DrawString($"Error printing: {ex.Message}", new Font("Arial", 10), Brushes.Red, 50, 50);
            }
        }

        /// <summary>
        /// Opens the Closing History dialog
        /// </summary>
        private void BtnClosingHistory_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CanViewClosingHistory())
                {
                    MessageBox.Show("Closing history is available only for admin or supervisor users.",
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var historyForm = new frmClosingHistory())
                {
                    historyForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening closing history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TriggerDayEndBackup()
        {
            try
            {
                string backupFolder = string.Empty;

                // Retrieve the configured backup path from company settings
                if (SessionContext.CompanyId > 0)
                {
                    var compRepo = new Repository.MasterRepositry.CompanyRepo();
                    var company = compRepo.GetCompanyById(SessionContext.CompanyId);
                    if (company != null && !string.IsNullOrWhiteSpace(company.BackupPath))
                    {
                        backupFolder = company.BackupPath.Trim();
                    }
                }

                // If no path has been configured, prompt the user to set one
                if (string.IsNullOrWhiteSpace(backupFolder))
                {
                    MessageBox.Show(
                        "No backup folder has been configured yet.\n\n" +
                        "Please go to:\nUtilities → Database → Browse and select a backup folder, then save.\n\n" +
                        "Day End Closing was saved successfully; backup was skipped.",
                        "Backup Path Not Set",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Inform the user the backup is starting
                MessageBox.Show(
                    $"Day End Closing completed successfully.\n\n" +
                    $"Automatic database backup has been initiated to:\n{backupFolder}\n\n" +
                    "The backup process will complete in the background.",
                    "Auto Database Backup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Run backup asynchronously on a background thread
                System.Threading.Tasks.Task.Run(() =>
                {
                    var dbRepo = new Repository.DatabaseRepository();
                    string backupFile;
                    string error;
                    if (dbRepo.BackupDatabase(backupFolder, out backupFile, out error))
                    {
                        System.Diagnostics.Debug.WriteLine($"Automatic Day End Backup completed: {backupFile}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Automatic Day End Backup failed: {error}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initiating auto database backup: {ex.Message}");
            }
        }
    }
}
