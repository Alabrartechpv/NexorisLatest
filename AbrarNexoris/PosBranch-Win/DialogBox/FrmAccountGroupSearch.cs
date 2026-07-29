using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Repository.Accounts;
using ModelClass;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmAccountGroupSearch : Form
    {
        // ── Fields ──────────────────────────────────────────────────────────────
        private const string SearchPlaceholder = "Search account groups...";
        private AccountGroupRepository repo;
        private DataTable dtData;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        public int SelectedGroupId { get; private set; } = -1;

        // ── Constructor ─────────────────────────────────────────────────────────
        public FrmAccountGroupSearch()
        {
            InitializeComponent();
            repo = new AccountGroupRepository();

            SetupUltraGridStyle();
            InitializeSearchFilterComboBox();
            InitializeColumnSortComboBox();
            ConnectNavigationPanelEvents();

            // Grid events
            this.ultraGrid1.InitializeLayout   += UltraGrid1_InitializeLayout;
            this.ultraGrid1.DoubleClickRow      += UltraGrid1_DoubleClickRow;
            this.ultraGrid1.KeyDown             += UltraGrid1_KeyDown;
            this.ultraGrid1.AfterSelectChange   += UltraGrid1_AfterSelectChange;
            this.ultraGrid1.Resize              += (s, e) => PreserveColumnWidths();

            // Search box
            this.textBoxsearch.TextChanged += TextBoxsearch_TextChanged;
            this.textBoxsearch.KeyDown     += TextBoxsearch_KeyDown;
            this.textBoxsearch.GotFocus    += TextBoxsearch_GotFocus;
            this.textBoxsearch.LostFocus   += TextBoxsearch_LostFocus;
            this.textBoxsearch.Text        = SearchPlaceholder;

            // Form events
            this.Load         += FrmAccountGroupSearch_Load;
            this.KeyDown      += FrmAccountGroupSearch_KeyDown;
            this.SizeChanged  += (s, e) => PreserveColumnWidths();
        }

        // ── Load ────────────────────────────────────────────────────────────────
        private void FrmAccountGroupSearch_Load(object sender, EventArgs e)
        {
            LoadData();
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
        }

        // ── Data Loading ────────────────────────────────────────────────────────
        private void LoadData()
        {
            try
            {
                int branchId = SessionContext.BranchId > 0
                    ? SessionContext.BranchId
                    : (int.TryParse(DataBase.BranchId, out int bid) ? bid : 0);

                dtData = repo.GetAllAccountGroups(branchId);

                ultraGrid1.DataSource = dtData;

                // Apply initial column widths after layout is ready
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    var band = ultraGrid1.DisplayLayout.Bands[0];
                    if (band.Columns.Exists("GroupID"))       band.Columns["GroupID"].Width       = 55;
                    if (band.Columns.Exists("GroupName"))     band.Columns["GroupName"].Width     = 200;
                    if (band.Columns.Exists("GroupType"))     band.Columns["GroupType"].Width     = 110;
                    if (band.Columns.Exists("AccountCategory")) band.Columns["AccountCategory"].Width = 110;
                    if (band.Columns.Exists("GroupUnder"))   band.Columns["GroupUnder"].Width    = 140;
                    if (band.Columns.Exists("Description"))  band.Columns["Description"].Width   = 180;
                }

                // Select first row
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                InitializeSavedColumnWidths();
                UpdateRecordCount();
                UpdateStatus($"Loaded {dtData.Rows.Count} account groups.");
            }
            catch (Exception ex)
            {
                UpdateStatus("Error loading data.");
                MessageBox.Show("Error loading account groups: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Grid InitializeLayout ────────────────────────────────────────────────
        private void UltraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                var band = e.Layout.Bands[0];

                // Hide all first
                foreach (UltraGridColumn col in band.Columns)
                    col.Hidden = true;

                // Columns to display in order
                var columns = new[]
                {
                    new { Key = "GroupID",         Caption = "ID",          Width = 55  },
                    new { Key = "GroupName",        Caption = "Name",        Width = 200 },
                    new { Key = "GroupType",        Caption = "Type",        Width = 110 },
                    new { Key = "AccountCategory",  Caption = "Category",    Width = 110 },
                    new { Key = "GroupUnder",       Caption = "Group Under", Width = 140 },
                    new { Key = "Description",      Caption = "Description", Width = 180 },
                };

                int pos = 0;
                foreach (var def in columns)
                {
                    if (!band.Columns.Exists(def.Key)) continue;
                    var col = band.Columns[def.Key];
                    col.Hidden = false;
                    col.Header.Caption = def.Caption;
                    col.Width = def.Width;
                    col.Header.VisiblePosition = pos++;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Grid layout error: {ex.Message}");
            }
        }

        // ── Premium Grid Styling (matches frmCustomerDialog) ────────────────────
        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGrid1.DisplayLayout.Reset();

                // Behaviour
                ultraGrid1.DisplayLayout.Override.AllowAddNew     = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete     = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate     = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.RowSelectors    = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow   = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.AllowColMoving  = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing  = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping= AllowColSwapping.WithinBand;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden        = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt        = string.Empty;

                // Borders
                ultraGrid1.DisplayLayout.BorderStyle                      = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRow          = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell         = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader       = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector  = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha  = Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

                // Spacing
                ultraGrid1.DisplayLayout.Override.CellPadding      = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore  = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter   = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing        = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing            = 0;

                // Colours
                Color lightBlue  = Color.FromArgb(173, 216, 230);
                Color headerBlue = Color.FromArgb(0, 123, 255);

                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor      = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor       = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor    = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Row height
                ultraGrid1.DisplayLayout.Override.MinRowHeight     = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight  = 30;

                // Header styling
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor           = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2          = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle   = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor           = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign          = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign          = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold       = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name       = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha  = Alpha.Transparent;

                // Row selector
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor         = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2        = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor         = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle   = RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle   = RowSelectorNumberStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth          = 15;

                // Clear images
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Image   = null;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.Image = null;

                // Row colours (white rows)
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor          = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2         = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle  = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor         = Color.FromArgb(245, 250, 255);
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2        = Color.FromArgb(245, 250, 255);
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Active / selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor         = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2        = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor         = Color.White;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor       = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor       = Color.White;

                // Font
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints  = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name         = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name          = "Microsoft Sans Serif";

                // Cell alignment
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = HAlign.Left;

                // Scroll styling
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle  = ScrollStyle.Immediate;

                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor           = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2          = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle   = GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor         = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor            = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2           = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle    = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor          = lightBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor            = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2           = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle    = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor          = headerBlue;
                }

                // Auto-fit off (manual column widths)
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = ColumnAutoSizeMode.None;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting up grid style: {ex.Message}");
            }
        }

        // ── Search / Filter ──────────────────────────────────────────────────────
        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "All", "ID", "Name", "Type", "Category" });
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, e) => FilterData();
        }

        private void InitializeColumnSortComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "None", "ID", "Name", "Type", "Category" });
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dtData == null) return;
            string sortBy = comboBox2.SelectedItem?.ToString();
            if      (sortBy == "ID")       dtData.DefaultView.Sort = "GroupID ASC";
            else if (sortBy == "Name")     dtData.DefaultView.Sort = "GroupName ASC";
            else if (sortBy == "Type")     dtData.DefaultView.Sort = "GroupType ASC";
            else if (sortBy == "Category") dtData.DefaultView.Sort = "AccountCategory ASC";
            else                           dtData.DefaultView.Sort = "";
        }

        private void TextBoxsearch_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void FilterData()
        {
            try
            {
                if (dtData == null) return;

                string searchText = textBoxsearch.Text.Trim();
                if (searchText == SearchPlaceholder) searchText = "";

                string escapedSearch = searchText.Replace("'", "''");
                string searchBy = comboBox1.SelectedItem?.ToString() ?? "All";
                string filter = "";

                if (!string.IsNullOrEmpty(escapedSearch))
                {
                    switch (searchBy)
                    {
                        case "ID":
                            filter = $"Convert(GroupID, 'System.String') LIKE '%{escapedSearch}%'";
                            break;
                        case "Name":
                            filter = $"GroupName LIKE '%{escapedSearch}%'";
                            break;
                        case "Type":
                            filter = $"GroupType LIKE '%{escapedSearch}%'";
                            break;
                        case "Category":
                            filter = $"AccountCategory LIKE '%{escapedSearch}%'";
                            break;
                        default: // All
                            filter = $"GroupName LIKE '%{escapedSearch}%' OR Convert(GroupID, 'System.String') LIKE '%{escapedSearch}%' OR AccountCategory LIKE '%{escapedSearch}%' OR GroupType LIKE '%{escapedSearch}%'";
                            break;
                    }
                }

                dtData.DefaultView.RowFilter = filter;

                // Re-select first row after filter
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                UpdateRecordCount();
                UpdateStatus(string.IsNullOrEmpty(filter)
                    ? $"Showing all {ultraGrid1.Rows.Count} records."
                    : $"Found {ultraGrid1.Rows.Count} matching records.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Filter error: {ex.Message}");
            }
        }

        // ── Search Box Placeholder ────────────────────────────────────────────────
        private void TextBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == SearchPlaceholder)
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = SystemColors.WindowText;
            }
        }

        private void TextBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = SearchPlaceholder;
                textBoxsearch.ForeColor = Color.Gray;
            }
        }

        // ── Record Count & Status ─────────────────────────────────────────────────
        private void UpdateRecordCount()
        {
            try
            {
                int count = ultraGrid1.Rows.GetFilteredInNonGroupByRows().Length;
                label1.Text = $"Total Records : {count}";
            }
            catch { }
        }

        private void UpdateStatus(string message)
        {
            // label1 is used as record count — no separate status bar needed,
            // but subclasses can extend this if a status label exists
        }

        // ── Column Width Management ───────────────────────────────────────────────
        private void InitializeSavedColumnWidths()
        {
            savedColumnWidths.Clear();
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;
            foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                    savedColumnWidths[col.Key] = col.Width;
            }
        }

        private void PreserveColumnWidths()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;
                ultraGrid1.SuspendLayout();
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        col.Width = savedColumnWidths[col.Key];
                }
                ultraGrid1.ResumeLayout();
            }
            catch { }
        }

        // ── Navigation Panels ─────────────────────────────────────────────────────
        private void ConnectNavigationPanelEvents()
        {
            // Up arrow nav
            ultraPanel3.Click              += MoveRowUp;
            ultraPanel3.ClientArea.Click   += MoveRowUp;
            ultraPictureBox5.Click         += MoveRowUp;

            // Down arrow nav
            ultraPanel7.Click              += MoveRowDown;
            ultraPanel7.ClientArea.Click   += MoveRowDown;
            ultraPictureBox6.Click         += MoveRowDown;

            // OK / Select
            ultraPanel5.Click            += (s, e) => SelectCurrentRow();
            ultraPanel5.ClientArea.Click += (s, e) => SelectCurrentRow();
            ultraPictureBox1.Click       += (s, e) => SelectCurrentRow();
            label5.Click                 += (s, e) => SelectCurrentRow();

            // Close
            ultraPanel6.Click            += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            ultraPanel6.ClientArea.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            ultraPictureBox2.Click       += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            label3.Click                 += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // New/Edit
            ultraPanel4.Click            += BtnNewEdit_Click;
            ultraPanel4.ClientArea.Click += BtnNewEdit_Click;
            ultraPictureBox3.Click       += BtnNewEdit_Click;
            label4.Click                 += BtnNewEdit_Click;

            // Clear search
            ultraPanel9.Click            += BtnClearSearch_Click;
            ultraPanel9.ClientArea.Click += BtnClearSearch_Click;
            ultraPictureBox4.Click       += BtnClearSearch_Click;

            // Hover effects
            SetupPanelHoverEffects();
        }

        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow == null || ultraGrid1.Rows.Count == 0) return;
            int idx = ultraGrid1.ActiveRow.Index;
            if (idx > 0)
            {
                var rowToActivate = ultraGrid1.Rows[idx - 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
        }

        private void MoveRowDown(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow == null || ultraGrid1.Rows.Count == 0) return;
            int idx = ultraGrid1.ActiveRow.Index;
            if (idx < ultraGrid1.Rows.Count - 1)
            {
                var rowToActivate = ultraGrid1.Rows[idx + 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
        }

        // ── Grid Events ───────────────────────────────────────────────────────────
        private void UltraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            // Keep selection and active row in sync
        }

        private void UltraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectCurrentRow();
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCurrentRow();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        // ── Form-level KeyDown (redirect typing to search box) ────────────────────
        private void FrmAccountGroupSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        // ── Search-box KeyDown (arrow keys → navigate grid) ───────────────────────
        private void TextBoxsearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                ultraGrid1.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                SelectCurrentRow();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        // ── Select ────────────────────────────────────────────────────────────────
        private void SelectCurrentRow()
        {
            if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.IsDataRow)
            {
                if (ultraGrid1.ActiveRow.Cells.Exists("GroupID") &&
                    ultraGrid1.ActiveRow.Cells["GroupID"].Value != DBNull.Value)
                {
                    SelectedGroupId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["GroupID"].Value);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        // ── Button Handlers ───────────────────────────────────────────────────────
        private void BtnNewEdit_Click(object sender, EventArgs e)
        {
            SelectedGroupId = 0;
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void BtnClearSearch_Click(object sender, EventArgs e)
        {
            textBoxsearch.Text      = SearchPlaceholder;
            textBoxsearch.ForeColor = Color.Gray;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            if (dtData != null) dtData.DefaultView.RowFilter = "";
            if (dtData != null) dtData.DefaultView.Sort      = "";
            UpdateRecordCount();
            textBoxsearch.Focus();
        }

        // ── Hover Effects ─────────────────────────────────────────────────────────
        private void SetupPanelHoverEffects()
        {
            SetupSinglePanelHover(ultraPanel5, label5,  ultraPictureBox1);
            SetupSinglePanelHover(ultraPanel6, label3,  ultraPictureBox2);
            SetupSinglePanelHover(ultraPanel4, label4,  ultraPictureBox3);
            SetupSinglePanelHover(ultraPanel3, null,    ultraPictureBox5);
            SetupSinglePanelHover(ultraPanel7, null,    ultraPictureBox6);
            SetupSinglePanelHover(ultraPanel9, null,    ultraPictureBox4);
        }

        private void SetupSinglePanelHover(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            Color origBack1 = panel.Appearance.BackColor;
            Color origBack2 = panel.Appearance.BackColor2;
            Color hover1    = BrightenColor(origBack1, 30);
            Color hover2    = BrightenColor(origBack2, 30);
            Color click1    = DarkenColor(origBack1, 20);
            Color click2    = DarkenColor(origBack2, 20);

            Action applyHover  = () => { panel.Appearance.BackColor = hover1;    panel.Appearance.BackColor2 = hover2;    panel.ClientArea.Cursor = Cursors.Hand; };
            Action removeHover = () => { panel.Appearance.BackColor = origBack1; panel.Appearance.BackColor2 = origBack2; panel.ClientArea.Cursor = Cursors.Default; };
            Action applyClick  = () => { panel.Appearance.BackColor = click1;    panel.Appearance.BackColor2 = click2; };

            panel.MouseEnter += (s, e) => applyHover();
            panel.MouseLeave += (s, e) => removeHover();
            panel.MouseDown  += (s, e) => applyClick();
            panel.MouseUp    += (s, e) => applyHover();

            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => { applyHover(); pictureBox.Cursor = Cursors.Hand; };
                pictureBox.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHover(); };
                pictureBox.MouseDown  += (s, e) => applyClick();
                pictureBox.MouseUp    += (s, e) => applyHover();
            }

            if (label != null)
            {
                label.MouseEnter += (s, e) => { applyHover(); label.Cursor = Cursors.Hand; };
                label.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHover(); };
                label.MouseDown  += (s, e) => applyClick();
                label.MouseUp    += (s, e) => applyHover();
            }
        }

        private static Color BrightenColor(Color color, int amount) =>
            Color.FromArgb(color.A, Math.Min(color.R + amount, 255),
                                    Math.Min(color.G + amount, 255),
                                    Math.Min(color.B + amount, 255));

        private static Color DarkenColor(Color color, int amount) =>
            Color.FromArgb(color.A, Math.Max(color.R - amount, 0),
                                    Math.Max(color.G - amount, 0),
                                    Math.Max(color.B - amount, 0));

        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }
    }
}
