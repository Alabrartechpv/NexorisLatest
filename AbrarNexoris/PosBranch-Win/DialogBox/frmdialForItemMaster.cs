using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Master;
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
using PosBranch_Win.Transaction;
using System.Data.SqlClient;

namespace PosBranch_Win.DialogBox
{
    public partial class frmdialForItemMaster : Form
    {
        Dropdowns dp = new Dropdowns();
        ItemMasterRepository ItemREpo = new ItemMasterRepository();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        BaseRepostitory con = new BaseRepostitory();

        string FormName;
        private DataTable fullDataTable = null;

        // Add a status label for debugging
        private Label lblStatus;

        // Add a class-level variable to track the original order
        private bool isOriginalOrder = true;

        // Add a class-level variable to store the column chooser form
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;

        // Add this dictionary as a class-level field to store column widths
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();

        // Add this property to allow the parent form to specify the price level
        public string SelectedPriceLevel { get; set; } = "RetailPrice";

        // Add this property to allow the parent form to specify the TaxType filter
        public string TaxTypeFilter { get; set; } = "ALL";

        // Properties to expose selected item details to parent dialogs
        public string SelectedItemName { get; private set; }
        public string SelectedBarcode { get; private set; }
        public string SelectedItemNo { get; private set; }
        public long SelectedItemId { get; private set; }

        public void CaptureSelectedItemData()
        {
            try
            {
                if (ultraGrid1 != null && ultraGrid1.ActiveRow != null)
                {
                    SelectedItemName = ultraGrid1.ActiveRow.Cells.Exists("Description") ? Convert.ToString(ultraGrid1.ActiveRow.Cells["Description"].Value) : string.Empty;
                    SelectedBarcode = ultraGrid1.ActiveRow.Cells.Exists("BarCode") ? Convert.ToString(ultraGrid1.ActiveRow.Cells["BarCode"].Value) : string.Empty;
                    SelectedItemNo = ultraGrid1.ActiveRow.Cells.Exists("ItemNo") ? Convert.ToString(ultraGrid1.ActiveRow.Cells["ItemNo"].Value) : string.Empty;
                    SelectedItemId = ultraGrid1.ActiveRow.Cells.Exists("ItemId") ? Convert.ToInt64(ultraGrid1.ActiveRow.Cells["ItemId"].Value ?? 0) : 0;
                    this.Tag = !string.IsNullOrWhiteSpace(SelectedItemName) ? SelectedItemName : SelectedBarcode;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CaptureSelectedItemData error: " + ex.Message);
            }
        }

        private void InitializeStatusLabel()
        {
            // Create a status label at the bottom of the form
            lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.AutoSize = false;
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 30;
            lblStatus.BackColor = Color.LightYellow;
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Text = "Ready";
            this.Controls.Add(lblStatus);
        }

        // Update status without interrupting the user
        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.Update();
            }
        }

        public frmdialForItemMaster(string Params)
        {
            InitializeComponent();
            FormName = Params;

            // Add debug logging
            System.Diagnostics.Debug.WriteLine($"frmdialForItemMaster initialized with FormName: '{FormName}'");

            // PERFORMANCE: Suspend all layout operations during initialization
            this.SuspendLayout();
            ultraGrid1.SuspendLayout();

            // Initialize status label
            InitializeStatusLabel();

            // Set up the grid style (deferred to reduce initial load time)
            SetupUltraGridStyle();

            // Set up the panel styles (lightweight operation)
            SetupUltraPanelStyle();

            // Initialize the search filter comboBox
            InitializeSearchFilterComboBox();

            // Initialize the column order comboBox
            InitializeColumnOrderComboBox();

            // Ensure search controls are properly initialized
            EnsureSearchControlsInitialized();

            // PERFORMANCE: Defer non-critical UI setup
            // These will be set up after the form is shown
            this.Load += (s, e) =>
            {
                // Set up panel hover effects after form is loaded
                SetupPanelHoverEffects();
            };

            // Use Shown event to set focus on textBoxsearch (more reliable than Load event)
            this.Shown += (s, e) =>
            {
                // Set focus to the search textbox when form is fully displayed
                if (textBoxsearch != null)
                {
                    textBoxsearch.Focus();
                    textBoxsearch.SelectAll();
                }
            };

            // Connect click events for opening ItemMaster form
            ConnectItemMasterClickEvents();

            // Set up the column chooser right-click menu
            SetupColumnChooserMenu();

            // Register key event handlers
            ultraGrid1.KeyPress += ultraGrid1_KeyPress;
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;

            // Add form closing event to ensure column chooser is closed when main form closes
            this.FormClosing += FrmDialForItemMaster_FormClosing;

            // Register events to preserve column widths
            ultraGrid1.AfterRowsDeleted += UltraGrid1_AfterRowsDeleted;
            ultraGrid1.AfterSortChange += UltraGrid1_AfterSortChange;
            ultraGrid1.AfterColPosChanged += UltraGrid1_AfterColPosChanged;

            // Add handler for form resize to preserve column widths
            this.SizeChanged += FrmDialForItemMaster_SizeChanged;
            ultraGrid1.Resize += UltraGrid1_Resize;

            // Initialize tooltip
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            // PERFORMANCE: Resume layout operations
            ultraGrid1.ResumeLayout(false);
            this.ResumeLayout(false);

            // Update status
            UpdateStatus("Form initialized");
        }

        // Add these event handlers to preserve column widths
        private void UltraGrid1_AfterRowsDeleted(object sender, EventArgs e)
        {
            PreserveColumnWidths();
        }

        private void UltraGrid1_AfterSortChange(object sender, BandEventArgs e)
        {
            PreserveColumnWidths();
        }

