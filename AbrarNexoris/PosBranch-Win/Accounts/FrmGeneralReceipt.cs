using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.TransactionModels;
using Repository;
using Repository.Accounts;

namespace PosBranch_Win.Accounts
{
    public partial class FrmGeneralReceipt : Form
    {
        private readonly Dropdowns dropdowns = new Dropdowns();
        private readonly LedgerRepository ledgerRepository = new LedgerRepository();
        private readonly GeneralReceiptRepository receiptRepository = new GeneralReceiptRepository();
        private DataTable ledgerTable;
        private DataTable journalLineTable;
        private UltraButton btnHistory;
        private long currentVoucherId;
        private bool isBinding;

        public FrmGeneralReceipt()
        {
            InitializeComponent();
            ConfigureGridDataSource();
            ConfigureHistoryButton();
            ConfigureGridEvents();
            ApplyModernTheme();
        }

        private void FrmGeneralReceipt_Load(object sender, EventArgs e)
        {
            isBinding = true;
            BindBranches();
            isBinding = false;
            BindLedgers();
            ClearForm();
        }

        private void BindBranches()
        {
            BranchDDlGrid branchDDL = dropdowns.getBanchDDl();
            CmboBranch.DataSource = branchDDL.List;
            CmboBranch.DisplayMember = "BranchName";
            CmboBranch.ValueMember = "Id";

            if (SessionContext.BranchId > 0)
            {
                CmboBranch.Value = SessionContext.BranchId;
            }
            else if (int.TryParse(DataBase.BranchId, out int branchId) && branchId > 0)
            {
                CmboBranch.Value = branchId;
            }

            CmboBranch.ReadOnly = true;
            CmboBranch.TabStop = false;
        }

        private void BindLedgers()
        {
            int branchId = GetSelectedBranchId();
            ledgerTable = ledgerRepository.GetAllLedgers(branchId);
            ApplyLedgerValueList();
        }

        private void ConfigureGridDataSource()
        {
            journalLineTable = new DataTable();
            journalLineTable.Columns.Add("LedgerID", typeof(int));
            journalLineTable.Columns.Add("Debit", typeof(decimal));
            journalLineTable.Columns.Add("Credit", typeof(decimal));
            journalLineTable.Columns.Add("Narration", typeof(string));
            journalLineTable.ColumnChanged += (sender, args) => UpdateTotals();
            journalLineTable.RowDeleted += (sender, args) => UpdateTotals();
            journalLineTable.RowChanged += (sender, args) => UpdateTotals();

            gridReceipt.DataSource = journalLineTable;
        }

