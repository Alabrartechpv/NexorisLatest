using ModelClass;
using ModelClass.Master;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Utilities
{
    public partial class FrmExcelImport : Form
    {
        private List<string[]> _parsedFileData = new List<string[]>();
        private string[] _fileHeaders = new string[0];
        private ExcelImportRepository _importRepo;
        private List<ExcelImportRepository.ImportRow> _validationRows = new List<ExcelImportRepository.ImportRow>();
        private List<ExcelImportRepository.ImportRow> _failedRows = new List<ExcelImportRepository.ImportRow>();

        // Programmatic Filter Controls
        private UltraLabel lblFilterStatus;
        private UltraComboEditor cmbFilterStatus;

        // Dynamic Mapping Controls list
        private Dictionary<string, UltraComboEditor> _mappingDropdowns = new Dictionary<string, UltraComboEditor>();

        public FrmExcelImport()
        {
            InitializeComponent();
            try
            {
                _importRepo = new ExcelImportRepository();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Repository initialization error at constructor time: {ex.Message}");
            }
            InitializeCustomLayout();
        }

        private void InitializeCustomLayout()
        {
            // Infragistics controls are styled in the designer via Appearance properties.
            // Set defaults for combo items here.

            // Duplicate behavior combo items
            cmbDuplicateBehavior.Items.Add("Skip");
            cmbDuplicateBehavior.Items.Add("Merge");
            cmbDuplicateBehavior.SelectedIndex = 0;

            // Programmatic Filter controls initialization
            lblFilterStatus = new UltraLabel();
            lblFilterStatus.Text = "Filter:";
            lblFilterStatus.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblFilterStatus.Location = new Point(245, 12);
            lblFilterStatus.Size = new Size(45, 20);
            lblFilterStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblFilterStatus.Visible = false;

            cmbFilterStatus = new UltraComboEditor();
            cmbFilterStatus.DropDownStyle = DropDownStyle.DropDownList;
            cmbFilterStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            cmbFilterStatus.Location = new Point(290, 8);
            cmbFilterStatus.Size = new Size(140, 25);
            cmbFilterStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbFilterStatus.Items.Add("Show All");
            cmbFilterStatus.Items.Add("Errors Only");
            cmbFilterStatus.Items.Add("Warnings Only");
            cmbFilterStatus.Items.Add("Valid Rows Only");
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.Visible = false;

            cmbFilterStatus.ValueChanged += CmbFilterStatus_ValueChanged;
            ultraGridPreview.AfterCellUpdate += ultraGridPreview_AfterCellUpdate;

            pnlPreviewBanner.ClientArea.Controls.Add(lblFilterStatus);
            pnlPreviewBanner.ClientArea.Controls.Add(cmbFilterStatus);
        }

        private void FrmExcelImport_Load(object sender, EventArgs e)
        {
            LoadFilters();
            ResetMappingPanel();
        }

        private void LoadFilters()
        {
            try
            {
                _importRepo.LoadDBCaches();

                // Category filter
                cmbExportCategory.Items.Clear();
                cmbExportCategory.Items.Add(new ComboBoxItem("All Categories", 0));
                cmbExportCategory.Items.Add(new ComboBoxItem("[No Category]", -1));
                foreach (var kvp in _importRepo.CategoryCache.OrderBy(k => k.Key))
                    cmbExportCategory.Items.Add(new ComboBoxItem(kvp.Key, kvp.Value));
                cmbExportCategory.SelectedIndex = 0;

                // Brand filter
                cmbExportBrand.Items.Clear();
                cmbExportBrand.Items.Add(new ComboBoxItem("All Brands", 0));
                cmbExportBrand.Items.Add(new ComboBoxItem("[No Brand]", -1));
                foreach (var kvp in _importRepo.BrandCache.OrderBy(k => k.Key))
                    cmbExportBrand.Items.Add(new ComboBoxItem(kvp.Key, kvp.Value));
                cmbExportBrand.SelectedIndex = 0;

                // Group filter
                cmbExportGroup.Items.Clear();
                cmbExportGroup.Items.Add(new ComboBoxItem("All Groups", 0));
                cmbExportGroup.Items.Add(new ComboBoxItem("[No Group]", -1));
                foreach (var kvp in _importRepo.GroupCache.OrderBy(k => k.Key))
                    cmbExportGroup.Items.Add(new ComboBoxItem(kvp.Key, kvp.Value));
                cmbExportGroup.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading filters: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetMappingPanel()
        {
            pnlMappingGrid.ClientArea.Controls.Clear();
            _mappingDropdowns.Clear();
            lblStats.Text = "No file loaded. Please select a CSV file to begin.";
            btnPreview.Enabled = false;
            btnImport.Enabled = false;
            btnDownloadErrorLog.Visible = false;
            if (cmbFilterStatus != null)
            {
                cmbFilterStatus.Visible = false;
                lblFilterStatus.Visible = false;
            }
            ultraGridPreview.DataSource = null;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx, *.xls)|*.xlsx;*.xls|All files (*.*)|*.*";
                ofd.Title = "Select Excel or CSV Inventory File";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    ResetMappingPanel();
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string path = txtFilePath.Text.Trim();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show("Please select a valid CSV or Excel file first.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lblProgress.Text = "Reading file...";
                progressBarImport.Value = 0;

                // For Excel (.xlsx/.xls) files, let's inform the user how to convert to CSV or parse it.
                // We will implement direct CSV parsing, which is extremely robust and does not require complex OLEDB installation on the client.
                string ext = Path.GetExtension(path).ToLower();
                if (ext == ".xlsx" || ext == ".xls")
                {
                    MessageBox.Show(
                        "Excel (.xlsx/.xls) files will be parsed using OLEDB. " +
                        "If you encounter compatibility errors, please save your spreadsheet as a 'CSV (Comma Delimited) (*.csv)' file and select it instead.",
                        "Excel Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Try to parse using OLEDB
                    LoadExcelViaOleDb(path);
                }
                else
                {
                    // Parse standard CSV
                    _parsedFileData = ExcelImportRepository.CSVHelper.ReadCSV(path);
                    if (_parsedFileData.Count > 0)
                    {
                        _fileHeaders = _parsedFileData[0];
                        _parsedFileData.RemoveAt(0); // Remove header row
                        BuildMappingControls();
                    }
                    else
                    {
                        throw new Exception("The CSV file is empty.");
                    }
                }

                lblProgress.Text = $"Loaded {_parsedFileData.Count} rows from sheet.";
            }
            catch (Exception ex)
            {
                lblProgress.Text = "Load failed.";
                MessageBox.Show($"Failed to load file: {ex.Message}\n\n" +
                                "If this is an Excel file, we highly recommend saving it as a CSV (Comma Delimited) file and trying again.", 
                                "Load Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadExcelViaOleDb(string filePath)
        {
            string connString = string.Empty;
            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".xls")
            {
                connString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={filePath};Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
            }
            else if (ext == ".xlsx")
            {
                connString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=\"Excel 12.0 Xml;HDR=Yes;IMEX=1\"";
            }

            using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(connString))
            {
                conn.Open();
                DataTable dtSchema = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                if (dtSchema == null || dtSchema.Rows.Count == 0)
                    throw new Exception("Could not find any sheets in the Excel file.");

                string sheetName = dtSchema.Rows[0]["TABLE_NAME"].ToString();
                
                using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand($"SELECT * FROM [{sheetName}]", conn))
                {
                    using (System.Data.OleDb.OleDbDataAdapter da = new System.Data.OleDb.OleDbDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        _fileHeaders = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
                        _parsedFileData.Clear();
                        
                        foreach (DataRow row in dt.Rows)
                        {
                            string[] fields = row.ItemArray.Select(x => Convert.ToString(x)).ToArray();
                            _parsedFileData.Add(fields);
                        }

                        BuildMappingControls();
                    }
                }
            }
        }

        private void BuildMappingControls()
        {
            ResetMappingPanel();

            // Target fields we expect
            string[] targetFields = new string[]
            {
                "Barcode", "Description", "ItemType", "Category", "Brand", "Group", "Unit", "Packing",
                "IsBaseUnit", "Cost", "RetailPrice", "WholeSalePrice", "MRP", "CardPrice", "CreditPrice",
                "StaffPrice", "MinPrice", "OpeningStock", "ReorderLevel", "TaxType", "TaxPer", "HSNCode",
                "AlternativeBarcodes", "OrderCycleDays", "BoxQty", "Perishable"
            };

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.ColumnCount = 2;
            tlp.RowCount = targetFields.Length;
            tlp.AutoScroll = true;
            tlp.Dock = DockStyle.Fill;
            tlp.Padding = new Padding(10);
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(SizeType.Percent, 45F));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(SizeType.Percent, 55F));

            // Modern row height with nice spacing
            for (int i = 0; i < targetFields.Length; i++)
            {
                tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            }

            for (int i = 0; i < targetFields.Length; i++)
            {
                string target = targetFields[i];
                
                // Infragistics UltraLabel for dynamic layout mapping labels
                UltraLabel lbl = new UltraLabel();
                lbl.Text = target + (IsFieldRequired(target) ? " *" : "");
                lbl.Font = new Font("Segoe UI", 9F, IsFieldRequired(target) ? FontStyle.Bold : FontStyle.Regular);
                lbl.Appearance.TextVAlign = VAlign.Middle;
                if (IsFieldRequired(target))
                {
                    lbl.Appearance.ForeColor = Color.FromArgb(220, 38, 38); // Slate rose red
                }
                else
                {
                    lbl.Appearance.ForeColor = Color.FromArgb(71, 85, 105); // Slate dark gray
                }
                lbl.Dock = DockStyle.Fill;
                tlp.Controls.Add(lbl, 0, i);

                // Modernized flat UltraComboEditor controls
                UltraComboEditor cmb = new UltraComboEditor();
                cmb.DropDownStyle = DropDownStyle.DropDownList;
                cmb.Dock = DockStyle.Fill;
                cmb.UseOsThemes = DefaultableBoolean.False;
                cmb.ButtonStyle = UIElementButtonStyle.Flat;
                cmb.Appearance.BorderColor = Color.FromArgb(203, 213, 225); // Slate 300 border
                cmb.Appearance.FontData.Name = "Segoe UI";
                cmb.Appearance.FontData.SizeInPoints = 9F;
                cmb.Items.Add("[Select Source Column]");
                
                foreach (var header in _fileHeaders)
                    cmb.Items.Add(header);

                cmb.SelectedIndex = 0;
                
                // Store in dictionary
                _mappingDropdowns[target] = cmb;
                tlp.Controls.Add(cmb, 1, i);
            }

            pnlMappingGrid.ClientArea.Controls.Add(tlp);
            btnAutoMap_Click(this, EventArgs.Empty); // Auto map columns

            btnPreview.Enabled = true;
            lblStats.Text = $"File loaded. Please verify the column mappings and click 'Validate & Preview'.";
        }

        private bool IsFieldRequired(string target)
        {
            return target == "Barcode" || target == "Description" || target == "Cost" || target == "RetailPrice";
        }

        private void btnAutoMap_Click(object sender, EventArgs e)
        {
            foreach (var kvp in _mappingDropdowns)
            {
                string target = kvp.Key.ToLower();
                UltraComboEditor cmb = kvp.Value;

                // Try to find matching column header
                for (int i = 1; i < cmb.Items.Count; i++)
                {
                    string option = cmb.Items[i].ToString().ToLower().Replace(" ", "").Replace("_", "");
                    if (option == target ||
                        (target == "alternativebarcodes" && (option == "alternativebarcode" || option == "alternatebarcode" || option == "alternatebarcodes" || option == "altbarcodes" || option == "altbarcode")) ||
                        (target == "ordercycledays" && (option == "ordercycledays" || option == "ordercycle" || option == "reordercycle" || option == "cycle")) ||
                        (target == "boxqty" && (option == "boxqty" || option == "boxquantity" || option == "boxpack")) ||
                        (target == "perishable" && (option == "perishable" || option == "isperishable")) ||
                        (target == "description" && (option == "itemname" || option == "name" || option == "desc" || option == "productname")) ||
                        (target == "retailprice" && (option == "sellingprice" || option == "price" || option == "retail" || option == "walkinprice")) ||
                        (target == "cost" && (option == "costprice" || option == "purchaseprice" || option == "unitcost")) ||
                        (target == "openingstock" && (option == "stock" || option == "qty" || option == "quantity" || option == "opnstk")) ||
                        (target == "reorderlevel" && (option == "reorder" || option == "reorderqty")) ||
                        (target == "taxper" && (option == "tax%" || option == "taxpercentage" || option == "taxrate" || option == "gst" || option == "vat")))
                    {
                        cmb.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            // Validate that required fields are mapped
            if (_mappingDropdowns["Barcode"].SelectedIndex == 0 ||
                _mappingDropdowns["Description"].SelectedIndex == 0 ||
                _mappingDropdowns["Cost"].SelectedIndex == 0 ||
                _mappingDropdowns["RetailPrice"].SelectedIndex == 0)
            {
                MessageBox.Show("Please map all required columns (*):\n- Barcode\n- Description/Name\n- Cost\n- RetailPrice", 
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _validationRows.Clear();
            _failedRows.Clear();

            bool altBarcodesMapped = _mappingDropdowns.TryGetValue("AlternativeBarcodes", out UltraComboEditor altCmb) && altCmb.SelectedIndex > 0;
            bool orderCycleDaysMapped = _mappingDropdowns.TryGetValue("OrderCycleDays", out UltraComboEditor ocdCmb) && ocdCmb.SelectedIndex > 0;
            bool boxQtyMapped = _mappingDropdowns.TryGetValue("BoxQty", out UltraComboEditor bqCmb) && bqCmb.SelectedIndex > 0;
            bool perishableMapped = _mappingDropdowns.TryGetValue("Perishable", out UltraComboEditor pCmb) && pCmb.SelectedIndex > 0;

            // Populate rows from mapping
            for (int i = 0; i < _parsedFileData.Count; i++)
            {
                string[] fileRow = _parsedFileData[i];
                var row = new ExcelImportRepository.ImportRow();

                row.RowIndex = i + 1;
                row.Barcode = ExcelImportRepository.CleanImportedBarcode(GetMappedValue("Barcode", fileRow));
                row.Description = GetMappedValue("Description", fileRow);
                row.ItemType = GetMappedValue("ItemType", fileRow);
                row.Category = GetMappedValue("Category", fileRow);
                row.Brand = GetMappedValue("Brand", fileRow);
                row.Group = GetMappedValue("Group", fileRow);
                row.Unit = GetMappedValue("Unit", fileRow);
                row.Packing = ParseDoubleValue(GetMappedValue("Packing", fileRow), 1.0);
                row.IsBaseUnit = GetMappedValue("IsBaseUnit", fileRow);
                if (string.IsNullOrWhiteSpace(row.IsBaseUnit))
                {
                    row.IsBaseUnit = (row.Packing == 1.0) ? "Y" : "N";
                }

                row.Cost = ParseDoubleValue(GetMappedValue("Cost", fileRow), 0.0);
                row.RetailPrice = ParseDoubleValue(GetMappedValue("RetailPrice", fileRow), 0.0);
                row.WholeSalePrice = ParseDoubleValue(GetMappedValue("WholeSalePrice", fileRow), row.RetailPrice);
                row.MRP = ParseDoubleValue(GetMappedValue("MRP", fileRow), row.RetailPrice);
                row.CardPrice = ParseDoubleValue(GetMappedValue("CardPrice", fileRow), row.RetailPrice);
                row.CreditPrice = ParseDoubleValue(GetMappedValue("CreditPrice", fileRow), row.RetailPrice);
                row.StaffPrice = ParseDoubleValue(GetMappedValue("StaffPrice", fileRow), row.RetailPrice);
                row.MinPrice = ParseDoubleValue(GetMappedValue("MinPrice", fileRow), row.RetailPrice);

                row.OpnStk = ParseDoubleValue(GetMappedValue("OpeningStock", fileRow), 0.0);
                row.ReOrder = ParseDoubleValue(GetMappedValue("ReorderLevel", fileRow), 0.0);
                row.TaxType = GetMappedValue("TaxType", fileRow);
                row.TaxPer = ParseDoubleValue(GetMappedValue("TaxPer", fileRow), 0.0);
                row.HSNCode = ExcelImportRepository.CleanImportedBarcode(GetMappedValue("HSNCode", fileRow));
                row.AlternativeBarcodes = altBarcodesMapped ? ExcelImportRepository.CleanAlternativeBarcodes(GetMappedValue("AlternativeBarcodes", fileRow)) : null;
                row.OrderCycleDays = orderCycleDaysMapped ? ParseIntValue(GetMappedValue("OrderCycleDays", fileRow), 7) : -99;
                row.BoxQty = boxQtyMapped ? ParseDoubleValue(GetMappedValue("BoxQty", fileRow), 1.0) : -99.0;
                row.Perishable = perishableMapped ? GetMappedValue("Perishable", fileRow) : null;

                _validationRows.Add(row);
            }

            string duplicateBehavior = cmbDuplicateBehavior.SelectedItem?.ToString() ?? "Skip";
            bool autoCreate = chkAutoCreate.Checked;
            bool autoGenerateBarcodes = chkAutoGenerateBarcodes.Checked;

            // Run validation
            btnPreview.Enabled = false;
            btnImport.Enabled = false;
            btnBrowse.Enabled = false;
            btnLoad.Enabled = false;

            lblProgress.Text = "Validating data...";
            progressBarImport.Value = 0;
            bgWorkerValidate.RunWorkerAsync(new object[] { duplicateBehavior, autoCreate, autoGenerateBarcodes });
        }

        private string GetMappedValue(string targetField, string[] fileRow)
        {
            if (_mappingDropdowns.TryGetValue(targetField, out UltraComboEditor cmb) && cmb.SelectedIndex > 0)
            {
                int fileColIndex = cmb.SelectedIndex - 1;
                if (fileColIndex < fileRow.Length)
                {
                    return fileRow[fileColIndex];
                }
            }
            return string.Empty;
        }

        private double ParseDoubleValue(string val, double defaultVal)
        {
            if (string.IsNullOrWhiteSpace(val)) return defaultVal;
            double result;
            return double.TryParse(val.Replace("$", "").Replace(",", "").Trim(), out result) ? result : defaultVal;
        }

        private int ParseIntValue(string val, int defaultVal)
        {
            if (string.IsNullOrWhiteSpace(val)) return defaultVal;
            int result;
            return int.TryParse(val.Replace(",", "").Trim(), out result) ? result : defaultVal;
        }

        // Background workers for non-blocking UI
        private void bgWorkerValidate_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            string duplicateBehavior = (string)args[0];
            bool autoCreate = (bool)args[1];
            bool autoGenerateBarcodes = (bool)args[2];

            // Set up caches in memory
            _importRepo.LoadDBCaches();

            long generatedBarcodeSeed = 200000000001;
            HashSet<string> sessionAddedBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Validate each row in memory
            foreach (var row in _validationRows)
            {
                // Barcode validation
                if (string.IsNullOrWhiteSpace(row.Barcode))
                {
                    if (autoGenerateBarcodes)
                    {
                        string genBarcode = generatedBarcodeSeed.ToString();
                        while (_importRepo.ExistingBarcodes.Contains(genBarcode) || sessionAddedBarcodes.Contains(genBarcode))
                        {
                            generatedBarcodeSeed++;
                            genBarcode = generatedBarcodeSeed.ToString();
                        }
                        row.Barcode = genBarcode;
                        sessionAddedBarcodes.Add(genBarcode);
                        row.HasWarning = true;
                        row.StatusMessage += $"[Barcode will be auto-generated: {genBarcode}] ";
                    }
                    else
                    {
                        row.HasError = true;
                        row.StatusMessage += "Barcode is missing. ";
                    }
                }
                else
                {
                    // Check duplicate barcode
                    bool isDuplicate = _importRepo.ExistingBarcodes.Contains(row.Barcode) || sessionAddedBarcodes.Contains(row.Barcode);
                    if (isDuplicate)
                    {
                        if (duplicateBehavior == "Skip")
                        {
                            row.HasWarning = true;
                            row.StatusMessage += "Duplicate barcode: row will be skipped. ";
                        }
                        else
                        {
                            row.HasWarning = true;
                            row.StatusMessage += "Duplicate barcode: will merge & update prices. ";
                        }
                    }
                    sessionAddedBarcodes.Add(row.Barcode);
                }

                if (string.IsNullOrWhiteSpace(row.Description))
                {
                    row.HasError = true;
                    row.StatusMessage += "Item Name / Description is missing. ";
                }

                if (row.Cost < 0)
                {
                    row.HasError = true;
                    row.StatusMessage += "Cost Price cannot be negative. ";
                }
                if (row.RetailPrice < 0)
                {
                    row.HasError = true;
                    row.StatusMessage += "Retail Price cannot be negative. ";
                }

                // Master validation
                if (!autoCreate)
                {
                    if (!string.IsNullOrWhiteSpace(row.Category) && !_importRepo.CategoryCache.ContainsKey(row.Category.Trim()))
                    {
                        row.HasError = true;
                        row.StatusMessage += $"Category '{row.Category}' does not exist. ";
                    }
                    if (!string.IsNullOrWhiteSpace(row.Brand) && !_importRepo.BrandCache.ContainsKey(row.Brand.Trim()))
                    {
                        row.HasError = true;
                        row.StatusMessage += $"Brand '{row.Brand}' does not exist. ";
                    }
                    if (!string.IsNullOrWhiteSpace(row.Unit) && !_importRepo.UnitCache.ContainsKey(row.Unit.Trim()))
                    {
                        row.HasError = true;
                        row.StatusMessage += $"Unit '{row.Unit}' does not exist. ";
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(row.Category) && !_importRepo.CategoryCache.ContainsKey(row.Category.Trim()))
                    {
                        row.HasWarning = true;
                        row.StatusMessage += $"[Will create Category: {row.Category}] ";
                    }
                    if (!string.IsNullOrWhiteSpace(row.Brand) && !_importRepo.BrandCache.ContainsKey(row.Brand.Trim()))
                    {
                        row.HasWarning = true;
                        row.StatusMessage += $"[Will create Brand: {row.Brand}] ";
                    }
                    if (!string.IsNullOrWhiteSpace(row.Unit) && !_importRepo.UnitCache.ContainsKey(row.Unit.Trim()))
                    {
                        row.HasWarning = true;
                        row.StatusMessage += $"[Will create Unit: {row.Unit}] ";
                    }
                }

                if (!row.HasError && !row.HasWarning)
                {
                    row.StatusMessage = "Valid";
                }
            }
        }

        private void bgWorkerValidate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (cmbFilterStatus != null)
            {
                cmbFilterStatus.SelectedIndex = 0; // Reset to "Show All"
                cmbFilterStatus.Visible = true;
                lblFilterStatus.Visible = true;
            }

            ApplyGridFilter();

            int total = _validationRows.Count;
            int errors = _validationRows.Count(r => r.HasError);
            int warnings = _validationRows.Count(r => r.HasWarning && !r.HasError);
            int valid = total - errors - warnings;

            lblStats.Text = $"Total Rows: {total}  |  Valid: {valid}  |  Warnings: {warnings}  |  Errors: {errors}";
            lblProgress.Text = "Validation completed.";

            if (errors > 0)
            {
                MessageBox.Show($"Validation completed with {errors} errors. Rows highlighted in red have errors that prevent import.", 
                                "Validation Summary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            btnPreview.Enabled = true;
            btnBrowse.Enabled = true;
            btnLoad.Enabled = true;

            // Enable import if we have at least one row without error
            btnImport.Enabled = (total - errors) > 0;
        }

        // UltraGrid InitializeLayout — configure grid appearance
        private void ultraGridPreview_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridLayout layout = e.Layout;
            layout.Override.AllowUpdate = DefaultableBoolean.True;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.None;
            layout.Override.CellPadding = 4;
            layout.Override.HeaderStyle = HeaderStyle.Standard;

            // Flat headers with Navy blue background
            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(31, 58, 86);
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI Semibold";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5F;

            // Row appearance
            layout.Override.RowAppearance.FontData.Name = "Segoe UI";
            layout.Override.RowAppearance.FontData.SizeInPoints = 9F;

            // Alternating row colors (slate blue white)
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);

            // Selection colors
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(37, 99, 235); // Royal Blue
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;

            // Cell border style - Solid light grey borders
            layout.Override.CellAppearance.BorderColor = Color.FromArgb(226, 232, 240); // Slate 200
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;

            // Auto-size columns
            layout.Override.DefaultColWidth = 100;
            foreach (UltraGridColumn col in layout.Bands[0].Columns)
            {
                if (col.Key == "RowIndex" || col.Key == "HasError" || col.Key == "HasWarning" || col.Key == "StatusMessage")
                {
                    col.CellActivation = Activation.NoEdit;
                    col.CellAppearance.BackColor = Color.FromArgb(241, 245, 249);
                }
                else
                {
                    col.CellActivation = Activation.AllowEdit;
                }
            }

            if (layout.Bands[0].Columns.Exists("StatusMessage"))
                layout.Bands[0].Columns["StatusMessage"].Width = 300;
            if (layout.Bands[0].Columns.Exists("Description"))
                layout.Bands[0].Columns["Description"].Width = 200;
        }

        // UltraGrid InitializeRow — color rows based on validation status
        private void ultraGridPreview_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            var rowData = e.Row.ListObject as ExcelImportRepository.ImportRow;
            if (rowData != null)
            {
                if (rowData.HasError)
                {
                    e.Row.Appearance.BackColor = Color.MistyRose;
                }
                else if (rowData.HasWarning)
                {
                    e.Row.Appearance.BackColor = Color.LightYellow;
                }
                else
                {
                    e.Row.Appearance.BackColor = Color.LightGreen;
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            string duplicateBehavior = cmbDuplicateBehavior.SelectedItem?.ToString() ?? "Skip";
            bool autoCreate = chkAutoCreate.Checked;
            bool autoGenerateBarcodes = chkAutoGenerateBarcodes.Checked;

            int totalRows = _validationRows.Count;
            int errors = _validationRows.Count(r => r.HasError);

            var confirmResult = MessageBox.Show(
                $"Are you sure you want to import {totalRows - errors} valid products?\n\n" +
                $"Please make sure you have backed up the database before continuing.",
                "Confirm Import",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult != DialogResult.Yes) return;

            btnImport.Enabled = false;
            btnPreview.Enabled = false;
            btnBrowse.Enabled = false;
            btnLoad.Enabled = false;

            lblProgress.Text = "Starting import...";
            progressBarImport.Value = 0;

            bgWorkerImport.RunWorkerAsync(new object[] { duplicateBehavior, autoCreate, autoGenerateBarcodes });
        }

        private void bgWorkerImport_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            string duplicateBehavior = (string)args[0];
            bool autoCreate = (bool)args[1];
            bool autoGenerateBarcodes = (bool)args[2];

            // Run import process
            var summary = _importRepo.ImportProducts(
                _validationRows,
                duplicateBehavior,
                autoCreate,
                autoGenerateBarcodes,
                (processed, total) =>
                {
                    bgWorkerImport.ReportProgress(processed * 100 / total, new object[] { processed, total });
                }
            );

            e.Result = summary;
        }

        private void bgWorkerImport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarImport.Value = e.ProgressPercentage;
            object[] stats = (object[])e.UserState;
            lblProgress.Text = $"Importing products: {stats[0]} of {stats[1]}...";
        }

        private void bgWorkerImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnPreview.Enabled = true;
            btnBrowse.Enabled = true;
            btnLoad.Enabled = true;
            progressBarImport.Value = 100;

            var summary = e.Result as ExcelImportRepository.ImportSummary;
            if (summary != null)
            {
                lblStats.Text = $"Import Complete! Imported: {summary.SucceededCount} rows  |  Failed: {summary.FailedCount} rows.";
                lblProgress.Text = "Import finished.";

                _failedRows = summary.Rows.Where(r => r.HasError || r.StatusMessage.StartsWith("Import rolled back") || r.StatusMessage.StartsWith("Skipped")).ToList();

                if (_failedRows.Count > 0)
                {
                    btnDownloadErrorLog.Visible = true;
                    MessageBox.Show(
                        $"Import completed with errors.\n\n" +
                        $"Successfully Imported: {summary.NewItemsCreated} new items\n" +
                        $"Successfully Updated: {summary.ItemsUpdated} existing items\n" +
                        $"Failed/Skipped: {summary.FailedCount} items.\n\n" +
                        $"You can click the 'Download Failure Log' button to export a file with the error details.",
                        "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(
                        $"Bulk Import completed successfully!\n\n" +
                        $"Created: {summary.NewItemsCreated} new items\n" +
                        $"Updated: {summary.ItemsUpdated} existing items.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetMappingPanel();
                    txtFilePath.Text = "";
                }
            }
            else
            {
                MessageBox.Show("An unexpected error occurred during import.", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnImport.Enabled = true;
            }
        }

        private void btnDownloadErrorLog_Click(object sender, EventArgs e)
        {
            if (_failedRows.Count == 0) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = "ItemImport_FailureLog.csv";
                sfd.Title = "Save Failure Log";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("RowNumber", typeof(int));
                        dt.Columns.Add("Barcode", typeof(string));
                        dt.Columns.Add("Description", typeof(string));
                        dt.Columns.Add("Category", typeof(string));
                        dt.Columns.Add("Brand", typeof(string));
                        dt.Columns.Add("Unit", typeof(string));
                        dt.Columns.Add("Cost", typeof(double));
                        dt.Columns.Add("RetailPrice", typeof(double));
                        dt.Columns.Add("FailureReason", typeof(string));

                        foreach (var row in _failedRows)
                        {
                            dt.Rows.Add(row.RowIndex, row.Barcode, row.Description, row.Category, row.Brand, row.Unit, row.Cost, row.RetailPrice, row.StatusMessage);
                        }

                        ExcelImportRepository.CSVHelper.WriteCSV(dt, sfd.FileName);
                        MessageBox.Show("Failure Log exported successfully.", "Export Log Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting failure log: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = "Nexoris_Item_Import_Template.csv";
                sfd.Title = "Save Import Template";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("Barcode", typeof(string));
                        dt.Columns.Add("Description", typeof(string));
                        dt.Columns.Add("ItemType", typeof(string));
                        dt.Columns.Add("Category", typeof(string));
                        dt.Columns.Add("Brand", typeof(string));
                        dt.Columns.Add("Group", typeof(string));
                        dt.Columns.Add("Unit", typeof(string));
                        dt.Columns.Add("Packing", typeof(double));
                        dt.Columns.Add("IsBaseUnit", typeof(string));
                        dt.Columns.Add("Cost", typeof(double));
                        dt.Columns.Add("RetailPrice", typeof(double));
                        dt.Columns.Add("WholeSalePrice", typeof(double));
                        dt.Columns.Add("MRP", typeof(double));
                        dt.Columns.Add("CardPrice", typeof(double));
                        dt.Columns.Add("CreditPrice", typeof(double));
                        dt.Columns.Add("StaffPrice", typeof(double));
                        dt.Columns.Add("MinPrice", typeof(double));
                        dt.Columns.Add("OpeningStock", typeof(double));
                        dt.Columns.Add("ReorderLevel", typeof(double));
                        dt.Columns.Add("TaxType", typeof(string));
                        dt.Columns.Add("TaxPer", typeof(double));
                        dt.Columns.Add("HSNCode", typeof(string));
                        dt.Columns.Add("AlternativeBarcodes", typeof(string));
                        dt.Columns.Add("OrderCycleDays", typeof(int));
                        dt.Columns.Add("BoxQty", typeof(double));
                        dt.Columns.Add("Perishable", typeof(string));

                        // Add dummy row as example
                        dt.Rows.Add("8801019203912", "Coca Cola 500ml", "STOCK ITEM", "Beverages", "Coke", "FMCG", "PCS", 1.0, "Y", 45.0, 50.0, 50.0, 50.0, 50.0, 50.0, 50.0, 50.0, 100.0, 10.0, "EXCL", 5.0, "2202", "8801019203912A,8801019203912B", 7, 24.0, "N");
                        dt.Rows.Add("8801019203929", "Coca Cola 500ml", "STOCK ITEM", "Beverages", "Coke", "FMCG", "BOX", 24.0, "N", 1000.0, 1100.0, 1100.0, 1100.0, 1100.0, 1100.0, 1100.0, 1100.0, 5.0, 1.0, "EXCL", 5.0, "2202", "", 7, 24.0, "N");

                        ExcelImportRepository.CSVHelper.WriteCSV(dt, sfd.FileName);
                        MessageBox.Show("Template spreadsheet saved successfully.", "Save Template Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving template: {ex.Message}", "Save Template Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Export logic
        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = "Exported_Products.csv";
                sfd.Title = "Export Products";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    btnExport.Enabled = false;
                    btnLoadPreview.Enabled = false;
                    progressBarExport.Value = 0;
                    lblProgressExport.Text = "Exporting items...";

                    // Get filter parameters
                    int categoryId = 0;
                    var catSelectedItem = cmbExportCategory.SelectedItem;
                    if (catSelectedItem != null)
                    {
                        object catVal = (catSelectedItem is Infragistics.Win.ValueListItem catVli) ? catVli.DataValue : catSelectedItem;
                        if (catVal is ComboBoxItem catItem) categoryId = catItem.Value;
                    }

                    int brandId = 0;
                    var brandSelectedItem = cmbExportBrand.SelectedItem;
                    if (brandSelectedItem != null)
                    {
                        object brandVal = (brandSelectedItem is Infragistics.Win.ValueListItem brandVli) ? brandVli.DataValue : brandSelectedItem;
                        if (brandVal is ComboBoxItem brandItem) brandId = brandItem.Value;
                    }

                    int groupId = 0;
                    var groupSelectedItem = cmbExportGroup.SelectedItem;
                    if (groupSelectedItem != null)
                    {
                        object groupVal = (groupSelectedItem is Infragistics.Win.ValueListItem groupVli) ? groupVli.DataValue : groupSelectedItem;
                        if (groupVal is ComboBoxItem groupItem) groupId = groupItem.Value;
                    }

                    string filePath = sfd.FileName;
                    string searchPattern = txtExportSearch.Text;

                    bgWorkerExport.RunWorkerAsync(new object[] { categoryId, brandId, groupId, filePath, searchPattern });
                }
            }
        }

        private void bgWorkerExport_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            int categoryId = (int)args[0];
            int brandId = (int)args[1];
            int groupId = (int)args[2];
            string filePath = (string)args[3];
            string searchPattern = (string)args[4];

            DataTable dt = _importRepo.GetProductsForExport(categoryId, brandId, groupId, searchPattern);
            ExcelImportRepository.CSVHelper.WriteCSV(dt, filePath);
            
            e.Result = dt.Rows.Count;
        }

        private void bgWorkerExport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnExport.Enabled = true;
            btnLoadPreview.Enabled = true;
            progressBarExport.Value = 100;

            if (e.Error != null)
            {
                lblProgressExport.Text = "Export failed.";
                MessageBox.Show($"Error exporting products: {e.Error.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                int count = Convert.ToInt32(e.Result);
                lblProgressExport.Text = $"Exported {count} items successfully.";
                MessageBox.Show($"Successfully exported {count} records!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnLoadPreview_Click(object sender, EventArgs e)
        {
            btnLoadPreview.Enabled = false;
            btnExport.Enabled = false;
            progressBarExport.Value = 0;
            lblProgressExport.Text = "Loading preview...";

            // Get filter parameters
            int categoryId = 0;
            var catSelectedItem = cmbExportCategory.SelectedItem;
            if (catSelectedItem != null)
            {
                object catVal = (catSelectedItem is Infragistics.Win.ValueListItem catVli) ? catVli.DataValue : catSelectedItem;
                if (catVal is ComboBoxItem catItem) categoryId = catItem.Value;
            }

            int brandId = 0;
            var brandSelectedItem = cmbExportBrand.SelectedItem;
            if (brandSelectedItem != null)
            {
                object brandVal = (brandSelectedItem is Infragistics.Win.ValueListItem brandVli) ? brandVli.DataValue : brandSelectedItem;
                if (brandVal is ComboBoxItem brandItem) brandId = brandItem.Value;
            }

            int groupId = 0;
            var groupSelectedItem = cmbExportGroup.SelectedItem;
            if (groupSelectedItem != null)
            {
                object groupVal = (groupSelectedItem is Infragistics.Win.ValueListItem groupVli) ? groupVli.DataValue : groupSelectedItem;
                if (groupVal is ComboBoxItem groupItem) groupId = groupItem.Value;
            }

            string searchPattern = txtExportSearch.Text;

            bgWorkerExportPreview.RunWorkerAsync(new object[] { categoryId, brandId, groupId, searchPattern });
        }

        private void bgWorkerExportPreview_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            int categoryId = (int)args[0];
            int brandId = (int)args[1];
            int groupId = (int)args[2];
            string searchPattern = (string)args[3];

            DataTable dt = _importRepo.GetProductsForExport(categoryId, brandId, groupId, searchPattern);
            e.Result = dt;
        }

        private void bgWorkerExportPreview_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnLoadPreview.Enabled = true;
            btnExport.Enabled = true;
            progressBarExport.Value = 100;

            if (e.Error != null)
            {
                lblProgressExport.Text = "Load failed.";
                MessageBox.Show($"Error loading products: {e.Error.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                DataTable dt = e.Result as DataTable;
                ultraGridExportPreview.DataSource = dt;
                
                int count = dt?.Rows.Count ?? 0;
                lblProgressExport.Text = $"Loaded {count} items for preview.";
            }
        }

        private void ultraGridExportPreview_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridLayout layout = e.Layout;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.None;
            layout.Override.CellPadding = 4;
            layout.Override.HeaderStyle = HeaderStyle.Standard;

            // Flat headers with Navy blue background
            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(31, 58, 86);
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI Semibold";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5F;

            // Row appearance
            layout.Override.RowAppearance.FontData.Name = "Segoe UI";
            layout.Override.RowAppearance.FontData.SizeInPoints = 9F;

            // Alternating row colors
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);

            // Selection colors
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(37, 99, 235);
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;

            // Cell border style
            layout.Override.CellAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;

            layout.Override.DefaultColWidth = 100;
            foreach (UltraGridColumn col in layout.Bands[0].Columns)
            {
                col.CellActivation = Activation.NoEdit;
            }

            // Auto resize important columns if they exist
            if (layout.Bands[0].Columns.Exists("Description"))
                layout.Bands[0].Columns["Description"].Width = 200;
            if (layout.Bands[0].Columns.Exists("Barcode"))
                layout.Bands[0].Columns["Barcode"].Width = 150;
        }

        // Programmatic grid filter and cell validation event handlers
        private void CmbFilterStatus_ValueChanged(object sender, EventArgs e)
        {
            ApplyGridFilter();
        }

        private void ApplyGridFilter()
        {
            if (cmbFilterStatus == null || !cmbFilterStatus.Visible) return;

            string selected = cmbFilterStatus.Value?.ToString() ?? "Show All";

            ultraGridPreview.DataSource = null;

            if (selected == "Errors Only")
            {
                ultraGridPreview.DataSource = _validationRows.Where(r => r.HasError).ToList();
            }
            else if (selected == "Warnings Only")
            {
                ultraGridPreview.DataSource = _validationRows.Where(r => r.HasWarning && !r.HasError).ToList();
            }
            else if (selected == "Valid Rows Only")
            {
                ultraGridPreview.DataSource = _validationRows.Where(r => !r.HasError && !r.HasWarning).ToList();
            }
            else
            {
                ultraGridPreview.DataSource = _validationRows;
            }
        }

        private void ultraGridPreview_AfterCellUpdate(object sender, CellEventArgs e)
        {
            var rowData = e.Cell.Row.ListObject as ExcelImportRepository.ImportRow;
            if (rowData != null)
            {
                // Re-validate the edited row
                ValidateSingleRow(rowData);

                // Update the statistics label and button states
                UpdateStatistics();

                // Refresh row visual appearance (trigger initializeRow again)
                e.Cell.Row.Refresh();
            }
        }

        private void ValidateSingleRow(ExcelImportRepository.ImportRow row)
        {
            row.HasError = false;
            row.HasWarning = false;
            row.StatusMessage = string.Empty;

            bool autoGenerateBarcodes = chkAutoGenerateBarcodes.Checked;
            bool autoCreate = chkAutoCreate.Checked;

            // Barcode validation
            if (string.IsNullOrWhiteSpace(row.Barcode))
            {
                if (autoGenerateBarcodes)
                {
                    row.HasWarning = true;
                    row.StatusMessage += "[Barcode will be auto-generated] ";
                }
                else
                {
                    row.HasError = true;
                    row.StatusMessage += "Barcode is missing. ";
                }
            }
            else
            {
                // Clean barcode first
                row.Barcode = ExcelImportRepository.CleanImportedBarcode(row.Barcode);

                // Check duplicate barcode
                bool isDuplicate = _importRepo.ExistingBarcodes.Contains(row.Barcode);
                if (isDuplicate)
                {
                    string duplicateBehavior = cmbDuplicateBehavior.SelectedItem?.ToString() ?? "Skip";
                    if (duplicateBehavior == "Skip")
                    {
                        row.HasWarning = true;
                        row.StatusMessage += "Duplicate barcode: row will be skipped. ";
                    }
                    else
                    {
                        row.HasWarning = true;
                        row.StatusMessage += "Duplicate barcode: will merge & update prices. ";
                    }
                }
            }

            // Description/Name validation
            if (string.IsNullOrWhiteSpace(row.Description))
            {
                row.HasError = true;
                row.StatusMessage += "Item Name / Description is missing. ";
            }

            // Price validation
            if (row.Cost < 0)
            {
                row.HasError = true;
                row.StatusMessage += "Cost Price cannot be negative. ";
            }
            if (row.RetailPrice < 0)
            {
                row.HasError = true;
                row.StatusMessage += "Retail Price cannot be negative. ";
            }

            // Category/Brand/Unit master validation
            if (!autoCreate)
            {
                if (!string.IsNullOrWhiteSpace(row.Category) && !_importRepo.CategoryCache.ContainsKey(row.Category.Trim()))
                {
                    row.HasError = true;
                    row.StatusMessage += $"Category '{row.Category}' does not exist. ";
                }
                if (!string.IsNullOrWhiteSpace(row.Brand) && !_importRepo.BrandCache.ContainsKey(row.Brand.Trim()))
                {
                    row.HasError = true;
                    row.StatusMessage += $"Brand '{row.Brand}' does not exist. ";
                }
                if (!string.IsNullOrWhiteSpace(row.Unit) && !_importRepo.UnitCache.ContainsKey(row.Unit.Trim()))
                {
                    row.HasError = true;
                    row.StatusMessage += $"Unit '{row.Unit}' does not exist. ";
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(row.Category) && !_importRepo.CategoryCache.ContainsKey(row.Category.Trim()))
                {
                    row.HasWarning = true;
                    row.StatusMessage += $"[Will create Category: {row.Category}] ";
                }
                if (!string.IsNullOrWhiteSpace(row.Brand) && !_importRepo.BrandCache.ContainsKey(row.Brand.Trim()))
                {
                    row.HasWarning = true;
                    row.StatusMessage += $"[Will create Brand: {row.Brand}] ";
                }
                if (!string.IsNullOrWhiteSpace(row.Unit) && !_importRepo.UnitCache.ContainsKey(row.Unit.Trim()))
                {
                    row.HasWarning = true;
                    row.StatusMessage += $"[Will create Unit: {row.Unit}] ";
                }
            }

            if (!row.HasError && !row.HasWarning)
            {
                row.StatusMessage = "Valid";
            }
        }

        private void UpdateStatistics()
        {
            int total = _validationRows.Count;
            int errors = _validationRows.Count(r => r.HasError);
            int warnings = _validationRows.Count(r => r.HasWarning && !r.HasError);
            int valid = total - errors - warnings;

            lblStats.Text = $"Total Rows: {total}  |  Valid: {valid}  |  Warnings: {warnings}  |  Errors: {errors}";

            // Enable import if we have at least one row without error
            btnImport.Enabled = (total - errors) > 0;
        }

        // Helper structure for Dropdown Items — works with UltraComboEditor.Items.Add(object)
        private class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public ComboBoxItem(string text, int value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
