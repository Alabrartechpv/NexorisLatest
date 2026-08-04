using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using ModelClass.Master;
using Repository;
using Repository.TransactionRepository;
using Repository.SettingsRepo;
using PosBranch_Win.DialogBox;
using ModelClass.TransactionModels;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win;

namespace PosBranch_Win.Transaction
{
    public partial class FrmPurchase : Form
    {
        private const string GRID_LAYOUT_FILE = "PurchaseGridLayout.xml";
        private string GridLayoutPath => System.IO.Path.Combine(Application.StartupPath, GRID_LAYOUT_FILE);

        // Load saved grid layout from XML file
        private void LoadGridLayout()
        {
            try
            {
                if (System.IO.File.Exists(GridLayoutPath))
                {
                    ultraGrid1.DisplayLayout.LoadFromXml(GridLayoutPath);
                    ApplyGridTheme(ultraGrid1);
                    RefreshGridColumnTheme(ultraGrid1);

                    // Re-apply essential layout configurations specifically overridden by XML structure
                    ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
                    ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = GridAltRow;
                    ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                    // Ensure old layouts don't override the new Gross/NetAmt requirement
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Gross") &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("NetAmt"))
                    {
                        var band = ultraGrid1.DisplayLayout.Bands[0];
                        band.Columns["Gross"].Header.VisiblePosition = band.Columns.Count - 2;
                        band.Columns["NetAmt"].Header.VisiblePosition = band.Columns.Count - 1;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading grid layout: {ex.Message}");
            }
        }

        // Save grid layout to XML file
        private void SaveGridLayout()
        {
            try
            {
                ultraGrid1.DisplayLayout.SaveAsXml(GridLayoutPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving grid layout: {ex.Message}");
            }
        }

        // Static event to notify other forms when prices are updated in PriceSettings
        // The int parameter is the ItemId that was updated
        public static event Action<int> OnPriceSettingsUpdated;

        private System.Collections.Generic.Dictionary<string, float> _originalSellingPrices = new System.Collections.Generic.Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);

        // Guard flag to prevent infinite recursion when updating cells from NetAmt change
        private bool _isUpdatingFromNetAmt = false;

        // Guard flag to prevent AfterCellUpdate from triggering during unit change (fixes rate multiplication bug)
        private bool _isUpdatingUnit = false;

        // Guard flag to prevent recursive cell redirection when locked cells are clicked
        private bool _isRedirectingLockedCellActivation = false;

        // Guard flag to allow internal Unit updates while keeping the cell locked for manual edits
        private bool _isApplyingUnitSelection = false;
        private bool _purchaseStatusRowHandlerAdded = false;
        private bool _isLoadingPurchaseForActivitySnapshot = false;
        private bool _isUpdatingPurchase = false;
        private string _activityInvoiceNo = string.Empty;
        private string _activityBilledBy = string.Empty;
        private DateTime? _activityInvoiceDate;
        private DateTime? _activityPurchaseDate;

        // Flag to track if the user manually changed label4 (Net Total) via ".." shortcut in txtBarcode
        private bool _isNetTotalManuallySet = false;

        // Read-only state for paid/half-paid purchases
        private bool _isReadOnly = false;
        private int _lastReadOnlyNotifiedPurchaseId = -1;   // tracks which PID already showed the read-only notice
        private const string MsgReadOnlyModeTitle = "Read-Only Mode";
        private const string MsgReadOnlyModeDetails = "GRN-{0} has vendor payment against it.\nPaid amount: {1:N2}\n\nLoaded in READ-ONLY mode. You cannot edit, add, or delete items.";
        private const string MsgReadOnlyBlock = "This purchase is in read-only mode because it has payments against it.";

        // Layout state for PO section toggle in the purchase screen
        private int _poPanelLeft;
        private int _poPanelTop;
        private int _poPanelWidth;
        private int _panelTop;
        private int _panelGap;
        private int _rightMargin;
        private int _itemPanelVisibleLeft;
        private int _label9OffsetX;
        private int _label10OffsetX;
        private int _labelOffsetY;
        private bool _purchaseOrderPanelVisible;

        // Helper method to raise the price update event safely
        private static void RaisePriceSettingsUpdated(int itemId)
        {
            try
            {
                OnPriceSettingsUpdated?.Invoke(itemId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error raising OnPriceSettingsUpdated event: {ex.Message}");
            }
        }

        // Helper methods to get dynamic CompanyId and BranchId
        // Prefer SessionContext, fallback to DataBase for backward compatibility
        private int GetCompanyId()
        {
            try
            {
                if (SessionContext.IsInitialized && SessionContext.CompanyId > 0)
                {
                    return SessionContext.CompanyId;
                }
                else if (!string.IsNullOrEmpty(DataBase.CompanyId) && int.TryParse(DataBase.CompanyId, out int companyId) && companyId > 0)
                {
                    return companyId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting CompanyId: {ex.Message}");
            }
            throw new InvalidOperationException("CompanyId is not set. Please ensure session is initialized.");
        }

        private int GetBranchId()
        {
            try
            {
                // First try SessionContext (preferred)
                if (SessionContext.IsInitialized && SessionContext.BranchId > 0)
                {
                    return SessionContext.BranchId;
                }

                // Second try the combo box selection (if form is loaded)
                if (CmboBranch != null && CmboBranch.SelectedValue != null)
                {
                    int branchId = Convert.ToInt32(CmboBranch.SelectedValue);
                    if (branchId > 0)
                    {
                        // Update SessionContext with the combo box value
                        if (SessionContext.IsInitialized)
                        {
                            SessionContext.BranchId = branchId;
                        }
                        return branchId;
                    }
                }

                // Third try DataBase
                if (!string.IsNullOrEmpty(DataBase.BranchId) && int.TryParse(DataBase.BranchId, out int dbBranchId) && dbBranchId > 0)
                {
                    return dbBranchId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting BranchId: {ex.Message}");
            }
            throw new InvalidOperationException("BranchId is not set. Please ensure session is initialized or a branch is selected.");
        }

        private int GetFinYearId()
        {
            try
            {
                // First try SessionContext
                if (SessionContext.IsInitialized && SessionContext.FinYearId > 0)
                {
                    return SessionContext.FinYearId;
                }

                // Second try DataBase
                if (!string.IsNullOrEmpty(DataBase.FinyearId) && int.TryParse(DataBase.FinyearId, out int finYearId) && finYearId > 0)
                {
                    return finYearId;
                }

                // Third try: Get active financial year from database using repository connection
                try
                {
                    System.Diagnostics.Debug.WriteLine("FinYearId not found in SessionContext or DataBase. Attempting to get active FinYear from database");

                    // Use the repository's DataConnection to query for active financial year
                    bool connectionWasOpen = ObjPurchaseInviceRepo.DataConnection.State == System.Data.ConnectionState.Open;
                    if (!connectionWasOpen)
                    {
                        ObjPurchaseInviceRepo.DataConnection.Open();
                    }

                    try
                    {
                        // First try to get active financial year
                        string finYearQuery = @"
                            SELECT TOP 1 Id 
                            FROM Finyears 
                            WHERE IsActive = 1 
                            ORDER BY Id DESC";

                        using (var cmd = new System.Data.SqlClient.SqlCommand(finYearQuery, (System.Data.SqlClient.SqlConnection)ObjPurchaseInviceRepo.DataConnection))
                        {
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                int dbFinYearId = Convert.ToInt32(result);
                                System.Diagnostics.Debug.WriteLine($"Retrieved active FinYearId from database: {dbFinYearId}");
                                return dbFinYearId;
                            }
                        }

                        // If no active financial year found, try to get any financial year
                        System.Diagnostics.Debug.WriteLine("No active financial year found. Attempting to get any financial year from database");
                        string anyFinYearQuery = @"
                            SELECT TOP 1 Id 
                            FROM Finyears 
                            ORDER BY Id DESC";

                        using (var cmd = new System.Data.SqlClient.SqlCommand(anyFinYearQuery, (System.Data.SqlClient.SqlConnection)ObjPurchaseInviceRepo.DataConnection))
                        {
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                int dbFinYearId = Convert.ToInt32(result);
                                System.Diagnostics.Debug.WriteLine($"Retrieved FinYearId from database (any available): {dbFinYearId}");
                                return dbFinYearId;
                            }
                        }
                    }
                    finally
                    {
                        // Only close connection if we opened it
                        if (!connectionWasOpen && ObjPurchaseInviceRepo.DataConnection.State == System.Data.ConnectionState.Open)
                        {
                            ObjPurchaseInviceRepo.DataConnection.Close();
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error retrieving FinYearId from database: {dbEx.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting FinYearId: {ex.Message}");
            }

            // Return 0 instead of throwing - repository will handle getting FinYearId from database
            System.Diagnostics.Debug.WriteLine("GetFinYearId: Returning 0 - repository will retrieve FinYearId from database");
            return 0;
        }

        private int GetUserId()
        {
            try
            {
                if (SessionContext.IsInitialized && SessionContext.UserId > 0)
                {
                    return SessionContext.UserId;
                }
                else if (!string.IsNullOrEmpty(DataBase.UserId) && int.TryParse(DataBase.UserId, out int userId) && userId > 0)
                {
                    return userId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting UserId: {ex.Message}");
            }
            throw new InvalidOperationException("UserId is not set. Please ensure session is initialized.");
        }

        private string GetUserName()
        {
            try
            {
                if (!string.IsNullOrEmpty(SessionContext.UserName))
                {
                    return SessionContext.UserName;
                }
                else if (!string.IsNullOrEmpty(DataBase.UserName))
                {
                    return DataBase.UserName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting UserName: {ex.Message}");
            }
            return string.Empty;
        }

        /// <summary>
        /// Generates and displays the purchase number dynamically using current CompanyId and BranchId
        /// </summary>
        private void GenerateAndDisplayPurchaseNumber()
        {
            try
            {
                int companyId = GetCompanyId();
                int branchId = GetBranchId();

                int purchaseNo = ObjPurchaseInviceRepo.GeneratePurchaseNO();

                if (purchaseNo > 0)
                {
                    txtPurchaseNo.Text = "GRN-" + purchaseNo.ToString();
                    System.Diagnostics.Debug.WriteLine($"Generated and displayed PurchaseNo: {purchaseNo} for CompanyId={companyId}, BranchId={branchId}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Warning: GeneratePurchaseNO returned 0 or invalid value");
                    txtPurchaseNo.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating purchase number: {ex.Message}");
                txtPurchaseNo.Text = "";
            }
        }

        // Helper method to get currency information dynamically
        private void SetCurrencyInfo(PurchaseMaster purchaseMaster)
        {
            try
            {
                Dropdowns dp = new Dropdowns();
                var currencies = dp.getCurrency();
                if (currencies?.List != null && currencies.List.Any())
                {
                    var defaultCurrency = currencies.List.FirstOrDefault();
                    purchaseMaster.CurrencyID = defaultCurrency?.CurrencyID ?? 1;
                    purchaseMaster.CurSymbol = defaultCurrency?.CurrencySymbol ?? "RM";
                }
                else
                {
                    purchaseMaster.CurrencyID = 1;
                    purchaseMaster.CurSymbol = "RM";
                }
            }
            catch (Exception ex)
            {
                // Fallback to defaults if database lookup fails
                purchaseMaster.CurrencyID = 1;
                purchaseMaster.CurSymbol = "RM";
                System.Diagnostics.Debug.WriteLine($"Error loading currency: {ex.Message}");
            }
        }

        Dropdowns drop = new Dropdowns();
        public float Amount, TotalAmount, SubTotal, NetTotal, OriginalNetTotal;
        public bool CheckExists;
        public PurchaseMaster ObjPurchaseMaster = new PurchaseMaster();
        public PurchaseDetails ObjPurchaseDetails = new PurchaseDetails();
        public PurchaseInvoiceRepository ObjPurchaseInviceRepo = new PurchaseInvoiceRepository();
        private readonly PurchaseOrderRepository _purchaseOrderRepository = new PurchaseOrderRepository();

        private static readonly Color FormBackColor = Color.FromArgb(214, 230, 240);
        private static readonly Color PanelBackColor = Color.FromArgb(214, 229, 241);
        private static readonly Color SoftGreyPanelBackColor = Color.FromArgb(228, 231, 235);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color WarmControlBackColor = Color.FromArgb(255, 229, 198);
        private static readonly Color SoftReadonlyBackColor = Color.FromArgb(233, 231, 214);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder = Color.FromArgb(144, 181, 223);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);
        private static readonly Color PanelHoverTopColor = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor = Color.FromArgb(170, 206, 244);
        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        // Define the highlight color for mandatory fields
        private Color mandatoryFieldColor = WarmControlBackColor;
        private Color defaultBackColor;

        // Add tooltip for barcode commands
        private System.Windows.Forms.ToolTip barcodeTooltip = new System.Windows.Forms.ToolTip();
        private Timer _barcodeFlashTimer;

        // Note: ultraPanel6 is defined in the Designer.cs file
        // We'll use gridFooterPanel as a separate panel for the footer functionality

        // Dedicated panel for grid footer
        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;
        private Infragistics.Win.Misc.UltraPanel ultraGrid2FooterPanel;

        // Dictionary to track footer labels by column key
        private Dictionary<string, Label> footerLabels = new Dictionary<string, Label>();
        private Dictionary<string, Label> ultraGrid2FooterLabels = new Dictionary<string, Label>();
        // Dictionary to track the aggregation function for each column
        private Dictionary<string, string> columnAggregations = new Dictionary<string, string>();

        // Column chooser functionality
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private Point startPoint;
        private UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private System.Windows.Forms.ToolTip columnToolTip = new System.Windows.Forms.ToolTip();

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

        public FrmPurchase()
        {
            InitializeComponent();
            CapturePurchasePanelLayout();
            ApplyPurchaseOrderTheme();

            // Setup Custom Digital Font for labels
            try
            {
                System.Drawing.Text.PrivateFontCollection pfc = new System.Drawing.Text.PrivateFontCollection();
                string fontPath = System.IO.Path.Combine(Application.StartupPath, "Font", "DS-DIGI.TTF");
                if (System.IO.File.Exists(fontPath))
                {
                    pfc.AddFontFile(fontPath);
                    if (pfc.Families.Length > 0)
                    {
                        label4.Font = new Font(pfc.Families[0], 36, FontStyle.Bold);
                        label6.Font = new Font(pfc.Families[0], 36, FontStyle.Bold);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading custom font: {ex.Message}");
            }

            // Initialize grid footer panel
            gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.Controls.Add(gridFooterPanel);
            gridFooterPanel.Visible = false; // Initially invisible until properly positioned

            ultraGrid2FooterPanel = new Infragistics.Win.Misc.UltraPanel();
            ultraPanel7.ClientArea.Controls.Add(ultraGrid2FooterPanel);
            ultraGrid2FooterPanel.Visible = false;

            // Add click event for save button
            pbxSave.Click += (s, e) => this.SavePurchase();

            // Add click event for update button
            ultraPictureBox4.Click += (s, e) => this.UpdatePurchase();

            // Add click event for delete button
            ultraPictureBox2.Click += (s, e) => this.DeletePurchase();

            // Add click event for clear button (ultraPictureBox1)
            ultraPictureBox1.Click += (s, e) => this.Clear();

            // Add click event for button2
            button2.Click += (s, e) => ShowPurchaseDisplayDialog();
            WireGridClearButton("button6", ClearUltraGrid1Contents);
            WireGridClearButton("button7", ClearUltraGrid2Contents);
            button8.Click += (s, e) => ShowBatchQtyPopup();

            // Add click event for btnFInd (F7)
            btnFInd.Click += btnFInd_Click;

            // UltraComboEditor raises ValueChanged when the selected item changes.
            CmboVendor.ValueChanged += CmboVendor_ValueChanged;

            // Add DoubleClickCell event for ultraGrid1
            ultraGrid1.DoubleClickCell += UltraGrid1_DoubleClickCell;
            ultraGrid2.InitializeLayout += UltraGrid2_InitializeLayout;
            ultraGrid2.Resize += (s, e) => UpdateUltraGrid2FooterCellPositions();

            // Register for resize events to update footer position & panel layout
            ultraGrid1.Resize += (s, e) => UpdateFooterCellPositions();
            this.Resize += (s, e) => { SetPurchaseOrderPanelVisibility(_purchaseOrderPanelVisible); UpdateFooterCellPositions(); };
            ultraPanel1.Resize += (s, e) => SetPurchaseOrderPanelVisibility(_purchaseOrderPanelVisible);

            // Add click event for ultraPictureBox7 to open PurchaseEdit with highlighted row


            // Add click event for pbxExit to remove selected item
            pbxExit.Click += (s, e) => RemoveSelectedItem();

            // Optimized navigation button handling - using MouseDown instead of Click for immediate response
            ultraPictureBox5.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    MoveSelectedItemUp();
                }
            };

            ultraPictureBox6.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    MoveSelectedItemDown();
                }
            };

            // Add KeyDown event for txtRoundOff to handle round-off calculations
            txtRoundOff.KeyDown += txtRoundOff_KeyDown;

            // Add KeyDown event for textBox1 to load vendor by ID
            textBox1.KeyDown += textBox1_KeyDown;

            // Add TextChanged event for textBox1
            textBox1.TextChanged += textBox1_TextChanged;

            // Add TextChanged events for mandatory fields
            CmboPayment.ValueChanged += MandatoryField_Changed;
            txtInvoiceNo.TextChanged += MandatoryField_Changed;
            txtBilledBy.TextChanged += MandatoryField_Changed;

            // Configure tooltip for barcode commands
            barcodeTooltip.AutoPopDelay = 5000;
            barcodeTooltip.InitialDelay = 500;
            barcodeTooltip.ReshowDelay = 200;
            barcodeTooltip.ShowAlways = true;
            barcodeTooltip.BackColor = Color.FromArgb(255, 210, 170); // Peach/orange-tan shade
            barcodeTooltip.ForeColor = Color.Black;
            barcodeTooltip.IsBalloon = false; // No balloon style
            barcodeTooltip.OwnerDraw = true; // Enable owner-draw for custom appearance

            // Handle owner-draw event to remove border
            barcodeTooltip.Draw += (s, e) =>
            {
                e.DrawBackground();
                e.DrawText();
                // Skip drawing the border by not calling e.DrawBorder()
            };

            // Add mouse events for txtBarcode to show/hide tooltip
            txtBarcode.MouseEnter += (s, e) =>
            {
                barcodeTooltip.Show("\"=\" edit  |  \"U/u\" unit  |  \". / del\" delete  |  \"*\" qty  |  \".x(Cost)\"", txtBarcode);
            };

            txtBarcode.MouseLeave += (s, e) =>
            {
                barcodeTooltip.Hide(txtBarcode);
            };

            // Set KeyPreview to true to capture F3 key at form level
            this.KeyPreview = true;
            this.KeyDown += FrmPurchase_KeyDown_ToggleSellingPrice;

            // Setup column chooser functionality
            SetupColumnChooserMenu();
        }

        private void ApplyPurchaseOrderTheme()
        {
            BackColor = FormBackColor;

            ultraPanel1.Appearance.BackColor = FormBackColor;
            ultraPanel1.Appearance.BackColor2 = FormBackColor;
            ultraPanel1.Appearance.BackGradientStyle = GradientStyle.None;

            ApplyLightPanelTheme(ultraPanel3, FormBackColor);
            ApplyLightPanelTheme(ultraPanel7, FormBackColor);
            ApplyLightPanelTheme(ultraPanel4, FormBackColor);

            ultraPanel5.Appearance.BackColor = FormBackColor;
            ultraPanel5.Appearance.BackColor2 = FormBackColor;
            ultraPanel5.Appearance.BackGradientStyle = GradientStyle.None;

            StyleUltraTextEditor(textBox1, true);
            StyleUltraTextEditor(txtPurchaseNo, false, SoftReadonlyBackColor);
            StyleUltraTextEditor(txtInvoiceNo, true);
            StyleUltraTextEditor(txtBilledBy, true);
            StyleUltraTextEditor(txtRoundOff, false);
            StyleUltraTextEditor(textBox2, false, SoftReadonlyBackColor);
            StyleUltraTextEditor(textBox3, false, SoftReadonlyBackColor);
            StyleUltraTextEditor(textBox4, false, SoftReadonlyBackColor);
            StyleUltraTextEditor(textBox5, false);
            StyleUltraTextEditor(txtBarcode, true);
            StyleUltraTextEditor(txtRemark, false);

            StyleUltraDateTimeEditor(DtpInoviceDate);
            StyleUltraDateTimeEditor(dtpPurchaseDate);

            StyleUltraComboEditor(CmboVendor);
            StyleUltraComboEditor(CmboPayment);
            StyleLegacyComboBox(CmboBranch);
            StyleUltraComboEditor(comboBox1);

            StyleStandardButton(button1, "F11");
            StyleStandardButton(button2, "...");
            StyleStandardButton(button3, "F6");
            StyleStandardButton(button4, ">>");
            StyleStandardButton(button5, ">");

            StyleSectionLabels(ultraPanel1.ClientArea.Controls);
            StyleSectionLabels(ultraPanel3.ClientArea.Controls);
            StyleSectionLabels(ultraPanel5.ClientArea.Controls);
            StyleSectionLabels(ultraPanel7.ClientArea.Controls);
            StyleSectionLabels(ultraPanel4.ClientArea.Controls);

            label7.Font = new Font("Tahoma", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Font = new Font("Tahoma", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.Font = new Font("Tahoma", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.ForeColor = Color.Black;
            label9.ForeColor = Color.Black;
            label10.ForeColor = Color.Black;

            lblSubTotalAmt.BackColor = SoftReadonlyBackColor;
            lblSubTotalAmt.ForeColor = ControlTextColor;
        }

        private static void ApplyLightPanelTheme(Infragistics.Win.Misc.UltraPanel panel, Color? backColorOverride = null)
        {
            if (panel == null)
                return;

            Color panelBackColor = backColorOverride ?? PanelBackColor;
            panel.Appearance.BackColor = panelBackColor;
            panel.Appearance.BackColor2 = panelBackColor;
            panel.Appearance.BackGradientStyle = GradientStyle.None;
            panel.Appearance.BorderColor = BorderBlue;
            panel.BorderStyle = UIElementBorderStyle.Solid;
        }

        private static void StyleStandardTextBox(TextBox textBox, bool warmBackColor, Color? backColorOverride = null)
        {
            if (textBox == null)
                return;

            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = backColorOverride ?? (warmBackColor ? WarmControlBackColor : ControlBackColor);
            textBox.ForeColor = ControlTextColor;
            textBox.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleUltraTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor, bool warmBackColor, Color? backColorOverride = null)
        {
            if (editor == null)
                return;

            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.Appearance.BackColor = backColorOverride ?? (warmBackColor ? WarmControlBackColor : ControlBackColor);
            editor.BackColor = editor.Appearance.BackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleUltraDateTimeEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dateEditor)
        {
            if (dateEditor == null)
                return;

            dateEditor.UseAppStyling = false;
            dateEditor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            dateEditor.BorderStyle = UIElementBorderStyle.Solid;
            dateEditor.UseOsThemes = DefaultableBoolean.False;
            dateEditor.Appearance.BackColor = ControlBackColor;
            dateEditor.BackColor = ControlBackColor;
            dateEditor.Appearance.BorderColor = SkyBlueOutline;
            dateEditor.Appearance.ForeColor = ControlTextColor;
            dateEditor.Appearance.FontData.Name = "Tahoma";
            dateEditor.Appearance.FontData.SizeInPoints = 10F;
            dateEditor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            dateEditor.FormatString = "dd/MMM/yyyy";
            dateEditor.MaskInput = "{date}";
        }

        private static void StyleUltraComboEditor(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null)
                return;

            combo.UseAppStyling = false;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            combo.Appearance.BackColor = ControlBackColor;
            combo.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 10F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
        }

        private static void StyleLegacyComboBox(ComboBox comboBox)
        {
            if (comboBox == null)
                return;

            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = ControlBackColor;
            comboBox.ForeColor = ControlTextColor;
            comboBox.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleStandardButton(Button button, string text)
        {
            if (button == null)
                return;

            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ButtonBlueBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 197, 230);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(126, 166, 214);
            button.BackColor = Color.FromArgb(155, 188, 224);
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        }

        private void InitializeActionPanels()
        {
            RegisterActionPanel(ultraPanel2, TogglePurchaseOrderPanelVisibility);
        }

        private void CapturePurchasePanelLayout()
        {
            _poPanelLeft = ultraPanel7.Left;
            _poPanelTop = ultraPanel7.Top;
            _poPanelWidth = ultraPanel7.Width;
            _panelTop = ultraPanel4.Top;
            _panelGap = ultraPanel4.Left - ultraPanel7.Right;
            _itemPanelVisibleLeft = _poPanelLeft + _poPanelWidth + _panelGap;
            _rightMargin = ultraPanel1.ClientArea.Width - ultraPanel4.Right;
            _label9OffsetX = label9.Left - ultraPanel7.Left;
            _label10OffsetX = label10.Left - ultraPanel4.Left;
            _labelOffsetY = label10.Top;
        }

        private void TogglePurchaseOrderPanelVisibility()
        {
            SetPurchaseOrderPanelVisibility(!_purchaseOrderPanelVisible);
        }

        private void SetPurchaseOrderPanelVisibility(bool visible)
        {
            if (ultraPanel1 == null)
                return;

            ultraPanel1.SuspendLayout();
            ultraPanel7.SuspendLayout();
            ultraPanel4.SuspendLayout();
            if (ultraPanel5 != null) ultraPanel5.SuspendLayout();

            try
            {
                _purchaseOrderPanelVisible = visible;

                int clientWidth = ultraPanel1.ClientArea.Width;
                int clientHeight = ultraPanel1.ClientArea.Height;

                if (clientWidth <= 0 || clientHeight <= 0)
                    return;

                // 1. Position bottom totals panel (ultraPanel5) dynamically at the bottom of ultraPanel1
                if (ultraPanel5 != null)
                {
                    int panel5Height = ultraPanel5.Height > 0 ? ultraPanel5.Height : 95;
                    ultraPanel5.Left = 6;
                    ultraPanel5.Width = Math.Max(100, clientWidth - 12);
                    ultraPanel5.Top = Math.Max(250, clientHeight - panel5Height - 5);
                }

                // 2. Determine top and height for middle panels (ultraPanel7 & ultraPanel4)
                int middleTop = _panelTop > 0 ? _panelTop : 162;
                int bottomTop = (ultraPanel5 != null) ? ultraPanel5.Top : (clientHeight - 95);
                int middleHeight = Math.Max(100, bottomTop - middleTop - 5);

                // 3. Position ultraPanel7 (PO Panel if visible) and ultraPanel4 (Item Information Panel)
                int itemPanelLeft = visible ? _itemPanelVisibleLeft : _poPanelLeft;
                int itemPanelWidth = Math.Max(0, clientWidth - _rightMargin - itemPanelLeft);

                ultraPanel7.Left = _poPanelLeft;
                ultraPanel7.Top = middleTop;
                ultraPanel7.Width = _poPanelWidth;
                ultraPanel7.Height = middleHeight;
                ultraPanel7.Visible = visible;

                label9.Location = new Point(_poPanelLeft + _label9OffsetX, _labelOffsetY);
                label9.Visible = visible;

                ultraPanel4.Left = itemPanelLeft;
                ultraPanel4.Top = middleTop;
                ultraPanel4.Width = itemPanelWidth;
                ultraPanel4.Height = middleHeight;
                label10.Location = new Point(itemPanelLeft + _label10OffsetX, _labelOffsetY);
            }
            finally
            {
                if (ultraPanel5 != null) ultraPanel5.ResumeLayout();
                ultraPanel4.ResumeLayout();
                ultraPanel7.ResumeLayout();
                ultraPanel1.ResumeLayout();
            }

            if (ultraPanel5 != null) ultraPanel5.PerformLayout();
            ultraPanel4.PerformLayout();
            ultraPanel7.PerformLayout();
            ultraPanel1.PerformLayout();
            UpdateFooterCellPositions();
            UpdateUltraGrid2FooterCellPositions();
        }

        private void RegisterActionPanel(Infragistics.Win.Misc.UltraPanel panel, Action clickAction)
        {
            if (panel == null)
                return;

            panel.UseAppStyling = false;
            panel.Cursor = Cursors.Hand;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;
            ApplyActionPanelStyle(panel, false, false);

            EventHandler clickHandler = (s, e) => clickAction?.Invoke();
            EventHandler mouseEnterHandler = (s, e) => ApplyActionPanelStyle(panel, true, false);
            EventHandler mouseLeaveHandler = (s, e) =>
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyActionPanelStyle(panel, isInside, false);
            };
            MouseEventHandler mouseDownHandler = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ApplyActionPanelStyle(panel, true, true);
                }
            };
            MouseEventHandler mouseUpHandler = (s, e) =>
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyActionPanelStyle(panel, isInside, false);
            };

            panel.Click += clickHandler;
            panel.MouseEnter += mouseEnterHandler;
            panel.MouseLeave += mouseLeaveHandler;
            panel.MouseDown += mouseDownHandler;
            panel.MouseUp += mouseUpHandler;

            panel.ClientArea.Click += clickHandler;
            panel.ClientArea.MouseEnter += mouseEnterHandler;
            panel.ClientArea.MouseLeave += mouseLeaveHandler;
            panel.ClientArea.MouseDown += mouseDownHandler;
            panel.ClientArea.MouseUp += mouseUpHandler;

            foreach (Control child in panel.ClientArea.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += clickHandler;
                child.MouseEnter += mouseEnterHandler;
                child.MouseLeave += mouseLeaveHandler;
                child.MouseDown += mouseDownHandler;
                child.MouseUp += mouseUpHandler;

                if (child is Label label)
                {
                    label.ForeColor = ButtonTextBlue;
                    label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    label.BackColor = Color.Transparent;
                }
            }
        }

        private static void ApplyActionPanelStyle(Infragistics.Win.Misc.UltraPanel panel, bool isHover, bool isPressed)
        {
            if (panel == null)
                return;

            AppearanceBase appearance = panel.Appearance;
            appearance.BackGradientStyle = GradientStyle.Vertical;
            appearance.BorderColor = ButtonBlueBorder;
            appearance.ForeColor = ButtonTextBlue;

            if (isPressed)
            {
                appearance.BackColor = PanelPressedTopColor;
                appearance.BackColor2 = PanelPressedBottomColor;
            }
            else if (isHover)
            {
                appearance.BackColor = PanelHoverTopColor;
                appearance.BackColor2 = PanelHoverBottomColor;
            }
            else
            {
                appearance.BackColor = ButtonBlueTop;
                appearance.BackColor2 = ButtonBlueBottom;
            }
        }

        private static void StyleSectionLabels(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Label label)
                {
                    label.ForeColor = Color.Black;
                    label.BackColor = Color.Transparent;
                    if (label.Font.Size < 11F)
                    {
                        label.Font = new Font("Tahoma", 9F, label.Font.Style, GraphicsUnit.Point, 0);
                    }
                }
            }
        }

        private void ConfigureUltraGrid2Theme()
        {
            ApplyGridTheme(ultraGrid2);
            RefreshGridColumnTheme(ultraGrid2);
        }

        private static void ApplyGridTheme(UltraGrid grid)
        {
            if (grid == null)
                return;

            grid.UseAppStyling = false;
            grid.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = grid.DisplayLayout;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Appearance.BackColor = Color.White;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Appearance.BorderColor = BorderBlue;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.True;
            layout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            // Use Standard style so custom BackColor/gradient settings are respected (not overridden by OS theme)
            layout.Override.HeaderStyle = HeaderStyle.Standard;

            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 19;
            layout.Override.DefaultRowHeight = 19;
            layout.RowConnectorStyle = RowConnectorStyle.Solid;
            layout.RowConnectorColor = GridRowLine;
            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            grid.BackColor = FormBackColor;
            grid.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void RefreshGridColumnTheme(UltraGrid grid)
        {
            if (grid == null || grid.DisplayLayout == null || grid.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = grid.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Header.Appearance.BackColor = GridHeaderBlue;
                column.Header.Appearance.BackColor2 = GridHeaderBlueDark;
                column.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                column.Header.Appearance.ForeColor = Color.White;
                column.Header.Appearance.BorderColor = BorderBlue;
                column.Header.Appearance.FontData.Bold = DefaultableBoolean.False;
                column.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                column.Header.Appearance.FontData.SizeInPoints = 8.25F;

                column.CellAppearance.BorderColor = GridRowLine;
                column.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
                column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                column.CellAppearance.FontData.SizeInPoints = 8.25F;
            }
        }

        private void SetupUltraGrid2Footer()
        {
            if (ultraGrid2FooterPanel == null)
                return;

            const int footerHeight = 24;
            if (ultraGrid2.Height > footerHeight + 60)
            {
                ultraGrid2.Height -= footerHeight;
            }

            ultraGrid2FooterPanel.Appearance.BackColor = GridHeaderBlue;
            ultraGrid2FooterPanel.Appearance.BackColor2 = GridHeaderBlue;
            ultraGrid2FooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2FooterPanel.Appearance.BorderColor = GridFooterBorder;
            ultraGrid2FooterPanel.BorderStyle = UIElementBorderStyle.Solid;
            ultraGrid2FooterPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            ultraGrid2FooterPanel.Left = ultraGrid2.Left;
            ultraGrid2FooterPanel.Width = ultraGrid2.Width;
            ultraGrid2FooterPanel.Height = footerHeight;
            ultraGrid2FooterPanel.Top = ultraGrid2.Bottom;
            ultraGrid2FooterPanel.Visible = true;
            ultraGrid2FooterPanel.BringToFront();

            InitializeUltraGrid2Footer();
        }

        private void UltraGrid2_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            ApplyGridTheme(ultraGrid2);

            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            string[] visibleColumns = { "SLNO", "BarCode", "Description", "Unit", "Packing", "Cost", "Qty", "TaxPer", "TaxAmt", "NetAmt" };
            int visiblePosition = 0;
            foreach (string columnKey in visibleColumns)
            {
                if (!band.Columns.Exists(columnKey))
                    continue;

                UltraGridColumn column = band.Columns[columnKey];
                column.Hidden = false;
                column.Header.VisiblePosition = visiblePosition++;
                column.Header.Appearance.BorderColor = GridRowLine;
                column.CellAppearance.BorderColor = GridRowLine;
                column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                column.CellAppearance.FontData.SizeInPoints = 8.25F;
            }

            if (band.Columns.Exists("SLNO")) band.Columns["SLNO"].Width = 48;
            if (band.Columns.Exists("BarCode")) band.Columns["BarCode"].Width = 95;
            if (band.Columns.Exists("Description")) band.Columns["Description"].Width = 180;
            if (band.Columns.Exists("Unit")) band.Columns["Unit"].Width = 62;
            if (band.Columns.Exists("Packing")) band.Columns["Packing"].Width = 70;
            if (band.Columns.Exists("Cost")) band.Columns["Cost"].Width = 72;
            if (band.Columns.Exists("Qty")) band.Columns["Qty"].Width = 62;
            if (band.Columns.Exists("TaxPer")) band.Columns["TaxPer"].Width = 62;
            if (band.Columns.Exists("TaxAmt")) band.Columns["TaxAmt"].Width = 72;
            if (band.Columns.Exists("NetAmt")) band.Columns["NetAmt"].Width = 82;

            InitializeUltraGrid2Footer();
        }

        private void InitializeUltraGrid2Footer()
        {
            if (ultraGrid2FooterPanel == null)
                return;

            ultraGrid2FooterPanel.ClientArea.Controls.Clear();
            ultraGrid2FooterLabels.Clear();
            CreateUltraGrid2FooterCells();
            UpdateUltraGrid2FooterCellPositions();
            UpdateUltraGrid2FooterValues();
        }

        private void CreateUltraGrid2FooterCells()
        {
            if (ultraGrid2.DisplayLayout == null || ultraGrid2.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGrid2.DisplayLayout.Bands[0];
            int xOffset = ultraGrid2.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label();
                footerLabel.Name = "footer2_" + column.Key;
                footerLabel.TextAlign = ContentAlignment.MiddleCenter;
                footerLabel.BackColor = GridHeaderBlue;
                footerLabel.ForeColor = Color.White;
                footerLabel.BorderStyle = BorderStyle.None;
                footerLabel.AutoSize = false;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraGrid2FooterPanel.Height - 2, 20);
                footerLabel.Left = xOffset;
                footerLabel.Top = 1;
                footerLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

                ultraGrid2FooterPanel.ClientArea.Controls.Add(footerLabel);
                ultraGrid2FooterLabels[column.Key] = footerLabel;
                xOffset += column.Width;
            }
        }

        private void UpdateUltraGrid2FooterCellPositions()
        {
            if (ultraGrid2FooterPanel == null || ultraGrid2.DisplayLayout == null || ultraGrid2.DisplayLayout.Bands.Count == 0 || ultraGrid2FooterLabels.Count == 0)
                return;

            ultraGrid2FooterPanel.Left = ultraGrid2.Left;
            ultraGrid2FooterPanel.Top = ultraGrid2.Bottom;
            ultraGrid2FooterPanel.Width = ultraGrid2.Width;

            int xOffset = ultraGrid2.DisplayLayout.Override.RowSelectorWidth;
            foreach (UltraGridColumn column in ultraGrid2.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !ultraGrid2FooterLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = ultraGrid2FooterLabels[column.Key];
                footerLabel.Left = xOffset;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraGrid2FooterPanel.Height - 2, 20);
                xOffset += column.Width;
            }
        }

        private void UpdateUltraGrid2FooterValues()
        {
            foreach (KeyValuePair<string, Label> entry in ultraGrid2FooterLabels)
            {
                entry.Value.Text = string.Empty;
            }

            if (ultraGrid2.Rows == null || ultraGrid2.Rows.Count == 0 || ultraGrid2.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridColumn firstVisibleColumn = ultraGrid2.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>()
                .Where(c => !c.Hidden)
                .OrderBy(c => c.Header.VisiblePosition)
                .FirstOrDefault();

            if (firstVisibleColumn != null && ultraGrid2FooterLabels.ContainsKey(firstVisibleColumn.Key))
            {
                ultraGrid2FooterLabels[firstVisibleColumn.Key].Text = ultraGrid2.Rows.Count.ToString();
            }
        }

        private void FrmPurchase_Load(object sender, EventArgs e)
        {
            //PurchaseInvoiceRepository ObjPurchaseRepo = new PurchaseInvoiceRepository();

            // New purchases must always start with the current system date.
            DateTime systemDate = DateTime.Today;
            DtpInoviceDate.Value = systemDate;
            dtpPurchaseDate.Value = systemDate;

            // Hide update button initially
            ultraPictureBox4.Visible = false;

            // Clear textBox1 to remove any default text like "ModelClass.VendorDDLG"
            textBox1.Text = "";

            BranchDDlGrid branchDDl = drop.getBanchDDl();
            CmboBranch.DataSource = branchDDl.List;
            CmboBranch.DisplayMember = "BranchName";
            CmboBranch.ValueMember = "Id";

            // Set the selected branch to match SessionContext if available
            if (SessionContext.IsInitialized && SessionContext.BranchId > 0)
            {
                try
                {
                    CmboBranch.SelectedValue = SessionContext.BranchId;
                }
                catch
                {
                    // If branch not found in list, select first item
                    if (CmboBranch.Items.Count > 0)
                    {
                        CmboBranch.SelectedIndex = 0;
                    }
                }
            }

            // Add event handler for branch change to regenerate purchase number and update SessionContext
            CmboBranch.SelectedValueChanged += CmboBranch_SelectedValueChanged;

            PaymodeDDlGrid ObjPayModeDDL = drop.GetPaymode();
            CmboPayment.DataSource = (ObjPayModeDDL.List ?? Enumerable.Empty<PaymodeDDl>())
                .Where(payMode =>
                    string.Equals(payMode.PayModeName, "Cash", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(payMode.PayModeName, "Credit", StringComparison.OrdinalIgnoreCase))
                .ToList();
            CmboPayment.DisplayMember = "PayModeName";
            CmboPayment.ValueMember = "PayModeID";
            SetDefaultPaymentMode();

            VendorDDLGrids VendorDDLGrids = drop.VendorDDL();
            CmboVendor.DataSource = VendorDDLGrids.List;
            CmboVendor.DisplayMember = "LedgerName";
            CmboVendor.ValueMember = "LedgerID";

            // Initialize comboBox1 with TaxType options
            comboBox1.DataSource = new[] { "ALL", "incl", "excl" };
            comboBox1.Value = "ALL";

            // Generate and display purchase number dynamically
            GenerateAndDisplayPurchaseNumber();

            ConfigureItemsGridLayout();
            ConfigureUltraGrid2Theme();
            SetupGridDocking();
            SetupRowFooter(); // Setup the row footer using gridFooterPanel
            SetupUltraGrid2Footer();

            // Load saved user layout customizations
            LoadGridLayout();

            // Initialize saved column widths
            InitializeSavedColumnWidths();

            // Add a timer to periodically check and update footer positions
            System.Windows.Forms.Timer footerUpdateTimer = new System.Windows.Forms.Timer();
            footerUpdateTimer.Interval = 500; // Check every half second
            footerUpdateTimer.Tick += (s2, e2) => UpdateFooterCellPositions();
            footerUpdateTimer.Start();

            // Let Windows adjust the layout, then update footer
            this.BeginInvoke(new Action(() =>
            {
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }));

            this.barcodeFocus();

            // Register event handlers for grid
            ultraGrid1.BeforeCellUpdate += UltraGrid1_BeforeCellUpdate;
            ultraGrid1.AfterCellUpdate += UltraGrid1_AfterCellUpdate;
            ultraGrid1.BeforeCellActivate += UltraGrid1_BeforeCellActivate;
            ultraGrid1.KeyDown += UltraGrid1_KeyDown;
            ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout;

            // Manually remove sort indicators after grid is loaded
            RemoveSortIndicators();

            // Add click event for button1 (F11)
            button1.Click += (s, ev) => ShowVendorDialog();
            button3.Click += (s, ev) => ShowPurchaseOrderDialog();
            button4.Click += (s, ev) => LoadAllPurchaseOrderItemsToPurchaseGrid();
            button5.Click += (s, ev) => LoadSelectedPurchaseOrderItemToPurchaseGrid();
            LblPid.Visible = false;
            lblVoucherId.Visible = false;

            // Load custom DS-Digital font from embedded resources for label4 and label6
            try
            {
                Utilities.CustomFontLoader.Initialize();
                System.Drawing.Font dsFont = Utilities.CustomFontLoader.GetDSDigitalFont(36, FontStyle.Bold);

                if (label4 != null) label4.Font = dsFont;
                if (label6 != null) label6.Font = dsFont;
            }
            catch (Exception fontEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load custom DS-Digital font: {fontEx.Message}");
                // Font will fall back to default if loading fails
            }

            // Store default background color
            defaultBackColor = textBox1.Appearance.BackColor;

            // Highlight mandatory fields
            HighlightMandatoryFields();
            InitializeActionPanels();
            SetPurchaseOrderPanelVisibility(false);

            // Style ultraPanel6 to look like a button (preserving current colors)
            if (this.Controls.Find("ultraPanel6", true).Length > 0)
            {
                Infragistics.Win.Misc.UltraPanel panel6 = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel6", true)[0];
                StylePanelAsButton(panel6);

                // Add click event to open Item Master in tab with selected item
                // Make entire panel area clickable
                panel6.Click += UltraPanel6_Click;
                panel6.ClientArea.Click += UltraPanel6_Click;

                // Find and add click events to label30 and ultraPictureBox8
                // Search in both panel.Controls and panel.ClientArea.Controls
                Control label30 = panel6.Controls.Find("label30", true).FirstOrDefault();
                if (label30 == null)
                {
                    label30 = panel6.ClientArea.Controls.Find("label30", true).FirstOrDefault();
                }
                if (label30 != null)
                {
                    label30.Click += UltraPanel6_Click;
                    label30.Cursor = Cursors.Hand;
                }

                Control ultraPictureBox8 = panel6.Controls.Find("ultraPictureBox8", true).FirstOrDefault();
                if (ultraPictureBox8 == null)
                {
                    ultraPictureBox8 = panel6.ClientArea.Controls.Find("ultraPictureBox8", true).FirstOrDefault();
                }
                if (ultraPictureBox8 != null)
                {
                    ultraPictureBox8.Click += UltraPanel6_Click;
                    if (ultraPictureBox8 is Infragistics.Win.UltraWinEditors.UltraPictureBox picBox)
                    {
                        picBox.Cursor = Cursors.Hand;
                    }
                }
            }

            // Apply same hover effect and click function to label1 (synchronized with ultraPanel6)
            // Similar to ultraPanel5 and label20 in frmItemMasterNew.cs
            if (this.Controls.Find("label1", true).Length > 0 && this.Controls.Find("ultraPanel6", true).Length > 0)
            {
                Control label1Control = this.Controls.Find("label1", true)[0];
                Infragistics.Win.Misc.UltraPanel panel6 = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel6", true)[0];

                // Get the current (styled) colors of ultraPanel6 as the base colors
                // These are the colors after StylePanelAsButton has been applied
                Color originalBackColor = panel6.Appearance.BackColor;
                Color originalBackColor2 = panel6.Appearance.BackColor2;

                // If BackColor2 is empty or same as BackColor, create a darker version
                if (originalBackColor2 == Color.Empty || originalBackColor2 == originalBackColor)
                {
                    originalBackColor2 = Color.FromArgb(
                        Math.Max(0, originalBackColor.R - 30),
                        Math.Max(0, originalBackColor.G - 30),
                        Math.Max(0, originalBackColor.B - 30)
                    );
                }

                // Define hover colors - brighter versions of the original colors (same as panel hover)
                Color hoverBackColor = Color.FromArgb(
                    Math.Min(originalBackColor.R + 20, 255),
                    Math.Min(originalBackColor.G + 20, 255),
                    Math.Min(originalBackColor.B + 20, 255));
                Color hoverBackColor2 = Color.FromArgb(
                    Math.Min(originalBackColor2.R + 20, 255),
                    Math.Min(originalBackColor2.G + 20, 255),
                    Math.Min(originalBackColor2.B + 20, 255));

                // When mouse enters label1, change ultraPanel6 appearance (same hover effect)
                label1Control.MouseEnter += (s, ev) =>
                {
                    panel6.Appearance.BackColor = hoverBackColor;
                    panel6.Appearance.BackColor2 = hoverBackColor2;
                    label1Control.Cursor = Cursors.Hand;
                };

                // When mouse leaves label1, restore ultraPanel6 appearance
                // but only if the mouse isn't still over the panel
                label1Control.MouseLeave += (s, ev) =>
                {
                    Point mousePos = panel6.PointToClient(Control.MousePosition);
                    if (!panel6.ClientRectangle.Contains(mousePos))
                    {
                        panel6.Appearance.BackColor = originalBackColor;
                        panel6.Appearance.BackColor2 = originalBackColor2;
                    }
                };

                // Add click event to label1 (same as ultraPanel6)
                label1Control.Click += UltraPanel6_Click;
                label1Control.Cursor = Cursors.Hand;
            }

            // Configure date pickers to match the format in the image
            ConfigureDatePickers();

            // Subscribe to frmItemMasterNew item update event to refresh grid when items are updated
            try
            {
                PosBranch_Win.Master.frmItemMasterNew.OnItemMasterUpdated += OnItemMasterUpdatedHandler;
                // Unsubscribe when form closes to prevent memory leaks
                this.FormClosed += (s, args) =>
                {
                    try
                    {
                        PosBranch_Win.Master.frmItemMasterNew.OnItemMasterUpdated -= OnItemMasterUpdatedHandler;
                    }
                    catch { }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error subscribing to item master update event: {ex.Message}");
            }

            // Subscribe to vendor save event to refresh vendor dropdown when a new vendor is created
            try
            {
                PosBranch_Win.Accounts.FrmVendor.OnVendorSaved += RefreshVendorDropdown;
                // Unsubscribe when form closes to prevent memory leaks
                this.FormClosed += (s, args) =>
                {
                    try
                    {
                        PosBranch_Win.Accounts.FrmVendor.OnVendorSaved -= RefreshVendorDropdown;
                    }
                    catch { }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error subscribing to vendor save event: {ex.Message}");
            }

            // Wire up keyboard shortcut buttons (Click events)
            // Note: Click events for pbxExit, ultraPictureBox1, pbxSave, ultraPictureBox2
            // are already wired in the constructor — do NOT wire them again here

            // Set initial focus to barcode field after form is fully rendered
            this.Shown += (s, args) =>
            {
                this.ActiveControl = txtBarcode;
                txtBarcode.Focus();
                // Use BeginInvoke to ensure focus after any pending UI updates
                txtBarcode.BeginInvoke(new Action(() =>
                {
                    txtBarcode.Clear();
                    txtBarcode.Focus();
                    txtBarcode.SelectionStart = txtBarcode.Text.Length;
                }));
            };
        }

        // Method to configure date pickers to match the format in the image
        private void ConfigureDatePickers()
        {
            // Apply custom format to date pickers (14/May/2025 format)
            dtpPurchaseDate.FormatString = "dd/MMM/yyyy";
            dtpPurchaseDate.MaskInput = "{date}";

            DtpInoviceDate.FormatString = "dd/MMM/yyyy";
            DtpInoviceDate.MaskInput = "{date}";

            // Set calendar font to match the image
            try
            {
                dtpPurchaseDate.Appearance.FontData.SizeInPoints = 9F;
                DtpInoviceDate.Appearance.FontData.SizeInPoints = 9F;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error configuring date picker appearance: " + ex.Message);
            }
        }

        // Method to style a panel as a button while preserving its current colors
        private void StylePanelAsButton(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;

            // Get current colors from the panel
            Color currentBackColor = panel.Appearance.BackColor;
            Color currentBackColor2 = panel.Appearance.BackColor2;
            Color currentBorderColor = panel.Appearance.BorderColor;

            // If BackColor2 is not set or same as BackColor, create a slightly darker version for gradient
            if (currentBackColor2 == Color.Empty || currentBackColor2 == currentBackColor)
            {
                // Create a slightly darker version for gradient effect
                currentBackColor2 = Color.FromArgb(
                    Math.Max(0, currentBackColor.R - 30),
                    Math.Max(0, currentBackColor.G - 30),
                    Math.Max(0, currentBackColor.B - 30)
                );
            }

            // If border color is not set, use a slightly darker version of back color
            if (currentBorderColor == Color.Empty)
            {
                currentBorderColor = Color.FromArgb(
                    Math.Max(0, currentBackColor.R - 40),
                    Math.Max(0, currentBackColor.G - 40),
                    Math.Max(0, currentBackColor.B - 40)
                );
            }

            // Store original colors for hover effect
            Color originalBackColor = currentBackColor;
            Color originalBackColor2 = currentBackColor2;

            // Create a gradient using current colors
            panel.Appearance.BackColor = currentBackColor;
            panel.Appearance.BackColor2 = currentBackColor2;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Set rounded border style to make it look like a button
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;

            // Set border color
            panel.Appearance.BorderColor = currentBorderColor;
            if (panel.Appearance.BorderColor3DBase == Color.Empty)
            {
                panel.Appearance.BorderColor3DBase = Color.FromArgb(
                    Math.Max(0, currentBorderColor.R - 20),
                    Math.Max(0, currentBorderColor.G - 20),
                    Math.Max(0, currentBorderColor.B - 20)
                );
            }

            // Ensure icons/labels inside have transparent background
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    Infragistics.Win.UltraWinEditors.UltraPictureBox pic = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;
                }
                else if (control is Label)
                {
                    Label lbl = (Label)control;
                    // Preserve label's current foreground color if it's not default
                    if (lbl.ForeColor == SystemColors.ControlText)
                    {
                        lbl.ForeColor = Color.White; // Default to white for better contrast
                    }
                    lbl.BackColor = Color.Transparent;
                }
            }

            // Create hover effect handler that applies to panel and all child controls
            EventHandler mouseEnterHandler = (sender, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(
                    Math.Min(255, originalBackColor.R + 20),
                    Math.Min(255, originalBackColor.G + 20),
                    Math.Min(255, originalBackColor.B + 20)
                );
                panel.Appearance.BackColor2 = Color.FromArgb(
                    Math.Min(255, originalBackColor2.R + 20),
                    Math.Min(255, originalBackColor2.G + 20),
                    Math.Min(255, originalBackColor2.B + 20)
                );
            };

            EventHandler mouseLeaveHandler = (sender, e) =>
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };

            // Add hover effect to panel
            panel.ClientArea.MouseEnter += mouseEnterHandler;
            panel.ClientArea.MouseLeave += mouseLeaveHandler;

            // Add hover effect to label30 and ultraPictureBox8 specifically
            // Search in both panel.Controls and panel.ClientArea.Controls
            Control label30 = panel.Controls.Find("label30", true).FirstOrDefault();
            if (label30 == null)
            {
                label30 = panel.ClientArea.Controls.Find("label30", true).FirstOrDefault();
            }
            if (label30 != null)
            {
                label30.MouseEnter += mouseEnterHandler;
                label30.MouseLeave += mouseLeaveHandler;
                label30.Cursor = Cursors.Hand;
            }

            Control ultraPictureBox8 = panel.Controls.Find("ultraPictureBox8", true).FirstOrDefault();
            if (ultraPictureBox8 == null)
            {
                ultraPictureBox8 = panel.ClientArea.Controls.Find("ultraPictureBox8", true).FirstOrDefault();
            }
            if (ultraPictureBox8 != null)
            {
                ultraPictureBox8.MouseEnter += mouseEnterHandler;
                ultraPictureBox8.MouseLeave += mouseLeaveHandler;
                if (ultraPictureBox8 is Infragistics.Win.UltraWinEditors.UltraPictureBox picBox)
                {
                    picBox.Cursor = Cursors.Hand;
                }
            }

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
        }

        // Method to style panels like ultraPanel7 from frmdialForItemMaster
        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;

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
                }
                else if (control is Label)
                {
                    ((Label)control).ForeColor = Color.White;
                    ((Label)control).Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    ((Label)control).BackColor = Color.Transparent;
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

        // Method to highlight mandatory fields
        private void HighlightMandatoryFields()
        {
            // Highlight mandatory fields with light reddish-yellow
            SetEditorBackColor(textBox1, mandatoryFieldColor);
            SetComboBackColor(CmboPayment, mandatoryFieldColor);
            SetEditorBackColor(txtInvoiceNo, mandatoryFieldColor);
            SetEditorBackColor(txtBilledBy, mandatoryFieldColor);

            // Check if any fields already have values and set appropriate colors
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
                SetEditorBackColor(textBox1, defaultBackColor);

            if (CmboPayment.SelectedIndex != -1)
                SetComboBackColor(CmboPayment, defaultBackColor);

            if (!string.IsNullOrWhiteSpace(txtInvoiceNo.Text))
                SetEditorBackColor(txtInvoiceNo, defaultBackColor);

            if (!string.IsNullOrWhiteSpace(txtBilledBy.Text))
                SetEditorBackColor(txtBilledBy, defaultBackColor);
        }

        private void SetFormReadOnly(bool readOnly)
        {
            _isReadOnly = readOnly;

            // Disable/Enable input controls
            txtBarcode.Enabled = !readOnly;
            txtInvoiceNo.Enabled = !readOnly;
            txtBilledBy.Enabled = !readOnly;
            txtRoundOff.Enabled = !readOnly;
            txtRemark.Enabled = !readOnly;
            textBox1.Enabled = !readOnly;

            CmboVendor.Enabled = !readOnly;
            CmboPayment.Enabled = !readOnly;
            CmboBranch.Enabled = !readOnly;

            DtpInoviceDate.Enabled = !readOnly;
            dtpPurchaseDate.Enabled = !readOnly;

            // Enable / Disable lookup/action helper buttons
            button1.Enabled = !readOnly; // F11 vendor
            button3.Enabled = !readOnly; // F6 lookup

            // Hide/Show Save, Update, Delete, Exit/Remove, and Clear grid buttons
            ultraPictureBox2.Visible = !readOnly; // Delete button
            pbxExit.Visible = !readOnly;         // Remove selected item

            Control clearGrid1Btn = this.Controls.Find("button6", true).FirstOrDefault();
            if (clearGrid1Btn != null)
            {
                clearGrid1Btn.Enabled = !readOnly;
            }
            Control clearGrid2Btn = this.Controls.Find("button7", true).FirstOrDefault();
            if (clearGrid2Btn != null)
            {
                clearGrid2Btn.Enabled = !readOnly;
            }

            if (ultraGrid1 != null && ultraGrid1.DisplayLayout != null && ultraGrid1.DisplayLayout.Override != null)
            {
                ultraGrid1.DisplayLayout.Override.AllowUpdate = readOnly
                    ? Infragistics.Win.DefaultableBoolean.False
                    : Infragistics.Win.DefaultableBoolean.True;
            }
        }

        private static void SetEditorBackColor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor, Color color)
        {
            if (editor == null)
                return;

            editor.Appearance.BackColor = color;
            editor.BackColor = color;
        }

        private static void SetComboBackColor(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, Color color)
        {
            if (combo == null)
                return;

            combo.Appearance.BackColor = color;
            combo.BackColor = color;
        }

        // Event handler for mandatory fields changes
        private void MandatoryField_Changed(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                // For TextBox controls
                if (control is Infragistics.Win.UltraWinEditors.UltraTextEditor)
                {
                    Infragistics.Win.UltraWinEditors.UltraTextEditor editor = control as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    SetEditorBackColor(editor, !string.IsNullOrWhiteSpace(editor.Text) ? defaultBackColor : mandatoryFieldColor);
                }
                else if (control is Infragistics.Win.UltraWinEditors.UltraComboEditor)
                {
                    Infragistics.Win.UltraWinEditors.UltraComboEditor combo = control as Infragistics.Win.UltraWinEditors.UltraComboEditor;
                    SetComboBackColor(combo, combo.SelectedIndex != -1 ? defaultBackColor : mandatoryFieldColor);
                }
            }
        }

        private void ConfigureItemsGridLayout()
        {
            try
            {
                // Make sure the grid is properly initialized
                if (ultraGrid1 == null || ultraGrid1.DisplayLayout == null)
                    return;

                // Create a DataTable to hold grid data
                DataTable dt = new DataTable();

                // Add columns to match the requested order
                dt.Columns.Add("SLNO", typeof(int));
                dt.Columns.Add("BarCode", typeof(string));
                dt.Columns.Add("Description", typeof(string));
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("Packing", typeof(float));
                dt.Columns.Add("RetailPrice", typeof(float));
                dt.Columns.Add("Free", typeof(float));
                dt.Columns.Add("SellingPrice", typeof(float));
                dt.Columns.Add("UnitSP", typeof(float)); // Unit Selling Price (Unit 1's wholesale price)
                dt.Columns.Add("BaseCost", typeof(float));
                dt.Columns.Add("Cost", typeof(float));
                dt.Columns.Add("Qty", typeof(float));
                // Amount and TotalAmount columns removed - not needed in grid
                dt.Columns.Add("Gross", typeof(float)); // Gross = Amount - TaxAmt (for incl tax type)
                dt.Columns.Add("NetAmt", typeof(float)); // Net Amount = Amount + TaxAmt (for excl tax type)
                dt.Columns.Add("NewBaseCost", typeof(float)); // New BaseCost = average of all rows' Cost values

                // Hidden columns needed for operations but not displayed
                dt.Columns.Add("ItemId", typeof(int));
                dt.Columns.Add("UnitId", typeof(int));
                dt.Columns.Add("UnitPrize", typeof(float));
                dt.Columns.Add("MarginPer", typeof(float));
                dt.Columns.Add("MarginAmt", typeof(float));
                dt.Columns.Add("TaxPer", typeof(float));
                dt.Columns.Add("TaxAmt", typeof(float));
                dt.Columns.Add("TaxType", typeof(string));
                dt.Columns.Add("WholeSalePrice", typeof(float));
                dt.Columns.Add("CreditPrice", typeof(float));
                dt.Columns.Add("CardPrice", typeof(float));
                dt.Columns.Add("ItemStatus", typeof(string));
                dt.Columns.Add("StatusReason", typeof(string));
                dt.Columns.Add("StatusDate", typeof(DateTime));
                dt.Columns.Add("BlockSale", typeof(bool));
                dt.Columns.Add("BlockPurchase", typeof(bool));

                // Set the DataTable as the grid's data source
                ultraGrid1.DataSource = dt;

                // Reset everything first to ensure clean slate
                ultraGrid1.DisplayLayout.Reset();
                ApplyGridTheme(ultraGrid1);

                Color lightBlue = GridRowLine;
                Color headerBlue = GridHeaderBlue;

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Enable column moving and dragging
                ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = AllowColSwapping.WithinBand;



                // Apply grid spacing and appearance - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 19;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 19;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0; // Remove row spacing
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Appearance.BorderColor = lightBlue; // Set the main grid border to light blue

                // Configure header appearance with solid color matching FrmPurchaseDisplayDialog.cs
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False; // Change from bold to regular
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;

                // Set font weight for all cells to regular (not bold)
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Bold = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 8.25F;

                // Set row selector appearance to match header with solid color matching FrmPurchaseDisplayDialog.cs
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 20;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

                // Configure row appearance like frmPurchaseOrder
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = GridAltRow;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                Color selectedRowBlue = GridSelectedBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectedRowBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = selectedRowBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;

                // Active row appearance - make it same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectedRowBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = selectedRowBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

                // Configure cell and row borders - change from dotted to solid as requested
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;

                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = GridRowLine;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;

                // Configure spacing and expansion behavior
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;


                // Configure columns in the UltraGrid
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                    // Set default vertical alignment for all cells
                    band.Override.CellAppearance.TextVAlign = VAlign.Middle;
                    band.Override.CellAppearance.FontData.Bold = DefaultableBoolean.False;
                    band.Override.CellAppearance.BorderColor = GridRowLine;

                    // Configure each column per the requested order
                    SetupColumn(band.Columns["SLNO"], "SLNO", 60, HAlign.Center, true, false);
                    SetupColumn(band.Columns["BarCode"], "Barcode", 120, HAlign.Center, true, false);
                    SetupColumn(band.Columns["Description"], "Item Name", 200, HAlign.Left, true, false);
                    SetupColumn(band.Columns["Unit"], "Unit", 80, HAlign.Center, true, false);
                    SetupColumn(band.Columns["Packing"], "Packing", 80, HAlign.Right, false, true, true, "N0");
                    SetupColumn(band.Columns["RetailPrice"], "Retail Price", 100, HAlign.Right, false, true, true);
                    SetupColumn(band.Columns["Free"], "Free", 50, HAlign.Right, false, true, true);
                    SetupColumn(band.Columns["SellingPrice"], "Selling Price", 100, HAlign.Right, false, true, true, "N2");
                    SetupColumn(band.Columns["UnitSP"], "Unit SP", 100, HAlign.Right, false, true, true, "N2");
                    SetupColumn(band.Columns["BaseCost"], "Base Cost", 100, HAlign.Right, true, true, false, "N2");
                    SetupColumn(band.Columns["Cost"], "Cost", 100, HAlign.Right, false, true, false, "N2");
                    SetupColumn(band.Columns["Qty"], "Qty", 80, HAlign.Right, false, true);
                    SetupColumn(band.Columns["Gross"], "Gross", 120, HAlign.Right, false, true, true); // Hidden
                    SetupColumn(band.Columns["NetAmt"], "Net Amount", 120, HAlign.Right, false, true, false); // Visible, editable
                    SetupColumn(band.Columns["NewBaseCost"], "New BaseCost", 120, HAlign.Right, true, true, true, "N2"); // Read-only, hidden by default, toggled by F5

                    // Tax columns - hidden by default (in field chooser)
                    SetupColumn(band.Columns["TaxPer"], "Tax %", 80, HAlign.Right, false, true, true, "N2");
                    SetupColumn(band.Columns["TaxAmt"], "Tax Amount", 100, HAlign.Right, false, true, true, "N2");
                    SetupColumn(band.Columns["TaxType"], "Tax Type", 80, HAlign.Center, false, false, true);

                    // Hide all other columns
                    SetupColumn(band.Columns["ItemId"], "ItemId", 0, HAlign.Center, false, false, true);
                    SetupColumn(band.Columns["UnitId"], "UnitId", 0, HAlign.Center, false, false, true);
                    SetupColumn(band.Columns["UnitPrize"], "UnitPrize", 0, HAlign.Right, false, true, true);
                    SetupColumn(band.Columns["MarginPer"], "MarginPer", 0, HAlign.Right, false, true, true);
                    SetupColumn(band.Columns["MarginAmt"], "MarginAmt", 0, HAlign.Right, false, true, true);
                    // Amount and TotalAmount columns removed from grid
                    SetupColumn(band.Columns["WholeSalePrice"], "WholeSalePrice", 0, HAlign.Right, false, true, true);
                    SetupColumn(band.Columns["CreditPrice"], "CreditPrice", 0, HAlign.Right, false, true, true);
                    SetupColumn(band.Columns["CardPrice"], "CardPrice", 0, HAlign.Right, false, true, true);
                    SetupColumn(band.Columns["ItemStatus"], "ItemStatus", 0, HAlign.Left, true, false, true);
                    SetupColumn(band.Columns["StatusReason"], "StatusReason", 0, HAlign.Left, true, false, true);
                    SetupColumn(band.Columns["StatusDate"], "StatusDate", 0, HAlign.Center, true, false, true);
                    SetupColumn(band.Columns["BlockSale"], "BlockSale", 0, HAlign.Center, true, false, true);
                    SetupColumn(band.Columns["BlockPurchase"], "BlockPurchase", 0, HAlign.Center, true, false, true);

                    // Ensure Gross and Net Amount are visually positioned at the very end of the grid
                    band.Columns["Gross"].Header.VisiblePosition = band.Columns.Count - 2;
                    band.Columns["NetAmt"].Header.VisiblePosition = band.Columns.Count - 1;
                }

                RefreshGridColumnTheme(ultraGrid1);

                // Subscribe to InitializeLayout event for consistent styling
                ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout;
                if (!_purchaseStatusRowHandlerAdded)
                {
                    ultraGrid1.InitializeRow += UltraGrid1_InitializeRow;
                    _purchaseStatusRowHandlerAdded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error configuring grid layout: " + ex.Message);
            }
        }

        // Helper method to configure grid columns consistently
        private void SetupColumn(UltraGridColumn column, string caption, int width, HAlign hAlign, bool readOnly, bool isNumeric, bool hidden = false, string format = null)
        {
            if (column == null) return;

            column.Header.Caption = caption;
            column.Width = width;
            column.CellAppearance.TextHAlign = hAlign;
            column.CellAppearance.TextVAlign = VAlign.Middle;
            column.CellAppearance.FontData.Bold = DefaultableBoolean.False;
            column.Header.Appearance.FontData.Bold = DefaultableBoolean.False; // Changed from Bold to regular
            column.CellActivation = readOnly ? Activation.NoEdit : Activation.AllowEdit;
            column.Hidden = hidden;

            if (isNumeric)
            {
                column.Format = format ?? "N2"; // Use provided format or default to "N2"
            }
        }

        private ItemStatusRuleInfo CreateDefaultPurchaseItemStatus(int itemId = 0)
        {
            return new ItemStatusRuleInfo
            {
                ItemId = itemId,
                StatusName = "Active",
                StatusReason = string.Empty,
                StatusDate = DateTime.Today,
                BlockSale = false,
                BlockPurchase = false
            };
        }

        private ItemStatusRuleInfo CreatePurchaseItemStatus(ItemDDl item)
        {
            if (item == null)
            {
                return CreateDefaultPurchaseItemStatus();
            }

            string statusName = Dropdowns.NormalizeItemStatusName(item.ItemStatus);
            return new ItemStatusRuleInfo
            {
                ItemId = item.ItemId,
                StatusName = statusName,
                StatusReason = item.StatusReason ?? string.Empty,
                StatusDate = item.StatusDate,
                BlockSale = item.BlockSale || Dropdowns.DoesStatusBlockSale(statusName),
                BlockPurchase = item.BlockPurchase || Dropdowns.DoesStatusBlockPurchase(statusName)
            };
        }

        private ItemStatusRuleInfo GetPurchaseItemStatus(int itemId)
        {
            if (itemId <= 0)
            {
                return CreateDefaultPurchaseItemStatus(itemId);
            }

            Dropdowns statusDropdown = new Dropdowns();
            return statusDropdown.GetItemStatus(itemId) ?? CreateDefaultPurchaseItemStatus(itemId);
        }

        private string GetPurchaseBlockedMessage(string itemName, ItemStatusRuleInfo status)
        {
            ItemStatusRuleInfo effectiveStatus = status ?? CreateDefaultPurchaseItemStatus();
            string displayName = string.IsNullOrWhiteSpace(itemName) ? "This item" : itemName;
            string message = $"{displayName} is marked as '{Dropdowns.NormalizeItemStatusName(effectiveStatus.StatusName)}' and is blocked for purchase.";

            if (!string.IsNullOrWhiteSpace(effectiveStatus.StatusReason))
            {
                message += $"\n\nReason: {effectiveStatus.StatusReason}";
            }

            return message;
        }

        private bool EnsureItemAllowedForPurchase(ItemDDl item)
        {
            ItemStatusRuleInfo status = CreatePurchaseItemStatus(item);
            if (!status.BlockPurchase)
            {
                return true;
            }

            MessageBox.Show(GetPurchaseBlockedMessage(item?.Description, status), "Blocked Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private bool EnsureItemAllowedForPurchase(int itemId, string itemName, out ItemStatusRuleInfo status)
        {
            status = GetPurchaseItemStatus(itemId);
            if (!status.BlockPurchase)
            {
                return true;
            }

            MessageBox.Show(GetPurchaseBlockedMessage(itemName, status), "Blocked Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private ItemStatusRuleInfo GetPurchaseRowStatus(UltraGridRow row)
        {
            if (row == null)
            {
                return CreateDefaultPurchaseItemStatus();
            }

            int itemId = 0;
            if (row.Cells.Exists("ItemId") && row.Cells["ItemId"].Value != null)
            {
                int.TryParse(row.Cells["ItemId"].Value.ToString(), out itemId);
            }

            string statusName = row.Cells.Exists("ItemStatus")
                ? Dropdowns.NormalizeItemStatusName(row.Cells["ItemStatus"].Value?.ToString())
                : "Active";
            string reason = row.Cells.Exists("StatusReason") ? row.Cells["StatusReason"].Value?.ToString() ?? string.Empty : string.Empty;
            DateTime? statusDate = null;
            if (row.Cells.Exists("StatusDate") &&
                row.Cells["StatusDate"].Value != null &&
                row.Cells["StatusDate"].Value != DBNull.Value)
            {
                statusDate = Convert.ToDateTime(row.Cells["StatusDate"].Value);
            }

            bool blockSale = row.Cells.Exists("BlockSale") &&
                             row.Cells["BlockSale"].Value != null &&
                             row.Cells["BlockSale"].Value != DBNull.Value &&
                             Convert.ToBoolean(row.Cells["BlockSale"].Value);
            bool blockPurchase = row.Cells.Exists("BlockPurchase") &&
                                 row.Cells["BlockPurchase"].Value != null &&
                                 row.Cells["BlockPurchase"].Value != DBNull.Value &&
                                 Convert.ToBoolean(row.Cells["BlockPurchase"].Value);

            return new ItemStatusRuleInfo
            {
                ItemId = itemId,
                StatusName = statusName,
                StatusReason = reason,
                StatusDate = statusDate,
                BlockSale = blockSale,
                BlockPurchase = blockPurchase || Dropdowns.DoesStatusBlockPurchase(statusName)
            };
        }

        private void ApplyItemStatusToPurchaseRow(DataRow row, ItemStatusRuleInfo status)
        {
            if (row == null || row.Table == null)
            {
                return;
            }

            ItemStatusRuleInfo effectiveStatus = status ?? CreateDefaultPurchaseItemStatus();
            if (row.Table.Columns.Contains("ItemStatus"))
            {
                row["ItemStatus"] = Dropdowns.NormalizeItemStatusName(effectiveStatus.StatusName);
            }

            if (row.Table.Columns.Contains("StatusReason"))
            {
                row["StatusReason"] = effectiveStatus.StatusReason ?? string.Empty;
            }

            if (row.Table.Columns.Contains("StatusDate"))
            {
                row["StatusDate"] = effectiveStatus.StatusDate.HasValue ? (object)effectiveStatus.StatusDate.Value.Date : DBNull.Value;
            }

            if (row.Table.Columns.Contains("BlockSale"))
            {
                row["BlockSale"] = effectiveStatus.BlockSale;
            }

            if (row.Table.Columns.Contains("BlockPurchase"))
            {
                row["BlockPurchase"] = effectiveStatus.BlockPurchase;
            }
        }

        private void ApplyItemStatusesToPurchaseGrid()
        {
            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt == null)
            {
                return;
            }

            Dropdowns statusDropdown = new Dropdowns();
            statusDropdown.ApplyItemStatuses(dt);

            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.Rows.Refresh(RefreshRow.FireInitializeRow);
            }
        }

        private void ApplyPurchaseRowVisualState(UltraGridRow row)
        {
            if (row == null)
            {
                return;
            }

            ItemStatusRuleInfo status = GetPurchaseRowStatus(row);
            bool isBlocked = status.BlockPurchase;
            string itemName = row.Cells.Exists("Description") ? row.Cells["Description"].Value?.ToString() ?? string.Empty : string.Empty;
            string toolTip = isBlocked ? GetPurchaseBlockedMessage(itemName, status) : string.Empty;

            row.Appearance.BackColor = isBlocked ? Color.FromArgb(255, 239, 230) : Color.Empty;
            row.Appearance.BackColor2 = Color.Empty;
            row.Appearance.ForeColor = isBlocked ? Color.DarkOrange : Color.Empty;
            row.Appearance.FontData.Bold = isBlocked ? DefaultableBoolean.True : DefaultableBoolean.False;

            foreach (UltraGridCell cell in row.Cells)
            {
                cell.ToolTipText = toolTip;
                cell.Appearance.BackColor = Color.Empty;
                cell.Appearance.ForeColor = Color.Empty;
                cell.Appearance.FontData.Bold = DefaultableBoolean.False;

                if (isBlocked)
                {
                    cell.Activation = Activation.NoEdit;
                    cell.Appearance.ForeColor = Color.DarkOrange;
                }
                else
                {
                    cell.Activation = row.Band.Columns[cell.Column.Key].CellActivation;
                }
            }
        }

        private bool EnsurePurchaseRowEditable(UltraGridRow row, bool showMessage)
        {
            if (_isReadOnly)
            {
                if (showMessage)
                {
                    MessageBox.Show(MsgReadOnlyBlock, MsgReadOnlyModeTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }

            ItemStatusRuleInfo status = GetPurchaseRowStatus(row);
            if (!status.BlockPurchase)
            {
                return true;
            }

            if (showMessage)
            {
                string itemName = row != null && row.Cells.Exists("Description")
                    ? row.Cells["Description"].Value?.ToString() ?? string.Empty
                    : string.Empty;
                MessageBox.Show(GetPurchaseBlockedMessage(itemName, status), "Blocked Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        private void UltraGrid1_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            ApplyPurchaseRowVisualState(e?.Row);
        }

        private bool IsLockedPurchaseGridColumn(string columnKey)
        {
            return string.Equals(columnKey, "BarCode", StringComparison.OrdinalIgnoreCase)
                || string.Equals(columnKey, "Description", StringComparison.OrdinalIgnoreCase)
                || string.Equals(columnKey, "Unit", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyLockedPurchaseGridColumns(UltraGridBand band)
        {
            if (band == null)
            {
                return;
            }

            string[] lockedColumns = { "BarCode", "Description", "Unit" };
            foreach (string columnKey in lockedColumns)
            {
                if (band.Columns.Exists(columnKey))
                {
                    band.Columns[columnKey].CellActivation = Activation.NoEdit;
                    band.Columns[columnKey].CellClickAction = CellClickAction.RowSelect;
                }
            }
        }

        private void UltraGrid1_BeforeCellActivate(object sender, CancelableCellEventArgs e)
        {
            if (_isRedirectingLockedCellActivation || e?.Cell == null || !IsLockedPurchaseGridColumn(e.Cell.Column.Key))
            {
                return;
            }

            e.Cancel = true;

            if (ultraGrid1 != null && ultraGrid1.ActiveCell != null && ultraGrid1.ActiveCell.IsInEditMode)
            {
                ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
            }

            int targetRowIndex = e.Cell.Row?.Index ?? -1;
            this.BeginInvoke(new Action(() =>
            {
                if (_isRedirectingLockedCellActivation || ultraGrid1 == null || ultraGrid1.IsDisposed)
                {
                    return;
                }

                if (targetRowIndex < 0 || targetRowIndex >= ultraGrid1.Rows.Count)
                {
                    return;
                }

                _isRedirectingLockedCellActivation = true;
                try
                {
                    if (ultraGrid1.ActiveCell != null && ultraGrid1.ActiveCell.IsInEditMode)
                    {
                        ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                    }

                    UltraGridRow targetRow = ultraGrid1.Rows[targetRowIndex];
                    ultraGrid1.ActiveRow = targetRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(targetRow);

                    UltraGridCell preferredCell = null;
                    if (targetRow.Cells.Exists("Qty") && !targetRow.Cells["Qty"].Column.Hidden &&
                        targetRow.Cells["Qty"].Column.CellActivation == Activation.AllowEdit)
                    {
                        preferredCell = targetRow.Cells["Qty"];
                    }

                    if (preferredCell == null)
                    {
                        preferredCell = FindFirstEditableCell(targetRow);
                    }

                    if (preferredCell != null)
                    {
                        ultraGrid1.ActiveCell = preferredCell;
                    }
                }
                finally
                {
                    _isRedirectingLockedCellActivation = false;
                }
            }));
        }

        private bool TryGetSelectedUnitFromDialogTag(object dialogTag, out string selectedUnit, out decimal iuRate)
        {
            selectedUnit = string.Empty;
            iuRate = 0;

            string tagValue = dialogTag?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tagValue))
            {
                return false;
            }

            if (tagValue.Contains("|"))
            {
                string[] parts = tagValue.Split('|');
                selectedUnit = parts[0];
                if (parts.Length > 1)
                {
                    decimal.TryParse(parts[1], out iuRate);
                }
            }
            else
            {
                selectedUnit = tagValue;
            }

            return !string.IsNullOrWhiteSpace(selectedUnit);
        }

        private void OpenUnitDialogForPurchaseRow(UltraGridRow targetRow, bool refocusBarcodeAfterClose)
        {
            try
            {
                if (targetRow == null)
                {
                    return;
                }

                if (!EnsurePurchaseRowEditable(targetRow, true))
                {
                    return;
                }

                int itemId = 0;
                if (targetRow.Cells.Exists("ItemId") && targetRow.Cells["ItemId"].Value != null)
                {
                    int.TryParse(targetRow.Cells["ItemId"].Value.ToString(), out itemId);
                }

                if (itemId <= 0)
                {
                    return;
                }

                frmUnitDialog uomDialog = new frmUnitDialog("frmSalesInvoice", itemId);
                if (uomDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string selectedUnit;
                decimal iuRate;
                if (!TryGetSelectedUnitFromDialogTag(uomDialog.Tag, out selectedUnit, out iuRate))
                {
                    return;
                }

                ApplySelectedUnitToPurchaseRow(targetRow, itemId, selectedUnit, iuRate);
            }
            finally
            {
                if (refocusBarcodeAfterClose)
                {
                    this.barcodeFocus();
                }
            }
        }

        private void ApplySelectedUnitToPurchaseRow(UltraGridRow targetRow, int itemId, string selectedUnit, decimal iuRate)
        {
            if (targetRow == null || itemId <= 0 || string.IsNullOrWhiteSpace(selectedUnit))
            {
                return;
            }

            string originalUnit = targetRow.Cells.Exists("Unit") ? targetRow.Cells["Unit"].Value?.ToString() ?? string.Empty : string.Empty;
            if (selectedUnit.Equals(originalUnit, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            float originalPacking = 0;
            float.TryParse(targetRow.Cells["Packing"].Value?.ToString(), out originalPacking);
            float originalCost = 0;
            float.TryParse(targetRow.Cells["Cost"].Value?.ToString(), out originalCost);
            float originalRetailPrice = 0;
            float.TryParse(targetRow.Cells["RetailPrice"].Value?.ToString(), out originalRetailPrice);
            float originalQty = 0;
            float.TryParse(targetRow.Cells["Qty"].Value?.ToString(), out originalQty);

            ultraGrid1.ActiveRow = targetRow;
            ultraGrid1.Selected.Rows.Clear();
            ultraGrid1.Selected.Rows.Add(targetRow);

            bool unitDataFound = false;
            _isUpdatingUnit = true;
            _isApplyingUnitSelection = true;

            try
            {
                if (targetRow.Cells.Exists("Unit"))
                {
                    targetRow.Cells["Unit"].Value = selectedUnit;
                }

                ItemDDlGrid itemData = drop.GetItemUnits(itemId);
                if (itemData != null && itemData.List != null && itemData.List.Any())
                {
                    var unitData = itemData.List.FirstOrDefault(u =>
                        u.Unit != null && u.Unit.Equals(selectedUnit, StringComparison.OrdinalIgnoreCase));

                    if (unitData != null)
                    {
                        unitDataFound = true;

                        if (targetRow.Cells.Exists("UnitId"))
                        {
                            targetRow.Cells["UnitId"].Value = unitData.UnitId;
                        }

                        float newPacking = (float)unitData.Packing;
                        targetRow.Cells["Packing"].Value = newPacking;

                        float newCost = (float)unitData.Cost;
                        if (newCost <= 0 && originalPacking > 0)
                        {
                            float packingRatio = newPacking / originalPacking;
                            newCost = originalCost * packingRatio;
                        }
                        targetRow.Cells["Cost"].Value = newCost;

                        float newRetailPrice = (float)unitData.RetailPrice;
                        if (newRetailPrice <= 0 && originalPacking > 0)
                        {
                            float packingRatio = newPacking / originalPacking;
                            newRetailPrice = originalRetailPrice * packingRatio;
                        }
                        targetRow.Cells["RetailPrice"].Value = newRetailPrice;

                        float sellingPriceValue = 0;
                        if (iuRate > 0)
                        {
                            sellingPriceValue = (float)iuRate;
                        }
                        else
                        {
                            sellingPriceValue = GetWholeSalePriceFromPriceSettings(itemId, unitData.UnitId);
                            if (sellingPriceValue <= 0)
                            {
                                sellingPriceValue = (float)unitData.WholeSalePrice;
                            }
                        }

                        if (targetRow.Cells.Exists("SellingPrice"))
                        {
                            targetRow.Cells["SellingPrice"].Value = sellingPriceValue;
                        }

                        if (targetRow.Cells.Exists("WholeSalePrice"))
                        {
                            targetRow.Cells["WholeSalePrice"].Value = sellingPriceValue > 0
                                ? sellingPriceValue
                                : (float)unitData.WholeSalePrice;
                        }
                        if (targetRow.Cells.Exists("CreditPrice"))
                        {
                            targetRow.Cells["CreditPrice"].Value = (float)unitData.CreditPrice;
                        }
                        if (targetRow.Cells.Exists("CardPrice"))
                        {
                            targetRow.Cells["CardPrice"].Value = (float)unitData.CardPrice;
                        }
                        if (targetRow.Cells.Exists("UnitPrize"))
                        {
                            targetRow.Cells["UnitPrize"].Value = newRetailPrice;
                        }
                    }
                }

                if (!unitDataFound)
                {
                    DataBase.Operations = "GETALL";
                    ItemDDlGrid allItems = drop.itemDDlGrid("", "");
                    if (allItems != null && allItems.List != null)
                    {
                        var matchingItem = allItems.List
                            .Where(i => i.ItemId == itemId && i.Unit == selectedUnit)
                            .FirstOrDefault();

                        if (matchingItem != null)
                        {
                            unitDataFound = true;

                            if (targetRow.Cells.Exists("UnitId") && matchingItem.UnitId > 0)
                            {
                                targetRow.Cells["UnitId"].Value = matchingItem.UnitId;
                            }

                            float newPacking = (float)matchingItem.Packing;
                            targetRow.Cells["Packing"].Value = newPacking;

                            float newCost = (float)matchingItem.Cost;
                            targetRow.Cells["Cost"].Value = newCost;

                            float newRetailPrice = (float)matchingItem.RetailPrice;
                            targetRow.Cells["RetailPrice"].Value = newRetailPrice;

                            float sellingPriceValue = 0;
                            if (iuRate > 0)
                            {
                                sellingPriceValue = (float)iuRate;
                            }
                            else
                            {
                                sellingPriceValue = GetWholeSalePriceFromPriceSettings(itemId, matchingItem.UnitId);
                                if (sellingPriceValue <= 0)
                                {
                                    sellingPriceValue = (float)matchingItem.WholeSalePrice;
                                }
                            }

                            if (targetRow.Cells.Exists("SellingPrice"))
                            {
                                targetRow.Cells["SellingPrice"].Value = sellingPriceValue;
                            }

                            if (targetRow.Cells.Exists("WholeSalePrice"))
                            {
                                targetRow.Cells["WholeSalePrice"].Value = sellingPriceValue > 0
                                    ? sellingPriceValue
                                    : (float)matchingItem.WholeSalePrice;
                            }
                            if (targetRow.Cells.Exists("CreditPrice"))
                            {
                                targetRow.Cells["CreditPrice"].Value = (float)matchingItem.CreditPrice;
                            }
                            if (targetRow.Cells.Exists("CardPrice"))
                            {
                                targetRow.Cells["CardPrice"].Value = (float)matchingItem.CardPrice;
                            }
                            if (targetRow.Cells.Exists("UnitPrize"))
                            {
                                targetRow.Cells["UnitPrize"].Value = (float)matchingItem.RetailPrice;
                            }
                        }
                    }
                }

                if (!unitDataFound)
                {
                    float newPacking = 1.0f;

                    DataBase.Operations = "Unit";
                    UnitDDlGrid unitGrid = drop.getUnitDDl();
                    if (unitGrid != null && unitGrid.List != null)
                    {
                        var unitInfo = unitGrid.List
                            .Where(u => u.UnitName == selectedUnit)
                            .FirstOrDefault();

                        if (unitInfo != null && unitInfo.Packing > 0)
                        {
                            newPacking = (float)unitInfo.Packing;
                        }
                    }

                    targetRow.Cells["Packing"].Value = newPacking;

                    if (originalPacking > 0)
                    {
                        float packingRatio = newPacking / originalPacking;

                        float newCost = originalCost * packingRatio;
                        targetRow.Cells["Cost"].Value = newCost;

                        float newRetailPrice = originalRetailPrice * packingRatio;
                        targetRow.Cells["RetailPrice"].Value = newRetailPrice;

                        int currentUnitId = 0;
                        if (targetRow.Cells.Exists("UnitId") && targetRow.Cells["UnitId"].Value != null)
                        {
                            int.TryParse(targetRow.Cells["UnitId"].Value.ToString(), out currentUnitId);
                        }

                        float sellingPriceValue = 0;
                        if (iuRate > 0)
                        {
                            sellingPriceValue = (float)iuRate;
                        }
                        else
                        {
                            sellingPriceValue = GetWholeSalePriceFromPriceSettings(itemId, currentUnitId);

                            if (sellingPriceValue <= 0 && originalPacking > 0)
                            {
                                float originalSellingPrice = 0;
                                if (targetRow.Cells.Exists("SellingPrice") && targetRow.Cells["SellingPrice"].Value != null)
                                {
                                    float.TryParse(targetRow.Cells["SellingPrice"].Value.ToString(), out originalSellingPrice);
                                }

                                if (originalSellingPrice > 0)
                                {
                                    sellingPriceValue = originalSellingPrice * packingRatio;
                                }
                            }
                        }

                        if (targetRow.Cells.Exists("SellingPrice"))
                        {
                            targetRow.Cells["SellingPrice"].Value = sellingPriceValue;
                        }

                        if (targetRow.Cells.Exists("UnitPrize"))
                        {
                            targetRow.Cells["UnitPrize"].Value = newRetailPrice;
                        }
                    }
                }
            }
            finally
            {
                _isApplyingUnitSelection = false;
                _isUpdatingUnit = false;
            }

            float currentQty = originalQty > 0 ? originalQty : 1;
            float updatedCostWithTax = 0;
            if (targetRow.Cells.Exists("Cost") && targetRow.Cells["Cost"].Value != null)
            {
                float.TryParse(targetRow.Cells["Cost"].Value.ToString(), out updatedCostWithTax);
            }

            if (updatedCostWithTax > 0)
            {
                RecalculateBaseCostAndTaxFromCost(targetRow, updatedCostWithTax, currentQty);
            }

            this.CaluateTotals();
        }

        private void UltraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                ApplyGridTheme(ultraGrid1);

                // Ensure column moving and swapping are always enabled even after layout restore
                e.Layout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                e.Layout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;
                e.Layout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;

                Color lightBlue = GridRowLine;
                Color headerBlue = GridHeaderBlue;

                // Apply proper grid line styles - changed to solid lines as requested
                e.Layout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;

                // Set grid line colors to light blue as shown in the image
                e.Layout.Override.RowAppearance.BorderColor = lightBlue;
                e.Layout.Override.CellAppearance.BorderColor = lightBlue;
                e.Layout.Appearance.BorderColor = lightBlue; // Main grid border

                // Set border style for the main grid
                e.Layout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

                // Remove cell padding/spacing as requested
                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0; // Remove row spacing
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;

                // Configure row height to match frmdialForItemMaster (30 pixels)
                e.Layout.Override.MinRowHeight = 19;
                e.Layout.Override.DefaultRowHeight = 19;

                // Define colors - matching frmdialForItemMaster
                Color selectedRowBlue = GridSelectedBlue;

                // Set default alignment for all cells
                e.Layout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                e.Layout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

                // Set font size for all cells (matching frmdialForItemMaster)
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 8.25F;
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Add header styling with solid color matching FrmPurchaseDisplayDialog.cs
                e.Layout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.Standard;
                e.Layout.Override.HeaderAppearance.BackColor = headerBlue;
                e.Layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
                e.Layout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
                e.Layout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                e.Layout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                e.Layout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.False; // Change from bold to regular
                e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
                e.Layout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
                e.Layout.Override.HeaderAppearance.BorderColor = BorderBlue;

                // Configure row selector appearance with solid color matching FrmPurchaseDisplayDialog.cs
                e.Layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
                e.Layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
                e.Layout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                e.Layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
                e.Layout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.ColumnChooserButton;
                e.Layout.Override.RowSelectorWidth = 20;
                e.Layout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.RowIndex;
                e.Layout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;

                // Set all cells to have white background (no alternate row coloring)
                e.Layout.Override.RowAppearance.BackColor = Color.White;
                e.Layout.Override.RowAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                e.Layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
                e.Layout.Override.RowAlternateAppearance.BackColor2 = GridAltRow;
                e.Layout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Configure selected row appearance with bright blue highlight (matching frmdialForItemMaster)
                e.Layout.Override.SelectedRowAppearance.BackColor = selectedRowBlue;
                e.Layout.Override.SelectedRowAppearance.BackColor2 = selectedRowBlue;
                e.Layout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                e.Layout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast
                e.Layout.Override.SelectedRowAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.False;

                // Configure active row appearance - make it same as selected row
                e.Layout.Override.ActiveRowAppearance.BackColor = selectedRowBlue;
                e.Layout.Override.ActiveRowAppearance.BackColor2 = selectedRowBlue;
                e.Layout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                e.Layout.Override.ActiveRowAppearance.ForeColor = Color.White;
                e.Layout.Override.ActiveRowAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.False;

                // Configure cell and row borders - changed to solid as requested with light blue color
                e.Layout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;

                // Configure spacing and expansion behavior
                e.Layout.InterBandSpacing = 0;


                // Configure scrollbar style
                e.Layout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                e.Layout.MaxColScrollRegions = 1;
                e.Layout.MaxRowScrollRegions = 1;

                // Enable column moving and dragging (matching frmdialForItemMaster)
                e.Layout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                e.Layout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                e.Layout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                e.Layout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;

                // Style scrollbars if available
                if (e.Layout.ScrollBarLook != null)
                {
                    // Style track and buttons with blue colors
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor2 = Color.FromArgb(225, 236, 246);
                    e.Layout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    e.Layout.ScrollBarLook.TrackAppearance.BorderColor = BorderBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor2 = GridHeaderBlueDark;
                    e.Layout.ScrollBarLook.ThumbAppearance.BorderColor = BorderBlue;
                }

                // Disable filter indicators and other unnecessary features
                e.Layout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                e.Layout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                e.Layout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                e.Layout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;

                // Customize each column individually
                if (e.Layout.Bands.Count > 0)
                {
                    UltraGridBand band = e.Layout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        // Customize column header with solid blue color
                        col.Header.Appearance.BackColor = headerBlue;
                        col.Header.Appearance.BackColor2 = GridHeaderBlueDark;
                        col.Header.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                        col.Header.Appearance.ForeColor = Color.White;
                        col.Header.Appearance.BorderColor = GridRowLine;
                        col.Header.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        col.Header.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                        col.Header.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.False; // Change from bold to regular

                        // This is important to remove sort indicators
                        col.Header.VisiblePosition = col.Header.VisiblePosition;

                        // Set cell appearance with solid borders and light blue color
                        col.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                        col.CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.False;
                        col.CellAppearance.BorderColor = GridRowLine;

                        // Format numeric columns
                        if (col.DataType == typeof(decimal) || col.DataType == typeof(float) ||
                            col.DataType == typeof(double) || col.DataType == typeof(int))
                        {
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                            col.Format = "N2";
                        }
                    }

                    ApplyLockedPurchaseGridColumns(band);
                }

                RefreshGridColumnTheme(ultraGrid1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in UltraGrid1_InitializeLayout: " + ex.Message);
            }
        }

        private void SetupGridDocking()
        {
            try
            {
                if (ultraGrid1 != null)
                {
                    // Define colors
                    Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                    Color selectedBlue = Color.FromArgb(173, 216, 255); // Light blue for selection (as requested)
                    Color gridLineColor = lightBlue; // Using light blue for grid lines as in image

                    // Header color matching FrmPurchaseDisplayDialog.cs
                    Color headerBlue = Color.FromArgb(0, 123, 255); // Solid blue color for headers

                    // Set basic grid appearance for optimal display
                    ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
                    ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                    ultraGrid1.DisplayLayout.Override.AllowColSwapping = AllowColSwapping.WithinBand;
                    ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                    ultraGrid1.DisplayLayout.UseFixedHeaders = false;
                    ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
                    ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 22;
                    ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0; // Remove row spacing as requested
                    ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                    ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                    ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                    ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                    ultraGrid1.DisplayLayout.MaxColScrollRegions = 1;
                    ultraGrid1.DisplayLayout.MaxRowScrollRegions = 1;
                    ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                    ultraGrid1.DisplayLayout.Appearance.BorderColor = lightBlue; // Set main grid border to light blue

                    // Set grid line colors to light blue as shown in the image
                    ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                    ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;

                    // Change from dotted to solid lines as requested
                    ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                    ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;

                    // Set row selector appearance with solid color matching FrmPurchaseDisplayDialog.cs
                    ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = lightBlue; // Set row selector border to light blue
                    ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
                    ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15; // Match FrmPurchaseDisplayDialog width
                    ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None; // Remove numbers

                    // Set selected row appearance with light blue highlighting
                    ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectedBlue;
                    ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = selectedBlue;
                    ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = SystemColors.ControlText;

                    // Active row appearance - make it same as selected row
                    ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectedBlue;
                    ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = selectedBlue;
                    ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = SystemColors.ControlText;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up grid docking: " + ex.Message);
            }
        }

        internal void AddItemToGrid(string itemId, string itemName, string barcode, string unit, decimal unitPrice1, int quantity, decimal cost, int unitId = 0, float taxPer = 0, float taxAmt = 0, string taxType = "")
        {
            try
            {
                if (_isReadOnly)
                {
                    MessageBox.Show(MsgReadOnlyBlock, MsgReadOnlyModeTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int itemIdValue = 0;
                int.TryParse(itemId, out itemIdValue);
                ItemStatusRuleInfo selectedStatus;
                if (!EnsureItemAllowedForPurchase(itemIdValue, itemName, out selectedStatus))
                {
                    return;
                }

                // Get the DataTable from the UltraGrid
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null)
                {
                    MessageBox.Show("Error: Grid data source is not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if this barcode already exists
                int existingItemIndex = -1;
                CheckBarcode(barcode, out existingItemIndex);

                if (existingItemIndex < 0)
                {
                    // Get the item data from the database to retrieve accurate packing value and WholeSalePrice
                    // Ensure packing defaults to base unit (1)
                    double packingValue = 1; // Base unit packing
                    float wholeSalePrice = 0; // Default value

                    // Fetch item data to get WholeSalePrice
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        try
                        {
                            DataBase.Operations = "BARCODEPURCHASE";
                            ItemDDlGrid itemDataResult = drop.itemDDlGrid(barcode, null);
                            if (itemDataResult != null && itemDataResult.List != null && itemDataResult.List.Any())
                            {
                                var itm = itemDataResult.List.FirstOrDefault();
                                if (itm != null)
                                {
                                    wholeSalePrice = (float)itm.WholeSalePrice;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error fetching WholeSalePrice: " + ex.Message);
                        }
                    }

                    // Add a new row
                    DataRow newRow = dt.NewRow();

                    newRow["SLNO"] = dt.Rows.Count + 1;

                    // Handle ItemId - ensure it's a valid integer
                    if (!string.IsNullOrEmpty(itemId) && int.TryParse(itemId, out itemIdValue))
                    {
                        newRow["ItemId"] = itemIdValue;
                    }
                    else
                    {
                        newRow["ItemId"] = DBNull.Value;
                    }

                    newRow["BarCode"] = barcode ?? string.Empty;
                    newRow["Description"] = itemName ?? string.Empty;
                    newRow["Unit"] = unit ?? string.Empty;
                    newRow["Packing"] = packingValue; // Always default to base unit packing
                    newRow["RetailPrice"] = (float)unitPrice1;
                    newRow["Free"] = 0;

                    // Get WholeSalePrice from PriceSettings table (remapped from PDetails.WholeSalePrice)
                    float wholeSalePriceFromPriceSettings = GetWholeSalePriceFromPriceSettings(itemIdValue, unitId);
                    newRow["SellingPrice"] = wholeSalePriceFromPriceSettings > 0
                        ? wholeSalePriceFromPriceSettings
                        : wholeSalePrice; // Fallback to item data if PriceSettings not found

                    // Get Unit 1's WholeSalePrice from PriceSettings for UnitSP column
                    // Use same fallback logic as SellingPrice (item's WholeSalePrice) if Unit 1's PriceSettings not found
                    float unit1WholeSalePrice = GetWholeSalePriceFromPriceSettings(itemIdValue, 1);
                    newRow["UnitSP"] = unit1WholeSalePrice > 0 ? unit1WholeSalePrice : wholeSalePrice;

                    // Calculate tax and cost structure based on tax type
                    float costWithTax = (float)cost;
                    float baseCostWithoutTax = costWithTax;
                    float calculatedTaxAmt = 0;
                    float finalTaxPer = 0;

                    // Normalize tax type from database
                    string normalizedTaxType = NormalizeTaxType(taxType);

                    // Check global tax setting - if disabled, set tax to 0
                    if (!DataBase.IsTaxEnabled)
                    {
                        calculatedTaxAmt = 0;
                        finalTaxPer = 0;
                        baseCostWithoutTax = costWithTax;
                    }
                    else if (!string.IsNullOrEmpty(normalizedTaxType) && taxPer > 0)
                    {
                        finalTaxPer = taxPer;
                        if (normalizedTaxType == "incl")
                        {
                            // Tax is included in the cost
                            calculatedTaxAmt = costWithTax * (taxPer / (100 + taxPer));
                            baseCostWithoutTax = costWithTax - calculatedTaxAmt;
                        }
                        else if (normalizedTaxType == "excl")
                        {
                            // Tax is exclusive (added on top)
                            calculatedTaxAmt = costWithTax * (taxPer / 100);
                            baseCostWithoutTax = costWithTax;
                        }
                    }

                    newRow["BaseCost"] = baseCostWithoutTax;
                    newRow["Cost"] = costWithTax;
                    newRow["Qty"] = quantity;

                    // Calculate NetAmt and Gross for new items
                    // NetAmt = Gross + TaxAmt (for both excl and incl when adding new items)
                    float amount = costWithTax * quantity;
                    if (normalizedTaxType == "excl")
                    {
                        // For excl: Amount is Gross, NetAmt = Amount + TaxAmt
                        newRow["NetAmt"] = amount + calculatedTaxAmt;
                        newRow["Gross"] = amount; // Gross = Amount for excl
                    }
                    else if (normalizedTaxType == "incl")
                    {
                        // For incl: Gross = Amount - TaxAmt, NetAmt = Amount (since Amount already includes tax)
                        float gross = amount - calculatedTaxAmt;
                        newRow["Gross"] = gross;
                        newRow["NetAmt"] = amount; // NetAmt = Amount for incl (already includes tax)
                    }
                    else
                    {
                        // No tax type: NetAmt = Amount, Gross = 0
                        newRow["NetAmt"] = amount;
                        newRow["Gross"] = 0f;
                    }

                    // If unitId is not provided, try to get it from the unit name
                    if (unitId <= 0)
                    {
                        unitId = GetUnitIdByName(unit);
                    }

                    // Hidden columns with default values
                    newRow["UnitId"] = unitId;
                    newRow["UnitPrize"] = (float)unitPrice1;
                    newRow["MarginPer"] = 0f;
                    newRow["MarginAmt"] = 0f;
                    newRow["TaxPer"] = finalTaxPer;
                    newRow["TaxAmt"] = calculatedTaxAmt; // Use calculated tax amount
                    newRow["TaxType"] = normalizedTaxType; // Save normalized tax type
                    newRow["WholeSalePrice"] = (float)unitPrice1;
                    newRow["CreditPrice"] = (float)unitPrice1;
                    newRow["CardPrice"] = (float)unitPrice1;
                    newRow["NewBaseCost"] = 0f; // Will be recalculated after row is added
                    ApplyItemStatusToPurchaseRow(newRow, selectedStatus);

                    // Add the row to the DataTable
                    dt.Rows.Add(newRow);

                    // Update NetAmt and Gross visibility based on tax type
                    UpdateNetAmtColumnVisibility();
                    UpdateGrossColumnVisibility();

                    // Select the newly added row
                    int newRowIndex = dt.Rows.Count - 1;
                    if (ultraGrid1.Rows.Count > newRowIndex)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[newRowIndex];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }

                    // Recalculate NewBaseCost (average cost) for all rows
                    RecalculateNewBaseCostForAllRows();

                    // Calculate totals
                    this.CaluateTotals();
                    ultraGrid1.Rows.Refresh(RefreshRow.FireInitializeRow);
                }
                else if (existingItemIndex >= 0)
                {
                    // Highlight the existing row that was updated
                    if (existingItemIndex < ultraGrid1.Rows.Count)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[existingItemIndex];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                }

                // Set focus to barcode field
                this.barcodeFocus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error adding item to grid: " + ex.Message);
                MessageBox.Show("Error adding item to grid: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to get UnitId by unit name
        private int GetUnitIdByName(string unitName)
        {
            try
            {
                // Default to 0 if not found
                int unitId = 0;

                // Try to get the UnitId from the database directly
                // This is a simpler approach that avoids type compatibility issues
                if (!string.IsNullOrEmpty(unitName))
                {
                    // For now, we'll just return 0 since we don't have direct access to the unit ID
                    // In a real implementation, you would query the database for the unit ID
                    // based on the unit name
                    unitId = 0;
                }

                return unitId;
            }
            catch (Exception)
            {
                // Return 0 if there's any error
                return 0;
            }
        }

        /// <summary>
        /// Gets WholeSalePrice from PriceSettings table for a specific ItemId and UnitId
        /// </summary>
        private float GetWholeSalePriceFromPriceSettings(int itemId, int unitId)
        {
            try
            {
                if (itemId <= 0 || unitId <= 0)
                    return 0f;

                // Query PriceSettings table directly using Dropdowns connection
                bool wasOpen = drop.DataConnection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                    drop.DataConnection.Open();

                try
                {
                    string query = @"
                        SELECT ISNULL(WholeSalePrice, 0) as WholeSalePrice
                        FROM PriceSettings 
                        WHERE BranchId = @BranchId 
                            AND CompanyId = @CompanyId
                            AND ItemId = @ItemId 
                            AND UnitId = @UnitId";

                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, (System.Data.SqlClient.SqlConnection)drop.DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                        cmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                        cmd.Parameters.AddWithValue("@ItemId", itemId);
                        cmd.Parameters.AddWithValue("@UnitId", unitId);

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToSingle(result);
                        }
                    }
                }
                finally
                {
                    if (!wasOpen && drop.DataConnection.State == System.Data.ConnectionState.Open)
                        drop.DataConnection.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting WholeSalePrice from PriceSettings: {ex.Message}");
            }

            return 0f;
        }

        /// <summary>
        /// Updates PriceSettings table WholeSalePrice for Unit 1 (base unit) of a specific ItemId
        /// </summary>
        private void UpdatePriceSettingsUnit1WholeSalePrice(int itemId, float wholeSalePrice)
        {
            try
            {
                if (itemId <= 0)
                    return;

                // Query PriceSettings table directly using Dropdowns connection
                bool wasOpen = drop.DataConnection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                    drop.DataConnection.Open();

                try
                {
                    // First check if record exists for Unit 1
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM PriceSettings 
                        WHERE BranchId = @BranchId 
                            AND CompanyId = @CompanyId
                            AND ItemId = @ItemId 
                            AND UnitId = 1";

                    int recordCount = 0;
                    using (System.Data.SqlClient.SqlCommand checkCmd = new System.Data.SqlClient.SqlCommand(checkQuery, (System.Data.SqlClient.SqlConnection)drop.DataConnection))
                    {
                        checkCmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                        checkCmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                        checkCmd.Parameters.AddWithValue("@ItemId", itemId);

                        object result = checkCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            recordCount = Convert.ToInt32(result);
                        }
                    }

                    if (recordCount > 0)
                    {
                        // Update existing record
                        string updateQuery = @"
                            UPDATE PriceSettings 
                            SET WholeSalePrice = @WholeSalePrice
                            WHERE BranchId = @BranchId 
                                AND CompanyId = @CompanyId
                                AND ItemId = @ItemId 
                                AND UnitId = 1";

                        using (System.Data.SqlClient.SqlCommand updateCmd = new System.Data.SqlClient.SqlCommand(updateQuery, (System.Data.SqlClient.SqlConnection)drop.DataConnection))
                        {
                            updateCmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                            updateCmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                            updateCmd.Parameters.AddWithValue("@ItemId", itemId);
                            updateCmd.Parameters.AddWithValue("@WholeSalePrice", wholeSalePrice);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Insert new record for Unit 1 (we need to get other required fields from existing PriceSettings or ItemMaster)
                        // For now, we'll try to get default values from any existing PriceSettings record for this item
                        string getDefaultsQuery = @"
                            SELECT TOP 1 Unit, Packing, Cost, RetailPrice, CreditPrice, CardPrice, MRP, Stock, OrderedStock, StockValue, ReOrder, BarCode
                            FROM PriceSettings 
                            WHERE BranchId = @BranchId 
                                AND CompanyId = @CompanyId
                                AND ItemId = @ItemId";

                        using (System.Data.SqlClient.SqlCommand getCmd = new System.Data.SqlClient.SqlCommand(getDefaultsQuery, (System.Data.SqlClient.SqlConnection)drop.DataConnection))
                        {
                            getCmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                            getCmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                            getCmd.Parameters.AddWithValue("@ItemId", itemId);

                            using (System.Data.SqlClient.SqlDataReader reader = getCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // Use values from existing record
                                    string unit = reader["Unit"]?.ToString() ?? "";
                                    float packing = Convert.ToSingle(reader["Packing"] ?? 1f);
                                    float cost = Convert.ToSingle(reader["Cost"] ?? 0f);
                                    float retailPrice = Convert.ToSingle(reader["RetailPrice"] ?? 0f);
                                    float creditPrice = Convert.ToSingle(reader["CreditPrice"] ?? 0f);
                                    float cardPrice = Convert.ToSingle(reader["CardPrice"] ?? 0f);
                                    float mrp = Convert.ToSingle(reader["MRP"] ?? 0f);
                                    float stock = Convert.ToSingle(reader["Stock"] ?? 0f);
                                    float orderedStock = Convert.ToSingle(reader["OrderedStock"] ?? 0f);
                                    float stockValue = Convert.ToSingle(reader["StockValue"] ?? 0f);
                                    float reOrder = Convert.ToSingle(reader["ReOrder"] ?? 0f);
                                    string barCode = reader["BarCode"]?.ToString() ?? "";

                                    reader.Close();

                                    // Insert new record for Unit 1
                                    string insertQuery = @"
                                        INSERT INTO PriceSettings 
                                        (BranchId, CompanyId, FinYearId, ItemId, UnitId, Unit, Packing, Cost, RetailPrice, 
                                         WholeSalePrice, CreditPrice, CardPrice, MRP, Stock, OrderedStock, StockValue, ReOrder, BarCode)
                                        VALUES 
                                        (@BranchId, @CompanyId, @FinYearId, @ItemId, 1, @Unit, @Packing, @Cost, @RetailPrice, 
                                         @WholeSalePrice, @CreditPrice, @CardPrice, @MRP, @Stock, @OrderedStock, @StockValue, @ReOrder, @BarCode)";

                                    using (System.Data.SqlClient.SqlCommand insertCmd = new System.Data.SqlClient.SqlCommand(insertQuery, (System.Data.SqlClient.SqlConnection)drop.DataConnection))
                                    {
                                        insertCmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                                        insertCmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                                        insertCmd.Parameters.AddWithValue("@FinYearId", Convert.ToInt32(DataBase.FinyearId));
                                        insertCmd.Parameters.AddWithValue("@ItemId", itemId);
                                        insertCmd.Parameters.AddWithValue("@Unit", unit);
                                        insertCmd.Parameters.AddWithValue("@Packing", packing);
                                        insertCmd.Parameters.AddWithValue("@Cost", cost);
                                        insertCmd.Parameters.AddWithValue("@RetailPrice", retailPrice);
                                        insertCmd.Parameters.AddWithValue("@WholeSalePrice", wholeSalePrice);
                                        insertCmd.Parameters.AddWithValue("@CreditPrice", creditPrice);
                                        insertCmd.Parameters.AddWithValue("@CardPrice", cardPrice);
                                        insertCmd.Parameters.AddWithValue("@MRP", mrp);
                                        insertCmd.Parameters.AddWithValue("@Stock", stock);
                                        insertCmd.Parameters.AddWithValue("@OrderedStock", orderedStock);
                                        insertCmd.Parameters.AddWithValue("@StockValue", stockValue);
                                        insertCmd.Parameters.AddWithValue("@ReOrder", reOrder);
                                        insertCmd.Parameters.AddWithValue("@BarCode", barCode);

                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    reader.Close();
                                    // If no existing record, we can't insert without all required fields
                                    // Log a warning but don't throw error
                                    System.Diagnostics.Debug.WriteLine($"Warning: Cannot insert PriceSettings for Unit 1 - no existing record found for ItemId {itemId}");
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (!wasOpen && drop.DataConnection.State == System.Data.ConnectionState.Open)
                        drop.DataConnection.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating PriceSettings Unit 1 WholeSalePrice: {ex.Message}");
                MessageBox.Show($"Error updating price settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates WholeSalePrice for ALL units of an item in PriceSettings table
        /// Each unit's WholeSalePrice = basePrice * unitPacking
        /// Example: If basePrice = 20 (Unit 1's price), Unit 1 gets 20*1=20, Unit 10 (OTR) gets 20*10=200
        /// </summary>
        /// <param name="itemId">The ItemId to update</param>
        /// <param name="basePrice">The base unit price (Unit 1's selling price)</param>
        private void UpdateAllUnitsWholeSalePriceInPriceSettings(int itemId, float basePrice)
        {
            try
            {
                if (itemId <= 0)
                    return;

                // Query PriceSettings table directly using Dropdowns connection
                bool wasOpen = drop.DataConnection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                    drop.DataConnection.Open();

                try
                {
                    // First, get all units for this item with their packing values
                    string selectQuery = @"
                        SELECT UnitId, Packing 
                        FROM PriceSettings 
                        WHERE BranchId = @BranchId 
                            AND CompanyId = @CompanyId
                            AND ItemId = @ItemId";

                    var unitsToUpdate = new System.Collections.Generic.List<System.Tuple<int, float>>();

                    using (System.Data.SqlClient.SqlCommand selectCmd = new System.Data.SqlClient.SqlCommand(selectQuery, (System.Data.SqlClient.SqlConnection)drop.DataConnection))
                    {
                        selectCmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                        selectCmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                        selectCmd.Parameters.AddWithValue("@ItemId", itemId);

                        using (System.Data.SqlClient.SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int unitId = 0;
                                float packing = 1;

                                if (reader["UnitId"] != null && reader["UnitId"] != DBNull.Value)
                                {
                                    unitId = Convert.ToInt32(reader["UnitId"]);
                                }
                                if (reader["Packing"] != null && reader["Packing"] != DBNull.Value)
                                {
                                    packing = Convert.ToSingle(reader["Packing"]);
                                }

                                // Ensure packing is at least 1 to avoid invalid calculations
                                if (packing <= 0)
                                    packing = 1;

                                unitsToUpdate.Add(new System.Tuple<int, float>(unitId, packing));
                            }
                        }
                    }

                    // Now update each unit's WholeSalePrice (DB.WholeSalePrice = retail/selling price → maps to txt_Retail)
                    if (unitsToUpdate.Count > 0)
                    {
                        string updateQuery = @"
                            UPDATE PriceSettings 
                            SET WholeSalePrice = @WholeSalePrice
                            WHERE BranchId = @BranchId 
                                AND CompanyId = @CompanyId
                                AND ItemId = @ItemId 
                                AND UnitId = @UnitId";

                        foreach (var unit in unitsToUpdate)
                        {
                            int unitId = unit.Item1;
                            float packing = unit.Item2;

                            // Calculate the WholeSalePrice for this unit
                            // WholeSalePrice = basePrice * packing
                            float wholeSalePriceForUnit = basePrice * packing;

                            using (System.Data.SqlClient.SqlCommand updateCmd = new System.Data.SqlClient.SqlCommand(updateQuery, (System.Data.SqlClient.SqlConnection)drop.DataConnection))
                            {
                                updateCmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                                updateCmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                                updateCmd.Parameters.AddWithValue("@ItemId", itemId);
                                updateCmd.Parameters.AddWithValue("@UnitId", unitId);
                                updateCmd.Parameters.AddWithValue("@WholeSalePrice", wholeSalePriceForUnit);

                                updateCmd.ExecuteNonQuery();
                            }

                            System.Diagnostics.Debug.WriteLine($"Updated PriceSettings: ItemId={itemId}, UnitId={unitId}, Packing={packing}, WholeSalePrice={wholeSalePriceForUnit}");
                        }

                        // Raise event to notify other forms (like frmItemMasterNew) that prices were updated
                        RaisePriceSettingsUpdated(itemId);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No units found in PriceSettings for ItemId {itemId}");
                    }
                }
                finally
                {
                    if (!wasOpen && drop.DataConnection.State == System.Data.ConnectionState.Open)
                        drop.DataConnection.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating all units WholeSalePrice in PriceSettings: {ex.Message}");
            }
        }

        private void UltraGrid1_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            if (e?.Cell?.Row != null && !EnsurePurchaseRowEditable(e.Cell.Row, true))
            {
                e.Cancel = true;
                e.Cell.CancelUpdate();
                return;
            }

            if (IsLockedPurchaseGridColumn(e.Cell.Column.Key))
            {
                bool allowInternalUnitUpdate = _isApplyingUnitSelection &&
                    string.Equals(e.Cell.Column.Key, "Unit", StringComparison.OrdinalIgnoreCase);

                if (!allowInternalUnitUpdate)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Save original value for comparison
            object originalValue = e.Cell.Value;

            // Handle cell update logic as needed
            if (e.Cell.Column.Key == "Qty" || e.Cell.Column.Key == "Cost" || e.Cell.Column.Key == "UnitSP" || e.Cell.Column.Key == "SellingPrice")
            {
                // Validate the value if needed
                // Example: Check if the value is a valid number
                float value;
                if (e.NewValue == null || !float.TryParse(e.NewValue.ToString(), out value))
                {
                    // If invalid, cancel the update
                    e.Cancel = true;
                    return;
                }

                // Don't allow negative values for UnitSP or SellingPrice
                if ((e.Cell.Column.Key == "UnitSP" || e.Cell.Column.Key == "SellingPrice") && value < 0)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void UltraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            // Handle Free quantity changes
            if (e.Cell.Column.Key == "Free")
            {
                float freeValue = 0;
                if (e.Cell.Value != null && float.TryParse(e.Cell.Value.ToString(), out freeValue))
                {
                    HandleFreeQuantityChange(e.Cell.Row, freeValue);
                    // Recalculate totals
                    CaluateTotals();
                    return;
                }
            }

            // Handle SellingPrice changes - recalculate base price (UnitSP) and update all units in PriceSettings
            if (e.Cell.Column.Key == "SellingPrice")
            {
                // Skip if we're in the middle of a unit change to prevent rate multiplication bug
                if (_isUpdatingUnit) return;

                try
                {
                    float sellingPriceValue = 0;
                    if (e.Cell.Value != null && float.TryParse(e.Cell.Value.ToString(), out sellingPriceValue))
                    {
                        // Get ItemId from the row
                        int itemId = 0;
                        if (e.Cell.Row.Cells.Exists("ItemId") && e.Cell.Row.Cells["ItemId"].Value != null)
                        {
                            int.TryParse(e.Cell.Row.Cells["ItemId"].Value.ToString(), out itemId);
                        }

                        // Get Packing from the row (the current unit's packing value)
                        float packing = 1;
                        if (e.Cell.Row.Cells.Exists("Packing") && e.Cell.Row.Cells["Packing"].Value != null)
                        {
                            float.TryParse(e.Cell.Row.Cells["Packing"].Value.ToString(), out packing);
                        }

                        // Avoid division by zero
                        if (packing <= 0)
                            packing = 1;

                        // Calculate the base unit price (Unit 1's selling price)
                        // Example: If selling price = 200 and packing = 10, then base price = 200/10 = 20
                        float basePrice = sellingPriceValue / packing;

                        // Update UnitSP column with the calculated base price
                        if (e.Cell.Row.Cells.Exists("UnitSP"))
                        {
                            e.Cell.Row.Cells["UnitSP"].Value = basePrice;
                        }

                        if (itemId > 0)
                        {
                            // Update all units' WholeSalePrice in PriceSettings table
                            // Each unit's WholeSalePrice = basePrice * unitPacking
                            UpdateAllUnitsWholeSalePriceInPriceSettings(itemId, basePrice);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error handling SellingPrice change: {ex.Message}");
                }
                return;
            }

            // Handle UnitSP changes - recalculate SellingPrice and update all units in PriceSettings
            if (e.Cell.Column.Key == "UnitSP")
            {
                try
                {
                    float unitSPValue = 0;
                    if (e.Cell.Value != null && float.TryParse(e.Cell.Value.ToString(), out unitSPValue))
                    {
                        // Get ItemId from the row
                        int itemId = 0;
                        if (e.Cell.Row.Cells.Exists("ItemId") && e.Cell.Row.Cells["ItemId"].Value != null)
                        {
                            int.TryParse(e.Cell.Row.Cells["ItemId"].Value.ToString(), out itemId);
                        }

                        // Get Packing from the row (the current unit's packing value)
                        float packing = 1;
                        if (e.Cell.Row.Cells.Exists("Packing") && e.Cell.Row.Cells["Packing"].Value != null)
                        {
                            float.TryParse(e.Cell.Row.Cells["Packing"].Value.ToString(), out packing);
                        }

                        // Avoid zero packing
                        if (packing <= 0)
                            packing = 1;

                        // Calculate the new SellingPrice for the current row's unit
                        // Example: If base price (UnitSP) = 20 and packing = 10, then SellingPrice = 20 * 10 = 200
                        float newSellingPrice = unitSPValue * packing;

                        // Update SellingPrice column
                        if (e.Cell.Row.Cells.Exists("SellingPrice"))
                        {
                            e.Cell.Row.Cells["SellingPrice"].Value = newSellingPrice;
                        }

                        if (itemId > 0)
                        {
                            // Update all units' WholeSalePrice in PriceSettings table
                            // Each unit's WholeSalePrice = basePrice * unitPacking
                            UpdateAllUnitsWholeSalePriceInPriceSettings(itemId, unitSPValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error handling UnitSP change: {ex.Message}");
                }
                return;
            }

            // Recalculate when Qty, Cost, BaseCost, TaxPer, TaxType, or TaxAmt is changed
            if (e.Cell.Column.Key == "Qty" || e.Cell.Column.Key == "Cost" || e.Cell.Column.Key == "BaseCost" ||
                e.Cell.Column.Key == "TaxPer" || e.Cell.Column.Key == "TaxType" || e.Cell.Column.Key == "TaxAmt")
            {
                if (e.Cell.Row.Cells.Exists("Qty") && e.Cell.Row.Cells.Exists("Cost"))
                {
                    if (e.Cell.Row.Cells["Qty"].Value != null && e.Cell.Row.Cells["Cost"].Value != null)
                    {
                        float qty = 0;
                        float costWithTax = 0;

                        if (float.TryParse(e.Cell.Row.Cells["Qty"].Value.ToString(), out qty) &&
                            float.TryParse(e.Cell.Row.Cells["Cost"].Value.ToString(), out costWithTax))
                        {
                            // If Cost was changed, recalculate BaseCost and TaxAmt
                            if (e.Cell.Column.Key == "Cost")
                            {
                                RecalculateBaseCostAndTaxFromCost(e.Cell.Row, costWithTax, qty);
                                // Recalculate NewBaseCost (average cost) for all rows
                                RecalculateNewBaseCostForAllRows();
                            }
                            else
                            {
                                // Otherwise recalculate based on current values
                                RecalculateTaxForRow(e.Cell.Row, costWithTax, qty);
                            }

                            // If TaxType was changed, update column visibility
                            if (e.Cell.Column.Key == "TaxType")
                            {
                                UpdateNetAmtColumnVisibility();
                                UpdateGrossColumnVisibility();
                            }

                            // Update totals
                            CaluateTotals();
                        }
                    }
                }
            }

            // Handle NetAmt (Net Amount) changes - calculate Cost and Gross
            if (e.Cell.Column.Key == "NetAmt")
            {
                // Guard to prevent infinite recursion
                if (_isUpdatingFromNetAmt) return;
                _isUpdatingFromNetAmt = true;

                try
                {
                    // Get tax type
                    string taxType = "";
                    if (e.Cell.Row.Cells.Exists("TaxType") && e.Cell.Row.Cells["TaxType"].Value != null)
                    {
                        taxType = NormalizeTaxType(e.Cell.Row.Cells["TaxType"].Value.ToString());
                    }

                    // Calculate for all tax types

                    // Get values
                    float netAmount = 0;
                    float qty = 1;
                    float taxPer = 0;

                    if (e.Cell.Value != null)
                        float.TryParse(e.Cell.Value.ToString(), out netAmount);
                    if (e.Cell.Row.Cells.Exists("Qty") && e.Cell.Row.Cells["Qty"].Value != null)
                        float.TryParse(e.Cell.Row.Cells["Qty"].Value.ToString(), out qty);
                    if (e.Cell.Row.Cells.Exists("TaxPer") && e.Cell.Row.Cells["TaxPer"].Value != null)
                        float.TryParse(e.Cell.Row.Cells["TaxPer"].Value.ToString(), out taxPer);

                    // Avoid division by zero
                    if (qty <= 0) qty = 1;

                    // Calculate Cost = Net Amount / Qty
                    float newCost = netAmount / qty;
                    if (e.Cell.Row.Cells.Exists("Cost"))
                    {
                        e.Cell.Row.Cells["Cost"].Value = newCost;
                    }

                    // Now that we have the new Cost, we should run the standard calculation
                    // This will properly calculate BaseCost, TaxAmt, Gross, etc. exactly like
                    // when the user types the Cost manually.
                    RecalculateBaseCostAndTaxFromCost(e.Cell.Row, newCost, qty);

                    // The RecalculateBaseCostAndTaxFromCost handles the math perfectly, 
                    // but we need to ensure Gross is updated correctly
                    float newTaxAmount = 0;
                    if (e.Cell.Row.Cells.Exists("TaxAmt") && e.Cell.Row.Cells["TaxAmt"].Value != null)
                        float.TryParse(e.Cell.Row.Cells["TaxAmt"].Value.ToString(), out newTaxAmount);

                    if (e.Cell.Row.Cells.Exists("Gross"))
                    {
                        e.Cell.Row.Cells["Gross"].Value = netAmount - newTaxAmount;
                    }

                    // Recalculate totals to update label4 and txtInvoiceAmt
                    CaluateTotals();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error handling NetAmt change: {ex.Message}");
                }
                finally
                {
                    _isUpdatingFromNetAmt = false;
                }
                return;
            }

            // Handle Gross changes - for inclusive tax items
            if (e.Cell.Column.Key == "Gross")
            {
                try
                {
                    // Just recalculate totals to update label4 and txtInvoiceAmt
                    // No need to set the cell value again as it's already updated by the user's edit
                    CaluateTotals();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error handling Gross change: {ex.Message}");
                }
                return;
            }
        }

        // Helper method to recalculate BaseCost and TaxAmt when Cost is changed
        private void RecalculateBaseCostAndTaxFromCost(UltraGridRow row, float costWithTax, float qty)
        {
            try
            {
                float taxPer = 0;
                float taxAmt = 0;
                string taxType = "";
                float baseCost = costWithTax;

                // Check global tax setting - if disabled, set tax to 0
                if (!DataBase.IsTaxEnabled)
                {
                    taxAmt = 0;
                    baseCost = costWithTax;
                    if (row.Cells.Exists("TaxPer"))
                        row.Cells["TaxPer"].Value = 0;
                    if (row.Cells.Exists("TaxAmt"))
                        row.Cells["TaxAmt"].Value = 0;
                    if (row.Cells.Exists("BaseCost"))
                        row.Cells["BaseCost"].Value = baseCost;
                    if (row.Cells.Exists("NetTotal")) row.Cells["NetTotal"].Value = costWithTax * qty;
                    return;
                }

                // Get tax percentage
                if (row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null)
                {
                    float.TryParse(row.Cells["TaxPer"].Value.ToString(), out taxPer);
                }

                // Get tax type
                if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null)
                {
                    taxType = row.Cells["TaxType"].Value.ToString();
                }

                // Calculate baseCost and taxAmt based on tax type
                if (!string.IsNullOrEmpty(taxType) && taxPer > 0)
                {
                    float taxAmtPerUnit = 0;
                    if (taxType.ToLower() == "incl")
                    {
                        // Tax is included in costWithTax - calculate per-unit tax
                        taxAmtPerUnit = costWithTax * (taxPer / (100 + taxPer));
                        baseCost = costWithTax - taxAmtPerUnit;
                        // Calculate total tax amount: per-unit tax × quantity
                        taxAmt = taxAmtPerUnit * qty;
                    }
                    else if (taxType.ToLower() == "excl")
                    {
                        // Tax is exclusive (added on top) - baseCost equals costWithTax
                        baseCost = costWithTax;
                        // Calculate per-unit tax amount
                        taxAmtPerUnit = baseCost * (taxPer / 100);
                        // Calculate total tax amount: per-unit tax × quantity
                        taxAmt = taxAmtPerUnit * qty;
                    }
                }

                // Update BaseCost and TaxAmt
                if (row.Cells.Exists("BaseCost"))
                {
                    row.Cells["BaseCost"].Value = baseCost;
                }
                if (row.Cells.Exists("TaxAmt"))
                {
                    row.Cells["TaxAmt"].Value = taxAmt;
                }

                // Update NetTotal
                float amount = costWithTax * qty;
                if (row.Cells.Exists("NetTotal")) row.Cells["NetTotal"].Value = amount;

                // Update NetAmt for excl tax type and Gross for incl tax type
                UpdateNetAmtForRow(row);
                UpdateGrossForRow(row);

                // Recalculate NewBaseCost (average cost) for all rows
                RecalculateNewBaseCostForAllRows();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error recalculating base cost and tax from cost: " + ex.Message);
            }
        }

        // Helper method to recalculate NewBaseCost (weighted average cost) for all rows
        // Formula: AvgCost = ((ExistingCost × ExistingStock) + (PurchaseCost × PurchaseQty)) / (ExistingStock + PurchaseQty)
        // Same formula as PurchaseInvoiceRepository.CalculateAverageCost
        private void RecalculateNewBaseCostForAllRows()
        {
            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                    return;

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        // Get ItemId and UnitId from the row
                        int itemId = 0;
                        int unitId = 0;
                        float purchaseCost = 0;
                        float purchaseQty = 0;

                        if (row["ItemId"] == null || row["ItemId"] == DBNull.Value)
                            continue;
                        int.TryParse(row["ItemId"].ToString(), out itemId);
                        if (itemId <= 0)
                            continue;

                        if (row["UnitId"] != null && row["UnitId"] != DBNull.Value)
                            int.TryParse(row["UnitId"].ToString(), out unitId);
                        if (unitId <= 0) unitId = 1;

                        if (row["Cost"] != null && row["Cost"] != DBNull.Value)
                            float.TryParse(row["Cost"].ToString(), out purchaseCost);

                        if (row["Qty"] != null && row["Qty"] != DBNull.Value)
                            float.TryParse(row["Qty"].ToString(), out purchaseQty);

                        // Get existing Cost and Stock from PriceSettings for this item/unit
                        float existingCost = 0;
                        float existingStock = 0;
                        GetExistingCostAndStock(itemId, unitId, out existingCost, out existingStock);

                        // Calculate weighted average cost
                        float avgCost = CalculateWeightedAverageCost(existingCost, existingStock, purchaseCost, purchaseQty);

                        row["NewBaseCost"] = avgCost;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error calculating NewBaseCost for row: " + ex.Message);
                    }
                }

                // Force grid to refresh the NewBaseCost column cells from the underlying DataTable
                // This ensures the user sees the updated values immediately, without needing to change focus
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Rows.Refresh(RefreshRow.FireInitializeRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error recalculating NewBaseCost for all rows: " + ex.Message);
            }
        }

        // Gets existing Cost and Stock from PriceSettings table for a specific ItemId and UnitId
        private void GetExistingCostAndStock(int itemId, int unitId, out float existingCost, out float existingStock)
        {
            existingCost = 0;
            existingStock = 0;

            try
            {
                bool wasOpen = drop.DataConnection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                    drop.DataConnection.Open();

                try
                {
                    string query = @"
                        SELECT ISNULL(Cost, 0) as Cost, ISNULL(Stock, 0) as Stock 
                        FROM PriceSettings 
                        WHERE BranchId = @BranchId 
                            AND CompanyId = @CompanyId
                            AND ItemId = @ItemId 
                            AND UnitId = @UnitId";

                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, (System.Data.SqlClient.SqlConnection)drop.DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                        cmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                        cmd.Parameters.AddWithValue("@ItemId", itemId);
                        cmd.Parameters.AddWithValue("@UnitId", unitId);

                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existingCost = Convert.ToSingle(reader["Cost"]);
                                existingStock = Convert.ToSingle(reader["Stock"]);
                            }
                        }
                    }
                }
                finally
                {
                    if (!wasOpen && drop.DataConnection.State == System.Data.ConnectionState.Open)
                        drop.DataConnection.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting existing cost and stock for ItemId={itemId}, UnitId={unitId}: {ex.Message}");
            }
        }

        // Calculates weighted average cost (same formula as PurchaseInvoiceRepository.CalculateAverageCost)
        // Formula: AvgCost = ((ExistingCost × ExistingStock) + (PurchaseCost × PurchaseQty)) / (ExistingStock + PurchaseQty)
        private float CalculateWeightedAverageCost(float existingCost, float existingStock, float purchaseCost, float purchaseQty)
        {
            // If there's no existing stock, just use the purchase cost
            if (existingStock <= 0)
                return purchaseCost;

            // If no new purchase quantity, keep existing cost
            if (purchaseQty <= 0)
                return existingCost;

            // Calculate weighted average cost
            float totalValue = (existingCost * existingStock) + (purchaseCost * purchaseQty);
            float totalQty = existingStock + purchaseQty;

            // Prevent division by zero
            if (totalQty <= 0)
                return purchaseCost;

            return totalValue / totalQty;
        }

        // Helper method to recalculate tax for a row
        private void RecalculateTaxForRow(UltraGridRow row, float costWithTax, float qty)
        {
            try
            {
                float taxPer = 0;
                float baseCost = 0;
                string taxType = "";

                // Get existing per-unit BaseCost (should remain unchanged)
                if (row.Cells.Exists("BaseCost") && row.Cells["BaseCost"].Value != null)
                {
                    float.TryParse(row.Cells["BaseCost"].Value.ToString(), out baseCost);
                }

                // Check global tax setting - if disabled, set tax to 0
                if (!DataBase.IsTaxEnabled)
                {
                    if (row.Cells.Exists("TaxPer"))
                    {
                        row.Cells["TaxPer"].Value = 0;
                    }
                    if (row.Cells.Exists("TaxAmt"))
                    {
                        row.Cells["TaxAmt"].Value = 0;
                    }
                    if (row.Cells.Exists("NetTotal")) row.Cells["NetTotal"].Value = costWithTax * qty;
                    return;
                }

                // Get tax percentage
                if (row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null)
                {
                    float.TryParse(row.Cells["TaxPer"].Value.ToString(), out taxPer);
                }

                // Get tax type
                if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null)
                {
                    taxType = row.Cells["TaxType"].Value.ToString();
                }

                // Calculate total tax amount based on quantity and base cost
                float taxAmt = 0;
                if (!string.IsNullOrEmpty(taxType) && taxPer > 0 && baseCost > 0)
                {
                    // TaxAmt = Qty × BaseCost × (TaxPer / 100)
                    taxAmt = qty * baseCost * (taxPer / 100);
                }

                // Update TaxAmt (BaseCost and Cost remain unchanged - they are per-unit values)
                if (row.Cells.Exists("TaxAmt"))
                {
                    row.Cells["TaxAmt"].Value = taxAmt;
                }

                // Update NetTotal (Qty × Cost)
                float amount = costWithTax * qty;
                if (row.Cells.Exists("NetTotal")) row.Cells["NetTotal"].Value = amount;

                // Update NetAmt for excl tax type: NetAmt = Amount + TaxAmt
                UpdateNetAmtForRow(row);
                // Update Gross for incl tax type: Gross = Amount - TaxAmt
                UpdateGrossForRow(row);

                // Recalculate NewBaseCost (average cost) for all rows
                RecalculateNewBaseCostForAllRows();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error recalculating tax for row: " + ex.Message);
            }
        }

        // Helper method to update NetAmt and show/hide column based on tax type
        private void UpdateNetAmtForRow(UltraGridRow row)
        {
            try
            {
                if (row == null || !row.Cells.Exists("TaxType") || !row.Cells.Exists("TaxAmt") || !row.Cells.Exists("Qty") || !row.Cells.Exists("Cost"))
                    return;

                string taxType = "";
                if (row.Cells["TaxType"].Value != null)
                {
                    taxType = NormalizeTaxType(row.Cells["TaxType"].Value.ToString());
                }

                float qty = 0;
                float cost = 0;
                float taxAmt = 0;

                if (row.Cells["Qty"].Value != null)
                    float.TryParse(row.Cells["Qty"].Value.ToString(), out qty);
                if (row.Cells["Cost"].Value != null)
                    float.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                if (row.Cells["TaxAmt"].Value != null)
                    float.TryParse(row.Cells["TaxAmt"].Value.ToString(), out taxAmt);

                // Calculate local amount from Qty * Cost
                float amount = qty * cost;

                // Calculate and show NetAmt for both excl and incl tax types
                if (row.Cells.Exists("NetAmt"))
                {
                    if (taxType == "excl")
                    {
                        // For excl: NetAmt = Amount + TaxAmt
                        float netAmt = amount + taxAmt;
                        row.Cells["NetAmt"].Value = netAmt;

                        // Set Gross to 0 for excl tax type
                        if (row.Cells.Exists("Gross"))
                        {
                            row.Cells["Gross"].Value = 0f;
                        }
                    }
                    else
                    {
                        // For incl or others: NetAmt = Amount (since Amount includes tax for incl)
                        float netAmt = amount;
                        row.Cells["NetAmt"].Value = netAmt;

                        // Set Gross based on formula (Amount - TaxAmt) for incl, or 0
                        if (row.Cells.Exists("Gross"))
                        {
                            if (taxType == "incl")
                            {
                                // Maintain Gross = Amount - TaxAmt for incl
                                row.Cells["Gross"].Value = amount - taxAmt;
                            }
                            else
                            {
                                row.Cells["Gross"].Value = 0f;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating NetAmt for row: " + ex.Message);
            }
        }

        // Helper method to update NetAmt column visibility based on all rows
        private void UpdateNetAmtColumnVisibility()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                    return;

                UltraGridColumn netAmtCol = ultraGrid1.DisplayLayout.Bands[0].Columns["NetAmt"];
                if (netAmtCol == null)
                    return;

                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    netAmtCol.Hidden = true;
                    return;
                }

                // Always show NetAmt column if there are rows, as it's now used for both Incl and Excl
                netAmtCol.Hidden = false;

                // NOTE: Do NOT recalculate NetAmt values here!
                // NetAmt is loaded from GrandTotal (PMaster) and should not be overwritten
                // The user's edited value must be preserved

                // Also update Amount column caption based on tax type

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating NetAmt column visibility: " + ex.Message);
            }
        }

        // Helper method to update Gross for a row
        private void UpdateGrossForRow(UltraGridRow row)
        {
            try
            {
                if (row == null || !row.Cells.Exists("TaxType") || !row.Cells.Exists("TaxAmt") || !row.Cells.Exists("Qty") || !row.Cells.Exists("Cost"))
                    return;

                string taxType = "";
                if (row.Cells["TaxType"].Value != null)
                {
                    taxType = NormalizeTaxType(row.Cells["TaxType"].Value.ToString());
                }

                float qty = 0;
                float cost = 0;
                float taxAmt = 0;

                if (row.Cells["Qty"].Value != null)
                    float.TryParse(row.Cells["Qty"].Value.ToString(), out qty);
                if (row.Cells["Cost"].Value != null)
                    float.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                if (row.Cells["TaxAmt"].Value != null)
                    float.TryParse(row.Cells["TaxAmt"].Value.ToString(), out taxAmt);

                // Calculate local amount from Qty * Cost
                float amount = qty * cost;

                // Only calculate and show Gross for incl tax type
                if (taxType == "incl" && row.Cells.Exists("Gross"))
                {
                    float gross = amount - taxAmt;
                    row.Cells["Gross"].Value = gross;

                    // Show Gross column if any row has incl tax type
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                    {
                        UltraGridColumn grossCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Gross"];
                        if (grossCol != null)
                        {
                            grossCol.Hidden = false;
                        }
                    }
                }
                else if (row.Cells.Exists("Gross"))
                {
                    // For excl or no tax, set Gross to 0 and hide column if no incl items exist
                    row.Cells["Gross"].Value = 0f;

                    // Check if any other row has incl tax type
                    bool hasInclTax = false;
                    DataTable dt = ultraGrid1.DataSource as DataTable;
                    if (dt != null)
                    {
                        foreach (DataRow dataRow in dt.Rows)
                        {
                            if (dataRow["TaxType"] != null && dataRow["TaxType"] != DBNull.Value)
                            {
                                string rowTaxType = NormalizeTaxType(dataRow["TaxType"].ToString());
                                if (rowTaxType == "incl")
                                {
                                    hasInclTax = true;
                                    break;
                                }
                            }
                        }
                    }

                    // Hide Gross column if no incl items exist
                    if (!hasInclTax && ultraGrid1.DisplayLayout.Bands.Count > 0)
                    {
                        UltraGridColumn grossCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Gross"];
                        if (grossCol != null)
                        {
                            grossCol.Hidden = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating Gross for row: " + ex.Message);
            }
        }

        // Helper method to update Gross column visibility based on all rows
        private void UpdateGrossColumnVisibility()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                    return;

                UltraGridColumn grossCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Gross"];
                if (grossCol == null)
                    return;

                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    grossCol.Hidden = true;
                    return;
                }

                // Check if any row has incl tax type
                bool hasInclTax = false;
                foreach (DataRow dataRow in dt.Rows)
                {
                    if (dataRow["TaxType"] != null && dataRow["TaxType"] != DBNull.Value)
                    {
                        string rowTaxType = NormalizeTaxType(dataRow["TaxType"].ToString());
                        if (rowTaxType == "incl")
                        {
                            hasInclTax = true;
                            break;
                        }
                    }
                }

                // Show/hide column based on whether any incl items exist
                grossCol.Hidden = !hasInclTax;

                // Also update Gross values for all rows
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    UpdateGrossForRow(row);
                }

                // Also update Amount column caption based on tax type

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating Gross column visibility: " + ex.Message);
            }
        }


        // Handle Free quantity changes: If Free >= 1, set Qty = Free and Cost = 0
        private void HandleFreeQuantityChange(UltraGridRow row, float freeValue)
        {
            try
            {
                if (row == null || !row.Cells.Exists("Free") || !row.Cells.Exists("Qty") || !row.Cells.Exists("Cost"))
                    return;

                // If Free >= 1, set Qty = Free and Cost = 0
                if (freeValue >= 1)
                {
                    // Update Qty to match Free
                    row.Cells["Qty"].Value = freeValue;

                    // Set Cost and BaseCost to 0
                    row.Cells["Cost"].Value = 0f;
                    row.Cells["BaseCost"].Value = 0f;
                    row.Cells["TaxAmt"].Value = 0f;
                    row.Cells["TaxPer"].Value = 0f;

                    // Amount and TotalAmount removed from grid

                    // Set NetAmt and Gross to 0
                    if (row.Cells.Exists("NetAmt"))
                        row.Cells["NetAmt"].Value = 0f;
                    if (row.Cells.Exists("Gross"))
                        row.Cells["Gross"].Value = 0f;

                    // Recalculate NewBaseCost (average cost) for all rows
                    RecalculateNewBaseCostForAllRows();
                }
                else
                {
                    // If Free < 1, reset to normal behavior
                    // Don't automatically change Qty or Cost - let user manage it
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error handling free quantity change: " + ex.Message);
            }
        }

        // Helper method to recalculate tax for existing items when quantity is updated
        private void RecalculateTaxForExistingItem(DataRow dataRow, float costWithTax, float qty)
        {
            try
            {
                float taxPer = 0;
                float baseCost = 0;
                string taxType = "";

                // Get existing per-unit BaseCost (should remain unchanged)
                if (dataRow["BaseCost"] != null && dataRow["BaseCost"] != DBNull.Value)
                {
                    float.TryParse(dataRow["BaseCost"].ToString(), out baseCost);
                }

                // Check global tax setting - if disabled, set tax to 0
                if (!DataBase.IsTaxEnabled)
                {
                    dataRow["TaxPer"] = 0;
                    dataRow["TaxAmt"] = 0;
                    return;
                }

                // Get tax percentage
                if (dataRow["TaxPer"] != null && dataRow["TaxPer"] != DBNull.Value)
                {
                    float.TryParse(dataRow["TaxPer"].ToString(), out taxPer);
                }

                // Get tax type
                if (dataRow["TaxType"] != null && dataRow["TaxType"] != DBNull.Value)
                {
                    taxType = dataRow["TaxType"].ToString();
                }

                // Calculate total tax amount based on quantity and base cost
                float taxAmt = 0;
                if (!string.IsNullOrEmpty(taxType) && taxPer > 0 && baseCost > 0)
                {
                    // TaxAmt = Qty × BaseCost × (TaxPer / 100)
                    taxAmt = qty * baseCost * (taxPer / 100);
                }

                // Update TaxAmt (BaseCost and Cost remain unchanged - they are per-unit values)
                dataRow["TaxAmt"] = taxAmt;

                // Recalculate NetAmt and Gross for the updated quantity
                float amount = qty * costWithTax;
                string normalizedTaxType = NormalizeTaxType(taxType);

                if (dataRow.Table.Columns.Contains("NetAmt"))
                {
                    if (normalizedTaxType == "excl")
                    {
                        dataRow["NetAmt"] = amount + taxAmt;
                    }
                    else
                    {
                        dataRow["NetAmt"] = amount;
                    }
                }

                if (dataRow.Table.Columns.Contains("Gross"))
                {
                    if (normalizedTaxType == "incl")
                    {
                        dataRow["Gross"] = amount - taxAmt;
                    }
                    else if (normalizedTaxType == "excl")
                    {
                        dataRow["Gross"] = amount;
                    }
                    else
                    {
                        dataRow["Gross"] = 0f;
                    }
                }

                // Recalculate NewBaseCost (average cost) for all rows
                RecalculateNewBaseCostForAllRows();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error recalculating tax for existing item: " + ex.Message);
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle key press events
            if (e.KeyCode == Keys.Enter)
            {
                // Move to the next cell or row
                UltraGrid grid = sender as UltraGrid;
                // ... (rest of Enter logic) ...
                if (grid != null && grid.ActiveCell != null)
                {
                    // Force an immediate update if the user presses Enter while editing NetAmt
                    if (grid.ActiveCell.Column.Key == "NetAmt" && grid.ActiveCell.IsInEditMode)
                    {
                        grid.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);
                    }

                    UltraGridCell nextCell = FindNextEditableCell(grid.ActiveRow, grid.ActiveCell);
                    // ...
                    if (nextCell != null)
                    {
                        grid.ActiveCell = nextCell;

                        // Enter edit mode for the next cell automatically if it's not a checkbox
                        if (nextCell.Column.DataType != typeof(bool))
                        {
                            grid.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.EnterEditMode);
                        }

                        e.Handled = true;
                    }
                    else if (grid.Rows.Count > 0 && grid.ActiveRow.Index < grid.Rows.Count - 1)
                    {
                        // Move to first editable cell in next row
                        UltraGridRow nextRow = grid.Rows[grid.ActiveRow.Index + 1];
                        UltraGridCell firstCell = FindFirstEditableCell(nextRow);
                        if (firstCell != null)
                        {
                            grid.ActiveCell = firstCell;
                            e.Handled = true;
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Remove selected item when Delete is pressed
                RemoveSelectedItem();
                e.Handled = true;
            }
        }

        private UltraGridCell FindNextEditableCell(UltraGridRow row, UltraGridCell currentCell)
        {
            if (row == null || currentCell == null)
                return null;

            bool foundCurrent = false;

            // Go through all cells in the row
            foreach (UltraGridCell cell in row.Cells)
            {
                // Skip hidden columns
                if (cell.Column.Hidden)
                    continue;

                // If we already found the current cell, check if this one is editable
                if (foundCurrent)
                {
                    if (cell.Column.CellActivation == Activation.AllowEdit)
                        return cell;
                }
                // Mark when we find the current cell
                else if (cell == currentCell)
                {
                    foundCurrent = true;
                }
            }

            return null;
        }

        private UltraGridCell FindFirstEditableCell(UltraGridRow row)
        {
            if (row == null)
                return null;

            // Go through all cells in the row
            foreach (UltraGridCell cell in row.Cells)
            {
                // Skip hidden columns
                if (cell.Column.Hidden)
                    continue;

                if (cell.Column.CellActivation == Activation.AllowEdit)
                    return cell;
            }

            return null;
        }

        private void FrmPurchase_KeyDown(object sender, KeyEventArgs e)
        {
            // Block modification shortcuts when form is in read-only mode
            if (_isReadOnly)
            {
                bool isModifyingKey = e.KeyCode == Keys.F7   // Item selection
                    || e.KeyCode == Keys.F11  // Vendor dialog
                    || e.KeyCode == Keys.F2   // Row edit
                    || e.KeyCode == Keys.F8   // Save / Update
                    || e.KeyCode == Keys.F12; // Delete

                if (isModifyingKey)
                {
                    MessageBox.Show(MsgReadOnlyBlock, MsgReadOnlyModeTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            if (e.KeyCode == Keys.F5)
            {
                // Toggle NewBaseCost column visibility
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    var col = ultraGrid1.DisplayLayout.Bands[0].Columns["NewBaseCost"];
                    if (col != null)
                    {
                        col.Hidden = !col.Hidden;
                    }
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F7)
            {
                ShowItemSelectionDialog();
            }
            else if (e.KeyCode == Keys.F11)
            {
                ShowVendorDialog();
            }
            else if (e.KeyCode == Keys.F6)
            {
                ShowPurchaseDisplayDialog();
            }
            else if (e.KeyCode == Keys.F1)
            {
                // Clear the form when F1 is pressed (mapped to ultraPictureBox1 action - New)
                this.Clear();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                // Open PurchaseEdit dialog when F2 is pressed (same as ultraPictureBox7.Click)
                if (ultraGrid1.ActiveRow == null)
                {
                    MessageBox.Show("Please select an item to edit", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                PurchaseEdit purchaseEditForm = new PurchaseEdit();
                purchaseEditForm.SetGridRow(ultraGrid1.ActiveRow);

                if (purchaseEditForm.ShowDialog() == DialogResult.OK && purchaseEditForm.IsUpdated)
                {
                    CaluateTotals();
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                // Exit the form (User requested F4 for Exit) - mapped to pbxExit logic
                this.Close();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F8)
            {
                // Force any active edits in the grid to commit
                if (ultraGrid1 != null && ultraGrid1.ActiveCell != null && ultraGrid1.ActiveCell.IsInEditMode)
                {
                    ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);
                    ultraGrid1.UpdateData();
                }

                if (ultraPictureBox4.Visible)
                {
                    try
                    {
                        this.UpdatePurchase();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error triggering update via F8: " + ex.Message);
                    }
                }
                else
                {
                    // Save the purchase (User requested F8 for Save)
                    this.SavePurchase();
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F12)
            {
                // Delete the purchase (Mapped to ultraPictureBox2 action - F12 label)
                this.DeletePurchase();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {
            // We'll only process barcodes when Enter key is pressed
            // No automatic processing while typing
        }

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if a row is selected in the grid
            bool hasSelectedItem = ultraGrid1.ActiveRow != null;

            // Only process commands when Enter key is pressed
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;  // Prevent "ding" sound
                e.Handled = true;

                // Handle special key inputs when a row is selected
                if (hasSelectedItem && txtBarcode.Text != null)
                {
                    string input = txtBarcode.Text.Trim();

                    // Handle "=" key to open Edit form
                    if (input == "=")
                    {
                        if (!EnsurePurchaseRowEditable(ultraGrid1.ActiveRow, true))
                        {
                            this.barcodeFocus();
                            return;
                        }

                        // Open the Edit form with the highlighted item's description
                        Edit editForm = Edit.CreateForPurchaseForm(this);

                        // Show the form
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            // Get the updated description
                            string updatedDescription = editForm.ItemDescription;

                            // Update the description in the grid if it changed
                            if (ultraGrid1.ActiveRow.Cells["Description"].Value.ToString() != updatedDescription)
                            {
                                ultraGrid1.ActiveRow.Cells["Description"].Value = updatedDescription;

                                // Refresh the grid
                                ultraGrid1.Refresh();
                            }
                        }

                        // Clear and focus barcode field after dialog closes
                        this.barcodeFocus();
                        return;
                    }

                    // Handle "u" or "U" key to open UOMeditDig form
                    if (input == "u" || input == "U")
                    {
                        if (!EnsurePurchaseRowEditable(ultraGrid1.ActiveRow, true))
                        {
                            this.barcodeFocus();
                            return;
                        }

                        OpenUnitDialogForPurchaseRow(ultraGrid1.ActiveRow, true);
                        return;
                    }

                    // Handle ".." to update total and show balance
                    // When user types ..{amount}, hide label4 panel, show label6 panel with new amount
                    // label6 = NetTotal (overridden). textBox2 = remaining balance.
                    if (input.StartsWith("..") && input.Length > 2)
                    {
                        string amountStr = input.Substring(2);
                        if (float.TryParse(amountStr, out float newTotal))
                        {
                            // Hide label4 panel, show label6 panel
                            ultraPanel11.Visible = false;
                            ultraPanel12.Visible = true;

                            // Set label6 with the overridden NetTotal amount
                            label6.Text = newTotal.ToString("N2");
                            txtInvoiceAmt.Text = newTotal.ToString("N2");

                            // Calculate remaining balance
                            float remaining = OriginalNetTotal - newTotal;

                            // Display remaining balance in textBox2
                            if (textBox2 != null)
                            {
                                textBox2.Text = remaining.ToString("N2");
                            }

                            // Mark that the user manually overrode the total
                            _isNetTotalManuallySet = true;

                            this.barcodeFocus();
                            return;
                        }
                    }

                    // Handle "." to delete the highlighted item
                    if (input == ".")
                    {
                        RemoveSelectedItem();
                        this.barcodeFocus();
                        return;
                    }

                    // Handle ".number" to set cost value
                    if (input.StartsWith(".") && input.Length > 1)
                    {
                        if (ultraGrid1.ActiveRow != null && !EnsurePurchaseRowEditable(ultraGrid1.ActiveRow, true))
                        {
                            this.barcodeFocus();
                            return;
                        }

                        // Extract the number after the dot
                        string numberStr = input.Substring(1);
                        float newCost;

                        if (float.TryParse(numberStr, out newCost) && newCost > 0)
                        {
                            // Set the cost cell value
                            ultraGrid1.ActiveRow.Cells["Cost"].Value = newCost;

                            // Get quantity
                            float quantity = 1;
                            if (ultraGrid1.ActiveRow.Cells["Qty"] != null &&
                                ultraGrid1.ActiveRow.Cells["Qty"].Value != null)
                            {
                                float.TryParse(ultraGrid1.ActiveRow.Cells["Qty"].Value.ToString(), out quantity);
                            }
                            if (quantity <= 0) quantity = 1;

                            // Get tax info
                            float taxPer = 0;
                            string taxType = "";
                            if (ultraGrid1.ActiveRow.Cells.Exists("TaxPer") && ultraGrid1.ActiveRow.Cells["TaxPer"].Value != null)
                                float.TryParse(ultraGrid1.ActiveRow.Cells["TaxPer"].Value.ToString(), out taxPer);
                            if (ultraGrid1.ActiveRow.Cells.Exists("TaxType") && ultraGrid1.ActiveRow.Cells["TaxType"].Value != null)
                                taxType = NormalizeTaxType(ultraGrid1.ActiveRow.Cells["TaxType"].Value.ToString());

                            // Calculate Amount = Cost * Qty (Amount/TotalAmount removed from grid)
                            float newAmount = newCost * quantity;

                            // Calculate Tax Amount and related values based on tax type
                            float newTaxAmt = 0;
                            float newNetAmt = 0;
                            float newGross = 0;

                            if (taxType == "excl" && taxPer > 0)
                            {
                                // For excl: Tax is added on top
                                // TaxAmt = Amount × TaxPer / 100
                                newTaxAmt = newAmount * taxPer / 100;
                                // NetAmt = Amount + TaxAmt
                                newNetAmt = newAmount + newTaxAmt;
                                // Gross = Amount (same as Amount for excl)
                                newGross = newAmount;
                            }
                            else if (taxType == "incl" && taxPer > 0)
                            {
                                // For incl: Tax is included in Amount
                                // TaxAmt = Amount × TaxPer / (100 + TaxPer)
                                newTaxAmt = newAmount * taxPer / (100 + taxPer);
                                // NetAmt = Amount (since Amount includes tax)
                                newNetAmt = newAmount;
                                // Gross = Amount - TaxAmt
                                newGross = newAmount - newTaxAmt;
                            }
                            else
                            {
                                // No tax
                                newTaxAmt = 0;
                                newNetAmt = newAmount;
                                newGross = 0;
                            }

                            // Update cells
                            if (ultraGrid1.ActiveRow.Cells.Exists("TaxAmt"))
                                ultraGrid1.ActiveRow.Cells["TaxAmt"].Value = newTaxAmt;
                            if (ultraGrid1.ActiveRow.Cells.Exists("NetAmt"))
                                ultraGrid1.ActiveRow.Cells["NetAmt"].Value = newNetAmt;
                            if (ultraGrid1.ActiveRow.Cells.Exists("Gross"))
                                ultraGrid1.ActiveRow.Cells["Gross"].Value = newGross;

                            // Recalculate totals
                            this.CaluateTotals();
                        }

                        this.barcodeFocus();
                        return;
                    }

                    // Handle "*number" to change quantity
                    if (input.StartsWith("*") && input.Length > 1)
                    {
                        if (ultraGrid1.ActiveRow != null && !EnsurePurchaseRowEditable(ultraGrid1.ActiveRow, true))
                        {
                            this.barcodeFocus();
                            return;
                        }

                        // Extract the number after the asterisk
                        string numberStr = input.Substring(1);
                        float newQty;

                        if (float.TryParse(numberStr, out newQty))
                        {
                            // Validation: Never allow qty to be 0 or negative
                            if (newQty <= 0)
                            {
                                // Ignore *0 or negative values - don't make any changes
                                this.barcodeFocus();
                                return;
                            }

                            // Update quantity
                            ultraGrid1.ActiveRow.Cells["Qty"].Value = newQty;

                            // Get cost with tax and recalculate everything
                            float costWithTax = 0;
                            if (ultraGrid1.ActiveRow.Cells["Cost"] != null &&
                                ultraGrid1.ActiveRow.Cells["Cost"].Value != null)
                            {
                                float.TryParse(ultraGrid1.ActiveRow.Cells["Cost"].Value.ToString(), out costWithTax);
                            }

                            // Recalculate tax, base cost, and amounts for the updated quantity
                            RecalculateTaxForRow(ultraGrid1.ActiveRow, costWithTax, newQty);

                            // Recalculate totals
                            this.CaluateTotals();
                        }

                        this.barcodeFocus();
                        return;
                    }
                }

                // If we get here, process as a regular barcode
                ProcessBarcode(txtBarcode.Text);
                return;
            }
            // Handle Delete key press directly (without requiring Enter)
            else if (e.KeyCode == Keys.Delete && hasSelectedItem)
            {
                e.SuppressKeyPress = true;  // Prevent "ding" sound
                e.Handled = true;

                // Remove the selected item
                RemoveSelectedItem();
                return;
            }
        }

        private void ProcessBarcode(string barcode)
        {
            // Check for special format (quantity * barcode)
            if (barcode.Contains('*'))
            {
                string[] strRow = barcode.Split('*');

                // Format: quantity*barcode
                if (strRow.Length == 2 && !string.IsNullOrEmpty(strRow[0]) && !string.IsNullOrEmpty(strRow[1]))
                {
                    float quantity;
                    if (float.TryParse(strRow[0], out quantity))
                    {
                        int existingItemIndex = -1;
                        DataBase.Operations = "BARCODEPURCHASE";
                        ItemDDlGrid itemDataResult = drop.itemDDlGrid(strRow[1], null);

                        if (itemDataResult != null && itemDataResult.List != null && itemDataResult.List.Count() > 0)
                        {
                            TryUpdateExistingItem(itemDataResult, strRow[1], out existingItemIndex, quantity);

                            if (CheckExists && existingItemIndex >= 0)
                            {
                                // Item already exists, update was already done in TryUpdateExistingItem
                                if (existingItemIndex < ultraGrid1.Rows.Count)
                                {
                                    ultraGrid1.ActiveRow = ultraGrid1.Rows[existingItemIndex];
                                    ultraGrid1.Selected.Rows.Clear();
                                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                                }
                            }
                            else
                            {
                                AddItemToGrid(itemDataResult, quantity);
                            }
                        }

                        CheckExists = false;
                        this.barcodeFocus();
                        return;
                    }
                }
                // Format: *rowIndex (for row selection)
                else if (strRow[0] == "" && strRow.Length == 2)
                {
                    // Existing row selection logic
                    int rowIndex;
                    if (int.TryParse(strRow[1], out rowIndex) && rowIndex > 0 && rowIndex <= ultraGrid1.Rows.Count)
                    {
                        ultraGrid1.Rows[rowIndex - 1].Selected = true;
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[rowIndex - 1];
                        if (ultraGrid1.ActiveRow.Cells.Exists("Qty"))
                        {
                            ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Qty"];
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        }
                        this.barcodeFocus();
                        return;
                    }
                }
            }

            // Standard barcode processing
            DataBase.Operations = "BARCODEPURCHASE";
            ItemDDlGrid itemDataResult2 = drop.itemDDlGrid(barcode, null);

            if (itemDataResult2 != null && itemDataResult2.List != null && itemDataResult2.List.Any())
            {
                int count = itemDataResult2.List.Count();
                if (count == 1)
                {
                    // Store the found barcode index before checking if it exists
                    int existingItemIndex = -1;
                    TryUpdateExistingItem(itemDataResult2, barcode, out existingItemIndex);

                    if (!CheckExists)
                    {
                        // Add new item
                        AddItemToGrid(itemDataResult2, 1);
                    }
                    else if (existingItemIndex >= 0)
                    {
                        // Highlight the updated row
                        if (existingItemIndex < ultraGrid1.Rows.Count)
                        {
                            ultraGrid1.ActiveRow = ultraGrid1.Rows[existingItemIndex];
                            ultraGrid1.Selected.Rows.Clear();
                            ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                        }
                    }
                    CheckExists = false;
                }
                else if (count > 1)
                {
                    // Multiple matches found — take the first match directly (like frmSalesInvoice)
                    AddItemToGrid(itemDataResult2, 1);
                }
                // If count == 0, try fallback lookup using GETITEMBYBARCODE
            }
            else
            {
                // Fallback: try GETITEMBYBARCODE operation (same as frmSalesInvoice)
                DataBase.Operations = "GETITEMBYBARCODE";
                ItemDDlGrid fallbackResult = drop.itemDDlGrid(barcode, "");
                if (fallbackResult != null && fallbackResult.List != null && fallbackResult.List.Count() > 0)
                {
                    int existingItemIndex = -1;
                    TryUpdateExistingItem(fallbackResult, barcode, out existingItemIndex);
                    if (!CheckExists)
                    {
                        AddItemToGrid(fallbackResult, 1);
                    }
                    else if (existingItemIndex >= 0 && existingItemIndex < ultraGrid1.Rows.Count)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[existingItemIndex];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                    CheckExists = false;
                }
            }

            // Clear and focus for next scan
            this.barcodeFocus();
        }

        private bool TryUpdateExistingItem(ItemDDlGrid itemData, string scannedBarcode, out int existingItemIndex, float additionalQty = 1)
        {
            existingItemIndex = -1;
            CheckBarcode(scannedBarcode, out existingItemIndex, additionalQty);
            if (CheckExists)
            {
                return true;
            }

            ItemDDl matchedItem = itemData?.List?.FirstOrDefault();
            if (matchedItem == null)
            {
                return false;
            }

            if (!EnsureItemAllowedForPurchase(matchedItem))
            {
                CheckExists = true;
                existingItemIndex = int.MaxValue;
                return true;
            }

            return TryUpdateExistingItemByIdentity(matchedItem.ItemId, matchedItem.UnitId, out existingItemIndex, additionalQty);
        }

        private bool TryUpdateExistingItemByIdentity(int itemId, int unitId, out int existingItemIndex, float additionalQty = 1)
        {
            existingItemIndex = -1;
            CheckExists = false;

            if (itemId <= 0)
            {
                return false;
            }

            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int rowItemId = 0;
                int rowUnitId = 0;
                int.TryParse(dt.Rows[i]["ItemId"]?.ToString(), out rowItemId);

                if (dt.Columns.Contains("UnitId"))
                {
                    int.TryParse(dt.Rows[i]["UnitId"]?.ToString(), out rowUnitId);
                }

                if (rowItemId != itemId)
                {
                    continue;
                }

                if (unitId > 0 && rowUnitId > 0 && rowUnitId != unitId)
                {
                    continue;
                }

                CheckExists = true;
                existingItemIndex = i;

                float qty = Convert.ToSingle(dt.Rows[i]["Qty"]);
                float newQty = qty + additionalQty;
                dt.Rows[i]["Qty"] = newQty;

                float costWithTax = 0;
                if (float.TryParse(dt.Rows[i]["Cost"]?.ToString(), out costWithTax))
                {
                    RecalculateTaxForExistingItem(dt.Rows[i], costWithTax, newQty);
                }

                this.CaluateTotals();
                this.barcodeFocus();
                return true;
            }

            return false;
        }

        private void AddItemToGrid(ItemDDlGrid itemData, float quantity)
        {
            try
            {
                if (_isReadOnly)
                {
                    MessageBox.Show(MsgReadOnlyBlock, MsgReadOnlyModeTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the DataTable from the UltraGrid
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null) return;

                // Add a new row
                DataRow newRow = dt.NewRow();

                var itm = itemData.List.FirstOrDefault();
                if (itm != null)
                {
                    if (!EnsureItemAllowedForPurchase(itm))
                    {
                        return;
                    }

                    newRow["SLNO"] = dt.Rows.Count + 1;
                    newRow["ItemId"] = itm.ItemId;
                    newRow["BarCode"] = itm.BarCode;
                    newRow["Description"] = itm.Description;
                    newRow["Unit"] = itm.Unit;
                    // Default packing to base unit (1)
                    newRow["Packing"] = 1;
                    newRow["RetailPrice"] = itm.RetailPrice;
                    newRow["Free"] = 0;

                    // Get WholeSalePrice from PriceSettings table (remapped from PDetails.WholeSalePrice)
                    float wholeSalePriceFromPriceSettings = GetWholeSalePriceFromPriceSettings(itm.ItemId, itm.UnitId);
                    newRow["SellingPrice"] = wholeSalePriceFromPriceSettings > 0
                        ? wholeSalePriceFromPriceSettings
                        : itm.WholeSalePrice; // Fallback to item data if PriceSettings not found

                    // Get Unit 1's WholeSalePrice from PriceSettings for UnitSP column
                    // Use same fallback logic as SellingPrice (item's WholeSalePrice) if Unit 1's PriceSettings not found
                    float unit1WholeSalePrice = GetWholeSalePriceFromPriceSettings(itm.ItemId, 1);
                    newRow["UnitSP"] = unit1WholeSalePrice > 0 ? unit1WholeSalePrice : (float)itm.WholeSalePrice;

                    // Calculate tax and cost structure based on tax type
                    double costWithTax = itm.Cost;
                    double baseCostWithoutTax = costWithTax;
                    double calculatedTaxAmt = 0;
                    double finalTaxPer = 0;
                    string taxType = itm.TaxType ?? string.Empty;

                    // Normalize tax type from database
                    string normalizedTaxType = NormalizeTaxType(taxType);

                    // Check global tax setting - if disabled, set tax to 0
                    if (!DataBase.IsTaxEnabled)
                    {
                        calculatedTaxAmt = 0;
                        finalTaxPer = 0;
                        baseCostWithoutTax = costWithTax;
                    }
                    else if (!string.IsNullOrEmpty(normalizedTaxType) && itm.TaxPer > 0)
                    {
                        finalTaxPer = itm.TaxPer;
                        if (normalizedTaxType == "incl")
                        {
                            // Tax is included in the cost
                            calculatedTaxAmt = costWithTax * (itm.TaxPer / (100 + itm.TaxPer));
                            baseCostWithoutTax = costWithTax - calculatedTaxAmt;
                        }
                        else if (normalizedTaxType == "excl")
                        {
                            // Tax is exclusive (added on top)
                            calculatedTaxAmt = costWithTax * (itm.TaxPer / 100);
                            baseCostWithoutTax = costWithTax;
                        }
                    }

                    newRow["BaseCost"] = baseCostWithoutTax;
                    newRow["Cost"] = costWithTax;
                    newRow["Qty"] = quantity;

                    // Hidden columns - make sure UnitId is properly set
                    newRow["UnitId"] = itm.UnitId;
                    newRow["UnitPrize"] = itm.RetailPrice;
                    newRow["MarginPer"] = itm.MarginPer;
                    newRow["MarginAmt"] = itm.MarginAmt;
                    newRow["TaxPer"] = finalTaxPer;
                    newRow["TaxAmt"] = calculatedTaxAmt; // Use calculated tax amount
                    newRow["TaxType"] = normalizedTaxType; // Save normalized tax type
                    newRow["WholeSalePrice"] = itm.WholeSalePrice;
                    newRow["CreditPrice"] = itm.CreditPrice;
                    newRow["CardPrice"] = itm.CardPrice;
                    newRow["NewBaseCost"] = 0f; // Will be recalculated after row is added
                    ApplyItemStatusToPurchaseRow(newRow, CreatePurchaseItemStatus(itm));

                    // Calculate NetAmt and Gross based on tax type
                    double amount = costWithTax * quantity;
                    if (normalizedTaxType == "excl")
                    {
                        newRow["NetAmt"] = amount + calculatedTaxAmt;
                        newRow["Gross"] = amount;
                    }
                    else if (normalizedTaxType == "incl")
                    {
                        newRow["NetAmt"] = amount;
                        newRow["Gross"] = amount - calculatedTaxAmt;
                    }
                    else
                    {
                        newRow["NetAmt"] = amount;
                        newRow["Gross"] = 0.0;
                    }
                }

                // Add the row to the DataTable
                dt.Rows.Add(newRow);

                // Select the newly added row
                int newRowIndex = dt.Rows.Count - 1;
                if (ultraGrid1.Rows.Count > newRowIndex)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[newRowIndex];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                }

                // Recalculate NewBaseCost (average cost) for all rows
                RecalculateNewBaseCostForAllRows();

                // Calculate totals
                this.CaluateTotals();
                ultraGrid1.Rows.Refresh(RefreshRow.FireInitializeRow);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error adding item to grid: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SavePurchase();
        }

        public void SavePurchase()
        {
            if (_isReadOnly)
            {
                MessageBox.Show(MsgReadOnlyBlock, MsgReadOnlyModeTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!ShiftSessionGuard.CanDoTransaction(out string transactionError))
                {
                    MessageBox.Show(transactionError, "Shift Closing Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to purchase", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtInvoiceNo.Text))
                {
                    MessageBox.Show("Please enter invoice number", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtInvoiceNo.Focus();
                    return;
                }

                if (CmboVendor.Value == null)
                {
                    MessageBox.Show("Please select a vendor", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CmboVendor.Focus();
                    return;
                }

                if (CmboPayment.Value == null)
                {
                    MessageBox.Show("Please select a payment mode", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CmboPayment.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtBilledBy.Text))
                {
                    MessageBox.Show("Please enter billed by information", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtBilledBy.Focus();
                    return;
                }

                // Store vendor ID in textBox1
                if (CmboVendor.Value != null)
                {
                    textBox1.Text = CmboVendor.Value.ToString();
                }

                // Setup purchase master data
                ObjPurchaseMaster.BranchId = GetBranchId();
                ObjPurchaseMaster.CompanyId = GetCompanyId();
                // Get FinYearId - returns 0 if not found, repository will handle getting it from database
                ObjPurchaseMaster.FinYearId = GetFinYearId();
                ObjPurchaseMaster.BranchName = CmboBranch.Text;
                ObjPurchaseMaster.PurchaseDate = Convert.ToDateTime(dtpPurchaseDate.Value);
                ObjPurchaseMaster.InvoiceNo = txtInvoiceNo.Text;
                ObjPurchaseMaster.InvoiceDate = Convert.ToDateTime(DtpInoviceDate.Value);
                ObjPurchaseMaster.LedgerID = Convert.ToInt32(CmboVendor.Value);
                ObjPurchaseMaster.VendorName = CmboVendor.Text;
                ObjPurchaseMaster.PaymodeID = Convert.ToInt32(CmboPayment.Value);
                ObjPurchaseMaster.Paymode = CmboPayment.Text;
                ObjPurchaseMaster.PaymodeLedgerID = 0;
                ObjPurchaseMaster.CreditPeriod = 0;
                ObjPurchaseMaster.SubTotal = SubTotal;
                ObjPurchaseMaster.SpDisPer = 0;
                ObjPurchaseMaster.SpDsiAmt = 0;
                ObjPurchaseMaster.BillDiscountPer = 0;
                ObjPurchaseMaster.BillDiscountAmt = _isNetTotalManuallySet
                    ? (double.TryParse(textBox2.Text, out double saveBd) ? saveBd : 0)
                    : 0;
                ObjPurchaseMaster.TaxPer = 0;
                ObjPurchaseMaster.TaxAmt = 0;
                ObjPurchaseMaster.Frieght = 0;
                ObjPurchaseMaster.ExpenseAmt = 0;
                ObjPurchaseMaster.OtherExpAmt = 0;
                ObjPurchaseMaster.GrandTotal = NetTotal;
                ObjPurchaseMaster.CancelFlag = false;
                ObjPurchaseMaster.UserID = GetUserId();
                ObjPurchaseMaster.UserName = GetUserName();
                ObjPurchaseMaster.TaxType = "I";
                ObjPurchaseMaster.Remarks = txtRemark != null ? txtRemark.Text : "";
                ObjPurchaseMaster.RoundOff = string.IsNullOrWhiteSpace(txtRoundOff.Text) ?
                    0 : (float.TryParse(txtRoundOff.Text, out float roundOff) ? roundOff : 0);
                ObjPurchaseMaster.CessPer = 0;
                ObjPurchaseMaster.CessAmt = 0;
                ObjPurchaseMaster.CalAfterTax = 0;
                // Set currency information dynamically
                SetCurrencyInfo(ObjPurchaseMaster);
                ObjPurchaseMaster.SeriesID = 0;
                // VoucherID will be generated dynamically by the repository
                ObjPurchaseMaster.VoucherID = 0;
                ObjPurchaseMaster.IsSyncd = false;
                ObjPurchaseMaster.Paid = false;
                ObjPurchaseMaster.Pid = 0;
                ObjPurchaseMaster.POrderMasterId = 0;

                ObjPurchaseMaster.BilledBy = txtBilledBy.Text;
                ObjPurchaseMaster.TrnsType = "Purchase";

                // NetTotal: if user overrode via ".." shortcut, save label6 value; otherwise same as GrandTotal
                ObjPurchaseMaster.NetTotal = _isNetTotalManuallySet
                    ? (double.TryParse(label6.Text, out double saveNt) ? saveNt : NetTotal)
                    : NetTotal;

                // Get data from ultraGrid1
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No items to save", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create temporary DataGridView to pass to the repository
                DataGridView tempGridView = new DataGridView();
                tempGridView.AllowUserToAddRows = false; // Prevent empty "new row" from being added
                tempGridView.AutoGenerateColumns = false;

                // Add the required columns (including BaseCost to preserve tax-excluded cost)
                tempGridView.Columns.Add("SLNO", "SLNO");
                tempGridView.Columns.Add("ItemId", "ItemId");
                tempGridView.Columns.Add("BarCode", "BarCode");
                tempGridView.Columns.Add("Description", "Description");
                tempGridView.Columns.Add("BaseCost", "BaseCost"); // Add BaseCost to save tax-excluded cost
                tempGridView.Columns.Add("Cost", "Cost");
                tempGridView.Columns.Add("UnitId", "UnitId");
                tempGridView.Columns.Add("Unit", "Unit");
                tempGridView.Columns.Add("Qty", "Qty");
                tempGridView.Columns.Add("Free", "Free");
                tempGridView.Columns.Add("UnitPrize", "UnitPrize");
                tempGridView.Columns.Add("Packing", "Packing");
                tempGridView.Columns.Add("MarginPer", "MarginPer");
                tempGridView.Columns.Add("MarginAmt", "MarginAmt");
                tempGridView.Columns.Add("TaxPer", "TaxPer");
                tempGridView.Columns.Add("TaxAmt", "TaxAmt");
                tempGridView.Columns.Add("TaxType", "TaxType");
                tempGridView.Columns.Add("RetailPrice", "RetailPrice");
                tempGridView.Columns.Add("WholeSalePrice", "WholeSalePrice");
                tempGridView.Columns.Add("CreditPrice", "CreditPrice");
                tempGridView.Columns.Add("CardPrice", "CardPrice");
                tempGridView.Columns.Add("NetAmt", "NetAmt");
                tempGridView.Columns.Add("Gross", "Gross");

                // Transfer data from UltraGrid (via DataTable) to DataGridView
                int slnoCounter = 1;
                foreach (DataRow row in dt.Rows)
                {
                    // Skip rows with null or invalid ItemId
                    if (row["ItemId"] == null || row["ItemId"] == DBNull.Value)
                        continue;

                    int itemIdValue = 0;
                    if (!int.TryParse(row["ItemId"].ToString(), out itemIdValue) || itemIdValue <= 0)
                        continue;

                    int index = tempGridView.Rows.Add();
                    tempGridView.Rows[index].Cells["SLNO"].Value = slnoCounter++;
                    tempGridView.Rows[index].Cells["ItemId"].Value = itemIdValue;
                    tempGridView.Rows[index].Cells["BarCode"].Value = row["BarCode"] != DBNull.Value ? row["BarCode"] : "";
                    tempGridView.Rows[index].Cells["Description"].Value = row["Description"] != DBNull.Value ? row["Description"] : "";
                    tempGridView.Rows[index].Cells["BaseCost"].Value = row["BaseCost"] != DBNull.Value ? row["BaseCost"] : 0f;
                    tempGridView.Rows[index].Cells["Cost"].Value = row["Cost"] != DBNull.Value ? row["Cost"] : 0f;
                    tempGridView.Rows[index].Cells["UnitId"].Value = row["UnitId"] != DBNull.Value ? row["UnitId"] : 0;
                    tempGridView.Rows[index].Cells["Unit"].Value = row["Unit"] != DBNull.Value ? row["Unit"] : "";
                    tempGridView.Rows[index].Cells["Qty"].Value = row["Qty"] != DBNull.Value ? row["Qty"] : 0f;
                    tempGridView.Rows[index].Cells["Free"].Value = row["Free"] != DBNull.Value ? row["Free"] : 0f;
                    tempGridView.Rows[index].Cells["UnitPrize"].Value = row["UnitPrize"] != DBNull.Value ? row["UnitPrize"] : 0f;
                    tempGridView.Rows[index].Cells["Packing"].Value = row["Packing"] != DBNull.Value ? row["Packing"] : 0f;
                    tempGridView.Rows[index].Cells["MarginPer"].Value = row["MarginPer"] != DBNull.Value ? row["MarginPer"] : 0f;
                    tempGridView.Rows[index].Cells["MarginAmt"].Value = row["MarginAmt"] != DBNull.Value ? row["MarginAmt"] : 0f;
                    tempGridView.Rows[index].Cells["TaxPer"].Value = row["TaxPer"] != DBNull.Value ? row["TaxPer"] : 0f;
                    tempGridView.Rows[index].Cells["TaxAmt"].Value = row["TaxAmt"] != DBNull.Value ? row["TaxAmt"] : 0f;
                    tempGridView.Rows[index].Cells["TaxType"].Value = row["TaxType"] != DBNull.Value ? row["TaxType"] : "I";
                    tempGridView.Rows[index].Cells["RetailPrice"].Value = row["RetailPrice"] != DBNull.Value ? row["RetailPrice"] : 0f;
                    // Use SellingPrice from grid if available, otherwise fall back to WholeSalePrice
                    if (row.Table.Columns.Contains("SellingPrice") && row["SellingPrice"] != null && row["SellingPrice"] != DBNull.Value)
                    {
                        tempGridView.Rows[index].Cells["WholeSalePrice"].Value = row["SellingPrice"];
                    }
                    else
                    {
                        tempGridView.Rows[index].Cells["WholeSalePrice"].Value = row["WholeSalePrice"] != DBNull.Value ? row["WholeSalePrice"] : 0f;
                    }
                    tempGridView.Rows[index].Cells["CreditPrice"].Value = row["CreditPrice"] != DBNull.Value ? row["CreditPrice"] : 0f;
                    tempGridView.Rows[index].Cells["CardPrice"].Value = row["CardPrice"] != DBNull.Value ? row["CardPrice"] : 0f;
                    tempGridView.Rows[index].Cells["NetAmt"].Value = row.Table.Columns.Contains("NetAmt") && row["NetAmt"] != null && row["NetAmt"] != DBNull.Value
                        ? row["NetAmt"]
                        : 0f;
                    tempGridView.Rows[index].Cells["Gross"].Value = row.Table.Columns.Contains("Gross") && row["Gross"] != null && row["Gross"] != DBNull.Value
                        ? row["Gross"]
                        : 0f;
                }

                // Set up purchase details with first item
                if (dt.Rows.Count > 0)
                {
                    // Setup the purchase details object with the first item's data
                    ObjPurchaseDetails = new PurchaseDetails();
                    ObjPurchaseDetails._Operation = "CREATE";
                    ObjPurchaseDetails.CompanyId = GetCompanyId();
                    ObjPurchaseDetails.BranchID = GetBranchId();
                    // Use FinYearId from ObjPurchaseMaster (already set with error handling above)
                    ObjPurchaseDetails.FinYearId = ObjPurchaseMaster.FinYearId;
                    ObjPurchaseDetails.BranchName = CmboBranch.Text;
                    ObjPurchaseDetails.PurchaseDate = Convert.ToDateTime(dtpPurchaseDate.Value);
                    ObjPurchaseDetails.InvoiceNo = txtInvoiceNo.Text;
                    ObjPurchaseDetails.SlNo = Convert.ToInt32(dt.Rows[0]["SLNO"]);
                    ObjPurchaseDetails.ItemID = Convert.ToInt32(dt.Rows[0]["ItemId"]);
                    ObjPurchaseDetails.ItemName = dt.Rows[0]["Description"].ToString();
                    ObjPurchaseDetails.UnitId = Convert.ToInt32(dt.Rows[0]["UnitId"]);
                    ObjPurchaseDetails.Unit = dt.Rows[0]["Unit"].ToString();
                    ObjPurchaseDetails.BaseUnit = "Y";
                    ObjPurchaseDetails.Packing = float.Parse(dt.Rows[0]["Packing"].ToString());
                    ObjPurchaseDetails.Qty = float.Parse(dt.Rows[0]["Qty"].ToString());
                    ObjPurchaseDetails.Cost = float.Parse(dt.Rows[0]["Cost"].ToString());
                    ObjPurchaseDetails.Free = float.Parse(dt.Rows[0]["Free"].ToString());
                    ObjPurchaseDetails.TaxPer = float.Parse(dt.Rows[0]["TaxPer"].ToString());
                    ObjPurchaseDetails.TaxAmt = float.Parse(dt.Rows[0]["TaxAmt"].ToString());
                    ObjPurchaseDetails.TaxType = dt.Rows[0]["TaxType"].ToString();
                }

                // Check CASH-IN-HAND balance if payment mode is Cash
                if (ObjPurchaseInviceRepo.IsCashPaymentMode(ObjPurchaseMaster.PaymodeID, ObjPurchaseMaster.Paymode))
                {
                    double availableCash = ObjPurchaseInviceRepo.GetAvailableCashBalance(GetBranchId());
                    double purchaseAmt = ObjPurchaseMaster.GrandTotal;
                    if (availableCash - purchaseAmt < 0)
                    {
                        MessageBox.Show($"Insufficient CASH-IN-HAND balance!\n\nCurrent Cash-In-Hand Balance: {availableCash:N2}\nPurchase Amount: {purchaseAmt:N2}\nAvailable Cash after Purchase: {(availableCash - purchaseAmt):N2}\n\nTransaction cannot be saved because CASH-IN-HAND balance cannot be negative.", "Insufficient Cash Balance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Check CASH-IN-HAND balance if payment mode is Cash
                if (ObjPurchaseInviceRepo.IsCashPaymentMode(ObjPurchaseMaster.PaymodeID, ObjPurchaseMaster.Paymode))
                {
                    double availableCash = ObjPurchaseInviceRepo.GetAvailableCashBalance(GetBranchId());
                    double purchaseAmt = ObjPurchaseMaster.GrandTotal;
                    if (availableCash - purchaseAmt < 0)
                    {
                        MessageBox.Show(
                            $"Insufficient CASH-IN-HAND balance!\n\n" +
                            $"Current Cash-In-Hand Balance: {availableCash:N2}\n" +
                            $"Purchase Amount: {purchaseAmt:N2}\n" +
                            $"Shortfall: {(purchaseAmt - availableCash):N2}\n\n" +
                            "Transaction cannot be saved — CASH-IN-HAND balance cannot go negative.",
                            "Insufficient Cash Balance",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Show confirmation dialog before saving
                DialogBox.frmSuccesMsg confirmDialog = new DialogBox.frmSuccesMsg(txtPurchaseNo.Text.Replace("GRN-", ""));
                if (confirmDialog.ShowDialog() != DialogResult.Yes)
                {
                    return; // User chose not to save
                }

                // Call the repository method to save the purchase invoice
                string message = ObjPurchaseInviceRepo.SavePurchaseInvoice(ObjPurchaseMaster, ObjPurchaseDetails, tempGridView);

                if (string.IsNullOrEmpty(message) || message.ToLower().Contains("success"))
                {
                    // Display the generated purchase number before clearing
                    if (ObjPurchaseMaster.PurchaseNo > 0)
                    {
                        txtPurchaseNo.Text = "GRN-" + ObjPurchaseMaster.PurchaseNo.ToString();
                        System.Diagnostics.Debug.WriteLine($"Displayed saved PurchaseNo: {ObjPurchaseMaster.PurchaseNo}");
                    }

                    SavePurchaseActivityLog("SAVE", ObjPurchaseMaster.PurchaseNo, ObjPurchaseMaster);

                    // Show a simple success message now instead of the popup
                    MessageBox.Show("Saved Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear fields after successful save (this will regenerate the next purchase number)
                    Clear();
                }
                else
                {
                    // Show error message
                    MessageBox.Show("Error saving purchase: " + message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving purchase: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add Clear method to reset form after saving
        public void Clear()
        {
            _isLoadingPurchaseForActivitySnapshot = true;
            try
            {
                SetFormReadOnly(false);
                _lastReadOnlyNotifiedPurchaseId = -1;
                _originalSellingPrices.Clear();
                ClearPurchaseHeaderActivitySnapshot();
                // Generate and display next purchase number dynamically
                GenerateAndDisplayPurchaseNumber();

                txtInvoiceNo.Clear();
                txtInvoiceAmt.Clear();
                txtBarcode.Clear();
                txtBilledBy.Clear();
                textBox1.Clear(); // Clear vendor ID textBox

                if (txtRemark != null)
                {
                    txtRemark.Clear();
                }

                if (CmboVendor != null)
                {
                    CmboVendor.Value = null;
                    CmboVendor.SelectedIndex = -1;
                    CmboVendor.Text = string.Empty;
                }

                txtRoundOff.Clear();

                // Reset dates to current date
                dtpPurchaseDate.Value = DateTime.Today;
                DtpInoviceDate.Value = DateTime.Today;

                // Reset grid
                ClearUltraGrid1Contents();
                ClearUltraGrid2Contents();

                // Reset calculation values
                SubTotal = 0;
                NetTotal = 0;
                OriginalNetTotal = 0;
                LblPid.Text = "";
                lblVoucherId.Text = "";
                lblSubTotalAmt.Text = "0.00";

                label4.Text = "0.00";
                label6.Text = "0.00";
                textBox2.Text = "";
                _isNetTotalManuallySet = false;

                // Reset panel visibility: show label4 panel, hide label6 panel
                ultraPanel11.Visible = true;
                ultraPanel12.Visible = false;

                // Reset button visibility
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // Reset payment mode to Credit as default
                SetDefaultPaymentMode();

                // Highlight mandatory fields again
                HighlightMandatoryFields();

                // Set focus to barcode field
                barcodeFocus();
            }
            finally
            {
                _isLoadingPurchaseForActivitySnapshot = false;
            }
        }

        private void SetDefaultPaymentMode()
        {
            try
            {
                if (CmboPayment == null || CmboPayment.Items == null || CmboPayment.Items.Count == 0)
                    return;

                foreach (var item in CmboPayment.Items)
                {
                    if (item.ListObject is PaymodeDDl pm && string.Equals(pm.PayModeName, "Credit", StringComparison.OrdinalIgnoreCase))
                    {
                        CmboPayment.Value = pm.PayModeID;
                        return;
                    }
                    if (string.Equals(item.DisplayText, "Credit", StringComparison.OrdinalIgnoreCase))
                    {
                        CmboPayment.SelectedItem = item;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting default payment mode: " + ex.Message);
            }
        }

        private void WireGridClearButton(string buttonName, Action clearAction)
        {
            if (clearAction == null || string.IsNullOrWhiteSpace(buttonName))
                return;

            Control clearButton = this.Controls.Find(buttonName, true).FirstOrDefault();
            if (clearButton != null)
            {
                clearButton.Click += (s, e) => clearAction();
            }
        }

        private void ClearUltraGrid1Contents()
        {
            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt != null)
            {
                dt.Rows.Clear();
            }

            ultraGrid1.ActiveRow = null;
            ultraGrid1.Selected.Rows.Clear();
            UpdateNetAmtColumnVisibility();
            CaluateTotals();
        }

        private void ClearUltraGrid2Contents()
        {
            DataTable purchaseOrderTable = ultraGrid2.DataSource as DataTable;
            if (purchaseOrderTable != null)
            {
                purchaseOrderTable.Rows.Clear();
            }
            else
            {
                ultraGrid2.DataSource = null;
            }

            ultraGrid2.ActiveRow = null;
            ultraGrid2.Selected.Rows.Clear();
            UpdateUltraGrid2FooterValues();
        }


        private void txtRoundOff_TextChanged(object sender, EventArgs e)
        {
            // We'll process round off only when Enter key is pressed
            // Implementation is in txtRoundOff_KeyDown event
        }

        private void txtRoundOff_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent "ding" sound
                e.Handled = true;

                // Apply rounding based on entered value
                ApplyRoundOff();

                // Keep focus in the txtRoundOff field
                txtRoundOff.Focus();
                txtRoundOff.SelectAll();
            }
        }

        private void ApplyRoundOff()
        {
            try
            {
                string input = txtRoundOff.Text.Trim();

                // Check if the input starts with a plus sign (for adding values)
                if (input.StartsWith("+") && input.Length > 1)
                {
                    // Extract the value after the + sign
                    string valueStr = input.Substring(1);
                    if (float.TryParse(valueStr, out float addValue))
                    {
                        // Add the value to the original total
                        NetTotal = OriginalNetTotal + addValue;

                        // Update the txtRoundOff with the actual round-off amount (negative value)
                        txtRoundOff.Text = (-addValue).ToString("0.00");
                    }
                    else
                    {
                        // Invalid number format after +
                        NetTotal = OriginalNetTotal;
                    }
                }
                // Original round-off logic
                else if (!string.IsNullOrWhiteSpace(input) && float.TryParse(input, out float roundOff))
                {
                    // Apply the round off to the original net total
                    NetTotal = OriginalNetTotal - roundOff;
                }
                else
                {
                    // If round off is cleared or invalid, reset to original total
                    NetTotal = OriginalNetTotal;
                }

                // Update UI with the new total

                label4.Text = NetTotal.ToString("N2");

                // Update invoice amount text field
                txtInvoiceAmt.Text = NetTotal.ToString("N2");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error applying round off: " + ex.Message);
            }
        }

        public void CaluateTotals()
        {
            // Reset totals
            SubTotal = 0;
            NetTotal = 0;

            // Calculate totals from grid
            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // SubTotal should be tax-excluded (BaseCost * Qty)
                    if (dt.Rows[i]["BaseCost"] != DBNull.Value && dt.Rows[i]["Qty"] != DBNull.Value)
                    {
                        float baseCost = float.Parse(dt.Rows[i]["BaseCost"].ToString());
                        float qty = float.Parse(dt.Rows[i]["Qty"].ToString());
                        SubTotal += baseCost * qty;
                    }

                    // Use NetAmt column for totals since it is populated for both Incl and Excl
                    if (dt.Columns.Contains("NetAmt") && dt.Rows[i]["NetAmt"] != null && dt.Rows[i]["NetAmt"] != DBNull.Value)
                    {
                        if (float.TryParse(dt.Rows[i]["NetAmt"].ToString(), out float netAmt))
                        {
                            NetTotal += netAmt;
                        }
                    }
                }
            }

            // finalTotal is exactly NetTotal
            float finalTotal = NetTotal;

            // Store original net total for round off calculations
            OriginalNetTotal = finalTotal;

            // Apply any existing round off
            float roundOff = 0;
            if (!string.IsNullOrWhiteSpace(txtRoundOff.Text) &&
                float.TryParse(txtRoundOff.Text, out roundOff))
            {
                finalTotal = OriginalNetTotal - roundOff;
            }

            // Update UI labels with the calculated totals
            lblSubTotalAmt.Text = SubTotal.ToString("N2");

            // label4 should show NetAmt total if any item has excl tax type, otherwise show Amount total
            label4.Text = finalTotal.ToString("N2");

            // Set invoice amount text field
            txtInvoiceAmt.Text = finalTotal.ToString("N2");

            // Also update footer values when totals are calculated
            UpdateFooterValues();

            // Set focus to barcode field
            this.ActiveControl = txtBarcode;
            txtBarcode.Focus();
        }

        public void CheckBarcode(string barcode, out int existingItemIndex)
        {
            existingItemIndex = -1;
            CheckExists = false;

            // If barcode is null or empty, just return
            if (string.IsNullOrEmpty(barcode))
            {
                return;
            }

            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // Make sure we don't try to compare with null values
                    string rowBarcode = dt.Rows[i]["BarCode"]?.ToString() ?? "";

                    if (rowBarcode == barcode)
                    {
                        CheckExists = true;
                        existingItemIndex = i;
                        float qty = Convert.ToSingle(dt.Rows[i]["Qty"]);
                        float newQty = qty + 1;
                        dt.Rows[i]["Qty"] = newQty;

                        float costWithTax = 0;
                        if (float.TryParse(dt.Rows[i]["Cost"]?.ToString(), out costWithTax))
                        {
                            // Recalculate tax for the updated quantity
                            RecalculateTaxForExistingItem(dt.Rows[i], costWithTax, newQty);
                        }

                        this.CaluateTotals();
                        this.barcodeFocus();
                        return;
                    }
                }
            }
        }

        public void CheckBarcode(string barcode, out int existingItemIndex, float additionalQty = 1)
        {
            existingItemIndex = -1;
            CheckExists = false;

            // If barcode is null or empty, just return
            if (string.IsNullOrEmpty(barcode))
            {
                return;
            }

            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // Make sure we don't try to compare with null values
                    string rowBarcode = dt.Rows[i]["BarCode"]?.ToString() ?? "";

                    if (rowBarcode == barcode)
                    {
                        CheckExists = true;
                        existingItemIndex = i;
                        float qty = Convert.ToSingle(dt.Rows[i]["Qty"]);
                        float newQty = qty + additionalQty;
                        dt.Rows[i]["Qty"] = newQty;

                        float costWithTax = 0;
                        if (float.TryParse(dt.Rows[i]["Cost"]?.ToString(), out costWithTax))
                        {
                            // Recalculate tax for the updated quantity
                            RecalculateTaxForExistingItem(dt.Rows[i], costWithTax, newQty);
                        }

                        this.CaluateTotals();
                        this.barcodeFocus();
                        return;
                    }
                }
            }
        }

        public void barcodeFocus()
        {
            this.ActiveControl = txtBarcode;
            txtBarcode.Clear();
            txtBarcode.Text = "";
            txtBarcode.Focus();
        }

        private void btnFInd_Click(object sender, EventArgs e)
        {
            ShowItemSelectionDialog();
        }

        private void ShowItemSelectionDialog()
        {
            try
            {
                // Create the item selection dialog with the correct form name parameter
                frmdialForItemMaster itemDialog = new frmdialForItemMaster("FromPurchase");

                // Set the Owner property to establish the parent-child relationship
                itemDialog.Owner = this;

                // Pass the selected TaxType from comboBox1 to the dialog
                string selectedTaxType = "ALL"; // Default to ALL
                if (!string.IsNullOrWhiteSpace(comboBox1.Text))
                {
                    selectedTaxType = comboBox1.Text;
                }
                itemDialog.TaxTypeFilter = selectedTaxType;

                // Show the dialog and wait for result
                if (itemDialog.ShowDialog() == DialogResult.OK)
                {
                    // The selected item is already added to the grid in the SendItemToSalesReturn method
                    // We just need to make sure totals are calculated
                    this.CaluateTotals();
                    this.barcodeFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing item selection dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to ensure no sort indicators are shown
        private void RemoveSortIndicators()
        {
            try
            {
                if (ultraGrid1 != null && ultraGrid1.DisplayLayout != null &&
                    ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    // Disable sorting functionality
                    ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.Select;
                    ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.NotAllowed;
                    ultraGrid1.DisplayLayout.Override.AllowColSwapping = AllowColSwapping.NotAllowed;

                    // Reset all column headers to ensure no sort indicators
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        // Force header redraw without indicators
                        col.Header.Fixed = true;
                        col.Header.FixedHeaderIndicator = FixedHeaderIndicator.None;
                        col.SortIndicator = SortIndicator.None;

                        // Set appearance to match our styling
                        col.Header.Appearance.BackColor = Color.FromArgb(0, 122, 204);
                        col.Header.Appearance.BackColor2 = Color.FromArgb(0, 102, 184);
                        col.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                        col.Header.Appearance.ForeColor = Color.White;
                        col.Header.Appearance.TextHAlign = HAlign.Center;
                        col.Header.Appearance.TextVAlign = VAlign.Middle;
                        col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                    }

                    // This forces a refresh of the grid
                    ultraGrid1.DisplayLayout.Bands[0].Override.HeaderClickAction = HeaderClickAction.Select;
                    ultraGrid1.Refresh();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error removing sort indicators: " + ex.Message);
            }
        }

        private void ShowVendorDialog()
        {
            DialogBox.frmVendorDig vendorDialog = new DialogBox.frmVendorDig();
            if (vendorDialog.ShowDialog() == DialogResult.OK)
            {
                // Refresh vendor dropdown in case a new vendor was created
                RefreshVendorDropdown();

                int vendorId = vendorDialog.SelectedVendorId;
                string vendorName = vendorDialog.SelectedVendorName;

                // Set selected vendor to combo box
                CmboVendor.Value = vendorId;

                // Update textBox1 with vendor ID
                textBox1.Text = vendorId.ToString();
            }
        }

        private void ShowPurchaseDisplayDialog()
        {
            DialogBox.FrmPurchaseDisplayDialog purchaseDisplayDialog = new DialogBox.FrmPurchaseDisplayDialog();

            // Make this FormPurchase accessible to FrmPurchaseDisplayDialog
            purchaseDisplayDialog.Tag = this;

            if (purchaseDisplayDialog.ShowDialog() == DialogResult.OK)
            {
                // If OK is returned, dialog already loaded data
                // Calculate totals
                this.CaluateTotals();
            }
        }

        private void ShowPurchaseOrderDialog()
        {
            try
            {
                int vendorId = GetSelectedVendorIdForPurchaseOrder();
                if (vendorId <= 0)
                {
                    MessageBox.Show("Please select a vendor first.", "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (frmPurchaseOrderDig dialog = new frmPurchaseOrderDig())
                {
                    dialog.FilterLedgerId = vendorId;
                    if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedPurchaseOrderId > 0)
                    {
                        LoadPurchaseOrderIntoUltraGrid2(dialog.SelectedPurchaseOrderId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening purchase order lookup: " + ex.Message, "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPurchaseOrderIntoUltraGrid2(int purchaseOrderId)
        {
            try
            {
                PurchaseOrderLoadResult result = _purchaseOrderRepository.GetPurchaseOrderById(purchaseOrderId);
                if (result == null || result.Master == null)
                {
                    MessageBox.Show("Unable to load the selected purchase order.", "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ultraGrid2.DataSource = BuildPurchaseOrderGridTable(result.Details);
                ConfigureUltraGrid2Theme();
                UpdateUltraGrid2FooterValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase order: " + ex.Message, "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static DataTable BuildPurchaseOrderGridTable(IEnumerable<PurchaseOrderDetail> details)
        {
            DataTable table = new DataTable();
            table.Columns.Add("SLNO", typeof(int));
            table.Columns.Add("BarCode", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Unit", typeof(string));
            table.Columns.Add("Packing", typeof(float));
            table.Columns.Add("Cost", typeof(float));
            table.Columns.Add("Qty", typeof(float));
            table.Columns.Add("TaxPer", typeof(float));
            table.Columns.Add("TaxAmt", typeof(float));
            table.Columns.Add("NetAmt", typeof(float));
            table.Columns.Add("ItemId", typeof(int));
            table.Columns.Add("UnitId", typeof(int));
            table.Columns.Add("TaxType", typeof(string));

            if (details == null)
                return table;

            int rowNo = 1;
            foreach (PurchaseOrderDetail detail in details)
            {
                if (detail == null)
                    continue;

                DataRow row = table.NewRow();
                row["SLNO"] = rowNo++;
                row["BarCode"] = detail.Barcode ?? string.Empty;
                row["Description"] = detail.ItemName ?? string.Empty;
                row["Unit"] = detail.Unit ?? string.Empty;
                row["Packing"] = Convert.ToSingle(detail.Packing);
                row["Cost"] = Convert.ToSingle(detail.Cost);
                row["Qty"] = Convert.ToSingle(detail.Qty);
                row["TaxPer"] = Convert.ToSingle(detail.TaxPer);
                row["TaxAmt"] = Convert.ToSingle(detail.TaxAmt);
                row["NetAmt"] = Convert.ToSingle(detail.TotalSP + detail.TaxAmt);
                row["ItemId"] = detail.ItemID;
                row["UnitId"] = detail.UnitId;
                row["TaxType"] = detail.TaxType ?? string.Empty;
                table.Rows.Add(row);
            }

            return table;
        }

        private int GetSelectedVendorIdForPurchaseOrder()
        {
            int vendorId;
            if (int.TryParse(textBox1.Text, out vendorId) && vendorId > 0)
                return vendorId;

            try
            {
                if (CmboVendor.Value != null)
                {
                    vendorId = Convert.ToInt32(CmboVendor.Value);
                    if (vendorId > 0)
                        return vendorId;
                }
            }
            catch
            {
            }

            return 0;
        }

        private void LoadSelectedPurchaseOrderItemToPurchaseGrid()
        {
            if (ultraGrid2.ActiveRow == null || !ultraGrid2.ActiveRow.IsDataRow)
            {
                MessageBox.Show("Please select a purchase order item first.", "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ImportPurchaseOrderRowToPurchaseGrid(ultraGrid2.ActiveRow, true);
        }

        private void LoadAllPurchaseOrderItemsToPurchaseGrid()
        {
            if (ultraGrid2.Rows == null || ultraGrid2.Rows.Count == 0)
            {
                MessageBox.Show("There are no purchase order items to load.", "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (UltraGridRow row in ultraGrid2.Rows)
            {
                if (row != null && row.IsDataRow)
                    ImportPurchaseOrderRowToPurchaseGrid(row, false);
            }

            FinalizePurchaseOrderImport();
            barcodeFocus();
        }

        private void ImportPurchaseOrderRowToPurchaseGrid(UltraGridRow sourceRow, bool flashBarcode)
        {
            if (sourceRow == null)
                return;

            DataTable table = ultraGrid1.DataSource as DataTable;
            if (table == null)
                return;

            string barcode = Convert.ToString(sourceRow.Cells["BarCode"].Value) ?? string.Empty;
            float qty = GetRowFloat(sourceRow, "Qty");
            if (qty <= 0)
                qty = 1f;

            DataRow existingRow = table.AsEnumerable().FirstOrDefault(row =>
                string.Equals(Convert.ToString(row["BarCode"]), barcode, StringComparison.OrdinalIgnoreCase));

            if (existingRow != null)
            {
                float existingQty = Convert.ToSingle(existingRow["Qty"]);
                float updatedQty = existingQty + qty;
                existingRow["Qty"] = updatedQty;

                float costWithTax = GetDataRowFloat(existingRow, "Cost");
                if (costWithTax <= 0)
                    costWithTax = GetRowFloat(sourceRow, "Cost");

                RecalculateTaxForExistingItem(existingRow, costWithTax, updatedQty);
            }
            else
            {
                DataRow newRow = table.NewRow();
                string taxType = NormalizeTaxType(GetRowString(sourceRow, "TaxType"));
                float costWithTax = GetRowFloat(sourceRow, "Cost");
                float taxPer = GetRowFloat(sourceRow, "TaxPer");
                float taxAmt = GetRowFloat(sourceRow, "TaxAmt");
                float baseCost = costWithTax;

                if (DataBase.IsTaxEnabled && taxPer > 0)
                {
                    if (taxType == "incl")
                    {
                        baseCost = qty > 0 ? costWithTax - (taxAmt / qty) : costWithTax;
                    }
                    else if (taxType == "excl")
                    {
                        baseCost = costWithTax;
                    }
                }

                newRow["SLNO"] = table.Rows.Count + 1;
                newRow["BarCode"] = barcode;
                newRow["Description"] = GetRowString(sourceRow, "Description");
                newRow["Unit"] = GetRowString(sourceRow, "Unit");
                newRow["Packing"] = GetRowFloat(sourceRow, "Packing");
                newRow["RetailPrice"] = 0f;
                newRow["Free"] = 0f;
                newRow["SellingPrice"] = 0f;
                newRow["UnitSP"] = 0f;
                newRow["BaseCost"] = baseCost;
                newRow["Cost"] = costWithTax;
                newRow["Qty"] = qty;
                newRow["Gross"] = 0f;
                newRow["NetAmt"] = 0f;
                newRow["NewBaseCost"] = 0f;
                newRow["ItemId"] = GetRowInt(sourceRow, "ItemId");
                newRow["UnitId"] = GetRowInt(sourceRow, "UnitId");
                newRow["UnitPrize"] = 0f;
                newRow["MarginPer"] = 0f;
                newRow["MarginAmt"] = 0f;
                newRow["TaxPer"] = DataBase.IsTaxEnabled ? taxPer : 0f;
                newRow["TaxAmt"] = DataBase.IsTaxEnabled ? taxAmt : 0f;
                newRow["TaxType"] = taxType;
                newRow["WholeSalePrice"] = 0f;
                newRow["CreditPrice"] = 0f;
                newRow["CardPrice"] = 0f;
                ApplyItemStatusToPurchaseRow(newRow, null);
                table.Rows.Add(newRow);

                RecalculateTaxForExistingItem(newRow, costWithTax, qty);
            }

            RenumberPurchaseGridRows(table);

            if (flashBarcode)
            {
                FinalizePurchaseOrderImport();
                FlashBarcode(barcode);
            }
        }

        private void FinalizePurchaseOrderImport()
        {
            UpdateNetAmtColumnVisibility();
            UpdateGrossColumnVisibility();
            RecalculateNewBaseCostForAllRows();
            CaluateTotals();
            ultraGrid1.Rows.Refresh(RefreshRow.FireInitializeRow);
        }

        private void RenumberPurchaseGridRows(DataTable table)
        {
            int rowNo = 1;
            foreach (DataRow row in table.Rows)
            {
                row["SLNO"] = rowNo++;
            }
        }

        private void FlashBarcode(string barcode)
        {
            if (_barcodeFlashTimer == null)
            {
                _barcodeFlashTimer = new Timer();
                _barcodeFlashTimer.Interval = 1000;
                _barcodeFlashTimer.Tick += (s, e) =>
                {
                    _barcodeFlashTimer.Stop();
                    barcodeFocus();
                };
            }

            _barcodeFlashTimer.Stop();
            txtBarcode.Text = barcode ?? string.Empty;
            txtBarcode.Focus();
            txtBarcode.SelectionStart = txtBarcode.Text.Length;
            _barcodeFlashTimer.Start();
        }

        private static float GetRowFloat(UltraGridRow row, string columnKey)
        {
            if (row == null || !row.Cells.Exists(columnKey) || row.Cells[columnKey].Value == null || row.Cells[columnKey].Value == DBNull.Value)
                return 0f;

            float value;
            return float.TryParse(Convert.ToString(row.Cells[columnKey].Value), out value) ? value : 0f;
        }

        private static int GetRowInt(UltraGridRow row, string columnKey)
        {
            if (row == null || !row.Cells.Exists(columnKey) || row.Cells[columnKey].Value == null || row.Cells[columnKey].Value == DBNull.Value)
                return 0;

            int value;
            return int.TryParse(Convert.ToString(row.Cells[columnKey].Value), out value) ? value : 0;
        }

        private static string GetRowString(UltraGridRow row, string columnKey)
        {
            if (row == null || !row.Cells.Exists(columnKey) || row.Cells[columnKey].Value == null || row.Cells[columnKey].Value == DBNull.Value)
                return string.Empty;

            return Convert.ToString(row.Cells[columnKey].Value) ?? string.Empty;
        }

        private static float GetDataRowFloat(DataRow row, string columnKey)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnKey) || row[columnKey] == null || row[columnKey] == DBNull.Value)
                return 0f;

            float value;
            return float.TryParse(Convert.ToString(row[columnKey]), out value) ? value : 0f;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        // Helper method to normalize TaxType values from database
        private string NormalizeTaxType(string taxType)
        {
            if (string.IsNullOrEmpty(taxType))
                return string.Empty;

            // Normalize to lowercase for comparison
            string normalized = taxType.Trim().ToLower();

            // Convert short forms to full forms
            if (normalized == "in" || normalized == "i")
                return "incl";
            else if (normalized == "ex" || normalized == "e")
                return "excl";
            else if (normalized == "incl" || normalized == "inclusive")
                return "incl";
            else if (normalized == "excl" || normalized == "exclusive")
                return "excl";

            // Return original if no match (empty or unknown value)
            return string.IsNullOrEmpty(normalized) ? string.Empty : normalized;
        }

        // Method to handle loading purchase data from the display dialog
        public void LoadPurchaseData(int purchaseId)
        {
            bool isPaidOrHalfPaid = false;
            try
            {
                _isLoadingPurchaseForActivitySnapshot = true;
                _originalSellingPrices.Clear();
                // First clear the grid
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null)
                {
                    dt.Rows.Clear();
                }

                // Get purchase details from repository
                PurchaseInvoiceGrid purchaseInvoiceGrid = ObjPurchaseInviceRepo.getPurchaseNumber(purchaseId);

                if (purchaseInvoiceGrid != null && purchaseInvoiceGrid.Listpmaster != null && purchaseInvoiceGrid.Listpmaster.Count() > 0)
                {
                    PurchaseMaster loadedMaster = purchaseInvoiceGrid.Listpmaster.First();
                    isPaidOrHalfPaid = loadedMaster.PayedAmount > 0;

                    if (isPaidOrHalfPaid && _lastReadOnlyNotifiedPurchaseId != purchaseId)
                    {
                        _lastReadOnlyNotifiedPurchaseId = purchaseId;
                        MessageBox.Show(
                            string.Format(MsgReadOnlyModeDetails, loadedMaster.PurchaseNo, loadedMaster.PayedAmount),
                            MsgReadOnlyModeTitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    // Toggle button visibility - hide save button, show update button
                    pbxSave.Visible = false;
                    ultraPictureBox4.Visible = !isPaidOrHalfPaid;

                    // Load master data to form controls
                    foreach (PurchaseMaster pm in purchaseInvoiceGrid.Listpmaster)
                    {
                        txtPurchaseNo.Text = "GRN-" + pm.PurchaseNo.ToString();
                        dtpPurchaseDate.Value = Convert.ToDateTime(pm.PurchaseDate.ToShortDateString());

                        CmboVendor.Value = pm.LedgerID;

                        // Update textBox1 with vendor ID
                        if (CmboVendor.Value != null)
                        {
                            textBox1.Text = CmboVendor.Value.ToString();
                        }

                        // Set payment mode combobox by PaymodeID value
                        CmboPayment.Value = pm.PaymodeID;

                        txtInvoiceNo.Text = pm.InvoiceNo.ToString();
                        DtpInoviceDate.Value = Convert.ToDateTime(pm.InvoiceDate.ToShortDateString());
                        txtInvoiceAmt.Text = pm.GrandTotal.ToString();

                        // Store Pid and VoucherId in labels
                        LblPid.Text = pm.Pid.ToString();
                        lblVoucherId.Text = pm.VoucherID.ToString();

                        if (!string.IsNullOrEmpty(pm.BilledBy))
                            txtBilledBy.Text = pm.BilledBy;

                        // Load round off value
                        txtRoundOff.Text = pm.RoundOff.ToString("0.00");
                    }

                    // Load purchase details into grid
                    if (purchaseInvoiceGrid.Listpdetails != null && purchaseInvoiceGrid.Listpdetails.Count() > 0)
                    {
                        // Get GrandTotal from PMaster for NetAmt calculation
                        // This is the saved value that user may have edited
                        float grandTotalFromMaster = (float)purchaseInvoiceGrid.Listpmaster.First().GrandTotal;
                        int totalItems = purchaseInvoiceGrid.Listpdetails.Count();

                        // Use a counter to ensure sequential SLNO values
                        int slnoCounter = 1;

                        foreach (PurchaseDetails pd in purchaseInvoiceGrid.Listpdetails)
                        {
                            // Store the original selling price for activity logging comparison
                            float initialPriceVal = GetWholeSalePriceFromPriceSettings(pd.ItemID, pd.UnitId);
                            float initialSellingPrice = initialPriceVal > 0
                                ? initialPriceVal
                                : (float)pd.WholeSalePrice;
                            string key = $"{pd.ItemID}_{pd.UnitId}";
                            _originalSellingPrices[key] = initialSellingPrice;

                            DataRow newRow = dt.NewRow();

                            // Assign sequential SLNO value from counter instead of using database value
                            newRow["SLNO"] = slnoCounter++;

                            newRow["ItemId"] = pd.ItemID;
                            newRow["BarCode"] = string.IsNullOrEmpty(pd.Barcode) ? "" : pd.Barcode;
                            newRow["Description"] = string.IsNullOrEmpty(pd.ItemName) ? "" : pd.ItemName;
                            newRow["Unit"] = string.IsNullOrEmpty(pd.Unit) ? "" : pd.Unit;
                            newRow["Packing"] = Convert.ToSingle(pd.Packing);
                            newRow["RetailPrice"] = Convert.ToSingle(pd.RetailPrice);
                            newRow["Free"] = Convert.ToSingle(pd.Free);

                            // Load saved values directly from database - don't recalculate
                            // This preserves exactly what the user saved
                            double costWithTax = Convert.ToDouble(pd.Cost.ToString());
                            double baseCostWithoutTax = costWithTax; // Default to Cost if BaseCost not available
                            double taxPer = Convert.ToDouble(pd.TaxPer.ToString());
                            double taxAmt = Convert.ToDouble(pd.TaxAmt.ToString());
                            string taxType = NormalizeTaxType(pd.TaxType ?? string.Empty);

                            // Try to get BaseCost from database if available
                            // Check if pd has a BaseCost property and use it directly
                            try
                            {
                                // Use reflection to check if BaseCost exists in the model
                                var baseCostProperty = pd.GetType().GetProperty("BaseCost");
                                if (baseCostProperty != null)
                                {
                                    var baseCostValue = baseCostProperty.GetValue(pd);
                                    if (baseCostValue != null)
                                    {
                                        baseCostWithoutTax = Convert.ToDouble(baseCostValue.ToString());
                                    }
                                }
                                else
                                {
                                    // If BaseCost property doesn't exist, calculate it from Cost and TaxAmt
                                    // This ensures backward compatibility
                                    if (DataBase.IsTaxEnabled && taxPer > 0 && !string.IsNullOrEmpty(taxType))
                                    {
                                        if (taxType == "incl")
                                        {
                                            // Tax is included - subtract tax amount to get base cost
                                            baseCostWithoutTax = costWithTax - (taxAmt / Convert.ToDouble(pd.Qty.ToString()));
                                        }
                                        else if (taxType == "excl")
                                        {
                                            // Tax is exclusive - base cost is same as cost
                                            baseCostWithoutTax = costWithTax;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // If reflection fails or any error, calculate from Cost and TaxType
                                if (DataBase.IsTaxEnabled && taxPer > 0 && !string.IsNullOrEmpty(taxType))
                                {
                                    if (taxType == "incl")
                                    {
                                        // Tax is included - subtract per-unit tax to get base cost
                                        double perUnitTax = taxAmt / Convert.ToDouble(pd.Qty.ToString());
                                        baseCostWithoutTax = costWithTax - perUnitTax;
                                    }
                                    else if (taxType == "excl")
                                    {
                                        // Tax is exclusive - base cost equals cost
                                        baseCostWithoutTax = costWithTax;
                                    }
                                }
                            }

                            // Use the saved values directly - no recalculation
                            newRow["BaseCost"] = (float)baseCostWithoutTax;
                            newRow["Cost"] = (float)costWithTax;
                            newRow["Qty"] = Convert.ToSingle(pd.Qty);

                            // Calculate amount from saved Cost and Qty (Amount/TotalAmount columns removed)
                            double qty = Convert.ToDouble(pd.Qty.ToString());
                            float amount = (float)(costWithTax * qty);

                            // NetAmt: ALWAYS use GrandTotal from PMaster (this is the saved/edited value)
                            // Do NOT use calculated value or value from PDetails
                            // For single item: NetAmt = GrandTotal from PMaster
                            // For multiple items: distribute proportionally
                            if (totalItems == 1)
                            {
                                // Single item: NetAmt = GrandTotal from PMaster (user's saved value)
                                newRow["NetAmt"] = grandTotalFromMaster;
                            }
                            else
                            {
                                // Multiple items: calculate proportional share based on amount
                                float totalAmount = 0;
                                foreach (PurchaseDetails pdItem in purchaseInvoiceGrid.Listpdetails)
                                {
                                    totalAmount += Convert.ToSingle(pdItem.Cost) * Convert.ToSingle(pdItem.Qty);
                                }
                                if (totalAmount > 0)
                                {
                                    float proportion = amount / totalAmount;
                                    newRow["NetAmt"] = proportion * grandTotalFromMaster;
                                }
                                else
                                {
                                    newRow["NetAmt"] = grandTotalFromMaster / totalItems;
                                }
                            }

                            // Gross: Try to load from database, or calculate if not available
                            bool grossLoaded = false;
                            try
                            {
                                var grossProperty = pd.GetType().GetProperty("Gross");
                                if (grossProperty != null)
                                {
                                    var grossValue = grossProperty.GetValue(pd);
                                    if (grossValue != null)
                                    {
                                        newRow["Gross"] = Convert.ToSingle(grossValue);
                                        grossLoaded = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error loading Gross from database: {ex.Message}");
                            }

                            // Calculate Gross if not loaded from database
                            if (!grossLoaded)
                            {
                                if (taxType == "incl")
                                {
                                    newRow["Gross"] = amount - (float)taxAmt;
                                }
                                else
                                {
                                    newRow["Gross"] = 0f;
                                }
                            }

                            // Hidden columns
                            newRow["UnitId"] = pd.UnitId;
                            newRow["UnitPrize"] = Convert.ToSingle(pd.RetailPrice);

                            // Set default values for fields that might not exist
                            newRow["MarginPer"] = 0f;
                            newRow["MarginAmt"] = 0f;

                            // Use saved tax values directly - preserve what user entered
                            newRow["TaxPer"] = (float)taxPer;
                            newRow["TaxAmt"] = (float)taxAmt;
                            newRow["TaxType"] = taxType; // TaxType is already normalized above

                            // Safe conversions for price fields
                            double wholesalePrice = Convert.ToDouble(pd.WholeSalePrice.ToString());
                            double creditPrice = Convert.ToDouble(pd.CreditPrice.ToString());
                            newRow["WholeSalePrice"] = (float)wholesalePrice;
                            newRow["CreditPrice"] = (float)creditPrice;

                            // CardPrice not in model, use retail price
                            newRow["CardPrice"] = Convert.ToSingle(pd.RetailPrice);

                            // Get WholeSalePrice from PriceSettings table (remapped from PDetails.WholeSalePrice)
                            float wholeSalePriceFromPriceSettings = GetWholeSalePriceFromPriceSettings(pd.ItemID, pd.UnitId);
                            newRow["SellingPrice"] = wholeSalePriceFromPriceSettings > 0
                                ? wholeSalePriceFromPriceSettings
                                : (float)wholesalePrice; // Fallback to PDetails.WholeSalePrice if PriceSettings not found

                            // Get Unit 1's WholeSalePrice from PriceSettings for UnitSP column
                            // Use same fallback logic as SellingPrice (PDetails.WholeSalePrice) if Unit 1's PriceSettings not found
                            float unit1WholeSalePrice = GetWholeSalePriceFromPriceSettings(pd.ItemID, 1);
                            newRow["UnitSP"] = unit1WholeSalePrice > 0 ? unit1WholeSalePrice : (float)wholesalePrice;

                            // Load NewBaseCost from PriceSettings directly - shows current Txt_UnitCost
                            float currentMstCost = 0;
                            float currentMstStock = 0;
                            GetExistingCostAndStock(pd.ItemID, pd.UnitId, out currentMstCost, out currentMstStock);
                            newRow["NewBaseCost"] = currentMstCost;

                            // Add the row to the datatable
                            dt.Rows.Add(newRow);
                        }

                        ApplyItemStatusesToPurchaseGrid();

                        // RecalculateNewBaseCostForAllRows() removed to prevent double-counting of loaded items
                        // Saved items should just show current Master Cost (Txt_UnitCost) as loaded above

                        // Calculate totals and update displays
                        CaluateTotals();

                        // Restore manually-set total if BillDiscountAmt was saved (user used ".." shortcut)
                        var pmLoaded = purchaseInvoiceGrid.Listpmaster.First();
                        if (pmLoaded.BillDiscountAmt > 0)
                        {
                            _isNetTotalManuallySet = true;
                            // Hide label4 panel, show label6 panel with saved NetTotal
                            ultraPanel11.Visible = false;
                            ultraPanel12.Visible = true;
                            label6.Text = pmLoaded.NetTotal.ToString("N2");
                            txtInvoiceAmt.Text = pmLoaded.NetTotal.ToString("N2");
                            textBox2.Text = pmLoaded.BillDiscountAmt.ToString("N2");
                        }

                        // Update NetAmt and Gross column visibility after loading, and Amount caption
                        UpdateNetAmtColumnVisibility();
                        UpdateGrossColumnVisibility();

                    }

                    CapturePurchaseHeaderActivitySnapshot();

                    // Apply read-only mode if the purchase has existing payments
                    SetFormReadOnly(isPaidOrHalfPaid);
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during data loading
                System.Diagnostics.Debug.WriteLine("Error loading purchase data: " + ex.Message);
                MessageBox.Show("Error loading purchase data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoadingPurchaseForActivitySnapshot = false;
            }
        }

        public void LoadPurchaseDataReadOnly(int purchaseId)
        {
            LoadPurchaseData(purchaseId);
            SetFormReadOnly(true);
            pbxSave.Visible = false;
            ultraPictureBox4.Visible = false;
        }

        // Add UpdatePurchase method to handle update operation
        public void UpdatePurchase()
        {
            if (_isReadOnly)
            {
                MessageBox.Show(MsgReadOnlyBlock, MsgReadOnlyModeTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isUpdatingPurchase)
            {
                return;
            }

            _isUpdatingPurchase = true;
            try
            {
                if (!ShiftSessionGuard.CanDoTransaction(out string transactionError))
                {
                    MessageBox.Show(transactionError, "Shift Closing Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to purchase", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtInvoiceNo.Text))
                {
                    MessageBox.Show("Please enter invoice number", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtInvoiceNo.Focus();
                    return;
                }

                if (CmboVendor.Value == null)
                {
                    MessageBox.Show("Please select a vendor", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CmboVendor.Focus();
                    return;
                }

                // Store vendor ID in textBox1
                if (CmboVendor.Value != null)
                {
                    textBox1.Text = CmboVendor.Value.ToString();
                }

                // Make sure we have the Pid value for update
                if (string.IsNullOrEmpty(LblPid.Text))
                {
                    MessageBox.Show("Purchase ID not found. Cannot update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Setup purchase master data for update
                ObjPurchaseMaster.BranchId = GetBranchId();
                ObjPurchaseMaster.CompanyId = GetCompanyId();
                // Important: Ensure we use the correct FinYearId from the original record
                ObjPurchaseMaster.FinYearId = GetOriginalFinYearId(Convert.ToInt32(LblPid.Text));
                // If GetOriginalFinYearId returns 0 or invalid, try to get it dynamically
                if (ObjPurchaseMaster.FinYearId <= 0)
                {
                    ObjPurchaseMaster.FinYearId = GetFinYearId();
                }
                ObjPurchaseMaster.BranchName = CmboBranch.Text;
                ObjPurchaseMaster.PurchaseDate = Convert.ToDateTime(dtpPurchaseDate.Value);
                ObjPurchaseMaster.InvoiceNo = txtInvoiceNo.Text;
                ObjPurchaseMaster.InvoiceDate = Convert.ToDateTime(DtpInoviceDate.Value);
                ObjPurchaseMaster.LedgerID = Convert.ToInt32(CmboVendor.Value);
                ObjPurchaseMaster.VendorName = CmboVendor.Text;
                ObjPurchaseMaster.PaymodeID = Convert.ToInt32(CmboPayment.Value);
                ObjPurchaseMaster.Paymode = CmboPayment.Text;
                ObjPurchaseMaster.PaymodeLedgerID = 0;
                ObjPurchaseMaster.CreditPeriod = 0;
                ObjPurchaseMaster.SubTotal = SubTotal;
                ObjPurchaseMaster.SpDisPer = 0;
                ObjPurchaseMaster.SpDsiAmt = 0;
                ObjPurchaseMaster.BillDiscountPer = 0;
                ObjPurchaseMaster.BillDiscountAmt = _isNetTotalManuallySet
                    ? (double.TryParse(textBox2.Text, out double updBd) ? updBd : 0)
                    : 0;
                ObjPurchaseMaster.TaxPer = 0;
                ObjPurchaseMaster.TaxAmt = 0;
                ObjPurchaseMaster.Frieght = 0;
                ObjPurchaseMaster.ExpenseAmt = 0;
                ObjPurchaseMaster.OtherExpAmt = 0;
                ObjPurchaseMaster.GrandTotal = NetTotal;
                ObjPurchaseMaster.CancelFlag = false;
                ObjPurchaseMaster.UserID = GetUserId();
                ObjPurchaseMaster.UserName = GetUserName();
                ObjPurchaseMaster.TaxType = "I";
                ObjPurchaseMaster.Remarks = "";
                ObjPurchaseMaster.RoundOff = string.IsNullOrWhiteSpace(txtRoundOff.Text) ?
                    0 : (float.TryParse(txtRoundOff.Text, out float roundOff) ? roundOff : 0);
                ObjPurchaseMaster.CessPer = 0;
                ObjPurchaseMaster.CessAmt = 0;
                ObjPurchaseMaster.CalAfterTax = 0;
                // Set currency information dynamically
                SetCurrencyInfo(ObjPurchaseMaster);
                ObjPurchaseMaster.SeriesID = 0;
                ObjPurchaseMaster.PurchaseNo = Convert.ToInt32(txtPurchaseNo.Text.Replace("GRN-", ""));
                ObjPurchaseMaster.Pid = Convert.ToInt32(LblPid.Text);
                ObjPurchaseMaster.VoucherID = Convert.ToInt32(lblVoucherId.Text);
                ObjPurchaseMaster.IsSyncd = false;
                ObjPurchaseMaster.Paid = false;
                ObjPurchaseMaster.POrderMasterId = 0;

                ObjPurchaseMaster.BilledBy = txtBilledBy.Text;
                ObjPurchaseMaster.TrnsType = "Purchase"; // Set transaction type for proper update

                // NetTotal: if user overrode via ".." shortcut, save label6 value; otherwise same as GrandTotal
                ObjPurchaseMaster.NetTotal = _isNetTotalManuallySet
                    ? (double.TryParse(label6.Text, out double updNt) ? updNt : NetTotal)
                    : NetTotal;

                // Get data from ultraGrid1
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No items to update", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create temporary DataGridView to pass to the repository
                DataGridView tempGridView = new DataGridView();

                // Add the required columns (including BaseCost to preserve tax-excluded cost)
                tempGridView.Columns.Add("SLNO", "SLNO");
                tempGridView.Columns.Add("ItemId", "ItemId");
                tempGridView.Columns.Add("BarCode", "BarCode");
                tempGridView.Columns.Add("Description", "Description");
                tempGridView.Columns.Add("BaseCost", "BaseCost"); // Add BaseCost to save tax-excluded cost
                tempGridView.Columns.Add("Cost", "Cost");
                tempGridView.Columns.Add("UnitId", "UnitId");
                tempGridView.Columns.Add("Unit", "Unit");
                tempGridView.Columns.Add("Qty", "Qty");
                tempGridView.Columns.Add("Free", "Free");
                tempGridView.Columns.Add("UnitPrize", "UnitPrize");
                tempGridView.Columns.Add("Packing", "Packing");
                tempGridView.Columns.Add("MarginPer", "MarginPer");
                tempGridView.Columns.Add("MarginAmt", "MarginAmt");
                tempGridView.Columns.Add("TaxPer", "TaxPer");
                tempGridView.Columns.Add("TaxAmt", "TaxAmt");
                tempGridView.Columns.Add("TaxType", "TaxType");
                tempGridView.Columns.Add("RetailPrice", "RetailPrice");
                tempGridView.Columns.Add("WholeSalePrice", "WholeSalePrice");
                tempGridView.Columns.Add("CreditPrice", "CreditPrice");
                tempGridView.Columns.Add("CardPrice", "CardPrice");
                tempGridView.Columns.Add("NetAmt", "NetAmt");
                tempGridView.Columns.Add("Gross", "Gross");
                tempGridView.Columns.Add("SellingPrice", "SellingPrice");
                tempGridView.Columns.Add("UnitSP", "UnitSP");

                // Transfer data from UltraGrid (via DataTable) to DataGridView
                int slnoCounter = 1;
                foreach (DataRow row in dt.Rows)
                {
                    int index = tempGridView.Rows.Add();
                    tempGridView.Rows[index].Cells["SLNO"].Value = slnoCounter++;
                    tempGridView.Rows[index].Cells["ItemId"].Value = row["ItemId"];
                    tempGridView.Rows[index].Cells["BarCode"].Value = row["BarCode"];
                    tempGridView.Rows[index].Cells["Description"].Value = row["Description"];
                    tempGridView.Rows[index].Cells["BaseCost"].Value = row["BaseCost"]; // Save BaseCost (tax-excluded)
                    tempGridView.Rows[index].Cells["Cost"].Value = row["Cost"];
                    tempGridView.Rows[index].Cells["UnitId"].Value = row["UnitId"];
                    tempGridView.Rows[index].Cells["Unit"].Value = row["Unit"];
                    tempGridView.Rows[index].Cells["Qty"].Value = row["Qty"];
                    tempGridView.Rows[index].Cells["Free"].Value = row["Free"];
                    tempGridView.Rows[index].Cells["UnitPrize"].Value = row["UnitPrize"];
                    tempGridView.Rows[index].Cells["Packing"].Value = row["Packing"];
                    tempGridView.Rows[index].Cells["MarginPer"].Value = row["MarginPer"];
                    tempGridView.Rows[index].Cells["MarginAmt"].Value = row["MarginAmt"];
                    tempGridView.Rows[index].Cells["TaxPer"].Value = row["TaxPer"];
                    tempGridView.Rows[index].Cells["TaxAmt"].Value = row["TaxAmt"];
                    tempGridView.Rows[index].Cells["TaxType"].Value = row["TaxType"]; // Ensure TaxType is saved
                    tempGridView.Rows[index].Cells["RetailPrice"].Value = row["RetailPrice"];
                    // Use SellingPrice from grid if available, otherwise fall back to WholeSalePrice
                    if (row.Table.Columns.Contains("SellingPrice") && row["SellingPrice"] != null && row["SellingPrice"] != DBNull.Value)
                    {
                        tempGridView.Rows[index].Cells["WholeSalePrice"].Value = row["SellingPrice"];
                    }
                    else
                    {
                        tempGridView.Rows[index].Cells["WholeSalePrice"].Value = row["WholeSalePrice"];
                    }
                    tempGridView.Rows[index].Cells["CreditPrice"].Value = row["CreditPrice"];
                    tempGridView.Rows[index].Cells["CardPrice"].Value = row["CardPrice"];
                    tempGridView.Rows[index].Cells["NetAmt"].Value = row.Table.Columns.Contains("NetAmt") && row["NetAmt"] != null && row["NetAmt"] != DBNull.Value
                        ? row["NetAmt"]
                        : 0f;
                    tempGridView.Rows[index].Cells["Gross"].Value = row.Table.Columns.Contains("Gross") && row["Gross"] != null && row["Gross"] != DBNull.Value
                        ? row["Gross"]
                        : 0f;
                    if (row.Table.Columns.Contains("SellingPrice") && row["SellingPrice"] != null && row["SellingPrice"] != DBNull.Value)
                    {
                        tempGridView.Rows[index].Cells["SellingPrice"].Value = row["SellingPrice"];
                    }
                    if (row.Table.Columns.Contains("UnitSP") && row["UnitSP"] != null && row["UnitSP"] != DBNull.Value)
                    {
                        tempGridView.Rows[index].Cells["UnitSP"].Value = row["UnitSP"];
                    }
                }

                // Set up purchase details with first item
                if (dt.Rows.Count > 0)
                {
                    // Important: Use the same FinYearId as the master record for consistency
                    ObjPurchaseDetails = new PurchaseDetails();
                    ObjPurchaseDetails._Operation = "CREATE";
                    ObjPurchaseDetails.CompanyId = GetCompanyId();
                    ObjPurchaseDetails.BranchID = GetBranchId();
                    ObjPurchaseDetails.FinYearId = ObjPurchaseMaster.FinYearId;
                    ObjPurchaseDetails.BranchName = CmboBranch.Text;
                    ObjPurchaseDetails.PurchaseDate = Convert.ToDateTime(dtpPurchaseDate.Value);
                    ObjPurchaseDetails.InvoiceNo = txtInvoiceNo.Text;
                    ObjPurchaseDetails.SlNo = Convert.ToInt32(dt.Rows[0]["SLNO"]);
                    ObjPurchaseDetails.ItemID = Convert.ToInt32(dt.Rows[0]["ItemId"]);
                    ObjPurchaseDetails.ItemName = dt.Rows[0]["Description"].ToString();
                    ObjPurchaseDetails.UnitId = Convert.ToInt32(dt.Rows[0]["UnitId"]);
                    ObjPurchaseDetails.Unit = dt.Rows[0]["Unit"].ToString();
                    ObjPurchaseDetails.BaseUnit = "Y";
                    ObjPurchaseDetails.Packing = float.Parse(dt.Rows[0]["Packing"].ToString());
                    ObjPurchaseDetails.Qty = float.Parse(dt.Rows[0]["Qty"].ToString());
                    ObjPurchaseDetails.Cost = float.Parse(dt.Rows[0]["Cost"].ToString());
                    ObjPurchaseDetails.Free = float.Parse(dt.Rows[0]["Free"].ToString());
                    ObjPurchaseDetails.TaxPer = float.Parse(dt.Rows[0]["TaxPer"].ToString());
                    ObjPurchaseDetails.TaxAmt = float.Parse(dt.Rows[0]["TaxAmt"].ToString());
                    ObjPurchaseDetails.TaxType = dt.Rows[0]["TaxType"].ToString();
                    ObjPurchaseDetails.PurchaseNo = ObjPurchaseMaster.PurchaseNo;
                }

                // Retrieve the old purchase invoice master and details before updating
                PurchaseInvoiceGrid oldPurchaseForLog = null;
                try
                {
                    oldPurchaseForLog = GetOldPurchaseForLog(ObjPurchaseMaster.Pid);
                    if (oldPurchaseForLog != null && oldPurchaseForLog.Listpdetails != null)
                    {
                        foreach (var pd in oldPurchaseForLog.Listpdetails)
                        {
                            string key = $"{pd.ItemID}_{pd.UnitId}";
                            if (_originalSellingPrices.ContainsKey(key))
                            {
                                pd.SalesPrice = _originalSellingPrices[key];
                            }
                            else
                            {
                                float wholeSalePriceFromPriceSettings = GetWholeSalePriceFromPriceSettings(pd.ItemID, pd.UnitId);
                                pd.SalesPrice = wholeSalePriceFromPriceSettings > 0
                                    ? wholeSalePriceFromPriceSettings
                                    : pd.WholeSalePrice;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading old purchase for log: " + ex.Message);
                }

                // Check CASH-IN-HAND balance if payment mode is Cash
                if (ObjPurchaseInviceRepo.IsCashPaymentMode(ObjPurchaseMaster.PaymodeID, ObjPurchaseMaster.Paymode))
                {
                    double currentCash = ObjPurchaseInviceRepo.GetAvailableCashBalance(GetBranchId());
                    double oldCashCredit = ObjPurchaseInviceRepo.GetOldCashVoucherCreditForPurchase(ObjPurchaseMaster.VoucherID, GetBranchId());
                    double effectiveCash = currentCash + oldCashCredit;
                    double purchaseAmt = ObjPurchaseMaster.GrandTotal;
                    if (effectiveCash - purchaseAmt < 0)
                    {
                        MessageBox.Show($"Insufficient CASH-IN-HAND balance!\n\nAvailable Cash-In-Hand Balance (before update): {effectiveCash:N2}\nUpdated Purchase Amount: {purchaseAmt:N2}\nAvailable Cash after Update: {(effectiveCash - purchaseAmt):N2}\n\nTransaction cannot be updated because CASH-IN-HAND balance cannot be negative.", "Insufficient Cash Balance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Check CASH-IN-HAND balance if payment mode is Cash
                if (ObjPurchaseInviceRepo.IsCashPaymentMode(ObjPurchaseMaster.PaymodeID, ObjPurchaseMaster.Paymode))
                {
                    double currentCash = ObjPurchaseInviceRepo.GetAvailableCashBalance(GetBranchId());
                    double oldCashCredit = ObjPurchaseInviceRepo.GetOldCashVoucherCreditForPurchase(ObjPurchaseMaster.VoucherID, GetBranchId());
                    double effectiveCash = currentCash + oldCashCredit;
                    double purchaseAmt = ObjPurchaseMaster.GrandTotal;
                    if (effectiveCash - purchaseAmt < 0)
                    {
                        MessageBox.Show(
                            $"Insufficient CASH-IN-HAND balance!\n\n" +
                            $"Available Cash-In-Hand Balance (before update): {effectiveCash:N2}\n" +
                            $"Updated Purchase Amount: {purchaseAmt:N2}\n" +
                            $"Shortfall: {(purchaseAmt - effectiveCash):N2}\n\n" +
                            "Transaction cannot be updated — CASH-IN-HAND balance cannot go negative.",
                            "Insufficient Cash Balance",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Show confirmation dialog before updating
                DialogBox.frmSuccesMsg confirmDialog = new DialogBox.frmSuccesMsg(txtPurchaseNo.Text.Replace("GRN-", ""));
                if (confirmDialog.ShowDialog() != DialogResult.Yes)
                {
                    return; // User chose not to update
                }

                // Call the repository method to update the purchase invoice
                string message = ObjPurchaseInviceRepo.UpdatePurchase(ObjPurchaseMaster, ObjPurchaseDetails, tempGridView);

                if (string.IsNullOrEmpty(message) || message.ToLower().Contains("success"))
                {
                    string updateActivityDetails = null;
                    if (oldPurchaseForLog != null)
                    {
                        try
                        {
                            updateActivityDetails = BuildPurchaseUpdateActivityDetails(
                                ObjPurchaseMaster.PurchaseNo,
                                oldPurchaseForLog,
                                tempGridView,
                                Convert.ToDecimal(ObjPurchaseMaster.NetTotal > 0 ? ObjPurchaseMaster.NetTotal : ObjPurchaseMaster.GrandTotal)
                            );
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error building purchase update log details: " + ex.Message);
                        }
                    }

                    SavePurchaseActivityLog(
                        "UPDATE", 
                        ObjPurchaseMaster.PurchaseNo, 
                        ObjPurchaseMaster, 
                        updateActivityDetails, 
                        !string.IsNullOrWhiteSpace(updateActivityDetails));

                    // Show a simple success message now instead of the popup
                    MessageBox.Show("Updated Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear fields after successful update
                    Clear();

                    // Reset button visibility
                    pbxSave.Visible = true;
                    ultraPictureBox4.Visible = false;
                }
                else
                {
                    // Show error message
                    MessageBox.Show("Error updating purchase: " + message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating purchase: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isUpdatingPurchase = false;
            }
        }

        // Helper method to get the original FinYearId from the database
        private int GetOriginalFinYearId(int pid)
        {
            try
            {
                // Default to 1 if something goes wrong
                int finYearId = 1;

                // Get the purchase details from database to retrieve the correct FinYearId
                PurchaseInvoiceGrid purchaseData = ObjPurchaseInviceRepo.getPurchaseNumber(pid);
                if (purchaseData != null && purchaseData.Listpmaster != null && purchaseData.Listpmaster.Count() > 0)
                {
                    finYearId = purchaseData.Listpmaster.First().FinYearId;
                }

                return finYearId;
            }
            catch (Exception)
            {
                // If there's any error, return 1 as default
                return 1;
            }
        }

        // Method to move grid selection up one row
        private void MoveGridSelectionUp()
        {
            if (ultraGrid1.Rows.Count == 0)
                return;

            try
            {
                ultraGrid1.BeginUpdate();

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
                    UltraGridRow previousRow = ultraGrid1.Rows[currentIndex - 1];
                    ultraGrid1.ActiveRow = previousRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(previousRow);

                    // Ensure the row is visible
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(previousRow);

                    // Set cell focus
                    if (previousRow.Cells.Exists("Qty"))
                    {
                        ultraGrid1.ActiveCell = previousRow.Cells["Qty"];
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }

                    // Force UI refresh
                    ultraGrid1.Refresh();
                }
            }
            finally
            {
                ultraGrid1.EndUpdate();
            }
        }

        // Method to move grid selection down one row
        private void MoveGridSelectionDown()
        {
            if (ultraGrid1.Rows.Count == 0)
                return;

            try
            {
                ultraGrid1.BeginUpdate();

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
                    UltraGridRow nextRow = ultraGrid1.Rows[currentIndex + 1];
                    ultraGrid1.ActiveRow = nextRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(nextRow);

                    // Ensure the row is visible
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(nextRow);

                    // Set cell focus
                    if (nextRow.Cells.Exists("Qty"))
                    {
                        ultraGrid1.ActiveCell = nextRow.Cells["Qty"];
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }

                    // Force UI refresh
                    ultraGrid1.Refresh();
                }
            }
            finally
            {
                ultraGrid1.EndUpdate();
            }
        }

        // Method to move selected item up in the grid (swap with the item above)
        private void MoveSelectedItemUp()
        {
            if (ultraGrid1.Rows.Count <= 1)
                return;

            // Check if there's an active row selected
            if (ultraGrid1.ActiveRow == null)
            {
                MessageBox.Show("Please select an item to move", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex > 0)
            {
                try
                {
                    if (!EnsurePurchaseRowEditable(ultraGrid1.ActiveRow, true))
                    {
                        return;
                    }

                    // Get the DataTable
                    DataTable dt = (DataTable)ultraGrid1.DataSource;

                    // Create a new DataRow to hold the current row temporarily
                    DataRow tempRow = dt.NewRow();
                    tempRow.ItemArray = dt.Rows[currentIndex].ItemArray;

                    // Remove the current row
                    dt.Rows.RemoveAt(currentIndex);

                    // Insert it at the position above
                    dt.Rows.InsertAt(tempRow, currentIndex - 1);

                    // Fix SLNO values for all rows to ensure sequential numbering
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["SLNO"] = i + 1;
                    }

                    // Now select the moved row
                    ultraGrid1.Rows[currentIndex - 1].Selected = true;
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex - 1];
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error moving row up: " + ex.Message);
                }
            }
        }

        // Method to move selected item down in the grid (swap with the item below)
        private void MoveSelectedItemDown()
        {
            if (ultraGrid1.Rows.Count <= 1)
                return;

            // Check if there's an active row selected
            if (ultraGrid1.ActiveRow == null)
            {
                MessageBox.Show("Please select an item to move", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex < ultraGrid1.Rows.Count - 1)
            {
                try
                {
                    if (!EnsurePurchaseRowEditable(ultraGrid1.ActiveRow, true))
                    {
                        return;
                    }

                    // Get the DataTable
                    DataTable dt = (DataTable)ultraGrid1.DataSource;

                    // Create a new DataRow to hold the current row temporarily
                    DataRow tempRow = dt.NewRow();
                    tempRow.ItemArray = dt.Rows[currentIndex].ItemArray;

                    // Remove the current row
                    dt.Rows.RemoveAt(currentIndex);

                    // Insert it at the position below
                    dt.Rows.InsertAt(tempRow, currentIndex + 1);

                    // Fix SLNO values for all rows to ensure sequential numbering
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["SLNO"] = i + 1;
                    }

                    // Now select the moved row
                    ultraGrid1.Rows[currentIndex + 1].Selected = true;
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex + 1];
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error moving row down: " + ex.Message);
                }
            }
        }

        // Add DeletePurchase method
        public void DeletePurchase()
        {
            if (_isReadOnly)
            {
                MessageBox.Show(MsgReadOnlyBlock, MsgReadOnlyModeTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Make sure we have the required data
                if (string.IsNullOrEmpty(LblPid.Text) || string.IsNullOrEmpty(lblVoucherId.Text) || string.IsNullOrEmpty(txtPurchaseNo.Text))
                {
                    MessageBox.Show("Please select a purchase invoice to delete", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion with the userF
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete Purchase No: " + txtPurchaseNo.Text + "?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    int purchaseNo = int.Parse(txtPurchaseNo.Text.Replace("GRN-", ""));
                    int voucherId = int.Parse(lblVoucherId.Text);

                    // Get the original FinYearId to ensure data consistency
                    int finYearId = GetOriginalFinYearId(Convert.ToInt32(LblPid.Text));

                    // Call repository method to delete the purchase
                    string message = ObjPurchaseInviceRepo.DeletePurchaseInvoice(
                        purchaseNo,
                        Convert.ToInt32(DataBase.BranchId),
                        finYearId,
                        voucherId);

                    if (string.IsNullOrEmpty(message) || message.ToLower().Contains("success"))
                    {
                        SavePurchaseActivityLog(
                            "DELETE",
                            purchaseNo,
                            txtInvoiceNo.Text,
                            CmboVendor.Text,
                            CmboPayment.Text,
                            ParseDecimal(label6.Text),
                            $"Purchase invoice GRN-{purchaseNo} deleted.");

                        // Show success message
                        MessageBox.Show("Purchase invoice deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Clear the form
                        this.Clear();
                    }
                    else
                    {
                        // Show error message
                        MessageBox.Show("Error deleting purchase: " + message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting purchase: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SavePurchaseActivityLog(string activityType, int purchaseNo, PurchaseMaster purchaseMaster, string details = null, bool detailsAreFinal = false)
        {
            if (purchaseMaster == null)
            {
                return;
            }

            decimal qty;
            decimal cost;
            string unit;
            string barcode;
            GetPurchaseActivitySummary(out qty, out cost, out unit, out barcode);

            decimal sellingPrice;
            decimal taxAmt;
            decimal taxPer;
            decimal baseCost;
            decimal packing;
            decimal retailPrice;
            decimal free;
            decimal unitSP;
            string taxType;
            decimal gross;
            GetPurchaseLogColumnSummary(out sellingPrice, out taxAmt, out taxPer, out baseCost, out packing, out retailPrice, out free, out unitSP, out taxType, out gross);

            decimal netAmount = Convert.ToDecimal(purchaseMaster.NetTotal > 0 ? purchaseMaster.NetTotal : purchaseMaster.GrandTotal);
            
            string fullDetails = detailsAreFinal && !string.IsNullOrWhiteSpace(details)
                ? details
                : BuildPurchaseActivityDetails(activityType, purchaseNo, netAmount);

            SavePurchaseActivityLog(
                activityType,
                purchaseNo,
                purchaseMaster.InvoiceNo,
                purchaseMaster.VendorName,
                purchaseMaster.Paymode,
                netAmount,
                fullDetails,
                qty,
                cost,
                unit,
                barcode,
                sellingPrice,
                taxAmt,
                taxPer,
                baseCost,
                packing,
                retailPrice,
                free,
                unitSP,
                taxType,
                gross);
        }

        private void SavePurchaseActivityLog(
            string activityType,
            int purchaseNo,
            string invoiceNo,
            string vendorName,
            string paymentMode,
            decimal netAmount,
            string details,
            decimal? qty = null,
            decimal? cost = null,
            string unit = null,
            string barcode = null,
            decimal? sPrice = null,
            decimal? taxAmt = null,
            decimal? taxPer = null,
            decimal? baseAmount = null,
            decimal? packing = null,
            decimal? retailPrice = null,
            decimal? free = null,
            decimal? unitSP = null,
            string taxType = null,
            decimal? gross = null)
        {
            try
            {
                using (var repo = new TransactionActivityLogRepository())
                {
                    repo.SavePurchaseActivity(
                        purchaseNo,
                        invoiceNo,
                        vendorName,
                        paymentMode,
                        netAmount,
                        activityType,
                        details,
                        qty,
                        cost,
                        unit,
                        barcode,
                        sPrice,
                        taxAmt,
                        taxPer,
                        baseAmount,
                        packing,
                        retailPrice,
                        free,
                        unitSP,
                        taxType,
                        gross);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to save purchase activity log: " + ex.Message);
            }
        }

        private void CapturePurchaseHeaderActivitySnapshot()
        {
            _activityInvoiceNo = txtInvoiceNo.Text ?? string.Empty;
            _activityBilledBy = txtBilledBy.Text ?? string.Empty;
            _activityInvoiceDate = GetEditorDate(DtpInoviceDate);
            _activityPurchaseDate = GetEditorDate(dtpPurchaseDate);
        }

        private void ClearPurchaseHeaderActivitySnapshot()
        {
            _activityInvoiceNo = string.Empty;
            _activityBilledBy = string.Empty;
            _activityInvoiceDate = null;
            _activityPurchaseDate = null;
        }

        private bool TryGetCurrentPurchaseNo(out int purchaseNo)
        {
            purchaseNo = 0;
            string raw = (txtPurchaseNo.Text ?? string.Empty).Replace("GRN-", string.Empty).Trim();
            return int.TryParse(raw, out purchaseNo) && purchaseNo > 0;
        }

        private DateTime? GetEditorDate(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            if (editor == null || editor.Value == null)
            {
                return null;
            }

            return Convert.ToDateTime(editor.Value).Date;
        }

        private void AddDateChange(List<string> changes, string caption, DateTime? oldValue, DateTime? newValue)
        {
            if (oldValue.GetValueOrDefault(DateTime.MinValue) == newValue.GetValueOrDefault(DateTime.MinValue))
            {
                return;
            }

            changes.Add($"{caption} changed from {FormatDate(oldValue)} to {FormatDate(newValue)}");
        }

        private void AddTextUpdate(List<string> changes, string caption, string oldValue, string newValue)
        {
            oldValue = (oldValue ?? string.Empty).Trim();
            newValue = (newValue ?? string.Empty).Trim();
            if (!string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase))
            {
                changes.Add($"{caption} updated to {FormatTextValue(newValue)} (was {FormatTextValue(oldValue)})");
            }
        }

        private void AddDateUpdate(List<string> changes, string caption, DateTime? oldValue, DateTime? newValue)
        {
            if (oldValue.GetValueOrDefault(DateTime.MinValue) == newValue.GetValueOrDefault(DateTime.MinValue))
            {
                return;
            }

            changes.Add($"{caption} updated to {FormatDate(newValue)} (was {FormatDate(oldValue)})");
        }

        private string FormatDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("dd MMM yyyy") : "(blank)";
        }

        private void GetPurchaseLogColumnSummary(
            out decimal sellingPrice,
            out decimal taxAmt,
            out decimal taxPer,
            out decimal baseCost,
            out decimal packing,
            out decimal retailPrice,
            out decimal free,
            out decimal unitSP,
            out string taxType,
            out decimal gross)
        {
            sellingPrice = 0m;
            taxAmt = 0m;
            taxPer = 0m;
            baseCost = 0m;
            packing = 0m;
            retailPrice = 0m;
            free = 0m;
            unitSP = 0m;
            taxType = string.Empty;
            gross = 0m;

            DataTable table = ultraGrid1.DataSource as DataTable;
            if (table == null || table.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                if (sellingPrice == 0m)
                {
                    sellingPrice = GetDecimal(row, "SellingPrice");
                }

                if (taxPer == 0m)
                {
                    taxPer = GetDecimal(row, "TaxPer");
                }

                if (packing == 0m)
                {
                    packing = GetDecimal(row, "Packing");
                }

                if (retailPrice == 0m)
                {
                    retailPrice = GetDecimal(row, "RetailPrice");
                }

                if (unitSP == 0m)
                {
                    unitSP = GetDecimal(row, "UnitSP");
                }

                if (string.IsNullOrEmpty(taxType))
                {
                    taxType = GetString(row, "TaxType");
                }

                taxAmt += GetDecimal(row, "TaxAmt");
                baseCost += GetDecimal(row, "BaseCost");
                free += GetDecimal(row, "Free");
                gross += GetDecimal(row, "Gross");
            }
        }

        private PurchaseInvoiceGrid GetOldPurchaseForLog(int pid)
        {
            try
            {
                return ObjPurchaseInviceRepo.getPurchaseNumber(pid);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to load old purchase details for activity log: " + ex.Message);
                return null;
            }
        }

        private string BuildPurchaseUpdateActivityDetails(
            int purchaseNo,
            PurchaseInvoiceGrid oldPurchase,
            DataGridView newDetails,
            decimal netAmount)
        {
            var changes = new List<string>();
            var updatedItemNames = new List<string>();
            var purchaseItemNames = new List<string>();
            PurchaseMaster oldMaster = oldPurchase != null && oldPurchase.Listpmaster != null ? oldPurchase.Listpmaster.FirstOrDefault() : null;

            if (oldMaster != null)
            {
                AddTextChange(changes, "Vendor", oldMaster.VendorName, CmboVendor.Text);
                AddTextChange(changes, "Paymode", oldMaster.Paymode, CmboPayment.Text);
                AddTextUpdate(changes, "txtInvoiceNo", oldMaster.InvoiceNo, txtInvoiceNo.Text);
                AddTextUpdate(changes, "txtBilledBy", oldMaster.BilledBy, txtBilledBy.Text);
                AddDateUpdate(changes, "DtpInoviceDate", oldMaster.InvoiceDate.Date, GetEditorDate(DtpInoviceDate));
                AddDateUpdate(changes, "dtpPurchaseDate", oldMaster.PurchaseDate.Date, GetEditorDate(dtpPurchaseDate));
            }

            var oldRows = oldPurchase != null && oldPurchase.Listpdetails != null
                ? oldPurchase.Listpdetails.ToList()
                : new List<PurchaseDetails>();

            if (newDetails != null)
            {
                foreach (DataGridViewRow row in newDetails.Rows)
                {
                    if (row.IsNewRow)
                    {
                        continue;
                    }

                    PurchaseDetails oldRow = TakeMatchingPurchaseSnapshot(oldRows, row);
                    string itemName = GetCellString(row, "Description"); // "Description" represents ItemName in grid
                    if (string.IsNullOrWhiteSpace(itemName) && oldRow != null)
                    {
                        itemName = oldRow.ItemName;
                    }

                    if (!string.IsNullOrWhiteSpace(itemName))
                    {
                        purchaseItemNames.Add(FormatItemName(itemName));
                    }

                    if (oldRow == null)
                    {
                        changes.Add($"Added item \"{FormatItemName(itemName)}\" with {BuildPurchaseItemSummary(row)}");
                        updatedItemNames.Add(FormatItemName(itemName));
                        continue;
                    }

                    var itemChanges = new List<string>();
                    AddTextChange(itemChanges, "Barcode", oldRow.Barcode, GetCellString(row, "BarCode"));
                    AddTextChange(itemChanges, "Unit", oldRow.Unit, GetCellString(row, "Unit"));
                    AddNumericChange(itemChanges, "Qty", Convert.ToDecimal(oldRow.Qty), GetCellDecimal(row, "Qty"));
                    AddNumericChange(itemChanges, "Cost", Convert.ToDecimal(oldRow.Cost), GetCellDecimal(row, "Cost"));
                    AddNumericChange(itemChanges, "Free", Convert.ToDecimal(oldRow.Free), GetCellDecimal(row, "Free"));
                    AddNumericChange(itemChanges, "Packing", Convert.ToDecimal(oldRow.Packing), GetCellDecimal(row, "Packing"));
                    AddNumericChange(itemChanges, "Tax %", Convert.ToDecimal(oldRow.TaxPer), GetCellDecimal(row, "TaxPer"));
                    AddNumericChange(itemChanges, "TaxAmt", Convert.ToDecimal(oldRow.TaxAmt), GetCellDecimal(row, "TaxAmt"));
                    AddNumericChange(itemChanges, "Retail Price", Convert.ToDecimal(oldRow.RetailPrice), GetCellDecimal(row, "RetailPrice"));
                    
                    decimal newSellingPrice = GetCellDecimal(row, "SellingPrice");
                    if (newSellingPrice == 0m)
                    {
                        newSellingPrice = GetCellDecimal(row, "WholeSalePrice");
                    }
                    AddNumericChange(itemChanges, "Selling price", Convert.ToDecimal(oldRow.SalesPrice), newSellingPrice);
                    AddNumericChange(itemChanges, "Unit SP", GetOldPurchaseUnitSP(oldRow), GetCellDecimal(row, "UnitSP"));
                    AddNumericChange(itemChanges, "Base Cost", GetOldPurchaseBaseCost(oldRow), GetCellDecimal(row, "BaseCost"));
                    AddNumericChange(itemChanges, "Gross", GetOldPurchaseGross(oldRow), GetCellDecimal(row, "Gross"));
                    
                    // Row level net amount: Qty * Cost + TaxAmt
                    decimal oldNetAmt = Convert.ToDecimal(oldRow.Qty * oldRow.Cost + oldRow.TaxAmt);
                    AddNumericChange(itemChanges, "Net amount", oldNetAmt, GetCellDecimal(row, "NetAmt"));

                    if (itemChanges.Count > 0)
                    {
                        changes.Add($"{FormatItemName(itemName)}: {string.Join("; ", itemChanges)}");
                        updatedItemNames.Add(FormatItemName(itemName));
                    }
                }
            }

            foreach (var remaining in oldRows)
            {
                changes.Add($"Removed item \"{FormatItemName(remaining.ItemName)}\"");
                updatedItemNames.Add(FormatItemName(remaining.ItemName));
            }

            if (changes.Count == 0 && updatedItemNames.Count == 0)
            {
                changes.Add("No line item change detected");
            }

            changes.Add($"Net amount = {FormatAmount(netAmount)}");

            var sourceItemNames = purchaseItemNames.Count > 0 ? purchaseItemNames : updatedItemNames;
            var uniqueItemNames = sourceItemNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(x => $"- \"{x}\"")
                .ToList();
            string itemSection = uniqueItemNames.Count > 0
                ? $"{Environment.NewLine}Items:{Environment.NewLine}{string.Join(Environment.NewLine, uniqueItemNames)}"
                : string.Empty;

            return $"Purchase invoice GRN-{purchaseNo} updated.{itemSection}{Environment.NewLine}Changes:{Environment.NewLine}- {string.Join(Environment.NewLine + "- ", changes)}";
        }

        private string BuildPurchaseItemSummary(DataGridViewRow row)
        {
            if (row == null)
            {
                return "item details unavailable";
            }

            var parts = new List<string>
            {
                $"Qty {FormatAmount(GetCellDecimal(row, "Qty"))}",
                $"Cost {FormatAmount(GetCellDecimal(row, "Cost"))}",
                $"Selling price {FormatAmount(GetCellDecimal(row, "SellingPrice"))}",
                $"TaxAmt {FormatAmount(GetCellDecimal(row, "TaxAmt"))}",
                $"NetAmt {FormatAmount(GetCellDecimal(row, "NetAmt"))}"
            };

            string unit = GetCellString(row, "Unit");
            if (!string.IsNullOrWhiteSpace(unit))
            {
                parts.Add($"Unit {unit}");
            }

            string barcode = GetCellString(row, "BarCode");
            if (!string.IsNullOrWhiteSpace(barcode))
            {
                parts.Add($"Barcode {barcode}");
            }

            return string.Join(", ", parts);
        }

        private PurchaseDetails TakeMatchingPurchaseSnapshot(List<PurchaseDetails> oldRows, DataGridViewRow newRow)
        {
            if (oldRows == null || oldRows.Count == 0 || newRow == null)
            {
                return null;
            }

            int itemId = GetInt(newRow, "ItemId");
            string barcode = NormalizeLogKey(GetCellString(newRow, "BarCode"));
            string itemName = NormalizeLogKey(GetCellString(newRow, "Description"));
            string unit = NormalizeLogKey(GetCellString(newRow, "Unit"));

            int matchIndex = oldRows.FindIndex(oldRow => itemId > 0 && oldRow.ItemID == itemId);

            if (matchIndex < 0 && !string.IsNullOrWhiteSpace(barcode))
            {
                matchIndex = oldRows.FindIndex(oldRow => NormalizeLogKey(oldRow.Barcode) == barcode);
            }

            if (matchIndex < 0 && !string.IsNullOrWhiteSpace(itemName) && !string.IsNullOrWhiteSpace(unit))
            {
                matchIndex = oldRows.FindIndex(oldRow =>
                    NormalizeLogKey(oldRow.ItemName) == itemName &&
                    NormalizeLogKey(oldRow.Unit) == unit);
            }

            if (matchIndex < 0 && !string.IsNullOrWhiteSpace(itemName))
            {
                matchIndex = oldRows.FindIndex(oldRow => NormalizeLogKey(oldRow.ItemName) == itemName);
            }

            if (matchIndex < 0)
            {
                return null;
            }

            PurchaseDetails match = oldRows[matchIndex];
            oldRows.RemoveAt(matchIndex);
            return match;
        }

        private decimal GetOldPurchaseUnitSP(PurchaseDetails oldRow)
        {
            if (oldRow == null)
            {
                return 0m;
            }

            float unit1WholeSalePrice = GetWholeSalePriceFromPriceSettings(oldRow.ItemID, 1);
            if (unit1WholeSalePrice > 0f)
            {
                return Convert.ToDecimal(unit1WholeSalePrice);
            }

            return Convert.ToDecimal(oldRow.WholeSalePrice);
        }

        private decimal GetOldPurchaseBaseCost(PurchaseDetails oldRow)
        {
            if (oldRow == null)
            {
                return 0m;
            }

            decimal cost = Convert.ToDecimal(oldRow.Cost);
            string taxType = oldRow.TaxType ?? "incl";
            decimal taxPer = Convert.ToDecimal(oldRow.TaxPer);

            if (string.Equals(taxType, "incl", StringComparison.OrdinalIgnoreCase) && taxPer > 0m)
            {
                decimal taxAmtPerUnit = (cost * taxPer) / (100m + taxPer);
                return cost - taxAmtPerUnit;
            }

            return cost;
        }

        private decimal GetOldPurchaseGross(PurchaseDetails oldRow)
        {
            if (oldRow == null)
            {
                return 0m;
            }

            decimal amount = Convert.ToDecimal(oldRow.Cost * oldRow.Qty);
            string taxType = oldRow.TaxType ?? "incl";
            if (string.Equals(taxType, "incl", StringComparison.OrdinalIgnoreCase))
            {
                return amount - Convert.ToDecimal(oldRow.TaxAmt);
            }

            return 0m;
        }

        private string NormalizeLogKey(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private decimal GetCellDecimal(DataGridViewRow row, string columnName)
        {
            if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(columnName))
            {
                return 0m;
            }

            object value = row.Cells[columnName].Value;
            decimal parsed;
            return value != null && value != DBNull.Value && decimal.TryParse(Convert.ToString(value), out parsed) ? parsed : 0m;
        }

        private int GetInt(DataGridViewRow row, string columnName)
        {
            return Convert.ToInt32(GetCellDecimal(row, columnName));
        }

        private string GetCellString(DataGridViewRow row, string columnName)
        {
            if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            object value = row.Cells[columnName].Value;
            return value == null || value == DBNull.Value ? string.Empty : Convert.ToString(value);
        }

        private string FormatAmount(decimal value)
        {
            return value.ToString("0.####");
        }

        private string FormatTextValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(blank)" : $"\"{value.Trim()}\"";
        }

        private string FormatItemName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Item" : value.Trim();
        }

        private void AddNumericChange(List<string> changes, string caption, decimal oldValue, decimal newValue)
        {
            if (Math.Abs(oldValue - newValue) >= 0.0001m)
            {
                changes.Add($"{caption} updated from {FormatAmount(oldValue)} to {FormatAmount(newValue)}");
            }
        }

        private void AddTextChange(List<string> changes, string caption, string oldValue, string newValue)
        {
            oldValue = (oldValue ?? string.Empty).Trim();
            newValue = (newValue ?? string.Empty).Trim();
            if (!string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase))
            {
                changes.Add($"{caption} updated from {FormatTextValue(oldValue)} to {FormatTextValue(newValue)}");
            }
        }

        private void GetPurchaseActivitySummary(out decimal qty, out decimal cost, out string unit, out string barcode)
        {
            qty = 0m;
            cost = 0m;
            unit = string.Empty;
            barcode = string.Empty;

            DataTable table = ultraGrid1.DataSource as DataTable;
            if (table == null || table.Rows.Count == 0)
            {
                return;
            }

            var units = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var barcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            decimal firstCost = 0m;

            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                qty += GetDecimal(row, "Qty");
                if (firstCost == 0m)
                {
                    firstCost = GetDecimal(row, "Cost");
                }

                string rowUnit = GetString(row, "Unit");
                if (!string.IsNullOrWhiteSpace(rowUnit))
                {
                    units.Add(rowUnit);
                }

                string rowBarcode = GetString(row, "BarCode");
                if (!string.IsNullOrWhiteSpace(rowBarcode))
                {
                    barcodes.Add(rowBarcode);
                }
            }

            cost = firstCost;
            unit = units.Count == 1 ? units.First() : units.Count > 1 ? "Multiple" : string.Empty;
            barcode = barcodes.Count == 1 ? barcodes.First() : barcodes.Count > 1 ? "Multiple" : string.Empty;
        }

        private string BuildPurchaseActivityDetails(string activityType, int purchaseNo, decimal netAmount)
        {
            string baseText = $"Purchase invoice GRN-{purchaseNo} {activityType.ToLowerInvariant()}d.";
            DataTable table = ultraGrid1.DataSource as DataTable;
            if (string.Equals(activityType, "SAVE", StringComparison.OrdinalIgnoreCase))
            {
                var lines = new List<string>
                {
                    baseText,
                    "Header:",
                    $"- txtInvoiceNo: {FormatTextValue(txtInvoiceNo.Text)}",
                    $"- txtBilledBy: {FormatTextValue(txtBilledBy.Text)}",
                    $"- DtpInoviceDate: {FormatDate(GetEditorDate(DtpInoviceDate))}",
                    $"- dtpPurchaseDate: {FormatDate(GetEditorDate(dtpPurchaseDate))}",
                    $"- Vendor: {FormatTextValue(CmboVendor.Text)}",
                    $"- Paymode: {FormatTextValue(CmboPayment.Text)}",
                    $"- Net amount: {FormatAmount(netAmount)}",
                    "Purchased items:"
                };

                if (table == null || table.Rows.Count == 0)
                {
                    lines.Add("- No item rows available.");
                    return string.Join(Environment.NewLine, lines);
                }

                int itemNo = 1;
                foreach (DataRow row in table.Rows)
                {
                    if (row.RowState == DataRowState.Deleted)
                    {
                        continue;
                    }

                    string itemName = GetString(row, "Description");
                    if (string.IsNullOrWhiteSpace(itemName))
                    {
                        itemName = "Item";
                    }

                    var itemParts = new List<string>
                    {
                        $"{itemNo}. {itemName}",
                        $"Qty: {FormatAmount(GetDecimal(row, "Qty"))}",
                        $"Cost: {FormatAmount(GetDecimal(row, "Cost"))}",
                        $"Selling price: {FormatAmount(GetDecimal(row, "SellingPrice"))}",
                        $"TaxAmt: {FormatAmount(GetDecimal(row, "TaxAmt"))}",
                        $"NetAmt: {FormatAmount(GetDecimal(row, "NetAmt"))}"
                    };

                    string unit = GetString(row, "Unit");
                    if (!string.IsNullOrWhiteSpace(unit))
                    {
                        itemParts.Add($"Unit: {unit}");
                    }

                    string barcode = GetString(row, "BarCode");
                    if (!string.IsNullOrWhiteSpace(barcode))
                    {
                        itemParts.Add($"Barcode: {barcode}");
                    }

                    lines.Add("- " + string.Join(", ", itemParts));
                    itemNo++;
                }

                return string.Join(Environment.NewLine, lines);
            }

            if (table == null || table.Rows.Count == 0)
            {
                return $"{baseText} Net amount = {netAmount:0.##}.";
            }

            var changes = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                AddChangedValue(changes, "Qty", GetDecimal(row, "OldQty"), GetDecimal(row, "Qty"));
                AddChangedValue(changes, "Cost", GetDecimal(row, "OriginalCost"), GetDecimal(row, "Cost"));
            }

            if (changes.Count == 0)
            {
                decimal qty;
                decimal cost;
                string unit;
                string barcode;
                GetPurchaseActivitySummary(out qty, out cost, out unit, out barcode);
                changes.Add($"Qty = {qty:0.####}");
                if (cost != 0m) changes.Add($"Cost = {cost:0.####}");
                if (!string.IsNullOrWhiteSpace(unit)) changes.Add($"Unit = {unit}");
                if (!string.IsNullOrWhiteSpace(barcode)) changes.Add($"Barcode = {barcode}");
            }

            changes.Add($"Net amount = {netAmount:0.##}");
            return $"{baseText} {string.Join(", ", changes)}.";
        }

        private void AddChangedValue(List<string> changes, string caption, decimal oldValue, decimal newValue)
        {
            if (oldValue != 0m && oldValue != newValue)
            {
                changes.Add($"{caption} changed from {oldValue:0.####} to {newValue:0.####}");
            }
        }

        private decimal GetDecimal(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0m;
            }

            decimal value;
            return decimal.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0m;
        }

        private string GetString(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName]);
        }

        private decimal ParseDecimal(string value)
        {
            decimal parsed;
            return decimal.TryParse(value, out parsed) ? parsed : 0m;
        }

        // Method to remove the selected item from the grid
        private void RemoveSelectedItem()
        {
            // Check if there's an active row selected
            if (ultraGrid1.ActiveRow == null)
            {
                MessageBox.Show("Please select an item to remove", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the DataTable from the UltraGrid
            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt == null) return;

            try
            {
                // Store the current selected index
                int currentIndex = ultraGrid1.ActiveRow.Index;
                if (!EnsurePurchaseRowEditable(ultraGrid1.ActiveRow, true))
                {
                    return;
                }

                // Remove the selected row from the DataTable
                dt.Rows.RemoveAt(currentIndex);

                // Renumber the SLNO column to maintain sequential numbering
                int slnoCounter = 1;
                foreach (DataRow row in dt.Rows)
                {
                    row["SLNO"] = slnoCounter++;
                }

                // Recalculate totals
                this.CaluateTotals();

                // Update column visibility and caption after item removal
                UpdateNetAmtColumnVisibility();
                UpdateGrossColumnVisibility();


                // Select another row if available
                if (dt.Rows.Count > 0)
                {
                    // Try to select the same index, or the last row if that index is now beyond bounds
                    int newIndex = Math.Min(currentIndex, dt.Rows.Count - 1);
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[newIndex];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                }

                // Move focus to barcode field
                this.barcodeFocus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error removing item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add this method to handle textBox1 KeyDown event
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if Enter key was pressed
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent "ding" sound
                e.Handled = true;

                // Try to parse the vendor ID from textBox1
                int vendorId;
                if (int.TryParse(textBox1.Text, out vendorId))
                {
                    // Get the DataSource from CmboVendor
                    var dataSource = CmboVendor.DataSource as IEnumerable<dynamic>;
                    if (dataSource != null)
                    {
                        // Find the vendor directly without cycling through items
                        CmboVendor.Value = vendorId;

                        // Check if a valid vendor was found
                        if (CmboVendor.SelectedIndex == -1)
                        {
                            MessageBox.Show("Vendor ID not found: " + vendorId, "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }

        // Add the event handler for CmboVendor
        private void CmboBranch_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (CmboBranch.SelectedValue != null)
                {
                    int branchId = Convert.ToInt32(CmboBranch.SelectedValue);

                    // Update SessionContext with the new branch
                    if (SessionContext.IsInitialized)
                    {
                        SessionContext.BranchId = branchId;
                        // Also update DataBase for backward compatibility
                        DataBase.BranchId = branchId.ToString();

                        // Get branch name
                        if (CmboBranch.SelectedItem != null)
                        {
                            var branch = CmboBranch.SelectedItem as dynamic;
                            if (branch != null)
                            {
                                string branchName = branch.BranchName?.ToString() ?? "";
                                SessionContext.BranchName = branchName;
                                DataBase.Branch = branchName;
                            }
                        }
                    }
                    else
                    {
                        // Initialize SessionContext if not already initialized
                        int companyId = GetCompanyId();
                        int finYearId = GetFinYearId();
                        int userId = GetUserId();

                        SessionContext.InitializeFromLogin(
                            companyId: companyId,
                            branchId: branchId,
                            finYearId: finYearId,
                            userId: userId,
                            userName: GetUserName(),
                            userLevel: DataBase.UserLevel ?? "",
                            emailId: DataBase.EmailId ?? "",
                            branchName: CmboBranch.Text,
                            companyName: null
                        );
                    }

                    // Regenerate purchase number for the new branch
                    GenerateAndDisplayPurchaseNumber();

                    System.Diagnostics.Debug.WriteLine($"Branch changed to BranchId={branchId}, regenerated purchase number");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling branch change: {ex.Message}");
            }
        }

        private void CmboVendor_ValueChanged(object sender, EventArgs e)
        {
            if (CmboVendor.Value != null)
            {
                textBox1.Text = CmboVendor.Value.ToString();
            }
        }

        // Add the TextChanged event handler for textBox1
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Check if the text is "ModelClass.VendorDDLG" and clear it
            if (textBox1.Text == "ModelClass.VendorDDLG")
            {
                textBox1.Text = "";
            }

            // Update background color based on content
            SetEditorBackColor(textBox1, !string.IsNullOrWhiteSpace(textBox1.Text) ?
                defaultBackColor : mandatoryFieldColor);
        }

        private void UltraGrid1_DoubleClickCell(object sender, Infragistics.Win.UltraWinGrid.DoubleClickCellEventArgs e)
        {
            // Check if a valid cell was double-clicked
            if (e.Cell == null || ultraGrid1.ActiveRow == null)
                return;

            // Get the column key to determine which dialog to show
            string columnKey = e.Cell.Column.Key;

            if (string.Equals(columnKey, "BarCode", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnKey, "Description", StringComparison.OrdinalIgnoreCase))
                return;

            UltraGridRow targetRow = e.Cell.Row;
            if (targetRow == null)
                return;

            ultraGrid1.ActiveRow = targetRow;
            ultraGrid1.Selected.Rows.Clear();
            ultraGrid1.Selected.Rows.Add(targetRow);

            if (!EnsurePurchaseRowEditable(targetRow, true))
                return;

            try
            {
                switch (columnKey)
                {
                    case "Description":
                        // Open Edit.cs for editing the item description
                        Edit editForm = Edit.CreateForPurchaseForm(this);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            // Update the item description in the grid
                            string updatedDescription = editForm.ItemDescription;
                            if (!string.IsNullOrEmpty(updatedDescription))
                            {
                                e.Cell.Value = updatedDescription;
                                ultraGrid1.Refresh();
                            }
                        }
                        break;

                    case "Unit":
                        OpenUnitDialogForPurchaseRow(targetRow, false);
                        break;

                    case "Qty":
                        // Open QuantityEditDig.cs for editing the quantity
                        decimal currentQty = 0;
                        if (e.Cell.Value != null)
                        {
                            decimal.TryParse(e.Cell.Value.ToString(), out currentQty);
                        }

                        // Use the correct namespace for frmQuantityDialog
                        PosBranch_Win.Transaction.frmQuantityDialog quantityDialog = new PosBranch_Win.Transaction.frmQuantityDialog(currentQty);
                        if (quantityDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Update the quantity
                            decimal newQty = quantityDialog.Quantity;
                            e.Cell.Value = (float)newQty;

                            // Get cost with tax and recalculate everything
                            float costWithTax = 0;
                            if (ultraGrid1.ActiveRow.Cells["Cost"].Value != null)
                            {
                                float.TryParse(ultraGrid1.ActiveRow.Cells["Cost"].Value.ToString(), out costWithTax);
                            }

                            // Recalculate tax, base cost, and amounts for the updated quantity
                            RecalculateTaxForRow(ultraGrid1.ActiveRow, costWithTax, (float)newQty);

                            // Recalculate totals
                            this.CaluateTotals();
                        }
                        break;

                    case "BaseCost":
                    case "Cost":
                        // Open frmCalculator.cs for editing the cost
                        decimal currentCost = 0;
                        if (e.Cell.Value != null)
                        {
                            decimal.TryParse(e.Cell.Value.ToString(), out currentCost);
                        }

                        DialogBox.frmCalculator calcDialog = new DialogBox.frmCalculator(currentCost);
                        if (calcDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Update the cost
                            decimal newCost = calcDialog.CalculatedValue;
                            e.Cell.Value = (float)newCost;

                            // The AfterCellUpdate event will handle recalculation automatically
                            // Recalculate totals
                            this.CaluateTotals();
                        }
                        break;

                    case "Free":
                        // Open frmCalculator.cs for editing the free quantity
                        decimal currentFree = 0;
                        if (e.Cell.Value != null)
                        {
                            decimal.TryParse(e.Cell.Value.ToString(), out currentFree);
                        }

                        DialogBox.frmCalculator freeCalcDialog = new DialogBox.frmCalculator(currentFree);
                        if (freeCalcDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Update the free quantity
                            decimal newFree = freeCalcDialog.CalculatedValue;
                            if (newFree < 0)
                                newFree = 0; // Don't allow negative free

                            e.Cell.Value = (float)newFree;

                            // Handle Free >= 1 logic: Set Qty = Free and Cost = 0
                            HandleFreeQuantityChange(e.Cell.Row, (float)newFree);

                            // Recalculate totals
                            this.CaluateTotals();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing cell double-click: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Setup row footer using gridFooterPanel
        private void SetupRowFooter()
        {
            try
            {
                // Get the parent control that contains ultraGrid1
                Control parent = ultraGrid1.Parent;
                if (parent == null) return;

                // Ensure the ultraGrid1 and gridFooterPanel are in the same parent container
                if (gridFooterPanel.Parent != parent)
                {
                    parent.Controls.Add(gridFooterPanel);
                }

                // Get the height from row header or use default height
                int footerHeight = 22; // Default height

                // Try to get the row header height from the grid
                // Add proper null checking to avoid NullReferenceException
                if (ultraGrid1 != null && ultraGrid1.DisplayLayout != null &&
                    ultraGrid1.DisplayLayout.Override != null)
                {
                    // Use the grid's default row height
                    footerHeight = ultraGrid1.DisplayLayout.Override.DefaultRowHeight;

                    // Make sure it's at least 22 pixels high for visibility
                    footerHeight = Math.Max(footerHeight, 22);
                }

                // Position gridFooterPanel immediately below ultraGrid1
                gridFooterPanel.Top = ultraGrid1.Bottom;
                gridFooterPanel.Left = ultraGrid1.Left;
                gridFooterPanel.Width = ultraGrid1.Width;
                gridFooterPanel.Height = footerHeight;

                // Set anchoring so it stays attached to the grid when resized
                gridFooterPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

                // Update ultraGrid1 anchoring to ensure it resizes properly
                ultraGrid1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

                // Adjust grid height to accommodate the footer panel
                ultraGrid1.Height -= gridFooterPanel.Height;

                // Make sure grid doesn't draw over the footer by setting scroll bounds
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;

                // Style the panel to match the grid header appearance (same as ultraPanelGridFooter in reports)
                gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
                gridFooterPanel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

                // Apply solid blue background — same as ultraPanelGridFooter in frmvendorpurchasereport
                gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
                gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
                gridFooterPanel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Clear any existing controls in the panel to prepare for footer cells
                gridFooterPanel.ClientArea.Controls.Clear();

                // Create footer cells for each visible column
                CreateFooterCells();

                // Add handlers to update the footer when grid changes
                ultraGrid1.AfterRowInsert += (s, e) => UpdateFooterValues();
                ultraGrid1.AfterRowsDeleted += (s, e) => UpdateFooterValues();
                ultraGrid1.AfterCellUpdate += (s, e) => UpdateFooterValues();

                // Also add a handler for grid InitializeLayout to handle column changes
                ultraGrid1.InitializeLayout += (s, e) =>
                {
                    // Recreate footer cells when needed - defer to avoid layout issues
                    this.BeginInvoke(new Action(() =>
                    {
                        UpdateFooterCellPositions();
                        UpdateFooterValues();
                    }));
                };

                // Set the panel to be visible
                gridFooterPanel.Visible = true;

                // Make sure footer is above other controls
                gridFooterPanel.BringToFront();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up row footer: " + ex.Message);
            }
        }

        // Create the footer cells for each visible column
        private void CreateFooterCells()
        {
            try
            {
                // Clear existing controls and dictionaries
                gridFooterPanel.ClientArea.Controls.Clear();
                footerLabels.Clear();

                if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                    return;

                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Get the row selector width for proper offset
                int rowSelectorWidth = ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                int xOffset = rowSelectorWidth;

                // For each visible column, create a footer cell
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Hidden)
                        continue;

                    // Create a label for this column's footer
                    Label lblFooter = new Label();
                    lblFooter.Name = "footer_" + col.Key;
                    lblFooter.Text = ""; // Empty by default
                    lblFooter.TextAlign = ContentAlignment.MiddleCenter; // Center text horizontally and vertically
                    lblFooter.BackColor = GridHeaderBlue; // Match column header theme color
                    lblFooter.BorderStyle = BorderStyle.None; // No borders initially
                    lblFooter.AutoSize = false;
                    lblFooter.Width = col.Width;
                    lblFooter.Height = gridFooterPanel.Height - 2; // Leave 1px margin top and bottom
                    lblFooter.Left = xOffset;
                    lblFooter.Top = 1; // 1px from top
                    lblFooter.Tag = col.Key; // Store column key for reference

                    // Use white text color for better contrast on blue background
                    lblFooter.ForeColor = Color.White;

                    // Add custom paint handler for curved box effect
                    lblFooter.Paint += FooterLabel_Paint;

                    // Add context menu for the footer cell
                    ContextMenuStrip menu = CreateFooterContextMenu(col.Key);
                    lblFooter.ContextMenuStrip = menu;

                    // Add to the panel and dictionary
                    gridFooterPanel.ClientArea.Controls.Add(lblFooter);
                    footerLabels[col.Key] = lblFooter;

                    // Move offset for next column
                    xOffset += col.Width;
                }

                // Initialize column aggregations dictionary if needed
                if (columnAggregations.Count == 0)
                {
                    // Initialize default aggregations - numeric columns use Sum, others use Count
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (IsNumericColumn(col))
                        {
                            columnAggregations[col.Key] = "None"; // Default to None
                        }
                        else
                        {
                            columnAggregations[col.Key] = "None"; // Default to None
                        }
                    }
                }

                // Update the footer values based on current aggregations
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error creating footer cells: " + ex.Message);
            }
        }

        // Create context menu for a footer cell
        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey; // Store the column key for reference

            // Add menu items - check if this column is numeric for Sum/Avg options
            bool isNumeric = IsNumericColumn(ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey]);

            // Create menu items
            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum");
            itemSum.Enabled = isNumeric; // Only enable for numeric columns
            itemSum.Tag = "Sum";
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

            ToolStripMenuItem itemAvg = new ToolStripMenuItem("Average");
            itemAvg.Enabled = isNumeric; // Only enable for numeric columns
            itemAvg.Tag = "Avg";
            itemAvg.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None");
            itemNone.Tag = "None";
            itemNone.Click += FooterContextMenu_Click;

            // Add the items to the menu
            menu.Items.Add(itemSum);
            menu.Items.Add(itemMin);
            menu.Items.Add(itemMax);
            menu.Items.Add(itemCount);
            menu.Items.Add(itemAvg);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemNone);

            // Add opening event to check the current aggregation
            menu.Opening += (s, e) =>
            {
                string currentAgg = "None";
                if (columnAggregations.ContainsKey(columnKey))
                    currentAgg = columnAggregations[columnKey];

                // Check the current aggregation
                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.Tag != null)
                    {
                        menuItem.Checked = (menuItem.Tag.ToString() == currentAgg);
                    }
                }
            };

            return menu;
        }

        // Handle context menu click
        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                if (item == null) return;

                // Get the column key from the parent menu
                ContextMenuStrip menu = item.Owner as ContextMenuStrip;
                if (menu == null || menu.Tag == null) return;

                string columnKey = menu.Tag.ToString();
                string aggregation = item.Tag.ToString();

                // Update the aggregation for this column
                columnAggregations[columnKey] = aggregation;

                // Clear the footer cell immediately if "None" is selected
                if (aggregation == "None" && footerLabels.ContainsKey(columnKey))
                {
                    // Clear the text
                    footerLabels[columnKey].Text = "";
                    footerLabels[columnKey].Tag = new Tuple<string, string>(columnKey, "");
                    footerLabels[columnKey].Invalidate(); // Force redraw
                }

                // Update footer values for other cells
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error handling footer menu click: " + ex.Message);
            }
        }

        // Check if a column is numeric
        private bool IsNumericColumn(UltraGridColumn column)
        {
            if (column == null) return false;

            // Check if the column is one of these numeric types
            return column.Key == "BaseCost" || column.Key == "Cost" || column.Key == "Qty" ||
                   column.Key == "Free" || column.Key == "RetailPrice" ||
                   column.Key == "Packing" || column.Key == "MarginPer" || column.Key == "MarginAmt" ||
                   column.Key == "TaxPer" || column.Key == "TaxAmt" || column.Key == "UnitPrize" ||
                   column.Key == "WholeSalePrice" || column.Key == "CreditPrice" || column.Key == "CardPrice" ||
                   column.DataType == typeof(int) || column.DataType == typeof(double) ||
                   column.DataType == typeof(float) || column.DataType == typeof(decimal) ||
                   column.DataType == typeof(long);
        }

        // Update the footer values based on the selected aggregations
        private void UpdateFooterValues()
        {
            try
            {
                // Make sure the gridFooterPanel exists and is visible
                if (gridFooterPanel == null || !gridFooterPanel.Visible)
                    return;

                // Check ultraGrid1 and DisplayLayout separately to avoid null reference exceptions
                if (ultraGrid1 == null || ultraGrid1.DisplayLayout == null)
                    return;

                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    // Clear all footer values if no data
                    foreach (string key in footerLabels.Keys)
                    {
                        if (footerLabels.ContainsKey(key))
                        {
                            footerLabels[key].Text = "";
                            footerLabels[key].Tag = new Tuple<string, string>(key, "");
                            footerLabels[key].ForeColor = Color.White;
                            footerLabels[key].Invalidate(); // Force redraw
                        }
                    }
                    return;
                }

                // For each column with a footer label
                foreach (string columnKey in footerLabels.Keys)
                {
                    if (!columnAggregations.ContainsKey(columnKey) ||
    columnAggregations[columnKey] == "None" ||
    !footerLabels.ContainsKey(columnKey))
                    {
                        // No aggregation or "None" selected - clear value
                        footerLabels[columnKey].Text = "";
                        footerLabels[columnKey].Tag = new Tuple<string, string>(columnKey, ""); // Clear stored text
                        footerLabels[columnKey].ForeColor = Color.White;
                        footerLabels[columnKey].Invalidate(); // Force redraw to clear box
                        continue;
                    }

                    string aggregation = columnAggregations[columnKey];
                    bool isNumeric = IsNumericColumn(ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey]);

                    // Skip inappropriate aggregations
                    if ((aggregation == "Sum" || aggregation == "Avg") && !isNumeric)
                    {
                        footerLabels[columnKey].Text = "";
                        footerLabels[columnKey].ForeColor = Color.White;
                        continue;
                    }

                    // Calculate the aggregation value
                    object result = null;

                    switch (aggregation)
                    {
                        case "Sum":
                            result = CalculateSum(dt, columnKey);
                            break;
                        case "Min":
                            result = CalculateMin(dt, columnKey);
                            break;
                        case "Max":
                            result = CalculateMax(dt, columnKey);
                            break;
                        case "Count":
                            result = dt.Rows.Count;
                            break;
                        case "Avg":
                            result = CalculateAverage(dt, columnKey);
                            break;
                    }

                    // Format and display the result
                    string displayValue = FormatAggregationResult(result, columnKey, aggregation);

                    // Store both the text and the original column key in the Tag property
                    footerLabels[columnKey].Tag = new Tuple<string, string>(columnKey, displayValue);
                    footerLabels[columnKey].Text = displayValue; // Set text (will be drawn by paint handler)
                    footerLabels[columnKey].ForeColor = Color.White; // Maintain white text color

                    // Force redraw to show the curved box
                    footerLabels[columnKey].Invalidate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating footer values: " + ex.Message);
            }
        }

        // Calculate sum for a column
        private object CalculateSum(DataTable dt, string columnKey)
        {
            try
            {
                double sum = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnKey] != DBNull.Value)
                    {
                        double value;
                        if (double.TryParse(row[columnKey].ToString(), out value))
                        {
                            sum += value;
                        }
                    }
                }
                return sum;
            }
            catch
            {
                return 0;
            }
        }

        // Calculate minimum for a column
        private object CalculateMin(DataTable dt, string columnKey)
        {
            try
            {
                bool hasValue = false;
                object minVal = null;

                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnKey] != DBNull.Value)
                    {
                        if (!hasValue)
                        {
                            minVal = row[columnKey];
                            hasValue = true;
                        }
                        else
                        {
                            // For numeric columns
                            if (row[columnKey] is IComparable)
                            {
                                if (((IComparable)row[columnKey]).CompareTo(minVal) < 0)
                                {
                                    minVal = row[columnKey];
                                }
                            }
                            // For string columns
                            else if (row[columnKey] is string && minVal is string)
                            {
                                if (string.Compare(row[columnKey].ToString(), minVal.ToString()) < 0)
                                {
                                    minVal = row[columnKey];
                                }
                            }
                        }
                    }
                }

                return hasValue ? minVal : null;
            }
            catch
            {
                return null;
            }
        }

        // Calculate maximum for a column
        private object CalculateMax(DataTable dt, string columnKey)
        {
            try
            {
                bool hasValue = false;
                object maxVal = null;

                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnKey] != DBNull.Value)
                    {
                        if (!hasValue)
                        {
                            maxVal = row[columnKey];
                            hasValue = true;
                        }
                        else
                        {
                            // For numeric columns
                            if (row[columnKey] is IComparable)
                            {
                                if (((IComparable)row[columnKey]).CompareTo(maxVal) > 0)
                                {
                                    maxVal = row[columnKey];
                                }
                            }
                            // For string columns
                            else if (row[columnKey] is string && maxVal is string)
                            {
                                if (string.Compare(row[columnKey].ToString(), maxVal.ToString()) > 0)
                                {
                                    maxVal = row[columnKey];
                                }
                            }
                        }
                    }
                }

                return hasValue ? maxVal : null;
            }
            catch
            {
                return null;
            }
        }

        // Calculate average for a column
        private object CalculateAverage(DataTable dt, string columnKey)
        {
            try
            {
                double sum = 0;
                int count = 0;

                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnKey] != DBNull.Value)
                    {
                        double value;
                        if (double.TryParse(row[columnKey].ToString(), out value))
                        {
                            sum += value;
                            count++;
                        }
                    }
                }

                return count > 0 ? sum / count : 0;
            }
            catch
            {
                return 0;
            }
        }

        // Format the aggregation result based on column type
        private string FormatAggregationResult(object result, string columnKey, string aggregation)
        {
            if (result == null)
                return "";

            try
            {
                UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey];

                // For Count aggregation
                if (aggregation == "Count")
                    return result.ToString();

                // For numeric columns (Sum, Min, Max, Avg)
                if (IsNumericColumn(column))
                {
                    double value;
                    if (double.TryParse(result.ToString(), out value))
                    {
                        // Use the column's format if available
                        if (!string.IsNullOrEmpty(column.Format))
                        {
                            return value.ToString(column.Format);
                        }
                        else
                        {
                            // Default format for numeric values
                            return value.ToString("N2");
                        }
                    }
                }

                // For other types, just return the string representation
                return result.ToString();
            }
            catch
            {
                return result.ToString();
            }
        }

        // Update the positions of footer cells when columns are resized
        private void UpdateFooterCellPositions()
        {
            try
            {
                // Check each object separately to avoid null reference exception
                if (ultraGrid1 == null || gridFooterPanel == null || !gridFooterPanel.Visible)
                    return;

                // Check DisplayLayout separately
                if (ultraGrid1.DisplayLayout == null)
                    return;

                // Check Bands separately
                if (ultraGrid1.DisplayLayout.Bands == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                    return;

                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Get the row selector width for proper offset
                int rowSelectorWidth = ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                int xOffset = rowSelectorWidth;

                // Update position and width of each footer label
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Hidden)
                        continue;

                    if (footerLabels.ContainsKey(col.Key))
                    {
                        Label lblFooter = footerLabels[col.Key];
                        lblFooter.Left = xOffset;
                        lblFooter.Width = col.Width;
                    }

                    xOffset += col.Width;
                }

                // Update panel position
                gridFooterPanel.Top = ultraGrid1.Bottom;
                gridFooterPanel.Width = ultraGrid1.Width;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating footer cell positions: " + ex.Message);
            }
        }

        // Handle grid column resize to update footer cells
        private void UltraGrid1_ColumnSized(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
        }

        // Paint handler for footer labels to create curved box effect
        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null)
                return;

            // Get the text from the label
            string displayText = lbl.Text;
            string columnKey = "";

            // If text is empty, check if we have text stored in the tag property
            if (lbl.Tag is Tuple<string, string>)
            {
                Tuple<string, string> tagData = (Tuple<string, string>)lbl.Tag;
                columnKey = tagData.Item1; // Get the column key
                displayText = tagData.Item2; // Get the display text from the tag
            }

            // If we still don't have text, don't draw anything
            if (string.IsNullOrEmpty(displayText))
                return;

            // Check if this column has aggregation set to "None"
            if (columnAggregations.ContainsKey(columnKey) && columnAggregations[columnKey] == "None")
                return; // Don't draw anything for "None" aggregation

            // Get the Graphics object and set high quality rendering
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Measure the text to center it properly
            SizeF textSize = g.MeasureString(displayText, lbl.Font);

            // Calculate the rounded rectangle size to fill most of the cell
            int padding = 6;
            int cornerRadius = 6; // Smaller corner radius to better align with cells
            int margin = 1; // Minimal margin to align with cell borders

            // Make the box fill the width of the cell with small margins
            int boxWidth = lbl.Width - (margin * 2);
            int boxHeight = (int)textSize.Height + padding;

            // Align the box with the cell borders
            int x = margin;
            int y = (lbl.Height - boxHeight) / 2;

            // Create rectangle and use the consistent header dark blue for the pill/box fill
            Rectangle rect = new Rectangle(x, y, boxWidth, boxHeight);
            Color boxColor = GridHeaderBlueDark; // Consistent with column header gradient dark tone

            // Draw the rounded rectangle
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                // Add arcs for the corners
                path.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);

                // Close the path
                path.CloseAllFigures();

                // Fill the rounded rectangle
                using (SolidBrush brush = new SolidBrush(boxColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw the text centered in the rounded rectangle
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                // Center the text in the box - ensure perfect centering
                float textX = x + (boxWidth - textSize.Width) / 2;
                float textY = y + (boxHeight - textSize.Height) / 2;

                // Add slight vertical adjustment for better visual centering
                textY -= 1;

                g.DrawString(displayText, lbl.Font, textBrush, textX, textY);
            }

            // Clear the original text to avoid drawing it twice
            lbl.Text = "";
        }

        private void FrmPurchase_KeyDown_ToggleSellingPrice(object sender, KeyEventArgs e)
        {
            // Handle F3 key to toggle SellingPrice and UnitSP column visibility
            if (e.KeyCode == Keys.F3)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                try
                {
                    if (ultraGrid1 != null && ultraGrid1.DisplayLayout != null && ultraGrid1.DisplayLayout.Bands.Count > 0)
                    {
                        UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                        UltraGridColumn sellingPriceColumn = band.Columns["SellingPrice"];
                        UltraGridColumn unitSPColumn = band.Columns["UnitSP"];

                        if (sellingPriceColumn != null)
                        {
                            // Toggle visibility
                            sellingPriceColumn.Hidden = !sellingPriceColumn.Hidden;
                        }

                        if (unitSPColumn != null)
                        {
                            // Toggle visibility (same as SellingPrice)
                            unitSPColumn.Hidden = !unitSPColumn.Hidden;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error toggling SellingPrice/UnitSP columns: " + ex.Message);
                }
            }
        }

        // ========== Column Chooser Functionality ==========

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

            // Set up direct header drag and drop
            SetupDirectHeaderDragDrop();
        }

        private void SetupDirectHeaderDragDrop()
        {
            // Enable the grid as a drop target
            ultraGrid1.AllowDrop = true;

            // Add drag event handlers for headers
            ultraGrid1.MouseDown += UltraGrid1_ColumnChooser_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_ColumnChooser_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_ColumnChooser_MouseUp;
            ultraGrid1.DragOver += UltraGrid1_ColumnChooser_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_ColumnChooser_DragDrop;

            // Create column chooser form but don't show it yet
            CreateColumnChooserForm();
        }

        private void CreateColumnChooserForm()
        {
            // Create a new form for the column chooser
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
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
                IntegralHeight = false
            };

            // Custom drawing for the ListBox items to make them look like blue buttons
            columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0) return;

                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;

                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);

                Color bgColor = Color.FromArgb(33, 150, 243);

                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
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

                        evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        evt.Graphics.FillPath(bgBrush, path);
                    }
                }

                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }

                if ((evt.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
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

            // Populate the listbox with currently hidden columns
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    this.Right - columnChooserForm.Width - 20,
                    this.Bottom - columnChooserForm.Height - 20);
                columnChooserForm.TopMost = true;
                columnChooserForm.BringToFront();
            }
        }

        private void UltraGrid1_ColumnChooser_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                isDraggingColumn = false;
                columnToMove = null;
                startPoint = new Point(e.X, e.Y);

                if (e.Y < 40) // Header area
                {
                    int xPos = 0;
                    if (ultraGrid1.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
                    {
                        xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                    }

                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden)
                        {
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

        private void UltraGrid1_ColumnChooser_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && columnToMove != null && isDraggingColumn)
                {
                    int deltaX = Math.Abs(e.X - startPoint.X);
                    int deltaY = Math.Abs(e.Y - startPoint.Y);

                    if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                    {
                        bool isDraggingDown = (e.Y > startPoint.Y && deltaY > deltaX);

                        if (isDraggingDown)
                        {
                            ultraGrid1.Cursor = Cursors.No;
                            string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ?
                                                columnToMove.Header.Caption : columnToMove.Key;
                            columnToolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");

                            if (e.Y - startPoint.Y > 50)
                            {
                                HideColumn(columnToMove);
                                columnToMove = null;
                                isDraggingColumn = false;
                                ultraGrid1.Cursor = Cursors.Default;
                                columnToolTip.SetToolTip(ultraGrid1, "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse move: " + ex.Message);
                ultraGrid1.Cursor = Cursors.Default;
                columnToolTip.SetToolTip(ultraGrid1, "");
            }
        }

        private void UltraGrid1_ColumnChooser_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                ultraGrid1.Cursor = Cursors.Default;
                columnToolTip.SetToolTip(ultraGrid1, "");
                isDraggingColumn = false;
                columnToMove = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse up: " + ex.Message);
            }
        }

        private void UltraGrid1_ColumnChooser_DragOver(object sender, DragEventArgs e)
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

        private void UltraGrid1_ColumnChooser_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                if (item != null)
                {
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                    {
                        UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];

                        ultraGrid1.SuspendLayout();
                        column.Hidden = false;

                        // Calculate drop position based on mouse coordinates
                        Point pointInGrid = ultraGrid1.PointToClient(new Point(e.X, e.Y));
                        Infragistics.Win.UIElement element = ultraGrid1.DisplayLayout.UIElement.ElementFromPoint(pointInGrid);
                        if (element != null)
                        {
                            Infragistics.Win.UltraWinGrid.HeaderUIElement headerElement = element.GetAncestor(typeof(Infragistics.Win.UltraWinGrid.HeaderUIElement)) as Infragistics.Win.UltraWinGrid.HeaderUIElement;
                            if (headerElement != null)
                            {
                                UltraGridColumn targetColumn = headerElement.GetContext(typeof(UltraGridColumn)) as UltraGridColumn;
                                if (targetColumn != null)
                                {
                                    bool dropAfter = pointInGrid.X > (headerElement.Rect.Left + headerElement.Rect.Width / 2);
                                    int newPos = targetColumn.Header.VisiblePosition;
                                    if (dropAfter) newPos++;

                                    // Ensure we don't exceed the column bounds
                                    int maxPos = ultraGrid1.DisplayLayout.Bands[0].Columns.Count - 1;
                                    if (newPos > maxPos) newPos = maxPos;

                                    column.Header.VisiblePosition = newPos;
                                }
                            }
                        }

                        // Restore saved width if available
                        if (savedColumnWidths.ContainsKey(item.ColumnKey))
                        {
                            column.Width = savedColumnWidths[item.ColumnKey];
                        }

                        columnChooserListBox.Items.Remove(item);
                        ultraGrid1.ResumeLayout();

                        columnToolTip.Show($"Column '{item.DisplayText}' restored",
                                ultraGrid1,
                                ultraGrid1.PointToClient(Control.MousePosition),
                                2000);
                    }
                }
            }
        }

        private void HideColumn(UltraGridColumn column)
        {
            try
            {
                if (column != null && !column.Hidden)
                {
                    string columnName = !string.IsNullOrEmpty(column.Header.Caption) ?
                                    column.Header.Caption : column.Key;

                    savedColumnWidths[column.Key] = column.Width;

                    ultraGrid1.SuspendLayout();
                    column.Hidden = true;

                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        {
                            col.Width = savedColumnWidths[col.Key];
                        }
                    }

                    ultraGrid1.ResumeLayout();

                    if (columnChooserForm == null || columnChooserForm.IsDisposed)
                    {
                        CreateColumnChooserForm();
                    }

                    if (columnChooserListBox != null)
                    {
                        // Define restricted standard columns for Purchase form
                        string[] standardColumns = new string[] {
                            "SLNO", "BarCode", "Description", "Unit", "Packing", "Free",
                            "SellingPrice", "UnitSP", "BaseCost", "Cost", "Qty", "Gross", "NetAmt",
                            "TaxPer", "TaxAmt", "TaxType", "RetailPrice"
                        };

                        if (standardColumns.Contains(column.Key))
                        {
                            bool alreadyExists = false;
                            foreach (object item in columnChooserListBox.Items)
                            {
                                if (item is ColumnItem columnItem && columnItem.ColumnKey == column.Key)
                                {
                                    alreadyExists = true;
                                    break;
                                }
                            }

                            if (!alreadyExists)
                            {
                                columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error hiding column: " + ex.Message);
            }
        }

        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                PopulateColumnChooserListBox();
                columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }

            CreateColumnChooserForm();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();

            this.LocationChanged += (s, evt) => PositionColumnChooserAtBottomRight();
            this.SizeChanged += (s, evt) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, evt) => PositionColumnChooserAtBottomRight();
        }

        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserListBox != null)
            {
                columnChooserListBox.MouseDown -= ColumnChooserListBox_MouseDown;
                columnChooserListBox.DragOver -= ColumnChooserListBox_DragOver;
                columnChooserListBox.DragDrop -= ColumnChooserListBox_DragDrop;
                columnChooserListBox = null;
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;

            columnChooserListBox.Items.Clear();

            // Define standard columns for Purchase form
            string[] standardColumns = new string[] {
                "SLNO", "BarCode", "Description", "Unit", "Packing", "Free",
                "SellingPrice", "UnitSP", "BaseCost", "Cost", "Qty", "Gross", "NetAmt",
                "TaxPer", "TaxAmt", "TaxType", "RetailPrice"
            };

            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "SLNO", "SLNO" },
                { "BarCode", "Barcode" },
                { "Description", "Item Name" },
                { "Unit", "Unit" },
                { "Packing", "Packing" },
                { "RetailPrice", "Retail Price" },
                { "Free", "Free" },
                { "SellingPrice", "Selling Price" },
                { "UnitSP", "Unit SP" },
                { "BaseCost", "Base Cost" },
                { "Cost", "Cost" },
                { "Qty", "Qty" },
                { "Gross", "Gross" },
                { "NetAmt", "Net Amount" },
                { "TaxPer", "Tax %" },
                { "TaxAmt", "Tax Amount" },
                { "TaxType", "Tax Type" }
            };

            HashSet<string> addedColumns = new HashSet<string>();

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

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                ColumnItem item = columnChooserListBox.Items[index] as ColumnItem;
                if (item != null)
                {
                    columnChooserListBox.SelectedIndex = index;
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(UltraGridColumn)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(UltraGridColumn)))
            {
                UltraGridColumn column = (UltraGridColumn)e.Data.GetData(typeof(UltraGridColumn));
                if (column != null && !column.Hidden)
                {
                    string columnName = !string.IsNullOrEmpty(column.Header.Caption) ?
                                    column.Header.Caption : column.Key;
                    column.Hidden = true;

                    // Define restricted standard columns for Purchase form
                    string[] standardColumns = new string[] {
                        "SLNO", "BarCode", "Description", "Unit", "Packing", "Free",
                        "SellingPrice", "UnitSP", "BaseCost", "Cost", "Qty", "Gross", "NetAmt",
                        "TaxPer", "TaxAmt", "TaxType", "RetailPrice"
                    };

                    if (standardColumns.Contains(column.Key))
                    {
                        columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                    }
                }
            }
        }

        private void InitializeSavedColumnWidths()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        savedColumnWidths[col.Key] = col.Width;
                    }
                }
            }
        }

        // Event handler for ultraPanel6 click - opens Item Master in tab with selected item
        private void UltraPanel6_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if there's a selected row in the grid
                if (ultraGrid1.ActiveRow == null)
                {
                    MessageBox.Show("Please select an item from the grid to open Item Master", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get ItemId from the selected row
                int itemId = 0;
                if (ultraGrid1.ActiveRow.Cells["ItemId"].Value != null)
                {
                    if (!int.TryParse(ultraGrid1.ActiveRow.Cells["ItemId"].Value.ToString(), out itemId))
                    {
                        MessageBox.Show("Invalid item selected. Cannot open Item Master.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (itemId <= 0)
                {
                    MessageBox.Show("No valid item ID found. Cannot open Item Master.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Find the parent Home form
                Form parentHome = FindParentHome();
                if (parentHome == null)
                {
                    MessageBox.Show("Cannot find parent Home form. Item Master will open as a separate window.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Fallback: Open as separate window
                    PosBranch_Win.Master.frmItemMasterNew itemMasterForm = new PosBranch_Win.Master.frmItemMasterNew();
                    itemMasterForm.WindowState = FormWindowState.Maximized;
                    itemMasterForm.StartPosition = FormStartPosition.CenterScreen;
                    itemMasterForm.Show();

                    // Load the item using reflection
                    MethodInfo loadMethod = itemMasterForm.GetType().GetMethod("LoadItemById",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (loadMethod != null)
                    {
                        loadMethod.Invoke(itemMasterForm, new object[] { itemId });
                    }
                    return;
                }

                // Get the tabControlMain from Home form using reflection
                FieldInfo tabControlField = parentHome.GetType().GetField("tabControlMain",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (tabControlField == null)
                {
                    MessageBox.Show("Cannot access tab control. Item Master will open as a separate window.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Fallback: Open as separate window
                    PosBranch_Win.Master.frmItemMasterNew itemMasterForm = new PosBranch_Win.Master.frmItemMasterNew();
                    itemMasterForm.WindowState = FormWindowState.Maximized;
                    itemMasterForm.StartPosition = FormStartPosition.CenterScreen;
                    itemMasterForm.Show();

                    // Load the item using reflection
                    MethodInfo loadMethod = itemMasterForm.GetType().GetMethod("LoadItemById",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (loadMethod != null)
                    {
                        loadMethod.Invoke(itemMasterForm, new object[] { itemId });
                    }
                    return;
                }

                var tabControl = tabControlField.GetValue(parentHome) as Infragistics.Win.UltraWinTabControl.UltraTabControl;
                if (tabControl == null)
                {
                    MessageBox.Show("Tab control not found. Item Master will open as a separate window.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Check if Item Master tab already exists
                string tabName = "Item Master";
                Infragistics.Win.UltraWinTabControl.UltraTab existingTab = null;
                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControl.Tabs)
                {
                    if (tab.Text == tabName && tab.TabPage.Controls.Count > 0 &&
                        tab.TabPage.Controls[0] is PosBranch_Win.Master.frmItemMasterNew existingForm)
                    {
                        existingTab = tab;
                        break;
                    }
                }

                PosBranch_Win.Master.frmItemMasterNew itemMasterForm2;

                if (existingTab != null)
                {
                    // Use existing form
                    itemMasterForm2 = existingTab.TabPage.Controls[0] as PosBranch_Win.Master.frmItemMasterNew;
                    tabControl.SelectedTab = existingTab;
                }
                else
                {
                    // Create new Item Master form
                    itemMasterForm2 = new PosBranch_Win.Master.frmItemMasterNew();

                    // Set form properties for embedding in tab
                    itemMasterForm2.TopLevel = false;
                    itemMasterForm2.FormBorderStyle = FormBorderStyle.None;
                    itemMasterForm2.Dock = DockStyle.Fill;
                    itemMasterForm2.Visible = true;
                    itemMasterForm2.WindowState = FormWindowState.Normal;

                    // Create new tab
                    string uniqueKey = $"Tab_{DateTime.Now.Ticks}_{tabName}";
                    var newTab = tabControl.Tabs.Add(uniqueKey, tabName);

                    // Set form size to match tab page
                    itemMasterForm2.Size = newTab.TabPage.Size;
                    itemMasterForm2.Location = new Point(0, 0);

                    // Add the form to the tab page
                    newTab.TabPage.Controls.Add(itemMasterForm2);

                    // Show the form
                    itemMasterForm2.Show();
                    itemMasterForm2.BringToFront();

                    // Set the new tab as active/selected
                    tabControl.SelectedTab = newTab;

                    // Wire up the form's FormClosed event to remove the tab
                    itemMasterForm2.FormClosed += (formSender, formE) =>
                    {
                        try
                        {
                            if (newTab != null && tabControl.Tabs.Contains(newTab))
                            {
                                tabControl.Tabs.Remove(newTab);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error removing tab: {ex.Message}");
                        }
                    };
                }

                // Load the item using reflection (LoadItemById is private)
                MethodInfo loadMethod2 = itemMasterForm2.GetType().GetMethod("LoadItemById",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (loadMethod2 != null)
                {
                    loadMethod2.Invoke(itemMasterForm2, new object[] { itemId });

                    // LoadItemById already calls SyncUomGridWithPriceGrid internally,
                    // but we need to ensure the UI is updated after loading
                    itemMasterForm2.Refresh();
                    Application.DoEvents();
                }
                else
                {
                    // Fallback: Set the item ID
                    itemMasterForm2.SetCurrentItemId(itemId);
                    MessageBox.Show("Item Master opened. Please use the Load button to load item ID: " + itemId,
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Item Master: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to find parent Home form
        private Form FindParentHome()
        {
            // Try to find the Home form by traversing up the parent chain
            Control current = this.Parent;
            while (current != null)
            {
                if (current is Form form && form.GetType().Name == "Home")
                {
                    return form;
                }
                current = current.Parent;
            }

            // If not found in parent chain, search through open forms
            foreach (Form form in Application.OpenForms)
            {
                if (form.GetType().Name == "Home" && !form.IsDisposed)
                {
                    return form;
                }
            }

            return null;
        }



        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void lblPurchaseNo_Click(object sender, EventArgs e)
        {

        }





        // Handler for vendor save event - refreshes vendor dropdown when a new vendor is created
        private void RefreshVendorDropdown()
        {
            try
            {
                // Store currently selected vendor if any
                object selectedVendorId = CmboVendor.Value;

                // Reload vendor dropdown
                VendorDDLGrids VendorDDLGrids = drop.VendorDDL();
                CmboVendor.DataSource = VendorDDLGrids.List;
                CmboVendor.DisplayMember = "LedgerName";
                CmboVendor.ValueMember = "LedgerID";

                // Restore selection if vendor still exists
                if (selectedVendorId != null)
                {
                    CmboVendor.Value = selectedVendorId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing vendor dropdown: {ex.Message}");
            }
        }

        // Handler for frmItemMasterNew item update event - refreshes grid when items are updated
        private void OnItemMasterUpdatedHandler(int updatedItemId)
        {
            try
            {
                // Refresh grid data for the updated item if it exists in the grid
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["ItemId"] != null && row["ItemId"] != DBNull.Value)
                        {
                            int itemId = Convert.ToInt32(row["ItemId"]);
                            if (itemId == updatedItemId)
                            {
                                // Refresh item data from database
                                try
                                {
                                    DataBase.Operations = "BARCODEPURCHASE";
                                    string barcode = row["BarCode"] != null ? row["BarCode"].ToString() : "";
                                    if (!string.IsNullOrEmpty(barcode))
                                    {
                                        ItemDDlGrid itemDataResult = drop.itemDDlGrid(barcode, null);
                                        if (itemDataResult != null && itemDataResult.List != null && itemDataResult.List.Any())
                                        {
                                            var itm = itemDataResult.List.FirstOrDefault();
                                            if (itm != null)
                                            {
                                                // Update row with latest data
                                                int unitId = row["UnitId"] != null && row["UnitId"] != DBNull.Value
                                                    ? Convert.ToInt32(row["UnitId"])
                                                    : 0;

                                                // Update SellingPrice from PriceSettings
                                                float wholeSalePriceFromPriceSettings = GetWholeSalePriceFromPriceSettings(itemId, unitId);
                                                if (wholeSalePriceFromPriceSettings > 0)
                                                {
                                                    row["SellingPrice"] = wholeSalePriceFromPriceSettings;
                                                }

                                                // Update UnitSP from PriceSettings (Unit 1)
                                                float unit1WholeSalePrice = GetWholeSalePriceFromPriceSettings(itemId, 1);
                                                if (unit1WholeSalePrice > 0)
                                                {
                                                    row["UnitSP"] = unit1WholeSalePrice;
                                                }

                                                // Update tax-related fields from item data
                                                // Only update TaxPer and TaxType here; TaxAmt will be
                                                // recalculated below by RecalculateBaseCostAndTaxFromCost
                                                // based on the purchase row's own Cost and Qty.
                                                row["TaxPer"] = (float)itm.TaxPer;
                                                if (!string.IsNullOrEmpty(itm.TaxType))
                                                {
                                                    row["TaxType"] = NormalizeTaxType(itm.TaxType);
                                                }

                                                // Find the corresponding UltraGridRow for recalculation
                                                UltraGridRow gridRow = null;
                                                int rowIndex = row.Table.Rows.IndexOf(row);
                                                if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
                                                {
                                                    gridRow = ultraGrid1.Rows[rowIndex];
                                                }

                                                // Recalculate tax and amounts based on updated tax info
                                                if (gridRow != null)
                                                {
                                                    float qty = row["Qty"] != null && row["Qty"] != DBNull.Value
                                                        ? Convert.ToSingle(row["Qty"])
                                                        : 0;
                                                    float cost = row["Cost"] != null && row["Cost"] != DBNull.Value
                                                        ? Convert.ToSingle(row["Cost"])
                                                        : 0;

                                                    if (qty > 0 && cost > 0)
                                                    {
                                                        // Use RecalculateBaseCostAndTaxFromCost (not RecalculateTaxForRow)
                                                        // because when TaxPer changes, BaseCost itself needs
                                                        // recalculation (especially for "incl" tax type).
                                                        RecalculateBaseCostAndTaxFromCost(gridRow, cost, qty);
                                                    }
                                                }

                                                // NOTE: ultraGrid1.Refresh() intentionally removed.
                                                // Calling Refresh() here was stealing Windows focus from frmItemMasterNew
                                                // (e.g. while the user was typing in txt_Retail), causing the caret to reset.
                                                // The DataTable mutations above already propagate via data binding.
                                                // CaluateTotals() will be called when the event fires on Leave/Enter (not on every keystroke).
                                                UpdateNetAmtColumnVisibility();
                                                CaluateTotals();
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error refreshing item {updatedItemId} in grid: {ex.Message}");
                                }

                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnItemMasterUpdatedHandler: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveGridLayout();

            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }
            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle "q" or "Q" key press in ultraGrid1 to jump to next item's Qty cell
            if ((keyData & Keys.KeyCode) == Keys.Q && (keyData & Keys.Modifiers) == Keys.None)
            {
                bool isGridFocused = ultraGrid1 != null && (ultraGrid1.ContainsFocus ||
                    (this.ActiveControl != null && (this.ActiveControl == ultraGrid1 || this.ActiveControl.Parent == ultraGrid1)));

                if (isGridFocused && ultraGrid1.Rows.Count > 0)
                {
                    try
                    {
                        ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);

                        int currentIndex = ultraGrid1.ActiveRow != null ? ultraGrid1.ActiveRow.Index : -1;
                        int nextIndex = currentIndex + 1;
                        if (nextIndex >= ultraGrid1.Rows.Count)
                        {
                            nextIndex = 0;
                        }

                        UltraGridRow targetRow = ultraGrid1.Rows[nextIndex];
                        ultraGrid1.ActiveRow = targetRow;
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(targetRow);

                        if (targetRow.Cells.Exists("Qty"))
                        {
                            ultraGrid1.ActiveCell = targetRow.Cells["Qty"];
                            ultraGrid1.Focus();
                            ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.EnterEditMode);
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error jumping to next item Qty cell: {ex.Message}");
                    }
                }
            }

            // Handle Down arrow on txtBarcode to move focus to ultraGrid1
            if (keyData == Keys.Down && this.ActiveControl == txtBarcode)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    if (ultraGrid1.ActiveRow == null)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    }
                    return true;
                }
            }

            // Handle Up/Down arrow navigation within ultraGrid1
            if ((keyData == Keys.Up || keyData == Keys.Down) &&
                this.ActiveControl != null &&
                (this.ActiveControl == ultraGrid1 || this.ActiveControl.Parent == ultraGrid1))
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Let the grid handle its own up/down navigation natively
                    // by NOT intercepting and returning true, we allow standard grid behavior
                    // However, if we need custom behavior across bands/rows we would do it here
                    try
                    {
                        if (keyData == Keys.Up)
                        {
                            ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.AboveRow);
                            return true;
                        }
                        else if (keyData == Keys.Down)
                        {
                            ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.BelowRow);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error navigating grid rows: {ex.Message}");
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ─────────────────────────────────────────────────────────────────
        // Batch Qty Popup
        // ─────────────────────────────────────────────────────────────────
        private void ShowBatchQtyPopup()
        {
            // Count rows with data in ultraGrid1
            int rowCount = 0;
            if (ultraGrid1.Rows != null)
                foreach (var r in ultraGrid1.Rows)
                    if (r.Cells.Exists("Qty")) rowCount++;

            if (rowCount == 0)
            {
                MessageBox.Show("No items in the grid to update.", "Batch Qty",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ── Build the popup form ─────────────────────────────────────
            using (var popup = new System.Windows.Forms.Form())
            {
                popup.Text = "";
                popup.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                popup.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                popup.Size = new System.Drawing.Size(320, 200);
                popup.BackColor = System.Drawing.Color.FromArgb(0, 174, 219);   // sky-blue base
                popup.Opacity = 0.97;

                // ── Gradient background panel ────────────────────────────
                var bgPanel = new System.Windows.Forms.Panel
                {
                    Dock = System.Windows.Forms.DockStyle.Fill,
                    Padding = new System.Windows.Forms.Padding(2)
                };
                bgPanel.Paint += (s, e) =>
                {
                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        bgPanel.ClientRectangle,
                        System.Drawing.Color.FromArgb(180, 240, 255),
                        System.Drawing.Color.FromArgb(0, 140, 190),
                        System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, bgPanel.ClientRectangle);
                    }
                    // glossy top highlight
                    var topRect = new System.Drawing.Rectangle(0, 0, bgPanel.Width, bgPanel.Height / 2);
                    using (var gloss = new System.Drawing.Drawing2D.LinearGradientBrush(
                        topRect,
                        System.Drawing.Color.FromArgb(120, System.Drawing.Color.White),
                        System.Drawing.Color.FromArgb(0, System.Drawing.Color.White),
                        System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(gloss, topRect);
                    }
                    // border
                    using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 100, 160), 2))
                    {
                        e.Graphics.DrawRectangle(pen,
                            new System.Drawing.Rectangle(1, 1, bgPanel.Width - 2, bgPanel.Height - 2));
                    }
                };
                popup.Controls.Add(bgPanel);

                // ── Title bar ────────────────────────────────────────────
                var titleLbl = new System.Windows.Forms.Label
                {
                    Text = $"  ⚡  Set Batch Qty  ({rowCount} items)",
                    Font = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(0, 40, 80),
                    AutoSize = false,
                    Size = new System.Drawing.Size(280, 32),
                    Location = new System.Drawing.Point(10, 12),
                    BackColor = System.Drawing.Color.Transparent
                };
                bgPanel.Controls.Add(titleLbl);

                // Close [X] button
                var btnClose = new System.Windows.Forms.Button
                {
                    Text = "✕",
                    FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                    Size = new System.Drawing.Size(28, 28),
                    Location = new System.Drawing.Point(284, 8),
                    BackColor = System.Drawing.Color.Transparent,
                    ForeColor = System.Drawing.Color.FromArgb(0, 40, 80),
                    Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
                    Cursor = System.Windows.Forms.Cursors.Hand
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, e) => { popup.DialogResult = System.Windows.Forms.DialogResult.Cancel; popup.Close(); };
                bgPanel.Controls.Add(btnClose);

                // Divider line
                var divider = new System.Windows.Forms.Label
                {
                    AutoSize = false,
                    Size = new System.Drawing.Size(296, 1),
                    Location = new System.Drawing.Point(12, 46),
                    BackColor = System.Drawing.Color.FromArgb(0, 100, 160)
                };
                bgPanel.Controls.Add(divider);

                // ── Qty input label ──────────────────────────────────────
                var qtyLbl = new System.Windows.Forms.Label
                {
                    Text = "Enter Quantity for ALL Items:",
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    ForeColor = System.Drawing.Color.FromArgb(0, 30, 70),
                    AutoSize = true,
                    Location = new System.Drawing.Point(20, 62),
                    BackColor = System.Drawing.Color.Transparent
                };
                bgPanel.Controls.Add(qtyLbl);

                // ── Qty input textbox ────────────────────────────────────
                var txtQty = new System.Windows.Forms.TextBox
                {
                    Font = new System.Drawing.Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(0, 40, 90),
                    BackColor = System.Drawing.Color.FromArgb(220, 245, 255),
                    BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                    TextAlign = System.Windows.Forms.HorizontalAlignment.Center,
                    Text = "1",
                    Size = new System.Drawing.Size(200, 48),
                    Location = new System.Drawing.Point(60, 86)
                };
                txtQty.KeyPress += (s, e) =>
                {
                    // allow digits, one dot, backspace
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                        e.Handled = true;
                    if (e.KeyChar == '.' && ((System.Windows.Forms.TextBox)s).Text.Contains('.'))
                        e.Handled = true;
                    // Apply on Enter
                    if (e.KeyChar == (char)Keys.Return)
                    {
                        popup.DialogResult = System.Windows.Forms.DialogResult.OK;
                        popup.Close();
                    }
                };
                bgPanel.Controls.Add(txtQty);

                // ── Apply button ─────────────────────────────────────────
                var btnApply = new System.Windows.Forms.Button
                {
                    Text = "✔  Apply to All",
                    Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.White,
                    BackColor = System.Drawing.Color.FromArgb(0, 90, 150),
                    FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                    Size = new System.Drawing.Size(140, 34),
                    Location = new System.Drawing.Point(90, 148),
                    Cursor = System.Windows.Forms.Cursors.Hand
                };
                btnApply.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 60, 120);
                btnApply.FlatAppearance.BorderSize = 1;
                // Hover effect
                btnApply.MouseEnter += (s, e) => btnApply.BackColor = System.Drawing.Color.FromArgb(0, 120, 190);
                btnApply.MouseLeave += (s, e) => btnApply.BackColor = System.Drawing.Color.FromArgb(0, 90, 150);
                btnApply.Click += (s, e) => { popup.DialogResult = System.Windows.Forms.DialogResult.OK; popup.Close(); };
                bgPanel.Controls.Add(btnApply);

                // Allow dragging the borderless popup
                bool dragging = false;
                System.Drawing.Point dragStart = System.Drawing.Point.Empty;
                bgPanel.MouseDown += (s, e) => { if (e.Button == System.Windows.Forms.MouseButtons.Left) { dragging = true; dragStart = e.Location; } };
                bgPanel.MouseMove += (s, e) => { if (dragging) { var p = popup.PointToScreen(e.Location); popup.Location = new System.Drawing.Point(p.X - dragStart.X, p.Y - dragStart.Y); } };
                bgPanel.MouseUp   += (s, e) => dragging = false;
                titleLbl.MouseDown += (s, e) => { if (e.Button == System.Windows.Forms.MouseButtons.Left) { dragging = true; dragStart = e.Location; } };
                titleLbl.MouseMove += (s, e) => { if (dragging) { var p = popup.PointToScreen(e.Location); popup.Location = new System.Drawing.Point(p.X - dragStart.X, p.Y - dragStart.Y); } };
                titleLbl.MouseUp   += (s, e) => dragging = false;

                // Focus the qty box on open
                popup.Shown += (s, e) => { txtQty.SelectAll(); txtQty.Focus(); };

                // ── Show and process result ──────────────────────────────
                if (popup.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (!float.TryParse(txtQty.Text.Trim(), out float batchQty) || batchQty <= 0)
                    {
                        MessageBox.Show("Please enter a valid positive quantity.", "Batch Qty",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // ── Apply qty to ALL rows and recalculate ────────────
                    try
                    {
                        ultraGrid1.BeginUpdate();
                        foreach (var row in ultraGrid1.Rows)
                        {
                            if (!row.Cells.Exists("Qty")) continue;
                            var qtyCell = row.Cells["Qty"];
                            if (qtyCell.Column.CellActivation == Infragistics.Win.UltraWinGrid.Activation.NoEdit) continue;
                            if (qtyCell.Column.Hidden) continue;

                            qtyCell.Value = batchQty;

                            // Recalculate tax / totals per row if Cost exists
                            if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null)
                            {
                                if (float.TryParse(row.Cells["Cost"].Value.ToString(), out float cost))
                                {
                                    RecalculateTaxForRow(row, cost, batchQty);
                                }
                            }
                        }
                    }
                    finally
                    {
                        ultraGrid1.EndUpdate();
                    }

                    RecalculateNewBaseCostForAllRows();
                    CaluateTotals();
                }
            }
        }
    };
}
