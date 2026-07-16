using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Repository.TransactionRepository;
using ModelClass;
using ModelClass.TransactionModels;

namespace PosBranch_Win.DialogBox
{
    public partial class frmSoldItemHistory : Form
    {
        private readonly int _itemId;
        private readonly string _itemName;
        private readonly bool _enableReturnMode; // Flag to enable double-click for returns
        private readonly SalesRepository salesRepository = new SalesRepository();
        private DataTable fullDataTable = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;

        // Column Chooser fields
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

        public frmSoldItemHistory()
        {
            InitializeComponent();
        }

        public frmSoldItemHistory(int itemId, string itemName, bool enableReturnMode = false)
        {
            InitializeComponent();
            _itemId = itemId;
            _itemName = itemName;
            _enableReturnMode = enableReturnMode;

            this.KeyPreview = true;
            SetupUltraGridStyle();
            SetupColumnChooserMenu();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();

            // Wire up event handlers
            this.ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            this.ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            this.ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;
            this.textBoxsearch.TextChanged += textBoxsearch_TextChanged;
            this.textBoxsearch.KeyDown += textBoxsearch_KeyDown;
            this.KeyDown += frmSoldItemHistory_KeyDown;

            // Add placeholder text handlers
            this.textBoxsearch.GotFocus += textBoxsearch_GotFocus;
            this.textBoxsearch.LostFocus += textBoxsearch_LostFocus;
            this.textBoxsearch.Text = "Search sales history...";
            this.textBoxsearch.ForeColor = Color.Gray;

            // Add event wiring for resize/activation
            this.SizeChanged += FrmSoldItemHistory_SizeChanged;
            this.LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, e) => PositionColumnChooserAtBottomRight();
            this.FormClosing += FrmSoldItemHistory_FormClosing;
            this.ultraGrid1.Resize += UltraGrid1_Resize;
        }

        private void frmSoldItemHistory_Load(object sender, EventArgs e)
        {
            LoadSoldItemHistory();
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
        }

