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
    public partial class FrmGeneralPayment : Form
    {
        private readonly Dropdowns dropdowns = new Dropdowns();
        private readonly LedgerRepository ledgerRepository = new LedgerRepository();
        private readonly GeneralPaymentRepository paymentRepository = new GeneralPaymentRepository();
        private DataTable ledgerTable;
        private DataTable cashBankTable;
        private DataTable journalLineTable;
        private UltraButton btnHistory;
        private UltraButton btnAddLedger;
        private UltraLabel lblCashBankBalance;
        private long currentVoucherId;
        private bool isBinding;

        public FrmGeneralPayment()
        {
            InitializeComponent();
            ConfigureHistoryButton();
            ConfigureHeaderExtraControls();
            ConfigureGridEvents();
            ConfigureGridDataSource();
            ApplyModernTheme();

            FrmLedgers.LedgerSaved += OnExternalLedgerSaved;
            this.Disposed += (s, e) => { FrmLedgers.LedgerSaved -= OnExternalLedgerSaved; };
            this.VisibleChanged += (s, e) => { if (this.Visible && !isBinding) { BindLedgers(); UpdateCashBankBalanceDisplay(); } };
            this.Enter += (s, e) => { if (!isBinding) { BindLedgers(); UpdateCashBankBalanceDisplay(); } };
        }

        private void OnExternalLedgerSaved(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            BindLedgers();
                            UpdateCashBankBalanceDisplay();
                        }
                    }));
                }
                else
                {
                    BindLedgers();
                    UpdateCashBankBalanceDisplay();
                }
            }
            catch { }
        }

        private void FrmGeneralPayment_Load(object sender, EventArgs e)
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

        private bool IsCashOrBankLedger(DataRow row)
        {
            if (row == null) return false;

            if (row.Table.Columns.Contains("GroupID") && row["GroupID"] != DBNull.Value)
            {
                if (int.TryParse(row["GroupID"].ToString(), out int gid))
                {
                    if (gid == (int)AccountGroup.CASH_IN_HAND ||
                        gid == (int)AccountGroup.BANK_ACCOUNTS ||
                        gid == (int)AccountGroup.BANK_OD_AC)
                    {
                        return true;
                    }
                }
            }

            string groupName = (Convert.ToString(row["GroupName"]) ?? string.Empty).ToUpperInvariant();
            string ledgerName = (Convert.ToString(row["LedgerName"]) ?? string.Empty).ToUpperInvariant();

            if (groupName.Contains("CASH") || groupName.Contains("BANK"))
            {
                return true;
            }

            if (ledgerName.Contains("CASH") || ledgerName.Contains("BANK"))
            {
                if (!ledgerName.Contains("DISCOUNT") && !ledgerName.Contains("EXCESS") && !ledgerName.Contains("SHORT"))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureCashBankLedgerPresent(long ledgerId)
        {
            if (ledgerId <= 0 || cashBankTable == null || ledgerTable == null) return;
            bool exists = cashBankTable.AsEnumerable().Any(r => GetLongValue(r["LedgerID"]) == ledgerId);
            if (!exists)
            {
                var row = ledgerTable.AsEnumerable().FirstOrDefault(r => GetLongValue(r["LedgerID"]) == ledgerId);
                if (row != null)
                {
                    cashBankTable.ImportRow(row);
                }
            }
        }

        private void EnsureGridLedgerValueListContains(int ledgerId, string ledgerName)
        {
            if (gridPayment.DisplayLayout.ValueLists.Exists("LedgerList"))
            {
                var vl = gridPayment.DisplayLayout.ValueLists["LedgerList"];
                if (!vl.ValueListItems.Cast<ValueListItem>().Any(item => Convert.ToInt32(item.DataValue) == ledgerId))
                {
                    vl.ValueListItems.Add(ledgerId, ledgerName);
                }
            }
        }

        private void BindLedgers()
        {
            try
            {
                int branchId = GetSelectedBranchId();
                DataTable allLedgers = ledgerRepository.GetAllLedgers(branchId);
                ledgerTable = allLedgers;

                // Build dedicated Cash/Bank table for header dropdown
                cashBankTable = allLedgers != null ? allLedgers.Clone() : new DataTable();
                if (allLedgers != null && allLedgers.Rows.Count > 0)
                {
                    foreach (DataRow row in allLedgers.Rows)
                    {
                        if (IsCashOrBankLedger(row))
                        {
                            cashBankTable.ImportRow(row);
                        }
                    }
                }

                // Fallback to allLedgers only if no cash/bank accounts exist in database
                if (cashBankTable.Rows.Count == 0 && allLedgers != null && allLedgers.Rows.Count > 0)
                {
                    cashBankTable = allLedgers.Copy();
                }

                long currentVal = GetLongValue(CmboCashBank.Value);
                if (currentVal > 0)
                {
                    EnsureCashBankLedgerPresent(currentVal);
                }

                CmboCashBank.DataSource = cashBankTable;
                CmboCashBank.DisplayMember = "LedgerName";
                CmboCashBank.ValueMember = "LedgerID";

                if (currentVal > 0 && cashBankTable.AsEnumerable().Any(r => GetLongValue(r["LedgerID"]) == currentVal))
                {
                    CmboCashBank.Value = currentVal;
                }
                else if (CmboCashBank.Value == null || Convert.ToInt64(CmboCashBank.Value) <= 0)
                {
                    SetDefaultCashLedger();
                }

                ApplyLedgerValueList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BindLedgers: {ex.Message}");
            }
        }

        private void SetDefaultCashLedger()
        {
            var sourceTable = cashBankTable != null && cashBankTable.Rows.Count > 0 ? cashBankTable : ledgerTable;
            if (sourceTable == null || sourceTable.Rows.Count == 0)
            {
                return;
            }

            // 1. Strictly look for "CASH IN HAND" / "CASH-IN-HAND"
            DataRow defaultCashRow = sourceTable.AsEnumerable()
                .FirstOrDefault(r => {
                    string name = (Convert.ToString(r["LedgerName"]) ?? string.Empty).Replace("-", " ").Trim();
                    return name.IndexOf("CASH IN HAND", StringComparison.OrdinalIgnoreCase) >= 0;
                });

            // 2. Look for exact "CASH", "CASH A/C", "CASH ACCOUNT", "MAIN CASH", "PETTY CASH"
            if (defaultCashRow == null)
            {
                defaultCashRow = sourceTable.AsEnumerable()
                    .FirstOrDefault(r => {
                        string name = (Convert.ToString(r["LedgerName"]) ?? string.Empty).Trim().ToUpperInvariant();
                        if (name.Contains("EXCESS") || name.Contains("SHORTAGE") || name.Contains("DISCOUNT")) return false;
                        return name == "CASH" || name == "CASH A/C" || name == "CASH A/C." ||
                               name == "CASH ACCOUNT" || name == "MAIN CASH" || name == "COUNTER CASH" ||
                               name == "PETTY CASH";
                    });
            }

            // 3. Look for ledger under "CASH-IN-HAND" group that is not excess/shortage/discount
            if (defaultCashRow == null)
            {
                defaultCashRow = sourceTable.AsEnumerable()
                    .FirstOrDefault(r => {
                        string gName = (Convert.ToString(r["GroupName"]) ?? string.Empty).ToUpperInvariant();
                        string lName = (Convert.ToString(r["LedgerName"]) ?? string.Empty).ToUpperInvariant();
                        if (lName.Contains("EXCESS") || lName.Contains("SHORTAGE") || lName.Contains("DISCOUNT")) return false;
                        return gName.Contains("CASH");
                    });
            }

            // 4. Any other non-excess cash ledger
            if (defaultCashRow == null)
            {
                defaultCashRow = sourceTable.AsEnumerable()
                    .FirstOrDefault(r => {
                        string name = (Convert.ToString(r["LedgerName"]) ?? string.Empty).ToUpperInvariant();
                        return name.Contains("CASH") && !name.Contains("EXCESS") && !name.Contains("SHORTAGE") && !name.Contains("DISCOUNT");
                    });
            }

            // 5. Fallback to first row
            if (defaultCashRow == null)
            {
                defaultCashRow = sourceTable.Rows[0];
            }

            if (defaultCashRow != null)
            {
                CmboCashBank.Value = defaultCashRow["LedgerID"];
            }
        }

        private void ConfigureGridDataSource()
        {
            journalLineTable = new DataTable();
            var colLedger = journalLineTable.Columns.Add("LedgerID", typeof(int));
            colLedger.Caption = "Particulars / Paid To";
            var colAmount = journalLineTable.Columns.Add("Amount", typeof(decimal));
            colAmount.Caption = "Amount";
            journalLineTable.ColumnChanged += (sender, args) => UpdateTotals();
            journalLineTable.RowDeleted += (sender, args) => UpdateTotals();
            journalLineTable.RowChanged += (sender, args) => UpdateTotals();

            gridPayment.DataSource = journalLineTable;

            if (gridPayment.DisplayLayout.Bands.Count > 0)
            {
                var band = gridPayment.DisplayLayout.Bands[0];
                if (band.Columns.Exists("LedgerID"))
                {
                    band.Columns["LedgerID"].Header.Caption = "Particulars / Paid To";
                }
                if (band.Columns.Exists("Amount"))
                {
                    band.Columns["Amount"].Header.Caption = "Amount";
                    band.Columns["Amount"].Format = "N2";
                    band.Columns["Amount"].CellAppearance.TextHAlign = HAlign.Right;
                }
            }
        }

        private void ConfigureGridEvents()
        {
            gridPayment.InitializeLayout += gridPayment_InitializeLayout;
            gridPayment.AfterCellUpdate += gridPayment_AfterCellUpdate;
            gridPayment.KeyDown += gridPayment_KeyDown;
            CmboCashBank.BeforeDropDown += (s, e) => { if (!isBinding) BindLedgers(); };
            txtVoucherNo.KeyDown += txtVoucherNo_KeyDown;
            dtpVoucherDate.KeyDown += dtpVoucherDate_KeyDown;
            CmboBranch.KeyDown += CmboBranch_KeyDown;
            CmboCashBank.KeyDown += CmboCashBank_KeyDown;
            txtNarration.KeyDown += txtNarration_KeyDown;
            this.Load += FrmGeneralPayment_Load;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Ribbon Save hotkey: F8 (also supports Ctrl+S, F12)
            if (keyData == Keys.F8 || keyData == (Keys.Control | Keys.S) || keyData == Keys.F12)
            {
                Save();
                return true;
            }
            // Ribbon Clear hotkey: F1 (also supports Ctrl+N)
            if (keyData == Keys.F1 || keyData == (Keys.Control | Keys.N))
            {
                ClearForm();
                return true;
            }
            // Quick Create Ledger hotkey: F2
            if (keyData == Keys.F2)
            {
                OpenQuickLedgerCreation();
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
            // Jump to Description / Narration hotkey: F6 / Alt+N / Alt+D / Ctrl+Enter
            if (keyData == Keys.F6 || keyData == (Keys.Alt | Keys.N) || keyData == (Keys.Alt | Keys.D) || keyData == (Keys.Control | Keys.Enter))
            {
                txtNarration.Focus();
                txtNarration.SelectAll();
                return true;
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

            this.BackColor = pageBack;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            lblHeader.Appearance.BackColor = headerBack;
            lblHeader.Appearance.ForeColor = navy;
            lblHeader.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblHeader.Appearance.FontData.SizeInPoints = 18F;

            headerPanel.Appearance.BackColor = cardBack;
            headerPanel.Appearance.BorderColor = Color.FromArgb(215, 228, 233);
            headerPanel.BorderStyle = UIElementBorderStyle.Solid;
            headerPanel.Height = 86;

            footerPanel.Appearance.BackColor = cardBack;
            footerPanel.Appearance.BorderColor = Color.FromArgb(215, 228, 233);
            footerPanel.BorderStyle = UIElementBorderStyle.Solid;

            narrationPanel.Appearance.BackColor = cardBack;
            narrationPanel.Appearance.BorderColor = Color.FromArgb(215, 228, 233);
            narrationPanel.BorderStyle = UIElementBorderStyle.Solid;

            StyleLabel(lblVocuherNo, navy);
            StyleLabel(lblVoucherDate, navy);
            StyleLabel(lblBranch, navy);
            StyleLabel(lblCashBank, navy);
            StyleLabel(lblNarration, navy);

            StyleInput(txtVoucherNo);
            StyleDate(dtpVoucherDate);
            StyleCombo(CmboBranch);
            StyleCombo(CmboCashBank);
            StyleInput(txtNarration);

            StyleLabel(lblDifference, navy);
            StyleTotalValue(lblDifferenceValue);

            lblTotalDebit.Visible = false;
            lblTotalDebitValue.Visible = false;
            lblTotalCredit.Visible = false;
            lblTotalCreditValue.Visible = false;

            lblDifference.Text = "Total Amount:";
            lblDifference.Appearance.ForeColor = navy;
            lblDifference.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDifference.Appearance.FontData.SizeInPoints = 10.5F;
            lblDifference.Appearance.TextHAlign = HAlign.Right;

            lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(183, 28, 28); // Crimson red for payments
            lblDifferenceValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDifferenceValue.Appearance.FontData.SizeInPoints = 15F;
            lblDifferenceValue.Appearance.TextHAlign = HAlign.Right;

            StyleHistoryButton();
            StyleGrid();
            LayoutHeaderControls();
            PositionSummaryCards();

            this.Resize += (sender, args) =>
            {
                LayoutHeaderControls();
                PositionSummaryCards();
            };
        }

        private void LayoutHeaderControls()
        {
            int topLabel = 14;
            int topInput = 39;
            int left = 28;
            int gap = 16;

            lblVocuherNo.Location = new Point(left, topLabel);
            txtVoucherNo.Location = new Point(left, topInput);
            txtVoucherNo.Size = new Size(160, 30);

            lblVoucherDate.Location = new Point(txtVoucherNo.Right + gap, topLabel);
            dtpVoucherDate.Location = new Point(txtVoucherNo.Right + gap, topInput);
            dtpVoucherDate.Size = new Size(130, 30);

            lblBranch.Location = new Point(dtpVoucherDate.Right + gap, topLabel);
            CmboBranch.Location = new Point(dtpVoucherDate.Right + gap, topInput);
            CmboBranch.Size = new Size(180, 30);

            lblCashBank.Location = new Point(CmboBranch.Right + gap, topLabel);
            CmboCashBank.Location = new Point(CmboBranch.Right + gap, topInput);
            CmboCashBank.Size = new Size(240, 30);

            if (lblCashBankBalance != null)
            {
                lblCashBankBalance.Location = new Point(lblCashBank.Right + 10, topLabel + 1);
            }

            if (btnHistory != null)
            {
                btnHistory.Location = new Point(CmboCashBank.Right + gap, topInput);
                btnHistory.Size = new Size(105, 30);
            }

            if (btnAddLedger != null && btnHistory != null)
            {
                btnAddLedger.Location = new Point(btnHistory.Right + 8, topInput);
                btnAddLedger.Size = new Size(115, 30);
            }
        }

        private void PositionSummaryCards()
        {
            int cardWidth = 240;
            int rightMargin = 28;
            int rightX = footerPanel.ClientArea.Width - cardWidth - rightMargin;
            int labelTop = 14;
            int valueTop = 36;

            if (rightX < 20)
            {
                rightX = 20;
            }

            lblDifference.Location = new Point(rightX, labelTop);
            lblDifference.Size = new Size(cardWidth, 20);
            lblDifference.AutoSize = false;
            lblDifference.Appearance.TextHAlign = HAlign.Right;

            lblDifferenceValue.Location = new Point(rightX, valueTop);
            lblDifferenceValue.Size = new Size(cardWidth, 32);
            lblDifferenceValue.Appearance.TextHAlign = HAlign.Right;
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
            gridPayment.Text = string.Empty;
            gridPayment.DisplayLayout.BorderStyle = UIElementBorderStyle.None;
            gridPayment.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            gridPayment.DisplayLayout.GroupByBox.Hidden = true;
            gridPayment.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            gridPayment.DisplayLayout.Appearance.BackColor = Color.White;
            gridPayment.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(18, 65, 89);
            gridPayment.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(18, 65, 89);
            gridPayment.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            gridPayment.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            gridPayment.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            gridPayment.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            gridPayment.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            gridPayment.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.Select;
            gridPayment.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            gridPayment.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 251, 252);
            gridPayment.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(219, 234, 254);
            gridPayment.DisplayLayout.Override.ActiveCellAppearance.BackColor = Color.FromArgb(239, 246, 255);
            gridPayment.DisplayLayout.Override.CellAppearance.ForeColor = Color.FromArgb(31, 42, 55);
            gridPayment.DisplayLayout.Override.CellPadding = 6;
            gridPayment.DisplayLayout.Override.RowSelectorWidth = 34;
            gridPayment.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
        }

        private void ConfigureHistoryButton()
        {
            btnHistory = new UltraButton
            {
                Text = "History (F5)",
                TabIndex = 6,
                Size = new Size(105, 30)
            };
            btnHistory.Click += btnHistory_Click;
            headerPanel.ClientArea.Controls.Add(btnHistory);
        }

        private void ConfigureHeaderExtraControls()
        {
            lblCashBankBalance = new UltraLabel
            {
                Text = "Available: ₹ 0.00",
                AutoSize = true,
                TabIndex = 99
            };
            lblCashBankBalance.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblCashBankBalance.Appearance.FontData.SizeInPoints = 8.75F;
            lblCashBankBalance.Appearance.ForeColor = Color.FromArgb(22, 101, 52);
            headerPanel.ClientArea.Controls.Add(lblCashBankBalance);

            btnAddLedger = new UltraButton
            {
                Text = "+ Ledger (F2)",
                TabIndex = 7,
                Size = new Size(115, 30)
            };
            btnAddLedger.Appearance.BackColor = Color.FromArgb(16, 110, 80);
            btnAddLedger.Appearance.ForeColor = Color.White;
            btnAddLedger.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnAddLedger.ButtonStyle = UIElementButtonStyle.FlatBorderless;
            btnAddLedger.UseOsThemes = DefaultableBoolean.False;
            btnAddLedger.Click += (s, e) => OpenQuickLedgerCreation();
            headerPanel.ClientArea.Controls.Add(btnAddLedger);

            CmboCashBank.ValueChanged += (s, e) =>
            {
                if (!isBinding)
                {
                    UpdateCashBankBalanceDisplay();
                }
            };
        }

        private void OpenQuickLedgerCreation()
        {
            try
            {
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?? homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabMethod != null)
                    {
                        var frm = new FrmLedgers();
                        frm.FormClosed += (s, e) =>
                        {
                            try
                            {
                                BindLedgers();
                                UpdateCashBankBalanceDisplay();
                            }
                            catch { }
                        };
                        openFormInTabMethod.Invoke(homeForm, new object[] { frm, "Ledger" });
                        return;
                    }
                }

                using (var frm = new FrmLedgers())
                {
                    frm.ShowDialog(this);
                }
                BindLedgers();
                UpdateCashBankBalanceDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Ledger screen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetCashBankCurrentBalance(long ledgerId)
        {
            if (ledgerId <= 0) return 0;
            try
            {
                int compId = GetCompanyId();
                int branchId = GetSelectedBranchId();
                int fyId = GetFinYearId();
                var balances = ledgerRepository.GetLedgerBalances(compId, branchId, fyId, DateTime.Now);
                if (balances != null && balances.TryGetValue((int)ledgerId, out decimal bal))
                {
                    return bal;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching balance: {ex.Message}");
            }
            return 0;
        }

        private void UpdateCashBankBalanceDisplay()
        {
            if (lblCashBankBalance == null) return;
            long ledgerId = GetLongValue(CmboCashBank.Value);
            if (ledgerId <= 0)
            {
                lblCashBankBalance.Text = "Available: ₹ 0.00";
                lblCashBankBalance.Appearance.ForeColor = Color.FromArgb(91, 111, 127);
                return;
            }

            decimal bal = GetCashBankCurrentBalance(ledgerId);
            string sign = bal >= 0 ? "Dr" : "Cr";
            lblCashBankBalance.Text = $"Available: ₹ {Math.Abs(bal):N2} {sign}";

            if (bal > 0)
            {
                lblCashBankBalance.Appearance.ForeColor = Color.FromArgb(22, 101, 52); // Dark Green
            }
            else if (bal < 0)
            {
                lblCashBankBalance.Appearance.ForeColor = Color.FromArgb(185, 28, 28); // Dark Red
            }
            else
            {
                lblCashBankBalance.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            }
        }

        private void gridPayment_InitializeLayout(object sender, InitializeLayoutEventArgs e)
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

            if (band.Columns.Exists("LedgerID"))
            {
                band.Columns["LedgerID"].Header.Caption = "Particulars / Paid To";
                band.Columns["LedgerID"].Width = 650;
                band.Columns["LedgerID"].MinWidth = 300;
                band.Columns["LedgerID"].Header.VisiblePosition = 0;
            }

            if (band.Columns.Exists("Amount"))
            {
                band.Columns["Amount"].Header.Caption = "Amount (₹)";
                band.Columns["Amount"].Width = 250;
                band.Columns["Amount"].MinWidth = 150;
                band.Columns["Amount"].Format = "N2";
                band.Columns["Amount"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["Amount"].Header.VisiblePosition = 1;
            }

            ApplyLedgerValueList();
        }

        private void ApplyLedgerValueList()
        {
            if (ledgerTable == null || gridPayment.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            ValueList ledgerList;
            if (gridPayment.DisplayLayout.ValueLists.Exists("LedgerList"))
            {
                ledgerList = gridPayment.DisplayLayout.ValueLists["LedgerList"];
                ledgerList.ValueListItems.Clear();
            }
            else
            {
                ledgerList = gridPayment.DisplayLayout.ValueLists.Add("LedgerList");
            }

            foreach (DataRow row in ledgerTable.Rows)
            {
                int ledgerId = GetIntValue(row["LedgerID"]);
                if (ledgerId > 0)
                {
                    bool isInExistingLines = journalLineTable != null && journalLineTable.AsEnumerable()
                        .Any(r => r.RowState != DataRowState.Deleted && GetIntValue(r["LedgerID"]) == ledgerId);

                    // Exclude Cash/Bank accounts from grid lines unless already present in an existing line
                    if (!isInExistingLines && IsCashOrBankLedger(row))
                    {
                        continue;
                    }

                    ledgerList.ValueListItems.Add(ledgerId, Convert.ToString(row["LedgerName"]));
                }
            }

            UltraGridBand band = gridPayment.DisplayLayout.Bands[0];
            if (band.Columns.Exists("LedgerID"))
            {
                band.Columns["LedgerID"].ValueList = ledgerList;
                band.Columns["LedgerID"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownValidate;
            }
        }

        public void Save()
        {
            currentVoucherId = 0;
            SavePayment(false);
        }

        public void UpdateRecord()
        {
            SavePayment(true);
        }

        public void RibbonClear() => ClearForm();
        public void Clear() => ClearForm();
        public void ClearFields() => ClearForm();
        public void ClearRecord() => ClearForm();
        public void Reset() => ClearForm();
        public void ResetForm() => ClearForm();
        public void btnClear_Click(object sender, EventArgs e) => ClearForm();
        public void btnReset_Click(object sender, EventArgs e) => ClearForm();

        public void Delete()
        {
            DeletePayment();
        }

        public void LoadVoucher()
        {
            LoadPayment();
        }

        public void ClearForm()
        {
            try
            {
                if (gridPayment != null)
                {
                    gridPayment.PerformAction(UltraGridAction.ExitEditMode);
                    gridPayment.ActiveCell = null;
                    gridPayment.ActiveRow = null;
                }
            }
            catch { }

            ClearRowErrors();

            isBinding = true;
            currentVoucherId = 0;
            txtVoucherNo.Text = string.Empty;
            dtpVoucherDate.Value = DateTime.Today;
            txtNarration.Text = string.Empty;

            if (journalLineTable != null)
            {
                journalLineTable.Clear();
                journalLineTable.Rows.Add(journalLineTable.NewRow());
            }

            SetDefaultCashLedger();

            isBinding = false;
            UpdateTotals();
            UpdateCashBankBalanceDisplay();

            this.BeginInvoke(new Action(() =>
            {
                dtpVoucherDate.Focus();
            }));
        }

        private JournalVoucher BuildPaymentFromGrid()
        {
            long cashBankLedgerId = GetLongValue(CmboCashBank.Value);
            string headerNarration = txtNarration.Text.Trim();

            var journal = new JournalVoucher
            {
                VoucherID = currentVoucherId,
                VoucherNumber = txtVoucherNo.Text.Trim(),
                VoucherDate = GetVoucherDate(),
                Narration = headerNarration,
                BranchID = GetSelectedBranchId(),
                CompanyID = GetCompanyId(),
                FinYearID = GetFinYearId(),
                UserID = SessionContext.UserId > 0 ? SessionContext.UserId : (int.TryParse(DataBase.UserId, out int uId) ? uId : 1),
                UserName = !string.IsNullOrWhiteSpace(SessionContext.UserName) ? SessionContext.UserName : (DataBase.UserName ?? string.Empty)
            };

            decimal totalAmount = 0;
            int slNo = 1;

            // Debit Lines: Paid To / Expense Accounts from Grid (Debit = amount)
            foreach (DataRow row in journalLineTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                long ledgerId = GetLongValue(row["LedgerID"]);
                decimal amount = GetDecimalValue(row["Amount"]);

                if (ledgerId <= 0 && amount == 0)
                {
                    continue;
                }

                totalAmount += amount;

                journal.Lines.Add(new JournalVoucherLine
                {
                    SlNo = slNo++,
                    LedgerID = ledgerId,
                    LedgerName = GetLedgerName(ledgerId),
                    Debit = amount,
                    Credit = 0,
                    Narration = headerNarration
                });
            }

            // Credit Line: Paid From Cash/Bank Account (Credit = totalAmount)
            var cashBankLine = new JournalVoucherLine
            {
                SlNo = slNo++,
                LedgerID = cashBankLedgerId,
                LedgerName = GetLedgerName(cashBankLedgerId),
                Debit = 0,
                Credit = totalAmount,
                Narration = headerNarration
            };
            journal.Lines.Add(cashBankLine);

            return journal;
        }

        private bool ValidatePaymentForSave(JournalVoucher journal)
        {
            ClearRowErrors();

            if (CmboCashBank.Value == null || Convert.ToInt64(CmboCashBank.Value) <= 0)
            {
                MessageBox.Show("Please select Paid From (Cash/Bank) account in header.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CmboCashBank.Focus();
                return false;
            }

            long cashBankId = Convert.ToInt64(CmboCashBank.Value);
            var gridLines = journal.Lines.Where(l => l.Debit > 0).ToList();

            if (gridLines.Count == 0)
            {
                MessageBox.Show("Please enter at least one payment line with ledger and amount.", "Validation",
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
                decimal amount = GetDecimalValue(row["Amount"]);

                if (ledgerId <= 0 && amount == 0)
                {
                    continue;
                }

                if (ledgerId <= 0)
                {
                    row.SetColumnError("LedgerID", "Select ledger.");
                    valid = false;
                }
                else if (ledgerId == cashBankId)
                {
                    row.SetColumnError("LedgerID", "Paid To Ledger cannot be the same as Paid From Cash/Bank account.");
                    valid = false;
                }
                else
                {
                    string accType = ledgerRepository.GetLedgerAccountType(ledgerId);
                    if (accType == "CUSTOMER")
                    {
                        row.SetColumnError("LedgerID", "Please use Customer Refund/Receipt screen for customer transactions.");
                        valid = false;
                    }
                    else if (accType == "SUPPLIER")
                    {
                        row.SetColumnError("LedgerID", "Please use Vendor Payment screen for supplier payments.");
                        valid = false;
                    }
                }

                if (amount <= 0)
                {
                    row.SetColumnError("Amount", "Enter amount.");
                    valid = false;
                }
            }

            decimal totalAmount = gridLines.Sum(l => l.Debit);
            if (totalAmount <= 0)
            {
                MessageBox.Show("Total payment amount must be greater than zero.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Negative cash balance warning
            decimal availableCash = GetCashBankCurrentBalance(cashBankId);
            if (availableCash <= 0 || totalAmount > availableCash)
            {
                string warnMsg = availableCash <= 0
                    ? $"The selected Cash/Bank account currently has zero or negative balance (₹ {availableCash:N2}).\n\nSaving this voucher of ₹ {totalAmount:N2} will result in negative cash.\n\nDo you want to proceed anyway?"
                    : $"The payment amount (₹ {totalAmount:N2}) exceeds the available cash balance (₹ {availableCash:N2}).\n\nRemaining balance will be negative (₹ {availableCash - totalAmount:N2}).\n\nDo you want to proceed anyway?";

                DialogResult warnResult = MessageBox.Show(warnMsg, "Negative Cash Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (warnResult != DialogResult.Yes)
                {
                    return false;
                }
            }

            return valid;
        }

        private void SavePayment(bool requireExisting)
        {
            try
            {
                gridPayment.PerformAction(UltraGridAction.ExitEditMode);

                if (requireExisting && currentVoucherId <= 0)
                {
                    MessageBox.Show("Load an existing payment voucher before updating.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                JournalVoucher journal = BuildPaymentFromGrid();
                if (!ValidatePaymentForSave(journal))
                {
                    UpdateTotals();
                    return;
                }

                string actionText = (requireExisting || currentVoucherId > 0) ? "update" : "save";
                DialogResult confirm = MessageBox.Show(
                    $"Do you want to {actionText} this payment voucher?",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                JournalVoucher saved = paymentRepository.Save(journal);
                currentVoucherId = saved.VoucherID;
                string savedVoucherNumber = saved.VoucherNumber;
                MessageBox.Show($"General Payment voucher {savedVoucherNumber} saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment voucher: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPayment()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtVoucherNo.Text))
                {
                    MessageBox.Show("Enter a voucher number or voucher ID to load.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                JournalVoucher journal = paymentRepository.GetJournalVoucher(txtVoucherNo.Text.Trim());
                if (journal == null)
                {
                    MessageBox.Show("General Payment voucher not found.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                LoadPaymentToForm(journal);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment voucher: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        public void LoadVoucherByNumber(string voucherNumber)
        {
            if (!string.IsNullOrWhiteSpace(voucherNumber))
            {
                txtVoucherNo.Text = voucherNumber.Trim();
                LoadPayment();
            }
        }

        private void LoadPaymentToForm(JournalVoucher journal)
        {
            isBinding = true;
            currentVoucherId = journal.VoucherID;
            txtVoucherNo.Text = journal.VoucherNumber;

            if (journal.VoucherDate != DateTime.MinValue)
            {
                dtpVoucherDate.Value = journal.VoucherDate;
            }

            if (journal.BranchID > 0)
            {
                CmboBranch.Value = journal.BranchID;
            }

            txtNarration.Text = journal.Narration;

            // Find Cash/Bank Credit line
            var creditLine = journal.Lines.FirstOrDefault(l => l.Credit > 0);
            if (creditLine != null && creditLine.LedgerID > 0)
            {
                EnsureCashBankLedgerPresent(creditLine.LedgerID);
                CmboCashBank.Value = creditLine.LedgerID;
            }

            journalLineTable.Clear();
            foreach (JournalVoucherLine line in journal.Lines)
            {
                // Debit lines are the Paid To / Expense lines
                if (line.Debit > 0)
                {
                    DataRow row = journalLineTable.NewRow();
                    row["LedgerID"] = line.LedgerID;
                    row["Amount"] = line.Debit;
                    journalLineTable.Rows.Add(row);
                }
            }

            if (journalLineTable.Rows.Count == 0)
            {
                journalLineTable.Rows.Add(journalLineTable.NewRow());
            }

            isBinding = false;
            UpdateTotals();
            UpdateCashBankBalanceDisplay();
        }

        private void DeletePayment()
        {
            if (currentVoucherId <= 0)
            {
                MessageBox.Show("Load a payment voucher before deleting.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this payment voucher?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                paymentRepository.Delete(currentVoucherId);
                ClearForm();
                MessageBox.Show("General Payment voucher deleted successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting payment voucher: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTotals()
        {
            if (isBinding || journalLineTable == null)
            {
                return;
            }

            decimal totalAmount = 0;
            foreach (DataRow row in journalLineTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                totalAmount += GetDecimalValue(row["Amount"]);
            }

            lblDifferenceValue.Text = totalAmount.ToString("N2");
        }

        private void ClearRowErrors()
        {
            foreach (DataRow row in journalLineTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }
                row.ClearErrors();
                row.RowError = string.Empty;
            }
        }

        private void gridPayment_AfterCellUpdate(object sender, CellEventArgs e)
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
                            MessageBox.Show("Please use Customer Refund/Receipt screen for customer transactions.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void DeleteActiveGridRow()
        {
            UltraGridRow activeRow = gridPayment.ActiveRow;
            if (activeRow == null || !activeRow.IsDataRow)
            {
                return;
            }

            if (gridPayment.Rows.Count <= 1)
            {
                if (activeRow.ListObject is DataRowView singleRowView)
                {
                    singleRowView["LedgerID"] = DBNull.Value;
                    singleRowView["Amount"] = DBNull.Value;
                }
                UpdateTotals();
                return;
            }

            DataRowView rowView = activeRow.ListObject as DataRowView;
            if (rowView != null)
            {
                rowView.Row.Delete();
            }

            UpdateTotals();
        }

        private void ActivateGridCell(int rowIndex, string columnName)
        {
            if (gridPayment.Rows.Count > rowIndex && rowIndex >= 0)
            {
                gridPayment.Focus();
                var row = gridPayment.Rows[rowIndex];
                gridPayment.ActiveRow = row;
                gridPayment.Selected.Rows.Clear();
                gridPayment.Selected.Rows.Add(row);
                if (row.Cells.Exists(columnName))
                {
                    gridPayment.ActiveCell = row.Cells[columnName];
                    gridPayment.PerformAction(UltraGridAction.EnterEditMode);
                }
            }
        }

        private void OpenLedgerSearchForActiveRow()
        {
            if (gridPayment.ActiveRow == null) return;
            using (var searchForm = new PosBranch_Win.DialogBox.FrmLedgerSearch())
            {
                if (searchForm.ShowDialog(this) == DialogResult.OK && searchForm.SelectedLedgerId > 0)
                {
                    if (gridPayment.ActiveRow.ListObject is DataRowView rowView)
                    {
                        EnsureGridLedgerValueListContains(searchForm.SelectedLedgerId, searchForm.SelectedLedgerName);
                        rowView["LedgerID"] = searchForm.SelectedLedgerId;
                        gridPayment.UpdateData();
                        int idx = gridPayment.ActiveRow.Index;
                        this.BeginInvoke(new Action(() =>
                        {
                            ActivateGridCell(idx, "Amount");
                        }));
                    }
                }
            }
        }

        private DateTime GetVoucherDate()
        {
            if (dtpVoucherDate.Value is DateTime date)
            {
                return date.Date;
            }
            return DateTime.Today;
        }

        private int GetSelectedBranchId()
        {
            if (CmboBranch.Value != null && int.TryParse(CmboBranch.Value.ToString(), out int branchId) && branchId > 0)
            {
                return branchId;
            }
            if (SessionContext.BranchId > 0)
            {
                return SessionContext.BranchId;
            }
            return int.TryParse(DataBase.BranchId, out int fallback) && fallback > 0 ? fallback : 1;
        }

        private int GetCompanyId()
        {
            if (SessionContext.CompanyId > 0)
            {
                return SessionContext.CompanyId;
            }
            if (int.TryParse(DataBase.CompanyId, out int compId) && compId > 0)
            {
                return compId;
            }
            return 1;
        }

        private int GetFinYearId()
        {
            if (SessionContext.FinYearId > 0)
            {
                return SessionContext.FinYearId;
            }
            if (int.TryParse(DataBase.FinyearId, out int fyId) && fyId > 0)
            {
                return fyId;
            }
            return 1;
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

        private long GetLongValue(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return long.TryParse(value.ToString(), out long result) ? result : 0;
        }

        private int GetIntValue(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }

        private decimal GetDecimalValue(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return decimal.TryParse(value.ToString(), out decimal result) ? result : 0;
        }

        #region Keyboard Navigation
        private void dtpVoucherDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                CmboCashBank.Focus();
            }
        }

        private void CmboCashBank_KeyDown(object sender, KeyEventArgs e)
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
                CmboCashBank.Focus();
            }
        }

        private void txtVoucherNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                if (!string.IsNullOrWhiteSpace(txtVoucherNo.Text))
                {
                    LoadPayment();
                }
                else
                {
                    dtpVoucherDate.Focus();
                }
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

        private void gridPayment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (gridPayment.ActiveCell == null && gridPayment.ActiveRow != null)
                {
                    gridPayment.ActiveCell = gridPayment.ActiveRow.Cells["LedgerID"];
                }

                if (gridPayment.ActiveCell == null) return;

                string colKey = gridPayment.ActiveCell.Column.Key;
                int rowIndex = gridPayment.ActiveRow.Index;

                if (gridPayment.ActiveCell.IsInEditMode)
                {
                    string cellText = gridPayment.ActiveCell.Text;
                    if (colKey == "Amount" && decimal.TryParse(cellText, out decimal parsedAmt))
                    {
                        gridPayment.ActiveCell.Value = parsedAmt;
                    }
                    gridPayment.PerformAction(UltraGridAction.ExitEditMode);
                }
                gridPayment.UpdateData();

                if (colKey == "LedgerID")
                {
                    long ledgerId = GetLongValue(gridPayment.ActiveCell.Value);
                    if (ledgerId <= 0)
                    {
                        // If empty row, user is done adding items — move focus to Narration / Description
                        txtNarration.Focus();
                        return;
                    }
                    // Move to Amount column on the same row
                    int currRow = rowIndex;
                    this.BeginInvoke(new Action(() => ActivateGridCell(currRow, "Amount")));
                }
                else if (colKey == "Amount")
                {
                    // Ensure entered amount is captured
                    decimal amount = GetDecimalValue(gridPayment.ActiveCell.Value);
                    if (amount <= 0 && decimal.TryParse(gridPayment.ActiveCell.Text, out decimal txtAmt))
                    {
                        gridPayment.ActiveCell.Value = txtAmt;
                        gridPayment.UpdateData();
                    }

                    // Move to next row or add a new row
                    if (rowIndex == gridPayment.Rows.Count - 1)
                    {
                        journalLineTable.Rows.Add(journalLineTable.NewRow());
                    }
                    int nextRow = rowIndex + 1;
                    this.BeginInvoke(new Action(() => ActivateGridCell(nextRow, "LedgerID")));
                }
            }
            else if (e.KeyCode == Keys.Down && (gridPayment.ActiveCell == null || !gridPayment.ActiveCell.IsInEditMode))
            {
                if (gridPayment.ActiveRow != null && gridPayment.ActiveRow.Index == gridPayment.Rows.Count - 1)
                {
                    int nextIndex = gridPayment.Rows.Count;
                    journalLineTable.Rows.Add(journalLineTable.NewRow());
                    this.BeginInvoke(new Action(() => ActivateGridCell(nextIndex, "LedgerID")));
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Delete && (gridPayment.ActiveCell == null || !gridPayment.ActiveCell.IsInEditMode))
            {
                DeleteActiveGridRow();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Tab && (gridPayment.ActiveCell != null && gridPayment.ActiveCell.Column.Key == "Amount"))
            {
                txtNarration.Focus();
                txtNarration.SelectAll();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                OpenLedgerSearchForActiveRow();
                e.Handled = true;
            }
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
            using (var historyForm = new FrmGeneralVoucherHistory("GENPAY"))
            {
                if (historyForm.ShowDialog(this) == DialogResult.OK && historyForm.SelectedVoucherId > 0)
                {
                    txtVoucherNo.Text = historyForm.SelectedVoucherId.ToString();
                    LoadPayment();
                }
            }
        }
        #endregion
    }
}
