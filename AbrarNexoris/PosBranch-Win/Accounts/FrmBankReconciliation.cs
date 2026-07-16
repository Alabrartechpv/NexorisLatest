using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using Repository.Accounts;
using Repository.MasterRepositry;

namespace PosBranch_Win.Accounts
{
    public partial class FrmBankReconciliation : Form
    {
        private readonly BankReconciliationRepository _repository = new BankReconciliationRepository();
        private BankReconciliationResult _currentData;
        private DataTable _allLedgersTable;

        // Modern colour palette (consistent with other Account forms)
        private static readonly Color ClrBackground = Color.FromArgb(243, 244, 246);
        private static readonly Color ClrSurface = Color.White;
        private static readonly Color ClrBorder = Color.FromArgb(209, 213, 219);
        private static readonly Color ClrTextPrimary = Color.FromArgb(17, 24, 39);
        private static readonly Color ClrTextSecondary = Color.FromArgb(75, 85, 99);
        private static readonly Color ClrHeaderBg1 = Color.FromArgb(18, 65, 89);
        private static readonly Color ClrHeaderBg2 = Color.FromArgb(28, 85, 110);

        // Button colours
        private static readonly Color ClrBtnBlue = Color.FromArgb(25, 118, 210);
        private static readonly Color ClrBtnBlue2 = Color.FromArgb(33, 150, 243);
        private static readonly Color ClrBtnSlate = Color.FromArgb(84, 110, 122);
        private static readonly Color ClrBtnSlate2 = Color.FromArgb(96, 125, 139);
        private static readonly Color ClrBtnTeal = Color.FromArgb(0, 121, 107);
        private static readonly Color ClrBtnTeal2 = Color.FromArgb(0, 150, 136);
        private static readonly Color ClrBtnGreen = Color.FromArgb(46, 125, 50);
        private static readonly Color ClrBtnGreen2 = Color.FromArgb(67, 160, 71);

        // Grid row colours
        private static readonly Color ClrReconciled = Color.FromArgb(232, 245, 233);  // light green
        private static readonly Color ClrUnreconciled = Color.FromArgb(255, 253, 231); // light yellow

        public FrmBankReconciliation()
        {
            InitializeComponent();
            ApplyModernTheme();
            InitializeEvents();
        }

        #region Initialisation

        private void InitializeEvents()
        {
            this.Load += FrmBankReconciliation_Load;
            btnLoad.Click += (s, e) => LoadData();
            btnSave.Click += (s, e) => SaveReconciliation();
            btnClear.Click += (s, e) => ClearGrid();
            btnReconcileAll.Click += (s, e) => ReconcileAllRows();
            btnClose.Click += (s, e) => this.Close();

            this.KeyPreview = true;
            this.KeyDown += FrmBankReconciliation_KeyDown;
        }

        private void FrmBankReconciliation_Load(object sender, EventArgs e)
        {
            BindBankLedgers();
            dtpFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpToDate.Value = DateTime.Today;
        }

        private void FrmBankReconciliation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadData();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveReconciliation();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        #endregion

        #region Data Binding

