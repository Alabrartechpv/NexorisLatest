namespace PosBranch_Win.Settings
{
    partial class UserActivityLog
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
            Infragistics.Win.ValueListItem valueListItemToday = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItemYesterday = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItemThisWeek = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItemThisMonth = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItemPrevMonth = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItemThisYear = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItemPrevYear = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItemCustom = new Infragistics.Win.ValueListItem();

            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.filterPanel = new PosBranch_Win.Settings.RoundedPanel();
            this.filterLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblFilters = new System.Windows.Forms.Label();
            this.lblQuickDate = new System.Windows.Forms.Label();
            this.cmbQuickDate = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.dateRangeLayout = new System.Windows.Forms.TableLayoutPanel();
            this.dtpFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.dtpTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblUser = new System.Windows.Forms.Label();
            this.cmbUser = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblActivityType = new System.Windows.Forms.Label();
            this.cmbActivityType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
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
            this.lblToDate = new System.Windows.Forms.Label();

            this.rootLayout.SuspendLayout();
            this.filterPanel.SuspendLayout();
            this.filterLayout.SuspendLayout();
            this.dateRangeLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbActivityType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
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

            // rootLayout
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
            this.rootLayout.Size = new System.Drawing.Size(1200, 650);
            this.rootLayout.TabIndex = 0;

            // filterPanel
            this.filterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.filterPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.filterPanel.BorderRadius = 8;
            this.filterPanel.Controls.Add(this.filterLayout);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterPanel.Location = new System.Drawing.Point(11, 11);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Padding = new System.Windows.Forms.Padding(12);
            this.filterPanel.Size = new System.Drawing.Size(244, 628);
            this.filterPanel.TabIndex = 0;

            // filterLayout
            this.filterLayout.ColumnCount = 1;
            this.filterLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.filterLayout.Controls.Add(this.lblFilters, 0, 0);
            this.filterLayout.Controls.Add(this.lblQuickDate, 0, 1);
            this.filterLayout.Controls.Add(this.cmbQuickDate, 0, 2);
            this.filterLayout.Controls.Add(this.lblFromDate, 0, 3);
            this.filterLayout.Controls.Add(this.dateRangeLayout, 0, 4);
            this.filterLayout.Controls.Add(this.lblUser, 0, 5);
            this.filterLayout.Controls.Add(this.cmbUser, 0, 6);
            this.filterLayout.Controls.Add(this.lblActivityType, 0, 7);
            this.filterLayout.Controls.Add(this.cmbActivityType, 0, 8);
            this.filterLayout.Controls.Add(this.lblSearch, 0, 9);
            this.filterLayout.Controls.Add(this.txtSearch, 0, 10);
            this.filterLayout.Controls.Add(this.btnApply, 0, 11);
            this.filterLayout.Controls.Add(this.btnReset, 0, 12);
            this.filterLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterLayout.Location = new System.Drawing.Point(12, 12);
            this.filterLayout.Name = "filterLayout";
            this.filterLayout.RowCount = 13;
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.filterLayout.Size = new System.Drawing.Size(220, 500);
            this.filterLayout.TabIndex = 0;

            // lblFilters
            this.lblFilters.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblFilters.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblFilters.Location = new System.Drawing.Point(3, 0);
            this.lblFilters.Name = "lblFilters";
            this.lblFilters.Size = new System.Drawing.Size(214, 34);
            this.lblFilters.TabIndex = 0;
            this.lblFilters.Text = "Search Filters";
            this.lblFilters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // lblQuickDate
            this.lblQuickDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblQuickDate.Location = new System.Drawing.Point(3, 42);
            this.lblQuickDate.Name = "lblQuickDate";
            this.lblQuickDate.Size = new System.Drawing.Size(214, 20);
            this.lblQuickDate.TabIndex = 1;
            this.lblQuickDate.Text = "Quick Period";
            this.lblQuickDate.TextAlign = System.Drawing.ContentAlignment.BottomLeft;

            // cmbQuickDate
            this.cmbQuickDate.Dock = System.Windows.Forms.DockStyle.Fill;
            valueListItemToday.DataValue = "Today";
            valueListItemToday.DisplayText = "Today";
            valueListItemYesterday.DataValue = "Yesterday";
            valueListItemYesterday.DisplayText = "Yesterday";
            valueListItemThisWeek.DataValue = "This Week";
            valueListItemThisWeek.DisplayText = "This Week";
            valueListItemThisMonth.DataValue = "This Month";
            valueListItemThisMonth.DisplayText = "This Month";
            valueListItemPrevMonth.DataValue = "Previous Month";
            valueListItemPrevMonth.DisplayText = "Previous Month";
            valueListItemThisYear.DataValue = "This Year";
            valueListItemThisYear.DisplayText = "This Year";
            valueListItemPrevYear.DataValue = "Previous Year";
            valueListItemPrevYear.DisplayText = "Previous Year";
            valueListItemCustom.DataValue = "Custom";
            valueListItemCustom.DisplayText = "Custom";
            this.cmbQuickDate.Items.AddRange(new Infragistics.Win.ValueListItem[] {
            valueListItemToday,
            valueListItemYesterday,
            valueListItemThisWeek,
            valueListItemThisMonth,
            valueListItemPrevMonth,
            valueListItemThisYear,
            valueListItemPrevYear,
            valueListItemCustom});
            this.cmbQuickDate.Location = new System.Drawing.Point(3, 69);
            this.cmbQuickDate.Name = "cmbQuickDate";
            this.cmbQuickDate.Size = new System.Drawing.Size(214, 24);
            this.cmbQuickDate.TabIndex = 2;
            this.cmbQuickDate.ValueChanged += new System.EventHandler(this.cmbQuickDate_SelectedIndexChanged);

            // lblFromDate
            this.lblFromDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblFromDate.Location = new System.Drawing.Point(3, 102);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(214, 20);
            this.lblFromDate.TabIndex = 3;
            this.lblFromDate.Text = "Date Range";
            this.lblFromDate.TextAlign = System.Drawing.ContentAlignment.BottomLeft;

            // dateRangeLayout
            this.dateRangeLayout.ColumnCount = 2;
            this.dateRangeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.dateRangeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.dateRangeLayout.Controls.Add(this.dtpFrom, 0, 0);
            this.dateRangeLayout.Controls.Add(this.dtpTo, 1, 0);
            this.dateRangeLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateRangeLayout.Location = new System.Drawing.Point(3, 129);
            this.dateRangeLayout.Name = "dateRangeLayout";
            this.dateRangeLayout.RowCount = 1;
            this.dateRangeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.dateRangeLayout.Size = new System.Drawing.Size(214, 30);
            this.dateRangeLayout.TabIndex = 4;

            // dtpFrom
            this.dtpFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpFrom.Location = new System.Drawing.Point(3, 3);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(101, 24);
            this.dtpFrom.TabIndex = 0;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.DatePicker_ValueChanged);

            // dtpTo
            this.dtpTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpTo.Location = new System.Drawing.Point(110, 3);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(101, 24);
            this.dtpTo.TabIndex = 1;
            this.dtpTo.ValueChanged += new System.EventHandler(this.DatePicker_ValueChanged);

            // lblUser
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblUser.Location = new System.Drawing.Point(3, 162);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(214, 20);
            this.lblUser.TabIndex = 5;
            this.lblUser.Text = "System User";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.BottomLeft;

            // cmbUser
            this.cmbUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbUser.Location = new System.Drawing.Point(3, 189);
            this.cmbUser.Name = "cmbUser";
            this.cmbUser.Size = new System.Drawing.Size(214, 24);
            this.cmbUser.TabIndex = 6;

            // lblActivityType
            this.lblActivityType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblActivityType.Location = new System.Drawing.Point(3, 226);
            this.lblActivityType.Name = "lblActivityType";
            this.lblActivityType.Size = new System.Drawing.Size(214, 20);
            this.lblActivityType.TabIndex = 7;
            this.lblActivityType.Text = "Action / Type";
            this.lblActivityType.TextAlign = System.Drawing.ContentAlignment.BottomLeft;

            // cmbActivityType
            this.cmbActivityType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbActivityType.Location = new System.Drawing.Point(3, 253);
            this.cmbActivityType.Name = "cmbActivityType";
            this.cmbActivityType.Size = new System.Drawing.Size(214, 24);
            this.cmbActivityType.TabIndex = 8;

            // lblSearch
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblSearch.Location = new System.Drawing.Point(3, 290);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(214, 20);
            this.lblSearch.TabIndex = 9;
            this.lblSearch.Text = "Search Text";
            this.lblSearch.TextAlign = System.Drawing.ContentAlignment.BottomLeft;

            // txtSearch
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Location = new System.Drawing.Point(3, 317);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(214, 24);
            this.txtSearch.TabIndex = 10;

            // btnApply
            this.btnApply.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnApply.Location = new System.Drawing.Point(3, 365);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(214, 32);
            this.btnApply.TabIndex = 11;
            this.btnApply.Text = "Apply Filters";
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);

            // btnReset
            this.btnReset.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnReset.Location = new System.Drawing.Point(3, 415);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(214, 32);
            this.btnReset.TabIndex = 12;
            this.btnReset.Text = "Reset Filters";
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            // contentLayout
            this.contentLayout.ColumnCount = 1;
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.contentLayout.Controls.Add(this.titlePanel, 0, 0);
            this.contentLayout.Controls.Add(this.summaryPanel, 0, 1);
            this.contentLayout.Controls.Add(this.gridFrame, 0, 2);
            this.contentLayout.Controls.Add(this.footerPanel, 0, 3);
            this.contentLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentLayout.Location = new System.Drawing.Point(261, 11);
            this.contentLayout.Name = "contentLayout";
            this.contentLayout.RowCount = 4;
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.contentLayout.Size = new System.Drawing.Size(928, 628);
            this.contentLayout.TabIndex = 1;

            // titlePanel
            this.titlePanel.Controls.Add(this.lblSubtitle);
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titlePanel.Location = new System.Drawing.Point(3, 3);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(922, 48);
            this.titlePanel.TabIndex = 0;

            // lblSubtitle
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.Gray;
            this.lblSubtitle.Location = new System.Drawing.Point(6, 26);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(340, 13);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Track user login sessions, logouts, roles and form navigation history.";

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(166, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "User Activity Logging";

            // summaryPanel
            this.summaryPanel.Controls.Add(this.cardTotal);
            this.summaryPanel.Controls.Add(this.cardToday);
            this.summaryPanel.Controls.Add(this.cardWeek);
            this.summaryPanel.Controls.Add(this.cardMonth);
            this.summaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summaryPanel.Location = new System.Drawing.Point(0, 54);
            this.summaryPanel.Margin = new System.Windows.Forms.Padding(0);
            this.summaryPanel.Name = "summaryPanel";
            this.summaryPanel.Size = new System.Drawing.Size(928, 74);
            this.summaryPanel.TabIndex = 1;

            // cardTotal
            this.cardTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.cardTotal.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.cardTotal.BorderRadius = 6;
            this.cardTotal.Controls.Add(this.lblTotal);
            this.cardTotal.Controls.Add(this.lblTotalCaption);
            this.cardTotal.Location = new System.Drawing.Point(3, 3);
            this.cardTotal.Name = "cardTotal";
            this.cardTotal.Size = new System.Drawing.Size(160, 60);
            this.cardTotal.TabIndex = 0;

            // lblTotal
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTotal.Location = new System.Drawing.Point(8, 22);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(144, 30);
            this.lblTotal.TabIndex = 1;
            this.lblTotal.Text = "0";

            // lblTotalCaption
            this.lblTotalCaption.AutoSize = true;
            this.lblTotalCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblTotalCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(140)))), ((int)(((byte)(180)))));
            this.lblTotalCaption.Location = new System.Drawing.Point(8, 6);
            this.lblTotalCaption.Name = "lblTotalCaption";
            this.lblTotalCaption.Size = new System.Drawing.Size(76, 13);
            this.lblTotalCaption.TabIndex = 0;
            this.lblTotalCaption.Text = "Filtered Totals";

            // cardToday
            this.cardToday.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.cardToday.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.cardToday.BorderRadius = 6;
            this.cardToday.Controls.Add(this.lblToday);
            this.cardToday.Controls.Add(this.lblTodayCaption);
            this.cardToday.Location = new System.Drawing.Point(169, 3);
            this.cardToday.Name = "cardToday";
            this.cardToday.Size = new System.Drawing.Size(160, 60);
            this.cardToday.TabIndex = 1;

            // lblToday
            this.lblToday.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblToday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblToday.Location = new System.Drawing.Point(8, 22);
            this.lblToday.Name = "lblToday";
            this.lblToday.Size = new System.Drawing.Size(144, 30);
            this.lblToday.TabIndex = 1;
            this.lblToday.Text = "0";

            // lblTodayCaption
            this.lblTodayCaption.AutoSize = true;
            this.lblTodayCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblTodayCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(140)))), ((int)(((byte)(180)))));
            this.lblTodayCaption.Location = new System.Drawing.Point(8, 6);
            this.lblTodayCaption.Name = "lblTodayCaption";
            this.lblTodayCaption.Size = new System.Drawing.Size(77, 13);
            this.lblTodayCaption.TabIndex = 0;
            this.lblTodayCaption.Text = "Logins Today";

            // cardWeek
            this.cardWeek.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.cardWeek.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.cardWeek.BorderRadius = 6;
            this.cardWeek.Controls.Add(this.lblWeek);
            this.cardWeek.Controls.Add(this.lblWeekCaption);
            this.cardWeek.Location = new System.Drawing.Point(335, 3);
            this.cardWeek.Name = "cardWeek";
            this.cardWeek.Size = new System.Drawing.Size(160, 60);
            this.cardWeek.TabIndex = 2;

            // lblWeek
            this.lblWeek.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblWeek.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblWeek.Location = new System.Drawing.Point(8, 22);
            this.lblWeek.Name = "lblWeek";
            this.lblWeek.Size = new System.Drawing.Size(144, 30);
            this.lblWeek.TabIndex = 1;
            this.lblWeek.Text = "0";

            // lblWeekCaption
            this.lblWeekCaption.AutoSize = true;
            this.lblWeekCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblWeekCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(140)))), ((int)(((byte)(180)))));
            this.lblWeekCaption.Location = new System.Drawing.Point(8, 6);
            this.lblWeekCaption.Name = "lblWeekCaption";
            this.lblWeekCaption.Size = new System.Drawing.Size(86, 13);
            this.lblWeekCaption.TabIndex = 0;
            this.lblWeekCaption.Text = "Logouts Today";

            // cardMonth
            this.cardMonth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.cardMonth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.cardMonth.BorderRadius = 6;
            this.cardMonth.Controls.Add(this.lblMonth);
            this.cardMonth.Controls.Add(this.lblMonthCaption);
            this.cardMonth.Location = new System.Drawing.Point(501, 3);
            this.cardMonth.Name = "cardMonth";
            this.cardMonth.Size = new System.Drawing.Size(160, 60);
            this.cardMonth.TabIndex = 3;

            // lblMonth
            this.lblMonth.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblMonth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblMonth.Location = new System.Drawing.Point(8, 22);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Size = new System.Drawing.Size(144, 30);
            this.lblMonth.TabIndex = 1;
            this.lblMonth.Text = "0";

            // lblMonthCaption
            this.lblMonthCaption.AutoSize = true;
            this.lblMonthCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblMonthCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(140)))), ((int)(((byte)(180)))));
            this.lblMonthCaption.Location = new System.Drawing.Point(8, 6);
            this.lblMonthCaption.Name = "lblMonthCaption";
            this.lblMonthCaption.Size = new System.Drawing.Size(99, 13);
            this.lblMonthCaption.TabIndex = 0;
            this.lblMonthCaption.Text = "Form Entries Today";

            // footerPanel
            this.footerPanel.Controls.Add(this.lblShowing);
            this.footerPanel.Controls.Add(this.btnExport);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.footerPanel.Location = new System.Drawing.Point(3, 589);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(922, 36);
            this.footerPanel.TabIndex = 3;

            // lblShowing
            this.lblShowing.AutoSize = true;
            this.lblShowing.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblShowing.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblShowing.Location = new System.Drawing.Point(3, 10);
            this.lblShowing.Name = "lblShowing";
            this.lblShowing.Size = new System.Drawing.Size(109, 15);
            this.lblShowing.TabIndex = 0;
            this.lblShowing.Text = "Showing 0 record(s)";

            // btnExport
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.BackColor = System.Drawing.Color.White;
            this.btnExport.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.btnExport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(226)))), ((int)(((byte)(250)))));
            this.btnExport.Location = new System.Drawing.Point(819, 3);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(100, 30);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "Export CSV";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);

            // gridFrame
            this.gridFrame.Controls.Add(this.gridActivity);
            this.gridFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridFrame.Location = new System.Drawing.Point(3, 131);
            this.gridFrame.Name = "gridFrame";
            this.gridFrame.Size = new System.Drawing.Size(922, 452);
            this.gridFrame.TabIndex = 2;

            // gridActivity
            this.gridActivity.AllowUserToAddRows = false;
            this.gridActivity.AllowUserToDeleteRows = false;
            this.gridActivity.AllowUserToResizeRows = false;
            this.gridActivity.BackgroundColor = System.Drawing.Color.White;
            this.gridActivity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridActivity.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridActivity.Location = new System.Drawing.Point(0, 0);
            this.gridActivity.MultiSelect = false;
            this.gridActivity.Name = "gridActivity";
            this.gridActivity.ReadOnly = true;
            this.gridActivity.RowHeadersVisible = false;
            this.gridActivity.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridActivity.Size = new System.Drawing.Size(922, 452);
            this.gridActivity.TabIndex = 0;

            // UserActivityLog
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 650);
            this.Controls.Add(this.rootLayout);
            this.Name = "UserActivityLog";
            this.Text = "User Activity Logging";
            this.Load += new System.EventHandler(this.UserActivityLog_Load);

            this.rootLayout.ResumeLayout(false);
            this.filterPanel.ResumeLayout(false);
            this.filterLayout.ResumeLayout(false);
            this.dateRangeLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtpFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbActivityType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            this.contentLayout.ResumeLayout(false);
            this.titlePanel.ResumeLayout(false);
            this.titlePanel.PerformLayout();
            this.summaryPanel.ResumeLayout(false);
            this.cardTotal.ResumeLayout(false);
            this.cardTotal.PerformLayout();
            this.cardToday.ResumeLayout(false);
            this.cardToday.PerformLayout();
            this.cardWeek.ResumeLayout(false);
            this.cardWeek.PerformLayout();
            this.cardMonth.ResumeLayout(false);
            this.cardMonth.PerformLayout();
            this.footerPanel.ResumeLayout(false);
            this.footerPanel.PerformLayout();
            this.gridFrame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridActivity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private PosBranch_Win.Settings.RoundedPanel filterPanel;
        private System.Windows.Forms.TableLayoutPanel filterLayout;
        private System.Windows.Forms.Label lblFilters;
        private System.Windows.Forms.Label lblQuickDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbQuickDate;
        private System.Windows.Forms.Label lblFromDate;
        private System.Windows.Forms.TableLayoutPanel dateRangeLayout;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpTo;
        private System.Windows.Forms.Label lblUser;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbUser;
        private System.Windows.Forms.Label lblActivityType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbActivityType;
        private System.Windows.Forms.Label lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.TableLayoutPanel contentLayout;
        private System.Windows.Forms.Panel titlePanel;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel summaryPanel;
        private PosBranch_Win.Settings.RoundedPanel cardTotal;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblTotalCaption;
        private PosBranch_Win.Settings.RoundedPanel cardToday;
        private System.Windows.Forms.Label lblToday;
        private System.Windows.Forms.Label lblTodayCaption;
        private PosBranch_Win.Settings.RoundedPanel cardWeek;
        private System.Windows.Forms.Label lblWeek;
        private System.Windows.Forms.Label lblWeekCaption;
        private PosBranch_Win.Settings.RoundedPanel cardMonth;
        private System.Windows.Forms.Label lblMonth;
        private System.Windows.Forms.Label lblMonthCaption;
        private System.Windows.Forms.Panel footerPanel;
        private System.Windows.Forms.Label lblShowing;
        private System.Windows.Forms.Button btnExport;
        private PosBranch_Win.Settings.RoundedPanel gridFrame;
        private System.Windows.Forms.DataGridView gridActivity;
        private System.Windows.Forms.Label lblToDate;
    }
}
