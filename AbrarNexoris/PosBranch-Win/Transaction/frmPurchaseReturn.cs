using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win;
using Infragistics.Win.Misc;
using ModelClass;
using ModelClass.TransactionModels;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using Repository.SettingsRepo;
using System.Drawing.Drawing2D;

namespace PosBranch_Win.Transaction
{
    public partial class frmPurchaseReturn : Form
    {
        // Add repository instance at class level
        private Repository.TransactionRepository.PurchaseReturnRepository prRepo = new Repository.TransactionRepository.PurchaseReturnRepository();

        // Add Dropdowns instance to access branch data
        private Dropdowns drop = new Dropdowns();

        // Flag to track if purchase items are loaded from btnAddPurchaceList
        private bool _isPurchaseDataLoaded = false;

        // Flag to track original ReadOnly state of TxtBarcode
        private bool _originalTxtBarcodeReadOnly = false;

        // Track the currently loaded purchase number to prevent duplicates
        private int _currentlyLoadedPurchaseNo = -1;

        // Column chooser, drag-to-hide, and footer cell synchronization
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        private readonly Dictionary<string, Label> footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Add a class-level variable for the quantity column name
        private string _quantityColumnName = "Quantity";

        // In the class, add a new field to control payment method reset
        private bool skipPaymentMethodReset = false;

        // Class-level variable to store original total amount of loaded bill (GRN) for real-time outstanding calculation
        private decimal _originalBillTotal = 0m;

        // Vendor suggestion popup list for ultraTextEditor2
        private ListBox _lstVendorSuggestions = null;
        private bool _isSettingVendorInfo = false;

        // Navigation state: list of PR numbers for |◄ ◄ ► ►| buttons
        private List<string> _navPRNumbers = new List<string>();
        private int _navCurrentIndex = -1;

        public frmPurchaseReturn()
        {
            InitializeComponent();
            HidePaymentMethodControls();

            // Set KeyPreview to true to enable form-level key handling
            this.KeyPreview = true;

            // Load branch data when form initializes
            LoadBranchData();

            // Add event handler for cmbBranch click event
            cmbBranch.Click += new EventHandler(cmbBranch_Click);

            // Add KeyDown event handler for TxtBarcode
            TxtBarcode.KeyDown += new KeyEventHandler(TxtBarcode_KeyDown);

            // Add form load event handler
            this.Load += new EventHandler(frmPurchaseReturn_Load);

            // Initialize the grid tag
            ultraGrid1.Tag = "NormalMode";

            // Add form-level KeyDown event handler for custom tab navigation
            this.KeyDown += new KeyEventHandler(frmPurchaseReturn_KeyDown);

            // Initialize navigation panels (ultraPanel11, ultraPanel4, ultraPanel6, ultraPanel5) as interactive buttons
            InitializePanelButtons();
        }

        private void RegisterEventHandlers()
        {
            try
            {
                // Register the BeforeCellUpdate event to manage cell update behavior
                ultraGrid1.BeforeCellUpdate += ultraGrid1_BeforeCellUpdate;

                // Register the KeyDown event to manage keypress behaviors in the grid
                ultraGrid1.KeyDown += ultraGrid1_KeyDown;

                // Register the AfterCellUpdate event to handle updates after a cell is changed
                ultraGrid1.AfterCellUpdate += ultraGrid1_AfterCellUpdate;

                // Register the InitializeLayout event to customize the grid appearance
                ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;

                // Register the CellListSelect event for handling dropdown selection
                ultraGrid1.CellListSelect += UltraGrid1_CellListSelect;

                // Register MouseDown for showing the dropdown on click
                ultraGrid1.MouseDown += UltraGrid1_MouseDown;

                // Register AfterCellActivate to handle showing dropdown when cell is activated
                ultraGrid1.AfterCellActivate += UltraGrid1_AfterCellActivate;

                // Register BeforeExitEditMode to track when a cell's value is modified
                ultraGrid1.BeforeExitEditMode += ultraGrid1_BeforeExitEditMode; // Add this line

                // Register KeyDown for textBox1 to load items on Enter
                textBox1.KeyDown += textBox1_KeyDown;

                // Register KeyDown for VendorName to load vendor by ID on Enter
                VendorName.KeyDown += VendorName_KeyDown;

                // Register KeyDown for ultraTextEditor2 to load vendor by Name on Enter and show suggestions on ValueChanged
                if (ultraTextEditor2 != null)
                {
                    ultraTextEditor2.KeyDown += ultraTextEditor2_KeyDown;
                    ultraTextEditor2.ValueChanged += ultraTextEditor2_ValueChanged;
                }

                // Register CheckedChanged for chkGRN and chkWithoutGRN
                if (chkGRN != null)
                {
                    chkGRN.CheckedChanged += chkGRN_CheckedChanged;
                }
                if (chkWithoutGRN != null)
                {
                    chkWithoutGRN.CheckedChanged += chkWithoutGRN_CheckedChanged;
                }

                // Register the row selection event for custom row highlighting
                ultraGrid1.AfterSelectChange += UltraGrid1_AfterSelectChange;

                // Register footer cell sync and column drag-to-hide handlers matching gridReport in frmvendorpurchasereport
                ultraGrid1.Resize += (s, e) => UpdateFooterCellPositions();
                ultraGrid1.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
                ultraGrid1.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
                ultraGrid1.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
                ultraGrid1.Paint += (s, e) => UpdateFooterCellPositions();

                SetupHeaderDragToHideAndColumnChooser();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error registering event handlers: " + ex.Message);
            }
        }

        private void HidePaymentMethodControls()
        {
            if (cmbPaymntMethod != null)
            {
                cmbPaymntMethod.Visible = false;
                cmbPaymntMethod.Enabled = false;
                cmbPaymntMethod.TabStop = false;
            }

           
            {

                {
                    {}
                    {
                      
                    }
                }
            }
        }

        private static string FormatPurchaseNoForDisplay(int purchaseNo)
        {
            return purchaseNo > 0 ? $"GRN-{purchaseNo}" : string.Empty;
        }

        private static string NormalizePurchaseNoText(string purchaseText)
        {
            if (string.IsNullOrWhiteSpace(purchaseText))
            {
                return string.Empty;
            }

            string trimmed = purchaseText.Trim();
            if (trimmed.Equals("WITHOUT GR", StringComparison.OrdinalIgnoreCase))
            {
                return "WITHOUT GR";
            }

            string digits = new string(trimmed.Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digits) ? trimmed : digits;
        }

        private bool TryGetPurchaseNoFromTextBox(out int purchaseNo)
        {
            return int.TryParse(NormalizePurchaseNoText(textBox1.Text), out purchaseNo);
        }

        private string GetPurchaseReferenceForSave()
        {
            string normalized = NormalizePurchaseNoText(textBox1.Text);
            if (normalized.Equals("WITHOUT GR", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            return int.TryParse(normalized, out int purchaseNo)
                ? FormatPurchaseNoForDisplay(purchaseNo)
                : (textBox1.Text ?? string.Empty).Trim();
        }

        private void ApplyHiddenPaymentDefaults(PReturnMaster target, PReturnMaster existing = null)
        {
            if (target == null)
            {
                return;
            }

            if (existing != null && existing.PaymodeID > 0)
            {
                target.PaymodeID = existing.PaymodeID;
                target.Paymode = existing.Paymode ?? string.Empty;
                target.PaymodeLedgerID = existing.PaymodeLedgerID;
                return;
            }

            PaymodeDDl hiddenPaymode = GetHiddenDefaultPaymode();
            target.PaymodeID = hiddenPaymode.PayModeID;
            target.Paymode = hiddenPaymode.PayModeName ?? string.Empty;
            target.PaymodeLedgerID = 0;
        }

        private PaymodeDDl GetHiddenDefaultPaymode()
        {
            try
            {
                PaymodeDDlGrid grid = drop.GetPaymode();
                var paymodes = grid?.List?.Where(p => p != null && p.PayModeID > 0).ToList();
                if (paymodes != null && paymodes.Count > 0)
                {
                    var preferred = paymodes.FirstOrDefault(p =>
                        string.Equals(p.PayModeName, "Credit", StringComparison.OrdinalIgnoreCase))
                        ?? paymodes.FirstOrDefault(p =>
                            string.Equals(p.PayModeName, "Cash", StringComparison.OrdinalIgnoreCase))
                        ?? paymodes.First();

                    return preferred;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to load hidden purchase return paymode: {ex.Message}");
            }

            return new PaymodeDDl { PayModeID = 1, PayModeName = "Credit" };
        }

        private void frmPurchaseReturn_Load(object sender, EventArgs e)
        {
            try
            {
                // Set up grid and panel docking
                SetupGridDocking();
                ResizeGridColumns();

                // Load branch data
                LoadBranchData();

                // Generate branch-scoped PR number via stored procedure
                GeneratePurchaseReturnNumber();

                // Make the PR number field read-only
                TxtSRNO.ReadOnly = true;

                // Configure items grid with initial columns
                FormatGrid();

                // Set some inputs to begin a new purchase return
                ultraDateTimeEditor1.Value = DateTime.Now;

                // Initialize the _originalTxtBarcodeReadOnly to false
                _originalTxtBarcodeReadOnly = false;
                TxtBarcode.ReadOnly = false;

                // Ensure KeyPreview is enabled for form-level key handling
                this.KeyPreview = true;

                // Set default placeholder text for vendor
                VendorName.Text = "";

                // Ensure branch data is loaded when form opens
                if (cmbBranch.Items.Count == 0)
                {
                    LoadBranchData();
                }

                // Register event handlers
                RegisterEventHandlers();

                // Ensure grid panel is docked properly
                SetupGridDocking();

                // Configure the grid layout
                ConfigureItemsGridLayout();

                // Set explicit tab indices for our target controls
                Vendorbutton.TabIndex = 1;
                ultraDateTimeEditor1.TabIndex = 2;
                TxtBarcode.TabIndex = 3;
                btnAddPurchaceList.TabIndex = 4;
                button1.TabIndex = 5;

                // Disable tab stop for all controls on the form except our target controls
                DisableTabStopForAllExcept(new Control[] { Vendorbutton, ultraDateTimeEditor1, TxtBarcode, btnAddPurchaceList, button1 });

                // Set the initial focus to the Vendorbutton
                this.ActiveControl = Vendorbutton;

                HidePaymentMethodControls();

                // Apply modern Image 2 light ice-blue theme
                ApplyImage2Theme();

                // Apply custom styles for VendorName, ultraTextEditor2, ultraTextEditor1, TxtSubTotal
                ApplyCustomControlStyles();

                // Style date editors to match ActivityLog dtpFrom look
                StyleDateEditors();

                // Add resize event handler to ensure grid fills available space
                this.Resize += frmPurchaseReturn_Resize;

                // Force an initial resize to set column proportions
                ResizeGridColumns();

                // Initialize GRN vs Without GRN mode checkboxes
                InitializeGRNModeCheckBoxes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in form load: " + ex.Message);
            }
        }

        private bool _isUpdatingModeCheckBoxes = false;

        private void StyleDateEditors()
        {
            StyleDateEditor(ultraDateTimeEditor1);
            StyleDateEditor(ultraDateTimeEditor2);
        }

        private void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            editor.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.BorderColor = Color.FromArgb(128, 183, 220);
            editor.Appearance.ForeColor = Color.FromArgb(20, 55, 120);
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 8.5F;
            editor.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Office2003ToolbarButton;
            editor.DropDownButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            editor.FormatString = "dd MMM yyyy";
            editor.MaskInput = "{date}";
        }

        private void InitializeGRNModeCheckBoxes()
        {
            try
            {
                if (chkGRN == null || chkWithoutGRN == null) return;

                chkGRN.CheckedChanged -= chkGRN_CheckedChanged;
                chkWithoutGRN.CheckedChanged -= chkWithoutGRN_CheckedChanged;

                chkGRN.CheckedChanged += chkGRN_CheckedChanged;
                chkWithoutGRN.CheckedChanged += chkWithoutGRN_CheckedChanged;

                ApplyGRNModeState(chkGRN.Checked);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error initializing GRN mode check boxes: " + ex.Message);
            }
        }

        private void chkGRN_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingModeCheckBoxes) return;
            try
            {
                _isUpdatingModeCheckBoxes = true;
                if (chkGRN.Checked)
                {
                    chkWithoutGRN.Checked = false;
                    ApplyGRNModeState(isGRNMode: true);
                }
                else if (!chkWithoutGRN.Checked)
                {
                    chkGRN.Checked = true;
                }
            }
            finally
            {
                _isUpdatingModeCheckBoxes = false;
            }
        }

        private void chkWithoutGRN_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingModeCheckBoxes) return;
            try
            {
                _isUpdatingModeCheckBoxes = true;
                if (chkWithoutGRN.Checked)
                {
                    chkGRN.Checked = false;
                    ApplyGRNModeState(isGRNMode: false);
                }
                else if (!chkGRN.Checked)
                {
                    chkWithoutGRN.Checked = true;
                }
            }
            finally
            {
                _isUpdatingModeCheckBoxes = false;
            }
        }

        private void ApplyGRNModeState(bool isGRNMode)
        {
            try
            {
                if (isGRNMode)
                {
                    // ── GRN MODE ────────────────────────────────────────────────
                    // Show: textBox1, btnAddPurchaceList, ultraTextEditor1, lblPRamount, lblPno, ultraTextEditor4, label1
                    if (textBox1 != null) { textBox1.Visible = true; textBox1.Enabled = true; }
                    if (btnAddPurchaceList != null) { btnAddPurchaceList.Visible = true; btnAddPurchaceList.Enabled = true; }
                    if (ultraTextEditor1 != null) { ultraTextEditor1.Visible = true; ultraTextEditor1.Enabled = true; }
                    if (ultraTextEditor4 != null) { ultraTextEditor4.Visible = true; ultraTextEditor4.Enabled = true; }
                    if (label1 != null) label1.Visible = true;
                    if (lblPRamount != null) lblPRamount.Visible = true;  // Purchase Amt label
                    if (lblPno != null) lblPno.Visible = true;            // Purchase No label
                    // Also restore Outstanding row, action buttons and panels in GRN mode
                    if (label2 != null) { label2.Visible = true; label2.Enabled = true; }
                    if (textBox2 != null) { textBox2.Visible = true; textBox2.Enabled = true; }
                    if (btnReturn != null) { btnReturn.Visible = true; btnReturn.Enabled = true; }
                    if (btnReturnAll != null) { btnReturnAll.Visible = true; btnReturnAll.Enabled = true; }
                    if (ultraPanel11 != null) ultraPanel11.Visible = true;
                    if (ultraPanel5 != null) ultraPanel5.Visible = true;
                    if (ultraPanel6 != null) ultraPanel6.Visible = true;
                    if (ultraPanel4 != null) ultraPanel4.Visible = true;
                    if (ultraPictureBox13 != null) ultraPictureBox13.Visible = true;
                    if (ultraPictureBox3 != null) ultraPictureBox3.Visible = true;
                    if (ultraPictureBox10 != null) ultraPictureBox10.Visible = true;
                    if (ultraPictureBox14 != null) ultraPictureBox14.Visible = true;

                    // Hide: ultraTextEditor3, button2 (Master reason row), TxtBarcode, BtnDial, lblBarcode, label3
                    if (ultraTextEditor3 != null) { ultraTextEditor3.Text = ""; ultraTextEditor3.Enabled = false; ultraTextEditor3.Visible = false; }
                    if (button2 != null) { button2.Enabled = false; button2.Visible = false; }
                    if (label3 != null) label3.Visible = false;           // Master reason label
                    if (TxtBarcode != null) { TxtBarcode.Text = ""; TxtBarcode.ReadOnly = true; TxtBarcode.Enabled = false; TxtBarcode.Visible = false; }
                    if (BtnDial != null) { BtnDial.Enabled = false; BtnDial.Visible = false; }
                    if (lblBarcode != null) lblBarcode.Visible = false;
                }
                else
                {
                    // ── WITHOUT GRN MODE ────────────────────────────────────────
                    // Show: ultraTextEditor3, button2, label3 (Master reason), TxtBarcode, BtnDial, lblBarcode
                    if (ultraTextEditor3 != null) { ultraTextEditor3.Visible = true; ultraTextEditor3.Enabled = true; }
                    if (button2 != null) { button2.Visible = true; button2.Enabled = true; }
                    if (label3 != null) label3.Visible = true;            // Master reason label
                    if (TxtBarcode != null) { TxtBarcode.Enabled = true; TxtBarcode.ReadOnly = false; TxtBarcode.Visible = true; }
                    if (BtnDial != null) { BtnDial.Enabled = true; BtnDial.Visible = true; }
                    if (lblBarcode != null) lblBarcode.Visible = true;

                    // Hide: textBox1, btnAddPurchaceList, ultraTextEditor1, lblPRamount, lblPno, ultraTextEditor4, label1
                    if (textBox1 != null) { textBox1.Text = ""; textBox1.Enabled = false; textBox1.Visible = false; }
                    if (btnAddPurchaceList != null) { btnAddPurchaceList.Enabled = false; btnAddPurchaceList.Visible = false; }
                    if (ultraTextEditor1 != null) { ultraTextEditor1.Text = ""; ultraTextEditor1.Enabled = false; ultraTextEditor1.Visible = false; }
                    if (ultraTextEditor4 != null) { ultraTextEditor4.Text = ""; ultraTextEditor4.Enabled = false; ultraTextEditor4.Visible = false; }
                    if (label1 != null) label1.Visible = false;
                    if (lblPRamount != null) lblPRamount.Visible = false; // Purchase Amt label
                    if (lblPno != null) lblPno.Visible = false;           // Purchase No label
                    // Also hide Outstanding row, action buttons and panels in Without GRN mode
                    if (label2 != null) { label2.Visible = false; label2.Enabled = false; }
                    if (textBox2 != null) { textBox2.Visible = false; textBox2.Enabled = false; }
                    if (btnReturn != null) { btnReturn.Visible = false; btnReturn.Enabled = false; }
                    if (btnReturnAll != null) { btnReturnAll.Visible = false; btnReturnAll.Enabled = false; }
                    if (ultraPanel11 != null) ultraPanel11.Visible = false;
                    if (ultraPanel5 != null) ultraPanel5.Visible = false;
                    if (ultraPanel6 != null) ultraPanel6.Visible = false;
                    if (ultraPanel4 != null) ultraPanel4.Visible = false;
                    if (ultraPictureBox13 != null) ultraPictureBox13.Visible = false;
                    if (ultraPictureBox3 != null) ultraPictureBox3.Visible = false;
                    if (ultraPictureBox10 != null) ultraPictureBox10.Visible = false;
                    if (ultraPictureBox14 != null) ultraPictureBox14.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error applying GRN mode state: " + ex.Message);
            }
        }

        private void ApplyImage2Theme()
        {
            try
            {
                Color mainBgColor = Color.FromArgb(220, 235, 252);
                Color panelBgColor = Color.FromArgb(220, 235, 252);
                Color panelBorderColor = Color.FromArgb(170, 205, 245);
                Color darkTextColor = Color.FromArgb(0, 35, 80);
                Color inputBgColor = Color.White;
                Color inputBorderColor = Color.FromArgb(170, 205, 245);
                Color headerGradientStart = Color.FromArgb(170, 205, 245);
                Color headerGradientEnd = Color.FromArgb(200, 225, 252);

                Font controlFont = new Font("Segoe UI", 9.75F, FontStyle.Regular);
                Font labelFont = new Font("Segoe UI", 9.75F, FontStyle.Regular);

                // 1. Form & Main Container Appearance
                this.BackColor = mainBgColor;
                if (ultraPanel1 != null)
                {
                    ultraPanel1.Appearance.BackColor = mainBgColor;
                    ultraPanel1.Appearance.BackColor2 = Color.FromArgb(205, 228, 250);
                    ultraPanel1.Appearance.BackGradientStyle = GradientStyle.Vertical;
                }

                // 2. Section Panels
                UltraPanel[] panels = new UltraPanel[] { ultraPanel3 };
                foreach (UltraPanel pnl in panels)
                {
                    if (pnl != null)
                    {
                        pnl.Appearance.BackColor = panelBgColor;
                        pnl.Appearance.BackColor2 = Color.FromArgb(210, 230, 250);
                        pnl.Appearance.BackGradientStyle = GradientStyle.Vertical;
                        pnl.Appearance.BorderColor = panelBorderColor;
                        pnl.BorderStyle = UIElementBorderStyle.Solid;
                    }
                }

                // 3. Child Controls Theme (Labels, UltraTextEditors, UltraComboEditors, Buttons)
                ApplyThemeToChildControls(this.Controls, darkTextColor, inputBgColor, inputBorderColor, controlFont, labelFont);

                // 4. Infragistics UltraGrid Theme
                if (ultraGrid1 != null && ultraGrid1.DisplayLayout != null)
                {
                    var layout = ultraGrid1.DisplayLayout;
                    layout.Appearance.BackColor = Color.White;
                    layout.Appearance.BorderColor = panelBorderColor;
                    layout.BorderStyle = UIElementBorderStyle.Solid;

                    layout.Override.HeaderStyle = HeaderStyle.Standard;
                    layout.Override.HeaderAppearance.BackColor = Color.FromArgb(93, 151, 214);
                    layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(67, 118, 184);
                    layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                    layout.Override.HeaderAppearance.ForeColor = Color.White;
                    layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
                    layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                    layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
                    layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
                    layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                    layout.Override.RowAppearance.BackColor = Color.White;
                    layout.Override.RowAppearance.ForeColor = darkTextColor;
                    layout.Override.RowAppearance.FontData.Name = "Segoe UI";
                    layout.Override.RowAppearance.FontData.SizeInPoints = 9;
                    layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 250, 255);
                    layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(185, 218, 250);
                    layout.Override.ActiveRowAppearance.ForeColor = darkTextColor;
                    layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(185, 218, 250);
                    layout.Override.SelectedRowAppearance.ForeColor = darkTextColor;

                    layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                    layout.Override.CellAppearance.BorderColor = Color.FromArgb(210, 232, 252);
                }

                // 5. Net Amount / Bottom Box (Restored to original Maroon LED gradient box style)
                if (ultraPanel10 != null)
                {
                    ultraPanel10.Appearance.BackColor = Color.Maroon;
                    ultraPanel10.Appearance.BackColor2 = Color.FromArgb(255, 128, 0);
                    ultraPanel10.Appearance.BackGradientStyle = GradientStyle.GlassTop50;
                    ultraPanel10.Appearance.BorderColor = Color.White;
                    ultraPanel10.BorderStyle = UIElementBorderStyle.Etched;
                    ultraPanel10.ForeColor = Color.Beige;
                    ultraPanel10.UseFlatMode = DefaultableBoolean.True;
                }

                if (lblNetAmount != null)
                {
                    lblNetAmount.Font = new Font("DS-Digital", 36F, FontStyle.Regular);
                    lblNetAmount.ForeColor = Color.Beige;
                    lblNetAmount.BackColor = Color.Transparent;
                }

                // Ensure custom control styling is reapplied after main theme
                ApplyCustomControlStyles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying Image2 theme: {ex.Message}");
            }
        }

        private void ApplyThemeToChildControls(Control.ControlCollection controls, Color darkTextColor, Color inputBgColor, Color inputBorderColor, Font controlFont, Font labelFont)
        {
            foreach (Control c in controls)
            {
                Label lbl = c as Label;
                if (lbl != null && lbl != lblNetAmount)
                {
                    lbl.ForeColor = darkTextColor;
                    lbl.Font = labelFont;
                }

                UltraLabel ulbl = c as UltraLabel;
                if (ulbl != null)
                {
                    ulbl.Appearance.ForeColor = darkTextColor;
                    ulbl.Font = labelFont;
                }

                Infragistics.Win.UltraWinEditors.UltraTextEditor utxt = c as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (utxt != null)
                {
                    utxt.Font = controlFont;
                    utxt.ForeColor = darkTextColor;
                    utxt.Appearance.BackColor = inputBgColor;
                    utxt.Appearance.ForeColor = darkTextColor;
                    utxt.Appearance.BorderColor = inputBorderColor;
                    utxt.BorderStyle = UIElementBorderStyle.Solid;
                    utxt.UseOsThemes = DefaultableBoolean.False;
                }

                Infragistics.Win.UltraWinEditors.UltraComboEditor ucmb = c as Infragistics.Win.UltraWinEditors.UltraComboEditor;
                if (ucmb != null)
                {
                    ucmb.Font = controlFont;
                    ucmb.ForeColor = darkTextColor;
                    ucmb.Appearance.BackColor = inputBgColor;
                    ucmb.Appearance.ForeColor = darkTextColor;
                    ucmb.Appearance.BorderColor = inputBorderColor;
                    ucmb.BorderStyle = UIElementBorderStyle.Solid;
                    ucmb.UseOsThemes = DefaultableBoolean.False;
                }

                Infragistics.Win.UltraWinEditors.UltraDateTimeEditor udte = c as Infragistics.Win.UltraWinEditors.UltraDateTimeEditor;
                if (udte != null)
                {
                    udte.Font = controlFont;
                    udte.ForeColor = darkTextColor;
                    udte.Appearance.BackColor = inputBgColor;
                    udte.Appearance.ForeColor = darkTextColor;
                    udte.Appearance.BorderColor = inputBorderColor;
                    udte.BorderStyle = UIElementBorderStyle.Solid;
                    udte.UseOsThemes = DefaultableBoolean.False;
                }

                TextBox txt = c as TextBox;
                if (txt != null && utxt == null)
                {
                    txt.Font = controlFont;
                    txt.BackColor = inputBgColor;
                    txt.ForeColor = darkTextColor;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }

                ComboBox cmb = c as ComboBox;
                if (cmb != null && ucmb == null)
                {
                    cmb.Font = controlFont;
                    cmb.BackColor = inputBgColor;
                    cmb.ForeColor = darkTextColor;
                    cmb.FlatStyle = FlatStyle.Flat;
                }

                Button btn = c as Button;
                if (btn != null)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(62, 104, 166);
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 197, 230);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(126, 166, 214);
                    btn.BackColor = Color.FromArgb(155, 188, 224);
                    btn.ForeColor = Color.FromArgb(14, 47, 108);

                    if (btn.BackgroundImage == null)
                    {
                        btn.Font = new Font("Tahoma", btn.Font.SizeInPoints > 9F ? btn.Font.SizeInPoints : 8.25F, FontStyle.Bold);
                    }
                }

                if (c.HasChildren)
                {
                    ApplyThemeToChildControls(c.Controls, darkTextColor, inputBgColor, inputBorderColor, controlFont, labelFont);
                }
            }
        }

        // Generates the next purchase return number scoped to the current branch and financial year.
        // Delegates entirely to the stored procedure _POS_PurchaseReturn (GENERATENUMBER)
        // which reads from TrackTrans WHERE BranchID = @BranchId AND FinYearID = @FinYearId.
        private void DirectlyFixTrackTransTable()
        {
            // Kept for backward compatibility — simply calls the proper generator.
            GeneratePurchaseReturnNumber();
        }

        private void ApplyUnifiedGridTheme(UltraGrid grid)
        {
            if (grid == null) return;

            grid.UseAppStyling = false;
            grid.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = grid.DisplayLayout;

            // Background matching Image 3 empty grid space
            layout.Appearance.BackColor = Color.FromArgb(232, 246, 255);
            layout.Appearance.BackColor2 = Color.FromArgb(232, 246, 255);
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Appearance.BorderColor = Color.FromArgb(197, 217, 241);
            layout.BorderStyle = UIElementBorderStyle.Solid;

            // Header style matching Image 3 (flat blue header theme)
            layout.Override.HeaderStyle = HeaderStyle.Standard;
            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(93, 151, 214);
            layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(67, 118, 184);
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            layout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Row selector styling matching headers
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(67, 118, 184);
            layout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(93, 151, 214);
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            // Row & Cell appearance matching Image 3
            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.RowAppearance.BorderColor = Color.FromArgb(197, 217, 241);
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 250, 255);
            layout.Override.RowAlternateAppearance.BorderColor = Color.FromArgb(197, 217, 241);

            // Selected / Active Row
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            layout.Override.SelectedRowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

            // Borders
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.CellAppearance.BorderColor = Color.FromArgb(197, 217, 241);
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

            // Compact spacing
            layout.Override.RowSizing = RowSizing.AutoFree;
            layout.Override.DefaultRowHeight = 22;
            layout.Override.RowSpacingBefore = 0;
            layout.Override.RowSpacingAfter = 0;
            layout.Override.CellPadding = 2;
            layout.Override.CellSpacing = 0;

            layout.GroupByBox.Hidden = true;
            layout.AutoFitStyle = AutoFitStyle.None;
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.Scrollbars = Scrollbars.Both;
        }

        private void SetupGridDocking()
        {
            try
            {
                if (ultraGrid1 != null && ultraPanel3 != null)
                {
                    ultraGrid1.Dock = DockStyle.None;
                    ultraGrid1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    ultraGrid1.Location = new Point(5, 51);
                    ultraGrid1.Size = new Size(Math.Max(100, ultraPanel3.ClientArea.Width - 10), Math.Max(100, ultraPanel3.ClientArea.Height - 81));
                }

                if (ultraPanelGridFooter != null && ultraPanel3 != null)
                {
                    ultraPanelGridFooter.Dock = DockStyle.None;
                    ultraPanelGridFooter.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    ultraPanelGridFooter.Location = new Point(5, ultraPanel3.ClientArea.Height - 30);
                    ultraPanelGridFooter.Size = new Size(Math.Max(100, ultraPanel3.ClientArea.Width - 10), 24);

                    ultraPanelGridFooter.Appearance.BackColor = Color.FromArgb(93, 151, 214);
                    ultraPanelGridFooter.Appearance.BackColor2 = Color.FromArgb(93, 151, 214);
                    ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
                    ultraPanelGridFooter.Appearance.BorderColor = Color.FromArgb(144, 181, 223);
                    ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
                    ultraPanelGridFooter.Visible = true;
                    ultraPanelGridFooter.BringToFront();
                }

                ApplyUnifiedGridTheme(ultraGrid1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up grid docking: " + ex.Message);
            }
        }

        private void frmPurchaseReturn_Resize(object sender, EventArgs e)
        {
            // Resize grid columns to fill available width when form is resized
            ResizeGridColumns();
        }

        private void ResizeGridColumns()
        {
            try
            {
                if (ultraGrid1 == null || ultraGrid1.DisplayLayout == null ||
                    ultraGrid1.DisplayLayout.Bands.Count == 0)
                    return;

                // Get the available width (subtract the width of the row selectors and vertical scrollbar)
                int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                int rowSelectorsWidth = ultraGrid1.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True ? 20 : 0;
                int availableWidth = ultraGrid1.Width - scrollBarWidth - rowSelectorsWidth - 2;

                if (availableWidth <= 0)
                    return;

                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Count visible columns and get total of current widths
                int visibleColumnCount = 0;
                int totalCurrentWidth = 0;

                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Hidden)
                    {
                        visibleColumnCount++;
                        totalCurrentWidth += col.Width;
                    }
                }

                if (visibleColumnCount == 0 || totalCurrentWidth == 0)
                    return;

                // Calculate scaling factor
                double scaleFactor = (double)availableWidth / totalCurrentWidth;

                // Adjust column widths proportionally
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Hidden)
                    {
                        // Apply minimum width for each column type
                        int minWidth = 60; // Default minimum width

                        switch (col.Key)
                        {
                            case "Description":
                                minWidth = 150; // Wider for item description
                                break;
                            case "Amount":
                                minWidth = 100; // Wider for amounts
                                break;
                            case "Returned":
                                minWidth = 100; // Wider for returned amounts
                                break;
                            case "Returned qty":
                                minWidth = 80; // Wider for returned quantity
                                break;
                            case "Reason":
                                minWidth = 120; // Wider for dropdown
                                break;
                        }

                        // Calculate new width but respect minimum
                        int newWidth = Math.Max(minWidth, (int)(col.Width * scaleFactor));
                        col.Width = newWidth;
                    }
                }

                // Force the grid to repaint
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error resizing grid columns: " + ex.Message);
            }
        }

        // Method to disable tab stop for all controls except the specified ones
        private void DisableTabStopForAllExcept(Control[] exceptControls)
        {
            DisableTabStopForControlsExcept(this.Controls, exceptControls);
        }

        // Helper method to recursively disable tab stop for controls
        private void DisableTabStopForControlsExcept(Control.ControlCollection controls, Control[] exceptControls)
        {
            foreach (Control control in controls)
            {
                // Skip the specified exception controls
                if (!exceptControls.Contains(control))
                {
                    // Disable tab stop for this control
                    control.TabStop = false;
                }
                else
                {
                    // Ensure tab stop is enabled for our target controls
                    control.TabStop = true;
                }

                // Process child controls recursively if this control has children
                if (control.Controls.Count > 0)
                {
                    DisableTabStopForControlsExcept(control.Controls, exceptControls);
                }
            }
        }

        // Method to load branch data into cmbBranch dropdown
        private void LoadBranchData()
        {
            try
            {
                // Show loading cursor
                Cursor.Current = Cursors.WaitCursor;

                // Get branch data from the dropdown class
                BranchDDlGrid branchGrid = drop.getBanchDDl();

                // Check if branches were found
                if (branchGrid?.List != null && branchGrid.List.Any())
                {
                    // Set up the dropdown properties
                    cmbBranch.DisplayMember = "BranchName";
                    cmbBranch.ValueMember = "Id";
                    cmbBranch.DataSource = branchGrid.List;

                    // Set a default branch if available
                    if (cmbBranch.Items.Count > 0)
                    {
                        cmbBranch.SelectedIndex = 0;
                    }
                }
                else
                {
                    // If no branches found, clear the dropdown
                    cmbBranch.DataSource = null;
                    cmbBranch.Items.Clear();

                    // Inform the user that no branches were found
                    MessageBox.Show("No branches found in the database. Please add branches first.",
                        "No Branches", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading branch data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }

        public void RefreshPaymode()
        {
            try
            {

                // Store current selection if any
                int currentPayModeId = 0;
                string currentPayModeName = "";
                if (cmbPaymntMethod.SelectedItem != null)
                {
                    Infragistics.Win.ValueListItem selectedItem = cmbPaymntMethod.SelectedItem as Infragistics.Win.ValueListItem;
                    if (selectedItem != null && selectedItem.DataValue is DataRowView)
                    {
                        DataRowView selectedRow = selectedItem.DataValue as DataRowView;
                        currentPayModeId = Convert.ToInt32(selectedRow["PayModeID"]);
                        currentPayModeName = selectedRow["PayModeName"].ToString();
                    }
                }

                // Create a new DataTable with an additional default item
                DataTable dt = new DataTable();
                dt.Columns.Add("PayModeID", typeof(int));
                dt.Columns.Add("PayModeName", typeof(string));

                // Add "Select Payment" as the first item with clearer text that it's required
                DataRow defaultRow = dt.NewRow();
                defaultRow["PayModeID"] = 0;
                defaultRow["PayModeName"] = "Select Payment";
                dt.Rows.Add(defaultRow);

                // Get payment methods from the database
                PaymodeDDlGrid grid = drop.GetPaymode();

                // Check if we have payment methods
                if (grid != null && grid.List != null && grid.List.Any())
                {

                    // Add the actual payment methods
                    foreach (var item in grid.List)
                    {
                        DataRow row = dt.NewRow();
                        row["PayModeID"] = item.PayModeID;
                        row["PayModeName"] = item.PayModeName;
                        dt.Rows.Add(row);
                    }
                }
                else
                {
                }

                // If we had a previously selected payment method that's not in the list yet, add it
                if (currentPayModeId > 0 && !dt.AsEnumerable().Any(row => Convert.ToInt32(row["PayModeID"]) == currentPayModeId))
                {
                    DataRow customRow = dt.NewRow();
                    customRow["PayModeID"] = currentPayModeId;
                    customRow["PayModeName"] = currentPayModeName;
                    dt.Rows.Add(customRow);
                }

                // Update the data source
                cmbPaymntMethod.DataSource = dt;
                cmbPaymntMethod.DisplayMember = "PayModeName";
                cmbPaymntMethod.ValueMember = "PayModeID";

                // Restore selection if possible, otherwise select first item (but not if skipping is requested)
                if (currentPayModeId > 0)
                {
                    // Try to find the previously selected payment method
                    for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                    {
                        Infragistics.Win.ValueListItem item = cmbPaymntMethod.Items[i] as Infragistics.Win.ValueListItem;
                        if (item != null && item.DataValue is DataRowView)
                        {
                            DataRowView row = item.DataValue as DataRowView;
                            if (Convert.ToInt32(row["PayModeID"]) == currentPayModeId)
                            {
                                cmbPaymntMethod.SelectedIndex = i;
                                return;
                            }
                        }
                    }
                }

                // If we couldn't restore the selection or had no previous selection, select the first item
                // But only do this if skipPaymentMethodReset is not set
                if (!skipPaymentMethodReset && cmbPaymntMethod.Items.Count > 0)
                {
                    cmbPaymntMethod.SelectedIndex = 0;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment methods: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ultraCombo1_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void lblNetAmount_Click(object sender, EventArgs e)
        {

        }

        private void ultraPanel2_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void ultraDateTimeEditor1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void lblBranch_Click(object sender, EventArgs e)
        {

        }

        private void lblVendor_Click(object sender, EventArgs e)
        {

        }

        // Method to get the selected branch ID
        private int GetSelectedBranchId()
        {
            try
            {
                if (cmbBranch.SelectedItem != null)
                {
                    // Get the selected branch ID by accessing the Id property of the BranchDDl object
                    BranchDDl selectedBranch = cmbBranch.SelectedItem as BranchDDl;
                    if (selectedBranch != null)
                    {
                        return selectedBranch.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting selected branch: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Return 0 if no branch is selected or an error occurs
            return 0;
        }

        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            // You can add code here to handle when a branch is selected
            // For example, you might want to load related data based on the selected branch
            int selectedBranchId = GetSelectedBranchId();

            // If a valid branch is selected, you can perform additional operations
            if (selectedBranchId > 0)
            {
                // Add your code here to load data related to the selected branch
                // For example, load vendors or products for this branch
            }
        }

        // Add a click event handler for the branch dropdown to ensure data is loaded when clicked
        private void cmbBranch_Click(object sender, EventArgs e)
        {
            // If the dropdown is empty, reload the branch data
            if (cmbBranch.Items.Count == 0)
            {
                LoadBranchData();
            }
        }

        private void cmbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void lblPRno_Click(object sender, EventArgs e)
        {

        }

        private void TxtSRNO_TextChanged(object sender, EventArgs e)
        {

        }

        // ─── Navigation Buttons ──────────────────────────────────────────────────

        /// <summary>
        /// Lazily loads the full list of PR numbers for the current branch/year
        /// so that |◄ ◄ ► ►| can navigate through them.
        /// </summary>
        private void EnsureNavListLoaded()
        {
            try
            {
                if (_navPRNumbers.Count == 0)
                {
                    // Pull all PR return numbers via the repo (returns newest-first or oldest-first)
                    var allRecords = prRepo.GetAllPurchaseReturnNumbers();
                    _navPRNumbers.Clear();
                    if (allRecords != null)
                        foreach (var n in allRecords)
                            _navPRNumbers.Add(n.ToString());

                    // Set current position to the record shown in TxtSRNO (if any)
                    string current = TxtSRNO.Text?.Trim();
                    if (!string.IsNullOrEmpty(current))
                        _navCurrentIndex = _navPRNumbers.IndexOf(current);
                    if (_navCurrentIndex < 0 && _navPRNumbers.Count > 0)
                        _navCurrentIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NavList load error: " + ex.Message);
            }
        }

        /// <summary>Navigates to a PR record by index in _navPRNumbers.</summary>
        private void NavigateToIndex(int index)
        {
            try
            {
                if (_navPRNumbers.Count == 0) return;
                index = Math.Max(0, Math.Min(index, _navPRNumbers.Count - 1));
                _navCurrentIndex = index;
                string prNumber = _navPRNumbers[_navCurrentIndex];
                TxtSRNO.Text = prNumber;
                // Trigger the same load logic used when a PR number is typed/selected
                TxtSRNO_TextChanged(TxtSRNO, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Navigation error: " + ex.Message);
            }
        }

        private void btnNavFirst_Click(object sender, EventArgs e)
        {
            EnsureNavListLoaded();
            NavigateToIndex(0);
        }

        private void btnNavPrev_Click(object sender, EventArgs e)
        {
            EnsureNavListLoaded();
            NavigateToIndex(_navCurrentIndex - 1);
        }

        private void btnNavNext_Click(object sender, EventArgs e)
        {
            EnsureNavListLoaded();
            NavigateToIndex(_navCurrentIndex + 1);
        }

        private void btnNavLast_Click(object sender, EventArgs e)
        {
            EnsureNavListLoaded();
            NavigateToIndex(_navPRNumbers.Count - 1);
        }

        // ─── Panel Navigation Buttons (ultraPanel11, ultraPanel4, ultraPanel6, ultraPanel5) ───

        private void InitializePanelButtons()
        {
            RegisterPanelButton(ultraPanel11); // First
            RegisterPanelButton(ultraPanel4);  // Prev
            RegisterPanelButton(ultraPanel6);  // Next
            RegisterPanelButton(ultraPanel5);  // Last
        }

        private void RegisterPanelButton(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null)
                return;

            panel.UseAppStyling = false;
            panel.Cursor = Cursors.Hand;
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            ApplyPanelButtonStyle(panel, false, false);

            panel.Click += PanelButton_Click;
            panel.MouseEnter += PanelButton_MouseEnter;
            panel.MouseLeave += PanelButton_MouseLeave;
            panel.MouseDown += PanelButton_MouseDown;
            panel.MouseUp += PanelButton_MouseUp;

            panel.ClientArea.Click += PanelButton_Click;
            panel.ClientArea.MouseEnter += PanelButton_MouseEnter;
            panel.ClientArea.MouseLeave += PanelButton_MouseLeave;
            panel.ClientArea.MouseDown += PanelButton_MouseDown;
            panel.ClientArea.MouseUp += PanelButton_MouseUp;

            foreach (Control child in panel.ClientArea.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += PanelButton_Click;
                child.MouseEnter += PanelButton_MouseEnter;
                child.MouseLeave += PanelButton_MouseLeave;
                child.MouseDown += PanelButton_MouseDown;
                child.MouseUp += PanelButton_MouseUp;
            }
        }

        private static readonly Color NavButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color NavButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color NavButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color NavButtonTextBlue = Color.FromArgb(14, 47, 108);
        private static readonly Color NavPanelHoverTopColor = Color.FromArgb(245, 250, 255);
        private static readonly Color NavPanelHoverBottomColor = Color.FromArgb(170, 206, 244);
        private static readonly Color NavPanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color NavPanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        private static void ApplyPanelButtonStyle(Infragistics.Win.Misc.UltraPanel panel, bool isHover, bool isPressed)
        {
            if (panel == null) return;
            Infragistics.Win.AppearanceBase appearance = panel.Appearance;
            appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance.BorderColor = NavButtonBlueBorder;
            appearance.ForeColor = NavButtonTextBlue;

            if (isPressed)
            {
                appearance.BackColor = NavPanelPressedTopColor;
                appearance.BackColor2 = NavPanelPressedBottomColor;
            }
            else if (isHover)
            {
                appearance.BackColor = NavPanelHoverTopColor;
                appearance.BackColor2 = NavPanelHoverBottomColor;
            }
            else
            {
                appearance.BackColor = NavButtonBlueTop;
                appearance.BackColor2 = NavButtonBlueBottom;
            }
        }

        private Infragistics.Win.Misc.UltraPanel GetActionPanel(object sender)
        {
            Control control = sender as Control;
            while (control != null)
            {
                Infragistics.Win.Misc.UltraPanel panel = control as Infragistics.Win.Misc.UltraPanel;
                if (panel != null)
                    return panel;

                control = control.Parent;
            }

            return null;
        }

        private void PanelButton_MouseEnter(object sender, EventArgs e)
        {
            Infragistics.Win.Misc.UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                ApplyPanelButtonStyle(panel, true, false);
            }
        }

        private void PanelButton_MouseLeave(object sender, EventArgs e)
        {
            Infragistics.Win.Misc.UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false);
            }
        }

        private void PanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            Infragistics.Win.Misc.UltraPanel panel = GetActionPanel(sender);
            if (panel != null && e.Button == MouseButtons.Left)
            {
                ApplyPanelButtonStyle(panel, true, true);
            }
        }

        private void PanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            Infragistics.Win.Misc.UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false);
            }
        }

        private void PanelButton_Click(object sender, EventArgs e)
        {
            Infragistics.Win.Misc.UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                panel.Focus();

                if (panel == ultraPanel11)
                {
                    btnNavFirst_Click(sender, e);
                }
                else if (panel == ultraPanel4)
                {
                    btnNavPrev_Click(sender, e);
                }
                else if (panel == ultraPanel6)
                {
                    btnNavNext_Click(sender, e);
                }
                else if (panel == ultraPanel5)
                {
                    btnNavLast_Click(sender, e);
                }
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1 == null || ultraGrid1.Rows == null || ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("No items available to return.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Exit edit mode to commit any active cell edits
                ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                ultraGrid1.UpdateData();

                // Check how many items are checked in the SELECT column
                int checkedCount = 0;
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (!row.IsDataRow || row.IsFilteredOut) continue;
                    if (row.Cells.Exists("SELECT") && row.Cells["SELECT"].Value != null &&
                        row.Cells["SELECT"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["SELECT"].Value))
                    {
                        checkedCount++;
                    }
                }

                // If no items are checked, check if there is an active or selected row
                if (checkedCount == 0)
                {
                    UltraGridRow targetRow = ultraGrid1.ActiveRow;
                    if (targetRow == null || !targetRow.IsDataRow || targetRow.IsFilteredOut)
                    {
                        if (ultraGrid1.Selected.Rows.Count > 0)
                        {
                            targetRow = ultraGrid1.Selected.Rows[0];
                        }
                    }

                    if (targetRow != null && targetRow.IsDataRow && !targetRow.IsFilteredOut)
                    {
                        // Check ONLY the single highlighted/active row
                        foreach (UltraGridRow row in ultraGrid1.Rows)
                        {
                            if (row.Cells.Exists("SELECT"))
                            {
                                row.Cells["SELECT"].Value = (row == targetRow);
                            }
                        }
                        ultraGrid1.UpdateData();
                    }
                    else
                    {
                        MessageBox.Show("Please select or highlight an item to return.", "Selection Required",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Save/Return the selected item(s)
                SavePurchaseReturn();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing return: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReturnAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1 == null || ultraGrid1.Rows == null || ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("No items available to return.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Exit edit mode to commit any active cell edits
                ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                ultraGrid1.UpdateData();

                // Select ALL items in ultraGrid1
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (!row.IsDataRow || row.IsFilteredOut) continue;
                    if (row.Cells.Exists("SELECT"))
                    {
                        row.Cells["SELECT"].Value = true;
                    }
                }
                ultraGrid1.UpdateData();

                // Save/Return all items for this bill
                SavePurchaseReturn();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing return all: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Check if items are already loaded via btnAddPurchaceList (vendor purchase items)
            if (_isPurchaseDataLoaded)
            {
                MessageBox.Show("Invalid operation. Purchase data is already loaded.",
                    "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if items are being loaded via BtnDial
            // In the original context, btnDial is being used when TxtBarcode is read-only
            if (TxtBarcode.ReadOnly && !_originalTxtBarcodeReadOnly)
            {
                MessageBox.Show("Invalid operation. Please finish the current item selection process before using this button.",
                    "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if items are loaded by BtnDial (indicated by "WITHOUT GR")
            // Only block if we actually have items in the grid (not just "WITHOUT GR" in textBox1)
            if (textBox1.Text == "WITHOUT GR" && ultraGrid1.Rows.Count > 0 && HasValidItems())
            {
                MessageBox.Show("Invalid operation. Items already loaded via direct selection.",
                    "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Original functionality
        }

        // Helper method to check if the grid has valid items
        private bool HasValidItems()
        {
            if (ultraGrid1.Rows.Count == 0)
                return false;

            // Check if there are any selected rows with their SELECT checkbox checked
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Count > 0 && !row.IsFilteredOut)
                {
                    // Check if the row has a valid item
                    bool hasValidItem = (row.Cells.Exists("ItemID") && row.Cells["ItemID"].Value != null) ||
                                      (row.Cells.Exists("Description") && row.Cells["Description"].Value != null);

                    if (!hasValidItem)
                        continue;

                    // Check if this row is selected via the SELECT column
                    if (row.Cells.Exists("SELECT") &&
                        row.Cells["SELECT"].Value != null &&
                        row.Cells["SELECT"].Value != DBNull.Value &&
                        Convert.ToBoolean(row.Cells["SELECT"].Value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Don't do anything when text changes - only load on Enter key press
            // This allows the user to type the complete number without interruption
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed
            if (e.KeyCode == Keys.Enter)
            {
                // Show pbxSave and hide ultraPictureBox4
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                e.Handled = true;
                e.SuppressKeyPress = true;

                try
                {
                    // Don't do anything if text is "WITHOUT GR" (special case)
                    if (textBox1.Text == "WITHOUT GR")
                    {
                        return;
                    }

                    // Don't do anything if empty
                    if (string.IsNullOrEmpty(textBox1.Text))
                    {
                        return;
                    }

                    // Don't do anything if vendor is not selected
                    if (string.IsNullOrEmpty(vendorid.Text) || vendorid.Text == "0")
                    {
                        MessageBox.Show("Please select a vendor first.", "Vendor Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Only attempt to load purchase items if textBox1 contains a valid purchase number
                    if (TryGetPurchaseNoFromTextBox(out int purchaseNo))
                    {
                        // Save the original readonly state of TxtBarcode
                        _originalTxtBarcodeReadOnly = TxtBarcode.ReadOnly;

                        // Make TxtBarcode read-only - We're removing this line to keep it writable
                        // TxtBarcode.ReadOnly = true;

                        // Ensure TxtBarcode remains writable - removing this line
                        // EnsureTxtBarcodeIsWritable();

                        int vendorId = int.Parse(vendorid.Text);
                        // Load purchase items based on the purchase number
                        LoadPurchaseItems(vendorId, purchaseNo);

                        // Populate Invoice No in ultraTextEditor4
                        if (ultraTextEditor4 != null && chkGRN != null && chkGRN.Checked)
                        {
                            ultraTextEditor4.Text = GetInvoiceNoFromPurchaseNo(purchaseNo);
                        }

                        // Set the flag to indicate purchase data is loaded
                        _isPurchaseDataLoaded = true;
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid purchase number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in textBox1_KeyDown: {ex.Message}");
                    MessageBox.Show($"Error loading purchase items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ultraDateTimeEditor2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void TxtBarcode_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Show pbxSave and hide ultraPictureBox4
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // Check if we're in "Without GR" mode
                if (textBox1.Text == "WITHOUT GR")
                {
                    // In "Without GR" mode, we don't filter the grid on text changed
                    // We'll only search when Enter is pressed
                    return;
                }

                // Check if a purchase number is already selected
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    return; // No purchase selected yet, so do nothing
                }

                // Get the purchase number and vendor ID
                if (!TryGetPurchaseNoFromTextBox(out int purchaseNo))
                {
                    return;
                }
                int vendorId = Convert.ToInt32(vendorid.Text);

                // Check if the barcode/item ID field has a value
                if (!string.IsNullOrEmpty(TxtBarcode.Text))
                {
                    // Try to parse the item ID (if it's a numeric barcode)
                    if (long.TryParse(TxtBarcode.Text, out long itemId))
                    {
                        // Filter the grid to show only the matching item ID or barcode
                        FilterGridByItemIdOrBarcode(itemId, TxtBarcode.Text);
                    }
                    else
                    {
                        // If not numeric, search by barcode text
                        FilterGridByItemIdOrBarcode(0, TxtBarcode.Text);
                    }
                }
                else
                {
                    // If barcode field is empty, reload all items for the purchase
                    LoadPurchaseItems(vendorId, purchaseNo);
                }
            }
            catch (Exception ex)
            {
                // Just log the error, don't show a message box as this is a text changed event
                System.Diagnostics.Debug.WriteLine("Error filtering by item ID or barcode: " + ex.Message);
            }
        }

        private void FilterGridByItemIdOrBarcode(long itemId, string barcode)
        {
            try
            {
                // Check if the grid has a data source
                if (ultraGrid1.DataSource == null)
                    return;

                // Get the current data source as a DataTable
                DataTable currentData = ultraGrid1.DataSource as DataTable;
                if (currentData == null)
                    return;

                // Create a filtered view of the data
                DataView dv = new DataView(currentData);

                // Build the filter condition
                string filter = "";

                if (itemId > 0)
                {
                    // If we have a numeric value, search by both ItemID and BarCode
                    filter = $"ItemID = {itemId} OR BarCode LIKE '%{barcode}%'";
                }
                else
                {
                    // If not numeric, search only by BarCode
                    filter = $"BarCode LIKE '%{barcode}%'";
                }

                dv.RowFilter = filter;

                // Apply the filter to the grid
                ultraGrid1.DataSource = dv.ToTable();

                // Configure the grid layout again
                ConfigureItemsGridLayout();

                // Set focus to the grid and select the first row if available
                ultraGrid1.Focus();
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);

                    // Set focus to the Unit cell of the first row and ensure edit mode is activated
                    if (ultraGrid1.ActiveRow.Cells.Exists("Unit"))
                    {
                        ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Unit"];
                        ultraGrid1.Focus();
                        // Enter edit mode with caret active inside the cell
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error filtering grid: " + ex.Message);
            }
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _enterPressed = true;
                e.Handled = true;
                e.SuppressKeyPress = true;

                try
                {
                    // Check for *-1 key combination to show purchase history
                    if (TxtBarcode.Text == "*-1" || TxtBarcode.Text == "*1")
                    {
                        TxtBarcode.Clear();

                        // Get the highlighted/selected row from the grid
                        UltraGridRow selectedRow = null;
                        if (ultraGrid1.Selected.Rows.Count > 0)
                        {
                            selectedRow = ultraGrid1.Selected.Rows[0];
                        }
                        else if (ultraGrid1.ActiveRow != null)
                        {
                            selectedRow = ultraGrid1.ActiveRow;
                        }

                        if (selectedRow == null)
                        {
                            MessageBox.Show("Please select an item from the grid to view its purchase history.",
                                "No Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // Get ItemID from the selected row
                        if (!selectedRow.Cells.Exists("ItemID") || selectedRow.Cells["ItemID"].Value == null)
                        {
                            MessageBox.Show("Selected row does not contain a valid Item ID.",
                                "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        long itemId = Convert.ToInt64(selectedRow.Cells["ItemID"].Value);

                        // Open purchase history dialog
                        DialogBox.FrmItemPurchaseHistory purchaseHistoryForm = new DialogBox.FrmItemPurchaseHistory();
                        purchaseHistoryForm.LoadPurchaseHistoryByItemId(itemId);
                        purchaseHistoryForm.StartPosition = FormStartPosition.CenterScreen;

                        if (purchaseHistoryForm.ShowDialog() == DialogResult.OK && purchaseHistoryForm.ItemSelected)
                        {
                            // Load the selected item into the grid
                            LoadItemFromPurchaseHistory(purchaseHistoryForm);
                        }
                        return;
                    }

                    // Check if the barcode field has a value
                    if (!string.IsNullOrEmpty(TxtBarcode.Text))
                    {
                        // Set textBox1 to "Without GR" to indicate barcode method is being used
                        if (this.Controls.Find("textBox1", true).Length > 0)
                        {
                            TextBox textBox1 = this.Controls.Find("textBox1", true)[0] as TextBox;
                            if (textBox1 != null)
                            {
                                textBox1.Text = "WITHOUT GR"; // Changed to all caps for consistency
                            }
                        }

                        // Store the barcode value and clear the field before searching
                        string barcodeToSearch = TxtBarcode.Text;
                        TxtBarcode.Clear();

                        // Search for the barcode in the database and add to grid
                        SearchItemsByBarcode(barcodeToSearch);

                        // Don't set focus back to TxtBarcode - let it stay in the Unit cell
                        // TxtBarcode.Focus(); - Remove this line to keep focus in the Unit cell
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error processing barcode: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SearchItemsByBarcode(string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode))
                {
                    MessageBox.Show("Please enter a barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the barcode already exists in the grid
                bool barcodeExists = false;
                UltraGridRow existingRow = null;

                if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                {
                    DataTable existingData = (DataTable)ultraGrid1.DataSource;

                    // Search for the barcode in existing rows
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells["Barcode"].Value != null &&
                            row.Cells["Barcode"].Value.ToString().Equals(barcode, StringComparison.OrdinalIgnoreCase))
                        {
                            barcodeExists = true;
                            existingRow = row;
                            break;
                        }
                    }

                    // If barcode exists, increment quantity and update amount
                    if (barcodeExists && existingRow != null)
                    {
                        // Get current quantity
                        double currentQty = Convert.ToDouble(existingRow.Cells["Quantity"].Value);

                        // Increment quantity by 1
                        double newQty = currentQty + 1;
                        existingRow.Cells["Quantity"].Value = newQty;

                        // Recalculate amount
                        decimal cost = Convert.ToDecimal(existingRow.Cells["Cost"].Value);
                        existingRow.Cells["Amount"].Value = cost * (decimal)newQty;

                        // Update the net amount total
                        UpdateNetAmount();

                        // Set focus to this row and highlight it
                        ultraGrid1.ActiveRow = existingRow;
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(existingRow);

                        // Set focus to the Quantity cell to show it was updated
                        if (existingRow.Cells.Exists("Quantity"))
                        {
                            ultraGrid1.ActiveCell = existingRow.Cells["Quantity"];
                            ultraGrid1.Focus();
                            // Flash the cell to indicate it was updated
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                            // Scroll to ensure the row is visible
                            ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(existingRow);
                        }

                        return; // Exit the method since we've updated the existing item
                    }
                }

                // If barcode doesn't exist, continue with existing functionality to add new item
                // Create a BaseRepostitory instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use the repository to search for the barcode
                using (System.Data.SqlClient.SqlConnection conn = (System.Data.SqlClient.SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("_POS_PurchaseReturn", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@barcode", barcode);
                        cmd.Parameters.AddWithValue("@_Operation", "BARCODEPURCHASE");

                        using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(cmd))
                        {
                            DataTable newItemsTable = new DataTable();
                            adapter.Fill(newItemsTable);

                            // Create a new table with our specified columns
                            DataTable formattedTable = new DataTable();

                            // Add our required columns in the correct order
                            formattedTable.Columns.Add("Sl No", typeof(int));
                            formattedTable.Columns.Add("Description", typeof(string)); // Item Name
                            formattedTable.Columns.Add("ItemID", typeof(long));
                            formattedTable.Columns.Add("Barcode", typeof(string));
                            formattedTable.Columns.Add("Unit", typeof(string));
                            formattedTable.Columns.Add("Packing", typeof(double));
                            formattedTable.Columns.Add("Cost", typeof(decimal));
                            formattedTable.Columns.Add("Quantity", typeof(double));
                            formattedTable.Columns.Add("Amount", typeof(decimal));
                            formattedTable.Columns.Add("Reason", typeof(string));
                            formattedTable.Columns.Add("SELECT", typeof(bool)); // Added SELECT column for schema consistency

                            // Add tax-related columns to match btnDial schema
                            formattedTable.Columns.Add("UnitId", typeof(int));
                            formattedTable.Columns.Add("TaxPer", typeof(decimal));
                            formattedTable.Columns.Add("TaxAmt", typeof(decimal));
                            formattedTable.Columns.Add("TaxType", typeof(string));

                            if (newItemsTable != null && newItemsTable.Rows.Count > 0)
                            {
                                // Copy data from the source table to our formatted table
                                DataRow newRow = formattedTable.NewRow();

                                // Set sequential SlNo
                                newRow["Sl No"] = ultraGrid1.Rows.Count + 1;

                                // Copy Item Name
                                if (newItemsTable.Columns.Contains("Description"))
                                    newRow["Description"] = newItemsTable.Rows[0]["Description"];
                                else if (newItemsTable.Columns.Contains("ItemName"))
                                    newRow["Description"] = newItemsTable.Rows[0]["ItemName"];
                                else
                                    newRow["Description"] = "";

                                // Copy ItemID
                                if (newItemsTable.Columns.Contains("ItemID"))
                                    newRow["ItemID"] = newItemsTable.Rows[0]["ItemID"];
                                else
                                    newRow["ItemID"] = 0;

                                // Copy Barcode
                                if (newItemsTable.Columns.Contains("Barcode"))
                                    newRow["Barcode"] = newItemsTable.Rows[0]["Barcode"];
                                else if (newItemsTable.Columns.Contains("BarCode"))
                                    newRow["Barcode"] = newItemsTable.Rows[0]["BarCode"];
                                else
                                    newRow["Barcode"] = barcode;

                                // Copy Unit
                                if (newItemsTable.Columns.Contains("Unit"))
                                    newRow["Unit"] = newItemsTable.Rows[0]["Unit"];
                                else if (newItemsTable.Columns.Contains("UnitName"))
                                    newRow["Unit"] = newItemsTable.Rows[0]["UnitName"];
                                else
                                    newRow["Unit"] = "";

                                // Copy Packing
                                if (newItemsTable.Columns.Contains("Packing"))
                                    newRow["Packing"] = newItemsTable.Rows[0]["Packing"];
                                else
                                    newRow["Packing"] = 1;

                                // Copy Cost
                                if (newItemsTable.Columns.Contains("Cost"))
                                    newRow["Cost"] = newItemsTable.Rows[0]["Cost"];
                                else
                                    newRow["Cost"] = 0;

                                // Set Quantity to 1 by default
                                newRow["Quantity"] = 1;

                                // Initialize Reason
                                newRow["Reason"] = "Select Reason";

                                // Set values for the additional columns
                                newRow["SELECT"] = false;
                                newRow["UnitId"] = newItemsTable.Columns.Contains("UnitId") ?
                                    newItemsTable.Rows[0]["UnitId"] : 0;
                                newRow["TaxPer"] = newItemsTable.Columns.Contains("TaxPer") ?
                                    newItemsTable.Rows[0]["TaxPer"] : 0m;
                                newRow["TaxAmt"] = newItemsTable.Columns.Contains("TaxAmt") ?
                                    newItemsTable.Rows[0]["TaxAmt"] : 0m;
                                newRow["TaxType"] = newItemsTable.Columns.Contains("TaxType") ?
                                    newItemsTable.Rows[0]["TaxType"] : "";

                                // Calculate Amount
                                decimal cost = Convert.ToDecimal(newRow["Cost"]);
                                double quantity = Convert.ToDouble(newRow["Quantity"]);
                                newRow["Amount"] = cost * (decimal)quantity;

                                // Add the row to the table
                                formattedTable.Rows.Add(newRow);
                            }
                            else
                            {
                                // No items found with this barcode
                                // Create an empty row with default values
                                DataRow emptyRow = formattedTable.NewRow();
                                emptyRow["Sl No"] = ultraGrid1.Rows.Count + 1;
                                emptyRow["Description"] = string.Empty;
                                emptyRow["ItemID"] = 0;
                                emptyRow["Barcode"] = barcode;
                                emptyRow["Unit"] = string.Empty;
                                emptyRow["Packing"] = 1;
                                emptyRow["Cost"] = 0;
                                emptyRow["Quantity"] = 1;
                                emptyRow["Amount"] = 0;
                                emptyRow["Reason"] = "Select Reason";
                                emptyRow["SELECT"] = false;
                                emptyRow["UnitId"] = 0;
                                emptyRow["TaxPer"] = 0m;
                                emptyRow["TaxAmt"] = 0m;
                                emptyRow["TaxType"] = "";

                                formattedTable.Rows.Add(emptyRow);

                                MessageBox.Show("No items found with this barcode. A new row has been added.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            // Check if we already have items in the grid
                            if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                            {
                                // Get the existing data
                                DataTable existingData = (DataTable)ultraGrid1.DataSource;

                                // Add the new rows to the existing table
                                foreach (DataRow row in formattedTable.Rows)
                                {
                                    DataRow newRow = existingData.NewRow();
                                    foreach (DataColumn col in formattedTable.Columns)
                                    {
                                        if (existingData.Columns.Contains(col.ColumnName))
                                        {
                                            newRow[col.ColumnName] = row[col.ColumnName];
                                        }
                                    }
                                    existingData.Rows.Add(newRow);
                                }

                                // Refresh the grid
                                ultraGrid1.DataSource = existingData;
                            }
                            else
                            {
                                // If no existing data, just set the new formatted table as the source
                                ultraGrid1.DataSource = formattedTable;
                            }

                            // Configure grid layout
                            ConfigureItemsGridLayout();

                            // Select the last row (the newly added one)
                            if (ultraGrid1.Rows.Count > 0)
                            {
                                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                                ultraGrid1.Selected.Rows.Clear();
                                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);

                                // Scroll to the newly added row
                                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);

                                // Set focus to the Unit cell and ensure edit mode is activated
                                if (ultraGrid1.ActiveRow.Cells.Exists("Unit"))
                                {
                                    ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Unit"];
                                    ultraGrid1.Focus();
                                    // Enter edit mode with caret active inside the cell
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                }
                            }

                            // Update the Net Amount label
                            UpdateNetAmount();

                            // Don't return focus to the barcode field - let it stay in the Unit cell
                            // TxtBarcode.Focus(); - We'll comment this out in the TxtBarcode_KeyDown method
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching barcode: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                TxtBarcode.Focus();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            lblNetAmount.Text = textBox2.Text;

            // Update TxtSubTotal to match textBox2
            TxtSubTotal.Text = textBox2.Text;
        }

        private void BtnDial_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if items are loaded via button1 (purchasereturnupdate)
                if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                {
                    MessageBox.Show("Invalid operation. Purchase return data is already loaded from update form.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Only prevent direct item selection if we're in the middle of a purchase return process
                // that was loaded via purchase number, but allow if we're already in "WITHOUT GR" mode
                if (_isPurchaseDataLoaded && !string.IsNullOrEmpty(textBox1.Text) &&
                    textBox1.Text != "WITHOUT GR" && textBox1.Text != "Without GR")
                {
                    MessageBox.Show("Items are already loaded from purchase number " + textBox1.Text + ". " +
                        "Please complete this return or clear the form first.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show pbxSave and hide ultraPictureBox4
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // Open the item master dialog without requiring vendor selection
                frmdialForItemMaster itemDialog = new frmdialForItemMaster("FromPurchaseReturn");
                itemDialog.Owner = this; // Set owner to establish parent-child relationship
                if (itemDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Save the original readonly state of TxtBarcode
                        _originalTxtBarcodeReadOnly = TxtBarcode.ReadOnly;

                        // Make TxtBarcode writable when using BtnDial
                        EnsureTxtBarcodeIsWritable();

                        // Set "WITHOUT GR" in textBox1 to indicate items added directly
                        textBox1.Text = "WITHOUT GR";

                        // Dictionary to store all item data fields
                        Dictionary<string, object> itemData;

                        // First try to get the data from GetSelectedItemData method which has all fields
                        itemData = itemDialog.GetSelectedItemData();

                        // Extract the essential values from the dictionary
                        long itemId = Convert.ToInt64(itemData["ItemId"]);
                        string itemName = itemData["Description"].ToString();
                        string barcode = itemData.ContainsKey("BarCode") ? itemData["BarCode"].ToString() : "";
                        string unit = itemData["Unit"].ToString();
                        double packing = Convert.ToDouble(itemData["Packing"]);
                        decimal cost = Convert.ToDecimal(itemData["Cost"]);

                        // Extract tax-related values if available
                        decimal taxPer = itemData.ContainsKey("TaxPer") ? Convert.ToDecimal(itemData["TaxPer"]) : 0m;
                        decimal taxAmt = itemData.ContainsKey("TaxAmt") ? Convert.ToDecimal(itemData["TaxAmt"]) : 0m;
                        string taxType = itemData.ContainsKey("TaxType") ? itemData["TaxType"].ToString() : "";
                        int unitId = itemData.ContainsKey("UnitId") ? Convert.ToInt32(itemData["UnitId"]) : 0;

                        // Check if we already have grid data
                        DataTable existingData = null;

                        if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                        {
                            existingData = ((DataTable)ultraGrid1.DataSource).Copy(); // Make a copy to preserve the schema

                            // Check if the item already exists in the grid
                            bool itemExists = false;
                            DataRow existingRow = null;

                            // Find matching item by ItemID
                            foreach (DataRow row in existingData.Rows)
                            {
                                if (row["ItemID"] != null && row["ItemID"] != DBNull.Value &&
                                    Convert.ToInt64(row["ItemID"]) == itemId)
                                {
                                    itemExists = true;
                                    existingRow = row;
                                    break;
                                }
                            }

                            // If item already exists, just increment the quantity
                            if (itemExists && existingRow != null)
                            {
                                // Get current quantity
                                double currentQty = Convert.ToDouble(existingRow["Quantity"]);

                                // Increment quantity by 1
                                double newQty = currentQty + 1;
                                existingRow["Quantity"] = newQty;

                                // Recalculate amount
                                decimal updatedAmount = cost * Convert.ToDecimal(packing) * (decimal)newQty;
                                existingRow["Amount"] = updatedAmount;

                                // Update the grid with the modified data
                                ultraGrid1.DataSource = existingData;

                                // Re-apply grid layout and styling
                                ConfigureItemsGridLayout();

                                // Update the Net Amount label
                                UpdateNetAmount();

                                // Find and select the updated row in the grid
                                int rowIndex = -1;
                                for (int i = 0; i < ultraGrid1.Rows.Count; i++)
                                {
                                    UltraGridRow row = ultraGrid1.Rows[i];
                                    if (row.Cells["ItemID"].Value != null &&
                                        Convert.ToInt64(row.Cells["ItemID"].Value) == itemId)
                                    {
                                        rowIndex = i;
                                        break;
                                    }
                                }

                                if (rowIndex >= 0)
                                {
                                    // Set focus to the updated row
                                    ultraGrid1.ActiveRow = ultraGrid1.Rows[rowIndex];
                                    ultraGrid1.Selected.Rows.Clear();
                                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);

                                    // Set focus to the Quantity cell
                                    ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Quantity"];
                                    ultraGrid1.Focus();

                                    // Enter edit mode to highlight the change
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                    ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);

                                    // Ensure the row is visible
                                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                                }

                                return; // Exit the method since we've updated the existing item
                            }
                        }
                        else
                        {
                            // Create a new DataTable with only the required columns
                            existingData = new DataTable();

                            // Add only the necessary columns in the specified order
                            existingData.Columns.Add("Sl No", typeof(int));
                            existingData.Columns.Add("Description", typeof(string)); // Item Name
                            existingData.Columns.Add("Barcode", typeof(string));
                            existingData.Columns.Add("Unit", typeof(string));
                            existingData.Columns.Add("Packing", typeof(double));
                            existingData.Columns.Add("Cost", typeof(decimal));
                            existingData.Columns.Add("Quantity", typeof(double));
                            existingData.Columns.Add("Amount", typeof(decimal));
                            existingData.Columns.Add("Returned", typeof(decimal));
                            existingData.Columns.Add("Returned qty", typeof(double));
                            existingData.Columns.Add("Reason", typeof(string));
                            existingData.Columns.Add("SELECT", typeof(bool)); // Add SELECT column at the end

                            // These are hidden columns, order doesn't matter for display
                            existingData.Columns.Add("ItemID", typeof(long));
                            existingData.Columns.Add("UnitId", typeof(int));
                            existingData.Columns.Add("TaxPer", typeof(decimal));
                            existingData.Columns.Add("TaxAmt", typeof(decimal));
                            existingData.Columns.Add("TaxType", typeof(string));
                        }

                        // Create a new row
                        DataRow newRow = existingData.NewRow();

                        // Set the SlNo value for the new row (number sequentially)
                        int slNo = existingData.Rows.Count + 1;
                        newRow["Sl No"] = slNo;

                        // Set SELECT to false by default if the column exists
                        if (existingData.Columns.Contains("SELECT"))
                        {
                            newRow["SELECT"] = false;
                        }

                        // Populate the row with the selected item data
                        newRow["Description"] = itemName;
                        newRow["ItemID"] = itemId;
                        newRow["Barcode"] = barcode;
                        newRow["Unit"] = unit;
                        newRow["UnitId"] = unitId;
                        newRow["Packing"] = packing;
                        newRow["Cost"] = cost;
                        newRow["TaxPer"] = taxPer;
                        newRow["TaxType"] = taxType;
                        newRow["Reason"] = "Select Reason"; // Set default value for Reason
                        newRow["Quantity"] = 1.0; // Default quantity

                        // Set default values for Returned qty and Returned
                        if (existingData.Columns.Contains("Returned qty"))
                        {
                            newRow["Returned qty"] = 0.00;
                        }
                        if (existingData.Columns.Contains("Returned"))
                        {
                            newRow["Returned"] = 0.00m;
                        }

                        // Calculate the amount using the formula: Cost × Packing × Quantity
                        decimal amount = cost * Convert.ToDecimal(packing) * 1.0m;
                        newRow["Amount"] = amount;

                        // Recalculate TaxAmt using the correct formula: Cost × (Tax% / (100 + Tax%))
                        // For inclusive tax, use Cost (not Amount) as the base
                        string normalizedTaxType = taxType?.ToLower().Trim();
                        if (normalizedTaxType == "incl" || normalizedTaxType == "i" || normalizedTaxType == "inclusive")
                        {
                            // For inclusive tax: Tax Amount = Cost × (Tax% / (100 + Tax%))
                            double costValue = (double)cost;
                            double denominator = 100.0 + (double)taxPer;
                            if (denominator > 0 && taxPer > 0)
                            {
                                double calculatedTaxAmt = costValue * ((double)taxPer / denominator);
                                newRow["TaxAmt"] = Math.Round((decimal)calculatedTaxAmt, 2);
                            }
                            else
                            {
                                newRow["TaxAmt"] = 0m;
                            }
                        }
                        else
                        {
                            // For exclusive tax: Tax Amount = Cost × (Tax% / 100)
                            if (taxPer > 0)
                            {
                                double calculatedTaxAmt = (double)cost * ((double)taxPer / 100.0);
                                newRow["TaxAmt"] = Math.Round((decimal)calculatedTaxAmt, 2);
                            }
                            else
                            {
                                newRow["TaxAmt"] = 0m;
                            }
                        }

                        // Add the row to the table
                        existingData.Rows.Add(newRow);

                        // Update the grid with the new data
                        ultraGrid1.DataSource = existingData;

                        // Configure the grid layout to ensure all columns are displayed properly
                        ConfigureItemsGridLayout();

                        // Select the newly added row and scroll to it
                        if (ultraGrid1.Rows.Count > 0)
                        {
                            ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                            ultraGrid1.Selected.Rows.Clear();
                            ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);

                            // Scroll to the newly added row
                            ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                        }

                        // Update the Net Amount label to reflect the new total
                        UpdateNetAmount();

                        // Set focus to the Description cell of the new row
                        SetFocusToLastRow();

                        // Items are now loaded directly, not from a purchase number
                        _isPurchaseDataLoaded = false;
                        _currentlyLoadedPurchaseNo = -1;

                        // Clear the PurchaseReturnUpdate tag if it exists
                        if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                        {
                            this.Tag = null;
                        }
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show("Error adding selected item: " + ex2.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        System.Diagnostics.Debug.WriteLine($"Error adding selected item: {ex2.Message}\n{ex2.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening item dialog: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in BtnDial_Click: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Method to load purchase items into ultraGrid1
        private void LoadPurchaseItems(int ledgerId, int purchaseNo)
        {
            try
            {
                // Show loading cursor
                Cursor.Current = Cursors.WaitCursor;

                // Get purchase items data
                DataTable purchaseItemsData = GetPurchaseItems(ledgerId, purchaseNo);

                if (ultraTextEditor4 != null && chkGRN != null && chkGRN.Checked && string.IsNullOrEmpty(ultraTextEditor4.Text))
                {
                    ultraTextEditor4.Text = GetInvoiceNoFromPurchaseNo(purchaseNo);
                }

                if (purchaseItemsData != null && purchaseItemsData.Rows.Count > 0)
                {
                    // Calculate and store original GRN bill total for real-time outstanding calculation
                    _originalBillTotal = 0m;
                    foreach (DataRow row in purchaseItemsData.Rows)
                    {
                        decimal itemCost = row.Table.Columns.Contains("Cost") && row["Cost"] != DBNull.Value ? Convert.ToDecimal(row["Cost"]) : 0m;
                        double itemQty = row.Table.Columns.Contains("Qty") && row["Qty"] != DBNull.Value ? Convert.ToDouble(row["Qty"]) : 0.0;
                        double itemPacking = row.Table.Columns.Contains("Packing") && row["Packing"] != DBNull.Value ? Convert.ToDouble(row["Packing"]) : 1.0;
                        _originalBillTotal += itemCost * (decimal)(itemQty * (itemPacking > 0 ? itemPacking : 1.0));
                    }
                    _originalBillTotal = Math.Round(_originalBillTotal, 2);

                    // Log the raw data to help debug
                    System.Diagnostics.Debug.WriteLine($"Loaded {purchaseItemsData.Rows.Count} rows from purchase items for ledger {ledgerId}, purchase {purchaseNo}");

                    // Get the existing data from the grid, if any
                    DataTable existingData = null;
                    int startingSlNo = 1;

                    // Check if this method was called from btnAddPurchaceList
                    bool isCalledFromBtnAddPurchaceList = new System.Diagnostics.StackTrace().GetFrames()
                        .Any(frame => frame.GetMethod().Name == "btnAddPurchaceList_Click");

                    if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                    {
                        existingData = (DataTable)ultraGrid1.DataSource;

                        // Always start at 1 when called from btnAddPurchaceList as we've already cleared the grid
                        if (!isCalledFromBtnAddPurchaceList && existingData.Rows.Count > 0)
                        {
                            startingSlNo = existingData.Rows.Count + 1;
                        }
                    }

                    // If there's no existing data, create a new table
                    if (existingData == null)
                    {
                        existingData = new DataTable();

                        // Add only the visible columns that we want to display
                        existingData.Columns.Add("Sl No", typeof(int));
                        existingData.Columns.Add("Description", typeof(string)); // Item Name
                        existingData.Columns.Add("Barcode", typeof(string));
                        existingData.Columns.Add("Unit", typeof(string));
                        existingData.Columns.Add("Packing", typeof(double));
                        existingData.Columns.Add("Cost", typeof(decimal));
                        existingData.Columns.Add("Quantity", typeof(double));
                        existingData.Columns.Add("Amount", typeof(decimal));
                        existingData.Columns.Add("Returned", typeof(decimal));
                        existingData.Columns.Add("Returned qty", typeof(double));
                        existingData.Columns.Add("Reason", typeof(string));
                        existingData.Columns.Add("SELECT", typeof(bool)); // Add SELECT column at the end

                        // Add ItemID as a hidden column, but we'll hide it in the grid layout
                        existingData.Columns.Add("ItemID", typeof(long));

                        // Add UnitId if needed
                        if (!existingData.Columns.Contains("UnitId"))
                        {
                            existingData.Columns.Add("UnitId", typeof(int));
                        }

                        // Add TaxType, TaxPer, TaxAmt columns
                        if (!existingData.Columns.Contains("TaxType"))
                        {
                            existingData.Columns.Add("TaxType", typeof(string));
                        }
                        if (!existingData.Columns.Contains("TaxPer"))
                        {
                            existingData.Columns.Add("TaxPer", typeof(decimal));
                        }
                        if (!existingData.Columns.Contains("TaxAmt"))
                        {
                            existingData.Columns.Add("TaxAmt", typeof(decimal));
                        }
                    }

                    // Copy data from the source table to our existing data table
                    for (int i = 0; i < purchaseItemsData.Rows.Count; i++)
                    {
                        DataRow newRow = existingData.NewRow();

                        // Set the sequential SlNo, continuing from existing data
                        newRow["Sl No"] = startingSlNo + i;

                        // Set SELECT to false by default for each row if the column exists
                        if (existingData.Columns.Contains("SELECT"))
                        {
                            newRow["SELECT"] = false;
                        }

                        // Copy Item Name/Description
                        if (purchaseItemsData.Columns.Contains("Description"))
                            newRow["Description"] = purchaseItemsData.Rows[i]["Description"];
                        else if (purchaseItemsData.Columns.Contains("ItemName"))
                            newRow["Description"] = purchaseItemsData.Rows[i]["ItemName"];
                        else
                            newRow["Description"] = "";
                        // Copy ItemID (but it will be hidden) - only from source, no hardcoded default
                        if (purchaseItemsData.Columns.Contains("ItemID") && purchaseItemsData.Rows[i]["ItemID"] != DBNull.Value)
                            newRow["ItemID"] = purchaseItemsData.Rows[i]["ItemID"];

                        // Copy Barcode
                        if (purchaseItemsData.Columns.Contains("Barcode"))
                            newRow["Barcode"] = purchaseItemsData.Rows[i]["Barcode"];
                        else if (purchaseItemsData.Columns.Contains("BarCode"))
                            newRow["Barcode"] = purchaseItemsData.Rows[i]["BarCode"];
                        else
                            newRow["Barcode"] = "";

                        // Copy Unit
                        if (purchaseItemsData.Columns.Contains("Unit"))
                            newRow["Unit"] = purchaseItemsData.Rows[i]["Unit"];
                        else if (purchaseItemsData.Columns.Contains("UnitName"))
                            newRow["Unit"] = purchaseItemsData.Rows[i]["UnitName"];
                        else
                            newRow["Unit"] = "";

                        // Copy UnitId - only from source, no hardcoded default
                        if (existingData.Columns.Contains("UnitId"))
                        {
                            if (purchaseItemsData.Columns.Contains("UnitId") && purchaseItemsData.Rows[i]["UnitId"] != DBNull.Value)
                                newRow["UnitId"] = purchaseItemsData.Rows[i]["UnitId"];
                        }

                        // Copy Packing - only from source, no hardcoded default
                        if (purchaseItemsData.Columns.Contains("Packing") && purchaseItemsData.Rows[i]["Packing"] != DBNull.Value)
                            newRow["Packing"] = purchaseItemsData.Rows[i]["Packing"];

                        // Copy Cost - only from source, no hardcoded default
                        if (purchaseItemsData.Columns.Contains("Cost") && purchaseItemsData.Rows[i]["Cost"] != DBNull.Value)
                            newRow["Cost"] = purchaseItemsData.Rows[i]["Cost"];

                        // Copy Quantity - only from source, no hardcoded default
                        if (purchaseItemsData.Columns.Contains("Qty") && purchaseItemsData.Rows[i]["Qty"] != DBNull.Value)
                            newRow["Quantity"] = purchaseItemsData.Rows[i]["Qty"];
                        else if (purchaseItemsData.Columns.Contains("Quantity") && purchaseItemsData.Rows[i]["Quantity"] != DBNull.Value)
                            newRow["Quantity"] = purchaseItemsData.Rows[i]["Quantity"];

                        // Copy Reason from source, or set default to "Select Reason"
                        if (purchaseItemsData.Columns.Contains("Reason") && purchaseItemsData.Rows[i]["Reason"] != DBNull.Value
                            && !string.IsNullOrWhiteSpace(purchaseItemsData.Rows[i]["Reason"].ToString()))
                        {
                            newRow["Reason"] = purchaseItemsData.Rows[i]["Reason"].ToString().Trim();
                        }
                        else
                        {
                            // Set default value if not present in source
                            newRow["Reason"] = "Select Reason";
                        }

                        // Get values for calculations
                        decimal cost = 0m;
                        double packing = 1.0;
                        double quantity = 1.0;

                        if (newRow["Cost"] != null && newRow["Cost"] != DBNull.Value)
                            cost = Convert.ToDecimal(newRow["Cost"]);
                        if (newRow["Packing"] != null && newRow["Packing"] != DBNull.Value)
                            packing = Convert.ToDouble(newRow["Packing"]);
                        if (newRow["Quantity"] != null && newRow["Quantity"] != DBNull.Value)
                            quantity = Convert.ToDouble(newRow["Quantity"]);

                        // Calculate Amount: Cost × Packing × Quantity
                        if (cost > 0 && packing > 0 && quantity > 0)
                        {
                            decimal amount = cost * Convert.ToDecimal(packing) * (decimal)quantity;
                            newRow["Amount"] = Math.Round(amount, 2);
                        }
                        else if (purchaseItemsData.Columns.Contains("Amount") && purchaseItemsData.Rows[i]["Amount"] != DBNull.Value)
                        {
                            // Fallback: use Amount from source if calculation not possible
                            newRow["Amount"] = purchaseItemsData.Rows[i]["Amount"];
                        }

                        // Copy TaxType and TaxPer - try multiple column name variations
                        string taxTypeStr = "";
                        decimal taxPer = 0m;

                        // Try to get TaxType from source
                        if (existingData.Columns.Contains("TaxType"))
                        {
                            if (purchaseItemsData.Columns.Contains("TaxType") && purchaseItemsData.Rows[i]["TaxType"] != DBNull.Value)
                            {
                                taxTypeStr = purchaseItemsData.Rows[i]["TaxType"].ToString().Trim().ToUpper();

                                // Convert to display format: "I" -> "Incl", "E" -> "Excl"
                                if (taxTypeStr == "I")
                                    newRow["TaxType"] = "Incl";
                                else if (taxTypeStr == "E")
                                    newRow["TaxType"] = "Excl";
                                else
                                    newRow["TaxType"] = taxTypeStr; // Keep original if not I or E
                            }
                        }

                        // Try to get TaxPer from source (check multiple possible column names)
                        if (existingData.Columns.Contains("TaxPer"))
                        {
                            if (purchaseItemsData.Columns.Contains("TaxPer") && purchaseItemsData.Rows[i]["TaxPer"] != DBNull.Value)
                            {
                                taxPer = Convert.ToDecimal(purchaseItemsData.Rows[i]["TaxPer"]);
                                newRow["TaxPer"] = taxPer;
                            }
                            else if (purchaseItemsData.Columns.Contains("TaxPercentage") && purchaseItemsData.Rows[i]["TaxPercentage"] != DBNull.Value)
                            {
                                taxPer = Convert.ToDecimal(purchaseItemsData.Rows[i]["TaxPercentage"]);
                                newRow["TaxPer"] = taxPer;
                            }
                        }

                        // Recalculate TaxAmt using the correct formula: (Cost × Qty × Tax%) / (100 + Tax%)
                        if (existingData.Columns.Contains("TaxAmt"))
                        {
                            // Normalize tax type - handle both "I"/"IN"/"Incl" from source and "Incl"/"Excl" from display
                            string normalizedTaxType = "";
                            if (!string.IsNullOrEmpty(taxTypeStr))
                            {
                                normalizedTaxType = taxTypeStr.ToLower().Trim();
                            }
                            else if (newRow["TaxType"] != null && newRow["TaxType"] != DBNull.Value)
                            {
                                normalizedTaxType = newRow["TaxType"].ToString().ToLower().Trim();
                            }

                            // Check for inclusive tax - handle "i", "in", "incl", "inclusive"
                            bool isInclusiveTax = normalizedTaxType == "incl" ||
                                                  normalizedTaxType == "i" ||
                                                  normalizedTaxType == "in" ||
                                                  normalizedTaxType == "inclusive";

                            if (isInclusiveTax)
                            {
                                // For inclusive tax: Tax Amount = (Cost × Qty × Tax%) / (100 + Tax%)
                                double denominator = 100.0 + (double)taxPer;
                                if (denominator > 0 && taxPer > 0 && cost > 0 && quantity > 0)
                                {
                                    double calculatedTaxAmt = ((double)cost * quantity * (double)taxPer) / denominator;
                                    newRow["TaxAmt"] = Math.Round((decimal)calculatedTaxAmt, 2);
                                }
                                else
                                {
                                    newRow["TaxAmt"] = 0m;
                                }
                            }
                            else
                            {
                                // For exclusive tax: Tax Amount = (Cost × Qty × Tax%) / 100
                                if (taxPer > 0 && cost > 0 && quantity > 0)
                                {
                                    double calculatedTaxAmt = ((double)cost * quantity * (double)taxPer) / 100.0;
                                    newRow["TaxAmt"] = Math.Round((decimal)calculatedTaxAmt, 2);
                                }
                                else
                                {
                                    newRow["TaxAmt"] = 0m;
                                }
                            }
                        }

                        // Always set Returned qty to 0.00 when loading new items - user will enter the return qty
                        if (existingData.Columns.Contains("Returned qty"))
                        {
                            newRow["Returned qty"] = 0.00;
                        }

                        // Copy Returned from source if available (for previously saved returns), otherwise set to 0.00
                        if (existingData.Columns.Contains("Returned"))
                        {
                            if (purchaseItemsData.Columns.Contains("Returned") && purchaseItemsData.Rows[i]["Returned"] != DBNull.Value)
                                newRow["Returned"] = purchaseItemsData.Rows[i]["Returned"];
                            else if (purchaseItemsData.Columns.Contains("ReturnedAmt") && purchaseItemsData.Rows[i]["ReturnedAmt"] != DBNull.Value)
                                newRow["Returned"] = purchaseItemsData.Rows[i]["ReturnedAmt"];
                            else
                            {
                                // Set default value to 0.00 if not present in source
                                newRow["Returned"] = 0.00m;
                            }
                        }

                        // Add the row to the formatted table
                        existingData.Rows.Add(newRow);
                    }

                    // Set the data source for ultraGrid1
                    ultraGrid1.DataSource = existingData;

                    // Removed tax recalculation - preserve exact TaxAmt values from database
                    // All tax values come directly from source data without recalculation

                    // Configure grid layout to ensure only requested columns are visible
                    // Configure grid layout to ensure only requested columns are visible
                    // ConfigureItemsGridLayout(); // Removed duplicate call, called later after UpdateNetAmount

                    // Update the Net Amount label
                    UpdateNetAmount();

                    // Apply our grid layout to ensure only the specified columns are visible
                    ConfigureItemsGridLayout();

                    // Make sure reason values are preserved
                    PreserveReasonValues();

                    // Check if this was called from btnAddPurchaceList
                    if (isCalledFromBtnAddPurchaceList)
                    {
                        // Set focus to the first row's Description cell
                        SetFocusToFirstRow();
                    }
                    else
                    {
                        // For other cases, use the original behavior
                        SetFocusToDescription();
                    }

                    // Set the flag to indicate purchase data is loaded
                    _isPurchaseDataLoaded = true;
                }
                else
                {
                    // If no items found, show a message
                    MessageBox.Show("No items found for this purchase.",
                        "No Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Check if this method was called from btnAddPurchaceList
                bool isCalledFromPurchaceList = new System.Diagnostics.StackTrace().GetFrames()
                    .Any(frame => frame.GetMethod().Name == "btnAddPurchaceList_Click");

                // If called from btnAddPurchaceList, keep TxtBarcode read-only
                // Otherwise, ensure it's writable
                if (!isCalledFromPurchaceList)
                {
                    // Ensure TxtBarcode is always writable
                    EnsureTxtBarcodeIsWritable();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase items: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }

        // Helper method to set focus to the Description column of the first row in the grid
        private void SetFocusToFirstRow()
        {
            try
            {
                // Make sure the grid has focus and there are rows
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Get the first row in the grid
                    UltraGridRow firstRow = ultraGrid1.Rows[0];

                    // Set this row as the active row and clear any existing selection
                    ultraGrid1.ActiveRow = firstRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(firstRow);

                    // Always focus specifically on the Description column (Item Name) as requested
                    if (firstRow.Cells.Exists("Description"))
                    {
                        ultraGrid1.ActiveCell = firstRow.Cells["Description"];

                        // Set focus to the grid and enter edit mode
                        ultraGrid1.Focus();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                        // Explicitly show highlighting or cursor to indicate the focus is set
                        if (ultraGrid1.ActiveCell != null)
                        {
                            ultraGrid1.ActiveCell.Appearance.BackColor = Color.FromArgb(255, 233, 233);
                            ultraGrid1.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting focus to first row Description: " + ex.Message);
            }
        }

        private void SetFocusToDescription()
        {
            try
            {
                // Make sure the grid has focus and there is an active row
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Get the last row in the grid instead of the active row
                    UltraGridRow activeRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];

                    // Set this row as the active row and clear any existing selection
                    ultraGrid1.ActiveRow = activeRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(activeRow);

                    // Set focus to the Description cell if it exists
                    if (activeRow.Cells.Exists("Description"))
                    {
                        ultraGrid1.ActiveCell = activeRow.Cells["Description"];

                        // Set focus to the grid and enter edit mode
                        ultraGrid1.Focus();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting focus to Description: " + ex.Message);
            }
        }

        // Method to get purchase items data
        private DataTable GetPurchaseItems(int ledgerId, int purchaseNo)
        {
            DataTable dt = new DataTable();

            try
            {
                // Create a BaseRepostitory instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use the repository to get purchase items data with tax information
                using (System.Data.SqlClient.SqlConnection conn = (System.Data.SqlClient.SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();

                    // Query to get purchase items with tax details from PDetails table
                    // Include Returned column to show previously returned quantities
                    string query = @"
                        SELECT 
                            Pd.SlNo, 
                            Pd.ItemID, 
                            ISNULL(Im.[Description], '') as Description,
                            ISNULL(Ps.BarCode, '') as BarCode, 
                            Pd.UnitId, 
                            Pd.Unit, 
                            Pd.Packing, 
                            Pd.Qty, 
                            Pd.Cost,
                            ISNULL(Pd.TaxType, 'I') as TaxType,
                            ISNULL(Pd.TaxPer, 0) as TaxPer,
                            ISNULL(Pd.TaxAmt, 0) as TaxAmt,
                            ISNULL((Pd.Cost * Pd.Packing * Pd.Qty), 0) as Amount,
                            ISNULL(Pd.Returned, 0) as Returned,
                            '' as Reason
                        FROM PDetails as Pd 
                        LEFT JOIN ItemMaster as Im ON(Pd.ItemID = Im.ItemId)
                        LEFT JOIN PriceSettings as Ps ON(Pd.ItemID = Ps.ItemId)
                        WHERE Pd.PurchaseNo = @PurchaseNo 
                            AND (Ps.UnitId IS NULL OR Pd.UnitId = Ps.UnitId)
                        ORDER BY Pd.SlNo";

                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);

                        using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dt;
        }

        // Method to format the grid with initial columns
        private void FormatGrid()
        {
            try
            {
                // Create a DataTable to hold the grid data
                DataTable dt = new DataTable();
                dt.Columns.Add("Sl No", typeof(int));
                dt.Columns.Add("Description", typeof(string)); // Item Name
                dt.Columns.Add("Barcode", typeof(string));
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("Packing", typeof(double));
                dt.Columns.Add("Cost", typeof(decimal));
                dt.Columns.Add("Quantity", typeof(double));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Returned", typeof(decimal));
                dt.Columns.Add("Returned qty", typeof(double));
                dt.Columns.Add("Reason", typeof(string));
                dt.Columns.Add("TaxType", typeof(string));
                dt.Columns.Add("TaxPer", typeof(decimal));
                dt.Columns.Add("TaxAmt", typeof(decimal));
                dt.Columns.Add("ItemID", typeof(long));
                dt.Columns.Add("UnitId", typeof(int));
                dt.Columns.Add("SELECT", typeof(bool));

                // Set the DataSource
                ultraGrid1.DataSource = dt;

                // Configure the grid layout (columns and styling)
                ConfigureItemsGridLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error formatting grid: " + ex.Message);
            }
        }

        // Method to configure the items grid layout
        private void ConfigureItemsGridLayout()
        {
            try
            {
                // Define colors for styling
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders and grid lines
                Color selectedBlue = Color.FromArgb(173, 216, 255); // Light blue for selection
                Color lightYellow = Color.FromArgb(255, 255, 224); // Light yellow for read-only historical data
                Color lightGreen = Color.FromArgb(224, 255, 224); // Light green for user-entered current values

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Apply compact design
                ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 22;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellPadding = 2;
                // Apply unified theme
                ApplyUnifiedGridTheme(ultraGrid1);

                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                    // Configure columns array for visible columns - ONLY include the requested columns
                    string[] visibleColumns = new string[] {
                        "Sl No", "Description", "Barcode", "Unit", "Packing", "Cost",
                        "Quantity", "Returned qty", "Reason", "Returned", "Amount", "TaxType", "TaxPer", "TaxAmt", "SELECT"
                    };

                    // Configure hidden columns - these are important for data storage but not shown in UI
                    string[] hiddenColumns = new string[] {
                        "ItemID", "UnitId"
                    };

                    // Set default vertical alignment for all cells
                    ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                    // First, hide ALL columns
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        col.Hidden = true;
                    }

                    // Ensure the SELECT column exists
                    if (!band.Columns.Exists("SELECT"))
                    {
                        band.Columns.Add("SELECT", ""); // Remove the caption text

                        // If the data source is a DataTable, make sure it has the SELECT column
                        if (ultraGrid1.DataSource is DataTable dt && !dt.Columns.Contains("SELECT"))
                        {
                            try
                            {
                                dt.Columns.Add("SELECT", typeof(bool));
                                // Set default values
                                foreach (DataRow row in dt.Rows)
                                {
                                    row["SELECT"] = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Error adding SELECT column to DataTable: " + ex.Message);
                            }
                        }
                    }

                    // Then, set up the visible columns in the specified order
                    foreach (string columnName in visibleColumns)
                    {
                        if (band.Columns.Exists(columnName))
                        {
                            UltraGridColumn column = band.Columns[columnName];

                            // Make column visible
                            column.Hidden = false;
                            column.CellActivation = Activation.AllowEdit;
                            column.CellAppearance.TextVAlign = VAlign.Middle;

                            // Explicitly set the display position based on the order in visibleColumns
                            int positionIndex = Array.IndexOf(visibleColumns, columnName);
                            column.Header.VisiblePosition = positionIndex;

                            // Configure specific columns
                            switch (columnName)
                            {
                                case "SELECT":
                                    column.Width = 70;
                                    column.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                                    column.Header.Caption = ""; // Remove header text
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    column.DataType = typeof(bool);
                                    column.DefaultCellValue = false;

                                    // Always set up the header checkbox regardless of whether it's visible
                                    column.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;
                                    column.Header.CheckBoxAlignment = HeaderCheckBoxAlignment.Center;
                                    break;

                                case "Sl No":
                                    column.Width = 100;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "SlNo";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    break;

                                case "Description":
                                    column.Width = 200;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "Item Name";
                                    break;

                                case "Barcode":
                                    column.Width = 100;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "Barcode";
                                    break;

                                case "Unit":
                                    column.Width = 80;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "Unit";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    break;

                                case "Packing":
                                    column.Width = 100;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "Packing";
                                    column.CellAppearance.TextHAlign = HAlign.Right;
                                    column.Format = "N2";
                                    break;

                                case "Cost":
                                    column.Width = 100;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Format = "N2";
                                    column.Header.Caption = "Cost";
                                    column.CellAppearance.TextHAlign = HAlign.Right;
                                    break;

                                case "Quantity":
                                    column.Width = 80;
                                    column.CellActivation = Activation.NoEdit; // Read-only - original purchased quantity
                                    column.Header.Caption = "Qty";
                                    column.CellAppearance.TextHAlign = HAlign.Right;
                                    column.Format = "N2";
                                    break;

                                case "Returned qty":
                                    column.Width = 100;
                                    column.CellActivation = Activation.AllowEdit; // Editable - current return quantity
                                    column.Format = "N2";
                                    column.Header.Caption = "Return Qty";
                                    column.CellAppearance.TextHAlign = HAlign.Right;
                                    break;

                                case "Returned":
                                    column.Width = 100;
                                    column.CellActivation = Activation.NoEdit; // Read-only - calculated from Returned qty * Cost
                                    column.Format = "N2";
                                    column.Header.Caption = "Returned";
                                    column.CellAppearance.TextHAlign = HAlign.Right;
                                    break;

                                case "Amount":
                                    column.Width = 100;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Format = "N2";
                                    column.Header.Caption = "Amount";
                                    column.CellAppearance.TextHAlign = HAlign.Right;
                                    break;

                                case "TaxType":
                                    column.Width = 80;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "Tax Type";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    // TaxType is stored as "Incl" or "Excl" in the DataTable (converted from "I"/"E" when loading)
                                    break;

                                case "TaxPer":
                                    column.Width = 80;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Format = "N2";
                                    column.Header.Caption = "Tax %";
                                    column.CellAppearance.TextHAlign = HAlign.Right;
                                    break;

                                case "TaxAmt":
                                    column.Width = 100;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Format = "N2";
                                    column.Header.Caption = "Tax Amount";
                                    column.CellAppearance.TextHAlign = HAlign.Right;
                                    break;

                                case "Reason":
                                    column.Width = 150;
                                    column.CellActivation = Activation.AllowEdit;
                                    column.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;
                                    column.Header.Caption = "Reason *"; // Add asterisk to indicate mandatory field
                                    column.CellAppearance.TextHAlign = HAlign.Center;

                                    // Create ValueList for Reason dropdown with valid options only
                                    Infragistics.Win.ValueList reasonList = new Infragistics.Win.ValueList();
                                    // Remove the 'Select Reason' option since we're making this mandatory
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Expired"));
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Damaged"));
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("NonOrdered"));
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("NonDemand"));

                                    column.ValueList = reasonList;
                                    column.CellAppearance.TextHAlign = HAlign.Left;

                                    // Make sure that the reason values from the database are respected
                                    // The dropdown style is already set by using ColumnStyle.DropDownList above

                                    // Log the current values for debugging
                                    System.Diagnostics.Debug.WriteLine("Current reason values in grid:");
                                    if (ultraGrid1.Rows.Count > 0)
                                    {
                                        foreach (UltraGridRow row in ultraGrid1.Rows)
                                        {
                                            if (row.Cells.Exists("Reason"))
                                            {
                                                object reasonValue = row.Cells["Reason"].Value;
                                                System.Diagnostics.Debug.WriteLine($"Row {row.Index}: Reason = {reasonValue}");
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    // Ensure ItemID is always hidden 
                    if (band.Columns.Exists("ItemID"))
                    {
                        band.Columns["ItemID"].Hidden = true;
                    }

                    // Apply SalesReturn grid styling
                    // Basic grid behavior
                    ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                    ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                    ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                    ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                    ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                    ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                    ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                    // Basic grid behavior
                    ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
                    ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
                    ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                    ultraGrid1.DisplayLayout.Scrollbars = Scrollbars.Both;

                    // Configure header appearance matching Image 2 flat blue header theme
                    ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(93, 151, 214);
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(67, 118, 184);
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                    // Configure row appearance
                    ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = SystemColors.Menu;
                    ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.Vertical;

                    // Set consistent row spacing to match other forms (2 pixels)
                    ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 2;
                    ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                    ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 22;

                    // Configure spacing and expansion behavior
                    ultraGrid1.DisplayLayout.InterBandSpacing = 0;
                    ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

                    // Register event handlers for cell editing
                    ultraGrid1.BeforeEnterEditMode -= ultraGrid1_BeforeEnterEditMode;
                    ultraGrid1.BeforeEnterEditMode += ultraGrid1_BeforeEnterEditMode;

                    ultraGrid1.BeforeCellUpdate -= ultraGrid1_BeforeCellUpdate;
                    ultraGrid1.BeforeCellUpdate += ultraGrid1_BeforeCellUpdate;

                    ultraGrid1.KeyDown -= ultraGrid1_KeyDown;
                    ultraGrid1.KeyDown += ultraGrid1_KeyDown;

                    ultraGrid1.BeforeExitEditMode -= ultraGrid1_BeforeExitEditMode;
                    ultraGrid1.BeforeExitEditMode += ultraGrid1_BeforeExitEditMode;

                    // Register the InitializeLayout event for styling
                    ultraGrid1.InitializeLayout -= ultraGrid1_InitializeLayout;
                    ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;

                    // The following lines will now be handled by PreserveReasonValues after the grid is configured
                    PreserveReasonValues();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error configuring grid layout: " + ex.Message);
            }
        }

        // Method to ensure that any existing Reason values from the database are preserved
        private void PreserveReasonValues()
        {
            try
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Preserving Reason values in grid...");

                    // Store the current reason values
                    var reasonValues = new Dictionary<int, string>();

                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("Reason") && row.Cells["Reason"].Value != null)
                        {
                            string currentReason = row.Cells["Reason"].Value.ToString();
                            if (!string.IsNullOrWhiteSpace(currentReason) && currentReason != "Select Reason")
                            {
                                reasonValues[row.Index] = currentReason;
                                System.Diagnostics.Debug.WriteLine($"Storing reason '{currentReason}' for row {row.Index}");
                            }
                        }
                    }

                    // Temporarily store current appearance settings
                    var currentAppearanceSettings = new Dictionary<int, object>();
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("Reason"))
                        {
                            // Store current appearance of the Reason cell
                            currentAppearanceSettings[row.Index] = row.Cells["Reason"].Appearance.BackColor;
                        }
                    }

                    // Now re-apply the values and ensure they're displayed correctly
                    foreach (var kvp in reasonValues)
                    {
                        int rowIndex = kvp.Key;
                        string reasonValue = kvp.Value;

                        if (ultraGrid1.Rows.Count > rowIndex)
                        {
                            UltraGridRow row = ultraGrid1.Rows[rowIndex];
                            if (row.Cells.Exists("Reason"))
                            {
                                // Set the reason value
                                row.Cells["Reason"].Value = reasonValue;
                                System.Diagnostics.Debug.WriteLine($"Restored reason '{reasonValue}' for row {rowIndex}");

                                // Reset cell appearance to normal
                                row.Cells["Reason"].Appearance.BackColor = Color.Empty;
                                row.Cells["Reason"].Appearance.ForeColor = Color.Empty;
                            }
                        }
                    }

                    // Refresh the grid to ensure changes are visible
                    ultraGrid1.Refresh();
                    System.Diagnostics.Debug.WriteLine("Reason values preservation complete");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error preserving reason values: " + ex.Message);
            }
        }

        private void ultraGrid1_BeforeEnterEditMode(object sender, CancelEventArgs e)
        {
            // Store the original value before entering edit mode
            if (ultraGrid1.ActiveCell != null)
            {
                _originalValue = ultraGrid1.ActiveCell.Value;
            }
        }

        private void ultraGrid1_BeforeExitEditMode(object sender, CancelEventArgs e)
        {
            // Track if the value was modified during edit (for Cost and Quantity cells)
            if (ultraGrid1.ActiveCell != null &&
                (ultraGrid1.ActiveCell.Column.Key == "Cost" || ultraGrid1.ActiveCell.Column.Key == "Quantity"))
            {
                // Compare original and new values
                bool valueChanged = false;

                if (_originalValue == null && ultraGrid1.ActiveCell.Value != null)
                    valueChanged = true;
                else if (_originalValue != null && ultraGrid1.ActiveCell.Value == null)
                    valueChanged = true;
                else if (_originalValue != null && ultraGrid1.ActiveCell.Value != null &&
                         !_originalValue.ToString().Equals(ultraGrid1.ActiveCell.Value.ToString()))
                    valueChanged = true;

                if (valueChanged)
                {
                    _valueEditedAfterEnter = true;
                    System.Diagnostics.Debug.WriteLine($"Value changed in {ultraGrid1.ActiveCell.Column.Key} from {_originalValue} to {ultraGrid1.ActiveCell.Value}");
                }
            }
        }

        private object _originalValue = null;
        private bool _enterPressed = false; // Used in BeforeCellUpdate to control cell updates
        private bool _isUpdatingCells = false; // Add flag to prevent recursive updates
        private DateTime _lastEnterPress = DateTime.MinValue; // Track time of last Enter key press
        private UltraGridCell _lastEnterCell = null; // Track the cell where Enter was last pressed
        private bool _valueEditedAfterEnter = false; // Track if value was edited after Enter press

        private void ultraGrid1_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            // Validate Returned qty against actual Quantity
            if (e.Cell.Column.Key == "Returned qty")
            {
                try
                {
                    // Get the new value being entered
                    double newReturnQty = 0;
                    if (e.NewValue != null && e.NewValue != DBNull.Value)
                    {
                        if (!double.TryParse(e.NewValue.ToString(), out newReturnQty))
                        {
                            // Invalid numeric value
                            MessageBox.Show("Please enter a valid numeric value for Return Quantity.",
                                "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            e.Cancel = true;
                            return;
                        }
                    }

                    // Get the actual Quantity and already Returned quantity from the same row
                    UltraGridRow row = e.Cell.Row;
                    if (row != null && row.Cells.Exists("Quantity"))
                    {
                        double actualQty = 0;
                        if (row.Cells["Quantity"].Value != null && row.Cells["Quantity"].Value != DBNull.Value)
                        {
                            if (!double.TryParse(row.Cells["Quantity"].Value.ToString(), out actualQty))
                            {
                                // Quantity value is invalid, allow the update (shouldn't happen normally)
                                return;
                            }
                        }

                        // Get already returned quantity (from "Returned" column - cumulative returned so far)
                        double alreadyReturnedQty = 0;
                        if (row.Cells.Exists("Returned") && row.Cells["Returned"].Value != null && row.Cells["Returned"].Value != DBNull.Value)
                        {
                            double.TryParse(row.Cells["Returned"].Value.ToString(), out alreadyReturnedQty);
                        }

                        // Calculate available quantity: Original Quantity - Already Returned
                        double availableQty = actualQty - alreadyReturnedQty;

                        // Validate: New return qty cannot exceed available quantity
                        if (newReturnQty > availableQty)
                        {
                            // Get item description for better error message
                            string itemDescription = "Item";
                            if (row.Cells.Exists("Description") && row.Cells["Description"].Value != null)
                            {
                                itemDescription = row.Cells["Description"].Value.ToString();
                            }

                            MessageBox.Show($"Return Quantity ({newReturnQty:N2}) cannot exceed the available quantity ({availableQty:N2}) for {itemDescription}.\n\n" +
                                $"Original Quantity: {actualQty:N2}\n" +
                                $"Already Returned: {alreadyReturnedQty:N2}\n" +
                                $"Available to Return: {availableQty:N2}\n\n" +
                                $"Please enter a value less than or equal to {availableQty:N2}.",
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            e.Cancel = true;
                            return;
                        }

                        // Additional validation: Ensure new return qty is not negative
                        if (newReturnQty < 0)
                        {
                            MessageBox.Show("Return Quantity cannot be negative. Please enter a value greater than or equal to 0.",
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error validating Returned qty: {ex.Message}");
                    // On error, allow the update to proceed (fail-safe)
                }
            }

            // For other editable columns, always allow updates
            if (e.Cell.Column.Key == "Cost" ||
                e.Cell.Column.Key == "Quantity" ||
                e.Cell.Column.Key == "Reason")
            {
                return; // Don't cancel, allow the update
            }

            // For other columns, use the default behavior
            // This allows Amount to be updated when Cost and Quantity change
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // Handle Down Arrow key specifically
                if (e.KeyCode == Keys.Down)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    UltraGridRow currentRow = ultraGrid1.ActiveRow;
                    if (currentRow != null && currentRow.Index < ultraGrid1.Rows.Count - 1)
                    {
                        UltraGridRow nextRow = ultraGrid1.Rows[currentRow.Index + 1];
                        string currentColumnKey = ultraGrid1.ActiveCell.Column.Key;

                        // Keep focus in the same column when moving down
                        if (nextRow.Cells.Exists(currentColumnKey))
                        {
                            ultraGrid1.ActiveRow = nextRow;
                            ultraGrid1.ActiveCell = nextRow.Cells[currentColumnKey];
                            ultraGrid1.Selected.Rows.Clear();
                            ultraGrid1.Selected.Rows.Add(nextRow);

                            // Enter edit mode if appropriate
                            if (nextRow.Cells[currentColumnKey].Column.CellActivation == Activation.AllowEdit)
                            {
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }
                    }
                    return;
                }

                // Handle Up Arrow key specifically
                if (e.KeyCode == Keys.Up)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    UltraGridRow currentRow = ultraGrid1.ActiveRow;
                    if (currentRow != null && currentRow.Index > 0)
                    {
                        UltraGridRow prevRow = ultraGrid1.Rows[currentRow.Index - 1];
                        string currentColumnKey = ultraGrid1.ActiveCell.Column.Key;

                        // Keep focus in the same column when moving up
                        if (prevRow.Cells.Exists(currentColumnKey))
                        {
                            ultraGrid1.ActiveRow = prevRow;
                            ultraGrid1.ActiveCell = prevRow.Cells[currentColumnKey];
                            ultraGrid1.Selected.Rows.Clear();
                            ultraGrid1.Selected.Rows.Add(prevRow);

                            // Enter edit mode if appropriate
                            if (prevRow.Cells[currentColumnKey].Column.CellActivation == Activation.AllowEdit)
                            {
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }
                    }
                    // If at the first row, keep the focus on the first row
                    return;
                }

                if (e.KeyCode == Keys.Enter && !e.Control && !e.Alt && !e.Shift)
                {
                    // Mark the event as handled to prevent default enter behavior
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    // Set the _enterPressed flag to true
                    _enterPressed = true;

                    if (ultraGrid1.ActiveCell != null)
                    {
                        UltraGridRow currentRow = ultraGrid1.ActiveRow;
                        string currentColumn = ultraGrid1.ActiveCell.Column.Key;

                        // Store current cell before exiting edit mode
                        UltraGridCell currentActiveCell = ultraGrid1.ActiveCell;

                        // First, exit edit mode to apply any pending changes
                        if (ultraGrid1.ActiveCell.IsInEditMode)
                        {
                            // Force exit edit mode to apply pending changes
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);

                            // Important: Allow the UI to update
                            Application.DoEvents();
                        }

                        // Removed all Amount calculations - no automatic calculations
                        if ((currentColumn == "Cost" || currentColumn == "Quantity") &&
                            currentRow != null)
                        {
                            // Removed calculation - Amount is not auto-calculated

                            // Check if this is the second Enter press in the same cell AND no value change since last Enter
                            bool isSameCell = (_lastEnterCell == currentActiveCell);
                            DateTime now = DateTime.Now;
                            TimeSpan timeSinceLastEnter = now - _lastEnterPress;

                            // If this is the second Enter press in the same cell (within 2 seconds) AND no value change since last Enter
                            if (isSameCell && timeSinceLastEnter.TotalSeconds <= 2 &&
                                _lastEnterPress != DateTime.MinValue && !_valueEditedAfterEnter)
                            {
                                // Reset tracking variables
                                _lastEnterPress = DateTime.MinValue;
                                _lastEnterCell = null;
                                _valueEditedAfterEnter = false;

                                // Move to the next logical cell
                                string nextColumn = GetNextColumnForNavigation(currentColumn);
                                if (currentRow.Cells.Exists(nextColumn))
                                {
                                    ultraGrid1.ActiveCell = currentRow.Cells[nextColumn];
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                    _enterPressed = false; // Reset flag before returning
                                    return;
                                }
                            }
                            else
                            {
                                // This is the first Enter press or value was edited - update tracking variables
                                _lastEnterCell = currentActiveCell;
                                _lastEnterPress = now;
                                // Reset the value edited flag for next time
                                _valueEditedAfterEnter = false;

                                // Stay in the current cell
                                ultraGrid1.ActiveCell = currentActiveCell;
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                _enterPressed = false; // Reset flag before returning
                                return;
                            }
                        }
                        else
                        {
                            // Removed all Amount calculations - no automatic calculations

                            // For non-Cost/Quantity cells, move to next cell immediately on first Enter
                            // Standard navigation logic
                            if (currentColumn == "Reason" && currentRow.Index < ultraGrid1.Rows.Count - 1)
                            {
                                // Move to the first cell of the next row
                                UltraGridRow nextRow = ultraGrid1.Rows[currentRow.Index + 1];
                                UltraGridCell firstCell = FindFirstEditableCell(nextRow);
                                if (firstCell != null)
                                {
                                    ultraGrid1.ActiveRow = nextRow;
                                    ultraGrid1.ActiveCell = firstCell;
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                }
                            }
                            else
                            {
                                // Move to next cell in standard navigation order
                                string nextColumn = GetNextColumnForNavigation(currentColumn);
                                if (currentRow.Cells.Exists(nextColumn))
                                {
                                    ultraGrid1.ActiveCell = currentRow.Cells[nextColumn];
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                }
                            }
                        }
                    }

                    // Reset the _enterPressed flag before returning
                    _enterPressed = false;
                    return;
                }

                // Track numeric key presses in Cost/Quantity/Returned qty cells to reset the valueEdited flag
                if (ultraGrid1.ActiveCell != null &&
                    (ultraGrid1.ActiveCell.Column.Key == "Cost" ||
                     ultraGrid1.ActiveCell.Column.Key == "Quantity" ||
                     ultraGrid1.ActiveCell.Column.Key == "Returned qty") &&
                    (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9 ||
                     e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9 ||
                     e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete ||
                     e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod))
                {
                    // User is typing in the cell, set the valueEdited flag
                    _valueEditedAfterEnter = true;
                }

                // Handle Tab navigation
                else if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt)
                {
                    if (ultraGrid1.ActiveCell != null)
                    {
                        // End edit mode first to apply any changes
                        if (ultraGrid1.ActiveCell.IsInEditMode)
                        {
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                        }

                        // Removed all Amount calculations - no automatic calculations

                        // Find next/previous cell based on Shift state
                        UltraGridCell nextCell = null;
                        UltraGridRow currentRow = ultraGrid1.ActiveRow;

                        if (e.Shift)
                        {
                            // Shift+Tab - Move to previous cell
                            nextCell = FindPreviousEditableCell(currentRow, ultraGrid1.ActiveCell);

                            // If we're at the first cell of the current row, try to move to the last cell of the previous row
                            if (nextCell == null || nextCell == ultraGrid1.ActiveCell)
                            {
                                if (currentRow.Index > 0)
                                {
                                    UltraGridRow prevRow = ultraGrid1.Rows[currentRow.Index - 1];
                                    nextCell = FindLastEditableCell(prevRow);
                                }
                                else
                                {
                                    // If we're at the first cell of the first row, let form handle focus cycling
                                    e.Handled = true;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            // Regular Tab - Move to next cell
                            nextCell = FindNextEditableCell(currentRow, ultraGrid1.ActiveCell);

                            // If we're at the last cell of the current row, try to move to the first cell of the next row
                            if (nextCell == null || nextCell == ultraGrid1.ActiveCell)
                            {
                                if (currentRow.Index < ultraGrid1.Rows.Count - 1)
                                {
                                    UltraGridRow nextRow = ultraGrid1.Rows[currentRow.Index + 1];
                                    nextCell = FindFirstEditableCell(nextRow);
                                }
                                else
                                {
                                    // If we're at the last cell of the last row, let form handle focus cycling
                                    e.Handled = true;
                                    return;
                                }
                            }
                        }

                        // Move focus to the next/previous cell
                        if (nextCell != null)
                        {
                            ultraGrid1.ActiveCell = nextCell;
                            ultraGrid1.ActiveRow = nextCell.Row;
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            e.Handled = true;
                        }
                    }
                }
                // Handle Delete key for removing rows
                else if (e.KeyCode == Keys.Delete)
                {
                    UltraGridRow activeRow = ultraGrid1.ActiveRow;
                    if (activeRow != null)
                    {
                        // Check if items are loaded via button1 (purchasereturnupdate)
                        if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                        {
                            MessageBox.Show("Cannot remove items when purchase return data is loaded from update form.",
                                "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Delete the row
                        if (ultraGrid1.DataSource is DataTable dt)
                        {
                            int rowIndex = activeRow.Index;
                            dt.Rows.RemoveAt(rowIndex);

                            // Renumber Sl No column for remaining rows
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                dt.Rows[i]["Sl No"] = i + 1;
                            }

                            // Update amount calculations
                            UpdateNetAmount();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in ultraGrid1_KeyDown: " + ex.Message);
            }
        }

        private void ultraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            UltraGridCell cell = e.Cell;
            UltraGridRow row = e.Cell.Row;
            string columnKey = e.Cell.Column.Key;

            _isUpdatingCells = true; // Set flag to prevent recursion

            try
            {
                // Capture the value in case we need to revert it
                var newValue = cell.Value;

                // Handle checkboxes in the SELECT column
                if (columnKey == "SELECT")
                {
                    try
                    {
                        bool isSelected = false;
                        if (cell.Value != null && cell.Value != DBNull.Value)
                        {
                            isSelected = Convert.ToBoolean(cell.Value);
                        }

                        System.Diagnostics.Debug.WriteLine($"SELECT checkbox changed for row {row.Index}: {isSelected}");

                        // Handle row selection state
                        UpdateRowSelectionState(row, isSelected);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error in SELECT column handling: " + ex.Message);
                    }
                }
                // Handle updates to the Reason cell
                else if (columnKey == "Reason")
                {
                    try
                    {
                        // Log the change to the Reason cell
                        string newReason = cell.Value == null ? "" : cell.Value.ToString().Trim();
                        System.Diagnostics.Debug.WriteLine($"Reason updated for row {row.Index}: '{newReason}'");

                        // Reset the cell appearance now that user has entered a reason
                        if (!string.IsNullOrEmpty(newReason) && newReason != "Select Reason")
                        {
                            cell.Appearance.BackColor = Color.Empty;
                            cell.Appearance.ForeColor = Color.Empty;
                        }

                        // Removed auto-select checkbox behavior
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error in Reason column handling: " + ex.Message);
                    }
                }
                // Handle updates to Returned qty - calculate Amount = Returned qty * Cost
                else if (columnKey == "Returned qty")
                {
                    try
                    {
                        // Get Returned qty value
                        double returnedQty = 0;
                        if (cell.Value != null && cell.Value != DBNull.Value)
                        {
                            if (!double.TryParse(cell.Value.ToString(), out returnedQty))
                            {
                                returnedQty = 0;
                            }
                        }

                        // Get Cost value from the same row
                        decimal cost = 0m;
                        if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null && row.Cells["Cost"].Value != DBNull.Value)
                        {
                            if (!decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost))
                            {
                                cost = 0m;
                            }
                        }

                        // Calculate Amount = Returned qty * Cost
                        decimal newAmount = (decimal)returnedQty * cost;
                        newAmount = Math.Round(newAmount, 2);

                        // Update the Amount cell (only Amount cell, NOT Returned cell)
                        if (row.Cells.Exists("Amount"))
                        {
                            row.Cells["Amount"].Value = newAmount;
                            System.Diagnostics.Debug.WriteLine($"Updated Amount for row {row.Index}: Returned qty ({returnedQty}) * Cost ({cost}) = {newAmount}");
                        }

                        // Recalculate tax amount based on the new Amount
                        if (row.Cells.Exists("TaxPer") && row.Cells.Exists("TaxType") && row.Cells.Exists("TaxAmt"))
                        {
                            // Get tax percentage and type from the row
                            double taxPercentage = 0;
                            string taxType = "excl";

                            if (row.Cells["TaxPer"].Value != null && row.Cells["TaxPer"].Value != DBNull.Value)
                            {
                                if (!double.TryParse(row.Cells["TaxPer"].Value.ToString(), out taxPercentage))
                                {
                                    taxPercentage = 0;
                                }
                            }

                            if (row.Cells["TaxType"].Value != null && row.Cells["TaxType"].Value != DBNull.Value)
                            {
                                taxType = row.Cells["TaxType"].Value.ToString().Trim();
                            }

                            // Calculate new tax amount based on the new Amount
                            double newTaxAmt = CalculateTaxAmount((double)newAmount, taxPercentage, taxType);

                            // Update the TaxAmt cell
                            row.Cells["TaxAmt"].Value = Math.Round((decimal)newTaxAmt, 2);
                            System.Diagnostics.Debug.WriteLine($"Updated TaxAmt for row {row.Index}: {newTaxAmt} (based on Amount: {newAmount}, Tax%: {taxPercentage}, Type: {taxType})");
                        }

                        // Update subtotal and net amount
                        UpdateNetAmount();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error calculating Amount from Returned qty: " + ex.Message);
                    }
                }

                // If we navigate away from a cell by pressing Enter
                if (_enterPressed)
                {
                    _enterPressed = false;
                }
            }
            finally
            {
                _isUpdatingCells = false; // Reset flag
            }
        }

        // New method to update visual state when rows are selected or deselected
        private void UpdateRowSelectionState(UltraGridRow row, bool isSelected)
        {
            if (row == null) return;

            try
            {
                // Change appearance of important fields based on selection status
                if (row.Cells.Exists("Reason"))
                {
                    if (isSelected)
                    {
                        // If selected but no reason provided, highlight the Reason cell
                        string reasonValue = row.Cells["Reason"].Value != null ? row.Cells["Reason"].Value.ToString() : "";
                        if (string.IsNullOrWhiteSpace(reasonValue) || reasonValue == "Select Reason")
                        {
                            // Highlight the reason cell to indicate it needs attention
                            row.Cells["Reason"].Appearance.BackColor = Color.MistyRose;
                            row.Cells["Reason"].Appearance.ForeColor = Color.Red;
                            System.Diagnostics.Debug.WriteLine($"Row {row.Index} selected but needs a reason - highlighting Reason cell");
                        }
                        else
                        {
                            // Reason is provided, use normal appearance
                            row.Cells["Reason"].Appearance.BackColor = Color.Empty;
                            row.Cells["Reason"].Appearance.ForeColor = Color.Empty;
                            System.Diagnostics.Debug.WriteLine($"Row {row.Index} selected with reason '{reasonValue}' - normal appearance");
                        }
                    }
                    else
                    {
                        // If deselected, remove any highlighting
                        row.Cells["Reason"].Appearance.BackColor = Color.Empty;
                        row.Cells["Reason"].Appearance.ForeColor = Color.Empty;
                        System.Diagnostics.Debug.WriteLine($"Row {row.Index} deselected - resetting cell appearance");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateRowSelectionState: {ex.Message}");
            }
        }

        private void UpdateNetAmount()
        {
            try
            {
                // Calculate the net amount
                decimal netAmount = 0m;
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("Amount") &&
                        row.Cells["Amount"].Value != null &&
                        row.Cells["Amount"].Value != DBNull.Value)
                    {
                        netAmount += Convert.ToDecimal(row.Cells["Amount"].Value);
                    }
                }

                // Round the net amount to 2 decimal places
                netAmount = Math.Round(netAmount, 2);

                // Format the value with 2 decimal places
                string formattedAmount = netAmount.ToString("N2");

                // Update lblNetAmount
                if (lblNetAmount != null)
                {
                    lblNetAmount.Text = formattedAmount;
                    lblNetAmount.Refresh();
                }

                // Update textBox2 (purchase amount)
                if (textBox2 != null)
                {
                    textBox2.Text = formattedAmount;
                    textBox2.Refresh();
                }

                // Update TxtSubTotal to match textBox2
                if (TxtSubTotal != null)
                {
                    TxtSubTotal.Text = formattedAmount;
                    TxtSubTotal.Refresh();
                }

                // Update real-time outstanding balance on ultraTextEditor1 for the loaded GRN Bill
                UpdateBillOutstandingRealTime(netAmount);

                // Log the update to debug output
                System.Diagnostics.Debug.WriteLine($"Updated Net Amount: {formattedAmount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating net amount: " + ex.Message);
            }
        }

        private void pbxSave_Click(object sender, EventArgs e)
        {
            // Make sure this button stays visible when clicked
            pbxSave.Visible = true;
            ultraPictureBox4.Visible = false;

            SavePurchaseReturn();
        }

        public void Save()
        {
            bool isUpdateMode = ultraPictureBox4 != null && ultraPictureBox4.Visible;

            if (isUpdateMode)
            {
                ultraPictureBox4_Click(this, EventArgs.Empty);
                return;
            }

            pbxSave_Click(this, EventArgs.Empty);
        }

        private void ultraPictureBox1_Click(object sender, EventArgs e)
        {
            ClearForm();
            // Ensure focus is set to vendor button or appropriate control
            Control focusControl = this.Controls.Find("BtnDial", true).FirstOrDefault();
            if (focusControl == null)
            {
                focusControl = this.Controls.Find("Vendorbutton", true).FirstOrDefault();
            }
            if (focusControl == null && VendorName != null)
            {
                focusControl = VendorName;
            }
            if (focusControl != null)
            {
                focusControl.Focus();
            }
        }

        private void ultraPictureBox2_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we need to remove selected items from grid or delete an entire PR
                bool isPRUpdateMode = (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate");

                // Case 1: For items loaded via TxtBarcode, dialForItemMaster, or btnAddPurchaceList
                // We'll remove selected items from the grid
                if (!isPRUpdateMode)
                {
                    System.Diagnostics.Debug.WriteLine("Delete operation started - checking selection state");
                    System.Diagnostics.Debug.WriteLine($"Grid row count: {ultraGrid1.Rows.Count}");

                    // Make sure any active cell edits are committed
                    if (ultraGrid1.ActiveCell != null)
                    {
                        try
                        {
                            ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error committing cell edit: {ex.Message}");
                        }
                    }

                    // First ensure the grid data is synced to the DataTable
                    ultraGrid1.UpdateData();

                    // Check if any items are selected (checked)
                    bool hasSelectedItems = false;
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("SELECT") &&
                            row.Cells["SELECT"].Value != null &&
                            row.Cells["SELECT"].Value != DBNull.Value &&
                            (bool)row.Cells["SELECT"].Value == true)
                        {
                            hasSelectedItems = true;
                            break;
                        }
                    }

                    // If no items are selected via the grid's SELECT column, check the DataTable directly
                    if (!hasSelectedItems && ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                    {
                        DataTable dt = (DataTable)ultraGrid1.DataSource;
                        foreach (DataRow dataRow in dt.Rows)
                        {
                            if (dataRow.Table.Columns.Contains("SELECT") &&
                                dataRow["SELECT"] != DBNull.Value &&
                                Convert.ToBoolean(dataRow["SELECT"]) == true)
                            {
                                hasSelectedItems = true;
                                break;
                            }
                        }

                        // Special case: If there's only one item in the grid and it's not selected,
                        // select it automatically for convenience
                        if (!hasSelectedItems && dt.Rows.Count == 1 && dt.Columns.Contains("SELECT"))
                        {
                            System.Diagnostics.Debug.WriteLine("Only one item in grid - auto-selecting it");
                            dt.Rows[0]["SELECT"] = true;
                            hasSelectedItems = true;
                        }
                    }

                    if (!hasSelectedItems)
                    {
                        MessageBox.Show("Please select (check) the items you want to remove.",
                            "No Items Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Confirm deletion with the user
                    DialogResult response = MessageBox.Show("Are you sure you want to remove the selected items?",
                        "Confirm Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (response == DialogResult.No)
                        return;

                    try
                    {
                        // We need to modify the DataTable
                        if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                        {
                            DataTable dt = (DataTable)ultraGrid1.DataSource;

                            // Force update of the grid data to the DataTable before processing
                            ultraGrid1.UpdateData();

                            // Create a list to track rows to be removed
                            List<int> rowIndicesToRemove = new List<int>();

                            // First identify which rows need to be removed by checking the SELECT column
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                // Debug output to check values
                                object selectValue = dt.Rows[i]["SELECT"];
                                bool isSelected = selectValue != DBNull.Value && Convert.ToBoolean(selectValue);
                                System.Diagnostics.Debug.WriteLine($"Row {i} SELECT value: {selectValue}, isSelected: {isSelected}");

                                if (isSelected)
                                {
                                    rowIndicesToRemove.Add(i);
                                }
                            }

                            System.Diagnostics.Debug.WriteLine($"Found {rowIndicesToRemove.Count} rows to remove");

                            // Remove rows in reverse order to avoid index issues
                            for (int i = rowIndicesToRemove.Count - 1; i >= 0; i--)
                            {
                                int indexToRemove = rowIndicesToRemove[i];
                                dt.Rows.RemoveAt(indexToRemove);
                            }

                            // Now we need to renumber the Sl No column
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                dt.Rows[i]["Sl No"] = i + 1;
                            }

                            // Refresh the data source
                            dt.AcceptChanges();

                            // Update the Net Amount after removing items
                            UpdateNetAmount();

                            // Show a message with the count of removed items
                            MessageBox.Show($"{rowIndicesToRemove.Count} item(s) have been removed.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception during item removal: " + ex.ToString());
                        MessageBox.Show("Error removing items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Case 2: For items loaded via button1 (PR update)
                // We'll delete the entire PR record from the database
                else
                {
                    // Check if PR number is provided
                    if (string.IsNullOrEmpty(TxtSRNO.Text))
                    {
                        MessageBox.Show("Please enter a Purchase Return number first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Parse PR number
                    int prNo = 0;
                    if (!int.TryParse(TxtSRNO.Text.Trim(), out prNo))
                    {
                        MessageBox.Show("Invalid Purchase Return number format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"Attempting to delete PR #{prNo}");

                    // Confirm deletion with the user
                    DialogResult response = MessageBox.Show("Are you sure you want to delete this purchase return?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (response == DialogResult.No)
                        return;

                    // Set cursor to wait while we process
                    Cursor.Current = Cursors.WaitCursor;

                    try
                    {
                        // Use the comprehensive deletion method to ensure complete removal
                        string deleteResult = prRepo.DeletePRDirectSQL(prNo);

                        if (deleteResult.StartsWith("Error"))
                        {
                            MessageBox.Show("Failed to delete Purchase Return: " + deleteResult, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            SavePurchaseReturnActivityLog("DELETE", new PReturnMaster
                            {
                                PReturnNo = prNo,
                                VendorName = VendorName != null ? VendorName.Text : string.Empty,
                                GrandTotal = Convert.ToDouble(decimal.TryParse(lblNetAmount != null ? lblNetAmount.Text : "0", out decimal _prNetTotal) ? _prNetTotal : 0m)
                            });

                            MessageBox.Show("Purchase Return deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // If this PR was loaded from PurchaseReturnUpdate form, clear the Tag flag
                            if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                            {
                                this.Tag = null;
                                _isPurchaseDataLoaded = false;
                            }

                            // Refresh any PurchaseReturnUpdate form that might be open
                            // This ensures the deleted PR doesn't appear in other forms
                            RefreshPurchaseReturnUpdateForms(prNo);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception during PR deletion: " + ex.ToString());
                        MessageBox.Show("Error deleting Purchase Return: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        // Reset cursor
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in ultraPictureBox2_Click: " + ex.ToString());
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }

        private void ultraPictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void lblSave_Click(object sender, EventArgs e)
        {
            SavePurchaseReturn();
        }

        private void ultraLabel5_Click(object sender, EventArgs e)
        {
            // Removed clearing functionality
        }

        private void ultraLabel2_Click(object sender, EventArgs e)
        {
            // Call the delete functionality
            ultraPictureBox2_Click(sender, e);
        }

        private void ultraLabel3_Click(object sender, EventArgs e)
        {

        }

        private void ultraLabel4_Click(object sender, EventArgs e)
        {

        }

        private void ultraPanel1_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void ultraPanel4_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void NetTotal_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Show pbxSave and hide ultraPictureBox4
            pbxSave.Visible = true;
            ultraPictureBox4.Visible = false;
            // Reset the purchase data loaded flag to allow loading new items
            _isPurchaseDataLoaded = false;

            frmVendorDig frmVend = new frmVendorDig();
            if (frmVend.ShowDialog() == DialogResult.OK)
            {
                // Get the selected vendor data from the dialog
                int vendorIdVal = frmVend.SelectedVendorId;
                string vendorNameVal = frmVend.SelectedVendorName;

                // Update the UI: VendorName shows Vendor ID, ultraTextEditor2 shows Vendor Name
                SetVendorInfo(vendorIdVal, vendorNameVal);
            }
        }

        private void VendorName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                LoadVendorFromInput();
            }
        }

        private void LoadVendorFromInput()
        {
            try
            {
                if (VendorName == null || string.IsNullOrWhiteSpace(VendorName.Text))
                {
                    if (ultraTextEditor2 != null) ultraTextEditor2.Text = string.Empty;
                    if (vendorid != null) vendorid.Text = "0";
                    return;
                }

                string input = VendorName.Text.Trim();
                if (int.TryParse(input, out int vId) && vId > 0)
                {
                    VendorDDLGrids vendorGrid = drop.VendorDDL();
                    var vendor = vendorGrid?.List?.FirstOrDefault(v => v.LedgerID == vId);
                    if (vendor != null)
                    {
                        SetVendorInfo(vendor.LedgerID, vendor.LedgerName);

                        if (textBox1 != null && textBox1.Visible && textBox1.Enabled)
                        {
                            textBox1.Focus();
                        }
                        else if (ultraDateTimeEditor1 != null)
                        {
                            ultraDateTimeEditor1.Focus();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Vendor ID '{vId}' not found.", "Vendor Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        if (ultraTextEditor2 != null) ultraTextEditor2.Text = string.Empty;
                        if (vendorid != null) vendorid.Text = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vendor by ID: {ex.Message}");
            }
        }

        public void SetVendorInfo(int vendorIdVal, string vendorNameVal)
        {
            try
            {
                _isSettingVendorInfo = true;
                if (VendorName != null) VendorName.Text = vendorIdVal > 0 ? vendorIdVal.ToString() : string.Empty;
                if (ultraTextEditor2 != null) ultraTextEditor2.Text = vendorNameVal ?? string.Empty;
                if (vendorid != null) vendorid.Text = vendorIdVal > 0 ? vendorIdVal.ToString() : "0";
                if (_lstVendorSuggestions != null) _lstVendorSuggestions.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting vendor info: {ex.Message}");
            }
            finally
            {
                _isSettingVendorInfo = false;
            }
        }

        private void InitializeVendorSuggestionPopup()
        {
            if (_lstVendorSuggestions != null) return;

            _lstVendorSuggestions = new ListBox
            {
                Visible = false,
                Width = ultraTextEditor2.Width,
                Height = 140,
                Font = new Font("Segoe UI", 9.75F, FontStyle.Regular),
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };

            if (ultraTextEditor2.Parent != null)
            {
                Point pt = ultraTextEditor2.Parent.PointToScreen(ultraTextEditor2.Location);
                Point formPt = this.PointToClient(pt);
                _lstVendorSuggestions.Location = new Point(formPt.X, formPt.Y + ultraTextEditor2.Height);
            }

            _lstVendorSuggestions.Click += LstVendorSuggestions_Click;
            _lstVendorSuggestions.KeyDown += LstVendorSuggestions_KeyDown;

            this.Controls.Add(_lstVendorSuggestions);
            _lstVendorSuggestions.BringToFront();
        }

        private void LstVendorSuggestions_Click(object sender, EventArgs e)
        {
            SelectVendorFromSuggestion();
        }

        private void LstVendorSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SelectVendorFromSuggestion();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (_lstVendorSuggestions != null) _lstVendorSuggestions.Visible = false;
                if (ultraTextEditor2 != null) ultraTextEditor2.Focus();
            }
        }

        private void SelectVendorFromSuggestion()
        {
            if (_lstVendorSuggestions != null && _lstVendorSuggestions.SelectedItem is ModelClass.VendorDDLG selected)
            {
                SetVendorInfo(selected.LedgerID, selected.LedgerName);
                if (textBox1 != null && textBox1.Visible && textBox1.Enabled)
                {
                    textBox1.Focus();
                }
                else if (ultraDateTimeEditor1 != null)
                {
                    ultraDateTimeEditor1.Focus();
                }
            }
        }

        private void ultraTextEditor2_ValueChanged(object sender, EventArgs e)
        {
            if (_isSettingVendorInfo) return;

            try
            {
                InitializeVendorSuggestionPopup();

                string query = ultraTextEditor2.Text.Trim();
                if (string.IsNullOrEmpty(query))
                {
                    if (_lstVendorSuggestions != null) _lstVendorSuggestions.Visible = false;
                    return;
                }

                VendorDDLGrids vendorGrid = drop.VendorDDL();
                if (vendorGrid != null && vendorGrid.List != null)
                {
                    var matches = vendorGrid.List
                        .Where(v => !string.IsNullOrEmpty(v.LedgerName) &&
                                    v.LedgerName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Take(20)
                        .ToList();

                    if (matches.Any())
                    {
                        _lstVendorSuggestions.DataSource = matches;
                        _lstVendorSuggestions.DisplayMember = "LedgerName";
                        _lstVendorSuggestions.ValueMember = "LedgerID";

                        if (ultraTextEditor2.Parent != null)
                        {
                            Point pt = ultraTextEditor2.Parent.PointToScreen(ultraTextEditor2.Location);
                            Point formPt = this.PointToClient(pt);
                            _lstVendorSuggestions.Location = new Point(formPt.X, formPt.Y + ultraTextEditor2.Height);
                            _lstVendorSuggestions.Width = ultraTextEditor2.Width;
                        }

                        _lstVendorSuggestions.Visible = true;
                        _lstVendorSuggestions.BringToFront();
                    }
                    else
                    {
                        if (_lstVendorSuggestions != null) _lstVendorSuggestions.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing vendor suggestions: {ex.Message}");
            }
        }

        private void ultraTextEditor2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && _lstVendorSuggestions != null && _lstVendorSuggestions.Visible && _lstVendorSuggestions.Items.Count > 0)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                _lstVendorSuggestions.Focus();
                if (_lstVendorSuggestions.SelectedIndex < 0) _lstVendorSuggestions.SelectedIndex = 0;
                return;
            }

            if (e.KeyCode == Keys.Escape && _lstVendorSuggestions != null)
            {
                _lstVendorSuggestions.Visible = false;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (_lstVendorSuggestions != null && _lstVendorSuggestions.Visible && _lstVendorSuggestions.SelectedItem is ModelClass.VendorDDLG selected)
                {
                    SelectVendorFromSuggestion();
                }
                else
                {
                    LoadVendorByNameInput();
                }
            }
        }

        private void LoadVendorByNameInput()
        {
            try
            {
                if (ultraTextEditor2 == null || string.IsNullOrWhiteSpace(ultraTextEditor2.Text))
                {
                    if (VendorName != null) VendorName.Text = string.Empty;
                    if (vendorid != null) vendorid.Text = "0";
                    if (_lstVendorSuggestions != null) _lstVendorSuggestions.Visible = false;
                    return;
                }

                string inputName = ultraTextEditor2.Text.Trim();
                VendorDDLGrids vendorGrid = drop.VendorDDL();

                if (vendorGrid != null && vendorGrid.List != null)
                {
                    var vendor = vendorGrid.List.FirstOrDefault(v => v.LedgerName.Equals(inputName, StringComparison.OrdinalIgnoreCase))
                              ?? vendorGrid.List.FirstOrDefault(v => v.LedgerName.IndexOf(inputName, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (vendor != null)
                    {
                        SetVendorInfo(vendor.LedgerID, vendor.LedgerName);

                        if (textBox1 != null && textBox1.Visible && textBox1.Enabled)
                        {
                            textBox1.Focus();
                        }
                        else if (ultraDateTimeEditor1 != null)
                        {
                            ultraDateTimeEditor1.Focus();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Vendor '{inputName}' not found.", "Vendor Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        if (VendorName != null) VendorName.Text = string.Empty;
                        if (vendorid != null) vendorid.Text = "0";
                        if (_lstVendorSuggestions != null) _lstVendorSuggestions.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vendor by name: {ex.Message}");
            }
        }

        private void SetupVendorAutoComplete()
        {
            try
            {
                if (ultraTextEditor2 == null) return;
                ultraTextEditor2.ReadOnly = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up vendor auto-complete: {ex.Message}");
            }
        }

        private void ApplyCustomControlStyles()
        {
            try
            {
                // 1. VendorName: Peach color like txt_barcode
                if (VendorName != null)
                {
                    VendorName.UseOsThemes = DefaultableBoolean.False;
                    VendorName.UseAppStyling = false;
                    VendorName.BackColor = Color.FromArgb(255, 224, 192);
                    VendorName.Appearance.BackColor = Color.FromArgb(255, 224, 192);
                }

                // 2. ultraTextEditor2: Writable with AutoComplete suggestions
                SetupVendorAutoComplete();

                // 3. ultraTextEditor1: Styled like txtTotal of frmPurchaseOrder.cs but reduced to 8pt font
                ApplyTxtTotalStyle(ultraTextEditor1);

                // 4. Revert TxtSubTotal back to standard white background
                if (TxtSubTotal != null)
                {
                    TxtSubTotal.UseOsThemes = DefaultableBoolean.False;
                    TxtSubTotal.UseAppStyling = false;
                    TxtSubTotal.BackColor = Color.White;
                    TxtSubTotal.ForeColor = Color.FromArgb(0, 35, 80);
                    TxtSubTotal.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
                    TxtSubTotal.Appearance.BackColor = Color.White;
                    TxtSubTotal.Appearance.ForeColor = Color.FromArgb(0, 35, 80);
                    TxtSubTotal.Appearance.FontData.Name = "Segoe UI";
                    TxtSubTotal.Appearance.FontData.SizeInPoints = 9.75F;
                    TxtSubTotal.Appearance.FontData.Bold = DefaultableBoolean.True;
                    TxtSubTotal.Appearance.TextHAlign = HAlign.Center;
                }

                // 5. Update visibility of ultraTextEditor4 & label1 based on chkGRN
                UpdateInvoiceNoVisibility();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying custom control styles: {ex.Message}");
            }
        }

        private void ApplyTxtTotalStyle(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.UseAppStyling = false;
            editor.ReadOnly = true;
            editor.BackColor = Color.Black;
            editor.ForeColor = Color.Yellow;
            editor.Font = new Font("Tahoma", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            editor.Appearance.BackColor = Color.Black;
            editor.Appearance.ForeColor = Color.Yellow;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 8F;
            editor.Appearance.FontData.Bold = DefaultableBoolean.True;
            editor.Appearance.TextHAlign = HAlign.Center;
        }

        private void UpdateBillOutstandingRealTime(decimal currentTotalReturnAmount)
        {
            try
            {
                if (ultraTextEditor1 != null)
                {
                    decimal outstanding = Math.Max(0m, _originalBillTotal - currentTotalReturnAmount);
                    ultraTextEditor1.Text = outstanding.ToString("N2");
                    ultraTextEditor1.Refresh();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating bill outstanding real-time: {ex.Message}");
            }
        }

        private void VendorName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Check if we're in edit mode
                bool isEditMode = (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate");

                // If in edit mode, ensure update button is visible and save button is hidden
                if (isEditMode)
                {
                    if (ultraPictureBox4 != null) ultraPictureBox4.Visible = true;
                    if (pbxSave != null) pbxSave.Visible = false;

                    System.Diagnostics.Debug.WriteLine("VendorName_TextChanged: In edit mode - showing Update button, hiding Save button");
                }
                // Otherwise show save button
                else
                {
                    if (pbxSave != null) pbxSave.Visible = true;
                    if (ultraPictureBox4 != null) ultraPictureBox4.Visible = false;

                    System.Diagnostics.Debug.WriteLine("VendorName_TextChanged: In new entry mode - showing Save button, hiding Update button");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in VendorName_TextChanged: {ex.Message}");
            }
        }

        private void vendorid_Click(object sender, EventArgs e)
        {

        }

        // Helper method to format PR numbers consistently
        private string FormatPRNumber(int prNumber)
        {
            string formattedNumber = prNumber.ToString();

            // Format with appropriate number of digits
            if (formattedNumber.Length == 1)
                return "000" + formattedNumber;
            else if (formattedNumber.Length == 2)
                return "00" + formattedNumber;
            else if (formattedNumber.Length == 3)
                return "0" + formattedNumber;
            else
                return formattedNumber;
        }

        private void GeneratePurchaseReturnNumber()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                System.Diagnostics.Debug.WriteLine("====== STARTING PURCHASE RETURN NUMBER GENERATION ======");

                // Get the max PR number for logging purposes
                int maxPRNo = GetMaxPRNumber();
                System.Diagnostics.Debug.WriteLine($"Current max PR number in database: {maxPRNo}");

                // Get the next PR number from database via repository
                // This will use the stored procedure to get PRBillNo + 1
                int nextPRNo = prRepo.GeneratePReturnNo();

                // Additional logging
                System.Diagnostics.Debug.WriteLine($"GeneratePurchaseReturnNumber received PR number: {nextPRNo}");

                // Format and display the PR number
                if (nextPRNo > 0)
                {
                    string formattedNumber = FormatPRNumber(nextPRNo);
                    TxtSRNO.Text = formattedNumber;
                    System.Diagnostics.Debug.WriteLine($"Set PR number in UI: {formattedNumber} (numeric value: {nextPRNo})");

                    // Verify the PR number is reasonable
                    if (nextPRNo <= maxPRNo)
                    {
                        System.Diagnostics.Debug.WriteLine($"WARNING: Generated PR number {nextPRNo} is NOT greater than max {maxPRNo}!");

                        // Force correct PR number
                        nextPRNo = maxPRNo + 1;
                        formattedNumber = FormatPRNumber(nextPRNo);
                        TxtSRNO.Text = formattedNumber;
                        System.Diagnostics.Debug.WriteLine($"CORRECTED PR number in UI to: {formattedNumber} (numeric value: {nextPRNo})");
                    }
                }
                else
                {
                    // Something went wrong with GeneratePReturnNo, so get max PR number as a fallback
                    nextPRNo = maxPRNo + 1;
                    string formattedNumber = FormatPRNumber(nextPRNo);
                    TxtSRNO.Text = formattedNumber;
                    System.Diagnostics.Debug.WriteLine($"Using fallback PR number in UI: {formattedNumber} (numeric value: {nextPRNo})");
                }

                System.Diagnostics.Debug.WriteLine("====== COMPLETED PURCHASE RETURN NUMBER GENERATION ======");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating PR number: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Get max PR number as a fallback
                int maxPRNo = GetMaxPRNumber() + 1;
                string formattedNumber = FormatPRNumber(maxPRNo);
                TxtSRNO.Text = formattedNumber;
                System.Diagnostics.Debug.WriteLine($"ERROR RECOVERY: Using max+1 PR number in UI: {formattedNumber} (numeric value: {maxPRNo})");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        // Method to get the maximum PR number from the database
        private int GetMaxPRNumber()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        SELECT ISNULL(MAX(PReturnNo), 0)
                        FROM PReturnMaster 
                        WHERE CompanyId = @CompanyId 
                        AND BranchId = @BranchId 
                        AND FinYearId = @FinYearId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int maxPRNo = Convert.ToInt32(result);
                            System.Diagnostics.Debug.WriteLine($"Max PR number from database: {maxPRNo}");
                            return maxPRNo;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting max PR number: {ex.Message}");
            }

            return 0;
        }

        // Diagnostic method to check TrackTrans and PReturnMaster
        private void RunPRNumberDiagnostics()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Check TrackTrans
                    string trackTransQuery = "SELECT * FROM TrackTrans WHERE BranchID = @BranchId AND FinYearID = @FinYearId";
                    using (SqlCommand cmd = new SqlCommand(trackTransQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                System.Diagnostics.Debug.WriteLine("TrackTrans record exists:");
                                while (reader.Read())
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  {reader.GetName(i)}: {reader[i]}");
                                    }
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("No TrackTrans record found for current Branch and FinYear");
                            }
                        }
                    }

                    // Check if TrackTrans has PRBillNo column
                    string columnCheckQuery = @"
                        SELECT COLUMN_NAME 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'TrackTrans' AND COLUMN_NAME = 'PRBillNo'";
                    using (SqlCommand cmd = new SqlCommand(columnCheckQuery, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            System.Diagnostics.Debug.WriteLine("WARNING: PRBillNo column does not exist in TrackTrans table!");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("PRBillNo column exists in TrackTrans table");
                        }
                    }

                    // Check highest PReturnNo
                    string prnoQuery = @"
                        SELECT MAX(PReturnNo) FROM PReturnMaster 
                        WHERE CompanyId = @CompanyId 
                        AND BranchId = @BranchId 
                        AND FinYearId = @FinYearId";
                    using (SqlCommand cmd = new SqlCommand(prnoQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int maxPRNo = Convert.ToInt32(result);
                            System.Diagnostics.Debug.WriteLine($"Highest PR number in PReturnMaster: {maxPRNo}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No PRs found in PReturnMaster");
                        }
                    }

                    // Count total PRs
                    string countQuery = @"
                        SELECT COUNT(*) FROM PReturnMaster 
                        WHERE CompanyId = @CompanyId 
                        AND BranchId = @BranchId 
                        AND FinYearId = @FinYearId";
                    using (SqlCommand cmd = new SqlCommand(countQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Total PRs in PReturnMaster: {count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PR diagnostics: {ex.Message}");
            }
        }

        private string GetConnectionString()
        {
            try
            {
                // Get connection string from the app config or somewhere else
                // This is a placeholder - replace with actual connection string access
                var sqlConn = (SqlConnection)prRepo.DataConnection;
                return sqlConn.ConnectionString;
            }
            catch
            {
                // Fallback connection string if unable to get it from repository
                // WARNING: Hardcoded connection string is just for diagnosis and should be removed in production
                return "Data Source=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;Integrated Security=True";
            }
        }

        // Method to save the purchase return
        private void SavePurchaseReturn()
        {
            try
            {
                if (!ShiftSessionGuard.CanDoTransaction(out string transactionError))
                {
                    MessageBox.Show(transactionError, "Shift Closing Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Track if we should refocus a cell after committing changes
                UltraGridCell cellToFocus = null;
                UltraGridRow rowToFocus = null;
                bool wasInReasonCell = false;

                // Commit any pending edits in the active cell before validation
                if (ultraGrid1.ActiveCell != null)
                {
                    try
                    {
                        // Check if we were in a Reason cell
                        if (ultraGrid1.ActiveCell.Column.Key == "Reason")
                        {
                            wasInReasonCell = true;
                            cellToFocus = ultraGrid1.ActiveCell;
                            rowToFocus = ultraGrid1.ActiveRow;
                        }

                        System.Diagnostics.Debug.WriteLine("Committing pending edits before validation");
                        // Exit edit mode to save any pending changes
                        ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                        ultraGrid1.PerformAction(UltraGridAction.CommitRow);
                        ultraGrid1.UpdateData();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error committing edits: {ex.Message}");
                    }
                }

                // Validation: only require vendor when NOT in "WITHOUT GR" mode
                if (string.IsNullOrEmpty(vendorid.Text) && textBox1.Text != "WITHOUT GR")
                {
                    MessageBox.Show("Please select a vendor first.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the grid has any data
                bool hasItems = false;
                DataTable gridDataTable = null;

                if (ultraGrid1.Rows.Count > 0)
                {
                    // Check if there are any selected rows
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Count > 0 && !row.IsFilteredOut)
                        {
                            // Check if this row has either ItemID or Description
                            bool hasItemData = (row.Cells.Exists("ItemID") && row.Cells["ItemID"].Value != null) ||
                                (row.Cells.Exists("Description") && row.Cells["Description"].Value != null);

                            if (!hasItemData)
                                continue;

                            // Now check if this row is selected via the SELECT column
                            if (row.Cells.Exists("SELECT") &&
                                row.Cells["SELECT"].Value != null &&
                                row.Cells["SELECT"].Value != DBNull.Value &&
                                Convert.ToBoolean(row.Cells["SELECT"].Value))
                            {
                                hasItems = true;
                                break;
                            }
                        }
                    }
                }
                else if (ultraGrid1.DataSource is DataTable dt && dt.Rows.Count > 0)
                {
                    gridDataTable = dt;
                    // Check if there are any rows with SELECT=true in the DataTable
                    foreach (DataRow row in dt.Rows)
                    {
                        if ((!row.IsNull("ItemID") || !row.IsNull("Description")) &&
                            !row.IsNull("SELECT") && Convert.ToBoolean(row["SELECT"]))
                        {
                            hasItems = true;
                            break;
                        }
                    }
                }

                if (!hasItems)
                {
                    MessageBox.Show("Please select at least one item to return by checking the SELECT column.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate that each item has a valid reason
                bool hasInvalidReason = false;
                string firstInvalidItemDesc = "";
                int firstInvalidRowIndex = -1;

                // Make sure the grid's data is current before validation
                ultraGrid1.UpdateData();

                // Predefined list of valid reasons
                string[] validReasons = new string[] { "Expired", "Damaged", "NonOrdered", "NonDemand", "EXPIRED", "DAMAGED", "NONORDERED", "NONDEMAND" };

                if (ultraGrid1.Rows.Count > 0)
                {
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (!row.IsDataRow || row.IsFilteredOut)
                            continue;

                        // Skip empty rows
                        if (!row.Cells.Exists("ItemID") || row.Cells["ItemID"].Value == null)
                            continue;

                        // Check if this row is selected via the SELECT column
                        bool isSelected = false;
                        if (row.Cells.Exists("SELECT"))
                        {
                            var selectValue = row.Cells["SELECT"].Value;
                            isSelected = selectValue != null && selectValue != DBNull.Value && Convert.ToBoolean(selectValue);
                        }

                        // Only validate reason for selected rows
                        if (!isSelected)
                            continue;

                        // Check if Reason is valid
                        bool validReasonFound = false;
                        string currentReason = "";

                        if (row.Cells.Exists("Reason") && row.Cells["Reason"].Value != null)
                        {
                            currentReason = row.Cells["Reason"].Value.ToString().Trim();

                            // Skip "Select Reason" as it's not a valid option
                            if (currentReason.Equals("Select Reason", StringComparison.OrdinalIgnoreCase))
                            {
                                validReasonFound = false;
                            }
                            else
                            {
                                // Check if the current reason is in the list of valid reasons
                                foreach (string validReason in validReasons)
                                {
                                    if (currentReason.Equals(validReason, StringComparison.OrdinalIgnoreCase))
                                    {
                                        validReasonFound = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!validReasonFound)
                        {
                            hasInvalidReason = true;
                            // Get item description for the error message
                            if (row.Cells.Exists("Description") && row.Cells["Description"].Value != null)
                            {
                                firstInvalidItemDesc = row.Cells["Description"].Value.ToString();
                            }
                            else
                            {
                                firstInvalidItemDesc = "Item ID: " + row.Cells["ItemID"].Value.ToString();
                            }

                            firstInvalidRowIndex = row.Index;
                            break;
                        }
                    }
                }

                if (hasInvalidReason)
                {
                    string validReasonsText = "Expired, Damaged, NonOrdered, NonDemand";
                    MessageBox.Show($"Please select a valid reason ({validReasonsText}) for item: {firstInvalidItemDesc}",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // If we were just editing a Reason cell, prioritize focusing on that cell
                    if (wasInReasonCell && rowToFocus != null && cellToFocus != null)
                    {
                        ultraGrid1.ActiveRow = rowToFocus;
                        ultraGrid1.ActiveCell = cellToFocus;
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        return;
                    }

                    // Otherwise focus on the cell with the invalid reason
                    if (firstInvalidRowIndex >= 0 && firstInvalidRowIndex < ultraGrid1.Rows.Count)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[firstInvalidRowIndex];
                        if (ultraGrid1.ActiveRow.Cells.Exists("Reason"))
                        {
                            ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Reason"];
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        }
                    }

                    return;
                }

                // Set cursor to wait
                Cursor.Current = Cursors.WaitCursor;

                // Create the purchase return master record
                PReturnMaster prMaster = new PReturnMaster();
                prMaster.CompanyId = Convert.ToInt32(DataBase.CompanyId);
                prMaster.FinYearId = Convert.ToInt32(DataBase.FinyearId);
                prMaster.BranchId = Convert.ToInt32(DataBase.BranchId);
                prMaster.BranchName = DataBase.Branch;
                prMaster.PReturnNo = 0; // Will be generated by the repository
                prMaster.PReturnDate = ultraDateTimeEditor1.Value != null ? (DateTime)ultraDateTimeEditor1.Value : DateTime.Now;

                // Get purchase invoice details
                prMaster.PInvoice = GetPurchaseReferenceForSave();
                prMaster.InvoiceNo = prMaster.PInvoice;

                // Handle potential DateTime overflows - ensure dates are within SQL Server valid range
                DateTime minSqlDate = new DateTime(1753, 1, 1);
                DateTime maxSqlDate = new DateTime(9999, 12, 31);

                // Set the return date (PReturnDate) with validation
                if (ultraDateTimeEditor1.Value != null)
                {
                    DateTime returnDate = (DateTime)ultraDateTimeEditor1.Value;
                    // Ensure date is within SQL Server's valid range
                    if (returnDate < minSqlDate)
                        returnDate = minSqlDate;
                    else if (returnDate > maxSqlDate)
                        returnDate = maxSqlDate;

                    prMaster.PReturnDate = returnDate;
                }
                else
                {
                    prMaster.PReturnDate = DateTime.Now;
                }

                // Set the invoice date with validation
                if (ultraDateTimeEditor2.Value != null)
                {
                    DateTime invoiceDate = (DateTime)ultraDateTimeEditor2.Value;
                    // Ensure date is within SQL Server's valid range
                    if (invoiceDate < minSqlDate)
                        invoiceDate = minSqlDate;
                    else if (invoiceDate > maxSqlDate)
                        invoiceDate = maxSqlDate;

                    prMaster.InvoiceDate = invoiceDate;
                }
                else
                {
                    // If no invoice date is provided, use the return date or current date
                    prMaster.InvoiceDate = prMaster.PReturnDate;
                }

                // Get vendor details - if in "WITHOUT GR" mode and no vendor is selected, use default values
                if (textBox1.Text == "WITHOUT GR" && string.IsNullOrEmpty(vendorid.Text))
                {
                    prMaster.LedgerID = 1; // Default vendor ledger ID
                    prMaster.VendorName = "Direct Return"; // Default vendor name
                }
                else
                {
                    // Use selected vendor
                    prMaster.LedgerID = Convert.ToInt32(vendorid.Text);
                    prMaster.VendorName = VendorName.Text;
                }

                prMaster.CreditPeriod = 0;

                ApplyHiddenPaymentDefaults(prMaster);

                // Get subtotal
                decimal subTotal = 0;
                if (decimal.TryParse(TxtSubTotal.Text, out decimal subTotalDecimal))
                {
                    subTotal = subTotalDecimal;
                }
                prMaster.SubTotal = Convert.ToDouble(subTotal);

                // Set other financial fields
                prMaster.SpDisPer = 0;
                prMaster.SpDsiAmt = 0;
                prMaster.BillDiscountPer = 0;
                prMaster.BillDiscountAmt = 0;

                // Calculate total tax amount from items
                decimal totalTaxAmount = 0;
                decimal taxPercentage = 0;
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("TaxAmt") && row.Cells["TaxAmt"].Value != DBNull.Value)
                    {
                        totalTaxAmount += Convert.ToDecimal(row.Cells["TaxAmt"].Value);
                    }

                    // Use the tax percentage from the first item as a reference
                    if (taxPercentage == 0 && row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != DBNull.Value)
                    {
                        taxPercentage = Convert.ToDecimal(row.Cells["TaxPer"].Value);
                    }
                }

                prMaster.TaxPer = Convert.ToDouble(taxPercentage);
                prMaster.TaxAmt = Convert.ToDouble(totalTaxAmount);
                prMaster.Frieght = 0;
                prMaster.ExpenseAmt = 0;
                prMaster.OtherExpAmt = 0;

                // Get grand total from the Net Amount label
                decimal grandTotal = 0;
                if (decimal.TryParse(lblNetAmount.Text, out decimal grandTotalDecimal))
                {
                    grandTotal = grandTotalDecimal;
                }
                prMaster.GrandTotal = Convert.ToDouble(grandTotal);

                // Set other fields
                prMaster.CancelFlag = false;
                prMaster.UserID = Convert.ToInt32(DataBase.UserId);
                prMaster.UserName = DataBase.UserName;

                // Get tax type from the first item that has it
                // Convert display format back to database format: "Incl" -> "I", "Excl" -> "E"
                string taxType = "I"; // Default to Inclusive
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != DBNull.Value)
                    {
                        string taxTypeDisplay = row.Cells["TaxType"].Value.ToString().Trim();
                        // Convert display format to database format
                        if (taxTypeDisplay.Equals("Incl", StringComparison.OrdinalIgnoreCase))
                            taxType = "I";
                        else if (taxTypeDisplay.Equals("Excl", StringComparison.OrdinalIgnoreCase))
                            taxType = "E";
                        else if (taxTypeDisplay == "I" || taxTypeDisplay == "E")
                            taxType = taxTypeDisplay.ToUpper(); // Already in correct format
                        else
                            taxType = "I"; // Default if unknown format
                        break;
                    }
                }
                prMaster.TaxType = taxType;
                prMaster.Remarks = "";
                prMaster.RoundOff = 0;
                prMaster.CessPer = 0;
                prMaster.CessAmt = 0;
                prMaster.CalAfterTax = 0;
                prMaster.CurrencyID = 0;
                prMaster.CurSymbol = "";
                prMaster.SeriesID = 0;
                // VoucherID will be generated by the repository - don't hardcode it to 0

                // Create detail record for the repository method call
                PReturnDetails prDetails = new PReturnDetails();
                prDetails.CompanyId = prMaster.CompanyId;
                prDetails.FinYearId = prMaster.FinYearId;
                prDetails.BranchID = prMaster.BranchId;
                prDetails.BranchName = prMaster.BranchName;
                prDetails.PReturnNo = prMaster.PReturnNo;
                prDetails.PReturnDate = prMaster.PReturnDate;
                prDetails.InvoiceNo = prMaster.InvoiceNo;

                // Create a DataGridView from the UltraGrid data
                DataGridView tempDgv = new DataGridView();

                if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                {
                    DataTable sourceTable = (DataTable)ultraGrid1.DataSource;
                    tempDgv.DataSource = sourceTable;
                }
                else
                {
                    // Create a new DataTable if the grid doesn't have one
                    DataTable tempDt = new DataTable();

                    // Add essential columns
                    tempDt.Columns.Add("ItemID", typeof(long));
                    tempDt.Columns.Add("Description", typeof(string));
                    tempDt.Columns.Add("UnitId", typeof(int));
                    tempDt.Columns.Add("BaseUnit", typeof(bool));
                    tempDt.Columns.Add("Packing", typeof(double));
                    tempDt.Columns.Add("IsExpiry", typeof(bool));
                    tempDt.Columns.Add("BatchNo", typeof(string));
                    tempDt.Columns.Add("Expiry", typeof(DateTime));
                    tempDt.Columns.Add("Qty", typeof(double));
                    tempDt.Columns.Add("TaxPer", typeof(double));
                    tempDt.Columns.Add("TaxAmt", typeof(double));
                    tempDt.Columns.Add("TaxType", typeof(string));
                    tempDt.Columns.Add("Reason", typeof(string));
                    tempDt.Columns.Add("Free", typeof(double));
                    tempDt.Columns.Add("Cost", typeof(double));
                    tempDt.Columns.Add("DisPer", typeof(double));
                    tempDt.Columns.Add("DisAmt", typeof(double));
                    tempDt.Columns.Add("SalesPrice", typeof(double));
                    tempDt.Columns.Add("OriginalCost", typeof(double));
                    tempDt.Columns.Add("TotalSP", typeof(double));
                    tempDt.Columns.Add("TotalAmount", typeof(double));
                    tempDt.Columns.Add("CessAmt", typeof(double));
                    tempDt.Columns.Add("CessPer", typeof(double));
                    tempDt.Columns.Add("SELECT", typeof(bool));

                    // Check if any rows are selected
                    bool anyRowsSelected = false;
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (!row.IsDataRow || row.IsFilteredOut)
                            continue;

                        // Check if the SELECT column exists and is checked
                        if (row.Cells.Exists("SELECT") &&
                            row.Cells["SELECT"].Value != null &&
                            row.Cells["SELECT"].Value != DBNull.Value &&
                            Convert.ToBoolean(row.Cells["SELECT"].Value))
                        {
                            anyRowsSelected = true;
                            break;
                        }
                    }

                    // If no rows are selected, show an error message
                    if (!anyRowsSelected)
                    {
                        MessageBox.Show("Please select at least one item by checking the checkbox.",
                            "No Items Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    // Only copy data from selected rows (where SELECT column is checked)
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (!row.IsDataRow || row.IsFilteredOut)
                            continue;

                        // Skip this row if the SELECT column isn't checked
                        if (!row.Cells.Exists("SELECT") ||
                            row.Cells["SELECT"].Value == null ||
                            row.Cells["SELECT"].Value == DBNull.Value ||
                            !Convert.ToBoolean(row.Cells["SELECT"].Value))
                        {
                            continue;
                        }

                        // Now that we know this row is selected, validate the Reason field
                        if (row.Cells.Exists("Reason") &&
                            (row.Cells["Reason"].Value == null ||
                             row.Cells["Reason"].Value == DBNull.Value ||
                             string.IsNullOrWhiteSpace(row.Cells["Reason"].Value.ToString())))
                        {
                            string itemDesc = "";
                            if (row.Cells["Description"].Value != null)
                            {
                                itemDesc = row.Cells["Description"].Value.ToString();
                            }
                            else if (row.Cells["ItemID"].Value != null)
                            {
                                itemDesc = "Item ID: " + row.Cells["ItemID"].Value.ToString();
                            }

                            MessageBox.Show($"Reason is mandatory for selected item: {itemDesc}. Please provide a reason.",
                                "Missing Reason", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            // Set focus to the Reason cell of this row
                            if (row.Cells.Exists("Reason"))
                            {
                                ultraGrid1.ActiveRow = row;
                                ultraGrid1.ActiveCell = row.Cells["Reason"];
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }

                            Cursor.Current = Cursors.Default;
                            return;
                        }

                        DataRow newRow = tempDt.NewRow();

                        // Copy cell values
                        foreach (UltraGridCell cell in row.Cells)
                        {
                            if (tempDt.Columns.Contains(cell.Column.Key) && cell.Value != null && cell.Value != DBNull.Value)
                            {
                                newRow[cell.Column.Key] = cell.Value;
                            }
                        }

                        tempDt.Rows.Add(newRow);
                    }

                    // If after filtering, no rows remain, show an error
                    if (tempDt.Rows.Count == 0)
                    {
                        MessageBox.Show("No valid items selected. Please select at least one item by checking the checkbox.",
                            "No Valid Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    tempDgv.DataSource = tempDt;
                }

                // Call the repository to save the master record and get the PR number
                string result = prRepo.savePR(prMaster, prDetails, tempDgv);

                // Check if the master record was saved successfully
                if (result.StartsWith("success:"))
                {
                    // Extract the PR number from the result
                    int returnedPrNo = int.Parse(result.Substring("success:".Length));

                    // Update the control to display the PR number
                    TxtSRNO.Text = FormatPRNumber(returnedPrNo);

                    // Save the PR number to prevent duplicates
                    _currentlyLoadedPurchaseNo = returnedPrNo;

                    // Call the method to save details
                    UpdatePurchaseReturnDetails(returnedPrNo);
                    prMaster.PReturnNo = returnedPrNo;
                    SavePurchaseReturnActivityLog("SAVE", prMaster);

                    // Generate and display debit note preview
                    DebitNote debitNote = GenerateDebitNote(prMaster, returnedPrNo);
                    DisplayDebitNote(debitNote, prMaster);

                    // Clear the form for next entry after successful save
                    ClearForm();
                }
                else
                {
                    // Display error message
                    MessageBox.Show("Error saving purchase return: " + result,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving purchase return: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine("Error in SavePurchaseReturn: " + ex.Message);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
            }
            finally
            {
                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }

        private void SavePurchaseReturnActivityLog(string activityType, PReturnMaster purchaseReturn)
        {
            if (purchaseReturn == null || purchaseReturn.PReturnNo <= 0)
            {
                return;
            }

            try
            {
                using (var repo = new TransactionActivityLogRepository())
                {
                    repo.SavePurchaseReturnActivity(
                        purchaseReturn.PReturnNo,
                        purchaseReturn.InvoiceNo,
                        purchaseReturn.VendorName,
                        purchaseReturn.Paymode,
                        Convert.ToDecimal(purchaseReturn.GrandTotal),
                        activityType,
                        $"Purchase Return No: {purchaseReturn.PReturnNo}, Vendor: {purchaseReturn.VendorName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to save purchase return activity log: " + ex.Message);
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {
            try
            {
                // Apply modern styling similar to SalesReturn form

                // Apply row spacing before (reduced to minimize blank space)
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.CellSpacing = 0;

                // Set selection behavior
                e.Layout.Override.SelectTypeRow = SelectType.Single;

                // Configure header click behavior
                e.Layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Configure cell click action
                e.Layout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Basic configuration
                e.Layout.Override.AllowAddNew = AllowAddNew.No;
                e.Layout.Override.AllowDelete = DefaultableBoolean.False;
                e.Layout.Override.AllowUpdate = DefaultableBoolean.True;
                e.Layout.Override.RowSelectors = DefaultableBoolean.True;

                // Always ensure SELECT column's checkbox header is properly configured
                if (e.Layout.Bands.Count > 0 && e.Layout.Bands[0].Columns.Exists("SELECT"))
                {
                    UltraGridColumn selectCol = e.Layout.Bands[0].Columns["SELECT"];
                    selectCol.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                    selectCol.Header.CheckBoxAlignment = HeaderCheckBoxAlignment.Center;
                    selectCol.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;

                    // Remove the "SELECT" text from the header
                    selectCol.Header.Caption = "";

                    // Ensure SELECT column is after Reason
                    if (e.Layout.Bands[0].Columns.Exists("Reason"))
                    {
                        int reasonPos = e.Layout.Bands[0].Columns["Reason"].Header.VisiblePosition;
                        selectCol.Header.VisiblePosition = reasonPos + 1;
                    }
                }

                // Apply unified theme cleanly
                ApplyUnifiedGridTheme(ultraGrid1);

                // Disable column auto-fitting so columns keep explicit widths without stretching/filling grid area
                e.Layout.AutoFitStyle = AutoFitStyle.None;

                // Set vertical alignment for all cells to middle
                e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Apply to all columns
                if (e.Layout.Bands.Count > 0)
                {
                    // Ensure SELECT column is after Reason column (after TaxType, TaxPer, TaxAmt)
                    if (e.Layout.Bands[0].Columns.Exists("SELECT") && e.Layout.Bands[0].Columns.Exists("Reason"))
                    {
                        // Get the current positions
                        int reasonPos = e.Layout.Bands[0].Columns["Reason"].Header.VisiblePosition;

                        // Explicitly set SELECT column position to be after Reason (TaxType, TaxPer, TaxAmt come before Reason)
                        e.Layout.Bands[0].Columns["SELECT"].Header.VisiblePosition = reasonPos + 1;
                    }

                    foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                    {
                        // Apply text alignment based on column content
                        if (col.Key == "Cost" || col.Key == "Quantity" || col.Key == "Amount" || col.Key == "Reason" ||
                            col.Key == "TaxType" || col.Key == "TaxPer" || col.Key == "TaxAmt")
                        {
                            col.CellAppearance.TextHAlign = HAlign.Center;
                            col.CellAppearance.TextVAlign = VAlign.Middle;
                        }

                        // Format numeric columns
                        if (col.Key == "Cost" || col.Key == "Amount" || col.Key == "TaxPer" || col.Key == "TaxAmt" || col.Key == "Quantity" || col.Key == "Returned qty" || col.Key == "Returned")
                        {
                            col.Format = "N2";
                        }
                    }
                }

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in InitializeLayout: " + ex.Message);
            }
        }

        private void TxtRoundOff_TextChanged(object sender, EventArgs e)
        {

        }

        private void TxtSubTotal_TextChanged(object sender, EventArgs e)
        {
            // Remove the code that updates lblNetAmount directly
            // lblNetAmount.Text = TxtSubTotal.Text;
        }

        private void ShowPlayfulVendorAlert()
        {
            try
            {
                using (Form alertForm = new Form())
                {
                    alertForm.Text = "Oops!";
                    alertForm.Size = new Size(380, 210);
                    alertForm.FormBorderStyle = FormBorderStyle.None;
                    alertForm.StartPosition = FormStartPosition.CenterParent;
                    alertForm.ShowInTaskbar = false;
                    alertForm.TopMost = true;
                    alertForm.BackColor = Color.FromArgb(235, 245, 255);

                    alertForm.Paint += (s, e) =>
                    {
                        Graphics g = e.Graphics;
                        g.SmoothingMode = SmoothingMode.AntiAlias;

                        Rectangle rect = new Rectangle(0, 0, alertForm.Width - 1, alertForm.Height - 1);
                        using (LinearGradientBrush brush = new LinearGradientBrush(rect, Color.FromArgb(242, 248, 255), Color.FromArgb(215, 235, 255), LinearGradientMode.Vertical))
                        {
                            g.FillRectangle(brush, rect);
                        }

                        using (Pen pen = new Pen(Color.FromArgb(102, 190, 255), 2))
                        {
                            g.DrawRectangle(pen, 1, 1, alertForm.Width - 3, alertForm.Height - 3);
                        }

                        using (Pen topPen = new Pen(Color.FromArgb(67, 118, 184), 3))
                        {
                            g.DrawLine(topPen, 10, 2, alertForm.Width - 10, 2);
                        }
                    };

                    Label lblBadge = new Label
                    {
                        Text = "🛒 OOPS!",
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(20, 55, 120),
                        BackColor = Color.Transparent,
                        AutoSize = false,
                        Size = new Size(340, 30),
                        Location = new Point(20, 18),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    alertForm.Controls.Add(lblBadge);

                    Label lblMessage = new Label
                    {
                        Text = "oops select vendor first please.",
                        Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(10, 31, 79),
                        BackColor = Color.Transparent,
                        AutoSize = false,
                        Size = new Size(340, 50),
                        Location = new Point(20, 52),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    alertForm.Controls.Add(lblMessage);

                    Button btnOk = new Button
                    {
                        Text = "Select Vendor 🛒",
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(93, 151, 214),
                        FlatStyle = FlatStyle.Flat,
                        Size = new Size(180, 42),
                        Location = new Point((alertForm.Width - 180) / 2, 125),
                        Cursor = Cursors.Hand,
                        DialogResult = DialogResult.OK
                    };
                    btnOk.FlatAppearance.BorderSize = 0;

                    btnOk.Paint += (s, e) =>
                    {
                        Graphics g = e.Graphics;
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        Rectangle bRect = new Rectangle(0, 0, btnOk.Width, btnOk.Height);
                        using (LinearGradientBrush bBrush = new LinearGradientBrush(bRect, Color.FromArgb(93, 151, 214), Color.FromArgb(67, 118, 184), LinearGradientMode.Vertical))
                        {
                            g.FillRectangle(bBrush, bRect);
                        }
                        using (Pen bPen = new Pen(Color.FromArgb(118, 154, 198), 1))
                        {
                            g.DrawRectangle(bPen, 0, 0, btnOk.Width - 1, btnOk.Height - 1);
                        }
                        TextRenderer.DrawText(g, btnOk.Text, btnOk.Font, bRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    };

                    alertForm.Controls.Add(btnOk);
                    alertForm.AcceptButton = btnOk;

                    alertForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error displaying playful vendor alert: " + ex.Message);
            }
        }

        private void btnAddPurchaceList_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if items are loaded via button1 (purchasereturnupdate)
                if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                {
                    MessageBox.Show("Invalid operation. Purchase return data is already loaded from update form.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if items are already loaded via BtnDial (dialforitemmaster)
                if (textBox1.Text == "WITHOUT GR")
                {
                    MessageBox.Show("Invalid operation. Items already loaded via direct selection.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Only check if items are being loaded via BtnDial
                // In the original context, btnDial is being used when TxtBarcode is read-only
                if (TxtBarcode.ReadOnly && !_originalTxtBarcodeReadOnly && textBox1.Text == "WITHOUT GR")
                {
                    MessageBox.Show("Invalid operation. Please finish the current item selection process before using this button.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                // Show pbxSave and hide ultraPictureBox4
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // Check if a valid vendor is selected (vendorid.Text must be non-empty and > 0)
                int vendorId = 0;
                if (vendorid == null || string.IsNullOrWhiteSpace(vendorid.Text) || !int.TryParse(vendorid.Text, out vendorId) || vendorId <= 0)
                {
                    ShowPlayfulVendorAlert();
                    button2_Click(sender, e);

                    // Re-check vendorid after vendor selection dialog closes
                    if (vendorid == null || string.IsNullOrWhiteSpace(vendorid.Text) || !int.TryParse(vendorid.Text, out vendorId) || vendorId <= 0)
                    {
                        return;
                    }
                }

                string vendorName = VendorName != null ? VendorName.Text : string.Empty;

                // Open the purchase details dialog for the selected vendor
                FrmVendorPurchaseDetailsDialog purchaseDialog = new FrmVendorPurchaseDetailsDialog(vendorId, vendorName);
                if (purchaseDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected purchase information from the dialog
                    int purchaseNo = purchaseDialog.SelectedPurchaseNo;

                    // Only update if it's not already set to true from a previous operation
                    if (!TxtBarcode.ReadOnly)
                    {
                        _originalTxtBarcodeReadOnly = false;
                    }

                    // Make TxtBarcode writable
                    EnsureTxtBarcodeIsWritable();

                    DateTime invoiceDate = purchaseDialog.SelectedPurchaseDate;
                    string invoiceNo = FormatPurchaseNoForDisplay(purchaseNo);

                    // Set the purchase number in textBox1
                    textBox1.Text = invoiceNo;

                    // Set the invoice number in ultraTextEditor4
                    if (ultraTextEditor4 != null)
                    {
                        string actualInvoiceNo = !string.IsNullOrWhiteSpace(purchaseDialog.SelectedInvoiceNo)
                            ? purchaseDialog.SelectedInvoiceNo
                            : GetInvoiceNoFromPurchaseNo(purchaseNo);

                        ultraTextEditor4.Text = actualInvoiceNo;
                    }

                    // Set the invoice date if the control exists
                    if (this.Controls.Find("dtInvoiceDate", true).Length > 0)
                    {
                        DateTimePicker dtInvoiceDate = this.Controls.Find("dtInvoiceDate", true)[0] as DateTimePicker;
                        if (dtInvoiceDate != null)
                        {
                            dtInvoiceDate.Value = invoiceDate;
                        }
                    }

                    // Set the form Tag to indicate this is from a vendor search
                    this.Tag = "VendorSearch";

                    if (ultraGrid1.DataSource != null)
                    {
                        DataTable dt = ultraGrid1.DataSource as DataTable;
                        if (dt != null)
                        {
                            dt.Clear();
                        }
                    }

                    // Reset the purchase data loaded flag before loading new data
                    _isPurchaseDataLoaded = false;

                    // Update the currently loaded purchase number
                    _currentlyLoadedPurchaseNo = purchaseNo;

                    // Load the purchase items into ultraGrid1
                    LoadPurchaseItems(vendorId, purchaseNo);

                    // Make sure the grid has focus and there is data
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        // Use BeginInvoke to ensure UI is fully updated before setting focus
                        this.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                // Get the first row
                                UltraGridRow firstRow = ultraGrid1.Rows[0];

                                // Set this row as the active row and clear any existing selection
                                ultraGrid1.ActiveRow = firstRow;
                                ultraGrid1.Selected.Rows.Clear();
                                ultraGrid1.Selected.Rows.Add(firstRow);

                                // Find the first editable cell in the row (Unit is preferred starting point)
                                UltraGridCell targetCell = null;

                                if (firstRow.Cells.Exists("Unit"))
                                {
                                    targetCell = firstRow.Cells["Unit"];
                                }
                                else if (firstRow.Cells.Exists("Description"))
                                {
                                    targetCell = firstRow.Cells["Description"];
                                }
                                else
                                {
                                    // Find any editable cell
                                    foreach (UltraGridCell cell in firstRow.Cells)
                                    {
                                        if (cell.Column.CellActivation == Activation.AllowEdit)
                                        {
                                            targetCell = cell;
                                            break;
                                        }
                                    }
                                }

                                if (targetCell != null)
                                {
                                    // Set focus to the target cell
                                    ultraGrid1.ActiveCell = targetCell;

                                    // Set focus to the grid and enter edit mode
                                    ultraGrid1.Focus();

                                    // Use PerformAction to enter edit mode if the cell is editable
                                    if (targetCell.Column.CellActivation == Activation.AllowEdit)
                                    {
                                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                    }

                                    // Scroll to ensure the row is visible
                                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(firstRow);

                                    // Update UI to show focus
                                    Application.DoEvents();
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error setting focus to grid after loading: {ex.Message}");
                            }
                        }));
                    }

                    // Make TxtBarcode read-only ONLY when loading items via btnAddPurchaceList
                    // But ensure it becomes writable when needed
                    TxtBarcode.ReadOnly = true;
                    _originalTxtBarcodeReadOnly = false; // Store original state for reference

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetInvoiceNoFromPurchaseNo(int purchaseNo)
        {
            try
            {
                BaseRepostitory baseRepo = new BaseRepostitory();
                using (var conn = (System.Data.SqlClient.SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();
                    string sql = "SELECT TOP 1 InvoiceNo FROM PMaster WHERE PurchaseNo = @PurchaseNo";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching InvoiceNo for PurchaseNo {purchaseNo}: {ex.Message}");
            }
            return string.Empty;
        }

        private void UpdateInvoiceNoVisibility()
        {
            try
            {
                bool isGrnChecked = chkGRN != null && chkGRN.Checked;
                if (ultraTextEditor4 != null)
                {
                    ultraTextEditor4.Visible = isGrnChecked;
                    if (!isGrnChecked)
                    {
                        ultraTextEditor4.Text = string.Empty;
                    }
                }
                if (label1 != null)
                {
                    label1.Visible = isGrnChecked;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating InvoiceNo visibility: {ex.Message}");
            }
        }





        private UltraGridCell FindNextEditableCell(UltraGridRow row, UltraGridCell currentCell)
        {
            if (row == null || currentCell == null) return null;

            // List of allowed editable column keys in the desired order
            string[] editableColumns = new string[] { "Description", "Unit", "Packing", "Cost", "Quantity", "Returned qty", "Reason" };

            // Find the current column in our editableColumns array
            int currentPos = -1;
            for (int i = 0; i < editableColumns.Length; i++)
            {
                if (currentCell.Column.Key == editableColumns[i])
                {
                    currentPos = i;
                    break;
                }
            }

            // If current column found, move to the next one in sequence
            if (currentPos >= 0)
            {
                // Move to next column in sequence
                int nextPos = (currentPos + 1) % editableColumns.Length;
                string nextColumn = editableColumns[nextPos];

                // Return the next column if it exists
                if (row.Cells.Exists(nextColumn))
                {
                    // For Description cell, we need to check if it's allowed to be edited
                    if (nextColumn == "Description" && row.Cells[nextColumn].Column.CellActivation == Activation.NoEdit)
                    {
                        // Even though Description is typically read-only, we want to focus on it
                        return row.Cells[nextColumn];
                    }
                    // For other cells, check if they're editable
                    else if (row.Cells[nextColumn].Column.CellActivation == Activation.AllowEdit)
                    {
                        return row.Cells[nextColumn];
                    }
                    // If the next cell isn't editable, try to find another editable cell
                    else
                    {
                        // Try subsequent cells
                        for (int i = 1; i < editableColumns.Length; i++)
                        {
                            int pos = (nextPos + i) % editableColumns.Length;
                            if (row.Cells.Exists(editableColumns[pos]))
                            {
                                if (editableColumns[pos] == "Description" ||
                                    row.Cells[editableColumns[pos]].Column.CellActivation == Activation.AllowEdit)
                                {
                                    return row.Cells[editableColumns[pos]];
                                }
                            }
                        }
                    }
                }
            }

            // If we didn't find the current column or next column, default to Description cell
            if (row.Cells.Exists("Description"))
            {
                return row.Cells["Description"];
            }

            // If Description doesn't exist, try to find any editable cell
            foreach (string columnKey in editableColumns)
            {
                if (row.Cells.Exists(columnKey) &&
                    (columnKey == "Description" || row.Cells[columnKey].Column.CellActivation == Activation.AllowEdit))
                {
                    return row.Cells[columnKey];
                }
            }

            // No appropriate cell found
            return null;
        }

        private UltraGridCell FindPreviousEditableCell(UltraGridRow row, UltraGridCell currentCell)
        {
            if (row == null || currentCell == null) return null;

            // List of allowed editable column keys in the desired order
            string[] editableColumns = new string[] { "Description", "Unit", "Packing", "Cost", "Quantity", "Returned qty", "Reason" };

            // Find the current column in our editableColumns array
            int currentPos = -1;
            for (int i = 0; i < editableColumns.Length; i++)
            {
                if (currentCell.Column.Key == editableColumns[i])
                {
                    currentPos = i;
                    break;
                }
            }

            // If current column found, move to the previous one in sequence
            if (currentPos >= 0)
            {
                // Move to previous column in sequence
                int prevPos = (currentPos - 1 + editableColumns.Length) % editableColumns.Length;
                string prevColumn = editableColumns[prevPos];

                // Return the previous column if it exists
                if (row.Cells.Exists(prevColumn))
                {
                    // For Description cell, we always want to focus on it
                    if (prevColumn == "Description")
                    {
                        return row.Cells[prevColumn];
                    }
                    // For other cells, check if they're editable
                    else if (row.Cells[prevColumn].Column.CellActivation == Activation.AllowEdit)
                    {
                        return row.Cells[prevColumn];
                    }
                    // If the previous cell isn't editable, try to find another editable cell
                    else
                    {
                        // Try previous cells
                        for (int i = 1; i < editableColumns.Length; i++)
                        {
                            int pos = (prevPos - i + editableColumns.Length) % editableColumns.Length;
                            if (row.Cells.Exists(editableColumns[pos]))
                            {
                                if (editableColumns[pos] == "Description" ||
                                    row.Cells[editableColumns[pos]].Column.CellActivation == Activation.AllowEdit)
                                {
                                    return row.Cells[editableColumns[pos]];
                                }
                            }
                        }
                    }
                }
            }

            // If we didn't find the current column or previous column, default to Description cell
            if (row.Cells.Exists("Description"))
            {
                return row.Cells["Description"];
            }

            // If we didn't find the Description column, default to the last editable column in our sequence
            for (int i = editableColumns.Length - 1; i >= 0; i--)
            {
                if (row.Cells.Exists(editableColumns[i]) &&
                    (editableColumns[i] == "Description" || row.Cells[editableColumns[i]].Column.CellActivation == Activation.AllowEdit))
                {
                    return row.Cells[editableColumns[i]];
                }
            }

            // No appropriate editable cell found
            return null;
        }

        private UltraGridCell FindFirstEditableCell(UltraGridRow row)
        {
            if (row == null) return null;

            // Always look for the Description cell first (Item Name) as requested
            if (row.Cells.Exists("Description"))
            {
                return row.Cells["Description"];
            }

            // If Description doesn't exist (unlikely), use fallback order
            string[] editableColumns = new string[] { "Description", "Unit", "Packing", "Cost", "Quantity", "Returned qty", "Reason" };

            // Return the first editable column in our sequence
            foreach (string columnKey in editableColumns)
            {
                if (row.Cells.Exists(columnKey) &&
                    (columnKey == "Description" || row.Cells[columnKey].Column.CellActivation == Activation.AllowEdit))
                {
                    return row.Cells[columnKey];
                }
            }

            // No appropriate editable cell found
            return null;
        }

        private UltraGridCell FindLastEditableCell(UltraGridRow row)
        {
            if (row == null) return null;

            // List of allowed editable column keys in the desired order
            string[] editableColumns = new string[] { "Description", "Unit", "Packing", "Cost", "Quantity", "Returned qty", "Reason" };

            // Return the last editable column in our sequence
            for (int i = editableColumns.Length - 1; i >= 0; i--)
            {
                if (row.Cells.Exists(editableColumns[i]) &&
                    (editableColumns[i] == "Description" || row.Cells[editableColumns[i]].Column.CellActivation == Activation.AllowEdit))
                {
                    return row.Cells[editableColumns[i]];
                }
            }

            // No appropriate editable cell found
            return null;
        }

        // Override ProcessCmdKey to handle special keys like F1
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F7)
            {
                // Check if items are loaded via button1 (purchasereturnupdate)
                if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                {
                    MessageBox.Show("Invalid operation. Purchase return data is already loaded from update form.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return true;
                }

                // Check if items are loaded from btnAddPurchaceList
                if (_isPurchaseDataLoaded && !string.IsNullOrEmpty(textBox1.Text) &&
                    textBox1.Text != "WITHOUT GR" && textBox1.Text != "Without GR")
                {
                    MessageBox.Show("Invalid input. Please clear the form first before selecting items.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return true;
                }

                BtnDial_Click(null, null);
                return true;
            }
            else if (keyData == Keys.F1)
            {
                return true;
            }
            else if (keyData == Keys.F4)
            {
                // Close the form when F4 is pressed
                this.Close();
                return true;
            }
            else if (keyData == Keys.F12)
            {
                // Call delete functionality when F12 is pressed
                ultraPictureBox2_Click(null, null);
                return true;
            }
            else if (keyData == Keys.F8)
            {
                // Determine if we should save or update based on which button is visible
                if (ultraPictureBox4 != null && ultraPictureBox4.Visible)
                {
                    // Call update functionality
                    ultraPictureBox4_Click(null, null);
                }
                else
                {
                    // Call save functionality
                    pbxSave_Click(null, null);
                }
                return true;
            }
            else if (keyData == Keys.Tab)
            {
                // Check if focus is currently in the UltraGrid
                if (ultraGrid1.Focused || ultraGrid1.ActiveCell != null)
                {
                    // Let the grid handle Tab navigation internally
                    return false;
                }

                // Use our custom focus cycling for regular Tab
                CycleFocus(true);
                return true;
            }
            else if (keyData == (Keys.Tab | Keys.Shift))
            {
                // Check if focus is currently in the UltraGrid
                if (ultraGrid1.Focused || ultraGrid1.ActiveCell != null)
                {
                    // Let the grid handle Shift+Tab navigation internally
                    return false;
                }

                // Handle Shift+Tab for form fields
                CycleFocus(false); // Use false to indicate backward direction
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        // Implement the CellListSelect handler for immediate updates when dropdown item is clicked
        private void UltraGrid1_CellListSelect(object sender, CellEventArgs e)
        {
            try
            {
                // Only handle Reason column
                if (e.Cell != null && e.Cell.Column.Key == "Reason")
                {
                    // Check if this event was triggered by a mouse click (not arrow keys)
                    // Only commit the value if it's a mouse click, not when navigating with arrow keys
                    if (Control.MouseButtons != MouseButtons.None)
                    {
                        // Get the current cell and row
                        UltraGridCell currentCell = e.Cell;
                        UltraGridRow currentRow = e.Cell.Row;

                        // Get the selected value
                        string selectedReason = currentCell.Text;
                        System.Diagnostics.Debug.WriteLine($"Reason selected: {selectedReason}");

                        // Exit edit mode to commit the changes
                        ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);

                        // Make sure the cell reflects the new value
                        if (currentCell.Value == null || !currentCell.Value.ToString().Equals(selectedReason))
                        {
                            currentCell.Value = selectedReason;
                        }

                        // Refresh the grid
                        ultraGrid1.Refresh();

                        // After selection, check if there's a next row
                        if (currentRow.Index < ultraGrid1.Rows.Count - 1)
                        {
                            // Move to the Unit cell of the next row
                            UltraGridRow nextRow = ultraGrid1.Rows[currentRow.Index + 1];
                            if (nextRow.Cells.Exists("Unit"))
                            {
                                ultraGrid1.ActiveRow = nextRow;
                                ultraGrid1.ActiveCell = nextRow.Cells["Unit"];
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }
                        else
                        {
                            // If this is the last row, just move focus to the Unit cell of this row
                            if (currentRow.Cells.Exists("Unit"))
                            {
                                ultraGrid1.ActiveCell = currentRow.Cells["Unit"];
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Reason dropdown selection committed: {selectedReason} on row {currentRow.Index}");
                    }
                    else
                    {
                        // If triggered by keyboard navigation (arrow keys), don't commit the value
                        System.Diagnostics.Debug.WriteLine("Arrow key navigation - not committing value");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in UltraGrid1_CellListSelect: " + ex.Message);
            }
        }

        // Implement the MouseDown handler to show dropdown when clicking on Reason cell
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the UI element at the mouse click point
                UIElement element = ultraGrid1.DisplayLayout.UIElement.ElementFromPoint(new Point(e.X, e.Y));
                if (element != null)
                {
                    // Check if the click is on a row selector
                    RowSelectorUIElement rowSelectorElement = element as RowSelectorUIElement;
                    if (rowSelectorElement != null && rowSelectorElement.Row != null)
                    {
                        UltraGridRow clickedRow = rowSelectorElement.Row;

                        // Make this row the active row and selected row
                        ultraGrid1.ActiveRow = clickedRow;
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(clickedRow);

                        // Restore highlighting for all rows with checked checkboxes
                        foreach (UltraGridRow row in ultraGrid1.Rows)
                        {
                            if (row.Cells.Exists("SELECT") &&
                                row.Cells["SELECT"].Value != null &&
                                row.Cells["SELECT"].Value != DBNull.Value &&
                                (bool)row.Cells["SELECT"].Value)
                            {
                                // Re-apply blue highlight for checked rows
                                row.Appearance.BackColor = Color.LightBlue;
                            }
                            else if (row != clickedRow)
                            {
                                // Remove highlighting for unchecked rows (except active row)
                                row.Appearance.BackColor = Color.Empty;
                                row.Appearance.ForeColor = Color.Empty;
                                row.Appearance.FontData.Bold = DefaultableBoolean.Default;
                            }
                        }

                        return; // Don't process further since we handled the row selector click
                    }

                    // Handle Reason column cell clicks (existing functionality)
                    UltraGridCell cell = element.GetContext(typeof(UltraGridCell)) as UltraGridCell;
                    if (cell != null && cell.Column.Key == "Reason")
                    {
                        // Make this the active cell
                        ultraGrid1.ActiveCell = cell;

                        // Enter edit mode directly
                        if (!ultraGrid1.ActiveCell.IsInEditMode)
                        {
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                            // Show dropdown immediately
                            SendKeys.SendWait("{F4}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in UltraGrid1_MouseDown: " + ex.Message);
            }
        }

        // Implement the AfterCellActivate handler
        private void UltraGrid1_AfterCellActivate(object sender, EventArgs e)
        {
            // This method intentionally left empty
            // No special handling for cell activation or arrow keys
            // All grid navigation will use default behavior
        }

        // Helper method to set focus to the Description column of the last row in the grid
        private void SetFocusToLastRow()
        {
            try
            {
                // Make sure the grid has focus and there is an active row
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Get the last row in the grid instead of the active row
                    UltraGridRow activeRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];

                    // Set this row as the active row and clear any existing selection
                    ultraGrid1.ActiveRow = activeRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(activeRow);

                    // Set focus to the Description cell if it exists
                    if (activeRow.Cells.Exists("Description"))
                    {
                        ultraGrid1.ActiveCell = activeRow.Cells["Description"];

                        // Set focus to the grid and enter edit mode
                        ultraGrid1.Focus();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting focus to Description: " + ex.Message);
            }
        }

        // Override OnShown to set focus after the form is completely shown
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Set focus to the Vendorbutton
            this.ActiveControl = Vendorbutton;

            // Force focus if needed using BeginInvoke to ensure UI is fully loaded
            this.BeginInvoke(new Action(() =>
            {
                Vendorbutton.Focus();
            }));
        }

        private void frmPurchaseReturn_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle F1 key to clear the form
            if (e.KeyCode == Keys.F1)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ClearForm();
                // Ensure focus is set to vendor button or appropriate control
                Control focusControl = this.Controls.Find("BtnDial", true).FirstOrDefault();
                if (focusControl == null)
                {
                    focusControl = this.Controls.Find("Vendorbutton", true).FirstOrDefault();
                }
                if (focusControl == null && VendorName != null)
                {
                    focusControl = VendorName;
                }
                if (focusControl != null)
                {
                    focusControl.Focus();
                }
                return;
            }

            // Only handle Tab key presses
            if (e.KeyCode == Keys.Tab)
            {
                // Check if focus is currently in the UltraGrid
                if (ultraGrid1.Focused || ultraGrid1.ActiveCell != null)
                {
                    // Let the grid handle tab navigation internally
                    // Don't modify this behavior as it allows tab key to work within the grid
                    return;
                }

                e.Handled = true; // Mark as handled to prevent default tab behavior
                e.SuppressKeyPress = true; // Suppress the key press to prevent beep sound

                // Use the custom focus cycling regardless of grid state
                CycleFocus(!e.Shift); // Forward if regular Tab, backward if Shift+Tab
            }
        }

        // Method to set focus to our target controls in sequence
        private void CycleFocus(bool forward = true)
        {
            Control currentControl = this.ActiveControl;

            if (forward)
            {
                // Forward cycling
                if (currentControl == Vendorbutton)
                    ultraDateTimeEditor1.Focus();
                else if (currentControl == ultraDateTimeEditor1)
                    TxtBarcode.Focus();
                else if (currentControl == TxtBarcode)
                    btnAddPurchaceList.Focus();
                else if (currentControl == btnAddPurchaceList)
                    button1.Focus();
                else if (currentControl == button1)
                    Vendorbutton.Focus();
                else
                    Vendorbutton.Focus();
            }
            else
            {
                // Backward cycling
                if (currentControl == Vendorbutton)
                    button1.Focus();
                else if (currentControl == button1)
                    btnAddPurchaceList.Focus();
                else if (currentControl == btnAddPurchaceList)
                    TxtBarcode.Focus();
                else if (currentControl == TxtBarcode)
                    ultraDateTimeEditor1.Focus();
                else if (currentControl == ultraDateTimeEditor1)
                    Vendorbutton.Focus();
                else
                    Vendorbutton.Focus();
            }
        }

        // Helper method removed - Amount cell is no longer calculated automatically

        // Helper method to determine the next column to navigate to
        private string GetNextColumnForNavigation(string currentColumn)
        {
            // Map of column navigation - where to go next when Enter is pressed in each column
            switch (currentColumn)
            {
                case "Description": return "Unit";
                case "Unit": return "Packing";
                case "Packing": return "Cost";
                case "Cost": return "Quantity";
                case "Quantity": return "Returned qty"; // Navigate to Returned qty after Quantity
                case "Returned qty": return "Reason"; // Navigate to Reason after Returned qty
                case "Reason": return "Returned"; // Navigate to Returned after Reason
                case "Returned": return "Unit"; // Cycle back to Unit of same or next row
                case "Amount": return "Unit"; // For completeness, though Amount is not usually editable
                default: return "Unit"; // Default to Unit if unknown column
            }
        }

        // Helper method to calculate and update a row's Returned value and the Net Amount
        private void CalculateAndUpdateRow(UltraGridRow row, bool highlightIfChanged = false)
        {
            if (row == null || !row.Cells.Exists("Cost") || !row.Cells.Exists("Quantity"))
                return;

            try
            {
                // Get the current values
                decimal cost = 0;
                double quantity = 0;

                // Parse the current values with error handling
                if (row.Cells["Cost"].Value != null)
                    decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                if (row.Cells["Quantity"].Value != null)
                    double.TryParse(row.Cells["Quantity"].Value.ToString(), out quantity);

                // Get tax values from the row if they exist
                double taxPercentage = 0;
                string taxType = "excl";
                if (row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null &&
                    double.TryParse(row.Cells["TaxPer"].Value.ToString(), out double existingTaxPer))
                {
                    taxPercentage = existingTaxPer;
                }
                if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null &&
                    !string.IsNullOrEmpty(row.Cells["TaxType"].Value.ToString()))
                {
                    taxType = row.Cells["TaxType"].Value.ToString().Trim();
                }

                // Normalize tax type for comparison (handle "Incl", "incl", "I", etc.)
                string normalizedTaxType = taxType?.ToLower();
                if (normalizedTaxType == "i" || normalizedTaxType == "incl" || normalizedTaxType == "inclusive")
                {
                    normalizedTaxType = "incl";
                }
                else
                {
                    normalizedTaxType = "excl";
                }

                // Determine the base amount for tax calculation
                // For inclusive tax, use Amount field if available (it's already tax-inclusive)
                // For exclusive tax, calculate from Cost * Quantity
                double baseAmount = (double)(cost * (decimal)quantity);

                // For inclusive tax, ALWAYS use Amount field if it exists and has a value
                // This ensures we use the correct tax-inclusive total from the database
                if (normalizedTaxType == "incl" && row.Cells.Exists("Amount") &&
                    row.Cells["Amount"].Value != null && row.Cells["Amount"].Value != DBNull.Value)
                {
                    if (decimal.TryParse(row.Cells["Amount"].Value.ToString(), out decimal amountValue))
                    {
                        baseAmount = (double)amountValue;
                    }
                }

                // Preserve existing TaxAmt from database - do not recalculate
                // Only update TaxPer and TaxType if needed, but preserve TaxAmt
                if (row.Cells.Exists("TaxPer"))
                {
                    row.Cells["TaxPer"].Value = taxPercentage;
                }
                // DO NOT overwrite TaxAmt - preserve the exact value from database
                // TaxAmt should only come from source data, never recalculated
                if (row.Cells.Exists("TaxType"))
                {
                    // Store the tax type in display format (Incl/Excl) but use normalized for calculation
                    string displayTaxType = normalizedTaxType == "incl" ? "Incl" : "Excl";
                    row.Cells["TaxType"].Value = displayTaxType;
                }

                // Update the net total
                UpdateNetAmount();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating row: {ex.Message}");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Check if items are already loaded via btnAddPurchaceList (vendor purchase items)
                // Only restrict if the Tag is NOT "PurchaseReturnUpdate"
                if (_isPurchaseDataLoaded && (this.Tag == null || this.Tag.ToString() != "PurchaseReturnUpdate"))
                {
                    MessageBox.Show("Invalid operation. Purchase data is already loaded.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if items are loaded by BtnDial (indicated by "WITHOUT GR")
                // Only block if we actually have items in the grid (not just "WITHOUT GR" in textBox1)
                if (textBox1.Text == "WITHOUT GR" && HasValidItems())
                {
                    MessageBox.Show("Invalid operation. Items already loaded via direct selection.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Removed restriction that prevented loading multiple times
                // Now allowing repeated loading via button1 even when previously loaded via button1

                if (string.IsNullOrEmpty(TxtSRNO.Text))
                {
                    MessageBox.Show("Please enter a PR number.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Open the Purchase Return Update form
                PurcahseReturnUpdate updateForm = new PurcahseReturnUpdate();
                if (updateForm.ShowDialog() == DialogResult.OK)
                {
                    // Save the original readonly state of TxtBarcode
                    _originalTxtBarcodeReadOnly = TxtBarcode.ReadOnly;

                    // Make TxtBarcode read-only since items are loaded via button1 (purchasereturnupdate)
                    TxtBarcode.ReadOnly = true;

                    int selectedPRNo = updateForm.SelectedPRNo;
                    System.Diagnostics.Debug.WriteLine($"Selected PR No: {selectedPRNo}");

                    // Set the PR number in TxtSRNO
                    if (TxtSRNO != null)
                    {
                        TxtSRNO.Text = selectedPRNo.ToString();
                    }

                    // Clear the existing grid data before loading new items
                    if (ultraGrid1.DataSource is DataTable existingData)
                    {
                        existingData.Clear();
                    }

                    // Get the PR master data to populate form fields
                    ModelClass.TransactionModels.PReturnMaster prMaster = GetPurchaseReturnMasterData(selectedPRNo);
                    System.Diagnostics.Debug.WriteLine($"PR Master returned: {(prMaster != null ? "Valid object" : "NULL")}");

                    // Store current cursor
                    Cursor.Current = Cursors.WaitCursor;

                    // Load vendor details, payment mode, return date, and purchase number to form fields
                    if (prMaster != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"PR Master Details: VendorName={prMaster.VendorName}, LedgerID={prMaster.LedgerID}, " +
                            $"Paymode={prMaster.Paymode}, PaymodeID={prMaster.PaymodeID}, " +
                            $"PReturnDate={prMaster.PReturnDate.ToString("yyyy-MM-dd")}, InvoiceNo={prMaster.InvoiceNo}");

                        // Set vendor name
                        if (!string.IsNullOrEmpty(prMaster.VendorName))
                        {
                            System.Diagnostics.Debug.WriteLine($"Setting VendorName to: {prMaster.VendorName}");
                            VendorName.Text = prMaster.VendorName;
                        }

                        // Set vendor ID
                        if (prMaster.LedgerID > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Setting vendorid to: {prMaster.LedgerID}");
                            vendorid.Text = prMaster.LedgerID.ToString();
                        }


                        // Set return date in ultraDateTimeEditor1
                        if (prMaster.PReturnDate != DateTime.MinValue)
                        {
                            System.Diagnostics.Debug.WriteLine($"Setting return date to: {prMaster.PReturnDate.ToString("yyyy-MM-dd")}");
                            ultraDateTimeEditor1.Value = prMaster.PReturnDate;
                        }

                        // Set invoice date in ultraDateTimeEditor2
                        if (prMaster.InvoiceDate != DateTime.MinValue)
                        {
                            System.Diagnostics.Debug.WriteLine($"Setting invoice date to: {prMaster.InvoiceDate.ToString("yyyy-MM-dd")}");
                            ultraDateTimeEditor2.Value = prMaster.InvoiceDate;
                        }
                        else if (prMaster.PReturnDate != DateTime.MinValue)
                        {
                            // Fallback: if invoice date is not available, use return date
                            System.Diagnostics.Debug.WriteLine($"Invoice date not available, using return date: {prMaster.PReturnDate.ToString("yyyy-MM-dd")}");
                            ultraDateTimeEditor2.Value = prMaster.PReturnDate;
                        }

                        // Set purchase number in textBox1
                        if (!string.IsNullOrEmpty(prMaster.InvoiceNo))
                        {
                            System.Diagnostics.Debug.WriteLine($"Setting invoice number to: {prMaster.InvoiceNo}");
                            string purchaseNoText = NormalizePurchaseNoText(prMaster.InvoiceNo);
                            textBox1.Text = int.TryParse(purchaseNoText, out int loadedPurchaseNo)
                                ? FormatPurchaseNoForDisplay(loadedPurchaseNo)
                                : prMaster.InvoiceNo;
                        }

                        // Set payment method - only if we have valid payment method data from database
                        // If no payment method found, leave it at "Select payment" (no hardcoding)
                        if (prMaster.PaymodeID > 0 && cmbPaymntMethod != null)
                        {
                            // Cache payment information
                            int cachedPaymodeId = prMaster.PaymodeID;
                            string cachedPaymodeName = prMaster.Paymode ?? "";

                            // Ensure RefreshPaymode is called before using the control
                            RefreshPaymode();

                            // More reliable way to set payment method - first check if cached method matches an item
                            bool paymentSet = false;

                            // Direct attempt to set payment method using ID
                            if (cachedPaymodeId > 0)
                            {
                                try
                                {
                                    cmbPaymntMethod.Value = cachedPaymodeId;
                                    paymentSet = cmbPaymntMethod.Value != null &&
                                                Convert.ToInt32(cmbPaymntMethod.Value) == cachedPaymodeId;

                                    if (paymentSet)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Set payment method by ID: {prMaster.Paymode} (ID: {prMaster.PaymodeID})");
                                    }
                                }
                                catch
                                {
                                    // Continue to next method
                                }
                            }

                            // If setting by ID failed, try setting by index
                            if (!paymentSet && cachedPaymodeId > 0)
                            {
                                for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                                {
                                    Infragistics.Win.ValueListItem item = cmbPaymntMethod.Items[i] as Infragistics.Win.ValueListItem;
                                    if (item != null && item.DataValue is DataRowView)
                                    {
                                        DataRowView row = item.DataValue as DataRowView;
                                        if (Convert.ToInt32(row["PayModeID"]) == cachedPaymodeId)
                                        {
                                            cmbPaymntMethod.SelectedIndex = i;
                                            paymentSet = true;
                                            System.Diagnostics.Debug.WriteLine($"Set payment method by index: {prMaster.Paymode} (ID: {prMaster.PaymodeID})");
                                            break;
                                        }
                                    }
                                }
                            }

                            // If still not set, try using the name
                            if (!paymentSet && !string.IsNullOrEmpty(cachedPaymodeName))
                            {
                                for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                                {
                                    Infragistics.Win.ValueListItem item = cmbPaymntMethod.Items[i] as Infragistics.Win.ValueListItem;
                                    if (item != null && item.DataValue is DataRowView)
                                    {
                                        DataRowView row = item.DataValue as DataRowView;
                                        if (row["PayModeName"].ToString().Equals(cachedPaymodeName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            cmbPaymntMethod.SelectedIndex = i;
                                            paymentSet = true;
                                            System.Diagnostics.Debug.WriteLine($"Set payment method by name: {prMaster.Paymode} (ID: {prMaster.PaymodeID})");
                                            break;
                                        }
                                    }
                                }
                            }

                            // If payment method not found in list, leave it at "Select payment" (first item)
                            // No hardcoding - just don't set anything if not found
                            if (!paymentSet)
                            {
                                System.Diagnostics.Debug.WriteLine($"Payment method {cachedPaymodeName} (ID: {cachedPaymodeId}) not found in list. Leaving at 'Select payment'.");
                                cmbPaymntMethod.SelectedIndex = 0; // Reset to "Select payment"
                            }
                        }
                        else
                        {
                            // No payment method in database - set to "Select payment" (first item)
                            if (cmbPaymntMethod != null && cmbPaymntMethod.Items.Count > 0)
                            {
                                cmbPaymntMethod.SelectedIndex = 0;
                                System.Diagnostics.Debug.WriteLine("No payment method in database. Set to 'Select payment'.");
                            }
                        }
                    }

                    // Get PR details for the grid
                    DataTable prDetails = GetPRDetailsByPRNo(selectedPRNo);

                    if (prDetails != null && prDetails.Rows.Count > 0)
                    {
                        // Set the data source for the grid
                        ultraGrid1.DataSource = prDetails;

                        // Apply grid formatting to ensure only the specified columns are visible
                        ConfigureItemsGridLayout();

                        // Make sure reason values are preserved - IMPORTANT for keeping original reasons
                        PreserveReasonValues();

                        // Double-check that reasons are displayed properly in the grid
                        foreach (UltraGridRow row in ultraGrid1.Rows)
                        {
                            if (row.Cells.Exists("Reason") && row.Cells["Reason"].Value != null)
                            {
                                string reason = row.Cells["Reason"].Value.ToString();
                                System.Diagnostics.Debug.WriteLine($"Row {row.Index} has reason: '{reason}'");
                            }
                        }

                        // Calculate totals
                        UpdateNetAmount();

                        // Make ultraPictureBox4 visible and hide pbxSave when items are loaded
                        ultraPictureBox4.Visible = true;
                        pbxSave.Visible = false;

                        // Set the flag to indicate purchase data is loaded via button1
                        _isPurchaseDataLoaded = true;

                        // Set a special flag in the Tag property to indicate loaded via button1
                        this.Tag = "PurchaseReturnUpdate";


                        // Set focus to the Unit cell of the first row
                        this.BeginInvoke(new Action(() =>
                        {
                            SetFocusToUnitCell();
                        }));
                    }
                    else
                    {
                        MessageBox.Show("No items found for this purchase return.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in button1_Click_1: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                MessageBox.Show("Error loading PR details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor.Current = Cursors.Default;
            }
        }

        // Method to get purchase return master data
        private ModelClass.TransactionModels.PReturnMaster GetPurchaseReturnMasterData(int prNo)
        {
            try
            {
                // Use the repository to get PR master data - with more debug information
                using (SqlConnection conn = (SqlConnection)new BaseRepostitory().DataConnection)
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine($"SQL connection opened for PR: {prNo}");

                    // Direct query to find PR by PR number
                    string directPRQuery = "SELECT * FROM PReturnMaster WHERE PReturnNo = @PReturnNo AND BranchId = @BranchId AND CompanyId = @CompanyId";
                    using (SqlCommand cmd = new SqlCommand(directPRQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PReturnNo", prNo);
                        cmd.Parameters.AddWithValue("@BranchId", ModelClass.SessionContext.BranchId);
                        cmd.Parameters.AddWithValue("@CompanyId", ModelClass.SessionContext.CompanyId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ModelClass.TransactionModels.PReturnMaster prMaster = new ModelClass.TransactionModels.PReturnMaster();

                                // Get Id first - this is critical for updates
                                if (!reader.IsDBNull(reader.GetOrdinal("Id")))
                                {
                                    prMaster.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved Id: {prMaster.Id}");
                                }

                                // Map the basic fields we need
                                prMaster.PReturnNo = prNo;

                                // Get LedgerID (vendor ID)
                                if (!reader.IsDBNull(reader.GetOrdinal("LedgerID")))
                                {
                                    prMaster.LedgerID = reader.GetInt32(reader.GetOrdinal("LedgerID"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved LedgerID: {prMaster.LedgerID}");
                                }

                                // Get VendorName
                                if (!reader.IsDBNull(reader.GetOrdinal("VendorName")))
                                {
                                    prMaster.VendorName = reader.GetString(reader.GetOrdinal("VendorName"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved VendorName: {prMaster.VendorName}");
                                }

                                // Get Invoice number
                                if (!reader.IsDBNull(reader.GetOrdinal("InvoiceNo")))
                                {
                                    prMaster.InvoiceNo = reader.GetString(reader.GetOrdinal("InvoiceNo"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved InvoiceNo: {prMaster.InvoiceNo}");
                                }

                                // Get PaymodeID
                                if (!reader.IsDBNull(reader.GetOrdinal("PaymodeID")))
                                {
                                    prMaster.PaymodeID = reader.GetInt32(reader.GetOrdinal("PaymodeID"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved PaymodeID: {prMaster.PaymodeID}");
                                }

                                // Get Paymode name
                                if (!reader.IsDBNull(reader.GetOrdinal("Paymode")))
                                {
                                    prMaster.Paymode = reader.GetString(reader.GetOrdinal("Paymode"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved Paymode: {prMaster.Paymode}");
                                }

                                // Get PReturn date
                                if (!reader.IsDBNull(reader.GetOrdinal("PReturnDate")))
                                {
                                    prMaster.PReturnDate = reader.GetDateTime(reader.GetOrdinal("PReturnDate"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved PReturnDate: {prMaster.PReturnDate.ToString("yyyy-MM-dd")}");
                                }

                                // Get InvoiceDate
                                if (!reader.IsDBNull(reader.GetOrdinal("InvoiceDate")))
                                {
                                    prMaster.InvoiceDate = reader.GetDateTime(reader.GetOrdinal("InvoiceDate"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved InvoiceDate: {prMaster.InvoiceDate.ToString("yyyy-MM-dd")}");
                                }

                                // Get SubTotal
                                if (!reader.IsDBNull(reader.GetOrdinal("SubTotal")))
                                {
                                    prMaster.SubTotal = reader.GetDouble(reader.GetOrdinal("SubTotal"));
                                    System.Diagnostics.Debug.WriteLine($"Retrieved SubTotal: {prMaster.SubTotal}");
                                }

                                return prMaster;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Direct SQL query found no matching record");
                            }
                        }
                    }

                    // If direct query failed, try with stored procedure as fallback
                    using (SqlCommand cmd = new SqlCommand("_POS_PurchaseReturn", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "GETAllPurchaseReturn");
                        cmd.Parameters.AddWithValue("@PReturnNo", prNo);
                        cmd.Parameters.AddWithValue("@CompanyId", ModelClass.DataBase.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", ModelClass.DataBase.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", ModelClass.DataBase.FinyearId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Debug the actual data we received
                            System.Diagnostics.Debug.WriteLine($"Stored procedure returned {dt.Rows.Count} rows");
                            System.Diagnostics.Debug.WriteLine("Columns in result set:");
                            foreach (DataColumn col in dt.Columns)
                            {
                                System.Diagnostics.Debug.WriteLine($"  {col.ColumnName} ({col.DataType})");
                            }

                            // Filter to get only the record matching the PR number
                            DataRow[] rows = dt.Select($"PReturnNo = {prNo}");
                            System.Diagnostics.Debug.WriteLine($"After filtering, found {rows.Length} matching rows for PR: {prNo}");

                            if (rows.Length > 0)
                            {
                                DataRow row = rows[0];
                                ModelClass.TransactionModels.PReturnMaster prMaster = new ModelClass.TransactionModels.PReturnMaster();

                                // Debug the actual row data
                                System.Diagnostics.Debug.WriteLine("Row data for matching PR:");
                                foreach (DataColumn col in dt.Columns)
                                {
                                    string value = row[col] == DBNull.Value ? "NULL" : row[col].ToString();
                                    System.Diagnostics.Debug.WriteLine($"  {col.ColumnName}: {value}");
                                }

                                // Get Id first - this is critical for updates
                                if (dt.Columns.Contains("Id") && row["Id"] != DBNull.Value)
                                {
                                    prMaster.Id = Convert.ToInt32(row["Id"]);
                                    System.Diagnostics.Debug.WriteLine("Id: " + prMaster.Id);
                                }

                                // Map the basic fields we need
                                prMaster.PReturnNo = prNo;

                                if (dt.Columns.Contains("LedgerID") && row["LedgerID"] != DBNull.Value)
                                {
                                    prMaster.LedgerID = Convert.ToInt32(row["LedgerID"]);
                                    System.Diagnostics.Debug.WriteLine("LedgerID: " + prMaster.LedgerID);
                                }

                                if (dt.Columns.Contains("InvoiceNo") && row["InvoiceNo"] != DBNull.Value)
                                {
                                    prMaster.InvoiceNo = row["InvoiceNo"].ToString();
                                    System.Diagnostics.Debug.WriteLine("Invoice No: " + prMaster.InvoiceNo);
                                }

                                if (dt.Columns.Contains("PReturnDate") && row["PReturnDate"] != DBNull.Value)
                                {
                                    prMaster.PReturnDate = Convert.ToDateTime(row["PReturnDate"]);
                                    System.Diagnostics.Debug.WriteLine("PReturn Date: " + prMaster.PReturnDate.ToString("yyyy-MM-dd"));
                                }

                                if (dt.Columns.Contains("InvoiceDate") && row["InvoiceDate"] != DBNull.Value)
                                {
                                    prMaster.InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]);
                                    System.Diagnostics.Debug.WriteLine("Invoice Date: " + prMaster.InvoiceDate.ToString("yyyy-MM-dd"));
                                }

                                if (dt.Columns.Contains("Paymode") && row["Paymode"] != DBNull.Value)
                                {
                                    prMaster.Paymode = row["Paymode"].ToString();
                                    System.Diagnostics.Debug.WriteLine("Pay Mode: " + prMaster.Paymode);
                                }

                                if (dt.Columns.Contains("PaymodeID") && row["PaymodeID"] != DBNull.Value)
                                {
                                    prMaster.PaymodeID = Convert.ToInt32(row["PaymodeID"]);
                                    System.Diagnostics.Debug.WriteLine("Pay Mode ID: " + prMaster.PaymodeID);
                                }

                                if (dt.Columns.Contains("VendorName") && row["VendorName"] != DBNull.Value)
                                {
                                    prMaster.VendorName = row["VendorName"].ToString();
                                    System.Diagnostics.Debug.WriteLine("Vendor Name: " + prMaster.VendorName);
                                }

                                if (dt.Columns.Contains("SubTotal") && row["SubTotal"] != DBNull.Value)
                                {
                                    prMaster.SubTotal = Convert.ToDouble(row["SubTotal"]);
                                    System.Diagnostics.Debug.WriteLine("SubTotal: " + prMaster.SubTotal);
                                }

                                return prMaster;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("No purchase return found with PR No: " + prNo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting PR master data: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
            }

            return null;
        }

        private DataTable GetPRDetailsByPRNo(int prNo)
        {
            DataTable dt = new DataTable();
            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetching purchase return details for PR#{prNo}");

                // Create a BaseRepository instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use direct SQL to fetch the exact data needed
                using (SqlConnection conn = (SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();

                    // Direct SQL query to get the correct Qty and Reason fields
                    // Updated to include Company, Branch and FinYear filters
                    string sql = @"SELECT 
                                  prd.SlNo,
                                  prd.ItemID, 
                                  im.Description AS ItemName,
                                  ps.BarCode,
                                  prd.Unit,
                                  prd.Packing,
                                  prd.Cost,
                                  -- Use TotalSP if available (saved recalculated value), otherwise calculate from Returned qty
                                  -- This ensures we load the correct amount that was saved
                                  CASE 
                                      WHEN prd.TotalSP > 0 THEN prd.TotalSP
                                      WHEN prd.Returned > 0 THEN (prd.Cost * prd.Returned)
                                      ELSE (prd.Cost * prd.Qty)
                                  END AS Amount,
                                  prd.Qty,
                                  prd.Reason,
                                  prd.UnitId,
                                  prd.TaxType,
                                  prd.TaxPer,
                                  prd.TaxAmt,
                                  prd.Returnqty,
                                  prd.Returned,
                                  prd.TotalSP
                               FROM 
                                  PReturnDetails prd
                               LEFT JOIN 
                                  ItemMaster im ON prd.ItemID = im.ItemId
                               LEFT JOIN 
                                  PriceSettings ps ON prd.ItemID = ps.ItemId AND prd.UnitId = ps.UnitId
                               WHERE
                                  prd.PReturnNo = @PReturnNo
                                  AND prd.CompanyId = @CompanyId 
                                  AND prd.BranchID = @BranchId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PReturnNo", prNo);
                        cmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                        cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));

                        System.Diagnostics.Debug.WriteLine($"Query parameters: PR#{prNo}, CompanyId={DataBase.CompanyId}, BranchId={DataBase.BranchId}");

                        // Check if this is from a vendor search by looking at how the form was opened
                        bool isFromVendorSearch = false;

                        // If a form with this PR number was opened through a vendor selection dialog
                        if (this.Tag != null && this.Tag.ToString() == "VendorSearch")
                        {
                            isFromVendorSearch = true;
                        }

                        // Create a formatted DataTable with the exact column names needed for the grid
                        DataTable formattedTable = new DataTable();
                        formattedTable.Columns.Add("Sl No", typeof(int));
                        formattedTable.Columns.Add("Description", typeof(string));
                        formattedTable.Columns.Add("Barcode", typeof(string));
                        formattedTable.Columns.Add("Unit", typeof(string));
                        formattedTable.Columns.Add("Packing", typeof(double));
                        formattedTable.Columns.Add("Cost", typeof(decimal));
                        formattedTable.Columns.Add("Quantity", typeof(double));
                        formattedTable.Columns.Add("Amount", typeof(decimal));
                        formattedTable.Columns.Add("Returned", typeof(decimal));
                        formattedTable.Columns.Add("Returned qty", typeof(double));
                        formattedTable.Columns.Add("Reason", typeof(string));
                        formattedTable.Columns.Add("SELECT", typeof(bool)); // Add SELECT column for checkboxes

                        // Only add ItemID and UnitId if not from vendor search
                        if (!isFromVendorSearch)
                        {
                            // Hidden columns for reference
                            formattedTable.Columns.Add("ItemID", typeof(long));
                            formattedTable.Columns.Add("UnitId", typeof(int));
                        }

                        // Add TaxPer, TaxAmt and TaxType columns
                        formattedTable.Columns.Add("TaxPer", typeof(double));
                        formattedTable.Columns.Add("TaxAmt", typeof(double));
                        formattedTable.Columns.Add("TaxType", typeof(string));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Convert the raw SQL data to our formatted table
                            DataTable rawData = new DataTable();
                            rawData.Load(reader);

                            // Debug diagnostics
                            System.Diagnostics.Debug.WriteLine($"Raw data returned {rawData.Rows.Count} rows with columns:");
                            foreach (DataColumn col in rawData.Columns)
                            {
                                System.Diagnostics.Debug.WriteLine($"  {col.ColumnName} ({col.DataType})");
                            }

                            // Copy data to the formatted table
                            for (int i = 0; i < rawData.Rows.Count; i++)
                            {
                                DataRow rawRow = rawData.Rows[i];
                                DataRow newRow = formattedTable.NewRow();

                                try
                                {
                                    // Set Sl No
                                    newRow["Sl No"] = i + 1;

                                    // Copy Description, BarCode, Unit, Packing, Cost, Quantity, Amount, Reason fields
                                    if (rawRow["ItemName"] != DBNull.Value)
                                        newRow["Description"] = rawRow["ItemName"];

                                    if (rawRow["BarCode"] != DBNull.Value)
                                        newRow["Barcode"] = rawRow["BarCode"];

                                    if (rawRow["Unit"] != DBNull.Value)
                                        newRow["Unit"] = rawRow["Unit"];

                                    if (rawRow["Packing"] != DBNull.Value)
                                        newRow["Packing"] = Convert.ToDouble(rawRow["Packing"]);

                                    if (rawRow["Cost"] != DBNull.Value)
                                        newRow["Cost"] = Convert.ToDecimal(rawRow["Cost"]);

                                    if (rawRow["Qty"] != DBNull.Value)
                                        newRow["Quantity"] = Convert.ToDouble(rawRow["Qty"]);

                                    // Load Returned amount from database - this is what was saved previously
                                    if (rawRow["Returned"] != DBNull.Value && rawRow["Returned"] != null)
                                    {
                                        newRow["Returned"] = Convert.ToDecimal(rawRow["Returned"]);
                                    }
                                    else
                                    {
                                        // If Returned is not in database, default to 0
                                        newRow["Returned"] = 0.00m;
                                    }

                                    // Reset Returned qty to 0 when loading - user will enter new return qty
                                    newRow["Returned qty"] = 0.00;

                                    // Load Amount from database - use the saved value (from TotalSP or calculated)
                                    // This preserves the exact amount that was saved, not recalculated
                                    decimal cost = 0;
                                    if (rawRow["Cost"] != DBNull.Value)
                                        cost = Convert.ToDecimal(rawRow["Cost"]);

                                    // Use the Amount from database query (which uses TotalSP or calculates from Returned)
                                    // This ensures we show the same amount that was saved
                                    if (rawRow["Amount"] != DBNull.Value)
                                    {
                                        // Use the saved Amount from database (from TotalSP or calculated)
                                        newRow["Amount"] = Convert.ToDecimal(rawRow["Amount"]);
                                    }
                                    else
                                    {
                                        // Fallback: calculate from Returned if available, otherwise from Qty
                                        decimal returned = 0;
                                        if (rawRow["Returned"] != DBNull.Value)
                                            returned = Convert.ToDecimal(rawRow["Returned"]);

                                        if (returned > 0)
                                        {
                                            newRow["Amount"] = cost * returned;
                                        }
                                        else
                                        {
                                            double qty = 0;
                                            if (rawRow["Qty"] != DBNull.Value)
                                                qty = Convert.ToDouble(rawRow["Qty"]);
                                            newRow["Amount"] = cost * (decimal)qty;
                                        }
                                    }

                                    // IMPROVED: Handle Reason field properly - preserve exact reason values
                                    if (rawRow["Reason"] != DBNull.Value && !string.IsNullOrWhiteSpace(rawRow["Reason"].ToString()))
                                    {
                                        string reasonValue = rawRow["Reason"].ToString().Trim();
                                        newRow["Reason"] = reasonValue;
                                        // Log each reason value to help debug
                                        System.Diagnostics.Debug.WriteLine($"Row {i + 1} Reason from DB: '{reasonValue}'");
                                    }
                                    else
                                    {
                                        newRow["Reason"] = "Select Reason";
                                        System.Diagnostics.Debug.WriteLine($"Row {i + 1} Reason not found, using default");
                                    }

                                    // Set SELECT column to false by default - always unchecked initially
                                    newRow["SELECT"] = false;

                                    // Copy TaxType, TaxPer, TaxAmt from raw data
                                    if (rawRow["TaxType"] != DBNull.Value && !string.IsNullOrWhiteSpace(rawRow["TaxType"].ToString()))
                                    {
                                        string dbTaxTypeValue = rawRow["TaxType"].ToString().Trim().ToUpper();
                                        // Convert to display format: "I" -> "Incl", "E" -> "Excl"
                                        if (dbTaxTypeValue == "I")
                                            newRow["TaxType"] = "Incl";
                                        else if (dbTaxTypeValue == "E")
                                            newRow["TaxType"] = "Excl";
                                        else
                                            newRow["TaxType"] = dbTaxTypeValue; // Keep original if not I or E
                                    }
                                    else
                                        newRow["TaxType"] = "Incl"; // Default to Inclusive (display format)

                                    if (rawRow["TaxPer"] != DBNull.Value)
                                        newRow["TaxPer"] = Convert.ToDouble(rawRow["TaxPer"]);
                                    else
                                        newRow["TaxPer"] = 0.0;

                                    if (rawRow["TaxAmt"] != DBNull.Value)
                                        newRow["TaxAmt"] = Convert.ToDouble(rawRow["TaxAmt"]);
                                    else
                                        newRow["TaxAmt"] = 0.0;

                                    // Set ItemID and UnitId only if not from vendor search
                                    if (!isFromVendorSearch)
                                    {
                                        // ItemID field with safe conversion
                                        long itemId = 0;
                                        if (rawRow["ItemID"] != DBNull.Value)
                                        {
                                            long.TryParse(rawRow["ItemID"].ToString(), out itemId);
                                        }
                                        newRow["ItemID"] = itemId;

                                        // UnitId field with safe conversion
                                        int unitId = 0;
                                        if (rawRow["UnitId"] != DBNull.Value)
                                        {
                                            int.TryParse(rawRow["UnitId"].ToString(), out unitId);
                                        }
                                        newRow["UnitId"] = unitId;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // If there's an error with a specific field conversion, log it but continue
                                    System.Diagnostics.Debug.WriteLine($"Error converting row {i}: {ex.Message}");
                                }

                                formattedTable.Rows.Add(newRow);
                            }

                            return formattedTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving purchase return details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        private void ultraPictureBox4_Click(object sender, EventArgs e)
        {
            // Make sure this button stays visible when clicked
            ultraPictureBox4.Visible = true;
            pbxSave.Visible = false;

            // Call UpdatePurchaseReturn instead of SavePurchaseReturn
            UpdatePurchaseReturn();
        }

        // Handle row selection highlighting
        private void UltraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            try
            {
                // Only focus the row - no additional functionality
                if (ultraGrid1.Selected.Rows.Count > 0)
                {
                    // Ensure only one row is focused
                    UltraGridRow selectedRow = ultraGrid1.Selected.Rows[0];

                    // Set as active row
                    ultraGrid1.ActiveRow = selectedRow;

                    // Restore highlighting for all rows with checked checkboxes
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("SELECT") &&
                            row.Cells["SELECT"].Value != null &&
                            row.Cells["SELECT"].Value != DBNull.Value &&
                            (bool)row.Cells["SELECT"].Value)
                        {
                            // Re-apply blue highlight for checked rows
                            row.Appearance.BackColor = Color.LightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in row selection: " + ex.Message);
            }
        }

        // This event handler implements "Select All" checkbox functionality for the SELECT column
        private void UltraGrid1_MouseUp_ForHeaderCheckbox(object sender, MouseEventArgs e)
        {
            try
            {
                UltraGrid grid = sender as UltraGrid;
                if (grid == null || grid.DisplayLayout.Bands.Count == 0)
                    return;

                // Only handle left-click
                if (e.Button != MouseButtons.Left)
                    return;

                // Check if y-coordinate is in the header region (typically first ~25 pixels)
                if (e.Y > 25)
                    return;

                // Approximate position for the SELECT column (at the end after Reason)
                // Note: This is an approximation and might need adjustment based on actual column widths
                int totalWidth = 0;
                UltraGridBand band = grid.DisplayLayout.Bands[0];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Hidden && col.Key != "SELECT")
                    {
                        totalWidth += col.Width;
                    }
                }

                // Check if the click is in the approximate x-position of the SELECT column
                // which should be somewhere after the Reason column
                int selectColumnStart = totalWidth - 60; // Approximate starting position
                int selectColumnEnd = totalWidth + 60;   // Approximate ending position

                if (e.X < selectColumnStart || e.X > selectColumnEnd)
                    return;

                // Toggle all checkboxes based on current state
                // If any are checked, uncheck all, otherwise check all
                bool isAnyChecked = false;
                foreach (UltraGridRow row in grid.Rows)
                {
                    if (row.Cells.Exists("SELECT") && row.Cells["SELECT"].Value != null && (bool)row.Cells["SELECT"].Value)
                    {
                        isAnyChecked = true;
                        break;
                    }
                }

                // Set the new state (inverse of current state)
                bool newState = !isAnyChecked;

                // First pass - handle the checkbox changes and highlighting
                foreach (UltraGridRow row in grid.Rows)
                {
                    if (row.Cells.Exists("SELECT"))
                    {
                        row.Cells["SELECT"].Value = newState;

                        // Apply the same blue highlighting as in AfterCellUpdate
                        if (newState)
                        {
                            // Apply blue highlight for selected rows
                            row.Appearance.BackColor = Color.LightBlue;

                            // Make Reason cells more prominent if they need a value
                            if (row.Cells.Exists("Reason"))
                            {
                                var reasonCell = row.Cells["Reason"];
                                string reasonValue = reasonCell.Value == null ? "" : reasonCell.Value.ToString().Trim();

                                if (string.IsNullOrEmpty(reasonValue) || reasonValue == "Select Reason")
                                {
                                    // Highlight Reason cells that need values
                                    reasonCell.Appearance.BackColor = Color.LightYellow;
                                    reasonCell.Appearance.ForeColor = Color.Red;
                                }
                            }
                        }
                        else
                        {
                            // Remove highlight for unselected rows
                            row.Appearance.BackColor = Color.Empty;

                            // Reset Reason cell appearance
                            if (row.Cells.Exists("Reason"))
                            {
                                row.Cells["Reason"].Appearance.BackColor = Color.Empty;
                                row.Cells["Reason"].Appearance.ForeColor = Color.Empty;
                            }
                        }
                    }
                }

                // Second pass - if we're checking all boxes, set focus to the first Reason cell that needs a value
                if (newState && grid.Rows.Count > 0)
                {
                    bool foundReasonToEdit = false;

                    foreach (UltraGridRow row in grid.Rows)
                    {
                        if (row.Cells.Exists("Reason"))
                        {
                            string reasonValue = row.Cells["Reason"].Value == null ? "" :
                                row.Cells["Reason"].Value.ToString().Trim();

                            if (string.IsNullOrEmpty(reasonValue) || reasonValue == "Select Reason")
                            {
                                // Focus on this Reason cell since it needs a value
                                grid.ActiveRow = row;
                                grid.ActiveCell = row.Cells["Reason"];

                                // Delay entering edit mode to ensure UI has updated
                                this.BeginInvoke(new Action(() =>
                                {
                                    try
                                    {
                                        grid.PerformAction(UltraGridAction.EnterEditMode);
                                    }
                                    catch
                                    {
                                        // Ignore any errors during edit mode entry
                                    }
                                }));

                                foundReasonToEdit = true;
                                break;
                            }
                        }
                    }

                    // If no empty Reason cells found, just set focus to the first Reason cell
                    if (!foundReasonToEdit && grid.Rows.Count > 0 && grid.Rows[0].Cells.Exists("Reason"))
                    {
                        grid.ActiveRow = grid.Rows[0];
                        grid.ActiveCell = grid.Rows[0].Cells["Reason"];
                    }
                }

                // Force refresh to show the changes
                grid.Invalidate();

                // Show a message prompting user to enter reason if checking all
                if (newState)
                {
                    // Use status bar if available, otherwise a brief message
                    // Don't use MessageBox as it's disruptive
                    this.Text = "Please enter a reason for all selected items";

                    // Reset the form text after 3 seconds
                    System.Threading.Timer timer = null;
                    timer = new System.Threading.Timer((state) =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.Text = "Purchase Return";
                            timer.Dispose();
                        }));
                    }, null, 3000, System.Threading.Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in SELECT column header click handler: " + ex.Message);
            }
        }

        private void panelSubtotalLine_PaintClient(object sender, PaintEventArgs e)
        {
            using (Pen dashedPen = new Pen(Color.Black, 1))
            {
                dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                // Draw a dashed line across the width of the panel
                e.Graphics.DrawLine(dashedPen, 0, ((Control)sender).Height / 2, ((Control)sender).Width, ((Control)sender).Height / 2);
            }
        }

        private void SubTotal_Click(object sender, EventArgs e)
        {
            // Handler for SubTotal label click event
        }

        // Add this helper method at the end of the class before the last closing brace
        private void EnsureTxtBarcodeIsWritable()
        {
            // Store the original ReadOnly state before modifying
            if (TxtBarcode.ReadOnly)
            {
                _originalTxtBarcodeReadOnly = TxtBarcode.ReadOnly;
            }

            // Always make TxtBarcode writable
            TxtBarcode.ReadOnly = false;
        }

        private void SetFocusToUnitCell()
        {
            try
            {
                // Make sure the grid has focus and there are rows
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Get the first row in the grid
                    UltraGridRow firstRow = ultraGrid1.Rows[0];

                    // Set this row as the active row and clear any existing selection
                    ultraGrid1.ActiveRow = firstRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(firstRow);

                    // Focus specifically on the Unit column as requested
                    if (firstRow.Cells.Exists("Unit"))
                    {
                        ultraGrid1.ActiveCell = firstRow.Cells["Unit"];

                        // Set focus to the grid and enter edit mode
                        ultraGrid1.Focus();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                        // Explicitly show highlighting or cursor to indicate the focus is set
                        if (ultraGrid1.ActiveCell != null)
                        {
                            ultraGrid1.ActiveCell.Appearance.BackColor = Color.FromArgb(255, 233, 233);
                            ultraGrid1.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting focus to Unit cell: " + ex.Message);
            }
        }

        // Initialize TrackTrans table to ensure PR numbers are generated correctly
        private void InitializeTrackTrans()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    // Get the connection from the repository
                    conn.ConnectionString = ((SqlConnection)prRepo.DataConnection).ConnectionString;
                    conn.Open();

                    // First, get the maximum PR number from existing records
                    string maxPRQuery = @"
                        SELECT ISNULL(MAX(PReturnNo), 0)
                        FROM PReturnMaster 
                        WHERE CompanyId = @CompanyId 
                        AND BranchId = @BranchId 
                        AND FinYearId = @FinYearId";

                    int maxPRNo = 0;

                    using (SqlCommand maxCmd = new SqlCommand(maxPRQuery, conn))
                    {
                        maxCmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                        maxCmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                        maxCmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                        object maxResult = maxCmd.ExecuteScalar();
                        if (maxResult != null && maxResult != DBNull.Value)
                        {
                            maxPRNo = Convert.ToInt32(maxResult);
                            System.Diagnostics.Debug.WriteLine($"Found maximum PR number: {maxPRNo}");
                        }
                    }

                    // Check if TrackTrans has a record for current branch/year
                    string checkRecordQuery = @"
                        SELECT COUNT(*) 
                        FROM TrackTrans 
                        WHERE BranchID = @BranchId AND FinYearID = @FinYearId";

                    using (SqlCommand checkRecordCmd = new SqlCommand(checkRecordQuery, conn))
                    {
                        checkRecordCmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                        checkRecordCmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                        int recordExists = Convert.ToInt32(checkRecordCmd.ExecuteScalar());

                        if (recordExists > 0)
                        {
                            // Update existing record
                            string updateQuery = @"
                                UPDATE TrackTrans 
                                SET PRBillNo = @PRBillNo
                                WHERE BranchID = @BranchId AND FinYearID = @FinYearId";

                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@PRBillNo", maxPRNo);
                                updateCmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                                updateCmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                                updateCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"FORCEFULLY updated TrackTrans PRBillNo to {maxPRNo}");
                            }

                            // Double-check the update
                            string verifyQuery = @"
                                SELECT PRBillNo 
                                FROM TrackTrans 
                                WHERE BranchID = @BranchId AND FinYearID = @FinYearId";

                            using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, conn))
                            {
                                verifyCmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                                verifyCmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                                object verifyResult = verifyCmd.ExecuteScalar();
                                if (verifyResult != null && verifyResult != DBNull.Value)
                                {
                                    int verifiedPRNo = Convert.ToInt32(verifyResult);
                                    System.Diagnostics.Debug.WriteLine($"Verified TrackTrans PRBillNo is now: {verifiedPRNo}");
                                }
                            }
                        }
                        else
                        {
                            // Insert new record
                            string insertQuery = @"
                                INSERT INTO TrackTrans (BranchID, FinYearID, PRBillNo)
                                VALUES (@BranchId, @FinYearId, @PRBillNo)";

                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                                insertCmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);
                                insertCmd.Parameters.AddWithValue("@PRBillNo", maxPRNo);

                                insertCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Created TrackTrans record with PRBillNo {maxPRNo}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing TrackTrans: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        // Utility method to update purchase return details in the database
        private void UpdatePurchaseReturnDetails(int prNo)
        {
            try
            {
                if (ultraGrid1.Rows.Count == 0)
                {
                    // Silently return without showing a message for empty grid
                    throw new Exception("No items to save");
                }

                // Create a master object to pass information needed for details
                ModelClass.TransactionModels.PReturnMaster pr = new ModelClass.TransactionModels.PReturnMaster();

                // Set the common values for all detail records
                pr.PReturnNo = prNo;
                pr.CompanyId = Convert.ToInt32(DataBase.CompanyId);
                pr.FinYearId = Convert.ToInt32(DataBase.FinyearId);
                pr.BranchId = Convert.ToInt32(DataBase.BranchId);
                pr.PReturnDate = ultraDateTimeEditor1.Value != null ? (DateTime)ultraDateTimeEditor1.Value : DateTime.Now;

                // Get vendor details
                pr.LedgerID = Convert.ToInt32(vendorid.Text);
                pr.VendorName = VendorName.Text;

                ApplyHiddenPaymentDefaults(pr);

                // Get the PReturnMaster Id for updating the record
                int prMasterId = 0;
                try
                {
                    // Get the ID of the PReturnMaster record based on PReturnNo
                    prMasterId = prRepo.GetIdByPReturnNo(prNo);

                    if (prMasterId > 0)
                    {
                        // Set the ID for the update
                        pr.Id = prMasterId;
                    }

                    System.Diagnostics.Debug.WriteLine($"Retrieved PReturnMaster ID: {prMasterId} for PR# {prNo}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting PReturnMaster ID: {ex.Message}");
                }

                // If we found the master ID, we need to get the full record to update properly
                if (prMasterId > 0)
                {
                    try
                    {
                        // Retrieve the full master record
                        ModelClass.TransactionModels.PReturnMaster existingMaster = prRepo.GetById(prMasterId);

                        if (existingMaster != null)
                        {
                            // Preserve the existing InvoiceNo; if it's missing, fall back to the form value
                            // This prevents updates from blanking the InvoiceNo in history views
                            pr.InvoiceNo = !string.IsNullOrWhiteSpace(existingMaster.InvoiceNo)
                                ? existingMaster.InvoiceNo
                                : (textBox1.Text ?? "").Trim();

                            System.Diagnostics.Debug.WriteLine($"InvoiceNo preservation: textBox1='{textBox1.Text}', using existing='{pr.InvoiceNo}'");

                            // Copy the existing values
                            pr.SubTotal = existingMaster.SubTotal;
                            pr.GrandTotal = existingMaster.GrandTotal;
                            pr.TaxAmt = existingMaster.TaxAmt;
                            pr.TaxPer = existingMaster.TaxPer;
                            pr.UserID = existingMaster.UserID;
                            pr.UserName = existingMaster.UserName;
                            pr.BillDiscountAmt = existingMaster.BillDiscountAmt;
                            pr.BillDiscountPer = existingMaster.BillDiscountPer;
                            pr.SpDisPer = existingMaster.SpDisPer;
                            pr.SpDsiAmt = existingMaster.SpDsiAmt;
                            pr.Frieght = existingMaster.Frieght;
                            pr.ExpenseAmt = existingMaster.ExpenseAmt;
                            pr.OtherExpAmt = existingMaster.OtherExpAmt;
                            pr.RoundOff = existingMaster.RoundOff;
                            pr.CessPer = existingMaster.CessPer;
                            pr.CessAmt = existingMaster.CessAmt;
                            pr.CalAfterTax = existingMaster.CalAfterTax;
                            pr.CurrencyID = existingMaster.CurrencyID;
                            pr.CurSymbol = existingMaster.CurSymbol;

                            // CRITICAL: Copy these fields - they are required by the stored procedure
                            pr.SeriesID = existingMaster.SeriesID > 0 ? existingMaster.SeriesID : 1;
                            pr.VoucherID = existingMaster.VoucherID;
                            pr.TrnsType = !string.IsNullOrEmpty(existingMaster.TrnsType) ? existingMaster.TrnsType : "PR";
                            pr.VoucherType = existingMaster.VoucherType ?? "";

                            pr.InvoiceDate = existingMaster.InvoiceDate;
                            pr.TaxType = existingMaster.TaxType;
                            pr.Remarks = existingMaster.Remarks;
                            pr.CancelFlag = existingMaster.CancelFlag;
                            pr.CreditPeriod = existingMaster.CreditPeriod;

                            ApplyHiddenPaymentDefaults(pr, existingMaster);

                            // Log the update
                            System.Diagnostics.Debug.WriteLine("=== UPDATING MASTER RECORD ===");
                            System.Diagnostics.Debug.WriteLine($"PReturnNo: {pr.PReturnNo}, Id: {pr.Id}");
                            System.Diagnostics.Debug.WriteLine($"Vendor: {pr.VendorName} (LedgerID: {pr.LedgerID})");
                            System.Diagnostics.Debug.WriteLine($"InvoiceNo: '{pr.InvoiceNo}' (preserved from existing record)");
                            System.Diagnostics.Debug.WriteLine($"Paymode: '{pr.Paymode}', PaymodeID: {pr.PaymodeID}, PaymodeLedgerID: {pr.PaymodeLedgerID}");
                            System.Diagnostics.Debug.WriteLine($"SeriesID: {pr.SeriesID}, VoucherID: {pr.VoucherID}");
                            System.Diagnostics.Debug.WriteLine($"TrnsType: '{pr.TrnsType}', VoucherType: '{pr.VoucherType}'");

                            // Now update the master record first
                            string updateResult = prRepo.UpdatePR(pr);
                            System.Diagnostics.Debug.WriteLine($"Updated PReturnMaster result: {updateResult}");

                            // Show success message to user
                            if (updateResult.Contains("success"))
                            {
                                MessageBox.Show($"Purchase Return #{prNo} updated successfully!",
                                    "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Update failed: {updateResult}", "Update Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"ERROR: Could not retrieve existing master record for ID {prMasterId}");
                            MessageBox.Show($"Could not retrieve existing Purchase Return record (ID: {prMasterId})",
                                "Data Retrieval Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating master record: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }

                        // Show detailed error to user
                        MessageBox.Show($"Error updating Purchase Return master record:\n\n{ex.Message}\n\nPlease check the debug output for more details.",
                            "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                List<UltraGridRow> selectedRows = new List<UltraGridRow>();
                List<string> invalidReasons = new List<string>();

                // Determine the actual quantity column name first
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (col.Key.Equals("Qty", StringComparison.OrdinalIgnoreCase) ||
                        col.Key.Equals("Quantity", StringComparison.OrdinalIgnoreCase))
                    {
                        _quantityColumnName = col.Key;
                        break;
                    }
                }

                // First collect all the selected rows that have valid data
                System.Diagnostics.Debug.WriteLine("Checking selected rows with valid data...");

                // To ensure all rows are checked correctly, force the grid to end edit mode
                // This ensures any in-progress edits get committed before we check selections
                if (ultraGrid1.ActiveCell != null)
                {
                    ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                }

                // Now refresh the grid to make sure all values are up to date
                ultraGrid1.Refresh();

                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (!row.IsDataRow || row.IsFilteredOut)
                        continue;

                    // Check if the SELECT column exists and is checked
                    if (row.Cells.Exists("SELECT") &&
                        row.Cells["SELECT"].Value != null &&
                        row.Cells["SELECT"].Value != DBNull.Value &&
                        Convert.ToBoolean(row.Cells["SELECT"].Value))
                    {
                        // Check if the Reason column exists and has a valid value
                        if (row.Cells.Exists("Reason"))
                        {
                            string reason = row.Cells["Reason"].Value?.ToString() ?? "";
                            if (string.IsNullOrWhiteSpace(reason) || reason == "Select Reason")
                            {
                                invalidReasons.Add(row.Cells["Description"].Value?.ToString() ?? "Unknown Item");
                                continue;
                            }
                        }
                        selectedRows.Add(row);
                    }
                }

                // If there are invalid reasons, show only one validation error message
                if (invalidReasons.Count > 0)
                {
                    string itemsList = string.Join("\n", invalidReasons);
                    MessageBox.Show($"Please select a valid reason for the following items:\n\n{itemsList}",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    throw new Exception("Missing reasons for selected items");
                }

                // If no rows are selected at all, silently return (no error message)
                if (selectedRows.Count == 0)
                {
                    throw new Exception("No items selected");
                }

                // Process each selected row
                int itemsProcessed = 0;
                bool isFirstRecord = true;

                foreach (UltraGridRow row in selectedRows)
                {
                    try
                    {
                        // Process the item
                        ProcessItem(pr, row, isFirstRecord);
                        itemsProcessed++;
                        isFirstRecord = false;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing row {row.Index}: {ex.Message}");
                        throw; // Don't show message box here, let the caller handle it
                    }
                }

                // If we processed any items, commit the final transaction
                if (itemsProcessed > 0)
                {
                    try
                    {
                        // Create a minimal details object for the final commit
                        ModelClass.TransactionModels.PReturnDetails finalDetails = new ModelClass.TransactionModels.PReturnDetails
                        {
                            CompanyId = pr.CompanyId,
                            FinYearId = pr.FinYearId,
                            BranchID = pr.BranchId,
                            PReturnNo = pr.PReturnNo,
                            PReturnDate = pr.PReturnDate,
                            InvoiceNo = pr.InvoiceNo,
                            ItemID = 0 // Set to 0 to indicate this is just for committing
                        };

                        // Explicitly commit with isFirstRecord=false
                        prRepo.UpdatePurchaseReturnDetails(pr, finalDetails, false);
                        System.Diagnostics.Debug.WriteLine($"Final commit completed successfully for {itemsProcessed} items");

                        MessageBox.Show($"Successfully updated {itemsProcessed} items for Purchase Return #{prNo}.",
                            "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception commitEx)
                    {
                        if (commitEx.Message.Contains("transaction has completed"))
                        {
                            // This is expected if the transaction was already committed
                            System.Diagnostics.Debug.WriteLine("Transaction already completed, items were saved successfully");
                            MessageBox.Show($"Successfully updated {itemsProcessed} items for Purchase Return #{prNo}.",
                                "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            throw; // Re-throw any other exceptions
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // For validation errors, we've already shown the message box
                // Only show error messages for non-validation errors
                if (!ex.Message.Contains("Missing reasons") && !ex.Message.Contains("No items"))
                {
                    MessageBox.Show("Error updating purchase return details: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Add diagnostic information
                System.Diagnostics.Debug.WriteLine("Exception details: " + ex.ToString());
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner exception: " + ex.InnerException.Message);
                }

                // Re-throw the exception to be caught by the caller
                throw;
            }
        }

        // New helper method to process each item in a consistent way
        private void ProcessItem(ModelClass.TransactionModels.PReturnMaster pr, UltraGridRow row, bool isFirstItem)
        {
            // Get ItemID
            long itemId = Convert.ToInt64(row.Cells["ItemID"].Value);

            // Get quantity
            double quantity = Convert.ToDouble(row.Cells[_quantityColumnName].Value);

            // Create details object
            ModelClass.TransactionModels.PReturnDetails details = new ModelClass.TransactionModels.PReturnDetails();
            details.CompanyId = pr.CompanyId;
            details.FinYearId = pr.FinYearId;
            details.BranchID = pr.BranchId;
            details.PReturnNo = pr.PReturnNo;
            details.PReturnDate = pr.PReturnDate;
            details.InvoiceNo = pr.InvoiceNo;
            details.SlNo = row.Index + 1;
            details.ItemID = itemId;

            details.Description = row.Cells.Exists("Description") && row.Cells["Description"].Value != null && row.Cells["Description"].Value != DBNull.Value
                ? row.Cells["Description"].Value.ToString()
                : "";

            details.UnitId = row.Cells.Exists("UnitId") && row.Cells["UnitId"].Value != null && row.Cells["UnitId"].Value != DBNull.Value
                ? Convert.ToInt32(row.Cells["UnitId"].Value)
                : 1;

            // Set BaseUnit as boolean value
            if (row.Cells.Exists("BaseUnit") && row.Cells["BaseUnit"].Value != null && row.Cells["BaseUnit"].Value != DBNull.Value)
            {
                if (row.Cells["BaseUnit"].Value is string)
                {
                    details.BaseUnit = row.Cells["BaseUnit"].Value.ToString().ToUpper() == "Y";
                }
                else
                {
                    details.BaseUnit = Convert.ToBoolean(row.Cells["BaseUnit"].Value);
                }
            }
            else
            {
                details.BaseUnit = true; // Default to true if not specified
            }

            // Get other values from the grid - using case-insensitive existence checks
            details.Packing = row.Cells.Exists("Packing") && row.Cells["Packing"].Value != null && row.Cells["Packing"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["Packing"].Value)
                : 1.0;

            details.IsExpiry = row.Cells.Exists("IsExpiry") && row.Cells["IsExpiry"].Value != null && row.Cells["IsExpiry"].Value != DBNull.Value
                ? Convert.ToBoolean(row.Cells["IsExpiry"].Value)
                : false;

            details.BatchNo = row.Cells.Exists("BatchNo") && row.Cells["BatchNo"].Value != null && row.Cells["BatchNo"].Value != DBNull.Value
                ? row.Cells["BatchNo"].Value.ToString()
                : "";

            if (row.Cells.Exists("Expiry") && row.Cells["Expiry"].Value != null && row.Cells["Expiry"].Value != DBNull.Value)
            {
                details.Expiry = Convert.ToDateTime(row.Cells["Expiry"].Value);
            }
            else
            {
                details.Expiry = null;
            }

            // Use the quantity we already verified above
            details.Qty = quantity;

            details.TaxPer = row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null && row.Cells["TaxPer"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["TaxPer"].Value)
                : 0;

            details.TaxAmt = row.Cells.Exists("TaxAmt") && row.Cells["TaxAmt"].Value != null && row.Cells["TaxAmt"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["TaxAmt"].Value)
                : 0;

            // Get the current reason value directly from the cell at processing time
            // This ensures we capture any recent changes
            string reasonValue = "Return"; // Default fallback
            if (row.Cells.Exists("Reason"))
            {
                // Make sure we're getting the most current value
                if (row.Cells["Reason"].Value != null && row.Cells["Reason"].Value != DBNull.Value)
                {
                    reasonValue = row.Cells["Reason"].Value.ToString().Trim();

                    // Replace default placeholder with generic reason, but keep any user-entered reason
                    if (string.IsNullOrWhiteSpace(reasonValue) || reasonValue == "Select Reason")
                    {
                        reasonValue = "Return";
                        System.Diagnostics.Debug.WriteLine($"Row {row.Index}: Using default reason 'Return' for item {details.ItemID}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Row {row.Index}: Using user-specified reason '{reasonValue}' for item {details.ItemID}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Row {row.Index}: Reason cell value is null, using default 'Return' for item {details.ItemID}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Row {row.Index}: Reason cell not found, using default 'Return' for item {details.ItemID}");
            }

            details.Reason = reasonValue;

            details.Free = row.Cells.Exists("Free") && row.Cells["Free"].Value != null && row.Cells["Free"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["Free"].Value)
                : 0;

            details.Cost = row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null && row.Cells["Cost"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["Cost"].Value)
                : 0;

            details.DisPer = row.Cells.Exists("DisPer") && row.Cells["DisPer"].Value != null && row.Cells["DisPer"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["DisPer"].Value)
                : 0;

            details.DisAmt = row.Cells.Exists("DisAmt") && row.Cells["DisAmt"].Value != null && row.Cells["DisAmt"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["DisAmt"].Value)
                : 0;

            details.SalesPrice = row.Cells.Exists("SalesPrice") && row.Cells["SalesPrice"].Value != null && row.Cells["SalesPrice"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["SalesPrice"].Value)
                : details.Cost; // Default to Cost if not available

            details.OriginalCost = row.Cells.Exists("OriginalCost") && row.Cells["OriginalCost"].Value != null && row.Cells["OriginalCost"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["OriginalCost"].Value)
                : details.Cost; // Default to Cost if not available

            details.TotalSP = row.Cells.Exists("TotalSP") && row.Cells["TotalSP"].Value != null && row.Cells["TotalSP"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["TotalSP"].Value)
                : details.Cost * details.Qty; // Calculate if not available

            details.CessAmt = row.Cells.Exists("CessAmt") && row.Cells["CessAmt"].Value != null && row.Cells["CessAmt"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["CessAmt"].Value)
                : 0;

            details.CessPer = row.Cells.Exists("CessPer") && row.Cells["CessPer"].Value != null && row.Cells["CessPer"].Value != DBNull.Value
                ? Convert.ToDouble(row.Cells["CessPer"].Value)
                : 0;

            // Get Returned qty from grid - this is what the user entered (new additional return quantity)
            double newReturnedQty = 0;
            if (row.Cells.Exists("Returned qty") && row.Cells["Returned qty"].Value != null && row.Cells["Returned qty"].Value != DBNull.Value)
            {
                double.TryParse(row.Cells["Returned qty"].Value.ToString(), out newReturnedQty);
            }
            else if (row.Cells.Exists("Returnqty") && row.Cells["Returnqty"].Value != null && row.Cells["Returnqty"].Value != DBNull.Value)
            {
                double.TryParse(row.Cells["Returnqty"].Value.ToString(), out newReturnedQty);
            }

            // When editing an existing Purchase Return loaded from PurcahseReturnUpdate,
            // the grid shows the already-returned quantity in the read-only "Returned" column.
            // In that case we must ADD the newReturnedQty to the existing Returned value so that:
            //   new total Returned = old Returned + newReturnedQty
            // For fresh returns (loaded from purchase list, not from PurcahseReturnUpdate),
            // the "Returned" column represents previously returned quantity from purchase history
            // and PReturnDetails.Returned should only store the NEW quantity for this PR.
            double previousReturnedQty = 0;
            if (row.Cells.Exists("Returned") && row.Cells["Returned"].Value != null && row.Cells["Returned"].Value != DBNull.Value)
            {
                double.TryParse(row.Cells["Returned"].Value.ToString(), out previousReturnedQty);
            }

            bool isLoadedFromPurchaseReturnUpdate = this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate";

            if (isLoadedFromPurchaseReturnUpdate)
            {
                // For updates via PurcahseReturnUpdate:
                //   details.Returned should be cumulative (old + new)
                details.Returned = previousReturnedQty + newReturnedQty;
            }
            else
            {
                // For new purchase returns:
                //   details.Returned is only the new quantity being returned in this PR
                details.Returned = newReturnedQty;
            }
            details.Returnqty = 0;

            System.Diagnostics.Debug.WriteLine($"ItemID: {details.ItemID}, Previous Returned: {previousReturnedQty}, New Returned Qty: {newReturnedQty}, Saved Returned (for PReturnDetails): {details.Returned}");

            // Get Amount from grid cell directly - save what user entered, don't recalculate
            // This preserves the exact amount that was displayed and saved
            string amountColumnName = null;
            foreach (string colName in new[] { "Amount", "TotalAmount", "NetAmount" })
            {
                if (row.Cells.Exists(colName))
                {
                    amountColumnName = colName;
                    break;
                }
            }

            if (amountColumnName != null && row.Cells[amountColumnName].Value != null && row.Cells[amountColumnName].Value != DBNull.Value)
            {
                // Save the Amount from grid cell as-is (what user sees and entered)
                details.TotalAmount = Convert.ToDouble(row.Cells[amountColumnName].Value);
            }
            else
            {
                // Fallback: calculate from new returned qty * Cost if Amount not available
                if (newReturnedQty > 0)
                {
                    details.TotalAmount = details.Cost * newReturnedQty;
                }
                else
                {
                    details.TotalAmount = details.Cost * details.Qty;
                }
            }

            // Save TotalSP as the recalculated value (new returned qty * Cost)
            // This is used for calculations and reporting
            if (newReturnedQty > 0)
            {
                details.TotalSP = details.Cost * newReturnedQty;
            }
            else
            {
                // If no return qty, use the same calculation as TotalAmount
                details.TotalSP = details.TotalAmount;
            }

            // Recalculate TaxAmt based on returned qty * cost
            // Get tax percentage and type from the row
            double taxPercentage = details.TaxPer;
            string taxType = "excl";
            if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null && row.Cells["TaxType"].Value != DBNull.Value)
            {
                string taxTypeValue = row.Cells["TaxType"].Value.ToString().Trim();
                // Normalize tax type
                if (taxTypeValue.Equals("Incl", StringComparison.OrdinalIgnoreCase) ||
                    taxTypeValue.Equals("I", StringComparison.OrdinalIgnoreCase) ||
                    taxTypeValue.Equals("incl", StringComparison.OrdinalIgnoreCase))
                {
                    taxType = "incl";
                }
                else
                {
                    taxType = "excl";
                }
            }

            // Calculate base amount for tax
            // If returned qty is 0, use original qty * cost; otherwise use returned qty * cost
            double taxBaseAmount;
            if (newReturnedQty > 0)
            {
                // Use returned qty * cost when there's a return
                taxBaseAmount = newReturnedQty * details.Cost;
            }
            else
            {
                // When returned qty is 0, use original qty * cost for tax calculation
                taxBaseAmount = details.Qty * details.Cost;
            }

            // Recalculate tax amount based on the appropriate base amount
            details.TaxAmt = CalculateTaxAmount(taxBaseAmount, taxPercentage, taxType);

            System.Diagnostics.Debug.WriteLine($"Tax calculation - Returned Qty: {newReturnedQty}, Original Qty: {details.Qty}, Base Amount: {taxBaseAmount}, Tax %: {taxPercentage}, Tax Type: {taxType}, Tax Amount: {details.TaxAmt}");

            // Log the item we're processing with extra detail for debugging
            System.Diagnostics.Debug.WriteLine($"Processing item #{details.SlNo} - ItemID: {details.ItemID}, Description: {details.Description}, Reason: '{details.Reason}', Returnqty: {details.Returnqty}, Returned: {details.Returned}, isFirstItem: {isFirstItem}");

            // Call repository to save this detail record
            prRepo.UpdatePurchaseReturnDetails(pr, details, isFirstItem);
        }

        // Adding back the missing method as an alias to UpdatePurchaseReturnDetails
        private void SavePurchaseReturnDetails(int prNo)
        {
            // Call the existing method that has the implementation 
            UpdatePurchaseReturnDetails(prNo);
        }

        // Add the UpdatePurchaseReturn method after the SavePurchaseReturn method
        private void UpdatePurchaseReturn()
        {
            try
            {
                // First make sure any active cell's value is committed
                if (ultraGrid1.ActiveCell != null)
                {
                    try
                    {
                        ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                        ultraGrid1.PerformAction(UltraGridAction.CommitRow);
                        ultraGrid1.UpdateData();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error committing edits: {ex.Message}");
                    }
                }

                // Check if we have a valid PR number to update
                if (string.IsNullOrEmpty(TxtSRNO.Text))
                {
                    MessageBox.Show("No Purchase Return number found to update.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int prNo;
                // Try to parse the PR number, handling various formats
                string prNoText = TxtSRNO.Text.Trim();

                // Remove any non-numeric characters (like "PR-", spaces, etc.)
                string cleanedPRNo = System.Text.RegularExpressions.Regex.Replace(prNoText, @"[^\d]", "");

                if (!int.TryParse(cleanedPRNo, out prNo))
                {
                    // If still can't parse, try removing leading zeros
                    cleanedPRNo = cleanedPRNo.TrimStart('0');
                    if (string.IsNullOrEmpty(cleanedPRNo))
                    {
                        cleanedPRNo = "0";
                    }
                    if (!int.TryParse(cleanedPRNo, out prNo))
                    {
                        MessageBox.Show("Invalid Purchase Return number format.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Check if any items are selected for update
                bool hasSelectedItems = false;
                List<string> invalidItems = new List<string>();

                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.IsDataRow && !row.IsFilteredOut && row.Cells.Count > 0)
                    {
                        // Check if this row is selected via the SELECT column
                        bool isSelected = false;
                        if (row.Cells.Exists("SELECT"))
                        {
                            var selectVal = row.Cells["SELECT"].Value;
                            isSelected = selectVal != null && selectVal != DBNull.Value && Convert.ToBoolean(selectVal);
                        }

                        if (isSelected)
                        {
                            hasSelectedItems = true;
                            break; // Found at least one selected item, that's enough
                        }
                    }
                }

                if (!hasSelectedItems)
                {
                    MessageBox.Show("Please select at least one item to update by checking the SELECT box.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Set cursor to wait
                Cursor.Current = Cursors.WaitCursor;

                try
                {
                    // Get existing purchase return data directly from the repository
                    // First try to get the master ID
                    int prMasterId = prRepo.GetIdByPReturnNo(prNo);

                    // If not found, try to get the master record directly to verify it exists
                    ModelClass.TransactionModels.PReturnMaster existingPR = null;

                    if (prMasterId > 0)
                    {
                        existingPR = prRepo.GetById(prMasterId);
                    }

                    // If still not found, try to get it using GetPurchaseReturnMasterData which uses a different approach
                    if (existingPR == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"PR Master not found via GetIdByPReturnNo, trying GetPurchaseReturnMasterData for PR#{prNo}");
                        existingPR = GetPurchaseReturnMasterData(prNo);
                    }

                    if (existingPR == null)
                    {
                        MessageBox.Show($"Purchase Return #{prNo} not found in the database. Please verify the PR number is correct.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    // Update prMasterId from the found record if it wasn't found earlier
                    if (prMasterId <= 0 && existingPR.Id > 0)
                    {
                        prMasterId = existingPR.Id;
                        System.Diagnostics.Debug.WriteLine($"Using PR Master ID from existingPR: {prMasterId}");
                    }

                    // Final check - if we still don't have a valid ID, we can't proceed
                    if (prMasterId <= 0)
                    {
                        MessageBox.Show($"Purchase Return #{prNo} found but could not retrieve valid ID. Please verify the PR number and try again.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    // Create a temporary DataGridView to store only selected rows
                    DataGridView tempDgv = new DataGridView();
                    tempDgv.Columns.Add("SlNo", "SlNo");
                    tempDgv.Columns.Add("ItemId", "ItemId");
                    tempDgv.Columns.Add("ItemName", "ItemName");
                    tempDgv.Columns.Add("BarCode", "BarCode");
                    tempDgv.Columns.Add("Unit", "Unit");
                    tempDgv.Columns.Add("UnitId", "UnitId");
                    tempDgv.Columns.Add("UnitPrice", "UnitPrice");
                    tempDgv.Columns.Add("Qty", "Qty");
                    tempDgv.Columns.Add("Packing", "Packing");
                    tempDgv.Columns.Add("Cost", "Cost");
                    tempDgv.Columns.Add("Amount", "Amount");
                    tempDgv.Columns.Add("Reason", "Reason");
                    tempDgv.Columns.Add("TaxPer", "TaxPer");
                    tempDgv.Columns.Add("TaxAmt", "TaxAmt");
                    tempDgv.Columns.Add("TaxType", "TaxType");
                    tempDgv.Columns.Add("Select", "Select");

                    // Copy only selected rows to the temporary grid
                    int slNo = 1;
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("SELECT") && row.Cells["SELECT"].Value != null && Convert.ToBoolean(row.Cells["SELECT"].Value))
                        {
                            // Double-check for NULL values in critical fields
                            if (row.Cells["Unit"].Value == null || row.Cells["Unit"].Value == DBNull.Value ||
                                string.IsNullOrWhiteSpace(row.Cells["Unit"].Value?.ToString()))
                            {
                                string itemName = row.Cells["Description"].Value?.ToString() ?? "Unknown item";
                                MessageBox.Show($"Missing Unit value for {itemName}. Please set a valid unit before updating.",
                                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Cursor.Current = Cursors.Default;
                                return;
                            }

                            // Get the UnitId for this unit
                            string unitName = row.Cells["Unit"].Value.ToString().Trim();
                            int unitId = 0;
                            // Try to get UnitId from the row if available
                            if (row.Cells.Exists("UnitId") && row.Cells["UnitId"].Value != null && row.Cells["UnitId"].Value != DBNull.Value)
                            {
                                unitId = Convert.ToInt32(row.Cells["UnitId"].Value);
                            }
                            else
                            {
                                // Fallback: use default unit ID
                                unitId = 1;
                            }

                            int index = tempDgv.Rows.Add();
                            tempDgv.Rows[index].Cells["SlNo"].Value = slNo++;
                            tempDgv.Rows[index].Cells["ItemId"].Value = row.Cells["ItemID"].Value;
                            tempDgv.Rows[index].Cells["ItemName"].Value = row.Cells["Description"].Value;
                            tempDgv.Rows[index].Cells["BarCode"].Value = row.Cells.Exists("Barcode") ? (row.Cells["Barcode"].Value ?? "") : "";
                            tempDgv.Rows[index].Cells["Unit"].Value = unitName;
                            tempDgv.Rows[index].Cells["UnitId"].Value = unitId;

                            // Get decimal values with validation
                            decimal unitPrice = 0;
                            if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null && row.Cells["Cost"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["Cost"].Value.ToString(), out unitPrice);
                            }
                            tempDgv.Rows[index].Cells["UnitPrice"].Value = unitPrice;

                            decimal qty = 0;
                            string qtyColName = row.Cells.Exists("Qty") ? "Qty" : (row.Cells.Exists("Quantity") ? "Quantity" : null);
                            if (qtyColName != null && row.Cells[qtyColName].Value != null && row.Cells[qtyColName].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells[qtyColName].Value.ToString(), out qty);
                            }
                            tempDgv.Rows[index].Cells["Qty"].Value = qty;

                            decimal amount = 0;
                            if (row.Cells.Exists("Amount") && row.Cells["Amount"].Value != null && row.Cells["Amount"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["Amount"].Value.ToString(), out amount);
                            }
                            else
                            {
                                // Calculate amount if not provided
                                amount = unitPrice * qty;
                            }
                            tempDgv.Rows[index].Cells["Amount"].Value = amount;

                            // Safe access for Packing column
                            decimal packing = 1;
                            if (row.Cells.Exists("Packing") && row.Cells["Packing"].Value != null && row.Cells["Packing"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["Packing"].Value.ToString(), out packing);
                            }
                            tempDgv.Rows[index].Cells["Packing"].Value = packing;

                            // Safe access for Cost column
                            decimal cost = 0;
                            if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null && row.Cells["Cost"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                            }
                            tempDgv.Rows[index].Cells["Cost"].Value = cost;

                            tempDgv.Rows[index].Cells["Reason"].Value = row.Cells.Exists("Reason") ? (row.Cells["Reason"].Value?.ToString().Trim() ?? "Return") : "Return";
                            tempDgv.Rows[index].Cells["Select"].Value = true;

                            // Tax fields
                            decimal taxPer = 0;
                            if (row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null && row.Cells["TaxPer"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["TaxPer"].Value.ToString(), out taxPer);
                            }
                            tempDgv.Rows[index].Cells["TaxPer"].Value = taxPer;

                            decimal taxAmt = 0;
                            if (row.Cells.Exists("TaxAmt") && row.Cells["TaxAmt"].Value != null && row.Cells["TaxAmt"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["TaxAmt"].Value.ToString(), out taxAmt);
                            }
                            tempDgv.Rows[index].Cells["TaxAmt"].Value = taxAmt;

                            string taxType = "excl";
                            if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null && row.Cells["TaxType"].Value != DBNull.Value)
                            {
                                taxType = row.Cells["TaxType"].Value.ToString();
                            }
                            tempDgv.Rows[index].Cells["TaxType"].Value = taxType;
                        }
                    }

                    // Set up the PReturnMaster object
                    ModelClass.TransactionModels.PReturnMaster pr = new ModelClass.TransactionModels.PReturnMaster();

                    // Safe conversion for DataBase fields
                    pr.BranchId = DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 0;
                    pr.CompanyId = DataBase.CompanyId != null ? Convert.ToInt32(DataBase.CompanyId) : 0;
                    pr.FinYearId = existingPR.FinYearId > 0 ? existingPR.FinYearId : (DataBase.FinyearId != null ? Convert.ToInt32(DataBase.FinyearId) : 1);

                    // Set the ID of the existing record for update
                    pr.PReturnNo = prNo;
                    pr.Id = prMasterId;
                    pr.VoucherID = existingPR.VoucherID;

                    // Set vendor information
                    pr.VendorName = VendorName.Text ?? "";
                    pr.LedgerID = !string.IsNullOrEmpty(vendorid.Text) ? Convert.ToInt32(vendorid.Text) : existingPR.LedgerID;

                    ApplyHiddenPaymentDefaults(pr, existingPR);

                    // Keep the purchase reference aligned with the visible purchase number.
                    pr.InvoiceNo = !string.IsNullOrWhiteSpace(textBox1.Text)
                        ? GetPurchaseReferenceForSave()
                        : (existingPR.InvoiceNo ?? string.Empty);

                    // Get invoice date from form control (ultraDateTimeEditor2) if available, otherwise use existing
                    DateTime minSqlDate = new DateTime(1753, 1, 1);
                    DateTime maxSqlDate = new DateTime(9999, 12, 31);

                    if (ultraDateTimeEditor2 != null && ultraDateTimeEditor2.Value != null)
                    {
                        DateTime invoiceDate = (DateTime)ultraDateTimeEditor2.Value;
                        // Ensure date is within SQL Server's valid range
                        if (invoiceDate < minSqlDate)
                            invoiceDate = minSqlDate;
                        else if (invoiceDate > maxSqlDate)
                            invoiceDate = maxSqlDate;
                        pr.InvoiceDate = invoiceDate;
                        System.Diagnostics.Debug.WriteLine($"Using invoice date from form: {invoiceDate:yyyy-MM-dd}");
                    }
                    else
                    {
                        // Fallback to existing invoice date if form control is not set
                        pr.InvoiceDate = existingPR.InvoiceDate;
                        System.Diagnostics.Debug.WriteLine($"Using existing invoice date: {existingPR.InvoiceDate:yyyy-MM-dd}");
                    }
                    pr.SpDisPer = existingPR.SpDisPer;
                    pr.SpDsiAmt = existingPR.SpDsiAmt;
                    pr.BillDiscountPer = existingPR.BillDiscountPer;
                    pr.BillDiscountAmt = existingPR.BillDiscountAmt;
                    pr.TaxPer = existingPR.TaxPer;
                    pr.TaxAmt = existingPR.TaxAmt;
                    pr.Frieght = existingPR.Frieght;

                    // Safe conversion for GrandTotal
                    decimal grandTotal = 0;
                    if (!string.IsNullOrEmpty(lblNetAmount.Text) && decimal.TryParse(lblNetAmount.Text, out grandTotal))
                    {
                        pr.GrandTotal = (double)grandTotal;
                    }
                    else
                    {
                        pr.GrandTotal = existingPR.GrandTotal;
                    }

                    // Safe conversion for SubTotal
                    decimal subTotal = 0;
                    if (!string.IsNullOrEmpty(TxtSubTotal.Text) && decimal.TryParse(TxtSubTotal.Text, out subTotal))
                    {
                        pr.SubTotal = (double)subTotal;
                    }
                    else
                    {
                        pr.SubTotal = existingPR.SubTotal;
                    }

                    // Safe conversion for UserID
                    int userId = 0;
                    if (DataBase.UserId != null && int.TryParse(DataBase.UserId.ToString(), out userId))
                    {
                        pr.UserID = userId;
                    }
                    else
                    {
                        pr.UserID = existingPR.UserID;
                    }

                    pr.CessAmt = existingPR.CessAmt;
                    pr.CessPer = existingPR.CessPer;
                    pr.BranchName = DataBase.Branch;
                    pr.CalAfterTax = existingPR.CalAfterTax;
                    pr.CurSymbol = existingPR.CurSymbol ?? "";
                    pr.PReturnDate = ultraDateTimeEditor1.Value != null ? (DateTime)ultraDateTimeEditor1.Value : existingPR.PReturnDate;
                    pr.SeriesID = existingPR.SeriesID > 0 ? existingPR.SeriesID : 1;
                    pr.TrnsType = existingPR.TrnsType ?? "PR";
                    pr.VoucherType = existingPR.VoucherType ?? "";
                    pr.TaxType = existingPR.TaxType ?? "I";
                    pr.Remarks = existingPR.Remarks ?? "";
                    pr.CancelFlag = existingPR.CancelFlag;
                    pr.RoundOff = existingPR.RoundOff;
                    pr._Operation = "UPDATE"; // Explicitly set the operation to UPDATE

                    // Track changes made for the success message
                    List<string> changesMade = new List<string>();

                    // Track vendor change
                    if (existingPR.LedgerID != pr.LedgerID || existingPR.VendorName != pr.VendorName)
                    {
                        changesMade.Add($"Vendor: {existingPR.VendorName ?? "N/A"} → {pr.VendorName}");
                    }

                    // Track total changes
                    if (Math.Abs(existingPR.GrandTotal - pr.GrandTotal) > 0.01)
                    {
                        changesMade.Add($"Grand Total: {existingPR.GrandTotal:F2} → {pr.GrandTotal:F2}");
                    }

                    if (Math.Abs(existingPR.SubTotal - pr.SubTotal) > 0.01)
                    {
                        changesMade.Add($"Sub Total: {existingPR.SubTotal:F2} → {pr.SubTotal:F2}");
                    }

                    // Track date changes
                    if (existingPR.PReturnDate.Date != pr.PReturnDate.Date)
                    {
                        changesMade.Add($"Return Date: {existingPR.PReturnDate:yyyy-MM-dd} → {pr.PReturnDate:yyyy-MM-dd}");
                    }

                    // Count selected items
                    int selectedItemsCount = tempDgv.Rows.Count;
                    if (selectedItemsCount > 0)
                    {
                        changesMade.Add($"Items Updated: {selectedItemsCount} item(s)");
                    }

                    // Call the repository to update the purchase return master record
                    string message = prRepo.UpdatePR(pr);

                    // Build detailed success message
                    if (message.Contains("success"))
                    {
                        // After successfully updating the master, update the details
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"Updating purchase return details for PR#{prNo}");
                            UpdatePurchaseReturnDetails(prNo);
                            System.Diagnostics.Debug.WriteLine($"Successfully updated purchase return details for PR#{prNo}");
                            SavePurchaseReturnActivityLog("UPDATE", pr);
                        }
                        catch (Exception detailsEx)
                        {
                            // Log the error but don't fail the entire update
                            System.Diagnostics.Debug.WriteLine($"Error updating purchase return details: {detailsEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack trace: {detailsEx.StackTrace}");

                            // Show a warning that master was updated but details had issues
                            MessageBox.Show($"Purchase Return master record was updated successfully, but there was an error updating details: {detailsEx.Message}",
                                "Partial Update Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        StringBuilder successMessage = new StringBuilder();
                        successMessage.AppendLine("Update Successful!");
                        successMessage.AppendLine($"Purchase Return #{prNo} has been updated.");
                        successMessage.AppendLine();

                        if (changesMade.Count > 0)
                        {
                            successMessage.AppendLine("Changes Made:");
                            foreach (string change in changesMade)
                            {
                                successMessage.AppendLine($"  • {change}");
                            }
                        }
                        else
                        {
                            successMessage.AppendLine("No changes detected.");
                        }

                        MessageBox.Show(successMessage.ToString(), "Update Successful",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Reset form for next entry
                        ClearForm();
                    }
                    else
                    {
                        // Show error message if update failed
                        MessageBox.Show(message, "Update Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating purchase return: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Reset cursor
                    Cursor.Current = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating purchase return: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }


        private void pbxExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Method to refresh any open PurchaseReturnUpdate forms to remove deleted PR
        private void RefreshPurchaseReturnUpdateForms(int deletedPrNo)
        {
            try
            {
                // Look for any open PurchaseReturnUpdate forms
                foreach (Form form in Application.OpenForms)
                {
                    if (form is DialogBox.PurcahseReturnUpdate purchaseReturnUpdateForm)
                    {
                        // Tell the form to reload its data (which will exclude the deleted PR)
                        if (purchaseReturnUpdateForm.IsHandleCreated && !purchaseReturnUpdateForm.IsDisposed)
                        {
                            // We'll use BeginInvoke to safely access the form's UI thread
                            purchaseReturnUpdateForm.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    // Use the LoadPurchaseReturnData which is now public
                                    purchaseReturnUpdateForm.LoadPurchaseReturnData();
                                    System.Diagnostics.Debug.WriteLine($"Refreshed data in PurchaseReturnUpdate form to remove PR #{deletedPrNo}");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error refreshing form data: {ex.Message}");
                                }
                            }));
                        }

                        System.Diagnostics.Debug.WriteLine($"Attempted to refresh PurchaseReturnUpdate form to remove PR #{deletedPrNo}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing PurchaseReturnUpdate forms: {ex.Message}");
                // Don't show error to user as this is a secondary operation
            }
        }

        private string GenerateDebitNote(int prNo, PReturnMaster prMaster)
        {
            try
            {
                // Create the debit note
                string debitNoteFilePath = Path.Combine(Application.StartupPath, "Reports", "DebitNotes");

                // Create directory if it doesn't exist
                if (!Directory.Exists(debitNoteFilePath))
                {
                    Directory.CreateDirectory(debitNoteFilePath);
                }

                string fileName = $"DebitNote_PR{prNo}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string fullPath = Path.Combine(debitNoteFilePath, fileName);

                using (StreamWriter writer = new StreamWriter(fullPath))
                {
                    // Header
                    writer.WriteLine("====================================================");
                    writer.WriteLine("                    DEBIT NOTE                      ");
                    writer.WriteLine("====================================================");
                    writer.WriteLine();

                    // Company details
                    writer.WriteLine($"Company ID: {prMaster.CompanyId}");
                    writer.WriteLine($"Branch: {prMaster.BranchName} (ID: {prMaster.BranchId})");
                    writer.WriteLine($"Date: {prMaster.PReturnDate:dd-MMM-yyyy}");
                    writer.WriteLine();

                    // Vendor details
                    writer.WriteLine($"Vendor Name: {prMaster.VendorName}");
                    writer.WriteLine($"Vendor ID: {prMaster.LedgerID}");
                    writer.WriteLine();

                    // Debit note details
                    writer.WriteLine($"Debit Note No: DN-{prNo}");
                    writer.WriteLine($"Reference PR No: {prNo}");
                    writer.WriteLine($"Original Invoice No: {prMaster.InvoiceNo}");
                    writer.WriteLine($"Original Invoice Date: {prMaster.InvoiceDate:dd-MMM-yyyy}");
                    writer.WriteLine($"Payment Mode: {prMaster.Paymode}");
                    writer.WriteLine();

                    // Financial details
                    writer.WriteLine("====================================================");
                    writer.WriteLine("FINANCIAL DETAILS:");
                    writer.WriteLine("====================================================");
                    writer.WriteLine($"Sub Total: {prMaster.SubTotal:N2}");
                    writer.WriteLine($"Tax Percentage: {prMaster.TaxPer:N2}%");
                    writer.WriteLine($"Tax Amount: {prMaster.TaxAmt:N2}");

                    if (prMaster.BillDiscountAmt > 0)
                    {
                        writer.WriteLine($"Discount: {prMaster.BillDiscountAmt:N2} ({prMaster.BillDiscountPer:N2}%)");
                    }

                    if (prMaster.RoundOff != 0)
                    {
                        writer.WriteLine($"Round Off: {prMaster.RoundOff:N2}");
                    }

                    writer.WriteLine($"Grand Total: {prMaster.GrandTotal:N2}");
                    writer.WriteLine();

                    // Add item details
                    writer.WriteLine("====================================================");
                    writer.WriteLine("ITEM DETAILS:");
                    writer.WriteLine("====================================================");
                    writer.WriteLine("ID\tDescription\tQty\tRate\tAmount\tReason");
                    writer.WriteLine("----------------------------------------------------");

                    // Get the details from grid
                    DataTable itemDetails = GetPRDetailsByPRNo(prNo);
                    if (itemDetails != null && itemDetails.Rows.Count > 0)
                    {
                        foreach (DataRow row in itemDetails.Rows)
                        {
                            string itemId = row["ItemID"].ToString();
                            string description = row["Description"].ToString();
                            string qty = row["Qty"].ToString();
                            string cost = row["Cost"].ToString();
                            string amount = row["Amount"].ToString();
                            string reason = row["Reason"].ToString();

                            writer.WriteLine($"{itemId}\t{description}\t{qty}\t{cost}\t{amount}\t{reason}");
                        }
                    }

                    writer.WriteLine("====================================================");
                    writer.WriteLine();

                    // Footer
                    writer.WriteLine($"Generated by: {prMaster.UserName} (ID: {prMaster.UserID})");
                    writer.WriteLine($"Generation Date: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}");
                    writer.WriteLine($"This is a computer generated debit note and requires no signature.");
                }

                System.Diagnostics.Debug.WriteLine($"Debit note generated successfully: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating debit note: {ex.Message}");
                // Don't show error to user as this is a secondary operation
                return null;
            }
        }

        private void ShowDebitNote(int prNo)
        {
            try
            {
                string debitNoteDir = Path.Combine(Application.StartupPath, "Reports", "DebitNotes");

                if (!Directory.Exists(debitNoteDir))
                {
                    return;
                }

                // Find the most recent debit note for this PR
                string[] files = Directory.GetFiles(debitNoteDir, $"DebitNote_PR{prNo}_*.txt");

                if (files.Length > 0)
                {
                    // Sort by creation time descending
                    Array.Sort(files, (a, b) => File.GetCreationTime(b).CompareTo(File.GetCreationTime(a)));

                    // Get the most recent file
                    string mostRecentFile = files[0];

                    // Create and show a form to display the debit note
                    Form debitNoteForm = new Form
                    {
                        Text = $"Debit Note - PR# {prNo}",
                        Width = 800,
                        Height = 600,
                        StartPosition = FormStartPosition.CenterScreen
                    };

                    TextBox textBox = new TextBox
                    {
                        Multiline = true,
                        ReadOnly = true,
                        Dock = DockStyle.Fill,
                        ScrollBars = ScrollBars.Vertical,
                        Font = new Font("Courier New", 10),
                        Text = File.ReadAllText(mostRecentFile)
                    };

                    Button printButton = new Button
                    {
                        Text = "Print",
                        Dock = DockStyle.Bottom
                    };

                    Button closeButton = new Button
                    {
                        Text = "Close",
                        Dock = DockStyle.Bottom
                    };

                    // Add event handlers
                    closeButton.Click += (sender, e) => debitNoteForm.Close();
                    printButton.Click += (sender, e) => PrintDebitNote(mostRecentFile);

                    // Add controls to form
                    debitNoteForm.Controls.Add(textBox);
                    debitNoteForm.Controls.Add(closeButton);
                    debitNoteForm.Controls.Add(printButton);

                    // Show the form
                    debitNoteForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing debit note: {ex.Message}");
            }
        }

        private void PrintDebitNote(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Create a PrintDocument object
                    System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();

                    // Read the file content
                    string fileContent = File.ReadAllText(filePath);

                    // Set up print handler
                    printDoc.PrintPage += (sender, e) =>
                    {
                        // Set font and calculate margins
                        Font printFont = new Font("Courier New", 10);
                        float leftMargin = e.MarginBounds.Left;
                        float topMargin = e.MarginBounds.Top;

                        // Print each line of the file
                        string[] lines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        float yPos = topMargin;
                        foreach (string line in lines)
                        {
                            e.Graphics.DrawString(line, printFont, Brushes.Black, leftMargin, yPos);
                            yPos += printFont.GetHeight(e.Graphics);

                            // Check if we need to continue on a new page
                            if (yPos + printFont.GetHeight(e.Graphics) > e.MarginBounds.Bottom)
                            {
                                e.HasMorePages = true;
                                break;
                            }
                        }
                    };

                    // Show print dialog
                    System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();
                    printDialog.Document = printDoc;

                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        printDoc.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing debit note: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Old DebitNote class removed - using new version with VendorLedgerId property below

        private DebitNote CreateDebitNoteObject(int prNo, PReturnMaster prMaster)
        {
            // Parse the subtotal from TxtSubTotal
            decimal subtotal = 0;
            if (!string.IsNullOrEmpty(TxtSubTotal.Text) && decimal.TryParse(TxtSubTotal.Text, out decimal parsedValue))
            {
                subtotal = parsedValue;
            }

            // Generate a debit note based on the purchase return
            DebitNote debitNote = new DebitNote
            {
                DebitNoteNo = prNo, // Use the same number as the purchase return for simplicity
                DebitNoteDate = DateTime.Now,
                VendorName = prMaster.VendorName,
                InvoiceNo = prMaster.InvoiceNo.ToString(),
                InvoiceDate = ultraDateTimeEditor2.Value != null ? (DateTime)ultraDateTimeEditor2.Value : DateTime.Now,
                TotalAmount = subtotal, // Use the value from TxtSubTotal
                PaymentMethod = prMaster.Paymode
            };

            return debitNote;
        }

        private void DisplayDebitNoteForm(DebitNote debitNote, int vendorLedgerId, string vendorName, double totalAmount = 0)
        {
            // Create a form to display the debit note
            Form debitNoteForm = new Form();
            debitNoteForm.Text = "Debit Note";
            debitNoteForm.Size = new Size(600, 600); // Increased width from 500 to 600
            debitNoteForm.StartPosition = FormStartPosition.CenterScreen;
            debitNoteForm.MaximizeBox = false;
            debitNoteForm.MinimizeBox = false;
            debitNoteForm.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Create a panel for the content
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.AutoScroll = true;

            // Add debit note details
            Label titleLabel = new Label();
            titleLabel.Text = "DEBIT NOTE";
            titleLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Size = new Size(560, 40); // Increased width
            titleLabel.Location = new Point(20, 20);

            Label debitNoteNoLabel = new Label();
            debitNoteNoLabel.Text = $"Debit Note #: {debitNote.DebitNoteNo}";
            debitNoteNoLabel.Font = new Font("Arial", 10);
            debitNoteNoLabel.Size = new Size(300, 20);
            debitNoteNoLabel.Location = new Point(20, 70);

            Label dateLabel = new Label();
            dateLabel.Text = $"Date: {debitNote.DebitNoteDate.ToString("dd/MM/yyyy")}";
            dateLabel.Font = new Font("Arial", 10);
            dateLabel.Size = new Size(300, 20);
            dateLabel.Location = new Point(20, 90);

            Label vendorLabel = new Label();
            vendorLabel.Text = $"Vendor: {debitNote.VendorName}";
            vendorLabel.Font = new Font("Arial", 10);
            vendorLabel.Size = new Size(300, 20);
            vendorLabel.Location = new Point(20, 120);

            Label invoiceLabel = new Label();
            invoiceLabel.Text = $"Original Invoice #: {debitNote.InvoiceNo}";
            invoiceLabel.Font = new Font("Arial", 10);
            invoiceLabel.Size = new Size(300, 20);
            invoiceLabel.Location = new Point(20, 140);

            Label invoiceDateLabel = new Label();
            invoiceDateLabel.Text = $"Invoice Date: {debitNote.InvoiceDate.ToString("dd/MM/yyyy")}";
            invoiceDateLabel.Font = new Font("Arial", 10);
            invoiceDateLabel.Size = new Size(300, 20);
            invoiceDateLabel.Location = new Point(20, 160);

            Label amountLabel = new Label();
            amountLabel.Text = $"Total Amount: {debitNote.TotalAmount.ToString("0.00")}";
            amountLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            amountLabel.Size = new Size(300, 25);
            amountLabel.Location = new Point(20, 190);

            Label paymentMethodLabel = new Label();
            paymentMethodLabel.Text = $"Payment Method: {debitNote.PaymentMethod}";
            paymentMethodLabel.Font = new Font("Arial", 10);
            paymentMethodLabel.Size = new Size(300, 20);
            paymentMethodLabel.Location = new Point(20, 220);

            // Create a panel for returned items
            Panel itemsPanel = new Panel();
            itemsPanel.Location = new Point(20, 250);
            itemsPanel.Size = new Size(540, 200); // Increased width from 440 to 540
            itemsPanel.AutoScroll = true;
            itemsPanel.BorderStyle = BorderStyle.FixedSingle;

            // Get items from UltraGrid and add to panel
            int yOffset = 10;

            // Add header
            Label itemHeaderLabel = new Label();
            itemHeaderLabel.Text = "Returned Items:";
            itemHeaderLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            itemHeaderLabel.Size = new Size(400, 20);
            itemHeaderLabel.Location = new Point(10, yOffset);
            itemsPanel.Controls.Add(itemHeaderLabel);
            yOffset += 30;

            // Add column headers
            Label itemNameHeader = new Label();
            itemNameHeader.Text = "Item";
            itemNameHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            itemNameHeader.Size = new Size(180, 20); // Slightly reduced to make room for reason
            itemNameHeader.Location = new Point(10, yOffset);
            itemsPanel.Controls.Add(itemNameHeader);

            Label qtyHeader = new Label();
            qtyHeader.Text = "Qty";
            qtyHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            qtyHeader.Size = new Size(40, 20);
            qtyHeader.Location = new Point(200, yOffset);
            itemsPanel.Controls.Add(qtyHeader);

            Label priceHeader = new Label();
            priceHeader.Text = "Price";
            priceHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            priceHeader.Size = new Size(60, 20);
            priceHeader.Location = new Point(250, yOffset);
            itemsPanel.Controls.Add(priceHeader);

            Label amountHeader = new Label();
            amountHeader.Text = "Amount";
            amountHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            amountHeader.Size = new Size(60, 20);
            amountHeader.Location = new Point(320, yOffset);
            itemsPanel.Controls.Add(amountHeader);

            Label reasonHeader = new Label();
            reasonHeader.Text = "Reason";
            reasonHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            reasonHeader.Size = new Size(150, 20);
            reasonHeader.Location = new Point(390, yOffset);
            itemsPanel.Controls.Add(reasonHeader);

            yOffset += 25;

            // Add returned items
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                {
                    Label itemNameLabel = new Label();
                    itemNameLabel.Text = row.Cells["Description"].Value?.ToString() ?? "";
                    itemNameLabel.Font = new Font("Arial", 9);
                    itemNameLabel.Size = new Size(180, 20); // Slightly reduced width
                    itemNameLabel.Location = new Point(10, yOffset);
                    itemsPanel.Controls.Add(itemNameLabel);

                    Label qtyLabel = new Label();
                    string returnQtyKey = row.Cells.Exists("Returned qty") ? "Returned qty" : _quantityColumnName;
                    qtyLabel.Text = row.Cells[returnQtyKey].Value?.ToString() ?? "";
                    qtyLabel.Font = new Font("Arial", 9);
                    qtyLabel.Size = new Size(40, 20);
                    qtyLabel.Location = new Point(200, yOffset);
                    itemsPanel.Controls.Add(qtyLabel);

                    Label priceLabel = new Label();
                    priceLabel.Text = row.Cells["Cost"].Value != null ?
                        Convert.ToDecimal(row.Cells["Cost"].Value).ToString("0.00") : "0.00";
                    priceLabel.Font = new Font("Arial", 9);
                    priceLabel.Size = new Size(60, 20);
                    priceLabel.Location = new Point(250, yOffset);
                    itemsPanel.Controls.Add(priceLabel);

                    Label itemAmountLabel = new Label();
                    itemAmountLabel.Text = row.Cells["Amount"].Value != null ?
                        Convert.ToDecimal(row.Cells["Amount"].Value).ToString("0.00") : "0.00";
                    itemAmountLabel.Font = new Font("Arial", 9);
                    itemAmountLabel.Size = new Size(60, 20);
                    itemAmountLabel.Location = new Point(320, yOffset);
                    itemsPanel.Controls.Add(itemAmountLabel);

                    Label reasonLabel = new Label();
                    reasonLabel.Text = row.Cells["Reason"].Value?.ToString() ?? "";
                    reasonLabel.Font = new Font("Arial", 9);
                    reasonLabel.Size = new Size(150, 20);
                    reasonLabel.Location = new Point(390, yOffset);
                    itemsPanel.Controls.Add(reasonLabel);

                    yOffset += 25;
                }
            }

            // Add note text
            Label noteLabel = new Label();
            noteLabel.Text = "This debit note is issued for the returned items. This is not a tax invoice.";
            noteLabel.Font = new Font("Arial", 9, FontStyle.Italic);
            noteLabel.Size = new Size(500, 40); // Increased width
            noteLabel.Location = new Point(20, 460);

            // Add print, view payments, and close buttons
            Button printButton = new Button();
            printButton.Text = "Print";
            printButton.Size = new Size(100, 30);
            printButton.Location = new Point(100, 510); // Adjusted position for wider form
            printButton.Click += (sender, e) => PrintDebitNoteForm(debitNote);

            Button viewPaymentsButton = new Button();
            viewPaymentsButton.Text = "View Payments";
            viewPaymentsButton.Size = new Size(120, 30);
            viewPaymentsButton.Location = new Point(220, 510);
            viewPaymentsButton.Click += (sender, e) =>
            {
                debitNoteForm.Close();
                OpenPaymentFormInTab(vendorLedgerId, vendorName, totalAmount);
            };

            Button closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Size = new Size(100, 30);
            closeButton.Location = new Point(360, 510); // Adjusted position for wider form
            closeButton.Click += (sender, e) => debitNoteForm.Close();

            // Add controls to the panel
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(debitNoteNoLabel);
            panel.Controls.Add(dateLabel);
            panel.Controls.Add(vendorLabel);
            panel.Controls.Add(invoiceLabel);
            panel.Controls.Add(invoiceDateLabel);
            panel.Controls.Add(amountLabel);
            panel.Controls.Add(paymentMethodLabel);
            panel.Controls.Add(itemsPanel);
            panel.Controls.Add(noteLabel);
            panel.Controls.Add(printButton);
            panel.Controls.Add(viewPaymentsButton);
            panel.Controls.Add(closeButton);

            // Add panel to the form
            debitNoteForm.Controls.Add(panel);

            // Show the form
            debitNoteForm.ShowDialog();
        }

        private void PrintDebitNoteForm(DebitNote debitNote)
        {
            try
            {
                // Create print dialog
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Printing debit note...", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Actual printing code would go here
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing debit note: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayDebitNoteObject(int prNo)
        {
            try
            {
                // Get purchase return master data
                PReturnMaster prMaster = GetPurchaseReturnMasterData(prNo);
                if (prMaster != null)
                {
                    // Generate and display debit note
                    DebitNote debitNote = CreateDebitNoteObject(prNo, prMaster);
                    DisplayDebitNoteForm(debitNote, (int)prMaster.LedgerID, prMaster.VendorName, prMaster.SubTotal);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing debit note: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPaymentFormInTab(int vendorLedgerId, string vendorName, double totalAmount = 0)
        {
            try
            {
                // Create FrmPayment form
                var paymentForm = new PosBranch_Win.Accounts.FrmPayment();

                // Find the Home form (parent form with tab control)
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    // Use reflection to call the OpenFormInTab method
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabMethod != null)
                    {
                        // Call the OpenFormInTab method
                        openFormInTabMethod.Invoke(homeForm, new object[] { paymentForm, $"Payment - {vendorName}" });

                        // Wait a moment for the form to be fully embedded, then set vendor info and payment amount
                        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                paymentForm.SetVendorInfo(vendorLedgerId, vendorName);
                                // Set the total payment amount from database (SubTotal)
                                if (totalAmount > 0)
                                {
                                    paymentForm.SetPaymentAmount(totalAmount);
                                }
                            }));
                        });
                        return;
                    }
                }

                // Fallback: show as regular form
                paymentForm.Show();
                paymentForm.BringToFront();
                paymentForm.SetVendorInfo(vendorLedgerId, vendorName);
                // Set the total payment amount from database (SubTotal)
                if (totalAmount > 0)
                {
                    paymentForm.SetPaymentAmount(totalAmount);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening payment form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddItemToGrid(string itemId, string itemName, string barcode, string unit, decimal unitPrice, decimal quantity, decimal amount)
        {
            try
            {
                if (ultraGrid1.DataSource == null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Sl No", typeof(int));
                    dt.Columns.Add("ItemID", typeof(long));
                    dt.Columns.Add("Description", typeof(string));
                    dt.Columns.Add("Barcode", typeof(string));
                    dt.Columns.Add("Unit", typeof(string));
                    dt.Columns.Add("Packing", typeof(double));
                    dt.Columns.Add("Cost", typeof(decimal));
                    dt.Columns.Add("Quantity", typeof(double));
                    dt.Columns.Add("Amount", typeof(decimal));
                    dt.Columns.Add("Returned", typeof(decimal));
                    dt.Columns.Add("Returned qty", typeof(double));
                    dt.Columns.Add("Reason", typeof(string));
                    dt.Columns.Add("SELECT", typeof(bool));
                    dt.Columns.Add("TaxPer", typeof(double));
                    dt.Columns.Add("TaxAmt", typeof(double));
                    dt.Columns.Add("TaxType", typeof(string));
                    ultraGrid1.DataSource = dt;
                }

                DataTable gridTable = ultraGrid1.DataSource as DataTable;
                if (gridTable != null)
                {
                    // Ensure Returned and Returned qty columns exist
                    if (!gridTable.Columns.Contains("Returned"))
                    {
                        gridTable.Columns.Add("Returned", typeof(decimal));
                    }
                    if (!gridTable.Columns.Contains("Returned qty"))
                    {
                        gridTable.Columns.Add("Returned qty", typeof(double));
                    }

                    DataRow newRow = gridTable.NewRow();
                    newRow["Sl No"] = gridTable.Rows.Count + 1;
                    newRow["ItemID"] = long.Parse(itemId);
                    newRow["Description"] = itemName;
                    newRow["Barcode"] = barcode;
                    newRow["Unit"] = unit;
                    newRow["Packing"] = 1.0;
                    newRow["Cost"] = unitPrice;
                    newRow["Quantity"] = quantity;
                    newRow["Amount"] = amount;
                    newRow["Returned qty"] = 0.00; // Set default value
                    newRow["Returned"] = 0.00m; // Set default value
                    newRow["Reason"] = "Select Reason"; // Set default value
                    newRow["SELECT"] = true;
                    gridTable.Rows.Add(newRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item to grid: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads an item from purchase history into the grid
        /// </summary>
        private void LoadItemFromPurchaseHistory(DialogBox.FrmItemPurchaseHistory purchaseHistoryForm)
        {
            try
            {
                if (ultraGrid1.DataSource == null)
                {
                    FormatGrid();
                }

                DataTable gridTable = ultraGrid1.DataSource as DataTable;
                if (gridTable == null)
                {
                    MessageBox.Show("Grid data source is not available.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if item already exists in grid
                bool itemExists = false;
                UltraGridRow existingRow = null;

                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("ItemID") && row.Cells["ItemID"].Value != null)
                    {
                        long rowItemId = Convert.ToInt64(row.Cells["ItemID"].Value);
                        if (rowItemId == purchaseHistoryForm.SelectedItemId)
                        {
                            itemExists = true;
                            existingRow = row;
                            break;
                        }
                    }
                }

                if (itemExists && existingRow != null)
                {
                    // Item exists, replace with new values
                    existingRow.Cells["Description"].Value = purchaseHistoryForm.SelectedItemName;
                    existingRow.Cells["Barcode"].Value = purchaseHistoryForm.SelectedBarcode;
                    existingRow.Cells["Unit"].Value = purchaseHistoryForm.SelectedUnit;
                    existingRow.Cells["UnitId"].Value = purchaseHistoryForm.SelectedUnitId;
                    existingRow.Cells["Packing"].Value = purchaseHistoryForm.SelectedPacking;
                    existingRow.Cells["Cost"].Value = purchaseHistoryForm.SelectedCost;
                    existingRow.Cells["Quantity"].Value = purchaseHistoryForm.SelectedQuantity;
                    existingRow.Cells["Amount"].Value = purchaseHistoryForm.SelectedAmount;
                    existingRow.Cells["TaxType"].Value = purchaseHistoryForm.SelectedTaxType;
                    existingRow.Cells["TaxPer"].Value = purchaseHistoryForm.SelectedTaxPer;
                    existingRow.Cells["TaxAmt"].Value = purchaseHistoryForm.SelectedTaxAmt;

                    // Update net amount
                    UpdateNetAmount();

                    // Select and focus the row
                    ultraGrid1.ActiveRow = existingRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(existingRow);
                    ultraGrid1.Focus();
                }
                else
                {
                    // Add new row
                    DataRow newRow = gridTable.NewRow();

                    // Calculate SL No
                    int slNo = gridTable.Rows.Count + 1;

                    newRow["Sl No"] = slNo;
                    newRow["ItemID"] = purchaseHistoryForm.SelectedItemId;
                    newRow["Description"] = purchaseHistoryForm.SelectedItemName;
                    newRow["Barcode"] = purchaseHistoryForm.SelectedBarcode;
                    newRow["Unit"] = purchaseHistoryForm.SelectedUnit;
                    newRow["UnitId"] = purchaseHistoryForm.SelectedUnitId;
                    newRow["Packing"] = purchaseHistoryForm.SelectedPacking;
                    newRow["Cost"] = purchaseHistoryForm.SelectedCost;
                    newRow["Quantity"] = purchaseHistoryForm.SelectedQuantity;
                    newRow["Amount"] = purchaseHistoryForm.SelectedAmount;
                    newRow["Returned"] = 0;
                    newRow["Returned qty"] = 0.0;
                    newRow["Reason"] = "";
                    newRow["SELECT"] = false;
                    newRow["TaxType"] = purchaseHistoryForm.SelectedTaxType;
                    newRow["TaxPer"] = purchaseHistoryForm.SelectedTaxPer;
                    newRow["TaxAmt"] = purchaseHistoryForm.SelectedTaxAmt;

                    gridTable.Rows.Add(newRow);

                    // Update net amount
                    UpdateNetAmount();

                    // Select and focus the new row
                    UltraGridRow addedRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                    ultraGrid1.ActiveRow = addedRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(addedRow);

                    // Set focus to Quantity cell for editing
                    if (addedRow.Cells.Exists("Quantity"))
                    {
                        ultraGrid1.ActiveCell = addedRow.Cells["Quantity"];
                        ultraGrid1.Focus();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading item from purchase history: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDefaultPaymentMode()
        {
            try
            {
                if (cmbPaymntMethod != null && cmbPaymntMethod.Items.Count > 0)
                {
                    // Set to first item which should be "Select payment" placeholder
                    // No hardcoding - just reset to the first item (placeholder)
                    cmbPaymntMethod.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // If setup fails, continue without default payment mode
                System.Diagnostics.Debug.WriteLine($"Error setting up default payment mode: {ex.Message}");
            }
        }

        private bool ValidatePaymentMethod(bool showErrorMessage = true)
        {
            // Check if payment method is selected and is valid (not the placeholder "Select Payment")
            if (cmbPaymntMethod.SelectedIndex <= 0)
            {
                if (showErrorMessage)
                {
                    MessageBox.Show("Please select a payment method before saving.", "Payment Method Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }

            // Get the selected payment method name
            string selectedPaymentMethod = cmbPaymntMethod.Text.ToLower();

            // Remove the restriction to only allow credit or debit cards
            // All payment methods are now valid as long as one is selected
            return true;
        }

        private void ClearForm()
        {
            try
            {
                // Store source of items before clearing
                bool wasPurchaseInvoice = !string.IsNullOrEmpty(textBox1.Text) && textBox1.Text != "WITHOUT GR";
                bool wasBarcodeItems = textBox1.Text == "WITHOUT GR" || string.IsNullOrEmpty(textBox1.Text);

                // Reset vendor information
                if (VendorName != null)
                {
                    VendorName.Text = "";
                }
                if (ultraTextEditor2 != null)
                {
                    ultraTextEditor2.Text = "";
                }
                if (_lstVendorSuggestions != null)
                {
                    _lstVendorSuggestions.Visible = false;
                }
                if (ultraTextEditor1 != null)
                {
                    ultraTextEditor1.Text = "0.00";
                }
                if (vendorid != null)
                {
                    vendorid.Text = "0";
                }
                _originalBillTotal = 0m;

                // Clear invoice information
                if (textBox1 != null)
                {
                    textBox1.Text = "";
                }

                // Reset dates to current date
                if (ultraDateTimeEditor1 != null)
                {
                    ultraDateTimeEditor1.Value = DateTime.Now;
                }
                if (ultraDateTimeEditor2 != null)
                {
                    ultraDateTimeEditor2.Value = DateTime.Now;
                }

                // Reset payment method to default only if not skipping the reset
                if (!skipPaymentMethodReset)
                {
                    SetupDefaultPaymentMode();
                }
                else
                {
                    // Reset the flag for next time
                    skipPaymentMethodReset = false;
                }

                // Clear grid
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null)
                {
                    // Ensure the data table has all columns including Select, Returned, and Returned qty
                    if (!dt.Columns.Contains("SELECT"))
                    {
                        dt.Columns.Add("SELECT", typeof(bool));
                    }
                    if (!dt.Columns.Contains("Returned"))
                    {
                        dt.Columns.Add("Returned", typeof(decimal));
                    }
                    if (!dt.Columns.Contains("Returned qty"))
                    {
                        dt.Columns.Add("Returned qty", typeof(double));
                    }

                    dt.Rows.Clear();
                    ultraGrid1.DataSource = dt;
                    UpdateFooterValues();
                }

                // Clear totals
                if (textBox2 != null)
                {
                    textBox2.Text = "0.00";
                }
                if (TxtSubTotal != null)
                {
                    TxtSubTotal.Text = "0.00";
                }
                if (lblNetAmount != null)
                {
                    lblNetAmount.Text = "0.00";
                }

                // Clear barcode textbox and make it writable
                if (TxtBarcode != null)
                {
                    TxtBarcode.Clear();
                    TxtBarcode.ReadOnly = _originalTxtBarcodeReadOnly;
                }

                // Re-apply GRN mode control states
                ApplyGRNModeState(chkGRN != null ? chkGRN.Checked : true);

                // Generate new PR number
                GeneratePurchaseReturnNumber();

                // Hide the update button if it exists
                Control[] updateButtons = this.Controls.Find("updtbtn", true);
                if (updateButtons.Length > 0)
                {
                    Button updtbtn = updateButtons[0] as Button;
                    if (updtbtn != null)
                    {
                        updtbtn.Visible = false;
                    }
                }

                // Show the save button
                if (pbxSave != null)
                {
                    pbxSave.Visible = true;
                }

                // Hide the update picture box if it exists
                Control[] updatePictureBoxes = this.Controls.Find("ultraPictureBox4", true);
                if (updatePictureBoxes.Length > 0)
                {
                    updatePictureBoxes[0].Visible = false;
                }

                // Reset flags
                _isPurchaseDataLoaded = false;
                this.Tag = null;
                _currentlyLoadedPurchaseNo = -1;

                // Set focus based on the source of previous items
                Control focusControl = null;
                if (wasPurchaseInvoice)
                {
                    // If items were from purchase invoice, focus on vendor button
                    focusControl = this.Controls.Find("BtnDial", true).FirstOrDefault();
                    if (focusControl == null)
                    {
                        focusControl = this.Controls.Find("Vendorbutton", true).FirstOrDefault();
                    }
                }
                else if (wasBarcodeItems)
                {
                    // If items were from barcode, focus on barcode textbox
                    focusControl = TxtBarcode;
                }
                else
                {
                    // Default focus to vendor button
                    focusControl = this.Controls.Find("BtnDial", true).FirstOrDefault();
                    if (focusControl == null)
                    {
                        focusControl = this.Controls.Find("Vendorbutton", true).FirstOrDefault();
                    }
                    if (focusControl == null && VendorName != null)
                    {
                        focusControl = VendorName;
                    }
                }

                // Set focus to the determined control
                if (focusControl != null)
                {
                    focusControl.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clearing form: " + ex.Message);
            }
        }

        private void cmbPaymntMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle payment method selection change
            Infragistics.Win.ValueListItem selectedItem = cmbPaymntMethod.SelectedItem as Infragistics.Win.ValueListItem;
            if (selectedItem != null && selectedItem.DataValue is DataRowView)
            {
                DataRowView selectedRow = selectedItem.DataValue as DataRowView;
                // You can add payment method-specific logic here if needed
            }
        }

        /// <summary>
        /// Calculates tax amount based on tax type
        /// For inclusive tax: Tax Amount = Total × (Tax% / (100 + Tax%))
        /// For exclusive tax: Tax Amount = Base × (Tax% / 100)
        /// </summary>
        /// <param name="totalAmount">The total amount (for incl) or base amount (for excl)</param>
        /// <param name="taxPercentage">Tax percentage</param>
        /// <param name="taxType">Tax type: "incl" or "excl"</param>
        /// <returns>Tax amount</returns>
        private double CalculateTaxAmount(double totalAmount, double taxPercentage, string taxType)
        {
            if (totalAmount <= 0 || taxPercentage <= 0) return 0;

            string normalizedTaxType = taxType?.ToLower().Trim();

            if (normalizedTaxType == "incl" || normalizedTaxType == "i" || normalizedTaxType == "inclusive")
            {
                // For inclusive tax: Tax Amount = Total × (Tax% / (100 + Tax%))
                // Example: Cost = 56, Tax% = 5, Tax Amount = 56 × (5 / 105) = 2.67
                double denominator = 100.0 + taxPercentage;
                if (denominator > 0)
                {
                    return totalAmount * (taxPercentage / denominator);
                }
                return 0;
            }
            else
            {
                // For exclusive tax: Tax Amount = Base × (Tax% / 100)
                return totalAmount * (taxPercentage / 100.0);
            }
        }

        /// <summary>
        /// Calculates total amount with tax based on tax type (same as frmSalesReturn.cs)
        /// </summary>
        /// <param name="sellingPrice">The selling price</param>
        /// <param name="taxPercentage">Tax percentage</param>
        /// <param name="taxType">Tax type: "incl" or "excl"</param>
        /// <returns>Total amount including tax</returns>
        private double CalculateTotalWithTax(double sellingPrice, double taxPercentage, string taxType)
        {
            if (sellingPrice <= 0) return 0;

            if (taxType?.ToLower() == "incl")
            {
                // For inclusive tax, selling price already includes tax
                return sellingPrice;
            }
            else
            {
                // For exclusive tax, add tax to selling price
                return sellingPrice + CalculateTaxAmount(sellingPrice, taxPercentage, taxType);
            }
        }

        #region Debit Note Preview Functionality

        /// <summary>
        /// Debit Note class for storing debit note details
        /// </summary>
        public class DebitNote
        {
            public int DebitNoteNo { get; set; }
            public DateTime DebitNoteDate { get; set; }
            public string VendorName { get; set; }
            public string InvoiceNo { get; set; }
            public DateTime InvoiceDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string PaymentMethod { get; set; }
            public int VendorLedgerId { get; set; }
        }

        /// <summary>
        /// Generates a DebitNote object from the current purchase return data
        /// </summary>
        private DebitNote GenerateDebitNote(PReturnMaster prMaster, int prNo)
        {
            DebitNote debitNote = new DebitNote
            {
                DebitNoteNo = prNo,
                DebitNoteDate = DateTime.Now,
                VendorName = prMaster.VendorName ?? "",
                InvoiceNo = prMaster.InvoiceNo ?? "",
                InvoiceDate = prMaster.PReturnDate,
                TotalAmount = (decimal)prMaster.GrandTotal,
                PaymentMethod = prMaster.Paymode ?? string.Empty,
                VendorLedgerId = (int)prMaster.LedgerID
            };
            return debitNote;
        }

        /// <summary>
        /// Displays the Debit Note preview form with returned items and action buttons
        /// </summary>
        private void DisplayDebitNote(DebitNote debitNote, PReturnMaster prMaster)
        {
            // Create a form to display the debit note
            Form debitNoteForm = new Form();
            debitNoteForm.Text = "Debit Note";
            debitNoteForm.Size = new Size(500, 600);
            debitNoteForm.StartPosition = FormStartPosition.CenterScreen;
            debitNoteForm.MaximizeBox = false;
            debitNoteForm.MinimizeBox = false;
            debitNoteForm.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Create a panel for the content
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.AutoScroll = true;

            // Add debit note details
            Label titleLabel = new Label();
            titleLabel.Text = "DEBIT NOTE";
            titleLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Size = new Size(460, 40);
            titleLabel.Location = new Point(20, 20);

            Label debitNoteNoLabel = new Label();
            debitNoteNoLabel.Text = $"Debit Note #: {debitNote.DebitNoteNo}";
            debitNoteNoLabel.Font = new Font("Arial", 10);
            debitNoteNoLabel.Size = new Size(300, 20);
            debitNoteNoLabel.Location = new Point(20, 70);

            Label dateLabel = new Label();
            dateLabel.Text = $"Date: {debitNote.DebitNoteDate:dd-MM-yyyy}";
            dateLabel.Font = new Font("Arial", 10);
            dateLabel.Size = new Size(300, 20);
            dateLabel.Location = new Point(20, 90);

            Label vendorLabel = new Label();
            vendorLabel.Text = $"Vendor: {debitNote.VendorName}";
            vendorLabel.Font = new Font("Arial", 10);
            vendorLabel.Size = new Size(300, 20);
            vendorLabel.Location = new Point(20, 120);

            Label invoiceLabel = new Label();
            invoiceLabel.Text = $"Original Invoice #: {debitNote.InvoiceNo}";
            invoiceLabel.Font = new Font("Arial", 10);
            invoiceLabel.Size = new Size(300, 20);
            invoiceLabel.Location = new Point(20, 140);

            Label invoiceDateLabel = new Label();
            invoiceDateLabel.Text = $"Invoice Date: {debitNote.InvoiceDate:dd-MM-yyyy}";
            invoiceDateLabel.Font = new Font("Arial", 10);
            invoiceDateLabel.Size = new Size(300, 20);
            invoiceDateLabel.Location = new Point(20, 160);

            Label amountLabel = new Label();
            amountLabel.Text = $"Total Amount: {debitNote.TotalAmount:N2}";
            amountLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            amountLabel.Size = new Size(300, 25);
            amountLabel.Location = new Point(20, 190);

            Label paymentMethodLabel = new Label();
            paymentMethodLabel.Text = $"Payment Method: {debitNote.PaymentMethod}";
            paymentMethodLabel.Font = new Font("Arial", 10);
            paymentMethodLabel.Size = new Size(300, 20);
            paymentMethodLabel.Location = new Point(20, 220);

            // Create a panel for returned items
            Panel itemsPanel = new Panel();
            itemsPanel.Location = new Point(20, 250);
            itemsPanel.Size = new Size(440, 200);
            itemsPanel.AutoScroll = true;
            itemsPanel.BorderStyle = BorderStyle.FixedSingle;

            // Add header
            int yOffset = 10;
            Label itemHeaderLabel = new Label();
            itemHeaderLabel.Text = "Returned Items:";
            itemHeaderLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            itemHeaderLabel.Size = new Size(400, 20);
            itemHeaderLabel.Location = new Point(10, yOffset);
            itemsPanel.Controls.Add(itemHeaderLabel);
            yOffset += 30;

            // Add column headers
            Label itemNameHeader = new Label();
            itemNameHeader.Text = "Item";
            itemNameHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            itemNameHeader.Size = new Size(200, 20);
            itemNameHeader.Location = new Point(10, yOffset);
            itemsPanel.Controls.Add(itemNameHeader);

            Label qtyHeader = new Label();
            qtyHeader.Text = "Qty";
            qtyHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            qtyHeader.Size = new Size(50, 20);
            qtyHeader.Location = new Point(220, yOffset);
            itemsPanel.Controls.Add(qtyHeader);

            Label priceHeader = new Label();
            priceHeader.Text = "Price";
            priceHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            priceHeader.Size = new Size(80, 20);
            priceHeader.Location = new Point(270, yOffset);
            itemsPanel.Controls.Add(priceHeader);

            Label amountHeader = new Label();
            amountHeader.Text = "Amount";
            amountHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            amountHeader.Size = new Size(80, 20);
            amountHeader.Location = new Point(350, yOffset);
            itemsPanel.Controls.Add(amountHeader);

            yOffset += 25;

            // Add returned items from the grid
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                bool isSelected = false;
                if (row.Cells.Exists("SELECT") && row.Cells["SELECT"].Value != null)
                {
                    isSelected = Convert.ToBoolean(row.Cells["SELECT"].Value);
                }

                if (isSelected)
                {
                    Label itemNameLabel = new Label();
                    itemNameLabel.Text = row.Cells.Exists("Description") ?
                        row.Cells["Description"].Value?.ToString() ?? "" : "";
                    itemNameLabel.Font = new Font("Arial", 9);
                    itemNameLabel.Size = new Size(200, 20);
                    itemNameLabel.Location = new Point(10, yOffset);
                    itemsPanel.Controls.Add(itemNameLabel);

                    Label qtyLabel = new Label();
                    qtyLabel.Text = row.Cells.Exists("Returned qty") ?
                        row.Cells["Returned qty"].Value?.ToString() ?? "" : "";
                    qtyLabel.Font = new Font("Arial", 9);
                    qtyLabel.Size = new Size(50, 20);
                    qtyLabel.Location = new Point(220, yOffset);
                    itemsPanel.Controls.Add(qtyLabel);

                    Label priceLabel = new Label();
                    if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null)
                    {
                        decimal price = 0;
                        decimal.TryParse(row.Cells["Cost"].Value.ToString(), out price);
                        priceLabel.Text = price.ToString("N2");
                    }
                    else
                    {
                        priceLabel.Text = "0.00";
                    }
                    priceLabel.Font = new Font("Arial", 9);
                    priceLabel.Size = new Size(80, 20);
                    priceLabel.Location = new Point(270, yOffset);
                    itemsPanel.Controls.Add(priceLabel);

                    Label itemAmountLabel = new Label();
                    if (row.Cells.Exists("Amount") && row.Cells["Amount"].Value != null)
                    {
                        decimal amt = 0;
                        decimal.TryParse(row.Cells["Amount"].Value.ToString(), out amt);
                        itemAmountLabel.Text = amt.ToString("N2");
                    }
                    else
                    {
                        itemAmountLabel.Text = "0.00";
                    }
                    itemAmountLabel.Font = new Font("Arial", 9);
                    itemAmountLabel.Size = new Size(80, 20);
                    itemAmountLabel.Location = new Point(350, yOffset);
                    itemsPanel.Controls.Add(itemAmountLabel);

                    yOffset += 25;
                }
            }

            // Add note text
            Label noteLabel = new Label();
            noteLabel.Text = "This debit note is issued for the returned items. This is not a tax invoice.";
            noteLabel.Font = new Font("Arial", 9, FontStyle.Italic);
            noteLabel.Size = new Size(400, 40);
            noteLabel.Location = new Point(20, 460);

            // Add Print, Apply Debit Note, and Close buttons
            Button printButton = new Button();
            printButton.Text = "Print";
            printButton.Size = new Size(100, 30);
            printButton.Location = new Point(50, 510);
            printButton.Click += (sender, e) => PrintDebitNote(debitNote);

            Button applyDebitNoteButton = new Button();
            applyDebitNoteButton.Text = "Apply Debit Note";
            applyDebitNoteButton.Size = new Size(120, 30);
            applyDebitNoteButton.Location = new Point(160, 510);
            applyDebitNoteButton.Click += (sender, e) => OpenDebitNoteForm(debitNote, debitNoteForm);

            Button closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Size = new Size(100, 30);
            closeButton.Location = new Point(290, 510);
            closeButton.Click += (sender, e) => debitNoteForm.Close();

            // Add controls to the panel
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(debitNoteNoLabel);
            panel.Controls.Add(dateLabel);
            panel.Controls.Add(vendorLabel);
            panel.Controls.Add(invoiceLabel);
            panel.Controls.Add(invoiceDateLabel);
            panel.Controls.Add(amountLabel);
            panel.Controls.Add(paymentMethodLabel);
            panel.Controls.Add(itemsPanel);
            panel.Controls.Add(noteLabel);
            panel.Controls.Add(printButton);
            panel.Controls.Add(applyDebitNoteButton);
            panel.Controls.Add(closeButton);

            // Add panel to the form
            debitNoteForm.Controls.Add(panel);

            // Show the form
            debitNoteForm.ShowDialog();
        }

        /// <summary>
        /// Prints the debit note
        /// </summary>
        private void PrintDebitNote(DebitNote debitNote)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Printing debit note...", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Actual printing code would go here
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing debit note: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens the FrmDebitNote form with pre-filled data
        /// </summary>
        private void OpenDebitNoteForm(DebitNote debitNote, Form debitNoteForm)
        {
            try
            {
                // Create the Debit Note form with Purchase Return data
                var frmDebitNote = new PosBranch_Win.Accounts.FrmDebitNote(
                    debitNote.DebitNoteNo,
                    debitNote.VendorLedgerId,
                    debitNote.VendorName,
                    debitNote.TotalAmount,
                    debitNote.InvoiceNo
                );

                // Close the summary popup
                debitNoteForm.Close();

                // Open the Debit Note form
                OpenFormInTab(frmDebitNote, $"Debit Note - {debitNote.VendorName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Debit Note form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens a form in the Home tab control or as a regular form
        /// </summary>
        private void OpenFormInTab(Form form, string tabName)
        {
            try
            {
                // Find the Home form (parent form with tab control)
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    // Use reflection to call the OpenFormInTab method
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabMethod != null)
                    {
                        // Call the OpenFormInTab method
                        openFormInTabMethod.Invoke(homeForm, new object[] { form, tabName });
                        return;
                    }
                }

                // Fallback: show as regular form
                form.Show();
                form.BringToFront();
            }
            catch (Exception ex)
            {
                // Fallback: show as regular form
                form.Show();
                form.BringToFront();
                System.Diagnostics.Debug.WriteLine($"Error opening in tab, showing as regular form: {ex.Message}");
            }
        }

        #endregion

        #region Column Chooser & Drag-Down to Hide

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(32, 32))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (SolidBrush bgBrush = new SolidBrush(Color.Black))
                    {
                        g.FillEllipse(bgBrush, 4, 4, 24, 24);
                    }

                    using (Pen whitePen = new Pen(Color.White, 3.5f))
                    {
                        whitePen.StartCap = LineCap.Round;
                        whitePen.EndCap = LineCap.Round;
                        g.DrawLine(whitePen, 11, 11, 21, 21);
                        g.DrawLine(whitePen, 21, 11, 11, 21);
                    }

                    IntPtr hIcon = bmp.GetHicon();
                    return new Cursor(hIcon);
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            ultraGrid1.AllowDrop = true;
            ultraGrid1.MouseDown += Grid_MouseDown;
            ultraGrid1.MouseMove += Grid_MouseMove;
            ultraGrid1.MouseUp += Grid_MouseUp;
            ultraGrid1.DragOver += Grid_DragOver;
            ultraGrid1.DragDrop += Grid_DragDrop;

            ContextMenuStrip headerMenu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            chooserItem.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            headerMenu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            headerMenu.Items.Add(showAllItem);

            ultraGrid1.ContextMenuStrip = headerMenu;
        }

        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            UIElement element = ultraGrid1.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            UltraGridColumn col = headerUI?.Header?.Column;
            if (headerUI != null && col != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    ShowHeaderContextMenu(col, e.Location);
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    isDraggingHeaderToHide = true;
                    columnBeingDragged = col;
                    headerDragStartPoint = new Point(e.X, e.Y);
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide || columnBeingDragged == null || e.Button != MouseButtons.Left)
                return;

            int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
            int deltaY = e.Y - headerDragStartPoint.Y;

            if (deltaY > 25 && deltaY > deltaX)
            {
                ultraGrid1.Cursor = blackXCursor;
                string colName = !string.IsNullOrEmpty(columnBeingDragged.Header.Caption) ? columnBeingDragged.Header.Caption : columnBeingDragged.Key;
                headerToolTip.SetToolTip(ultraGrid1, $"✖ Drag down to hide '{colName}' column");

                if (deltaY > 50)
                {
                    HideColumn(columnBeingDragged);
                    isDraggingHeaderToHide = false;
                    columnBeingDragged = null;
                    ultraGrid1.Cursor = Cursors.Default;
                    headerToolTip.SetToolTip(ultraGrid1, string.Empty);
                }
            }
        }

        private void Grid_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide)
            {
                if (columnBeingDragged != null && (e.Y - headerDragStartPoint.Y) > 40)
                {
                    HideColumn(columnBeingDragged);
                }
                isDraggingHeaderToHide = false;
                columnBeingDragged = null;
                ultraGrid1.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(ultraGrid1, string.Empty);
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void Grid_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = ultraGrid1.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = ultraGrid1.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (!col.Hidden)
                {
                    UIElement hUI = col.Header.GetUIElement();
                    if (hUI != null && pt.X >= hUI.Rect.Left && pt.X <= hUI.Rect.Right)
                    {
                        return col.Header.VisiblePosition;
                    }
                }
            }

            return band.Columns.Count;
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null) return;
            userHiddenColumnKeys.Add(col.Key);
            col.Hidden = true;
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn col, Point location)
        {
            if (col == null) return;
            ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            string colName = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(ultraGrid1, location);
        }

        private void ShowColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooser();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization (Field Chooser)",
                Size = new Size(240, 300),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 244, 248),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248),
                ItemHeight = 34,
                IntegralHeight = false
            };

            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;

            columnChooserForm.Controls.Add(columnChooserListBox);
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnChooserListBox != null)
            {
                int index = columnChooserListBox.IndexFromPoint(e.Location);
                if (index >= 0 && index < columnChooserListBox.Items.Count)
                {
                    if (columnChooserListBox.Items[index] is ColumnChooserItem item)
                    {
                        columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                    }
                }
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden && !col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.ColumnKey);
            }
        }

        private void UnhideColumn(string columnKey, int? targetVisiblePosition = null)
        {
            userHiddenColumnKeys.Remove(columnKey);
            if (ultraGrid1.DisplayLayout.Bands.Count > 0 && ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn col = ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey];
                col.Hidden = false;
                if (targetVisiblePosition.HasValue)
                {
                    col.Header.VisiblePosition = targetVisiblePosition.Value;
                }
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (!col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    col.Hidden = false;
                }
            }
            UpdateFooterCellPositions();
            UpdateFooterValues();
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    Right - columnChooserForm.Width - 30,
                    Bottom - columnChooserForm.Height - 30);
                columnChooserForm.BringToFront();
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count)
                return;

            if (!(columnChooserListBox.Items[e.Index] is ColumnChooserItem item))
                return;

            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -3);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0, 121, 211)))
            using (GraphicsPath path = RoundedRect(rect, 4))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                using (Font textFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    e.Graphics.DrawString(item.DisplayText, textFont, textBrush, rect, sf);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        #endregion

        #region UltraPanelGridFooter Dynamic Alignment & Calculation

        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null) return;
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            footerLabels.Clear();

            if (ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            int xOffset = ultraGrid1.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label();
                footerLabel.Name = "footer_" + column.Key;
                footerLabel.Text = string.Empty;
                footerLabel.TextAlign = ContentAlignment.MiddleCenter;
                footerLabel.BackColor = Color.FromArgb(93, 151, 214);
                footerLabel.BorderStyle = BorderStyle.None;
                footerLabel.AutoSize = false;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Left = xOffset;
                footerLabel.Top = 1;
                footerLabel.Tag = Tuple.Create(column.Key, string.Empty);
                footerLabel.ForeColor = Color.White;
                footerLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                footerLabel.Paint += FooterLabel_Paint;
                footerLabel.ContextMenuStrip = CreateFooterContextMenu(column.Key);

                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                footerLabels[column.Key] = footerLabel;

                if (!columnAggregations.ContainsKey(column.Key))
                {
                    columnAggregations[column.Key] = "None";
                }

                xOffset += column.Width;
            }
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null) return;

            using (Pen borderPen = new Pen(Color.FromArgb(118, 154, 198), 1))
            {
                e.Graphics.DrawLine(borderPen, lbl.Width - 1, 0, lbl.Width - 1, lbl.Height);
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            bool isNumeric = ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                             ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey]);

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

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type t = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
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

        private void UpdateFooterCellPositions()
        {
            if (ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0 || footerLabels.Count == 0 || ultraPanelGridFooter == null)
                return;

            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            int rowSelectorWidth = ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
            int scrollOffset = 0;
            if (ultraGrid1.ActiveColScrollRegion != null)
            {
                scrollOffset = ultraGrid1.ActiveColScrollRegion.Position;
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

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (ultraGrid1.Rows == null) yield break;
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return null;

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
            if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            if (result == null)
            {
                if (string.Equals(aggregation, "Count", StringComparison.OrdinalIgnoreCase))
                    return "0";
                if (string.Equals(aggregation, "Sum", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Avg", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Min", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Max", StringComparison.OrdinalIgnoreCase))
                    return "0.00";
                return string.Empty;
            }

            if (aggregation == "Count")
                return Convert.ToString(result);

            if (ultraGrid1.DisplayLayout != null &&
                ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey];
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

        private static decimal? GetNumericValue(object rawValue)
        {
            if (rawValue == null || rawValue == DBNull.Value)
                return null;

            if (rawValue is decimal decVal) return decVal;
            if (rawValue is double dblVal) return Convert.ToDecimal(dblVal);
            if (rawValue is float fltVal) return Convert.ToDecimal(fltVal);
            if (rawValue is int intVal) return Convert.ToDecimal(intVal);
            if (rawValue is long lngVal) return Convert.ToDecimal(lngVal);

            string strVal = Convert.ToString(rawValue);
            if (decimal.TryParse(strVal, out decimal parsed))
                return parsed;

            return null;
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString());
        }

        #endregion

        private void TxtSubTotal_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
