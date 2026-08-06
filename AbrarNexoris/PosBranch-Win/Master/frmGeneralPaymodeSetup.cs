using Infragistics.Win;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PosBranch_Win.Master
{
    public partial class frmGeneralPaymodeSetup : Form
    {
        private readonly PaymodeRepository repo = new PaymodeRepository();
        private List<PaymodeModel> paymodeList = new List<PaymodeModel>();
        private PaymodeModel currentModel = new PaymodeModel();
        private int currentIndex = -1;
        private int selectedLedgerId = 0;

        private readonly Color midPearlBlue = Color.FromArgb(198, 222, 248);
        private readonly Color peachCream = Color.FromArgb(254, 234, 212);
        private readonly Color skyBlueOutline = Color.FromArgb(120, 180, 240);
        private readonly Color navy = Color.FromArgb(20, 55, 120);

        public frmGeneralPaymodeSetup()
        {
            InitializeComponent();
            ApplyCustomStyling();
            RegisterEvents();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            LockPearlBlueBackground();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LockPearlBlueBackground();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LockPearlBlueBackground();
        }

        private void LockPearlBlueBackground()
        {
            if (BackColor != midPearlBlue)
            {
                BackColor = midPearlBlue;
            }
            if (panelMainContent != null)
            {
                panelMainContent.BackColor = midPearlBlue;
                EnforceChildControlsBackground(panelMainContent);
            }
            StyleAllUltraPanels();
        }

        private void EnforceChildControlsBackground(Control parent)
        {
            if (parent == null) return;
            foreach (Control c in parent.Controls)
            {
                if (c is Infragistics.Win.Misc.UltraPanel)
                {
                    continue; // Skip UltraPanels (they are styled as gradient action buttons)
                }

                if (c is Label || c is CheckBox || c is GroupBox || (c is Panel && c != panelImageBox))
                {
                    c.BackColor = midPearlBlue;

                    c.BackColorChanged -= Control_BackColorChanged;
                    c.BackColorChanged += Control_BackColorChanged;
                }

                if (c.HasChildren && c != panelImageBox && !(c is Infragistics.Win.Misc.UltraPanel))
                {
                    EnforceChildControlsBackground(c);
                }
            }
        }

        private void Control_BackColorChanged(object sender, EventArgs e)
        {
            if (sender is Control c && c != panelImageBox && c.BackColor != midPearlBlue && !(c is Infragistics.Win.Misc.UltraPanel))
            {
                c.BackColor = midPearlBlue;
            }
        }

        private void frmGeneralPaymodeSetup_Load(object sender, EventArgs e)
        {
            try
            {
                LockPearlBlueBackground();
                PopulateDropdownDefaults();
                LoadAllPaymodes();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Paymode Setup: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyCustomStyling()
        {
            LockPearlBlueBackground();

            panelMainContent.BackColorChanged += (s, e) =>
            {
                if (panelMainContent.BackColor != midPearlBlue)
                {
                    panelMainContent.BackColor = midPearlBlue;
                }
            };

            StyleSmallNavButton(btnSearchLedger);

            // Style Editors with soft peach/cream as in Image 2
            StylePeachEditor(txtPayModeName);
            StylePeachEditor(txtDescription);
            StylePeachCombo(cmbFunctionKey);
            StyleWhiteCombo(cmbType);
            StyleWhiteCombo(cmbCategory);
            StyleWhiteCombo(cmbFileName);

            StyleEditor(txtLedgerName);
            StyleAllUltraPanels();
        }

        private void StyleAllUltraPanels()
        {
            string[] panelNames = { "ultraPanel4", "ultraPanel14", "ultraPanel19", "ultraPanel11", "ultraPanel5", "ultraPanel6", "ultraPanel22" };
            foreach (string panelName in panelNames)
            {
                Control[] found = Controls.Find(panelName, true);
                if (found != null && found.Length > 0 && found[0] is Infragistics.Win.Misc.UltraPanel panel)
                {
                    StyleIconPanel(panel);
                }
            }
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;

            panel.UseAppStyling = false;

            // ReportFormat button theme colors (matching Image 1)
            Color topColor = Color.FromArgb(234, 244, 255);       // #EAF4FF
            Color bottomColor = Color.FromArgb(152, 188, 235);    // #98BCEB
            Color borderColor = Color.FromArgb(73, 119, 184);     // #4977B8
            Color textColor = Color.FromArgb(0, 46, 127);         // #002E7F bold dark blue

            Color hoverTop = Color.FromArgb(245, 250, 255);
            Color hoverBottom = Color.FromArgb(170, 206, 244);

            Color pressedTop = Color.FromArgb(205, 226, 248);
            Color pressedBottom = Color.FromArgb(128, 170, 224);

            panel.Appearance.BackColor = topColor;
            panel.Appearance.BackColor2 = bottomColor;
            panel.Appearance.BackGradientStyle = GradientStyle.Vertical;

            panel.BorderStyle = UIElementBorderStyle.Rounded1;
            panel.Appearance.BorderColor = borderColor;

            Action setHoverState = () =>
            {
                panel.Appearance.BackColor = hoverTop;
                panel.Appearance.BackColor2 = hoverBottom;
            };

            Action setNormalState = () =>
            {
                panel.Appearance.BackColor = topColor;
                panel.Appearance.BackColor2 = bottomColor;
            };

            Action setPressedState = () =>
            {
                panel.Appearance.BackColor = pressedTop;
                panel.Appearance.BackColor2 = pressedBottom;
            };

            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox pic)
                {
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;
                    pic.Cursor = Cursors.Hand;

                    pic.MouseEnter += (s, e) => setHoverState();
                    pic.MouseLeave += (s, e) => setNormalState();
                    pic.MouseDown += (s, e) => setPressedState();
                    pic.MouseUp += (s, e) => setHoverState();
                }
                else if (control is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = textColor;
                    lbl.Font = new Font("Segoe UI", lbl.Font.SizeInPoints > 0 ? lbl.Font.SizeInPoints : 9F, FontStyle.Bold);
                    lbl.Cursor = Cursors.Hand;

                    lbl.MouseEnter += (s, e) => setHoverState();
                    lbl.MouseLeave += (s, e) => setNormalState();
                    lbl.MouseDown += (s, e) => setPressedState();
                    lbl.MouseUp += (s, e) => setHoverState();
                }
            }

            panel.ClientArea.MouseEnter += (s, e) => setHoverState();
            panel.ClientArea.MouseLeave += (s, e) => setNormalState();
            panel.ClientArea.MouseDown += (s, e) => setPressedState();
            panel.ClientArea.MouseUp += (s, e) => setHoverState();

            panel.ClientArea.Cursor = Cursors.Hand;
        }

        private void StyleSmallNavButton(Button btn)
        {
            if (btn == null) return;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btn.ForeColor = navy;
            btn.BackColor = Color.FromArgb(180, 210, 242);
            btn.FlatAppearance.BorderColor = skyBlueOutline;
            btn.FlatAppearance.BorderSize = 1;
        }

        private void StylePeachEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor ed)
        {
            if (ed == null) return;
            ed.UseAppStyling = false;
            ed.UseOsThemes = DefaultableBoolean.False;
            ed.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            ed.BorderStyle = UIElementBorderStyle.Solid;
            ed.Appearance.BackColor = peachCream;
            ed.Appearance.BorderColor = skyBlueOutline;
            ed.Appearance.ForeColor = Color.Black;
            ed.Appearance.FontData.Name = "Segoe UI";
            ed.Appearance.FontData.SizeInPoints = 11F;
        }

        private void StylePeachCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor cmb)
        {
            if (cmb == null) return;
            cmb.UseAppStyling = false;
            cmb.UseOsThemes = DefaultableBoolean.False;
            cmb.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            cmb.BorderStyle = UIElementBorderStyle.Solid;
            cmb.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            cmb.Appearance.BackColor = peachCream;
            cmb.Appearance.BorderColor = skyBlueOutline;
            cmb.Appearance.ForeColor = Color.Black;
            cmb.Appearance.FontData.Name = "Segoe UI";
            cmb.Appearance.FontData.SizeInPoints = 11F;
        }

        private void StyleWhiteCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor cmb)
        {
            if (cmb == null) return;
            cmb.UseAppStyling = false;
            cmb.UseOsThemes = DefaultableBoolean.False;
            cmb.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            cmb.BorderStyle = UIElementBorderStyle.Solid;
            cmb.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            cmb.Appearance.BackColor = Color.White;
            cmb.Appearance.BorderColor = skyBlueOutline;
            cmb.Appearance.ForeColor = Color.Black;
            cmb.Appearance.FontData.Name = "Segoe UI";
            cmb.Appearance.FontData.SizeInPoints = 11F;
        }

        private void StyleEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor ed)
        {
            if (ed == null) return;
            ed.UseAppStyling = false;
            ed.UseOsThemes = DefaultableBoolean.False;
            ed.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            ed.BorderStyle = UIElementBorderStyle.Solid;
            ed.Appearance.BackColor = Color.White;
            ed.Appearance.BorderColor = skyBlueOutline;
            ed.Appearance.ForeColor = Color.Black;
            ed.Appearance.FontData.Name = "Segoe UI";
            ed.Appearance.FontData.SizeInPoints = 11F;
        }

        private void RegisterEvents()
        {
            Load += frmGeneralPaymodeSetup_Load;

            ConnectPanelClickEvents();
            WireUniversalButtons(this);

            btnSearchLedger.Click += btnSearchLedger_Click;

            menuItemAddImage.Click += menuItemAddImage_Click;
            menuItemRemoveImage.Click += menuItemRemoveImage_Click;
            picPaymode.DoubleClick += menuItemAddImage_Click;
            lblNoImage.DoubleClick += menuItemAddImage_Click;
        }

        private void ConnectPanelClickEvents()
        {
            Control[] found19 = Controls.Find("ultraPanel19", true);
            if (found19 != null && found19.Length > 0) ConnectClick(found19[0], btnF11Search_Click);

            Control[] found4 = Controls.Find("ultraPanel4", true);
            if (found4 != null && found4.Length > 0) ConnectClick(found4[0], (s, e) => NavigateTo(0));

            Control[] found14 = Controls.Find("ultraPanel14", true);
            if (found14 != null && found14.Length > 0) ConnectClick(found14[0], (s, e) => NavigateTo(0));

            Control[] found11 = Controls.Find("ultraPanel11", true);
            if (found11 != null && found11.Length > 0) ConnectClick(found11[0], (s, e) => NavigateTo(currentIndex - 1));

            Control[] found5 = Controls.Find("ultraPanel5", true);
            if (found5 != null && found5.Length > 0) ConnectClick(found5[0], (s, e) => NavigateTo(currentIndex + 1));

            Control[] found6 = Controls.Find("ultraPanel6", true);
            if (found6 != null && found6.Length > 0) ConnectClick(found6[0], (s, e) => NavigateTo(paymodeList.Count - 1));

            Control[] found22 = Controls.Find("ultraPanel22", true);
            if (found22 != null && found22.Length > 0) ConnectClick(found22[0], btnSearchLedger_Click);
        }

        private void ConnectClick(Control ctrl, EventHandler handler)
        {
            if (ctrl == null) return;
            ctrl.Click -= handler;
            ctrl.Click += handler;

            if (ctrl is Infragistics.Win.Misc.UltraPanel p)
            {
                p.ClientArea.Click -= handler;
                p.ClientArea.Click += handler;
                foreach (Control c in p.ClientArea.Controls)
                {
                    c.Click -= handler;
                    c.Click += handler;
                }
            }
        }

        private void PopulateDropdownDefaults()
        {
            // Function Key DDL
            cmbFunctionKey.Items.Clear();
            cmbFunctionKey.Items.Add("", "None");
            for (int i = 1; i <= 12; i++)
            {
                cmbFunctionKey.Items.Add($"F{i}", $"F{i}");
            }
            cmbFunctionKey.SelectedIndex = -1;

            // Type DDL
            cmbType.Items.Clear();
            cmbType.Items.Add("Cash", "Cash");
            cmbType.Items.Add("Card", "Card");
            cmbType.Items.Add("Credit", "Credit");
            cmbType.Items.Add("Cheque", "Cheque");
            cmbType.Items.Add("Bank Transfer", "Bank Transfer");
            cmbType.Items.Add("UPI / QR", "UPI / QR");
            cmbType.Items.Add("Gift Voucher", "Gift Voucher");
            cmbType.Items.Add("Others", "Others");
            cmbType.SelectedIndex = -1;

            // Category DDL
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("General", "General");
            cmbCategory.Items.Add("Banking", "Banking");
            cmbCategory.Items.Add("Digital Payment", "Digital Payment");
            cmbCategory.Items.Add("Card Gateway", "Card Gateway");
            cmbCategory.Items.Add("Credit Account", "Credit Account");
            cmbCategory.SelectedIndex = -1;

            // File Name DDL
            cmbFileName.Items.Clear();
            cmbFileName.Items.Add("DefaultPaymode.dll", "DefaultPaymode.dll");
            cmbFileName.Items.Add("CashPaymode.dll", "CashPaymode.dll");
            cmbFileName.Items.Add("CardPaymode.dll", "CardPaymode.dll");
            cmbFileName.Items.Add("BankPaymode.dll", "BankPaymode.dll");
            cmbFileName.Items.Add("CustomPaymode.dll", "CustomPaymode.dll");
            cmbFileName.SelectedIndex = -1;
        }

        private void LoadAllPaymodes()
        {
            try
            {
                paymodeList = repo.GetAllPaymodes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching paymodes list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Clear()
        {
            ClearFields();
        }

        public void ClearFields()
        {
            currentModel = new PaymodeModel();
            currentIndex = -1;
            selectedLedgerId = 0;

            txtPayModeName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtLedgerName.Text = string.Empty;

            ResetComboBlank(cmbFunctionKey);
            ResetComboBlank(cmbType);
            ResetComboBlank(cmbCategory);
            ResetComboBlank(cmbFileName);

            UpdateImageDisplay(null);
            chkRequireReference.Checked = false;
            chkHide.Checked = false;
            chkDontOpenDrawer.Checked = false;

            txtPayModeName.Focus();
        }

        private void ResetComboBlank(Infragistics.Win.UltraWinEditors.UltraComboEditor cmb)
        {
            if (cmb == null) return;
            cmb.SelectedIndex = -1;
            cmb.Value = null;
            cmb.Text = string.Empty;
        }

        private void DisplayPaymode(PaymodeModel model)
        {
            if (model == null) return;
            currentModel = model;

            txtPayModeName.Text = model.PayModeName ?? string.Empty;
            txtDescription.Text = model.Description ?? string.Empty;

            SetComboValue(cmbFunctionKey, model.FunctionKey);
            SetComboValue(cmbType, model.PaymodeType);
            SetComboValue(cmbCategory, model.Category);

            chkRequireReference.Checked = model.RequireFillInReference;
            chkHide.Checked = model.IsHide;
            chkDontOpenDrawer.Checked = model.DontOpenDrawer;

            selectedLedgerId = model.LedgerID;
            txtLedgerName.Text = model.LedgerName ?? string.Empty;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(model.Photo))
                    {
                        UpdateImageDisplay(Image.FromStream(ms), model.FileName);
                    }
                }
                catch
                {
                    UpdateImageDisplay(null);
                }
            }
            else
            {
                UpdateImageDisplay(null);
            }
        }

        private void UpdateImageDisplay(Image img, string fileName = null)
        {
            picPaymode.Image = img;
            if (img != null)
            {
                picPaymode.Visible = true;
                picPaymode.BringToFront();
                lblNoImage.Visible = false;

                cmbFileName.Enabled = true;
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    EnsureComboItem(cmbFileName, fileName);
                }
                else if (string.IsNullOrWhiteSpace(cmbFileName.Text))
                {
                    string defaultName = !string.IsNullOrWhiteSpace(currentModel?.PayModeName)
                        ? $"{currentModel.PayModeName}.png"
                        : "Image.png";
                    EnsureComboItem(cmbFileName, defaultName);
                }
            }
            else
            {
                picPaymode.Visible = false;
                lblNoImage.Visible = true;
                lblNoImage.BringToFront();

                cmbFileName.SelectedIndex = -1;
                cmbFileName.Value = null;
                cmbFileName.Text = string.Empty;
                cmbFileName.Enabled = false;
            }
        }

        private void EnsureComboItem(Infragistics.Win.UltraWinEditors.UltraComboEditor cmb, string itemText)
        {
            if (cmb == null || string.IsNullOrWhiteSpace(itemText)) return;
            foreach (var item in cmb.Items)
            {
                if (string.Equals(item.DisplayText, itemText, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.DataValue?.ToString(), itemText, StringComparison.OrdinalIgnoreCase))
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
            cmb.Items.Add(itemText, itemText);
            cmb.Text = itemText;
        }

        private void SetComboValue(Infragistics.Win.UltraWinEditors.UltraComboEditor cmb, string val)
        {
            if (cmb == null) return;
            if (string.IsNullOrWhiteSpace(val))
            {
                // Leave blank — do not auto-select any item
                cmb.SelectedIndex = -1;
                cmb.Value = null;
                cmb.Text = string.Empty;
                return;
            }

            foreach (var item in cmb.Items)
            {
                if (string.Equals(item.DataValue?.ToString(), val, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.DisplayText, val, StringComparison.OrdinalIgnoreCase))
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }

            cmb.Text = val;
        }

        private void NavigateTo(int index)
        {
            if (paymodeList == null || paymodeList.Count == 0)
            {
                ClearFields();
                return;
            }

            if (index < 0) index = 0;
            if (index >= paymodeList.Count) index = paymodeList.Count - 1;

            currentIndex = index;
            DisplayPaymode(paymodeList[currentIndex]);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S) || keyData == Keys.F2)
            {
                SaveRecord();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.N) || keyData == Keys.F3)
            {
                ClearFields();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void WireUniversalButtons(Control parent)
        {
            if (parent == null) return;
            foreach (Control c in parent.Controls)
            {
                string name = c.Name.ToLower();
                string text = c.Text.ToLower();

                if ((name.Contains("save") || text.Contains("save") || name.Contains("update") || text.Contains("update")) && !(c is Infragistics.Win.Misc.UltraPanel))
                {
                    c.Click -= UniversalSave_Click;
                    c.Click += UniversalSave_Click;
                }
                else if ((name.Contains("clear") || text.Contains("clear") || name.Contains("new") || text.Contains("reset")) && !(c is Infragistics.Win.Misc.UltraPanel))
                {
                    c.Click -= UniversalClear_Click;
                    c.Click += UniversalClear_Click;
                }

                if (c.HasChildren) WireUniversalButtons(c);
            }
        }

        private void UniversalSave_Click(object sender, EventArgs e)
        {
            SaveRecord();
        }

        private void UniversalClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        public void Save()
        {
            SaveRecord();
        }

        public void SaveRecord()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPayModeName.Text))
                {
                    MessageBox.Show("Please enter Payment Mode Name.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPayModeName.Focus();
                    return;
                }

                bool isNew = (currentModel == null || currentModel.PayModeID <= 0);
                string paymodeName = txtPayModeName.Text.Trim();
                string confirmMsg = isNew
                    ? $"Are you sure you want to add this payment mode '{paymodeName}'?"
                    : $"Are you sure you want to update payment mode '{paymodeName}'?";

                DialogResult confirmResult = MessageBox.Show(confirmMsg, isNew ? "Confirm Add Payment Mode" : "Confirm Update Payment Mode", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirmResult != DialogResult.Yes)
                {
                    return;
                }

                if (currentModel == null)
                {
                    currentModel = new PaymodeModel();
                }

                currentModel.PayModeName = txtPayModeName.Text.Trim();
                currentModel.Description = txtDescription.Text != null ? txtDescription.Text.Trim() : string.Empty;
                currentModel.FunctionKey = cmbFunctionKey.Value?.ToString() ?? cmbFunctionKey.Text ?? string.Empty;
                currentModel.PaymodeType = cmbType.Value?.ToString() ?? cmbType.Text ?? string.Empty;
                currentModel.Category = cmbCategory.Value?.ToString() ?? cmbCategory.Text ?? string.Empty;
                currentModel.FileName = cmbFileName.Enabled ? (cmbFileName.Value?.ToString() ?? cmbFileName.Text ?? string.Empty) : string.Empty;

                currentModel.RequireFillInReference = chkRequireReference.Checked;
                currentModel.IsHide = chkHide.Checked;
                currentModel.DontOpenDrawer = chkDontOpenDrawer.Checked;
                currentModel.LedgerID = selectedLedgerId;

                if (picPaymode.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        picPaymode.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        currentModel.Photo = ms.ToArray();
                    }
                }
                else
                {
                    currentModel.Photo = null;
                }

                int savedId = repo.SavePaymode(currentModel);
                if (savedId > 0)
                {
                    MessageBox.Show("Payment Mode saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload list and clear form so it's ready for a new entry
                    LoadAllPaymodes();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Unable to save Payment Mode record into database.", "Save Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment mode: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Public alias so the universal Delete ribbon button can invoke this via reflection
        public void Delete()
        {
            DeleteRecord();
        }

        public void DeleteRecord()
        {
            try
            {
                if (currentModel == null || currentModel.PayModeID <= 0)
                {
                    MessageBox.Show("Please select a valid payment mode to delete.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show($"Are you sure you want to delete payment mode '{currentModel.PayModeName}'?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (repo.DeletePaymode(currentModel.PayModeID))
                    {
                        MessageBox.Show("Payment mode deleted successfully.", "Deleted",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadAllPaymodes();
                        if (paymodeList.Count > 0)
                            NavigateTo(0);
                        else
                            ClearFields();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting payment mode: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnF11Search_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new PosBranch_Win.DialogBox.paymentmethorddig())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedPaymodeId > 0)
                    {
                        LoadAllPaymodes();
                        int foundIndex = paymodeList.FindIndex(p => p.PayModeID == dlg.SelectedPaymodeId);
                        if (foundIndex >= 0)
                        {
                            NavigateTo(foundIndex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Payment Method dialog: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearchLedger_Click(object sender, EventArgs e)
        {
            try
            {
                using (var searchForm = new PosBranch_Win.DialogBox.FrmLedgerSearch())
                {
                    if (searchForm.ShowDialog(this) == DialogResult.OK && searchForm.SelectedLedgerId > 0)
                    {
                        selectedLedgerId = searchForm.SelectedLedgerId;
                        txtLedgerName.Text = searchForm.SelectedLedgerName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching account ledgers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemAddImage_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                    if (ofd.ShowDialog(this) == DialogResult.OK)
                    {
                        string imgFileName = Path.GetFileName(ofd.FileName);
                        UpdateImageDisplay(Image.FromFile(ofd.FileName), imgFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemRemoveImage_Click(object sender, EventArgs e)
        {
            UpdateImageDisplay(null);
            if (currentModel != null && currentModel.PayModeID > 0)
            {
                repo.RemovePhoto(currentModel.PayModeID);
            }
        }
    }
}
