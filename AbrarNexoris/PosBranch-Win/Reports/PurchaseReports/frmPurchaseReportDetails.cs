using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.PurchaseReports
{
    /// <summary>
    /// Professional Purchase Report Form with Hierarchical Master-Detail UltraGrid
    /// </summary>
    public partial class frmPurchaseReportDetails : Form
    {
        #region Helper Classes
        private sealed class ColumnItem
        {
            public ColumnItem(string columnKey, string displayText, int bandIndex = 0)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
                BandIndex = bandIndex;
            }

            public string ColumnKey { get; }
            public string DisplayText { get; }
            public int BandIndex { get; }

            public override string ToString()
            {
                return DisplayText;
            }
        }
        #endregion

        #region Private Fields
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonLightOutline = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private PurchaseReportRepository reportRepository;
        private DataSet dsHierarchical;
        private bool isLoading = false;

        // Grid calculation footer fields
        private readonly Dictionary<string, Label> footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Column chooser & drag-drop fields
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private System.Windows.Forms.ToolTip gridToolTip = new System.Windows.Forms.ToolTip();
        private Point headerDragStartPoint;
        private UltraGridColumn columnToHideByDrag;
        private bool isDraggingHeaderColumn;
        private readonly Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        #endregion

        #region Constructor
        public frmPurchaseReportDetails()
        {
            InitializeComponent();
            InitializeForm();
        }
        #endregion

        #region Form Initialization
        private void InitializeForm()
        {
            try
            {
                reportRepository = new PurchaseReportRepository();

                // Set form properties
                this.Text = "Purchase Details Report";
                if (this.TopLevel)
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.StartPosition = FormStartPosition.CenterScreen;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.Dock = DockStyle.Fill;
                }
                this.MinimumSize = new Size(0, 0);

                // Initialize date controls
                InitializeDateControls();

                // Initialize search controls
                InitializeSearchControls();

                // Initialize hierarchical DataSet
                InitializeHierarchicalDataSet();

                // Setup Hierarchical Grid
                SetupHierarchicalGrid();

                // Initialize panels
                InitializePanels();

                // Initialize control positions and IRS POS layout
                InitializeControlLayout();

                // Initialize Grid Footer & Calculation Bar
                InitializeGridFooter();

                // Initialize Grid Context Menu and Drag-Drop
                InitializeGridContextMenuAndDragDrop();

                // Don't load data here - will load in Form_Load event after form is shown
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeDateControls()
        {
            // Set default date range (ALL records)
            ultraDateTimeEditorFrom.Value = new DateTime(1990, 1, 1);
            ultraDateTimeEditorTo.Value = DateTime.Today;

            // Set date format
            ultraDateTimeEditorFrom.FormatString = "dd/MM/yyyy";
            ultraDateTimeEditorTo.FormatString = "dd/MM/yyyy";
        }

        private void InitializeSearchControls()
        {
            // Initialize Date Mode options: ALL and Date by Range (matching IRS POS style)
            ultraComboPresetDates.Items.Clear();
            ultraComboPresetDates.Items.Add("ALL", "ALL");
            ultraComboPresetDates.Items.Add("DATE_RANGE", "Date by Range");

            // Set default to "ALL"
            ultraComboPresetDates.Value = "ALL";

            // Initialize numeric editors
            ultraNumericEditorPurchaseNo.FormatString = "0";

            // Set placeholder text
            ultraTextEditorVendor.NullText = "Enter vendor name...";

            // Add tooltips for better UX
            InitializeTooltips();

            // Style buttons
            StyleButtons();

            // Add keyboard shortcuts
            SetupKeyboardShortcuts();
        }

        private void InitializeControlLayout()
        {
            // Remove / Hide Amount filters completely
            ultraLabelAmountFrom.Visible = false;
            ultraNumericEditorAmountFrom.Visible = false;
            ultraLabelAmountTo.Visible = false;
            ultraNumericEditorAmountTo.Visible = false;

            // Vendor & Purchase No (Row 1)
            ultraLabelVendorSearch.Text = "Vendor";
            ultraLabelVendorSearch.Location = new Point(20, 15);
            ultraLabelVendorSearch.Size = new Size(55, 20);
            ultraLabelVendorSearch.Appearance.ForeColor = ControlTextColor;
            ultraLabelVendorSearch.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            ultraTextEditorVendor.Location = new Point(80, 12);
            ultraTextEditorVendor.Size = new Size(236, 24);
            ultraTextEditorVendor.KeyDown += UltraTextEditorVendor_KeyDown;

            // Picture Box for Vendor Search (Opens Vendor Dialog)
            pbVendorSearch.Location = new Point(320, 12);
            pbVendorSearch.Size = new Size(26, 24);
            pbVendorSearch.Cursor = Cursors.Hand;
            pbVendorSearch.BackColor = Color.FromArgb(232, 241, 252);
            pbVendorSearch.Paint += PbVendorSearch_Paint;
            pbVendorSearch.MouseEnter += (s, e) => { pbVendorSearch.BackColor = Color.FromArgb(215, 235, 255); };
            pbVendorSearch.MouseLeave += (s, e) => { pbVendorSearch.BackColor = Color.FromArgb(232, 241, 252); };
            pbVendorSearch.Click += PbVendorSearch_Click;

            ultraLabelPurchaseNoSearch.Text = "Doc No";
            ultraLabelPurchaseNoSearch.Location = new Point(360, 15);
            ultraLabelPurchaseNoSearch.Size = new Size(60, 20);
            ultraLabelPurchaseNoSearch.Appearance.ForeColor = ControlTextColor;
            ultraLabelPurchaseNoSearch.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            ultraNumericEditorPurchaseNo.Location = new Point(425, 12);
            ultraNumericEditorPurchaseNo.Size = new Size(110, 24);

            // Date Preset & Pickers (Row 2)
            ultraLabelPreset.Text = "Date";
            ultraLabelPreset.Location = new Point(20, 46);
            ultraLabelPreset.Size = new Size(55, 20);
            ultraLabelPreset.Appearance.ForeColor = ControlTextColor;
            ultraLabelPreset.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            ultraComboPresetDates.Location = new Point(80, 44);
            ultraComboPresetDates.Size = new Size(180, 24);

            ultraLabelFromDate.Text = "From";
            ultraLabelFromDate.Location = new Point(280, 46);
            ultraLabelFromDate.Size = new Size(40, 20);
            ultraLabelFromDate.Appearance.ForeColor = ControlTextColor;
            ultraLabelFromDate.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            ultraDateTimeEditorFrom.Location = new Point(325, 44);
            ultraDateTimeEditorFrom.Size = new Size(115, 24);

            ultraLabelToDate.Text = "To";
            ultraLabelToDate.Location = new Point(455, 46);
            ultraLabelToDate.Size = new Size(25, 20);
            ultraLabelToDate.Appearance.ForeColor = ControlTextColor;
            ultraLabelToDate.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            ultraDateTimeEditorTo.Location = new Point(485, 44);
            ultraDateTimeEditorTo.Size = new Size(115, 24);

            ultraPanelControls.Height = 78;

            UpdateDateControlVisibility();
            UpdateSelectionToggleButtonText();
        }

        private void PbVendorSearch_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw subtle border
            using (Pen borderPen = new Pen(ButtonLightOutline))
            {
                g.DrawRectangle(borderPen, 0, 0, pbVendorSearch.Width - 1, pbVendorSearch.Height - 1);
            }

            // Draw magnifying glass icon
            using (Pen iconPen = new Pen(ButtonTextBlue, 2f))
            {
                g.DrawEllipse(iconPen, 5, 4, 10, 10);
                g.DrawLine(iconPen, 13, 12, 19, 18);
            }
        }

        private void PbVendorSearch_Click(object sender, EventArgs e)
        {
            OpenVendorDialog();
        }

        private void UltraTextEditorVendor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11 || e.KeyCode == Keys.Enter)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
        }

        private void OpenVendorDialog()
        {
            try
            {
                using (frmVendorDig vendorDialog = new frmVendorDig())
                {
                    if (vendorDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        if (vendorDialog.SelectedVendorId > 0 || !string.IsNullOrWhiteSpace(vendorDialog.SelectedVendorName))
                        {
                            ultraTextEditorVendor.Text = vendorDialog.SelectedVendorName ?? string.Empty;
                            ultraTextEditorVendor.Value = vendorDialog.SelectedVendorName ?? string.Empty;
                            LoadPurchaseDataWithFilters();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening vendor lookup: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDateControlVisibility()
        {
            bool isAll = string.Equals(Convert.ToString(ultraComboPresetDates.Value ?? ultraComboPresetDates.Text), "ALL", StringComparison.OrdinalIgnoreCase);
            ultraLabelFromDate.Visible = !isAll;
            ultraDateTimeEditorFrom.Visible = !isAll;
            ultraLabelToDate.Visible = !isAll;
            ultraDateTimeEditorTo.Visible = !isAll;
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            if (btnToggleSelection != null)
            {
                btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
            }
        }

        /// <summary>
        /// Initialize tooltips for better user experience
        /// </summary>
        private void InitializeTooltips()
        {
            try
            {
                System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
                toolTip.SetToolTip(ultraDateTimeEditorFrom, "Select start date for the report");
                toolTip.SetToolTip(ultraDateTimeEditorTo, "Select end date for the report");
                toolTip.SetToolTip(ultraComboPresetDates, "Quick date range selection");
                toolTip.SetToolTip(ultraNumericEditorPurchaseNo, "Enter specific purchase number to search");
                toolTip.SetToolTip(ultraTextEditorVendor, "Enter vendor name (partial match supported) or press F11 / click search icon");
                toolTip.SetToolTip(pbVendorSearch, "Click to search and select a vendor (F11)");
                toolTip.SetToolTip(btnSearch, "View Grid (F5)");
                toolTip.SetToolTip(btnClearFilters, "Reset all search filters (F6)");
                toolTip.SetToolTip(btnRefresh, "Preview Grid");
                toolTip.SetToolTip(btnExport, "Export Grid to Excel (Ctrl+E)");
                toolTip.SetToolTip(btnPrint, "Preview Report / Print (Ctrl+P)");
                toolTip.SetToolTip(btnToggleSelection, "Hide / View Filter Selection Panel");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up tooltips: {ex.Message}");
            }
        }

        /// <summary>
        /// Style buttons like Vendor Outstanding / IRS POS action toolbar
        /// </summary>
        private void StyleButtons()
        {
            try
            {
                btnSearch.Text = "View Grid";
                btnRefresh.Text = "Preview Grid";
                btnPrint.Text = "Preview Report";
                btnExport.Text = "Export Grid";
                btnClearFilters.Text = "Reset Filters";
                btnToggleSelection.Text = "Hide Selection";

                StyleClassicButton(btnSearch);
                StyleClassicButton(btnRefresh);
                StyleClassicButton(btnPrint);
                StyleClassicButton(btnExport);
                StyleClassicButton(btnClearFilters);
                StyleClassicButton(btnToggleSelection);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error styling buttons: {ex.Message}");
            }
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton button)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
            button.Appearance.FontData.SizeInPoints = 9;
            button.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button.HotTrackAppearance.BackColor = Color.FromArgb(241, 247, 254);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = ButtonLightOutline;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;
            button.PressedAppearance.BackColor = Color.FromArgb(118, 161, 214);
            button.PressedAppearance.BackColor2 = Color.FromArgb(217, 231, 247);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(148, 163, 182);
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        /// <summary>
        /// Setup keyboard shortcuts
        /// </summary>
        private void SetupKeyboardShortcuts()
        {
            try
            {
                // Set up keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += Form_KeyDown;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up keyboard shortcuts: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle keyboard shortcuts
        /// </summary>
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control && e.KeyCode == Keys.E)
                {
                    btnExport_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.P)
                {
                    btnPrint_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F5)
                {
                    if (e.Control)
                        btnRefresh_Click(sender, e);
                    else
                        btnSearch_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F6)
                {
                    btnClearFilters_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling keyboard shortcut: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize hierarchical DataSet with master-detail relationship
        /// </summary>
        private void InitializeHierarchicalDataSet()
        {
            dsHierarchical = new DataSet("PurchaseReport");

            // Create Master Table (Purchase Bills)
            DataTable masterTable = new DataTable("PurchaseMaster");
            masterTable.Columns.Add("PurchaseNo", typeof(int));
            masterTable.Columns.Add("PurchaseDate", typeof(DateTime));
            masterTable.Columns.Add("InvoiceNo", typeof(string));
            masterTable.Columns.Add("InvoiceDate", typeof(DateTime));
            masterTable.Columns.Add("VendorName", typeof(string));
            masterTable.Columns.Add("Paymode", typeof(string));
            masterTable.Columns.Add("SubTotal", typeof(decimal));
            masterTable.Columns.Add("GrandTotal", typeof(decimal));
            masterTable.Columns.Add("PayedAmount", typeof(decimal));
            masterTable.Columns.Add("BilledBy", typeof(string));
            masterTable.PrimaryKey = new[] { masterTable.Columns["PurchaseNo"] };

            // Create Detail Table (Purchase Items)
            DataTable detailTable = new DataTable("PurchaseDetail");
            detailTable.Columns.Add("DetailID", typeof(int)); // Auto-increment unique ID
            detailTable.Columns.Add("PurchaseNo", typeof(int));
            detailTable.Columns.Add("SlNo", typeof(int));
            detailTable.Columns.Add("ItemName", typeof(string));
            detailTable.Columns.Add("BarCode", typeof(string));
            detailTable.Columns.Add("Unit", typeof(string));
            detailTable.Columns.Add("Packing", typeof(string));
            detailTable.Columns.Add("Qty", typeof(decimal));
            detailTable.Columns.Add("Cost", typeof(decimal));
            detailTable.Columns.Add("Amount", typeof(decimal));
            detailTable.Columns.Add("Free", typeof(decimal));

            // Set auto-increment for DetailID
            detailTable.Columns["DetailID"].AutoIncrement = true;
            detailTable.Columns["DetailID"].AutoIncrementSeed = 1;
            detailTable.Columns["DetailID"].AutoIncrementStep = 1;

            // Add tables to DataSet
            dsHierarchical.Tables.Add(masterTable);
            dsHierarchical.Tables.Add(detailTable);

            // Create relationship between master and detail
            DataRelation relation = new DataRelation(
                "MasterDetail",
                masterTable.Columns["PurchaseNo"],
                detailTable.Columns["PurchaseNo"],
                true
            );
            dsHierarchical.Relations.Add(relation);
        }

        /// <summary>
        /// Setup Hierarchical Grid for Master-Detail view
        /// </summary>
        private void SetupHierarchicalGrid()
        {
            try
            {
                // Reset grid layout
                ultraGridMaster.DisplayLayout.Reset();

                // Basic properties
                ultraGridMaster.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGridMaster.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGridMaster.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;

                // CRITICAL: Set ViewStyleBand to Vertical for hierarchical display
                ultraGridMaster.DisplayLayout.ViewStyleBand = ViewStyleBand.Vertical;

                // Enable row expansion
                ultraGridMaster.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
                ultraGridMaster.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGridMaster.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
                ultraGridMaster.DisplayLayout.Override.RowSelectorWidth = 40;
                ultraGridMaster.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGridMaster.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Style row selectors - Modern look
                ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
                ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

                // Allow row selection by clicking
                ultraGridMaster.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                // Column interactions
                ultraGridMaster.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGridMaster.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;

                // Appearance
                ultraGridMaster.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
                ultraGridMaster.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.GroupByBox.Hidden = true;

                // Row height
                ultraGridMaster.DisplayLayout.Override.MinRowHeight = 25;
                ultraGridMaster.DisplayLayout.Override.DefaultRowHeight = 25;

                // Modern selection colors matching Vendor Outstanding theme
                ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
                ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Modern header styling - Blue gradient matching Vendor Outstanding
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.5F;

                // Modern alternating row colors
                ultraGridMaster.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGridMaster.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
                ultraGridMaster.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
                ultraGridMaster.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;

                // Modern hover effects - Light blue
                ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(215, 235, 255);
                ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
                ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.BorderColor = BorderBlue;

                // Event handlers
                ultraGridMaster.InitializeLayout += UltraGridMaster_InitializeLayout;
                ultraGridMaster.AfterRowExpanded += UltraGridMaster_AfterRowExpanded;
                ultraGridMaster.BeforeRowExpanded += UltraGridMaster_BeforeRowExpanded;
                ultraGridMaster.InitializeRow += UltraGridMaster_InitializeRow;
                ultraGridMaster.AfterColPosChanged += UltraGridMaster_AfterColPosChanged;
                ultraGridMaster.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
                ultraGridMaster.AfterRowFilterChanged += UltraGridMaster_AfterRowFilterChanged;
                ultraGridMaster.AfterSortChange += UltraGridMaster_AfterSortChange;
                ultraGridMaster.Resize += UltraGridMaster_Resize;

                System.Diagnostics.Debug.WriteLine("Hierarchical grid setup completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up hierarchical grid: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePanels()
        {
            this.BackColor = FormBackColor;

            // Setup control panel (Vendor & Date Filters - Topmost)
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelControls.Height = 78;

            // Setup action toolbar panel (IRS POS Style - Docks directly below ultraPanelControls)
            ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
            ultraPanelAction.Appearance.BorderColor = BorderBlue;
            ultraPanelAction.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            ultraPanelAction.Dock = DockStyle.Top;
            ultraPanelAction.Height = 45;

            // Setup master panel (contains hierarchical grid and calculation footer)
            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            ultraPanelMaster.Dock = DockStyle.Fill;

            // Setup grid calculation footer panel
            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
                ultraPanelGridFooter.Appearance.BorderColor = BorderBlue;
                ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelGridFooter.Dock = DockStyle.Bottom;
                ultraPanelGridFooter.Height = 26;
            }

            ultraGridMaster.Dock = DockStyle.Fill;

            // Enforce proper z-order for docking calculations:
            // Controls (Topmost Y=0) -> Action toolbar (Below Controls Y=78) -> Master (Fill remaining space)
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.SendToBack();
            }
            ultraGridMaster.BringToFront();
        }
        #endregion

        #region UltraGrid Layout Configuration
        /// <summary>
        /// Configure Hierarchical Grid layout
        /// </summary>
        private void UltraGridMaster_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Configure Master Band (Band 0)
                if (e.Layout.Bands.Count > 0)
                {
                    ConfigureMasterBandColumns(e.Layout.Bands[0]);
                }

                // Configure Detail Band (Band 1)
                if (e.Layout.Bands.Count > 1)
                {
                    ConfigureDetailBandColumns(e.Layout.Bands[1]);

                    // Set detail band caption
                    e.Layout.Bands[1].Header.Caption = "Purchase Details";
                    e.Layout.Bands[1].HeaderVisible = true;
                }

                // Enable AutoFit for all columns
                e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring grid layout: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Configure columns for Master Band (Purchase Bills)
        /// </summary>
        private void ConfigureMasterBandColumns(UltraGridBand masterBand)
        {
            // PurchaseNo column
            if (masterBand.Columns["PurchaseNo"] != null)
            {
                masterBand.Columns["PurchaseNo"].Header.Caption = "Purchase No";
                masterBand.Columns["PurchaseNo"].Width = 100;
                masterBand.Columns["PurchaseNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["PurchaseNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
            }

            // Format date columns
            if (masterBand.Columns["PurchaseDate"] != null)
            {
                masterBand.Columns["PurchaseDate"].Format = "dd/MM/yyyy";
                masterBand.Columns["PurchaseDate"].Header.Caption = "Purchase Date";
                masterBand.Columns["PurchaseDate"].Width = 100;
            }

            if (masterBand.Columns["InvoiceNo"] != null)
            {
                masterBand.Columns["InvoiceNo"].Header.Caption = "Invoice No";
                masterBand.Columns["InvoiceNo"].Width = 100;
            }

            if (masterBand.Columns["InvoiceDate"] != null)
            {
                masterBand.Columns["InvoiceDate"].Format = "dd/MM/yyyy";
                masterBand.Columns["InvoiceDate"].Header.Caption = "Invoice Date";
                masterBand.Columns["InvoiceDate"].Width = 100;
            }

            if (masterBand.Columns["VendorName"] != null)
            {
                masterBand.Columns["VendorName"].Header.Caption = "Vendor";
                masterBand.Columns["VendorName"].Width = 200;
            }

            if (masterBand.Columns["Paymode"] != null)
            {
                masterBand.Columns["Paymode"].Header.Caption = "Payment Mode";
                masterBand.Columns["Paymode"].Width = 120;
                masterBand.Columns["Paymode"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (masterBand.Columns["BilledBy"] != null)
            {
                masterBand.Columns["BilledBy"].Header.Caption = "Billed By";
                masterBand.Columns["BilledBy"].Width = 120;
            }

            // Format currency columns with modern styling
            if (masterBand.Columns["SubTotal"] != null)
            {
                masterBand.Columns["SubTotal"].Format = "₹ #,##0.00";
                masterBand.Columns["SubTotal"].Header.Caption = "Sub Total";
                masterBand.Columns["SubTotal"].Width = 110;
                masterBand.Columns["SubTotal"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            if (masterBand.Columns["GrandTotal"] != null)
            {
                masterBand.Columns["GrandTotal"].Format = "₹ #,##0.00";
                masterBand.Columns["GrandTotal"].Header.Caption = "Grand Total";
                masterBand.Columns["GrandTotal"].Width = 120;
                masterBand.Columns["GrandTotal"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                masterBand.Columns["GrandTotal"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["GrandTotal"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            if (masterBand.Columns["PayedAmount"] != null)
            {
                masterBand.Columns["PayedAmount"].Format = "₹ #,##0.00";
                masterBand.Columns["PayedAmount"].Header.Caption = "Payed Amount";
                masterBand.Columns["PayedAmount"].Width = 120;
                masterBand.Columns["PayedAmount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                masterBand.Columns["PayedAmount"].CellAppearance.ForeColor = Color.FromArgb(211, 84, 0);
            }
        }

        /// <summary>
        /// Configure columns for Detail Band (Purchase Items)
        /// </summary>
        private void ConfigureDetailBandColumns(UltraGridBand detailBand)
        {
            // Hide DetailID (auto-increment) and PurchaseNo (foreign key)
            if (detailBand.Columns["DetailID"] != null)
            {
                detailBand.Columns["DetailID"].Hidden = true;
                detailBand.Columns["DetailID"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }
            if (detailBand.Columns["PurchaseNo"] != null)
            {
                detailBand.Columns["PurchaseNo"].Hidden = true;
                detailBand.Columns["PurchaseNo"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            // Set column captions and widths with modern formatting
            if (detailBand.Columns["SlNo"] != null)
            {
                detailBand.Columns["SlNo"].Header.Caption = "S.No";
                detailBand.Columns["SlNo"].Width = 50;
                detailBand.Columns["SlNo"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (detailBand.Columns["ItemName"] != null)
            {
                detailBand.Columns["ItemName"].Header.Caption = "Item Name";
                detailBand.Columns["ItemName"].Width = 220;
                detailBand.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (detailBand.Columns["BarCode"] != null)
            {
                detailBand.Columns["BarCode"].Header.Caption = "Barcode";
                detailBand.Columns["BarCode"].Width = 120;
                detailBand.Columns["BarCode"].CellAppearance.ForeColor = Color.FromArgb(84, 110, 122);
            }

            if (detailBand.Columns["Unit"] != null)
            {
                detailBand.Columns["Unit"].Header.Caption = "Unit";
                detailBand.Columns["Unit"].Width = 60;
                detailBand.Columns["Unit"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (detailBand.Columns["Packing"] != null)
            {
                detailBand.Columns["Packing"].Header.Caption = "Packing";
                detailBand.Columns["Packing"].Width = 80;
            }

            if (detailBand.Columns["Qty"] != null)
            {
                detailBand.Columns["Qty"].Header.Caption = "Quantity";
                detailBand.Columns["Qty"].Format = "0.00";
                detailBand.Columns["Qty"].Width = 80;
                detailBand.Columns["Qty"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (detailBand.Columns["Cost"] != null)
            {
                detailBand.Columns["Cost"].Header.Caption = "Cost";
                detailBand.Columns["Cost"].Format = "₹ #,##0.00";
                detailBand.Columns["Cost"].Width = 90;
                detailBand.Columns["Cost"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            if (detailBand.Columns["Amount"] != null)
            {
                detailBand.Columns["Amount"].Header.Caption = "Amount";
                detailBand.Columns["Amount"].Format = "₹ #,##0.00";
                detailBand.Columns["Amount"].Width = 120;
                detailBand.Columns["Amount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            if (detailBand.Columns["Free"] != null)
            {
                detailBand.Columns["Free"].Header.Caption = "Free Qty";
                detailBand.Columns["Free"].Format = "0.00";
                detailBand.Columns["Free"].Width = 80;
                detailBand.Columns["Free"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["Free"].CellAppearance.ForeColor = Color.FromArgb(22, 160, 133);
            }

            // Configure summaries for detail band
            ConfigureDetailBandSummaries(detailBand);

            // Style detail band differently - Modern gradient look
            detailBand.Override.RowAppearance.BackColor = Color.FromArgb(252, 252, 255);
            detailBand.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 248, 252);

            // Detail band header styling - Modern accent color
            detailBand.Override.HeaderAppearance.BackColor = Color.FromArgb(41, 128, 185);
            detailBand.Override.HeaderAppearance.ForeColor = Color.White;
            detailBand.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
        }

        /// <summary>
        /// Configure summaries (totals) for detail band columns
        /// </summary>
        private void ConfigureDetailBandSummaries(UltraGridBand detailBand)
        {
            try
            {
                // Enable summary footer
                detailBand.SummaryFooterCaption = "Detail Totals:";

                // Add summaries for key columns
                if (detailBand.Columns["Qty"] != null)
                {
                    SummarySettings sumQty = detailBand.Summaries.Add("SumQty", SummaryType.Sum, detailBand.Columns["Qty"], SummaryPosition.UseSummaryPositionColumn);
                    sumQty.DisplayFormat = "Qty: {0:N2}";
                    sumQty.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumQty.Appearance.ForeColor = Color.FromArgb(44, 62, 80);
                    sumQty.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (detailBand.Columns["Amount"] != null)
                {
                    SummarySettings sumAmount = detailBand.Summaries.Add("SumAmount", SummaryType.Sum, detailBand.Columns["Amount"], SummaryPosition.UseSummaryPositionColumn);
                    sumAmount.DisplayFormat = "₹ {0:N2}";
                    sumAmount.Appearance.BackColor = Color.FromArgb(52, 73, 94);
                    sumAmount.Appearance.ForeColor = Color.White;
                    sumAmount.Appearance.FontData.Bold = DefaultableBoolean.True;
                    sumAmount.Appearance.FontData.SizeInPoints = 10;
                }

                if (detailBand.Columns["Free"] != null)
                {
                    SummarySettings sumFree = detailBand.Summaries.Add("SumFree", SummaryType.Sum, detailBand.Columns["Free"], SummaryPosition.UseSummaryPositionColumn);
                    sumFree.DisplayFormat = "Free: {0:N2}";
                    sumFree.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumFree.Appearance.ForeColor = Color.FromArgb(22, 160, 133);
                    sumFree.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                // Style the summary footer
                detailBand.Override.SummaryFooterAppearance.BackColor = Color.FromArgb(236, 240, 241);
                detailBand.Override.SummaryFooterAppearance.ForeColor = Color.FromArgb(44, 62, 80);
                detailBand.Override.SummaryFooterAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Override.SummaryFooterAppearance.BorderColor = Color.FromArgb(52, 152, 219);

                System.Diagnostics.Debug.WriteLine($"Configured {detailBand.Summaries.Count} summaries for detail band");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring summaries: {ex.Message}");
            }
        }
        #endregion

        #region Helper: Column Customization Eligibility Check
        /// <summary>
        /// Determines if a grid column is eligible for Column Chooser, hiding, or drag-and-drop.
        /// Prevents chaptered relation columns, internal IDs, and essential columns from throwing exceptions.
        /// </summary>
        private bool IsCustomizableColumn(UltraGridColumn col, int bandIndex)
        {
            if (col == null) return false;
            if (col.IsChaptered) return false;
            if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            if (string.Equals(col.Key, "MasterDetail", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "SlNo", StringComparison.OrdinalIgnoreCase)) return false;
            if (bandIndex > 0 && string.Equals(col.Key, "PurchaseNo", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
        #endregion

        #region UltraPanelGridFooter Dynamic Alignment & Calculation
        private void InitializeGridFooter()
        {
            if (ultraPanelGridFooter == null) return;
            ultraPanelGridFooter.ClientArea.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var ctrl = ultraPanelGridFooter.ClientArea.GetChildAtPoint(e.Location);
                    if (ctrl == null || !(ctrl is Label))
                    {
                        ContextMenuStrip panelMenu = CreateGridOrFooterContextMenu(null);
                        panelMenu.Show(ultraPanelGridFooter.ClientArea, e.Location);
                    }
                }
            };
        }

        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null) return;
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            footerLabels.Clear();

            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            int xOffset = ultraGridMaster.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label();
                footerLabel.Name = "footer_" + column.Key;
                footerLabel.Text = string.Empty;
                footerLabel.TextAlign = ContentAlignment.MiddleCenter;
                footerLabel.BackColor = GridHeaderBlue;
                footerLabel.BorderStyle = BorderStyle.None;
                footerLabel.AutoSize = false;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Left = xOffset;
                footerLabel.Top = 1;
                footerLabel.Tag = Tuple.Create(column.Key, string.Empty);
                footerLabel.ForeColor = Color.White;
                footerLabel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
                footerLabel.Paint += FooterLabel_Paint;
                footerLabel.ContextMenuStrip = CreateFooterContextMenu(column.Key);

                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                footerLabels[column.Key] = footerLabel;

                if (!columnAggregations.ContainsKey(column.Key))
                {
                    if (column.Key.Equals("SubTotal", StringComparison.OrdinalIgnoreCase) ||
                        column.Key.Equals("GrandTotal", StringComparison.OrdinalIgnoreCase) ||
                        column.Key.Equals("PayedAmount", StringComparison.OrdinalIgnoreCase))
                    {
                        columnAggregations[column.Key] = "Sum";
                    }
                    else
                    {
                        columnAggregations[column.Key] = "None";
                    }
                }

                xOffset += column.Width;
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            bool isNumeric = ultraGridMaster.DisplayLayout.Bands.Count > 0 &&
                             ultraGridMaster.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(ultraGridMaster.DisplayLayout.Bands[0].Columns[columnKey]);

            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum");
            itemSum.Tag = "Sum";
            itemSum.Enabled = isNumeric;
            itemSum.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min");
            itemMin.Tag = "Min";
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max");
            itemMax.Tag = "Max";
            itemMax.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count");
            itemCount.Tag = "Count";
            itemCount.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average");
            itemAverage.Tag = "Avg";
            itemAverage.Enabled = isNumeric;
            itemAverage.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None");
            itemNone.Tag = "None";
            itemNone.Click += FooterContextMenu_Click;

            menu.Items.Add(itemSum);
            menu.Items.Add(itemMin);
            menu.Items.Add(itemMax);
            menu.Items.Add(itemCount);
            menu.Items.Add(itemAverage);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemNone);

            menu.Opening += (sender, e) =>
            {
                string currentAggregation = columnAggregations.ContainsKey(columnKey)
                    ? columnAggregations[columnKey]
                    : "None";

                foreach (ToolStripItem menuItem in menu.Items)
                {
                    ToolStripMenuItem toolStripMenuItem = menuItem as ToolStripMenuItem;
                    if (toolStripMenuItem != null && toolStripMenuItem.Tag != null)
                    {
                        toolStripMenuItem.Checked = string.Equals(toolStripMenuItem.Tag.ToString(), currentAggregation, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

            return menu;
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null)
                return;

            ContextMenuStrip menu = item.Owner as ContextMenuStrip;
            if (menu == null || menu.Tag == null || item.Tag == null)
                return;

            string columnKey = menu.Tag.ToString();
            string aggregation = item.Tag.ToString();

            columnAggregations[columnKey] = aggregation;
            UpdateFooterValues();
        }

        private void UpdateFooterValues()
        {
            if (footerLabels.Count == 0)
                return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> footerEntry in footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (!columnAggregations.ContainsKey(columnKey) ||
                    string.Equals(columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result = CalculateAggregation(columnKey, columnAggregations[columnKey], visibleRows);
                string displayValue = FormatAggregationResult(columnKey, columnAggregations[columnKey], result);

                footerLabel.Text = displayValue;
                footerLabel.Tag = Tuple.Create(columnKey, displayValue);
                footerLabel.ForeColor = Color.White;
                footerLabel.Invalidate();
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return aggregation == "Count" ? (object)0 : null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Sum(value => value.Value);
                case "Min":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderBy(value => value)
                        .FirstOrDefault();
                case "Max":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderByDescending(value => value)
                        .FirstOrDefault();
                case "Count":
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                    List<decimal> values = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Select(value => value.Value)
                        .ToList();
                    return values.Count == 0 ? 0m : values.Average();
                default:
                    return null;
            }
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null)
                return string.Empty;

            if (aggregation == "Count")
                return Convert.ToString(result);

            if (ultraGridMaster.DisplayLayout != null &&
                ultraGridMaster.DisplayLayout.Bands.Count > 0 &&
                ultraGridMaster.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = ultraGridMaster.DisplayLayout.Bands[0].Columns[columnKey];
                decimal? numericValue = GetNumericValue(result);
                if (numericValue.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(column.Format))
                        return numericValue.Value.ToString(column.Format);

                    return numericValue.Value.ToString("N2");
                }
            }

            return Convert.ToString(result);
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0 || footerLabels.Count == 0 || ultraPanelGridFooter == null)
                return;

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            int rowSelectorWidth = ultraGridMaster.DisplayLayout.Override.RowSelectorWidth;
            int scrollOffset = 0;
            if (ultraGridMaster.ActiveColScrollRegion != null)
            {
                scrollOffset = ultraGridMaster.ActiveColScrollRegion.Position;
            }

            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = footerLabels[column.Key];
                var headerUI = column.Header.GetUIElement();
                int left, width;

                if (headerUI != null)
                {
                    left = headerUI.Rect.Left;
                    width = headerUI.Rect.Width;
                }
                else
                {
                    left = calculatedX;
                    width = column.Width;
                }

                calculatedX += column.Width;

                footerLabel.Left = left;
                footerLabel.Width = width;
                footerLabel.Top = 0;
                footerLabel.Height = ultraPanelGridFooter.Height;
                footerLabel.Visible = (left + width > 0 && left < ultraPanelGridFooter.Width);
                footerLabel.Invalidate();
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            foreach (UltraGridRow row in ultraGridMaster.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null &&
                   value != DBNull.Value &&
                   !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            decimal result;
            return decimal.TryParse(Convert.ToString(value), out result) ? result : (decimal?)null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null)
                return false;

            Type type = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return type == typeof(decimal) ||
                   type == typeof(double) ||
                   type == typeof(float) ||
                   type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(short) ||
                   type == typeof(byte);
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label footerLabel = sender as Label;
            if (footerLabel == null)
                return;

            Tuple<string, string> tagData = footerLabel.Tag as Tuple<string, string>;
            string columnKey = tagData != null ? tagData.Item1 : string.Empty;
            string displayText = tagData != null ? tagData.Item2 : footerLabel.Text;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fill background matching header blue
            Rectangle rect = new Rectangle(0, 0, footerLabel.Width, footerLabel.Height);
            using (SolidBrush bgBrush = new SolidBrush(GridHeaderBlue))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // Draw right and top border grid lines
            using (Pen borderPen = new Pen(Color.FromArgb(118, 154, 198), 1))
            {
                g.DrawLine(borderPen, footerLabel.Width - 1, 0, footerLabel.Width - 1, footerLabel.Height);
                g.DrawLine(borderPen, 0, 0, footerLabel.Width, 0);
            }

            if (!string.IsNullOrWhiteSpace(displayText))
            {
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    g.DrawString(displayText, footerLabel.Font, textBrush, rect, sf);
                }
            }
        }
        #endregion

        #region Grid Column Chooser, Drag-Drop & Context Menu
        private void InitializeGridContextMenuAndDragDrop()
        {
            ultraGridMaster.AllowDrop = true;
            ultraGridMaster.MouseDown += GridMaster_MouseDown;
            ultraGridMaster.MouseMove += GridMaster_MouseMove;
            ultraGridMaster.MouseUp += GridMaster_MouseUp;
            ultraGridMaster.DragOver += GridMaster_DragOver;
            ultraGridMaster.DragDrop += GridMaster_DragDrop;
        }

        private void UltraGridMaster_AfterColPosChanged(object sender, AfterColPosChangedEventArgs e)
        {
            UpdateFooterCellPositions();
            RefreshColumnChooserList();
        }

        private void UltraGridMaster_AfterRowFilterChanged(object sender, AfterRowFilterChangedEventArgs e)
        {
            UpdateFooterValues();
            UpdateFooterCellPositions();
        }

        private void UltraGridMaster_AfterSortChange(object sender, BandEventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private void UltraGridMaster_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private ContextMenuStrip CreateGridOrFooterContextMenu(UltraGridColumn clickedColumn)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            if (clickedColumn != null)
            {
                int bIdx = clickedColumn.Band != null ? clickedColumn.Band.Index : 0;
                if (IsCustomizableColumn(clickedColumn, bIdx))
                {
                    ToolStripMenuItem hideItem = new ToolStripMenuItem($"Hide '{clickedColumn.Header.Caption}'");
                    hideItem.Click += (s, ev) => HideGridColumn(clickedColumn);
                    menu.Items.Add(hideItem);
                    menu.Items.Add(new ToolStripSeparator());
                }
            }

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("Field / Column Chooser...");
            chooserItem.Click += (s, ev) => ShowColumnChooserDialog();
            menu.Items.Add(chooserItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem calcFooterItem = new ToolStripMenuItem("Calculation Footer");
            calcFooterItem.Checked = (ultraPanelGridFooter != null && ultraPanelGridFooter.Visible);
            calcFooterItem.Click += (s, ev) =>
            {
                if (ultraPanelGridFooter != null)
                {
                    ultraPanelGridFooter.Visible = !ultraPanelGridFooter.Visible;
                    UpdateFooterCellPositions();
                }
            };
            menu.Items.Add(calcFooterItem);

            return menu;
        }

        private void GridMaster_MouseDown(object sender, MouseEventArgs e)
        {
            if (ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

            UIElement elem = ultraGridMaster.DisplayLayout.UIElement?.ElementFromPoint(e.Location);
            HeaderUIElement headerElem = elem as HeaderUIElement ?? elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (e.Button == MouseButtons.Right)
            {
                UltraGridColumn clickedCol = headerElem?.Header?.Column;
                ContextMenuStrip menu = CreateGridOrFooterContextMenu(clickedCol);
                menu.Show(ultraGridMaster, e.Location);
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (headerElem?.Header?.Column != null)
                {
                    UltraGridColumn col = headerElem.Header.Column;
                    int bIdx = col.Band != null ? col.Band.Index : 0;
                    if (IsCustomizableColumn(col, bIdx))
                    {
                        headerDragStartPoint = e.Location;
                        columnToHideByDrag = col;
                        isDraggingHeaderColumn = false;
                    }
                }
            }
        }

        private void GridMaster_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnToHideByDrag != null)
            {
                int dx = Math.Abs(e.X - headerDragStartPoint.X);
                int dy = Math.Abs(e.Y - headerDragStartPoint.Y);

                if (!isDraggingHeaderColumn && (dx > 8 || dy > 8))
                {
                    isDraggingHeaderColumn = true;
                }

                if (isDraggingHeaderColumn)
                {
                    bool isDraggingDown = e.Y - headerDragStartPoint.Y > 30;
                    bool isOutside = !ultraGridMaster.ClientRectangle.Contains(e.Location);

                    if (isDraggingDown || isOutside)
                    {
                        ultraGridMaster.Cursor = Cursors.No;
                        gridToolTip.SetToolTip(ultraGridMaster, $"Drag down/out to hide '{columnToHideByDrag.Header.Caption}'");
                    }
                    else
                    {
                        ultraGridMaster.Cursor = Cursors.Default;
                        gridToolTip.SetToolTip(ultraGridMaster, string.Empty);
                    }
                }
            }
        }

        private void GridMaster_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderColumn && columnToHideByDrag != null)
            {
                bool isDraggingDown = e.Y - headerDragStartPoint.Y > 40;
                bool isOutside = !ultraGridMaster.ClientRectangle.Contains(e.Location);

                if (isDraggingDown || isOutside)
                {
                    HideGridColumn(columnToHideByDrag);
                }
            }

            columnToHideByDrag = null;
            isDraggingHeaderColumn = false;
            ultraGridMaster.Cursor = Cursors.Default;
            gridToolTip.SetToolTip(ultraGridMaster, string.Empty);
        }

        private void GridMaster_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void GridMaster_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                if (item != null && ultraGridMaster.DisplayLayout.Bands.Count > item.BandIndex)
                {
                    var band = ultraGridMaster.DisplayLayout.Bands[item.BandIndex];
                    if (band.Columns.Exists(item.ColumnKey))
                    {
                        var col = band.Columns[item.ColumnKey];
                        if (IsCustomizableColumn(col, item.BandIndex))
                        {
                            try
                            {
                                col.Hidden = false;
                                string key = $"{item.BandIndex}_{col.Key}";
                                if (savedColumnWidths.ContainsKey(key))
                                {
                                    col.Width = savedColumnWidths[key];
                                }
                                CreateFooterCells();
                                UpdateFooterCellPositions();
                                UpdateFooterValues();
                                RefreshColumnChooserList();
                                gridToolTip.Show($"Column '{item.DisplayText}' restored to grid", ultraGridMaster, ultraGridMaster.PointToClient(Cursor.Position), 1500);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error unhiding column on drag-drop: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void HideGridColumn(UltraGridColumn column)
        {
            if (column == null || column.Hidden) return;
            int bandIdx = column.Band != null ? column.Band.Index : 0;
            if (!IsCustomizableColumn(column, bandIdx))
            {
                MessageBox.Show($"The '{column.Header.Caption}' column cannot be hidden.", "Cannot Hide Column", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                savedColumnWidths[$"{bandIdx}_{column.Key}"] = column.Width;
                column.Hidden = true;

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                RefreshColumnChooserList();

                if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    PositionColumnChooserAtBottomRight();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hide column failed: {ex.Message}");
            }
        }

        private void ShowColumnChooserDialog()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show(this);
                PositionColumnChooserAtBottomRight();
                RefreshColumnChooserList();
                return;
            }

            CreateColumnChooserForm();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
            RefreshColumnChooserList();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(240, 340),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(245, 247, 250),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };

            Label lblInfo = new Label
            {
                Text = "Drag columns into grid or double-click to restore:",
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(8, 6, 8, 2),
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 80, 95),
                BackColor = Color.FromArgb(240, 244, 248)
            };

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(245, 247, 250),
                ItemHeight = 32,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(lblInfo);

            this.LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            this.SizeChanged += (s, e) => PositionColumnChooserAtBottomRight();
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count) return;

            ColumnItem item = columnChooserListBox.Items[e.Index] as ColumnItem;
            if (item == null) return;

            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(245, 247, 250)), e.Bounds);

            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -3);

            Color badgeColor = item.BandIndex == 0 ? Color.FromArgb(41, 128, 185) : Color.FromArgb(70, 90, 120);

            using (SolidBrush bgBrush = new SolidBrush(badgeColor))
            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 4;
                int diameter = radius * 2;
                Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
                path.AddArc(arcRect, 180, 90);
                arcRect.X = rect.Right - diameter;
                path.AddArc(arcRect, 270, 90);
                arcRect.Y = rect.Bottom - diameter;
                path.AddArc(arcRect, 0, 90);
                arcRect.X = rect.Left;
                path.AddArc(arcRect, 90, 90);
                path.CloseFigure();
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                e.Graphics.DrawString(item.DisplayText, e.Font, textBrush, rect, sf);
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (columnChooserListBox == null) return;

            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches && index < columnChooserListBox.Items.Count)
            {
                ColumnItem item = columnChooserListBox.Items[index] as ColumnItem;
                if (item != null)
                {
                    columnChooserListBox.SelectedIndex = index;
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox == null) return;

            ColumnItem item = columnChooserListBox.SelectedItem as ColumnItem;
            if (item != null && ultraGridMaster.DisplayLayout.Bands.Count > item.BandIndex)
            {
                var band = ultraGridMaster.DisplayLayout.Bands[item.BandIndex];
                if (band.Columns.Exists(item.ColumnKey))
                {
                    var col = band.Columns[item.ColumnKey];
                    if (IsCustomizableColumn(col, item.BandIndex))
                    {
                        try
                        {
                            col.Hidden = false;
                            string key = $"{item.BandIndex}_{col.Key}";
                            if (savedColumnWidths.ContainsKey(key))
                            {
                                col.Width = savedColumnWidths[key];
                            }
                            CreateFooterCells();
                            UpdateFooterCellPositions();
                            UpdateFooterValues();
                            RefreshColumnChooserList();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error unhiding column on double-click: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                Point screenPoint = this.PointToScreen(Point.Empty);
                columnChooserForm.Location = new Point(
                    screenPoint.X + this.Width - columnChooserForm.Width - 30,
                    screenPoint.Y + this.Height - columnChooserForm.Height - 50);
                columnChooserForm.BringToFront();
            }
        }

        private void RefreshColumnChooserList()
        {
            if (columnChooserListBox == null || ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

            columnChooserListBox.Items.Clear();

            for (int b = 0; b < ultraGridMaster.DisplayLayout.Bands.Count; b++)
            {
                var band = ultraGridMaster.DisplayLayout.Bands[b];
                string bandPrefix = b > 0 ? "[Item] " : "";

                foreach (var col in band.Columns)
                {
                    if (col.Hidden && IsCustomizableColumn(col, b))
                    {
                        string caption = string.IsNullOrEmpty(col.Header.Caption) ? col.Key : col.Header.Caption;
                        columnChooserListBox.Items.Add(new ColumnItem(col.Key, bandPrefix + caption, b));
                    }
                }
            }
        }
        #endregion

        #region Data Loading Methods
        /// <summary>
        /// Load all purchase data without filters
        /// </summary>
        private void LoadPurchaseData()
        {
            try
            {
                isLoading = true;

                DateTime fromDate = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeEditorTo.Value);

                // STEP 1: Unbind grid completely
                ultraGridMaster.DataSource = null;
                ultraGridMaster.DataMember = null;
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // STEP 2: Remove relation
                if (dsHierarchical.Relations.Count > 0)
                {
                    dsHierarchical.Relations.Clear();
                }

                // STEP 3: Clear tables
                dsHierarchical.Tables["PurchaseDetail"].Clear();
                dsHierarchical.Tables["PurchaseMaster"].Clear();

                // STEP 4: Load master data
                LoadMasterData(fromDate, toDate);

                // STEP 5: Load detail data
                LoadAllDetailData(fromDate, toDate);

                // STEP 6: Recreate relation
                DataTable masterTable = dsHierarchical.Tables["PurchaseMaster"];
                DataTable detailTable = dsHierarchical.Tables["PurchaseDetail"];

                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["PurchaseNo"],
                    detailTable.Columns["PurchaseNo"],
                    true
                );
                dsHierarchical.Relations.Add(relation);

                // STEP 7: Accept changes
                dsHierarchical.AcceptChanges();

                // STEP 8: Rebind grid
                ultraGridMaster.DataSource = dsHierarchical;
                ultraGridMaster.DataMember = "PurchaseMaster";

                // STEP 9: Force refresh
                ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // STEP 10: Update totals
                UpdateGrandTotals();

                // STEP 11: Setup and refresh footer cells & column chooser
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                RefreshColumnChooserList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Load master data (Purchase Bills)
        /// </summary>
        private void LoadMasterData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                int branchId = SessionContext.BranchId;
                List<PurchaseReportMaster> masterData = reportRepository.GetPurchaseBills(fromDate, toDate, branchId);
                DataTable masterTable = dsHierarchical.Tables["PurchaseMaster"];

                foreach (var bill in masterData)
                {
                    if (bill.PurchaseNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["PurchaseNo"] = bill.PurchaseNo;
                        row["PurchaseDate"] = bill.PurchaseDate;
                        row["InvoiceNo"] = bill.InvoiceNo ?? "";
                        row["InvoiceDate"] = bill.InvoiceDate;
                        row["VendorName"] = bill.VendorName ?? "";
                        row["Paymode"] = bill.Paymode ?? "";
                        row["SubTotal"] = bill.SubTotal;
                        row["GrandTotal"] = bill.GrandTotal;
                        row["PayedAmount"] = bill.PayedAmount;
                        row["BilledBy"] = bill.BilledBy ?? "";
                        masterTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading master data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load ALL detail data for the date range (optimization)
        /// </summary>
        private void LoadAllDetailData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                DataTable detailTable = dsHierarchical.Tables["PurchaseDetail"];

                // Get all PurchaseNo values from master
                var purchaseNumbers = dsHierarchical.Tables["PurchaseMaster"].AsEnumerable()
                    .Select(r => r.Field<int>("PurchaseNo"))
                    .ToList();

                // Load details for each purchase
                foreach (int purchaseNo in purchaseNumbers)
                {
                    PurchaseReportData reportData = reportRepository.GetPurchaseReportDetails(purchaseNo);

                    if (reportData?.Details != null && reportData.Details.Count > 0)
                    {
                        foreach (var detail in reportData.Details)
                        {
                            DataRow row = detailTable.NewRow();
                            // DetailID will auto-increment
                            row["PurchaseNo"] = purchaseNo;
                            row["SlNo"] = detail.SlNo;
                            row["ItemName"] = detail.ItemName ?? "";
                            row["BarCode"] = detail.BarCode ?? "";
                            row["Unit"] = detail.Unit ?? "";
                            row["Packing"] = detail.Packing ?? "";
                            row["Qty"] = detail.Qty;
                            row["Cost"] = detail.Cost;
                            row["Amount"] = detail.Amount;
                            row["Free"] = detail.Free;
                            detailTable.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading detail data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update grand totals for all loaded data and display in summary panel
        /// </summary>
        private void UpdateGrandTotals()
        {
            try
            {
                DataTable masterTable = dsHierarchical.Tables["PurchaseMaster"];
                decimal grandTotal = 0;
                decimal grandSubTotal = 0;
                decimal grandPayed = 0;
                int totalPurchases = masterTable.Rows.Count;

                foreach (DataRow row in masterTable.Rows)
                {
                    grandSubTotal += Convert.ToDecimal(row["SubTotal"]);
                    grandTotal += Convert.ToDecimal(row["GrandTotal"]);
                    grandPayed += Convert.ToDecimal(row["PayedAmount"]);
                }

                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"Grand Totals Updated - Purchases: {totalPurchases}, SubTotal: {grandSubTotal:N2}, Grand: {grandTotal:N2}, Payed: {grandPayed:N2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating grand totals: {ex.Message}");
            }
        }
        #endregion

        #region Grid Event Handlers
        /// <summary>
        /// Initialize row event - enhance row appearance
        /// </summary>
        private void UltraGridMaster_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                // Only style master band (Band 0)
                if (e.Row.Band.Index == 0)
                {
                    // Enhanced alternating rows with better colors
                    if (e.Row.Index % 2 == 0)
                    {
                        e.Row.Appearance.BackColor = Color.White;
                    }
                    else
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(246, 248, 252);
                    }

                    // Add subtle border to rows for better separation
                    e.Row.Appearance.BorderColor = Color.FromArgb(224, 224, 224);
                }
                else if (e.Row.Band.Index == 1) // Detail band styling
                {
                    // Enhanced alternating rows for detail band
                    if (e.Row.Index % 2 == 0)
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(252, 252, 255);
                    }
                    else
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(245, 247, 252);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeRow: {ex.Message}");
            }
        }

        /// <summary>
        /// Before row expanded event
        /// </summary>
        private void UltraGridMaster_BeforeRowExpanded(object sender, CancelableRowEventArgs e)
        {
            try
            {
                // Details are already loaded, so just update summary
                if (e.Row.Band.Index == 0 && e.Row.Cells["PurchaseNo"] != null)
                {
                    var dataRowView = e.Row.ListObject as DataRowView;
                    if (dataRowView != null)
                    {
                        // Can add additional logic here if needed
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BeforeRowExpanded: {ex.Message}");
            }
        }

        /// <summary>
        /// After row expanded event
        /// </summary>
        private void UltraGridMaster_AfterRowExpanded(object sender, RowEventArgs e)
        {
            try
            {
                if (e.Row.Band.Index == 0 && e.Row.Cells["PurchaseNo"] != null)
                {
                    int purchaseNo = Convert.ToInt32(e.Row.Cells["PurchaseNo"].Value);
                    int detailCount = e.Row.ChildBands[0].Rows.Count;

                    System.Diagnostics.Debug.WriteLine($"Expanded Purchase No: {purchaseNo}, Detail Count: {detailCount}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AfterRowExpanded: {ex.Message}");
            }
        }
        #endregion

        #region Button Event Handlers
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                LoadPurchaseData();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (dsHierarchical.Tables["PurchaseMaster"].Rows.Count > 0)
                {
                    PrintPurchaseReport();
                }
                else
                {
                    MessageBox.Show("No data available to print.", "Print",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Print the purchase report
        /// </summary>
        private void PrintPurchaseReport()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = "Purchase Report";
                printDocument.PrintPage += PrintDocument_PrintPage;

                printDialog.Document = printDocument;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Print document event handler
        /// </summary>
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                Font titleFont = new Font("Arial", 16, FontStyle.Bold);
                Font headerFont = new Font("Arial", 10, FontStyle.Bold);
                Font dataFont = new Font("Arial", 9);
                Font summaryFont = new Font("Arial", 10, FontStyle.Bold);

                float yPosition = 50;
                float leftMargin = 50;
                float rightMargin = e.MarginBounds.Right;

                // Print title
                e.Graphics.DrawString("PURCHASE REPORT - MASTER DETAIL VIEW", titleFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;

                // Print date range
                string dateRange = $"From: {ultraDateTimeEditorFrom.Value:dd/MM/yyyy} To: {ultraDateTimeEditorTo.Value:dd/MM/yyyy}";
                e.Graphics.DrawString(dateRange, dataFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;

                // Print summary
                DataTable masterTable = dsHierarchical.Tables["PurchaseMaster"];
                decimal totalAmount = 0;
                decimal totalPayed = 0;
                int totalPurchases = masterTable.Rows.Count;

                foreach (DataRow row in masterTable.Rows)
                {
                    totalAmount += Convert.ToDecimal(row["GrandTotal"]);
                    totalPayed += Convert.ToDecimal(row["PayedAmount"]);
                }

                e.Graphics.DrawString($"Total Purchases: {totalPurchases}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Grand Total: {totalAmount:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Total Payed: {totalPayed:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;

                // Print master data headers
                string[] headers = { "Purchase No", "Date", "Vendor", "Payment", "Sub Total", "Grand Total", "Payed" };
                float[] columnWidths = { 90, 80, 150, 100, 90, 90, 90 };
                float xPosition = leftMargin;

                for (int i = 0; i < headers.Length; i++)
                {
                    e.Graphics.DrawString(headers[i], headerFont, Brushes.Black, xPosition, yPosition);
                    xPosition += columnWidths[i];
                }
                yPosition += 25;

                // Draw line under headers
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition - 5, rightMargin, yPosition - 5);
                yPosition += 10;

                // Print master data
                foreach (DataRow row in masterTable.Rows)
                {
                    if (yPosition > e.MarginBounds.Bottom - 100)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    xPosition = leftMargin;
                    string[] values = {
                        row["PurchaseNo"].ToString(),
                        Convert.ToDateTime(row["PurchaseDate"]).ToString("dd/MM/yyyy"),
                        row["VendorName"].ToString(),
                        row["Paymode"].ToString(),
                        Convert.ToDecimal(row["SubTotal"]).ToString("N2"),
                        Convert.ToDecimal(row["GrandTotal"]).ToString("N2"),
                        Convert.ToDecimal(row["PayedAmount"]).ToString("N2")
                    };

                    for (int i = 0; i < values.Length; i++)
                    {
                        e.Graphics.DrawString(values[i], dataFont, Brushes.Black, xPosition, yPosition);
                        xPosition += columnWidths[i];
                    }
                    yPosition += 20;
                }

                // Print totals at bottom
                yPosition = e.MarginBounds.Bottom - 50;
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, rightMargin, yPosition);
                yPosition += 10;
                e.Graphics.DrawString($"GRAND TOTAL: {totalAmount:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
            }
            catch (Exception ex)
            {
                e.Graphics.DrawString($"Error printing: {ex.Message}", new Font("Arial", 10), Brushes.Red, 50, 50);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dsHierarchical.Tables["PurchaseMaster"].Rows.Count > 0)
                {
                    ExportToExcel();
                }
                else
                {
                    MessageBox.Show("No data available to export.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            // Validate date range
            if (!ValidateDateRange())
                return;

            // Validate amount range
            if (!ValidateAmountRange())
                return;

            // Show wait cursor during search
            this.Cursor = Cursors.WaitCursor;
            try
            {
                LoadPurchaseDataWithFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Validate date range input
        /// </summary>
        private bool ValidateDateRange()
        {
            try
            {
                DateTime fromDate = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeEditorTo.Value);

                if (fromDate > toDate)
                {
                    MessageBox.Show("From date cannot be greater than To date.", "Invalid Date Range",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraDateTimeEditorFrom.Focus();
                    return false;
                }

                if (fromDate > DateTime.Now)
                {
                    MessageBox.Show("From date cannot be in the future.", "Invalid Date",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraDateTimeEditorFrom.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid date format: {ex.Message}", "Date Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Validate amount range input (Amounts removed from UI)
        /// </summary>
        private bool ValidateAmountRange()
        {
            return true;
        }

        /// <summary>
        /// Ribbon Clear support
        /// </summary>
        public void RibbonClear()
        {
            btnClearFilters_Click(this, EventArgs.Empty);
        }

        public void Clear()
        {
            btnClearFilters_Click(this, EventArgs.Empty);
        }

        public void ClearForm()
        {
            btnClearFilters_Click(this, EventArgs.Empty);
        }

        public void ResetForm()
        {
            btnClearFilters_Click(this, EventArgs.Empty);
        }

        public void ResetFilters()
        {
            btnClearFilters_Click(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clear filters and reload
        /// </summary>
        public void btnClearFilters_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // Clear all search filters
                ultraNumericEditorPurchaseNo.Value = null;

                // Clear vendor text field
                ultraTextEditorVendor.Value = null;
                ultraTextEditorVendor.Text = string.Empty;

                ultraComboPresetDates.Value = "ALL";
                ultraDateTimeEditorFrom.Value = new DateTime(1990, 1, 1);
                ultraDateTimeEditorTo.Value = DateTime.Today;
                UpdateDateControlVisibility();

                // Force UI update
                Application.DoEvents();

                // Reload all data
                LoadPurchaseData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing filters: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ultraComboPresetDates_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (ultraComboPresetDates.Value == null) return;

                string preset = Convert.ToString(ultraComboPresetDates.Value ?? ultraComboPresetDates.Text);

                if (string.Equals(preset, "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    ultraDateTimeEditorFrom.Value = new DateTime(1990, 1, 1);
                    ultraDateTimeEditorTo.Value = DateTime.Today;
                }
                else
                {
                    // Date by Range selected: default to start of current month if it was in 1990
                    DateTime today = DateTime.Today;
                    if (Convert.ToDateTime(ultraDateTimeEditorFrom.Value).Year < 2000)
                    {
                        ultraDateTimeEditorFrom.Value = new DateTime(today.Year, today.Month, 1);
                        ultraDateTimeEditorTo.Value = today;
                    }
                }

                UpdateDateControlVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting date range: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load purchase data with applied filters
        /// </summary>
        private void LoadPurchaseDataWithFilters()
        {
            try
            {
                isLoading = true;
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeEditorTo.Value);

                // CRITICAL: Completely unbind and reset grid
                ultraGridMaster.DataSource = null;
                ultraGridMaster.DataMember = null;
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // Clear the DataSet completely and rebuild from scratch
                RebuildDataSetWithFilters(fromDate, toDate);

                // Rebind the grid with fresh data
                ultraGridMaster.DataSource = dsHierarchical;
                ultraGridMaster.DataMember = "PurchaseMaster";

                // Force complete grid refresh
                ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // Update totals
                UpdateGrandTotals();

                // Setup and refresh footer cells & column chooser
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                RefreshColumnChooserList();

                // Show result count
                int resultCount = dsHierarchical.Tables["PurchaseMaster"].Rows.Count;
                if (resultCount == 0)
                {
                    MessageBox.Show("No records found matching the search criteria.", "Search Results",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching purchase data: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Rebuild the entire DataSet with filters applied
        /// </summary>
        private void RebuildDataSetWithFilters(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Step 1: Remove the relation temporarily
                if (dsHierarchical.Relations.Count > 0)
                {
                    dsHierarchical.Relations.Clear();
                }

                // Step 2: Clear both tables
                dsHierarchical.Tables["PurchaseDetail"].Clear();
                dsHierarchical.Tables["PurchaseMaster"].Clear();

                // Step 3: Load master data from database
                int branchId = SessionContext.BranchId;
                List<PurchaseReportMaster> masterData = reportRepository.GetPurchaseBills(fromDate, toDate, branchId);

                // Step 4: Apply filters to master data BEFORE adding to DataTable
                var filteredMasterData = ApplyMasterFilters(masterData);

                // Step 5: Add filtered master data to DataTable
                DataTable masterTable = dsHierarchical.Tables["PurchaseMaster"];
                foreach (var bill in filteredMasterData)
                {
                    if (bill.PurchaseNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["PurchaseNo"] = bill.PurchaseNo;
                        row["PurchaseDate"] = bill.PurchaseDate;
                        row["InvoiceNo"] = bill.InvoiceNo ?? "";
                        row["InvoiceDate"] = bill.InvoiceDate;
                        row["VendorName"] = bill.VendorName ?? "";
                        row["Paymode"] = bill.Paymode ?? "";
                        row["SubTotal"] = bill.SubTotal;
                        row["GrandTotal"] = bill.GrandTotal;
                        row["PayedAmount"] = bill.PayedAmount;
                        row["BilledBy"] = bill.BilledBy ?? "";
                        masterTable.Rows.Add(row);
                    }
                }

                // Step 6: Load details only for filtered purchases
                if (masterTable.Rows.Count > 0)
                {
                    LoadDetailsForFilteredPurchases();
                }

                // Step 7: Recreate the relation
                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["PurchaseNo"],
                    dsHierarchical.Tables["PurchaseDetail"].Columns["PurchaseNo"],
                    true
                );
                dsHierarchical.Relations.Add(relation);

                // Step 8: Accept all changes
                dsHierarchical.AcceptChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error rebuilding dataset with filters: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Apply filters to master data list BEFORE adding to DataTable
        /// </summary>
        private List<PurchaseReportMaster> ApplyMasterFilters(List<PurchaseReportMaster> masterData)
        {
            var filtered = masterData.AsEnumerable();

            // Filter by Purchase Number
            if (ultraNumericEditorPurchaseNo.Value != null && Convert.ToInt32(ultraNumericEditorPurchaseNo.Value) > 0)
            {
                int purchaseNo = Convert.ToInt32(ultraNumericEditorPurchaseNo.Value);
                filtered = filtered.Where(x => x.PurchaseNo == purchaseNo);
            }

            // Filter by Vendor Name (case-insensitive partial match)
            // Check both Value and Text to handle Infragistics control behavior
            string vendorText = ultraTextEditorVendor.Value?.ToString() ?? ultraTextEditorVendor.Text;
            if (!string.IsNullOrWhiteSpace(vendorText))
            {
                string vendorSearch = vendorText.Trim().ToLower();
                filtered = filtered.Where(x =>
                    !string.IsNullOrEmpty(x.VendorName) &&
                    x.VendorName.ToLower().Contains(vendorSearch));
            }

            // Filter by Amount Range
            if (ultraNumericEditorAmountFrom.Value != null && Convert.ToDecimal(ultraNumericEditorAmountFrom.Value) > 0)
            {
                decimal fromAmount = Convert.ToDecimal(ultraNumericEditorAmountFrom.Value);
                filtered = filtered.Where(x => x.GrandTotal >= (double)fromAmount);
            }

            if (ultraNumericEditorAmountTo.Value != null && Convert.ToDecimal(ultraNumericEditorAmountTo.Value) > 0)
            {
                decimal toAmount = Convert.ToDecimal(ultraNumericEditorAmountTo.Value);
                filtered = filtered.Where(x => x.GrandTotal <= (double)toAmount);
            }

            return filtered.ToList();
        }

        /// <summary>
        /// Load details only for purchases that passed the filter
        /// </summary>
        private void LoadDetailsForFilteredPurchases()
        {
            try
            {
                DataTable masterTable = dsHierarchical.Tables["PurchaseMaster"];
                DataTable detailTable = dsHierarchical.Tables["PurchaseDetail"];

                // Get all PurchaseNo values from filtered master
                var purchaseNumbers = masterTable.AsEnumerable()
                    .Select(r => r.Field<int>("PurchaseNo"))
                    .ToList();

                // Load details for each filtered purchase
                foreach (int purchaseNo in purchaseNumbers)
                {
                    PurchaseReportData reportData = reportRepository.GetPurchaseReportDetails(purchaseNo);

                    if (reportData?.Details != null && reportData.Details.Count > 0)
                    {
                        foreach (var detail in reportData.Details)
                        {
                            DataRow row = detailTable.NewRow();
                            row["PurchaseNo"] = purchaseNo;
                            row["SlNo"] = detail.SlNo;
                            row["ItemName"] = detail.ItemName ?? "";
                            row["BarCode"] = detail.BarCode ?? "";
                            row["Unit"] = detail.Unit ?? "";
                            row["Packing"] = detail.Packing ?? "";
                            row["Qty"] = detail.Qty;
                            row["Cost"] = detail.Cost;
                            row["Amount"] = detail.Amount;
                            row["Free"] = detail.Free;
                            detailTable.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading filtered details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Export data to Excel
        /// </summary>
        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|Excel Files (*.xls)|*.xls";
                saveDialog.FileName = $"PurchaseReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Create a new DataTable for export with flattened data
                    DataTable exportTable = CreateExportTable();

                    // Export to Excel using simple CSV approach
                    ExportToCSV(exportTable, saveDialog.FileName);

                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Create flattened table for export
        /// </summary>
        private DataTable CreateExportTable()
        {
            DataTable exportTable = new DataTable("PurchaseReport");

            // Add columns
            exportTable.Columns.Add("PurchaseNo", typeof(int));
            exportTable.Columns.Add("PurchaseDate", typeof(DateTime));
            exportTable.Columns.Add("InvoiceNo", typeof(string));
            exportTable.Columns.Add("InvoiceDate", typeof(DateTime));
            exportTable.Columns.Add("VendorName", typeof(string));
            exportTable.Columns.Add("Paymode", typeof(string));
            exportTable.Columns.Add("ItemName", typeof(string));
            exportTable.Columns.Add("BarCode", typeof(string));
            exportTable.Columns.Add("Unit", typeof(string));
            exportTable.Columns.Add("Quantity", typeof(decimal));
            exportTable.Columns.Add("Cost", typeof(decimal));
            exportTable.Columns.Add("Amount", typeof(decimal));
            exportTable.Columns.Add("Free", typeof(decimal));
            exportTable.Columns.Add("SubTotal", typeof(decimal));
            exportTable.Columns.Add("GrandTotal", typeof(decimal));
            exportTable.Columns.Add("PayedAmount", typeof(decimal));

            // Flatten the hierarchical data
            foreach (DataRow masterRow in dsHierarchical.Tables["PurchaseMaster"].Rows)
            {
                DataRow[] detailRows = dsHierarchical.Tables["PurchaseDetail"].Select($"PurchaseNo = {masterRow["PurchaseNo"]}");

                if (detailRows.Length > 0)
                {
                    foreach (DataRow detailRow in detailRows)
                    {
                        DataRow exportRow = exportTable.NewRow();
                        exportRow["PurchaseNo"] = masterRow["PurchaseNo"];
                        exportRow["PurchaseDate"] = masterRow["PurchaseDate"];
                        exportRow["InvoiceNo"] = masterRow["InvoiceNo"];
                        exportRow["InvoiceDate"] = masterRow["InvoiceDate"];
                        exportRow["VendorName"] = masterRow["VendorName"];
                        exportRow["Paymode"] = masterRow["Paymode"];
                        exportRow["ItemName"] = detailRow["ItemName"];
                        exportRow["BarCode"] = detailRow["BarCode"];
                        exportRow["Unit"] = detailRow["Unit"];
                        exportRow["Quantity"] = detailRow["Qty"];
                        exportRow["Cost"] = detailRow["Cost"];
                        exportRow["Amount"] = detailRow["Amount"];
                        exportRow["Free"] = detailRow["Free"];
                        exportRow["SubTotal"] = masterRow["SubTotal"];
                        exportRow["GrandTotal"] = masterRow["GrandTotal"];
                        exportRow["PayedAmount"] = masterRow["PayedAmount"];
                        exportTable.Rows.Add(exportRow);
                    }
                }
                else
                {
                    // Add master row even if no details
                    DataRow exportRow = exportTable.NewRow();
                    exportRow["PurchaseNo"] = masterRow["PurchaseNo"];
                    exportRow["PurchaseDate"] = masterRow["PurchaseDate"];
                    exportRow["InvoiceNo"] = masterRow["InvoiceNo"];
                    exportRow["InvoiceDate"] = masterRow["InvoiceDate"];
                    exportRow["VendorName"] = masterRow["VendorName"];
                    exportRow["Paymode"] = masterRow["Paymode"];
                    exportRow["SubTotal"] = masterRow["SubTotal"];
                    exportRow["GrandTotal"] = masterRow["GrandTotal"];
                    exportRow["PayedAmount"] = masterRow["PayedAmount"];
                    exportTable.Rows.Add(exportRow);
                }
            }

            return exportTable;
        }

        /// <summary>
        /// Export DataTable to CSV file
        /// </summary>
        private void ExportToCSV(DataTable dataTable, string fileName)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName))
            {
                // Write headers
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    writer.Write(dataTable.Columns[i].ColumnName);
                    if (i < dataTable.Columns.Count - 1)
                        writer.Write(",");
                }
                writer.WriteLine();

                // Write data
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        string value = row[i].ToString();
                        // Escape commas and quotes
                        if (value.Contains(",") || value.Contains("\""))
                        {
                            value = "\"" + value.Replace("\"", "\"\"") + "\"";
                        }
                        writer.Write(value);
                        if (i < dataTable.Columns.Count - 1)
                            writer.Write(",");
                    }
                    writer.WriteLine();
                }
            }
        }
        #endregion

        #region Form Events
        private void frmPurchaseReportDetails_Load(object sender, EventArgs e)
        {
            // Load data after form is shown with wait cursor
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (!this.TopLevel)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.Dock = DockStyle.Fill;
                }

                // Ensure layout order is strictly enforced: Controls (Topmost) -> Action toolbar -> Master
                ultraPanelControls.SendToBack();
                ultraPanelAction.BringToFront();
                ultraPanelMaster.BringToFront();
                if (ultraPanelGridFooter != null) ultraPanelGridFooter.SendToBack();
                ultraGridMaster.BringToFront();

                Application.DoEvents(); // Allow form to paint first
                LoadPurchaseData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void frmPurchaseReportDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (columnChooserForm != null && !columnChooserForm.IsDisposed)
                {
                    columnChooserForm.Dispose();
                    columnChooserForm = null;
                }

                // Cleanup if needed
                if (dsHierarchical != null)
                {
                    dsHierarchical.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
