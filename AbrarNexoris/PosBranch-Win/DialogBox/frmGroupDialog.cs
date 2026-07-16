using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Master;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmGroupDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        public TextBox TargetTextBox { get; set; }
        public int SelectedGroupId { get; private set; }
        public string SelectedGroupName { get; private set; }

        // Add debounce timer and tracking for textBox3
        private System.Windows.Forms.Timer textBox3DebounceTimer;
        private string lastProcessedTextBox3Value = string.Empty;

        public frmGroupDialog()
        {
            InitializeComponent();
        }

        private void frmGroupDialog_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "GETALL";
            GroupDDlGrid group = drop.getGroupDDl();
            group.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;

            // Apply consistent UltraGrid look (match frmBrandDialog)
            SetupUltraGridStyle();
            ts1.GridColumnStyles.Add(datagrid);
            dgv_Grop.DataSource = group.List;

            // Wire up double-click handler for grid (same as Enter key)
            this.dgv_Grop.DoubleClickRow += dgv_Grop_DoubleClickRow;

            // Wire search box (textBox1) for filtering and selection
            WireSearchBoxHandlers();

            // Initialize count box with totals, then wire its handlers
            InitializeCountBoxAndLabel();
            WireCountBoxHandlers();

            // Initialize debounce timer for textBox3
            InitializeTextBox3DebounceTimer();

            // Match panel look to frmItemMasterNew ultraPanel5 for selected panels
            ApplyPanelStyles();

            // Setup hover effects and click handlers for ultraPanel4
            SetupPanelGroupHoverEffects(FindControlByName(this, "ultraPanel4") as Infragistics.Win.Misc.UltraPanel,
                FindControlByName(this, "label4") as Label,
                FindControlByName(this, "ultraPictureBox3") as Infragistics.Win.UltraWinEditors.UltraPictureBox);

            // Add click handlers for ultraPanel4
            var ultraPanel4 = FindControlByName(this, "ultraPanel4") as Infragistics.Win.Misc.UltraPanel;
            if (ultraPanel4 != null)
            {
                ultraPanel4.Click += Panel4_Click;
                ultraPanel4.ClientArea.Click += Panel4_Click;
            }

            var label4 = FindControlByName(this, "label4") as Label;
            if (label4 != null)
            {
                label4.Click += Panel4_Click;
            }

            var ultraPictureBox3 = FindControlByName(this, "ultraPictureBox3") as Infragistics.Win.UltraWinEditors.UltraPictureBox;
            if (ultraPictureBox3 != null)
            {
                ultraPictureBox3.Click += Panel4_Click;
            }

            // Ensure there is an active row to prevent null ActiveRow on first key press
            try
            {
                if (this.dgv_Grop.Rows != null && this.dgv_Grop.Rows.Count > 0)
                {
                    this.dgv_Grop.ActiveRow = this.dgv_Grop.Rows[0];
                    this.dgv_Grop.Selected.Rows.Clear();
                    this.dgv_Grop.Selected.Rows.Add(this.dgv_Grop.Rows[0]);
                }
            }
            catch { }

            // Wire up ultraPanel5 (OK) and ultraPanel6 (Close) click handlers
            WirePanelClickHandlers();

            // Use Shown event to set focus on textBox1 (more reliable than Load event)
            this.Shown += (s, evt) =>
            {
                // Set focus to the search textbox when form is fully displayed
                Control[] found = this.Controls.Find("textBox1", true);
                if (found != null && found.Length > 0 && found[0] is TextBox tb)
                {
                    tb.Focus();
                    tb.SelectAll();
                }
            };
        }

        // Override ProcessCmdKey to handle Escape key
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle Escape key to close the form
            if (keyData == Keys.Escape)
            {
                System.Diagnostics.Debug.WriteLine("Escape key pressed, closing form");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Wire up click handlers for ultraPanel5 (OK) and ultraPanel6 (Close)
        /// </summary>
        private void WirePanelClickHandlers()
        {
            try
            {
                // Find ultraPanel5 (OK button) and wire up handlers
                var ultraPanel5 = FindControlByName(this, "ultraPanel5") as Infragistics.Win.Misc.UltraPanel;
                var label5 = FindControlByName(this, "label5") as Label;
                var ultraPictureBox1 = FindControlByName(this, "ultraPictureBox1") as Infragistics.Win.UltraWinEditors.UltraPictureBox;

                if (ultraPanel5 != null)
                {
                    ultraPanel5.Click += Panel5_OKClick;
                    ultraPanel5.ClientArea.Click += Panel5_OKClick;
                }
                if (label5 != null) label5.Click += Panel5_OKClick;
                if (ultraPictureBox1 != null) ultraPictureBox1.Click += Panel5_OKClick;

                // Find ultraPanel6 (Close button) and wire up handlers
                var ultraPanel6 = FindControlByName(this, "ultraPanel6") as Infragistics.Win.Misc.UltraPanel;
                var label3 = FindControlByName(this, "label3") as Label;
                var ultraPictureBox2 = FindControlByName(this, "ultraPictureBox2") as Infragistics.Win.UltraWinEditors.UltraPictureBox;

                if (ultraPanel6 != null)
                {
                    ultraPanel6.Click += Panel6_CloseClick;
                    ultraPanel6.ClientArea.Click += Panel6_CloseClick;
                }
                if (label3 != null) label3.Click += Panel6_CloseClick;
                if (ultraPictureBox2 != null) ultraPictureBox2.Click += Panel6_CloseClick;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error wiring panel click handlers: {ex.Message}");
            }
        }

        /// <summary>
        /// OK button click handler - selects the group and closes
        /// </summary>
        private void Panel5_OKClick(object sender, EventArgs e)
        {
            try
            {
                // Reuse existing grid Enter key logic
                if (this.dgv_Grop.ActiveRow != null)
                {
                    ApplyGroupSelection(this.dgv_Grop.ActiveRow);
                }
                else if (this.dgv_Grop.Rows != null && this.dgv_Grop.Rows.Count > 0)
                {
                    // Select first row if none is selected
                    this.dgv_Grop.ActiveRow = this.dgv_Grop.Rows[0];
                    this.dgv_Grop.Selected.Rows.Clear();
                    this.dgv_Grop.Selected.Rows.Add(this.dgv_Grop.ActiveRow);
                    Panel5_OKClick(sender, e); // Recursive call with now-selected row
                }
                else
                {
                    MessageBox.Show("Please select a group first.", "No Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OK panel click: {ex.Message}");
            }
        }

        /// <summary>
        /// Close button click handler - closes without selecting
        /// </summary>
        private void Panel6_CloseClick(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Close button clicked in frmGroupDialog");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Close panel click: {ex.Message}");
            }
        }

        /// <summary>
        /// Double-click handler for grid - loads the selected item (same as Enter key)
        /// </summary>
        private void dgv_Grop_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            try
            {
                // Skip if it's a group-by row
                if (e.Row == null || e.Row.IsGroupByRow)
                    return;

                // Set the active row to the clicked row
                this.dgv_Grop.ActiveRow = e.Row;
                this.dgv_Grop.Selected.Rows.Clear();
                this.dgv_Grop.Selected.Rows.Add(e.Row);

                ApplyGroupSelection(e.Row);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in grid double-click: {ex.Message}");
            }
        }

        private void dgv_Grop_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only act on Enter; ignore other key presses to avoid firing with no selection
            if (e.KeyChar != (char)Keys.Enter)
                return;

            try
            {
                var row = this.dgv_Grop.ActiveRow;
                if (row == null || row.IsGroupByRow)
                    return;

                ApplyGroupSelection(row);
            }
            catch
            {
                // Swallow to avoid crashing the app due to unexpected nulls
            }
        }

        // Apply the same UltraGrid theme/look as in frmBrandDialog
        private void SetupUltraGridStyle()
        {
            try
            {
                this.dgv_Grop.DisplayLayout.Reset();

                // Behavior
                this.dgv_Grop.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                this.dgv_Grop.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                this.dgv_Grop.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                this.dgv_Grop.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                this.dgv_Grop.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                this.dgv_Grop.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;

                // Column interactions
                this.dgv_Grop.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                this.dgv_Grop.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                this.dgv_Grop.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;
                this.dgv_Grop.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;

                // GroupBy box
                this.dgv_Grop.DisplayLayout.GroupByBox.Hidden = true;
                this.dgv_Grop.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Hide grid caption/header above column headers
                this.dgv_Grop.Text = string.Empty;
                this.dgv_Grop.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;

                // Borders and grid lines
                this.dgv_Grop.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                this.dgv_Grop.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                this.dgv_Grop.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                this.dgv_Grop.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                this.dgv_Grop.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;

                // Spacing
                this.dgv_Grop.DisplayLayout.Override.CellPadding = 0;
                this.dgv_Grop.DisplayLayout.Override.RowSpacingBefore = 0;
                this.dgv_Grop.DisplayLayout.Override.RowSpacingAfter = 0;
                this.dgv_Grop.DisplayLayout.Override.CellSpacing = 0;
                this.dgv_Grop.DisplayLayout.InterBandSpacing = 0;

                // Colors
                Color lightBlue = Color.FromArgb(173, 216, 230);
                Color headerBlue = Color.FromArgb(0, 123, 255);
                this.dgv_Grop.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                this.dgv_Grop.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                this.dgv_Grop.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Row height
                this.dgv_Grop.DisplayLayout.Override.MinRowHeight = 30;
                this.dgv_Grop.DisplayLayout.Override.DefaultRowHeight = 30;

                // Header styling
                this.dgv_Grop.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Row selector styling
                this.dgv_Grop.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                this.dgv_Grop.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                this.dgv_Grop.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.dgv_Grop.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                this.dgv_Grop.DisplayLayout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                this.dgv_Grop.DisplayLayout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None;
                this.dgv_Grop.DisplayLayout.Override.RowSelectorWidth = 15;
                this.dgv_Grop.DisplayLayout.Override.ActiveRowAppearance.Image = null;
                this.dgv_Grop.DisplayLayout.Override.SelectedRowAppearance.Image = null;
                this.dgv_Grop.DisplayLayout.Override.RowSelectorAppearance.Image = null;

                // Row backgrounds
                this.dgv_Grop.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                this.dgv_Grop.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                this.dgv_Grop.DisplayLayout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.dgv_Grop.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                this.dgv_Grop.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                this.dgv_Grop.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Selection/active colors
                Color selectionBlue = Color.FromArgb(0, 120, 215);
                this.dgv_Grop.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectionBlue;
                this.dgv_Grop.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = selectionBlue;
                this.dgv_Grop.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.dgv_Grop.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                this.dgv_Grop.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectionBlue;
                this.dgv_Grop.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = selectionBlue;
                this.dgv_Grop.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.dgv_Grop.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Fonts
                this.dgv_Grop.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                this.dgv_Grop.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                this.dgv_Grop.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                this.dgv_Grop.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Scrollbars
                this.dgv_Grop.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                this.dgv_Grop.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
                if (this.dgv_Grop.DisplayLayout.ScrollBarLook != null)
                {
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    this.dgv_Grop.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Alignment
                this.dgv_Grop.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                this.dgv_Grop.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

                // Auto-fit and filtering settings to match
                this.dgv_Grop.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;
                this.dgv_Grop.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.None;
                this.dgv_Grop.DisplayLayout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                this.dgv_Grop.DisplayLayout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                this.dgv_Grop.DisplayLayout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                this.dgv_Grop.DisplayLayout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;

                // InitializeLayout handler for per-column header re-style after binding
                this.dgv_Grop.InitializeLayout -= dgv_Grop_InitializeLayout;
                this.dgv_Grop.InitializeLayout += dgv_Grop_InitializeLayout;
            }
            catch { }
        }

        private void ApplyGroupSelection(UltraGridRow row)
        {
            if (row == null || row.IsGroupByRow)
            {
                return;
            }

            UltraGridCell groupNameCell = row.Cells.Exists("GroupName") ? row.Cells["GroupName"] : null;
            UltraGridCell groupIdCell = row.Cells.Exists("Id") ? row.Cells["Id"] : null;
            if (groupNameCell == null)
            {
                return;
            }

            SelectedGroupName = Convert.ToString(groupNameCell.Value);
            SelectedGroupId = groupIdCell != null ? Convert.ToInt32(groupIdCell.Value) : 0;

            if (TargetTextBox != null)
            {
                TargetTextBox.Text = SelectedGroupName;
            }
            else
            {
                ItemMaster = Application.OpenForms.OfType<frmItemMasterNew>().FirstOrDefault();
                if (ItemMaster?.txt_Group != null)
                {
                    ItemMaster.txt_Group.Text = SelectedGroupName;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        // Ensure column headers adopt the themed colors/fonts after the data binds
        private void dgv_Grop_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                Color headerBlue = Color.FromArgb(0, 123, 255);

                e.Layout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;
                e.Layout.Override.MinRowHeight = 30;
                e.Layout.Override.DefaultRowHeight = 30;

                // Ensure caption/header above column headers is hidden
                e.Layout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;

                // Center text by default
                e.Layout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                e.Layout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Style headers for all columns
                if (e.Layout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                    {
                        col.Header.Appearance.BackColor = headerBlue;
                        col.Header.Appearance.BackColor2 = headerBlue;
                        col.Header.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                        col.Header.Appearance.ForeColor = Color.White;
                        col.Header.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                        col.Header.Appearance.FontData.SizeInPoints = 9;
                        col.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                        col.Header.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        col.Header.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                    }
                }

                // Row selector to blue and compact
                e.Layout.Override.RowSelectorAppearance.BackColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;
                e.Layout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                e.Layout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None;
                e.Layout.Override.RowSelectorWidth = 15;

                // Disable filter indicators to match
                e.Layout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                e.Layout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                e.Layout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                e.Layout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                e.Layout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;
            }
            catch { }
        }

        // Locate textBox1 and attach search handlers
        private void WireSearchBoxHandlers()
        {
            try
            {
                Control[] found = this.Controls.Find("textBox1", true);
                if (found != null && found.Length > 0 && found[0] is TextBox)
                {
                    TextBox tb = (TextBox)found[0];
                    tb.TextChanged -= textBox1_TextChanged;
                    tb.TextChanged += textBox1_TextChanged;
                    tb.KeyDown -= textBox1_KeyDown;
                    tb.KeyDown += textBox1_KeyDown;
                }
            }
            catch { }
        }

        // Live filter grid rows by GroupName containing the search text
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string term = ((TextBox)sender).Text ?? string.Empty;
                ApplyRowFilter(term);
            }
            catch { }
        }

        // On Enter in search, focus grid and select current highlighted row
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Ensure grid has focus and an active row
                    if (this.dgv_Grop.Rows != null && this.dgv_Grop.Rows.VisibleRowCount > 0)
                    {
                        // If no active row, set first visible row active
                        if (this.dgv_Grop.ActiveRow == null || this.dgv_Grop.ActiveRow.Hidden)
                        {
                            UltraGridRow firstVisible = this.dgv_Grop.Rows.GetRowAtVisibleIndex(0);
                            if (firstVisible != null)
                            {
                                this.dgv_Grop.ActiveRow = firstVisible;
                                this.dgv_Grop.Selected.Rows.Clear();
                                this.dgv_Grop.Selected.Rows.Add(firstVisible);
                            }
                        }

                        // Select the active row (same as pressing Enter on grid)
                        if (this.dgv_Grop.ActiveRow != null && !this.dgv_Grop.ActiveRow.Hidden)
                        {
                            ApplyGroupSelection(this.dgv_Grop.ActiveRow);
                        }
                    }

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    // Move focus to grid and select first visible
                    this.dgv_Grop.Focus();
                    if (this.dgv_Grop.Rows != null && this.dgv_Grop.Rows.VisibleRowCount > 0)
                    {
                        UltraGridRow firstVisible = this.dgv_Grop.Rows.GetRowAtVisibleIndex(0);
                        if (firstVisible != null)
                        {
                            this.dgv_Grop.ActiveRow = firstVisible;
                            this.dgv_Grop.Selected.Rows.Clear();
                            this.dgv_Grop.Selected.Rows.Add(firstVisible);
                        }
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            catch { }
        }

        // Apply filtering by hiding rows that do not match GroupName (case-insensitive contains)
        private void ApplyRowFilter(string searchText)
        {
            try
            {
                string term = (searchText ?? string.Empty).Trim();
                bool hasTerm = term.Length > 0;
                string termLower = term.ToLowerInvariant();

                if (this.dgv_Grop.Rows == null) return;

                // Suspend painting for performance
                this.dgv_Grop.SuspendLayout();

                foreach (UltraGridRow row in this.dgv_Grop.Rows)
                {
                    bool match = true;
                    if (hasTerm)
                    {
                        string value = string.Empty;
                        if (row.Cells.Exists("GroupName") && row.Cells["GroupName"].Value != null)
                        {
                            value = Convert.ToString(row.Cells["GroupName"].Value);
                        }
                        match = value != null && value.ToLowerInvariant().Contains(termLower);
                    }
                    row.Hidden = !match;
                }

                // Ensure an active visible row after filtering
                if (this.dgv_Grop.Rows.VisibleRowCount > 0)
                {
                    UltraGridRow firstVisible = this.dgv_Grop.Rows.GetRowAtVisibleIndex(0);
                    if (firstVisible != null)
                    {
                        this.dgv_Grop.ActiveRow = firstVisible;
                        this.dgv_Grop.Selected.Rows.Clear();
                        this.dgv_Grop.Selected.Rows.Add(firstVisible);
                    }
                }

                this.dgv_Grop.ResumeLayout();
            }
            catch { }
        }

        // Initialize textBox3 with total count and update label1
        private void InitializeCountBoxAndLabel()
        {
            try
            {
                int total = (this.dgv_Grop.Rows != null) ? this.dgv_Grop.Rows.Count : 0;
                Control[] found3 = this.Controls.Find("textBox3", true);
                if (found3 != null && found3.Length > 0 && found3[0] is TextBox)
                {
                    ((TextBox)found3[0]).Text = total.ToString();
                }
                UpdateCountsLabel();
            }
            catch { }
        }

        // Hook up textBox3 handlers
        private void WireCountBoxHandlers()
        {
            try
            {
                Control[] found3 = this.Controls.Find("textBox3", true);
                if (found3 != null && found3.Length > 0 && found3[0] is TextBox)
                {
                    TextBox tb = (TextBox)found3[0];
                    tb.TextChanged -= textBox3_TextChanged;
                    tb.TextChanged += textBox3_TextChanged;
                    tb.KeyDown -= textBox3_KeyDown;
                    tb.KeyDown += textBox3_KeyDown;
                }
            }
            catch { }
        }

        // Initialize debounce timer for textBox3
        private void InitializeTextBox3DebounceTimer()
        {
            try
            {
                textBox3DebounceTimer = new System.Windows.Forms.Timer();
                textBox3DebounceTimer.Interval = 5; // Very short interval for instant response
                textBox3DebounceTimer.Tick += TextBox3DebounceTimer_Tick;
            }
            catch { }
        }

        // When textBox3 changes, apply numeric filter/limit live and update label
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Reset and restart the timer on each keystroke
                textBox3DebounceTimer.Stop();

                // If the value is the same as last processed, don't reprocess
                string currentValue = ((TextBox)sender).Text ?? string.Empty;
                if (currentValue == lastProcessedTextBox3Value)
                    return;

                // Start the timer to process after a very short delay
                textBox3DebounceTimer.Start();
            }
            catch { }
        }

        // Timer tick handler for debounced processing
        private void TextBox3DebounceTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Stop the timer
                textBox3DebounceTimer.Stop();

                // Process the value
                ProcessTextBox3ValueImmediate();
            }
            catch { }
        }

        // Process the textBox3 value immediately
        private void ProcessTextBox3ValueImmediate()
        {
            try
            {
                // Get current value from textBox3
                string currentValue = string.Empty;
                Control[] found3 = this.Controls.Find("textBox3", true);
                if (found3 != null && found3.Length > 0 && found3[0] is TextBox)
                {
                    currentValue = ((TextBox)found3[0]).Text ?? string.Empty;
                }

                // Store the value being processed to avoid reprocessing
                lastProcessedTextBox3Value = currentValue;

                // Apply filters and update label
                ApplyFiltersFromTextBox3();
                UpdateCountsLabel();
            }
            catch { }
        }

        // On Enter in textBox3, apply filter/limit and update label
        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Stop timer and process immediately
                    textBox3DebounceTimer.Stop();
                    ProcessTextBox3ValueImmediate();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            catch { }
        }

        // Update label1 with current visible vs total counts
        private void UpdateCountsLabel()
        {
            try
            {
                int total = (this.dgv_Grop.Rows != null) ? this.dgv_Grop.Rows.Count : 0;
                int visible = 0;
                if (this.dgv_Grop.Rows != null)
                {
                    foreach (UltraGridRow r in this.dgv_Grop.Rows)
                    {
                        if (!r.Hidden) visible++;
                    }
                }

                Control[] found = this.Controls.Find("label1", true);
                if (found != null && found.Length > 0 && found[0] is Label)
                {
                    Label lbl = (Label)found[0];
                    lbl.Text = $"Showing {visible} of {total} records";
                }
            }
            catch { }
        }

        // Apply filters based on textBox3 input: numbers filter rows; empty/all resets to show all
        private void ApplyFiltersFromTextBox3()
        {
            try
            {
                if (this.dgv_Grop.Rows == null) return;

                // Read term
                string input = string.Empty;
                Control[] found3 = this.Controls.Find("textBox3", true);
                if (found3 != null && found3.Length > 0 && found3[0] is TextBox)
                {
                    input = ((TextBox)found3[0]).Text.Trim();
                }

                // If empty or "all" → show all items (reset any previous filtering)
                if (string.IsNullOrEmpty(input) || string.Equals(input, "all", StringComparison.OrdinalIgnoreCase))
                {
                    // Show all rows (reset any hiding from previous filters)
                    this.dgv_Grop.SuspendLayout();
                    foreach (UltraGridRow row in this.dgv_Grop.Rows)
                    {
                        row.Hidden = false;
                    }
                    this.dgv_Grop.ResumeLayout();
                    EnsureFirstVisibleRowActive();
                    return;
                }

                // Try to parse as number for row limiting
                if (int.TryParse(input, out int limit) && limit > 0)
                {
                    // Show all rows first, then apply limit
                    this.dgv_Grop.SuspendLayout();
                    foreach (UltraGridRow row in this.dgv_Grop.Rows)
                    {
                        row.Hidden = false;
                    }
                    this.dgv_Grop.ResumeLayout();

                    // Apply row limit to show only last N items
                    ApplyRowLimit(limit);
                    EnsureFirstVisibleRowActive();
                    return;
                }

                // If not a valid number, show all items (reset)
                this.dgv_Grop.SuspendLayout();
                foreach (UltraGridRow row in this.dgv_Grop.Rows)
                {
                    row.Hidden = false;
                }
                this.dgv_Grop.ResumeLayout();
                EnsureFirstVisibleRowActive();
            }
            catch { }
        }

        // Apply a limit to currently visible rows (keep only the last N visible rows)
        private void ApplyRowLimit(int limit)
        {
            try
            {
                if (this.dgv_Grop.Rows == null) return;

                // Build list of indices for currently visible rows
                List<UltraGridRow> visibleRows = new List<UltraGridRow>();
                foreach (UltraGridRow r in this.dgv_Grop.Rows)
                {
                    if (!r.Hidden) visibleRows.Add(r);
                }

                // If limit is invalid or >= visible count, show all visible rows
                if (limit <= 0 || limit >= visibleRows.Count)
                {
                    return;
                }

                // Determine rows to keep (last N visible)
                int keepStart = Math.Max(0, visibleRows.Count - limit);
                HashSet<UltraGridRow> keep = new HashSet<UltraGridRow>(visibleRows.GetRange(keepStart, visibleRows.Count - keepStart));

                // Now hide any visible rows not in keep set
                foreach (UltraGridRow r in visibleRows)
                {
                    r.Hidden = !keep.Contains(r);
                }
            }
            catch { }
        }

        // Ensure first visible row is active/selected
        private void EnsureFirstVisibleRowActive()
        {
            try
            {
                if (this.dgv_Grop.Rows != null && this.dgv_Grop.Rows.VisibleRowCount > 0)
                {
                    UltraGridRow firstVisible = this.dgv_Grop.Rows.GetRowAtVisibleIndex(0);
                    if (firstVisible != null)
                    {
                        this.dgv_Grop.ActiveRow = firstVisible;
                        this.dgv_Grop.Selected.Rows.Clear();
                        this.dgv_Grop.Selected.Rows.Add(firstVisible);
                    }
                }
            }
            catch { }
        }

        // Apply style to ultraPanel5, ultraPanel6, ultraPanel7, ultraPanel3, ultraPanel4 to match frmBrandDialog's ultraPanel5
        private void ApplyPanelStyles()
        {
            try
            {
                // Find and style the panels
                StyleIconPanel(FindControlByName(this, "ultraPanel5") as Infragistics.Win.Misc.UltraPanel);
                StyleIconPanel(FindControlByName(this, "ultraPanel6") as Infragistics.Win.Misc.UltraPanel);
                StyleIconPanel(FindControlByName(this, "ultraPanel7") as Infragistics.Win.Misc.UltraPanel);
                StyleIconPanel(FindControlByName(this, "ultraPanel3") as Infragistics.Win.Misc.UltraPanel);
                StyleIconPanel(FindControlByName(this, "ultraPanel4") as Infragistics.Win.Misc.UltraPanel);
            }
            catch { }
        }

        // Style helper matching frmBrandDialog.StyleIconPanel
        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;

            // Colors as used in frmBrandDialog
            Color lightBlue = Color.FromArgb(127, 219, 255);
            Color darkBlue = Color.FromArgb(0, 116, 217);
            Color borderBlue = Color.FromArgb(0, 150, 220);
            Color borderBase = Color.FromArgb(0, 100, 180);

            // Background gradient
            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Rounded border and colors
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
            panel.Appearance.BorderColor = borderBlue;
            panel.Appearance.BorderColor3DBase = borderBase;

            // Style child controls
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    var pic = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;
                    pic.Cursor = Cursors.Hand;

                    pic.MouseEnter += (s, e) => { pic.Appearance.BorderColor = Color.White; };
                    pic.MouseLeave += (s, e) => { pic.Appearance.BorderColor = Color.Transparent; };
                }
                else if (control is Label)
                {
                    Label lbl = (Label)control;
                    lbl.ForeColor = Color.White;
                    lbl.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    lbl.BackColor = Color.Transparent;
                    lbl.Cursor = Cursors.Hand;
                    lbl.MouseEnter += (s, e) => { lbl.ForeColor = Color.White; };
                    lbl.MouseLeave += (s, e) => { lbl.ForeColor = Color.White; };
                }
            }

            // Hover effect
            panel.ClientArea.MouseEnter += (s, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            panel.ClientArea.MouseLeave += (s, e) =>
            {
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
                panel.ClientArea.Cursor = Cursors.Default;
            };
        }

        /// <summary>
        /// Helper method to find a control by name recursively
        /// </summary>
        private Control FindControlByName(Control container, string name)
        {
            try
            {
                // Check if the container itself is the control we're looking for
                if (container.Name == name)
                    return container;

                // Search through all child controls
                foreach (Control ctrl in container.Controls)
                {
                    // Recursively check this control and its children
                    Control foundControl = FindControlByName(ctrl, name);
                    if (foundControl != null)
                        return foundControl;
                }

                // Not found in this container
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding control '{name}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Click handler for Panel4 - Opens FrmGroup in UltraTabControl
        /// </summary>
        private void Panel4_Click(object sender, EventArgs e)
        {
            try
            {
                // Find the main Home form that contains tabControlMain
                Form homeForm = FindHomeForm();

                if (homeForm != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Home form found: {homeForm.Name}");

                    // Get the tabControlMain from the Home form
                    var tabControlMain = GetTabControlFromHome(homeForm);

                    if (tabControlMain != null)
                    {
                        // Check if Group Master tab already exists
                        foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                        {
                            if (tab.Text == "Group Master")
                            {
                                tabControlMain.SelectedTab = tab;
                                System.Diagnostics.Debug.WriteLine("Group Master tab already exists, selected existing tab");

                                // Close the frmGroupDialog form
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                return;
                            }
                        }

                        // Create new tab using the same approach as Home.cs
                        string uniqueKey = $"Tab_{DateTime.Now.Ticks}_Group Master";
                        var newTab = tabControlMain.Tabs.Add(uniqueKey, "Group Master");

                        // Create and configure FrmGroup for embedding (same as Home.cs)
                        FrmGroup groupForm = new FrmGroup();
                        groupForm.TopLevel = false;
                        groupForm.FormBorderStyle = FormBorderStyle.None;
                        groupForm.Dock = DockStyle.Fill;
                        groupForm.Visible = true;
                        groupForm.BackColor = SystemColors.Control;

                        // Ensure form is properly initialized
                        if (!groupForm.IsHandleCreated)
                        {
                            groupForm.CreateControl();
                        }

                        // Add the form to the tab page
                        newTab.TabPage.Controls.Add(groupForm);

                        // Show the form AFTER adding to tab page
                        groupForm.Show();
                        groupForm.BringToFront();

                        // Set the new tab as active/selected
                        tabControlMain.SelectedTab = newTab;

                        // Force refresh to ensure proper display
                        newTab.TabPage.Refresh();
                        groupForm.Refresh();
                        tabControlMain.Refresh();

                        // Wire up the form's FormClosed event to remove the tab
                        groupForm.FormClosed += (formSender, formE) =>
                        {
                            try
                            {
                                if (newTab != null && tabControlMain.Tabs.Contains(newTab))
                                {
                                    tabControlMain.Tabs.Remove(newTab);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error removing tab: {ex.Message}");
                            }
                        };

                        System.Diagnostics.Debug.WriteLine("FrmGroup opened in UltraTabControl using Home.cs approach");

                        // Close the frmGroupDialog form after successfully opening FrmGroup
                        // Add a small delay to ensure FrmGroup is fully loaded
                        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }));
                        });
                    }
                    else
                    {
                        // Fallback: show as regular form
                        FrmGroup groupForm = new FrmGroup();
                        groupForm.Show();
                        System.Diagnostics.Debug.WriteLine("FrmGroup opened as regular form (tabControlMain not found)");

                        // Close the frmGroupDialog form with delay
                        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }));
                        });
                    }
                }
                else
                {
                    // If no Home form found, show as a regular form
                    FrmGroup groupForm = new FrmGroup();
                    groupForm.Show();
                    System.Diagnostics.Debug.WriteLine("FrmGroup opened as regular form (no Home form found)");

                    // Close the frmGroupDialog form with delay
                    System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }));
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error opening FrmGroup: " + ex.Message);
                MessageBox.Show("Error opening Group Master: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Helper method to find the Home form
        /// </summary>
        private Form FindHomeForm()
        {
            try
            {
                // Look for Home form in all open forms
                foreach (Form form in Application.OpenForms)
                {
                    if (form.GetType().Name == "Home" || form.Name == "Home")
                    {
                        System.Diagnostics.Debug.WriteLine($"Found Home form: {form.Name}");
                        return form;
                    }
                }

                System.Diagnostics.Debug.WriteLine("Home form not found");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding Home form: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Helper method to get tabControlMain from Home form
        /// </summary>
        private Infragistics.Win.UltraWinTabControl.UltraTabControl GetTabControlFromHome(Form homeForm)
        {
            try
            {
                // Look for tabControlMain in the Home form
                var tabControl = FindControlByName(homeForm, "tabControlMain");
                if (tabControl is Infragistics.Win.UltraWinTabControl.UltraTabControl)
                {
                    System.Diagnostics.Debug.WriteLine("Found tabControlMain in Home form");
                    return tabControl as Infragistics.Win.UltraWinTabControl.UltraTabControl;
                }

                System.Diagnostics.Debug.WriteLine("tabControlMain not found in Home form");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting tabControlMain: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Set up hover effects for a panel group
        /// </summary>
        private void SetupPanelGroupHoverEffects(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            if (panel == null) return;

            // Store original colors
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;

            // Define hover colors - brighter versions of the original colors
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);

            // Create actions for mouse enter and leave
            Action applyHoverEffect = () =>
            {
                // Change panel colors
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;

                // Change cursor to hand
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            Action removeHoverEffect = () =>
            {
                // Restore original colors
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;

                // Restore cursor
                panel.ClientArea.Cursor = Cursors.Default;
            };

            // Add hover effects to the panel
            panel.MouseEnter += (s, e) =>
            {
                applyHoverEffect();
            };

            panel.MouseLeave += (s, e) =>
            {
                removeHoverEffect();
            };

            // Add hover effects to the picture box if provided
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) =>
                {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    pictureBox.Cursor = Cursors.Hand;
                };

                pictureBox.MouseLeave += (s, e) =>
                {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel))
                    {
                        removeHoverEffect();
                    }
                };
            }

            // Add hover effects to the label if provided
            if (label != null)
            {
                label.MouseEnter += (s, e) =>
                {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    label.Cursor = Cursors.Hand;
                };

                label.MouseLeave += (s, e) =>
                {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel))
                    {
                        removeHoverEffect();
                    }
                };
            }
        }

        /// <summary>
        /// Helper method to brighten a color
        /// </summary>
        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        /// <summary>
        /// Helper method to check if mouse is over a control
        /// </summary>
        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }
    }
}
