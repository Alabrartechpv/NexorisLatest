using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmLedgerSearch : Form
    {
        private const string SearchPlaceholder = "Search ledgers...";

        private readonly Dictionary<string, string> sortColumnMap = new Dictionary<string, string>();
        private Repository.Accounts.LedgerRepository ledgerRepo;
        private DataTable dtLedgers;
        private Label lblStatus;

        public int SelectedLedgerId { get; private set; } = -1;

        public FrmLedgerSearch()
        {
            InitializeComponent();

            if (IsInDesignMode())
            {
                return;
            }

            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupUltraGridStyle();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();

            Load += FrmLedgerSearch_Load;
            KeyDown += FrmLedgerSearch_KeyDown;

            ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout;
            ultraGrid1.DoubleClickRow += UltraGrid1_DoubleClickRow;
            ultraGrid1.KeyDown += UltraGrid1_KeyDown;
            ultraGrid1.AfterSelectChange += UltraGrid1_AfterSelectChange;

            textBoxsearch.TextChanged += TextBoxsearch_TextChanged;
            textBoxsearch.KeyDown += TextBoxsearch_KeyDown;
            textBoxsearch.GotFocus += TextBoxsearch_GotFocus;
            textBoxsearch.LostFocus += TextBoxsearch_LostFocus;

            ultraPanel5.Click += BtnOk_Click;
            ultraPanel5.ClientArea.Click += BtnOk_Click;
            label5.Click += BtnOk_Click;
            ultraPictureBox1.Click += BtnOk_Click;

            ultraPanel6.Click += BtnClose_Click;
            ultraPanel6.ClientArea.Click += BtnClose_Click;
            label3.Click += BtnClose_Click;
            ultraPictureBox2.Click += BtnClose_Click;

            ultraPanel4.Click += BtnNewEdit_Click;
            ultraPanel4.ClientArea.Click += BtnNewEdit_Click;
            label4.Click += BtnNewEdit_Click;
            ultraPictureBox3.Click += BtnNewEdit_Click;

            ultraPanel9.Click += BtnSearchIcon_Click;
            ultraPanel9.ClientArea.Click += BtnSearchIcon_Click;
            ultraPictureBox4.Click += BtnSearchIcon_Click;

            SetSearchPlaceholder();
        }

        private void FrmLedgerSearch_Load(object sender, EventArgs e)
        {
            LoadLedgers();
            BeginInvoke((Action)(() => textBoxsearch.Focus()));
        }

        private void LoadLedgers()
        {
            try
            {
                // Pass 0 to fetch ledgers across all branches to ensure visibility
                int branchId = 0; 
                if (ledgerRepo == null)
                {
                    ledgerRepo = new Repository.Accounts.LedgerRepository();
                }

                dtLedgers = ledgerRepo.GetAllLedgers(branchId);

                ultraGrid1.DataSource = dtLedgers;
                FormatGrid();
                ApplySort();
                SelectFirstRow();

                int rowCount = dtLedgers == null ? 0 : dtLedgers.DefaultView.Count;
                UpdateStatus(rowCount > 0 ? string.Format("Loaded {0} ledgers.", rowCount) : "No ledgers found.");
            }
            catch (Exception ex)
            {
                UpdateStatus("Unable to load ledgers.");
                MessageBox.Show("Error loading ledgers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsInDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode;
        }

        private void ApplyDialogAppearance()
        {
            Color surface = Color.FromArgb(222, 240, 250);
            Color panelSurface = Color.FromArgb(238, 248, 253);
            Color primary = Color.FromArgb(15, 77, 128);
            Color primaryLight = Color.FromArgb(26, 130, 190);
            Color border = Color.FromArgb(110, 170, 210);

            BackColor = surface;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            KeyPreview = true;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ledger Search";

            ultPanelPurchaseDisplay.Appearance.BackColor = surface;
            ultPanelPurchaseDisplay.Appearance.BackColor2 = panelSurface;
            ultPanelPurchaseDisplay.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultPanelPurchaseDisplay.BorderStyle = UIElementBorderStyle.None;
            ultPanelPurchaseDisplay.Dock = DockStyle.Fill;

            ultraPanel2.Appearance.BackColor = primary;
            ultraPanel2.Appearance.BackColor2 = primaryLight;
            ultraPanel2.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanel2.BorderStyle = UIElementBorderStyle.None;

            lblSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSearch.ForeColor = Color.White;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.ForeColor = Color.White;

            StyleComboBox(comboBox1);
            StyleComboBox(comboBox2);

            textBoxsearch.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            textBoxsearch.BorderStyle = BorderStyle.FixedSingle;
            textBoxsearch.BackColor = Color.White;

            StyleActionPanel(ultraPanel5, label5, Color.FromArgb(0, 122, 204), Color.FromArgb(0, 96, 160));
            StyleActionPanel(ultraPanel6, label3, Color.FromArgb(80, 104, 122), Color.FromArgb(55, 75, 92));
            StyleActionPanel(ultraPanel4, label4, Color.FromArgb(0, 122, 204), Color.FromArgb(0, 96, 160));
            StyleActionPanel(ultraPanel3, null, Color.FromArgb(0, 122, 204), Color.FromArgb(0, 96, 160));
            StyleActionPanel(ultraPanel7, null, Color.FromArgb(0, 122, 204), Color.FromArgb(0, 96, 160));

            ultraPanel8.Appearance.BackColor = border;
            ultraPanel8.Appearance.BackColor2 = Color.FromArgb(170, 208, 232);
            ultraPanel8.Appearance.BackGradientStyle = GradientStyle.Horizontal;

            ultraGrid1.BackColor = Color.White;
        }

        private void StyleComboBox(ComboBox comboBox)
        {
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = Color.White;
            comboBox.ForeColor = Color.FromArgb(20, 58, 92);
            comboBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        }

        private void StyleActionPanel(UltraPanel panel, Label label, Color backColor, Color backColor2)
        {
            panel.Appearance.BackColor = backColor;
            panel.Appearance.BackColor2 = backColor2;
            panel.Appearance.BackGradientStyle = GradientStyle.Vertical;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;
            panel.Cursor = Cursors.Hand;
            panel.ClientArea.Cursor = Cursors.Hand;

            if (label != null)
            {
                label.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                label.ForeColor = Color.White;
                label.Cursor = Cursors.Hand;
            }
        }



        private void UpdateStatus(string message)
        {
            if (label1 != null)
            {
                label1.Text = message;
                label1.Update();
            }
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = AllowColSwapping.WithinBand;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellPadding = 3;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;

                Color gridLine = Color.FromArgb(170, 208, 232);
                Color headerBlue = Color.FromArgb(15, 77, 128);
                Color selectedBlue = Color.FromArgb(0, 120, 215);

                ultraGrid1.DisplayLayout.Appearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = gridLine;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = gridLine;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(24, 108, 168);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 18;

                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowAppearance.ForeColor = Color.FromArgb(20, 58, 92);
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Segoe UI";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(244, 250, 253);
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.FromArgb(244, 250, 253);
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectedBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = selectedBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectedBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = selectedBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                ultraGrid1.DisplayLayout.Override.CellAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.CellAppearance.ForeColor = Color.FromArgb(20, 58, 92);
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = HAlign.Left;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Segoe UI";
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 9;

                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;
                ultraGrid1.DisplayLayout.ViewStyleBand = ViewStyleBand.OutlookGroupBy;
            }
            catch (Exception ex)
            {
                UpdateStatus("Grid style warning: " + ex.Message);
            }
        }

        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Ledger ID");
            comboBox1.Items.Add("Ledger Name");
            comboBox1.Items.Add("Alias");
            comboBox1.Items.Add("Account Group");
            comboBox1.Items.Add("Branch");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        private void InitializeColumnOrderComboBox()
        {
            sortColumnMap.Clear();
            sortColumnMap.Add("Recently Added", "LedgerID DESC");
            sortColumnMap.Add("Ledger Name", "LedgerName ASC");
            sortColumnMap.Add("Account Group", "GroupName ASC");
            sortColumnMap.Add("Branch", "BranchName ASC");
            sortColumnMap.Add("Balance", "Balance ASC");
            sortColumnMap.Add("Opening Debit", "OpnDebit ASC");
            sortColumnMap.Add("Opening Credit", "OpnCredit ASC");

            comboBox2.Items.Clear();
            foreach (string item in sortColumnMap.Keys)
            {
                comboBox2.Items.Add(item);
            }

            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySearchFilter();
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySort();
        }

        private void FormatGrid()
        {
            try
            {
                SetupUltraGridStyle();

                if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                {
                    return;
                }

                ApplyLedgerColumnLayout(ultraGrid1.DisplayLayout.Bands[0]);
            }
            catch (Exception ex)
            {
                UpdateStatus("Grid format warning: " + ex.Message);
            }
        }

        private void UltraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            SetupUltraGridStyle();
            ApplyLedgerColumnLayout(e.Layout.Bands[0]);
        }

        private void ApplyLedgerColumnLayout(UltraGridBand band)
        {
            if (band == null)
            {
                return;
            }

            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
                column.CellActivation = Activation.NoEdit;
                column.CellAppearance.TextVAlign = VAlign.Middle;
            }

            int position = 0;
            ConfigureColumn(band, "LedgerName", "Ledger Name", position++, 220, HAlign.Left, null);
            ConfigureColumn(band, "Alias", "Alias", position++, 120, HAlign.Left, null);
            ConfigureColumn(band, "GroupName", "Account Group", position++, 160, HAlign.Left, null);
            ConfigureColumn(band, "Description", "Description", position++, 190, HAlign.Left, null);
            ConfigureColumn(band, "OpnDebit", "Open Debit", position++, 100, HAlign.Right, "N2");
            ConfigureColumn(band, "OpnCredit", "Open Credit", position++, 100, HAlign.Right, "N2");
            ConfigureColumn(band, "Balance", "Balance", position++, 100, HAlign.Right, "N2");
            ConfigureColumn(band, "BranchName", "Branch", position++, 120, HAlign.Left, null);
            ConfigureColumn(band, "Notes", "Notes", position, 180, HAlign.Left, null);

            band.Override.AllowUpdate = DefaultableBoolean.False;
            band.Override.CellClickAction = CellClickAction.RowSelect;
        }

        private void ConfigureColumn(UltraGridBand band, string key, string caption, int visiblePosition, int width, HAlign align, string format)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = caption;
            column.Header.VisiblePosition = visiblePosition;
            column.Width = width;
            column.CellAppearance.TextHAlign = align;

            if (!string.IsNullOrEmpty(format))
            {
                column.Format = format;
            }
        }

        private void TextBoxsearch_TextChanged(object sender, EventArgs e)
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            try
            {
                if (dtLedgers == null)
                {
                    return;
                }

                string searchValue = GetSearchText();
                if (string.IsNullOrEmpty(searchValue))
                {
                    dtLedgers.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    dtLedgers.DefaultView.RowFilter = BuildRowFilter(searchValue);
                }

                ApplySort();
                SelectFirstRow();
                textBox3.Text = dtLedgers.DefaultView.Count.ToString();
                UpdateStatus(string.Format("Showing {0} of {1} ledgers.", dtLedgers.DefaultView.Count, dtLedgers.Rows.Count));
            }
            catch (Exception ex)
            {
                UpdateStatus("Search filter warning: " + ex.Message);
            }
        }

        private string BuildRowFilter(string searchValue)
        {
            string escapedValue = EscapeRowFilterValue(searchValue);
            string selectedFilter = comboBox1.SelectedItem == null ? "All" : comboBox1.SelectedItem.ToString();
            List<string> filters = new List<string>();

            if (selectedFilter == "All" || selectedFilter == "Ledger ID")
            {
                AddFilterIfColumnExists(filters, "LedgerID", string.Format("Convert(LedgerID, 'System.String') LIKE '%{0}%'", escapedValue));
            }

            if (selectedFilter == "All" || selectedFilter == "Ledger Name")
            {
                AddFilterIfColumnExists(filters, "LedgerName", string.Format("LedgerName LIKE '%{0}%'", escapedValue));
            }

            if (selectedFilter == "All" || selectedFilter == "Alias")
            {
                AddFilterIfColumnExists(filters, "Alias", string.Format("Alias LIKE '%{0}%'", escapedValue));
            }

            if (selectedFilter == "All" || selectedFilter == "Account Group")
            {
                AddFilterIfColumnExists(filters, "GroupName", string.Format("GroupName LIKE '%{0}%'", escapedValue));
            }

            if (selectedFilter == "All" || selectedFilter == "Branch")
            {
                AddFilterIfColumnExists(filters, "BranchName", string.Format("BranchName LIKE '%{0}%'", escapedValue));
            }

            if (selectedFilter == "All")
            {
                AddFilterIfColumnExists(filters, "Description", string.Format("Description LIKE '%{0}%'", escapedValue));
                AddFilterIfColumnExists(filters, "Notes", string.Format("Notes LIKE '%{0}%'", escapedValue));
            }

            return filters.Count == 0 ? string.Empty : string.Join(" OR ", filters.ToArray());
        }

        private void AddFilterIfColumnExists(List<string> filters, string columnName, string filter)
        {
            if (dtLedgers != null && dtLedgers.Columns.Contains(columnName))
            {
                filters.Add(filter);
            }
        }

        private string EscapeRowFilterValue(string value)
        {
            return value.Replace("'", "''")
                        .Replace("[", "[[]")
                        .Replace("%", "[%]")
                        .Replace("*", "[*]");
        }

        private string GetSearchText()
        {
            string value = textBoxsearch.Text.Trim();
            return value == SearchPlaceholder ? string.Empty : value;
        }

        private void ApplySort()
        {
            if (dtLedgers == null || comboBox2.SelectedItem == null)
            {
                return;
            }

            string selectedSort = comboBox2.SelectedItem.ToString();
            string sortExpression;
            if (sortColumnMap.TryGetValue(selectedSort, out sortExpression))
            {
                // Verify the base column exists before sorting
                string baseColumn = sortExpression.Split(' ')[0];
                if (dtLedgers.Columns.Contains(baseColumn))
                {
                    dtLedgers.DefaultView.Sort = sortExpression;
                    UpdateStatus("Sorted by " + selectedSort + ".");
                }
            }
        }

        private void SelectFirstRow()
        {
            if (ultraGrid1.Rows.Count == 0)
            {
                return;
            }

            ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            ultraGrid1.Selected.Rows.Clear();
            ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
        }

        private void UltraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.IsDataRow)
            {
                UpdateStatus(string.Format("Selected ledger: {0}", GetActiveLedgerName()));
            }
        }

        private string GetActiveLedgerName()
        {
            if (ultraGrid1.ActiveRow == null || !ultraGrid1.ActiveRow.Cells.Exists("LedgerName"))
            {
                return string.Empty;
            }

            object value = ultraGrid1.ActiveRow.Cells["LedgerName"].Value;
            return value == null ? string.Empty : value.ToString();
        }

        private void TextBoxsearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                SelectFirstRow();
                ultraGrid1.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (ultraGrid1.Rows.Count == 1)
                {
                    ConfirmSelection();
                }
                else
                {
                    SelectFirstRow();
                    ultraGrid1.Focus();
                }

                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                BtnClose_Click(sender, EventArgs.Empty);
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                BtnClose_Click(sender, EventArgs.Empty);
            }
        }

        private void FrmLedgerSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                BtnClose_Click(sender, EventArgs.Empty);
            }
        }

        private void TextBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == SearchPlaceholder)
            {
                textBoxsearch.Text = string.Empty;
                textBoxsearch.ForeColor = Color.FromArgb(20, 58, 92);
            }
        }

        private void TextBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                SetSearchPlaceholder();
            }
        }

        private void SetSearchPlaceholder()
        {
            textBoxsearch.Text = SearchPlaceholder;
            textBoxsearch.ForeColor = Color.Gray;
        }

        private void ConnectNavigationPanelEvents()
        {
            ultraPanel3.Click += MoveRowUp;
            ultraPanel3.ClientArea.Click += MoveRowUp;
            ultraPictureBox5.Click += MoveRowUp;

            ultraPanel7.Click += MoveRowDown;
            ultraPanel7.ClientArea.Click += MoveRowDown;
            ultraPictureBox6.Click += MoveRowDown;
        }

        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow == null || ultraGrid1.ActiveRow.Index <= 0)
            {
                return;
            }

            UltraGridRow previousRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index - 1];
            ultraGrid1.ActiveRow = previousRow;
            ultraGrid1.Selected.Rows.Clear();
            ultraGrid1.Selected.Rows.Add(previousRow);
            ultraGrid1.Focus();
        }

        private void MoveRowDown(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow == null || ultraGrid1.ActiveRow.Index >= ultraGrid1.Rows.Count - 1)
            {
                return;
            }

            UltraGridRow nextRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index + 1];
            ultraGrid1.ActiveRow = nextRow;
            ultraGrid1.Selected.Rows.Clear();
            ultraGrid1.Selected.Rows.Add(nextRow);
            ultraGrid1.Focus();
        }

        private void SetupPanelHoverEffects()
        {
            SetupPanelGroupHoverEffects(ultraPanel5, label5, ultraPictureBox1);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
            SetupPanelGroupHoverEffects(ultraPanel9, null, ultraPictureBox4);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
        }

        private void SetupPanelGroupHoverEffects(UltraPanel panel, Label label, UltraPictureBox pictureBox)
        {
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 28);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 28);

            Action applyHoverEffect = delegate
            {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            Action removeHoverEffect = delegate
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };

            panel.MouseEnter += delegate { applyHoverEffect(); };
            panel.MouseLeave += delegate { removeHoverEffect(); };
            panel.ClientArea.MouseEnter += delegate { applyHoverEffect(); };
            panel.ClientArea.MouseLeave += delegate { if (!IsMouseOverControl(panel)) removeHoverEffect(); };

            if (pictureBox != null)
            {
                pictureBox.MouseEnter += delegate { applyHoverEffect(); pictureBox.Cursor = Cursors.Hand; };
                pictureBox.MouseLeave += delegate { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }

            if (label != null)
            {
                label.MouseEnter += delegate { applyHoverEffect(); label.Cursor = Cursors.Hand; };
                label.MouseLeave += delegate { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }
        }

        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        private bool IsMouseOverControl(Control control)
        {
            Point mousePosition = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePosition);
        }

        private void UltraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row != null && e.Row.IsDataRow)
            {
                ConfirmSelection();
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (ultraGrid1.ActiveRow == null || !ultraGrid1.ActiveRow.IsDataRow || !ultraGrid1.ActiveRow.Cells.Exists("LedgerID"))
            {
                MessageBox.Show("Please select a ledger first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            object cellValue = ultraGrid1.ActiveRow.Cells["LedgerID"].Value;
            int id;
            if (cellValue != null && int.TryParse(cellValue.ToString(), out id))
            {
                SelectedLedgerId = id;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            MessageBox.Show("Selected ledger is invalid.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnNewEdit_Click(object sender, EventArgs e)
        {
            try
            {
                PosBranch_Win.Accounts.FrmLedgers frm = new PosBranch_Win.Accounts.FrmLedgers();
                if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.IsDataRow && ultraGrid1.ActiveRow.Cells.Exists("LedgerID"))
                {
                    object cellValue = ultraGrid1.ActiveRow.Cells["LedgerID"].Value;
                    if (cellValue != null && int.TryParse(cellValue.ToString(), out int id))
                    {
                        frm.ShowDialog();
                        LoadLedgers();
                        return;
                    }
                }
                frm.ShowDialog();
                LoadLedgers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSearchIcon_Click(object sender, EventArgs e)
        {
            ApplySearchFilter();
        }
    }
}
