using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.Transaction;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Repository; // For BaseRepository

namespace PosBranch_Win.DialogBox
{
    public partial class FrmDialogSHold : Form
    {
        private SalesRepository Sales = new SalesRepository();
        private frmSalesInvoice invoice;
        private List<GetHoldBill> holdBillData; // To hold the original data

        // --- Advanced Feature Fields (ported from frmdialForItemMaster) ---
        private Label lblStatus;
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

        public FrmDialogSHold()
        {
            InitializeComponent();
            this.KeyPreview = true;

            // Initialize all advanced features
            InitializeStatusLabel();
            SetupUltraGridStyle();
            SetupUltraPanelStyle();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents(); // Renamed from ConnectItemMasterClickEvents
            SetupColumnChooserMenu();

            // Wire up event handlers
            this.Load += FrmDialogSHold_Load;
            this.ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            this.ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            this.ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;
            this.textBoxsearch.KeyDown += textBoxsearch_KeyDown;
            this.ultraPictureBox4.Click += UltraPictureBox4_Click; // Sorting toggle
            this.FormClosing += FrmDialogSHold_FormClosing;
        }

        private void FrmDialogSHold_Load(object sender, EventArgs e)
        {
            LoadHoldBillData();
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
            UpdateRecordCountLabel();
            UpdateStatus("Ready");
        }

        private void LoadHoldBillData()
        {
            try
            {
                GetHoldBillGrid hold = Sales.GetHolBill();
                holdBillData = hold.List.ToList();
                ultraGrid1.DataSource = holdBillData;

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
                UpdateStatus("Error loading hold bills: " + ex.Message);
            }
        }
        
        private void FrmDialogSHold_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }

        #region User Interaction (Select Bill, KeyDown)

