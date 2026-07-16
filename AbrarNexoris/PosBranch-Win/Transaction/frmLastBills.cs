using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmLastBills : Form
    {
        private readonly BaseRepostitory cn = new BaseRepostitory();
        private readonly SalesRepository salesRepository = new SalesRepository();
        private string paymentModeFilter;
        private readonly List<long> availableBillNumbers = new List<long>();
        private readonly string[] summaryTypes = { "None", "Sum", "Min", "Max", "Average", "Count" };
        private readonly Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

        private long currentBillNo;
        private bool suppressPaymentModeChange;
        private bool gridLayoutLoaded;
        private bool isDraggingColumn;
        private Point startPoint;
        private UltraGridColumn columnToMove;
        private Form columnChooserForm;
        private ListBox columnChooserListBox;

        public frmLastBills() : this("Cash")
        {
        }

        public frmLastBills(string paymentModeFilter)
        {
            InitializeComponent();
            this.paymentModeFilter = string.Equals(paymentModeFilter, "Credit", StringComparison.OrdinalIgnoreCase)
                ? "Credit"
                : "Cash";
            KeyPreview = true;
            InitializeUi();
            WireEvents();
        }

        private void InitializeUi()
        {
            Text = "Last Bills";
            ConfigureActionButton(btnPrint, Color.SeaGreen);
            
            InitializeDesignedControls();
            SetupColumnChooserMenu();
            paymentModeFilter = "Cash";
            ClearViewer(true);
            RefreshAvailableBillNumbers();
        }

        private void InitializeDesignedControls()
        {
            panel1.BackColor = Color.FromArgb(233, 245, 255);

            if (detailsPanel != null)
            {
                detailsPanel.Appearance.BackColor = Color.FromArgb(236, 247, 255);
                detailsPanel.Appearance.BackColor2 = Color.FromArgb(221, 239, 252);
                detailsPanel.Appearance.BackGradientStyle = GradientStyle.Vertical;
            }

            if (summaryStrip != null)
            {
                summaryStrip.Appearance.BackColor = Color.FromArgb(0, 158, 255);
                summaryStrip.Appearance.BackColor2 = Color.FromArgb(0, 110, 212);
                summaryStrip.Appearance.BackGradientStyle = GradientStyle.Vertical;
                labelItemsCaption.Visible = false;
                lblItemCountValue.Visible = false;
                labelQtyCaption.Visible = false;
                lblQtyValue.Visible = false;
                InitializeSummarySelector();
            }

            ConfigureReadOnlyEditor(txtAccountCode);
            ConfigureReadOnlyEditor(txtAccountName);
            ConfigureReadOnlyEditor(txtDocNoDisplay);
            ConfigureReadOnlyEditor(txtBranch);
            ConfigureReadOnlyEditor(txtBillDate);
            ConfigureReadOnlyEditor(txtPaymentMode);
            ConfigureReadOnlyEditor(txtReference);
            ConfigureReadOnlyEditor(txtPaymentTerm);
            ConfigureReadOnlyEditor(txtOutstanding);
            ConfigureReadOnlyEditor(txtSalesPerson);
            ConfigureReadOnlyEditor(txtUser);
            ConfigureReadOnlyEditor(txtPoints);
            ConfigureReadOnlyEditor(txtRemark);

            if (txtRemark != null)
            {
                txtRemark.Multiline = true;
            }

            if (innerBrowseButton != null)
            {
                innerBrowseButton.Click -= btnBrowse_Click;
                innerBrowseButton.Click += btnBrowse_Click;
                ConfigureActionButton(innerBrowseButton, Color.DeepSkyBlue);
            }

            if (itemsGrid != null)
            {
                ConfigureItemsGrid();
            }
        }

        private void ConfigureReadOnlyEditor(UltraTextEditor editor)
        {
            if (editor == null)
            {
                return;
            }

            editor.ReadOnly = true;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.BorderColor = Color.DeepSkyBlue;
        }

        private void WireEvents()
        {
            Load += frmLastBills_Load;
            KeyDown += frmLastBills_KeyDown;
            FormClosing += frmLastBills_FormClosing;

            rdoCash.CheckedChanged += PaymentModeRadio_CheckedChanged;
            rdoCredit.CheckedChanged += PaymentModeRadio_CheckedChanged;

            BindNavigationClick(ultraPanel3, ultraPictureBox2, NavigationFirst_Click);
            BindNavigationClick(ultraPanel9, ultraPictureBox4, NavigationPrevious_Click);
            BindNavigationClick(ultraPanel8, ultraPictureBox6, NavigationNext_Click);
            BindNavigationClick(ultraPanel10, ultraPictureBox5, NavigationLast_Click);
        }

        private static void BindNavigationClick(Control panel, Control icon, EventHandler handler)
        {
            if (panel == null || icon == null || handler == null)
            {
                return;
            }

            panel.Click -= handler;
            panel.Click += handler;
            icon.Click -= handler;
            icon.Click += handler;
        }

        private void NavigationFirst_Click(object sender, EventArgs e)
        {
            if (ultraPanel3 != null && ultraPanel3.Enabled)
            {
                NavigateToBoundary(true);
            }
        }

        private void NavigationPrevious_Click(object sender, EventArgs e)
        {
            if (ultraPanel9 != null && ultraPanel9.Enabled)
            {
                NavigateRelative(1);
            }
        }

        private void NavigationNext_Click(object sender, EventArgs e)
        {
            if (ultraPanel8 != null && ultraPanel8.Enabled)
            {
                NavigateRelative(-1);
            }
        }

        private void NavigationLast_Click(object sender, EventArgs e)
        {
            if (ultraPanel10 != null && ultraPanel10.Enabled)
            {
                NavigateToBoundary(false);
            }
        }

        private void UpdateModeBadge()
        {
            if (string.Equals(paymentModeFilter, "Credit", StringComparison.OrdinalIgnoreCase))
            {
                Text = "Last Bills - Credit Sales";
            }
            else if (string.Equals(paymentModeFilter, "Cash", StringComparison.OrdinalIgnoreCase))
            {
                Text = "Last Bills - Cash Sales";
            }
            else
            {
                Text = "Last Bills";
            }
        }

        private void ConfigureActionButton(Button button, Color backColor)
        {
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = Color.White;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backColor);
            button.FlatStyle = FlatStyle.Flat;
        }

        private void frmLastBills_Load(object sender, EventArgs e)
        {
            paymentModeFilter = "Cash";
            ClearViewer(true);
            RefreshAvailableBillNumbers();
        }

        private void frmLastBills_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGridLayout();
        }

        private void BuildPosViewer()
        {
            panel1.Controls.Clear();
            panel1.BackColor = Color.FromArgb(233, 245, 255);

            detailsPanel = new UltraPanel
            {
                Dock = DockStyle.Top,
                Height = 165
            };
            detailsPanel.Appearance.BackColor = Color.FromArgb(236, 247, 255);
            detailsPanel.Appearance.BackColor2 = Color.FromArgb(221, 239, 252);
            detailsPanel.Appearance.BackGradientStyle = GradientStyle.Vertical;
            detailsPanel.BorderStyle = UIElementBorderStyle.Solid;
            panel1.Controls.Add(detailsPanel);

            itemsGrid = new UltraGrid
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };
            ConfigureItemsGrid();
            panel1.Controls.Add(itemsGrid);

            summaryStrip = new UltraPanel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                BorderStyle = UIElementBorderStyle.Solid
            };
            summaryStrip.Appearance.BackColor = Color.FromArgb(0, 158, 255);
            summaryStrip.Appearance.BackColor2 = Color.FromArgb(0, 110, 212);
            summaryStrip.Appearance.BackGradientStyle = GradientStyle.Vertical;
            panel1.Controls.Add(summaryStrip);

            totalsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 92,
                BackColor = Color.FromArgb(223, 241, 255)
            };
            panel1.Controls.Add(totalsPanel);

            BuildHeaderFields();
            BuildSummaryStrip();
            BuildTotalsPanel();
            panel1.Controls.SetChildIndex(detailsPanel, 0);
            panel1.Controls.SetChildIndex(itemsGrid, 1);
            panel1.Controls.SetChildIndex(summaryStrip, 2);
            panel1.Controls.SetChildIndex(totalsPanel, 3);
        }

        private void BuildHeaderFields()
        {
            Control host = detailsPanel.ClientArea;

            txtAccountCode = CreateReadOnlyEditor(112, 38, 145);
            txtAccountName = CreateReadOnlyEditor(264, 38, 300);
            txtBranch = CreateReadOnlyEditor(112, 70, 145);
            txtBillDate = CreateReadOnlyEditor(390, 70, 180);
            txtPaymentMode = CreateReadOnlyEditor(576, 38, 120);
            txtReference = CreateReadOnlyEditor(703, 38, 120);
            txtPaymentTerm = CreateReadOnlyEditor(390, 102, 180);
            txtOutstanding = CreateReadOnlyEditor(390, 134, 180);
            txtSalesPerson = CreateReadOnlyEditor(112, 102, 145);
            txtUser = CreateReadOnlyEditor(112, 134, 145);
            txtPoints = CreateReadOnlyEditor(390, 134, 180);
            txtRemark = CreateReadOnlyEditor(12, 32, 800);
            txtRemark.Multiline = true;

            rdoCash = new RadioButton
            {
                Text = "Cash Sales",
                AutoSize = true,
                Location = new Point(470, 11),
                Font = new Font("Segoe UI", 11F),
                Enabled = true
            };

            rdoCredit = new RadioButton
            {
                Text = "Credit Sales",
                AutoSize = true,
                Location = new Point(595, 11),
                Font = new Font("Segoe UI", 11F),
                Enabled = true
            };

            chkReceiptPrinter = new CheckBox
            {
                Text = "Send To Receipt Printer",
                AutoSize = true,
                Location = new Point(720, 13),
                Font = new Font("Segoe UI", 10F)
            };

            Button innerBrowseButton = new Button
            {
                Name = "innerBrowseButton",
                Text = "F11",
                BackColor = Color.DeepSkyBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(264, 3),
                Size = new Size(42, 32)
            };
            innerBrowseButton.Click += btnBrowse_Click;

            host.Controls.Add(CreateLabel("Doc No.", 24, 8));
            host.Controls.Add(CreateLabel("Account", 24, 41));
            host.Controls.Add(CreateLabel("Branch", 31, 73));
            host.Controls.Add(CreateLabel("Date", 330, 73));
            host.Controls.Add(CreateLabel("Payment Mode", 576, 41));
            host.Controls.Add(CreateLabel("Reference", 703, 41));
            host.Controls.Add(CreateLabel("Payment Term", 285, 105));
            host.Controls.Add(CreateLabel("Outstanding", 300, 137));
            host.Controls.Add(CreateLabel("Salesperson", 12, 105));
            host.Controls.Add(CreateLabel("User", 53, 137));
            host.Controls.Add(CreateLabel("Points", 350, 137));

            host.Controls.Add(txtAccountCode);
            host.Controls.Add(txtAccountName);
            host.Controls.Add(txtBranch);
            host.Controls.Add(txtBillDate);
            host.Controls.Add(txtPaymentMode);
            host.Controls.Add(txtReference);
            host.Controls.Add(txtPaymentTerm);
            host.Controls.Add(txtOutstanding);
            host.Controls.Add(txtSalesPerson);
            host.Controls.Add(txtUser);
            host.Controls.Add(txtPoints);
            host.Controls.Add(rdoCash);
            host.Controls.Add(rdoCredit);
            host.Controls.Add(chkReceiptPrinter);
            host.Controls.Add(innerBrowseButton);
        }

        private void BuildTotalsPanel()
        {
            totalsPanel.Controls.Add(CreateLabel("Remark", 12, 14));

            txtRemark.Location = new Point(78, 10);
            txtRemark.Size = new Size(560, 52);
            totalsPanel.Controls.Add(txtRemark);

            int left = 650;
            totalsPanel.Controls.Add(CreateLabel("Subtotal", left, 12));
            totalsPanel.Controls.Add(CreateLabel("Disc$", left + 90, 12));
            totalsPanel.Controls.Add(CreateLabel("Tax$", left + 165, 12));
            totalsPanel.Controls.Add(CreateLabel("Round", left + 240, 12));
            totalsPanel.Controls.Add(CreateLabel("Total", left + 320, 12));

            lblSubTotalValue = CreateValueBox(left, 36, false);
            lblDiscountValue = CreateValueBox(left + 80, 36, false);
            lblTaxValue = CreateValueBox(left + 160, 36, false);
            lblRoundValue = CreateValueBox(left + 240, 36, false);
            lblTotalValue = CreateValueBox(left + 320, 36, true);

            totalsPanel.Controls.Add(lblSubTotalValue);
            totalsPanel.Controls.Add(lblDiscountValue);
            totalsPanel.Controls.Add(lblTaxValue);
            totalsPanel.Controls.Add(lblRoundValue);
            totalsPanel.Controls.Add(lblTotalValue);
        }

        private void BuildSummaryStrip()
        {
            Control host = summaryStrip.ClientArea;
            host.Controls.Clear();

            host.Controls.Add(CreateStripLabel("Items", 14));
            lblItemCountValue = CreateStripValue("0", 72);
            host.Controls.Add(lblItemCountValue);

            host.Controls.Add(CreateStripLabel("Qty", 180));
            lblQtyValue = CreateStripValue("0.0000", 228);
            host.Controls.Add(lblQtyValue);
        }

        private Label CreateStripLabel(string text, int x)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, 7),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
        }

        private Label CreateStripValue(string text, int x)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, 5),
                Size = new Size(88, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 138, 228),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void ConfigureItemsGrid()
        {
            itemsGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            itemsGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            itemsGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            itemsGrid.DisplayLayout.Override.AllowRowSummaries = AllowRowSummaries.False;
            itemsGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            itemsGrid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            itemsGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            itemsGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            itemsGrid.DisplayLayout.GroupByBox.Hidden = true;
            itemsGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            itemsGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            itemsGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            itemsGrid.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            itemsGrid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 136, 255);
            itemsGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 204);
            itemsGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            itemsGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            itemsGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            itemsGrid.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            itemsGrid.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
            itemsGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            itemsGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(244, 250, 255);
            itemsGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(128, 200, 255);
            itemsGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.Black;
            itemsGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
            itemsGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            itemsGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(0, 158, 255);
            itemsGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(0, 110, 212);
            itemsGrid.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            itemsGrid.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            itemsGrid.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            itemsGrid.DisplayLayout.Override.RowSelectorWidth = 25;
            itemsGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            itemsGrid.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            itemsGrid.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;
            itemsGrid.DisplayLayout.ViewStyleBand = ViewStyleBand.Horizontal;
            itemsGrid.InitializeLayout += itemsGrid_InitializeLayout;
        }

        private void itemsGrid_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            UltraGridBand band = e.Layout.Bands[0];

            ConfigureColumn(band, "No", 45, HAlign.Center);
            ConfigureColumn(band, "ItemNo", 120, HAlign.Left, "Item No.");
            ConfigureColumn(band, "Description", 250, HAlign.Left);
            ConfigureColumn(band, "UOM", 80, HAlign.Center);
            ConfigureColumn(band, "Qty", 70, HAlign.Right, "Qty", "N2");
            ConfigureColumn(band, "QtyReturned", 95, HAlign.Right, "Qty Returned", "N2");
            ConfigureColumn(band, "Price", 80, HAlign.Right, "Price", "N2");
            ConfigureColumn(band, "DiscPercent", 70, HAlign.Right, "Disc%", "N2");
            ConfigureColumn(band, "SellingPrice", 85, HAlign.Right, "S/Price", "N2");
            ConfigureColumn(band, "Amount", 90, HAlign.Right, "Amount", "N2");
            ConfigureColumn(band, "Points", 70, HAlign.Center);
            ConfigureColumn(band, "Salesperson", 120, HAlign.Left);
            ConfigureColumn(band, "WarrantyDate", 120, HAlign.Center, "Warranty Date");
            ConfigureColumn(band, "FOC", 60, HAlign.Center);
            ConfigureColumn(band, "Remark", 120, HAlign.Left);
            ApplySavedLayoutIfAvailable();
            PopulateColumnChooserListBox();
            ApplyGridSummaries();
        }

        private void ConfigureColumn(UltraGridBand band, string key, int width, HAlign align, string caption = null, string format = null)
        {
            if (!band.Columns.Exists(key)) return;
            UltraGridColumn col = band.Columns[key];
            col.Width = width;
            col.CellAppearance.TextHAlign = align;
            col.Header.Caption = caption ?? key;
            if (!string.IsNullOrEmpty(format))
            {
                col.Format = format;
            }
        }

        private string GridLayoutPath
        {
            get
            {
                string layoutFolder = Path.Combine(Application.StartupPath, "GridLayouts");
                if (!Directory.Exists(layoutFolder))
                {
                    Directory.CreateDirectory(layoutFolder);
                }

                return Path.Combine(layoutFolder, "frmLastBills.xml");
            }
        }

        private void SetupColumnChooserMenu()
        {
            if (itemsGrid == null)
            {
                return;
            }

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Field/Column Chooser", null, (s, e) => ShowColumnChooser());
            itemsGrid.ContextMenuStrip = menu;
            SetupDirectHeaderDragDrop();
        }

        private void SetupDirectHeaderDragDrop()
        {
            if (itemsGrid == null)
            {
                return;
            }

            itemsGrid.AllowDrop = true;
            itemsGrid.MouseDown -= ItemsGrid_MouseDown;
            itemsGrid.MouseMove -= ItemsGrid_MouseMove;
            itemsGrid.MouseUp -= ItemsGrid_MouseUp;
            itemsGrid.DragOver -= ItemsGrid_DragOver;
            itemsGrid.DragDrop -= ItemsGrid_DragDrop;

            itemsGrid.MouseDown += ItemsGrid_MouseDown;
            itemsGrid.MouseMove += ItemsGrid_MouseMove;
            itemsGrid.MouseUp += ItemsGrid_MouseUp;
            itemsGrid.DragOver += ItemsGrid_DragOver;
            itemsGrid.DragDrop += ItemsGrid_DragDrop;

            CreateColumnChooserForm();
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
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

            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };
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

            columnChooserListBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0)
                {
                    return;
                }

                ColumnItem item = columnChooserListBox.Items[e.Index] as ColumnItem;
                if (item == null)
                {
                    return;
                }

                Rectangle rect = e.Bounds;
                rect.Inflate(-3, -3);

                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(33, 150, 243)))
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
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(bgBrush, path);
                }

                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat stringFormat = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(item.DisplayText, e.Font, textBrush, rect, stringFormat);
                }
            };

            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;

            columnChooserForm.Controls.Add(columnChooserListBox);
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    Right - columnChooserForm.Width - 20,
                    Bottom - columnChooserForm.Height - 20);
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || itemsGrid == null || itemsGrid.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            columnChooserListBox.Items.Clear();
            foreach (UltraGridColumn column in itemsGrid.DisplayLayout.Bands[0].Columns)
            {
                if (column.Hidden)
                {
                    string displayText = string.IsNullOrWhiteSpace(column.Header.Caption) ? column.Key : column.Header.Caption;
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, displayText));
                }
            }
        }

        private sealed class ColumnItem
        {
            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }

            public string ColumnKey { get; }
            public string DisplayText { get; }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        private void ItemsGrid_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);

            if (itemsGrid.DisplayLayout.Bands.Count == 0 || e.Y >= 40)
            {
                return;
            }

            int xPos = 0;
            if (itemsGrid.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
            {
                xPos += itemsGrid.DisplayLayout.Override.RowSelectorWidth;
            }

            foreach (UltraGridColumn column in itemsGrid.DisplayLayout.Bands[0].Columns)
            {
                if (column.Hidden)
                {
                    continue;
                }

                if (e.X >= xPos && e.X < xPos + column.Width)
                {
                    columnToMove = column;
                    isDraggingColumn = true;
                    break;
                }

                xPos += column.Width;
            }
        }

        private void ItemsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingColumn || columnToMove == null || e.Button != MouseButtons.Left)
            {
                return;
            }

            int deltaX = Math.Abs(e.X - startPoint.X);
            int deltaY = Math.Abs(e.Y - startPoint.Y);
            if (deltaX <= SystemInformation.DragSize.Width && deltaY <= SystemInformation.DragSize.Height)
            {
                return;
            }

            bool isDraggingDown = e.Y > startPoint.Y && deltaY > deltaX;
            if (!isDraggingDown)
            {
                return;
            }

            itemsGrid.Cursor = Cursors.No;
            string columnName = string.IsNullOrWhiteSpace(columnToMove.Header.Caption) ? columnToMove.Key : columnToMove.Header.Caption;
            toolTip.SetToolTip(itemsGrid, $"Drag down to hide '{columnName}' column");

            if (e.Y - startPoint.Y > 50)
            {
                HideColumn(columnToMove);
                columnToMove = null;
                isDraggingColumn = false;
                itemsGrid.Cursor = Cursors.Default;
                toolTip.SetToolTip(itemsGrid, string.Empty);
            }
        }

        private void ItemsGrid_MouseUp(object sender, MouseEventArgs e)
        {
            itemsGrid.Cursor = Cursors.Default;
            toolTip.SetToolTip(itemsGrid, string.Empty);
            isDraggingColumn = false;
            columnToMove = null;
        }

        private void HideColumn(UltraGridColumn column)
        {
            if (column == null || column.Hidden)
            {
                return;
            }

            savedColumnWidths[column.Key] = column.Width;
            column.Hidden = true;
            PopulateColumnChooserListBox();
            SaveGridLayout();
            ApplyGridSummaries();
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches)
            {
                return;
            }

            if (columnChooserListBox.Items[index] is ColumnItem item)
            {
                columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
            }
        }

        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(UltraGridColumn))
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            UltraGridColumn column = e.Data.GetData(typeof(UltraGridColumn)) as UltraGridColumn;
            if (column == null || column.Hidden)
            {
                return;
            }

            HideColumn(column);
        }

        private void ItemsGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem))
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        private void ItemsGrid_DragDrop(object sender, DragEventArgs e)
        {
            ColumnItem item = e.Data.GetData(typeof(ColumnItem)) as ColumnItem;
            if (item == null || itemsGrid.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            if (!itemsGrid.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
            {
                return;
            }

            UltraGridColumn column = itemsGrid.DisplayLayout.Bands[0].Columns[item.ColumnKey];
            column.Hidden = false;
            if (savedColumnWidths.TryGetValue(item.ColumnKey, out int width) && width > 0)
            {
                column.Width = width;
            }

            PopulateColumnChooserListBox();
            SaveGridLayout();
            ApplyGridSummaries();
        }

        private void ApplySavedLayoutIfAvailable()
        {
            if (gridLayoutLoaded || !File.Exists(GridLayoutPath))
            {
                return;
            }

            if (itemsGrid.DisplayLayout.Bands.Count == 0 || itemsGrid.DisplayLayout.Bands[0].Columns.Count == 0)
            {
                return;
            }

            try
            {
                itemsGrid.DisplayLayout.LoadFromXml(GridLayoutPath);
                gridLayoutLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying last bills grid layout: {ex.Message}");
            }
        }

        private void SaveGridLayout()
        {
            if (itemsGrid == null || itemsGrid.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            try
            {
                itemsGrid.DisplayLayout.SaveAsXml(GridLayoutPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving last bills grid layout: {ex.Message}");
            }
        }

        private void InitializeSummarySelector()
        {
            if (summaryStrip == null || summaryStrip.ClientArea == null)
            {
                return;
            }
            summaryStrip.ClientArea.Controls.Clear();
            summaryStrip.ClientArea.MouseUp -= SummaryStrip_MouseUp;
            summaryStrip.ClientArea.MouseUp += SummaryStrip_MouseUp;
            summaryStrip.Paint += (s, e) => AlignSummaryLabels();
            summaryStrip.Resize += (s, e) => AlignSummaryLabels();
            itemsGrid.AfterColPosChanged += (s, e) => AlignSummaryLabels();
            itemsGrid.AfterSortChange += (s, e) => AlignSummaryLabels();
            itemsGrid.AfterRowFilterChanged += (s, e) => ApplyGridSummaries();
            itemsGrid.SizeChanged += (s, e) => AlignSummaryLabels();
        }

        private void ApplyGridSummaries()
        {
            if (summaryStrip == null || summaryStrip.ClientArea == null)
            {
                return;
            }

            summaryStrip.ClientArea.SuspendLayout();
            summaryStrip.ClientArea.Controls.Clear();
            summaryLabels.Clear();

            if (itemsGrid == null || itemsGrid.DisplayLayout == null || itemsGrid.DisplayLayout.Bands.Count == 0)
            {
                summaryStrip.ClientArea.ResumeLayout();
                return;
            }

            UltraGridBand band = itemsGrid.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || !IsNumericColumn(column))
                {
                    continue;
                }

                if (!columnAggregations.TryGetValue(column.Key, out string aggregationType) ||
                    string.Equals(aggregationType, "None", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Label summaryLabel = new Label
                {
                    AutoSize = false,
                    Height = summaryStrip.Height - 6,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(0, 138, 228),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BorderStyle = BorderStyle.FixedSingle,
                    ContextMenuStrip = CreateSummaryMenu(column.Key)
                };

                summaryStrip.ClientArea.Controls.Add(summaryLabel);
                summaryLabels[column.Key] = summaryLabel;
            }

            UpdateSummaryStripValues();
            AlignSummaryLabels();
            summaryStrip.ClientArea.ResumeLayout();
        }

        private ContextMenuStrip CreateSummaryMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            foreach (string type in summaryTypes)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(type)
                {
                    Tag = type
                };
                item.Click += (s, e) =>
                {
                    if (string.Equals(type, "None", StringComparison.OrdinalIgnoreCase))
                    {
                        columnAggregations.Remove(columnKey);
                    }
                    else
                    {
                        columnAggregations[columnKey] = type;
                    }

                    ApplyGridSummaries();
                };
                menu.Items.Add(item);
            }

            menu.Opening += (s, e) =>
            {
                string currentType = columnAggregations.ContainsKey(columnKey) ? columnAggregations[columnKey] : "None";
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = string.Equals(currentType, Convert.ToString(item.Tag), StringComparison.OrdinalIgnoreCase);
                }
            };

            return menu;
        }

        private void SummaryStrip_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            string columnKey = GetColumnKeyAtSummaryPoint(e.Location);
            if (string.IsNullOrWhiteSpace(columnKey))
            {
                return;
            }

            ContextMenuStrip menu = CreateSummaryMenu(columnKey);
            menu.Show(summaryStrip.ClientArea, e.Location);
        }

        private string GetColumnKeyAtSummaryPoint(Point location)
        {
            if (itemsGrid == null || itemsGrid.DisplayLayout == null || itemsGrid.DisplayLayout.Bands.Count == 0)
            {
                return null;
            }

            int stripLeft = summaryStrip.ClientArea.PointToScreen(Point.Empty).X;
            UltraGridBand band = itemsGrid.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || !IsNumericColumn(column))
                {
                    continue;
                }

                UIElement headerElement = column.Header.GetUIElement();
                if (headerElement == null)
                {
                    continue;
                }

                Point headerPoint = headerElement.Control.PointToScreen(headerElement.Rect.Location);
                int left = headerPoint.X - stripLeft;
                int right = left + headerElement.Rect.Width;
                if (location.X >= left && location.X <= right)
                {
                    return column.Key;
                }
            }

            return null;
        }

        private void UpdateSummaryStripValues()
        {
            if (!(itemsGrid?.DataSource is DataTable dt))
            {
                return;
            }

            foreach (KeyValuePair<string, Label> kvp in summaryLabels)
            {
                string columnKey = kvp.Key;
                string aggregationType = columnAggregations.ContainsKey(columnKey) ? columnAggregations[columnKey] : "None";
                if (string.Equals(aggregationType, "None", StringComparison.OrdinalIgnoreCase))
                {
                    kvp.Value.Text = string.Empty;
                    continue;
                }

                List<double> values = dt.AsEnumerable()
                    .Where(r => r[columnKey] != DBNull.Value)
                    .Select(r => Convert.ToDouble(r[columnKey]))
                    .ToList();

                string text = string.Empty;
                switch (aggregationType)
                {
                    case "Sum":
                        text = values.Count > 0 ? values.Sum().ToString("N2") : "0.00";
                        break;
                    case "Min":
                        text = values.Count > 0 ? values.Min().ToString("N2") : "0.00";
                        break;
                    case "Max":
                        text = values.Count > 0 ? values.Max().ToString("N2") : "0.00";
                        break;
                    case "Average":
                        text = values.Count > 0 ? values.Average().ToString("N2") : "0.00";
                        break;
                    case "Count":
                        text = values.Count.ToString();
                        break;
                }

                kvp.Value.Text = text;
            }
        }

        private void AlignSummaryLabels()
        {
            if (summaryStrip == null || summaryStrip.ClientArea == null || itemsGrid == null || itemsGrid.DisplayLayout == null || itemsGrid.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            int stripLeft = summaryStrip.ClientArea.PointToScreen(Point.Empty).X;
            foreach (UltraGridColumn column in itemsGrid.DisplayLayout.Bands[0].Columns)
            {
                if (!summaryLabels.TryGetValue(column.Key, out Label summaryLabel))
                {
                    continue;
                }

                UIElement headerElement = column.Header.GetUIElement();
                if (headerElement == null)
                {
                    continue;
                }

                Point headerPoint = headerElement.Control.PointToScreen(headerElement.Rect.Location);
                summaryLabel.Left = (headerPoint.X - stripLeft) + 1;
                summaryLabel.Top = 2;
                summaryLabel.Width = Math.Max(20, headerElement.Rect.Width - 2);
                summaryLabel.Height = summaryStrip.Height - 6;
            }
        }

        private bool IsNumericColumn(UltraGridColumn column)
        {
            Type type = column.DataType;
            return type == typeof(short) ||
                   type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal);
        }

        private UltraTextEditor CreateReadOnlyEditor(int x, int y, int width)
        {
            UltraTextEditor editor = new UltraTextEditor
            {
                Location = new Point(x, y),
                Size = new Size(width, 27),
                Font = new Font("Segoe UI", 11F),
                ReadOnly = true,
                UseOsThemes = DefaultableBoolean.False,
                BorderStyle = UIElementBorderStyle.Solid
            };
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.BorderColor = Color.DeepSkyBlue;
            return editor;
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                BackColor = Color.Transparent
            };
        }

        private Label CreateValueBox(int x, int y, bool highlight)
        {
            return new Label
            {
                Location = new Point(x, y),
                Size = new Size(72, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = highlight ? Color.Black : Color.White,
                ForeColor = highlight ? Color.Gold : Color.FromArgb(0, 90, 170),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "0.00"
            };
        }

        private void frmLastBills_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenBillPicker();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F8)
            {
                button1_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenBillPicker();
        }

        private void PaymentModeRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (suppressPaymentModeChange)
            {
                return;
            }

            RadioButton changedRadio = sender as RadioButton;
            if (changedRadio != null && !changedRadio.Checked)
            {
                return;
            }

            if (!rdoCash.Checked && !rdoCredit.Checked)
            {
                paymentModeFilter = string.Empty;
                UpdateModeBadge();
                availableBillNumbers.Clear();
                UpdateNavigationState();
                return;
            }

            string selectedMode;
            if (ReferenceEquals(sender, rdoCash))
            {
                selectedMode = "Cash";
            }
            else if (ReferenceEquals(sender, rdoCredit))
            {
                selectedMode = "Credit";
            }
            else
            {
                selectedMode = rdoCash.Checked ? "Cash" : "Credit";
            }

            if (string.Equals(paymentModeFilter, selectedMode, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            paymentModeFilter = selectedMode;
            UpdateModeBadge();
            RefreshAvailableBillNumbers();
            ClearViewer(true);
        }

        private void OpenBillPicker()
        {
            if (!string.Equals(paymentModeFilter, "Cash", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(paymentModeFilter, "Credit", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Please choose Cash Sales or Credit Sales first.", "Select Sales Type", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (frmLastBillDig dialog = new frmLastBillDig(paymentModeFilter))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedBillNo > 0)
                {
                    LoadBillView(dialog.SelectedBillNo);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string enteredBillNo = txtDocNoDisplay == null ? string.Empty : txtDocNoDisplay.Text.Trim();
            if (string.IsNullOrWhiteSpace(enteredBillNo))
            {
                MessageBox.Show("Please enter or choose a bill number.", "Bill Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!long.TryParse(enteredBillNo, out long billNo) || billNo <= 0)
            {
                MessageBox.Show("Please enter a valid bill number.", "Invalid Bill", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadBillView(billNo);
        }

        private void LoadBillView(long billNo)
        {
            salesGrid data = salesRepository.GetById(billNo);
            SalesMaster master = data?.ListSales?.FirstOrDefault();

            if (master == null)
            {
                MessageBox.Show("Bill not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool selectedCash = string.Equals(paymentModeFilter, "Cash", StringComparison.OrdinalIgnoreCase);
            bool selectedCredit = string.Equals(paymentModeFilter, "Credit", StringComparison.OrdinalIgnoreCase);
            bool billIsCredit = IsCreditSale(master);
            if ((selectedCash && billIsCredit) || (selectedCredit && !billIsCredit))
            {
                MessageBox.Show($"Bill {billNo} is not a {paymentModeFilter} sale.", "Mode Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            currentBillNo = billNo;
            txtDocNoDisplay.Text = billNo.ToString();
            txtAccountCode.Text = GetAccountCode(master);
            txtAccountName.Text = GetAccountName(master);
            txtBranch.Text = SessionContext.BranchName ?? SessionContext.BranchId.ToString();
            txtBillDate.Text = master.BillDate == DateTime.MinValue ? string.Empty : master.BillDate.ToString("dd/MMM/yyyy HH:mm");
            txtPaymentMode.Text = (master.PaymodeName ?? string.Empty).ToUpperInvariant();
            txtReference.Text = master.PaymentReference ?? string.Empty;
            txtPaymentTerm.Text = master.CreditDays > 0 ? $"{master.CreditDays} DAYS" : "CASH";
            txtOutstanding.Text = master.Balance.ToString("N2");
            txtSalesPerson.Text = master.EmpID > 0 ? $"EMP-{master.EmpID}" : string.Empty;
            txtUser.Text = GetUserText(master.UserId);
            txtPoints.Text = "0";
            txtRemark.Text = master.PaymentReference ?? string.Empty;

            lblSubTotalValue.Text = master.SubTotal.ToString("N2");
            lblDiscountValue.Text = master.DiscountAmt.ToString("N2");
            lblTaxValue.Text = master.TaxAmt.ToString("N2");
            lblRoundValue.Text = master.RoundOff.ToString("N2");
            lblTotalValue.Text = master.NetAmount.ToString("N2");

            List<SalesDetails> details = data.ListSDetails?.ToList() ?? new List<SalesDetails>();
            itemsGrid.DataSource = BuildItemsTable(details, master);
            itemsGrid.DataBind();
            if (itemsGrid.DisplayLayout.Bands.Count > 0)
            {
                itemsGrid.DisplayLayout.Bands[0].ColHeadersVisible = true;
            }
            if (itemsGrid.Rows.Count > 0)
            {
                itemsGrid.ActiveRow = itemsGrid.Rows[0];
            }
            itemsGrid.Refresh();
            if (lblItemCountValue != null)
            {
                lblItemCountValue.Text = details.Count.ToString();
            }
            if (lblQtyValue != null)
            {
                lblQtyValue.Text = details.Sum(x => x.Qty).ToString("N4");
            }

            RefreshAvailableBillNumbers();
            ApplyGridSummaries();
        }

        private DataTable BuildItemsTable(List<SalesDetails> details, SalesMaster master)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("No", typeof(int));
            dt.Columns.Add("ItemNo", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("UOM", typeof(string));
            dt.Columns.Add("Qty", typeof(double));
            dt.Columns.Add("QtyReturned", typeof(double));
            dt.Columns.Add("Price", typeof(double));
            dt.Columns.Add("DiscPercent", typeof(double));
            dt.Columns.Add("SellingPrice", typeof(double));
            dt.Columns.Add("Amount", typeof(double));
            dt.Columns.Add("Points", typeof(string));
            dt.Columns.Add("Salesperson", typeof(string));
            dt.Columns.Add("WarrantyDate", typeof(string));
            dt.Columns.Add("FOC", typeof(bool));
            dt.Columns.Add("Remark", typeof(string));

            for (int i = 0; i < details.Count; i++)
            {
                SalesDetails row = details[i];
                double displayAmount = row.TotalAmount > 0 ? row.TotalAmount : row.Amount;
                double sellingPrice = row.UnitPrice > 0 ? row.UnitPrice : row.Amount;
                dt.Rows.Add(
                    i + 1,
                    string.IsNullOrWhiteSpace(row.Barcode) ? row.ItemId.ToString() : row.Barcode,
                    row.ItemName ?? string.Empty,
                    row.Unit ?? string.Empty,
                    row.Qty,
                    0d,
                    row.UnitPrice,
                    row.DiscountPer,
                    sellingPrice,
                    displayAmount,
                    "0",
                    master.EmpID > 0 ? $"EMP-{master.EmpID}" : string.Empty,
                    string.Empty,
                    false,
                    string.Empty);
            }

            return dt;
        }

        private void RefreshAvailableBillNumbers()
        {
            availableBillNumbers.Clear();
            availableBillNumbers.AddRange(GetAvailableBillNumbers());
            UpdateNavigationState();
        }

        private IEnumerable<long> GetAvailableBillNumbers()
        {
            if (!string.Equals(paymentModeFilter, "Cash", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(paymentModeFilter, "Credit", StringComparison.OrdinalIgnoreCase))
            {
                return Enumerable.Empty<long>();
            }

            string filterSql = paymentModeFilter == "Credit"
                ? " AND (ISNULL(sm.IsPaid, 0) = 0 OR ISNULL(sm.[Status], '') = 'Pending' OR UPPER(ISNULL(sm.PaymodeName, '')) = 'CREDIT' OR ISNULL(sm.CreditDays, 0) > 0)"
                : " AND ISNULL(sm.IsPaid, 0) = 1 AND ISNULL(sm.[Status], '') = 'Complete' AND UPPER(ISNULL(sm.PaymodeName, '')) <> 'CREDIT' AND ISNULL(sm.CreditDays, 0) = 0";

            string sql = @"
SELECT TOP 300 sm.BillNo
FROM SMaster sm
WHERE sm.BranchId = @BranchId
  AND sm.CompanyId = @CompanyId
  AND sm.FinYearId = @FinYearId
  AND (sm.CancelFlag = 0 OR sm.CancelFlag IS NULL)
  AND ISNULL(sm.[Status], '') <> 'Hold'" + filterSql + @"
ORDER BY sm.BillNo DESC";

            List<long> result = new List<long>();

            try
            {
                cn.DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)cn.DataConnection))
                {
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["BillNo"] != DBNull.Value)
                            {
                                result.Add(Convert.ToInt64(reader["BillNo"]));
                            }
                        }
                    }
                }
            }
            finally
            {
                if (cn.DataConnection.State == ConnectionState.Open)
                {
                    cn.DataConnection.Close();
                }
            }

            return result;
        }

        private bool IsCreditSale(SalesMaster master)
        {
            if (master == null)
            {
                return false;
            }

            return string.Equals(master.PaymodeName, "Credit", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(master.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                   master.CreditDays > 0;
        }

        private void NavigateRelative(int offset)
        {
            if (!EnsureAvailableBillNumbers(showMessage: true))
            {
                return;
            }

            if (currentBillNo <= 0)
            {
                int startIndex = offset > 0 ? availableBillNumbers.Count - 1 : 0;
                LoadBillView(availableBillNumbers[startIndex]);
                return;
            }

            int currentIndex = availableBillNumbers.IndexOf(currentBillNo);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            int targetIndex = currentIndex + offset;
            if (targetIndex < 0 || targetIndex >= availableBillNumbers.Count)
            {
                return;
            }

            LoadBillView(availableBillNumbers[targetIndex]);
        }

        private void NavigateToBoundary(bool first)
        {
            if (!EnsureAvailableBillNumbers(showMessage: true))
            {
                return;
            }

            int targetIndex = first ? availableBillNumbers.Count - 1 : 0;
            LoadBillView(availableBillNumbers[targetIndex]);
        }

        private void UpdateNavigationState()
        {
            bool modeSelected = string.Equals(paymentModeFilter, "Cash", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(paymentModeFilter, "Credit", StringComparison.OrdinalIgnoreCase);
            if (!modeSelected)
            {
                SetNavigationState(false, false, false, false);
                return;
            }

            if (availableBillNumbers.Count == 0)
            {
                // Keep enabled so click can fetch fresh bills from DB.
                SetNavigationState(true, true, true, true);
                return;
            }

            int currentIndex = availableBillNumbers.IndexOf(currentBillNo);
            if (currentIndex < 0)
            {
                // No active bill yet: allow navigation buttons to fetch from boundaries.
                SetNavigationState(true, true, true, true);
                return;
            }

            // availableBillNumbers is ordered DESC (latest first).
            // Oldest/Older => move to higher index; Latest/Newer => move to lower index.
            bool hasNewer = currentIndex > 0;
            bool hasOlder = currentIndex < availableBillNumbers.Count - 1;

            // ultraPanel3 = oldest, ultraPanel9 = older, ultraPanel8 = newer, ultraPanel10 = latest
            SetNavigationState(hasOlder, hasOlder, hasNewer, hasNewer);
        }

        private void SetNavigationState(bool firstEnabled, bool previousEnabled, bool nextEnabled, bool lastEnabled)
        {
            SetNavigationControlState(ultraPanel3, ultraPictureBox2, firstEnabled);
            SetNavigationControlState(ultraPanel9, ultraPictureBox4, previousEnabled);
            SetNavigationControlState(ultraPanel8, ultraPictureBox6, nextEnabled);
            SetNavigationControlState(ultraPanel10, ultraPictureBox5, lastEnabled);
        }

        private static void SetNavigationControlState(Control panel, Control icon, bool enabled)
        {
            if (panel == null || icon == null)
            {
                return;
            }

            panel.Enabled = enabled;
            icon.Enabled = enabled;
            panel.Cursor = enabled ? Cursors.Hand : Cursors.Default;
            icon.Cursor = enabled ? Cursors.Hand : Cursors.Default;
        }

        private bool EnsureAvailableBillNumbers(bool showMessage)
        {
            bool modeSelected = string.Equals(paymentModeFilter, "Cash", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(paymentModeFilter, "Credit", StringComparison.OrdinalIgnoreCase);
            if (!modeSelected)
            {
                if (showMessage)
                {
                    MessageBox.Show("Please choose Cash Sales or Credit Sales first.", "Select Sales Type", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return false;
            }

            if (availableBillNumbers.Count == 0)
            {
                RefreshAvailableBillNumbers();
            }

            if (availableBillNumbers.Count == 0)
            {
                if (showMessage)
                {
                    MessageBox.Show($"No {paymentModeFilter} bills found.", "No Bills", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return false;
            }

            return true;
        }

        private void ClearViewer(bool preserveModeSelection = true)
        {
            currentBillNo = 0;
            txtDocNoDisplay.Text = string.Empty;
            txtAccountCode.Text = string.Empty;
            txtAccountName.Text = string.Empty;
            txtBranch.Text = string.Empty;
            txtBillDate.Text = string.Empty;
            txtPaymentMode.Text = string.Empty;
            txtReference.Text = string.Empty;
            txtPaymentTerm.Text = string.Empty;
            txtOutstanding.Text = string.Empty;
            txtSalesPerson.Text = string.Empty;
            txtUser.Text = string.Empty;
            txtPoints.Text = "0";
            txtRemark.Text = string.Empty;
            lblSubTotalValue.Text = "0.00";
            lblDiscountValue.Text = "0.00";
            lblTaxValue.Text = "0.00";
            lblRoundValue.Text = "0.00";
            lblTotalValue.Text = "0.00";
            if (lblItemCountValue != null) lblItemCountValue.Text = "0";
            if (lblQtyValue != null) lblQtyValue.Text = "0.0000";
            itemsGrid.DataSource = null;
            summaryLabels.Clear();
            if (summaryStrip != null && summaryStrip.ClientArea != null)
            {
                summaryStrip.ClientArea.Controls.Clear();
            }
            if (itemsGrid.DisplayLayout != null && itemsGrid.DisplayLayout.Bands.Count > 0)
            {
                itemsGrid.DisplayLayout.Bands[0].Summaries.Clear();
            }
            suppressPaymentModeChange = true;
            if (preserveModeSelection)
            {
                rdoCash.Checked = paymentModeFilter == "Cash";
                rdoCredit.Checked = paymentModeFilter == "Credit";
            }
            else
            {
                paymentModeFilter = string.Empty;
                rdoCash.Checked = false;
                rdoCredit.Checked = false;
            }
            suppressPaymentModeChange = false;
            UpdateModeBadge();
            UpdateNavigationState();
        }

        private string GetAccountCode(SalesMaster master)
        {
            if (master.LedgerID > 0)
            {
                return master.LedgerID.ToString();
            }

            return string.IsNullOrWhiteSpace(master.PaymodeName)
                ? string.Empty
                : master.PaymodeName.ToUpperInvariant();
        }

        private string GetAccountName(SalesMaster master)
        {
            if (!string.IsNullOrWhiteSpace(master.CustomerName))
            {
                return master.CustomerName;
            }

            if (!string.IsNullOrWhiteSpace(master.PaymodeName))
            {
                return master.PaymodeName.ToUpperInvariant();
            }

            return "Walk In Customer";
        }

        private string GetUserText(int userId)
        {
            if (userId > 0 && userId == SessionContext.UserId && !string.IsNullOrWhiteSpace(SessionContext.UserName))
            {
                return SessionContext.UserName;
            }

            return userId > 0 ? $"USER-{userId}" : (SessionContext.UserName ?? string.Empty);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (currentBillNo <= 0)
            {
                MessageBox.Show("Please choose a bill first, then print.", "Bill Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                long billNoToPrint = currentBillNo;
                Task.Run(() =>
                {
                    try
                    {
                        ReportViewer printer = new ReportViewer();
                        printer.PrintBill(billNoToPrint);
                    }
                    catch (Exception ex)
                    {
                        BeginInvoke((Action)(() =>
                            MessageBox.Show("Printing failed: " + ex.Message, "Print Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Printing failed: " + ex.Message, "Print Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11)
            {
                OpenBillPicker();
                e.Handled = true;
            }
        }
    }
}
