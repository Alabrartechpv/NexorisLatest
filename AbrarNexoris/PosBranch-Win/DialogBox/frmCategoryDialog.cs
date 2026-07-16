using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Master;
using PosBranch_Win.Transaction;
using Repository;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PosBranch_Win.DialogBox
{

    public partial class frmCategoryDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        public TextBox TargetTextBox { get; set; }
        public int SelectedCategoryId { get; private set; }
        public string SelectedCategoryName { get; private set; }
        string formname;
        FrmStockAdjustment stockadj = new FrmStockAdjustment();
        // Add fields for search/filter controls
        private DataTable fullDataTable = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;

        // Add status label for debugging
        private Label lblStatus;

        // Add a class-level variable to store the column chooser form
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;

        // Add a tooltip control for user feedback
        private ToolTip toolTip = new ToolTip();

        public frmCategoryDialog(string formname1)
        {
            InitializeComponent();
            formname = formname1;

            // Initialize status label
            InitializeStatusLabel();

            // Set up the grid style
            SetupUltraGridStyle();

            // Set up panel hover effects
            SetupPanelHoverEffects();

            // Connect event handlers
            ConnectEventHandlers();

            // Set up the column chooser right-click menu
            SetupColumnChooserMenu();

            // Initialize tooltip
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            // Set form properties
            this.Text = "Category Selection";
            this.KeyPreview = true;

            // Register form closing event to clean up resources
            this.FormClosing += FrmCategoryDialog_FormClosing;

            // Use Shown event to reliably set focus to search box after form is fully visible
            this.Shown += (s, e) =>
            {
                textBoxsearch.Text = "";
                this.ActiveControl = textBoxsearch;
                textBoxsearch.Focus();
            };
        }

        private void FrmCategoryDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close the column chooser form if it exists
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
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

        // Track mouse position and column for drag
        private Point startPoint;
        private UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;

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

        // Event handler for the column chooser menu item
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
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

        // Event handler for drag over in the grid
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            // Allow dropping ColumnItems onto the grid
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
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
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                if (item != null)
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

                        // Show feedback
                        toolTip.Show($"Column '{item.DisplayText}' restored",
                                ultraGrid1,
                                ultraGrid1.PointToClient(Control.MousePosition),
                                2000);
                    }
                }
            }
        }

        // Method to create the column chooser form without showing it
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
            columnChooserListBox.DrawItem += (s, evt) => {
                if (evt.Index < 0) return;

                // Get the column item
                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;

                // Calculate the item rectangle
                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);

                // Draw the background
                Color bgColor = Color.FromArgb(33, 150, 243);

                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    // Create rounded rectangle
                    using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        int radius = 4;
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

        // Method to populate the column chooser listbox with hidden columns
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;

            // Clear existing items
            columnChooserListBox.Items.Clear();

            // Define the standard columns that should be tracked in the column chooser
            string[] standardColumns = new string[] { "Id", "CategoryName" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
                {
                    { "Id", "ID" },
                    { "CategoryName", "Category Name" }
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

                    // Start drag operation
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
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

        // Method to show the column chooser form
        private void ShowColumnChooser()
        {
            // If the column chooser form already exists, just show it
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
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
            lblStatus.Visible = false; // Hide by default, can be enabled via key combo for debugging
            lblStatus.DoubleClick += (s, e) => { lblStatus.Visible = !lblStatus.Visible; }; // Toggle visibility on double-click
            this.Controls.Add(lblStatus);

            // Add a keyboard shortcut to toggle visibility
            this.KeyDown += (s, e) => {
                if (e.Control && e.Alt && e.KeyCode == Keys.D) // Ctrl+Alt+D
                {
                    lblStatus.Visible = !lblStatus.Visible;
                    UpdateStatus("Debug mode " + (lblStatus.Visible ? "enabled" : "disabled"));

                    // Also toggle debug button visibility
                    Control debugBtn = this.Controls.Find("btnDebug", true).FirstOrDefault();
                    if (debugBtn != null)
                        debugBtn.Visible = lblStatus.Visible;
                }
            };
        }

        // Update status without interrupting the user
        private void UpdateStatus(string message)
        {
            if (lblStatus != null && lblStatus.Visible)
            {
                lblStatus.Text = message;
                lblStatus.Update();
            }
        }

        private void ConnectEventHandlers()
        {
            // Connect grid events
            ultraGrid1.DoubleClickCell += ultraGrid1_DoubleClickCell;
            ultraGrid1.KeyPress += ultraGrid1_KeyPress;

            // Connect search box events
            textBoxsearch.TextChanged += textBoxsearch_TextChanged;
            textBoxsearch.KeyDown += textBoxsearch_KeyDown;

            // Add placeholder text behavior to search box
            textBoxsearch.Text = "Search categories...";
            textBoxsearch.GotFocus += (s, e) => {
                if (textBoxsearch.Text == "Search categories...")
                    textBoxsearch.Text = "";
            };
            textBoxsearch.LostFocus += (s, e) => {
                if (string.IsNullOrEmpty(textBoxsearch.Text))
                    textBoxsearch.Text = "Search categories...";
            };

            // Connect panel click events
            ultraPanel5.Click += (s, e) => SelectCategory(); // OK button
            ultraPictureBox1.Click += (s, e) => SelectCategory(); // OK button icon
            label5.Click += (s, e) => SelectCategory(); // OK button label

            ultraPanel6.Click += (s, e) => this.Close(); // Close button
            ultraPictureBox2.Click += (s, e) => this.Close(); // Close button icon
            label3.Click += (s, e) => this.Close(); // Close button label

            // Connect navigation panel events
            ultraPanel3.Click += (s, e) => MoveSelectionUp(); // Up arrow panel
            ultraPictureBox5.Click += (s, e) => MoveSelectionUp(); // Up arrow icon

            ultraPanel7.Click += (s, e) => MoveSelectionDown(); // Down arrow panel
            ultraPictureBox6.Click += (s, e) => MoveSelectionDown(); // Down arrow icon

            // Connect ultraPanel50 click events to open FrmCategory
            ultraPanel50.Click += UltraPanel50_Click;
            ultraPictureBox3.Click += UltraPanel50_Click;
            label4.Click += UltraPanel50_Click;

            // Add row order toggle function for ultraPictureBox4 (if available)
            if (ultraPictureBox4 != null)
            {
                ultraPictureBox4.Click += ultraPictureBox4_Click;

                // Add tooltip for sort icon
                toolTip.SetToolTip(ultraPictureBox4, "Click to toggle between original order and reverse order");

                // Add visual hover effect
                ultraPictureBox4.MouseEnter += (s, e) => {
                    // Make the control appear brighter on hover
                    ultraPictureBox4.Appearance.BorderColor = Color.FromArgb(0, 120, 215);
                    ultraPictureBox4.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                };

                ultraPictureBox4.MouseLeave += (s, e) => {
                    // Restore normal appearance
                    ultraPictureBox4.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
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
                UpdateStatus($"Error preserving row order: {ex.Message}");
            }
        }

        // Event handler for the sort icon click
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
                    UpdateStatus("Displaying categories in original order (first to last)");
                }
                else
                {
                    UpdateStatus("Displaying categories in reverse order (last to first)");
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
                        // Fallback to sorting by the ID column if OriginalRowOrder doesn't exist
                        if (isOriginalOrder)
                        {
                            dataView.Sort = "Id ASC";
                        }
                        else
                        {
                            dataView.Sort = "Id DESC";
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

        // Helper methods for navigation
        private void MoveSelectionUp()
        {
            if (ultraGrid1.Rows.Count == 0) return;

            // If no active row, select the last row
            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                return;
            }

            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex - 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
            }
        }

        private void MoveSelectionDown()
        {
            if (ultraGrid1.Rows.Count == 0) return;

            // If no active row, select the first row
            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                return;
            }

            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex < ultraGrid1.Rows.Count - 1)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex + 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
            }
        }

        private void frmCategoryDialog_Load(object sender, EventArgs e)
        {
            try
            {
                // Setup search/filter combos
                InitializeSearchFilterComboBox();
                InitializeColumnOrderComboBox();

                // Load data
                LoadAllData();

                // Apply initial filter
                ApplyFilter("");

                // Update record count
                UpdateRecordCountLabel();

                // Set ActiveControl to search box (Shown event will also ensure focus)
                this.ActiveControl = textBoxsearch;

                // Initialize saved column widths
                InitializeSavedColumnWidths();

                // Add debug button (hidden by default)
                AddDebugButton();
                Control debugBtn = this.Controls.Find("btnDebug", true).FirstOrDefault();
                if (debugBtn != null)
                    debugBtn.Visible = lblStatus.Visible; // Only show if debug mode is enabled

                UpdateStatus("Form loaded successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Error loading form: {ex.Message}");
            }
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

            // Register events to preserve column widths
            ultraGrid1.AfterRowsDeleted += UltraGrid1_AfterRowsDeleted;
            ultraGrid1.AfterSortChange += UltraGrid1_AfterSortChange;
            ultraGrid1.AfterColPosChanged += UltraGrid1_AfterColPosChanged;

            // Add handler for form resize to preserve column widths
            this.SizeChanged += FrmCategoryDialog_SizeChanged;
            ultraGrid1.Resize += UltraGrid1_Resize;
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
            PreserveColumnWidths();
        }

        private void FrmCategoryDialog_SizeChanged(object sender, EventArgs e)
        {
            // Preserve column widths when form is resized
            PreserveColumnWidths();
        }

        private void UltraGrid1_Resize(object sender, EventArgs e)
        {
            // Preserve column widths when grid is resized
            PreserveColumnWidths();
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

                // Configure selected row appearance with light blue highlight
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue
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

                // Auto-fit settings - DISABLE to prevent column resizing during filtering
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;

                // Disable automatic column sizing
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.None;

                // Disable filter indicators
                ultraGrid1.DisplayLayout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;

                // Refresh the grid to apply all changes
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }

        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Category Name");
            comboBox1.Items.Add("ID");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, e) => ApplyFilter(textBoxsearch.Text.Trim());
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Category Name");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += (s, e) => ApplyFilter(textBoxsearch.Text.Trim());
        }

        private void LoadAllData()
        {
            try
            {
                UpdateStatus("Loading category data...");

                // Create the datatable structure
                fullDataTable = new DataTable();
                fullDataTable.Columns.Add("Id", typeof(int));
                fullDataTable.Columns.Add("CategoryName", typeof(string));

                // Add a column to preserve the original row order
                PreserveOriginalRowOrder(fullDataTable);

                // Get data from the database
                DataBase.Operations = "Category";
                CategoryDDlGrid category = drop.getCategoryDDl("");

                // Fill the datatable with data
                int rowIndex = 0;
                foreach (var item in category.List)
                {
                    DataRow newRow = fullDataTable.NewRow();
                    newRow["Id"] = item.Id;
                    newRow["CategoryName"] = item.CategoryName;
                    newRow["OriginalRowOrder"] = rowIndex++;
                    fullDataTable.Rows.Add(newRow);
                }

                // Set the initial data source for the grid
                ultraGrid1.DataSource = fullDataTable;

                // Update the form title with record count
                this.Text = $"Category Selection - {fullDataTable.Rows.Count} categories loaded";

                // Configure columns
                ConfigureGridColumns();

                // Initialize saved column widths
                InitializeSavedColumnWidths();

                UpdateStatus($"Loaded {fullDataTable.Rows.Count} categories");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
                UpdateStatus($"Error loading data: {ex.Message}");
            }
        }

        // Add debug button for testing functionality
        private void AddDebugButton()
        {
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

        // Debug button click event handler
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

                MessageBox.Show(debugInfo, "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Debug error: {ex.Message}");
            }
        }

        // Override OnFormClosing to ensure proper cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Unregister event handlers to prevent memory leaks
                if (ultraGrid1 != null)
                {
                    ultraGrid1.DoubleClickCell -= ultraGrid1_DoubleClickCell;
                    ultraGrid1.KeyPress -= ultraGrid1_KeyPress;
                    ultraGrid1.AfterRowsDeleted -= UltraGrid1_AfterRowsDeleted;
                    ultraGrid1.AfterSortChange -= UltraGrid1_AfterSortChange;
                    ultraGrid1.AfterColPosChanged -= UltraGrid1_AfterColPosChanged;
                    ultraGrid1.Resize -= UltraGrid1_Resize;

                    if (ultraGrid1.ContextMenuStrip != null)
                    {
                        ultraGrid1.ContextMenuStrip.Dispose();
                        ultraGrid1.ContextMenuStrip = null;
                    }
                }

                // Clean up the column chooser form if it exists
                if (columnChooserForm != null && !columnChooserForm.IsDisposed)
                {
                    columnChooserForm.Close();
                    columnChooserForm.Dispose();
                    columnChooserForm = null;
                }

                // Clean up tooltip resources
                if (toolTip != null)
                {
                    toolTip.Dispose();
                }

                // Call the base implementation
                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                // Just log the error but don't prevent form from closing
                System.Diagnostics.Debug.WriteLine($"Error during form cleanup: {ex.Message}");
            }
        }

        private void ConfigureGridColumns()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                // Set column properties
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Id"))
                {
                    var idCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Id"];
                    idCol.Header.Caption = "ID";
                    idCol.Width = 60;
                }

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("CategoryName"))
                {
                    var nameCol = ultraGrid1.DisplayLayout.Bands[0].Columns["CategoryName"];
                    nameCol.Header.Caption = "Category Name";
                    nameCol.Width = 300;
                }
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

                // Update status with data info
                UpdateStatus($"Filtering with text: '{searchText}' on {fullDataTable.Rows.Count} rows");

                // Store current column widths and positions before changing data source
                Dictionary<string, int> columnWidths = new Dictionary<string, int>();
                Dictionary<string, int> columnPositions = new Dictionary<string, int>();

                // Remember the current column order
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

                // Create a view of the data
                DataView dv = fullDataTable.DefaultView;

                // If search text is provided, apply filter
                if (!string.IsNullOrEmpty(searchText))
                {
                    // Get the selected search filter option
                    string filterOption = comboBox1.SelectedItem?.ToString() ?? "Select all";

                    // Build a filter based on the selected option
                    string filter = BuildFilterString(searchText, filterOption);

                    dv.RowFilter = filter;

                    // Update status with filter info
                    UpdateStatus($"Filter applied: '{filter}' - Found {dv.Count} matching rows");

                    // Update title with filter results
                    this.Text = $"Category Selection - Showing {dv.Count} of {fullDataTable.Rows.Count} categories";
                }
                else
                {
                    // Clear any existing filter
                    dv.RowFilter = string.Empty;
                    this.Text = $"Category Selection - All {fullDataTable.Rows.Count} categories";

                    UpdateStatus("No filter applied - showing all rows");
                }

                // Temporarily suspend layout to prevent flickering
                ultraGrid1.SuspendLayout();

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

                // Resume layout
                ultraGrid1.ResumeLayout();

                // Final status update
                UpdateStatus($"Grid updated with {ultraGrid1.Rows.Count} rows. Filter: {(string.IsNullOrEmpty(dv.RowFilter) ? "None" : dv.RowFilter)}");
            }
            catch (Exception ex)
            {
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
                case "Category Name":
                    return $"CategoryName LIKE '%{escapedSearchText}%'";

                case "ID":
                    return $"CONVERT(Id, 'System.String') LIKE '%{escapedSearchText}%'";

                case "Select all":
                default:
                    // Search across all relevant columns
                    return $"CategoryName LIKE '%{escapedSearchText}%' OR " +
                        $"CONVERT(Id, 'System.String') LIKE '%{escapedSearchText}%'";
            }
        }

        // Helper method to configure visible columns while preserving layout
        private void ConfigureVisibleColumnsPreserveLayout(Dictionary<string, int> columnWidths, Dictionary<string, int> columnPositions)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                // Define the columns to show
                string[] columnsToShow = new string[] { "Id", "CategoryName" };

                foreach (string colKey in columnsToShow)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey))
                    {
                        Infragistics.Win.UltraWinGrid.UltraGridColumn col = ultraGrid1.DisplayLayout.Bands[0].Columns[colKey];
                        col.Hidden = false;

                        // Preserve the position if we have it stored
                        if (columnPositions.ContainsKey(colKey))
                        {
                            col.Header.VisiblePosition = columnPositions[colKey];
                        }

                        // Configure specific columns
                        switch (colKey)
                        {
                            case "Id":
                                col.Header.Caption = "ID";
                                // Preserve width if we have it stored
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 60;
                                break;
                            case "CategoryName":
                                col.Header.Caption = "Category Name";
                                col.Width = columnWidths.ContainsKey(colKey) ? columnWidths[colKey] : 300;
                                break;
                        }
                    }
                }

                // Ensure the OriginalRowOrder column is hidden if it exists
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("OriginalRowOrder"))
                {
                    ultraGrid1.DisplayLayout.Bands[0].Columns["OriginalRowOrder"].Hidden = true;
                }
            }
        }

        // Reorder columns based on selected option
        private void ReorderColumns(string selectedColumn)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            // Store current column widths
            Dictionary<string, int> columnWidths = new Dictionary<string, int>();
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                {
                    columnWidths[col.Key] = col.Width;
                }
            }

            // Suspend layout to prevent flickering
            ultraGrid1.SuspendLayout();

            // Define the columns to show in the specified order
            List<string> columnsToShow = new List<string> { "Id", "CategoryName" };

            // Move selected column to the front
            string columnKey = GetColumnKeyFromDisplayName(selectedColumn);
            if (!string.IsNullOrEmpty(columnKey) && columnsToShow.Contains(columnKey))
            {
                columnsToShow.Remove(columnKey);
                columnsToShow.Insert(0, columnKey);
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

            // Resume layout
            ultraGrid1.ResumeLayout();
            ultraGrid1.Refresh();
        }

        // Helper method to convert display name to column key
        private string GetColumnKeyFromDisplayName(string displayName)
        {
            switch (displayName)
            {
                case "ID": return "Id";
                case "Category Name": return "CategoryName";
                default: return "";
            }
        }

        // Helper method to configure column appearance
        private void ConfigureColumnAppearance(Infragistics.Win.UltraWinGrid.UltraGridColumn col, string colKey)
        {
            switch (colKey)
            {
                case "Id":
                    col.Header.Caption = "ID";
                    if (col.Width <= 0) col.Width = 60;
                    break;
                case "CategoryName":
                    col.Header.Caption = "Category Name";
                    if (col.Width <= 0) col.Width = 300;
                    break;
            }
        }

        private void UpdateRecordCountLabel()
        {
            int count = 0;
            if (ultraGrid1.DataSource is DataView dv)
                count = dv.Count;
            else if (ultraGrid1.DataSource is DataTable dt)
                count = dt.Rows.Count;
            label1.Text = $"Showing {count} of {fullDataTable?.Rows.Count ?? 0} records";
        }

        private void textBoxsearch_TextChanged(object sender, EventArgs e)
        {
            // Skip filtering if the text is the placeholder
            if (textBoxsearch.Text == "Search categories...")
                return;

            // Apply the filter
            ApplyFilter(textBoxsearch.Text.Trim());
        }

        private void ultraGrid1_DoubleClickCell(object sender, Infragistics.Win.UltraWinGrid.DoubleClickCellEventArgs e)
        {
            SelectCategory();
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SelectCategory();
            }
        }

        private void SelectCategory()
        {
            if (ultraGrid1.ActiveRow != null)
            {
                var id = ultraGrid1.ActiveRow.Cells["Id"].Value.ToString();
                var name = ultraGrid1.ActiveRow.Cells["CategoryName"].Value.ToString();
                SelectedCategoryName = name;
                SelectedCategoryId = int.TryParse(id, out int parsedId) ? parsedId : 0;

                if (TargetTextBox != null)
                {
                    TargetTextBox.Text = name;
                    this.DialogResult = DialogResult.OK;
                }
                else if (formname == "frmItemMasterNew")
                {
                    ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                    ItemMaster.txt_Category.Text = name;
                    this.DialogResult = DialogResult.OK;
                }
                else if (formname == "FrmStockAdjustment")
                {
                    stockadj = (FrmStockAdjustment)Application.OpenForms["FrmStockAdjustment"];
                    stockadj.txtb_category.Text = name;
                    stockadj.ultlbl_catid.Text = id;
                    this.DialogResult = DialogResult.OK;
                }
                this.Close();
            }
        }

        private void textBoxsearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                ultraGrid1.Focus();
            }
        }

        // Add panel hover effects
        private void SetupPanelHoverEffects()
        {
            try
            {
                // First apply the base styling to all panels
                StyleIconPanel(ultraPanel3); // Up arrow

                StyleIconPanel(ultraPanel5); // OK button
                StyleIconPanel(ultraPanel6); // Close button
                StyleIconPanel(ultraPanel7); // Down arrow
                StyleIconPanel(ultraPanel50); // New/Edit/Del button

                // Now explicitly set panels to match colors for consistency


                UpdateStatus("Panel hover effects initialized");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting up panel hover effects: {ex.Message}");
            }
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;

            // Define consistent colors for all panels
            Color lightBlue = Color.FromArgb(127, 219, 255); // Light blue
            Color darkBlue = Color.FromArgb(0, 116, 217);    // Darker blue
            Color borderBlue = Color.FromArgb(0, 150, 220);  // Border blue
            Color borderBase = Color.FromArgb(0, 100, 180);  // Border base color

            // Create a gradient from light to dark blue
            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Set highly rounded border style
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;

            // Add border color
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
            panel.ClientArea.MouseEnter += (sender, e) => {
                panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
            };

            panel.ClientArea.MouseLeave += (sender, e) => {
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
            };

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
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

        // Override ProcessCmdKey to handle keyboard shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle Enter key to select the current category
            if (keyData == Keys.Enter && ultraGrid1.ActiveRow != null)
            {
                SelectCategory();
                return true;
            }

            // Handle Escape key to close the form
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Click handler for UltraPanel50 - Opens FrmCategory in UltraTabControl
        /// </summary>
        private void UltraPanel50_Click(object sender, EventArgs e)
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
                        // Check if Category Master tab already exists
                        foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                        {
                            if (tab.Text == "Category Master")
                            {
                                tabControlMain.SelectedTab = tab;
                                System.Diagnostics.Debug.WriteLine("Category Master tab already exists, selected existing tab");

                                // Close the frmCategoryDialog form
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                return;
                            }
                        }

                        // Create new tab using the same approach as Home.cs
                        string uniqueKey = $"Tab_{DateTime.Now.Ticks}_Category Master";
                        var newTab = tabControlMain.Tabs.Add(uniqueKey, "Category Master");

                        // Create and configure FrmCategory for embedding (same as Home.cs)
                        FrmCategory categoryForm = new FrmCategory();
                        categoryForm.TopLevel = false;
                        categoryForm.FormBorderStyle = FormBorderStyle.None;
                        categoryForm.Dock = DockStyle.Fill;
                        categoryForm.Visible = true;
                        categoryForm.BackColor = SystemColors.Control;

                        // Ensure form is properly initialized
                        if (!categoryForm.IsHandleCreated)
                        {
                            categoryForm.CreateControl();
                        }

                        // Add the form to the tab page
                        newTab.TabPage.Controls.Add(categoryForm);

                        // Show the form AFTER adding to tab page
                        categoryForm.Show();
                        categoryForm.BringToFront();

                        // Set the new tab as active/selected
                        tabControlMain.SelectedTab = newTab;

                        // Force refresh to ensure proper display
                        newTab.TabPage.Refresh();
                        categoryForm.Refresh();
                        tabControlMain.Refresh();

                        // Wire up the form's FormClosed event to remove the tab
                        categoryForm.FormClosed += (formSender, formE) => {
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

                        System.Diagnostics.Debug.WriteLine("FrmCategory opened in UltraTabControl using Home.cs approach");

                        // Close the frmCategoryDialog form after successfully opening FrmCategory
                        // Add a small delay to ensure FrmCategory is fully loaded
                        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => {
                            this.Invoke(new Action(() => {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }));
                        });
                    }
                    else
                    {
                        // Fallback: show as regular form
                        FrmCategory categoryForm = new FrmCategory();
                        categoryForm.Show();
                        System.Diagnostics.Debug.WriteLine("FrmCategory opened as regular form (tabControlMain not found)");

                        // Close the frmCategoryDialog form with delay
                        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => {
                            this.Invoke(new Action(() => {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }));
                        });
                    }
                }
                else
                {
                    // If no Home form found, show as a regular form
                    FrmCategory categoryForm = new FrmCategory();
                    categoryForm.Show();
                    System.Diagnostics.Debug.WriteLine("FrmCategory opened as regular form (no Home form found)");

                    // Close the frmCategoryDialog form with delay
                    System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => {
                        this.Invoke(new Action(() => {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }));
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error opening FrmCategory: " + ex.Message);
                MessageBox.Show("Error opening Category Master: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