        private void LoadSoldItemHistory()
        {
            try
            {
                label1.Text = $"Sold Item History for: {_itemName}";

                int branchId = SessionContext.BranchId;
                int companyId = SessionContext.CompanyId;
                int finYearId = SessionContext.FinYearId;

                List<SoldItemHistory> history = salesRepository.GetSoldItemHistory(_itemId, branchId, companyId, finYearId);

                if (history != null && history.Count > 0)
                {
                    fullDataTable = ToDataTable(history);
                    PreserveOriginalRowOrder(fullDataTable);
                    ultraGrid1.DataSource = fullDataTable;

                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                    }

                    InitializeSavedColumnWidths();
                    UpdateRecordCountLabel();
                }
                else
                {
                    MessageBox.Show("No sales history found for this item.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sold item history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable ToDataTable(List<SoldItemHistory> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(SoldItemHistory));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (SoldItemHistory item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        private void PreserveOriginalRowOrder(DataTable table)
        {
            if (!table.Columns.Contains("OriginalRowOrder"))
            {
                DataColumn orderColumn = new DataColumn("OriginalRowOrder", typeof(int));
                table.Columns.Add(orderColumn);
                int rowIndex = 0;
                foreach (DataRow row in table.Rows)
                {
                    row["OriginalRowOrder"] = rowIndex++;
                }
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                if (e.Layout.Bands.Count == 0) return;

                var band = e.Layout.Bands[0];

                // Hide OriginalRowOrder column
                if (band.Columns.Exists("OriginalRowOrder"))
                {
                    band.Columns["OriginalRowOrder"].Hidden = true;
                }

                // Set column headers and widths
                if (band.Columns.Exists("BillNo"))
                {
                    band.Columns["BillNo"].Header.Caption = "Bill No";
                    band.Columns["BillNo"].Width = 80;
                }

                if (band.Columns.Exists("BillDate"))
                {
                    band.Columns["BillDate"].Header.Caption = "Bill Date";
                    band.Columns["BillDate"].Width = 100;
                    band.Columns["BillDate"].Format = "dd/MM/yyyy";
                }

                if (band.Columns.Exists("CustomerName"))
                {
                    band.Columns["CustomerName"].Header.Caption = "Customer";
                    band.Columns["CustomerName"].Width = 150;
                }

                if (band.Columns.Exists("PaymodeName"))
                {
                    band.Columns["PaymodeName"].Header.Caption = "Payment Mode";
                    band.Columns["PaymodeName"].Width = 100;
                }

                if (band.Columns.Exists("Qty"))
                {
                    band.Columns["Qty"].Header.Caption = "Quantity";
                    band.Columns["Qty"].Width = 70;
                }

                if (band.Columns.Exists("UnitPrice"))
                {
                    band.Columns["UnitPrice"].Header.Caption = "Unit Price";
                    band.Columns["UnitPrice"].Width = 90;
                    band.Columns["UnitPrice"].Format = "N2";
                }

                if (band.Columns.Exists("SubTotal"))
                {
                    band.Columns["SubTotal"].Header.Caption = "Sub Total";
                    band.Columns["SubTotal"].Width = 90;
                    band.Columns["SubTotal"].Format = "N2";
                }

                if (band.Columns.Exists("TaxAmt"))
                {
                    band.Columns["TaxAmt"].Header.Caption = "Tax Amount";
                    band.Columns["TaxAmt"].Width = 90;
                    band.Columns["TaxAmt"].Format = "N2";
                }

                if (band.Columns.Exists("NetAmount"))
                {
                    band.Columns["NetAmount"].Header.Caption = "Net Amount";
                    band.Columns["NetAmount"].Width = 100;
                    band.Columns["NetAmount"].Format = "N2";
                }

                if (band.Columns.Exists("ReceivedAmount"))
                {
                    band.Columns["ReceivedAmount"].Header.Caption = "Received";
                    band.Columns["ReceivedAmount"].Width = 90;
                    band.Columns["ReceivedAmount"].Format = "N2";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing grid layout: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGrid1.DisplayLayout.Reset();
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Border styles
                ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;

                // Border alpha and spacing
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;

                // Colors
                Color lightBlue = Color.FromArgb(173, 216, 230);
                Color headerBlue = Color.FromArgb(0, 123, 255);

                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Row heights
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;

                // Header styling
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Row selector styling
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;

                // Remove images
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.Image = null;

                // Row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Selected row appearance
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;

                // Active row appearance
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Font settings
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Scroll settings
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;

                // Scrollbar styling
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Bill No");
            comboBox1.Items.Add("Customer");
            comboBox1.Items.Add("Payment Mode");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterSalesHistory();
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Bill Date");
            comboBox2.Items.Add("Bill No");
            comboBox2.Items.Add("Customer");
            comboBox2.Items.Add("Net Amount");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SortData();
        }

        private void SortData()
        {
            try
            {
                if (fullDataTable == null) return;

                string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "Bill Date";
                string sortColumn = "";

                switch (selectedColumn)
                {
                    case "Bill Date":
                        sortColumn = "BillDate";
                        break;
                    case "Bill No":
                        sortColumn = "BillNo";
                        break;
                    case "Customer":
                        sortColumn = "CustomerName";
                        break;
                    case "Net Amount":
                        sortColumn = "NetAmount";
                        break;
                }

                if (!string.IsNullOrEmpty(sortColumn) && fullDataTable.Columns.Contains(sortColumn))
                {
                    fullDataTable.DefaultView.Sort = $"{sortColumn} DESC";
                }

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sorting data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxsearch_TextChanged(object sender, EventArgs e)
        {
            FilterSalesHistory();
        }

        private void FilterSalesHistory()
        {
            try
            {
                if (fullDataTable == null) return;

                string searchText = textBoxsearch.Text.Trim();
                if (searchText == "Search sales history...")
                {
                    searchText = "";
                }

                DataView dv = fullDataTable.DefaultView;

                if (!string.IsNullOrEmpty(searchText))
                {
                    string selectedField = comboBox1.SelectedItem?.ToString() ?? "All";
                    string filter = "";
                    string escapedSearchText = searchText.Replace("'", "''");

                    switch (selectedField)
                    {
                        case "Bill No":
                            filter = $"CONVERT(BillNo, 'System.String') LIKE '%{escapedSearchText}%'";
                            break;
                        case "Customer":
                            filter = $"CustomerName LIKE '%{escapedSearchText}%'";
                            break;
                        case "Payment Mode":
                            filter = $"PaymodeName LIKE '%{escapedSearchText}%'";
                            break;
                        case "All":
                        default:
                            filter = $"CONVERT(BillNo, 'System.String') LIKE '%{escapedSearchText}%' OR CustomerName LIKE '%{escapedSearchText}%' OR PaymodeName LIKE '%{escapedSearchText}%'";
                            break;
                    }

                    dv.RowFilter = filter;
                }
                else
                {
                    dv.RowFilter = string.Empty;
                }

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering sales history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == "Search sales history...")
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = Color.Black;
            }
        }

        private void textBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = "Search sales history...";
                textBoxsearch.ForeColor = Color.Gray;
            }
        }

        private void textBoxsearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            // If return mode is enabled, add item to sales invoice with negative quantity
            if (_enableReturnMode && e.Row != null)
            {
                try
                {
                    // Extract item data from the selected row
                    int itemId = 0;
                    string itemName = "";
                    string barcode = "";
                    string unit = "";
                    int unitId = 0;
                    decimal unitPrice = 0;
                    int qty = 0;
                    double taxPer = 0;
                    string taxType = "incl";
                    double cost = 0;
                    double mrp = 0;
                    double marginPer = 0;
                    double discountPer = 0;
                    double packing = 1;

                    // Get values from the row
                    if (e.Row.Cells["ItemId"].Value != null && e.Row.Cells["ItemId"].Value != DBNull.Value)
                        itemId = Convert.ToInt32(e.Row.Cells["ItemId"].Value);

                    if (e.Row.Cells["ItemName"].Value != null && e.Row.Cells["ItemName"].Value != DBNull.Value)
                        itemName = e.Row.Cells["ItemName"].Value.ToString();

                    if (e.Row.Cells["Barcode"].Value != null && e.Row.Cells["Barcode"].Value != DBNull.Value)
                        barcode = e.Row.Cells["Barcode"].Value.ToString();

                    if (e.Row.Cells["Unit"].Value != null && e.Row.Cells["Unit"].Value != DBNull.Value)
                        unit = e.Row.Cells["Unit"].Value.ToString();

                    if (e.Row.Cells["UnitId"].Value != null && e.Row.Cells["UnitId"].Value != DBNull.Value)
                        unitId = Convert.ToInt32(e.Row.Cells["UnitId"].Value);

                    if (e.Row.Cells["UnitPrice"].Value != null && e.Row.Cells["UnitPrice"].Value != DBNull.Value)
                        unitPrice = Convert.ToDecimal(e.Row.Cells["UnitPrice"].Value);

                    if (e.Row.Cells["Qty"].Value != null && e.Row.Cells["Qty"].Value != DBNull.Value)
                        qty = Convert.ToInt32(e.Row.Cells["Qty"].Value);

                    if (e.Row.Cells["TaxPer"].Value != null && e.Row.Cells["TaxPer"].Value != DBNull.Value)
                        taxPer = Convert.ToDouble(e.Row.Cells["TaxPer"].Value);

                    if (e.Row.Cells["TaxType"].Value != null && e.Row.Cells["TaxType"].Value != DBNull.Value)
                        taxType = e.Row.Cells["TaxType"].Value.ToString();

                    if (e.Row.Cells["Cost"].Value != null && e.Row.Cells["Cost"].Value != DBNull.Value)
                        cost = Convert.ToDouble(e.Row.Cells["Cost"].Value);

                    if (e.Row.Cells["MRP"].Value != null && e.Row.Cells["MRP"].Value != DBNull.Value)
                        mrp = Convert.ToDouble(e.Row.Cells["MRP"].Value);

                    if (e.Row.Cells["MarginPer"].Value != null && e.Row.Cells["MarginPer"].Value != DBNull.Value)
                        marginPer = Convert.ToDouble(e.Row.Cells["MarginPer"].Value);

                    if (e.Row.Cells["DiscountPer"].Value != null && e.Row.Cells["DiscountPer"].Value != DBNull.Value)
                        discountPer = Convert.ToDouble(e.Row.Cells["DiscountPer"].Value);

                    if (e.Row.Cells["Packing"].Value != null && e.Row.Cells["Packing"].Value != DBNull.Value)
                        packing = Convert.ToDouble(e.Row.Cells["Packing"].Value);

                    // Validate required data
                    if (itemId > 0 && !string.IsNullOrEmpty(itemName))
                    {
                        // Find the open sales invoice form
                        var salesInvoiceForm = Application.OpenForms.OfType<PosBranch_Win.Transaction.frmSalesInvoice>().FirstOrDefault();
                        
                        if (salesInvoiceForm != null)
                        {
                            // Add the item to the sales invoice grid with NEGATIVE quantity for return
                            decimal amount = -qty * unitPrice; // Negative amount for return
                            
                            salesInvoiceForm.AddItemToGrid(
                                itemId.ToString(),
                                itemName,
                                barcode ?? "",
                                unit,
                                unitPrice,
                                -qty, // NEGATIVE quantity for return
                                amount
                            );

                            // Close this dialog
                            this.Close();
                            
                            // Bring sales invoice form to front
                            salesInvoiceForm.BringToFront();
                            salesInvoiceForm.Focus();
                            
                            MessageBox.Show($"Item '{itemName}' added to invoice as return (negative quantity).\nQuantity: {-qty}", 
                                "Return Item Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Sales Invoice form is not open. Please open a sales invoice first.",
                                "Form Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to retrieve complete item information. Please ensure the item has valid data.",
                            "Incomplete Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding return item to sales invoice: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void frmSoldItemHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (ultraGrid1.Focused && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    int currentIndex = ultraGrid1.ActiveRow.Index;

                    if (e.KeyCode == Keys.Up && currentIndex > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex - 1];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                    else if (e.KeyCode == Keys.Down && currentIndex < ultraGrid1.Rows.Count - 1)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex + 1];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                }
                else if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                e.Handled = true;
            }
        }

        private void SetupPanelHoverEffects()
        {
            // Add hover effects for all interactive panels

        }

        private void SetupPanelGroupHoverEffects(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);

            Action applyHoverEffect = () =>
            {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            Action removeHoverEffect = () =>
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };

            panel.MouseEnter += (s, e) => { applyHoverEffect(); };
            panel.MouseLeave += (s, e) => { removeHoverEffect(); };

            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => { applyHoverEffect(); pictureBox.Cursor = Cursors.Hand; };
                pictureBox.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }

            if (label != null)
            {
                label.MouseEnter += (s, e) => { applyHoverEffect(); label.Cursor = Cursors.Hand; };
                label.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
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
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        private void ConnectNavigationPanelEvents()
        {
            // Up/Down navigation panels
            ultraPanel3.Click += MoveRowUp;
            ultraPanel3.ClientArea.Click += MoveRowUp;
            ultraPictureBox5.Click += MoveRowUp;

            ultraPanel7.Click += MoveRowDown;
            ultraPanel7.ClientArea.Click += MoveRowDown;
            ultraPictureBox6.Click += MoveRowDown;

            // OK Button
            ultraPanel5.Click += UltraPanel5_Click;
            ultraPanel5.ClientArea.Click += UltraPanel5_Click;
            label5.Click += UltraPanel5_Click;
            ultraPictureBox7.Click += UltraPanel5_Click;

            // Close Button
            ultraPanel6.Click += UltraPanel6_Click;
            ultraPanel6.ClientArea.Click += UltraPanel6_Click;
            label3.Click += UltraPanel6_Click;
            ultraPictureBox2.Click += UltraPanel6_Click;

            // Sort toggle button
            ultraPictureBox4.Click += UltraPictureBox4_Click;
            ultraPictureBox1.Click += UltraPictureBox4_Click;
        }

        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows.Count == 0) return;

            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                return;
            }

            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex > 0)
            {
                var rowToActivate = ultraGrid1.Rows[currentIndex - 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }

            UpdateRecordCountLabel();
        }

        private void MoveRowDown(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows.Count == 0) return;

            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                return;
            }

            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex < ultraGrid1.Rows.Count - 1)
            {
                var rowToActivate = ultraGrid1.Rows[currentIndex + 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }

            UpdateRecordCountLabel();
        }

        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            if (fullDataTable == null) return;

