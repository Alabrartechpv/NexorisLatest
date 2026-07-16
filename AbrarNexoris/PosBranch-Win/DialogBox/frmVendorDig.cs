using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using ModelClass.Master;
using Repository;
using Repository.MasterRepositry;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.DialogBox
{
    public partial class frmVendorDig : Form
    {
        private VendorRepository objRepoVendor;
        private Dropdowns drop;
        private int maxRecordsToDisplay = int.MaxValue; // Changed to show all items by default
        private DataTable originalDataTable = null; // Store the original data

        // Add properties to store selected vendor information
        public int SelectedVendorId { get; private set; }
        public string SelectedVendorName { get; private set; }

        // Add a timer field at the class level
        private System.Windows.Forms.Timer inputDebounceTimer;
        private string lastProcessedValue = string.Empty;

        // New method to handle panel clicks with protection against multiple calls
        private bool isHandlingClick = false;

        // Add a class-level variable to track the original order
        private bool isOriginalOrder = true;

        // Add these fields to track drag operations at the class level
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;

        // Add a class-level variable to store the column chooser form
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;

        // Add the savedColumnWidths dictionary if it doesn't exist
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();

        // Add a ToolTip control for feedback
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

        public frmVendorDig()
        {
            InitializeComponent();
            objRepoVendor = new VendorRepository();
            drop = new Dropdowns();

            // Style the panels to match frmdialForItemMaster.cs if they exist
            if (this.Controls.Find("ultraPanel10", true).Length > 0)
                StyleIconPanel((Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel10", true)[0]);

            if (this.Controls.Find("ultraPanel11", true).Length > 0)
                StyleIconPanel((Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel11", true)[0]);

            if (this.Controls.Find("ultraPanel12", true).Length > 0)
                StyleIconPanel((Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel12", true)[0]);

            if (this.Controls.Find("ultraPanel4", true).Length > 0)
                StyleIconPanel((Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel4", true)[0]);

            if (this.Controls.Find("ultraPanel5", true).Length > 0)
                StyleIconPanel((Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel5", true)[0]);

            // Connect panel click events
            ConnectPanelClickEvents();

            // Initialize search filter comboBox
            InitializeSearchFilterComboBox();

            // Initialize column order comboBox2
            InitializeColumnOrderComboBox();

            SetupUltraGridStyle();

            // Initialize label2 with record count info if it exists
            if (this.Controls.Find("label2", true).Length > 0)
            {
                Label label2 = this.Controls.Find("label2", true)[0] as Label;
                if (label2 != null)
                {
                    // Set initial text with "0 of 0 records"
                    label2.Text = "Showing 0 of 0 records";
                    label2.AutoSize = true;
                    label2.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                    label2.ForeColor = Color.FromArgb(0, 70, 170); // Dark blue color
                }
            }

            // Load vendor grid and update counts
            LoadVendorGrid();

            // Set up search box event
            Searchbx.TextChanged += Searchbx_TextChanged;
            Searchbx.KeyDown += Searchbx_KeyDown;

            // Set up grid events
            ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;

            // Set up drag-drop column functionality
            SetupColumnDragDrop();

            // Add resize event to adjust columns when form is resized
            this.Resize += frmVendorDig_Resize;

            // Initialize the debounce timer
            inputDebounceTimer = new System.Windows.Forms.Timer();
            inputDebounceTimer.Interval = 5; // Very short interval for instant response
            inputDebounceTimer.Tick += InputDebounceTimer_Tick;

            // Register textBox1 event for record limit
            textBox1.TextChanged += textBox1_TextChanged;
            textBox1.KeyDown += TextBox1_KeyDown;

            // Set up OK button click handler if it exists
            if (this.Controls.Find("btnOK", true).Length > 0)
            {
                Button btnOK = this.Controls.Find("btnOK", true)[0] as Button;
                if (btnOK != null)
                {
                    btnOK.Click += btnOK_Click;
                }
            }

            // Connect ultraPictureBox4 click event for sorting toggle
            if (this.Controls.Find("ultraPictureBox4", true).Length > 0)
            {
                Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox4 =
                    (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox4", true)[0];

                // Set cursor to hand to indicate clickable
                pictureBox4.Cursor = Cursors.Hand;

                // Add tooltip
                System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
                tooltip.SetToolTip(pictureBox4, "Click to toggle between original order and reverse order");

                // Connect click event
                pictureBox4.Click += UltraPictureBox4_Click;

                // Add hover effect
                pictureBox4.MouseEnter += (s, e) =>
                {
                    // Make the control appear brighter on hover
                    pictureBox4.Appearance.BorderColor = Color.FromArgb(0, 120, 215);
                    pictureBox4.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                };

                pictureBox4.MouseLeave += (s, e) =>
                {
                    // Restore normal appearance
                    pictureBox4.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
                };
            }

            // Add the Load event handler
            this.Load += frmVendorDig_Load;
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Define consistent colors for all panels
            Color lightBlue = Color.FromArgb(127, 219, 255); // Light blue
            Color darkBlue = Color.FromArgb(0, 116, 217);    // Darker blue
            Color borderBlue = Color.FromArgb(0, 150, 220);  // Border blue
            Color borderBase = Color.FromArgb(0, 100, 180);  // Border base color

            // Create a gradient from light to dark blue with exact specified colors
            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Set highly rounded border style
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;

            // Add exact specified border color
            panel.Appearance.BorderColor = borderBlue;
            panel.Appearance.BorderColor3DBase = borderBase;

            // Ensure icons inside have transparent background
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    Infragistics.Win.UltraWinEditors.UltraPictureBox pic = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;

                    // Add hover effect to picture box
                    pic.MouseEnter += (sender, e) =>
                    {
                        pic.Appearance.BorderColor = Color.White;
                    };

                    pic.MouseLeave += (sender, e) =>
                    {
                        pic.Appearance.BorderColor = Color.Transparent;
                    };

                    // Set cursor to indicate clickable
                    pic.Cursor = Cursors.Hand;
                }
                else if (control is Label)
                {
                    Label lbl = (Label)control;
                    lbl.ForeColor = Color.White;
                    lbl.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    lbl.BackColor = Color.Transparent;

                    // Set cursor to indicate clickable
                    lbl.Cursor = Cursors.Hand;

                    // Add hover effect to label - keep white color (removed yellow)
                    lbl.MouseEnter += (sender, e) =>
                    {
                        lbl.ForeColor = Color.White; // Keep white instead of yellow
                    };

                    lbl.MouseLeave += (sender, e) =>
                    {
                        lbl.ForeColor = Color.White;
                    };
                }
            }

            // Add hover effect with consistent colors
            panel.ClientArea.MouseEnter += (sender, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
            };

            panel.ClientArea.MouseLeave += (sender, e) =>
            {
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
            };

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                // Reset everything first to ensure clean slate
                ultraGrid1.DisplayLayout.Reset();

                // Remove the grid caption (banner)
                ultraGrid1.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.ViewStyle = ViewStyle.SingleBand;

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Enable column moving and dragging - important for column draggability
                ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = AllowColSwapping.WithinBand;

                // Important: This setting ensures we get only row selection on click, not automatic action
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                // Hide the group-by area (gray bar)
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Set rounded borders for the entire grid
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

                // Configure grid lines - single line borders for rows and columns
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set border width to single line
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

                // Remove ALL cell padding/spacing - critical to remove unwanted space
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;
                ultraGrid1.DisplayLayout.MaxBandDepth = 1;
                ultraGrid1.DisplayLayout.MaxColScrollRegions = 1;
                ultraGrid1.DisplayLayout.MaxRowScrollRegions = 1;

                // Define colors
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Set light blue border color for cells
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Configure row height - increase to match the image (about 26-30 pixels)
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.Fixed;

                // Add header styling - blue headers
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                // Configure row selector appearance with blue
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None; // Remove numbers
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

                // Set all cells to have white background (no alternate row coloring)
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Configure selected row appearance with light blue highlight
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue from the image
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast

                // Configure active row appearance - make it same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Set font size for all cells to match the image (standard text size)
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Configure scrollbar style
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

                // Configure the scrollbar look
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    // Configure button appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                    // Configure track appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                    // Configure thumb appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Configure cell appearance to increase vertical content alignment
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = HAlign.Center;

                // Connect events
                ultraGrid1.DoubleClickCell += new DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);
                ultraGrid1.ClickCell += new ClickCellEventHandler(ultraGrid1_ClickCell);

                // Allow drop operation for drag-drop support
                ultraGrid1.AllowDrop = true;

                // Configure column auto size behavior for optimal resizing
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = ColumnAutoSizeMode.VisibleRows;

                // Refresh the grid to apply all changes
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Remove the grid caption (banner)
                e.Layout.CaptionVisible = DefaultableBoolean.False;
                e.Layout.ViewStyle = ViewStyle.SingleBand;

                // Set the row selector width to minimum
                e.Layout.Override.RowSelectorWidth = 15;

                // Apply proper grid line styles
                e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set border style for the main grid
                e.Layout.BorderStyle = UIElementBorderStyle.Solid;

                // Remove ALL cell padding/spacing - critical to remove unwanted space
                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;
                e.Layout.MaxBandDepth = 1;
                e.Layout.MaxColScrollRegions = 1;
                e.Layout.MaxRowScrollRegions = 1;

                // Configure row height to match the image
                e.Layout.Override.MinRowHeight = 30;
                e.Layout.Override.DefaultRowHeight = 30;
                e.Layout.Override.RowSizing = RowSizing.Fixed;

                // Define colors
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Set default alignment for all cells
                e.Layout.Override.CellAppearance.TextHAlign = HAlign.Center;
                e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Set font size for all cells to match the image (standard text size)
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Enable column moving and dragging - important for column draggability
                e.Layout.Override.AllowColMoving = AllowColMoving.WithinBand;
                e.Layout.Override.AllowColSizing = AllowColSizing.Free;
                e.Layout.Override.AllowColSwapping = AllowColSwapping.WithinBand;

                // Configure columns
                foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                {
                    // Set header style for all columns
                    col.Header.Appearance.BackColor = headerBlue;
                    col.Header.Appearance.BackColor2 = headerBlue;
                    col.Header.Appearance.BackGradientStyle = GradientStyle.None;
                    col.Header.Appearance.ForeColor = Color.White;
                    col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                    col.Header.Appearance.FontData.SizeInPoints = 9;
                    col.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                    col.Header.Appearance.TextHAlign = HAlign.Center;
                    col.Header.Appearance.TextVAlign = VAlign.Middle;

                    // Configure specific columns
                    switch (col.Key)
                    {
                        case "LedgerID":
                            col.Header.Caption = "ID";
                            col.Width = 100;
                            col.CellAppearance.TextHAlign = HAlign.Center;
                            break;
                        case "LedgerName":
                            col.Header.Caption = "Vendor Name";
                            col.Width = 250;
                            col.CellAppearance.TextHAlign = HAlign.Left;
                            break;
                        case "VEN_ACCTCODE":
                            col.Header.Caption = "Acct Code";
                            col.Width = 120;
                            col.CellAppearance.TextHAlign = HAlign.Center;
                            break;
                    }
                }

                // Apply row selector appearance with blue
                e.Layout.Override.RowSelectorAppearance.BackColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;
                e.Layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                e.Layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None;

                // Set all cells to white background
                e.Layout.Override.RowAppearance.BackColor = Color.White;
                e.Layout.Override.RowAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                e.Layout.Override.RowAlternateAppearance.BackColor = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Configure selected row appearance with light blue highlight
                e.Layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue from the image
                e.Layout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                e.Layout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast

                // Configure active row appearance - make it same as selected row
                e.Layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                e.Layout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                e.Layout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Configure scrollbar style
                e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
                e.Layout.ScrollStyle = ScrollStyle.Immediate;

                // Configure the scrollbar look
                if (e.Layout.ScrollBarLook != null)
                {
                    // Style track and buttons with blue colors
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    e.Layout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                    e.Layout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing grid layout: {ex.Message}");
            }
        }

        private void ultraGrid1_DoubleClickCell(object sender, Infragistics.Win.UltraWinGrid.DoubleClickCellEventArgs e)
        {
            if (e.Cell != null && e.Cell.Row != null)
            {
                SetSelectedVendor();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void ultraGrid1_ClickCell(object sender, Infragistics.Win.UltraWinGrid.ClickCellEventArgs e)
        {
            // Code to handle cell click if needed
        }

        private void LoadVendorGrid()
        {
            try
            {
                VendorDDLGrids vendorDDLGrids = drop.VendorDDL();

                // Create DataTable from the list to allow sorting and searching
                DataTable dt = new DataTable();
                if (vendorDDLGrids.List != null && vendorDDLGrids.List.Count() > 0)
                {
                    // Convert List to DataTable
                    System.ComponentModel.PropertyDescriptorCollection properties =
                        System.ComponentModel.TypeDescriptor.GetProperties(typeof(ModelClass.VendorDDLG));

                    foreach (System.ComponentModel.PropertyDescriptor prop in properties)
                    {
                        dt.Columns.Add(prop.Name, System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }

                    foreach (var item in vendorDDLGrids.List)
                    {
                        DataRow row = dt.NewRow();
                        foreach (System.ComponentModel.PropertyDescriptor prop in properties)
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        dt.Rows.Add(row);
                    }

                    // Add a column to preserve the original row order
                    PreserveOriginalRowOrder(dt);

                    // Store the original DataTable
                    originalDataTable = dt.Copy();

                    // Update maxRecordsToDisplay to show all records and update textBox1
                    maxRecordsToDisplay = dt.Rows.Count;
                    textBox1.Text = maxRecordsToDisplay.ToString();

                    // Use the full DataTable as the datasource - showing all records
                    ultraGrid1.DataSource = dt;
                }
                else
                {
                    ultraGrid1.DataSource = vendorDDLGrids.List;
                    textBox1.Text = "0";
                }

                ConfigureGridLayout();

                // Apply column ordering based on comboBox2 selection
                if (comboBox2.SelectedItem != null)
                {
                    ReorderColumns(comboBox2.SelectedItem.ToString());
                }

                // Update the record count label
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading vendor data: " + ex.Message);
            }
        }

        /// <summary>Reloads vendor data — called after a vendor is saved or updated.</summary>
        public void RefreshData() => LoadVendorGrid();

        private void ConfigureGridLayout()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Hide all columns first
                foreach (UltraGridColumn col in band.Columns)
                {
                    col.Hidden = true;
                }

                // Set the row selector width to minimum
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;

                // Calculate total available width (grid width minus row selector width and vertical scrollbar)
                int totalWidth = ultraGrid1.Width - ultraGrid1.DisplayLayout.Override.RowSelectorWidth - 20; // 20 for scrollbar and borders

                // Get current column order from comboBox2
                string currentOrder = comboBox2.SelectedItem?.ToString() ?? "Id";

                // Show only specific columns with adjusted widths for equal distribution
                if (band.Columns.Exists("LedgerID"))
                {
                    band.Columns["LedgerID"].Hidden = false;
                    band.Columns["LedgerID"].Header.Caption = "ID";
                    band.Columns["LedgerID"].Width = (int)(totalWidth * 0.15); // 15% of available space
                    band.Columns["LedgerID"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["LedgerID"].Header.VisiblePosition = currentOrder == "Id" ? 0 : 1;
                }

                if (band.Columns.Exists("LedgerName"))
                {
                    band.Columns["LedgerName"].Hidden = false;
                    band.Columns["LedgerName"].Header.Caption = "Vendor Name";
                    band.Columns["LedgerName"].Width = (int)(totalWidth * 0.85); // 85% of available space
                    band.Columns["LedgerName"].CellAppearance.TextHAlign = HAlign.Left;
                    band.Columns["LedgerName"].Header.VisiblePosition = currentOrder == "Vendor Name" ? 0 : 1;
                }

                if (band.Columns.Exists("VEN_ACCTCODE"))
                {
                    band.Columns["VEN_ACCTCODE"].Hidden = true; // Hide this column to give more space to the others
                }

                // Force the layout to update
                ultraGrid1.DisplayLayout.Bands[0].Override.AllowColSizing = AllowColSizing.None;
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                var visibleRows = ultraGrid1.Rows.Cast<UltraGridRow>()
                    .Where(r => !r.Hidden)
                    .ToList();

                int currentIndex = visibleRows.IndexOf(ultraGrid1.ActiveRow);
                UltraGridRow nextRow = null;

                if (e.KeyCode == Keys.Up)
                {
                    if (currentIndex <= 0)
                    {
                        // If at first row, go back to search box
                        Searchbx.Focus();
                        return;
                    }
                    nextRow = visibleRows[currentIndex - 1];
                }
                else
                {
                    nextRow = currentIndex < visibleRows.Count - 1 ? visibleRows[currentIndex + 1] : visibleRows.LastOrDefault();
                }

                if (nextRow != null)
                {
                    ultraGrid1.ActiveRow = nextRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(nextRow);
                }

                e.Handled = true;
            }

            if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
            {
                // Set the selected vendor properties when Enter is pressed
                SetSelectedVendor();
                DialogResult = DialogResult.OK;
                Close();
                e.Handled = true;
            }
        }

        // Add a method to set the selected vendor properties
        private void SetSelectedVendor()
        {
            if (ultraGrid1.ActiveRow != null)
            {
                // Get the selected vendor data from the active row
                if (ultraGrid1.ActiveRow.Cells["LedgerID"] != null &&
                    ultraGrid1.ActiveRow.Cells["LedgerName"] != null)
                {
                    SelectedVendorId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["LedgerID"].Value);
                    SelectedVendorName = ultraGrid1.ActiveRow.Cells["LedgerName"].Value.ToString();
                }
            }
        }

        // Add double-click event handler
        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row != null)
            {
                SetSelectedVendor();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null)
            {
                SetSelectedVendor();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a vendor first.", "Selection Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                System.Diagnostics.Debug.WriteLine("Escape key pressed, closing form");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void frmVendorDig_Load(object sender, EventArgs e)
        {
            // Ensure record count label is updated with accurate numbers on form load
            if (originalDataTable != null)
            {
                UpdateRecordCountLabel();
            }

            // Wire up ultraPanel12 (OK) and ultraPanel11 (Close) click handlers
            WireOKCloseClickHandlers();

            // Use Shown event to set focus on Searchbx (more reliable than Load event)
            this.Shown += (s, evt) =>
            {
                // Set focus to the search textbox when form is fully displayed
                if (Searchbx != null)
                {
                    Searchbx.Focus();
                    Searchbx.SelectAll();
                }
            };
        }

        /// <summary>
        /// Wire up click handlers for ultraPanel12 (OK) and ultraPanel11 (Close)
        /// </summary>
        private void WireOKCloseClickHandlers()
        {
            try
            {
                // Wire up ultraPanel12 (OK button) - with label7 and ultraPictureBox9
                ultraPanel12.Click += Panel12_OKClick;
                ultraPanel12.ClientArea.Click += Panel12_OKClick;
                label7.Click += Panel12_OKClick;
                ultraPictureBox9.Click += Panel12_OKClick;

                // Wire up ultraPanel11 (Close button) - with label6 and ultraPictureBox8
                ultraPanel11.Click += Panel11_CloseClick;
                ultraPanel11.ClientArea.Click += Panel11_CloseClick;
                label6.Click += Panel11_CloseClick;
                ultraPictureBox8.Click += Panel11_CloseClick;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error wiring OK/Close click handlers: {ex.Message}");
            }
        }

        /// <summary>
        /// OK button click handler - selects the vendor and closes
        /// </summary>
        private void Panel12_OKClick(object sender, EventArgs e)
        {
            if (isHandlingClick) return;
            isHandlingClick = true;

            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    SetSelectedVendor();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else if (ultraGrid1.Rows != null && ultraGrid1.Rows.Count > 0)
                {
                    // Select first row if none is selected
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    SetSelectedVendor();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Please select a vendor first.", "No Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OK panel click: {ex.Message}");
            }
            finally
            {
                isHandlingClick = false;
            }
        }

        /// <summary>
        /// Close button click handler - closes without selecting
        /// </summary>
        private void Panel11_CloseClick(object sender, EventArgs e)
        {
            if (isHandlingClick) return;
            isHandlingClick = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("Close button clicked in frmVendorDig");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Close panel click: {ex.Message}");
            }
            finally
            {
                isHandlingClick = false;
            }
        }

        // Add resize event handler to adjust column widths when form is resized
        private void frmVendorDig_Resize(object sender, EventArgs e)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Calculate total available width
                int totalWidth = ultraGrid1.Width - ultraGrid1.DisplayLayout.Override.RowSelectorWidth - 20; // 20 for scrollbar and borders

                // Check current column order from comboBox2
                string currentOrder = comboBox2.SelectedItem?.ToString() ?? "Id";

                if (currentOrder == "Id")
                {
                    // Id first, then Vendor Name
                    if (band.Columns.Exists("LedgerID") && !band.Columns["LedgerID"].Hidden)
                    {
                        band.Columns["LedgerID"].Width = (int)(totalWidth * 0.15);
                        band.Columns["LedgerID"].Header.VisiblePosition = 0;
                    }

                    if (band.Columns.Exists("LedgerName") && !band.Columns["LedgerName"].Hidden)
                    {
                        band.Columns["LedgerName"].Width = (int)(totalWidth * 0.85);
                        band.Columns["LedgerName"].Header.VisiblePosition = 1;
                    }
                }
                else // Vendor Name
                {
                    // Vendor Name first, then Id
                    if (band.Columns.Exists("LedgerName") && !band.Columns["LedgerName"].Hidden)
                    {
                        band.Columns["LedgerName"].Width = (int)(totalWidth * 0.85);
                        band.Columns["LedgerName"].Header.VisiblePosition = 0;
                    }

                    if (band.Columns.Exists("LedgerID") && !band.Columns["LedgerID"].Hidden)
                    {
                        band.Columns["LedgerID"].Width = (int)(totalWidth * 0.15);
                        band.Columns["LedgerID"].Header.VisiblePosition = 1;
                    }
                }
            }
        }

        private void ultraPanel10_PaintClient(object sender, PaintEventArgs e)
        {
            // If we get here, the panel exists, so we can handle it safely
            // Empty implementation to allow our styling to take effect
        }

        private void ultraPanel11_PaintClient(object sender, PaintEventArgs e)
        {
            // If we get here, the panel exists, so we can handle it safely
            // Empty implementation to allow our styling to take effect
        }

        private void ultraPanel12_PaintClient(object sender, PaintEventArgs e)
        {
            // If we get here, the panel exists, so we can handle it safely
            // Empty implementation to allow our styling to take effect
        }

        private void ultraPanel4_PaintClient(object sender, PaintEventArgs e)
        {
            // If we get here, the panel exists, so we can handle it safely
            // Empty implementation to allow our styling to take effect
        }

        private void ultraPanel5_PaintClient(object sender, PaintEventArgs e)
        {
            // If we get here, the panel exists, so we can handle it safely
            // Empty implementation to allow our styling to take effect
        }

        private void ConnectPanelClickEvents()
        {
            // Connect click events for panels if they exist
            string[] panelNames = { "ultraPanel10", "ultraPanel11", "ultraPanel12", "ultraPanel4", "ultraPanel5" };

            foreach (string panelName in panelNames)
            {
                if (this.Controls.Find(panelName, true).Length > 0)
                {
                    Infragistics.Win.Misc.UltraPanel panel = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find(panelName, true)[0];

                    // Special handling for navigation panels
                    if (panelName == "ultraPanel4" || panelName == "ultraPanel5")
                    {
                        // Connect navigation panel events with MouseDown for instant response
                        ConnectNavigationPanelEvents(panel, panelName);
                    }
                    else
                    {
                        // Connect panel click events - use HandlePanelClick to avoid multiple event firing
                        panel.Click += (sender, e) => HandlePanelClick(panelName);
                        panel.ClientArea.Click += (sender, e) => HandlePanelClick(panelName);

                        // Connect click events for child controls too - but NOT directly to Panel_Click
                        foreach (Control control in panel.ClientArea.Controls)
                        {
                            if (control is Label || control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                            {
                                // Use a separate method to prevent multiple form loads
                                control.Click += (sender, e) => HandlePanelClick(panelName);
                            }
                        }
                    }
                }
            }

            // Set up hover effects for panels and their child controls
            SetupPanelHoverEffects();
        }

        // Helper method to connect navigation panel events with visual feedback
        private void ConnectNavigationPanelEvents(Infragistics.Win.Misc.UltraPanel panel, string panelName)
        {
            // Store original colors for visual feedback
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;

            // Create brighter colors for feedback
            Color pressedBackColor = BrightenColor(originalBackColor, 50);
            Color pressedBackColor2 = BrightenColor(originalBackColor2, 50);

            // Find the picture box in the panel
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox = null;
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    pictureBox = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                    break;
                }
            }

            // Determine which navigation function to use
            EventHandler navigationHandler = panelName == "ultraPanel4" ?
                new EventHandler(MoveItemHighlighterUp) :
                new EventHandler(MoveItemHighlighterDown);

            // Panel MouseDown - move and provide visual feedback
            panel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Change color for visual feedback
                    panel.Appearance.BackColor = pressedBackColor;
                    panel.Appearance.BackColor2 = pressedBackColor2;

                    // Call the navigation handler
                    navigationHandler(s, e);

                    // Use BeginInvoke to reset color after a short delay
                    panel.BeginInvoke(new Action(() =>
                    {
                        panel.Appearance.BackColor = originalBackColor;
                        panel.Appearance.BackColor2 = originalBackColor2;
                    }));
                }
            };

            // Panel ClientArea MouseDown - same as panel
            panel.ClientArea.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Change color for visual feedback
                    panel.Appearance.BackColor = pressedBackColor;
                    panel.Appearance.BackColor2 = pressedBackColor2;

                    // Call the navigation handler
                    navigationHandler(s, e);

                    // Use BeginInvoke to reset color after a short delay
                    panel.BeginInvoke(new Action(() =>
                    {
                        panel.Appearance.BackColor = originalBackColor;
                        panel.Appearance.BackColor2 = originalBackColor2;
                    }));
                }
            };

            // PictureBox MouseDown - change panel color and call handler
            if (pictureBox != null)
            {
                pictureBox.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        // Change color for visual feedback
                        panel.Appearance.BackColor = pressedBackColor;
                        panel.Appearance.BackColor2 = pressedBackColor2;

                        // Call the navigation handler
                        navigationHandler(s, e);

                        // Use BeginInvoke to reset color after a short delay
                        panel.BeginInvoke(new Action(() =>
                        {
                            panel.Appearance.BackColor = originalBackColor;
                            panel.Appearance.BackColor2 = originalBackColor2;
                        }));
                    }
                };
            }
        }

        // Event handler to move the item highlighter up
        private void MoveItemHighlighterUp(object sender, EventArgs e)
        {
            try
            {
                // Don't use SuspendLayout/ResumeLayout for faster response
                if (ultraGrid1.Rows.Count == 0)
                    return;

                // If no active row, select the last row
                if (ultraGrid1.ActiveRow == null)
                {
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                    return;
                }

                int currentIndex = ultraGrid1.ActiveRow.Index;
                if (currentIndex > 0)
                {
                    // Activate the previous row
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex - 1];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);

                    // Ensure the row is visible
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving highlighter up: {ex.Message}");
            }
        }

        // Event handler to move the item highlighter down
        private void MoveItemHighlighterDown(object sender, EventArgs e)
        {
            try
            {
                // Don't use SuspendLayout/ResumeLayout for faster response
                if (ultraGrid1.Rows.Count == 0)
                    return;

                // If no active row, select the first row
                if (ultraGrid1.ActiveRow == null)
                {
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                    return;
                }

                int currentIndex = ultraGrid1.ActiveRow.Index;
                if (currentIndex < ultraGrid1.Rows.Count - 1)
                {
                    // Activate the next row
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex + 1];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);

                    // Ensure the row is visible
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving highlighter down: {ex.Message}");
            }
        }

        // Helper method to brighten a color
        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        // Initialize comboBox2 with column order options
        private void InitializeColumnOrderComboBox()
        {
            // Clear existing items
            comboBox2.Items.Clear();

            // Add column order options
            comboBox2.Items.Add("Id");
            comboBox2.Items.Add("Vendor Name");

            // Set default selection to Id
            comboBox2.SelectedIndex = 0;

            // Add event handler for selection change
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        // Handle selection change in comboBox2
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reorder columns based on selection
            ReorderColumns(comboBox2.SelectedItem.ToString());
        }

        // Method to reorder columns based on selection
        private void ReorderColumns(string selectedOption)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Calculate total available width
                int totalWidth = ultraGrid1.Width - ultraGrid1.DisplayLayout.Override.RowSelectorWidth - 20; // 20 for scrollbar and borders

                // Reorder columns based on selection
                if (selectedOption == "Id")
                {
                    // Id first, then Vendor Name
                    if (band.Columns.Exists("LedgerID"))
                    {
                        band.Columns["LedgerID"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("LedgerName"))
                    {
                        band.Columns["LedgerName"].Header.VisiblePosition = 1;
                    }
                }
                else if (selectedOption == "Vendor Name")
                {
                    // Vendor Name first, then Id
                    if (band.Columns.Exists("LedgerName"))
                    {
                        band.Columns["LedgerName"].Header.VisiblePosition = 0;
                        band.Columns["LedgerName"].Width = (int)(totalWidth * 0.85); // 85% of available space
                    }
                    if (band.Columns.Exists("LedgerID"))
                    {
                        band.Columns["LedgerID"].Header.VisiblePosition = 1;
                        band.Columns["LedgerID"].Width = (int)(totalWidth * 0.15); // 15% of available space
                    }
                }

                // Force the layout to update
                ultraGrid1.DisplayLayout.Bands[0].Override.AllowColSizing = AllowColSizing.None;
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                ultraGrid1.Refresh();
            }
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Apply changes immediately on Enter key press
            if (e.KeyCode == Keys.Enter)
            {
                // Process the value immediately
                ProcessTextBoxValueImmediate(textBox1.Text);

                // Move focus away to trigger immediate application
                ultraGrid1.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // Handler for textBox1 text change event
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Reset and restart the timer on each keystroke
            inputDebounceTimer.Stop();

            // If the value is the same as last processed, don't reprocess
            if (textBox1.Text == lastProcessedValue)
                return;

            // Start the timer to process after a very short delay
            inputDebounceTimer.Start();
        }

        // Timer tick handler for debounced processing
        private void InputDebounceTimer_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            inputDebounceTimer.Stop();

            // Process the value
            ProcessTextBoxValueImmediate(textBox1.Text);

            // The UpdateRecordCountLabel() will be called in ApplyRecordLimitFast()
        }

        // Process the textBox1 value immediately
        private void ProcessTextBoxValueImmediate(string value)
        {
            // Store the value being processed to avoid reprocessing
            lastProcessedValue = value;

            // Handle empty text case
            if (string.IsNullOrWhiteSpace(value))
            {
                // Show all records if empty
                maxRecordsToDisplay = originalDataTable != null ? originalDataTable.Rows.Count : int.MaxValue;
                ApplyRecordLimitFast();
                return;
            }

            // Try to parse the input as integer
            if (int.TryParse(value, out int recordCount))
            {
                // Ensure the value is at least 1
                if (recordCount > 0)
                {
                    maxRecordsToDisplay = recordCount;
                    ApplyRecordLimitFast();
                }
                else
                {
                    // If zero or negative, set to minimum of 1
                    maxRecordsToDisplay = 1;
                    textBox1.Text = "1";
                    lastProcessedValue = "1";
                    ApplyRecordLimitFast();
                }
            }
            // If not a valid integer, don't update (keep previous value)
        }

        // Optimized version of ApplyRecordLimit for faster response
        private void ApplyRecordLimitFast()
        {
            if (originalDataTable == null || originalDataTable.Rows.Count == 0)
                return;

            try
            {
                // Suspend layout to prevent flickering
                ultraGrid1.BeginUpdate();

                // OPTIMIZATION: Use direct DataView filtering instead of creating new DataTables
                DataView view = new DataView(originalDataTable);

                // Get current sort field from comboBox2
                string sortField = comboBox2.SelectedItem?.ToString() == "Id" ? "LedgerID" : "LedgerName";

                // Set the sort on the view
                view.Sort = sortField;

                // Apply the limit directly to the grid without creating intermediate tables
                if (maxRecordsToDisplay >= originalDataTable.Rows.Count)
                {
                    // Show all records
                    ultraGrid1.DataSource = view;
                }
                else
                {
                    // Create a limited table more efficiently
                    DataTable limitedTable = originalDataTable.Clone();

                    // Take only the rows we need - directly from the view for better performance
                    for (int i = 0; i < maxRecordsToDisplay && i < view.Count; i++)
                    {
                        limitedTable.ImportRow(view[i].Row);
                    }

                    ultraGrid1.DataSource = limitedTable;
                }

                // Restore column widths and positions
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        {
                            col.Width = savedColumnWidths[col.Key];
                        }
                    }
                }

                // Re-apply column styling to ensure consistent appearance
                ConfigureGridLayout();

                // Re-apply column ordering
                if (comboBox2.SelectedItem != null)
                {
                    ReorderColumns(comboBox2.SelectedItem.ToString());
                }

                // Select first visible row if any
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                // Update the record count label
                UpdateRecordCountLabel();

                // Resume layout
                ultraGrid1.EndUpdate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error applying record limit: " + ex.Message);
            }
        }

        // Keep the original method for backward compatibility
        private void ApplyRecordLimit()
        {
            ApplyRecordLimitFast();
        }

        // New method to handle panel clicks with protection against multiple calls
        private void HandlePanelClick(string panelName)
        {
            // Prevent multiple calls
            if (isHandlingClick) return;

            try
            {
                isHandlingClick = true;
                Panel_Click(null, EventArgs.Empty, panelName);
            }
            finally
            {
                isHandlingClick = false;
            }
        }

        private void Panel_Click(object sender, EventArgs e, string panelName)
        {
            // Handle panel clicks based on panel name
            // This provides a central place to handle all panel clicks

            switch (panelName)
            {
                case "ultraPanel10":
                    // Open FrmVendor form and close this form
                    OpenVendorForm();
                    break;

                case "ultraPanel11":
                    // Handle ultraPanel11 click
                    break;

                case "ultraPanel12":
                    // Handle ultraPanel12 click
                    break;

                case "ultraPanel4":
                    // Handle ultraPanel4 click - now handled by ConnectNavigationPanelEvents
                    break;

                case "ultraPanel5":
                    // Handle ultraPanel5 click - now handled by ConnectNavigationPanelEvents
                    break;
            }
        }

        // Modify the OpenVendorForm method to ensure form appears in front
        private void OpenVendorForm()
        {
            try
            {
                // Create and show the FrmVendor form
                PosBranch_Win.Accounts.FrmVendor vendorForm = new PosBranch_Win.Accounts.FrmVendor();

                // Set the form to open maximized
                vendorForm.WindowState = FormWindowState.Maximized;

                // Set the form's position to center screen
                vendorForm.StartPosition = FormStartPosition.CenterScreen;

                // Set TopMost to ensure it appears in front
                vendorForm.TopMost = true;

                // Hide current form
                this.Hide();

                // Show the vendor form
                vendorForm.Show();

                // After a short delay, reset TopMost to false so other windows can go in front if needed
                vendorForm.BeginInvoke(new Action(() =>
                {
                    // Short delay to ensure form is visible on top
                    System.Threading.Thread.Sleep(100);
                    vendorForm.TopMost = false;

                    // Make sure it stays active
                    vendorForm.Activate();
                    vendorForm.BringToFront();
                }));

                // Close this form after ensuring the Vendor form is shown
                this.BeginInvoke(new Action(() =>
                {
                    // Process any pending messages
                    Application.DoEvents();

                    // Close this form
                    this.Close();
                }));
            }
            catch (Exception ex)
            {
                // If there's an error, make sure this form is visible again
                this.Show();

                MessageBox.Show($"Error opening Vendor form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupPanelHoverEffects()
        {
            // Focus specifically on ultraPanel10 and its controls
            if (this.Controls.Find("ultraPanel10", true).Length > 0)
            {
                Infragistics.Win.Misc.UltraPanel panel10 = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel10", true)[0];
                Label label5 = null;
                Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox7 = null;

                // Find label5 and ultraPictureBox7 in panel10's controls
                foreach (Control control in panel10.ClientArea.Controls)
                {
                    if (control is Label && control.Name == "label5")
                    {
                        label5 = (Label)control;
                    }
                    else if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox && control.Name == "ultraPictureBox7")
                    {
                        pictureBox7 = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                    }
                }

                // Apply special hover effects for label5 and ultraPictureBox7
                if (label5 != null && pictureBox7 != null)
                {
                    // Set up label5
                    label5.ForeColor = Color.White; // Ensure label5 has white color
                    label5.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    label5.BackColor = Color.Transparent;
                    label5.Cursor = Cursors.Hand;

                    // Set up pictureBox7
                    pictureBox7.BackColor = Color.Transparent;
                    pictureBox7.BackColorInternal = Color.Transparent;
                    pictureBox7.BorderShadowColor = Color.Transparent;
                    pictureBox7.Cursor = Cursors.Hand;

                    // Store original panel colors
                    Color originalBackColor = panel10.Appearance.BackColor;
                    Color originalBackColor2 = panel10.Appearance.BackColor2;

                    // Define hover colors
                    Color hoverBackColor = Color.FromArgb(160, 230, 255);
                    Color hoverBackColor2 = Color.FromArgb(30, 140, 230);

                    // Add hover effect to label - keep color white on hover (removed yellow)
                    label5.MouseEnter += (sender, e) =>
                    {
                        label5.ForeColor = Color.White; // Keep white instead of yellow
                        panel10.Appearance.BackColor = hoverBackColor;
                        panel10.Appearance.BackColor2 = hoverBackColor2;
                    };

                    label5.MouseLeave += (sender, e) =>
                    {
                        label5.ForeColor = Color.White;
                        if (!IsMouseOverControl(panel10) && !IsMouseOverControl(pictureBox7))
                        {
                            panel10.Appearance.BackColor = originalBackColor;
                            panel10.Appearance.BackColor2 = originalBackColor2;
                        }
                    };

                    // Add hover effect to picture box
                    pictureBox7.MouseEnter += (sender, e) =>
                    {
                        pictureBox7.Appearance.BorderColor = Color.White;
                        panel10.Appearance.BackColor = hoverBackColor;
                        panel10.Appearance.BackColor2 = hoverBackColor2;
                    };

                    pictureBox7.MouseLeave += (sender, e) =>
                    {
                        pictureBox7.Appearance.BorderColor = Color.Transparent;
                        if (!IsMouseOverControl(panel10) && !IsMouseOverControl(label5))
                        {
                            panel10.Appearance.BackColor = originalBackColor;
                            panel10.Appearance.BackColor2 = originalBackColor2;
                        }
                    };

                    // Add hover effect to the panel itself
                    panel10.ClientArea.MouseEnter += (sender, e) =>
                    {
                        panel10.Appearance.BackColor = hoverBackColor;
                        panel10.Appearance.BackColor2 = hoverBackColor2;
                        panel10.ClientArea.Cursor = Cursors.Hand;
                    };

                    panel10.ClientArea.MouseLeave += (sender, e) =>
                    {
                        if (!IsMouseOverControl(label5) && !IsMouseOverControl(pictureBox7))
                        {
                            panel10.Appearance.BackColor = originalBackColor;
                            panel10.Appearance.BackColor2 = originalBackColor2;
                        }
                    };
                }
                else
                {
                    // If we couldn't find the specific controls, use the general method
                    SetupPanelGroupHoverEffects(panel10, label5, pictureBox7);
                }
            }

            // Handle other panels
            string[] otherPanelNames = { "ultraPanel11", "ultraPanel12" }; // Removed ultraPanel4 and ultraPanel5 as they're handled by ConnectNavigationPanelEvents
            foreach (string panelName in otherPanelNames)
            {
                if (this.Controls.Find(panelName, true).Length > 0)
                {
                    Infragistics.Win.Misc.UltraPanel panel = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find(panelName, true)[0];

                    // Find label and picturebox in panel's controls
                    Label panelLabel = null;
                    Infragistics.Win.UltraWinEditors.UltraPictureBox panelPictureBox = null;

                    foreach (Control control in panel.ClientArea.Controls)
                    {
                        if (control is Label)
                        {
                            panelLabel = (Label)control;
                        }
                        else if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                        {
                            panelPictureBox = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                        }
                    }

                    // Set up hover effects for this panel group
                    SetupPanelGroupHoverEffects(panel, panelLabel, panelPictureBox);
                }
            }
        }

        // Helper method to check if mouse is over a control
        private bool IsMouseOverControl(Control control)
        {
            if (control == null) return false;

            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        // Method to set up hover effects for a panel group (panel, label, picturebox)
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

            panel.ClientArea.MouseEnter += (s, e) =>
            {
                applyHoverEffect();
            };

            panel.ClientArea.MouseLeave += (s, e) =>
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

                    // Add white border
                    pictureBox.Appearance.BorderColor = Color.White;
                };

                pictureBox.MouseLeave += (s, e) =>
                {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel) && (label == null || !IsMouseOverControl(label)))
                    {
                        removeHoverEffect();
                    }

                    // Remove border
                    pictureBox.Appearance.BorderColor = Color.Transparent;
                };
            }

            // Add hover effects to the label if provided
            if (label != null)
            {
                // Ensure label has white text color
                label.ForeColor = Color.White;

                label.MouseEnter += (s, e) =>
                {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    label.Cursor = Cursors.Hand;

                    // Keep text color white (removed yellow color change)
                    label.ForeColor = Color.White;
                };

                label.MouseLeave += (s, e) =>
                {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel) && (pictureBox == null || !IsMouseOverControl(pictureBox)))
                    {
                        removeHoverEffect();
                    }

                    // Keep text color white
                    label.ForeColor = Color.White;
                };
            }
        }

        // Add a method to preserve the original row order
        private void PreserveOriginalRowOrder(DataTable table)
        {
            try
            {
                // Check if the table already has a row order column
                if (!table.Columns.Contains("OriginalRowOrder"))
                {
                    // Add a column to track the original row order
                    DataColumn orderColumn = new DataColumn("OriginalRowOrder", typeof(int));
                    table.Columns.Add(orderColumn);

                    // Set the row order values
                    int rowIndex = 0;
                    foreach (DataRow row in table.Rows)
                    {
                        row["OriginalRowOrder"] = rowIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preserving row order: {ex.Message}");
            }
        }

        // Event handler for ultraPictureBox4 click to toggle sorting
        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have data to sort
                if (ultraGrid1.DataSource == null || ultraGrid1.Rows.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No data to sort");
                    return;
                }

                // Toggle between original order and reverse order
                isOriginalOrder = !isOriginalOrder;

                // Suspend layout while sorting
                ultraGrid1.BeginUpdate();

                // Get the current DataView
                if (ultraGrid1.DataSource is DataView dataView)
                {
                    // Check if we have the OriginalRowOrder column
                    if (dataView.Table.Columns.Contains("OriginalRowOrder"))
                    {
                        // Sort by the original row order column
                        if (isOriginalOrder)
                        {
                            dataView.Sort = "OriginalRowOrder ASC";
                        }
                        else
                        {
                            dataView.Sort = "OriginalRowOrder DESC";
                        }
                    }
                    else
                    {
                        // Fallback to sorting by the first visible column if OriginalRowOrder doesn't exist
                        if (isOriginalOrder)
                        {
                            dataView.Sort = "";  // Clear sort to restore original order
                        }
                        else
                        {
                            // Find the first visible column to sort by
                            string sortColumn = "";

                            if (ultraGrid1.DisplayLayout.Bands.Count > 0 && ultraGrid1.DisplayLayout.Bands[0].Columns.Count > 0)
                            {
                                // Find the first visible column
                                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                                {
                                    if (!col.Hidden)
                                    {
                                        sortColumn = col.Key;
                                        break;
                                    }
                                }

                                // If we found a column, sort by it in descending order
                                if (!string.IsNullOrEmpty(sortColumn))
                                {
                                    dataView.Sort = sortColumn + " DESC";
                                }
                            }
                        }
                    }

                    // Force the grid to refresh
                    ultraGrid1.Refresh();
                }
                else if (ultraGrid1.DataSource is DataTable dataTable)
                {
                    // Create a view from the table
                    DataView view = new DataView(dataTable);

                    // Check if we have the OriginalRowOrder column
                    if (dataTable.Columns.Contains("OriginalRowOrder"))
                    {
                        // Sort by the original row order column
                        if (isOriginalOrder)
                        {
                            view.Sort = "OriginalRowOrder ASC";
                        }
                        else
                        {
                            view.Sort = "OriginalRowOrder DESC";
                        }
                    }
                    else
                    {
                        // Fallback to sorting by the first visible column if OriginalRowOrder doesn't exist
                        if (isOriginalOrder)
                        {
                            view.Sort = "";  // Clear sort to restore original order
                        }
                        else
                        {
                            // Find the first visible column to sort by
                            string sortColumn = "";

                            if (ultraGrid1.DisplayLayout.Bands.Count > 0 && ultraGrid1.DisplayLayout.Bands[0].Columns.Count > 0)
                            {
                                // Find the first visible column
                                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                                {
                                    if (!col.Hidden)
                                    {
                                        sortColumn = col.Key;
                                        break;
                                    }
                                }

                                // If we found a column, sort by it in descending order
                                if (!string.IsNullOrEmpty(sortColumn))
                                {
                                    view.Sort = sortColumn + " DESC";
                                }
                            }
                        }
                    }

                    // Set the sorted view as the data source
                    ultraGrid1.DataSource = view;
                }

                // Resume layout
                ultraGrid1.EndUpdate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during sort: {ex.Message}");
            }
        }

        // Setup column drag and drop functionality
        private void SetupColumnDragDrop()
        {
            // Enable the grid as a drop target
            ultraGrid1.AllowDrop = true;

            // Add drag event handlers for headers
            ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            ultraGrid1.DragOver += UltraGrid1_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop;

            // Create column chooser form but don't show it yet
            CreateColumnChooserForm();

            // Add context menu for column chooser
            SetupColumnChooserMenu();
        }

        // Add a method to set up the column chooser right-click menu
        private void SetupColumnChooserMenu()
        {
            // Create a context menu for the grid
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();

            // Add the column chooser menu item
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);

            // Assign the context menu to the grid
            ultraGrid1.ContextMenuStrip = gridContextMenu;
        }

        // Event handler for the column chooser menu item
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        // Create the column chooser form without showing it
        private void CreateColumnChooserForm()
        {
            // Create a new form for the column chooser
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual, // Changed from CenterParent to Manual for positioning
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240), // Light gray background
                ShowIcon = false,
                ShowInTaskbar = false // Don't show in taskbar
            };

            // Add handlers for form events
            columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();

            // Create a listbox to hold hidden columns with blue button styling
            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30,
                IntegralHeight = false // Allow partial items to be visible
            };

            // Custom drawing for the ListBox items to make them look like blue buttons
            columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0) return;

                // Get the column item
                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;

                // Calculate the item rectangle
                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3); // Leave a small gap between buttons

                // Draw the background
                Color bgColor = Color.FromArgb(33, 150, 243); // Bright blue color from image

                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    // Create rounded rectangle
                    using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        int radius = 4; // Corner radius
                        int diameter = radius * 2;
                        Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

                        // Top left corner
                        path.AddArc(arcRect, 180, 90);

                        // Top right corner
                        arcRect.X = rect.Right - diameter;
                        path.AddArc(arcRect, 270, 90);

                        // Bottom right corner
                        arcRect.Y = rect.Bottom - diameter;
                        path.AddArc(arcRect, 0, 90);

                        // Bottom left corner
                        arcRect.X = rect.Left;
                        path.AddArc(arcRect, 90, 90);

                        // Close the path
                        path.CloseFigure();

                        // Fill the rounded rectangle
                        evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        evt.Graphics.FillPath(bgBrush, path);
                    }
                }

                // Draw the text
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }

                // Draw focus rectangle if item is selected
                if ((evt.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    // Use a subtle focus indicator
                    using (Pen focusPen = new Pen(Color.White, 1.5f))
                    {
                        focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        Rectangle focusRect = rect;
                        focusRect.Inflate(-2, -2);
                        evt.Graphics.DrawRectangle(focusPen, focusRect);
                    }
                }
            };

            // Add event handlers for drag and drop operations
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;

            // Add the listbox to the form
            columnChooserForm.Controls.Add(columnChooserListBox);

            // Custom scrollbar styling
            // Unfortunately, .NET doesn't easily allow custom scrollbar styling in a ListBox
            // We would need to P/Invoke or use a 3rd-party control for that
            // Here we just ensure the scrollbar is visible when needed
            columnChooserListBox.ScrollAlwaysVisible = false;

            // Populate the listbox with currently hidden columns
            PopulateColumnChooserListBox();
        }

        // Method to show the column chooser form
        private void ShowColumnChooser()
        {
            // If the column chooser form already exists, just show it
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show();
                columnChooserForm.BringToFront();
                return;
            }

            // Create the form if it doesn't exist
            CreateColumnChooserForm();

            // Populate the listbox with currently hidden columns
            PopulateColumnChooserListBox();

            // Show the form
            columnChooserForm.Show(this);

            // Position the column chooser at the bottom of the screen
            columnChooserForm.Location = new Point(
                this.Location.X + (this.Width - columnChooserForm.Width) / 2,
                this.Location.Y + this.Height - columnChooserForm.Height - 20);
        }

        // Method to populate the column chooser listbox with hidden columns
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;

            // Clear existing items
            columnChooserListBox.Items.Clear();

            // Define the standard columns that should be tracked in the column chooser
            string[] standardColumns = new string[] { "LedgerID", "LedgerName", "VEN_ACCTCODE" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "LedgerID", "ID" },
                { "LedgerName", "Vendor Name" },
                { "VEN_ACCTCODE", "Acct Code" }
            };

            // Track columns we've already added to prevent duplicates
            HashSet<string> addedColumns = new HashSet<string>();

            // Add only standard columns that are currently hidden
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (string colKey in standardColumns)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey) &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns[colKey].Hidden &&
                        !addedColumns.Contains(colKey))
                    {
                        string displayText = displayNames.ContainsKey(colKey) ? displayNames[colKey] : colKey;
                        columnChooserListBox.Items.Add(new ColumnItem(colKey, displayText));
                        addedColumns.Add(colKey);
                    }
                }
            }
        }

        // Helper class to store column information in the listbox
        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }

            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        // Event handler for column chooser form closing
        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up event handlers to prevent memory leaks
            if (columnChooserListBox != null)
            {
                columnChooserListBox.MouseDown -= ColumnChooserListBox_MouseDown;
                columnChooserListBox.DragOver -= ColumnChooserListBox_DragOver;
                columnChooserListBox.DragDrop -= ColumnChooserListBox_DragDrop;
                columnChooserListBox = null;
            }
        }

        // Override form closing to ensure proper cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up the column chooser form if it exists
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }

            // Call the base implementation
            base.OnFormClosing(e);
        }

        // Event handler for mouse down in the column chooser listbox
        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Get the index of the item under the mouse
                int index = columnChooserListBox.IndexFromPoint(e.X, e.Y);
                if (index != ListBox.NoMatches)
                {
                    // Start the drag operation
                    ColumnItem item = (ColumnItem)columnChooserListBox.Items[index];
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        // Event handler for drag over in the column chooser listbox
        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            // Accept headers, columns, or our custom format
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.HeaderUIElement)) ||
                e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)) ||
                e.Data.GetDataPresent("ColumnToRemove") ||
                e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        // Event handler for drop in the column chooser listbox
        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // Check if the dragged data is a column header or column
                Infragistics.Win.UltraWinGrid.UltraGridColumn column = null;

                if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.HeaderUIElement)))
                {
                    Infragistics.Win.UltraWinGrid.HeaderUIElement headerElement = (Infragistics.Win.UltraWinGrid.HeaderUIElement)e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.HeaderUIElement));
                    // Access the column through the correct property of HeaderUIElement
                    if (headerElement.Header != null && headerElement.Header.Column != null)
                    {
                        column = headerElement.Header.Column;
                    }
                }
                else if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
                {
                    column = (Infragistics.Win.UltraWinGrid.UltraGridColumn)e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn));
                }
                else if (e.Data.GetDataPresent("ColumnToRemove"))
                {
                    // Get the column key from our custom format
                    string columnKey = e.Data.GetData("ColumnToRemove") as string;
                    if (!string.IsNullOrEmpty(columnKey) &&
                        ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnKey))
                    {
                        column = ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey];
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.StringFormat))
                {
                    // Try to interpret string data as a column key
                    string columnKey = e.Data.GetData(DataFormats.StringFormat) as string;
                    if (!string.IsNullOrEmpty(columnKey) &&
                        ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnKey))
                    {
                        column = ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey];
                    }
                }

                if (column != null && !column.Hidden)
                {
                    // Hide the column in the grid
                    column.Hidden = true;

                    // Add the column to the listbox
                    string displayText = string.IsNullOrEmpty(column.Header.Caption) ? column.Key : column.Header.Caption;
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, displayText));

                    // Force refresh
                    ultraGrid1.Refresh();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ColumnChooserListBox_DragDrop: {ex.Message}");
            }
        }

        // Event handler for drag over in the grid
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            // Check if the dragged data is a column item from our listbox or a column
            if (e.Data.GetDataPresent(typeof(ColumnItem)) ||
                e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)) ||
                e.Data.GetDataPresent("ColumnToRemove"))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        // Event handler for drop in the grid
        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            // Check if the dragged data is a column item from our listbox
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));

                // Find the column in the grid
                if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                    ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    Infragistics.Win.UltraWinGrid.UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];

                    // Show the column in the grid
                    column.Hidden = false;

                    // Remove the item from the listbox
                    for (int i = 0; i < columnChooserListBox.Items.Count; i++)
                    {
                        ColumnItem listItem = (ColumnItem)columnChooserListBox.Items[i];
                        if (listItem.ColumnKey == item.ColumnKey)
                        {
                            columnChooserListBox.Items.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        // Handle mouse down on the grid to initiate potential drag
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset drag state
                isDraggingColumn = false;
                columnToMove = null;

                // Store the mouse down position
                startPoint = new Point(e.X, e.Y);

                // If we're in the header area, try to determine which column
                if (e.Y < 40) // Assuming header is in the top 40 pixels
                {
                    // Use a simpler approach to find which column was clicked
                    // Calculate horizontal position of each column
                    int xPos = 0;

                    // Account for row selector width if present
                    if (ultraGrid1.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
                    {
                        xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                    }

                    // Find which column contains the x position
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden)
                        {
                            // Check if click is within this column's width
                            if (e.X >= xPos && e.X < xPos + col.Width)
                            {
                                columnToMove = col;
                                isDraggingColumn = true;
                                break;
                            }

                            xPos += col.Width;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse down: " + ex.Message);
                isDraggingColumn = false;
                columnToMove = null;
            }
        }

        // Handle mouse move to initiate drag if needed
        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Only track movement if we're dragging a column
                if (e.Button == MouseButtons.Left && columnToMove != null && isDraggingColumn)
                {
                    // Calculate how far the mouse has moved
                    int deltaX = Math.Abs(e.X - startPoint.X);
                    int deltaY = Math.Abs(e.Y - startPoint.Y);

                    // Only start drag if moved beyond threshold
                    if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                    {
                        // Check if moving primarily downward (column to chooser)
                        bool isDraggingDown = (e.Y > startPoint.Y && deltaY > deltaX);

                        if (isDraggingDown)
                        {
                            // Change cursor to indicate a drag operation
                            ultraGrid1.Cursor = Cursors.No;

                            // Show tooltip with hint
                            string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ?
                                            columnToMove.Header.Caption : columnToMove.Key;
                            if (toolTip != null)
                                toolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");

                            // If dragged downward more than 50 pixels, hide the column
                            if (e.Y - startPoint.Y > 50)
                            {
                                // Hide the column and add to customization
                                HideColumn(columnToMove);

                                // Reset drag state
                                columnToMove = null;
                                isDraggingColumn = false;
                                ultraGrid1.Cursor = Cursors.Default;
                                if (toolTip != null)
                                    toolTip.SetToolTip(ultraGrid1, "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse move: " + ex.Message);
                ultraGrid1.Cursor = Cursors.Default;
                if (toolTip != null)
                    toolTip.SetToolTip(ultraGrid1, "");
            }
        }

        // Method to directly hide a column and add it to the column chooser
        private void HideColumn(UltraGridColumn column)
        {
            try
            {
                if (column != null && !column.Hidden)
                {
                    // Get column name before hiding it
                    string columnName = !string.IsNullOrEmpty(column.Header.Caption) ?
                                       column.Header.Caption : column.Key;

                    // Store the column width before hiding it
                    if (!savedColumnWidths.ContainsKey(column.Key))
                        savedColumnWidths[column.Key] = column.Width;

                    // Temporarily disable layout updates to prevent visual flicker
                    ultraGrid1.SuspendLayout();

                    // Hide the specific column that was dragged
                    column.Hidden = true;

                    // Ensure other columns don't resize by explicitly setting their widths
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        {
                            col.Width = savedColumnWidths[col.Key];
                        }
                    }

                    // Resume layout updates
                    ultraGrid1.ResumeLayout();

                    // Make sure the column chooser exists
                    if (columnChooserForm == null || columnChooserForm.IsDisposed)
                    {
                        CreateColumnChooserForm();
                    }

                    // Add to column chooser listbox without showing it automatically
                    if (columnChooserListBox != null)
                    {
                        // Check if this column is already in the list to prevent duplicates
                        bool alreadyExists = false;
                        foreach (object item in columnChooserListBox.Items)
                        {
                            if (item is ColumnItem columnItem && columnItem.ColumnKey == column.Key)
                            {
                                alreadyExists = true;
                                break;
                            }
                        }

                        // Only add if it doesn't already exist
                        if (!alreadyExists)
                        {
                            columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error hiding column: " + ex.Message);
            }
        }

        // Method to highlight the column chooser during drag operations
        private void HighlightColumnChooserForm()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                try
                {
                    // Save original background color
                    Color originalColor = columnChooserForm.BackColor;

                    // Change background color to highlight
                    columnChooserForm.BackColor = Color.LightBlue;

                    // Flash the form border to draw attention
                    columnChooserForm.FormBorderStyle = FormBorderStyle.FixedDialog;

                    // Use a timer to restore the original appearance after a short delay
                    Timer highlightTimer = new Timer();
                    highlightTimer.Interval = 500; // half second
                    highlightTimer.Tick += (s, e) =>
                    {
                        if (!columnChooserForm.IsDisposed)
                        {
                            columnChooserForm.BackColor = originalColor;
                            columnChooserForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                        }
                        highlightTimer.Stop();
                        highlightTimer.Dispose();
                    };
                    highlightTimer.Start();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error highlighting form: {ex.Message}");
                }
            }
        }

        // Update the UltraGrid1_MouseUp method
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset cursor
                ultraGrid1.Cursor = Cursors.Default;
                if (toolTip != null)
                    toolTip.SetToolTip(ultraGrid1, "");

                // Reset drag state
                isDraggingColumn = false;
                columnToMove = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse up: " + ex.Message);
            }
        }

        // Initialize comboBox1 with search filter options
        private void InitializeSearchFilterComboBox()
        {
            // Clear existing items
            comboBox1.Items.Clear();

            // Add search options
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Id");
            comboBox1.Items.Add("Vendor name");

            // Set default selection
            comboBox1.SelectedIndex = 0;

            // Add event handler for selection change
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Handle selection change in comboBox1
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reapply search with current filter if there's text in the search box
            if (!string.IsNullOrWhiteSpace(Searchbx.Text))
            {
                Searchbx_TextChanged(Searchbx, EventArgs.Empty);
            }
        }

        private void Searchbx_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = Searchbx.Text.ToLower();

                if (originalDataTable == null)
                    return;

                // Store the current column widths and positions before changing the data source
                Dictionary<string, int> columnWidths = new Dictionary<string, int>();
                Dictionary<string, int> columnPositions = new Dictionary<string, int>();

                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden)
                        {
                            columnWidths[col.Key] = col.Width;
                            columnPositions[col.Key] = col.Header.VisiblePosition;
                        }
                    }
                }

                // Create a new DataTable with the same schema
                DataTable filteredDataTable = originalDataTable.Clone();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // If search box is empty, reapply the record limit to show the default view
                    ApplyRecordLimit();
                    return;
                }

                // Get the selected search filter option
                string filterOption = comboBox1.SelectedItem?.ToString() ?? "Select all";

                // Build the filter function based on selected search field
                Func<DataRow, bool> filterFunc = row =>
                {
                    switch (filterOption)
                    {
                        case "Id":
                            // Search only in LedgerID column
                            return row["LedgerID"].ToString().ToLower().Contains(searchText);

                        case "Vendor name":
                            // Search only in LedgerName column
                            return row["LedgerName"].ToString().ToLower().Contains(searchText);

                        case "Select all":
                        default:
                            // Search in all relevant columns
                            return row["LedgerID"].ToString().ToLower().Contains(searchText) ||
                                   row["LedgerName"].ToString().ToLower().Contains(searchText);
                    }
                };

                // Apply the filter to get matching rows
                var matchingRows = originalDataTable.AsEnumerable().Where(filterFunc).ToList();

                // Get current sort field from comboBox2
                string sortField = comboBox2.SelectedItem?.ToString() == "Id" ? "LedgerID" : "LedgerName";

                // Sort the matching rows based on the current sort field
                var sortedMatchingRows = sortField == "LedgerID"
                    ? matchingRows.OrderBy(r => Convert.ToInt32(r[sortField])).ToList()
                    : matchingRows.OrderBy(r => r[sortField].ToString()).ToList();

                // Take the first maxRecordsToDisplay rows from the sorted result
                var limitedRows = sortedMatchingRows.Take(maxRecordsToDisplay);

                // Add the filtered rows to the new DataTable
                foreach (var row in limitedRows)
                {
                    filteredDataTable.ImportRow(row);
                }

                // Set the filtered DataTable as the DataSource
                ultraGrid1.DataSource = filteredDataTable;

                // Restore column widths and positions
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden && columnWidths.ContainsKey(col.Key))
                        {
                            col.Width = columnWidths[col.Key];
                            if (columnPositions.ContainsKey(col.Key))
                            {
                                col.Header.VisiblePosition = columnPositions[col.Key];
                            }
                        }
                    }
                }

                // Re-apply column styling to ensure consistent appearance
                ConfigureGridLayout();

                // Re-apply column ordering
                if (comboBox2.SelectedItem != null)
                {
                    ReorderColumns(comboBox2.SelectedItem.ToString());
                }

                // Select first visible row if any
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                // Update the record count label
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while searching: " + ex.Message);
                // If error occurs, clear the filter and reapply record limit
                ApplyRecordLimit();
            }
        }

        private void Searchbx_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
            }
        }

        // Add the PositionColumnChooserAtBottomRight method
        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                // Calculate position at bottom right with a small margin
                columnChooserForm.Location = new Point(
                    this.Right - columnChooserForm.Width - 20,
                    this.Bottom - columnChooserForm.Height - 20);

                // Ensure the form stays on top
                columnChooserForm.TopMost = true;

                // Bring to front to ensure it's visible
                columnChooserForm.BringToFront();
            }
        }

        // Add a method to update the record count label
        private void UpdateRecordCountLabel()
        {
            if (originalDataTable != null)
            {
                int currentDisplayCount = 0;
                if (ultraGrid1.DataSource is DataTable dt)
                {
                    currentDisplayCount = dt.Rows.Count;
                }
                else if (ultraGrid1.DataSource is DataView dv)
                {
                    currentDisplayCount = dv.Count;
                }
                else if (ultraGrid1.Rows != null)
                {
                    currentDisplayCount = ultraGrid1.Rows.Count;
                }

                int totalCount = originalDataTable.Rows.Count;

                // Get the current limit from textBox1
                int currentLimit = maxRecordsToDisplay;
                if (!string.IsNullOrWhiteSpace(textBox1.Text) && int.TryParse(textBox1.Text, out int limit))
                {
                    currentLimit = limit;
                }

                // Update label2 with the current textBox1 value and total count
                if (this.Controls.Find("label2", true).Length > 0)
                {
                    Label label2 = this.Controls.Find("label2", true)[0] as Label;
                    if (label2 != null)
                    {
                        label2.Text = $"Showing {currentLimit} of {totalCount} records";
                    }
                }
            }
        }
    }
}
