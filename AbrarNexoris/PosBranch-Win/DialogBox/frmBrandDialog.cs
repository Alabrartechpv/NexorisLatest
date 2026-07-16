using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinTabControl;
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
    public partial class frmBrandDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        public TextBox TargetTextBox { get; set; }
        public int SelectedBrandId { get; private set; }
        public string SelectedBrandName { get; private set; }

        // Add debounce timer and tracking for textBox3
        private System.Windows.Forms.Timer textBox3DebounceTimer;
        private string lastProcessedTextBox3Value = string.Empty;

        public frmBrandDialog()
        {
            InitializeComponent();
        }

        private void frmBrandDialog_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "GETALL";
            BrandDDLGrid brand = drop.getBrandDDL();
            brand.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            // Apply consistent UltraGrid look (match frmdialForItemMaster)
            SetupUltraGridStyle();
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = brand.List;

            // Wire double-click to select brand
            this.ultraGrid1.DoubleClickCell -= ultraGrid1_DoubleClickCell;
            this.ultraGrid1.DoubleClickCell += ultraGrid1_DoubleClickCell;

            // Hide any header label that displays "Brand"
            HideBrandHeaderLabels(this);

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
            SetupPanelGroupHoverEffects(this.ultraPanel4, this.label4, this.ultraPictureBox3);

            // Add click handlers for ultraPanel4
            this.ultraPanel4.Click += Panel4_Click;
            this.ultraPanel4.ClientArea.Click += Panel4_Click;
            this.label4.Click += Panel4_Click;
            this.ultraPictureBox3.Click += Panel4_Click;

            // Connect OK button click events (ultraPanel5)
            this.ultraPanel5.Click += PerformOKAction;
            this.ultraPanel5.ClientArea.Click += PerformOKAction;
            this.ultraPictureBox1.Click += PerformOKAction;
            this.label5.Click += PerformOKAction;

            // Connect Close button click events (ultraPanel6)
            this.ultraPanel6.Click += PerformCloseAction;
            this.ultraPanel6.ClientArea.Click += PerformCloseAction;
            this.ultraPictureBox2.Click += PerformCloseAction;
            this.label3.Click += PerformCloseAction;

            // Ensure there is an active row to prevent null ActiveRow on first key press
            try
            {
                if (this.ultraGrid1.Rows != null && this.ultraGrid1.Rows.Count > 0)
                {
                    this.ultraGrid1.ActiveRow = this.ultraGrid1.Rows[0];
                }
            }
            catch { }

            // Use Shown event to set focus on textBox1 (more reliable than Load event)
            this.Shown += (s, evt) =>
            {
                // Set focus to the search textbox when form is fully displayed
                if (textBox1 != null)
                {
                    textBox1.Focus();
                    textBox1.SelectAll();
                }
            };
        }

        // Event handler for OK button (ultraPanel5) click
        private void PerformOKAction(object sender, EventArgs e)
        {
            try
            {
                // If there's an active row, select it
                if (this.ultraGrid1.ActiveRow != null && !this.ultraGrid1.ActiveRow.Hidden)
                {
                    SelectBrandFromRow(this.ultraGrid1.ActiveRow);
                }
                else if (this.ultraGrid1.Rows != null && this.ultraGrid1.Rows.VisibleRowCount > 0)
                {
                    // If no active row, select first visible row
                    UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                    if (firstVisible != null)
                    {
                        SelectBrandFromRow(firstVisible);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a brand first.", "No Brand Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PerformOKAction: {ex.Message}");
            }
        }

        // Event handler for Close button (ultraPanel6) click
        private void PerformCloseAction(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Close button clicked in frmBrandDialog");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PerformCloseAction: {ex.Message}");
            }
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

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only act on Enter; ignore other key presses to avoid firing with no selection
            if (e.KeyChar != (char)Keys.Enter)
                return;

            try
            {
                // Find the open Item Master form safely
                ItemMaster = Application.OpenForms
                    .OfType<frmItemMasterNew>()
                    .FirstOrDefault();

                if (ItemMaster == null)
                {
                    // No target form is open; just close the dialog quietly
                    this.Close();
                    return;
                }

                var row = this.ultraGrid1.ActiveRow;
                if (row == null || row.IsGroupByRow)
                    return;

                SelectBrandFromRow(row);
            }
            catch
            {
                // Swallow to avoid crashing the app due to unexpected nulls
            }

        }

        // Handle double-click on a grid cell to select the brand
        private void ultraGrid1_DoubleClickCell(object sender, Infragistics.Win.UltraWinGrid.DoubleClickCellEventArgs e)
        {
            SelectBrandFromRow(e.Cell.Row);
        }

        // Shared logic to transfer selected brand to Item Master and close
        private void SelectBrandFromRow(UltraGridRow row)
        {
            try
            {
                if (row == null || row.IsGroupByRow)
                    return;

                UltraGridCell brandNameCell = row.Cells.Exists("BrandName") ? row.Cells["BrandName"] : null;
                UltraGridCell brandIdCell = row.Cells.Exists("Id") ? row.Cells["Id"] : null;
                if (brandNameCell == null)
                    return;

                SelectedBrandName = Convert.ToString(brandNameCell.Value);
                SelectedBrandId = brandIdCell != null ? Convert.ToInt32(brandIdCell.Value) : 0;

                if (TargetTextBox != null)
                {
                    TargetTextBox.Text = SelectedBrandName;
                }
                else
                {
                    ItemMaster = Application.OpenForms
                        .OfType<frmItemMasterNew>()
                        .FirstOrDefault();

                    if (ItemMaster?.txt_Brand != null)
                    {
                        ItemMaster.txt_Brand.Text = SelectedBrandName;
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch { }
        }

        // Hide labels titled "Brand" anywhere on the form
        private void HideBrandHeaderLabels(Control root)
        {
            try
            {
                foreach (Control c in root.Controls)
                {
                    if (c is Label)
                    {
                        Label lbl = (Label)c;
                        string text = (lbl.Text ?? string.Empty).Trim();
                        if (string.Equals(text, "Brand", StringComparison.OrdinalIgnoreCase))
                        {
                            lbl.Visible = false;
                        }
                    }

                    if (c.HasChildren)
                    {
                        HideBrandHeaderLabels(c);
                    }
                }
            }
            catch { }
        }

        // Apply style to ultraPanel5, ultraPanel7, ultraPanel6, ultraPanel3, ultraPanel4 to match frmItemMasterNew's ultraPanel5
        private void ApplyPanelStyles()
        {
            try
            {
                StyleIconPanel(this.ultraPanel5);
                StyleIconPanel(this.ultraPanel7);
                StyleIconPanel(this.ultraPanel6);
                StyleIconPanel(this.ultraPanel3);
                StyleIconPanel(this.ultraPanel4);
            }
            catch { }
        }

        // Style helper matching frmItemMasterNew.StyleIconPanel
        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;

            // Colors as used in frmItemMasterNew
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

        // Initialize textBox3 with total count and update label1
        private void InitializeCountBoxAndLabel()
        {
            try
            {
                int total = (this.ultraGrid1.Rows != null) ? this.ultraGrid1.Rows.Count : 0;
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
                int total = (this.ultraGrid1.Rows != null) ? this.ultraGrid1.Rows.Count : 0;
                int visible = 0;
                if (this.ultraGrid1.Rows != null)
                {
                    foreach (UltraGridRow r in this.ultraGrid1.Rows)
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
                if (this.ultraGrid1.Rows == null) return;

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
                    this.ultraGrid1.SuspendLayout();
                    foreach (UltraGridRow row in this.ultraGrid1.Rows)
                    {
                        row.Hidden = false;
                    }
                    this.ultraGrid1.ResumeLayout();
                    EnsureFirstVisibleRowActive();
                    return;
                }

                // Try to parse as number for row limiting
                if (int.TryParse(input, out int limit) && limit > 0)
                {
                    // Show all rows first, then apply limit
                    this.ultraGrid1.SuspendLayout();
                    foreach (UltraGridRow row in this.ultraGrid1.Rows)
                    {
                        row.Hidden = false;
                    }
                    this.ultraGrid1.ResumeLayout();

                    // Apply row limit to show only last N items
                    ApplyRowLimit(limit);
                    EnsureFirstVisibleRowActive();
                    return;
                }

                // If not a valid number, show all items (reset)
                this.ultraGrid1.SuspendLayout();
                foreach (UltraGridRow row in this.ultraGrid1.Rows)
                {
                    row.Hidden = false;
                }
                this.ultraGrid1.ResumeLayout();
                EnsureFirstVisibleRowActive();
            }
            catch { }
        }

        // Apply a limit to currently visible rows (keep only the last N visible rows)
        private void ApplyRowLimit(int limit)
        {
            try
            {
                if (this.ultraGrid1.Rows == null) return;

                // Build list of indices for currently visible rows
                List<UltraGridRow> visibleRows = new List<UltraGridRow>();
                foreach (UltraGridRow r in this.ultraGrid1.Rows)
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
                if (this.ultraGrid1.Rows != null && this.ultraGrid1.Rows.VisibleRowCount > 0)
                {
                    UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                    if (firstVisible != null)
                    {
                        this.ultraGrid1.ActiveRow = firstVisible;
                        this.ultraGrid1.Selected.Rows.Clear();
                        this.ultraGrid1.Selected.Rows.Add(firstVisible);
                    }
                }
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

        // Live filter grid rows by BrandName containing the search text
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
                    if (this.ultraGrid1.Rows != null && this.ultraGrid1.Rows.VisibleRowCount > 0)
                    {
                        // If no active row, set first visible row active
                        if (this.ultraGrid1.ActiveRow == null || this.ultraGrid1.ActiveRow.Hidden)
                        {
                            UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                            if (firstVisible != null)
                            {
                                this.ultraGrid1.ActiveRow = firstVisible;
                                this.ultraGrid1.Selected.Rows.Clear();
                                this.ultraGrid1.Selected.Rows.Add(firstVisible);
                            }
                        }

                        // Select the active row (same as pressing Enter on grid)
                        if (this.ultraGrid1.ActiveRow != null && !this.ultraGrid1.ActiveRow.Hidden)
                        {
                            SelectBrandFromRow(this.ultraGrid1.ActiveRow);
                        }
                    }

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    // Move focus to grid and select first visible
                    this.ultraGrid1.Focus();
                    if (this.ultraGrid1.Rows != null && this.ultraGrid1.Rows.VisibleRowCount > 0)
                    {
                        UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                        if (firstVisible != null)
                        {
                            this.ultraGrid1.ActiveRow = firstVisible;
                            this.ultraGrid1.Selected.Rows.Clear();
                            this.ultraGrid1.Selected.Rows.Add(firstVisible);
                        }
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            catch { }
        }

        // Apply filtering by hiding rows that do not match BrandName (case-insensitive contains)
        private void ApplyRowFilter(string searchText)
        {
            try
            {
                string term = (searchText ?? string.Empty).Trim();
                bool hasTerm = term.Length > 0;
                string termLower = term.ToLowerInvariant();

                if (this.ultraGrid1.Rows == null) return;

                // Suspend painting for performance
                this.ultraGrid1.SuspendLayout();

                foreach (UltraGridRow row in this.ultraGrid1.Rows)
                {
                    bool match = true;
                    if (hasTerm)
                    {
                        string value = string.Empty;
                        if (row.Cells.Exists("BrandName") && row.Cells["BrandName"].Value != null)
                        {
                            value = Convert.ToString(row.Cells["BrandName"].Value);
                        }
                        match = value != null && value.ToLowerInvariant().Contains(termLower);
                    }
                    row.Hidden = !match;
                }

                // Ensure an active visible row after filtering
                if (this.ultraGrid1.Rows.VisibleRowCount > 0)
                {
                    UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                    if (firstVisible != null)
                    {
                        this.ultraGrid1.ActiveRow = firstVisible;
                        this.ultraGrid1.Selected.Rows.Clear();
                        this.ultraGrid1.Selected.Rows.Add(firstVisible);
                    }
                }

                this.ultraGrid1.ResumeLayout();
            }
            catch { }
        }
        // Apply the same UltraGrid theme/look as in frmdialForItemMaster
        private void SetupUltraGridStyle()
        {
            try
            {
                this.ultraGrid1.DisplayLayout.Reset();

                // Behavior
                this.ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                this.ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                this.ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                this.ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                this.ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                this.ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;

                // Column interactions
                this.ultraGrid1.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                this.ultraGrid1.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                this.ultraGrid1.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;
                this.ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;

                // GroupBy box
                this.ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                this.ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Hide grid caption/header above column headers
                this.ultraGrid1.Text = string.Empty;
                this.ultraGrid1.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;

                // Borders and grid lines
                this.ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                this.ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                this.ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                this.ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                this.ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;

                // Spacing
                this.ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                this.ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                this.ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                this.ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                this.ultraGrid1.DisplayLayout.InterBandSpacing = 0;

                // Colors
                Color lightBlue = Color.FromArgb(173, 216, 230);
                Color headerBlue = Color.FromArgb(0, 123, 255);
                this.ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                this.ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                this.ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Row height
                this.ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                this.ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;

                // Header styling
                this.ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Row selector styling
                this.ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                this.ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                this.ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                this.ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                this.ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None;
                this.ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;
                this.ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Image = null;
                this.ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Image = null;
                this.ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.Image = null;

                // Row backgrounds
                this.ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                this.ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                this.ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                this.ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                this.ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Selection/active colors
                Color selectionBlue = Color.FromArgb(0, 120, 215);
                this.ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectionBlue;
                this.ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = selectionBlue;
                this.ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                this.ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectionBlue;
                this.ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = selectionBlue;
                this.ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                this.ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Fonts
                this.ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                this.ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                this.ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                this.ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Scrollbars
                this.ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                this.ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
                if (this.ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    this.ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Alignment
                this.ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                this.ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

                // Auto-fit and filtering settings to match
                this.ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;
                this.ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.None;
                this.ultraGrid1.DisplayLayout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                this.ultraGrid1.DisplayLayout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                this.ultraGrid1.DisplayLayout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                this.ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;

                // InitializeLayout handler for per-column header re-style after binding
                this.ultraGrid1.InitializeLayout -= ultraGrid1_InitializeLayout;
                this.ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            }
            catch { }
        }

        // Ensure column headers adopt the themed colors/fonts after the data binds
        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
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

        /// <summary>
        /// Click handler for Panel4 - Opens FrmBrand in UltraTabControl
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
                        // Check if Brand Master tab already exists
                        foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                        {
                            if (tab.Text == "Brand Master")
                            {
                                tabControlMain.SelectedTab = tab;
                                System.Diagnostics.Debug.WriteLine("Brand Master tab already exists, selected existing tab");

                                // Close the frmBrandDialog form
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                return;
                            }
                        }

                        // Create new tab using the same approach as Home.cs
                        string uniqueKey = $"Tab_{DateTime.Now.Ticks}_Brand Master";
                        var newTab = tabControlMain.Tabs.Add(uniqueKey, "Brand Master");

                        // Create and configure FrmBrand for embedding (same as Home.cs)
                        FrmBrand brandForm = new FrmBrand();
                        brandForm.TopLevel = false;
                        brandForm.FormBorderStyle = FormBorderStyle.None;
                        brandForm.Dock = DockStyle.Fill;
                        brandForm.Visible = true;
                        brandForm.BackColor = SystemColors.Control;

                        // Ensure form is properly initialized
                        if (!brandForm.IsHandleCreated)
                        {
                            brandForm.CreateControl();
                        }

                        // Add the form to the tab page
                        newTab.TabPage.Controls.Add(brandForm);

                        // Show the form AFTER adding to tab page
                        brandForm.Show();
                        brandForm.BringToFront();

                        // Set the new tab as active/selected
                        tabControlMain.SelectedTab = newTab;

                        // Force refresh to ensure proper display
                        newTab.TabPage.Refresh();
                        brandForm.Refresh();
                        tabControlMain.Refresh();

                        // Wire up the form's FormClosed event to remove the tab
                        brandForm.FormClosed += (formSender, formE) =>
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

                        System.Diagnostics.Debug.WriteLine("FrmBrand opened in UltraTabControl using Home.cs approach");

                        // Close the frmBrandDialog form after successfully opening FrmBrand
                        // Add a small delay to ensure FrmBrand is fully loaded
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
                        FrmBrand brandForm = new FrmBrand();
                        brandForm.Show();
                        System.Diagnostics.Debug.WriteLine("FrmBrand opened as regular form (tabControlMain not found)");

                        // Close the frmBrandDialog form with delay
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
                    FrmBrand brandForm = new FrmBrand();
                    brandForm.Show();
                    System.Diagnostics.Debug.WriteLine("FrmBrand opened as regular form (no Home form found)");

                    // Close the frmBrandDialog form with delay
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
                System.Diagnostics.Debug.WriteLine("Error opening FrmBrand: " + ex.Message);
                MessageBox.Show("Error opening Brand Master: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