            isOriginalOrder = !isOriginalOrder;

            DataView dv = fullDataTable.DefaultView;
            if (isOriginalOrder)
            {
                dv.Sort = "OriginalRowOrder ASC";
            }
            else
            {
                dv.Sort = "OriginalRowOrder DESC";
            }

            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
            }
        }

        private void UltraPanel5_Click(object sender, EventArgs e)
        {
            // OK button - can be used to close or perform action
            this.Close();
        }

        private void UltraPanel6_Click(object sender, EventArgs e)
        {
            // Close button
            this.Close();
        }

        private void InitializeSavedColumnWidths()
        {
            savedColumnWidths.Clear();
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    savedColumnWidths[col.Key] = col.Width;
                }
            }
        }

        // --- Column Chooser Feature ---
        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGrid1.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }

        private void SetupDirectHeaderDragDrop()
        {
            ultraGrid1.AllowDrop = true;
            ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            ultraGrid1.DragOver += UltraGrid1_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop;
            CreateColumnChooserForm();
        }

        private void CreateColumnChooserForm()
        {
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
            columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();

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

            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserListBox.ScrollAlwaysVisible = false;
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(this.Right - columnChooserForm.Width - 20, this.Bottom - columnChooserForm.Height - 20);
                columnChooserForm.TopMost = true;
                columnChooserForm.BringToFront();
            }
        }

        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40)
            {
                int xPos = 0;
                if (ultraGrid1.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                {
                    xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                }
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
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

        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
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
                        string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ? columnToMove.Header.Caption : columnToMove.Key;
                        toolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");
                        if (e.Y - startPoint.Y > 50)
                        {
                            HideColumn(columnToMove);
                            columnToMove = null;
                            isDraggingColumn = false;
                            ultraGrid1.Cursor = Cursors.Default;
                            toolTip.SetToolTip(ultraGrid1, "");
                        }
                    }
                }
            }
        }

        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            ultraGrid1.Cursor = Cursors.Default;
            toolTip.SetToolTip(ultraGrid1, "");
            isDraggingColumn = false;
            columnToMove = null;
        }

        private void HideColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
                string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                savedColumnWidths[column.Key] = column.Width;
                ultraGrid1.SuspendLayout();
                column.Hidden = true;
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
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

        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
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

            string[] standardColumns = new string[] { "BillNo", "BillDate", "CustomerName", "PaymodeName", "Qty", "UnitPrice", "SubTotal", "TaxAmt", "NetAmount", "ReceivedAmount" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "BillNo", "Bill No" },
                { "BillDate", "Bill Date" },
                { "CustomerName", "Customer" },
                { "PaymodeName", "Payment Mode" },
                { "Qty", "Quantity" },
                { "UnitPrice", "Unit Price" },
                { "SubTotal", "Sub Total" },
                { "TaxAmt", "Tax Amount" },
                { "NetAmount", "Net Amount" },
                { "ReceivedAmount", "Received" }
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

        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }
            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }
            public override string ToString() { return DisplayText; }
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
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
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
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
            {
                var column = (Infragistics.Win.UltraWinGrid.UltraGridColumn)e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn));
                if (column != null && !column.Hidden)
                {
                    string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                    column.Hidden = true;
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                }
            }
        }

        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
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

        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                if (item != null)
                {
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                    {
                        var column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                        ultraGrid1.SuspendLayout();
                        column.Hidden = false;
                        columnChooserListBox.Items.Remove(item);
                        ultraGrid1.ResumeLayout();
                        toolTip.Show($"Column '{item.DisplayText}' restored",
                                ultraGrid1,
                                ultraGrid1.PointToClient(Control.MousePosition),
                                2000);
                    }
                }
            }
        }

        private void FrmSoldItemHistory_SizeChanged(object sender, EventArgs e)
        {
            PreserveColumnWidths();
            PositionColumnChooserAtBottomRight();
        }

        private void UltraGrid1_Resize(object sender, EventArgs e)
        {
            PreserveColumnWidths();
        }

        private void FrmSoldItemHistory_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }

        private void PreserveColumnWidths()
        {
            try
            {
                ultraGrid1.SuspendLayout();
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGrid1.ResumeLayout();
            }
            catch
            {
                // Silently handle errors
            }
        }

        private void UpdateRecordCountLabel()
        {
            int currentDisplayCount = ultraGrid1.Rows?.Count ?? 0;
            int totalCount = fullDataTable?.Rows.Count ?? 0;
            label1.Text = $"Sold Item History for: {_itemName} (Showing {currentDisplayCount} of {totalCount} records)";

            if (textBox3 != null)
            {
                textBox3.Text = currentDisplayCount.ToString();
            }
        }

        private void OpenSalesReturnInTab(Form form, string tabName)
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
    }
}
