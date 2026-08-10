using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository;
using Repository.Accounts;
using ModelClass;
using ModelClass.Accounts;
using ModelClass.Master;
using System.Diagnostics;
using PosBranch_Win.Accounts;

namespace PosBranch_Win.ChartOfAccount
{
    public partial class FrmChartOfAcc : Form
    {
        // Repository instances
        private Dropdowns drop = new Dropdowns();
        private AccountGroupRepository accountGroupRepo;
        private LedgerRepository ledgerRepo;

        // Icons for the tree
        private Image groupIcon;
        private Image ledgerIcon;

        // Controls for the details panel
        private TextBox txtName;
        private TextBox txtDescription;
        private TextBox txtType;
        private TextBox txtParentGroup;
        private TextBox txtLedgerCount;
        private Label txtBalance;
        private Label lblBreadcrumb;
        private Label lblDetailsTitle;
        private TextBox txtSearchBox;
        private Button btnSearch;

        // Context menu for right-click operations
        private ContextMenuStrip contextMenuTree;
        private ToolStripMenuItem menuAddGroup;
        private ToolStripMenuItem menuAddLedger;
        private ToolStripMenuItem menuEdit;
        private ToolStripMenuItem menuDelete;

        public FrmChartOfAcc()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            // Initialize repositories
            accountGroupRepo = new AccountGroupRepository();
            ledgerRepo = new LedgerRepository();

            // Set clean fonts and sizes on standard components
            ultraTree1.Font = new Font("Segoe UI", 9.75F);
            ultraTree1.Override.ItemHeight = 24;
            ultraTree1.Indent = 20;
            ultraTree1.Override.ActiveNodeAppearance.BackColor = Color.FromArgb(239, 246, 255); // Soft blue-50
            ultraTree1.Override.ActiveNodeAppearance.ForeColor = Color.FromArgb(30, 41, 59);    // Slate-800
            ultraTree1.Override.SelectedNodeAppearance.BackColor = Color.FromArgb(219, 234, 254); // Soft blue-100
            ultraTree1.Override.SelectedNodeAppearance.ForeColor = Color.FromArgb(30, 41, 59);

            // Style bottom buttons
            foreach (var btn in new[] { btnRefresh, btnExpandAll, btnCollapseAll })
            {
                btn.Appearance.BackColor = Color.FromArgb(15, 77, 128);
                btn.Appearance.BackColor2 = Color.FromArgb(10, 60, 105);
                btn.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                btn.Appearance.FontData.BoldAsString = "True";
                btn.Appearance.FontData.Name = "Segoe UI";
                btn.Appearance.FontData.SizeInPoints = 9F;
                btn.Appearance.ForeColor = Color.White;
                btn.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
                btn.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            }

            // Register event handlers
            this.Load += FrmChartOfAcc_Load;
            ultraTree1.AfterSelect += UltraTree1_AfterSelect;
            ultraTree1.DoubleClick += UltraTree1_DoubleClick;
            ultraTree1.MouseDown += UltraTree1_MouseDown_ContextMenu;

            // Register button events
            btnRefresh.Click += BtnRefresh_Click;
            btnExpandAll.Click += BtnExpandAll_Click;
            btnCollapseAll.Click += BtnCollapseAll_Click;

            // Load custom modern vector-drawn icons
            try
            {
                groupIcon = CreateFolderIcon();
                ledgerIcon = CreateLedgerIcon();
            }
            catch
            {
                // Fallback to default icons if drawing fails
                groupIcon = SystemIcons.Application.ToBitmap();
                ledgerIcon = SystemIcons.Information.ToBitmap();
            }

            // Initialize detail panel controls
            InitializeDetailPanel();

            // Initialize context menu
            InitializeContextMenu();

            // Enable drag and drop functionality
            EnableDragAndDrop();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5 || keyData == (Keys.Control | Keys.R))
            {
                LoadChartOfAccountsTreeSimple();
                return true;
            }
            if (keyData == Keys.F3 || keyData == (Keys.Control | Keys.F))
            {
                if (txtSearchBox != null)
                {
                    txtSearchBox.Focus();
                    txtSearchBox.SelectAll();
                }
                return true;
            }
            if (keyData == Keys.F8 || keyData == (Keys.Control | Keys.E))
            {
                BtnEdit_Click(null, null);
                return true;
            }
            if (keyData == Keys.F4 || keyData == Keys.Delete)
            {
                MenuDelete_Click(null, null);
                return true;
            }
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void InitializeContextMenu()
        {
            // Create context menu
            contextMenuTree = new ContextMenuStrip();

            // Create menu items
            menuAddGroup = new ToolStripMenuItem("Add Group");
            menuAddLedger = new ToolStripMenuItem("Add Ledger");
            menuEdit = new ToolStripMenuItem("Edit");
            menuDelete = new ToolStripMenuItem("Delete");

            // Add click handlers
            menuAddGroup.Click += MenuAddGroup_Click;
            menuAddLedger.Click += MenuAddLedger_Click;
            menuEdit.Click += MenuEdit_Click;
            menuDelete.Click += MenuDelete_Click;

            // Add items to context menu
            contextMenuTree.Items.Add(menuAddGroup);
            contextMenuTree.Items.Add(menuAddLedger);
            contextMenuTree.Items.Add(new ToolStripSeparator());
            contextMenuTree.Items.Add(menuEdit);
            contextMenuTree.Items.Add(menuDelete);

            // Assign the context menu to the tree
            ultraTree1.ContextMenuStrip = contextMenuTree;
        }

        private void UltraTree1_MouseDown_ContextMenu(object sender, MouseEventArgs e)
        {
            // Handle right-click for context menu
            if (e.Button == MouseButtons.Right)
            {
                // Get the node at the mouse position
                Infragistics.Win.UltraWinTree.UltraTreeNode node =
                    ultraTree1.GetNodeFromPoint(new Point(e.X, e.Y));

                if (node != null)
                {
                    // Select the node
                    ultraTree1.SelectedNodes.Clear();
                    node.Selected = true;

                    // Get the data row
                    DataRow row = node.Tag as DataRow;

                    // Enable/disable menu items based on the node type
                    if (row != null)
                    {
                        bool isGroup = row.Table.Columns.Contains("GroupName");
                        bool isLedger = row.Table.Columns.Contains("LedgerName");
                        bool isRootCategory = (node.Parent == null);

                        // Enable/disable menu items
                        menuAddGroup.Enabled = isGroup || isRootCategory; // Only allow adding groups to groups or root categories
                        menuAddLedger.Enabled = isGroup; // Only allow adding ledgers to groups
                        menuEdit.Enabled = (isGroup || isLedger) && !isRootCategory; // Allow editing groups and ledgers, but not root categories
                        menuDelete.Enabled = (isGroup || isLedger) && !isRootCategory; // Allow deleting groups and ledgers, but not root categories
                    }
                    else
                    {
                        // This is a root category node with no data row
                        menuAddGroup.Enabled = true; // Allow adding groups to categories
                        menuAddLedger.Enabled = false; // Don't allow adding ledgers directly to categories
                        menuEdit.Enabled = false; // Don't allow editing categories
                        menuDelete.Enabled = false; // Don't allow deleting categories
                    }
                }
                else
                {
                    // No node at the click position, disable all menu items
                    menuAddGroup.Enabled = false;
                    menuAddLedger.Enabled = false;
                    menuEdit.Enabled = false;
                    menuDelete.Enabled = false;
                }
            }
        }

