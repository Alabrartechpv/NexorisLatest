using ModelClass;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Master;
using ModelClass.TransactionModels;
using PosBranch_Win.Master;
using PosBranch_Win.Transaction;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmPurchaseDisplayDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        PurchaseInvoiceRepository ObjPurchaseInvRepo = new PurchaseInvoiceRepository();
        public int Pid = 0;
        private string currentSearchField = "All"; // Default search field
        private string currentSortField = "PurchaseNo"; // Default sort field changed back to PurchaseNo
        private int maxRecordsToDisplay = 100; // Default max records to display
        private DataTable originalDataTable = null; // Store the original data
        private bool isAscendingSort = false; // Default to descending (newest first)
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip(); // ToolTip control for feedback
        private bool _isSelectingPurchase;

        // Virtual keyboard variables
        private Infragistics.Win.Misc.UltraPanel keyboardPanel;
        private TextBox focusedTextBox;
        private bool isKeyboardVisible = false;
        private bool isShiftActive = false;

        // Column drag-to-hide variables
        private bool isDraggingColumn = false;
        private UltraGridColumn draggedColumn = null;
        private Point dragStartPoint;

        // Column customization context menu
        private ContextMenuStrip columnCustomizationMenu;

        // Add column customization form variables
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;

        // Add this dictionary as a class-level field to store column widths
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();

        public FrmPurchaseDisplayDialog()
        {
            InitializeComponent();

            // Set up UltraGrid styling
            SetupUltraGridStyle();

            // Set up UltraPanel styling
            SetupUltraPanelStyle();

            // Register key event handlers
            ultraGrid1.KeyPress += ultraGrid1_KeyPress;
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;

            // Add form closing event to ensure column chooser is closed when main form closes
            this.FormClosing += FrmPurchaseDisplayDialog_FormClosing;

            // Register layout initialization event
            ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;

            // Register events to preserve column widths
            ultraGrid1.AfterRowsDeleted += UltraGrid1_AfterRowsDeleted;
            ultraGrid1.AfterSortChange += UltraGrid1_AfterSortChange;
            ultraGrid1.AfterColPosChanged += UltraGrid1_AfterColPosChanged;

            // Add handler for form resize to preserve column widths
            this.SizeChanged += FrmPurchaseDisplayDialog_SizeChanged;
            ultraGrid1.Resize += UltraGrid1_Resize;

            // Register search box event
            txtSearch.TextChanged += txtSearch_TextChanged;

            // Register textBox1 event for record limit
            textBox1.TextChanged += textBox1_TextChanged;

            // Set default value for textBox1 - controls the number of records displayed in the grid
            textBox1.Text = maxRecordsToDisplay.ToString();

            // Register click event for ultraPictureBox4 to toggle sort direction
            ultraPictureBox4.Click += ultraPictureBox4_Click;

            // Register focus events for textboxes to track which one is active
            txtSearch.GotFocus += TextBox_GotFocus;
            textBox1.GotFocus += TextBox_GotFocus;

            // Initialize comboBox1 with search options
            InitializeComboBox();

            // Initialize comboBox2 with sort options
            InitializeSortComboBox();

            // Initialize virtual keyboard
            InitializeVirtualKeyboard();

            // Initialize column customization context menu
            InitializeColumnCustomizationMenu();

            // Add mouse event for context menu
            ultraGrid1.MouseClick += UltraGrid1_MouseClick;

            // Register mouse events for drag-to-hide functionality
            ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp;

            // Make the grid a drop target
            ultraGrid1.AllowDrop = true;
            ultraGrid1.DragOver += UltraGrid1_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop;

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

            // Subscribe to SessionContext branch change event to refresh data when branch changes
            SessionContext.OnSessionPropertyChanged += SessionContext_OnSessionPropertyChanged;
        }

        private void SessionContext_OnSessionPropertyChanged(string propertyName, object value)
        {
            // Refresh data when branch changes
            if (propertyName == "BranchId" && this.IsHandleCreated && !this.IsDisposed)
            {
                // Use Invoke to ensure thread safety when updating UI
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => RefreshData()));
                }
                else
                {
                    RefreshData();
                }
            }
        }

        private void TextBox_GotFocus(object sender, EventArgs e)
        {
            // Track which textbox has focus for the virtual keyboard
            focusedTextBox = sender as TextBox;
        }

        private void InitializeVirtualKeyboard()
        {
            // Create the keyboard panel
            keyboardPanel = new Infragistics.Win.Misc.UltraPanel();
            keyboardPanel.Location = new Point(10, 320);
            keyboardPanel.Size = new Size(760, 180);
            keyboardPanel.Visible = false;

            // Set appearance for the keyboard panel
            keyboardPanel.Appearance.BackColor = Color.FromArgb(220, 240, 255);
            keyboardPanel.Appearance.BackColor2 = Color.FromArgb(180, 220, 250);
            keyboardPanel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            keyboardPanel.BorderStyle = UIElementBorderStyle.Rounded4;
            keyboardPanel.Appearance.BorderColor = Color.FromArgb(0, 150, 220);

            // Add the keyboard panel to the form
            ultPanelPurchaseDisplay.ClientArea.Controls.Add(keyboardPanel);

            // Create keyboard rows
            CreateKeyboardRow1();
            CreateKeyboardRow2();
            CreateKeyboardRow3();
            CreateKeyboardRow4();
            CreateKeyboardRow5();
        }

        private void CreateKeyboardRow1()
        {
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=" };
            int buttonWidth = 55;
            int buttonHeight = 30;
            int startX = 10;
            int startY = 10;

            for (int i = 0; i < keys.Length; i++)
            {
                Button btn = CreateKeyButton(keys[i], new Point(startX + (i * (buttonWidth + 2)), startY), new Size(buttonWidth, buttonHeight));
                keyboardPanel.ClientArea.Controls.Add(btn);
            }

            // Add Backspace button
            Button backspaceBtn = CreateKeyButton("Backspace", new Point(startX + (keys.Length * (buttonWidth + 2)), startY), new Size(100, buttonHeight));
            backspaceBtn.Click += (sender, e) =>
            {
                if (focusedTextBox != null && focusedTextBox.Text.Length > 0)
                {
                    focusedTextBox.Text = focusedTextBox.Text.Substring(0, focusedTextBox.Text.Length - 1);
                    focusedTextBox.SelectionStart = focusedTextBox.Text.Length;
                }
            };
            keyboardPanel.ClientArea.Controls.Add(backspaceBtn);
        }

        private void CreateKeyboardRow2()
        {
            string[] keys = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]" };
            int buttonWidth = 55;
            int buttonHeight = 30;
            int startX = 25;
            int startY = 45;

            for (int i = 0; i < keys.Length; i++)
            {
                Button btn = CreateKeyButton(keys[i], new Point(startX + (i * (buttonWidth + 2)), startY), new Size(buttonWidth, buttonHeight));
                keyboardPanel.ClientArea.Controls.Add(btn);
            }

            // Add Enter button (first part - it will be taller and span two rows)
            Button enterBtn = CreateKeyButton("ENTER", new Point(startX + (keys.Length * (buttonWidth + 2)), startY), new Size(85, 65));
            enterBtn.Click += (sender, e) =>
            {
                if (focusedTextBox != null)
                {
                    SendKeys.Send("{ENTER}");
                }
            };
            keyboardPanel.ClientArea.Controls.Add(enterBtn);
        }

        private void CreateKeyboardRow3()
        {
            string[] keys = { "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'" };
            int buttonWidth = 55;
            int buttonHeight = 30;
            int startX = 40;
            int startY = 80;

            for (int i = 0; i < keys.Length; i++)
            {
                Button btn = CreateKeyButton(keys[i], new Point(startX + (i * (buttonWidth + 2)), startY), new Size(buttonWidth, buttonHeight));
                keyboardPanel.ClientArea.Controls.Add(btn);
            }
        }

        private void CreateKeyboardRow4()
        {
            string[] keys = { "z", "x", "c", "v", "b", "n", "m", ",", ".", "/" };
            int buttonWidth = 55;
            int buttonHeight = 30;
            int startX = 70;
            int startY = 115;

            for (int i = 0; i < keys.Length; i++)
            {
                Button btn = CreateKeyButton(keys[i], new Point(startX + (i * (buttonWidth + 2)), startY), new Size(buttonWidth, buttonHeight));
                keyboardPanel.ClientArea.Controls.Add(btn);
            }

            // Add Shift button
            Button shiftBtn = CreateKeyButton("Shift", new Point(10, 115), new Size(55, buttonHeight));
            shiftBtn.Click += (sender, e) =>
            {
                isShiftActive = !isShiftActive;

                // Update button appearance to show shift state
                if (isShiftActive)
                {
                    shiftBtn.Text = "SHIFT";
                    // Update all letter keys to uppercase
                    foreach (Control control in keyboardPanel.ClientArea.Controls)
                    {
                        if (control is Button button && button.Text.Length == 1 && char.IsLetter(button.Text[0]))
                        {
                            button.Text = button.Text.ToUpper();
                        }
                    }
                }
                else
                {
                    shiftBtn.Text = "Shift";
                    // Update all letter keys to lowercase
                    foreach (Control control in keyboardPanel.ClientArea.Controls)
                    {
                        if (control is Button button && button.Text.Length == 1 && char.IsLetter(button.Text[0]))
                        {
                            button.Text = button.Text.ToLower();
                        }
                    }
                }

                // Force a repaint of the shift button
                shiftBtn.Invalidate();
            };
            keyboardPanel.ClientArea.Controls.Add(shiftBtn);

            // Add Clear button
            Button clearBtn = CreateKeyButton("Clear", new Point(startX + (keys.Length * (buttonWidth + 2)), 115), new Size(65, buttonHeight));
            clearBtn.Click += (sender, e) =>
            {
                if (focusedTextBox != null)
                {
                    focusedTextBox.Text = string.Empty;
                }
            };
            keyboardPanel.ClientArea.Controls.Add(clearBtn);
        }

        private void CreateKeyboardRow5()
        {
            // Space bar
            Button spaceBtn = CreateKeyButton("Space", new Point(150, 150), new Size(300, 30));
            spaceBtn.Click += (sender, e) =>
            {
                if (focusedTextBox != null)
                {
                    focusedTextBox.Text += " ";
                    focusedTextBox.SelectionStart = focusedTextBox.Text.Length;
                }
            };
            keyboardPanel.ClientArea.Controls.Add(spaceBtn);

            // Close keyboard button
            Button closeBtn = CreateKeyButton("Close", new Point(660, 150), new Size(80, 30));
            closeBtn.BackColor = Color.FromArgb(255, 128, 128);
            closeBtn.ForeColor = Color.White;
            closeBtn.Click += (sender, e) => ToggleKeyboard();
            keyboardPanel.ClientArea.Controls.Add(closeBtn);

            // Tab button
            Button tabBtn = CreateKeyButton("Tab", new Point(10, 150), new Size(60, 30));
            tabBtn.Click += (sender, e) =>
            {
                SendKeys.Send("{TAB}");
            };
            keyboardPanel.ClientArea.Controls.Add(tabBtn);

            // ESC button
            Button escBtn = CreateKeyButton("ESC", new Point(75, 150), new Size(60, 30));
            escBtn.Click += (sender, e) =>
            {
                SendKeys.Send("{ESC}");
            };
            keyboardPanel.ClientArea.Controls.Add(escBtn);

            // Arrow buttons
            Button leftBtn = CreateKeyButton("←", new Point(470, 150), new Size(40, 30));
            leftBtn.Click += (sender, e) =>
            {
                if (focusedTextBox != null && focusedTextBox.SelectionStart > 0)
                {
                    focusedTextBox.SelectionStart--;
                }
            };
            keyboardPanel.ClientArea.Controls.Add(leftBtn);

            Button rightBtn = CreateKeyButton("→", new Point(515, 150), new Size(40, 30));
            rightBtn.Click += (sender, e) =>
            {
                if (focusedTextBox != null && focusedTextBox.SelectionStart < focusedTextBox.Text.Length)
                {
                    focusedTextBox.SelectionStart++;
                }
            };
            keyboardPanel.ClientArea.Controls.Add(rightBtn);

            Button upBtn = CreateKeyButton("↑", new Point(560, 150), new Size(40, 30));
            keyboardPanel.ClientArea.Controls.Add(upBtn);

            Button downBtn = CreateKeyButton("↓", new Point(605, 150), new Size(40, 30));
            keyboardPanel.ClientArea.Controls.Add(downBtn);
        }

        private Button CreateKeyButton(string text, Point location, Size size)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = location;
            btn.Size = size;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = Color.FromArgb(0, 123, 255);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Arial", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            // Add hover effect
            btn.MouseEnter += (sender, e) =>
            {
                btn.Invalidate(); // Force repaint with hover colors
            };

            btn.MouseLeave += (sender, e) =>
            {
                btn.Invalidate(); // Force repaint with normal colors
            };

            // Add click handler for regular keys
            if (text.Length == 1)
            {
                btn.Click += (sender, e) =>
                {
                    if (focusedTextBox != null)
                    {
                        string keyText = text;

                        // Apply shift if active
                        if (isShiftActive)
                        {
                            if (char.IsLetter(keyText[0]))
                            {
                                keyText = keyText.ToUpper();
                            }
                            else
                            {
                                // Handle special shift characters
                                switch (keyText)
                                {
                                    case "1": keyText = "!"; break;
                                    case "2": keyText = "@"; break;
                                    case "3": keyText = "#"; break;
                                    case "4": keyText = "$"; break;
                                    case "5": keyText = "%"; break;
                                    case "6": keyText = "^"; break;
                                    case "7": keyText = "&"; break;
                                    case "8": keyText = "*"; break;
                                    case "9": keyText = "("; break;
                                    case "0": keyText = ")"; break;
                                    case "-": keyText = "_"; break;
                                    case "=": keyText = "+"; break;
                                    case "[": keyText = "{"; break;
                                    case "]": keyText = "}"; break;
                                    case ";": keyText = ":"; break;
                                    case "'": keyText = "\""; break;
                                    case ",": keyText = "<"; break;
                                    case ".": keyText = ">"; break;
                                    case "/": keyText = "?"; break;
                                }
                            }

                            // Turn off shift after one character
                            isShiftActive = false;

                            // Update all keys back to lowercase
                            foreach (Control control in keyboardPanel.ClientArea.Controls)
                            {
                                if (control is Button button && button.Text == "SHIFT")
                                {
                                    button.Text = "Shift";
                                    button.Invalidate();
                                }
                                else if (control is Button button2 && button2.Text.Length == 1 && char.IsLetter(button2.Text[0]) && char.IsUpper(button2.Text[0]))
                                {
                                    button2.Text = button2.Text.ToLower();
                                    button2.Invalidate();
                                }
                            }
                        }

                        focusedTextBox.Text += keyText;
                        focusedTextBox.SelectionStart = focusedTextBox.Text.Length;
                    }
                };
            }

            // Create custom painting for rounded corners and gradient
            btn.Paint += (sender, e) =>
            {
                // Determine if button is being hovered or is active (like Shift)
                bool isHovered = (Control.MouseButtons == MouseButtons.None && btn.ClientRectangle.Contains(btn.PointToClient(Control.MousePosition)));
                bool isSpecialActive = (btn.Text == "SHIFT" && isShiftActive);

                // Create rounded rectangle path
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                int radius = 8; // Corner radius
                Rectangle rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);

                // Top left corner
                path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                // Top right corner
                path.AddArc(rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                // Bottom right corner
                path.AddArc(rect.Width - radius * 2, rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                // Bottom left corner
                path.AddArc(rect.X, rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseAllFigures();

                // Choose colors based on state
                Color topColor, bottomColor;

                if (isHovered || isSpecialActive)
                {
                    // Brighter colors for hover/active state
                    topColor = Color.FromArgb(80, 200, 255);
                    bottomColor = Color.FromArgb(30, 144, 255);
                }
                else
                {
                    // Normal state colors
                    topColor = Color.FromArgb(60, 180, 240);
                    bottomColor = Color.FromArgb(0, 116, 217);
                }

                // Special color for Close button
                if (btn.Text == "Close")
                {
                    topColor = Color.FromArgb(255, 100, 100);
                    bottomColor = Color.FromArgb(220, 50, 50);
                }

                // Create gradient brush
                using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, topColor, bottomColor,
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);

                    // Draw border
                    using (Pen pen = new Pen(Color.FromArgb(0, 90, 180), 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                // Draw text
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(btn.Text, btn.Font, Brushes.White, rect, sf);
            };

            // Make the button transparent so our custom painting shows through
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;

            return btn;
        }

        private void ultraPictureBox3_Click(object sender, EventArgs e)
        {
            ToggleKeyboard();
        }

        private void ToggleKeyboard()
        {
            isKeyboardVisible = !isKeyboardVisible;
            keyboardPanel.Visible = isKeyboardVisible;

            // Adjust form size if needed
            if (isKeyboardVisible)
            {
                // Make sure the keyboard is visible and doesn't go off-screen
                keyboardPanel.BringToFront();

                // Set focus to the search textbox by default if no textbox has focus
                if (focusedTextBox == null)
                {
                    txtSearch.Focus();
                    focusedTextBox = txtSearch;
                }
            }
        }

        private void InitializeSortComboBox()
        {
            // Clear existing items
            comboBox2.Items.Clear();

            // Add sort options
            comboBox2.Items.Add("Purchase No");
            comboBox2.Items.Add("Vendor Name");
            comboBox2.Items.Add("Purchase Date");
            comboBox2.Items.Add("Pid");

            // Set default selection to Purchase No
            comboBox2.SelectedIndex = 0;

            // Add event handler for selection change
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update current sort field based on selection
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    currentSortField = "PurchaseNo";
                    break;
                case 1:
                    currentSortField = "VendorName";
                    break;
                case 2:
                    currentSortField = "PurchaseDate";
                    break;
                case 3:
                    currentSortField = "Pid";
                    break;
                default:
                    currentSortField = "PurchaseNo";
                    break;
            }

            // Apply sorting with the current direction (A-Z or Z-A)
            ApplySorting();

            // Update tooltip to show current sort field and direction
            string direction = isAscendingSort ? "A-Z" : "Z-A";
            string field = GetReadableSortFieldName(currentSortField);
            toolTip.SetToolTip(comboBox2, $"Sorting {field} {direction}");
        }

        private void ApplySorting()
        {
            try
            {
                // Get the DataTable from the grid's DataSource
                DataTable dataTable = null;

                if (ultraGrid1.DataSource is DataTable dt)
                {
                    dataTable = dt;
                }
                else if (ultraGrid1.DataSource is DataView dv)
                {
                    dataTable = dv.Table;
                }

                if (dataTable == null)
                    return;

                // Apply the sort with the current direction
                string sortDirection = isAscendingSort ? "ASC" : "DESC";

                // Sort by the selected column in the chosen direction
                dataTable.DefaultView.Sort = $"{currentSortField} {sortDirection}";

                // Rearrange columns to put the sorted column first in UI
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                    // Reset all column headers to their original captions
                    ResetColumnCaptions(band);

                    // Get the column to highlight/show first
                    string columnToHighlight = currentSortField;

                    // Check if the column exists
                    if (band.Columns.Exists(columnToHighlight))
                    {
                        // Get the column
                        UltraGridColumn column = band.Columns[columnToHighlight];

                        // Set the column position to 0 (first visible column)
                        column.Header.VisiblePosition = 0;

                        // We're removing the up/down arrow indicators
                        // Just set the caption to the readable name without arrows
                        string originalCaption = GetReadableSortFieldName(columnToHighlight);
                        column.Header.Caption = originalCaption;

                        // Rearrange other visible columns
                        int position = 1;
                        foreach (UltraGridColumn col in band.Columns)
                        {
                            if (!col.Hidden && col.Key != columnToHighlight)
                            {
                                col.Header.VisiblePosition = position++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error applying sort: " + ex.Message);
            }
        }

        // Helper method to reset all column captions to their original values
        private void ResetColumnCaptions(UltraGridBand band)
        {
            if (band.Columns.Exists("PurchaseNo"))
                band.Columns["PurchaseNo"].Header.Caption = "Purchase No";

            if (band.Columns.Exists("PurchaseDate"))
                band.Columns["PurchaseDate"].Header.Caption = "Purchase Date";

            if (band.Columns.Exists("VendorName"))
                band.Columns["VendorName"].Header.Caption = "Vendor Name";

            if (band.Columns.Exists("Paymode"))
                band.Columns["Paymode"].Header.Caption = "Payment Mode";

            if (band.Columns.Exists("GrandTotal"))
                band.Columns["GrandTotal"].Header.Caption = "Total Amount";

            if (band.Columns.Exists("UserName"))
                band.Columns["UserName"].Header.Caption = "User";

            if (band.Columns.Exists("Pid"))
                band.Columns["Pid"].Header.Caption = "Pid";
        }

        private void InitializeComboBox()
        {
            // Clear existing items
            comboBox1.Items.Clear();

            // Add search options
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Vendor Name");
            comboBox1.Items.Add("Purchase No");
            comboBox1.Items.Add("Pid No");

            // Set default selection
            comboBox1.SelectedIndex = 0;

            // Add event handler for selection change
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update current search field based on selection
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    currentSearchField = "All";
                    break;
                case 1:
                    currentSearchField = "VendorName";
                    break;
                case 2:
                    currentSearchField = "PurchaseNo";
                    break;
                case 3:
                    currentSearchField = "Pid";
                    break;
                default:
                    currentSearchField = "All";
                    break;
            }

            // Re-apply search with current text and new field selection
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                PerformSearch(txtSearch.Text);
            }
        }

        private void SetupUltraPanelStyle()
        {
            // Common styling for action panels (OK, Cancel buttons)
            StyleActionPanel(ultraPanel5);
            StyleActionPanel(ultraPanel6);

            // Common styling for icon panels
            StyleIconPanel(ultraPanel3);
            StyleIconPanel(ultraPanel7);

            // Set appearance for the main panel
            ultPanelPurchaseDisplay.Appearance.BackColor = Color.White;
            ultPanelPurchaseDisplay.Appearance.BackColor2 = Color.FromArgb(200, 230, 250);
            ultPanelPurchaseDisplay.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultPanelPurchaseDisplay.BorderStyle = UIElementBorderStyle.Rounded4;
        }

        private void StyleActionPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Create a smooth gradient from bright to dark blue to match second image
            panel.Appearance.BackColor = Color.FromArgb(60, 180, 240);  // Bright blue
            panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217); // Darker blue
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Set highly rounded border style for very curved edges
            panel.BorderStyle = UIElementBorderStyle.Rounded4;

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
            // Create a gradient from light to dark blue to match the second image
            panel.Appearance.BackColor = Color.FromArgb(127, 219, 255); // Light blue
            panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217);  // Darker blue
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Set highly rounded border style
            panel.BorderStyle = UIElementBorderStyle.Rounded4;

            // Add shadow/3D effect
            panel.Appearance.BorderColor = Color.FromArgb(0, 150, 220);
            panel.Appearance.BorderColor3DBase = Color.FromArgb(0, 100, 180);

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
            }

            // Add hover effect
            panel.ClientArea.MouseEnter += (sender, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
            };

            panel.ClientArea.MouseLeave += (sender, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(127, 219, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217);
            };

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
        }

        private void FrmPurchaseDisplayDialog_Load(object sender, EventArgs e)
        {
            // Ensure SessionContext is initialized before loading data
            if (!SessionContext.IsInitialized || SessionContext.BranchId <= 0)
            {
                // Try to initialize from DataBase if SessionContext is not initialized
                int branchId, companyId, userId, finYearId = 0;
                if (int.TryParse(DataBase.BranchId, out branchId) &&
                    int.TryParse(DataBase.CompanyId, out companyId) &&
                    int.TryParse(DataBase.UserId, out userId) &&
                    branchId > 0 && companyId > 0)
                {
                    if (!string.IsNullOrEmpty(DataBase.FinyearId) && int.TryParse(DataBase.FinyearId, out finYearId) && finYearId > 0)
                    {
                        // Use existing FinYearId
                    }
                    else
                    {
                        finYearId = 1; // Default fallback
                    }

                    SessionContext.InitializeFromLogin(
                        companyId: companyId,
                        branchId: branchId,
                        finYearId: finYearId,
                        userId: userId,
                        userName: DataBase.UserName,
                        userLevel: DataBase.UserLevel,
                        emailId: DataBase.EmailId,
                        branchName: DataBase.Branch,
                        companyName: null
                    );
                }
                else
                {
                    // Show error if we can't determine the branch
                    MessageBox.Show(
                        "Cannot determine the current branch. Please ensure you are logged in and a branch is selected.",
                        "Branch Not Set",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    this.Close();
                    return;
                }
            }

            // Log the current branch being used (for debugging)
            System.Diagnostics.Debug.WriteLine($"FrmPurchaseDisplayDialog_Load: Loading purchases for BranchId={SessionContext.BranchId}, CompanyId={SessionContext.CompanyId}");

            PurchaseGrid purchaseGrid = drop.getAllPurchaseMaster();

            // Create DataTable from the list to allow sorting and searching
            DataTable dt = new DataTable();
            if (purchaseGrid.ListPurchase != null && purchaseGrid.ListPurchase.Count() > 0)
            {
                // Convert List to DataTable
                System.ComponentModel.PropertyDescriptorCollection properties =
                    System.ComponentModel.TypeDescriptor.GetProperties(typeof(PurchaseMaster));

                foreach (System.ComponentModel.PropertyDescriptor prop in properties)
                {
                    dt.Columns.Add(prop.Name, System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }

                // Get the current branch ID and name for filtering
                int currentBranchId = SessionContext.IsInitialized && SessionContext.BranchId > 0
                    ? SessionContext.BranchId
                    : (int.TryParse(DataBase.BranchId, out int dbBranchId) ? dbBranchId : 0);
                int currentCompanyId = SessionContext.IsInitialized && SessionContext.CompanyId > 0
                    ? SessionContext.CompanyId
                    : (int.TryParse(DataBase.CompanyId, out int dbCompanyId) ? dbCompanyId : 0);
                string currentBranchName = SessionContext.IsInitialized && !string.IsNullOrEmpty(SessionContext.BranchName)
                    ? SessionContext.BranchName
                    : DataBase.Branch;

                System.Diagnostics.Debug.WriteLine($"FrmPurchaseDisplayDialog_Load: Filtering purchases - BranchId={currentBranchId}, CompanyId={currentCompanyId}, BranchName={currentBranchName}");
                System.Diagnostics.Debug.WriteLine($"FrmPurchaseDisplayDialog_Load: Total purchases from stored procedure: {purchaseGrid.ListPurchase?.Count() ?? 0}");

                int filteredCount = 0;
                int skippedCount = 0;

                foreach (var item in purchaseGrid.ListPurchase)
                {
                    // Client-side filtering: Only include purchases that match the current branch
                    // Since the stored procedure GETALL doesn't return BranchId/CompanyId, we filter by BranchName
                    // and also check BranchId if available (it might be 0 if not returned by stored procedure)
                    bool shouldInclude = true;

                    // First try to filter by BranchId if it's available and valid
                    if (currentBranchId > 0 && item.BranchId > 0)
                    {
                        if (item.BranchId != currentBranchId)
                        {
                            System.Diagnostics.Debug.WriteLine($"Filtering out purchase Pid={item.Pid}, PurchaseNo={item.PurchaseNo} - BranchId mismatch: {item.BranchId} != {currentBranchId}");
                            shouldInclude = false;
                            skippedCount++;
                        }
                    }
                    // Fallback to BranchName filtering if BranchId is not available (0 or not returned)
                    else if (!string.IsNullOrEmpty(currentBranchName) && !string.IsNullOrEmpty(item.BranchName))
                    {
                        if (!item.BranchName.Equals(currentBranchName, StringComparison.OrdinalIgnoreCase))
                        {
                            System.Diagnostics.Debug.WriteLine($"Filtering out purchase Pid={item.Pid}, PurchaseNo={item.PurchaseNo} - BranchName mismatch: '{item.BranchName}' != '{currentBranchName}'");
                            shouldInclude = false;
                            skippedCount++;
                        }
                    }

                    // Also filter by CompanyId if available
                    if (shouldInclude && currentCompanyId > 0 && item.CompanyId > 0 && item.CompanyId != currentCompanyId)
                    {
                        System.Diagnostics.Debug.WriteLine($"Filtering out purchase Pid={item.Pid}, PurchaseNo={item.PurchaseNo} - CompanyId mismatch: {item.CompanyId} != {currentCompanyId}");
                        shouldInclude = false;
                        skippedCount++;
                    }

                    if (!shouldInclude)
                    {
                        continue; // Skip purchases from other branches/companies
                    }

                    filteredCount++;

                    DataRow row = dt.NewRow();
                    foreach (System.ComponentModel.PropertyDescriptor prop in properties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                    dt.Rows.Add(row);
                }

                System.Diagnostics.Debug.WriteLine($"FrmPurchaseDisplayDialog_Load: After filtering - Included: {filteredCount}, Skipped: {skippedCount}, Total rows in DataTable: {dt.Rows.Count}");

                // Set default sort to PurchaseNo in descending order (most recent first)
                currentSortField = "PurchaseNo";
                isAscendingSort = false;

                // Update the sort combobox to reflect the default sort
                comboBox2.SelectedIndex = 0; // Index for "Purchase No"

                // Sort by PurchaseNo in descending order
                string sortDirection = "DESC";
                dt.DefaultView.Sort = $"{currentSortField} {sortDirection}";

                // Store the original DataTable
                originalDataTable = dt.Copy();

                // Create a limited DataTable with only the first maxRecordsToDisplay rows
                DataTable limitedDt = dt.Clone();

                // Copy the rows to the limited DataTable
                DataRow[] sortedRows = dt.Select("", $"{currentSortField} {sortDirection}");
                int rowCount = Math.Min(maxRecordsToDisplay, sortedRows.Length);

                for (int i = 0; i < rowCount; i++)
                {
                    limitedDt.ImportRow(sortedRows[i]);
                }

                // Use the limited DataTable as the datasource
                ultraGrid1.DataSource = limitedDt;
            }
            else
            {
                ultraGrid1.DataSource = purchaseGrid.ListPurchase;
            }

            // Hide unnecessary columns
            this.HideUltraGridColumns();

            // Configure column layout with specific widths and alignments
            ConfigureGridLayout();

            // Ensure Pid column is properly aligned and sized
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                if (band.Columns.Exists("Pid"))
                {
                    // Set specific width and alignment for Pid column
                    band.Columns["Pid"].Width = 80;
                    band.Columns["Pid"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["Pid"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["Pid"].Header.Appearance.TextHAlign = HAlign.Center;
                }
            }

            // Apply initial sorting
            ApplySorting();

            // Initialize saved column widths
            InitializeSavedColumnWidths();

            // Update the record count label
            UpdateRecordCountLabel();

            // Wire up ultraPanel5 (OK) and ultraPanel6 (Close) click handlers
            WireOKCloseClickHandlers();

            // Use Shown event to set focus on txtSearch (more reliable than Load event)
            this.Shown += (s, evt) =>
            {
                // Set focus to the search textbox when form is fully displayed
                if (txtSearch != null)
                {
                    txtSearch.Focus();
                    txtSearch.SelectAll();
                }
            };
        }

        /// <summary>
        /// Wire up click handlers for ultraPanel5 (OK) and ultraPanel6 (Close)
        /// </summary>
        private void WireOKCloseClickHandlers()
        {
            try
            {
                // Wire up ultraPanel5 (OK button) - with label5 and ultraPictureBox1
                ultraPanel5.Click += Panel5_OKClick;
                ultraPanel5.ClientArea.Click += Panel5_OKClick;
                label5.Click += Panel5_OKClick;
                ultraPictureBox1.Click += Panel5_OKClick;

                // Wire up ultraPanel6 (Close button) - with label3 and ultraPictureBox2
                ultraPanel6.Click += Panel6_CloseClick;
                ultraPanel6.ClientArea.Click += Panel6_CloseClick;
                label3.Click += Panel6_CloseClick;
                ultraPictureBox2.Click += Panel6_CloseClick;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error wiring OK/Close click handlers: {ex.Message}");
            }
        }

        /// <summary>
        /// OK button click handler - selects the purchase and loads it to the parent form's grid
        /// </summary>
        private void Panel5_OKClick(object sender, EventArgs e)
        {
            if (_isSelectingPurchase)
                return;

            try
            {
                _isSelectingPurchase = true;
                if (ultraGrid1.ActiveRow != null)
                {
                    // Get the selected Pid
                    if (ultraGrid1.ActiveRow.Cells["Pid"] != null)
                    {
                        Pid = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["Pid"].Value);

                        // Load the purchase data to the parent FrmPurchase form
                        if (this.Tag is Transaction.FrmPurchase purchaseForm)
                        {
                            purchaseForm.LoadPurchaseData(Pid);
                        }
                    }
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else if (ultraGrid1.Rows != null && ultraGrid1.Rows.Count > 0)
                {
                    // Select first row if none is selected
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    if (ultraGrid1.ActiveRow.Cells["Pid"] != null)
                    {
                        Pid = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["Pid"].Value);

                        // Load the purchase data to the parent FrmPurchase form
                        if (this.Tag is Transaction.FrmPurchase purchaseForm)
                        {
                            purchaseForm.LoadPurchaseData(Pid);
                        }
                    }
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please select a purchase first.", "No Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OK panel click: {ex.Message}");
                MessageBox.Show($"Error loading purchase: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isSelectingPurchase = false;
            }
        }

        /// <summary>
        /// Close button click handler - closes without selecting
        /// </summary>
        private void Panel6_CloseClick(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Close button clicked in FrmPurchaseDisplayDialog");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Close panel click: {ex.Message}");
            }
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Disable AutoFitStyle to prevent columns from auto-resizing when others are hidden
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;

                // Disable automatic column resizing
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;

                // Hide the group-by area (gray bar)
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;

                // Set rounded borders for the entire grid
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Rounded3;

                // Configure grid lines - single line borders for rows and columns
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set border width to single line
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

                // Ensure consistent single line borders
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

                // Remove cell padding/spacing
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.CellSelect;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;

                // Set light blue border color for cells
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Apply border colors
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Configure row height - increase to match the image (about 26-30 pixels)
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;

                // Add header styling - blue headers
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
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

                // Configure selected row appearance with highlight that maintains readability
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(210, 232, 255); // Very light blue highlight
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(210, 232, 255);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black; // Keep text readable

                // Configure spacing and expansion behavior
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

                // Configure scrollbar style
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

                // Configure the scrollbar look
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    // Configure button appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.None;
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

                // Register additional cell events
                ultraGrid1.DoubleClickCell += new DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);
                ultraGrid1.ClickCell += new ClickCellEventHandler(ultraGrid1_ClickCell);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error configuring grid: " + ex.Message);
            }
        }

        private void ConfigureGridLayout()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Set default alignment for all cells
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Set font size to match the image (standard text size)
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;

                // Configure visible columns with appropriate styling
                if (band.Columns.Exists("PurchaseNo"))
                {
                    band.Columns["PurchaseNo"].Header.Caption = "Purchase No";
                    band.Columns["PurchaseNo"].Width = 100;
                    band.Columns["PurchaseNo"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["PurchaseNo"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["PurchaseNo"].Header.Appearance.TextHAlign = HAlign.Center;
                }

                if (band.Columns.Exists("PurchaseDate"))
                {
                    band.Columns["PurchaseDate"].Header.Caption = "Purchase Date";
                    band.Columns["PurchaseDate"].Width = 120;
                    band.Columns["PurchaseDate"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["PurchaseDate"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["PurchaseDate"].Header.Appearance.TextHAlign = HAlign.Center;
                }

                if (band.Columns.Exists("VendorName"))
                {
                    band.Columns["VendorName"].Header.Caption = "Vendor Name";
                    band.Columns["VendorName"].Width = 250;
                    band.Columns["VendorName"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["VendorName"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["VendorName"].Header.Appearance.TextHAlign = HAlign.Center;
                }

                if (band.Columns.Exists("Paymode"))
                {
                    band.Columns["Paymode"].Header.Caption = "Payment Mode";
                    band.Columns["Paymode"].Width = 120;
                    band.Columns["Paymode"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["Paymode"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["Paymode"].Header.Appearance.TextHAlign = HAlign.Center;
                    // Ensure Paymode values are displayed accurately
                    band.Columns["Paymode"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.Default;
                }

                if (band.Columns.Exists("GrandTotal"))
                {
                    band.Columns["GrandTotal"].Header.Caption = "Total Amount";
                    band.Columns["GrandTotal"].Width = 120;
                    band.Columns["GrandTotal"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["GrandTotal"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["GrandTotal"].Format = "N2";
                    band.Columns["GrandTotal"].Header.Appearance.TextHAlign = HAlign.Center;
                }

                if (band.Columns.Exists("UserName"))
                {
                    band.Columns["UserName"].Header.Caption = "User";
                    band.Columns["UserName"].Width = 100;
                    band.Columns["UserName"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["UserName"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["UserName"].Header.Appearance.TextHAlign = HAlign.Center;
                    // Ensure UserName values are displayed accurately
                    band.Columns["UserName"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.Default;
                }

                // Explicitly configure the Pid column
                if (band.Columns.Exists("Pid"))
                {
                    band.Columns["Pid"].Header.Caption = "Pid";
                    band.Columns["Pid"].Width = 80;
                    band.Columns["Pid"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["Pid"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["Pid"].Header.Appearance.TextHAlign = HAlign.Center;
                }
            }
        }

        private void ultraGrid1_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            if (e.Cell != null && e.Cell.Row != null)
            {
                // Get the selected purchase ID
                UltraGridCell purchaseIdCell = e.Cell.Row.Cells["Pid"];
                if (purchaseIdCell != null && purchaseIdCell.Value != null)
                {
                    int purchaseId = Convert.ToInt32(purchaseIdCell.Value);

                    // Get the FrmPurchase instance that opened this dialog
                    FrmPurchase parentForm = this.Tag as FrmPurchase;
                    if (parentForm != null)
                    {
                        // Load the purchase data to the parent form
                        parentForm.LoadPurchaseData(purchaseId);

                        // Close this dialog with OK result
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
        }

        private void ultraGrid1_ClickCell(object sender, ClickCellEventArgs e)
        {
            // Handle click cell events if needed
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
            {
                // Get the selected purchase ID
                UltraGridCell purchaseIdCell = ultraGrid1.ActiveRow.Cells["Pid"];
                if (purchaseIdCell != null && purchaseIdCell.Value != null)
                {
                    int purchaseId = Convert.ToInt32(purchaseIdCell.Value);

                    // Get the FrmPurchase instance that opened this dialog
                    FrmPurchase parentForm = this.Tag as FrmPurchase;
                    if (parentForm != null)
                    {
                        // Load the purchase data to the parent form
                        parentForm.LoadPurchaseData(purchaseId);

                        // Close this dialog with OK result
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
        }

        public void HideUltraGridColumns()
        {
            if (ultraGrid1 == null || ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            string[] columnsToHide = new string[]
            {
                "CompanyId", "FinYearId", "BranchId", "BranchName", "InvoiceNo", "InvoiceDate",
                "LedgerID", "PaymodeID", "PaymodeLedgerID", "CreditPeriod", "SubTotal", "SpDisPer",
                "SpDsiAmt", "BillDiscountPer", "BillDiscountAmt", "TaxPer", "TaxAmt", "Frieght",
                "ExpenseAmt", "OtherExpAmt", "CancelFlag", "UserID", "TaxType", "Remarks",
                "RoundOff", "CessPer", "CessAmt", "CalAfterTax", "CurrencyID", "CurSymbol",
                "SeriesID", "VoucherID", "IsSyncd", "Paid", "POrderMasterId",
                "PayedAmount", "BilledBy", "_Operation", "TrnsType", "LastUpdated"
            };

            foreach (string colName in columnsToHide)
            {
                try
                {
                    if (band.Columns.Exists(colName))
                    {
                        band.Columns[colName].Hidden = true;
                    }
                }
                catch { }
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Return)
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Get the selected purchase ID
                    UltraGridCell purchaseIdCell = ultraGrid1.ActiveRow.Cells["Pid"];
                    if (purchaseIdCell != null && purchaseIdCell.Value != null)
                    {
                        int purchaseId = Convert.ToInt32(purchaseIdCell.Value);

                        // Get the FrmPurchase instance that opened this dialog
                        FrmPurchase parentForm = this.Tag as FrmPurchase;
                        if (parentForm != null)
                        {
                            // Load the purchase data to the parent form
                            parentForm.LoadPurchaseData(purchaseId);

                            // Close this dialog with OK result
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }
                }
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row != null)
            {
                // Get the selected purchase ID
                UltraGridCell purchaseIdCell = e.Row.Cells["Pid"];
                if (purchaseIdCell != null && purchaseIdCell.Value != null)
                {
                    int purchaseId = Convert.ToInt32(purchaseIdCell.Value);

                    // Get the FrmPurchase instance that opened this dialog
                    FrmPurchase parentForm = this.Tag as FrmPurchase;
                    if (parentForm != null)
                    {
                        // Load the purchase data to the parent form
                        parentForm.LoadPurchaseData(purchaseId);

                        // Close this dialog with OK result
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Apply proper grid line styles
                e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set border style for the main grid
                e.Layout.BorderStyle = UIElementBorderStyle.Solid;

                // Remove cell padding/spacing
                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;

                // Configure row height to match the image
                e.Layout.Override.MinRowHeight = 30;
                e.Layout.Override.DefaultRowHeight = 30;

                // Disable AutoFitStyle to prevent columns from auto-resizing when others are hidden
                e.Layout.AutoFitStyle = AutoFitStyle.None;

                // Disable automatic column resizing
                e.Layout.Override.AllowColSizing = AllowColSizing.Free;

                // Define colors
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Apply row selector appearance with blue
                e.Layout.Override.RowSelectorAppearance.BackColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;

                // Set vibrant blue border color for cell borders
                e.Layout.Override.CellAppearance.BorderColor = lightBlue;
                e.Layout.Override.RowAppearance.BorderColor = lightBlue;
                e.Layout.Override.HeaderAppearance.BorderColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Set all cells to white background
                e.Layout.Override.RowAppearance.BackColor = Color.White;
                e.Layout.Override.RowAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                e.Layout.Override.RowAlternateAppearance.BackColor = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Configure selected row appearance with light blue highlight
                e.Layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(210, 232, 255);
                e.Layout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(210, 232, 255);
                e.Layout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.SelectedRowAppearance.ForeColor = Color.Black; // Keep text readable

                // Configure scrollbars
                e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
                e.Layout.ScrollStyle = ScrollStyle.Immediate;

                // Style track and buttons with blue colors
                if (e.Layout.ScrollBarLook != null)
                {
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

                // Configure columns
                foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                {
                    // Set header style for all columns
                    col.Header.Appearance.BackColor = headerBlue;
                    col.Header.Appearance.BackColor2 = headerBlue;
                    col.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                    col.Header.Appearance.ForeColor = Color.White;
                    col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                    col.Header.Appearance.TextHAlign = HAlign.Center;
                }

                // Ensure text is vertically centered in cells
                e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Add event handler for double-click
                ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;

                // Enable column dragging and dropping for hiding
                e.Layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                e.Layout.Override.AllowColMoving = AllowColMoving.WithinBand;
                e.Layout.Override.AllowColSizing = AllowColSizing.Free;

                // Register the mouse events for drag-to-hide functionality
                ultraGrid1.MouseDown += UltraGrid1_MouseDown;
                ultraGrid1.MouseMove += UltraGrid1_MouseMove;
                ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in InitializeLayout: " + ex.Message);
            }
        }

        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset drag state
                isDraggingColumn = false;
                draggedColumn = null;

                // Store the mouse down position
                dragStartPoint = new Point(e.X, e.Y);

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
                                draggedColumn = col;
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
                draggedColumn = null;
            }
        }

        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Only track movement if we're dragging a column
                if (e.Button == MouseButtons.Left && draggedColumn != null && isDraggingColumn)
                {
                    // Calculate how far the mouse has moved
                    int deltaX = Math.Abs(e.X - dragStartPoint.X);
                    int deltaY = Math.Abs(e.Y - dragStartPoint.Y);

                    // Only start drag if moved beyond threshold
                    if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                    {
                        // Check if moving primarily downward (column to chooser)
                        bool isDraggingDown = (e.Y > dragStartPoint.Y && deltaY > deltaX);

                        if (isDraggingDown)
                        {
                            // Change cursor to indicate a drag operation
                            ultraGrid1.Cursor = Cursors.No;

                            // Show tooltip with hint
                            string columnName = !string.IsNullOrEmpty(draggedColumn.Header.Caption) ?
                                              draggedColumn.Header.Caption : draggedColumn.Key;
                            toolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");

                            // If dragged downward more than 50 pixels, hide the column
                            if (e.Y - dragStartPoint.Y > 50)
                            {
                                // Hide the column and add to customization
                                HideColumn(draggedColumn);

                                // Reset drag state
                                draggedColumn = null;
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
                System.Diagnostics.Debug.WriteLine("Error in mouse move: " + ex.Message);
                ultraGrid1.Cursor = Cursors.Default;
                toolTip.SetToolTip(ultraGrid1, "");
            }
        }

        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset cursor
                ultraGrid1.Cursor = Cursors.Default;
                toolTip.SetToolTip(ultraGrid1, "");

                // Reset drag state
                isDraggingColumn = false;
                draggedColumn = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse up: " + ex.Message);
            }
        }

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

        private void ShowColumnChooser()
        {
            // If the column chooser form already exists, just show it
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show();
                PositionColumnChooserAtBottomRight(); // Always reposition to ensure correct placement
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

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;

            // Clear existing items
            columnChooserListBox.Items.Clear();

            // Add all hidden columns that aren't system columns
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                // Track columns we've already added to prevent duplicates
                HashSet<string> addedColumns = new HashSet<string>();

                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (col.Hidden && !IsSystemColumn(col.Key) && !addedColumns.Contains(col.Key))
                    {
                        string displayText = !string.IsNullOrEmpty(col.Header.Caption) ?
                                           col.Header.Caption : col.Key;
                        columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText));
                        addedColumns.Add(col.Key);
                    }
                }
            }
        }

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

                    // Store width before hiding
                    savedColumnWidths[column.Key] = column.Width;

                    // Hide the column
                    column.Hidden = true;

                    // Add to listbox
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                }
            }
        }

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

                        // Restore width from saved values or set a default
                        if (savedColumnWidths.ContainsKey(item.ColumnKey))
                        {
                            column.Width = savedColumnWidths[item.ColumnKey];
                        }
                        else
                        {
                            column.Width = 100; // Default width
                            savedColumnWidths[item.ColumnKey] = 100;
                        }

                        // Remove from listbox
                        columnChooserListBox.Items.Remove(item);

                        // Preserve other column widths
                        PreserveColumnWidths();

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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ultPanelPurchaseDisplay_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void ultraPanel5_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Perform search as user types
            PerformSearch(txtSearch.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Handle empty text case
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                // Default to 100 records if empty
                maxRecordsToDisplay = 100;
                ApplyRecordLimit();
                return;
            }

            // Try to parse the input as integer
            if (int.TryParse(textBox1.Text, out int recordCount))
            {
                // Ensure the value is at least 1
                if (recordCount > 0)
                {
                    maxRecordsToDisplay = recordCount;
                    ApplyRecordLimit();
                }
                else
                {
                    // If zero or negative, set to minimum of 1
                    maxRecordsToDisplay = 1;
                    textBox1.Text = "1";
                    ApplyRecordLimit();
                }
            }
            // If not a valid integer, don't update (keep previous value)
        }

        private void ApplyRecordLimit()
        {
            if (originalDataTable == null || originalDataTable.Rows.Count == 0)
                return;

            try
            {
                // Store the current column widths before changing the data source
                // We'll use our savedColumnWidths dictionary instead of a temporary one
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        savedColumnWidths[col.Key] = col.Width;
                    }
                }

                // Store column positions
                Dictionary<string, int> columnPositions = new Dictionary<string, int>();
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden)
                        {
                            columnPositions[col.Key] = col.Header.VisiblePosition;
                        }
                    }
                }

                // Create a new DataTable with the same schema
                DataTable limitedDataTable = originalDataTable.Clone();

                // Get the sort direction
                string sortDirection = isAscendingSort ? "ASC" : "DESC";

                // Always sort by PurchaseNo in the specified direction
                DataRow[] sortedRows = originalDataTable.Select("", $"PurchaseNo {sortDirection}");

                // Take the first maxRecordsToDisplay rows from the sorted result
                int rowCount = Math.Min(maxRecordsToDisplay, sortedRows.Length);

                // Copy the rows to the limited DataTable
                for (int i = 0; i < rowCount; i++)
                {
                    limitedDataTable.ImportRow(sortedRows[i]);
                }

                // Set the DataSource to the limited DataTable
                ultraGrid1.DataSource = limitedDataTable;

                // Hide unnecessary columns
                HideUltraGridColumns();

                // Restore column widths and positions
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        // Restore width from saved values
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        {
                            col.Width = savedColumnWidths[col.Key];
                        }

                        // Restore position if available
                        if (columnPositions.ContainsKey(col.Key))
                        {
                            col.Header.VisiblePosition = columnPositions[col.Key];
                        }
                    }
                }

                // Re-apply column styling to ensure consistent appearance
                ConfigureGridLayout();

                // Re-apply sorting
                ApplySorting();

                // Re-apply search if needed
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    PerformSearch(txtSearch.Text);
                }

                // Update the record count label
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error applying record limit: " + ex.Message);
            }
        }

        private void PerformSearch(string searchText)
        {
            try
            {
                if (originalDataTable == null)
                    return;

                // Store the current column widths and positions
                // We'll use our savedColumnWidths dictionary for widths
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        savedColumnWidths[col.Key] = col.Width;
                    }
                }

                // Store column positions
                Dictionary<string, int> columnPositions = new Dictionary<string, int>();
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden)
                        {
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

                // Build the filter expression based on selected search field
                Func<DataRow, bool> filterFunc = row =>
                {
                    switch (currentSearchField)
                    {
                        case "All":
                            // Search across multiple fields
                            string purchaseNo = row["PurchaseNo"].ToString();
                            string vendorName = row["VendorName"].ToString();
                            string pid = row["Pid"].ToString();

                            return purchaseNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   vendorName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   pid.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                        case "VendorName":
                            return row["VendorName"].ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                        case "PurchaseNo":
                            return row["PurchaseNo"].ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                        case "Pid":
                            return row["Pid"].ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                        default:
                            return false;
                    }
                };

                // Apply the filter to get matching rows
                var matchingRows = originalDataTable.AsEnumerable().Where(filterFunc).ToList();

                // Sort the matching rows according to current sort field and direction
                string sortDirection = isAscendingSort ? "ASC" : "DESC";
                var sortedMatchingRows = matchingRows;

                // Sort the matching rows based on the current sort field
                if (currentSortField == "PurchaseDate")
                {
                    sortedMatchingRows = isAscendingSort
                        ? matchingRows.OrderBy(r => r.Field<DateTime?>(currentSortField)).ToList()
                        : matchingRows.OrderByDescending(r => r.Field<DateTime?>(currentSortField)).ToList();
                }
                else if (currentSortField == "PurchaseNo" || currentSortField == "Pid")
                {
                    sortedMatchingRows = isAscendingSort
                        ? matchingRows.OrderBy(r => Convert.ToInt32(r[currentSortField])).ToList()
                        : matchingRows.OrderByDescending(r => Convert.ToInt32(r[currentSortField])).ToList();
                }
                else // String fields like VendorName
                {
                    sortedMatchingRows = isAscendingSort
                        ? matchingRows.OrderBy(r => r[currentSortField].ToString()).ToList()
                        : matchingRows.OrderByDescending(r => r[currentSortField].ToString()).ToList();
                }

                // Take the first maxRecordsToDisplay rows from the sorted result
                var limitedRows = sortedMatchingRows.Take(maxRecordsToDisplay);

                // Add the filtered rows to the new DataTable
                foreach (var row in limitedRows)
                {
                    filteredDataTable.ImportRow(row);
                }

                // Set the filtered DataTable as the DataSource
                ultraGrid1.DataSource = filteredDataTable;

                // Hide unnecessary columns
                HideUltraGridColumns();

                // Restore column widths and positions
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        // Restore width from saved values
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        {
                            col.Width = savedColumnWidths[col.Key];
                        }

                        // Restore position if available
                        if (columnPositions.ContainsKey(col.Key))
                        {
                            col.Header.VisiblePosition = columnPositions[col.Key];
                        }
                    }
                }

                // Re-apply column styling to ensure consistent appearance
                ConfigureGridLayout();

                // Apply sorting to the filtered data
                ApplySorting();

                // Update the record count label
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in search: " + ex.Message);
                // If error occurs, clear the filter and reapply record limit
                ApplyRecordLimit();
            }
        }

        // Click event handler for ultraPictureBox4 to toggle sort direction
        private void ultraPictureBox4_Click(object sender, EventArgs e)
        {
            // Toggle sort direction
            isAscendingSort = !isAscendingSort;

            // Apply the sorting with the new direction
            ApplySorting();

            // Show feedback to the user about the current sort direction
            string direction = isAscendingSort ? "A-Z" : "Z-A";
            string field = GetReadableSortFieldName(currentSortField);

            // Set tooltip on the ultraPictureBox4
            toolTip.SetToolTip(ultraPictureBox4, $"Sorting {field} {direction}");

            // Optional: Update the column header to show sort direction
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                if (band.Columns.Exists(currentSortField))
                {
                    // Get the column
                    UltraGridColumn column = band.Columns[currentSortField];

                    // Update the caption to indicate sort direction
                    string originalCaption = GetReadableSortFieldName(currentSortField);
                    column.Header.Caption = isAscendingSort ?
                        $"{originalCaption} ↑" : // Up arrow for ascending
                        $"{originalCaption} ↓"; // Down arrow for descending
                }
            }

            // Update label or status bar with sort information
            // If you have a status label, you could update it here:
            // lblStatus.Text = $"Sorted by {field} ({direction})";
        }

        // Helper method to get a readable name for the sort field
        private string GetReadableSortFieldName(string fieldName)
        {
            switch (fieldName)
            {
                case "PurchaseNo": return "Purchase No";
                case "VendorName": return "Vendor Name";
                case "PurchaseDate": return "Purchase Date";
                case "Pid": return "Pid";
                default: return fieldName;
            }
        }

        // Add this method to refresh the grid data
        public void RefreshData()
        {
            try
            {
                // Ensure SessionContext is valid before refreshing
                if (!SessionContext.IsInitialized || SessionContext.BranchId <= 0)
                {
                    // Try to initialize from DataBase
                    int branchId, companyId, userId, finYearId = 0;
                    if (int.TryParse(DataBase.BranchId, out branchId) &&
                        int.TryParse(DataBase.CompanyId, out companyId) &&
                        int.TryParse(DataBase.UserId, out userId) &&
                        branchId > 0 && companyId > 0)
                    {
                        if (!string.IsNullOrEmpty(DataBase.FinyearId) && int.TryParse(DataBase.FinyearId, out finYearId) && finYearId > 0)
                        {
                            // Use existing FinYearId
                        }
                        else
                        {
                            finYearId = 1;
                        }

                        SessionContext.InitializeFromLogin(
                            companyId: companyId,
                            branchId: branchId,
                            finYearId: finYearId,
                            userId: userId,
                            userName: DataBase.UserName,
                            userLevel: DataBase.UserLevel,
                            emailId: DataBase.EmailId,
                            branchName: DataBase.Branch,
                            companyName: null
                        );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("RefreshData: Cannot refresh - BranchId or CompanyId not set");
                        return;
                    }
                }

                // Log the current branch being used (for debugging)
                System.Diagnostics.Debug.WriteLine($"RefreshData: Refreshing purchases for BranchId={SessionContext.BranchId}, CompanyId={SessionContext.CompanyId}");

                // Store the current column widths and positions
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        savedColumnWidths[col.Key] = col.Width;
                    }
                }

                // Store column positions and hidden state
                Dictionary<string, int> columnPositions = new Dictionary<string, int>();
                Dictionary<string, bool> columnHiddenState = new Dictionary<string, bool>();

                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        columnPositions[col.Key] = col.Header.VisiblePosition;
                        columnHiddenState[col.Key] = col.Hidden;
                    }
                }

                // Get fresh data from the database
                PurchaseGrid purchaseGrid = drop.getAllPurchaseMaster();

                if (purchaseGrid.ListPurchase != null && purchaseGrid.ListPurchase.Count() > 0)
                {
                    // Convert List to DataTable
                    DataTable dt = new DataTable();
                    System.ComponentModel.PropertyDescriptorCollection properties =
                        System.ComponentModel.TypeDescriptor.GetProperties(typeof(PurchaseMaster));

                    foreach (System.ComponentModel.PropertyDescriptor prop in properties)
                    {
                        dt.Columns.Add(prop.Name, System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }

                    // Get the current branch ID and name for filtering
                    int currentBranchId = SessionContext.IsInitialized && SessionContext.BranchId > 0
                        ? SessionContext.BranchId
                        : (int.TryParse(DataBase.BranchId, out int dbBranchId) ? dbBranchId : 0);
                    int currentCompanyId = SessionContext.IsInitialized && SessionContext.CompanyId > 0
                        ? SessionContext.CompanyId
                        : (int.TryParse(DataBase.CompanyId, out int dbCompanyId) ? dbCompanyId : 0);
                    string currentBranchName = SessionContext.IsInitialized && !string.IsNullOrEmpty(SessionContext.BranchName)
                        ? SessionContext.BranchName
                        : DataBase.Branch;

                    System.Diagnostics.Debug.WriteLine($"RefreshData: Filtering purchases - BranchId={currentBranchId}, CompanyId={currentCompanyId}, BranchName={currentBranchName}");
                    System.Diagnostics.Debug.WriteLine($"RefreshData: Total purchases from stored procedure: {purchaseGrid.ListPurchase?.Count() ?? 0}");

                    int filteredCount = 0;
                    int skippedCount = 0;

                    foreach (var item in purchaseGrid.ListPurchase)
                    {
                        // Client-side filtering: Only include purchases that match the current branch
                        // Since the stored procedure GETALL doesn't return BranchId/CompanyId, we filter by BranchName
                        // and also check BranchId if available (it might be 0 if not returned by stored procedure)
                        bool shouldInclude = true;

                        // First try to filter by BranchId if it's available and valid
                        if (currentBranchId > 0 && item.BranchId > 0)
                        {
                            if (item.BranchId != currentBranchId)
                            {
                                System.Diagnostics.Debug.WriteLine($"RefreshData: Filtering out purchase Pid={item.Pid}, PurchaseNo={item.PurchaseNo} - BranchId mismatch: {item.BranchId} != {currentBranchId}");
                                shouldInclude = false;
                                skippedCount++;
                            }
                        }
                        // Fallback to BranchName filtering if BranchId is not available (0 or not returned)
                        else if (!string.IsNullOrEmpty(currentBranchName) && !string.IsNullOrEmpty(item.BranchName))
                        {
                            if (!item.BranchName.Equals(currentBranchName, StringComparison.OrdinalIgnoreCase))
                            {
                                System.Diagnostics.Debug.WriteLine($"RefreshData: Filtering out purchase Pid={item.Pid}, PurchaseNo={item.PurchaseNo} - BranchName mismatch: '{item.BranchName}' != '{currentBranchName}'");
                                shouldInclude = false;
                                skippedCount++;
                            }
                        }

                        // Also filter by CompanyId if available
                        if (shouldInclude && currentCompanyId > 0 && item.CompanyId > 0 && item.CompanyId != currentCompanyId)
                        {
                            System.Diagnostics.Debug.WriteLine($"RefreshData: Filtering out purchase Pid={item.Pid}, PurchaseNo={item.PurchaseNo} - CompanyId mismatch: {item.CompanyId} != {currentCompanyId}");
                            shouldInclude = false;
                            skippedCount++;
                        }

                        if (!shouldInclude)
                        {
                            continue; // Skip purchases from other branches/companies
                        }

                        filteredCount++;

                        DataRow row = dt.NewRow();
                        foreach (System.ComponentModel.PropertyDescriptor prop in properties)
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        dt.Rows.Add(row);
                    }

                    System.Diagnostics.Debug.WriteLine($"RefreshData: After filtering - Included: {filteredCount}, Skipped: {skippedCount}, Total rows in DataTable: {dt.Rows.Count}");

                    // Store the updated original DataTable
                    originalDataTable = dt.Copy();

                    // Sort by current sort field and direction
                    string sortDirection = isAscendingSort ? "ASC" : "DESC";
                    dt.DefaultView.Sort = $"{currentSortField} {sortDirection}";

                    // Create a limited DataTable with only the first maxRecordsToDisplay rows (most recent)
                    DataTable limitedDt = dt.Clone();

                    // Get sorted rows according to current sort field and direction
                    DataRow[] sortedRows = dt.Select("", $"{currentSortField} {sortDirection}");

                    // Take the first maxRecordsToDisplay rows from the sorted result
                    int rowCount = Math.Min(maxRecordsToDisplay, sortedRows.Length);

                    // Copy the rows to the limited DataTable
                    for (int i = 0; i < rowCount; i++)
                    {
                        limitedDt.ImportRow(sortedRows[i]);
                    }

                    // Use the limited DataTable as the datasource
                    ultraGrid1.DataSource = limitedDt;

                    // Hide unnecessary columns
                    HideUltraGridColumns();

                    // Restore column widths, positions, and hidden state
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                    {
                        UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                        foreach (UltraGridColumn col in band.Columns)
                        {
                            // Restore hidden state if we have it saved
                            if (columnHiddenState.ContainsKey(col.Key))
                            {
                                col.Hidden = columnHiddenState[col.Key];
                            }

                            // Restore width from saved values (only for visible columns)
                            if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                            {
                                col.Width = savedColumnWidths[col.Key];
                            }

                            // Restore position if available
                            if (columnPositions.ContainsKey(col.Key))
                            {
                                col.Header.VisiblePosition = columnPositions[col.Key];
                            }
                        }
                    }

                    // Configure column layout
                    ConfigureGridLayout();

                    // Apply sorting to update column headers and position
                    ApplySorting();

                    // Update the record count label
                    UpdateRecordCountLabel();
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error refreshing data: " + ex.Message);
            }
        }

        private void InitializeColumnCustomizationMenu()
        {
            // Create context menu
            columnCustomizationMenu = new ContextMenuStrip();
            columnCustomizationMenu.Items.Add("Column Customization", null, ColumnCustomizationMenu_Click);
        }

        private void ColumnCustomizationMenu_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        private void UltraGrid1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Show column customization on right-click
                Point clientPoint = ultraGrid1.PointToClient(Control.MousePosition);

                // Clear any existing sub-items
                if (columnCustomizationMenu.Items.Count > 1)
                {
                    // Keep only the first item "Column Customization"
                    while (columnCustomizationMenu.Items.Count > 1)
                    {
                        columnCustomizationMenu.Items.RemoveAt(1);
                    }
                }

                // Show the context menu
                columnCustomizationMenu.Show(ultraGrid1, clientPoint);
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
                System.Diagnostics.Debug.WriteLine("Error preserving column widths: " + ex.Message);
            }
        }

        private void FrmPurchaseDisplayDialog_SizeChanged(object sender, EventArgs e)
        {
            // Preserve column widths when form is resized
            PreserveColumnWidths();

            // No need to add PositionColumnChooserAtBottomRight() here as we already
            // added a separate SizeChanged event handler in ShowColumnChooser method
        }

        private void UltraGrid1_Resize(object sender, EventArgs e)
        {
            // Preserve column widths when grid is resized
            PreserveColumnWidths();
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

        // Helper method to identify system columns that should always be hidden
        private bool IsSystemColumn(string columnKey)
        {
            // List of system columns that should always be hidden
            string[] systemColumns = new string[]
            {
                "CompanyId", "FinYearId", "BranchId", "InvoiceNo", "InvoiceDate",
                "LedgerID", "PaymodeID", "PaymodeLedgerID", "CreditPeriod", "SubTotal",
                "SpDisPer", "SpDsiAmt", "BillDiscountPer", "BillDiscountAmt", "TaxPer",
                "TaxAmt", "Frieght", "ExpenseAmt", "OtherExpAmt", "CancelFlag",
                "UserID", "TaxType", "Remarks", "RoundOff", "CessPer",
                "CessAmt", "CalAfterTax", "CurrencyID", "CurSymbol", "SeriesID",
                "VoucherID", "IsSyncd", "Paid", "POrderMasterId", "PayedAmount",
                "BilledBy", "_Operation", "TrnsType", "LastUpdated", "BranchName"
            };

            return Array.IndexOf(systemColumns, columnKey) >= 0;
        }

        // Helper class to store column information in the ListBox
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

        // Handle form closing to ensure column chooser is closed properly
        private void FrmPurchaseDisplayDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unsubscribe from SessionContext events to prevent memory leaks
            SessionContext.OnSessionPropertyChanged -= SessionContext_OnSessionPropertyChanged;

            // Close the column chooser form if it exists
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

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

                // Update label2 with the current count and total count
                if (this.Controls.Find("label2", true).Length > 0)
                {
                    Label label2 = this.Controls.Find("label2", true)[0] as Label;
                    if (label2 != null)
                    {
                        label2.Text = $"Showing {currentLimit} of {totalCount} records";
                        label2.AutoSize = true;
                        label2.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                        label2.ForeColor = Color.FromArgb(0, 70, 170); // Dark blue color
                    }
                }
            }
        }
    }
}

