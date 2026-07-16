using Infragistics.Controls.Barcodes;
using Infragistics.Win.DataVisualization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using PosBranch_Win.DialogBox;
using Infragistics.Win.UltraWinGrid;
using Repository;
using ModelClass;

namespace PosBranch_Win.Utilities
{
    public partial class frmBarcode : Form
    {
        // Store selected item data from dialog
        private Dictionary<string, object> selectedItemData = null;
        private DataTable selectedItemsTable = null;
        private int nextRowNumber = 1;

        // Repository for item search
        private Dropdowns dp = new Dropdowns();

        // Footer Panel Fields for Price Summary
        private Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>();
        private string currentSummaryType = "Sum";
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };
        private Dictionary<string, string> columnAggregations = new Dictionary<string, string>();

        public frmBarcode()
        {
            InitializeComponent();

            // Initialize the footer panel
            InitializeSummaryFooterPanel();

            // Initialize the grid
            InitializeGrid();

            // Connect button events
            if (button5 != null)
                button5.Click += Button5_Click;



            // Connect additional buttons
            ConnectUIButtons();

            // Connect grid events
            if (ultraGrid1 != null)
                ultraGrid1.AfterCellUpdate += UltraGrid1_AfterCellUpdate;

            // Connect ultraTextEditor1 events for barcode search
            ConnectBarcodeSearchEvents();

            // Initialize UI
            UpdateUIWithSelectedItem();
            UpdateSummaryLabels();

            // Set focus to barcode input when form is shown
            this.Shown += (s, e) =>
            {
                if (ultraTextEditor1 != null)
                {
                    ultraTextEditor1.Focus();
                }
            };
        }

        private void InitializeGrid()
        {
            try
            {
                // Create DataTable for selected items
                selectedItemsTable = new DataTable();
                selectedItemsTable.Columns.Add("No", typeof(int));
                selectedItemsTable.Columns.Add("Barcode", typeof(string));
                selectedItemsTable.Columns.Add("Description", typeof(string));
                selectedItemsTable.Columns.Add("Qty", typeof(int));
                selectedItemsTable.Columns.Add("Price", typeof(decimal));

                // Bind to grid
                ultraGrid1.DataSource = selectedItemsTable;

                // Apply professional formatting similar to frmSalesInvoice
                FormatBarcodeGrid();

                // Connect grid events
                ConnectGridEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing grid: {ex.Message}");
            }
        }

