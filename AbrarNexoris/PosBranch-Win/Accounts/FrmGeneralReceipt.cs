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
        private DataTable cashBankTable;
        private DataTable journalLineTable;
        private UltraButton btnHistory;
        private UltraButton btnAddLedger;
        private UltraLabel lblCashBankBalance;
        private long currentVoucherId;
        private bool isBinding;

        public FrmGeneralReceipt()
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
            try
            {
                int branchId = GetSelectedBranchId();
                DataTable allLedgers = ledgerRepository.GetAllLedgers(branchId);
                ledgerTable = allLedgers;

                long currentVal = GetLongValue(CmboCashBank.Value);
                CmboCashBank.DataSource = allLedgers;
                CmboCashBank.DisplayMember = "LedgerName";
                CmboCashBank.ValueMember = "LedgerID";

                if (currentVal > 0 && allLedgers.AsEnumerable().Any(r => GetLongValue(r["LedgerID"]) == currentVal))
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
            if (ledgerTable == null || ledgerTable.Rows.Count == 0)
            {
                return;
            }

            // 1. Strictly look for "CASH IN HAND" / "CASH-IN-HAND"
            DataRow defaultCashRow = ledgerTable.AsEnumerable()
                .FirstOrDefault(r => {
                    string name = (Convert.ToString(r["LedgerName"]) ?? string.Empty).Replace("-", " ").Trim();
                    return name.IndexOf("CASH IN HAND", StringComparison.OrdinalIgnoreCase) >= 0;
                });

            // 2. Look for exact "CASH", "CASH A/C", "CASH ACCOUNT", "MAIN CASH", "PETTY CASH"
            if (defaultCashRow == null)
            {
                defaultCashRow = ledgerTable.AsEnumerable()
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
                defaultCashRow = ledgerTable.AsEnumerable()
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
                defaultCashRow = ledgerTable.AsEnumerable()
                    .FirstOrDefault(r => {
                        string name = (Convert.ToString(r["LedgerName"]) ?? string.Empty).ToUpperInvariant();
                        return name.Contains("CASH") && !name.Contains("EXCESS") && !name.Contains("SHORTAGE") && !name.Contains("DISCOUNT");
                    });
            }

            // 5. Fallback to first row
            if (defaultCashRow == null)
            {
                defaultCashRow = ledgerTable.Rows[0];
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
            colLedger.Caption = "Received From / Ledger Name";
            var colAmount = journalLineTable.Columns.Add("Amount", typeof(decimal));
            colAmount.Caption = "Amount";
            journalLineTable.ColumnChanged += (sender, args) => UpdateTotals();
            journalLineTable.RowDeleted += (sender, args) => UpdateTotals();
            journalLineTable.RowChanged += (sender, args) => UpdateTotals();

            gridReceipt.DataSource = journalLineTable;

            if (gridReceipt.DisplayLayout.Bands.Count > 0)
            {
                var band = gridReceipt.DisplayLayout.Bands[0];
                if (band.Columns.Exists("LedgerID"))
                {
                    band.Columns["LedgerID"].Header.Caption = "Received From / Ledger Name";
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
            gridReceipt.InitializeLayout += gridReceipt_InitializeLayout;
            gridReceipt.AfterCellUpdate += gridReceipt_AfterCellUpdate;
            gridReceipt.KeyDown += gridReceipt_KeyDown;
            CmboCashBank.BeforeDropDown += (s, e) => { if (!isBinding) BindLedgers(); };
            txtVoucherNo.KeyDown += txtVoucherNo_KeyDown;
            dtpVoucherDate.KeyDown += dtpVoucherDate_KeyDown;
            CmboBranch.KeyDown += CmboBranch_KeyDown;
            CmboCashBank.KeyDown += CmboCashBank_KeyDown;
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
            headerPanel.Height = 86;
            StylePanel(narrationPanel, cardBack);
            narrationPanel.Height = 100;
            StylePanel(footerPanel, cardBack);
            footerPanel.Height = 78;

            StyleLabel(lblVocuherNo, muted);
            StyleLabel(lblVoucherDate, muted);
            StyleLabel(lblBranch, muted);
            StyleLabel(lblCashBank, muted);
            StyleLabel(lblNarration, muted);
            StyleLabel(lblTotalDebit, muted);

            StyleInput(txtVoucherNo);
            StyleInput(txtNarration);
            StyleCombo(CmboBranch);
            StyleCombo(CmboCashBank);
            StyleDate(dtpVoucherDate);
            StyleTotalValue(lblTotalDebitValue);
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

        private void LayoutNarrationControls()
        {
            lblNarration.Location = new Point(28, 10);
            txtNarration.Location = new Point(28, 33);
            txtNarration.Size = new Size(Math.Max(200, narrationPanel.ClientSize.Width - 56), 52);
        }

        private void LayoutFooterControls()
        {
            int cardWidth = 240;
            int right = footerPanel.ClientSize.Width - 28;
            int labelTop = 14;
            int valueTop = 36;
            int rightX = right - cardWidth;

            if (rightX < 20) rightX = 20;

            lblTotalDebit.Location = new Point(rightX, labelTop);
            lblTotalDebit.Size = new Size(cardWidth, 20);
            lblTotalDebit.AutoSize = false;
            lblTotalDebit.Appearance.TextHAlign = HAlign.Right;

            lblTotalDebitValue.Location = new Point(rightX, valueTop);
            lblTotalDebitValue.Size = new Size(cardWidth, 32);
            lblTotalDebitValue.Appearance.TextHAlign = HAlign.Right;
        }

        private void StyleLabel(UltraLabel label, Color color)
        {
            label.Appearance.ForeColor = color;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = 10.5F;
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
            label.Appearance.FontData.SizeInPoints = 15F;
            label.Appearance.ForeColor = Color.FromArgb(22, 101, 52); // Forest green for receipts
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
                TabIndex = 5,
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
            band.Columns["LedgerID"].Header.Caption = "Received From / Ledger Name";
            band.Columns["LedgerID"].Width = 520;
            band.Columns["LedgerID"].MinWidth = 300;
            band.Columns["Amount"].Header.Caption = "Amount";
            band.Columns["Amount"].Width = 240;
            band.Columns["Amount"].MinWidth = 150;
            band.Columns["Amount"].Format = "N2";
            band.Columns["Amount"].CellAppearance.TextHAlign = HAlign.Right;

            ApplyLedgerValueList();
        }

        private void ApplyLedgerValueList()
        {
            if (ledgerTable == null || gridReceipt.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            ValueList ledgerList;
            if (gridReceipt.DisplayLayout.ValueLists.Exists("LedgerList"))
            {
                ledgerList = gridReceipt.DisplayLayout.ValueLists["LedgerList"];
                ledgerList.ValueListItems.Clear();
            }
            else
            {
                ledgerList = gridReceipt.DisplayLayout.ValueLists.Add("LedgerList");
            }

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
            DeleteReceipt();
        }

        public void LoadVoucher()
        {
            LoadReceipt();
        }

        public void ClearForm()
        {
            try
            {
                if (gridReceipt != null)
                {
                    gridReceipt.PerformAction(UltraGridAction.ExitEditMode);
                    gridReceipt.ActiveCell = null;
                    gridReceipt.ActiveRow = null;
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

        private JournalVoucher BuildReceiptFromGrid()
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

            // Debit Line: Cash/Bank Account (SlNo = 1)
            var cashBankLine = new JournalVoucherLine
            {
                SlNo = slNo++,
                LedgerID = cashBankLedgerId,
                LedgerName = GetLedgerName(cashBankLedgerId),
                Debit = 0, // Will update with totalAmount
                Credit = 0,
                Narration = headerNarration
            };
            journal.Lines.Add(cashBankLine);

            // Credit Lines: Received From Accounts from Grid
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
                    Debit = 0,
                    Credit = amount,
                    Narration = headerNarration
                });
            }

            cashBankLine.Debit = totalAmount;

            return journal;
        }

        private bool ValidateReceiptForSave(JournalVoucher journal)
        {
            ClearRowErrors();

            long cashBankLedgerId = GetLongValue(CmboCashBank.Value);
            if (cashBankLedgerId <= 0)
            {
                MessageBox.Show("Please select a Deposit To (Cash/Bank) account.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CmboCashBank.Focus();
                return false;
            }

            // Must have at least 1 credit line (so at least 2 lines total in journal: 1 Cash/Bank + 1 Received From)
            var creditLines = journal.Lines.Where(l => l.Credit > 0 && l.LedgerID > 0).ToList();
            if (creditLines.Count == 0)
            {
                MessageBox.Show("Please enter at least one valid receipt entry with a ledger and amount.", "Validation",
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

                if (amount <= 0)
                {
                    row.SetColumnError("Amount", "Amount must be greater than zero.");
                    valid = false;
                }
            }

            if (!valid)
            {
                MessageBox.Show("Please fix highlighted receipt lines.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (journal.TotalDebit <= 0 || Math.Round(journal.TotalDebit, 2) != Math.Round(journal.TotalCredit, 2))
            {
                MessageBox.Show("Total receipt amount must be greater than zero.", "Receipt Amount Invalid",
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

            // Identify Debit line (Cash/Bank account) vs Credit line(s) (Received From accounts)
            var debitLine = journal.Lines.FirstOrDefault(l => l.Debit > 0);
            if (debitLine != null && debitLine.LedgerID > 0)
            {
                CmboCashBank.Value = debitLine.LedgerID;
            }

            journalLineTable.Clear();
            foreach (var line in journal.Lines.Where(l => l.Credit > 0).OrderBy(l => l.SlNo))
            {
                DataRow row = journalLineTable.NewRow();
                row["LedgerID"] = Convert.ToInt32(line.LedgerID);
                row["Amount"] = line.Credit == 0 ? (object)DBNull.Value : line.Credit;
                journalLineTable.Rows.Add(row);
            }

            if (journalLineTable.Rows.Count == 0)
            {
                journalLineTable.Rows.Add(journalLineTable.NewRow());
            }

            isBinding = false;
            UpdateTotals();
            UpdateCashBankBalanceDisplay();
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

                string actionText = (requireExisting || currentVoucherId > 0) ? "update" : "save";
                DialogResult confirm = MessageBox.Show(
                    $"Do you want to {actionText} this general receipt voucher?",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
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

            decimal totalAmount = 0;

            foreach (DataRow row in journalLineTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                totalAmount += GetDecimalValue(row["Amount"]);
            }

            lblTotalDebitValue.Text = totalAmount.ToString("N2");
            lblTotalDebitValue.Appearance.ForeColor = totalAmount > 0 ? Color.FromArgb(46, 125, 50) : Color.FromArgb(91, 111, 127);
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
            if (CmboBranch.Value != null && int.TryParse(CmboBranch.Value.ToString(), out int selectedBranchId) && selectedBranchId > 0)
            {
                return selectedBranchId;
            }

            if (SessionContext.BranchId > 0)
            {
                return SessionContext.BranchId;
            }

            return int.TryParse(DataBase.BranchId, out int branchId) && branchId > 0 ? branchId : 1;
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
                            ActivateGridCell(idx, "Amount");
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
                    rowView["Amount"] = 0;
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

                if (gridReceipt.ActiveCell.IsInEditMode)
                {
                    string cellText = gridReceipt.ActiveCell.Text;
                    if (colKey == "Amount" && decimal.TryParse(cellText, out decimal parsedAmt))
                    {
                        gridReceipt.ActiveCell.Value = parsedAmt;
                    }
                    gridReceipt.PerformAction(UltraGridAction.ExitEditMode);
                }
                gridReceipt.UpdateData();

                if (colKey == "LedgerID")
                {
                    long ledgerId = GetLongValue(gridReceipt.ActiveCell.Value);
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
                    decimal amount = GetDecimalValue(gridReceipt.ActiveCell.Value);
                    if (amount <= 0 && decimal.TryParse(gridReceipt.ActiveCell.Text, out decimal txtAmt))
                    {
                        gridReceipt.ActiveCell.Value = txtAmt;
                        gridReceipt.UpdateData();
                    }

                    // Move to next row or add a new row
                    if (rowIndex == gridReceipt.Rows.Count - 1)
                    {
                        journalLineTable.Rows.Add(journalLineTable.NewRow());
                    }
                    int nextRow = rowIndex + 1;
                    this.BeginInvoke(new Action(() => ActivateGridCell(nextRow, "LedgerID")));
                }
            }
            else if (e.KeyCode == Keys.Down && (gridReceipt.ActiveCell == null || !gridReceipt.ActiveCell.IsInEditMode))
            {
                if (gridReceipt.ActiveRow != null && gridReceipt.ActiveRow.Index == gridReceipt.Rows.Count - 1)
                {
                    int nextIndex = gridReceipt.Rows.Count;
                    journalLineTable.Rows.Add(journalLineTable.NewRow());
                    this.BeginInvoke(new Action(() => ActivateGridCell(nextIndex, "LedgerID")));
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Delete && (gridReceipt.ActiveCell == null || !gridReceipt.ActiveCell.IsInEditMode))
            {
                DeleteActiveGridRow();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Tab && (gridReceipt.ActiveCell != null && gridReceipt.ActiveCell.Column.Key == "Amount"))
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
                CmboCashBank.Focus();
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

        private void CmboCashBank_KeyDown(object sender, KeyEventArgs e)
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