        private void MenuAddGroup_Click(object sender, EventArgs e)
        {
            // Handle adding a new group
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];

                // Open the account group form
                FrmAccountGroup frmAccountGroup = new FrmAccountGroup();

                // If a group is selected, we need to pre-select it as the parent
                if (selectedNode.Tag is DataRow row && row.Table.Columns.Contains("GroupName"))
                {
                    int groupId = Convert.ToInt32(row["GroupID"]);
                    MessageBox.Show($"Creating a new group under parent group with ID: {groupId}\nPlease select this as the parent group in the next form.",
                        "Create Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Root category selected, show general message
                    MessageBox.Show("Creating a new account group. Please fill in the details in the next form.",
                        "Create Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                frmAccountGroup.ShowDialog();

                // Refresh the tree after adding
                LoadChartOfAccountsTreeSimple();
            }
        }

        private void MenuAddLedger_Click(object sender, EventArgs e)
        {
            // Handle adding a new ledger
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];

                if (selectedNode.Tag is DataRow row && row.Table.Columns.Contains("GroupName"))
                {
                    int groupId = Convert.ToInt32(row["GroupID"]);

                    // Open the ledger form
                    FrmLedgers frmLedgers = new FrmLedgers();

                    MessageBox.Show($"Creating a new ledger under group with ID: {groupId}\nPlease select this as the account group in the next form.",
                        "Create Ledger", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    frmLedgers.ShowDialog();

                    // Refresh the tree after adding
                    LoadChartOfAccountsTreeSimple();
                }
            }
        }

