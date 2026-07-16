using ModelClass.Accounts;
using ModelClass;
using PosBranch_Win.Reports.FinancialReports;
using Repository.Accounts;
using Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Accounts
{
    public partial class FrmManualPartyBalance : Form
    {
        private readonly ManualPartyBalanceRepository _repository;
        private readonly Dictionary<string, string> _customerNameMap;
        private readonly Dictionary<string, string> _vendorNameMap;
        private readonly List<string> _customerMasterNames;
        private readonly List<string> _vendorMasterNames;

        private static readonly Color ClrPrimaryHover = Color.FromArgb(29, 78, 216);
        private static readonly Color ClrSuccess = Color.FromArgb(16, 185, 129);
        private static readonly Color ClrSurface = Color.White;
        private static readonly Color ClrBorder = Color.FromArgb(209, 213, 219);
        private static readonly Color ClrTextPrimary = Color.FromArgb(17, 24, 39);
        private static readonly Color ClrTextSecondary = Color.FromArgb(75, 85, 99);
        private static readonly Color ClrGridHeaderBg = Color.FromArgb(55, 71, 79);
        private static readonly Color ClrGridHeaderBg2 = Color.FromArgb(69, 90, 100);
        private static readonly Color ClrGridAlt = Color.FromArgb(250, 250, 252);
        private static readonly Color ClrGridSel = Color.FromArgb(66, 165, 245);
        private static readonly Color ClrGridHover = Color.FromArgb(227, 242, 253);
        private static readonly Color ClrStatusOpen = Color.FromArgb(21, 128, 61);
        private static readonly Color ClrStatusClosed = Color.FromArgb(107, 114, 128);
        private static readonly Color ClrBtnBlue = Color.FromArgb(25, 118, 210);
        private static readonly Color ClrBtnBlue2 = Color.FromArgb(33, 150, 243);
        private static readonly Color ClrBtnGreen = Color.FromArgb(56, 142, 60);
        private static readonly Color ClrBtnGreen2 = Color.FromArgb(76, 175, 80);
        private static readonly Color ClrBtnOrange = Color.FromArgb(245, 124, 0);
        private static readonly Color ClrBtnOrange2 = Color.FromArgb(255, 152, 0);
        private static readonly Color ClrBtnTeal = Color.FromArgb(0, 121, 107);
        private static readonly Color ClrBtnTeal2 = Color.FromArgb(0, 150, 136);
        private static readonly Color ClrBtnPurple = Color.FromArgb(81, 45, 168);
        private static readonly Color ClrBtnPurple2 = Color.FromArgb(103, 58, 183);
        private static readonly Color ClrBtnSlate = Color.FromArgb(84, 110, 122);
        private static readonly Color ClrBtnSlate2 = Color.FromArgb(96, 125, 139);
        private static readonly Color ClrBtnRed = Color.FromArgb(211, 47, 47);
        private static readonly Color ClrBtnRed2 = Color.FromArgb(244, 67, 54);

        private int _currentEntryId;
        private bool _loadingEntry;
        private Timer _searchDebounce;

        public FrmManualPartyBalance()
        {
            _repository = new ManualPartyBalanceRepository();
            _customerNameMap = BuildNameMap(new CustomerRepositoty().GetCustomerDDL().List?.Select(x => x.LedgerName) ?? Enumerable.Empty<string>());
            _vendorNameMap = BuildNameMap(new VendorRepository().GetVendorDDL().List?.Select(x => x.LedgerName) ?? Enumerable.Empty<string>());
            _customerMasterNames = _customerNameMap.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            _vendorMasterNames = _vendorNameMap.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            InitializeComponent();
            ApplyStyles();

            gridEntries.AfterSelectChange += GridEntries_AfterSelectChange;
            gridEntries.InitializeLayout += GridEntries_InitializeLayout;
            gridEntries.InitializeRow += GridEntries_InitializeRow;
            btnNew.Click += (s, e) => ResetEntryForm();
            btnSave.Click += (s, e) => SaveEntry();
            btnDelete.Click += (s, e) => DeleteEntry();
            btnReport.Click += (s, e) => OpenReport();
            btnRefresh.Click += (s, e) => LoadEntries();
            btnAddSettlement.Click += (s, e) => AddSettlement();
            btnViewHistory.Click += (s, e) => ViewSettlementHistory();

            cmbFilterPartyType.ValueChanged += (s, e) => LoadEntries();
            cmbFilterBalanceType.ValueChanged += (s, e) => LoadEntries();
            chkOpenOnly.CheckedChanged += (s, e) => LoadEntries();
            txtPartyName.Leave += (s, e) => ApplyResolvedPartyName();

            _searchDebounce = new Timer { Interval = 400 };
            _searchDebounce.Tick += (s, e) => { _searchDebounce.Stop(); LoadEntries(); };
            txtSearch.TextChanged += (s, e) => { _searchDebounce.Stop(); _searchDebounce.Start(); };

            Load += (s, e) =>
            {
                cmbPartyType.SelectedIndex = 0;
                cmbBalanceType.SelectedIndex = 0;
                cmbFilterPartyType.SelectedIndex = 0;
                cmbFilterBalanceType.SelectedIndex = 0;
                dtEntryDate.Value = DateTime.Today;
                dtSettlementDate.Value = DateTime.Today;

                ConfigureEntryGrid();
                LoadEntries();

                topSplit.SplitterDistance = (int)(topSplit.Width * 0.60);
            };

            topSplit.SizeChanged += (s, e) =>
            {
                if (topSplit.Width > 0)
                {
                    topSplit.SplitterDistance = (int)(topSplit.Width * 0.60);
                }
            };
        }

        private void ApplyStyles()
        {
            BackColor = Color.FromArgb(243, 244, 246);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9.75f);

            foreach (var grp in new Control[] { groupEntry, groupSettlement, listPanel, filterPanel })
            {
                grp.BackColor = ClrSurface;
            }

            foreach (var grp in new Infragistics.Win.Misc.UltraGroupBox[] { groupEntry, groupSettlement })
            {
                grp.BorderStyle = Infragistics.Win.Misc.GroupBoxBorderStyle.RectangularSolid;
                grp.Appearance.BorderColor = ClrBorder;
                grp.HeaderAppearance.BackColor = ClrSurface;
                grp.HeaderAppearance.ForeColor = ClrTextPrimary;
                grp.HeaderAppearance.FontData.SizeInPoints = 12;
                grp.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.False;
                grp.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
                grp.UseAppStyling = false;
            }

            StyleGradientButton(btnNew, ClrBtnSlate, ClrBtnSlate2, Color.FromArgb(69, 90, 100), Color.FromArgb(120, 144, 156), 86);
            StyleGradientButton(btnSave, ClrBtnBlue, ClrBtnBlue2, Color.FromArgb(21, 101, 192), Color.FromArgb(66, 165, 245), 86);
            StyleGradientButton(btnDelete, ClrBtnRed, ClrBtnRed2, Color.FromArgb(198, 40, 40), Color.FromArgb(239, 83, 80), 92);
            StyleGradientButton(btnReport, ClrBtnTeal, ClrBtnTeal2, Color.FromArgb(0, 105, 92), Color.FromArgb(38, 166, 154), 120);
            StyleGradientButton(btnAddSettlement, ClrBtnGreen, ClrBtnGreen2, Color.FromArgb(46, 125, 50), Color.FromArgb(102, 187, 106), 136);
            StyleGradientButton(btnViewHistory, ClrBtnPurple, ClrBtnPurple2, Color.FromArgb(69, 39, 160), Color.FromArgb(126, 87, 194), 128);
            StyleGradientButton(btnRefresh, ClrBtnOrange, ClrBtnOrange2, Color.FromArgb(230, 81, 0), Color.FromArgb(255, 167, 38), 92);

            foreach (var lbl in new Infragistics.Win.Misc.UltraLabel[] {
                lblPartyType, lblPartyName, lblBalanceType, lblAmount, lblEntryDate,
                lblRemarks, lblSettledAmount, lblRemaining,
                lblSettleAmount, lblSettleDate, lblSettleRemarks,
                lblFilterParty, lblFilterBalance, lblFilterName })
            {
                lbl.Appearance.ForeColor = ClrTextSecondary;
            }

            lblSettledValue.Appearance.ForeColor = ClrPrimaryHover;
            lblSettledValue.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            lblRemainingValue.Appearance.ForeColor = ClrSuccess;
            lblRemainingValue.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            lblSettlementRemaining.Appearance.ForeColor = ClrPrimaryHover;
            lblSettlementRemaining.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            lblSettlementTarget.Appearance.ForeColor = ClrTextPrimary;

            foreach (Control ctrl in Controls)
            {
                SetFlatInputs(ctrl);
            }
        }

        private static void StyleGradientButton(Infragistics.Win.Misc.UltraButton button, Color backColor, Color backColor2, Color borderColor, Color hoverColor, int width)
        {
            button.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            button.UseAppStyling = false;
            button.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            button.Size = new Size(width, 36);
            button.Appearance.BackColor = backColor;
            button.Appearance.BackColor2 = backColor2;
            button.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 9.5f;
            button.Appearance.BorderColor = borderColor;
            button.HotTrackAppearance.BackColor = hoverColor;
            button.HotTrackAppearance.ForeColor = Color.White;
            button.HotTrackAppearance.BorderColor = borderColor;
        }

        private void SetFlatInputs(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Infragistics.Win.UltraWinEditors.UltraTextEditor txt)
                {
                    txt.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
                }
                else if (ctrl is Infragistics.Win.UltraWinEditors.UltraComboEditor cmb)
                {
                    cmb.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
                }
                else if (ctrl is Infragistics.Win.UltraWinEditors.UltraNumericEditor num)
                {
                    num.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
                }
                else if (ctrl is Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dt)
                {
                    dt.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
                }

                if (ctrl.HasChildren)
                {
                    SetFlatInputs(ctrl);
                }
            }
        }

        private void StyleGrid(Infragistics.Win.UltraWinGrid.UltraGrid grid)
        {
            grid.UseAppStyling = false;
            grid.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            var layout = grid.DisplayLayout;
            layout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            layout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            layout.Appearance.BorderColor = ClrBorder;
            layout.GroupByBox.Hidden = true;

            layout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.Standard;
            layout.Override.HeaderAppearance.BackColor = ClrGridHeaderBg;
            layout.Override.HeaderAppearance.BackColor2 = ClrGridHeaderBg2;
            layout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;
            layout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            layout.Override.HeaderAppearance.BorderColor = ClrGridHeaderBg2;

            layout.Override.RowAppearance.BackColor = ClrSurface;
            layout.Override.RowAlternateAppearance.BackColor = ClrGridAlt;
            layout.Override.SelectedRowAppearance.BackColor = ClrGridSel;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            layout.Override.ActiveRowAppearance.BackColor = ClrGridHover;
            layout.Override.ActiveRowAppearance.ForeColor = ClrTextPrimary;
            layout.Override.ActiveRowAppearance.BorderColor = ClrGridSel;
            layout.Override.HotTrackRowAppearance.BackColor = ClrGridHover;
            layout.Override.HotTrackRowAppearance.ForeColor = ClrTextPrimary;

            layout.Override.DefaultRowHeight = 30;
            layout.Override.CellAppearance.FontData.SizeInPoints = 9.5f;
            layout.Override.CellAppearance.BorderColor = ClrBorder;
            layout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            layout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
            layout.Override.RowSelectorAppearance.BackColor = ClrGridHeaderBg2;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            layout.Override.RowSelectorWidth = 28;
            layout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
            layout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;
            layout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
            layout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
            layout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
            layout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
        }

        private void ConfigureEntryGrid()
        {
            StyleGrid(gridEntries);
        }

        private void GridEntries_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
            {
                return;
            }

            ConfigureEntryGridColumns(e.Layout.Bands[0]);
        }

        private void ConfigureEntryGridColumns(Infragistics.Win.UltraWinGrid.UltraGridBand band)
        {
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            ConfigureGridColumn(band, "EntryDate", "Date", 110, "dd-MMM-yyyy");
            ConfigureGridColumn(band, "PartyType", "Party Type", 95);
            ConfigureGridColumn(band, "PartyName", "Party Name", 220);
            ConfigureGridColumn(band, "BalanceType", "Type", 90);
            ConfigureGridColumn(band, "Amount", "Amount", 120, "N2", Infragistics.Win.HAlign.Right);
            ConfigureGridColumn(band, "SettledAmount", "Settled", 120, "N2", Infragistics.Win.HAlign.Right);
            ConfigureGridColumn(band, "RemainingAmount", "Remaining", 125, "N2", Infragistics.Win.HAlign.Right);
            ConfigureGridColumn(band, "Status", "Status", 90, null, Infragistics.Win.HAlign.Center);
            ConfigureGridColumn(band, "Remarks", "Remarks", 260);
        }

        private static void ConfigureGridColumn(Infragistics.Win.UltraWinGrid.UltraGridBand band, string key, string header, int width, string format = null, Infragistics.Win.HAlign textAlign = Infragistics.Win.HAlign.Left)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            var col = band.Columns[key];
            col.Hidden = false;
            col.Header.Caption = header;
            col.Width = width;
            col.CellAppearance.TextHAlign = textAlign;

            if (!string.IsNullOrEmpty(format))
            {
                col.Format = format;
            }
        }

        private void GridEntries_InitializeRow(object sender, Infragistics.Win.UltraWinGrid.InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("RemainingAmount"))
            {
                e.Row.Cells["RemainingAmount"].Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                e.Row.Cells["RemainingAmount"].Appearance.ForeColor = ClrPrimaryHover;
            }

            if (e.Row.Cells.Exists("Status"))
            {
                var statusCell = e.Row.Cells["Status"];
                statusCell.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                statusCell.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;

                string status = statusCell.Value?.ToString() ?? string.Empty;
                if (string.Equals(status, "Open", StringComparison.OrdinalIgnoreCase))
                {
                    statusCell.Appearance.ForeColor = ClrStatusOpen;
                }
                else
                {
                    statusCell.Appearance.ForeColor = ClrStatusClosed;
                }
            }
        }

        private void LoadEntries(int? focusEntryId = null)
        {
            List<ManualPartyBalanceEntry> entries = _repository.GetEntries(
                NormalizeFilter(cmbFilterPartyType),
                NormalizeFilter(cmbFilterBalanceType),
                txtSearch?.Text?.Trim(),
                chkOpenOnly?.Checked ?? false);

            gridEntries.DataSource = entries;

            if (gridEntries.DisplayLayout.Bands.Count > 0)
            {
                ConfigureEntryGridColumns(gridEntries.DisplayLayout.Bands[0]);
            }

            if (entries.Count == 0)
            {
                ResetEntryForm(false);
                return;
            }

            int targetId = focusEntryId ?? _currentEntryId;
            if (targetId > 0)
            {
                foreach (var row in gridEntries.Rows)
                {
                    if (row.ListObject is ManualPartyBalanceEntry entry && entry.Id == targetId)
                    {
                        row.Selected = true;
                        row.Activate();
                        LoadSelectedEntry(entry);
                        return;
                    }
                }
            }

            gridEntries.Rows[0].Selected = true;
            gridEntries.Rows[0].Activate();
            if (gridEntries.Rows[0].ListObject is ManualPartyBalanceEntry firstEntry)
            {
                LoadSelectedEntry(firstEntry);
            }
        }

        private void GridEntries_AfterSelectChange(object sender, Infragistics.Win.UltraWinGrid.AfterSelectChangeEventArgs e)
        {
            if (_loadingEntry || gridEntries.ActiveRow == null)
            {
                return;
            }

            if (gridEntries.ActiveRow.ListObject is ManualPartyBalanceEntry entry)
            {
                LoadSelectedEntry(entry);
            }
        }

        private void LoadSelectedEntry(ManualPartyBalanceEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            _loadingEntry = true;
            try
            {
                _currentEntryId = entry.Id;
                SetComboValue(cmbPartyType, entry.PartyType);
                txtPartyName.Text = entry.PartyName;
                SetComboValue(cmbBalanceType, entry.BalanceType);
                numAmount.Value = entry.Amount;
                dtEntryDate.Value = entry.EntryDate == DateTime.MinValue ? DateTime.Today : entry.EntryDate;
                txtRemarks.Text = entry.Remarks ?? string.Empty;
                lblSettledValue.Text = entry.SettledAmount.ToString("N2");
                lblRemainingValue.Text = entry.RemainingAmount.ToString("N2");
                lblSettlementTarget.Text = $"Selected: {entry.PartyType} - {entry.PartyName}";
                lblSettlementRemaining.Text = $"Remaining: {entry.RemainingAmount:N2}";

                numSettlementAmount.Value = 0;
                txtSettlementRemarks.Clear();
                dtSettlementDate.Value = DateTime.Today;
            }
            finally
            {
                _loadingEntry = false;
            }
        }

        private void ResetEntryForm(bool clearSelection = true)
        {
            _loadingEntry = true;
            try
            {
                _currentEntryId = 0;
                SetComboValue(cmbPartyType, "Customer");
                txtPartyName.Clear();
                SetComboValue(cmbBalanceType, "Balance");
                numAmount.Value = 0;
                dtEntryDate.Value = DateTime.Today;
                txtRemarks.Clear();
                lblSettledValue.Text = "0.00";
                lblRemainingValue.Text = "0.00";
                lblSettlementTarget.Text = "Selected: None";
                lblSettlementRemaining.Text = "Remaining: 0.00";
                numSettlementAmount.Value = 0;
                dtSettlementDate.Value = DateTime.Today;
                txtSettlementRemarks.Clear();

                if (clearSelection)
                {
                    gridEntries.Selected.Rows.Clear();
                }
            }
            finally
            {
                _loadingEntry = false;
            }
        }

        private void SaveEntry()
        {
            if (string.IsNullOrWhiteSpace(txtPartyName.Text))
            {
                MessageBox.Show("Party name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPartyName.Focus();
                return;
            }

            decimal amount = Convert.ToDecimal(numAmount.Value);
            if (amount <= 0)
            {
                MessageBox.Show("Amount must be greater than zero.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numAmount.Focus();
                return;
            }

            try
            {
                string resolvedPartyName = ResolvePartyName(cmbPartyType.Text, txtPartyName.Text.Trim(), true);
                txtPartyName.Text = resolvedPartyName;

                var entry = new ManualPartyBalanceEntry
                {
                    Id = _currentEntryId,
                    PartyType = cmbPartyType.Text,
                    PartyName = resolvedPartyName,
                    BalanceType = cmbBalanceType.Text,
                    Amount = amount,
                    EntryDate = Convert.ToDateTime(dtEntryDate.Value).Date,
                    Remarks = txtRemarks.Text.Trim()
                };

                int savedId = _repository.SaveEntry(entry);
                LoadEntries(savedId);
                MessageBox.Show("Manual party balance saved successfully.", "Manual Balance", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteEntry()
        {
            if (_currentEntryId <= 0)
            {
                MessageBox.Show("Select an entry to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Delete this manual party balance entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _repository.DeleteEntry(_currentEntryId);
                LoadEntries();
                MessageBox.Show("Entry deleted successfully.", "Manual Balance", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddSettlement()
        {
            if (_currentEntryId <= 0)
            {
                MessageBox.Show("Select an entry before adding a settlement.", "Settlement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal settlementAmount = Convert.ToDecimal(numSettlementAmount.Value);
            if (settlementAmount <= 0)
            {
                MessageBox.Show("Settlement amount must be greater than zero.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSettlementAmount.Focus();
                return;
            }

            if (decimal.TryParse(lblRemainingValue.Text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out decimal remaining)
                && settlementAmount > remaining)
            {
                MessageBox.Show($"Settlement amount ({settlementAmount:N2}) cannot exceed the remaining balance ({remaining:N2}).",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSettlementAmount.Focus();
                return;
            }

            try
            {
                _repository.AddSettlement(new ManualPartyBalanceSettlement
                {
                    ManualPartyBalanceId = _currentEntryId,
                    SettlementAmount = settlementAmount,
                    SettlementDate = Convert.ToDateTime(dtSettlementDate.Value).Date,
                    Remarks = txtSettlementRemarks.Text.Trim()
                });

                LoadEntries(_currentEntryId);
                MessageBox.Show("Settlement added successfully.", "Settlement", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Settlement Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewSettlementHistory()
        {
            if (_currentEntryId <= 0)
            {
                MessageBox.Show("Select an entry to view history.", "Manual Balance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var historyForm = new FrmSettlementHistory(_currentEntryId, $"{cmbPartyType.Text} - {txtPartyName.Text}"))
            {
                historyForm.ShowDialog(this);
                LoadEntries(_currentEntryId);
            }
        }

        private void OpenReport()
        {
            var reportForm = new FrmCombinedPartyBalanceReport();
            reportForm.MdiParent = this.MdiParent;
            reportForm.Show();
        }

        private void ApplyResolvedPartyName()
        {
            if (_loadingEntry || string.IsNullOrWhiteSpace(txtPartyName.Text))
            {
                return;
            }

            txtPartyName.Text = ResolvePartyName(cmbPartyType.Text, txtPartyName.Text.Trim(), true);
        }

        private static string NormalizeFilter(Infragistics.Win.UltraWinEditors.UltraComboEditor comboBox)
        {
            string value = comboBox.Text;
            return string.Equals(value, "All", StringComparison.OrdinalIgnoreCase) ? null : value;
        }

        private static void SetComboValue(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            foreach (var item in combo.Items)
            {
                if (string.Equals(item.DisplayText, value, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        private string ResolvePartyName(string partyType, string rawName, bool allowApproximate)
        {
            string normalizedName = NormalizePartyName(rawName);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return string.Empty;
            }

            Dictionary<string, string> nameMap = GetNameMap(partyType);
            if (nameMap == null || nameMap.Count == 0)
            {
                return normalizedName;
            }

            string exactKey = BuildLookupKey(normalizedName);
            string resolvedName;
            if (nameMap.TryGetValue(exactKey, out resolvedName))
            {
                return resolvedName;
            }

            if (!allowApproximate)
            {
                return normalizedName;
            }

            List<string> candidates = GetMasterNames(partyType)
                .Where(x => IsCloseMatch(normalizedName, x))
                .ToList();

            return candidates.Count == 1 ? candidates[0] : normalizedName;
        }

        private Dictionary<string, string> GetNameMap(string partyType)
        {
            if (string.Equals(partyType, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return _customerNameMap;
            }

            if (string.Equals(partyType, "Vendor", StringComparison.OrdinalIgnoreCase))
            {
                return _vendorNameMap;
            }

            return null;
        }

        private List<string> GetMasterNames(string partyType)
        {
            if (string.Equals(partyType, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return _customerMasterNames;
            }

            if (string.Equals(partyType, "Vendor", StringComparison.OrdinalIgnoreCase))
            {
                return _vendorMasterNames;
            }

            return new List<string>();
        }

        private static Dictionary<string, string> BuildNameMap(IEnumerable<string> names)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in names.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                string normalizedName = NormalizePartyName(name);
                string key = BuildLookupKey(normalizedName);
                if (!map.ContainsKey(key))
                {
                    map.Add(key, normalizedName);
                }
            }

            return map;
        }

        private static string NormalizePartyName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return string.Join(" ", value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static string BuildLookupKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string(value
                .ToUpperInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
        }

        private static bool IsCloseMatch(string source, string target)
        {
            string sourceKey = BuildLookupKey(source);
            string targetKey = BuildLookupKey(target);

            if (string.IsNullOrWhiteSpace(sourceKey) || string.IsNullOrWhiteSpace(targetKey))
            {
                return false;
            }

            if (sourceKey[0] != targetKey[0] || Math.Abs(sourceKey.Length - targetKey.Length) > 2)
            {
                return false;
            }

            return ComputeLevenshteinDistance(sourceKey, targetKey) <= 2;
        }

        private static int ComputeLevenshteinDistance(string source, string target)
        {
            int[,] distance = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
            {
                distance[i, 0] = i;
            }

            for (int j = 0; j <= target.Length; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[source.Length, target.Length];
        }
    }
}
