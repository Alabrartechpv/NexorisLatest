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
    public partial class FrmContra : Form
    {
        private readonly Dropdowns dropdowns = new Dropdowns();
        private readonly LedgerRepository ledgerRepository = new LedgerRepository();
        private readonly ContraVoucherRepository contraRepository = new ContraVoucherRepository();
        private DataTable ledgerTable;
        private DataTable contraLineTable;
        private UltraButton btnHistory;
        private long currentVoucherId;
        private bool isBinding;

        public FrmContra()
        {
            InitializeComponent();
            ConfigureGridDataSource();
            ConfigureHistoryButton();
            ConfigureGridEvents();
            ApplyModernTheme();
        }

        private void FrmContra_Load(object sender, EventArgs e)
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
        }

        private void BindLedgers()
        {
            int branchId = GetSelectedBranchId();
            DataTable allLedgers = ledgerRepository.GetAllLedgers(branchId);
            ledgerTable = allLedgers.Clone();

            foreach (DataRow row in allLedgers.Rows)
            {
                string groupName = Convert.ToString(row["GroupName"]) ?? string.Empty;
                string ledgerName = Convert.ToString(row["LedgerName"]) ?? string.Empty;
                if (IsCashOrBankLedger(groupName, ledgerName))
                {
                    ledgerTable.ImportRow(row);
                }
            }

            ApplyLedgerValueList();
        }

        private bool IsCashOrBankLedger(string groupName, string ledgerName)
        {
            string value = $"{groupName} {ledgerName}".ToUpperInvariant();
            return value.Contains("CASH") || value.Contains("BANK");
        }

        private void ConfigureGridDataSource()
        {
            contraLineTable = new DataTable();
            contraLineTable.Columns.Add("LedgerID", typeof(int));
            contraLineTable.Columns.Add("Debit", typeof(decimal));
            contraLineTable.Columns.Add("Credit", typeof(decimal));
            contraLineTable.Columns.Add("Narration", typeof(string));
            contraLineTable.ColumnChanged += (sender, args) => UpdateTotals();
            contraLineTable.RowChanged += (sender, args) => UpdateTotals();
            contraLineTable.RowDeleted += (sender, args) => UpdateTotals();
            gridContra.DataSource = contraLineTable;
        }

        private void ConfigureGridEvents()
        {
            gridContra.InitializeLayout += gridContra_InitializeLayout;
            gridContra.AfterCellUpdate += gridContra_AfterCellUpdate;
            gridContra.KeyDown += gridContra_KeyDown;
            txtVoucherNo.KeyDown += txtVoucherNo_KeyDown;
        }

        private void ApplyModernTheme()
        {
            BackColor = Color.FromArgb(236, 244, 247);
            lblHeader.Appearance.BackColor = Color.FromArgb(205, 229, 236);
            lblHeader.Appearance.ForeColor = Color.FromArgb(8, 47, 73);
            lblHeader.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblHeader.Appearance.FontData.SizeInPoints = 18F;
            lblHeader.Appearance.TextVAlign = VAlign.Middle;
            lblHeader.Padding = new Size(28, 0);

            StylePanel(headerPanel);
            StylePanel(narrationPanel);
            StylePanel(footerPanel);
            StyleLabel(lblVocuherNo);
            StyleLabel(lblVoucherDate);
            StyleLabel(lblBranch);
            StyleLabel(lblNarration);
            StyleLabel(lblTotalDebit);
            StyleLabel(lblTotalCredit);
            StyleLabel(lblDifference);
            StyleInput(txtVoucherNo);
            StyleInput(txtNarration);
            StyleCombo(CmboBranch);
            StyleDate(dtpVoucherDate);
            StyleTotalValue(lblTotalDebitValue);
            StyleTotalValue(lblTotalCreditValue);
            StyleTotalValue(lblDifferenceValue);
            StyleHistoryButton();
            LayoutControls();
            StyleGrid();

            Resize += (sender, args) => LayoutControls();
        }

        private void StylePanel(UltraPanel panel)
        {
            panel.Appearance.BackColor = Color.FromArgb(248, 251, 252);
            panel.BackColor = Color.FromArgb(248, 251, 252);
        }

        private void StyleLabel(UltraLabel label)
        {
            label.Appearance.ForeColor = Color.FromArgb(91, 111, 127);
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.AutoSize = true;
        }

        private void StyleInput(UltraTextEditor textBox)
        {
            textBox.Appearance.BackColor = Color.White;
            textBox.Appearance.ForeColor = Color.FromArgb(31, 42, 55);
            textBox.BorderStyle = UIElementBorderStyle.Solid;
        }

        private void StyleCombo(UltraComboEditor comboBox)
        {
            comboBox.Appearance.BackColor = Color.White;
            comboBox.Appearance.ForeColor = Color.FromArgb(31, 42, 55);
            comboBox.BorderStyle = UIElementBorderStyle.Solid;
        }

        private void StyleDate(UltraDateTimeEditor dateEditor)
        {
            dateEditor.Appearance.BackColor = Color.White;
            dateEditor.Appearance.ForeColor = Color.FromArgb(31, 42, 55);
            dateEditor.BorderStyle = UIElementBorderStyle.Solid;
        }

        private void StyleTotalValue(UltraLabel label)
        {
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = 13F;
            label.Appearance.TextHAlign = HAlign.Right;
        }

        private void StyleHistoryButton()
        {
            btnHistory.Appearance.BackColor = Color.FromArgb(18, 65, 89);
            btnHistory.Appearance.ForeColor = Color.White;
            btnHistory.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnHistory.ButtonStyle = UIElementButtonStyle.FlatBorderless;
            btnHistory.UseOsThemes = DefaultableBoolean.False;
        }

        private void ConfigureHistoryButton()
        {
            btnHistory = new UltraButton
            {
                Text = "History"
            };
            btnHistory.Click += btnHistory_Click;
            headerPanel.ClientArea.Controls.Add(btnHistory);
        }

        private void LayoutControls()
        {
            lblVocuherNo.Location = new Point(28, 14);
            txtVoucherNo.Location = new Point(28, 39);
            txtVoucherNo.Size = new Size(310, 30);

            lblVoucherDate.Location = new Point(txtVoucherNo.Right + 24, 14);
            dtpVoucherDate.Location = new Point(txtVoucherNo.Right + 24, 39);
            dtpVoucherDate.Size = new Size(150, 30);

            lblBranch.Location = new Point(dtpVoucherDate.Right + 24, 14);
            CmboBranch.Location = new Point(dtpVoucherDate.Right + 24, 39);
            CmboBranch.Size = new Size(260, 30);

            btnHistory.Location = new Point(CmboBranch.Right + 24, 39);
            btnHistory.Size = new Size(110, 30);

            lblNarration.Location = new Point(28, 10);
            txtNarration.Location = new Point(28, 33);
            txtNarration.Size = new Size(Math.Max(200, narrationPanel.ClientSize.Width - 56), 52);

            int right = footerPanel.ClientSize.Width - 28;
            int cardWidth = 170;
            lblDifferenceValue.Location = new Point(right - cardWidth, 36);
            lblDifferenceValue.Size = new Size(cardWidth, 30);
            lblDifference.Location = new Point(lblDifferenceValue.Left, 14);
            lblTotalCreditValue.Location = new Point(lblDifferenceValue.Left - cardWidth - 34, 36);
            lblTotalCreditValue.Size = new Size(cardWidth, 30);
            lblTotalCredit.Location = new Point(lblTotalCreditValue.Left, 14);
            lblTotalDebitValue.Location = new Point(lblTotalCreditValue.Left - cardWidth - 34, 36);
            lblTotalDebitValue.Size = new Size(cardWidth, 30);
            lblTotalDebit.Location = new Point(lblTotalDebitValue.Left, 14);
        }

        private void StyleGrid()
        {
            gridContra.Text = string.Empty;
            gridContra.DisplayLayout.BorderStyle = UIElementBorderStyle.None;
            gridContra.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            gridContra.DisplayLayout.GroupByBox.Hidden = true;
            gridContra.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            gridContra.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(18, 65, 89);
            gridContra.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            gridContra.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            gridContra.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            gridContra.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            gridContra.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            gridContra.DisplayLayout.Override.AllowAddNew = AllowAddNew.TemplateOnBottom;
            gridContra.DisplayLayout.Override.AllowDelete = DefaultableBoolean.True;
            gridContra.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            gridContra.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            gridContra.DisplayLayout.Override.RowSelectorWidth = 34;
            gridContra.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 251, 252);
        }

        private void gridContra_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            e.Layout.CaptionVisible = DefaultableBoolean.False;
            e.Layout.GroupByBox.Hidden = true;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            UltraGridBand band = e.Layout.Bands[0];
            band.HeaderVisible = false;
            band.Columns["LedgerID"].Header.Caption = "Cash / Bank Ledger";
            band.Columns["LedgerID"].Width = 420;
            band.Columns["LedgerID"].MinWidth = 260;
            band.Columns["Debit"].Width = 180;
            band.Columns["Credit"].Width = 180;
            band.Columns["Narration"].Header.Caption = "Line Narration";
            band.Columns["Narration"].Width = 460;
            band.Columns["Debit"].Format = "N2";
            band.Columns["Credit"].Format = "N2";
            band.Columns["Debit"].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns["Credit"].CellAppearance.TextHAlign = HAlign.Right;
            ApplyLedgerValueList();
        }

        private void ApplyLedgerValueList()
        {
            if (ledgerTable == null || gridContra.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            ValueList existingList = null;
            foreach (ValueList valueList in gridContra.DisplayLayout.ValueLists)
            {
                if (string.Equals(valueList.Key, "ContraLedgerList", StringComparison.OrdinalIgnoreCase))
                {
                    existingList = valueList;
                    break;
                }
            }

            if (existingList != null)
            {
                gridContra.DisplayLayout.ValueLists.Remove(existingList);
            }

            ValueList ledgerList = gridContra.DisplayLayout.ValueLists.Add("ContraLedgerList");
            foreach (DataRow row in ledgerTable.Rows)
            {
                int ledgerId = GetIntValue(row["LedgerID"]);
                if (ledgerId > 0)
                {
                    ledgerList.ValueListItems.Add(ledgerId, Convert.ToString(row["LedgerName"]));
                }
            }

            UltraGridBand band = gridContra.DisplayLayout.Bands[0];
            if (band.Columns.Exists("LedgerID"))
            {
                band.Columns["LedgerID"].ValueList = ledgerList;
                band.Columns["LedgerID"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownValidate;
            }
        }

        public void Save()
        {
            currentVoucherId = 0;
            SaveContra(false);
        }

        public void UpdateRecord()
        {
            SaveContra(true);
        }

        public void Clear()
        {
            ClearForm();
        }

        public void Delete()
        {
            DeleteContra();
        }

        public void LoadVoucher()
        {
            LoadContra();
        }

        private void ClearForm()
        {
            isBinding = true;
            currentVoucherId = 0;
            txtVoucherNo.Text = string.Empty;
            dtpVoucherDate.Value = DateTime.Today;
            txtNarration.Text = string.Empty;
            contraLineTable.Clear();
            contraLineTable.Rows.Add(contraLineTable.NewRow());
            isBinding = false;
            UpdateTotals();
        }

        private JournalVoucher BuildContraFromGrid()
        {
            var contra = new JournalVoucher
            {
                VoucherID = currentVoucherId,
                VoucherNumber = txtVoucherNo.Text.Trim(),
                VoucherDate = GetVoucherDate(),
                Narration = txtNarration.Text.Trim(),
                BranchID = GetSelectedBranchId()
            };

            int slNo = 1;
            foreach (DataRow row in contraLineTable.Rows)
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

                contra.Lines.Add(new JournalVoucherLine
                {
                    SlNo = slNo++,
                    LedgerID = ledgerId,
                    LedgerName = GetLedgerName(ledgerId),
                    Debit = debit,
                    Credit = credit,
                    Narration = narration.Trim()
                });
            }

            return contra;
        }

        private bool ValidateContra(JournalVoucher contra)
        {
            ClearRowErrors();
            if (contra.Lines.Count < 2)
            {
                MessageBox.Show("Please enter at least two contra lines.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            bool valid = true;
            foreach (DataRow row in contraLineTable.Rows)
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

                if (ledgerId <= 0 || !IsAllowedContraLedger(ledgerId))
                {
                    row.SetColumnError("LedgerID", "Select a Cash/Bank ledger.");
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
                MessageBox.Show("Please fix highlighted contra lines.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Math.Round(contra.TotalDebit, 2) != Math.Round(contra.TotalCredit, 2))
            {
                MessageBox.Show("Total Debit must equal Total Credit before saving.", "Contra Not Balanced", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool IsAllowedContraLedger(long ledgerId)
        {
            return ledgerTable != null && ledgerTable.Select($"LedgerID = {ledgerId}").Length > 0;
        }

        private void SaveContra(bool requireExisting)
        {
            try
            {
                gridContra.PerformAction(UltraGridAction.ExitEditMode);
                if (requireExisting && currentVoucherId <= 0)
                {
                    MessageBox.Show("Load an existing contra before updating.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                JournalVoucher contra = BuildContraFromGrid();
                if (!ValidateContra(contra))
                {
                    UpdateTotals();
                    return;
                }

                JournalVoucher saved = contraRepository.Save(contra);
                currentVoucherId = saved.VoucherID;
                string savedVoucherNumber = saved.VoucherNumber;
                MessageBox.Show($"Contra voucher {savedVoucherNumber} saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving contra voucher: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadContra()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtVoucherNo.Text))
                {
                    MessageBox.Show("Enter a voucher number or voucher ID to load.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                JournalVoucher contra = contraRepository.GetJournalVoucher(txtVoucherNo.Text.Trim());
                if (contra == null)
                {
                    MessageBox.Show("Contra voucher not found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                LoadContraToForm(contra);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading contra voucher: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadContraToForm(JournalVoucher contra)
        {
            isBinding = true;
            currentVoucherId = contra.VoucherID;
            txtVoucherNo.Text = contra.VoucherNumber;
            dtpVoucherDate.Value = contra.VoucherDate;
            txtNarration.Text = contra.Narration;
            if (contra.BranchID > 0)
            {
                CmboBranch.Value = contra.BranchID;
            }

            contraLineTable.Clear();
            foreach (var line in contra.Lines.OrderBy(line => line.SlNo))
            {
                DataRow row = contraLineTable.NewRow();
                row["LedgerID"] = Convert.ToInt32(line.LedgerID);
                row["Debit"] = line.Debit == 0 ? (object)DBNull.Value : line.Debit;
                row["Credit"] = line.Credit == 0 ? (object)DBNull.Value : line.Credit;
                row["Narration"] = line.Narration;
                contraLineTable.Rows.Add(row);
            }
            contraLineTable.Rows.Add(contraLineTable.NewRow());
            isBinding = false;
            UpdateTotals();
        }

        private void DeleteContra()
        {
            if (currentVoucherId <= 0)
            {
                MessageBox.Show("Load a contra voucher before deleting.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Delete this contra voucher?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                contraRepository.Delete(currentVoucherId);
                ClearForm();
                MessageBox.Show("Contra voucher deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting contra voucher: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTotals()
        {
            if (isBinding || contraLineTable == null)
            {
                return;
            }

            decimal totalDebit = 0;
            decimal totalCredit = 0;
            foreach (DataRow row in contraLineTable.Rows)
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
            Color statusColor = Math.Round(totalDebit, 2) == Math.Round(totalCredit, 2)
                ? Color.FromArgb(46, 125, 50)
                : Color.FromArgb(198, 40, 40);
            lblTotalDebitValue.Appearance.ForeColor = statusColor;
            lblTotalCreditValue.Appearance.ForeColor = statusColor;
            lblDifferenceValue.Appearance.ForeColor = statusColor;
        }

        private void ClearRowErrors()
        {
            foreach (DataRow row in contraLineTable.Rows)
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
            return dtpVoucherDate.Value is DateTime date ? date.Date : DateTime.Today;
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
            return value == null || value == DBNull.Value ? 0 : int.TryParse(value.ToString(), out int result) ? result : 0;
        }

        private long GetLongValue(object value)
        {
            return value == null || value == DBNull.Value ? 0 : long.TryParse(value.ToString(), out long result) ? result : 0;
        }

        private decimal GetDecimalValue(object value)
        {
            return value == null || value == DBNull.Value ? 0 : decimal.TryParse(value.ToString(), out decimal result) ? result : 0;
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
            using (var historyForm = new global::PosBranch_Win.DialogBox.FrmContraHistory(GetSelectedBranchId()))
            {
                if (historyForm.ShowDialog(this) == DialogResult.OK && historyForm.SelectedVoucherId > 0)
                {
                    txtVoucherNo.Text = historyForm.SelectedVoucherId.ToString();
                    LoadContra();
                }
            }
        }

        private void gridContra_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (e.Cell.Row?.ListObject is DataRowView rowView)
            {
                rowView.Row.ClearErrors();
                rowView.Row.RowError = string.Empty;
            }
            UpdateTotals();
        }

        private void gridContra_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                gridContra.PerformAction(UltraGridAction.NextCellByTab);
                e.Handled = true;
            }
        }

        private void txtVoucherNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(txtVoucherNo.Text))
            {
                LoadContra();
                e.Handled = true;
            }
        }
    }
}