        private void BindBankLedgers()
        {
            try
            {
                int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId);
                _allLedgersTable = new Repository.Accounts.LedgerRepository().GetAllLedgers(branchId);

                if (_allLedgersTable == null) return;

                var bankTable = _allLedgersTable.Clone();
                foreach (DataRow row in _allLedgersTable.Rows)
                {
                    string groupName = Convert.ToString(row["GroupName"]) ?? string.Empty;
                    string ledgerName = Convert.ToString(row["LedgerName"]) ?? string.Empty;
                    string combined = $"{groupName} {ledgerName}".ToUpperInvariant();

                    if (combined.Contains("BANK"))
                    {
                        bankTable.ImportRow(row);
                    }
                }

                cmbBankAccount.DataSource = bankTable;
                cmbBankAccount.ValueMember = "LedgerID";
                cmbBankAccount.DisplayMember = "LedgerName";
                cmbBankAccount.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bank accounts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            if (cmbBankAccount.Value == null || Convert.ToInt32(cmbBankAccount.Value) <= 0)
            {
                MessageBox.Show("Please select a Bank Account.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbBankAccount.Focus();
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                lblStatus.Text = "Loading...";

                int ledgerId = Convert.ToInt32(cmbBankAccount.Value);
                DateTime fromDate = Convert.ToDateTime(dtpFromDate.Value).Date;
                DateTime toDate = Convert.ToDateTime(dtpToDate.Value).Date;

                _currentData = _repository.GetReconciliationData(ledgerId, fromDate, toDate);

                BindGrid(_currentData);
                BindSummary(_currentData.Summary);

                int total = _currentData.Items.Count;
                int reconciled = _currentData.Items.Count(i => i.IsReconciled);
                lblStatus.Text = $"Showing {total} transactions ({reconciled} reconciled, {total - reconciled} pending)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reconciliation data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BindGrid(BankReconciliationResult data)
        {
            // Build a DataTable for the grid
            var dt = new DataTable("Reconciliation");
            dt.Columns.Add("Select", typeof(bool));
            dt.Columns.Add("VoucherID", typeof(long));
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("VoucherDate", typeof(DateTime));
            dt.Columns.Add("VoucherNumber", typeof(string));
            dt.Columns.Add("VoucherType", typeof(string));
            dt.Columns.Add("Particulars", typeof(string));
            dt.Columns.Add("Debit", typeof(decimal));
            dt.Columns.Add("Credit", typeof(decimal));
            dt.Columns.Add("ReconciliationDate", typeof(DateTime));
            dt.Columns.Add("Narration", typeof(string));
            dt.Columns.Add("IsReconciled", typeof(bool));

            foreach (var item in data.Items)
            {
                var row = dt.NewRow();
                row["Select"] = item.IsReconciled;
                row["VoucherID"] = item.VoucherID;
                row["SlNo"] = item.SlNo;
                row["VoucherDate"] = item.VoucherDate;
                row["VoucherNumber"] = item.VoucherNumber;
                row["VoucherType"] = item.VoucherType;
                row["Particulars"] = item.Particulars;
                row["Debit"] = item.Debit;
                row["Credit"] = item.Credit;
                if (item.ReconciliationDate.HasValue)
                    row["ReconciliationDate"] = item.ReconciliationDate.Value;
                else
                    row["ReconciliationDate"] = DBNull.Value;
                row["Narration"] = item.Narration;
                row["IsReconciled"] = item.IsReconciled;
                dt.Rows.Add(row);
            }

            gridReconciliation.DataSource = dt;
            FormatGrid();
        }

        private void FormatGrid()
        {
            if (gridReconciliation.DisplayLayout.Bands.Count == 0) return;

            var band = gridReconciliation.DisplayLayout.Bands[0];
            band.Override.HeaderClickAction = HeaderClickAction.SortMulti;

            // Column visibility and widths
            band.Columns["VoucherID"].Hidden = true;
            band.Columns["SlNo"].Hidden = true;
            band.Columns["IsReconciled"].Hidden = true;

            band.Columns["Select"].Header.Caption = "✓";
            band.Columns["Select"].Width = 40;
            band.Columns["Select"].CellActivation = Activation.AllowEdit;
            band.Columns["Select"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;

            band.Columns["VoucherDate"].Header.Caption = "Date";
            band.Columns["VoucherDate"].Width = 100;
            band.Columns["VoucherDate"].Format = "dd/MM/yyyy";
            band.Columns["VoucherDate"].CellActivation = Activation.NoEdit;

            band.Columns["VoucherNumber"].Header.Caption = "Voucher No";
            band.Columns["VoucherNumber"].Width = 110;
            band.Columns["VoucherNumber"].CellActivation = Activation.NoEdit;

            band.Columns["VoucherType"].Header.Caption = "Type";
            band.Columns["VoucherType"].Width = 80;
            band.Columns["VoucherType"].CellActivation = Activation.NoEdit;

            band.Columns["Particulars"].Header.Caption = "Particulars";
            band.Columns["Particulars"].Width = 180;
            band.Columns["Particulars"].CellActivation = Activation.NoEdit;

            band.Columns["Debit"].Header.Caption = "Debit (Dr)";
            band.Columns["Debit"].Width = 100;
            band.Columns["Debit"].Format = "#,##0.00";
            band.Columns["Debit"].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns["Debit"].CellActivation = Activation.NoEdit;

            band.Columns["Credit"].Header.Caption = "Credit (Cr)";
            band.Columns["Credit"].Width = 100;
            band.Columns["Credit"].Format = "#,##0.00";
            band.Columns["Credit"].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns["Credit"].CellActivation = Activation.NoEdit;

            band.Columns["ReconciliationDate"].Header.Caption = "Reconciled On";
            band.Columns["ReconciliationDate"].Width = 110;
            band.Columns["ReconciliationDate"].Format = "dd/MM/yyyy";
            band.Columns["ReconciliationDate"].CellActivation = Activation.AllowEdit;
            band.Columns["ReconciliationDate"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.Date;

            band.Columns["Narration"].Header.Caption = "Narration";
            band.Columns["Narration"].Width = 160;
            band.Columns["Narration"].CellActivation = Activation.NoEdit;

            // Header styling
            band.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            band.Override.HeaderAppearance.BackColor = ClrHeaderBg1;
            band.Override.HeaderAppearance.BackColor2 = ClrHeaderBg2;
            band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            band.Override.HeaderAppearance.ForeColor = Color.White;
            band.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Override.HeaderAppearance.FontData.SizeInPoints = 9F;

            // Row styling
            band.Override.RowAppearance.BackColor = ClrSurface;
            band.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 249, 250);
            band.Override.RowAppearance.FontData.SizeInPoints = 9F;
            band.Override.CellAppearance.BorderColor = ClrBorder;

            // Active row
            band.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            band.Override.ActiveRowAppearance.ForeColor = ClrTextPrimary;

            // Coloring based on reconciliation status
            gridReconciliation.InitializeRow -= GridReconciliation_InitializeRow;
            gridReconciliation.InitializeRow += GridReconciliation_InitializeRow;
        }

        private void GridReconciliation_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            bool isReconciled = false;
            if (e.Row.Cells["IsReconciled"].Value != null && e.Row.Cells["IsReconciled"].Value != DBNull.Value)
            {
                isReconciled = Convert.ToBoolean(e.Row.Cells["IsReconciled"].Value);
            }

            if (isReconciled)
            {
                e.Row.Appearance.BackColor = ClrReconciled;
            }
            else
            {
                e.Row.Appearance.BackColor = ClrUnreconciled;
            }
        }

        private void BindSummary(BankReconciliationSummary summary)
        {
            lblBooksBalanceValue.Text = summary.BooksBalance.ToString("#,##0.00");
            lblUnclearedReceiptsValue.Text = summary.UnclearedReceipts.ToString("#,##0.00");
            lblUnclearedPaymentsValue.Text = summary.UnclearedPayments.ToString("#,##0.00");
            lblBankBalanceValue.Text = summary.BankBalance.ToString("#,##0.00");
        }

        #endregion

        #region Actions

        private void ReconcileAllRows()
        {
            if (gridReconciliation.Rows.Count == 0) return;

            DateTime reconDate = Convert.ToDateTime(dtpToDate.Value).Date;

            foreach (UltraGridRow row in gridReconciliation.Rows)
            {
                bool alreadyReconciled = Convert.ToBoolean(row.Cells["IsReconciled"].Value);
                if (!alreadyReconciled)
                {
                    row.Cells["Select"].Value = true;
                    row.Cells["ReconciliationDate"].Value = reconDate;
                }
            }

            lblStatus.Text = "All rows marked for reconciliation. Click Save to persist.";
        }

        private void ClearGrid()
        {
            gridReconciliation.DataSource = null;
            lblBooksBalanceValue.Text = "0.00";
            lblUnclearedReceiptsValue.Text = "0.00";
            lblUnclearedPaymentsValue.Text = "0.00";
            lblBankBalanceValue.Text = "0.00";
            lblStatus.Text = "";
            _currentData = null;
        }

        private void SaveReconciliation()
        {
            if (_currentData == null || gridReconciliation.Rows.Count == 0)
            {
                MessageBox.Show("No data to save. Please load data first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                int ledgerId = Convert.ToInt32(cmbBankAccount.Value);
                var itemsToSave = new List<BankReconciliationItem>();

                foreach (UltraGridRow row in gridReconciliation.Rows)
                {
                    bool isSelected = row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value);
                    bool wasOriginallyReconciled = row.Cells["IsReconciled"].Value != null && Convert.ToBoolean(row.Cells["IsReconciled"].Value);

                    // Determine the reconciliation date from the grid
                    DateTime? reconDate = null;
                    if (isSelected)
                    {
                        if (row.Cells["ReconciliationDate"].Value != null && row.Cells["ReconciliationDate"].Value != DBNull.Value)
                        {
                            reconDate = Convert.ToDateTime(row.Cells["ReconciliationDate"].Value);
                        }
                        else
                        {
                            // Default to the "To Date" filter value
                            reconDate = Convert.ToDateTime(dtpToDate.Value).Date;
                        }
                    }

                    // Only save if state has changed
                    if (isSelected != wasOriginallyReconciled)
                    {
                        itemsToSave.Add(new BankReconciliationItem
                        {
                            VoucherID = Convert.ToInt64(row.Cells["VoucherID"].Value),
                            SlNo = Convert.ToInt32(row.Cells["SlNo"].Value),
                            ReconciliationDate = isSelected ? reconDate : null
                        });
                    }
                }

                if (itemsToSave.Count == 0)
                {
                    MessageBox.Show("No changes to save.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int updated = _repository.ReconcileBatch(itemsToSave, ledgerId);

                MessageBox.Show($"{updated} transaction(s) updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reload data to refresh summary
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving reconciliation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Theming & Layout

        private void ApplyModernTheme()
        {
            this.BackColor = ClrBackground;
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10F);

            // Header Panel
            headerPanel.Appearance.BackColor = ClrHeaderBg1;
            headerPanel.Appearance.BackColor2 = ClrHeaderBg2;
            headerPanel.Appearance.BackGradientStyle = GradientStyle.Vertical;
            headerPanel.BorderStyle = UIElementBorderStyle.None;

            lblHeader.Appearance.ForeColor = Color.White;
            lblHeader.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblHeader.Appearance.FontData.SizeInPoints = 16F;
            lblHeader.Appearance.TextVAlign = VAlign.Middle;
            lblHeader.Padding = new Size(24, 0);

            // Labels
            foreach (var ctrl in this.Controls)
            {
                if (ctrl is UltraLabel lbl && lbl != lblHeader && !lbl.Name.Contains("Value"))
                {
                    lbl.Appearance.ForeColor = ClrTextSecondary;
                    lbl.Appearance.FontData.Bold = DefaultableBoolean.True;
                    lbl.AutoSize = true;
                }
            }

            // Summary value labels (bold, dark)
            foreach (var ctrl in this.Controls)
            {
                if (ctrl is UltraLabel lbl && lbl.Name.Contains("Value"))
                {
                    lbl.Appearance.ForeColor = ClrTextPrimary;
                    lbl.Appearance.FontData.Bold = DefaultableBoolean.True;
                    lbl.Appearance.FontData.SizeInPoints = 11F;
                    lbl.AutoSize = true;
                }
            }

            // Special style for Bank Balance value
            lblBankBalanceValue.Appearance.ForeColor = Color.FromArgb(0, 121, 107);
            lblBankBalanceValue.Appearance.FontData.SizeInPoints = 13F;

            // Input controls
            SetFlatInputs(this);

            // Buttons
            StyleGradientButton(btnLoad, ClrBtnBlue, ClrBtnBlue2, Color.FromArgb(21, 101, 192), Color.FromArgb(66, 165, 245), 90);
            StyleGradientButton(btnSave, ClrBtnGreen, ClrBtnGreen2, Color.FromArgb(27, 94, 32), Color.FromArgb(102, 187, 106), 100);
            StyleGradientButton(btnReconcileAll, ClrBtnTeal, ClrBtnTeal2, Color.FromArgb(0, 105, 92), Color.FromArgb(38, 166, 154), 120);
            StyleGradientButton(btnClear, ClrBtnSlate, ClrBtnSlate2, Color.FromArgb(69, 90, 100), Color.FromArgb(120, 144, 156), 100);
            StyleGradientButton(btnClose, ClrBtnSlate, ClrBtnSlate2, Color.FromArgb(69, 90, 100), Color.FromArgb(120, 144, 156), 100);

            // Grid styling
            gridReconciliation.DisplayLayout.Override.RowSelectors = DefaultableBoolean.False;
            gridReconciliation.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            gridReconciliation.DisplayLayout.Appearance.BackColor = ClrSurface;
            gridReconciliation.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Dotted;
            gridReconciliation.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            gridReconciliation.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;

            // Status label
            lblStatus.Appearance.ForeColor = ClrTextSecondary;
            lblStatus.Appearance.FontData.SizeInPoints = 9F;

            LayoutControls();
            this.SizeChanged += (s, e) => LayoutControls();
        }

        private void LayoutControls()
        {
            int pad = 24;
            int topOffset = 72;
            int labelH = 20;
            int inputH = 28;

            // --- Filter Row ---
            lblBankAccount.Location = new Point(pad, topOffset);
            cmbBankAccount.Location = new Point(pad, topOffset + labelH);
            cmbBankAccount.Size = new Size(240, inputH);

            lblFromDate.Location = new Point(cmbBankAccount.Right + 20, topOffset);
            dtpFromDate.Location = new Point(cmbBankAccount.Right + 20, topOffset + labelH);
            dtpFromDate.Size = new Size(140, inputH);

            lblToDate.Location = new Point(dtpFromDate.Right + 20, topOffset);
            dtpToDate.Location = new Point(dtpFromDate.Right + 20, topOffset + labelH);
            dtpToDate.Size = new Size(140, inputH);

            btnLoad.Size = new Size(90, inputH);
            btnLoad.Location = new Point(dtpToDate.Right + 20, topOffset + labelH);

            // --- Summary Row ---
            int summaryTop = cmbBankAccount.Bottom + 20;
            int totalWidth = this.ClientSize.Width - pad * 2;
            int colWidth = Math.Max(180, totalWidth / 4);

            lblBooksBalanceTitle.Location = new Point(pad, summaryTop);
            lblBooksBalanceValue.Location = new Point(pad, summaryTop + labelH + 2);

            lblUnclearedReceiptsTitle.Location = new Point(pad + colWidth, summaryTop);
            lblUnclearedReceiptsValue.Location = new Point(pad + colWidth, summaryTop + labelH + 2);

            lblUnclearedPaymentsTitle.Location = new Point(pad + colWidth * 2, summaryTop);
            lblUnclearedPaymentsValue.Location = new Point(pad + colWidth * 2, summaryTop + labelH + 2);

            lblBankBalanceTitle.Location = new Point(pad + colWidth * 3, summaryTop);
            lblBankBalanceValue.Location = new Point(pad + colWidth * 3, summaryTop + labelH + 2);

            // --- Grid ---
            int gridTop = lblBooksBalanceValue.Bottom + 16;
            int gridBottom = this.ClientSize.Height - 70;
            gridReconciliation.Location = new Point(pad, gridTop);
            gridReconciliation.Size = new Size(this.ClientSize.Width - pad * 2, Math.Max(100, gridBottom - gridTop));

            // --- Bottom Buttons ---
            int btnTop = gridBottom + 10;
            btnSave.Location = new Point(pad, btnTop);
            btnReconcileAll.Location = new Point(btnSave.Right + 12, btnTop);
            btnClear.Location = new Point(btnReconcileAll.Right + 12, btnTop);
            btnClose.Location = new Point(btnClear.Right + 12, btnTop);

            lblStatus.Location = new Point(btnClose.Right + 20, btnTop + 8);
            lblStatus.AutoSize = true;
        }

        private void SetFlatInputs(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is UltraComboEditor cmb)
                {
                    cmb.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
                }
                else if (ctrl is UltraDateTimeEditor dt)
                {
                    dt.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
                }

                if (ctrl.HasChildren)
                {
                    SetFlatInputs(ctrl);
                }
            }
        }

        private void StyleGradientButton(UltraButton button, Color backColor, Color backColor2, Color borderColor, Color hoverColor, int width)
        {
            button.UseOsThemes = DefaultableBoolean.False;
            button.UseAppStyling = false;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.Size = new Size(width, 36);
            button.Appearance.BackColor = backColor;
            button.Appearance.BackColor2 = backColor2;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 9.5F;
            button.Appearance.BorderColor = borderColor;
            button.HotTrackAppearance.BackColor = hoverColor;
            button.HotTrackAppearance.ForeColor = Color.White;
            button.HotTrackAppearance.BorderColor = borderColor;
        }

        #endregion
    }
}