        private void SelectHoldBill()
        {
            if (ultraGrid1.ActiveRow == null) return;

            try
            {
                invoice = (frmSalesInvoice)Application.OpenForms["frmSalesInvoice"];
                if (invoice == null)
                {
                    MessageBox.Show("Sales Invoice form is not open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                UltraGridCell billNoCell = this.ultraGrid1.ActiveRow.Cells["BillNo"];
                long billNo = Convert.ToInt64(billNoCell.Value);
                salesGrid sale = Sales.GetById(billNo);

                if (sale.ListSales.Any())
                {
                    foreach (SalesMaster sm in sale.ListSales)
                    {
                        invoice.GetMyBill(sm);
                    }

                    SalesDetails[] details = sale.ListSDetails.ToArray();
                    invoice.LoadBillItems(details);

                    // Explicitly mark as editing an existing held bill
                    invoice.MarkEditingHoldBill(billNo);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Could not find details for bill #{billNo}.", "Bill Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting bill: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectHoldBill();
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectHoldBill();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
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
            else if (e.KeyCode == Keys.Enter)
            {
                if (ultraGrid1.Rows.Count == 1)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    SelectHoldBill();
                }
                else if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                }
                e.Handled = true;
            }
        }
        #endregion

        #region Status Label & UI Updates

        private void InitializeStatusLabel()
        {
            lblStatus = new Label
            {
                Name = "lblStatus",
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Ready"
            };
            this.Controls.Add(lblStatus);
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.Update();
            }
        }
        
        private void UpdateRecordCountLabel()
        {
            int currentDisplayCount = ultraGrid1.Rows?.Count ?? 0;
            int totalCount = holdBillData?.Count ?? 0;
            if (this.Controls.Find("label1", true).Length > 0)
            {
                Label label1 = this.Controls.Find("label1", true)[0] as Label;
                if (label1 != null)
                {
                    label1.Text = $"Showing {currentDisplayCount} of {totalCount} records";
                    label1.AutoSize = true;
                }
            }
            //Also update textbox3 if it exists from the designer
            if (this.Controls.Find("textBox3", true).Length > 0)
            {
                TextBox textBox3 = this.Controls.Find("textBox3", true)[0] as TextBox;
                if (textBox3 != null)
                {
                    textBox3.Text = totalCount.ToString();
                }
            }
        }

        #endregion
        
        #region Grid, Panel Styling and Layout
        
        private void SetupUltraPanelStyle()
        {
            // First apply the base styling to all panels
            StyleIconPanel(ultraPanel3);
            StyleIconPanel(ultraPanel4);
            StyleIconPanel(ultraPanel5);
            StyleIconPanel(ultraPanel6);
            StyleIconPanel(ultraPanel7);

            // Now explicitly set ultraPanel3 and ultraPanel7 to match ultraPanel4's colors
            ultraPanel3.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel3.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel3.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel3.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            ultraPanel7.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel7.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel7.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel7.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            // Set appearance for the main panel
            ultPanelPurchaseDisplay.Appearance.BackColor = Color.White;
            ultPanelPurchaseDisplay.Appearance.BackColor2 = Color.FromArgb(200, 230, 250);
            ultPanelPurchaseDisplay.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultPanelPurchaseDisplay.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
        }
        
        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if(panel == null) return;
            // Define consistent colors for all panels
            Color lightBlue = Color.FromArgb(127, 219, 255); 
            Color darkBlue = Color.FromArgb(0, 116, 217);    
            Color borderBlue = Color.FromArgb(0, 150, 220);  
            Color borderBase = Color.FromArgb(0, 100, 180);  
            
            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
            panel.Appearance.BorderColor = borderBlue;
            panel.Appearance.BorderColor3DBase = borderBase;

            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox pic)
                {
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;
                }
                else if (control is Label lbl)
                {
                    lbl.ForeColor = Color.White;
                    lbl.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    lbl.BackColor = Color.Transparent;
                }
            }

            // Add hover effect
            panel.ClientArea.MouseEnter += (sender, e) => {
                panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
            };

            panel.ClientArea.MouseLeave += (sender, e) => {
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
            };

            panel.ClientArea.Cursor = Cursors.Hand;
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
                ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = AllowColSwapping.WithinBand;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;
                Color lightBlue = Color.FromArgb(173, 216, 230);
                Color headerBlue = Color.FromArgb(0, 123, 255);
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // --- Explicit Font Settings ---
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting up grid style: {ex.Message}");
            }
        }
        
        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                e.Layout.Bands[0].Columns.Cast<UltraGridColumn>().ToList().ForEach(c => c.Hidden = true);

                string[] columnsToShow = { "BillNo", "CustomerName", "NetAmount" };
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    string colKey = columnsToShow[i];
                    if (e.Layout.Bands[0].Columns.Exists(colKey))
                    {
                        var col = e.Layout.Bands[0].Columns[colKey];
                        col.Hidden = false;
                        col.Header.VisiblePosition = i;
                        switch (colKey)
                        {
                            case "BillNo":
                                col.Header.Caption = "Bill No";
                                col.Width = 100;
                                break;
                            case "CustomerName":
                                col.Header.Caption = "Customer Name";
                                col.Width = 350;
                                col.CellAppearance.TextHAlign = HAlign.Left;
                                break;
                            case "NetAmount":
                                col.Header.Caption = "Net Amount";
                                col.Width = 150;
                                col.Format = "N2";
                                col.CellAppearance.TextHAlign = HAlign.Right;
                                break;
                        }
                    }
                }
                
                // Ensure correct font is applied on initialization
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 10;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error initializing grid layout: {ex.Message}");
            }
        }
        #endregion

        #region Search, Filter, Sort, Reorder

        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "All", "Bill No", "Customer Name" });
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, e) => FilterData();
            textBoxsearch.TextChanged += (s, e) => FilterData();
        }

        private void FilterData()
        {
            try
            {
                string searchText = textBoxsearch.Text.Trim().ToLower();
                if (holdBillData == null) return;

                IEnumerable<GetHoldBill> filtered = holdBillData;

                if (!string.IsNullOrEmpty(searchText))
                {
                    string selectedField = comboBox1.SelectedItem?.ToString() ?? "All";
                    filtered = holdBillData.Where(b =>
                    {
                        switch (selectedField)
                        {
                            case "Bill No":
                                return b.BillNo.ToString().ToLower().Contains(searchText);
                            case "Customer Name":
                                return b.CustomerName?.ToLower().Contains(searchText) ?? false;
                            default: // "All"
                                return b.BillNo.ToString().ToLower().Contains(searchText) ||
                                       (b.CustomerName?.ToLower().Contains(searchText) ?? false);
                        }
                    });
                }
                ultraGrid1.DataSource = filtered.ToList();
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
                UpdateStatus($"Error filtering data: {ex.Message}");
            }
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "Bill No", "Customer Name", "Net Amount" });
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += (s, e) => ReorderColumns(comboBox2.SelectedItem?.ToString() ?? "Bill No");
        }

        private void ReorderColumns(string selectedColumn)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;
            ultraGrid1.SuspendLayout();

            List<string> columnsToShow = new List<string> { "BillNo", "CustomerName", "NetAmount" };

            Dictionary<string, string> columnMap = new Dictionary<string, string>
            {
                { "Bill No", "BillNo" },
                { "Customer Name", "CustomerName" },
                { "Net Amount", "NetAmount" }
            };
            
            string columnKey = columnMap.ContainsKey(selectedColumn) ? columnMap[selectedColumn] : "BillNo";

            if (columnsToShow.Contains(columnKey))
            {
                columnsToShow.Remove(columnKey);
                columnsToShow.Insert(0, columnKey);
            }

            foreach (var col in ultraGrid1.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>())
            {
                col.Hidden = true;
            }

            for (int i = 0; i < columnsToShow.Count; i++)
            {
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnsToShow[i]))
                {
                    var col = ultraGrid1.DisplayLayout.Bands[0].Columns[columnsToShow[i]];
                    col.Hidden = false;
                    col.Header.VisiblePosition = i;
                }
            }

            ultraGrid1.ResumeLayout();
            ultraGrid1.Refresh();
        }
        
        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            isOriginalOrder = !isOriginalOrder;
            if (ultraGrid1.DataSource is List<GetHoldBill> list)
            {
                var sortedList = isOriginalOrder ?
                    list.OrderBy(b => b.BillNo).ToList() :
                    list.OrderByDescending(b => b.BillNo).ToList();
                ultraGrid1.DataSource = sortedList;
                 if (ultraGrid1.Rows.Count > 0) 
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                }
            }
            UpdateRecordCountLabel();
        }
        #endregion

        #region Column Chooser
        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += (s,e) => ShowColumnChooser();
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
                Text = "Customization", Size = new Size(220, 280), FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual, TopMost = true, MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240), ShowIcon = false, ShowInTaskbar = false
            };
            columnChooserForm.FormClosing += (s, e) => { UpdateStatus("Column customization closed"); };
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();
            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill, AllowDrop = true, DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30, IntegralHeight = false
            };
            columnChooserListBox.DrawItem += (s, evt) => {
                if (evt.Index < 0) return;
                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;
                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(33, 150, 243)))
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
                    StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }
            };
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += (s, e) => { e.Effect = DragDropEffects.Move; };
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            PopulateColumnChooserListBox();
        }
        
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();
            string[] allCols = { "BillNo", "CustomerName", "NetAmount" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "BillNo", "Bill No" }, { "CustomerName", "Customer Name" }, { "NetAmount", "Net Amount" }
            };

            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (string colKey in allCols)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey) && 
                        ultraGrid1.DisplayLayout.Bands[0].Columns[colKey].Hidden)
                    {
                        columnChooserListBox.Items.Add(new ColumnItem(colKey, displayNames[colKey]));
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
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }
        
        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                if (item != null && ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                    column.Hidden = false;
                    columnChooserListBox.Items.Remove(item);
                }
            }
        }
        
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void HideColumn(UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
                string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                savedColumnWidths[column.Key] = column.Width;
                column.Hidden = true;
                if (columnChooserListBox != null && !columnChooserListBox.Items.Cast<ColumnItem>().Any(i => i.ColumnKey == column.Key))
                {
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                }
            }
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(this.Right - columnChooserForm.Width - 20, this.Bottom - columnChooserForm.Height - 20);
                columnChooserForm.BringToFront();
            }
        }
        
        private Point startPoint;
        private UltraGridColumn columnToMove = null;
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
             columnToMove = null;
            var element = ultraGrid1.DisplayLayout.UIElement.ElementFromPoint(e.Location);
            var header = element?.GetAncestor(typeof(Infragistics.Win.UltraWinGrid.HeaderUIElement)) as Infragistics.Win.UltraWinGrid.HeaderUIElement;
            if (header != null)
            {
                columnToMove = header.Header.Column;
                startPoint = e.Location;
            }
        }

        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnToMove != null)
            {
                if (Math.Abs(e.Y - startPoint.Y) > 50)
                {
                    HideColumn(columnToMove);
                    columnToMove = null;
                }
            }
        }

        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            columnToMove = null;
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
        }

        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }
            public ColumnItem(string key, string text) { ColumnKey = key; DisplayText = text; }
            public override string ToString() { return DisplayText; }
        }
        
        #endregion
        
        #region Navigation and Hover Effects
        private void ConnectNavigationPanelEvents()
        {
            // Connect click events for navigation
            ConnectNavEvents(ultraPanel3, MoveRowUp);
            ConnectNavEvents(ultraPictureBox5, MoveRowUp);
            ConnectNavEvents(ultraPanel7, MoveRowDown);
            ConnectNavEvents(ultraPictureBox6, MoveRowDown);

            // Connect click for close button
            if (ultraPanel6 != null) ultraPanel6.Click += (s, e) => this.Close();
            if (label3 != null) label3.Click += (s, e) => this.Close();
            if (ultraPictureBox2 != null) ultraPictureBox2.Click += (s, e) => this.Close();
            
            // Connect click for OK button
            if (ultraPanel5 != null) ultraPanel5.Click += (s, e) => SelectHoldBill();
            if (label5 != null) label5.Click += (s, e) => SelectHoldBill();
            if (ultraPictureBox1 != null) ultraPictureBox1.Click += (s, e) => SelectHoldBill();
        }

        private void ConnectNavEvents(Control control, EventHandler handler)
        {
            if(control != null) control.Click += handler;
        }

        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow == null) {
                if(ultraGrid1.Rows.Count > 0) ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                return;
            };
            if (ultraGrid1.ActiveRow.Index > 0)
            {
                var rowToActivate = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index - 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
        }

        private void MoveRowDown(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow == null) {
                if(ultraGrid1.Rows.Count > 0) ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                return;
            };
            if (ultraGrid1.ActiveRow.Index < ultraGrid1.Rows.Count - 1)
            {
                var rowToActivate = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index + 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
        }
        
        private void SetupPanelHoverEffects() 
        {
            SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
            SetupPanelGroupHoverEffects(ultraPanel5, label5, ultraPictureBox1);
        }
        
        private void SetupPanelGroupHoverEffects(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
             if (panel == null) return;
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);
            Action applyHoverEffect = () => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };
            Action removeHoverEffect = () => {
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
            return Color.FromArgb(color.A, Math.Min(color.R + amount, 255), Math.Min(color.G + amount, 255), Math.Min(color.B + amount, 255));
        }
        
        private bool IsMouseOverControl(Control control)
        {
            if(control == null || !control.IsHandleCreated) return false;
            return control.ClientRectangle.Contains(control.PointToClient(Control.MousePosition));
        }
        #endregion
    }
}
