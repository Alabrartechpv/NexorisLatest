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
    /// Professional Purchase Return Report Form with Hierarchical Master-Detail UltraGrid
    /// </summary>
    public partial class PurchaseReturnReport : Form
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

        private PurchaseReturnReportRepository reportRepository;
        private DataSet dsHierarchical;
        private bool isLoading = false;

        // Grid calculation footer fields
        private readonly Dictionary<string, Label> footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Column chooser & drag-drop fields
        private Form columnChooserForm;
        private System.Windows.Forms.ToolTip gridToolTip = new System.Windows.Forms.ToolTip();
        private Point headerDragStartPoint;
        private UltraGridColumn columnToHideByDrag;
        private bool isDraggingHeaderColumn;
        private readonly Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        #endregion

        #region Constructor
        public PurchaseReturnReport()
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
                reportRepository = new PurchaseReturnReportRepository();

                // Set form properties
                this.Text = "Purchase Return Report";
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
            ultraNumericEditorReturnNo.FormatString = "0";

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

            // Vendor & Return No (Row 1)
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

            ultraLabelReturnNoSearch.Text = "Doc No";
            ultraLabelReturnNoSearch.Location = new Point(360, 15);
            ultraLabelReturnNoSearch.Size = new Size(60, 20);
            ultraLabelReturnNoSearch.Appearance.ForeColor = ControlTextColor;
            ultraLabelReturnNoSearch.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            ultraNumericEditorReturnNo.Location = new Point(425, 12);
            ultraNumericEditorReturnNo.Size = new Size(110, 24);

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
                            LoadPurchaseReturnDataWithFilters();
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
                toolTip.SetToolTip(ultraNumericEditorReturnNo, "Enter specific return number to search");
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
            dsHierarchical = new DataSet("PurchaseReturnReport");

            // Create Master Table (Purchase Return Records)
            DataTable masterTable = new DataTable("PurchaseReturnMaster");
            masterTable.Columns.Add("PReturnNo", typeof(int));
            masterTable.Columns.Add("PReturnDate", typeof(DateTime));
            masterTable.Columns.Add("InvoiceNo", typeof(string));
            masterTable.Columns.Add("InvoiceDate", typeof(DateTime));
            masterTable.Columns.Add("VendorName", typeof(string));
            masterTable.Columns.Add("Paymode", typeof(string));
            masterTable.Columns.Add("SubTotal", typeof(decimal));
            masterTable.Columns.Add("GrandTotal", typeof(decimal));
            masterTable.PrimaryKey = new[] { masterTable.Columns["PReturnNo"] };

            // Create Detail Table (Return Items)
            DataTable detailTable = new DataTable("PurchaseReturnDetail");
            detailTable.Columns.Add("DetailID", typeof(int)); // Auto-increment unique ID
            detailTable.Columns.Add("PReturnNo", typeof(int));
            detailTable.Columns.Add("SlNo", typeof(int));
            detailTable.Columns.Add("ItemName", typeof(string));
            detailTable.Columns.Add("Unit", typeof(string));
            detailTable.Columns.Add("Packing", typeof(string));
            detailTable.Columns.Add("Qty", typeof(decimal));
            detailTable.Columns.Add("Cost", typeof(decimal));
            detailTable.Columns.Add("TaxPer", typeof(decimal));
            detailTable.Columns.Add("TaxAmt", typeof(decimal));
            detailTable.Columns.Add("Amount", typeof(decimal));
            detailTable.Columns.Add("Reason", typeof(string));

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
                masterTable.Columns["PReturnNo"],
                detailTable.Columns["PReturnNo"],
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
                    e.Layout.Bands[1].Header.Caption = "Return Details";
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
        /// Configure columns for Master Band (Purchase Return Records)
        /// </summary>
        private void ConfigureMasterBandColumns(UltraGridBand masterBand)
        {
            // PReturnNo column
            if (masterBand.Columns["PReturnNo"] != null)
            {
                masterBand.Columns["PReturnNo"].Header.Caption = "Return No";
                masterBand.Columns["PReturnNo"].Width = 100;
                masterBand.Columns["PReturnNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["PReturnNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
            }

            // Format date columns
            if (masterBand.Columns["PReturnDate"] != null)
            {
                masterBand.Columns["PReturnDate"].Format = "dd/MM/yyyy";
                masterBand.Columns["PReturnDate"].Header.Caption = "Return Date";
                masterBand.Columns["PReturnDate"].Width = 100;
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
        }

        /// <summary>
        /// Configure columns for Detail Band (Return Items)
        /// </summary>
        private void ConfigureDetailBandColumns(UltraGridBand detailBand)
        {
            // Hide DetailID (auto-increment) and PReturnNo (foreign key)
            if (detailBand.Columns["DetailID"] != null)
                detailBand.Columns["DetailID"].Hidden = true;
            if (detailBand.Columns["PReturnNo"] != null)
                detailBand.Columns["PReturnNo"].Hidden = true;

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

            if (detailBand.Columns["TaxPer"] != null)
            {
                detailBand.Columns["TaxPer"].Header.Caption = "Tax %";
                detailBand.Columns["TaxPer"].Format = "0.00 %";
                detailBand.Columns["TaxPer"].Width = 70;
                detailBand.Columns["TaxPer"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (detailBand.Columns["TaxAmt"] != null)
            {
                detailBand.Columns["TaxAmt"].Header.Caption = "Tax Amount";
                detailBand.Columns["TaxAmt"].Format = "₹ #,##0.00";
                detailBand.Columns["TaxAmt"].Width = 100;
                detailBand.Columns["TaxAmt"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["TaxAmt"].CellAppearance.ForeColor = Color.FromArgb(211, 84, 0);
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

            if (detailBand.Columns["Reason"] != null)
            {
                detailBand.Columns["Reason"].Header.Caption = "Return Reason";
                detailBand.Columns["Reason"].Width = 200;
                detailBand.Columns["Reason"].CellAppearance.ForeColor = Color.FromArgb(198, 40, 40);
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

                if (detailBand.Columns["TaxAmt"] != null)
                {
                    SummarySettings sumTax = detailBand.Summaries.Add("SumTax", SummaryType.Sum, detailBand.Columns["TaxAmt"], SummaryPosition.UseSummaryPositionColumn);
                    sumTax.DisplayFormat = "₹ {0:N2}";
                    sumTax.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumTax.Appearance.ForeColor = Color.FromArgb(211, 84, 0);
                    sumTax.Appearance.FontData.Bold = DefaultableBoolean.True;
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

                // Style the summary footer
                detailBand.Override.SummaryFooterAppearance.BackColor = Color.FromArgb(236, 240, 241);
                detailBand.Override.SummaryFooterAppearance.ForeColor = Color.FromArgb(44, 62, 80);
                detailBand.Override.SummaryFooterAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Override.SummaryFooterAppearance.BorderColor = Color.FromArgb(52, 152, 219);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring summaries: {ex.Message}");
            }
        }
        #endregion

        #region Data Loading Methods
        /// <summary>
        /// Load all purchase return data without filters
        /// </summary>
        private void LoadPurchaseReturnData()
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
                dsHierarchical.Tables["PurchaseReturnDetail"].Clear();
                dsHierarchical.Tables["PurchaseReturnMaster"].Clear();

                // STEP 4: Load master data
                LoadMasterData(fromDate, toDate);

                // STEP 5: Load detail data
                LoadAllDetailData(fromDate, toDate);

                // STEP 6: Recreate relation
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                DataTable detailTable = dsHierarchical.Tables["PurchaseReturnDetail"];

                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["PReturnNo"],
                    detailTable.Columns["PReturnNo"],
                    true
                );
                dsHierarchical.Relations.Add(relation);

                // STEP 7: Accept changes
                dsHierarchical.AcceptChanges();

                // STEP 8: Rebind grid
                ultraGridMaster.DataSource = dsHierarchical;
                ultraGridMaster.DataMember = "PurchaseReturnMaster";

                // STEP 9: Force refresh
                ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // Update Grid Calculation Footer
                RecalculateAggregations();
                UpdateFooterCellPositions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase return data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Load master data (Purchase Return Records)
        /// </summary>
        private void LoadMasterData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                int branchId = SessionContext.BranchId;
                List<PurchaseReturnReportMaster> masterData = reportRepository.GetPurchaseReturnRecords(fromDate, toDate, branchId);
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];

                foreach (var returnRecord in masterData)
                {
                    if (returnRecord.PReturnNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["PReturnNo"] = returnRecord.PReturnNo;
                        row["PReturnDate"] = returnRecord.PReturnDate;
                        row["InvoiceNo"] = returnRecord.InvoiceNo ?? "";
                        row["InvoiceDate"] = returnRecord.InvoiceDate;
                        row["VendorName"] = returnRecord.VendorName ?? "";
                        row["Paymode"] = returnRecord.Paymode ?? "";
                        row["SubTotal"] = returnRecord.SubTotal;
                        row["GrandTotal"] = returnRecord.GrandTotal;
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
                DataTable detailTable = dsHierarchical.Tables["PurchaseReturnDetail"];

                // Get all PReturnNo values from master
                var returnNumbers = dsHierarchical.Tables["PurchaseReturnMaster"].AsEnumerable()
                    .Select(r => r.Field<int>("PReturnNo"))
                    .ToList();

                // Load details for each return
                foreach (int pReturnNo in returnNumbers)
                {
                    PurchaseReturnReportData reportData = reportRepository.GetPurchaseReturnReportDetails(pReturnNo);

                    if (reportData?.Details != null && reportData.Details.Count > 0)
                    {
                        foreach (var detail in reportData.Details)
                        {
                            DataRow row = detailTable.NewRow();
                            // DetailID will auto-increment
                            row["PReturnNo"] = pReturnNo;
                            row["SlNo"] = detail.SlNo;
                            row["ItemName"] = detail.ItemName ?? "";
                            row["Unit"] = detail.Unit ?? "";
                            row["Packing"] = detail.Packing ?? "";
                            row["Qty"] = detail.Qty;
                            row["Cost"] = detail.Cost;
                            row["TaxPer"] = detail.TaxPer;
                            row["TaxAmt"] = detail.TaxAmt;
                            row["Amount"] = detail.Amount;
                            row["Reason"] = detail.Reason ?? "";
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
        #endregion

        #region Grid Footer Calculation Bar
        private void InitializeGridFooter()
        {
            if (ultraPanelGridFooter == null) return;

            ultraPanelGridFooter.ClientArea.Controls.Clear();
            footerLabels.Clear();

            string[] masterColumns = { "PReturnNo", "PReturnDate", "InvoiceNo", "InvoiceDate", "VendorName", "Paymode", "SubTotal", "GrandTotal" };

            foreach (string colKey in masterColumns)
            {
                Label lbl = new Label
                {
                    Name = "lblFooter_" + colKey,
                    Text = string.Empty,
                    BackColor = GridHeaderBlue,
                    ForeColor = Color.White,
                    Font = new Font("Tahoma", 8.25F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight,
                    BorderStyle = BorderStyle.FixedSingle,
                    Visible = false,
                    Tag = colKey,
                    Cursor = Cursors.Hand
                };

                lbl.MouseUp += FooterLabel_MouseUp;
                ultraPanelGridFooter.ClientArea.Controls.Add(lbl);
                footerLabels[colKey] = lbl;
            }

            // Set default footer calculations
            columnAggregations["SubTotal"] = "SUM";
            columnAggregations["GrandTotal"] = "SUM";
            columnAggregations["PReturnNo"] = "COUNT";

            UpdateFooterCellPositions();
        }

        private void FooterLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            if (!(sender is Label lbl)) return;
            string colKey = lbl.Tag as string;
            if (string.IsNullOrEmpty(colKey)) return;

            ContextMenuStrip cms = new ContextMenuStrip();
            string currentAgg = columnAggregations.ContainsKey(colKey) ? columnAggregations[colKey] : "NONE";

            ToolStripMenuItem miNone = new ToolStripMenuItem("None", null, (s, ev) => SetColumnAggregation(colKey, "NONE")) { Checked = currentAgg == "NONE" };
            ToolStripMenuItem miSum = new ToolStripMenuItem("Sum", null, (s, ev) => SetColumnAggregation(colKey, "SUM")) { Checked = currentAgg == "SUM" };
            ToolStripMenuItem miAvg = new ToolStripMenuItem("Average", null, (s, ev) => SetColumnAggregation(colKey, "AVG")) { Checked = currentAgg == "AVG" };
            ToolStripMenuItem miCount = new ToolStripMenuItem("Count", null, (s, ev) => SetColumnAggregation(colKey, "COUNT")) { Checked = currentAgg == "COUNT" };
            ToolStripMenuItem miMin = new ToolStripMenuItem("Min", null, (s, ev) => SetColumnAggregation(colKey, "MIN")) { Checked = currentAgg == "MIN" };
            ToolStripMenuItem miMax = new ToolStripMenuItem("Max", null, (s, ev) => SetColumnAggregation(colKey, "MAX")) { Checked = currentAgg == "MAX" };

            cms.Items.AddRange(new ToolStripItem[] { miNone, new ToolStripSeparator(), miSum, miAvg, miCount, miMin, miMax });
            cms.Show(lbl, e.Location);
        }

        private void SetColumnAggregation(string colKey, string aggType)
        {
            if (aggType == "NONE")
                columnAggregations.Remove(colKey);
            else
                columnAggregations[colKey] = aggType;

            RecalculateAggregations();
        }

        private void RecalculateAggregations()
        {
            if (ultraGridMaster.Rows == null || ultraGridMaster.Rows.Count == 0)
            {
                foreach (var kvp in footerLabels)
                {
                    kvp.Value.Text = string.Empty;
                }
                return;
            }

            var visibleRows = ultraGridMaster.Rows.Where(r => !r.IsFilteredOut).ToList();

            foreach (var kvp in footerLabels)
            {
                string colKey = kvp.Key;
                Label lbl = kvp.Value;

                if (!columnAggregations.ContainsKey(colKey) || columnAggregations[colKey] == "NONE")
                {
                    lbl.Text = string.Empty;
                    continue;
                }

                string agg = columnAggregations[colKey];
                List<decimal> numericValues = new List<decimal>();

                foreach (var row in visibleRows)
                {
                    if (row.Cells.Exists(colKey) && row.Cells[colKey].Value != null && decimal.TryParse(row.Cells[colKey].Value.ToString(), out decimal d))
                    {
                        numericValues.Add(d);
                    }
                }

                switch (agg)
                {
                    case "SUM":
                        lbl.Text = numericValues.Count > 0 ? numericValues.Sum().ToString("N2") : "0.00";
                        break;
                    case "AVG":
                        lbl.Text = numericValues.Count > 0 ? numericValues.Average().ToString("N2") : "0.00";
                        break;
                    case "COUNT":
                        lbl.Text = visibleRows.Count.ToString("N0");
                        break;
                    case "MIN":
                        lbl.Text = numericValues.Count > 0 ? numericValues.Min().ToString("N2") : "0.00";
                        break;
                    case "MAX":
                        lbl.Text = numericValues.Count > 0 ? numericValues.Max().ToString("N2") : "0.00";
                        break;
                    default:
                        lbl.Text = string.Empty;
                        break;
                }
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraPanelGridFooter == null || ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand masterBand = ultraGridMaster.DisplayLayout.Bands[0];
            int selectorWidth = ultraGridMaster.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True ? ultraGridMaster.DisplayLayout.Override.RowSelectorWidth : 0;

            foreach (var kvp in footerLabels)
            {
                string colKey = kvp.Key;
                Label lbl = kvp.Value;

                if (!masterBand.Columns.Exists(colKey) || masterBand.Columns[colKey].Hidden)
                {
                    lbl.Visible = false;
                    continue;
                }

                UltraGridColumn col = masterBand.Columns[colKey];
                UIElement colElement = col.Header.GetUIElement();

                if (colElement != null)
                {
                    Rectangle rect = colElement.Rect;
                    lbl.SetBounds(rect.X, 1, rect.Width, ultraPanelGridFooter.Height - 2);
                    lbl.Visible = true;

                    if (col.CellAppearance.TextHAlign == HAlign.Right)
                        lbl.TextAlign = ContentAlignment.MiddleRight;
                    else if (col.CellAppearance.TextHAlign == HAlign.Center)
                        lbl.TextAlign = ContentAlignment.MiddleCenter;
                    else
                        lbl.TextAlign = ContentAlignment.MiddleLeft;
                }
                else
                {
                    lbl.Visible = false;
                }
            }
        }
        #endregion

        #region Grid Context Menu & Drag-Drop Column Chooser
        private void InitializeGridContextMenuAndDragDrop()
        {
            ContextMenuStrip gridMenu = new ContextMenuStrip();

            ToolStripMenuItem miFreeze = new ToolStripMenuItem("Freeze Columns", null, (s, e) => FreezeSelectedColumn());
            ToolStripMenuItem miHide = new ToolStripMenuItem("Hide Column", null, (s, e) => HideSelectedColumn());
            ToolStripMenuItem miExport = new ToolStripMenuItem("Export to Excel", null, (s, e) => btnExport_Click(s, e));
            ToolStripMenuItem miCopy = new ToolStripMenuItem("Copy Cell", null, (s, e) => CopySelectedCell());
            ToolStripMenuItem miPrint = new ToolStripMenuItem("Print Report", null, (s, e) => btnPrint_Click(s, e));
            ToolStripMenuItem miColumnChooser = new ToolStripMenuItem("Show Column Chooser", null, (s, e) => ShowColumnChooser());
            ToolStripMenuItem miAutoSize = new ToolStripMenuItem("Auto Size All Columns", null, (s, e) => AutoSizeAllColumns());

            gridMenu.Items.AddRange(new ToolStripItem[] {
                miFreeze, miHide, new ToolStripSeparator(),
                miColumnChooser, miAutoSize, new ToolStripSeparator(),
                miCopy, miExport, miPrint
            });

            ultraGridMaster.ContextMenuStrip = gridMenu;
            ultraGridMaster.MouseDown += UltraGridMaster_MouseDown;
            ultraGridMaster.MouseMove += UltraGridMaster_MouseMove;
            ultraGridMaster.MouseUp += UltraGridMaster_MouseUp;
        }

        private void FreezeSelectedColumn()
        {
            if (ultraGridMaster.ActiveCell != null)
            {
                UltraGridColumn col = ultraGridMaster.ActiveCell.Column;
                col.Header.Fixed = !col.Header.Fixed;
            }
        }

        private void HideSelectedColumn()
        {
            if (ultraGridMaster.ActiveCell != null)
            {
                ultraGridMaster.ActiveCell.Column.Hidden = true;
                UpdateFooterCellPositions();
            }
        }

        private void CopySelectedCell()
        {
            if (ultraGridMaster.ActiveCell != null && ultraGridMaster.ActiveCell.Value != null)
            {
                Clipboard.SetText(ultraGridMaster.ActiveCell.Value.ToString());
            }
        }

        private void AutoSizeAllColumns()
        {
            ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
            UpdateFooterCellPositions();
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                columnChooserForm = new Form
                {
                    Text = "Column Chooser",
                    Size = new Size(260, 350),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    TopMost = true
                };

                CheckedListBox clb = new CheckedListBox
                {
                    Dock = DockStyle.Fill,
                    CheckOnClick = true
                };

                if (ultraGridMaster.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
                    for (int i = 0; i < band.Columns.Count; i++)
                    {
                        UltraGridColumn col = band.Columns[i];
                        clb.Items.Add(new ColumnItem(col.Key, col.Header.Caption ?? col.Key, 0), !col.Hidden);
                    }
                }

                clb.ItemCheck += (s, e) =>
                {
                    if (clb.Items[e.Index] is ColumnItem item)
                    {
                        if (ultraGridMaster.DisplayLayout.Bands[item.BandIndex].Columns.Exists(item.ColumnKey))
                        {
                            ultraGridMaster.DisplayLayout.Bands[item.BandIndex].Columns[item.ColumnKey].Hidden = (e.NewValue != CheckState.Checked);
                            this.BeginInvoke((MethodInvoker)delegate { UpdateFooterCellPositions(); });
                        }
                    }
                };

                columnChooserForm.Controls.Add(clb);
            }

            columnChooserForm.Show(this);
        }

        private void UltraGridMaster_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                UIElement element = ultraGridMaster.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                HeaderUIElement headerElement = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerElement?.GetContext(typeof(UltraGridColumn)) is UltraGridColumn col)
                {
                    headerDragStartPoint = e.Location;
                    columnToHideByDrag = col;
                    isDraggingHeaderColumn = false;
                }
            }
        }

        private void UltraGridMaster_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnToHideByDrag != null)
            {
                if (!isDraggingHeaderColumn)
                {
                    int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
                    int deltaY = Math.Abs(e.Y - headerDragStartPoint.Y);
                    if (deltaX > 8 || deltaY > 8)
                    {
                        isDraggingHeaderColumn = true;
                    }
                }
            }
        }

        private void UltraGridMaster_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderColumn && columnToHideByDrag != null)
            {
                // If dropped far outside header area (e.g. dragged down into grid to hide)
                if (e.Y > 70 && !ultraGridMaster.ClientRectangle.Contains(e.Location))
                {
                    columnToHideByDrag.Hidden = true;
                    UpdateFooterCellPositions();
                }
            }

            columnToHideByDrag = null;
            isDraggingHeaderColumn = false;
        }

        private void UltraGridMaster_AfterColPosChanged(object sender, AfterColPosChangedEventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private void UltraGridMaster_AfterRowFilterChanged(object sender, AfterRowFilterChangedEventArgs e)
        {
            RecalculateAggregations();
        }

        private void UltraGridMaster_AfterSortChange(object sender, BandEventArgs e)
        {
            RecalculateAggregations();
        }

        private void UltraGridMaster_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
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
                // Details are already loaded
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
                if (e.Row.Band.Index == 0 && e.Row.Cells["PReturnNo"] != null)
                {
                    int pReturnNo = Convert.ToInt32(e.Row.Cells["PReturnNo"].Value);
                    int detailCount = e.Row.ChildBands[0].Rows.Count;
                    System.Diagnostics.Debug.WriteLine($"Expanded Return No: {pReturnNo}, Detail Count: {detailCount}");
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
                LoadPurchaseReturnData();
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
                if (dsHierarchical.Tables["PurchaseReturnMaster"].Rows.Count > 0)
                {
                    PrintPurchaseReturnReport();
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
        /// Print the purchase return report
        /// </summary>
        private void PrintPurchaseReturnReport()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = "Purchase Return Report";
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
                e.Graphics.DrawString("PURCHASE RETURN REPORT - MASTER DETAIL VIEW", titleFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;

                // Print date range
                string dateRange = $"From: {ultraDateTimeEditorFrom.Value:dd/MM/yyyy} To: {ultraDateTimeEditorTo.Value:dd/MM/yyyy}";
                e.Graphics.DrawString(dateRange, dataFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;

                // Print summary
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                decimal totalAmount = 0;
                int totalReturns = masterTable.Rows.Count;

                foreach (DataRow row in masterTable.Rows)
                {
                    totalAmount += Convert.ToDecimal(row["GrandTotal"]);
                }

                e.Graphics.DrawString($"Total Returns: {totalReturns}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Grand Total: ₹ {totalAmount:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;

                // Print master data headers
                string[] headers = { "Return No", "Date", "Vendor", "Payment", "Sub Total", "Grand Total" };
                float[] columnWidths = { 90, 90, 180, 100, 110, 110 };
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
                        row["PReturnNo"].ToString(),
                        Convert.ToDateTime(row["PReturnDate"]).ToString("dd/MM/yyyy"),
                        row["VendorName"].ToString(),
                        row["Paymode"].ToString(),
                        Convert.ToDecimal(row["SubTotal"]).ToString("N2"),
                        Convert.ToDecimal(row["GrandTotal"]).ToString("N2")
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
                e.Graphics.DrawString($"GRAND TOTAL: ₹ {totalAmount:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
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
                if (dsHierarchical.Tables["PurchaseReturnMaster"].Rows.Count > 0)
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
                LoadPurchaseReturnDataWithFilters();
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
        /// Validate amount range input
        /// </summary>
        private bool ValidateAmountRange()
        {
            try
            {
                if (ultraNumericEditorAmountFrom.Value != null && ultraNumericEditorAmountTo.Value != null)
                {
                    decimal fromAmount = Convert.ToDecimal(ultraNumericEditorAmountFrom.Value);
                    decimal toAmount = Convert.ToDecimal(ultraNumericEditorAmountTo.Value);

                    if (fromAmount > toAmount)
                    {
                        MessageBox.Show("From amount cannot be greater than To amount.", "Invalid Amount Range",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ultraNumericEditorAmountFrom.Focus();
                        return false;
                    }

                    if (fromAmount < 0 || toAmount < 0)
                    {
                        MessageBox.Show("Amount values cannot be negative.", "Invalid Amount",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid amount format: {ex.Message}", "Amount Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        public void RibbonClear() => btnClearFilters_Click(this, EventArgs.Empty);
        public void Clear() => btnClearFilters_Click(this, EventArgs.Empty);

        public void btnClearFilters_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // Clear all search filters
                ultraDateTimeEditorFrom.Value = new DateTime(1990, 1, 1);
                ultraDateTimeEditorTo.Value = DateTime.Today;
                ultraNumericEditorAmountFrom.Value = null;
                ultraNumericEditorAmountTo.Value = null;
                ultraNumericEditorReturnNo.Value = null;

                // Clear vendor text field
                ultraTextEditorVendor.Value = null;
                ultraTextEditorVendor.Text = string.Empty;

                ultraComboPresetDates.Value = "ALL";

                // Force UI update
                Application.DoEvents();

                // Reload all data
                LoadPurchaseReturnData();
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

                string preset = ultraComboPresetDates.Value.ToString();
                DateTime fromDate, toDate;

                switch (preset)
                {
                    case "ALL":
                        fromDate = new DateTime(1990, 1, 1);
                        toDate = DateTime.Today;
                        break;
                    case "DATE_RANGE":
                        fromDate = DateTime.Today.AddDays(-30);
                        toDate = DateTime.Today;
                        break;
                    case "Today":
                        fromDate = DateTime.Now.Date;
                        toDate = DateTime.Now.Date;
                        break;
                    case "Yesterday":
                        fromDate = DateTime.Now.AddDays(-1).Date;
                        toDate = DateTime.Now.AddDays(-1).Date;
                        break;
                    case "ThisWeek":
                        fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).Date;
                        toDate = DateTime.Now.Date;
                        break;
                    case "LastWeek":
                        fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 7).Date;
                        toDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 1).Date;
                        break;
                    case "ThisMonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        toDate = DateTime.Now.Date;
                        break;
                    case "LastMonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                        break;
                    case "ThisQuarter":
                        int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                        fromDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                        toDate = DateTime.Now.Date;
                        break;
                    case "LastQuarter":
                        int lastQuarter = (DateTime.Now.Month - 1) / 3;
                        if (lastQuarter == 0)
                        {
                            lastQuarter = 4;
                            fromDate = new DateTime(DateTime.Now.Year - 1, 10, 1);
                        }
                        else
                        {
                            fromDate = new DateTime(DateTime.Now.Year, (lastQuarter - 1) * 3 + 1, 1);
                        }
                        toDate = fromDate.AddMonths(3).AddDays(-1);
                        break;
                    case "ThisYear":
                        fromDate = new DateTime(DateTime.Now.Year, 1, 1);
                        toDate = DateTime.Now.Date;
                        break;
                    case "LastYear":
                        fromDate = new DateTime(DateTime.Now.Year - 1, 1, 1);
                        toDate = new DateTime(DateTime.Now.Year - 1, 12, 31);
                        break;
                    default:
                        return; // Custom range - don't change dates
                }

                ultraDateTimeEditorFrom.Value = fromDate;
                ultraDateTimeEditorTo.Value = toDate;
                UpdateDateControlVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting preset dates: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load purchase return data with applied filters
        /// </summary>
        private void LoadPurchaseReturnDataWithFilters()
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
                ultraGridMaster.DataMember = "PurchaseReturnMaster";

                // Force complete grid refresh
                ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // Update Grid Calculation Footer
                RecalculateAggregations();
                UpdateFooterCellPositions();

                // Show result count
                int resultCount = dsHierarchical.Tables["PurchaseReturnMaster"].Rows.Count;
                if (resultCount == 0)
                {
                    MessageBox.Show("No records found matching the search criteria.", "Search Results",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching purchase return data: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error",
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
                dsHierarchical.Tables["PurchaseReturnDetail"].Clear();
                dsHierarchical.Tables["PurchaseReturnMaster"].Clear();

                // Step 3: Load master data from database
                int branchId = SessionContext.BranchId;
                List<PurchaseReturnReportMaster> masterData = reportRepository.GetPurchaseReturnRecords(fromDate, toDate, branchId);

                // Step 4: Apply filters to master data BEFORE adding to DataTable
                var filteredMasterData = ApplyMasterFilters(masterData);

                // Step 5: Add filtered master data to DataTable
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                foreach (var returnRecord in filteredMasterData)
                {
                    if (returnRecord.PReturnNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["PReturnNo"] = returnRecord.PReturnNo;
                        row["PReturnDate"] = returnRecord.PReturnDate;
                        row["InvoiceNo"] = returnRecord.InvoiceNo ?? "";
                        row["InvoiceDate"] = returnRecord.InvoiceDate;
                        row["VendorName"] = returnRecord.VendorName ?? "";
                        row["Paymode"] = returnRecord.Paymode ?? "";
                        row["SubTotal"] = returnRecord.SubTotal;
                        row["GrandTotal"] = returnRecord.GrandTotal;
                        masterTable.Rows.Add(row);
                    }
                }

                // Step 6: Load details only for filtered returns
                if (masterTable.Rows.Count > 0)
                {
                    LoadDetailsForFilteredReturns();
                }

                // Step 7: Recreate the relation
                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["PReturnNo"],
                    dsHierarchical.Tables["PurchaseReturnDetail"].Columns["PReturnNo"],
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
        private List<PurchaseReturnReportMaster> ApplyMasterFilters(List<PurchaseReturnReportMaster> masterData)
        {
            var filtered = masterData.AsEnumerable();

            // Filter by Return Number
            if (ultraNumericEditorReturnNo.Value != null && Convert.ToInt32(ultraNumericEditorReturnNo.Value) > 0)
            {
                int returnNo = Convert.ToInt32(ultraNumericEditorReturnNo.Value);
                filtered = filtered.Where(x => x.PReturnNo == returnNo);
            }

            // Filter by Vendor Name (case-insensitive partial match)
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
        /// Load details only for returns that passed the filter
        /// </summary>
        private void LoadDetailsForFilteredReturns()
        {
            try
            {
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                DataTable detailTable = dsHierarchical.Tables["PurchaseReturnDetail"];

                // Get all PReturnNo values from filtered master
                var returnNumbers = masterTable.AsEnumerable()
                    .Select(r => r.Field<int>("PReturnNo"))
                    .ToList();

                // Load details for each filtered return
                foreach (int pReturnNo in returnNumbers)
                {
                    PurchaseReturnReportData reportData = reportRepository.GetPurchaseReturnReportDetails(pReturnNo);

                    if (reportData?.Details != null && reportData.Details.Count > 0)
                    {
                        foreach (var detail in reportData.Details)
                        {
                            DataRow row = detailTable.NewRow();
                            row["PReturnNo"] = pReturnNo;
                            row["SlNo"] = detail.SlNo;
                            row["ItemName"] = detail.ItemName ?? "";
                            row["Unit"] = detail.Unit ?? "";
                            row["Packing"] = detail.Packing ?? "";
                            row["Qty"] = detail.Qty;
                            row["Cost"] = detail.Cost;
                            row["TaxPer"] = detail.TaxPer;
                            row["TaxAmt"] = detail.TaxAmt;
                            row["Amount"] = detail.Amount;
                            row["Reason"] = detail.Reason ?? "";
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
                saveDialog.FileName = $"PurchaseReturnReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable exportTable = CreateExportTable();
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
            DataTable exportTable = new DataTable("PurchaseReturnReport");

            // Add columns
            exportTable.Columns.Add("PReturnNo", typeof(int));
            exportTable.Columns.Add("PReturnDate", typeof(DateTime));
            exportTable.Columns.Add("InvoiceNo", typeof(string));
            exportTable.Columns.Add("InvoiceDate", typeof(DateTime));
            exportTable.Columns.Add("VendorName", typeof(string));
            exportTable.Columns.Add("Paymode", typeof(string));
            exportTable.Columns.Add("ItemName", typeof(string));
            exportTable.Columns.Add("Unit", typeof(string));
            exportTable.Columns.Add("Quantity", typeof(decimal));
            exportTable.Columns.Add("Cost", typeof(decimal));
            exportTable.Columns.Add("TaxPer", typeof(decimal));
            exportTable.Columns.Add("TaxAmt", typeof(decimal));
            exportTable.Columns.Add("Amount", typeof(decimal));
            exportTable.Columns.Add("Reason", typeof(string));
            exportTable.Columns.Add("SubTotal", typeof(decimal));
            exportTable.Columns.Add("GrandTotal", typeof(decimal));

            // Flatten the hierarchical data
            foreach (DataRow masterRow in dsHierarchical.Tables["PurchaseReturnMaster"].Rows)
            {
                DataRow[] detailRows = dsHierarchical.Tables["PurchaseReturnDetail"].Select($"PReturnNo = {masterRow["PReturnNo"]}");

                if (detailRows.Length > 0)
                {
                    foreach (DataRow detailRow in detailRows)
                    {
                        DataRow exportRow = exportTable.NewRow();
                        exportRow["PReturnNo"] = masterRow["PReturnNo"];
                        exportRow["PReturnDate"] = masterRow["PReturnDate"];
                        exportRow["InvoiceNo"] = masterRow["InvoiceNo"];
                        exportRow["InvoiceDate"] = masterRow["InvoiceDate"];
                        exportRow["VendorName"] = masterRow["VendorName"];
                        exportRow["Paymode"] = masterRow["Paymode"];
                        exportRow["ItemName"] = detailRow["ItemName"];
                        exportRow["Unit"] = detailRow["Unit"];
                        exportRow["Quantity"] = detailRow["Qty"];
                        exportRow["Cost"] = detailRow["Cost"];
                        exportRow["TaxPer"] = detailRow["TaxPer"];
                        exportRow["TaxAmt"] = detailRow["TaxAmt"];
                        exportRow["Amount"] = detailRow["Amount"];
                        exportRow["Reason"] = detailRow["Reason"];
                        exportRow["SubTotal"] = masterRow["SubTotal"];
                        exportRow["GrandTotal"] = masterRow["GrandTotal"];
                        exportTable.Rows.Add(exportRow);
                    }
                }
                else
                {
                    // Add master row even if no details
                    DataRow exportRow = exportTable.NewRow();
                    exportRow["PReturnNo"] = masterRow["PReturnNo"];
                    exportRow["PReturnDate"] = masterRow["PReturnDate"];
                    exportRow["InvoiceNo"] = masterRow["InvoiceNo"];
                    exportRow["InvoiceDate"] = masterRow["InvoiceDate"];
                    exportRow["VendorName"] = masterRow["VendorName"];
                    exportRow["Paymode"] = masterRow["Paymode"];
                    exportRow["SubTotal"] = masterRow["SubTotal"];
                    exportRow["GrandTotal"] = masterRow["GrandTotal"];
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
        private void PurchaseReturnReport_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                Application.DoEvents(); // Allow form to paint first
                LoadPurchaseReturnData();
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

        private void PurchaseReturnReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
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