        private void UltraGrid1_AfterColPosChanged(object sender, AfterColPosChangedEventArgs e)
        {
            // If the user manually resized a column, update our saved widths
            if (e.PosChanged == PosChanged.Sized)
            {
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        savedColumnWidths[col.Key] = col.Width;
                    }
                }
            }
            else
            {
                PreserveColumnWidths();
            }
        }

        // Helper method to preserve column widths
        private void PreserveColumnWidths()
        {
            try
            {
                // Temporarily disable layout updates
                ultraGrid1.SuspendLayout();

                // Restore column widths from saved values
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }
                }

                // Resume layout updates
                ultraGrid1.ResumeLayout();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error preserving column widths: {ex.Message}");
            }
        }

        private void FrmDialForItemMaster_SizeChanged(object sender, EventArgs e)
        {
            // Preserve column widths when form is resized
            PreserveColumnWidths();
        }

        private void UltraGrid1_Resize(object sender, EventArgs e)
        {
            // Preserve column widths when grid is resized
            PreserveColumnWidths();
        }

        private void FrmDialForItemMaster_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close the column chooser form if it exists
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            // Add key handling as needed
        }

        // Add this method to set up the column chooser right-click menu
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

        // Set up direct header drag and drop functionality
        private void SetupDirectHeaderDragDrop()
        {
            // Enable the grid as a drop target
            ultraGrid1.AllowDrop = true;

            // Add drag event handlers for headers
            ultraGrid1.MouseDown += new MouseEventHandler(UltraGrid1_MouseDown);
            ultraGrid1.MouseMove += new MouseEventHandler(UltraGrid1_MouseMove);
            ultraGrid1.MouseUp += new MouseEventHandler(UltraGrid1_MouseUp);
            ultraGrid1.DragOver += new DragEventHandler(UltraGrid1_DragOver);
            ultraGrid1.DragDrop += new DragEventHandler(UltraGrid1_DragDrop);

            // Create column chooser form but don't show it yet
            CreateColumnChooserForm();
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

            // Add double-click handler to restore column to the grid
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

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

        // Method to position the column chooser at the bottom right of the form
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

        // Track mouse position and column for drag
        private Point startPoint;
        private UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip(); // ToolTip control for feedback

        // Handle mouse down on grid to initiate drag
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
                    if (ultraGrid1.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
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
                UpdateStatus($"Error in mouse down: {ex.Message}");
                isDraggingColumn = false;
                columnToMove = null;
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

                    // Make sure the column chooser exists but don't show it automatically
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
                                toolTip.SetToolTip(ultraGrid1, "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in mouse move: {ex.Message}");
                ultraGrid1.Cursor = Cursors.Default;
                toolTip.SetToolTip(ultraGrid1, "");
            }
        }

        // Event handler for mouse up on the grid
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset cursor
                ultraGrid1.Cursor = Cursors.Default;
                toolTip.SetToolTip(ultraGrid1, "");

                // Reset drag state
                isDraggingColumn = false;
                columnToMove = null;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in mouse up: {ex.Message}");
            }
        }

        // Event handler for the column chooser menu item
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        // Method to show the column chooser form
        private void ShowColumnChooser()
        {
            // If the column chooser form already exists, just show it
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                PopulateColumnChooserListBox();
                columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }

            // Create the column chooser form
            CreateColumnChooserForm();

            // Show the form
            columnChooserForm.Show(this);

            // Position the column chooser at the bottom right of the form
            PositionColumnChooserAtBottomRight();

            // Add event handler to reposition the column chooser when the main form moves or resizes
            this.LocationChanged += (s, evt) => PositionColumnChooserAtBottomRight();
            this.SizeChanged += (s, evt) => PositionColumnChooserAtBottomRight();

            // Also reposition when the form is activated to ensure it stays in the correct position
            this.Activated += (s, evt) => PositionColumnChooserAtBottomRight();
        }

        // Method to highlight the column chooser during drag operations
        private void HighlightColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed) return;

            // Store original back color
            Color originalColor = columnChooserForm.BackColor;

            // Flash the form by changing its background color
            columnChooserForm.BackColor = Color.LightBlue;

            // Create a timer to reset the color after a delay
            Timer flashTimer = new Timer();
            flashTimer.Interval = 300;
            flashTimer.Tick += (s, e) =>
            {
                columnChooserForm.BackColor = originalColor;
                flashTimer.Stop();
                flashTimer.Dispose();
            };
            flashTimer.Start();
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

            // Update status
            UpdateStatus("Column customization closed");
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

        // Method to populate the column chooser listbox with hidden columns
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;

            // Clear existing items
            columnChooserListBox.Items.Clear();

            // Define the standard columns that should be tracked in the column chooser
            string[] standardColumns = new string[] { "BarCode", "Description", "UnitId", "Unit", "Cost", "RetailPrice", "Stock" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
                {
                    { "BarCode", "Barcode" },
                    { "Description", "Item Name" },
                    { "UnitId", "UnitId" },
                    { "Unit", "Unit" },
                    { "Cost", "Cost" },
                    { "RetailPrice", "Price" },
                    { "Stock", "Stock" }
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

        // Event handler for mouse down in the column chooser listbox
        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            // Start drag from the listbox
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                ColumnItem item = columnChooserListBox.Items[index] as ColumnItem;
                if (item != null)
                {
                    // Select the item being dragged
                    columnChooserListBox.SelectedIndex = index;

                    // Start drag operation using a delimited string to avoid serialization issues
                    string dragData = item.ColumnKey + "|" + item.DisplayText;
                    columnChooserListBox.DoDragDrop(dragData, DragDropEffects.Move);
                }
            }
        }

        // Event handler for double-click in the column chooser listbox - restores column to grid
        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnItem item)
            {
                // Find the column in the grid
                if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                    ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];

                    // Temporarily disable layout updates
                    ultraGrid1.SuspendLayout();

                    // Show the column
                    column.Hidden = false;

                    // Remove from listbox
                    columnChooserListBox.Items.Remove(item);

                    // Resume layout updates
                    ultraGrid1.ResumeLayout();
                }
            }
        }

        // Event handler for drag over in the column chooser listbox
        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            // Allow dropping columns into the listbox
            if (e.Data.GetDataPresent(typeof(UltraGridColumn)))
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
            // Handle dropping a column into the listbox
            if (e.Data.GetDataPresent(typeof(UltraGridColumn)))
            {
                UltraGridColumn column = (UltraGridColumn)e.Data.GetData(typeof(UltraGridColumn));
                if (column != null && !column.Hidden)
                {
                    // Get column name
                    string columnName = !string.IsNullOrEmpty(column.Header.Caption) ?
                                    column.Header.Caption : column.Key;

                    // Hide the column
                    column.Hidden = true;

                    // Add to listbox
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                }
            }
        }

        // Event handler for drag over in the grid
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            // Allow dropping string data onto the grid
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
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
            // Handle dropping a column item onto the grid
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dragData = (string)e.Data.GetData(DataFormats.StringFormat);
                if (!string.IsNullOrEmpty(dragData) && dragData.Contains("|"))
                {
                    string[] parts = dragData.Split('|');
                    string columnKey = parts[0];
                    string displayText = parts.Length > 1 ? parts[1] : columnKey;
                    
                    // Find the column item in the listbox to remove it
                    ColumnItem itemToRemove = null;
                    foreach (ColumnItem itm in columnChooserListBox.Items)
                    {
                        if (itm.ColumnKey == columnKey)
                        {
                            itemToRemove = itm;
                            break;
                        }
                    }

                    // Find the column in the grid
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnKey))
                    {
                        UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey];

                        // Temporarily disable layout updates
                        ultraGrid1.SuspendLayout();

                        // Show the column
                        column.Hidden = false;

                        // Remove from listbox
                        if (itemToRemove != null)
                        {
                            columnChooserListBox.Items.Remove(itemToRemove);
                        }

                        // Resume layout updates
                        ultraGrid1.ResumeLayout();

                        // Show feedback
                        toolTip.Show($"Column '{displayText}' restored",
                                ultraGrid1,
                                ultraGrid1.PointToClient(Control.MousePosition),
                                2000);
                    }
                }
            }
        }

        private void SetupUltraPanelStyle()
        {
            // First apply the base styling to all panels
            StyleIconPanel(ultraPanel3);
            StyleIconPanel(ultraPanel4);
            StyleIconPanel(ultraPanel5);
            StyleIconPanel(ultraPanel6);
            StyleIconPanel(ultraPanel7);

            // Now explicitly set ultraPanel3 and ultraPanel7 to match ultraPanel4's colors
            // This ensures they have exactly the same appearance
            ultraPanel3.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel3.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel3.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel3.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            ultraPanel7.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel7.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel7.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel7.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            // Set appearance for the main panel - removed curved border
            ultPanelPurchaseDisplay.Appearance.BackColor = Color.White;
            ultPanelPurchaseDisplay.Appearance.BackColor2 = Color.FromArgb(200, 230, 250);
            ultPanelPurchaseDisplay.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultPanelPurchaseDisplay.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
        }

        private void StyleGlassPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Create a glass-like gradient with the exact colors specified
            panel.Appearance.BackColor = Color.FromArgb(0, 174, 239);  // Bright blue (0, 174, 239)
            panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217); // Darker blue (0, 116, 217)
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.GlassTop50;

            // Set highly rounded border style for very curved edges
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;

            // Add subtle border
            panel.Appearance.BorderColor = Color.FromArgb(150, 200, 240);

            // Add glass-like reflection effect
            panel.Appearance.BackHatchStyle = Infragistics.Win.BackHatchStyle.None;
            panel.Appearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

            // Ensure text is visible on the gradient background
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Label)
                {
                    ((Label)control).ForeColor = Color.White;
                    ((Label)control).Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    ((Label)control).BackColor = Color.Transparent;
                }
                else if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    // Make sure the pictureboxes look good
                    ((Infragistics.Win.UltraWinEditors.UltraPictureBox)control).BackColor = Color.Transparent;
                    ((Infragistics.Win.UltraWinEditors.UltraPictureBox)control).BackColorInternal = Color.Transparent;
                }
            }

            // Add hover effect
            panel.ClientArea.MouseEnter += (sender, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(20, 194, 255); // Slightly brighter
                panel.Appearance.BackColor2 = Color.FromArgb(10, 136, 237); // Slightly brighter
            };

            panel.ClientArea.MouseLeave += (sender, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(0, 174, 239);
                panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217);
            };

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
        }

        private void StyleActionPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Create a smooth gradient from bright to dark blue to match second image
            panel.Appearance.BackColor = Color.FromArgb(60, 180, 240);  // Bright blue
            panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217); // Darker blue
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Set highly rounded border style for very curved edges
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;

            // Add shadow/3D effect
            panel.Appearance.BorderColor = Color.FromArgb(0, 150, 220);
            panel.Appearance.BorderColor3DBase = Color.FromArgb(0, 100, 180);

            // Ensure text is visible on the gradient background
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Label)
                {
                    ((Label)control).ForeColor = Color.White;
                    ((Label)control).Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    ((Label)control).BackColor = Color.Transparent;
                }
                else if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    // Make sure the pictureboxes look good
                    ((Infragistics.Win.UltraWinEditors.UltraPictureBox)control).BackColor = Color.Transparent;
                    ((Infragistics.Win.UltraWinEditors.UltraPictureBox)control).BackColorInternal = Color.Transparent;
                }
            }

            // Add hover effect
            panel.ClientArea.MouseEnter += (sender, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(80, 200, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(0, 130, 230);
            };

            panel.ClientArea.MouseLeave += (sender, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(60, 180, 240);
                panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217);
            };

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
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

        private void SetupUltraGridStyle()
        {
            try
            {
                // Reset everything first to ensure clean slate
                ultraGrid1.DisplayLayout.Reset();

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;

                // Enable column moving and dragging
                ultraGrid1.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;

                // Important: This setting ensures we get only row selection on click, not automatic action
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;

                // Hide the group-by area (gray bar)
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Set rounded borders for the entire grid
                ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

                // Configure grid lines - single line borders for rows and columns
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;

                // Set border width to single line
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;

                // Remove cell padding/spacing
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;

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

                // Add header styling - blue headers
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Configure row selector appearance with blue
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None; // Remove numbers
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

                // Set these properties to completely clean the row headers - remove all indicators
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.Image = null;

                // Set all cells to have white background (no alternate row coloring)
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Configure selected row appearance with light blue highlight - UPDATED to match FrmPurchaseDisplayDialog.cs
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue from the image
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast

                // Configure active row appearance - make it same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Set font size for all cells to match the image (standard text size)
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Configure scrollbar style
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;

                // Configure the scrollbar look
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    // Configure button appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                    // Configure track appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                    // Configure thumb appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Configure cell appearance to increase vertical content alignment
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

                // Connect events
                ultraGrid1.DoubleClickCell += new Infragistics.Win.UltraWinGrid.DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);
                ultraGrid1.ClickCell += new Infragistics.Win.UltraWinGrid.ClickCellEventHandler(ultraGrid1_ClickCell);

                // Auto-fit settings - DISABLE to prevent column resizing during filtering
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;

                // Disable automatic column sizing
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.None;

                // IMPORTANT: Add code to ensure only the specified columns are visible
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    // First, hide all columns
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        col.Hidden = true;
                    }

                    // Define the columns to show in the specified order
                    List<string> columnsList = new List<string> { "BarCode", "Description", "UnitId", "Unit", "Cost", "RetailPrice", "Stock" };
                    if (FormName == "frmSalesInvoice")
                    {
                        columnsList.Remove("Cost");
                    }
                    string[] columnsToShow = columnsList.ToArray();

                    // Show and configure only the specified columns in the specified order
                    for (int i = 0; i < columnsToShow.Length; i++)
                    {
                        string colKey = columnsToShow[i];

                        if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey))
                        {
                            Infragistics.Win.UltraWinGrid.UltraGridColumn col = ultraGrid1.DisplayLayout.Bands[0].Columns[colKey];
                            col.Hidden = false;
                            col.Header.VisiblePosition = i; // Set the order

                            // Configure specific columns
                            switch (colKey)
                            {
                                case "BarCode":
                                    col.Header.Caption = "Barcode";
                                    col.Width = 120;
                                    break;
                                case "Description":
                                    col.Header.Caption = "Item Name";
                                    col.Width = 250;
                                    break;
                                case "UnitId":
                                    col.Header.Caption = "UnitId";
                                    col.Width = 80;
                                    break;
                                case "Unit":
                                    col.Header.Caption = "Unit";
                                    col.Width = 80;
                                    break;
                                case "Cost":
                                    col.Header.Caption = "Cost";
                                    col.Width = 100;
                                    col.Format = "N2";
                                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                    break;
                                case "RetailPrice":
                                    col.Header.Caption = "Price";
                                    col.Width = 100;
                                    col.Format = "N2";
                                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                    break;
                                case "Stock":
                                    col.Header.Caption = "Stock";
                                    col.Width = 80;
                                    col.Format = "N2";
                                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                    break;
                            }
                        }
                    }
                }

                // Enable multi-select for stock transfer and stock adjustment to allow bulk adding
                if (FormName == "FrmStockTransfer" || FormName == "FrmStockAdjustment")
                {
                    ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Extended;
                }

                // Refresh the grid to apply all changes
                ultraGrid1.Refresh();

                // In SetupUltraGridStyle method - add these lines to disable filter indicators
                ultraGrid1.DisplayLayout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }

        private void frmdialForItemMaster_Load(object sender, EventArgs e)
        {
            try
            {
                // Update status
                UpdateStatus("Form loading...");

                // Verify textbox exists and is properly connected
                if (textBoxsearch == null)
                {
                    UpdateStatus("ERROR: textBox1 is null! Creating a new one.");
                    EnsureSearchControlsInitialized();
                }
                else
                {
                    // Make sure events are connected
                    textBoxsearch.TextChanged -= textBox1_TextChanged; // Remove to avoid duplicates
                    textBoxsearch.TextChanged += textBox1_TextChanged; // Add it back
                }

                // Initialize label1 with record count info if it exists
                if (this.Controls.Find("label1", true).Length > 0)
                {
                    Label label1 = this.Controls.Find("label1", true)[0] as Label;
                    if (label1 != null)
                    {
                        // Set initial text with "Loading..."
                        label1.Text = "Loading...";
                        label1.AutoSize = true;
                        label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                        label1.ForeColor = Color.FromArgb(0, 70, 170); // Dark blue color
                    }
                }

                // Note: Focus is now set in the Shown event for more reliable behavior

                // PERFORMANCE: Load data asynchronously to avoid UI freeze
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        // Open connection on background thread
                        con.DataConnection.Open();

                        // Load all data on background thread
                        LoadAllDataAsync();
                    }
                    catch (Exception ex)
                    {
                        // Handle errors on UI thread
                        this.BeginInvoke(new Action(() =>
                        {
                            UpdateStatus($"Error loading data: {ex.Message}");
                            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                    finally
                    {
                        // Close connection on background thread
                        if (con.DataConnection.State == ConnectionState.Open)
                            con.DataConnection.Close();
                    }
                });

                // Update status
                UpdateStatus("Form ready - loading data in background...");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading form: {ex.Message}");
            }
        }

        // New async method to load data without blocking UI
        private void LoadAllDataAsync()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemDetalisDDL, (SqlConnection)con.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@ItemName", "");
                    cmd.Parameters.AddWithValue("@Operation", "GETALL");

                    // Load all data into our cached DataTable
                    DataTable tempDataTable = new DataTable();
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(tempDataTable);
                    }

                    // Sort the data to show latest items first (on background thread)
                    SortDataByLatestFirstAsync(tempDataTable);

                    // Apply TaxType filter if specified (on background thread)
                    ApplyTaxTypeFilter(tempDataTable);

                    // Apply item status columns and transaction-specific status rules
                    ApplyItemStatusContextFilter(tempDataTable);

                    // Add a column to preserve the original row order
                    PreserveOriginalRowOrder(tempDataTable);

                    // Update UI on the UI thread
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // PERFORMANCE: Suspend layout during data binding
                            ultraGrid1.SuspendLayout();

                            // Assign the loaded data
                            fullDataTable = tempDataTable;

                            // Set the initial data source for the grid
                            ultraGrid1.DataSource = fullDataTable;

                            // Show row count in status or title with TaxType filter info
                            string taxTypeInfo = "";
                            if (!string.IsNullOrEmpty(TaxTypeFilter) && TaxTypeFilter.ToUpper() != "ALL")
                            {
                                taxTypeInfo = $" - TaxType: {TaxTypeFilter.ToUpper()}";
                            }
                            this.Text = $"Item Master - {fullDataTable.Rows.Count} items loaded (Latest first){taxTypeInfo}";

                            // Update textBox3 with the total item count
                            if (textBox3 != null)
                            {
                                textBox3.Text = fullDataTable.Rows.Count.ToString();
                            }

                            // Initialize saved column widths
                            InitializeSavedColumnWidths();

                            // Update the record count label
                            UpdateRecordCountLabel();

                            // Apply initial filtering (empty search = show all)
                            ApplyFilter(string.Empty);

                            // PERFORMANCE: Resume layout
                            ultraGrid1.ResumeLayout();

                            // Update status with TaxType filter info
                            string statusMsg = "Data loaded successfully";
                            if (!string.IsNullOrEmpty(TaxTypeFilter) && TaxTypeFilter.ToUpper() != "ALL")
                            {
                                statusMsg += $" - Filtered by TaxType: {TaxTypeFilter.ToUpper()}";
                            }
                            UpdateStatus(statusMsg);
                        }
                        catch (Exception ex)
                        {
                            UpdateStatus($"Error updating UI: {ex.Message}");
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                // Handle errors on UI thread
                this.BeginInvoke(new Action(() =>
                {
                    UpdateStatus($"Error loading data: {ex.Message}");
                    MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        // Async version of SortDataByLatestFirst
        private void SortDataByLatestFirstAsync(DataTable dataTable)
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count == 0)
                    return;

                // Create a DataView to sort the data
                DataView dataView = dataTable.DefaultView;

                // Try to sort by different possible date/ID columns in descending order
                string sortColumn = "";

                if (dataTable.Columns.Contains("CreatedDate"))
                    sortColumn = "CreatedDate DESC";
                else if (dataTable.Columns.Contains("ModifiedDate"))
                    sortColumn = "ModifiedDate DESC";
                else if (dataTable.Columns.Contains("CreatedDateTime"))
                    sortColumn = "CreatedDateTime DESC";
                else if (dataTable.Columns.Contains("ModifiedDateTime"))
                    sortColumn = "ModifiedDateTime DESC";
                else if (dataTable.Columns.Contains("ItemId"))
                    sortColumn = "ItemId DESC";
                else if (dataTable.Columns.Contains("Id"))
                    sortColumn = "Id DESC";

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    dataView.Sort = sortColumn;
                    // Create a new DataTable with the sorted data
                    DataTable sortedTable = dataView.ToTable();

                    // Copy sorted data back to original table
                    dataTable.Rows.Clear();
                    foreach (DataRow row in sortedTable.Rows)
                    {
                        dataTable.ImportRow(row);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sorting data: {ex.Message}");
            }
        }

        // Method to apply TaxType filter to the DataTable
        private void ApplyTaxTypeFilter(DataTable dataTable)
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count == 0)
                    return;

                // If TaxTypeFilter is "ALL", don't filter - show all items
                if (string.IsNullOrEmpty(TaxTypeFilter) || TaxTypeFilter.ToUpper() == "ALL")
                    return;

                // Check if TaxType column exists
                if (!dataTable.Columns.Contains("TaxType"))
                    return;

                // Normalize the filter value (handle variations like "incl", "in", "inclusive", etc.)
                string normalizedFilter = NormalizeTaxTypeFilter(TaxTypeFilter);

                // Create a list of rows to remove
                List<DataRow> rowsToRemove = new List<DataRow>();

                // Filter rows based on TaxType
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row["TaxType"] == DBNull.Value || row["TaxType"] == null)
                    {
                        // Remove rows with null/empty TaxType if filter is specific
                        rowsToRemove.Add(row);
                        continue;
                    }

                    string rowTaxType = NormalizeTaxTypeFilter(row["TaxType"].ToString());

                    // If the normalized TaxType doesn't match the filter, remove the row
                    if (rowTaxType != normalizedFilter)
                    {
                        rowsToRemove.Add(row);
                    }
                }

                // Remove filtered rows
                foreach (DataRow row in rowsToRemove)
                {
                    dataTable.Rows.Remove(row);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying TaxType filter: {ex.Message}");
            }
        }

        // Helper method to normalize TaxType filter values
        private string NormalizeTaxTypeFilter(string taxType)
        {
            if (string.IsNullOrEmpty(taxType))
                return string.Empty;

            // Normalize to lowercase for comparison
            string normalized = taxType.Trim().ToLower();

            // Convert variations to standard forms
            if (normalized == "in" || normalized == "i" || normalized == "inclusive")
                return "incl";
            else if (normalized == "ex" || normalized == "e" || normalized == "exclusive")
                return "excl";
            else if (normalized == "incl")
                return "incl";
            else if (normalized == "excl")
                return "excl";

            // Return as-is if no match
            return normalized;
        }

        private bool IsSalesInvoiceDialog()
        {
            return string.Equals(FormName, "frmSalesInvoice", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPurchaseDialog()
        {
            return string.Equals(FormName, "FromPurchase", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsStatusBlockedForCurrentDialog(DataRow row)
        {
            if (row == null)
            {
                return false;
            }

            bool blockSale = row.Table.Columns.Contains("BlockSale") &&
                             row["BlockSale"] != DBNull.Value &&
                             Convert.ToBoolean(row["BlockSale"]);
            bool blockPurchase = row.Table.Columns.Contains("BlockPurchase") &&
                                 row["BlockPurchase"] != DBNull.Value &&
                                 Convert.ToBoolean(row["BlockPurchase"]);

            return (IsSalesInvoiceDialog() && blockSale) || (IsPurchaseDialog() && blockPurchase);
        }

        private void ApplyItemStatusContextFilter(DataTable dataTable)
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    return;
                }

                Dropdowns statusDropdown = new Dropdowns();
                statusDropdown.ApplyItemStatuses(dataTable);

                if (!IsSalesInvoiceDialog() && !IsPurchaseDialog())
                {
                    return;
                }

                List<DataRow> rowsToRemove = new List<DataRow>();
                foreach (DataRow row in dataTable.Rows)
                {
                    if (IsStatusBlockedForCurrentDialog(row))
                    {
                        rowsToRemove.Add(row);
                    }
                }

                foreach (DataRow row in rowsToRemove)
                {
                    dataTable.Rows.Remove(row);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error applying item status filter: {ex.Message}");
            }
        }

        private bool EnsureSelectedRowAllowedForCurrentDialog(UltraGridRow row)
        {
            if (row == null)
            {
                return false;
            }

            bool blockSale = row.Cells.Exists("BlockSale") &&
                             row.Cells["BlockSale"].Value != null &&
                             row.Cells["BlockSale"].Value != DBNull.Value &&
                             Convert.ToBoolean(row.Cells["BlockSale"].Value);
            bool blockPurchase = row.Cells.Exists("BlockPurchase") &&
                                 row.Cells["BlockPurchase"].Value != null &&
                                 row.Cells["BlockPurchase"].Value != DBNull.Value &&
                                 Convert.ToBoolean(row.Cells["BlockPurchase"].Value);

            bool isBlocked = (IsSalesInvoiceDialog() && blockSale) || (IsPurchaseDialog() && blockPurchase);
            if (!isBlocked)
            {
                return true;
            }

            string itemName = row.Cells.Exists("Description") ? row.Cells["Description"].Value?.ToString() ?? "Item" : "Item";
            string statusName = row.Cells.Exists("ItemStatus")
                ? Dropdowns.NormalizeItemStatusName(row.Cells["ItemStatus"].Value?.ToString())
                : "Active";
            string reason = row.Cells.Exists("StatusReason") ? row.Cells["StatusReason"].Value?.ToString() ?? string.Empty : string.Empty;
            string actionName = IsSalesInvoiceDialog() ? "sale" : "purchase";
            string message = $"{itemName} is marked as '{statusName}' and is blocked for {actionName}.";

            if (!string.IsNullOrWhiteSpace(reason))
            {
                message += $"\n\nReason: {reason}";
            }

            MessageBox.Show(message, "Blocked Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // Add this method to initialize saved column widths
        private void InitializeSavedColumnWidths()
        {
            savedColumnWidths.Clear();

            // Save the initial width of all columns
            foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (!column.Hidden)
                {
                    savedColumnWidths[column.Key] = column.Width;
                }
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
                UpdateStatus($"Error preserving row order: {ex.Message}");
            }
        }

        private void LoadAllData()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemDetalisDDL, (SqlConnection)con.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@ItemName", "");
                    cmd.Parameters.AddWithValue("@Operation", "GETALL");

                    // Load all data into our cached DataTable
                    fullDataTable = new DataTable();
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(fullDataTable);
                    }

                    // Sort the data to show latest items first
                    SortDataByLatestFirst();

                    // Apply item status columns and transaction-specific status rules
                    ApplyItemStatusContextFilter(fullDataTable);

                    // Add a column to preserve the original row order
                    PreserveOriginalRowOrder(fullDataTable);

                    // Verify data was loaded
                    if (fullDataTable != null && fullDataTable.Rows.Count > 0)
                    {
                        // Set the initial data source for the grid
                        ultraGrid1.DataSource = fullDataTable;

                        // Show row count in status or title
                        this.Text = $"Item Master - {fullDataTable.Rows.Count} items loaded (Latest first)";

                        // Update textBox3 with the total item count
                        if (textBox3 != null)
                        {
                            textBox3.Text = fullDataTable.Rows.Count.ToString();
                        }

                        // Update the record count label
                        UpdateRecordCountLabel();
                    }
                    else
                    {
                        MessageBox.Show("No data was loaded from the database.");

                        // Update textBox3 to show zero items
                        if (textBox3 != null)
                        {
                            textBox3.Text = "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        // Method to sort data by latest items first
        private void SortDataByLatestFirst()
        {
            try
            {
                if (fullDataTable == null || fullDataTable.Rows.Count == 0)
                    return;

                // Create a DataView to sort the data
                DataView dataView = fullDataTable.DefaultView;

                // Try to sort by different possible date/ID columns in descending order
                // Priority: CreatedDate, ModifiedDate, ItemId (as fallback)
                string sortColumn = "";

                if (fullDataTable.Columns.Contains("CreatedDate"))
                {
                    sortColumn = "CreatedDate DESC";
                }
                else if (fullDataTable.Columns.Contains("ModifiedDate"))
                {
                    sortColumn = "ModifiedDate DESC";
                }
                else if (fullDataTable.Columns.Contains("CreatedDateTime"))
                {
                    sortColumn = "CreatedDateTime DESC";
                }
                else if (fullDataTable.Columns.Contains("ModifiedDateTime"))
                {
                    sortColumn = "ModifiedDateTime DESC";
                }
                else if (fullDataTable.Columns.Contains("ItemId"))
                {
                    // Sort by ItemId in descending order (assuming higher IDs are newer)
                    sortColumn = "ItemId DESC";
                }
                else if (fullDataTable.Columns.Contains("Id"))
                {
                    // Sort by Id in descending order (assuming higher IDs are newer)
                    sortColumn = "Id DESC";
                }

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    dataView.Sort = sortColumn;

                    // Create a new DataTable with the sorted data
                    DataTable sortedTable = dataView.ToTable();
                    fullDataTable = sortedTable;

                    UpdateStatus($"Data sorted by: {sortColumn}");
                }
                else
                {
                    UpdateStatus("No suitable sort column found, keeping original order");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error sorting data: {ex.Message}");
                // Continue with original data if sorting fails
            }
        }

        // Method to apply latest-first sorting to a DataView
        private void ApplyLatestFirstSorting(DataView dataView)
        {
            try
            {
                if (dataView == null || dataView.Table == null)
                    return;

                // Try to sort by different possible date/ID columns in descending order
                // Priority: CreatedDate, ModifiedDate, ItemId (as fallback)
                string sortColumn = "";

                if (dataView.Table.Columns.Contains("CreatedDate"))
                {
                    sortColumn = "CreatedDate DESC";
                }
                else if (dataView.Table.Columns.Contains("ModifiedDate"))
                {
                    sortColumn = "ModifiedDate DESC";
                }
                else if (dataView.Table.Columns.Contains("CreatedDateTime"))
                {
                    sortColumn = "CreatedDateTime DESC";
                }
                else if (dataView.Table.Columns.Contains("ModifiedDateTime"))
                {
                    sortColumn = "ModifiedDateTime DESC";
                }
                else if (dataView.Table.Columns.Contains("ItemId"))
                {
                    // Sort by ItemId in descending order (assuming higher IDs are newer)
                    sortColumn = "ItemId DESC";
                }
                else if (dataView.Table.Columns.Contains("Id"))
                {
                    // Sort by Id in descending order (assuming higher IDs are newer)
                    sortColumn = "Id DESC";
                }

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    dataView.Sort = sortColumn;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error applying sorting to DataView: {ex.Message}");
                // Continue without sorting if it fails
            }
        }

        private void ApplyFilter(string searchText)
        {
            try
            {
                if (fullDataTable == null)
                {
                    UpdateStatus("No data available to filter. Please reload the form.");
                    return;
                }

                // PERFORMANCE: Suspend layout immediately to prevent multiple redraws
                ultraGrid1.SuspendLayout();

                // Update status with data info
                UpdateStatus($"Filtering with text: '{searchText}' on {fullDataTable.Rows.Count} rows");

                // Store current column widths and positions before changing data source
                Dictionary<string, int> columnWidths = new Dictionary<string, int>();
                Dictionary<string, int> columnPositions = new Dictionary<string, int>();

                // Also remember the current column order
                string firstColumnKey = "";
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    // Find the first visible column
                    int minPosition = int.MaxValue;
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden)
                        {
                            columnWidths[col.Key] = col.Width;
                            columnPositions[col.Key] = col.Header.VisiblePosition;

                            if (col.Header.VisiblePosition < minPosition)
                            {
                                minPosition = col.Header.VisiblePosition;
                                firstColumnKey = col.Key;
                            }
                        }
                    }
                }

                // Create a view of the data
                DataView dv = fullDataTable.DefaultView;

                // Apply the latest-first sorting to the DataView
                ApplyLatestFirstSorting(dv);

                // If search text is provided, apply filter
                if (!string.IsNullOrEmpty(searchText) && searchText != "Search items...")
                {
                    // Get the selected search filter option
                    string filterOption = comboBox1.SelectedItem?.ToString() ?? "Select all";

                    // Build a filter based on the selected option
                    string filter = BuildFilterString(searchText, filterOption);

                    dv.RowFilter = filter;

                    // Update status with filter info
                    UpdateStatus($"Filter applied: '{filter}' - Found {dv.Count} matching rows (Latest first)");

                    // Update title with filter results
                    this.Text = $"Item Master - Showing {dv.Count} of {fullDataTable.Rows.Count} items (Latest first)";

                    // Update textBox3 with the filtered count
                    if (textBox3 != null)
                    {
                        textBox3.Text = dv.Count.ToString();
                    }
                }
                else
                {
                    // Clear any existing filter
                    dv.RowFilter = string.Empty;
                    this.Text = $"Item Master - All {fullDataTable.Rows.Count} items (Latest first)";

                    UpdateStatus("No filter applied - showing all rows (Latest first)");

                    // Update textBox3 with the total count
                    if (textBox3 != null)
                    {
                        textBox3.Text = fullDataTable.Rows.Count.ToString();
                    }
                }

                // Apply the filtered view to the grid
                ultraGrid1.DataSource = dv;

                // Configure visible columns but preserve widths and positions
                ConfigureVisibleColumnsPreserveLayout(columnWidths, columnPositions);

                // Always maintain the column order based on comboBox2 selection
                if (comboBox2.SelectedItem != null)
                {
                    string selectedColumn = comboBox2.SelectedItem.ToString();
                    ReorderColumns(selectedColumn);
                }

                // Update the record count label
                UpdateRecordCountLabel();

                // PERFORMANCE: Resume layout once at the end
                ultraGrid1.ResumeLayout(true);

                // Final status update
                UpdateStatus($"Grid updated with {ultraGrid1.Rows.Count} rows. Filter: {(string.IsNullOrEmpty(dv.RowFilter) ? "None" : dv.RowFilter)}");
            }
            catch (Exception ex)
            {
                // Make sure to resume layout even if there's an error
                try { ultraGrid1.ResumeLayout(true); } catch { }
                UpdateStatus($"Error applying filter: {ex.Message}");
            }
        }

        // Build filter string based on selected filter option
        private string BuildFilterString(string searchText, string filterOption)
        {
            // Escape any single quotes in the search text
            string escapedSearchText = searchText.Replace("'", "''");

            // Build the filter based on the selected option
            switch (filterOption)
            {
                case "Barcode":
                    return $"BarCode LIKE '%{escapedSearchText}%'";

                case "Item Name":
                    return $"Description LIKE '%{escapedSearchText}%'";

                case "UnitID":
                    return $"CONVERT(UnitId, 'System.String') LIKE '%{escapedSearchText}%'";

                case "Unit":
                    return $"Unit LIKE '%{escapedSearchText}%'";

                case "Select all":
                default:
                    // Search across all relevant columns
                    return $"BarCode LIKE '%{escapedSearchText}%' OR " +
                        $"Description LIKE '%{escapedSearchText}%' OR " +
                        $"CONVERT(UnitId, 'System.String') LIKE '%{escapedSearchText}%' OR " +
                        $"Unit LIKE '%{escapedSearchText}%'";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the search text
                string searchText = textBoxsearch.Text.Trim();

                // Update status
                UpdateStatus($"Search text changed: '{searchText}'");

                // Apply the filter
                ApplyFilter(searchText);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error during search: {ex.Message}");
            }
        }

        // Add a debug button to verify search functionality
        private void btnDebug_Click(object sender, EventArgs e)
        {
            try
            {
                if (fullDataTable == null)
                {
                    MessageBox.Show("No data loaded yet");
                    return;
                }

                string filterStatus = "No filter applied";
                int visibleRows = fullDataTable.Rows.Count;

                if (ultraGrid1.DataSource is DataView dv && !string.IsNullOrEmpty(dv.RowFilter))
                {
                    filterStatus = $"Current filter: {dv.RowFilter}";
                    visibleRows = dv.Count;
                }

                string debugInfo = $"Data status:\n" +
                                $"- Total rows in data: {fullDataTable.Rows.Count}\n" +
                                $"- Visible rows in grid: {visibleRows}\n" +
                                $"- {filterStatus}\n" +
                                $"- Search text: '{textBoxsearch.Text}'";

                MessageBox.Show(debugInfo, "Search Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Debug error: {ex.Message}");
            }
        }

        // Make sure the search textbox is properly initialized in the designer
        private void EnsureSearchControlsInitialized()
        {
            // Check if textBox1 exists and is properly set up
            if (textBoxsearch != null)
            {
                // We'll just add the event handlers - if they're already there, it won't hurt
                textBoxsearch.GotFocus += (s, e) => { if (textBoxsearch.Text == "Search items...") textBoxsearch.Text = ""; };
                textBoxsearch.LostFocus += (s, e) => { if (string.IsNullOrEmpty(textBoxsearch.Text)) textBoxsearch.Text = "Search items..."; };

                // Add key press handler for diagnostics
                textBoxsearch.KeyPress += TextBox1_KeyPress;

                // Add key down handler for Enter key navigation
                textBoxsearch.KeyDown -= textBoxsearch_KeyDown; // Remove to avoid duplicates
                textBoxsearch.KeyDown += textBoxsearch_KeyDown; // Add it back

                // Make sure it's visible and enabled
                textBoxsearch.Visible = true;
                textBoxsearch.Enabled = true;
                textBoxsearch.BringToFront();

                UpdateStatus($"Existing textBox1 configured: Location={textBoxsearch.Location}, Size={textBoxsearch.Size}, Visible={textBoxsearch.Visible}");
            }
            else
            {
                // Create it if it doesn't exist
                textBoxsearch = new TextBox();
                textBoxsearch.Name = "textBox1";
                textBoxsearch.Location = new Point(100, 10);
                textBoxsearch.Size = new Size(200, 25);
                // Use Text property instead of PlaceholderText for compatibility
                textBoxsearch.Text = "Search items...";
                this.Controls.Add(textBoxsearch);
                textBoxsearch.TextChanged += textBox1_TextChanged;

                // Add key press handler for diagnostics
                textBoxsearch.KeyPress += TextBox1_KeyPress;

                // Add key down handler for Enter key navigation
                textBoxsearch.KeyDown += textBoxsearch_KeyDown;

                // Add event to clear the text on focus
                textBoxsearch.GotFocus += (s, e) => { if (textBoxsearch.Text == "Search items...") textBoxsearch.Text = ""; };
                textBoxsearch.LostFocus += (s, e) => { if (string.IsNullOrEmpty(textBoxsearch.Text)) textBoxsearch.Text = "Search items..."; };

                UpdateStatus($"New textBox1 created: Location={textBoxsearch.Location}, Size={textBoxsearch.Size}, Visible={textBoxsearch.Visible}");
            }

            // Check if debug button already exists
            Control existingDebugBtn = this.Controls.Find("btnDebug", true).FirstOrDefault();
            if (existingDebugBtn == null)
            {
                // Add a debug button
                Button btnDebug = new Button();
                btnDebug.Name = "btnDebug";
                btnDebug.Text = "Debug";
                btnDebug.Location = new Point(textBoxsearch.Right + 10, textBoxsearch.Top);
                btnDebug.Size = new Size(60, 25);
                btnDebug.Click += btnDebug_Click;
                this.Controls.Add(btnDebug);
            }
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Update status to show key presses
            UpdateStatus($"Key pressed in textBox1: '{e.KeyChar}' (ASCII: {(int)e.KeyChar})");
        }

        // Keep the LoadGrid method for backward compatibility with other code
        private void LoadGrid(string searchText = "")
        {
            // Store current column widths and positions before changing data source
            Dictionary<string, int> columnWidths = new Dictionary<string, int>();
            Dictionary<string, int> columnPositions = new Dictionary<string, int>();

            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        columnWidths[col.Key] = col.Width;
                        columnPositions[col.Key] = col.Header.VisiblePosition;
                    }
                }
            }

            // Temporarily suspend layout
            ultraGrid1.SuspendLayout();

            // Just delegate to our new filtering system
            ApplyFilter(searchText);

            // Resume layout
            ultraGrid1.ResumeLayout();
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Apply proper grid line styles
                e.Layout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;

                // Set border style for the main grid
                e.Layout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

                // Remove cell padding/spacing
                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;

                // Configure row height to match the image
                e.Layout.Override.MinRowHeight = 30;
                e.Layout.Override.DefaultRowHeight = 30;

                // Define colors
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Set default alignment for all cells
                e.Layout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                e.Layout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;

                // Set font size for all cells to match the image (standard text size)
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Hide all columns first
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in e.Layout.Bands[0].Columns)
                {
                    col.Hidden = true;
                }

                // Define the columns to show in the specified order
                List<string> columnsList = new List<string> { "BarCode", "Description", "UnitId", "Unit", "Cost", "RetailPrice", "Stock" };
                if (FormName == "frmSalesInvoice")
                {
                    columnsList.Remove("Cost");
                }
                string[] columnsToShow = columnsList.ToArray();

                // Show and configure only the specified columns in the specified order
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    string colKey = columnsToShow[i];

                    if (e.Layout.Bands[0].Columns.Exists(colKey))
                    {
                        Infragistics.Win.UltraWinGrid.UltraGridColumn col = e.Layout.Bands[0].Columns[colKey];
                        col.Hidden = false;
                        col.Header.VisiblePosition = i; // Set the order

                        // Set header style for all columns
                        col.Header.Appearance.BackColor = headerBlue;
                        col.Header.Appearance.BackColor2 = headerBlue;
                        col.Header.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                        col.Header.Appearance.ForeColor = Color.White;
                        col.Header.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                        col.Header.Appearance.FontData.SizeInPoints = 9;
                        col.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                        col.Header.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        col.Header.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;

                        // Configure specific columns
                        switch (colKey)
                        {
                            case "BarCode":
                                col.Header.Caption = "Barcode";
                                col.Width = 120;
                                break;
                            case "Description":
                                col.Header.Caption = "Item Name";
                                col.Width = 250;
                                break;
                            case "UnitId":
                                col.Header.Caption = "UnitId";
                                col.Width = 80;
                                break;
                            case "Unit":
                                col.Header.Caption = "Unit";
                                col.Width = 80;
                                break;
                            case "Cost":
                                col.Header.Caption = "Cost";
                                col.Width = 100;
                                col.Format = "N2";
                                col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                break;
                            case "RetailPrice":
                                col.Header.Caption = "Price";
                                col.Width = 100;
                                col.Format = "N2";
                                col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                break;
                            case "Stock":
                                col.Header.Caption = "Stock";
                                col.Width = 80;
                                col.Format = "N2";
                                col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                break;
                        }
                    }
                }

                // Ensure the OriginalRowOrder column is hidden if it exists
                if (e.Layout.Bands[0].Columns.Exists("OriginalRowOrder"))
                {
                    e.Layout.Bands[0].Columns["OriginalRowOrder"].Hidden = true;
                }

                // Apply row selector appearance with blue
                e.Layout.Override.RowSelectorAppearance.BackColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;
                e.Layout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                e.Layout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None;
                e.Layout.Override.RowSelectorWidth = 15;

                // Set these properties to completely clean the row headers - remove all indicators
                e.Layout.Override.ActiveRowAppearance.Image = null;
                e.Layout.Override.SelectedRowAppearance.Image = null;
                e.Layout.Override.RowSelectorAppearance.Image = null;

                // Set all cells to white background
                e.Layout.Override.RowAppearance.BackColor = Color.White;
                e.Layout.Override.RowAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                e.Layout.Override.RowAlternateAppearance.BackColor = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Configure selected row appearance with light blue highlight to match the image
                e.Layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue from the image
                e.Layout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                e.Layout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                e.Layout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast

                // Active row appearance - make it same as selected row
                e.Layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                e.Layout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                e.Layout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                e.Layout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Configure scrollbar style to match FrmPurchaseDisplayDialog.cs
                e.Layout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                e.Layout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;

                // Configure the scrollbar look to match FrmPurchaseDisplayDialog.cs
                if (e.Layout.ScrollBarLook != null)
                {
                    // Style track and buttons with blue colors
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    e.Layout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    e.Layout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // In ultraGrid1_InitializeLayout method - add these lines to disable filter indicators
                e.Layout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                e.Layout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                e.Layout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                e.Layout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                e.Layout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;

                // Repopulate the column chooser list box since column visibility has changed
                PopulateColumnChooserListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing grid layout: {ex.Message}");
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Handle Enter key press for various forms
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Log to debug
                System.Diagnostics.Debug.WriteLine($"Enter key pressed in frmdialForItemMaster. FormName: {FormName}");

                if (FormName == "frmSalesReturn" || FormName == "frmSalesInvoice")
                {
                    SendItemToSalesReturn();
                }
                else if (FormName == "FromPurchase")
                {
                    SendItemToPurchase();
                }
                else if (FormName == "FromItemMaster")
                {
                    // Use the shared method to load item master data
                    LoadItemMasterData();
                }
                else if (FormName == "FrmStockAdjustment")
                {
                    // Send the item to FrmStockAdjustment
                    System.Diagnostics.Debug.WriteLine("Sending item to Stock Adjustment form");
                    SendItemToStockAdjustment();
                }
                else if (FormName == "FrmStockTransfer")
                {
                    System.Diagnostics.Debug.WriteLine("Sending item to Stock Transfer form");
                    SendItemToStockTransfer();
                }
                else if (FormName == "FromPurchaseReturn")
                {
                    SendItemToPurchaseReturn();
                }
                else if (FormName == "ItemHistoryLog")
                {
                    CaptureSelectedItemData();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        // Override form's KeyDown to ensure Enter key works
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

            if (keyData == Keys.Enter)
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Log to debug
                    System.Diagnostics.Debug.WriteLine($"Enter key processed in frmdialForItemMaster. FormName: {FormName}");

                    if (FormName == "FrmStockAdjustment")
                    {
                        SendItemToStockAdjustment();
                        return true;
                    }
                    else if (FormName == "FrmStockTransfer")
                    {
                        SendItemToStockTransfer();
                        return true;
                    }
                    else if (FormName == "frmSalesReturn" || FormName == "frmSalesInvoice")
                    {
                        SendItemToSalesReturn();
                        return true;
                    }
                    else if (FormName == "FromPurchase")
                    {
                        SendItemToPurchase();
                        return true;
                    }
                    else if (FormName == "FromItemMaster")
                    {
                        LoadItemMasterData();
                        return true;
                    }
                    else if (FormName == "FromPurchaseReturn")
                    {
                        SendItemToPurchaseReturn();
                        return true;
                    }
                    else if (FormName == "FrmBarcode" || FormName == "frmChangeItemNo" || FormName == "frmvendorpurchasereport" || FormName == "ItemHistoryLog")
                    {
                        // Handle simple selection dialogs
                        System.Diagnostics.Debug.WriteLine($"Enter key pressed, selecting item for {FormName}");
                        CaptureSelectedItemData();
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return true;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ultraGrid1_DoubleClickCell(object sender, Infragistics.Win.UltraWinGrid.DoubleClickCellEventArgs e)
        {
            try
            {
                CaptureSelectedItemData();

                // Process based on form type
                if (FormName == "frmSalesReturn" || FormName == "frmSalesInvoice")
                {
                    // Send the item to sales return or sales invoice form
                    SendItemToSalesReturn();
                }
                else if (FormName == "FromPurchase")
                {
                    // Send the item to purchase form
                    SendItemToPurchase();
                }
                else if (FormName == "FromPurchaseReturn")
                {
                    // Send the item to purchase return form
                    SendItemToPurchaseReturn();
                }
                else if (FormName == "FromItemMaster")
                {
                    // Load item master data
                    LoadItemMasterData();
                }
                else if (FormName == "FrmStockAdjustment")
                {
                    // Send the item to FrmStockAdjustment
                    System.Diagnostics.Debug.WriteLine("Double-click detected, sending item to Stock Adjustment");
                    SendItemToStockAdjustment();
                }
                else if (FormName == "FrmStockTransfer")
                {
                    System.Diagnostics.Debug.WriteLine("Double-click detected, sending item to Stock Transfer");
                    SendItemToStockTransfer();
                }
                else if (FormName == "FrmBarcode" || FormName == "frmChangeItemNo" || FormName == "frmvendorpurchasereport" || FormName == "ItemStockActivity" || FormName == "ItemHistoryLog")
                {
                    // Handle simple selection dialogs
                    System.Diagnostics.Debug.WriteLine($"Double-click detected, selecting item for {FormName}");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in double-click handler: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in double-click: {ex.Message}");
            }
        }

        private void ultraGrid1_ClickCell(object sender, Infragistics.Win.UltraWinGrid.ClickCellEventArgs e)
        {
            // We will no longer send items on single click for most forms
            // Items will only be loaded on double-click or Enter key press

            // However, for FrmBarcode, we can allow single click for better UX
            if (FormName == "FrmBarcode" || FormName == "frmChangeItemNo" || FormName == "frmvendorpurchasereport" || FormName == "ItemHistoryLog")
            {
                // For simple selection dialogs, allow single click selection
                CaptureSelectedItemData();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }



        /// <summary>
        /// Gets the selected item data as a Dictionary
        /// </summary>
        /// <returns>Dictionary containing the selected item's data</returns>
        public Dictionary<string, object> GetSelectedItemData()
        {
            Dictionary<string, object> itemData = new Dictionary<string, object>();

            if (ultraGrid1 != null && ultraGrid1.ActiveRow != null)
            {
                UltraGridRow row = ultraGrid1.ActiveRow;

                // Add all cell values to the dictionary with null checking
                foreach (UltraGridCell cell in row.Cells)
                {
                    if (cell.Value != null)
                    {
                        itemData[cell.Column.Key] = cell.Value;
                    }
                    else
                    {
                        // Add null or default values based on column type
                        if (cell.Column.DataType == typeof(string))
                            itemData[cell.Column.Key] = string.Empty;
                        else if (cell.Column.DataType == typeof(int) || cell.Column.DataType == typeof(Int32))
                            itemData[cell.Column.Key] = 0;
                        else if (cell.Column.DataType == typeof(decimal) || cell.Column.DataType == typeof(float) ||
                                 cell.Column.DataType == typeof(double))
                            itemData[cell.Column.Key] = 0.0;
                        else
                            itemData[cell.Column.Key] = null;
                    }
                }

                // Ensure essential fields exist with proper type handling
                if (!itemData.ContainsKey("ItemId") && row.Cells.Exists("ItemId"))
                {
                    object value = row.Cells["ItemId"].Value;
                    itemData["ItemId"] = value != null ? value : 0;
                }

                if (!itemData.ContainsKey("Description") && row.Cells.Exists("Description"))
                {
                    object value = row.Cells["Description"].Value;
                    itemData["Description"] = value != null ? value.ToString() : string.Empty;
                }

                if (!itemData.ContainsKey("BarCode") && row.Cells.Exists("BarCode"))
                {
                    object value = row.Cells["BarCode"].Value;
                    itemData["BarCode"] = value != null ? value.ToString() : string.Empty;
                }

                if (!itemData.ContainsKey("Unit") && row.Cells.Exists("Unit"))
                {
                    object value = row.Cells["Unit"].Value;
                    itemData["Unit"] = value != null ? value.ToString() : string.Empty;
                }

                if (!itemData.ContainsKey("Packing") && row.Cells.Exists("Packing"))
                {
                    object value = row.Cells.Exists("Packing") ? row.Cells["Packing"].Value : null;
                    itemData["Packing"] = value != null ? Convert.ToSingle(value) : 1;
                }

                if (!itemData.ContainsKey("Cost") && row.Cells.Exists("Cost"))
                {
                    object value = row.Cells.Exists("Cost") ? row.Cells["Cost"].Value : null;
                    itemData["Cost"] = value != null ? Convert.ToSingle(value) : 0;
                }

                if (!itemData.ContainsKey("RetailPrice") && row.Cells.Exists("RetailPrice"))
                {
                    object value = row.Cells.Exists("RetailPrice") ? row.Cells["RetailPrice"].Value : null;
                    itemData["RetailPrice"] = value != null ? Convert.ToSingle(value) : 0;
                }

                if (!itemData.ContainsKey("TaxPer") && row.Cells.Exists("TaxPer"))
                {
                    object value = row.Cells.Exists("TaxPer") ? row.Cells["TaxPer"].Value : null;
                    itemData["TaxPer"] = value != null ? Convert.ToSingle(value) : 0;
                }

                if (!itemData.ContainsKey("TaxAmt") && row.Cells.Exists("TaxAmt"))
                {
                    object value = row.Cells.Exists("TaxAmt") ? row.Cells["TaxAmt"].Value : null;
                    itemData["TaxAmt"] = value != null ? Convert.ToSingle(value) : 0;
                }

                if (!itemData.ContainsKey("TaxType") && row.Cells.Exists("TaxType"))
                {
                    object value = row.Cells.Exists("TaxType") ? row.Cells["TaxType"].Value : null;
                    itemData["TaxType"] = value != null ? value.ToString() : string.Empty;
                }

                if (!itemData.ContainsKey("UnitId") && row.Cells.Exists("UnitId"))
                {
                    object value = row.Cells.Exists("UnitId") ? row.Cells["UnitId"].Value : null;
                    itemData["UnitId"] = value != null ? Convert.ToInt32(value) : 0;
                }
            }

            return itemData;
        }

        private void SendItemToSalesReturn()
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    if (!EnsureSelectedRowAllowedForCurrentDialog(ultraGrid1.ActiveRow))
                    {
                        return;
                    }

                    // Handle based on the form type
                    if (FormName == "frmSalesReturn")
                    {
                        frmSalesReturn parentForm = this.Owner as frmSalesReturn;
                        if (parentForm != null)
                        {
                            string itemId = ultraGrid1.ActiveRow.Cells["ItemId"].Value?.ToString() ?? "";
                            string itemName = ultraGrid1.ActiveRow.Cells["Description"].Value?.ToString() ?? "";
                            string barcode = ultraGrid1.ActiveRow.Cells["BarCode"].Value?.ToString() ?? "";
                            string unit = ultraGrid1.ActiveRow.Cells["Unit"].Value?.ToString() ?? "";
                            decimal unitPrice = GetSelectedUnitPrice(ultraGrid1.ActiveRow);

                            parentForm.AddItemToGrid(itemId, itemName, barcode, unit, unitPrice, 1, unitPrice);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }
                    else if (FormName == "frmSalesInvoice")
                    {
                        frmSalesInvoice parentForm = this.Owner as frmSalesInvoice;
                        if (parentForm != null)
                        {
                            string itemId = ultraGrid1.ActiveRow.Cells["ItemId"].Value?.ToString() ?? "";
                            string itemName = ultraGrid1.ActiveRow.Cells["Description"].Value?.ToString() ?? "";
                            string barcode = ultraGrid1.ActiveRow.Cells["BarCode"].Value?.ToString() ?? "";
                            string unit = ultraGrid1.ActiveRow.Cells["Unit"].Value?.ToString() ?? "";
                            decimal unitPrice = GetSelectedUnitPrice(ultraGrid1.ActiveRow);

                            parentForm.AddItemToGrid(itemId, itemName, barcode, unit, unitPrice, 1, unitPrice);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending item to form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in SendItemToSalesReturn: {ex.Message}");
            }
        }

        private decimal GetSelectedUnitPrice(UltraGridRow row)
        {
            if (row == null)
                return 0m;

            string level = (SelectedPriceLevel ?? "RetailPrice").Trim();
            string priceColumnKey;

            switch (level.ToUpperInvariant())
            {
                case "WHOLESALEPRICE":
                    priceColumnKey = "RetailPrice";
                    break;
                case "CREDITPRICE":
                    priceColumnKey = "CreditPrice";
                    break;
                case "CARDPRICE":
                    priceColumnKey = "CardPrice";
                    break;
                case "MRP":
                    priceColumnKey = "MRP";
                    break;
                case "STAFFPRICE":
                    priceColumnKey = "StaffPrice";
                    break;
                case "MINPRICE":
                    priceColumnKey = "MinPrice";
                    break;
                case "RETAILPRICE":
                default:
                    priceColumnKey = "WholeSalePrice";
                    break;
            }

            if (!row.Cells.Exists(priceColumnKey))
            {
                if (row.Cells.Exists("WholeSalePrice"))
                    priceColumnKey = "WholeSalePrice";
                else if (row.Cells.Exists("RetailPrice"))
                    priceColumnKey = "RetailPrice";
                else
                    return 0m;
            }

            decimal.TryParse(row.Cells[priceColumnKey].Value?.ToString(), out decimal unitPrice);
            return unitPrice;
        }

        private void SendItemToPurchaseReturn()
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Get the parent form
                    var parentForm = this.Owner as PosBranch_Win.Transaction.frmPurchaseReturn;
                    if (parentForm != null)
                    {
                        // Close the dialog with OK result so the parent form can handle the data
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending item to Purchase Return: {ex.Message}");
            }
        }

        public void SetImageFromBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                MessageBox.Show("Base64 string is null or empty.");
                return;
            }

            // Remove the data URL scheme if present
            if (base64String.StartsWith("data:image/"))
            {
                var commaIndex = base64String.IndexOf(',');
                if (commaIndex >= 0)
                {
                    base64String = base64String.Substring(commaIndex + 1);
                }
            }

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    Image image = Image.FromStream(ms);
                    //ItemMaster.pictureBox1.Image = image; // Set the PictureBox image
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Invalid Base64 string: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        public static byte[] StringToByteArray(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Input string cannot be null or empty.");
            }

            // Use UTF8 encoding (you can choose another encoding if needed)
            return Encoding.UTF8.GetBytes(str);
        }
        public void LoadImageFromByteArray(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                Image image = Image.FromStream(ms);
                //ItemMaster.openFileDialog1.FileName = image;
            }
        }
        public void SetImageFromByteArray(byte[] imageBytes)
        {
            if (imageBytes != null && imageBytes.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    ms.Position = 0;
                    Image image = Image.FromStream(ms);
                    //ItemMaster.pictureBox1.Image = image; // Set the PictureBox image
                }
            }
            else
            {
                //ItemMaster.pictureBox1.Image = null; // Optionally set to null if no image
            }
        }

        // Method replaced by textBoxsearch_KeyDown which handles both Enter and Down keys
        // private void textBox1_KeyDown(object sender, KeyEventArgs e)
        // {
        //     if (e.KeyCode == Keys.Down)
        //     {
        //         ultraGrid1.Focus();
        //     }
        // }

        // Helper method to load data to ItemMaster

        private void LoadProfitMarginValues(frmItemMasterNew itemMaster, ItemMasterPriceSettings[] baseUnitList)
        {
            try
            {
                if (itemMaster == null || baseUnitList == null || baseUnitList.Length == 0)
                    return;

                var baseUnit = baseUnitList[0];
                double unitCost = baseUnit.Cost;

                if (unitCost <= 0)
                {
                    // If no unit cost, clear all profit margin fields
                    itemMaster.SetProfitMarginValues(0, 0, 0, 0, 0);
                    return;
                }

                // Calculate profit margins for each price type
                // Note: ultraTextEditor4 is for txt_Retail (WholeSalePrice), ultraTextEditor10 is for txt_walkin (RetailPrice)
                double retailMargin = CalculateProfitMargin(baseUnit.WholeSalePrice, unitCost); // For ultraTextEditor4 (txt_Retail)
                double walkingMargin = CalculateProfitMargin(baseUnit.RetailPrice, unitCost); // For ultraTextEditor10 (txt_walkin)
                double creditMargin = CalculateProfitMargin(baseUnit.CreditPrice, unitCost);
                double mrpMargin = CalculateProfitMargin(baseUnit.MRP, unitCost);
                double cardMargin = CalculateProfitMargin(baseUnit.CardPrice, unitCost);

                // Set all profit margin values at once
                itemMaster.SetProfitMarginValues(retailMargin, walkingMargin, creditMargin, mrpMargin, cardMargin);

                System.Diagnostics.Debug.WriteLine($"Successfully loaded profit margins for item {baseUnit.ItemId} - Retail: {retailMargin}, Walking: {walkingMargin}, Credit: {creditMargin}, MRP: {mrpMargin}, Card: {cardMargin}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profit margin values: {ex.Message}");
            }
        }

        private int ResolvePidFromPurchaseNo(int purchaseNo)
        {
            if (purchaseNo <= 0)
                return 0;

            BaseRepostitory repo = null;
            SqlConnection sqlConnection = null;
            try
            {
                repo = new BaseRepostitory();
                sqlConnection = repo.DataConnection as SqlConnection;
                if (sqlConnection == null)
                    return 0;

                if (sqlConnection.State != ConnectionState.Open)
                    sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 Pid FROM PMaster WHERE PurchaseNo = @PurchaseNo AND (@BranchId = 0 OR BranchId = @BranchId) AND (@CompanyId = 0 OR CompanyId = @CompanyId) ORDER BY Pid DESC",
                    sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);

                    int branchId;
                    int.TryParse(DataBase.BranchId, out branchId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    int companyId;
                    int.TryParse(DataBase.CompanyId, out companyId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);

                    object result = cmd.ExecuteScalar();
                    return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resolving Pid for PurchaseNo {purchaseNo}: {ex.Message}");
                return 0;
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private double CalculateProfitMargin(double sellingPrice, double unitCost)
        {
            try
            {
                if (sellingPrice <= 0 || unitCost <= 0)
                {
                    return 0.0;
                }

                // Calculate profit margin percentage: ((SellingPrice - UnitCost) / SellingPrice) * 100
                double profitMarginPercent = ((sellingPrice - unitCost) / sellingPrice) * 100.0;

                // Clamp to reasonable range (0-100%)
                if (profitMarginPercent < 0) profitMarginPercent = 0;
                if (profitMarginPercent > 100) profitMarginPercent = 100;

                return Math.Round(profitMarginPercent, 2);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating profit margin: {ex.Message}");
                return 0.0;
            }
        }




        void LoadItemMasterData()
        {
            try
            {
                ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                if (ItemMaster == null || ultraGrid1.ActiveRow == null)
                    return;

                UltraGridCell ItemId = this.ultraGrid1.ActiveRow.Cells["ItemId"];
                int ItemID = Convert.ToInt32(ItemId.Value.ToString());

                // Set the current item ID in the ItemMaster form for hold details
                ItemMaster.SetCurrentItemId(ItemID);

                // Set loading flag to prevent synchronization during loading
                ItemMaster.SetLoadingFlag(true);

                ItemGet getItem = ItemREpo.GetByIdItem(ItemID);

                // Determine base unit rows (prefer UnitId == BaseUnitId, fallback to Packing == 1, then first row)
                ItemMasterPriceSettings[] baseUnitList = new ItemMasterPriceSettings[0];
                try
                {
                    if (getItem != null && getItem.List != null && getItem.List.Length > 0)
                    {
                        int baseUnitId = getItem.BaseUnitId;
                        baseUnitList = getItem.List
                            .Where(p => (baseUnitId > 0 && p.UnitId == baseUnitId) || Math.Abs(p.Packing - 1) < 0.00001)
                            .ToArray();

                        if (baseUnitList.Length == 0)
                        {
                            baseUnitList = new ItemMasterPriceSettings[] { getItem.List[0] };
                        }
                    }
                }
                catch { baseUnitList = getItem.List ?? new ItemMasterPriceSettings[0]; }

                // Get stock value from grid if it exists
                float stockValue = 0;
                if (ultraGrid1.ActiveRow.Cells.Exists("Stock") &&
                    ultraGrid1.ActiveRow.Cells["Stock"].Value != null)
                {
                    float.TryParse(ultraGrid1.ActiveRow.Cells["Stock"].Value.ToString(), out stockValue);
                }

                // Get hold value if it exists
                float holdValue = 0;
                if (ultraGrid1.ActiveRow.Cells.Exists("Hold") &&
                    ultraGrid1.ActiveRow.Cells["Hold"].Value != null)
                {
                    float.TryParse(ultraGrid1.ActiveRow.Cells["Hold"].Value.ToString(), out holdValue);
                }

                // Set the stock fields first
                try
                {
                    ItemMaster.SetQtyValue(stockValue.ToString());
                    ItemMaster.SetAvailableValue(stockValue.ToString());
                    ItemMaster.SetHoldValue(holdValue.ToString());

                    // Set the price values from base unit row
                    if (baseUnitList != null && baseUnitList.Length > 0)
                    {
                        var baseUnit = baseUnitList[0];
                        ItemMaster.SetWalkingPriceValue(baseUnit.RetailPrice.ToString("0.000"));
                        ItemMaster.SetRetailPriceValue(baseUnit.WholeSalePrice.ToString("0.000"));
                        ItemMaster.SetCreditPriceValue(baseUnit.CreditPrice.ToString("0.000"));
                        ItemMaster.SetMrpValue(baseUnit.MRP.ToString("0.000"));
                        ItemMaster.SetCardPriceValue(baseUnit.CardPrice.ToString("0.000"));

                        // Mirror LoadItemById: also load ALL markdown values and compute txt_SF/txt_MinP + their profit margins
                        try
                        {
                            // Load all markdown fields from database
                            var mdWalkinEditor = FindControlRecursive(ItemMaster, "ultraTextEditor16") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            var mdCreditEditor = FindControlRecursive(ItemMaster, "ultraTextEditor15") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            var mdMrpEditor = FindControlRecursive(ItemMaster, "ultraTextEditor14") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            var mdCardEditor = FindControlRecursive(ItemMaster, "ultraTextEditor13") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            var mdStaffEditor = FindControlRecursive(ItemMaster, "ultraTextEditor12") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            var mdMinEditor = FindControlRecursive(ItemMaster, "ultraTextEditor11") as Infragistics.Win.UltraWinEditors.UltraTextEditor;

                            if (mdWalkinEditor != null) mdWalkinEditor.Text = baseUnit.MDWalkinPrice.ToString("0.00");
                            if (mdCreditEditor != null) mdCreditEditor.Text = baseUnit.MDCreditPrice.ToString("0.00");
                            if (mdMrpEditor != null) mdMrpEditor.Text = baseUnit.MDMrpPrice.ToString("0.00");
                            if (mdCardEditor != null) mdCardEditor.Text = baseUnit.MDCardPrice.ToString("0.00");
                            if (mdStaffEditor != null) mdStaffEditor.Text = baseUnit.MDStaffPrice.ToString("0.00");
                            if (mdMinEditor != null) mdMinEditor.Text = baseUnit.MDMinPrice.ToString("0.00");

                            // Use stored StaffPrice and MinPrice from database directly.
                            // If the user didn't set individual values (i.e. they are 0),
                            // default them to the master retail price (WholeSalePrice).
                            // NOTE: txt_SF and txt_MinP are UltraTextEditor, NOT TextBox ? cast as Control
                            var txtSF = FindControlRecursive(ItemMaster, "txt_SF") as Control;
                            var txtMinP = FindControlRecursive(ItemMaster, "txt_MinP") as Control;
                            double retailDefault = baseUnit.WholeSalePrice; // WholeSalePrice = stored retail price
                            double sfPrice = baseUnit.StaffPrice > 0 ? baseUnit.StaffPrice : retailDefault;
                            double minPrice = baseUnit.MinPrice > 0 ? baseUnit.MinPrice : retailDefault;
                            if (txtSF != null) txtSF.Text = sfPrice.ToString("0.000");
                            if (txtMinP != null) txtMinP.Text = minPrice.ToString("0.000");

                            // Let the ItemMaster form handle profit margin calculation using its UpdateAllProfitMargins method
                            // This ensures consistent calculation logic and handles all edge cases properly
                        }
                        catch (Exception exMd)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error applying staff/min markdown and margins: {exMd.Message}");
                        }
                    }

                    // Apply UI updates immediately to make the changes visible
                    ItemMaster.Refresh();
                    Application.DoEvents();

                    // Update all profit margins after setting all price fields
                    ItemMaster.UpdateAllProfitMargins();

                    // TRIPLE-CHECK: Sometimes UpdateAllProfitMargins or Refresh recalculates markdown values
                    // without proper ".00" formatting. Ensure all 6 markdowns firmly receive ".00" now.
                    var mdWalkinF = FindControlRecursive(ItemMaster, "ultraTextEditor16") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var mdCreditF = FindControlRecursive(ItemMaster, "ultraTextEditor15") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var mdMrpF = FindControlRecursive(ItemMaster, "ultraTextEditor14") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var mdCardF = FindControlRecursive(ItemMaster, "ultraTextEditor13") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var mdStaffF = FindControlRecursive(ItemMaster, "ultraTextEditor12") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var mdMinF = FindControlRecursive(ItemMaster, "ultraTextEditor11") as Infragistics.Win.UltraWinEditors.UltraTextEditor;

                    if (baseUnitList != null && baseUnitList.Length > 0)
                    {
                        var baseUnit = baseUnitList[0];
                        if (mdWalkinF != null) mdWalkinF.Text = baseUnit.MDWalkinPrice.ToString("0.00");
                        if (mdCreditF != null) mdCreditF.Text = baseUnit.MDCreditPrice.ToString("0.00");
                        if (mdMrpF != null) mdMrpF.Text = baseUnit.MDMrpPrice.ToString("0.00");
                        if (mdCardF != null) mdCardF.Text = baseUnit.MDCardPrice.ToString("0.00");
                        if (mdStaffF != null) mdStaffF.Text = baseUnit.MDStaffPrice.ToString("0.00");
                        if (mdMinF != null) mdMinF.Text = baseUnit.MDMinPrice.ToString("0.00");
                    }

                    // FINAL: Ensure txt_SF and txt_MinP show the correct value.
                    // Must run AFTER UpdateAllProfitMargins() which may overwrite them.
                    // txt_SF and txt_MinP are UltraTextEditor ? cast as Control (NOT TextBox!)
                    if (baseUnitList != null && baseUnitList.Length > 0)
                    {
                        var baseUnit2 = baseUnitList[0];
                        var txtSFFinal = FindControlRecursive(ItemMaster, "txt_SF") as Control;
                        var txtMinPFinal = FindControlRecursive(ItemMaster, "txt_MinP") as Control;
                        double retailFinal = baseUnit2.WholeSalePrice; // WholeSalePrice = stored retail price
                        double sfFinal = baseUnit2.StaffPrice > 0 ? baseUnit2.StaffPrice : retailFinal;
                        double minFinal = baseUnit2.MinPrice > 0 ? baseUnit2.MinPrice : retailFinal;
                        if (txtSFFinal != null) txtSFFinal.Text = sfFinal.ToString("0.000");
                        if (txtMinPFinal != null) txtMinPFinal.Text = minFinal.ToString("0.000");
                    }

                    // Log values for debugging
                    System.Diagnostics.Debug.WriteLine($"Setting stock values - Qty: {stockValue}, Available: {stockValue}, Hold: {holdValue}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting text fields: {ex.Message}");
                    MessageBox.Show($"Error setting stock values: {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Populate other ItemMaster fields

                ItemMaster.txt_description.Text = getItem.Description;
                ItemMaster.txt_LocalLanguage.Text = getItem.NameInLocalLanguage;

                // Set the base unit text without triggering synchronization
                // We'll set it after populating the UOM grid to avoid clearing the grid
                ItemMaster.Txt_UnitCost.Text = getItem.List[0].Cost.ToString("0.000");
                ItemMaster.txt_Brand.Text = getItem.BrandName;
                ItemMaster.txt_Category.Text = getItem.CategoryName;

                ItemMaster.txt_CustomerType.Text = getItem.ForCustomerType;
                ItemMaster.txt_Group.Text = getItem.GroupName;

                ItemMaster.txt_ItemType.Text = getItem.ItemType;

                ItemMaster.txt_ItemNo.Text = getItem.ItemNo.ToString();
                ItemMaster.SetSmartReorderValues(
                    getItem.Order_Cycle_Days,
                    getItem.Box_Quantity,
                    getItem.Is_Perishable);


                // Load and set HSN code into textBox4 if present
                try
                {
                    TextBox hsnTextBox = FindControlRecursive(ItemMaster, "textBox4") as TextBox;
                    if (hsnTextBox != null)
                    {
                        string hsn = string.Empty;
                        try { hsn = getItem.HSNCode; } catch { hsn = string.Empty; }
                        hsnTextBox.Text = hsn ?? string.Empty;
                    }
                }
                catch { }

                // Load and set item photo early so it's visible immediately
                try
                {
                    byte[] photoBytes = null;
                    // Prefer GETBYID from price settings which returns Photo
                    var priceInfo = ItemREpo.GetByIdItemPrice(ItemID);
                    if (priceInfo != null)
                    {
                        if (priceInfo.PhotoByteArray != null && priceInfo.PhotoByteArray.Length > 0)
                            photoBytes = priceInfo.PhotoByteArray;
                        else if (priceInfo.Photo != null && priceInfo.Photo.Length > 0)
                            photoBytes = priceInfo.Photo;
                    }
                    // Fallback to item list's first row if present
                    if ((photoBytes == null || photoBytes.Length == 0) && getItem != null && getItem.List != null && getItem.List.Length > 0)
                    {
                        var ps = getItem.List[0];
                        if (ps.PhotoByteArray != null && ps.PhotoByteArray.Length > 0) photoBytes = ps.PhotoByteArray;
                        else if (ps.Photo != null && ps.Photo.Length > 0) photoBytes = ps.Photo;
                    }

                    ItemMaster.SetItemPhoto(photoBytes);
                }
                catch (Exception imgEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading photo from dialog: {imgEx.Message}");
                }

                // Create a DataTable for ultraGrid1
                DataTable dtUom = new DataTable();
                dtUom.Columns.Add("Unit", typeof(string));
                dtUom.Columns.Add("UnitId", typeof(string));
                dtUom.Columns.Add("Packing", typeof(string));
                // dtUom.Columns.Add("BarCode", typeof(string)); // Removed
                dtUom.Columns.Add("Reorder", typeof(string));
                dtUom.Columns.Add("OpnStk", typeof(string));
                dtUom.Columns.Add("Cost", typeof(float));
                dtUom.Columns.Add("MarginAmt", typeof(float));
                dtUom.Columns.Add("MarginPer", typeof(float));
                dtUom.Columns.Add("TaxPer", typeof(float));
                dtUom.Columns.Add("TaxAmt", typeof(float));
                dtUom.Columns.Add("RetailPrice", typeof(float));
                dtUom.Columns.Add("MRP", typeof(float));
                dtUom.Columns.Add("WholeSalePrice", typeof(float));
                dtUom.Columns.Add("CreditPrice", typeof(float));
                dtUom.Columns.Add("CardPrice", typeof(float));
                dtUom.Columns.Add("StaffPrice", typeof(float));
                dtUom.Columns.Add("MinPrice", typeof(float));

                dtUom.Columns.Add("AliasBarcode", typeof(string));

                // Add ALL saved unit rows to the DataTable
                foreach (var priceRow in (getItem.List ?? new ItemMasterPriceSettings[0]))
                {
                    DataRow row = dtUom.NewRow();
                    row["Unit"] = priceRow.Unit;
                    row["UnitId"] = priceRow.UnitId.ToString();
                    row["Packing"] = priceRow.Packing;
                    // row["BarCode"] = priceRow.BarCode;
                    row["Reorder"] = priceRow.ReOrder;
                    row["OpnStk"] = priceRow.OpnStk;
                    row["Cost"] = priceRow.Cost;
                    row["MarginAmt"] = priceRow.MarginAmt;
                    row["MarginPer"] = priceRow.MarginPer;
                    row["TaxPer"] = priceRow.TaxPer;
                    row["TaxAmt"] = priceRow.TaxAmt;
                    row["MRP"] = priceRow.MRP;
                    row["RetailPrice"] = priceRow.WholeSalePrice;
                    row["WholeSalePrice"] = priceRow.RetailPrice;
                    row["CreditPrice"] = priceRow.CreditPrice;
                    row["CardPrice"] = priceRow.CardPrice;
                    row["StaffPrice"] = priceRow.StaffPrice;
                    row["MinPrice"] = priceRow.MinPrice;
                    row["AliasBarcode"] = priceRow.AliasBarcode ?? string.Empty;
                    dtUom.Rows.Add(row);
                }

                // Set the DataTable as the DataSource for ultraGrid1
                ItemMaster.SetUltraGridDataSource(dtUom);

                // Now set the base unit text after the UOM grid is populated
                // This prevents the SynchronizeBaseUnitWithGrid from clearing the grid
                ItemMaster.txt_BaseUnit.Text = getItem.UnitName;

                // Load saved alternative barcodes into ultraGrid3 when that grid exists on the target form.
                // Alias barcodes are kept in the UOM grid and must not be copied into ALT PLU.
                try
                {
                    Infragistics.Win.UltraWinGrid.UltraGrid altGrid =
                        FindControlRecursive(ItemMaster, "ultraGrid3") as Infragistics.Win.UltraWinGrid.UltraGrid;

                    if (altGrid != null)
                    {
                        DataTable dtAlt = altGrid.DataSource as DataTable;
                        if (dtAlt == null)
                        {
                            dtAlt = new DataTable();
                            dtAlt.Columns.Add("Barcode", typeof(string));
                            altGrid.DataSource = dtAlt;
                        }

                        dtAlt.Rows.Clear();

                        HashSet<string> addedAlternativeBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        HashSet<string> aliasBarcodes = new HashSet<string>((getItem.List ?? new ItemMasterPriceSettings[0])
                            .Select(priceRow => priceRow?.AliasBarcode?.Trim() ?? string.Empty)
                            .Where(aliasBarcode => !string.IsNullOrWhiteSpace(aliasBarcode)), StringComparer.OrdinalIgnoreCase);

                        // Preserve the pasted-code path that loads saved alternative barcodes directly.
                        foreach (var altBarcode in (getItem.ListAlternativeBarcodes ?? new ItemAlternativeBarcode[0]))
                        {
                            string barcodeValue = altBarcode?.Barcode?.Trim();
                            if (string.IsNullOrWhiteSpace(barcodeValue) || aliasBarcodes.Contains(barcodeValue) || !addedAlternativeBarcodes.Add(barcodeValue))
                            {
                                continue;
                            }

                            DataRow altRow = dtAlt.NewRow();
                            altRow["Barcode"] = barcodeValue;
                            dtAlt.Rows.Add(altRow);
                        }

                        altGrid.Refresh();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading alternative barcodes into ultraGrid3: {ex.Message}");
                }

                // Keep dialog-based item loading aligned with frmItemMasterNew.LoadItemById
                // so the loaded main barcode stays locked for update mode.
                string loadedBarcode = getItem.Barcode;
                if (string.IsNullOrWhiteSpace(loadedBarcode) && getItem.List != null && getItem.List.Length > 0)
                {
                    loadedBarcode = getItem.List[0].BarCode;
                }

                if (!string.IsNullOrWhiteSpace(loadedBarcode))
                {
                    try
                    {
                        ItemMaster.SetLoadedItemBarcode(loadedBarcode);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error locking loaded barcode via helper: {ex.Message}");

                        try
                        {
                            TextBox barcodeTextBox = FindControlRecursive(ItemMaster, "txt_barcode") as TextBox;
                            if (barcodeTextBox != null)
                            {
                                barcodeTextBox.Text = loadedBarcode;
                                barcodeTextBox.ReadOnly = true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("txt_barcode control not found");
                            }
                        }
                        catch (Exception fallbackEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error setting barcode: {fallbackEx.Message}");
                        }
                    }
                }

                // Now populate Ult_Price
                // Find Ult_Price control
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    FindControlRecursive(ItemMaster, "Ult_Price") as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null)
                {
                    // Create DataTable for Ult_Price with proper column types
                    DataTable dtPrice = new DataTable();
                    dtPrice.Columns.Add("Unit", typeof(string));
                    dtPrice.Columns.Add("Packing", typeof(int));
                    dtPrice.Columns.Add("Cost", typeof(float));
                    dtPrice.Columns.Add("MarginAmt", typeof(float));
                    dtPrice.Columns.Add("MarginPer", typeof(float));
                    dtPrice.Columns.Add("TaxPer", typeof(float));
                    dtPrice.Columns.Add("TaxAmt", typeof(float));
                    dtPrice.Columns.Add("MRP", typeof(float));
                    dtPrice.Columns.Add("RetailPrice", typeof(float));
                    dtPrice.Columns.Add("WholeSalePrice", typeof(float));
                    dtPrice.Columns.Add("CreditPrice", typeof(float));
                    dtPrice.Columns.Add("CardPrice", typeof(float));
                    // New columns for Staff Price and Min Price
                    dtPrice.Columns.Add("StaffPrice", typeof(float));
                    dtPrice.Columns.Add("MinPrice", typeof(float));

                    // Add ALL saved unit rows to the DataTable with proper type conversion
                    foreach (var priceRow in (getItem.List ?? new ItemMasterPriceSettings[0]))
                    {
                        DataRow row = dtPrice.NewRow();
                        row["Unit"] = priceRow.Unit;
                        row["Packing"] = Convert.ToInt32(priceRow.Packing);
                        row["Cost"] = priceRow.Cost;
                        row["MarginAmt"] = priceRow.MarginAmt;
                        row["MarginPer"] = priceRow.MarginPer;
                        row["TaxPer"] = priceRow.TaxPer;
                        row["TaxAmt"] = priceRow.TaxAmt;
                        row["MRP"] = priceRow.MRP;
                        row["RetailPrice"] = priceRow.WholeSalePrice;
                        row["WholeSalePrice"] = priceRow.RetailPrice;
                        row["CreditPrice"] = priceRow.CreditPrice;
                        row["CardPrice"] = priceRow.CardPrice;
                        // Populate Staff and Min prices (fallback to markdown-derived values if zeros)
                        double baseRetailForCalc = priceRow.WholeSalePrice > 0 ? priceRow.WholeSalePrice : priceRow.RetailPrice;
                        double staffPrice = priceRow.StaffPrice;
                        if ((staffPrice <= 0) && baseRetailForCalc > 0 && priceRow.MDStaffPrice != 0)
                        {
                            staffPrice = baseRetailForCalc * (1.0 - (priceRow.MDStaffPrice / 100.0));
                        }
                        double minPrice = priceRow.MinPrice;
                        if ((minPrice <= 0) && baseRetailForCalc > 0 && priceRow.MDMinPrice != 0)
                        {
                            minPrice = baseRetailForCalc * (1.0 - (priceRow.MDMinPrice / 100.0));
                        }
                        row["StaffPrice"] = staffPrice;
                        row["MinPrice"] = minPrice;
                        dtPrice.Rows.Add(row);
                    }

                    // Set the DataTable as the DataSource for Ult_Price
                    Ult_Price.DataSource = dtPrice;
                    ItemMaster?.SyncUomGridWithPriceGrid();

                    // Configure columns if needed
                    if (Ult_Price.DisplayLayout.Bands.Count > 0)
                    {
                        // Format numeric columns
                        Ult_Price.DisplayLayout.Bands[0].Columns["Cost"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["MarginAmt"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["MarginPer"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["TaxPer"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["TaxAmt"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["MRP"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["RetailPrice"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["WholeSalePrice"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["CreditPrice"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["CardPrice"].Format = "N2";
                        // New columns formatting
                        Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].Format = "N2";
                        Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].Format = "N2";

                        // Set text alignment for numeric columns
                        Ult_Price.DisplayLayout.Bands[0].Columns["Cost"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["MarginAmt"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["MarginPer"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["TaxPer"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["TaxAmt"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["MRP"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["RetailPrice"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["WholeSalePrice"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["CreditPrice"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["CardPrice"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;

                        // Set column headers and widths for the new columns
                        Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].Header.Caption = "Staff Price";
                        Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].Header.Caption = "Min Price";
                        Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].Width = 100;
                        Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].Width = 100;
                    }
                }

                // Find ultraGrid2 control
                Infragistics.Win.UltraWinGrid.UltraGrid ultraGrid2 =
                    FindControlRecursive(ItemMaster, "ultraGrid2") as Infragistics.Win.UltraWinGrid.UltraGrid;

                // Always create a fresh empty DataTable for vendor details
                DataTable dtVendors = new DataTable();
                dtVendors.Columns.Add("LedgerID", typeof(int));
                dtVendors.Columns.Add("VendorName", typeof(string));
                dtVendors.Columns.Add("Cost", typeof(double));
                dtVendors.Columns.Add("Unit", typeof(string));
                dtVendors.Columns.Add("InvoiceDate", typeof(DateTime));
                dtVendors.Columns.Add("PurchaseNo", typeof(int));
                dtVendors.Columns.Add("InvoiceNo", typeof(string));
                dtVendors.Columns.Add("Pid", typeof(int));

                // Bind vendor details to ultraGrid2 if available
                if (getItem.ListVendor != null && getItem.ListVendor.Length > 0)
                {
                    try
                    {
                        // Add vendor details to the DataTable
                        foreach (var vendor in getItem.ListVendor)
                        {
                            DataRow row = dtVendors.NewRow();
                            row["LedgerID"] = vendor.LedgerID;
                            row["VendorName"] = vendor.VendorName;
                            row["Cost"] = vendor.Cost;
                            row["Unit"] = vendor.Unit;
                            row["InvoiceDate"] = vendor.InvoiceDate;
                            row["PurchaseNo"] = vendor.PurchaseNo;
                            row["InvoiceNo"] = vendor.InvoiceNo;
                            int pid = vendor.Pid;
                            if (pid <= 0)
                            {
                                if (ItemMaster != null)
                                {
                                    pid = ItemMaster.ResolvePidFromPurchaseNo(vendor.PurchaseNo);
                                }
                                else
                                {
                                    pid = ResolvePidFromPurchaseNo(vendor.PurchaseNo);
                                }
                            }
                            row["Pid"] = pid;
                            dtVendors.Rows.Add(row);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error binding vendor details: {ex.Message}");
                    }
                }

                // Set the DataSource for ultraGrid2 regardless of whether there are vendor details or not
                if (ultraGrid2 != null)
                {
                    ultraGrid2.DataSource = dtVendors;

                    // Configure columns if needed
                    if (ultraGrid2.DisplayLayout.Bands.Count > 0)
                    {
                        ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].Format = "N2";
                        ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Format = "dd/MM/yyyy";
                        ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;

                        // Set column headers
                        ultraGrid2.DisplayLayout.Bands[0].Columns["LedgerID"].Header.Caption = "Ledger ID";
                        ultraGrid2.DisplayLayout.Bands[0].Columns["VendorName"].Header.Caption = "Vendor Name";
                        ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].Header.Caption = "Cost";
                        ultraGrid2.DisplayLayout.Bands[0].Columns["Unit"].Header.Caption = "Unit";
                        ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Header.Caption = "Invoice Date";
                        ultraGrid2.DisplayLayout.Bands[0].Columns["PurchaseNo"].Header.Caption = "Purchase No";
                        ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceNo"].Header.Caption = "Invoice No";

                        // Set column widths
                        ultraGrid2.DisplayLayout.Bands[0].Columns["VendorName"].Width = 150;
                        ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceNo"].Width = 120;
                        ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Width = 100;

                        // Hide LedgerID column if needed
                        ultraGrid2.DisplayLayout.Bands[0].Columns["LedgerID"].Hidden = true;
                    }

                    // Refresh the grid
                    ultraGrid2.Refresh();
                }

                // Load markup percentage to textBox1 if available
                // textBox1 is used for markup percentage, not margin percentage
                if (baseUnitList.Length > 0 && ItemMaster.textBox1 != null)
                {
                    var baseUnit = baseUnitList[0];
                    double unitCost = baseUnit.Cost;
                    double retailPrice = baseUnit.WholeSalePrice; // txt_Retail maps to WholeSalePrice

                    if (unitCost > 0 && retailPrice > 0)
                    {
                        // Calculate markup percentage: ((Retail Price / Unit Cost) - 1) * 100
                        double markupPercent = (retailPrice / unitCost - 1.0) * 100.0;
                        ItemMaster.textBox1.Text = markupPercent.ToString("0.00");
                    }
                    else
                    {
                        ItemMaster.textBox1.Text = "0.00";
                    }
                }

                // Set tax fields (type and percentage). Do not directly set txt_TaxAmount; recompute based on mode
                if (baseUnitList.Length > 0)
                {
                    ItemMaster.txt_TaxType.Text = baseUnitList[0].TaxType ?? ItemMaster.txt_TaxType.Text;
                    ItemMaster.txt_TaxPer.Text = baseUnitList[0].TaxPer.ToString();
                }

                // After tax fields and prices are set, recompute total-with-tax and tax-only display
                try { ItemMaster.RecomputeTaxAmountFromRetailAndTax(); } catch { }

                // Load markdown and selling price fields for base unit
                if (baseUnitList != null && baseUnitList.Length > 0)
                {
                    var baseUnit = baseUnitList[0];

                    // MD Walkin (markdown for walking price)
                    var mdWalkinCtrl = FindControlRecursive(ItemMaster, "ultraTextEditor16") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (mdWalkinCtrl != null)
                    {
                        mdWalkinCtrl.Text = baseUnit.MDWalkinPrice.ToString("0.00");
                    }

                    // MD Credit (markdown for credit price)
                    var mdCreditCtrl = FindControlRecursive(ItemMaster, "ultraTextEditor15") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (mdCreditCtrl != null)
                    {
                        mdCreditCtrl.Text = baseUnit.MDCreditPrice.ToString("0.00");
                    }

                    // MD MRP (markdown for MRP price) - ultraTextEditor14
                    var mdMrpCtrl = FindControlRecursive(ItemMaster, "ultraTextEditor14") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (mdMrpCtrl != null)
                    {
                        mdMrpCtrl.Text = baseUnit.MDMrpPrice.ToString("0.00");
                    }

                    // MD Card (markdown for card price)
                    var mdCardCtrl = FindControlRecursive(ItemMaster, "ultraTextEditor13") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (mdCardCtrl != null)
                    {
                        mdCardCtrl.Text = baseUnit.MDCardPrice.ToString("0.00");
                    }

                    // Staff price + markdown + margin
                    var txt_SF = FindControlRecursive(ItemMaster, "txt_SF") as Control;
                    var mdStaffCtrl = FindControlRecursive(ItemMaster, "ultraTextEditor12") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var staffMarginCtrl = FindControlRecursive(ItemMaster, "ultraTextEditor6") as Infragistics.Win.UltraWinEditors.UltraTextEditor;

                    // Fill Staff fields; if StaffPrice is 0 try deriving from Retail and MDStaffPrice
                    double staffPrice = baseUnit.StaffPrice;
                    double cost = baseUnit.Cost;
                    double retailForCalc = baseUnit.WholeSalePrice; // master retail reference
                    if ((staffPrice <= 0) && retailForCalc > 0 && baseUnit.MDStaffPrice != 0)
                    {
                        // staffPrice = retail * (1 - md/100)
                        staffPrice = retailForCalc * (1.0 - (baseUnit.MDStaffPrice / 100.0));
                    }
                    if (txt_SF != null)
                    {
                        txt_SF.Text = staffPrice.ToString("0.000");
                    }
                    if (mdStaffCtrl != null)
                    {
                        mdStaffCtrl.Text = baseUnit.MDStaffPrice.ToString("0.00");
                    }
                    if (staffMarginCtrl != null)
                    {
                        double staffMarginPercent = 0;
                        if (cost > 0 && staffPrice > 0)
                        {
                            staffMarginPercent = ((staffPrice - cost) / staffPrice) * 100.0;
                        }
                        staffMarginCtrl.Text = staffMarginPercent.ToString("0.00");
                    }

                    // Min price + markdown + margin
                    var txt_MinP = FindControlRecursive(ItemMaster, "txt_MinP") as Control;
                    var mdMinCtrl = FindControlRecursive(ItemMaster, "ultraTextEditor11") as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var minMarginCtrl = FindControlRecursive(ItemMaster, "ultraTextEditor5") as Infragistics.Win.UltraWinEditors.UltraTextEditor;

                    // Fill Min fields; if MinPrice is 0 try deriving from Retail and MDMinPrice
                    double minPrice = baseUnit.MinPrice;
                    double cost2 = baseUnit.Cost;
                    if ((minPrice <= 0) && retailForCalc > 0 && baseUnit.MDMinPrice != 0)
                    {
                        // minPrice = retail * (1 - md/100)
                        minPrice = retailForCalc * (1.0 - (baseUnit.MDMinPrice / 100.0));
                    }
                    if (txt_MinP != null)
                    {
                        txt_MinP.Text = minPrice.ToString("0.000");
                    }
                    if (mdMinCtrl != null)
                    {
                        mdMinCtrl.Text = baseUnit.MDMinPrice.ToString("0.00");
                    }
                    if (minMarginCtrl != null)
                    {
                        double minMarginPercent = 0;
                        if (cost2 > 0 && minPrice > 0)
                        {
                            minMarginPercent = ((minPrice - cost2) / minPrice) * 100.0;
                        }
                        minMarginCtrl.Text = minMarginPercent.ToString("0.00");
                    }
                }

                // Update hold quantity from hold details
                ItemMaster.UpdateHoldQuantityFromHoldDetails();

                // Final refresh to make sure all values are displayed
                ItemMaster.Refresh();
                Application.DoEvents();

                // Set button visibility for update mode
                ItemMaster.btnUpdate.Visible = true;
                ItemMaster.button3.Visible = false;

                // Load profit margin values for each price type
                LoadProfitMarginValues(ItemMaster, baseUnitList);

                // Reset loading flag to allow normal synchronization
                ItemMaster.SetLoadingFlag(false);

                // Close the dialog
                this.Close();
            }
            catch (Exception ex)
            {
                // Reset loading flag in case of error
                ItemMaster.SetLoadingFlag(false);
                MessageBox.Show($"Error loading item data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ultraPictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have data to sort
                if (ultraGrid1.DataSource == null || ultraGrid1.Rows.Count == 0)
                {
                    UpdateStatus("No data to sort");
                    return;
                }

                // Toggle between original order and reverse order
                isOriginalOrder = !isOriginalOrder;

                // Update status message
                if (isOriginalOrder)
                {
                    UpdateStatus("Displaying items in original order (first to last)");
                }
                else
                {
                    UpdateStatus("Displaying items in reverse order (last to first)");
                }

                // Suspend layout while sorting
                ultraGrid1.SuspendLayout();

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

                // Resume layout
                ultraGrid1.ResumeLayout();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error during sort: {ex.Message}");
            }
        }

        private void lblSearch_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // Helper method to configure visible columns while preserving layout
        private void ConfigureVisibleColumnsPreserveLayout(Dictionary<string, int> columnWidths, Dictionary<string, int> columnPositions)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                // First, hide all columns
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    col.Hidden = true;
                }

                // Define the columns to show in the specified order
                    // Define the columns to show in the specified order
                    List<string> columnsList = new List<string> { "BarCode", "Description", "UnitId", "Unit", "Cost", "RetailPrice", "Stock" };
                    if (FormName == "frmSalesInvoice")
                    {
                        columnsList.Remove("Cost");
                    }
                    string[] columnsToShow = columnsList.ToArray();

                // Show and configure only the specified columns in the specified order
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    string colKey = columnsToShow[i];

                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey))
                    {
                        Infragistics.Win.UltraWinGrid.UltraGridColumn col = ultraGrid1.DisplayLayout.Bands[0].Columns[colKey];
                        col.Hidden = false;

                        // Preserve the position if we have it stored
                        if (columnPositions.ContainsKey(colKey))
                        {
                            col.Header.VisiblePosition = columnPositions[colKey];
                        }
                        else
                        {
                            col.Header.VisiblePosition = i; // Set the default order
                        }

                        // Configure specific columns
                        switch (colKey)
                        {
                            case "BarCode":
                                col.Header.Caption = "Barcode";
                                // Preserve width if we have it stored
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 120;
                                break;
                            case "Description":
                                col.Header.Caption = "Item Name";
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 250;
                                break;
                            case "UnitId":
                                col.Header.Caption = "UnitId";
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 80;
                                break;
                            case "Unit":
                                col.Header.Caption = "Unit";
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 80;
                                break;
                            case "Cost":
                                col.Header.Caption = "Cost";
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 100;
                                col.Format = "N2";
                                col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                break;
                            case "RetailPrice":
                                col.Header.Caption = "Price";
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 100;
                                col.Format = "N2";
                                col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                break;
                            case "Stock":
                                col.Header.Caption = "Stock";
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 80;
                                col.Format = "N2";
                                col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                break;
                        }
                    }
                }
            }
        }

        // Initialize the search filter comboBox
        private void InitializeSearchFilterComboBox()
        {
            // Clear any existing items
            comboBox1.Items.Clear();

            // Add search filter options
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Barcode");
            comboBox1.Items.Add("Item Name");
            comboBox1.Items.Add("UnitID");
            comboBox1.Items.Add("Unit");

            // Select "Select all" by default
            comboBox1.SelectedIndex = 0;

            // Add event handler
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Handle comboBox1 selection change
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Re-apply the current filter with the new search column selection
            string searchText = textBoxsearch.Text.Trim();
            if (searchText == "Search items...")
                searchText = string.Empty;

            ApplyFilter(searchText);
        }

        // Initialize the column order comboBox
        private void InitializeColumnOrderComboBox()
        {
            // Clear any existing items
            comboBox2.Items.Clear();

            // Add column options for reordering (removed "Default Order")
            comboBox2.Items.Add("Barcode");
            comboBox2.Items.Add("Item Name");
            comboBox2.Items.Add("UnitId");
            comboBox2.Items.Add("Unit");

            // Select "Barcode" by default (since "Default Order" is removed)
            comboBox2.SelectedIndex = 0;

            // Add event handler
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        // Handle comboBox2 selection change (column ordering)
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the selected column option
                string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "Barcode";

                // Reorder columns based on selection
                ReorderColumns(selectedColumn);

                UpdateStatus($"Column order changed: {selectedColumn} is now first");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error changing column order: {ex.Message}");
            }
        }

        // Reorder columns based on selected option
        private void ReorderColumns(string selectedColumn)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            // PERFORMANCE: Check if already suspended before suspending again
            bool wasSuspended = false;
            try
            {
                // Store current column widths
                Dictionary<string, int> columnWidths = new Dictionary<string, int>();
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        columnWidths[col.Key] = col.Width;
                    }
                }

                // Suspend layout to prevent flickering (only if not already suspended)
                ultraGrid1.SuspendLayout();
                wasSuspended = true;

                // Define the columns to show in the specified order
                List<string> columnsToShow = new List<string> { "BarCode", "Description", "UnitId", "Unit", "Cost", "RetailPrice", "Stock" };
                if (FormName == "frmSalesInvoice")
                {
                    columnsToShow.Remove("Cost");
                }

                // Move selected column to the front (always do this since "Default Order" is removed)
                string columnKey = GetColumnKeyFromDisplayName(selectedColumn);
                if (!string.IsNullOrEmpty(columnKey) && columnsToShow.Contains(columnKey))
                {
                    columnsToShow.Remove(columnKey);
                    columnsToShow.Insert(0, columnKey);
                }

                // First, hide all columns
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    col.Hidden = true;
                }

                // Show and configure columns in the new order
                for (int i = 0; i < columnsToShow.Count; i++)
                {
                    string colKey = columnsToShow[i];

                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey))
                    {
                        Infragistics.Win.UltraWinGrid.UltraGridColumn col = ultraGrid1.DisplayLayout.Bands[0].Columns[colKey];
                        col.Hidden = false;
                        col.Header.VisiblePosition = i; // Set the order

                        // Preserve width if we have it stored
                        if (columnWidths.ContainsKey(colKey))
                        {
                            col.Width = columnWidths[colKey];
                        }

                        // Configure specific columns
                        ConfigureColumnAppearance(col, colKey);
                    }
                }
            }
            finally
            {
                // PERFORMANCE: Only resume if we suspended
                if (wasSuspended)
                {
                    ultraGrid1.ResumeLayout(false); // Don't force refresh here
                }
            }
        }

        // Helper method to configure column appearance
        private void ConfigureColumnAppearance(Infragistics.Win.UltraWinGrid.UltraGridColumn col, string colKey)
        {
            switch (colKey)
            {
                case "BarCode":
                    col.Header.Caption = "Barcode";
                    if (col.Width <= 0) col.Width = 120;
                    break;
                case "Description":
                    col.Header.Caption = "Item Name";
                    if (col.Width <= 0) col.Width = 250;
                    break;
                case "UnitId":
                    col.Header.Caption = "UnitId";
                    if (col.Width <= 0) col.Width = 80;
                    break;
                case "Unit":
                    col.Header.Caption = "Unit";
                    if (col.Width <= 0) col.Width = 80;
                    break;
                case "Cost":
                    col.Header.Caption = "Cost";
                    if (col.Width <= 0) col.Width = 100;
                    col.Format = "N2";
                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    break;
                case "RetailPrice":
                    col.Header.Caption = "Price";
                    if (col.Width <= 0) col.Width = 100;
                    col.Format = "N2";
                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    break;
                case "Stock":
                    col.Header.Caption = "Stock";
                    if (col.Width <= 0) col.Width = 80;
                    col.Format = "N2";
                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    break;
            }
        }

        // Helper method to convert display name to column key
        private string GetColumnKeyFromDisplayName(string displayName)
        {
            switch (displayName)
            {
                case "Barcode": return "BarCode";
                case "Item Name": return "Description";
                case "UnitId": return "UnitId";
                case "Unit": return "Unit";
                default: return "";
            }
        }

        // Helper method to configure visible columns
        private void ConfigureVisibleColumns()
        {
            // Create empty dictionaries since we don't have any stored values
            Dictionary<string, int> columnWidths = new Dictionary<string, int>();
            Dictionary<string, int> columnPositions = new Dictionary<string, int>();

            // Call the new method that preserves layout
            ConfigureVisibleColumnsPreserveLayout(columnWidths, columnPositions);
        }

        // Add hover effects to all panels and their child controls
        private void SetupPanelHoverEffects()
        {
            // Set up hover effects for each panel group
            SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);

            // Set up hover effect for ultraPictureBox4 (sort icon)
            if (ultraPictureBox4 != null)
            {
                // Set hand cursor to indicate clickable
                ultraPictureBox4.Cursor = Cursors.Hand;

                // Add tooltip effect
                ToolTip tooltip = new ToolTip();
                tooltip.SetToolTip(ultraPictureBox4, "Click to toggle between original order and reverse order");

                // Add visual hover effect
                ultraPictureBox4.MouseEnter += (s, e) =>
                {
                    // Make the control appear brighter on hover
                    ultraPictureBox4.Appearance.BorderColor = Color.FromArgb(0, 120, 215);
                    ultraPictureBox4.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                };

                ultraPictureBox4.MouseLeave += (s, e) =>
                {
                    // Restore normal appearance
                    ultraPictureBox4.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
                };
            }

            UpdateStatus("Panel hover effects initialized");
        }

        // Set up hover effects for a panel group (panel, label, picturebox)
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

                // Update status
                UpdateStatus($"Hovering over {panel.Name}");
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

                    // No changes to label text style or color as requested
                };

                label.MouseLeave += (s, e) =>
                {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel))
                    {
                        removeHoverEffect();
                    }

                    // No need to restore label style since we didn't change it
                };
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

        // Helper method to check if mouse is over a control
        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        // Method to connect click events for ultraPanel4, ultraPictureBox3, and label4
        private void ConnectItemMasterClickEvents()
        {
            try
            {
                // Connect click events for panel, picture box, and label
                ultraPanel4.Click += OpenItemMasterForm;
                ultraPictureBox3.Click += OpenItemMasterForm;
                label4.Click += OpenItemMasterForm;

                // Also connect their client area click events
                ultraPanel4.ClientArea.Click += OpenItemMasterForm;

                // Connect navigation panel events - use MouseDown for instant response
                ConnectNavigationPanelEvents(ultraPanel3, ultraPictureBox5, MoveItemHighlighterUp);
                ConnectNavigationPanelEvents(ultraPanel7, ultraPictureBox6, MoveItemHighlighterDown);

                // Connect OK button click events (ultraPanel5)
                ultraPanel5.Click += PerformOKAction;
                ultraPanel5.ClientArea.Click += PerformOKAction;
                ultraPictureBox1.Click += PerformOKAction;
                label5.Click += PerformOKAction;

                // Connect Close button click events (ultraPanel6)
                ultraPanel6.Click += PerformCloseAction;
                ultraPanel6.ClientArea.Click += PerformCloseAction;
                ultraPictureBox2.Click += PerformCloseAction;
                label3.Click += PerformCloseAction;

                // Update status
                UpdateStatus("Item Master click events connected");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error connecting click events: {ex.Message}");
            }
        }

        // Event handler for OK button (ultraPanel5) click
        private void PerformOKAction(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Log to debug
                    System.Diagnostics.Debug.WriteLine($"OK button clicked in frmdialForItemMaster. FormName: {FormName}");

                    CaptureSelectedItemData();

                    if (FormName == "FrmStockAdjustment")
                    {
                        SendItemToStockAdjustment();
                    }
                    else if (FormName == "FrmStockTransfer")
                    {
                        SendItemToStockTransfer();
                    }
                    else if (FormName == "frmSalesReturn" || FormName == "frmSalesInvoice")
                    {
                        SendItemToSalesReturn();
                    }
                    else if (FormName == "FromPurchase")
                    {
                        SendItemToPurchase();
                    }
                    else if (FormName == "FromItemMaster")
                    {
                        LoadItemMasterData();
                    }
                    else if (FormName == "FromPurchaseReturn")
                    {
                        SendItemToPurchaseReturn();
                    }
                    else if (FormName == "FrmBarcode" || FormName == "ItemStockActivity" || FormName == "ItemHistoryLog")
                    {
                        // Handle barcode form / item stock activity selection
                        System.Diagnostics.Debug.WriteLine($"OK button clicked, selecting item for {FormName}");
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        // Default: just close with OK result
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Please select an item first.", "No Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error performing OK action: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in PerformOKAction: {ex.Message}");
            }
        }

        // Event handler for Close button (ultraPanel6) click
        private void PerformCloseAction(object sender, EventArgs e)
        {
            try
            {
                // Log to debug
                System.Diagnostics.Debug.WriteLine("Close button clicked in frmdialForItemMaster");

                // Set dialog result to Cancel and close the form
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in PerformCloseAction: {ex.Message}");
            }
        }

        // Helper method to connect navigation panel events with visual feedback
        private void ConnectNavigationPanelEvents(Infragistics.Win.Misc.UltraPanel panel,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox,
            EventHandler handler)
        {
            // Store original colors for visual feedback
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;

            // Create brighter colors for feedback
            Color pressedBackColor = BrightenColor(originalBackColor, 30);
            Color pressedBackColor2 = BrightenColor(originalBackColor2, 30);

            // Connect the click event instead of MouseDown for smoother operation
            panel.Click += handler;
            panel.ClientArea.Click += handler;
            if (pictureBox != null)
            {
                pictureBox.Click += handler;
            }

            // Add visual feedback without causing the handler to fire multiple times
            panel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Change color for visual feedback only
                    panel.Appearance.BackColor = pressedBackColor;
                    panel.Appearance.BackColor2 = pressedBackColor2;
                }
            };

            panel.MouseUp += (s, e) =>
            {
                // Restore original colors
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };

            // Same for client area
            panel.ClientArea.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    panel.Appearance.BackColor = pressedBackColor;
                    panel.Appearance.BackColor2 = pressedBackColor2;
                }
            };

            panel.ClientArea.MouseUp += (s, e) =>
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };

            // Same for picture box
            if (pictureBox != null)
            {
                pictureBox.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        panel.Appearance.BackColor = pressedBackColor;
                        panel.Appearance.BackColor2 = pressedBackColor2;
                    }
                };

                pictureBox.MouseUp += (s, e) =>
                {
                    panel.Appearance.BackColor = originalBackColor;
                    panel.Appearance.BackColor2 = originalBackColor2;
                };
            }
        }

        // Event handler to move the item highlighter up
        private void MoveItemHighlighterUp(object sender, EventArgs e)
        {
            try
            {
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
                        ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                    }
                    return;
                }

                int currentIndex = ultraGrid1.ActiveRow.Index;
                if (currentIndex > 0)
                {
                    // Get the row to activate before making any changes
                    UltraGridRow rowToActivate = ultraGrid1.Rows[currentIndex - 1];

                    // Ensure the row will be visible before activating it
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);

                    // Now activate the row and update selection
                    ultraGrid1.ActiveRow = rowToActivate;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(rowToActivate);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error moving highlighter up: {ex.Message}");
            }
        }

        // Event handler to move the item highlighter down
        private void MoveItemHighlighterDown(object sender, EventArgs e)
        {
            try
            {
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
                        ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                    }
                    return;
                }

                int currentIndex = ultraGrid1.ActiveRow.Index;
                if (currentIndex < ultraGrid1.Rows.Count - 1)
                {
                    // Get the row to activate before making any changes
                    UltraGridRow rowToActivate = ultraGrid1.Rows[currentIndex + 1];

                    // Ensure the row will be visible before activating it
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);

                    // Now activate the row and update selection
                    ultraGrid1.ActiveRow = rowToActivate;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(rowToActivate);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error moving highlighter down: {ex.Message}");
            }
        }

        // Event handler to open the ItemMaster form
        private void OpenItemMasterForm(object sender, EventArgs e)
        {
            try
            {
                UpdateStatus("Opening Item Master form...");

                // Create a new instance of the ItemMaster form
                frmItemMasterNew itemMasterForm = new frmItemMasterNew();

                // Set the form to open maximized
                itemMasterForm.WindowState = FormWindowState.Maximized;

                // Set the form's position to center screen
                itemMasterForm.StartPosition = FormStartPosition.CenterScreen;

                // Set TopMost property to ensure it appears in front of all other forms
                itemMasterForm.TopMost = true;

                // Hide this form first to avoid flickering
                this.Hide();

                // Process any pending messages
                Application.DoEvents();

                // Show the form
                itemMasterForm.Show();

                // Activate the form to ensure it has focus
                itemMasterForm.Activate();

                // Bring it to the front
                itemMasterForm.BringToFront();

                // Use BeginInvoke to reset TopMost after a short delay
                // This ensures the form appears on top initially but doesn't stay permanently on top
                itemMasterForm.BeginInvoke(new Action(() =>
                {
                    // Process any pending messages
                    Application.DoEvents();

                    // Short delay to ensure visibility
                    System.Threading.Thread.Sleep(50);

                    // Reset TopMost
                    itemMasterForm.TopMost = false;

                    // Ensure it's still active and in front
                    itemMasterForm.Activate();
                    itemMasterForm.BringToFront();
                }));

                // Close this form after ensuring the ItemMaster form is shown
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

                UpdateStatus($"Error opening Item Master form: {ex.Message}");
                MessageBox.Show($"Error opening Item Master form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        // Add a method to update the record count label
        private void UpdateRecordCountLabel()
        {
            if (fullDataTable != null)
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

                int totalCount = fullDataTable.Rows.Count;

                // Get the current limit from textBox1 (if it's used for record limiting)
                int currentLimit = currentDisplayCount;

                // Update label1 with the current count and total count
                if (this.Controls.Find("label1", true).Length > 0)
                {
                    Label label1 = this.Controls.Find("label1", true)[0] as Label;
                    if (label1 != null)
                    {
                        label1.Text = $"Showing {currentLimit} of {totalCount} records";
                        label1.AutoSize = true;
                        label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                        label1.ForeColor = Color.FromArgb(0, 70, 170); // Dark blue color
                    }
                }
            }
        }

        // Add a new method to send data to Stock Adjustment form
        private void SendItemToStockAdjustment()
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Get the parent form
                    var parentForm = this.Owner as PosBranch_Win.Transaction.FrmStockAdjustment;
                    if (parentForm != null)
                    {
                        // Get the required values from the selected row
                        string itemId = ultraGrid1.ActiveRow.Cells["ItemId"].Value?.ToString() ?? "";
                        string barcode = ultraGrid1.ActiveRow.Cells["BarCode"].Value?.ToString() ?? "";
                        string description = ultraGrid1.ActiveRow.Cells["Description"].Value?.ToString() ?? "";
                        string unit = ultraGrid1.ActiveRow.Cells["Unit"].Value?.ToString() ?? "";

                        // Get the stock value if it exists
                        string stockQty = "0";
                        if (ultraGrid1.ActiveRow.Cells.Exists("Stock") &&
                            ultraGrid1.ActiveRow.Cells["Stock"].Value != null)
                        {
                            stockQty = ultraGrid1.ActiveRow.Cells["Stock"].Value.ToString();
                        }

                        // Log what we're sending to the Stock Adjustment form
                        System.Diagnostics.Debug.WriteLine($"Sending to Stock Adjustment: ItemId={itemId}, Description={description}, Stock={stockQty}");

                        // Add item to the parent form's grid - default adjustment qty is 1
                        parentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);

                        // Set dialog result and close
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Parent form is not FrmStockAdjustment");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending item to Stock Adjustment: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in SendItemToStockAdjustment: {ex.Message}");
            }
        }

        // Add a new method to send data to Stock Transfer form
        private void SendItemToStockTransfer()
        {
            try
            {
                var parentForm = this.Owner as PosBranch_Win.Transaction.FrmStockTransfer;
                if (parentForm != null)
                {
                    int itemsAdded = 0;
                    if (ultraGrid1.Selected.Rows.Count > 0)
                    {
                        foreach (var selectedRow in ultraGrid1.Selected.Rows)
                        {
                            AddSingleRowToTransfer(selectedRow, parentForm);
                            itemsAdded++;
                        }
                        ultraGrid1.Selected.Rows.Clear();
                    }
                    else if (ultraGrid1.ActiveRow != null)
                    {
                        AddSingleRowToTransfer(ultraGrid1.ActiveRow, parentForm);
                        itemsAdded++;
                    }

                    if (itemsAdded > 0)
                    {
                        UpdateStatus($"Added {itemsAdded} item(s) to transfer.");
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending item to Stock Transfer: {ex.Message}");
            }
        }

        private void AddSingleRowToTransfer(UltraGridRow row, PosBranch_Win.Transaction.FrmStockTransfer parentForm)
        {
            string itemId = row.Cells["ItemId"].Value?.ToString() ?? "";
            string barcode = row.Cells["BarCode"].Value?.ToString() ?? "";
            string description = row.Cells["Description"].Value?.ToString() ?? "";
            string unit = row.Cells["Unit"].Value?.ToString() ?? "";
            string rate = "0";
            if (row.Cells.Exists("RetailPrice") && row.Cells["RetailPrice"].Value != null)
            {
                rate = row.Cells["RetailPrice"].Value.ToString();
            }
            else if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null)
            {
                rate = row.Cells["Cost"].Value.ToString();
            }

            int unitId = 0;
            if (row.Cells.Exists("UnitId") && row.Cells["UnitId"].Value != null)
            {
                int.TryParse(row.Cells["UnitId"].Value.ToString(), out unitId);
            }

            parentForm.AddItemToGrid(itemId, barcode, description, unit, rate, unitId);
        }

        private void textBoxsearch_KeyDown(object sender, KeyEventArgs e)
        {
            // When Enter key is pressed in search box, move focus to grid
            if (e.KeyCode == Keys.Enter)
            {
                // Move focus to grid
                ultraGrid1.Focus();

                // If grid has rows, select first row
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                }

                // Mark event as handled
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                // Move focus to grid on Down arrow as well
                ultraGrid1.Focus();

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // Dedicated method for sending items to Purchase form
        private void SendItemToPurchase()
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    if (!EnsureSelectedRowAllowedForCurrentDialog(ultraGrid1.ActiveRow))
                    {
                        return;
                    }

                    // Get the parent form
                    var parentForm = this.Owner as PosBranch_Win.Transaction.FrmPurchase;
                    if (parentForm != null)
                    {
                        // Get the required values from the selected row
                        string itemId = ultraGrid1.ActiveRow.Cells["ItemId"].Value?.ToString() ?? "";
                        string itemName = ultraGrid1.ActiveRow.Cells["Description"].Value?.ToString() ?? "";
                        string barcode = ultraGrid1.ActiveRow.Cells["BarCode"].Value?.ToString() ?? "";
                        string unit = ultraGrid1.ActiveRow.Cells["Unit"].Value?.ToString() ?? "";

                        decimal retailPrice = 0;
                        if (ultraGrid1.ActiveRow.Cells["RetailPrice"].Value != null)
                        {
                            decimal.TryParse(ultraGrid1.ActiveRow.Cells["RetailPrice"].Value.ToString(), out retailPrice);
                        }

                        decimal cost = 0;
                        if (ultraGrid1.ActiveRow.Cells["Cost"].Value != null)
                        {
                            decimal.TryParse(ultraGrid1.ActiveRow.Cells["Cost"].Value.ToString(), out cost);
                        }

                        // Get UnitId if available
                        int unitId = 0;
                        if (ultraGrid1.ActiveRow.Cells.Exists("UnitId") && ultraGrid1.ActiveRow.Cells["UnitId"].Value != null)
                        {
                            int.TryParse(ultraGrid1.ActiveRow.Cells["UnitId"].Value.ToString(), out unitId);
                        }

                        // Get tax-related values
                        float taxPer = 0;
                        if (ultraGrid1.ActiveRow.Cells.Exists("TaxPer") && ultraGrid1.ActiveRow.Cells["TaxPer"].Value != null)
                        {
                            float.TryParse(ultraGrid1.ActiveRow.Cells["TaxPer"].Value.ToString(), out taxPer);
                        }

                        float taxAmt = 0;
                        if (ultraGrid1.ActiveRow.Cells.Exists("TaxAmt") && ultraGrid1.ActiveRow.Cells["TaxAmt"].Value != null)
                        {
                            float.TryParse(ultraGrid1.ActiveRow.Cells["TaxAmt"].Value.ToString(), out taxAmt);
                        }

                        string taxType = "";
                        if (ultraGrid1.ActiveRow.Cells.Exists("TaxType") && ultraGrid1.ActiveRow.Cells["TaxType"].Value != null)
                        {
                            taxType = ultraGrid1.ActiveRow.Cells["TaxType"].Value.ToString();
                        }

                        // Add item to the parent form's grid, passing the unitId and tax data
                        parentForm.AddItemToGrid(itemId, itemName, barcode, unit, retailPrice, 1, cost, unitId, taxPer, taxAmt, taxType);

                        // Set dialog result and close
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending item to Purchase form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in SendItemToPurchase: {ex.Message}");
            }
        }

        // Add a helper method to find a control recursively by name
        private Control FindControlRecursive(Control container, string name)
        {
            if (container == null)
                return null;
            if (container.Name == name)
                return container;
            foreach (Control ctrl in container.Controls)
            {
                Control found = FindControlRecursive(ctrl, name);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}



