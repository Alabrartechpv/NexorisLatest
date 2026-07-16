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

namespace PosBranch_Win.DialogBox
{
    public partial class Edit : Form
    {
        private string _itemName = "";
        private int _itemId = 0;
        private Infragistics.Win.UltraWinGrid.UltraGrid _parentGrid = null;
        private int _rowIndex = -1;
        private string _itemNameColumnKey = "ItemName"; // Default column key for item name

        // Property to access the edited description
        public string ItemDescription
        {
            get { return ultraTextEditor1.Text; }
            set { ultraTextEditor1.Text = value; }
        }

        public Edit()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates an Edit form for a highlighted row in FrmPurchase
        /// </summary>
        /// <param name="purchaseForm">Reference to the parent FrmPurchase form</param>
        /// <returns>A new Edit form instance with the highlighted item's description</returns>
        public static Edit CreateForPurchaseForm(Form purchaseForm)
        {
            if (purchaseForm == null)
                return new Edit();

            // Try to get the ultraGrid1 from the purchase form using reflection
            var ultraGrid1Property = purchaseForm.GetType().GetField("ultraGrid1",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (ultraGrid1Property != null)
            {
                var grid = ultraGrid1Property.GetValue(purchaseForm) as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (grid != null && grid.ActiveRow != null)
                {
                    // Get the description from the highlighted row
                    string itemName = "";
                    int itemId = 0;

                    // Try to get the Description column value
                    if (grid.ActiveRow.Cells.Exists("Description") &&
                        grid.ActiveRow.Cells["Description"].Value != null)
                    {
                        itemName = grid.ActiveRow.Cells["Description"].Value.ToString();
                    }

                    // Try to get the ItemId column value
                    if (grid.ActiveRow.Cells.Exists("ItemId") &&
                        grid.ActiveRow.Cells["ItemId"].Value != null)
                    {
                        int.TryParse(grid.ActiveRow.Cells["ItemId"].Value.ToString(), out itemId);
                    }

                    // Create and return the Edit form with the item name and ID
                    return new Edit(itemName, itemId, grid, grid.ActiveRow.Index);
                }
            }

            return new Edit();
        }

        /// <summary>
        /// Constructor with parameters for grid integration
        /// </summary>
        /// <param name="itemName">Current item name/description</param>
        /// <param name="itemId">Item ID</param>
        /// <param name="parentGrid">Reference to the parent grid to update</param>
        /// <param name="rowIndex">Row index in the grid to update</param>
        /// <param name="itemNameColumnKey">Column key for the item name (default is "ItemName")</param>
        public Edit(string itemName, int itemId = 0, Infragistics.Win.UltraWinGrid.UltraGrid parentGrid = null,
                   int rowIndex = -1, string itemNameColumnKey = "ItemName")
        {
            InitializeComponent();
            _itemName = itemName;
            _itemId = itemId;
            _parentGrid = parentGrid;
            _rowIndex = rowIndex;
            _itemNameColumnKey = itemNameColumnKey;
        }

        /// <summary>
        /// Static method to create an Edit form for a highlighted row in a grid
        /// </summary>
        /// <param name="grid">The UltraGrid containing the highlighted row</param>
        /// <returns>A new Edit form instance</returns>
        public static Edit CreateForHighlightedRow(Infragistics.Win.UltraWinGrid.UltraGrid grid)
        {
            if (grid == null || grid.ActiveRow == null)
                return new Edit();

            // Get the active row
            var row = grid.ActiveRow;

            // Try to find the item name and ID
            string itemName = "";
            int itemId = 0;

            // Check common column names for item description/name
            string[] nameColumns = new string[] { "Description", "ItemName", "ItemDesc", "Name", "ProductName", "Item" };
            foreach (string colName in nameColumns)
            {
                if (row.Cells.Exists(colName) && row.Cells[colName].Value != null)
                {
                    itemName = row.Cells[colName].Value.ToString();
                    break;
                }
            }

            // Check common column names for item ID
            string[] idColumns = new string[] { "ItemID", "ID", "ItemId", "ProductID", "ProductId" };
            foreach (string colName in idColumns)
            {
                if (row.Cells.Exists(colName) && row.Cells[colName].Value != null)
                {
                    int.TryParse(row.Cells[colName].Value.ToString(), out itemId);
                    break;
                }
            }

            // Create the Edit form with the found values
            return new Edit(itemName, itemId, grid, row.Index);
        }

        // EXAMPLE USAGE:
        // To use this form with a highlighted row in a grid:
        //
        // private void ultraGrid1_DoubleClickRow(object sender, Infragistics.Win.UltraWinGrid.DoubleClickRowEventArgs e)
        // {
        //     Edit editForm = Edit.CreateForHighlightedRow(ultraGrid1);
        //     editForm.ShowDialog();
        // }
        //
        // For barcode scanning:
        //
        // private void HandleBarcodeScan(string barcode)
        // {
        //     // Find the item row by barcode
        //     for (int i = 0; i < ultraGrid1.Rows.Count; i++)
        //     {
        //         if (ultraGrid1.Rows[i].Cells["Barcode"].Value.ToString() == barcode)
        //         {
        //             // Select this row
        //             ultraGrid1.ActiveRow = ultraGrid1.Rows[i];
        //             
        //             // Create and show the Edit form
        //             Edit editForm = Edit.CreateForHighlightedRow(ultraGrid1);
        //             editForm.ShowDialog();
        //             break;
        //         }
        //     }
        // }

        private void Edit_Load(object sender, EventArgs e)
        {
            try
            {
                // Display item name/description in ultraTextEditor1
                // First check if we have a direct item name from constructor
                if (!string.IsNullOrEmpty(_itemName))
                {
                    ultraTextEditor1.Text = _itemName;
                }
                // If not, try to get it from the grid if available
                else if (_parentGrid != null && _rowIndex >= 0 && _rowIndex < _parentGrid.Rows.Count)
                {
                    // Try to get the item name from the grid
                    if (_parentGrid.Rows[_rowIndex].Cells.Exists(_itemNameColumnKey))
                    {
                        var cellValue = _parentGrid.Rows[_rowIndex].Cells[_itemNameColumnKey].Value;
                        if (cellValue != null)
                        {
                            ultraTextEditor1.Text = cellValue.ToString();
                            _itemName = cellValue.ToString();
                        }
                    }
                    // If the specified column doesn't exist, try common alternatives
                    else
                    {
                        string[] possibleColumns = new string[] { "ItemName", "Description", "ItemDesc", "Name", "ProductName" };
                        foreach (string colName in possibleColumns)
                        {
                            if (_parentGrid.Rows[_rowIndex].Cells.Exists(colName))
                            {
                                var cellValue = _parentGrid.Rows[_rowIndex].Cells[colName].Value;
                                if (cellValue != null)
                                {
                                    ultraTextEditor1.Text = cellValue.ToString();
                                    _itemName = cellValue.ToString();
                                    break;
                                }
                            }
                        }
                    }
                }

                // If we have a parent form that has an active row in a grid, try to get the item name from there
                if (string.IsNullOrEmpty(ultraTextEditor1.Text))
                {
                    Form parentForm = this.Owner;
                    if (parentForm != null)
                    {
                        // Try to find an UltraGrid in the parent form
                        foreach (Control control in parentForm.Controls)
                        {
                            if (control is Infragistics.Win.UltraWinGrid.UltraGrid)
                            {
                                var grid = (Infragistics.Win.UltraWinGrid.UltraGrid)control;
                                if (grid.ActiveRow != null)
                                {
                                    // Try common column names for item name/description
                                    string[] possibleColumns = new string[] { "ItemName", "Description", "ItemDesc", "Name", "ProductName" };
                                    foreach (string colName in possibleColumns)
                                    {
                                        if (grid.ActiveRow.Cells.Exists(colName))
                                        {
                                            var cellValue = grid.ActiveRow.Cells[colName].Value;
                                            if (cellValue != null)
                                            {
                                                ultraTextEditor1.Text = cellValue.ToString();
                                                _itemName = cellValue.ToString();

                                                // Store the grid reference for later updates
                                                _parentGrid = grid;
                                                _rowIndex = grid.ActiveRow.Index;
                                                _itemNameColumnKey = colName;
                                                break;
                                            }
                                        }
                                    }
                                }
                                break; // Only check the first grid we find
                            }
                        }
                    }
                }

                // If still empty, check if we have an item ID and can look up the name
                if (string.IsNullOrEmpty(ultraTextEditor1.Text) && _itemId > 0)
                {
                    // Try to get the item name from the database based on item ID
                    try
                    {
                        // This is a placeholder - implement according to your data access layer
                        string itemName = GetItemNameById(_itemId);
                        if (!string.IsNullOrEmpty(itemName))
                        {
                            ultraTextEditor1.Text = itemName;
                            _itemName = itemName;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error getting item name: " + ex.Message);
                    }
                }

                // Focus on the text editor and select all text for easy editing
                ultraTextEditor1.Focus();
                ultraTextEditor1.SelectAll();

                // Style panels to match the appearance in UOMeditDig.cs
                StyleIconPanel(ultraPanel5);
                StyleIconPanel(ultraPanel6);

                // Setup hover effects and click handlers for panels
                SetupPanelHoverEffects();

                // Set form title
                this.Text = "Edit Item";

                // Position the form in the center of the screen for best appearance
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Get item name by ID - implement according to your data access layer
        /// </summary>
        private string GetItemNameById(int itemId)
        {
            // This method should be implemented according to your specific data access layer
            // For now, we'll return a placeholder value to avoid linter errors
            System.Diagnostics.Debug.WriteLine($"Getting item name for ID: {itemId}");

            // In a real implementation, you would query your database or service
            // to get the item name based on the ID

            return $"Item #{itemId}"; // Placeholder - replace with actual implementation
        }

        /// <summary>
        /// Styles panels to match the appearance in UOMeditDig.cs
        /// </summary>
        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Define consistent colors for all panels - exact match from UOMeditDig.cs
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
            panel.Appearance.BorderColor = borderBlue;
            panel.Appearance.BorderColor3DBase = borderBase;

            // Add shadow/3D effect
            panel.Appearance.BorderColor3DBase = Color.FromArgb(0, 100, 180);

            // Add glass-like reflection effect
            panel.Appearance.BackHatchStyle = Infragistics.Win.BackHatchStyle.None;
            panel.Appearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

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
                    ((Label)control).TextAlign = ContentAlignment.MiddleCenter; // Center-align text
                }
            }
        }

        /// <summary>
        /// Setup hover effects and click handlers for panels
        /// </summary>
        private void SetupPanelHoverEffects()
        {
            try
            {
                // Set up hover effects for each panel with its controls
                SetupPanelGroupHoverEffects(ultraPanel5, label5, ultraPictureBox1);
                SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);

                // Add click handlers for ultraPanel5 and ultraPanel6
                ultraPanel5.Click += Panel5_Click;
                ultraPanel5.ClientArea.Click += Panel5_Click;
                label5.Click += Panel5_Click;
                ultraPictureBox1.Click += Panel5_Click;

                ultraPanel6.Click += Panel6_Click;
                ultraPanel6.ClientArea.Click += Panel6_Click;
                label3.Click += Panel6_Click;
                ultraPictureBox2.Click += Panel6_Click;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up panel effects: " + ex.Message);
            }
        }

