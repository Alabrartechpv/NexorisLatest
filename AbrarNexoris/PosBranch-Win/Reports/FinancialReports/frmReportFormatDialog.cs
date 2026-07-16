using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.Misc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AppResources = PosBranch_Win.Properties.Resources;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmReportFormatDialog : Form
    {
        private static readonly Color DialogBackColor = Color.FromArgb(220, 234, 249);
        private static readonly Color PanelBackColor = Color.FromArgb(214, 230, 246);
        private static readonly Color ButtonTopColor = Color.FromArgb(234, 244, 255);
        private static readonly Color ButtonBottomColor = Color.FromArgb(152, 188, 235);
        private static readonly Color ButtonBorderColor = Color.FromArgb(73, 119, 184);
        private static readonly Color GridHeaderColor = Color.FromArgb(155, 197, 242);
        private static readonly Color GridHeaderBorderColor = Color.FromArgb(103, 152, 214);
        private static readonly Color GridSelectedColor = Color.FromArgb(124, 118, 244);
        private static readonly Color GridLineColor = Color.FromArgb(184, 209, 240);
        private static readonly Color StatusTextColor = Color.Red;
        private static readonly Color PanelHoverTopColor = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor = Color.FromArgb(170, 206, 244);
        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        public string SelectedFormatDescription
        {
            get
            {
                UltraGridRow activeRow = gridFormats.ActiveRow;
                if (activeRow == null || !activeRow.IsDataRow)
                    return string.Empty;

                return Convert.ToString(activeRow.Cells["Description"].Value);
            }
        }

        public string SelectedAction { get; private set; }

        public frmReportFormatDialog(string reportCaption, IEnumerable<string> formatDescriptions)
        {
            InitializeComponent();

            Text = string.Format("{0} Report Format", reportCaption);
            lblStatus.Text = "Please select a Report Format to print";

            gridFormats.InitializeLayout += gridFormats_InitializeLayout;
            gridFormats.DoubleClickRow += gridFormats_DoubleClickRow;
            Load += frmReportFormatDialog_Load;

            ApplyVisualStyle();
            LoadFormats(formatDescriptions);
        }

        private void frmReportFormatDialog_Load(object sender, EventArgs e)
        {
            if (gridFormats.Rows.Count > 0)
            {
                gridFormats.Rows[0].Activated = true;
                gridFormats.Rows[0].Selected = true;
            }
        }

        private void ApplyVisualStyle()
        {
            BackColor = DialogBackColor;
            ultraPanelTop.Appearance.BackColor = PanelBackColor;
            ultraPanelBottom.Appearance.BackColor = PanelBackColor;
            ultraPanelGrid.Appearance.BackColor = Color.White;
            ultraPanelGrid.Appearance.BorderColor = GridHeaderBorderColor;
            ultraPanelGrid.BorderStyle = UIElementBorderStyle.Solid;

            lblStatus.Appearance.ForeColor = StatusTextColor;
            lblStatus.Appearance.BackColor = Color.Transparent;
            lblStatus.Appearance.FontData.Name = "Microsoft Sans Serif";
            lblStatus.Appearance.FontData.SizeInPoints = 12f;

            InitializePanelButtons();
        }

        private void InitializePanelButtons()
        {
            RegisterPanelButton(ultraPanel1, "add");
            RegisterPanelButton(ultraPanel3, "remove");
            RegisterPanelButton(ultraPanel2, "edit");
            RegisterPanelButton(ultraPanel6, "preview");
            RegisterPanelButton(ultraPanel5, "print");
            RegisterPanelButton(ultraPanel8, "printer");
            RegisterPanelButton(ultraPanel7, "design");
            RegisterPanelButton(ultraPanel9, "cancel", true);
        }

        private void RegisterPanelButton(UltraPanel panel, string actionName, bool isLarge = false)
        {
            if (panel == null)
                return;

            panel.Tag = actionName;
            panel.Cursor = Cursors.Hand;
            panel.UseAppStyling = false;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;

            ApplyPanelButtonStyle(panel, false, false, isLarge);

            panel.Click += PanelButton_Click;
            panel.MouseEnter += PanelButton_MouseEnter;
            panel.MouseLeave += PanelButton_MouseLeave;
            panel.MouseDown += PanelButton_MouseDown;
            panel.MouseUp += PanelButton_MouseUp;

            panel.ClientArea.Click += PanelButton_Click;
            panel.ClientArea.MouseEnter += PanelButton_MouseEnter;
            panel.ClientArea.MouseLeave += PanelButton_MouseLeave;
            panel.ClientArea.MouseDown += PanelButton_MouseDown;
            panel.ClientArea.MouseUp += PanelButton_MouseUp;

            foreach (Control child in panel.ClientArea.Controls)
            {
                child.Tag = actionName;
                child.Cursor = Cursors.Hand;
                child.Click += PanelButton_Click;
                child.MouseEnter += PanelButton_MouseEnter;
                child.MouseLeave += PanelButton_MouseLeave;
                child.MouseDown += PanelButton_MouseDown;
                child.MouseUp += PanelButton_MouseUp;
            }
        }

        private static void ApplyPanelButtonStyle(UltraPanel panel, bool isHover, bool isPressed, bool isLarge)
        {
            Infragistics.Win.Appearance appearance = (Infragistics.Win.Appearance)panel.Appearance;
            appearance.BackGradientStyle = GradientStyle.Vertical;
            appearance.BorderColor = ButtonBorderColor;

            if (isPressed)
            {
                appearance.BackColor = PanelPressedTopColor;
                appearance.BackColor2 = PanelPressedBottomColor;
            }
            else if (isHover)
            {
                appearance.BackColor = PanelHoverTopColor;
                appearance.BackColor2 = PanelHoverBottomColor;
            }
            else
            {
                appearance.BackColor = ButtonTopColor;
                appearance.BackColor2 = ButtonBottomColor;
            }

            if (isLarge)
            {
                appearance.BorderColor = Color.FromArgb(86, 128, 191);
            }
        }

        private UltraPanel GetActionPanel(object sender)
        {
            Control control = sender as Control;
            while (control != null)
            {
                UltraPanel panel = control as UltraPanel;
                if (panel != null)
                    return panel;

                control = control.Parent;
            }

            return null;
        }

        private void PanelButton_MouseEnter(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                ApplyPanelButtonStyle(panel, true, false, panel == ultraPanel9);
            }
        }

        private void PanelButton_MouseLeave(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false, panel == ultraPanel9);
            }
        }

        private void PanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null && e.Button == MouseButtons.Left)
            {
                ApplyPanelButtonStyle(panel, true, true, panel == ultraPanel9);
            }
        }

        private void PanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false, panel == ultraPanel9);
            }
        }

        private void PanelButton_Click(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel == null || panel.Tag == null)
                return;

            string action = Convert.ToString(panel.Tag);
            SelectedAction = action;

            switch (action)
            {
                case "preview":
                case "print":
                case "printer":
                case "design":
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
                case "cancel":
                    DialogResult = DialogResult.Cancel;
                    Close();
                    break;
            }
        }

        private void LoadFormats(IEnumerable<string> formatDescriptions)
        {
            List<ReportFormatRow> rows = (formatDescriptions ?? Enumerable.Empty<string>())
                .Select((description, index) => new ReportFormatRow
                {
                    No = index + 1,
                    Description = description,
                    Owner = "System"
                })
                .ToList();

            gridFormats.DataSource = rows;
        }

        private void gridFormats_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridLayout layout = e.Layout;
            layout.Reset();
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.None;
            layout.GroupByBox.Hidden = true;
            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 16;
            layout.Override.HeaderClickAction = HeaderClickAction.Select;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSizing = RowSizing.Fixed;
            layout.Override.DefaultRowHeight = 24;
            layout.Override.MinRowHeight = 24;

            layout.Appearance.BackColor = Color.White;
            layout.Override.HeaderAppearance.BackColor = GridHeaderColor;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderColor;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            layout.Override.HeaderAppearance.ForeColor = Color.FromArgb(0, 46, 127);
            layout.Override.HeaderAppearance.BorderColor = GridHeaderBorderColor;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 10F;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAppearance.ForeColor = Color.Black;
            layout.Override.RowAppearance.BorderColor = GridLineColor;
            layout.Override.RowAlternateAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BorderColor = GridLineColor;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedColor;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedColor;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.CellAppearance.BorderColor = GridLineColor;
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 10F;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.ScrollStyle = ScrollStyle.Immediate;

            UltraGridBand band = layout.Bands[0];
            band.ColHeadersVisible = true;
            band.Columns["No"].Header.Caption = "No";
            band.Columns["No"].Width = 48;
            band.Columns["No"].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns["No"].Header.VisiblePosition = 0;

            band.Columns["Description"].Header.Caption = "Description";
            band.Columns["Description"].Width = 520;
            band.Columns["Description"].Header.VisiblePosition = 1;

            band.Columns["Owner"].Header.Caption = "Owner";
            band.Columns["Owner"].Width = 90;
            band.Columns["Owner"].Header.VisiblePosition = 2;

            layout.AutoFitStyle = AutoFitStyle.None;
        }

        

        private void gridFormats_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow)
                return;

            SelectedAction = "preview";
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private sealed class ReportFormatRow
        {
            public int No { get; set; }
            public string Description { get; set; }
            public string Owner { get; set; }
        }
    }
}
