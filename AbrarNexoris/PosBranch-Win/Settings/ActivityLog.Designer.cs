namespace PosBranch_Win.Settings
{
    partial class ActivityLog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Infragistics.Win.ValueListItem valueListItem1 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem2 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem3 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem4 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem5 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem6 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem7 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem8 = new Infragistics.Win.ValueListItem();
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.filterPanel = new PosBranch_Win.Settings.RoundedPanel();
            this.filterLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblFilters = new System.Windows.Forms.Label();
            this.lblQuickDate = new System.Windows.Forms.Label();
            this.dateRangeLayout = new System.Windows.Forms.TableLayoutPanel();
            this.dtpFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.dtpTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblUser = new System.Windows.Forms.Label();
            this.cmbUser = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblActivityType = new System.Windows.Forms.Label();
            this.cmbActivityType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblItemSearch = new System.Windows.Forms.Label();
            this.txtItemSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblAction = new System.Windows.Forms.Label();
            this.cmbAction = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.contentLayout = new System.Windows.Forms.TableLayoutPanel();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.summaryPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.cardTotal = new PosBranch_Win.Settings.RoundedPanel();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblTotalCaption = new System.Windows.Forms.Label();
            this.cardToday = new PosBranch_Win.Settings.RoundedPanel();
            this.lblToday = new System.Windows.Forms.Label();
            this.lblTodayCaption = new System.Windows.Forms.Label();
            this.cardWeek = new PosBranch_Win.Settings.RoundedPanel();
            this.lblWeek = new System.Windows.Forms.Label();
            this.lblWeekCaption = new System.Windows.Forms.Label();
            this.cardMonth = new PosBranch_Win.Settings.RoundedPanel();
            this.lblMonth = new System.Windows.Forms.Label();
            this.lblMonthCaption = new System.Windows.Forms.Label();
            this.footerPanel = new System.Windows.Forms.Panel();
            this.lblShowing = new System.Windows.Forms.Label();
            this.btnExport = new System.Windows.Forms.Button();
            this.gridFrame = new PosBranch_Win.Settings.RoundedPanel();
            this.gridActivity = new System.Windows.Forms.DataGridView();
            this.cmbQuickDate = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.lblToDate = new System.Windows.Forms.Label();
            this.rootLayout.SuspendLayout();
            this.filterPanel.SuspendLayout();
            this.filterLayout.SuspendLayout();
            this.dateRangeLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbActivityType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAction)).BeginInit();
            this.contentLayout.SuspendLayout();
            this.titlePanel.SuspendLayout();
            this.summaryPanel.SuspendLayout();
            this.cardTotal.SuspendLayout();
            this.cardToday.SuspendLayout();
            this.cardWeek.SuspendLayout();
            this.cardMonth.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.gridFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridActivity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).BeginInit();
            this.SuspendLayout();
            // 
            // rootLayout
            // 
            this.rootLayout.BackColor = System.Drawing.Color.White;
            this.rootLayout.ColumnCount = 2;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.filterPanel, 0, 0);
            this.rootLayout.Controls.Add(this.contentLayout, 1, 0);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(8);
            this.rootLayout.RowCount = 1;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Size = new System.Drawing.Size(1184, 561);
            this.rootLayout.TabIndex = 0;
            // 
            // filterPanel
            // 
            this.filterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.filterPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.filterPanel.BorderRadius = 8;
            this.filterPanel.Controls.Add(this.filterLayout);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterPanel.Location = new System.Drawing.Point(11, 11);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Padding = new System.Windows.Forms.Padding(12);
            this.filterPanel.Size = new System.Drawing.Size(244, 539);
            this.filterPanel.TabIndex = 0;
            // 
            // filterLayout
            // 
            this.filterLayout.ColumnCount = 1;
            this.filterLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.filterLayout.Controls.Add(this.lblFilters, 0, 0);
            this.filterLayout.Controls.Add(this.lblQuickDate, 0, 1);
            this.filterLayout.Controls.Add(this.dateRangeLayout, 0, 2);
            this.filterLayout.Controls.Add(this.lblUser, 0, 3);
            this.filterLayout.Controls.Add(this.cmbUser, 0, 4);
            this.filterLayout.Controls.Add(this.lblActivityType, 0, 5);
            this.filterLayout.Controls.Add(this.cmbActivityType, 0, 6);
            this.filterLayout.Controls.Add(this.lblItemSearch, 0, 7);
            this.filterLayout.Controls.Add(this.txtItemSearch, 0, 8);
            this.filterLayout.Controls.Add(this.lblAction, 0, 9);
            this.filterLayout.Controls.Add(this.cmbAction, 0, 10);
            this.filterLayout.Controls.Add(this.btnApply, 0, 11);
            this.filterLayout.Controls.Add(this.btnReset, 0, 12);
            this.filterLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterLayout.Location = new System.Drawing.Point(12, 12);
            this.filterLayout.Name = "filterLayout";
            this.filterLayout.RowCount = 13;
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.Size = new System.Drawing.Size(220, 468);
            this.filterLayout.TabIndex = 0;
            // 
            // lblFilters
            // 
            this.lblFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFilters.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblFilters.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblFilters.Location = new System.Drawing.Point(3, 0);
            this.lblFilters.Name = "lblFilters";
            this.lblFilters.Size = new System.Drawing.Size(214, 34);
            this.lblFilters.TabIndex = 0;
            this.lblFilters.Text = "Filters";
            this.lblFilters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblQuickDate
            // 
            this.lblQuickDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblQuickDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblQuickDate.Location = new System.Drawing.Point(3, 42);
            this.lblQuickDate.Name = "lblQuickDate";
            this.lblQuickDate.Size = new System.Drawing.Size(214, 24);
            this.lblQuickDate.TabIndex = 1;
            this.lblQuickDate.Text = "Date Range";
            this.lblQuickDate.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // dateRangeLayout
            // 
            this.dateRangeLayout.ColumnCount = 2;
            this.dateRangeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.dateRangeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.dateRangeLayout.Controls.Add(this.dtpFrom, 0, 0);
            this.dateRangeLayout.Controls.Add(this.dtpTo, 1, 0);
            this.dateRangeLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.dateRangeLayout.Location = new System.Drawing.Point(3, 69);
            this.dateRangeLayout.Name = "dateRangeLayout";
            this.dateRangeLayout.RowCount = 1;
            this.dateRangeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.dateRangeLayout.Size = new System.Drawing.Size(214, 32);
            this.dateRangeLayout.TabIndex = 2;
            // 
            // dtpFrom
            // 
            this.dtpFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpFrom.FormatString = "dd MMM yyyy";
            this.dtpFrom.Location = new System.Drawing.Point(0, 0);
            this.dtpFrom.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.dtpFrom.MaskInput = "{date}";
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(103, 25);
            this.dtpFrom.TabIndex = 0;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.DatePicker_ValueChanged);
            // 
            // dtpTo
            // 
            this.dtpTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpTo.FormatString = "dd MMM yyyy";
            this.dtpTo.Location = new System.Drawing.Point(111, 0);
            this.dtpTo.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.dtpTo.MaskInput = "{date}";
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(103, 25);
            this.dtpTo.TabIndex = 1;
            this.dtpTo.ValueChanged += new System.EventHandler(this.DatePicker_ValueChanged);
            // 
            // lblUser
            // 
            this.lblUser.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblUser.Location = new System.Drawing.Point(3, 116);
            this.lblUser.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(214, 16);
            this.lblUser.TabIndex = 7;
            this.lblUser.Text = "User";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cmbUser
            // 
            this.cmbUser.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbUser.Location = new System.Drawing.Point(3, 135);
            this.cmbUser.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.cmbUser.Name = "cmbUser";
            this.cmbUser.Size = new System.Drawing.Size(214, 25);
            this.cmbUser.TabIndex = 8;
            // 
            // lblActivityType
            // 
            this.lblActivityType.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblActivityType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblActivityType.Location = new System.Drawing.Point(3, 182);
            this.lblActivityType.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblActivityType.Name = "lblActivityType";
            this.lblActivityType.Size = new System.Drawing.Size(214, 16);
            this.lblActivityType.TabIndex = 9;
            this.lblActivityType.Text = "Activity Type";
            this.lblActivityType.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cmbActivityType
            // 
            this.cmbActivityType.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbActivityType.Location = new System.Drawing.Point(3, 201);
            this.cmbActivityType.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.cmbActivityType.Name = "cmbActivityType";
            this.cmbActivityType.Size = new System.Drawing.Size(214, 25);
            this.cmbActivityType.TabIndex = 10;
            // 
            // lblItemSearch
            // 
            this.lblItemSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblItemSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblItemSearch.Location = new System.Drawing.Point(3, 248);
            this.lblItemSearch.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblItemSearch.Name = "lblItemSearch";
            this.lblItemSearch.Size = new System.Drawing.Size(214, 16);
            this.lblItemSearch.TabIndex = 11;
            this.lblItemSearch.Text = "Item / Barcode";
            this.lblItemSearch.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // txtItemSearch
            // 
            this.txtItemSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtItemSearch.Location = new System.Drawing.Point(3, 267);
            this.txtItemSearch.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.txtItemSearch.Name = "txtItemSearch";
            this.txtItemSearch.Size = new System.Drawing.Size(214, 25);
            this.txtItemSearch.TabIndex = 12;
            // 
            // lblAction
            // 
            this.lblAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblAction.Location = new System.Drawing.Point(3, 314);
            this.lblAction.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(214, 16);
            this.lblAction.TabIndex = 13;
            this.lblAction.Text = "Action";
            this.lblAction.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cmbAction
            // 
            this.cmbAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbAction.Location = new System.Drawing.Point(3, 333);
            this.cmbAction.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.cmbAction.Name = "cmbAction";
            this.cmbAction.Size = new System.Drawing.Size(214, 25);
            this.cmbAction.TabIndex = 14;
            // 
            // btnApply
            // 
            this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(119)))), ((int)(((byte)(237)))));
            this.btnApply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApply.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnApply.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(119)))), ((int)(((byte)(237)))));
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnApply.ForeColor = System.Drawing.Color.White;
            this.btnApply.Location = new System.Drawing.Point(3, 382);
            this.btnApply.Margin = new System.Windows.Forms.Padding(3, 10, 3, 8);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(214, 36);
            this.btnApply.TabIndex = 15;
            this.btnApply.Text = "Apply Filters";
            this.btnApply.UseVisualStyleBackColor = false;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.White;
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnReset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnReset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.btnReset.Location = new System.Drawing.Point(3, 429);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(214, 36);
            this.btnReset.TabIndex = 16;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // contentLayout
            // 
            this.contentLayout.BackColor = System.Drawing.Color.White;
            this.contentLayout.ColumnCount = 1;
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.contentLayout.Controls.Add(this.titlePanel, 0, 0);
            this.contentLayout.Controls.Add(this.summaryPanel, 0, 1);
            this.contentLayout.Controls.Add(this.footerPanel, 0, 3);
            this.contentLayout.Controls.Add(this.gridFrame, 0, 2);
            this.contentLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentLayout.Location = new System.Drawing.Point(261, 11);
            this.contentLayout.Name = "contentLayout";
            this.contentLayout.Padding = new System.Windows.Forms.Padding(10, 4, 0, 0);
            this.contentLayout.RowCount = 4;
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.contentLayout.Size = new System.Drawing.Size(912, 539);
            this.contentLayout.TabIndex = 1;
            // 
            // titlePanel
            // 
            this.titlePanel.BackColor = System.Drawing.Color.White;
            this.titlePanel.Controls.Add(this.lblSubtitle);
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titlePanel.Location = new System.Drawing.Point(13, 7);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(896, 52);
            this.titlePanel.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(89)))), ((int)(((byte)(130)))));
            this.lblSubtitle.Location = new System.Drawing.Point(0, 28);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(896, 20);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Track item master saves, updates, deletions, and unit removals.";
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(896, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Activity Log - Item Master";
            // 
            // summaryPanel
            // 
            this.summaryPanel.BackColor = System.Drawing.Color.White;
            this.summaryPanel.Controls.Add(this.cardTotal);
            this.summaryPanel.Controls.Add(this.cardToday);
            this.summaryPanel.Controls.Add(this.cardWeek);
            this.summaryPanel.Controls.Add(this.cardMonth);
            this.summaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summaryPanel.Location = new System.Drawing.Point(13, 65);
            this.summaryPanel.Name = "summaryPanel";
            this.summaryPanel.Size = new System.Drawing.Size(896, 52);
            this.summaryPanel.TabIndex = 1;
            this.summaryPanel.WrapContents = false;
            // 
            // cardTotal
            // 
            this.cardTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardTotal.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.cardTotal.BorderRadius = 8;
            this.cardTotal.Controls.Add(this.lblTotal);
            this.cardTotal.Controls.Add(this.lblTotalCaption);
            this.cardTotal.Location = new System.Drawing.Point(0, 0);
            this.cardTotal.Margin = new System.Windows.Forms.Padding(0, 0, 10, 6);
            this.cardTotal.Name = "cardTotal";
            this.cardTotal.Size = new System.Drawing.Size(132, 50);
            this.cardTotal.TabIndex = 0;
            // 
            // lblTotal
            // 
            this.lblTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTotal.Location = new System.Drawing.Point(0, 21);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.lblTotal.Size = new System.Drawing.Size(132, 29);
            this.lblTotal.TabIndex = 1;
            this.lblTotal.Text = "0";
            // 
            // lblTotalCaption
            // 
            this.lblTotalCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTotalCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(78)))), ((int)(((byte)(120)))));
            this.lblTotalCaption.Location = new System.Drawing.Point(0, 0);
            this.lblTotalCaption.Name = "lblTotalCaption";
            this.lblTotalCaption.Padding = new System.Windows.Forms.Padding(9, 4, 0, 0);
            this.lblTotalCaption.Size = new System.Drawing.Size(132, 21);
            this.lblTotalCaption.TabIndex = 0;
            this.lblTotalCaption.Text = "Selected Range";
            // 
            // cardToday
            // 
            this.cardToday.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardToday.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.cardToday.BorderRadius = 8;
            this.cardToday.Controls.Add(this.lblToday);
            this.cardToday.Controls.Add(this.lblTodayCaption);
            this.cardToday.Location = new System.Drawing.Point(142, 0);
            this.cardToday.Margin = new System.Windows.Forms.Padding(0, 0, 10, 6);
            this.cardToday.Name = "cardToday";
            this.cardToday.Size = new System.Drawing.Size(132, 50);
            this.cardToday.TabIndex = 1;
            // 
            // lblToday
            // 
            this.lblToday.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblToday.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblToday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblToday.Location = new System.Drawing.Point(0, 21);
            this.lblToday.Name = "lblToday";
            this.lblToday.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.lblToday.Size = new System.Drawing.Size(132, 29);
            this.lblToday.TabIndex = 1;
            this.lblToday.Text = "0";
            // 
            // lblTodayCaption
            // 
            this.lblTodayCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTodayCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(78)))), ((int)(((byte)(120)))));
            this.lblTodayCaption.Location = new System.Drawing.Point(0, 0);
            this.lblTodayCaption.Name = "lblTodayCaption";
            this.lblTodayCaption.Padding = new System.Windows.Forms.Padding(9, 4, 0, 0);
            this.lblTodayCaption.Size = new System.Drawing.Size(132, 21);
            this.lblTodayCaption.TabIndex = 0;
            this.lblTodayCaption.Text = "Today";
            // 
            // cardWeek
            // 
            this.cardWeek.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardWeek.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.cardWeek.BorderRadius = 8;
            this.cardWeek.Controls.Add(this.lblWeek);
            this.cardWeek.Controls.Add(this.lblWeekCaption);
            this.cardWeek.Location = new System.Drawing.Point(284, 0);
            this.cardWeek.Margin = new System.Windows.Forms.Padding(0, 0, 10, 6);
            this.cardWeek.Name = "cardWeek";
            this.cardWeek.Size = new System.Drawing.Size(132, 50);
            this.cardWeek.TabIndex = 2;
            // 
            // lblWeek
            // 
            this.lblWeek.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWeek.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblWeek.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblWeek.Location = new System.Drawing.Point(0, 21);
            this.lblWeek.Name = "lblWeek";
            this.lblWeek.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.lblWeek.Size = new System.Drawing.Size(132, 29);
            this.lblWeek.TabIndex = 1;
            this.lblWeek.Text = "0";
            // 
            // lblWeekCaption
            // 
            this.lblWeekCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWeekCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(78)))), ((int)(((byte)(120)))));
            this.lblWeekCaption.Location = new System.Drawing.Point(0, 0);
            this.lblWeekCaption.Name = "lblWeekCaption";
            this.lblWeekCaption.Padding = new System.Windows.Forms.Padding(9, 4, 0, 0);
            this.lblWeekCaption.Size = new System.Drawing.Size(132, 21);
            this.lblWeekCaption.TabIndex = 0;
            this.lblWeekCaption.Text = "This Week";
            // 
            // cardMonth
            // 
            this.cardMonth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardMonth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.cardMonth.BorderRadius = 8;
            this.cardMonth.Controls.Add(this.lblMonth);
            this.cardMonth.Controls.Add(this.lblMonthCaption);
            this.cardMonth.Location = new System.Drawing.Point(426, 0);
            this.cardMonth.Margin = new System.Windows.Forms.Padding(0, 0, 10, 6);
            this.cardMonth.Name = "cardMonth";
            this.cardMonth.Size = new System.Drawing.Size(132, 50);
            this.cardMonth.TabIndex = 3;
            // 
            // lblMonth
            // 
            this.lblMonth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMonth.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblMonth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblMonth.Location = new System.Drawing.Point(0, 21);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.lblMonth.Size = new System.Drawing.Size(132, 29);
            this.lblMonth.TabIndex = 1;
            this.lblMonth.Text = "0";
            // 
            // lblMonthCaption
            // 
            this.lblMonthCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMonthCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(78)))), ((int)(((byte)(120)))));
            this.lblMonthCaption.Location = new System.Drawing.Point(0, 0);
            this.lblMonthCaption.Name = "lblMonthCaption";
            this.lblMonthCaption.Padding = new System.Windows.Forms.Padding(9, 4, 0, 0);
            this.lblMonthCaption.Size = new System.Drawing.Size(132, 21);
            this.lblMonthCaption.TabIndex = 0;
            this.lblMonthCaption.Text = "This Month";
            // 
            // footerPanel
            // 
            this.footerPanel.BackColor = System.Drawing.Color.White;
            this.footerPanel.Controls.Add(this.lblShowing);
            this.footerPanel.Controls.Add(this.btnExport);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.footerPanel.Location = new System.Drawing.Point(13, 500);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(896, 36);
            this.footerPanel.TabIndex = 3;
            // 
            // lblShowing
            // 
            this.lblShowing.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblShowing.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(76)))), ((int)(((byte)(110)))));
            this.lblShowing.Location = new System.Drawing.Point(0, 0);
            this.lblShowing.Name = "lblShowing";
            this.lblShowing.Size = new System.Drawing.Size(420, 36);
            this.lblShowing.TabIndex = 0;
            this.lblShowing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.White;
            this.btnExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnExport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnExport.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.btnExport.Location = new System.Drawing.Point(776, 0);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(120, 36);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // gridFrame
            // 
            this.gridFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.gridFrame.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.gridFrame.BorderRadius = 8;
            this.gridFrame.Controls.Add(this.gridActivity);
            this.gridFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridFrame.Location = new System.Drawing.Point(13, 123);
            this.gridFrame.Name = "gridFrame";
            this.gridFrame.Padding = new System.Windows.Forms.Padding(2);
            this.gridFrame.Size = new System.Drawing.Size(896, 371);
            this.gridFrame.TabIndex = 2;
            // 
            // gridActivity
            // 
            this.gridActivity.AllowUserToAddRows = false;
            this.gridActivity.AllowUserToDeleteRows = false;
            this.gridActivity.AllowUserToResizeRows = false;
            this.gridActivity.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.gridActivity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridActivity.ColumnHeadersHeight = 36;
            this.gridActivity.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridActivity.Location = new System.Drawing.Point(2, 2);
            this.gridActivity.Margin = new System.Windows.Forms.Padding(0);
            this.gridActivity.MultiSelect = false;
            this.gridActivity.Name = "gridActivity";
            this.gridActivity.ReadOnly = true;
            this.gridActivity.RowHeadersVisible = false;
            this.gridActivity.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridActivity.Size = new System.Drawing.Size(892, 367);
            this.gridActivity.TabIndex = 0;
            // 
            // cmbQuickDate
            // 
            this.cmbQuickDate.Dock = System.Windows.Forms.DockStyle.Top;
            valueListItem1.DataValue = "Today";
            valueListItem1.DisplayText = "Today";
            valueListItem2.DataValue = "Yesterday";
            valueListItem2.DisplayText = "Yesterday";
            valueListItem3.DataValue = "This Week";
            valueListItem3.DisplayText = "This Week";
            valueListItem4.DataValue = "This Month";
            valueListItem4.DisplayText = "This Month";
            valueListItem5.DataValue = "Previous Month";
            valueListItem5.DisplayText = "Previous Month";
            valueListItem6.DataValue = "This Year";
            valueListItem6.DisplayText = "This Year";
            valueListItem7.DataValue = "Previous Year";
            valueListItem7.DisplayText = "Previous Year";
            valueListItem8.DataValue = "Custom";
            valueListItem8.DisplayText = "Custom";
            this.cmbQuickDate.Items.AddRange(new Infragistics.Win.ValueListItem[] {
            valueListItem1,
            valueListItem2,
            valueListItem3,
            valueListItem4,
            valueListItem5,
            valueListItem6,
            valueListItem7,
            valueListItem8});
            this.cmbQuickDate.Location = new System.Drawing.Point(3, 57);
            this.cmbQuickDate.Name = "cmbQuickDate";
            this.cmbQuickDate.Size = new System.Drawing.Size(214, 21);
            this.cmbQuickDate.TabIndex = 2;
            this.cmbQuickDate.Visible = false;
            this.cmbQuickDate.ValueChanged += new System.EventHandler(this.cmbQuickDate_SelectedIndexChanged);
            // 
            // lblFromDate
            // 
            this.lblFromDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFromDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblFromDate.Location = new System.Drawing.Point(3, 82);
            this.lblFromDate.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(214, 12);
            this.lblFromDate.TabIndex = 3;
            this.lblFromDate.Text = "From Date";
            this.lblFromDate.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblToDate
            // 
            this.lblToDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblToDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblToDate.Location = new System.Drawing.Point(3, 122);
            this.lblToDate.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(214, 12);
            this.lblToDate.TabIndex = 5;
            this.lblToDate.Text = "To Date";
            this.lblToDate.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ActivityLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1184, 561);
            this.Controls.Add(this.rootLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "ActivityLog";
            this.Text = "Activity Log - Item Master";
            this.Load += new System.EventHandler(this.ActivityLog_Load);
            this.rootLayout.ResumeLayout(false);
            this.filterPanel.ResumeLayout(false);
            this.filterLayout.ResumeLayout(false);
            this.filterLayout.PerformLayout();
            this.dateRangeLayout.ResumeLayout(false);
            this.dateRangeLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbActivityType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAction)).EndInit();
            this.contentLayout.ResumeLayout(false);
            this.titlePanel.ResumeLayout(false);
            this.summaryPanel.ResumeLayout(false);
            this.cardTotal.ResumeLayout(false);
            this.cardToday.ResumeLayout(false);
            this.cardWeek.ResumeLayout(false);
            this.cardMonth.ResumeLayout(false);
            this.footerPanel.ResumeLayout(false);
            this.gridFrame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridActivity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private RoundedPanel filterPanel;
        private System.Windows.Forms.TableLayoutPanel filterLayout;
        private System.Windows.Forms.Label lblFilters;
        private System.Windows.Forms.Label lblQuickDate;
        private System.Windows.Forms.TableLayoutPanel dateRangeLayout;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbQuickDate;
        private System.Windows.Forms.Label lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpFrom;
        private System.Windows.Forms.Label lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpTo;
        private System.Windows.Forms.Label lblUser;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbUser;
        private System.Windows.Forms.Label lblActivityType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbActivityType;
        private System.Windows.Forms.Label lblItemSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtItemSearch;
        private System.Windows.Forms.Label lblAction;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbAction;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.TableLayoutPanel contentLayout;
        private System.Windows.Forms.Panel titlePanel;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel summaryPanel;
        private RoundedPanel cardTotal;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblTotalCaption;
        private RoundedPanel cardToday;
        private System.Windows.Forms.Label lblToday;
        private System.Windows.Forms.Label lblTodayCaption;
        private RoundedPanel cardWeek;
        private System.Windows.Forms.Label lblWeek;
        private System.Windows.Forms.Label lblWeekCaption;
        private RoundedPanel cardMonth;
        private System.Windows.Forms.Label lblMonth;
        private System.Windows.Forms.Label lblMonthCaption;
        private System.Windows.Forms.Panel footerPanel;
        private System.Windows.Forms.Label lblShowing;
        private System.Windows.Forms.Button btnExport;
        private RoundedPanel gridFrame;
        private System.Windows.Forms.DataGridView gridActivity;
    }
}