        /// <summary>
        /// Click handler for Panel5 (OK button)
        /// </summary>
        private void Panel5_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the updated item name from the text editor
                string updatedItemName = ultraTextEditor1.Text.Trim();

                // Update the grid if a reference was provided
                if (_parentGrid != null && _rowIndex >= 0 && _rowIndex < _parentGrid.Rows.Count)
                {
                    try
                    {
                        // Update the item name in the grid
                        if (_parentGrid.Rows[_rowIndex].Cells.Exists(_itemNameColumnKey))
                        {
                            _parentGrid.Rows[_rowIndex].Cells[_itemNameColumnKey].Value = updatedItemName;

                            // If there's a "Description" column, update that too
                            if (_parentGrid.Rows[_rowIndex].Cells.Exists("Description"))
                            {
                                _parentGrid.Rows[_rowIndex].Cells["Description"].Value = updatedItemName;
                            }

                            // If there's an "ItemDesc" column, update that too (common column name)
                            if (_parentGrid.Rows[_rowIndex].Cells.Exists("ItemDesc"))
                            {
                                _parentGrid.Rows[_rowIndex].Cells["ItemDesc"].Value = updatedItemName;
                            }

                            // Refresh the grid to show the changes
                            _parentGrid.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating grid: " + ex.Message, "Grid Update Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // Store the edited text in Tag for the calling form to access
                this.Tag = updatedItemName;

                // Return OK result
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Click handler for Panel6 (Close button)
        /// </summary>
        private void Panel6_Click(object sender, EventArgs e)
        {
            try
            {
                // Handle Close button click
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Set up hover effects for a panel group
        /// </summary>
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
            Action applyHoverEffect = () => {
                // Change panel colors
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;

                // Change cursor to hand
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            Action removeHoverEffect = () => {
                // Restore original colors
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;

                // Restore cursor
                panel.ClientArea.Cursor = Cursors.Default;
            };

            // Add hover effects to the panel
            panel.MouseEnter += (s, e) => {
                applyHoverEffect();
            };

            panel.MouseLeave += (s, e) => {
                removeHoverEffect();
            };

            // Add hover effects to the picture box if provided
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    pictureBox.Cursor = Cursors.Hand;
                };

                pictureBox.MouseLeave += (s, e) => {
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
                label.MouseEnter += (s, e) => {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    label.Cursor = Cursors.Hand;
                };

                label.MouseLeave += (s, e) => {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel))
                    {
                        removeHoverEffect();
                    }
                };
            }
        }

        /// <summary>
        /// Helper method to brighten a color
        /// </summary>
        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        /// <summary>
        /// Helper method to check if mouse is over a control
        /// </summary>
        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        // This is just an example method - comment it out to avoid errors
        /*
        private void ultraGrid1_DoubleClickRow(object sender, Infragistics.Win.UltraWinGrid.DoubleClickRowEventArgs e)
        {
            // Create the Edit form for the highlighted row
            Edit editForm = Edit.CreateForHighlightedRow(ultraGrid1);
            
            // Show the form - the item name will already be displayed in ultraTextEditor1
            editForm.ShowDialog();
        }
        */
    }
}
