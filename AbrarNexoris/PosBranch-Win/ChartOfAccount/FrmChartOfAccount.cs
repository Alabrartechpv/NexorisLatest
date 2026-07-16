using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.ChartOfAccount
{
    public partial class FrmChartOfAccount : Form
    {
        // Variables for form dragging
        private bool isDragging = false;
        private Point lastPosition;

        private Panel panelMain;
        private Panel panelLeft;
        private Panel panelRight;
        private TextBox txtSearch;
        private TreeView treeAccounts;
        private Label lblAccountCode;
        private Label lblAccountName;
        private Label lblCategory;
        private Label lblParentAccount;
        private Label lblDescription;
        private TextBox txtAccountCode;
        private TextBox txtAccountName;
        private ComboBox cmbCategory;
        private ComboBox cmbParentAccount;
        private TextBox txtDescription;
        private Button btnAdd;
        private Button btnSave;
        private Button btnAddSubAccount;
        private Button btnAddAccount;
        private Button btnDelete;
        private ContextMenuStrip contextMenuStrip;
        private ImageList treeImageList;

        public FrmChartOfAccount()
        {
            InitializeComponent();
            InitializeCustomComponents();
            InitializeContextMenu();
            InitializeTreeIcons();
            LoadChartOfAccounts();
        }

        private void InitializeCustomComponents()
        {
            // Set form properties
            this.Text = "Chart of Accounts";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(11, 32, 71); // Dark blue background
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;

            // Add title panel with gradient
            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 50;
            titlePanel.Paint += (s, e) => {
                Rectangle rect = new Rectangle(0, 0, titlePanel.Width, titlePanel.Height);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect, Color.FromArgb(20, 60, 140), Color.FromArgb(11, 32, 71), 0.0f))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }

                // Draw title text
                using (Font titleFont = new Font("Segoe UI", 14, FontStyle.Bold))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString("Chart of Accounts", titleFont, Brushes.White, rect, sf);
                }
            };
            this.Controls.Add(titlePanel);

            // Create close button
            Button btnClose = new Button();
            btnClose.Text = "✕";
            btnClose.Size = new Size(40, 40);
            btnClose.Location = new Point(this.Width - 50, 5);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Font = new Font("Arial", 14);
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.Transparent;
            btnClose.Click += (s, e) => this.Close();
            titlePanel.Controls.Add(btnClose);

            // Create main panel
            panelMain = new Panel();
            panelMain.BackColor = Color.FromArgb(20, 59, 120); // Medium blue
            panelMain.Location = new Point(20, 70);
            panelMain.Size = new Size(this.Width - 40, this.Height - 90);
            this.Controls.Add(panelMain);

            // Left Panel (Tree View and Search)
            panelLeft = new Panel();
            panelLeft.BackColor = Color.White;
            panelLeft.Location = new Point(0, 0);
            panelLeft.Size = new Size(panelMain.Width / 2 - 10, panelMain.Height);
            panelMain.Controls.Add(panelLeft);

            // Search Box with icon - similar to second image
            Panel searchPanel = new Panel();
            searchPanel.Size = new Size(panelLeft.Width - 20, 40);
            searchPanel.Location = new Point(10, 10);
            searchPanel.BackColor = Color.White;
            searchPanel.BorderStyle = BorderStyle.FixedSingle;
            panelLeft.Controls.Add(searchPanel);

            // Add search icon
            PictureBox searchIcon = new PictureBox();
            searchIcon.Size = new Size(20, 20);
            searchIcon.Location = new Point(10, 10);
            searchIcon.Image = CreateSearchIcon();
            searchIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            searchPanel.Controls.Add(searchIcon);

            // Search TextBox
            txtSearch = new TextBox();
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Size = new Size(searchPanel.Width - 50, 30);
            txtSearch.Location = new Point(40, 8);
            txtSearch.Font = new Font("Segoe UI", 12);
            txtSearch.BackColor = Color.White;
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Text = "Search";

            // Add search box events
            txtSearch.Enter += (s, e) => {
                if (txtSearch.Text == "Search")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };
            txtSearch.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "Search";
                    txtSearch.ForeColor = Color.Gray;
                }
            };
            txtSearch.TextChanged += (s, e) => {
                if (txtSearch.Text != "Search" && !string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    FilterTreeNodes(txtSearch.Text);
                }
                else if (txtSearch.Text == "")
                {
                    ResetTreeView();
                }
            };
            searchPanel.Controls.Add(txtSearch);

            // TreeView
            treeAccounts = new TreeView();
            treeAccounts.Location = new Point(10, 60);
            treeAccounts.Size = new Size(panelLeft.Width - 20, panelLeft.Height - 70);
            treeAccounts.Font = new Font("Segoe UI", 11);
            treeAccounts.BackColor = Color.White;
            treeAccounts.BorderStyle = BorderStyle.None;
            treeAccounts.ShowLines = true;
            treeAccounts.FullRowSelect = true;
            treeAccounts.HideSelection = false;
            treeAccounts.ItemHeight = 30;
            treeAccounts.Indent = 25;
            treeAccounts.ShowNodeToolTips = true;
            treeAccounts.LabelEdit = false; // Prevent direct label editing
            treeAccounts.AfterSelect += TreeAccounts_AfterSelect;
            treeAccounts.NodeMouseClick += TreeAccounts_NodeMouseClick;

            // Enable drag and drop
            treeAccounts.AllowDrop = true;
            treeAccounts.ItemDrag += TreeAccounts_ItemDrag;
            treeAccounts.DragEnter += TreeAccounts_DragEnter;
            treeAccounts.DragOver += TreeAccounts_DragOver;
            treeAccounts.DragDrop += TreeAccounts_DragDrop;

            panelLeft.Controls.Add(treeAccounts);

            // Right Panel (Account Details)
            panelRight = new Panel();
            panelRight.BackColor = Color.FromArgb(20, 59, 120);
            panelRight.Location = new Point(panelMain.Width / 2, 0);
            panelRight.Size = new Size(panelMain.Width / 2, panelMain.Height);
            panelMain.Controls.Add(panelRight);

            // Account Code
            lblAccountCode = new Label();
            lblAccountCode.Text = "Account Code";
            lblAccountCode.Font = new Font("Segoe UI", 11);
            lblAccountCode.ForeColor = Color.White;
            lblAccountCode.AutoSize = true;
            lblAccountCode.Location = new Point(30, 30);
            panelRight.Controls.Add(lblAccountCode);

            txtAccountCode = new TextBox();
            txtAccountCode.Size = new Size(panelRight.Width - 60, 35);
            txtAccountCode.Location = new Point(30, 60);
            txtAccountCode.Font = new Font("Segoe UI", 12);
            txtAccountCode.BackColor = Color.White;
            txtAccountCode.MaxLength = 10;
            panelRight.Controls.Add(txtAccountCode);

            // Account Name
            lblAccountName = new Label();
            lblAccountName.Text = "Account Name";
            lblAccountName.Font = new Font("Segoe UI", 11);
            lblAccountName.ForeColor = Color.White;
            lblAccountName.AutoSize = true;
            lblAccountName.Location = new Point(30, 110);
            panelRight.Controls.Add(lblAccountName);

            txtAccountName = new TextBox();
            txtAccountName.Size = new Size(panelRight.Width - 60, 35);
            txtAccountName.Location = new Point(30, 140);
            txtAccountName.Font = new Font("Segoe UI", 12);
            txtAccountName.BackColor = Color.White;
            panelRight.Controls.Add(txtAccountName);

            // Category
            lblCategory = new Label();
            lblCategory.Text = "Category";
            lblCategory.Font = new Font("Segoe UI", 11);
            lblCategory.ForeColor = Color.White;
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(30, 190);
            panelRight.Controls.Add(lblCategory);

            cmbCategory = new ComboBox();
            cmbCategory.Size = new Size(panelRight.Width - 60, 35);
            cmbCategory.Location = new Point(30, 220);
            cmbCategory.Font = new Font("Segoe UI", 12);
            cmbCategory.BackColor = Color.White;
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Items.AddRange(new object[] { "Assets", "Liabilities", "Income", "Expenses", "Equity" });
            panelRight.Controls.Add(cmbCategory);

            // Parent Account
            lblParentAccount = new Label();
            lblParentAccount.Text = "Parent Account";
            lblParentAccount.Font = new Font("Segoe UI", 11);
            lblParentAccount.ForeColor = Color.White;
            lblParentAccount.AutoSize = true;
            lblParentAccount.Location = new Point(30, 270);
            panelRight.Controls.Add(lblParentAccount);

            cmbParentAccount = new ComboBox();
            cmbParentAccount.Size = new Size(panelRight.Width - 60, 35);
            cmbParentAccount.Location = new Point(30, 300);
            cmbParentAccount.Font = new Font("Segoe UI", 12);
            cmbParentAccount.BackColor = Color.White;
            cmbParentAccount.DropDownStyle = ComboBoxStyle.DropDownList;
            panelRight.Controls.Add(cmbParentAccount);

            // Description
            lblDescription = new Label();
            lblDescription.Text = "Description";
            lblDescription.Font = new Font("Segoe UI", 11);
            lblDescription.ForeColor = Color.White;
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(30, 350);
            panelRight.Controls.Add(lblDescription);

            txtDescription = new TextBox();
            txtDescription.Multiline = true;
            txtDescription.Size = new Size(panelRight.Width - 60, 80); // Reduce height to match second image
            txtDescription.Location = new Point(30, 380);
            txtDescription.Font = new Font("Segoe UI", 12);
            txtDescription.BackColor = Color.White;
            panelRight.Controls.Add(txtDescription);

            // Top Row Buttons (Add, Save, Delete)
            // Add button with plus icon
            btnAdd = new Button();
            btnAdd.Text = "Add";
            btnAdd.Size = new Size(150, 40);
            btnAdd.Location = new Point(30, 490);
            btnAdd.Font = new Font("Segoe UI", 11);
            btnAdd.BackColor = Color.FromArgb(25, 120, 220);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.TextAlign = ContentAlignment.MiddleCenter;
            btnAdd.ImageAlign = ContentAlignment.MiddleLeft;
            btnAdd.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAdd.Image = CreatePlusIcon(16, 16);
            btnAdd.Padding = new Padding(10, 0, 0, 0);
            btnAdd.Click += BtnAdd_Click;
            panelRight.Controls.Add(btnAdd);

            // Save Button
            btnSave = new Button();
            btnSave.Text = "Save";
            btnSave.Size = new Size(150, 40);
            btnSave.Location = new Point(panelRight.Width - 180, 490);
            btnSave.Font = new Font("Segoe UI", 11);
            btnSave.BackColor = Color.FromArgb(25, 120, 220);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            panelRight.Controls.Add(btnSave);

            // Bottom Row Buttons (Add Sub-Account, Add)
            // Add Sub-Account Button with plus icon
            btnAddSubAccount = new Button();
            btnAddSubAccount.Text = "Add Sub-Account";
            btnAddSubAccount.Size = new Size(180, 40);
            btnAddSubAccount.Location = new Point(30, 550);
            btnAddSubAccount.Font = new Font("Segoe UI", 11);
            btnAddSubAccount.BackColor = Color.White;
            btnAddSubAccount.ForeColor = Color.FromArgb(20, 59, 120);
            btnAddSubAccount.FlatStyle = FlatStyle.Flat;
            btnAddSubAccount.FlatAppearance.BorderSize = 1;
            btnAddSubAccount.FlatAppearance.BorderColor = Color.FromArgb(20, 59, 120);
            btnAddSubAccount.TextAlign = ContentAlignment.MiddleCenter;
            btnAddSubAccount.ImageAlign = ContentAlignment.MiddleLeft;
            btnAddSubAccount.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAddSubAccount.Image = CreatePlusIcon(16, 16, Color.FromArgb(20, 59, 120));
            btnAddSubAccount.Padding = new Padding(10, 0, 0, 0);
            btnAddSubAccount.Click += BtnAddSubAccount_Click;
            panelRight.Controls.Add(btnAddSubAccount);

            // Bottom Add Button with document icon
            btnAddAccount = new Button();
            btnAddAccount.Text = "Add";
            btnAddAccount.Size = new Size(150, 40);
            btnAddAccount.Location = new Point(panelRight.Width - 180, 550);
            btnAddAccount.Font = new Font("Segoe UI", 11);
            btnAddAccount.BackColor = Color.White;
            btnAddAccount.ForeColor = Color.FromArgb(20, 59, 120);
            btnAddAccount.FlatStyle = FlatStyle.Flat;
            btnAddAccount.FlatAppearance.BorderSize = 1;
            btnAddAccount.FlatAppearance.BorderColor = Color.FromArgb(20, 59, 120);
            btnAddAccount.TextAlign = ContentAlignment.MiddleCenter;
            btnAddAccount.ImageAlign = ContentAlignment.MiddleLeft;
            btnAddAccount.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAddAccount.Image = CreateDocumentIcon(16, 16);
            btnAddAccount.Padding = new Padding(10, 0, 0, 0);
            btnAddAccount.Click += BtnAddAccount_Click;
            panelRight.Controls.Add(btnAddAccount);

            // Delete Button hidden - will show contextually
            btnDelete = new Button();
            btnDelete.Text = "Delete";
            btnDelete.Size = new Size(120, 40);
            btnDelete.Location = new Point(panelRight.Width - 310, 490);
            btnDelete.Font = new Font("Segoe UI", 11);
            btnDelete.BackColor = Color.FromArgb(220, 53, 69); // Red color for delete
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;
            btnDelete.Visible = false; // Hide by default, show when editing
            panelRight.Controls.Add(btnDelete);

            // Enable form dragging on the title panel
            titlePanel.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    lastPosition = e.Location;
                }
            };

            titlePanel.MouseMove += (s, e) => {
                if (isDragging)
                {
                    this.Location = new Point(
                        this.Location.X + (e.X - lastPosition.X),
                        this.Location.Y + (e.Y - lastPosition.Y));
                }
            };

            titlePanel.MouseUp += (s, e) => {
                isDragging = false;
            };
        }

        private void InitializeContextMenu()
        {
            // Initialize context menu
            contextMenuStrip = new ContextMenuStrip();

            // Add menu items
            ToolStripMenuItem addMenuItem = new ToolStripMenuItem("Add Account");
            addMenuItem.Click += (s, e) => BtnAddAccount_Click(s, e);

            ToolStripMenuItem addSubMenuItem = new ToolStripMenuItem("Add Sub-Account");
            addSubMenuItem.Click += (s, e) => BtnAddSubAccount_Click(s, e);

            ToolStripMenuItem editMenuItem = new ToolStripMenuItem("Edit");
            editMenuItem.Click += (s, e) => {
                // Populate form fields with selected node data
                if (treeAccounts.SelectedNode != null)
                {
                    TreeAccounts_AfterSelect(treeAccounts, new TreeViewEventArgs(treeAccounts.SelectedNode));
                }
            };

            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("Delete");
            deleteMenuItem.Click += (s, e) => BtnDelete_Click(s, e);

            // Add items to context menu
            contextMenuStrip.Items.Add(addMenuItem);
            contextMenuStrip.Items.Add(addSubMenuItem);
            contextMenuStrip.Items.Add(editMenuItem);
            contextMenuStrip.Items.Add(deleteMenuItem);

            // Set the context menu for the TreeView
            treeAccounts.ContextMenuStrip = contextMenuStrip;
        }

        private void InitializeTreeIcons()
        {
            // Create image list for tree icons
            treeImageList = new ImageList();
            treeImageList.ColorDepth = ColorDepth.Depth32Bit;
            treeImageList.ImageSize = new Size(16, 16);

            // Add icons for different categories
            treeImageList.Images.Add("folder", CreateFolderIcon(Color.FromArgb(25, 120, 220))); // Blue folder for categories
            treeImageList.Images.Add("assets", CreateCategoryIcon(Color.FromArgb(46, 204, 113))); // Green for assets
            treeImageList.Images.Add("liabilities", CreateCategoryIcon(Color.FromArgb(231, 76, 60))); // Red for liabilities
            treeImageList.Images.Add("equity", CreateCategoryIcon(Color.FromArgb(155, 89, 182))); // Purple for equity
            treeImageList.Images.Add("income", CreateCategoryIcon(Color.FromArgb(52, 152, 219))); // Blue for income
            treeImageList.Images.Add("expenses", CreateCategoryIcon(Color.FromArgb(241, 196, 15))); // Yellow for expenses
            treeImageList.Images.Add("account", CreateTreeNodeDocumentIcon(Color.FromArgb(149, 165, 166))); // Gray for accounts

            // Assign to treeview
            treeAccounts.ImageList = treeImageList;
        }

        // Helper method to create folder icon
        private Bitmap CreateFolderIcon(Color color)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw folder icon
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddRectangle(new Rectangle(1, 3, 14, 12));
                path.AddPolygon(new Point[] {
                    new Point(1, 3),
                    new Point(5, 1),
                    new Point(15, 1),
                    new Point(15, 3)
                });

                g.FillPath(new SolidBrush(color), path);
                g.DrawPath(new Pen(Color.Black, 0.5f), path);
            }
            return bmp;
        }

        // Helper method to create category icon
        private Bitmap CreateCategoryIcon(Color color)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw simple circle icon
                g.FillEllipse(new SolidBrush(color), new Rectangle(2, 2, 12, 12));
                g.DrawEllipse(new Pen(Color.Black, 0.5f), new Rectangle(2, 2, 12, 12));
            }
            return bmp;
        }

        // Helper method to create document icon for accounts in TreeView
        private Bitmap CreateDocumentIcon(int width, int height)
        {
            Color iconColor = Color.FromArgb(20, 59, 120);
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw document outline
                using (Pen pen = new Pen(iconColor, 1.5f))
                {
                    g.DrawRectangle(pen, new Rectangle(2, 2, width - 4, height - 4));
                }
            }
            return bmp;
        }

        // Helper method to create document icon for TreeView nodes (with color fill)
        private Bitmap CreateTreeNodeDocumentIcon(Color color)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw document icon
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddRectangle(new Rectangle(2, 1, 12, 14));

                g.FillPath(new SolidBrush(color), path);
                g.DrawPath(new Pen(Color.Black, 0.5f), path);

                // Draw lines for text
                g.DrawLine(new Pen(Color.White, 1), new Point(4, 4), new Point(12, 4));
                g.DrawLine(new Pen(Color.White, 1), new Point(4, 7), new Point(12, 7));
                g.DrawLine(new Pen(Color.White, 1), new Point(4, 10), new Point(10, 10));
            }
            return bmp;
        }

        // Helper method to create search icon
        private Image CreateSearchIcon()
        {
            Bitmap bmp = new Bitmap(20, 20);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw search glass
                using (Pen pen = new Pen(Color.Gray, 2))
                {
                    g.DrawEllipse(pen, new Rectangle(2, 2, 12, 12));
                    g.DrawLine(pen, new Point(13, 13), new Point(18, 18));
                }
            }
            return bmp;
        }

        // Helper method to create plus icon
        private Image CreatePlusIcon(int width, int height, Color? color = null)
        {
            Color iconColor = color ?? Color.White;
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw plus sign
                using (Pen pen = new Pen(iconColor, 2))
                {
                    g.DrawLine(pen, new Point(width / 2, 3), new Point(width / 2, height - 3));
                    g.DrawLine(pen, new Point(3, height / 2), new Point(width - 3, height / 2));
                }
            }
            return bmp;
        }

        // Event handler for right-clicking on a node
        private void TreeAccounts_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Select the clicked node
                treeAccounts.SelectedNode = e.Node;

                // Configure context menu items based on node type
                bool isRootNode = e.Node.Parent == null;

                // Get the menu items
                ToolStripMenuItem addMenuItem = contextMenuStrip.Items[0] as ToolStripMenuItem;
                ToolStripMenuItem addSubMenuItem = contextMenuStrip.Items[1] as ToolStripMenuItem;
                ToolStripMenuItem editMenuItem = contextMenuStrip.Items[2] as ToolStripMenuItem;
                ToolStripMenuItem deleteMenuItem = contextMenuStrip.Items[3] as ToolStripMenuItem;

                // For root nodes (categories), show Add but hide Delete
                if (isRootNode)
                {
                    addMenuItem.Visible = true;
                    addSubMenuItem.Visible = true;
                    editMenuItem.Visible = false; // Can't edit root categories
                    deleteMenuItem.Visible = false; // Can't delete root categories
                }
                else
                {
                    // For normal accounts, show all options
                    addMenuItem.Visible = true;
                    addSubMenuItem.Visible = true;
                    editMenuItem.Visible = true;
                    deleteMenuItem.Visible = true;
                }

                // Show context menu
                contextMenuStrip.Show(treeAccounts, e.Location);
            }
        }

        // Helper method to create styled textboxes with rounded corners
        private TextBox CreateStyledTextBox(int width, int height, int x, int y)
        {
            TextBox textBox = new TextBox();
            textBox.Size = new Size(width, height);
            textBox.Location = new Point(x, y);
            textBox.Font = new Font("Segoe UI", 12);
            textBox.BorderStyle = BorderStyle.None;

            // Create a panel to host the textbox with rounded corners
            Panel panel = new Panel();
            panel.Size = new Size(width, height);
            panel.Location = new Point(x, y);
            panel.BackColor = Color.White;
            panel.Paint += (s, e) => {
                // Draw rounded rectangle
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, 10, 10, 180, 90);
                path.AddArc(panel.Width - 10, 0, 10, 10, 270, 90);
                path.AddArc(panel.Width - 10, panel.Height - 10, 10, 10, 0, 90);
                path.AddArc(0, panel.Height - 10, 10, 10, 90, 90);
                path.CloseAllFigures();
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillPath(new SolidBrush(Color.White), path);
            };

            // Adjust textbox position inside panel
            textBox.Location = new Point(10, (height - textBox.Height) / 2);
            textBox.Width = width - 20;
            panel.Controls.Add(textBox);
            panelRight.Controls.Add(panel);

            return textBox;
        }

        // Helper method to create styled buttons
        private Button CreateStyledButton(string text, int x, int y, int width, int height, bool isBlue)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(width, height);
            button.Location = new Point(x, y);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            button.Cursor = Cursors.Hand;

            if (isBlue)
            {
                // Blue button style (Add, Save)
                button.BackColor = Color.FromArgb(25, 120, 220); // Bright blue
                button.ForeColor = Color.White;

                button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(35, 130, 230);
                button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(25, 120, 220);
            }
            else
            {
                // White button style (Add Sub-Account, Add at bottom)
                button.BackColor = Color.White;
                button.ForeColor = Color.FromArgb(20, 59, 120);

                button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(240, 240, 240);
                button.MouseLeave += (s, e) => button.BackColor = Color.White;
            }

            // Round corners for button
            button.Paint += (s, e) => {
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, 10, 10, 180, 90);
                path.AddArc(button.Width - 10, 0, 10, 10, 270, 90);
                path.AddArc(button.Width - 10, button.Height - 10, 10, 10, 0, 90);
                path.AddArc(0, button.Height - 10, 10, 10, 90, 90);
                path.CloseAllFigures();
                button.Region = new Region(path);
            };

            return button;
        }

        // Create simple image for buttons
        private Image CreateButtonImage(string text, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (Font font = new Font("Segoe UI", 9))
                {
                    g.DrawString(text, font, Brushes.White, 0, 0);
                }
            }
            return bmp;
        }

        // Filter tree view nodes based on search text
        private void FilterTreeNodes(string searchText)
        {
            searchText = searchText.ToLower();

            // Clear and reset the tree view
            treeAccounts.BeginUpdate();

            // Store current selection
            string selectedNodeText = treeAccounts.SelectedNode?.Text ?? "";

            // Reload data
            LoadChartOfAccounts();

            // If search text is not empty, collapse all and then expand matching nodes
            if (!string.IsNullOrEmpty(searchText))
            {
                treeAccounts.CollapseAll();

                // Find and highlight matching nodes
                FindAndExpandMatchingNodes(treeAccounts.Nodes, searchText);
            }

            // Try to restore selection
            if (!string.IsNullOrEmpty(selectedNodeText))
            {
                foreach (TreeNode node in GetAllNodes(treeAccounts.Nodes))
                {
                    if (node.Text == selectedNodeText)
                    {
                        treeAccounts.SelectedNode = node;
                        break;
                    }
                }
            }

            treeAccounts.EndUpdate();
        }

        // Helper method to find and expand nodes matching search text
        private bool FindAndExpandMatchingNodes(TreeNodeCollection nodes, string searchText)
        {
            bool anyMatches = false;

            foreach (TreeNode node in nodes)
            {
                // Check if current node matches
                bool nodeMatches = node.Text.ToLower().Contains(searchText);

                // Check if any children match
                bool childrenMatch = FindAndExpandMatchingNodes(node.Nodes, searchText);

                if (nodeMatches || childrenMatch)
                {
                    // Expand all parent nodes
                    ExpandParentNodes(node);
                    anyMatches = true;
                }
            }

            return anyMatches;
        }

        // Helper method to expand all parent nodes
        private void ExpandParentNodes(TreeNode node)
        {
            if (node.Parent != null)
            {
                node.Parent.Expand();
                ExpandParentNodes(node.Parent);
            }
        }

        // Reset tree view to normal state
        private void ResetTreeView()
        {
            treeAccounts.BeginUpdate();
            LoadChartOfAccounts();
            treeAccounts.EndUpdate();
        }

        // Helper method to get all nodes in tree view
        private List<TreeNode> GetAllNodes(TreeNodeCollection nodes)
        {
            List<TreeNode> allNodes = new List<TreeNode>();

            foreach (TreeNode node in nodes)
            {
                allNodes.Add(node);
                allNodes.AddRange(GetAllNodes(node.Nodes));
            }

            return allNodes;
        }

        private void LoadChartOfAccounts()
        {
            treeAccounts.Nodes.Clear();
            cmbParentAccount.Items.Clear();

            // Initialize root nodes with account codes
            TreeNode assetsNode = new TreeNode("1. Assets");
            assetsNode.ImageKey = "assets";
            assetsNode.SelectedImageKey = "assets";
            assetsNode.ToolTipText = "Asset accounts represent what the business owns";

            TreeNode liabilitiesNode = new TreeNode("2. Liabilities");
            liabilitiesNode.ImageKey = "liabilities";
            liabilitiesNode.SelectedImageKey = "liabilities";
            liabilitiesNode.ToolTipText = "Liability accounts represent what the business owes";

            TreeNode equityNode = new TreeNode("3. Equity");
            equityNode.ImageKey = "equity";
            equityNode.SelectedImageKey = "equity";
            equityNode.ToolTipText = "Equity accounts represent the owner's investment in the business";

            TreeNode incomeNode = new TreeNode("4. Income");
            incomeNode.ImageKey = "income";
            incomeNode.SelectedImageKey = "income";
            incomeNode.ToolTipText = "Income accounts represent revenue earned by the business";

            TreeNode expensesNode = new TreeNode("5. Expenses");
            expensesNode.ImageKey = "expenses";
            expensesNode.SelectedImageKey = "expenses";
            expensesNode.ToolTipText = "Expense accounts represent costs incurred by the business";

            treeAccounts.Nodes.Add(assetsNode);
            treeAccounts.Nodes.Add(liabilitiesNode);
            treeAccounts.Nodes.Add(equityNode);
            treeAccounts.Nodes.Add(incomeNode);
            treeAccounts.Nodes.Add(expensesNode);

            // Add child nodes to Assets
            AddAccountNode(assetsNode, "1100", "Cash");
            AddAccountNode(assetsNode, "1200", "Bank Accounts");
            AddAccountNode(assetsNode, "1300", "Inventory");
            AddAccountNode(assetsNode, "1400", "Accounts Receivable");

            // Add child nodes to Liabilities
            AddAccountNode(liabilitiesNode, "2100", "Accounts Payable");
            AddAccountNode(liabilitiesNode, "2200", "Loans");
            AddAccountNode(liabilitiesNode, "2300", "Tax Payable");

            // Add child nodes to Equity
            AddAccountNode(equityNode, "3100", "Capital");
            AddAccountNode(equityNode, "3200", "Retained Earnings");

            // Add child nodes to Income
            AddAccountNode(incomeNode, "4100", "Sales Revenue");
            AddAccountNode(incomeNode, "4200", "Other Income");

            // Add child nodes to Expenses
            AddAccountNode(expensesNode, "5100", "Cost of Goods Sold");
            AddAccountNode(expensesNode, "5200", "Operating Expenses");
            AddAccountNode(expensesNode, "5300", "Payroll Expenses");

            // Expand all nodes
            treeAccounts.ExpandAll();
        }

        private void AddAccountNode(TreeNode parentNode, string code, string name)
        {
            TreeNode node = new TreeNode(code + "   " + name);
            node.Tag = code; // Store code in Tag for reference
            node.ImageKey = "account";
            node.SelectedImageKey = "account";
            node.ToolTipText = "Account Code: " + code + "\nAccount Name: " + name;
            parentNode.Nodes.Add(node);
            cmbParentAccount.Items.Add(code + " - " + name);
        }

        private void TreeAccounts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string nodeText = e.Node.Text;

            // Show Delete button only for non-root nodes
            btnDelete.Visible = (e.Node.Parent != null);

            // Check if this is a leaf node with an account code
            if (nodeText.Contains("."))
            {
                // This is a header node like "1. Assets"
                string[] parts = nodeText.Split('.');
                txtAccountCode.Text = "";
                txtAccountName.Text = parts[1].Trim();
                cmbCategory.SelectedIndex = GetCategoryIndex(parts[0].Trim());
                cmbParentAccount.SelectedIndex = -1;
            }
            else if (nodeText.Contains("   "))
            {
                // This is an account node like "1100   Cash"
                string[] parts = nodeText.Split(new string[] { "   " }, StringSplitOptions.None);
                txtAccountCode.Text = parts[0];
                txtAccountName.Text = parts[1];

                // Try to determine parent from the account code
                string parentPrefix = parts[0].Substring(0, 1);
                cmbCategory.SelectedIndex = GetCategoryIndex(parentPrefix);

                // If this is a child node, set parent account
                if (e.Node.Parent != null)
                {
                    string parentText = e.Node.Parent.Text;
                    for (int i = 0; i < cmbParentAccount.Items.Count; i++)
                    {
                        if (cmbParentAccount.Items[i].ToString().Contains(parentText))
                        {
                            cmbParentAccount.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        private int GetCategoryIndex(string prefix)
        {
            switch (prefix)
            {
                case "1": return 0; // Assets
                case "2": return 1; // Liabilities
                case "3": return 2; // Equity
                case "4": return 3; // Income
                case "5": return 4; // Expenses
                default: return -1;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Add or update account logic
            SaveAccount();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Save account logic
            SaveAccount();
        }

        private void SaveAccount()
        {
            // Validate basic requirements
            if (string.IsNullOrWhiteSpace(txtAccountCode.Text) || string.IsNullOrWhiteSpace(txtAccountName.Text))
            {
                MessageBox.Show("Account code and name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate account code format (numeric only)
            if (!txtAccountCode.Text.All(char.IsDigit))
            {
                MessageBox.Show("Account code must contain only numbers.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate account code uniqueness
            if (IsAccountCodeDuplicate(txtAccountCode.Text, treeAccounts.SelectedNode))
            {
                MessageBox.Show("Account code must be unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get selected node or parent node
            TreeNode parentNode = null;
            TreeNode selectedNode = treeAccounts.SelectedNode;

            if (selectedNode != null)
            {
                if (selectedNode.Parent != null)
                {
                    // If selected node has a parent, use the parent as the parent node
                    parentNode = selectedNode.Parent;
                }
                else
                {
                    // If selected node is a root node, use it as the parent node
                    parentNode = selectedNode;
                }
            }
            else if (cmbParentAccount.SelectedIndex >= 0)
            {
                // Find parent node from combobox selection
                string parentText = cmbParentAccount.SelectedItem.ToString();
                foreach (TreeNode node in treeAccounts.Nodes)
                {
                    if (node.Text.Contains(parentText) || parentText.Contains(node.Text))
                    {
                        parentNode = node;
                        break;
                    }
                }
            }

            if (parentNode == null)
            {
                MessageBox.Show("Please select a parent account.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create new node or update existing node
            string nodeText = txtAccountCode.Text + "   " + txtAccountName.Text;

            if (selectedNode != null && selectedNode.Parent != null)
            {
                // Update existing node
                selectedNode.Text = nodeText;
                selectedNode.Tag = txtAccountCode.Text;
            }
            else
            {
                // Create new node
                TreeNode newNode = new TreeNode(nodeText);
                newNode.Tag = txtAccountCode.Text;
                parentNode.Nodes.Add(newNode);
                parentNode.Expand();
            }

            // Refresh parent account combobox
            RefreshParentAccountComboBox();

            // Clear form or keep values based on your preference
            ClearForm();
        }

        private void RefreshParentAccountComboBox()
        {
            cmbParentAccount.Items.Clear();

            foreach (TreeNode rootNode in treeAccounts.Nodes)
            {
                cmbParentAccount.Items.Add(rootNode.Text);
                foreach (TreeNode childNode in rootNode.Nodes)
                {
                    string code = childNode.Tag?.ToString() ?? "";
                    string name = childNode.Text.Contains("   ") ? childNode.Text.Split(new string[] { "   " }, StringSplitOptions.None)[1] : childNode.Text;
                    cmbParentAccount.Items.Add(code + " - " + name);
                }
            }
        }

        private void ClearForm()
        {
            txtAccountCode.Text = "";
            txtAccountName.Text = "";
            cmbCategory.SelectedIndex = -1;
            cmbParentAccount.SelectedIndex = -1;
            txtDescription.Text = "";
        }

        private void BtnAddSubAccount_Click(object sender, EventArgs e)
        {
            // For right-click context menu, show the account dialog
            if (sender is ToolStripMenuItem)
            {
                if (treeAccounts.SelectedNode == null)
                {
                    MessageBox.Show("Please select an account to add a sub-account.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string accountCode = "";
                string accountName = "";
                int categoryIndex = -1;
                int parentAccountIndex = -1;
                string description = "";

                // Pre-fill values based on selected node
                TreeNode selectedNode = treeAccounts.SelectedNode;

                // Auto-generate next account code based on parent
                if (selectedNode.Tag != null)
                {
                    accountCode = selectedNode.Tag.ToString() + "01";
                }
                else if (selectedNode.Text.Contains("."))
                {
                    // This is a category node like "1. Assets"
                    accountCode = selectedNode.Text.Split('.')[0].Trim() + "101";
                }

                // Set category based on parent
                if (selectedNode.Text.Contains("."))
                {
                    string prefix = selectedNode.Text.Split('.')[0].Trim();
                    categoryIndex = GetCategoryIndex(prefix);
                }
                else if (selectedNode.Tag != null)
                {
                    string prefix = selectedNode.Tag.ToString().Substring(0, 1);
                    categoryIndex = GetCategoryIndex(prefix);
                }

                // Set parent in combobox items
                for (int i = 0; i < cmbParentAccount.Items.Count; i++)
                {
                    if (cmbParentAccount.Items[i].ToString().Contains(selectedNode.Text))
                    {
                        parentAccountIndex = i;
                        break;
                    }
                }

                // Show dialog to add sub-account
                if (ShowAccountDialog("Add Sub-Account", ref accountCode, ref accountName,
                    ref categoryIndex, ref parentAccountIndex, ref description))
                {
                    // Validate input
                    if (string.IsNullOrWhiteSpace(accountCode) || string.IsNullOrWhiteSpace(accountName))
                    {
                        MessageBox.Show("Account code and name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Validate account code format (numeric only)
                    if (!accountCode.All(char.IsDigit))
                    {
                        MessageBox.Show("Account code must contain only numbers.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check for duplicate account code
                    if (IsAccountCodeDuplicate(accountCode, null))
                    {
                        MessageBox.Show("Account code must be unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Create new node
                    TreeNode newNode = new TreeNode(accountCode + "   " + accountName);
                    newNode.Tag = accountCode;
                    selectedNode.Nodes.Add(newNode);
                    selectedNode.Expand();

                    // Select the new node
                    treeAccounts.SelectedNode = newNode;

                    // Refresh parent account combobox
                    RefreshParentAccountComboBox();

                    // Update form with new account
                    txtAccountCode.Text = accountCode;
                    txtAccountName.Text = accountName;
                    cmbCategory.SelectedIndex = categoryIndex;

                    // Find the correct parent account in the dropdown
                    for (int i = 0; i < cmbParentAccount.Items.Count; i++)
                    {
                        if (cmbParentAccount.Items[i].ToString().Contains(selectedNode.Text))
                        {
                            cmbParentAccount.SelectedIndex = i;
                            break;
                        }
                    }

                    txtDescription.Text = description;
                }
            }
            else
            {
                // Traditional button click
                if (treeAccounts.SelectedNode == null)
                {
                    MessageBox.Show("Please select an account to add a sub-account.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ClearForm();

                // Auto-generate next account code based on parent
                string parentCode = "";
                if (treeAccounts.SelectedNode.Tag != null)
                {
                    parentCode = treeAccounts.SelectedNode.Tag.ToString();
                }
                else if (treeAccounts.SelectedNode.Text.Contains("."))
                {
                    // This is a category node like "1. Assets"
                    parentCode = treeAccounts.SelectedNode.Text.Split('.')[0].Trim();
                }

                if (!string.IsNullOrEmpty(parentCode))
                {
                    // Generate next code (simple implementation)
                    txtAccountCode.Text = parentCode + "01";
                }

                // Set parent account in dropdown
                for (int i = 0; i < cmbParentAccount.Items.Count; i++)
                {
                    if (cmbParentAccount.Items[i].ToString().Contains(treeAccounts.SelectedNode.Text))
                    {
                        cmbParentAccount.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void BtnAddAccount_Click(object sender, EventArgs e)
        {
            // For right-click context menu, show the account dialog
            if (sender is ToolStripMenuItem)
            {
                string accountCode = "";
                string accountName = "";
                int categoryIndex = -1;
                int parentAccountIndex = -1;
                string description = "";

                // If node is selected, pre-populate with parent category
                if (treeAccounts.SelectedNode != null)
                {
                    string nodeText = treeAccounts.SelectedNode.Text;
                    if (nodeText.Contains("."))
                    {
                        // If it's a root category node, set the category
                        string prefix = nodeText.Split('.')[0].Trim();
                        categoryIndex = GetCategoryIndex(prefix);
                        // Auto-generate a code based on the category
                        accountCode = prefix + "100";
                    }
                }

                // Show dialog to add account
                if (ShowAccountDialog("Add Account", ref accountCode, ref accountName,
                    ref categoryIndex, ref parentAccountIndex, ref description))
                {
                    // Validate input
                    if (string.IsNullOrWhiteSpace(accountCode) || string.IsNullOrWhiteSpace(accountName))
                    {
                        MessageBox.Show("Account code and name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Validate account code format (numeric only)
                    if (!accountCode.All(char.IsDigit))
                    {
                        MessageBox.Show("Account code must contain only numbers.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check for duplicate account code
                    if (IsAccountCodeDuplicate(accountCode, null))
                    {
                        MessageBox.Show("Account code must be unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Find parent node based on selection
                    TreeNode parentNode = null;

                    if (parentAccountIndex >= 0)
                    {
                        // Get parent from combobox selection
                        string parentText = cmbParentAccount.Items[parentAccountIndex].ToString();
                        foreach (TreeNode node in GetAllNodes(treeAccounts.Nodes))
                        {
                            if (node.Text.Contains(parentText) || parentText.Contains(node.Text))
                            {
                                parentNode = node;
                                break;
                            }
                        }
                    }
                    else if (categoryIndex >= 0)
                    {
                        // If no parent specified but category selected, use category root node
                        string catPrefix = (categoryIndex + 1).ToString();
                        foreach (TreeNode node in treeAccounts.Nodes)
                        {
                            if (node.Text.StartsWith(catPrefix + "."))
                            {
                                parentNode = node;
                                break;
                            }
                        }
                    }

                    if (parentNode == null)
                    {
                        MessageBox.Show("Please select a parent account or category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Create new node
                    TreeNode newNode = new TreeNode(accountCode + "   " + accountName);
                    newNode.Tag = accountCode;
                    parentNode.Nodes.Add(newNode);
                    parentNode.Expand();

                    // Select the new node
                    treeAccounts.SelectedNode = newNode;

                    // Refresh parent account combobox
                    RefreshParentAccountComboBox();

                    // Update form with new account
                    txtAccountCode.Text = accountCode;
                    txtAccountName.Text = accountName;
                    cmbCategory.SelectedIndex = categoryIndex;
                    cmbParentAccount.SelectedIndex = parentAccountIndex;
                    txtDescription.Text = description;
                }
            }
            else
            {
                // Traditional button click - just clear the form
                ClearForm();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Check if a node is selected
            if (treeAccounts.SelectedNode == null)
            {
                MessageBox.Show("Please select an account to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Don't allow deletion of root nodes
            if (treeAccounts.SelectedNode.Parent == null)
            {
                MessageBox.Show("Cannot delete main account categories.", "Delete Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirm deletion
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this account?\n" + treeAccounts.SelectedNode.Text,
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Store the parent node for refreshing
                TreeNode parentNode = treeAccounts.SelectedNode.Parent;

                // Remove the node
                treeAccounts.SelectedNode.Remove();

                // Refresh parent account combobox
                RefreshParentAccountComboBox();

                // Clear form
                ClearForm();

                // Select parent node
                if (parentNode != null)
                {
                    treeAccounts.SelectedNode = parentNode;
                }
            }
        }

        // Custom input dialog method for adding accounts via context menu
        private bool ShowAccountDialog(string title, ref string accountCode, ref string accountName,
            ref int categoryIndex, ref int parentAccountIndex, ref string description)
        {
            Form form = new Form();
            Label lblCode = new Label();
            TextBox txtCode = new TextBox();
            Label lblName = new Label();
            TextBox txtName = new TextBox();
            Label lblCat = new Label();
            ComboBox cmbCat = new ComboBox();
            Label lblParent = new Label();
            ComboBox cmbParent = new ComboBox();
            Label lblDesc = new Label();
            TextBox txtDesc = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            // Form setup
            form.Text = title;
            form.BackColor = Color.White;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ClientSize = new Size(400, 450);

            // Account Code
            lblCode.Text = "Account Code:";
            lblCode.AutoSize = true;
            lblCode.Font = new Font("Segoe UI", 10);
            lblCode.Location = new Point(20, 20);

            txtCode.Text = accountCode;
            txtCode.Font = new Font("Segoe UI", 12);
            txtCode.Size = new Size(350, 30);
            txtCode.Location = new Point(20, 45);
            txtCode.MaxLength = 10;

            // Account Name
            lblName.Text = "Account Name:";
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 10);
            lblName.Location = new Point(20, 85);

            txtName.Text = accountName;
            txtName.Font = new Font("Segoe UI", 12);
            txtName.Size = new Size(350, 30);
            txtName.Location = new Point(20, 110);

            // Category
            lblCat.Text = "Category:";
            lblCat.AutoSize = true;
            lblCat.Font = new Font("Segoe UI", 10);
            lblCat.Location = new Point(20, 150);

            cmbCat.Font = new Font("Segoe UI", 12);
            cmbCat.Size = new Size(350, 30);
            cmbCat.Location = new Point(20, 175);
            cmbCat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCat.Items.AddRange(new object[] { "Assets", "Liabilities", "Equity", "Income", "Expenses" });
            if (categoryIndex >= 0 && categoryIndex < cmbCat.Items.Count)
                cmbCat.SelectedIndex = categoryIndex;

            // Parent Account
            lblParent.Text = "Parent Account:";
            lblParent.AutoSize = true;
            lblParent.Font = new Font("Segoe UI", 10);
            lblParent.Location = new Point(20, 215);

            cmbParent.Font = new Font("Segoe UI", 12);
            cmbParent.Size = new Size(350, 30);
            cmbParent.Location = new Point(20, 240);
            cmbParent.DropDownStyle = ComboBoxStyle.DropDownList;

            // Populate parent account items
            foreach (var item in cmbParentAccount.Items)
            {
                cmbParent.Items.Add(item);
            }

            if (parentAccountIndex >= 0 && parentAccountIndex < cmbParent.Items.Count)
                cmbParent.SelectedIndex = parentAccountIndex;

            // Description
            lblDesc.Text = "Description:";
            lblDesc.AutoSize = true;
            lblDesc.Font = new Font("Segoe UI", 10);
            lblDesc.Location = new Point(20, 280);

            txtDesc.Text = description;
            txtDesc.Font = new Font("Segoe UI", 12);
            txtDesc.Multiline = true;
            txtDesc.Size = new Size(350, 80);
            txtDesc.Location = new Point(20, 305);

            // Buttons
            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;
            buttonOk.BackColor = Color.FromArgb(25, 120, 220);
            buttonOk.ForeColor = Color.White;
            buttonOk.FlatStyle = FlatStyle.Flat;
            buttonOk.Size = new Size(120, 35);
            buttonOk.Location = new Point(120, 400);

            buttonCancel.Text = "Cancel";
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.BackColor = Color.LightGray;
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.Size = new Size(120, 35);
            buttonCancel.Location = new Point(250, 400);

            // Add controls to form
            form.Controls.AddRange(new Control[] {
                lblCode, txtCode, lblName, txtName, lblCat, cmbCat,
                lblParent, cmbParent, lblDesc, txtDesc, buttonOk, buttonCancel
            });

            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            // Show dialog and get result
            bool result = form.ShowDialog() == DialogResult.OK;

            if (result)
            {
                accountCode = txtCode.Text;
                accountName = txtName.Text;
                categoryIndex = cmbCat.SelectedIndex;
                parentAccountIndex = cmbParent.SelectedIndex;
                description = txtDesc.Text;
            }

            return result;
        }

        // Check if account code already exists
        private bool IsAccountCodeDuplicate(string accountCode, TreeNode currentNode)
        {
            // Loop through all nodes
            foreach (TreeNode node in GetAllNodes(treeAccounts.Nodes))
            {
                // Skip the current node (if editing)
                if (node == currentNode)
                    continue;

                // Check if code matches
                if (node.Tag != null && node.Tag.ToString() == accountCode)
                    return true;
            }

            return false;
        }

        // Add these drag and drop event handlers

        // Handles the beginning of a drag operation
        private void TreeAccounts_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Only allow dragging non-root nodes
            if (e.Item is TreeNode node && node.Parent != null)
            {
                // Start the drag operation
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        // Called when the drag operation enters the control
        private void TreeAccounts_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        // Handles drag over event - controls visual feedback
        private void TreeAccounts_DragOver(object sender, DragEventArgs e)
        {
            // Get the node under the mouse cursor
            Point targetPoint = treeAccounts.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = treeAccounts.GetNodeAt(targetPoint);

            // Highlight the target node
            treeAccounts.SelectedNode = targetNode;

            // Determine if drop is allowed
            if (CanDropNode(e.Data.GetData(typeof(TreeNode)) as TreeNode, targetNode))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        // Handles the drop operation
        private void TreeAccounts_DragDrop(object sender, DragEventArgs e)
        {
            // Get the source node being dragged
            TreeNode sourceNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;

            // Get the destination node
            Point targetPoint = treeAccounts.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = treeAccounts.GetNodeAt(targetPoint);

            // Check if the drop is allowed
            if (sourceNode != null && targetNode != null && CanDropNode(sourceNode, targetNode))
            {
                // Determine if we're dropping on a node or in a category
                if (IsRootNode(targetNode))
                {
                    // Dropping on a category - adjust the account code
                    string newCode = GetNewAccountCodeForCategory(sourceNode, targetNode);
                    string oldText = sourceNode.Text;
                    string accountName = oldText.Contains("   ") ? oldText.Split(new string[] { "   " }, StringSplitOptions.None)[1] : oldText;

                    // Clone the node
                    TreeNode newNode = new TreeNode(newCode + "   " + accountName);
                    newNode.Tag = newCode;
                    newNode.ImageKey = sourceNode.ImageKey;
                    newNode.SelectedImageKey = sourceNode.SelectedImageKey;
                    newNode.ToolTipText = "Account Code: " + newCode + "\nAccount Name: " + accountName;

                    // Add to target and remove original
                    targetNode.Nodes.Add(newNode);
                    sourceNode.Remove();

                    // Select the new node
                    treeAccounts.SelectedNode = newNode;
                    targetNode.Expand();

                    // Show success message
                    ShowDragDropSuccessMessage(newNode);
                }
                else
                {
                    // Dropping on another node - add to its parent node
                    TreeNode parentNode = targetNode.Parent;

                    if (parentNode != null)
                    {
                        // If the target is not a root node, we want to add it to the same parent
                        string newCode = GetNewAccountCodeForParent(sourceNode, parentNode);
                        string oldText = sourceNode.Text;
                        string accountName = oldText.Contains("   ") ? oldText.Split(new string[] { "   " }, StringSplitOptions.None)[1] : oldText;

                        // Clone the node
                        TreeNode newNode = new TreeNode(newCode + "   " + accountName);
                        newNode.Tag = newCode;
                        newNode.ImageKey = sourceNode.ImageKey;
                        newNode.SelectedImageKey = sourceNode.SelectedImageKey;
                        newNode.ToolTipText = "Account Code: " + newCode + "\nAccount Name: " + accountName;

                        // Add to target's parent and remove original
                        parentNode.Nodes.Add(newNode);
                        sourceNode.Remove();

                        // Select the new node
                        treeAccounts.SelectedNode = newNode;
                        parentNode.Expand();

                        // Show success message
                        ShowDragDropSuccessMessage(newNode);
                    }
                }

                // Refresh the parent account combobox
                RefreshParentAccountComboBox();
            }
        }

        // Determines if the source node can be dropped on the target node
        private bool CanDropNode(TreeNode sourceNode, TreeNode targetNode)
        {
            // No target or source, can't drop
            if (sourceNode == null || targetNode == null)
                return false;

            // Can't drop onto itself
            if (sourceNode == targetNode)
                return false;

            // Can't drop a parent onto its child (would create circular reference)
            if (IsChildNode(targetNode, sourceNode))
                return false;

            // Can't drop root nodes
            if (sourceNode.Parent == null)
                return false;

            // Allow dropping on a category node or a regular node
            return true;
        }

        // Checks if potentialChild is a child/grandchild of parent
        private bool IsChildNode(TreeNode potentialChild, TreeNode parent)
        {
            if (potentialChild.Parent == parent)
                return true;

            if (potentialChild.Parent == null)
                return false;

            return IsChildNode(potentialChild.Parent, parent);
        }

        // Check if a node is a root node (category)
        private bool IsRootNode(TreeNode node)
        {
            return node.Parent == null;
        }

        // Get a new account code when moving to a different category
        private string GetNewAccountCodeForCategory(TreeNode sourceNode, TreeNode categoryNode)
        {
            string categoryPrefix = "1"; // Default to Assets

            // Extract the category number from the category node text (e.g., "1. Assets" -> "1")
            if (categoryNode.Text.Contains("."))
            {
                categoryPrefix = categoryNode.Text.Split('.')[0].Trim();
            }

            // Find the last account code in this category to determine the next one
            string highestCode = categoryPrefix + "000";

            foreach (TreeNode node in categoryNode.Nodes)
            {
                if (node.Tag != null && node.Tag.ToString().StartsWith(categoryPrefix))
                {
                    string nodeCode = node.Tag.ToString();
                    if (string.Compare(nodeCode, highestCode) > 0)
                    {
                        highestCode = nodeCode;
                    }
                }
            }

            // Increment the highest code
            int codeNumber = int.Parse(highestCode);
            codeNumber += 100;

            return codeNumber.ToString();
        }

        // Get a new account code when moving under a parent account
        private string GetNewAccountCodeForParent(TreeNode sourceNode, TreeNode parentNode)
        {
            string parentCode = "";

            // Get the parent code
            if (parentNode.Tag != null)
            {
                parentCode = parentNode.Tag.ToString();
            }
            else if (parentNode.Text.Contains("."))
            {
                // This is a category node like "1. Assets"
                parentCode = parentNode.Text.Split('.')[0].Trim();
            }

            if (string.IsNullOrEmpty(parentCode))
                return sourceNode.Tag?.ToString() ?? "1000";

            // Find the highest account code among siblings
            string highestCode = parentCode + "00";

            foreach (TreeNode node in parentNode.Nodes)
            {
                if (node.Tag != null && node.Tag.ToString().StartsWith(parentCode))
                {
                    string nodeCode = node.Tag.ToString();
                    if (string.Compare(nodeCode, highestCode) > 0)
                    {
                        highestCode = nodeCode;
                    }
                }
            }

            // Increment the highest code
            int baseNumber = int.Parse(parentCode);
            int lastPart = int.Parse(highestCode.Substring(parentCode.Length));
            lastPart += 1;

            // Ensure the suffix is at least 2 digits
            string suffix = lastPart.ToString().PadLeft(2, '0');

            return parentCode + suffix;
        }

        // Show a confirmation message after successful drag and drop
        private void ShowDragDropSuccessMessage(TreeNode node)
        {
            // Create and show the message
            Form messageForm = new Form();
            messageForm.Size = new Size(300, 100);
            messageForm.StartPosition = FormStartPosition.CenterParent;
            messageForm.FormBorderStyle = FormBorderStyle.None;
            messageForm.BackColor = Color.FromArgb(25, 120, 220);
            messageForm.Opacity = 0.9;

            Label label = new Label();
            label.Text = "Account moved successfully";
            label.ForeColor = Color.White;
            label.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;

            messageForm.Controls.Add(label);

            // Show the form for 1.5 seconds
            Timer timer = new Timer();
            timer.Interval = 1500;
            timer.Tick += (s, e) => {
                messageForm.Close();
                timer.Stop();
            };

            messageForm.Show(this);
            timer.Start();
        }

        private void ultraPanel1_PaintClient(object sender, PaintEventArgs e)
        {

        }
    }
    }