        private void FormatBarcodeGrid()
        {
            try
            {
                // Configure main grid appearance with modern styling
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortMulti;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Enable alternating row colors
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = System.Drawing.Color.FromArgb(240, 242, 245);
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

                // Configure header appearance with modern gradient look
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Reset appearance settings
                ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

                // Set key navigation and selection properties
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.TabNavigation = Infragistics.Win.UltraWinGrid.TabNavigation.NextCell;

                // Set basic row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;

                // Configure active row highlighting
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.Black;

                // Remove active cell highlighting
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = System.Drawing.Color.Empty;
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = System.Drawing.Color.Black;

                // Configure scrolling and layout
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;
                ultraGrid1.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.AutoFree;
                ultraGrid1.DisplayLayout.InterBandSpacing = 10;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 2;

                // Configure columns
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    var band = ultraGrid1.DisplayLayout.Bands[0];

                    // No column
                    if (band.Columns.Exists("No"))
                    {
                        band.Columns["No"].Header.Caption = "No";
                        band.Columns["No"].Width = 50;
                        band.Columns["No"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        band.Columns["No"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["No"].MinWidth = 40;
                    }

                    // Barcode column
                    if (band.Columns.Exists("Barcode"))
                    {
                        band.Columns["Barcode"].Header.Caption = "Barcode";
                        band.Columns["Barcode"].Width = 150;
                        band.Columns["Barcode"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["Barcode"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["Barcode"].MinWidth = 100;
                    }

                    // Description column
                    if (band.Columns.Exists("Description"))
                    {
                        band.Columns["Description"].Header.Caption = "Description";
                        band.Columns["Description"].Width = 250;
                        band.Columns["Description"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["Description"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["Description"].MinWidth = 150;
                    }

                    // Qty column
                    if (band.Columns.Exists("Qty"))
                    {
                        band.Columns["Qty"].Header.Caption = "Qty";
                        band.Columns["Qty"].Width = 60;
                        band.Columns["Qty"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        band.Columns["Qty"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;
                        band.Columns["Qty"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.Integer;
                        band.Columns["Qty"].MinWidth = 50;
                    }

                    // Price column
                    if (band.Columns.Exists("Price"))
                    {
                        band.Columns["Price"].Header.Caption = "Price";
                        band.Columns["Price"].Width = 100;
                        band.Columns["Price"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        band.Columns["Price"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                        band.Columns["Price"].CellAppearance.ForeColor = System.Drawing.Color.FromArgb(0, 122, 204);
                        band.Columns["Price"].Format = "0.00";
                        band.Columns["Price"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["Price"].MinWidth = 80;
                    }
                }

                // Initialize footer panel after grid is formatted
                UpdateSummaryFooter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting grid: {ex.Message}");
            }
        }

        private void ConnectGridEvents()
        {
            try
            {
                // Add keyboard event handler for delete functionality
                ultraGrid1.KeyDown += UltraGrid1_KeyDown;

                // Add right-click context menu
                SetupContextMenu();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting grid events: {ex.Message}");
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete && ultraGrid1.ActiveRow != null)
                {
                    // Delete selected row
                    DeleteSelectedRow();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null && ultraGrid1.ActiveCell?.Column.Key == "Qty")
                {
                    // Move to next row when Enter is pressed in Qty column
                    if (ultraGrid1.ActiveRow.Index < ultraGrid1.Rows.Count - 1)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index + 1];
                        ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Qty"];
                    }
                }
                else if (e.KeyCode == Keys.F8)
                {
                    // Print labels
                    Print_btn_Click(sender, e);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in grid key handler: {ex.Message}");
            }
        }

        private void DeleteSelectedRow()
        {
            try
            {
                if (ultraGrid1.ActiveRow != null && selectedItemsTable != null)
                {
                    // Confirm deletion
                    string itemName = ultraGrid1.ActiveRow.Cells["Description"].Value?.ToString() ?? "Unknown Item";
                    DialogResult result = MessageBox.Show($"Delete item '{itemName}' from the list?", "Confirm Delete",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        int rowIndex = ultraGrid1.ActiveRow.Index;
                        selectedItemsTable.Rows.RemoveAt(rowIndex);

                        // Renumber remaining rows
                        RenumberRows();

                        // Update UI
                        UpdateUIWithSelectedItem();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting row: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenumberRows()
        {
            try
            {
                if (selectedItemsTable == null) return;

                for (int i = 0; i < selectedItemsTable.Rows.Count; i++)
                {
                    selectedItemsTable.Rows[i]["No"] = i + 1;
                }

                nextRowNumber = selectedItemsTable.Rows.Count + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error renumbering rows: {ex.Message}");
            }
        }

        private void SetupContextMenu()
        {
            try
            {
                ContextMenuStrip contextMenu = new ContextMenuStrip();

                // Delete item menu
                ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete Item");
                deleteItem.Image = SystemIcons.Error.ToBitmap();
                deleteItem.Click += (s, e) => DeleteSelectedRow();
                contextMenu.Items.Add(deleteItem);

                // Separator
                contextMenu.Items.Add(new ToolStripSeparator());

                // Clear all items menu
                ToolStripMenuItem clearAll = new ToolStripMenuItem("Clear All Items");
                clearAll.Image = SystemIcons.Warning.ToBitmap();
                clearAll.Click += (s, e) => ClearAllItems();
                contextMenu.Items.Add(clearAll);

                // Separator
                contextMenu.Items.Add(new ToolStripSeparator());

                // Preview selected item
                ToolStripMenuItem previewItem = new ToolStripMenuItem("Preview This Item");
                previewItem.Image = SystemIcons.Information.ToBitmap();
                previewItem.Click += (s, e) => PreviewSelectedItem();
                contextMenu.Items.Add(previewItem);

                ultraGrid1.ContextMenuStrip = contextMenu;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up context menu: {ex.Message}");
            }
        }

        private void ClearAllItems()
        {
            try
            {
                if (selectedItemsTable == null || selectedItemsTable.Rows.Count == 0) return;

                DialogResult result = MessageBox.Show($"Clear all {selectedItemsTable.Rows.Count} items from the list?",
                    "Confirm Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    selectedItemsTable.Clear();
                    nextRowNumber = 1;
                    UpdateUIWithSelectedItem();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing items: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PreviewSelectedItem()
        {
            try
            {
                if (ultraGrid1.ActiveRow == null) return;

                // Create a temporary single-item table for preview
                DataTable tempTable = selectedItemsTable.Clone();
                DataRowView selectedRowView = ultraGrid1.ActiveRow.ListObject as DataRowView;
                if (selectedRowView?.Row != null)
                {
                    tempTable.ImportRow(selectedRowView.Row);

                    // Temporarily store current table and show preview for single item
                    DataTable originalTable = selectedItemsTable;
                    selectedItemsTable = tempTable;
                    ShowBarcodePreview();
                    selectedItemsTable = originalTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error previewing item: {ex.Message}", "Preview Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectUIButtons()
        {
            try
            {
                // Connect print button
                var printBtn = this.Controls.Find("Print_btn", true).FirstOrDefault() as Button;
                if (printBtn == null)
                {
                    var ultraPrintBtn = this.Controls.Find("Print_btn", true).FirstOrDefault() as Infragistics.Win.Misc.UltraButton;
                    if (ultraPrintBtn != null)
                    {
                        ultraPrintBtn.Click += Print_btn_Click;
                        ultraPrintBtn.Text = "Print (F8)";
                    }
                }
                else
                {
                    printBtn.Click += Print_btn_Click;
                    printBtn.Text = "Print (F8)";
                }

                // Connect preview button
                var previewBtn = this.Controls.Find("preview_btn", true).FirstOrDefault() as Button;
                if (previewBtn == null)
                {
                    var ultraPreviewBtn = this.Controls.Find("preview_btn", true).FirstOrDefault() as Infragistics.Win.Misc.UltraButton;
                    if (ultraPreviewBtn != null)
                    {
                        ultraPreviewBtn.Click += Preview_btn_Click;
                        ultraPreviewBtn.Text = "Preview";
                    }
                }
                else
                {
                    previewBtn.Click += Preview_btn_Click;
                    previewBtn.Text = "Preview";
                }

                // Connect clear button
                var clearBtn = this.Controls.Find("clear_btn", true).FirstOrDefault() as Button;
                if (clearBtn == null)
                {
                    var ultraClearBtn = this.Controls.Find("clear_btn", true).FirstOrDefault() as Infragistics.Win.Misc.UltraButton;
                    if (ultraClearBtn != null)
                    {
                        ultraClearBtn.Click += Clear_btn_Click;
                        ultraClearBtn.Text = "Clear All";
                    }
                }
                else
                {
                    clearBtn.Click += Clear_btn_Click;
                    clearBtn.Text = "Clear All";
                }

                // Connect delete button
                var deleteBtn = this.Controls.Find("delete_btn", true).FirstOrDefault() as Button;
                if (deleteBtn == null)
                {
                    var ultraDeleteBtn = this.Controls.Find("delete_btn", true).FirstOrDefault() as Infragistics.Win.Misc.UltraButton;
                    if (ultraDeleteBtn != null)
                    {
                        ultraDeleteBtn.Click += Delete_btn_Click;
                        ultraDeleteBtn.Text = "Delete";
                    }
                }
                else
                {
                    deleteBtn.Click += Delete_btn_Click;
                    deleteBtn.Text = "Delete";
                }

                // Connect search button (button5) and set text
                if (button5 != null)
                {
                    button5.Text = "F7";
                    button5.Click -= Button5_Click; // Prevent duplicate wiring
                    button5.Click += Button5_Click;
                }
                else
                {
                    var btn5 = this.Controls.Find("button5", true).FirstOrDefault();
                    if (btn5 != null)
                    {
                        btn5.Text = "F7";
                        btn5.Click -= Button5_Click; // Prevent duplicate wiring
                        btn5.Click += Button5_Click;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting UI buttons: {ex.Message}");
            }
        }

        private void Clear_btn_Click(object sender, EventArgs e)
        {
            ClearAllItems();
        }

        private void Delete_btn_Click(object sender, EventArgs e)
        {
            DeleteSelectedRow();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            try
            {
                // Open the Item Master dialog
                frmdialForItemMaster itemDialog = new frmdialForItemMaster("FrmBarcode");
                itemDialog.StartPosition = FormStartPosition.CenterScreen;

                if (itemDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get selected item data
                    selectedItemData = itemDialog.GetSelectedItemData();

                    if (selectedItemData != null && selectedItemData.Count > 0)
                    {
                        // Add item to the grid
                        AddItemToGrid();

                        // Update the UI with selected item information
                        UpdateUIWithSelectedItem();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening item dialog: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Preview_btn_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedItemsTable == null || selectedItemsTable.Rows.Count == 0)
                {
                    MessageBox.Show("Please select items first using the F7 button.", "No Items Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create and show preview form
                ShowBarcodePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing preview: {ex.Message}", "Preview Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowBarcodePreview()
        {
            // Create preview form
            Form previewForm = new Form();
            previewForm.Text = "Barcode Label Preview";
            previewForm.Size = new Size(800, 600);
            previewForm.StartPosition = FormStartPosition.CenterParent;
            previewForm.MaximizeBox = false;
            previewForm.MinimizeBox = false;

            // Create panel to hold all previews
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;
            mainPanel.BackColor = Color.White;

            int yPosition = 10;
            int totalLabels = 0;

            foreach (DataRow row in selectedItemsTable.Rows)
            {
                // Get item data
                string barcode = row["Barcode"]?.ToString() ?? "";
                string description = row["Description"]?.ToString() ?? "";
                string price = row["Price"]?.ToString() ?? "0.00";
                int qty = 1;
                if (int.TryParse(row["Qty"]?.ToString(), out int parsedQty))
                    qty = parsedQty;

                totalLabels += qty;

                // Create preview panel for this item
                Panel itemPanel = CreateItemPreviewPanel(barcode, description, price, qty);
                itemPanel.Location = new System.Drawing.Point(10, yPosition);
                mainPanel.Controls.Add(itemPanel);

                yPosition += itemPanel.Height + 15;
            }

            // Add summary label
            Label summaryLabel = new Label();
            summaryLabel.Text = $"Total Items: {selectedItemsTable.Rows.Count} | Total Labels: {totalLabels}";
            summaryLabel.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            summaryLabel.ForeColor = Color.DarkBlue;
            summaryLabel.AutoSize = true;
            summaryLabel.Location = new System.Drawing.Point(10, yPosition + 10);
            mainPanel.Controls.Add(summaryLabel);

            // Add close button
            Button closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Size = new Size(100, 30);
            closeButton.Location = new System.Drawing.Point(350, yPosition + 40);
            closeButton.Click += (s, args) => previewForm.Close();
            mainPanel.Controls.Add(closeButton);

            previewForm.Controls.Add(mainPanel);
            previewForm.ShowDialog(this);
        }

        private Panel CreateItemPreviewPanel(string barcode, string description, string price, int qty)
        {
            Panel panel = new Panel();
            panel.Size = new Size(760, 180);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.LightGray;

            // Item info label
            string truncatedDescription = TruncateItemName(description);
            Label infoLabel = new Label();
            infoLabel.Text = $"Item: {description} (Print: {truncatedDescription}) | Barcode: {barcode} | Price: ${price} | Quantity: {qty}";
            infoLabel.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            infoLabel.Location = new System.Drawing.Point(5, 5);
            infoLabel.Size = new Size(750, 20);
            panel.Controls.Add(infoLabel);

            // Create label preview (simulating the actual label layout)
            Panel labelPreview = CreateLabelPreview(barcode, description, price);
            labelPreview.Location = new System.Drawing.Point(5, 30);
            panel.Controls.Add(labelPreview);

            // Quantity indicator
            Label qtyLabel = new Label();
            qtyLabel.Text = $"× {qty} labels will be printed";
            qtyLabel.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Italic);
            qtyLabel.ForeColor = Color.DarkGreen;
            qtyLabel.Location = new System.Drawing.Point(5, 155);
            qtyLabel.Size = new Size(200, 15);
            panel.Controls.Add(qtyLabel);

            return panel;
        }

        private Panel CreateLabelPreview(string barcode, string description, string price)
        {
            Panel labelPanel = new Panel();
            labelPanel.Size = new Size(750, 120);
            labelPanel.BackColor = Color.White;
            labelPanel.BorderStyle = BorderStyle.FixedSingle;

            // Create two labels side by side as per the template
            Panel leftLabel = CreateSingleLabelPreview(barcode, description, price, 10);
            Panel rightLabel = CreateSingleLabelPreview(barcode, description, price, 385);

            labelPanel.Controls.Add(leftLabel);
            labelPanel.Controls.Add(rightLabel);

            return labelPanel;
        }

        private Panel CreateSingleLabelPreview(string barcode, string description, string price, int xOffset)
        {
            Panel singleLabel = new Panel();
            singleLabel.Size = new Size(360, 110);
            singleLabel.Location = new System.Drawing.Point(xOffset, 5);
            singleLabel.BorderStyle = BorderStyle.FixedSingle;
            singleLabel.BackColor = Color.White;


            // Company name
            Label companyLabel = new Label();
            companyLabel.Text = SessionContext.CompanyName ?? "Company Name";
            companyLabel.Font = new Font("Arial", 8, FontStyle.Bold);
            companyLabel.Location = new System.Drawing.Point(5, 5);
            companyLabel.Size = new Size(350, 15);
            companyLabel.TextAlign = ContentAlignment.MiddleCenter;
            singleLabel.Controls.Add(companyLabel);

            // Create barcode visual representation
            Panel barcodePanel = new Panel();
            barcodePanel.Size = new Size(200, 40);
            barcodePanel.Location = new System.Drawing.Point(80, 25);
            barcodePanel.BackColor = Color.Black;

            // Add white lines to simulate barcode
            for (int i = 0; i < 200; i += 3)
            {
                Panel line = new Panel();
                line.Size = new Size(1, 40);
                line.Location = new System.Drawing.Point(i, 0);
                line.BackColor = i % 6 == 0 ? Color.White : Color.Black;
                barcodePanel.Controls.Add(line);
            }
            singleLabel.Controls.Add(barcodePanel);

            // Barcode text
            Label barcodeText = new Label();
            barcodeText.Text = barcode;
            barcodeText.Font = new Font("Arial", 7);
            barcodeText.Location = new System.Drawing.Point(5, 70);
            barcodeText.Size = new Size(350, 15);
            barcodeText.TextAlign = ContentAlignment.MiddleCenter;
            singleLabel.Controls.Add(barcodeText);

            // Item description - use the same truncation logic as the actual print
            string truncatedDescription = TruncateItemName(description);
            Label descLabel = new Label();
            descLabel.Text = truncatedDescription;
            descLabel.Font = new Font("Arial", 7);
            descLabel.Location = new System.Drawing.Point(5, 85);
            descLabel.Size = new Size(250, 15);
            descLabel.TextAlign = ContentAlignment.MiddleLeft; // Align to show spaces properly
            singleLabel.Controls.Add(descLabel);

            // Price
            Label priceLabel = new Label();
            priceLabel.Text = $"${price}";
            priceLabel.Font = new Font("Arial", 8, FontStyle.Bold);
            priceLabel.Location = new System.Drawing.Point(260, 85);
            priceLabel.Size = new Size(95, 15);
            priceLabel.TextAlign = ContentAlignment.MiddleRight;
            singleLabel.Controls.Add(priceLabel);

            return singleLabel;
        }

        private void UltraGrid1_AfterCellUpdate(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            try
            {
                // If quantity was updated, refresh the UI summary
                if (e.Cell.Column.Key == "Qty")
                {
                    System.Diagnostics.Debug.WriteLine($"Quantity cell updated: '{e.Cell.Value}'");

                    // Validate quantity is positive and within reasonable limits
                    if (int.TryParse(e.Cell.Value?.ToString(), out int qty))
                    {
                        if (qty <= 0)
                        {
                            e.Cell.Value = 1; // Set minimum quantity to 1
                            System.Diagnostics.Debug.WriteLine("Quantity was <= 0, reset to 1");
                            MessageBox.Show("Quantity must be at least 1.", "Invalid Quantity",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (qty > 1000) // Set reasonable upper limit
                        {
                            e.Cell.Value = 1000; // Cap at 1000
                            System.Diagnostics.Debug.WriteLine($"Quantity was {qty}, capped to 1000");
                            MessageBox.Show("Quantity cannot exceed 1000. Set to maximum of 1000.", "Quantity Limited",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Valid quantity entered: {qty}");
                        }
                    }
                    else
                    {
                        e.Cell.Value = 1; // Set to 1 if invalid input
                        System.Diagnostics.Debug.WriteLine("Invalid quantity format, reset to 1");
                        MessageBox.Show("Please enter a valid number for quantity.", "Invalid Quantity",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    // Update UI to reflect new quantity totals
                    UpdateUIWithSelectedItem();
                    UpdateSummaryLabels();

                    // Refresh footer panel when data changes
                    RefreshSummaryFooter();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in quantity update: {ex.Message}");
            }
        }

        private void AddItemToGrid()
        {
            if (selectedItemData == null || selectedItemsTable == null) return;

            try
            {
                // Extract data with default values
                string barcode = selectedItemData.ContainsKey("BarCode") ? selectedItemData["BarCode"]?.ToString() ?? "" : "";
                string description = selectedItemData.ContainsKey("Description") ? selectedItemData["Description"]?.ToString() ?? "Unknown Item" : "Unknown Item";

                // Handle price conversion safely
                decimal price = 0;
                if (selectedItemData.ContainsKey("RetailPrice") && selectedItemData["RetailPrice"] != null)
                {
                    if (!decimal.TryParse(selectedItemData["RetailPrice"].ToString(), out price))
                    {
                        price = 0;
                    }
                }

                // Check if item already exists in grid
                if (!IsItemAlreadyInGrid(barcode))
                {
                    // Add new row to the grid
                    DataRow newRow = selectedItemsTable.NewRow();
                    newRow["No"] = nextRowNumber;
                    newRow["Barcode"] = barcode;
                    newRow["Description"] = description;
                    newRow["Qty"] = 1; // Default quantity is 1
                    newRow["Price"] = price;

                    selectedItemsTable.Rows.Add(newRow);
                    nextRowNumber++;


                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        $"Item '{description}' is already in the list.\n\nWould you like to increment the quantity?",
                        "Duplicate Item",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        IncrementItemQuantity(barcode);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item to grid: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error adding item to grid: {ex.Message}");
            }
        }

        /// <summary>
        /// Public method to load item from itemId into the barcode grid
        /// </summary>
        public void LoadItemFromItemId(int itemId, string barcode = "", string description = "", decimal retailPrice = 0)
        {
            try
            {
                if (itemId <= 0)
                {
                    MessageBox.Show("Invalid item ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // If barcode, description, or price not provided, fetch from repository
                if (string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(description) || retailPrice == 0)
                {
                    Repository.MasterRepositry.ItemMasterRepository itemRepo = new Repository.MasterRepositry.ItemMasterRepository();
                    ModelClass.Master.ItemGet getItem = itemRepo.GetByIdItem(itemId);

                    if (getItem == null)
                    {
                        MessageBox.Show("Item not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Use provided values or fallback to item data
                    if (string.IsNullOrEmpty(barcode))
                    {
                        barcode = getItem.Barcode ?? "";
                    }

                    if (string.IsNullOrEmpty(description))
                    {
                        description = getItem.Description ?? "Unknown Item";
                    }

                    if (retailPrice == 0 && getItem.List != null && getItem.List.Length > 0)
                    {
                        retailPrice = (decimal)getItem.List[0].RetailPrice;
                    }
                }

                // Create item data dictionary
                selectedItemData = new Dictionary<string, object>();
                selectedItemData["ItemId"] = itemId;
                selectedItemData["BarCode"] = barcode;
                selectedItemData["Description"] = description;
                selectedItemData["RetailPrice"] = retailPrice;

                // Add item to grid
                AddItemToGrid();

                // Update UI
                UpdateUIWithSelectedItem();
                UpdateSummaryLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading item: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error loading item from itemId: {ex.Message}");
            }
        }

        private void UpdateUIWithSelectedItem()
        {
            try
            {
                // Keep the text editor blank for barcode input
                if (ultraTextEditor1 != null)
                {
                    ultraTextEditor1.Text = "";
                    ultraTextEditor1.Focus(); // Auto-focus for better UX
                }

                // Update summary labels
                UpdateSummaryLabels();

                // Enable/disable buttons based on items availability
                bool hasItems = selectedItemsTable != null && selectedItemsTable.Rows.Count > 0;


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating UI: {ex.Message}");
            }
        }

        private void UpdateSummaryLabels()
        {
            try
            {
                if (selectedItemsTable == null)
                {
                    if (lblTotalItems != null)
                    {
                        lblTotalItems.Text = "Total Items: 0";
                        StyleSummaryLabel(lblTotalItems);
                    }
                    if (lblTotalLabels != null)
                    {
                        lblTotalLabels.Text = "Total Labels: 0";
                        StyleSummaryLabel(lblTotalLabels);
                    }

                    // Refresh footer panel when data changes
                    RefreshSummaryFooter();
                    return;
                }

                int totalItems = selectedItemsTable.Rows.Count;
                int totalLabels = 0;

                foreach (DataRow row in selectedItemsTable.Rows)
                {
                    if (int.TryParse(row["Qty"]?.ToString(), out int qty))
                        totalLabels += qty;
                }

                if (lblTotalItems != null)
                {
                    lblTotalItems.Text = $"Total Items: {totalItems}";
                    StyleSummaryLabel(lblTotalItems);
                }
                if (lblTotalLabels != null)
                {
                    lblTotalLabels.Text = $"Total Labels: {totalLabels}";
                    StyleSummaryLabel(lblTotalLabels);
                }

                // Refresh footer panel when data changes
                RefreshSummaryFooter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating summary labels: {ex.Message}");
            }
        }

        private void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            try
            {
                if (label != null)
                {
                    label.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
                    label.ForeColor = System.Drawing.Color.FromArgb(52, 58, 64);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error styling summary label: {ex.Message}");
            }
        }



        private void Print_btn_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedItemsTable == null || selectedItemsTable.Rows.Count == 0)
                {
                    MessageBox.Show("Please select items first using the F7 button.", "No Items Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Use relative path to the executable directory
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string existingFilePath = Path.Combine(appDirectory, "BarTenderFile & Its Txt File", "3X1 2 Column.txt");

                if (!File.Exists(existingFilePath))
                {
                    MessageBox.Show($"Text file not found: {existingFilePath}", "File Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Print labels for all items in the grid using the existing file
                int totalPrinted = 0;
                int totalItems = selectedItemsTable.Rows.Count;

                int itemIndex = 0;
                foreach (DataRow row in selectedItemsTable.Rows)
                {
                    itemIndex++;
                    System.Diagnostics.Debug.WriteLine($"\n--- Processing Item {itemIndex}/{totalItems} ---");

                    // Get quantity from the row - handle different data types
                    int qty = 1;
                    object qtyValue = row["Qty"];

                    System.Diagnostics.Debug.WriteLine($"Raw quantity value: '{qtyValue}', Type: {qtyValue?.GetType().Name ?? "null"}");

                    if (qtyValue != null && qtyValue != DBNull.Value)
                    {
                        if (qtyValue is int intQty)
                        {
                            qty = intQty > 0 ? intQty : 1;
                            System.Diagnostics.Debug.WriteLine($"Quantity parsed as int: {qty}");
                        }
                        else if (qtyValue is decimal decimalQty)
                        {
                            qty = (int)decimalQty > 0 ? (int)decimalQty : 1;
                            System.Diagnostics.Debug.WriteLine($"Quantity parsed as decimal: {qty}");
                        }
                        else if (int.TryParse(qtyValue.ToString(), out int parsedQty))
                        {
                            qty = parsedQty > 0 ? parsedQty : 1;
                            System.Diagnostics.Debug.WriteLine($"Quantity parsed from string: {qty}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse quantity '{qtyValue}', using default: 1");
                            qty = 1;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Quantity value is null or DBNull, using default: 1");
                        qty = 1;
                    }

                    // Get item info for debugging
                    string itemName = row["Description"]?.ToString() ?? "Unknown";
                    string barcode = row["Barcode"]?.ToString() ?? "Unknown";
                    string price = row["Price"]?.ToString() ?? "0.00";

                    System.Diagnostics.Debug.WriteLine($"Item: {itemName}");
                    System.Diagnostics.Debug.WriteLine($"Barcode: {barcode}");
                    System.Diagnostics.Debug.WriteLine($"Price: {price}");
                    System.Diagnostics.Debug.WriteLine($"Requested Quantity: {qty}");

                    // Generate dynamic label commands for this specific item and quantity
                    System.Diagnostics.Debug.WriteLine($"▶ GENERATING DYNAMIC LABELS: Will create {qty} labels for '{itemName}'");
                    string labelCommands = GenerateDynamicLabelCommands(row, qty);

                    if (!string.IsNullOrEmpty(labelCommands))
                    {
                        // Create temporary file with dynamic commands
                        string tempFile = Path.Combine(Path.GetTempPath(), $"barcode_temp_{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt");
                        File.WriteAllText(tempFile, labelCommands);

                        System.Diagnostics.Debug.WriteLine($"Sending dynamic commands to printer: {tempFile}");

                        try
                        {
                            PrintLabelFile(tempFile, "SNBC TVSE LP 46 NEO BPLE", 1); // Print once since commands already contain all copies
                            totalPrinted += qty;

                            // Clean up temp file
                            if (File.Exists(tempFile))
                                File.Delete(tempFile);

                            // Debug output with confirmation
                            System.Diagnostics.Debug.WriteLine($"✓ COMPLETED: Successfully printed {qty} labels for '{itemName}'");
                            System.Diagnostics.Debug.WriteLine($"Running total printed: {totalPrinted} labels");
                        }
                        catch (Exception printEx)
                        {
                            // Clean up temp file on error
                            if (File.Exists(tempFile))
                                File.Delete(tempFile);

                            System.Diagnostics.Debug.WriteLine($"✗ FAILED: Error printing {qty} labels for '{itemName}': {printEx.Message}");
                            throw; // Re-throw to be caught by outer try-catch
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ FAILED: Could not generate commands for '{itemName}'");
                    }

                    // Small delay between items to prevent printer queue issues
                    System.Threading.Thread.Sleep(200);

                    System.Diagnostics.Debug.WriteLine($"--- End Item {itemIndex} ---\n");
                }

                MessageBox.Show($"Successfully printed {totalPrinted} labels for {totalItems} items!",
                    "Print Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing labels: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
            }
        }

        private string GenerateDynamicLabelCommands(DataRow itemRow, int quantity)
        {
            try
            {
                // Get data from the specific row
                string barcode = itemRow["Barcode"]?.ToString() ?? "A1234567891234";
                string itemName = itemRow["Description"]?.ToString() ?? "Item Name";

                // Format price properly
                string price = "0.00";
                if (itemRow["Price"] != null && itemRow["Price"] != DBNull.Value)
                {
                    if (decimal.TryParse(itemRow["Price"].ToString(), out decimal priceValue))
                    {
                        price = priceValue.ToString("0.00");
                    }
                }

                string companyName = SessionContext.CompanyName ?? "Company Name";

                // Truncate item name to fit within the 70-unit width constraint
                // Based on the template: A600,70,2,4,0,1,N,"Item Name" - width is 70 units
                // For a 3.5 cm width label, 70 units represents approximately 19-23 characters
                string truncatedItemName = TruncateItemName(itemName);

                System.Diagnostics.Debug.WriteLine($"Generating dynamic commands for: {itemName}, Barcode: {barcode}, Price: {price}, Qty: {quantity}");
                System.Diagnostics.Debug.WriteLine($"Original item name: '{itemName}' -> Truncated: '{truncatedItemName}'");

                if (quantity <= 0) quantity = 1;

                // Calculate how many rows needed (2 labels per row)
                int rowsNeeded = (int)Math.Ceiling((double)quantity / 2.0);
                string allCommands = "";

                System.Diagnostics.Debug.WriteLine($"Quantity {quantity} requires {rowsNeeded} rows (2 labels per row)");

                for (int row = 0; row < rowsNeeded; row++)
                {
                    // Start each row with header commands (like the template)
                    string rowCommands = $@"I8,A
q597
O
JF
ZT
Q206,25
N";

                    int labelsInThisRow = Math.Min(2, quantity - (row * 2));
                    System.Diagnostics.Debug.WriteLine($"Row {row + 1}: Will print {labelsInThisRow} labels");

                    // Left column (first label in row) - always print if we need labels
                    if (labelsInThisRow >= 1)
                    {
                        rowCommands += $@"
B581,154,2,1,2,4,46,N,""{barcode}""
A531,99,2,2,1,2,N,""{barcode}""
A575,185,2,4,1,1,N,""{companyName}""
A600,70,2,4,0,1,N,""{truncatedItemName}""
A487,33,2,4,1,1,N,""{price}""";
                        System.Diagnostics.Debug.WriteLine($"Row {row + 1}: Added left column label");
                    }

                    // Right column (second label in row) - only if we need 2 labels in this row
                    if (labelsInThisRow >= 2)
                    {
                        rowCommands += $@"
B277,154,2,1,2,4,46,N,""{barcode}""
A227,99,2,2,1,2,N,""{barcode}""
A271,185,2,4,1,1,N,""{companyName}""
A280,70,2,4,0,1,N,""{truncatedItemName}""
A183,33,2,4,1,1,N,""{price}""";
                        System.Diagnostics.Debug.WriteLine($"Row {row + 1}: Added right column label");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Row {row + 1}: Right column unused (partial row)");
                    }

                    // Add print command for this row (like the template)
                    rowCommands += "\nP1";

                    // Add separator between rows (except for the first row)
                    if (row > 0)
                        allCommands += "\n\n";
                    allCommands += rowCommands;

                    System.Diagnostics.Debug.WriteLine($"Row {row + 1} commands generated");
                }

                System.Diagnostics.Debug.WriteLine($"Generated {rowsNeeded} separate print jobs with {quantity} total labels");

                int totalRows = (int)Math.Ceiling((double)quantity / 2.0);
                System.Diagnostics.Debug.WriteLine($"Total commands generated for {quantity} labels across {totalRows} rows");
                return allCommands;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating dynamic label commands: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Truncates item name to fit within the label width constraint and centers shorter names
        /// </summary>
        /// <param name="itemName">Original item name</param>
        /// <param name="maxLength">Maximum characters allowed (default calculated for 3.5 cm width)</param>
        /// <returns>Truncated item name with ellipsis if needed, or centered shorter name</returns>
        private string TruncateItemName(string itemName, int maxLength = 0)
        {
            if (string.IsNullOrEmpty(itemName))
                return "Item Name";

            // Calculate optimal character limit based on label dimensions
            // Label width: 3.5 cm, template width: 70 units
            // Each label takes approximately half the width: 1.75 cm
            // For 70 units width, approximately 19-23 characters fit comfortably
            if (maxLength <= 0)
            {
                maxLength = CalculateOptimalCharacterLimit();
            }

            // Remove any existing ellipsis and trim
            itemName = itemName.Trim();

            if (itemName.Length <= maxLength)
            {
                // Center the text by adding spaces on both sides
                string centeredText = CenterText(itemName, maxLength);
                System.Diagnostics.Debug.WriteLine($"Centering text: '{itemName}' -> '{centeredText}' (length: {centeredText.Length})");
                return centeredText;
            }

            // Truncate and add ellipsis
            return itemName.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Centers text within the specified width by adding spaces
        /// </summary>
        /// <param name="text">Text to center</param>
        /// <param name="maxWidth">Maximum width in characters</param>
        /// <returns>Centered text</returns>
        private string CenterText(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text) || text.Length >= maxWidth)
                return text;

            int spacesNeeded = maxWidth - text.Length;
            int leftSpaces = spacesNeeded / 2;
            int rightSpaces = spacesNeeded - leftSpaces;

            return new string(' ', leftSpaces) + text + new string(' ', rightSpaces);
        }

        /// <summary>
        /// Calculates the optimal character limit for item names based on label dimensions
        /// </summary>
        /// <returns>Optimal character limit</returns>
        private int CalculateOptimalCharacterLimit()
        {
            // Label dimensions: 3.5 cm width x 2.4 cm height
            // Each label is approximately 1.75 cm wide (half of 3.5 cm)
            // The item name field has 70 units width in the template
            // For 3.5 cm width, we can fit more characters than 3.4 cm but less than 3.6 cm
            // Typical font at 8-10pt can fit approximately 19-23 characters in 3.5 cm

            // Calculate based on 3.5 cm width
            // Conservative estimate: 19 characters for 3.5 cm width
            int limit = 19;
            System.Diagnostics.Debug.WriteLine($"Calculated optimal character limit: {limit} for 3.5 cm x 2.4 cm labels");
            return limit;
        }

        private void PrintLabelFile(string filePath, string printerName, int copies)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Label file not found: {filePath}");
                }

                // Read the existing text file content
                string fileContent = File.ReadAllText(filePath);
                byte[] rawData = System.Text.Encoding.ASCII.GetBytes(fileContent);

                System.Diagnostics.Debug.WriteLine($"📄 PRINT JOB START: Reading and printing {copies} copies of file: {filePath} to printer: {printerName}");
                System.Diagnostics.Debug.WriteLine($"File content length: {fileContent.Length} characters, {rawData.Length} bytes");
                System.Diagnostics.Debug.WriteLine($"File content preview: {fileContent.Substring(0, Math.Min(100, fileContent.Length))}...");

                int successfulCopies = 0;

                for (int i = 0; i < copies; i++)
                {
                    System.Diagnostics.Debug.WriteLine($" PRINTING: Copy {i + 1} of {copies} to printer '{printerName}'...");
                    bool success = RawPrinterHelper.SendBytesToPrinter(printerName, rawData, $"BarcodeLabel_Copy_{i + 1}");
                    if (!success)
                    {
                        System.Diagnostics.Debug.WriteLine($" PRINT FAILED: Copy {i + 1} failed to print");
                        throw new Exception($"Failed to print copy {i + 1} to printer {printerName}");
                    }

                    successfulCopies++;
                    System.Diagnostics.Debug.WriteLine($"✅ PRINT SUCCESS: Copy {i + 1} sent successfully ({successfulCopies}/{copies})");

                    // Add a small delay between copies to prevent interference
                    if (i < copies - 1) // Don't delay after the last copy
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }

                // Force printer to process the job immediately - especially important for small jobs
                System.Diagnostics.Debug.WriteLine($" FORCING PRINT FLUSH: Ensuring printer processes {copies} copies immediately");
                System.Threading.Thread.Sleep(300); // Give printer time to process the job

                // Send a small flush command to ensure the printer doesn't buffer the job
                try
                {
                    byte[] flushCommand = System.Text.Encoding.ASCII.GetBytes("\n");
                    RawPrinterHelper.SendBytesToPrinter(printerName, flushCommand, "FlushCommand");
                    System.Diagnostics.Debug.WriteLine($" FLUSH SENT: Forced printer to process buffered jobs");
                }
                catch (Exception flushEx)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ FLUSH WARNING: Could not send flush command: {flushEx.Message}");
                    // Don't throw here, as the main print job might still work
                }

                System.Diagnostics.Debug.WriteLine($"🎉 PRINT JOB COMPLETE: Successfully printed all {successfulCopies} copies (requested: {copies})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PrintLabelFile: {ex.Message}");
                throw;
            }
        }

        private void ConnectBarcodeSearchEvents()
        {
            if (ultraTextEditor1 != null)
            {
                ultraTextEditor1.KeyDown += UltraTextEditor1_KeyDown;

                // Set focus on load
                ultraTextEditor1.Focus();
            }
        }

        private void UltraTextEditor1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string text = ultraTextEditor1.Text.Trim();

                // Check if user is entering a quantity command (e.g., *10)
                if (text.StartsWith("*"))
                {
                    string qtyStr = text.Substring(1);
                    if (int.TryParse(qtyStr, out int newQty) && newQty > 0)
                    {
                        // Update quantity of the active or last item
                        if (ultraGrid1.ActiveRow != null && !ultraGrid1.ActiveRow.IsFilterRow)
                        {
                            ultraGrid1.ActiveRow.Cells["Qty"].Value = newQty;
                            ultraTextEditor1.Text = ""; // Clear input
                        }
                        else if (ultraGrid1.Rows.Count > 0)
                        {
                            // If no row is selected, update the last added item
                            var lastRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                            ultraGrid1.ActiveRow = lastRow;
                            lastRow.Cells["Qty"].Value = newQty;
                            ultraTextEditor1.Text = ""; // Clear input
                        }
                        else
                        {
                            MessageBox.Show("No item selected to update quantity.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid quantity format. Use * followed by a number (e.g., *10).", "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Search for item by barcode
                    SearchItemByBarcode();
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F7)
            {
                // Open the Item Master dialog
                Button5_Click(sender, e);
                e.Handled = true; // Prevent default F7 behavior
            }
            else if (e.KeyCode == Keys.F8)
            {
                // Print labels
                Print_btn_Click(sender, e);
                e.Handled = true;
            }
        }

        private void SearchItemByBarcode()
        {
            try
            {
                if (ultraTextEditor1 == null || string.IsNullOrWhiteSpace(ultraTextEditor1.Text))
                {
                    MessageBox.Show("Please enter a barcode number to search.", "No Barcode Entered",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextEditor1.Focus();
                    return;
                }

                string barcodeToSearch = ultraTextEditor1.Text.Trim();

                // Show searching message
                ultraTextEditor1.Text = "Searching...";
                Application.DoEvents();

                // Clear the text editor after a brief delay
                System.Threading.Thread.Sleep(100);
                ultraTextEditor1.Text = "";

                // Search for item by barcode
                DataBase.Operations = "GETITEMBYBARCODE";
                ItemDDlGrid items = dp.itemDDlGrid(barcodeToSearch, "");

                if (items != null && items.List != null && items.List.Count() > 0)
                {
                    var item = items.List.First();
                    AddItemFromSearch(item);
                }
                else
                {
                    // Try searching by item name/description as fallback
                    DataBase.Operations = "GETITEM";
                    items = dp.itemDDlGrid("", barcodeToSearch);

                    if (items != null && items.List != null && items.List.Count() > 0)
                    {
                        var item = items.List.First();
                        AddItemFromSearch(item);
                    }
                    else
                    {
                        MessageBox.Show($"Item not found with barcode: {barcodeToSearch}\n\nPlease check the barcode or use F7 to open Item Master dialog.",
                            "Item Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ultraTextEditor1.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching for item: {ex.Message}", "Search Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error searching for item: {ex.Message}");
            }
        }

        private void AddItemFromSearch(ItemDDl item)
        {
            try
            {
                // Check if item already exists in grid
                if (IsItemAlreadyInGrid(item.BarCode))
                {
                    DialogResult result = MessageBox.Show(
                        $"Item '{item.Description}' is already in the list.\n\nWould you like to increment the quantity?",
                        "Duplicate Item",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        IncrementItemQuantity(item.BarCode);
                    }
                    return;
                }

                // Convert ItemDDl to Dictionary format similar to frmdialForItemMaster
                selectedItemData = new Dictionary<string, object>
                {
                    ["ItemId"] = item.ItemId,
                    ["BarCode"] = item.BarCode,
                    ["Description"] = item.Description,
                    ["RetailPrice"] = item.RetailPrice,
                    ["Cost"] = item.Cost,
                    ["Unit"] = item.Unit,
                    ["UnitId"] = item.UnitId,
                    ["Packing"] = item.Packing,
                    ["Stock"] = item.Stock
                };

                // Add item to the grid
                AddItemToGrid();

                // Update the UI with selected item information
                UpdateUIWithSelectedItem();

                // Show success message
                MessageBox.Show($"Item '{item.Description}' added successfully!", "Item Added",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item from search: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error adding item from search: {ex.Message}");
            }
        }

        private bool IsItemAlreadyInGrid(string barcode)
        {
            if (selectedItemsTable == null || string.IsNullOrEmpty(barcode)) return false;

            foreach (DataRow row in selectedItemsTable.Rows)
            {
                if (row["Barcode"]?.ToString() == barcode)
                {
                    return true;
                }
            }
            return false;
        }

        private void IncrementItemQuantity(string barcode)
        {
            try
            {
                if (selectedItemsTable == null || string.IsNullOrEmpty(barcode)) return;

                foreach (DataRow row in selectedItemsTable.Rows)
                {
                    if (row["Barcode"]?.ToString() == barcode)
                    {
                        // Get current quantity and increment by 1
                        int currentQty = Convert.ToInt32(row["Qty"]);
                        row["Qty"] = currentQty + 1;

                        // Update UI
                        UpdateUIWithSelectedItem();

                        // Show success message
                        string itemName = row["Description"]?.ToString() ?? "Unknown Item";
                        MessageBox.Show($"Quantity incremented for '{itemName}'. New quantity: {currentQty + 1}",
                            "Quantity Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error incrementing quantity: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error incrementing quantity: {ex.Message}");
            }
        }

        // Footer Panel Methods
        private void InitializeSummaryFooterPanel()
        {
            if (gridFooterPanel == null) return;

            // Set default aggregations for numeric columns
            columnAggregations["Qty"] = "Sum";
            columnAggregations["Price"] = "Sum";

            // Wire up events
            gridFooterPanel.Paint += (s, e) => { AlignSummaryLabels(); };
            gridFooterPanel.Resize += (s, e) => { AlignSummaryLabels(); };
            ultraGrid1.AfterColPosChanged += (s, e) => AlignSummaryLabels();
            ultraGrid1.AfterSortChange += (s, e) => AlignSummaryLabels();
            ultraGrid1.AfterRowFilterChanged += (s, e) => AlignSummaryLabels();
            ultraGrid1.InitializeLayout += (s, e) => AlignSummaryLabels();
            ultraGrid1.SizeChanged += (s, e) => AlignSummaryLabels();

            // Create context menu for footer panel
            var panelMenu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type, null, OnPanelSummaryTypeSelected) { Tag = type };
                panelMenu.Items.Add(item);
            }
            gridFooterPanel.ClientArea.ContextMenuStrip = panelMenu;

            // Right-click to show menu
            gridFooterPanel.ClientArea.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var ctrl = gridFooterPanel.ClientArea.GetChildAtPoint(e.Location);
                    if (ctrl == null || !(ctrl is Label))
                        panelMenu.Show(gridFooterPanel.ClientArea, e.Location);
                }
            };
        }

        private void OnPanelSummaryTypeSelected(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string type)
            {
                currentSummaryType = type;
                // Set all visible numeric columns to this type
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (var col in ultraGrid1.DisplayLayout.Bands[0].Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
                    {
                        if (!col.Hidden && IsNumericColumn(col))
                            columnAggregations[col.Key] = type;
                    }
                }
                UpdateSummaryFooter();
            }
        }

        private void UpdateSummaryFooter()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null || ultraGrid1 == null ||
                ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            gridFooterPanel.ClientArea.SuspendLayout();
            gridFooterPanel.ClientArea.Controls.Clear();
            summaryLabels.Clear();

            var band = ultraGrid1.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!IsNumericColumn(col)) continue;
                if (!columnAggregations.ContainsKey(col.Key) || columnAggregations[col.Key] == "None") continue;

                string agg = columnAggregations[col.Key];
                var lbl = new Label
                {
                    Name = $"lblSummary_{col.Key}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Height = gridFooterPanel.Height - 4,
                    ContextMenuStrip = CreateFooterLabelMenu(col.Key)
                };

                summaryLabels[col.Key] = lbl;
                gridFooterPanel.ClientArea.Controls.Add(lbl);
            }

            gridFooterPanel.ClientArea.ResumeLayout();
            UpdateFooterValues();
            AlignSummaryLabels();
        }

        private ContextMenuStrip CreateFooterLabelMenu(string columnKey)
        {
            var menu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type)
                {
                    Tag = type
                };
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }

            // Highlight current aggregation
            menu.Opening += (s, e) =>
            {
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = columnAggregations.ContainsKey(columnKey) &&
                                  columnAggregations[columnKey] == (string)item.Tag;
                }
            };
            return menu;
        }

        private void UpdateFooterValues()
        {
            if (ultraGrid1 == null || ultraGrid1.DataSource == null) return;

            if (ultraGrid1.DataSource is DataTable dt)
            {
                foreach (var kvp in summaryLabels)
                {
                    string colKey = kvp.Key;
                    Label lbl = kvp.Value;

                    string agg = columnAggregations.ContainsKey(colKey) ? columnAggregations[colKey] : "None";
                    var values = dt.AsEnumerable()
                        .Where(r => r[colKey] != DBNull.Value)
                        .Select(r => Convert.ToDouble(r[colKey]))
                        .ToList();

                    string text = "";
                    switch (agg)
                    {
                        case "Sum":
                            text = values.Count > 0 ? values.Sum().ToString("N2") : "0.00";
                            break;
                        case "Min":
                            text = values.Count > 0 ? values.Min().ToString("N2") : "-";
                            break;
                        case "Max":
                            text = values.Count > 0 ? values.Max().ToString("N2") : "-";
                            break;
                        case "Average":
                            text = values.Count > 0 ? values.Average().ToString("N2") : "-";
                            break;
                        case "Count":
                            text = values.Count.ToString();
                            break;
                    }
                    lbl.Text = text;
                }
            }
        }

        private void AlignSummaryLabels()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null || ultraGrid1 == null ||
                ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            var band = ultraGrid1.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!summaryLabels.TryGetValue(col.Key, out var lbl)) continue;

                var headerUI = ultraGrid1.DisplayLayout.Bands[0].Columns[col.Key].Header?.GetUIElement();
                if (headerUI != null)
                {
                    var gridPoint = ultraGrid1.PointToScreen(System.Drawing.Point.Empty);
                    var headerPoint = headerUI.Control.PointToScreen(headerUI.Rect.Location);
                    int colLeft = headerPoint.X - gridFooterPanel.PointToScreen(System.Drawing.Point.Empty).X;
                    int colWidth = headerUI.Rect.Width;

                    lbl.Left = colLeft;
                    lbl.Width = colWidth;
                    lbl.Top = 2;
                    lbl.Height = gridFooterPanel.Height - 4;
                }
            }
        }

        private bool IsNumericColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn col)
        {
            var t = col.DataType;
            return t == typeof(int) || t == typeof(float) || t == typeof(double) ||
                   t == typeof(decimal) || t == typeof(long) || t == typeof(short);
        }

        private void RefreshSummaryFooter()
        {
            UpdateFooterValues();
            AlignSummaryLabels();
        }
    }
}