        private void MenuEdit_Click(object sender, EventArgs e)
        {
            // Handle editing the selected item
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Check if it's a group or ledger
                        if (row.Table.Columns.Contains("GroupName"))
                        {
                            // This is an account group - open the group form
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            OpenAccountGroupForm(groupId);
                        }
                        else if (row.Table.Columns.Contains("LedgerName"))
                        {
                            // This is a ledger - open the ledger form
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);
                            OpenLedgerForm(ledgerId);
                        }
                    }
                }
            }
        }

        private void MenuDelete_Click(object sender, EventArgs e)
        {
            // Handle deleting the selected item
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Check if it's a group or ledger
                        if (row.Table.Columns.Contains("GroupName"))
                        {
                            // This is an account group - confirm and delete
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            string groupName = row["GroupName"].ToString();

                            DialogResult result = MessageBox.Show($"Are you sure you want to delete the account group '{groupName}'?\nThis will also delete any ledgers associated with this group.",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                            if (result == DialogResult.Yes)
                            {
                                // Check if the group has child nodes
                                if (selectedNode.Nodes.Count > 0)
                                {
                                    MessageBox.Show("Cannot delete this group because it contains child groups or ledgers.\nPlease delete or move them first.",
                                        "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                // Delete the group
                                bool success = accountGroupRepo.DeleteAccountGroup(groupId);

                                if (success)
                                {
                                    MessageBox.Show($"Account group '{groupName}' deleted successfully.",
                                        "Delete Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // Refresh the tree after deleting
                                    LoadChartOfAccountsTreeSimple();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete account group. It may be in use by other records.",
                                        "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else if (row.Table.Columns.Contains("LedgerName"))
                        {
                            // This is a ledger - confirm and delete
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);
                            string ledgerName = row["LedgerName"].ToString();

                            DialogResult result = MessageBox.Show($"Are you sure you want to delete the ledger '{ledgerName}'?",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                            if (result == DialogResult.Yes)
                            {
                                // Delete the ledger
                                bool success = ledgerRepo.DeleteLedger(ledgerId);

                                if (success)
                                {
                                    MessageBox.Show($"Ledger '{ledgerName}' deleted successfully.",
                                        "Delete Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // Refresh the tree after deleting
                                    LoadChartOfAccountsTreeSimple();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete ledger. It may be in use by other records.",
                                        "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
        }

        private Bitmap CreateFolderIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw a modern folder icon in Slate/Amber
                Color folderColor = Color.FromArgb(234, 179, 8); // Amber-500
                Color folderBorderColor = Color.FromArgb(202, 138, 4); // Amber-600
                
                // Folder tab path points
                PointF[] points = {
                    new PointF(1, 13),
                    new PointF(1, 4),
                    new PointF(5, 4),
                    new PointF(7, 6),
                    new PointF(14, 6),
                    new PointF(14, 13)
                };
                
                using (Brush brush = new SolidBrush(folderColor))
                {
                    g.FillPolygon(brush, points);
                }
                using (Pen pen = new Pen(folderBorderColor, 1.2f))
                {
                    g.DrawPolygon(pen, points);
                }
                // Light accent line
                using (Pen pen = new Pen(Color.FromArgb(254, 240, 138), 1f))
                {
                    g.DrawLine(pen, 2, 7, 13, 7);
                }
            }
            return bmp;
        }

        private Bitmap CreateLedgerIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw a modern ledger sheet/document icon (Teal-blue color)
                Color docColor = Color.FromArgb(248, 250, 252); // White/Slate-50
                Color docBorder = Color.FromArgb(14, 116, 144); // Teal-700
                
                Rectangle rect = new Rectangle(2, 1, 11, 13);
                using (Brush brush = new SolidBrush(docColor))
                {
                    g.FillRectangle(brush, rect);
                }
                using (Pen pen = new Pen(docBorder, 1.2f))
                {
                    g.DrawRectangle(pen, rect);
                }
                
                // Draw internal lines representing transaction entries
                using (Pen pen = new Pen(Color.FromArgb(148, 163, 184), 1f))
                {
                    g.DrawLine(pen, 4, 4, 11, 4);
                    g.DrawLine(pen, 4, 7, 11, 7);
                    g.DrawLine(pen, 4, 10, 9, 10);
                }
            }
            return bmp;
        }

        private void InitializeDetailPanel()
        {
            // Create and add the details panel controls
            ultraPanelDataEntri.ClientArea.Controls.Clear();
            ultraPanelDataEntri.Appearance.BackColor = Color.FromArgb(241, 245, 249); // Slate-100 background

            // Font & Color palette definitions
            Font headerFont = new Font("Segoe UI Semibold", 13, FontStyle.Bold);
            Font cardTitleFont = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
            Font labelFont = new Font("Segoe UI Semibold", 9F);
            Font inputFont = new Font("Segoe UI", 9.5F);
            Font balanceFont = new Font("Segoe UI", 20F, FontStyle.Bold);
            Font breadcrumbFont = new Font("Segoe UI", 9F, FontStyle.Italic);
            Font buttonFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);

            Color headerColor = Color.FromArgb(30, 41, 59);       // Slate-800
            Color labelColor = Color.FromArgb(71, 85, 105);       // Slate-600
            Color inputForeColor = Color.FromArgb(15, 23, 42);    // Slate-900
            Color inputBgColor = Color.FromArgb(248, 250, 252);   // Slate-50
            Color buttonBgColor = Color.FromArgb(15, 77, 128);    // Deep Navy
            Color searchBtnColor = Color.FromArgb(15, 77, 128);   // Deep Navy

            // CARD 1: Search & Quick Actions Card
            Panel searchCard = new Panel();
            searchCard.Location = new Point(15, 15);
            searchCard.Width = ultraPanelDataEntri.ClientArea.Width - 30;
            searchCard.Height = 55;
            searchCard.BackColor = Color.White;
            searchCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            searchCard.Paint += (s, pe) => {
                using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    pe.Graphics.DrawRectangle(pen, 0, 0, searchCard.Width - 1, searchCard.Height - 1);
                }
            };
            ultraPanelDataEntri.ClientArea.Controls.Add(searchCard);

            Label lblSearch = new Label();
            lblSearch.Location = new Point(15, 20);
            lblSearch.AutoSize = true;
            lblSearch.Font = labelFont;
            lblSearch.ForeColor = labelColor;
            lblSearch.Text = "Search Account:";
            searchCard.Controls.Add(lblSearch);

            txtSearchBox = new TextBox();
            txtSearchBox.Location = new Point(110, 16);
            txtSearchBox.Width = searchCard.Width - 215;
            txtSearchBox.Font = inputFont;
            txtSearchBox.ForeColor = inputForeColor;
            txtSearchBox.BackColor = Color.White;
            txtSearchBox.BorderStyle = BorderStyle.FixedSingle;
            txtSearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearchBox.KeyDown += TxtSearchBox_KeyDown;
            txtSearchBox.TextChanged += (s, ev) =>
            {
                if (searchDebounceTimer == null)
                {
                    searchDebounceTimer = new Timer { Interval = 250 };
                    searchDebounceTimer.Tick += (st, args) =>
                    {
                        searchDebounceTimer.Stop();
                        SearchNodes();
                    };
                }
                searchDebounceTimer.Stop();
                searchDebounceTimer.Start();
            };
            searchCard.Controls.Add(txtSearchBox);

            btnSearch = new Button();
            btnSearch.Location = new Point(searchCard.Width - 95, 13);
            btnSearch.Width = 80;
            btnSearch.Height = 28;
            btnSearch.Text = "Find";
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.BackColor = searchBtnColor;
            btnSearch.ForeColor = Color.White;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Font = buttonFont;
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSearch.Cursor = Cursors.Hand;
            btnSearch.Click += BtnSearch_Click;
            searchCard.Controls.Add(btnSearch);

            // CARD 2: Account Details Card
            Panel detailsCard = new Panel();
            detailsCard.Location = new Point(15, 85);
            detailsCard.Width = ultraPanelDataEntri.ClientArea.Width - 30;
            detailsCard.Height = 260;
            detailsCard.BackColor = Color.White;
            detailsCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsCard.Paint += (s, pe) => {
                using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    pe.Graphics.DrawRectangle(pen, 0, 0, detailsCard.Width - 1, detailsCard.Height - 1);
                }
            };
            ultraPanelDataEntri.ClientArea.Controls.Add(detailsCard);

            lblDetailsTitle = new Label();
            lblDetailsTitle.Location = new Point(15, 15);
            lblDetailsTitle.AutoSize = true;
            lblDetailsTitle.Font = cardTitleFont;
            lblDetailsTitle.ForeColor = Color.FromArgb(30, 66, 114); // Deep Navy Accent
            lblDetailsTitle.Text = "Account Details";
            detailsCard.Controls.Add(lblDetailsTitle);

            // Setup Details Card fields
            // Field 1: Name
            Label lblName = new Label();
            lblName.Location = new Point(15, 48);
            lblName.AutoSize = true;
            lblName.Font = labelFont;
            lblName.ForeColor = labelColor;
            lblName.Text = "Name:";
            detailsCard.Controls.Add(lblName);

            txtName = new TextBox();
            txtName.Location = new Point(110, 45);
            txtName.Width = detailsCard.Width - 125;
            txtName.Font = inputFont;
            txtName.ForeColor = inputForeColor;
            txtName.BackColor = inputBgColor;
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtName.ReadOnly = true;
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsCard.Controls.Add(txtName);

            // Field 2: Type
            Label lblType = new Label();
            lblType.Location = new Point(15, 83);
            lblType.AutoSize = true;
            lblType.Font = labelFont;
            lblType.ForeColor = labelColor;
            lblType.Text = "Type:";
            detailsCard.Controls.Add(lblType);

            txtType = new TextBox();
            txtType.Location = new Point(110, 80);
            txtType.Width = detailsCard.Width - 125;
            txtType.Font = inputFont;
            txtType.ForeColor = inputForeColor;
            txtType.BackColor = inputBgColor;
            txtType.BorderStyle = BorderStyle.FixedSingle;
            txtType.ReadOnly = true;
            txtType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsCard.Controls.Add(txtType);

            // Field 3: Parent Group
            Label lblParentGroup = new Label();
            lblParentGroup.Location = new Point(15, 118);
            lblParentGroup.AutoSize = true;
            lblParentGroup.Font = labelFont;
            lblParentGroup.ForeColor = labelColor;
            lblParentGroup.Text = "Parent:";
            detailsCard.Controls.Add(lblParentGroup);

            txtParentGroup = new TextBox();
            txtParentGroup.Location = new Point(110, 115);
            txtParentGroup.Width = detailsCard.Width - 125;
            txtParentGroup.Font = inputFont;
            txtParentGroup.ForeColor = inputForeColor;
            txtParentGroup.BackColor = inputBgColor;
            txtParentGroup.BorderStyle = BorderStyle.FixedSingle;
            txtParentGroup.ReadOnly = true;
            txtParentGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsCard.Controls.Add(txtParentGroup);

            // Field 4: Ledger Count
            Label lblLedgerCount = new Label();
            lblLedgerCount.Location = new Point(15, 153);
            lblLedgerCount.AutoSize = true;
            lblLedgerCount.Font = labelFont;
            lblLedgerCount.ForeColor = labelColor;
            lblLedgerCount.Text = "Ledgers:";
            detailsCard.Controls.Add(lblLedgerCount);

            txtLedgerCount = new TextBox();
            txtLedgerCount.Location = new Point(110, 150);
            txtLedgerCount.Width = detailsCard.Width - 125;
            txtLedgerCount.Font = inputFont;
            txtLedgerCount.ForeColor = inputForeColor;
            txtLedgerCount.BackColor = inputBgColor;
            txtLedgerCount.BorderStyle = BorderStyle.FixedSingle;
            txtLedgerCount.ReadOnly = true;
            txtLedgerCount.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsCard.Controls.Add(txtLedgerCount);

            // Field 5: Description
            Label lblDescription = new Label();
            lblDescription.Location = new Point(15, 188);
            lblDescription.AutoSize = true;
            lblDescription.Font = labelFont;
            lblDescription.ForeColor = labelColor;
            lblDescription.Text = "Description:";
            detailsCard.Controls.Add(lblDescription);

            txtDescription = new TextBox();
            txtDescription.Location = new Point(110, 185);
            txtDescription.Width = detailsCard.Width - 125;
            txtDescription.Font = inputFont;
            txtDescription.ForeColor = inputForeColor;
            txtDescription.BackColor = inputBgColor;
            txtDescription.BorderStyle = BorderStyle.FixedSingle;
            txtDescription.ReadOnly = true;
            txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsCard.Controls.Add(txtDescription);

            // Edit Button inside Details Card
            Button btnEdit = new Button();
            btnEdit.Location = new Point(detailsCard.Width - 105, 220);
            btnEdit.Width = 90;
            btnEdit.Height = 28;
            btnEdit.Text = "Edit Account";
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.BackColor = buttonBgColor;
            btnEdit.ForeColor = Color.White;
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Font = buttonFont;
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnEdit.Click += BtnEdit_Click;
            detailsCard.Controls.Add(btnEdit);

            // CARD 3: Balance & Statistics Card
            Panel balanceCard = new Panel();
            balanceCard.Location = new Point(15, 360);
            balanceCard.Width = ultraPanelDataEntri.ClientArea.Width - 30;
            balanceCard.Height = 140;
            balanceCard.BackColor = Color.White;
            balanceCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            balanceCard.Paint += (s, pe) => {
                using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    pe.Graphics.DrawRectangle(pen, 0, 0, balanceCard.Width - 1, balanceCard.Height - 1);
                }
            };
            ultraPanelDataEntri.ClientArea.Controls.Add(balanceCard);

            Label lblBalanceTitle = new Label();
            lblBalanceTitle.Location = new Point(15, 12);
            lblBalanceTitle.AutoSize = true;
            lblBalanceTitle.Font = cardTitleFont;
            lblBalanceTitle.ForeColor = Color.FromArgb(30, 66, 114);
            lblBalanceTitle.Text = "Account Balance";
            balanceCard.Controls.Add(lblBalanceTitle);

            txtBalance = new Label();
            txtBalance.Location = new Point(15, 35);
            txtBalance.Width = balanceCard.Width - 30;
            txtBalance.Height = 40;
            txtBalance.Font = balanceFont;
            txtBalance.ForeColor = Color.FromArgb(14, 116, 144); // Teal-700
            txtBalance.TextAlign = ContentAlignment.MiddleLeft;
            txtBalance.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            balanceCard.Controls.Add(txtBalance);

            // Breadcrumb path label
            lblBreadcrumb = new Label();
            lblBreadcrumb.Location = new Point(15, 85);
            lblBreadcrumb.Width = balanceCard.Width - 30;
            lblBreadcrumb.Height = 45;
            lblBreadcrumb.Font = breadcrumbFont;
            lblBreadcrumb.ForeColor = Color.FromArgb(100, 116, 139); // Slate-500
            lblBreadcrumb.TextAlign = ContentAlignment.TopLeft;
            lblBreadcrumb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            balanceCard.Controls.Add(lblBreadcrumb);
        }

        private void FrmChartOfAcc_Load(object sender, EventArgs e)
        {
            try
            {
                // Load the chart of accounts tree
                LoadChartOfAccountsTreeSimple();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading chart of accounts: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChartOfAccountsTreeSimple()
        {
            ultraTree1.BeginUpdate();
            try
            {
                // Clear the tree
                ultraTree1.Nodes.Clear();

                // Set up the image list for the tree
                if (ultraTree1.ImageList == null)
                {
                    ultraTree1.ImageList = new ImageList();
                    ultraTree1.ImageList.Images.Add(groupIcon); // Index 0 - Group icon
                    ultraTree1.ImageList.Images.Add(ledgerIcon); // Index 1 - Ledger icon
                }

                // 1. Create root nodes for main account categories
                var categories = new Dictionary<string, Infragistics.Win.UltraWinTree.UltraTreeNode>();
                // Assuming you have a way to get these categories, e.g., from a dedicated table or enum
                // For now, let's hardcode them based on typical accounting structures.
                string[] mainCategories = { "ASSETS", "LIABILITIES", "INCOME", "EXPENSES" };

                foreach (string catName in mainCategories)
                {
                    Infragistics.Win.UltraWinTree.UltraTreeNode categoryNode = new Infragistics.Win.UltraWinTree.UltraTreeNode();
                    categoryNode.Text = catName.ToUpper(); // Ensure consistent casing
                    categoryNode.Tag = catName; // Store category type for later reference if needed
                    categoryNode.Override.NodeAppearance.Image = groupIcon; // Use group icon or a specific category icon
                    categoryNode.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    categoryNode.Override.NodeAppearance.ForeColor = Color.FromArgb(15, 23, 42); // Slate-900
                    categories[catName.ToUpper()] = categoryNode;
                    ultraTree1.Nodes.Add(categoryNode);
                }

                int companyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
                int branchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
                int finYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

                if (companyId <= 0) companyId = 1;
                if (branchId <= 0) branchId = 1;
                if (finYearId <= 0) finYearId = 1;

                // Load only current branch account groups.
                var accountGroupsTable = accountGroupRepo.GetAllAccountGroups(branchId);

                if (accountGroupsTable == null || accountGroupsTable.Rows.Count == 0)
                {
                    MessageBox.Show("No account groups found.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Still show categories even if no groups
                    ultraTree1.ExpandAll();
                    return;
                }

                var groupNodes = new Dictionary<int, Infragistics.Win.UltraWinTree.UltraTreeNode>();

                // First pass: Create all group nodes (without adding to tree yet, just to dictionary)
                foreach (DataRow row in accountGroupsTable.Rows)
                {
                    int groupId = Convert.ToInt32(row["GroupID"]);
                    string groupName = row["GroupName"].ToString();

                    Infragistics.Win.UltraWinTree.UltraTreeNode node = new Infragistics.Win.UltraWinTree.UltraTreeNode();
                    node.Text = groupName;
                    node.Tag = row;
                    node.Override.NodeAppearance.Image = groupIcon;
                    node.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.False; // Sub-groups not necessarily bold initially
                    node.Override.NodeAppearance.ForeColor = Color.FromArgb(71, 85, 105); // Slate-600
                    groupNodes[groupId] = node;
                }

                // Second pass: Build the tree structure under categories and parent groups
                foreach (DataRow row in accountGroupsTable.Rows)
                {
                    int groupId = Convert.ToInt32(row["GroupID"]);
                    int parentGroupId = row["ParentGroupId"] != DBNull.Value ? Convert.ToInt32(row["ParentGroupId"]) : 0;
                    string groupCategoryKey = ResolveAccountCategory(row);

                    Infragistics.Win.UltraWinTree.UltraTreeNode node = groupNodes[groupId];

                    if (parentGroupId > 0 && groupNodes.ContainsKey(parentGroupId))
                    {
                        // This is a child of another group
                        groupNodes[parentGroupId].Nodes.Add(node);
                    }
                    else if (categories.ContainsKey(groupCategoryKey))
                    {
                        // This is a top-level group under a main category
                        categories[groupCategoryKey].Nodes.Add(node);
                        node.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True; // Make top-level groups bold
                    }
                    else
                    {
                        // Fallback for groups that don't match a main category and have no parent
                        if (!categories.ContainsKey("UNCATEGORIZED"))
                        {
                            Infragistics.Win.UltraWinTree.UltraTreeNode uncategorizedNode = new Infragistics.Win.UltraWinTree.UltraTreeNode();
                            uncategorizedNode.Text = "UNCATEGORIZED";
                            uncategorizedNode.Tag = "UNCATEGORIZED_TAG";
                            uncategorizedNode.Override.NodeAppearance.Image = groupIcon;
                            uncategorizedNode.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                            categories["UNCATEGORIZED"] = uncategorizedNode;
                            ultraTree1.Nodes.Add(uncategorizedNode);
                        }
                        categories["UNCATEGORIZED"].Nodes.Add(node);
                    }
                }

                // Third pass: Add ledgers under their groups
                var ledgersTable = ledgerRepo.GetAllLedgers(branchId);
                ApplyDynamicLedgerBalances(ledgersTable, companyId, branchId, finYearId);
                if (ledgersTable != null)
                {
                    foreach (int groupIdInDict in groupNodes.Keys) // Iterate over groups already placed in the tree
                    {
                        Infragistics.Win.UltraWinTree.UltraTreeNode groupNode = groupNodes[groupIdInDict];
                        DataRow groupDataRow = (DataRow)groupNode.Tag;
                        int actualGroupId = Convert.ToInt32(groupDataRow["GroupID"]); // Get GroupID from the DataRow tag

                        var groupLedgers = ledgersTable.AsEnumerable()
                            .Where(r => r["GroupID"] != DBNull.Value && Convert.ToInt32(r["GroupID"]) == actualGroupId);

                        foreach (var ledgerRow in groupLedgers)
                        {
                            string ledgerName = ledgerRow["LedgerName"].ToString();

                            Infragistics.Win.UltraWinTree.UltraTreeNode ledgerNode = new Infragistics.Win.UltraWinTree.UltraTreeNode();
                            ledgerNode.Text = ledgerName;
                            ledgerNode.Tag = ledgerRow;
                            ledgerNode.Override.NodeAppearance.Image = ledgerIcon;
                            ledgerNode.Override.NodeAppearance.ForeColor = Color.FromArgb(14, 116, 144); // Teal-700

                            groupNode.Nodes.Add(ledgerNode);
                        }
                    }
                }

                // Fourth pass: Format rolled-up balances recursively for the entire tree
                foreach (Infragistics.Win.UltraWinTree.UltraTreeNode rootNode in ultraTree1.Nodes)
                {
                    FormatNodeBalancesRecursive(rootNode);
                }

                ultraTree1.ExpandAll();

                // Select the first node by default so details & balance panel populates
                if (ultraTree1.Nodes.Count > 0)
                {
                    ultraTree1.Nodes[0].Selected = true;
                    UpdateDetailsPanel(ultraTree1.Nodes[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading chart of accounts: " + ex.Message + "\nStackTrace: " + ex.StackTrace, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ultraTree1.EndUpdate();
            }
        }

        private void ApplyDynamicLedgerBalances(DataTable ledgersTable, int companyId, int branchId, int finYearId)
        {
            if (ledgersTable == null)
            {
                return;
            }

            if (!ledgersTable.Columns.Contains("Balance"))
            {
                ledgersTable.Columns.Add("Balance", typeof(decimal));
            }

            if (companyId <= 0) companyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            if (companyId <= 0) companyId = 1;

            if (branchId <= 0) branchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            if (branchId <= 0) branchId = 1;

            if (finYearId <= 0) finYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);
            if (finYearId <= 0) finYearId = 1;

            // Get closing balances from POS_TrialBalance stored procedure
            DateTime toDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            Dictionary<int, decimal> balances = null;
            try
            {
                balances = ledgerRepo.GetLedgerBalances(companyId, branchId, finYearId, toDate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching ledger balances: {ex.Message}");
            }

            foreach (DataRow row in ledgersTable.Rows)
            {
                if (row["LedgerID"] == DBNull.Value)
                {
                    continue;
                }

                int ledgerId = Convert.ToInt32(row["LedgerID"]);

                // 1st priority: Closing balance from POS_TrialBalance SP
                decimal txnBalance = 0;
                if (balances != null && balances.TryGetValue(ledgerId, out txnBalance) && txnBalance != 0)
                {
                    row["Balance"] = txnBalance;
                    continue;
                }

                // 2nd priority: Existing Balance column from GETALL SP
                decimal existingBal = 0;
                if (row["Balance"] != DBNull.Value)
                {
                    existingBal = Convert.ToDecimal(row["Balance"]);
                }
                if (existingBal != 0)
                {
                    continue;
                }

                // 3rd priority: Opening balance from OpnDebit - OpnCredit
                decimal opnDebit = row.Table.Columns.Contains("OpnDebit") && row["OpnDebit"] != DBNull.Value ? Convert.ToDecimal(row["OpnDebit"]) : 0;
                decimal opnCredit = row.Table.Columns.Contains("OpnCredit") && row["OpnCredit"] != DBNull.Value ? Convert.ToDecimal(row["OpnCredit"]) : 0;
                decimal openingBal = opnDebit - opnCredit;

                if (openingBal != 0)
                {
                    row["Balance"] = openingBal;
                }
            }
        }

        private int GetContextValue(int sessionValue, string legacyValue)
        {
            if (sessionValue > 0)
            {
                return sessionValue;
            }

            int parsedValue;
            return int.TryParse(legacyValue, out parsedValue) ? parsedValue : 0;
        }

        private string ResolveAccountCategory(DataRow row)
        {
            int groupId = row.Table.Columns.Contains("GroupID") && row["GroupID"] != DBNull.Value
                ? Convert.ToInt32(row["GroupID"])
                : 0;

            if (IsAssetGroup(groupId)) return "ASSETS";
            if (IsLiabilityGroup(groupId)) return "LIABILITIES";
            if (IsIncomeGroup(groupId)) return "INCOME";
            if (IsExpenseGroup(groupId)) return "EXPENSES";

            string groupCategoryKey = string.Empty;
            if (row.Table.Columns.Contains("GroupType") && row["GroupType"] != DBNull.Value)
            {
                groupCategoryKey = row["GroupType"].ToString().ToUpper().Trim();
            }
            else if (row.Table.Columns.Contains("AccountCategory") && row["AccountCategory"] != DBNull.Value)
            {
                groupCategoryKey = row["AccountCategory"].ToString().ToUpper().Trim();
            }

            if (groupCategoryKey == "ASSET") return "ASSETS";
            if (groupCategoryKey == "LIABILITY") return "LIABILITIES";
            if (groupCategoryKey == "EXPENSE") return "EXPENSES";
            if (groupCategoryKey == "EQUITY") return "LIABILITIES";

            return string.IsNullOrWhiteSpace(groupCategoryKey) ? "UNCATEGORIZED" : groupCategoryKey;
        }

        private bool IsAssetGroup(int groupId)
        {
            return new[] { 4, 5, 6, 7, 14, 15, 16, 18, 19, 21, 22 }.Contains(groupId);
        }

        private bool IsLiabilityGroup(int groupId)
        {
            return new[] { 1, 2, 3, 17, 20, 23, 24, 25, 26, 27, 28, 29 }.Contains(groupId);
        }

        private bool IsIncomeGroup(int groupId)
        {
            return new[] { 8, 11, 13 }.Contains(groupId);
        }

        private bool IsExpenseGroup(int groupId)
        {
            return new[] { 9, 10, 12 }.Contains(groupId);
        }

        private void UltraTree1_AfterSelect(object sender, EventArgs e)
        {
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null)
                {
                    UpdateDetailsPanel(selectedNode);
                }
            }
        }

        private void UpdateDetailsPanel(Infragistics.Win.UltraWinTree.UltraTreeNode selectedNode)
        {
            try
            {
                // Clear previous values
                txtName.Text = string.Empty;
                txtDescription.Text = string.Empty;
                txtType.Text = string.Empty;
                txtParentGroup.Text = string.Empty;
                txtLedgerCount.Text = "0";
                txtBalance.Text = "$0.00";
                lblBreadcrumb.Text = string.Empty;

                if (selectedNode == null) return;

                // 1. Calculate recursive stats (balance and ledger count)
                decimal totalBalance;
                int ledgerCount;
                CalculateNodeStats(selectedNode, out totalBalance, out ledgerCount);

                // 2. Format breadcrumb path
                lblBreadcrumb.Text = GetNodeBreadcrumbPath(selectedNode);

                string formattedBalance = totalBalance != 0 ? FormatBalanceInBrackets(totalBalance) : "(0.00 Dr)";

                // 3. Determine the type of node selected
                // Case A: Root category node (string tag)
                if (selectedNode.Tag is string catName)
                {
                    lblDetailsTitle.Text = "Category Details";
                    txtType.Text = "Category";
                    txtName.Text = catName;
                    txtParentGroup.Text = "None (Root)";
                    txtLedgerCount.Text = ledgerCount.ToString();
                    txtDescription.Text = $"System account classification for {catName}.";
                    txtBalance.Text = formattedBalance;
                }
                // Case B: Group or Ledger node (DataRow tag)
                else if (selectedNode.Tag is DataRow row)
                {
                    if (row.Table.Columns.Contains("LedgerName"))
                    {
                        lblDetailsTitle.Text = "Ledger Details";
                        txtType.Text = "Ledger";
                        txtName.Text = row["LedgerName"].ToString();
                        txtDescription.Text = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty;
                        txtLedgerCount.Text = "1"; // Ledger is a leaf node

                        // Parent group name
                        if (selectedNode.Parent != null && selectedNode.Parent.Tag is DataRow parentRow && parentRow.Table.Columns.Contains("GroupName"))
                        {
                            txtParentGroup.Text = parentRow["GroupName"].ToString();
                        }
                        else
                        {
                            txtParentGroup.Text = "Unknown Group";
                        }

                        txtBalance.Text = formattedBalance;
                    }
                    else if (row.Table.Columns.Contains("GroupName"))
                    {
                        lblDetailsTitle.Text = "Account Group Details";
                        txtType.Text = "Group";
                        txtName.Text = row["GroupName"].ToString();
                        txtDescription.Text = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty;
                        txtLedgerCount.Text = ledgerCount.ToString();

                        // Get parent group name
                        if (selectedNode.Parent != null)
                        {
                            if (selectedNode.Parent.Tag is DataRow parentRow && parentRow.Table.Columns.Contains("GroupName"))
                            {
                                txtParentGroup.Text = parentRow["GroupName"].ToString();
                            }
                            else if (selectedNode.Parent.Tag is string cat)
                            {
                                txtParentGroup.Text = cat;
                            }
                        }
                        else
                        {
                            txtParentGroup.Text = "None";
                        }

                        txtBalance.Text = formattedBalance;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating details panel: {ex.Message}");
            }
        }

        private void CalculateNodeStats(Infragistics.Win.UltraWinTree.UltraTreeNode node, out decimal totalBalance, out int ledgerCount)
        {
            totalBalance = 0;
            ledgerCount = 0;

            if (node == null) return;

            // If it is a ledger node (leaf)
            if (node.Tag is DataRow row && row.Table.Columns.Contains("LedgerName"))
            {
                if (row.Table.Columns.Contains("Balance") && row["Balance"] != DBNull.Value)
                {
                    totalBalance = Convert.ToDecimal(row["Balance"]);
                }
                else
                {
                    decimal opnDebit = row.Table.Columns.Contains("OpnDebit") && row["OpnDebit"] != DBNull.Value ? Convert.ToDecimal(row["OpnDebit"]) : 0;
                    decimal opnCredit = row.Table.Columns.Contains("OpnCredit") && row["OpnCredit"] != DBNull.Value ? Convert.ToDecimal(row["OpnCredit"]) : 0;
                    totalBalance = opnDebit - opnCredit;
                }
                ledgerCount = 1;
                return;
            }

            // For groups and root categories, accumulate from child nodes recursively
            foreach (Infragistics.Win.UltraWinTree.UltraTreeNode childNode in node.Nodes)
            {
                decimal childBalance;
                int childLedgerCount;
                CalculateNodeStats(childNode, out childBalance, out childLedgerCount);
                totalBalance += childBalance;
                ledgerCount += childLedgerCount;
            }
        }

        private string GetNodeBreadcrumbPath(Infragistics.Win.UltraWinTree.UltraTreeNode node)
        {
            if (node == null) return string.Empty;

            List<string> pathParts = new List<string>();
            var current = node;
            while (current != null)
            {
                if (current.Tag is DataRow row)
                {
                    if (row.Table.Columns.Contains("LedgerName"))
                        pathParts.Insert(0, row["LedgerName"].ToString());
                    else if (row.Table.Columns.Contains("GroupName"))
                        pathParts.Insert(0, row["GroupName"].ToString());
                }
                else if (current.Tag is string catName)
                {
                    pathParts.Insert(0, catName);
                }
                else
                {
                    pathParts.Insert(0, current.Text);
                }
                current = current.Parent;
            }
            return string.Join("  >  ", pathParts);
        }

        private string FormatBalanceInBrackets(decimal balance)
        {
            if (balance == 0) return string.Empty;
            string drCr = balance >= 0 ? "Dr" : "Cr";
            return $"({Math.Abs(balance):N2} {drCr})";
        }

        private decimal FormatNodeBalancesRecursive(Infragistics.Win.UltraWinTree.UltraTreeNode node)
        {
            if (node == null) return 0;

            // If it's a ledger, show its balance directly in the tree and return it for parent totals.
            if (node.Tag is DataRow row && row.Table.Columns.Contains("LedgerName"))
            {
                string ledgerName = row["LedgerName"].ToString();
                decimal ledgerBalance = 0;
                if (row.Table.Columns.Contains("Balance") && row["Balance"] != DBNull.Value)
                {
                    ledgerBalance = Convert.ToDecimal(row["Balance"]);
                }
                else
                {
                    decimal opnDebit = row.Table.Columns.Contains("OpnDebit") && row["OpnDebit"] != DBNull.Value ? Convert.ToDecimal(row["OpnDebit"]) : 0;
                    decimal opnCredit = row.Table.Columns.Contains("OpnCredit") && row["OpnCredit"] != DBNull.Value ? Convert.ToDecimal(row["OpnCredit"]) : 0;
                    ledgerBalance = opnDebit - opnCredit;
                }

                node.Text = ledgerBalance != 0
                    ? $"{ledgerName} {FormatBalanceInBrackets(ledgerBalance)}"
                    : ledgerName;

                return ledgerBalance;
            }

            // Sum child balances
            decimal totalBalance = 0;
            foreach (Infragistics.Win.UltraWinTree.UltraTreeNode childNode in node.Nodes)
            {
                totalBalance += FormatNodeBalancesRecursive(childNode);
            }

            // Set node text
            if (node.Tag is DataRow groupRow && groupRow.Table.Columns.Contains("GroupName"))
            {
                string groupName = groupRow["GroupName"].ToString();
                node.Text = totalBalance != 0
                    ? $"{groupName} {FormatBalanceInBrackets(totalBalance)}"
                    : groupName;
            }
            else if (node.Tag is string catName && catName != "UNCATEGORIZED_TAG")
            {
                node.Text = totalBalance != 0
                    ? $"{catName} {FormatBalanceInBrackets(totalBalance)}"
                    : catName;
            }

            return totalBalance;
        }

        private void UltraTree1_DoubleClick(object sender, EventArgs e)
        {
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Check if it's a group or ledger
                        if (row.Table.Columns.Contains("LedgerName"))
                        {
                            // This is a ledger - open the ledger form
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);
                            OpenLedgerForm(ledgerId);
                        }
                        else if (row.Table.Columns.Contains("GroupName"))
                        {
                            // This is an account group - open the group form
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            OpenAccountGroupForm(groupId);
                        }
                    }
                }
            }
        }

        private void OpenAccountGroupForm(int groupId)
        {
            try
            {
                // Open the account group form
                PosBranch_Win.Accounts.FrmAccountGroup frmAccountGroup = new Accounts.FrmAccountGroup();

                // Show the form first so all controls are initialized
                frmAccountGroup.Show();

                // Load the account group data
                frmAccountGroup.LoadGroupById(groupId);

                // Bring the form to front
                frmAccountGroup.BringToFront();

                // Refresh the tree after editing when the form is closed
                frmAccountGroup.FormClosed += (s, args) => LoadChartOfAccountsTreeSimple();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening account group form: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenLedgerForm(int ledgerId)
        {
            try
            {
                // Open the ledger form
                PosBranch_Win.Accounts.FrmLedgers frmLedgers = new Accounts.FrmLedgers();

                // Show the form first so all controls are initialized
                frmLedgers.Show();

                // Load the ledger data
                frmLedgers.LoadLedgerById(ledgerId);

                // Bring the form to front
                frmLedgers.BringToFront();

                // Refresh the tree after editing when the form is closed
                frmLedgers.FormClosed += (s, args) => LoadChartOfAccountsTreeSimple();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening ledger form: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Button event handlers
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadChartOfAccountsTreeSimple();
        }

        private void BtnExpandAll_Click(object sender, EventArgs e)
        {
            ultraTree1.BeginUpdate();
            try
            {
                ultraTree1.ExpandAll();
            }
            finally
            {
                ultraTree1.EndUpdate();
            }
        }

        private void BtnCollapseAll_Click(object sender, EventArgs e)
        {
            ultraTree1.BeginUpdate();
            try
            {
                ultraTree1.CollapseAll();
            }
            finally
            {
                ultraTree1.EndUpdate();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Check if it's a group or ledger
                        if (row.Table.Columns.Contains("LedgerName"))
                        {
                            // This is a ledger - open the ledger form
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);
                            OpenLedgerForm(ledgerId);
                        }
                        else if (row.Table.Columns.Contains("GroupName"))
                        {
                            // This is an account group - open the group form
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            OpenAccountGroupForm(groupId);
                        }
                    }
                }
            }
        }

        private Rectangle dragBoxFromMouseDown = Rectangle.Empty;
        private Infragistics.Win.UltraWinTree.UltraTreeNode dragNode = null;

        private void EnableDragAndDrop()
        {
            // Enable drag and drop for the tree safely with drag movement threshold
            ultraTree1.AllowDrop = true;
            ultraTree1.MouseDown += UltraTree1_MouseDown;
            ultraTree1.MouseMove += UltraTree1_MouseMove;
            ultraTree1.MouseUp += UltraTree1_MouseUp;
            ultraTree1.DragOver += UltraTree1_DragOver;
            ultraTree1.DragDrop += UltraTree1_DragDrop;
        }

        private void UltraTree1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Infragistics.Win.UltraWinTree.UltraTreeNode node =
                    ultraTree1.GetNodeFromPoint(new Point(e.X, e.Y));

                if (node != null)
                {
                    DataRow row = node.Tag as DataRow;

                    // Only prepare drag for ledger nodes if mouse actually moves
                    if (row != null && row.Table.Columns.Contains("LedgerName"))
                    {
                        Size dragSize = SystemInformation.DragSize;
                        dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                                       e.Y - (dragSize.Height / 2)), dragSize);
                        dragNode = node;
                        return;
                    }
                }
            }
            dragBoxFromMouseDown = Rectangle.Empty;
            dragNode = null;
        }

        private void UltraTree1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown != Rectangle.Empty &&
                    !dragBoxFromMouseDown.Contains(e.X, e.Y) &&
                    dragNode != null)
                {
                    ultraTree1.DoDragDrop(dragNode, DragDropEffects.Move);
                    dragBoxFromMouseDown = Rectangle.Empty;
                    dragNode = null;
                }
            }
        }

        private void UltraTree1_MouseUp(object sender, MouseEventArgs e)
        {
            dragBoxFromMouseDown = Rectangle.Empty;
            dragNode = null;
        }

        private void UltraTree1_DragOver(object sender, DragEventArgs e)
        {
            // Determine if the drop is allowed
            Infragistics.Win.UltraWinTree.UltraTreeNode sourceNode =
                e.Data.GetData(typeof(Infragistics.Win.UltraWinTree.UltraTreeNode)) as Infragistics.Win.UltraWinTree.UltraTreeNode;

            if (sourceNode != null)
            {
                Point clientPoint = ultraTree1.PointToClient(new Point(e.X, e.Y));
                Infragistics.Win.UltraWinTree.UltraTreeNode targetNode =
                    ultraTree1.GetNodeFromPoint(clientPoint);

                if (targetNode != null)
                {
                    DataRow targetRow = targetNode.Tag as DataRow;

                    // Only allow dropping on group nodes
                    if (targetRow != null && targetRow.Table.Columns.Contains("GroupName"))
                    {
                        e.Effect = DragDropEffects.Move;
                        return;
                    }
                }
            }

            e.Effect = DragDropEffects.None;
        }

        private void UltraTree1_DragDrop(object sender, DragEventArgs e)
        {
            // Perform the move operation
            Infragistics.Win.UltraWinTree.UltraTreeNode sourceNode =
                e.Data.GetData(typeof(Infragistics.Win.UltraWinTree.UltraTreeNode)) as Infragistics.Win.UltraWinTree.UltraTreeNode;

            if (sourceNode != null)
            {
                Point clientPoint = ultraTree1.PointToClient(new Point(e.X, e.Y));
                Infragistics.Win.UltraWinTree.UltraTreeNode targetNode =
                    ultraTree1.GetNodeFromPoint(clientPoint);

                if (targetNode != null)
                {
                    DataRow sourceRow = sourceNode.Tag as DataRow;
                    DataRow targetRow = targetNode.Tag as DataRow;

                    if (sourceRow != null && targetRow != null &&
                        sourceRow.Table.Columns.Contains("LedgerName") &&
                        targetRow.Table.Columns.Contains("GroupName"))
                    {
                        // Get the ledger and group IDs
                        int ledgerId = Convert.ToInt32(sourceRow["LedgerID"]);
                        int newGroupId = Convert.ToInt32(targetRow["GroupID"]);

                        // Move the ledger to the new group only if group actually changed
                        MoveLedgerToGroup(ledgerId, newGroupId);
                    }
                }
            }
        }

        private void MoveLedgerToGroup(int ledgerId, int newGroupId)
        {
            try
            {
                // Load ledger details filtered to current branch
                int branchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
                DataTable ledgers = ledgerRepo.GetAllLedgers(branchId);
                DataRow[] matchingLedgers = ledgers.Select($"LedgerID = {ledgerId}");

                if (matchingLedgers.Length > 0)
                {
                    DataRow ledgerRow = matchingLedgers[0];

                    int currentGroupId = Convert.ToInt32(ledgerRow["GroupID"]);
                    if (currentGroupId == newGroupId)
                    {
                        // Already in this group, no need to move
                        return;
                    }

                    // Get CompanyID dynamically from the ledgerRow
                    int companyId = SessionContext.CompanyId;
                    if (ledgerRow.Table.Columns.Contains("CompanyID") && ledgerRow["CompanyID"] != DBNull.Value)
                    {
                        companyId = Convert.ToInt32(ledgerRow["CompanyID"]);
                    }

                    // Create a ledger object to update
                    ModelClass.Accounts.Ledger ledger = new ModelClass.Accounts.Ledger
                    {
                        LedgerID = ledgerId,
                        LedgerName = ledgerRow["LedgerName"].ToString(),
                        Alias = ledgerRow["Alias"] != DBNull.Value ? ledgerRow["Alias"].ToString() : string.Empty,
                        Description = ledgerRow["Description"] != DBNull.Value ? ledgerRow["Description"].ToString() : string.Empty,
                        Notes = ledgerRow["Notes"] != DBNull.Value ? ledgerRow["Notes"].ToString() : string.Empty,
                        GroupID = newGroupId,
                        BranchID = ledgerRow["BranchID"] != DBNull.Value ? Convert.ToInt32(ledgerRow["BranchID"]) : 0,
                        OpnDebit = ledgerRow["OpnDebit"] != DBNull.Value ? Convert.ToDecimal(ledgerRow["OpnDebit"]) : 0,
                        OpnCredit = ledgerRow["OpnCredit"] != DBNull.Value ? Convert.ToDecimal(ledgerRow["OpnCredit"]) : 0,
                        CompanyID = companyId // Use the dynamically retrieved CompanyID
                    };

                    // Update the ledger with the new group ID
                    bool success = ledgerRepo.UpdateLedger(ledger);

                    if (success)
                    {
                        MessageBox.Show($"Ledger '{ledger.LedgerName}' moved successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh the tree view
                        LoadChartOfAccountsTreeSimple();
                    }
                    else
                    {
                        MessageBox.Show("Failed to move ledger. Please try again.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Ledger with ID {ledgerId} not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving ledger: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Timer searchDebounceTimer;

        private void TxtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchNodes();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            SearchNodes();
        }

        private void SearchNodes()
        {
            string searchText = txtSearchBox.Text.ToLower().Trim();

            ultraTree1.BeginUpdate();
            try
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    // Reset highlights and expand all
                    foreach (Infragistics.Win.UltraWinTree.UltraTreeNode node in ultraTree1.Nodes)
                    {
                        ResetNodeStylesRecursive(node);
                    }
                    ultraTree1.SelectedNodes.Clear();
                    ultraTree1.ExpandAll();
                    return;
                }

                ultraTree1.SelectedNodes.Clear();
                ultraTree1.CollapseAll();

                foreach (Infragistics.Win.UltraWinTree.UltraTreeNode node in ultraTree1.Nodes)
                {
                    SearchNodeRecursive(node, searchText);
                }

                if (ultraTree1.SelectedNodes.Count > 0)
                {
                    ultraTree1.SelectedNodes[0].BringIntoView();
                }
            }
            finally
            {
                ultraTree1.EndUpdate();
            }
        }

        private bool SearchNodeRecursive(Infragistics.Win.UltraWinTree.UltraTreeNode node, string searchText)
        {
            bool matchInSelf = node.Text != null && node.Text.ToLower().Contains(searchText);

            if (matchInSelf)
            {
                node.Selected = true;
                node.Override.NodeAppearance.BackColor = Color.FromArgb(254, 240, 138); // Yellow highlight
                node.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            }
            else
            {
                node.Override.NodeAppearance.ResetBackColor();
            }

            bool childMatch = false;
            foreach (Infragistics.Win.UltraWinTree.UltraTreeNode childNode in node.Nodes)
            {
                if (SearchNodeRecursive(childNode, searchText))
                {
                    childMatch = true;
                }
            }

            if (matchInSelf || childMatch)
            {
                node.Expanded = true;
                return true;
            }

            return false;
        }

        private void ResetNodeStylesRecursive(Infragistics.Win.UltraWinTree.UltraTreeNode node)
        {
            node.Override.NodeAppearance.ResetBackColor();
            foreach (Infragistics.Win.UltraWinTree.UltraTreeNode childNode in node.Nodes)
            {
                ResetNodeStylesRecursive(childNode);
            }
        }
    }
}