        private void ConfigureGridEvents()
        {
            gridReceipt.InitializeLayout += gridReceipt_InitializeLayout;
            gridReceipt.AfterCellUpdate += gridReceipt_AfterCellUpdate;
            gridReceipt.KeyDown += gridReceipt_KeyDown;
            txtVoucherNo.KeyDown += txtVoucherNo_KeyDown;
            dtpVoucherDate.KeyDown += dtpVoucherDate_KeyDown;
            CmboBranch.KeyDown += CmboBranch_KeyDown;
            txtNarration.KeyDown += txtNarration_KeyDown;
            this.Load += FrmGeneralReceipt_Load;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Ribbon Save hotkey: F8 (also supports Ctrl+S, F12)
            if (keyData == Keys.F8 || keyData == (Keys.Control | Keys.S) || keyData == Keys.F12)
            {
                Save();
                return true;
            }
            // Ribbon Clear hotkey: F1 (also supports Ctrl+N, F2)
            if (keyData == Keys.F1 || keyData == (Keys.Control | Keys.N) || keyData == Keys.F2)
            {
                ClearForm();
                return true;
            }
            // Ribbon Exit hotkey: F4
            if (keyData == Keys.F4)
            {
                this.Close();
                return true;
            }
            // Ribbon Delete hotkey: Ctrl+B (also supports Ctrl+D, Ctrl+Delete)
            if (keyData == (Keys.Control | Keys.B) || keyData == (Keys.Control | Keys.D) || keyData == (Keys.Control | Keys.Delete))
            {
                DeleteActiveGridRow();
                return true;
            }
            // History hotkey: F5 / Ctrl+H
            if (keyData == Keys.F5 || keyData == (Keys.Control | Keys.H))
            {
                btnHistory_Click(this, EventArgs.Empty);
                return true;
            }
            // Ledger Search hotkey: F3 / Ctrl+L
            if (keyData == Keys.F3 || keyData == (Keys.Control | Keys.L))
            {
                if (gridReceipt.Focused || gridReceipt.ContainsFocus)
                {
                    OpenLedgerSearchForActiveRow();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ApplyModernTheme()
        {
            Color pageBack = Color.FromArgb(236, 244, 247);
            Color cardBack = Color.FromArgb(248, 251, 252);
            Color muted = Color.FromArgb(91, 111, 127);
            Color navy = Color.FromArgb(8, 47, 73);
            Color headerBack = Color.FromArgb(205, 229, 236);

            BackColor = pageBack;
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            lblHeader.Appearance.BackColor = headerBack;
            lblHeader.Appearance.ForeColor = navy;
            lblHeader.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblHeader.Appearance.FontData.SizeInPoints = 18F;
            lblHeader.Appearance.TextVAlign = VAlign.Middle;
            lblHeader.Appearance.TextHAlign = HAlign.Left;
            lblHeader.Height = 50;
            lblHeader.Padding = new Size(28, 0);

            StylePanel(headerPanel, cardBack);
            headerPanel.Height = 92;
            StylePanel(narrationPanel, cardBack);
            narrationPanel.Height = 100;
            StylePanel(footerPanel, cardBack);
            footerPanel.Height = 78;

            StyleLabel(lblVocuherNo, muted);
            StyleLabel(lblVoucherDate, muted);
            StyleLabel(lblBranch, muted);
            StyleLabel(lblNarration, muted);
            StyleLabel(lblTotalDebit, muted);
            StyleLabel(lblTotalCredit, muted);
            StyleLabel(lblDifference, muted);

            StyleInput(txtVoucherNo);
            StyleInput(txtNarration);
            StyleCombo(CmboBranch);
            StyleDate(dtpVoucherDate);
            StyleTotalValue(lblTotalDebitValue);
            StyleTotalValue(lblTotalCreditValue);
            StyleTotalValue(lblDifferenceValue);
            StyleHistoryButton();

            LayoutHeaderControls();
            LayoutNarrationControls();
            LayoutFooterControls();
            StyleGrid();

            Resize += (sender, args) =>
            {
                LayoutHeaderControls();
                LayoutNarrationControls();
                LayoutFooterControls();
            };
        }

        private void StylePanel(UltraPanel panel, Color backColor)
        {
            panel.Appearance.BackColor = backColor;
            panel.BackColor = backColor;
        }

        private void LayoutHeaderControls()
        {
            int topLabel = 14;
            int topInput = 39;
            int left = 28;
            int gap = 24;

            lblVocuherNo.Location = new Point(left, topLabel);
            txtVoucherNo.Location = new Point(left, topInput);
            txtVoucherNo.Size = new Size(310, 30);

            lblVoucherDate.Location = new Point(txtVoucherNo.Right + gap, topLabel);
            dtpVoucherDate.Location = new Point(txtVoucherNo.Right + gap, topInput);
            dtpVoucherDate.Size = new Size(150, 30);

            lblBranch.Location = new Point(dtpVoucherDate.Right + gap, topLabel);
            CmboBranch.Location = new Point(dtpVoucherDate.Right + gap, topInput);
            CmboBranch.Size = new Size(260, 30);

            btnHistory.Location = new Point(CmboBranch.Right + gap, topInput);
            btnHistory.Size = new Size(110, 30);
        }

        private void LayoutNarrationControls()
        {
            lblNarration.Location = new Point(28, 10);
            txtNarration.Location = new Point(28, 33);
            txtNarration.Size = new Size(Math.Max(200, narrationPanel.ClientSize.Width - 56), 52);
        }

        private void LayoutFooterControls()
        {
            int right = footerPanel.ClientSize.Width - 28;
            int cardWidth = 170;
            int labelTop = 14;
            int valueTop = 36;

            lblDifferenceValue.Location = new Point(right - cardWidth, valueTop);
            lblDifferenceValue.Size = new Size(cardWidth, 30);
            lblDifference.Location = new Point(lblDifferenceValue.Left, labelTop);

            lblTotalCreditValue.Location = new Point(lblDifferenceValue.Left - cardWidth - 34, valueTop);
            lblTotalCreditValue.Size = new Size(cardWidth, 30);
            lblTotalCredit.Location = new Point(lblTotalCreditValue.Left, labelTop);

            lblTotalDebitValue.Location = new Point(lblTotalCreditValue.Left - cardWidth - 34, valueTop);
            lblTotalDebitValue.Size = new Size(cardWidth, 30);
            lblTotalDebit.Location = new Point(lblTotalDebitValue.Left, labelTop);
        }

        private void StyleLabel(UltraLabel label, Color color)
        {
            label.Appearance.ForeColor = color;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = 9.25F;
            label.AutoSize = true;
        }

        private void StyleInput(UltraTextEditor textBox)
        {
            textBox.Appearance.BackColor = Color.White;
            textBox.Appearance.ForeColor = Color.FromArgb(31, 42, 55);
            textBox.Appearance.FontData.SizeInPoints = 10.25F;
            textBox.BorderStyle = UIElementBorderStyle.Solid;
        }

        private void StyleCombo(UltraComboEditor comboBox)
        {
            comboBox.Appearance.BackColor = Color.White;
            comboBox.Appearance.ForeColor = Color.FromArgb(31, 42, 55);
            comboBox.Appearance.FontData.SizeInPoints = 10.25F;
            comboBox.BorderStyle = UIElementBorderStyle.Solid;
        }

        private void StyleDate(UltraDateTimeEditor dateEditor)
        {
            dateEditor.Appearance.BackColor = Color.White;
            dateEditor.Appearance.ForeColor = Color.FromArgb(31, 42, 55);
            dateEditor.Appearance.FontData.SizeInPoints = 10.25F;
            dateEditor.BorderStyle = UIElementBorderStyle.Solid;
        }

        private void StyleTotalValue(UltraLabel label)
        {
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = 13F;
            label.Appearance.TextHAlign = HAlign.Right;
            label.Appearance.TextVAlign = VAlign.Middle;
        }

        private void StyleHistoryButton()
        {
            btnHistory.Appearance.BackColor = Color.FromArgb(18, 65, 89);
            btnHistory.Appearance.ForeColor = Color.White;
            btnHistory.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnHistory.ButtonStyle = UIElementButtonStyle.FlatBorderless;
            btnHistory.UseOsThemes = DefaultableBoolean.False;
        }

        private void StyleGrid()
        {
            gridReceipt.Text = string.Empty;
            gridReceipt.DisplayLayout.BorderStyle = UIElementBorderStyle.None;
            gridReceipt.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            gridReceipt.DisplayLayout.GroupByBox.Hidden = true;
            gridReceipt.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            gridReceipt.DisplayLayout.Appearance.BackColor = Color.White;
            gridReceipt.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(18, 65, 89);
            gridReceipt.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(18, 65, 89);
            gridReceipt.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            gridReceipt.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            gridReceipt.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            gridReceipt.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            gridReceipt.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            gridReceipt.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.Select;
            gridReceipt.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            gridReceipt.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 251, 252);
            gridReceipt.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(219, 234, 254);
            gridReceipt.DisplayLayout.Override.ActiveCellAppearance.BackColor = Color.FromArgb(239, 246, 255);
            gridReceipt.DisplayLayout.Override.CellAppearance.ForeColor = Color.FromArgb(31, 42, 55);
            gridReceipt.DisplayLayout.Override.CellPadding = 6;
            gridReceipt.DisplayLayout.Override.RowSelectorWidth = 34;
            gridReceipt.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            gridReceipt.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            gridReceipt.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            gridReceipt.DisplayLayout.Override.AllowDelete = DefaultableBoolean.True;
            gridReceipt.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            gridReceipt.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            gridReceipt.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            gridReceipt.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;
            gridReceipt.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
        }

        private void ConfigureHistoryButton()
        {
            btnHistory = new UltraButton
            {
                Text = "History (F5)",
                TabIndex = 4
            };
            btnHistory.Click += btnHistory_Click;
            headerPanel.ClientArea.Controls.Add(btnHistory);
        }

        private void gridReceipt_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            e.Layout.CaptionVisible = DefaultableBoolean.False;
            e.Layout.GroupByBox.Hidden = true;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            e.Layout.Override.AllowAddNew = AllowAddNew.No;
            e.Layout.Override.AllowDelete = DefaultableBoolean.True;
            e.Layout.Override.AllowUpdate = DefaultableBoolean.True;
            e.Layout.Override.CellClickAction = CellClickAction.EditAndSelectText;

            UltraGridBand band = e.Layout.Bands[0];
            band.HeaderVisible = false;
            band.Columns["LedgerID"].Header.Caption = "Ledger Name";
            band.Columns["LedgerID"].Width = 420;
            band.Columns["LedgerID"].MinWidth = 260;
            band.Columns["Debit"].Width = 180;
            band.Columns["Debit"].MinWidth = 130;
            band.Columns["Credit"].Width = 180;
            band.Columns["Credit"].MinWidth = 130;
            band.Columns["Narration"].Header.Caption = "Line Narration";
            band.Columns["Narration"].Width = 460;
            band.Columns["Narration"].MinWidth = 260;
            band.Columns["Debit"].Format = "N2";
            band.Columns["Credit"].Format = "N2";
            band.Columns["Debit"].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns["Credit"].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns["Narration"].CellMultiLine = DefaultableBoolean.True;

            if (band.Columns.Exists("LedgerID"))
            {
                band.Columns["LedgerID"].AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            }

            ApplyLedgerValueList();
        }

        private void ApplyLedgerValueList()
        {
            if (ledgerTable == null || gridReceipt.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            ValueList existingList = null;
            foreach (ValueList valueList in gridReceipt.DisplayLayout.ValueLists)
            {
                if (string.Equals(valueList.Key, "LedgerList", StringComparison.OrdinalIgnoreCase))
                {
                    existingList = valueList;
                    break;
                }
            }

            if (existingList != null)
            {
                gridReceipt.DisplayLayout.ValueLists.Remove(existingList);
            }

            ValueList ledgerList = gridReceipt.DisplayLayout.ValueLists.Add("LedgerList");
            foreach (DataRow row in ledgerTable.Rows)
            {
                int ledgerId = GetIntValue(row["LedgerID"]);
                if (ledgerId > 0)
                {
                    ledgerList.ValueListItems.Add(ledgerId, Convert.ToString(row["LedgerName"]));
                }
            }

            UltraGridBand band = gridReceipt.DisplayLayout.Bands[0];
            if (band.Columns.Exists("LedgerID"))
            {
                band.Columns["LedgerID"].ValueList = ledgerList;
                band.Columns["LedgerID"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownValidate;
            }
        }

        public void Save()
        {
            currentVoucherId = 0;
            SaveReceipt(false);
        }

        public void UpdateRecord()
        {
            SaveReceipt(true);
        }

        public void Clear()
        {
            ClearForm();
        }

        public void Delete()
        {
            DeleteReceipt();
        }

        public void LoadVoucher()
        {
            LoadReceipt();
        }

        public void ClearForm()
        {
            isBinding = true;
            currentVoucherId = 0;
            txtVoucherNo.Text = string.Empty;
            dtpVoucherDate.Value = DateTime.Today;
            txtNarration.Text = string.Empty;
            journalLineTable.Clear();
            journalLineTable.Rows.Add(journalLineTable.NewRow());
            isBinding = false;
            UpdateTotals();

            this.BeginInvoke(new Action(() =>
            {
                dtpVoucherDate.Focus();
            }));
        }

        private JournalVoucher BuildReceiptFromGrid()
        {
            var journal = new JournalVoucher
            {
                VoucherID = currentVoucherId,
                VoucherNumber = txtVoucherNo.Text.Trim(),
                VoucherDate = GetVoucherDate(),
                Narration = txtNarration.Text.Trim(),
                BranchID = GetSelectedBranchId()
            };

            int slNo = 1;
            foreach (DataRow row in journalLineTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                long ledgerId = GetLongValue(row["LedgerID"]);
                decimal debit = GetDecimalValue(row["Debit"]);
                decimal credit = GetDecimalValue(row["Credit"]);
                string narration = Convert.ToString(row["Narration"]) ?? string.Empty;

                if (ledgerId <= 0 && debit == 0 && credit == 0 && string.IsNullOrWhiteSpace(narration))
                {
                    continue;
                }

                journal.Lines.Add(new JournalVoucherLine
                {
                    SlNo = slNo++,
                    LedgerID = ledgerId,
                    LedgerName = GetLedgerName(ledgerId),
                    Debit = debit,
                    Credit = credit,
                    Narration = narration.Trim()
                });
            }

            return journal;
        }

        private bool ValidateReceiptForSave(JournalVoucher journal)
        {
            ClearRowErrors();

            if (journal.Lines.Count < 2)
            {
                MessageBox.Show("Please enter at least two receipt lines.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            bool valid = true;
            foreach (DataRow row in journalLineTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                long ledgerId = GetLongValue(row["LedgerID"]);
                decimal debit = GetDecimalValue(row["Debit"]);
                decimal credit = GetDecimalValue(row["Credit"]);

                if (ledgerId <= 0 && debit == 0 && credit == 0)
                {
                    continue;
                }

                if (ledgerId <= 0)
                {
                    row.SetColumnError("LedgerID", "Select ledger.");
                    valid = false;
                }
                else
                {
                    string accType = ledgerRepository.GetLedgerAccountType(ledgerId);
                    if (accType == "CUSTOMER")
                    {
                        row.SetColumnError("LedgerID", "Please use Customer Receipt screen for customer payments.");
                        valid = false;
                    }
                    else if (accType == "SUPPLIER")
                    {
                        row.SetColumnError("LedgerID", "Please use Vendor Payment screen for supplier payments.");
                        valid = false;
                    }
                }

                if (debit < 0 || credit < 0)
                {
                    row.RowError = "Amount cannot be negative.";
                    valid = false;
                }

                if ((debit <= 0 && credit <= 0) || (debit > 0 && credit > 0))
                {
                    row.RowError = "Enter either Debit or Credit.";
                    valid = false;
                }
            }

            if (!valid)
            {
                MessageBox.Show("Please fix highlighted receipt lines.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Math.Round(journal.TotalDebit, 2) != Math.Round(journal.TotalCredit, 2))
            {
                MessageBox.Show("Total Debit must equal Total Credit before saving.", "Receipt Not Balanced",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void LoadReceiptToForm(JournalVoucher journal)
        {
            isBinding = true;
            currentVoucherId = journal.VoucherID;
            txtVoucherNo.Text = journal.VoucherNumber;
            dtpVoucherDate.Value = journal.VoucherDate;
            txtNarration.Text = journal.Narration;
            if (journal.BranchID > 0)
            {
                CmboBranch.Value = journal.BranchID;
            }

            journalLineTable.Clear();
            foreach (var line in journal.Lines.OrderBy(line => line.SlNo))
            {
                DataRow row = journalLineTable.NewRow();
                row["LedgerID"] = Convert.ToInt32(line.LedgerID);
                row["Debit"] = line.Debit == 0 ? (object)DBNull.Value : line.Debit;
                row["Credit"] = line.Credit == 0 ? (object)DBNull.Value : line.Credit;
                row["Narration"] = line.Narration;
                journalLineTable.Rows.Add(row);
            }
            journalLineTable.Rows.Add(journalLineTable.NewRow());

            isBinding = false;
            UpdateTotals();
        }

        private void SaveReceipt(bool requireExisting)
        {
            try
            {
                gridReceipt.PerformAction(UltraGridAction.ExitEditMode);

                if (requireExisting && currentVoucherId <= 0)
                {
                    MessageBox.Show("Load an existing receipt voucher before updating.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                JournalVoucher journal = BuildReceiptFromGrid();
                if (!ValidateReceiptForSave(journal))
                {
                    UpdateTotals();
                    return;
                }

                JournalVoucher saved = receiptRepository.Save(journal);
                currentVoucherId = saved.VoucherID;
                string savedVoucherNumber = saved.VoucherNumber;
                MessageBox.Show($"General Receipt voucher {savedVoucherNumber} saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving receipt voucher: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadReceipt()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtVoucherNo.Text))
                {
                    MessageBox.Show("Enter a voucher number or voucher ID to load.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                JournalVoucher journal = receiptRepository.GetJournalVoucher(txtVoucherNo.Text.Trim());
                if (journal == null)
                {
                    MessageBox.Show("General Receipt voucher not found.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                LoadReceiptToForm(journal);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading receipt voucher: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteReceipt()
        {
            if (currentVoucherId <= 0)
            {
                MessageBox.Show("Load a receipt voucher before deleting.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("Delete this receipt voucher?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                receiptRepository.Delete(currentVoucherId);
                ClearForm();
                MessageBox.Show("General Receipt voucher deleted successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting receipt voucher: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTotals()
        {
            if (isBinding || journalLineTable == null)
            {
                return;
            }

            decimal totalDebit = 0;
            decimal totalCredit = 0;

            foreach (DataRow row in journalLineTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                totalDebit += GetDecimalValue(row["Debit"]);
                totalCredit += GetDecimalValue(row["Credit"]);
            }

            lblTotalDebitValue.Text = totalDebit.ToString("N2");
            lblTotalCreditValue.Text = totalCredit.ToString("N2");
            lblDifferenceValue.Text = Math.Abs(totalDebit - totalCredit).ToString("N2");

            bool balanced = Math.Round(totalDebit, 2) == Math.Round(totalCredit, 2);
            Color statusColor = balanced ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
            lblTotalDebitValue.Appearance.ForeColor = statusColor;
            lblTotalCreditValue.Appearance.ForeColor = statusColor;
            lblDifferenceValue.Appearance.ForeColor = statusColor;
        }

        private void ClearRowErrors()
        {
            foreach (DataRow row in journalLineTable.Rows)
            {
                row.ClearErrors();
                row.RowError = string.Empty;
            }
        }

        private int GetSelectedBranchId()
        {
            if (CmboBranch.Value != null && int.TryParse(CmboBranch.Value.ToString(), out int selectedBranchId))
            {
                return selectedBranchId;
            }

            if (SessionContext.BranchId > 0)
            {
                return SessionContext.BranchId;
            }

            return int.TryParse(DataBase.BranchId, out int branchId) ? branchId : 0;
        }

        private DateTime GetVoucherDate()
        {
            if (dtpVoucherDate.Value is DateTime date)
            {
                return date.Date;
            }

            return DateTime.Today;
        }

        private string GetLedgerName(long ledgerId)
        {
            if (ledgerTable == null || ledgerId <= 0)
            {
                return string.Empty;
            }

            DataRow[] rows = ledgerTable.Select($"LedgerID = {ledgerId}");
            return rows.Length > 0 ? rows[0]["LedgerName"].ToString() : string.Empty;
        }

        private int GetIntValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }

        private long GetLongValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            return long.TryParse(value.ToString(), out long result) ? result : 0;
        }

        private decimal GetDecimalValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            return decimal.TryParse(value.ToString(), out decimal result) ? result : 0;
        }

        private void CmboBranch_ValueChanged(object sender, EventArgs e)
        {
            if (!isBinding)
            {
                BindLedgers();
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            using (var historyForm = new FrmGeneralVoucherHistory("GENREC"))
            {
                if (historyForm.ShowDialog(this) == DialogResult.OK && historyForm.SelectedVoucherId > 0)
                {
                    txtVoucherNo.Text = historyForm.SelectedVoucherId.ToString();
                    LoadReceipt();
                }
            }
        }

        private void gridReceipt_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (e.Cell.Row?.ListObject is DataRowView rowView)
            {
                rowView.Row.ClearErrors();
                rowView.Row.RowError = string.Empty;

                if (e.Cell.Column.Key == "LedgerID")
                {
                    long ledgerId = GetLongValue(e.Cell.Value);
                    if (ledgerId > 0)
                    {
                        string accType = ledgerRepository.GetLedgerAccountType(ledgerId);
                        if (accType == "CUSTOMER")
                        {
                            MessageBox.Show("Please use Customer Receipt screen for customer payments.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            isBinding = true;
                            e.Cell.Value = DBNull.Value;
                            isBinding = false;
                        }
                        else if (accType == "SUPPLIER")
                        {
                            MessageBox.Show("Please use Vendor Payment screen for supplier payments.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            isBinding = true;
                            e.Cell.Value = DBNull.Value;
                            isBinding = false;
                        }
                    }
                }
            }

            UpdateTotals();
        }

        // ── Keyboard Navigation & Helper Methods ─────────────────────────────────

        private void ActivateGridCell(int rowIndex, string columnName)
        {
            if (gridReceipt.Rows.Count > rowIndex && rowIndex >= 0)
            {
                gridReceipt.Focus();
                var row = gridReceipt.Rows[rowIndex];
                gridReceipt.ActiveRow = row;
                gridReceipt.Selected.Rows.Clear();
                gridReceipt.Selected.Rows.Add(row);
                if (row.Cells.Exists(columnName))
                {
                    gridReceipt.ActiveCell = row.Cells[columnName];
                    gridReceipt.PerformAction(UltraGridAction.EnterEditMode);
                }
            }
        }

        private void OpenLedgerSearchForActiveRow()
        {
            if (gridReceipt.ActiveRow == null) return;
            using (var searchForm = new PosBranch_Win.DialogBox.FrmLedgerSearch())
            {
                if (searchForm.ShowDialog(this) == DialogResult.OK && searchForm.SelectedLedgerId > 0)
                {
                    if (gridReceipt.ActiveRow.ListObject is DataRowView rowView)
                    {
                        rowView["LedgerID"] = searchForm.SelectedLedgerId;
                        gridReceipt.UpdateData();
                        int idx = gridReceipt.ActiveRow.Index;
                        this.BeginInvoke(new Action(() =>
                        {
                            ActivateGridCell(idx, "Debit");
                        }));
                    }
                }
            }
        }

        private void DeleteActiveGridRow()
        {
            if (gridReceipt.ActiveRow != null && gridReceipt.ActiveRow.ListObject is DataRowView rowView)
            {
                if (journalLineTable.Rows.Count > 1)
                {
                    rowView.Row.Delete();
                    UpdateTotals();
                }
                else
                {
                    rowView["LedgerID"] = DBNull.Value;
                    rowView["Debit"] = 0;
                    rowView["Credit"] = 0;
                    rowView["Narration"] = string.Empty;
                    UpdateTotals();
                }
            }
        }

        private void gridReceipt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (gridReceipt.ActiveCell == null && gridReceipt.ActiveRow != null)
                {
                    gridReceipt.ActiveCell = gridReceipt.ActiveRow.Cells["LedgerID"];
                }

                if (gridReceipt.ActiveCell == null) return;

                string colKey = gridReceipt.ActiveCell.Column.Key;
                int rowIndex = gridReceipt.ActiveRow.Index;

                gridReceipt.PerformAction(UltraGridAction.ExitEditMode);

                if (colKey == "LedgerID")
                {
                    long ledgerId = GetLongValue(gridReceipt.ActiveCell.Value);
                    if (ledgerId <= 0)
                    {
                        if (rowIndex == gridReceipt.Rows.Count - 1 && journalLineTable.Rows.Count > 1)
                        {
                            decimal totalDebit = 0, totalCredit = 0;
                            foreach (DataRow r in journalLineTable.Rows)
                            {
                                if (r.RowState != DataRowState.Deleted)
                                {
                                    totalDebit += GetDecimalValue(r["Debit"]);
                                    totalCredit += GetDecimalValue(r["Credit"]);
                                }
                            }
                            if (totalDebit > 0 && Math.Round(totalDebit, 2) == Math.Round(totalCredit, 2))
                            {
                                txtNarration.Focus();
                                return;
                            }
                        }
                        OpenLedgerSearchForActiveRow();
                        return;
                    }
                    ActivateGridCell(rowIndex, "Debit");
                }
                else if (colKey == "Debit")
                {
                    decimal debit = GetDecimalValue(gridReceipt.ActiveCell.Value);
                    if (debit > 0)
                    {
                        if (gridReceipt.ActiveRow.Cells.Exists("Credit"))
                            gridReceipt.ActiveRow.Cells["Credit"].Value = 0;
                    }
                    ActivateGridCell(rowIndex, "Credit");
                }
                else if (colKey == "Credit")
                {
                    decimal credit = GetDecimalValue(gridReceipt.ActiveCell.Value);
                    if (credit > 0)
                    {
                        if (gridReceipt.ActiveRow.Cells.Exists("Debit"))
                            gridReceipt.ActiveRow.Cells["Debit"].Value = 0;
                    }
                    ActivateGridCell(rowIndex, "Narration");
                }
                else if (colKey == "Narration")
                {
                    long ledgerId = GetLongValue(gridReceipt.ActiveRow.Cells["LedgerID"].Value);
                    if (rowIndex == gridReceipt.Rows.Count - 1)
                    {
                        if (ledgerId > 0)
                        {
                            DataRow newRow = journalLineTable.NewRow();
                            journalLineTable.Rows.Add(newRow);
                            ActivateGridCell(rowIndex + 1, "LedgerID");
                        }
                        else
                        {
                            txtNarration.Focus();
                        }
                    }
                    else
                    {
                        ActivateGridCell(rowIndex + 1, "LedgerID");
                    }
                }
            }
            else if (e.KeyCode == Keys.Delete && (gridReceipt.ActiveCell == null || !gridReceipt.ActiveCell.IsInEditMode))
            {
                DeleteActiveGridRow();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                OpenLedgerSearchForActiveRow();
                e.Handled = true;
            }
        }

        private void txtVoucherNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                if (!string.IsNullOrWhiteSpace(txtVoucherNo.Text))
                {
                    LoadReceipt();
                }
                else
                {
                    dtpVoucherDate.Focus();
                }
            }
        }

        private void dtpVoucherDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                ActivateGridCell(0, "LedgerID");
            }
        }

        private void CmboBranch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                ActivateGridCell(0, "LedgerID");
            }
        }

        private void txtNarration_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && (e.Control || !txtNarration.Text.Contains("\n")))
            {
                e.Handled = true;
                Save();
            }
        }
    }
}
