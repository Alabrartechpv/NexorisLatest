using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;

namespace PosBranch_Win.Master
{
    public partial class FrmCurrency : Form
    {
        private CurrencyRepository repository = new CurrencyRepository();
        private List<CurrencyModel> currencyList = new List<CurrencyModel>();
        private int currentIndex = -1;
        private int selectedCurrencyId = 0;
        private byte[] currentImageBytes = null;
        private string baseCurrencySymbol = "₹";

        public FrmCurrency()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }

        private void FrmCurrency_Load(object sender, EventArgs e)
        {
            StyleFormControls();
            AttachEvents();
            LoadBaseCurrencySymbol();
            RefreshCurrencyList();
            ClearForm();
        }

        private void AttachEvents()
        {
            btnLookupF7.Click += (s, e) => OpenCurrencyLookup();

            // Navigation panel buttons (matching frmItemMasterNew)
            if (ultraPanel3 != null) ultraPanel3.Click += (s, e) => NavigateRecord(0);
            if (ultraPictureBox2 != null) ultraPictureBox2.Click += (s, e) => NavigateRecord(0);

            if (ultraPanel9 != null) ultraPanel9.Click += (s, e) => NavigateRecord(currentIndex - 1);
            if (ultraPictureBox4 != null) ultraPictureBox4.Click += (s, e) => NavigateRecord(currentIndex - 1);

            if (ultraPanel8 != null) ultraPanel8.Click += (s, e) => NavigateRecord(currentIndex + 1);
            if (ultraPictureBox6 != null) ultraPictureBox6.Click += (s, e) => NavigateRecord(currentIndex + 1);

            if (ultraPanel10 != null) ultraPanel10.Click += (s, e) => NavigateRecord(currencyList.Count - 1);
            if (ultraPictureBox5 != null) ultraPictureBox5.Click += (s, e) => NavigateRecord(currencyList.Count - 1);

            menuAddImage.Click += (s, e) => AddCurrencyImage();
            menuDeleteImage.Click += (s, e) => DeleteCurrencyImage();
            picCurrency.Paint += PicCurrency_Paint;

            txtCurrencyCode.ValueChanged += TxtCurrencyCode_ValueChanged;
            txtCurrencyCode.Leave += TxtCurrencyCode_Leave;
            txtExchangeRate.Leave += TxtExchangeRate_Leave;
        }

        private void TxtExchangeRate_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtExchangeRate.Text.Trim(), out decimal rate))
            {
                txtExchangeRate.Text = rate.ToString("0.0000");
            }
            else
            {
                txtExchangeRate.Text = "1.0000";
            }
        }

        private void TxtCurrencyCode_ValueChanged(object sender, EventArgs e)
        {
            UpdateFormulaLabel();
        }

        private void UpdateFormulaLabel()
        {
            string code = txtCurrencyCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
                lblFormulaOne.Text = "1 unit =";
            else
                lblFormulaOne.Text = $"1 {code} =";
        }

        private void LoadBaseCurrencySymbol()
        {
            try
            {
                baseCurrencySymbol = "₹";
            }
            catch
            {
                baseCurrencySymbol = "₹";
            }
            lblBaseCurrencySymbol.Text = baseCurrencySymbol;
        }

        public void RefreshCurrencyList()
        {
            try
            {
                currencyList = repository.GetAllCurrencies();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading currency list: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ClearForm()
        {
            selectedCurrencyId = 0;
            currentIndex = -1;
            txtCurrencyCode.Text = "";
            txtCurrencyName.Text = "";
            txtExchangeRate.Text = "1.0000";
            currentImageBytes = null;
            UpdateFormulaLabel();
            UpdateImageDisplay();

            txtCurrencyCode.Focus();
        }

        private void DisplayCurrency(CurrencyModel model, int index)
        {
            if (model == null) return;
            selectedCurrencyId = model.CurrencyID;
            currentIndex = index;

            txtCurrencyCode.Text = !string.IsNullOrEmpty(model.CurrencyCode) ? model.CurrencyCode : model.CurrencySymbol;
            txtCurrencyName.Text = model.CurrencyName ?? model.CurrencyCode ?? "";
            txtExchangeRate.Text = model.ExchangeRate > 0 ? model.ExchangeRate.ToString("0.0000") : "1.0000";
            UpdateFormulaLabel();

            if (model.CurrencyImage != null && model.CurrencyImage.Length > 0)
            {
                currentImageBytes = model.CurrencyImage;
            }
            else
            {
                currentImageBytes = null;
            }

            UpdateImageDisplay();
        }

        private void UpdateImageDisplay()
        {
            if (currentImageBytes != null && currentImageBytes.Length > 0)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(currentImageBytes))
                    {
                        picCurrency.Image = Image.FromStream(ms);
                    }
                    return;
                }
                catch { }
            }

            int width = picCurrency.Width > 0 ? picCurrency.Width : 200;
            int height = picCurrency.Height > 0 ? picCurrency.Height : 160;
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Soft metallic card gradient background
                using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, width, height),
                    Color.FromArgb(250, 252, 255),
                    Color.FromArgb(226, 238, 252),
                    45f))
                {
                    g.FillRectangle(bgBrush, 0, 0, width, height);
                }

                // Inner subtle card border
                using (Pen borderPen = new Pen(Color.FromArgb(175, 200, 235), 1f))
                {
                    g.DrawRectangle(borderPen, 2, 2, width - 5, height - 5);
                }

                // Draw Currency Banknote Vector Graphic
                int cx = width / 2;
                int cy = height / 2 - 12;
                int nw = 64;
                int nh = 38;
                Rectangle noteRect = new Rectangle(cx - nw / 2, cy - nh / 2, nw, nh);

                using (Pen notePen = new Pen(Color.FromArgb(130, 168, 215), 2f))
                using (SolidBrush noteFill = new SolidBrush(Color.FromArgb(240, 246, 255)))
                {
                    g.FillRectangle(noteFill, noteRect);
                    g.DrawRectangle(notePen, noteRect);
                    g.DrawEllipse(notePen, cx - 11, cy - 11, 22, 22);
                }

                // Text caption
                using (Font font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.FromArgb(65, 95, 135)))
                {
                    string text = "No Image Uploaded";
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (width - textSize.Width) / 2;
                    float y = cy + nh / 2 + 10;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            picCurrency.Image = bmp;
        }

        private void NavigateRecord(int targetIndex)
        {
            if (currencyList == null || currencyList.Count == 0) return;
            if (targetIndex < 0) targetIndex = 0;
            if (targetIndex >= currencyList.Count) targetIndex = currencyList.Count - 1;

            DisplayCurrency(currencyList[targetIndex], targetIndex);
        }

        public void OpenCurrencyLookup()
        {
            using (CurrencyDialog dialog = new CurrencyDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int id = dialog.SelectedCurrencyID;
                    CurrencyModel model = repository.GetByIdCurrency(id);
                    if (model == null || model.CurrencyID == 0)
                    {
                        int idx = currencyList.FindIndex(c => c.CurrencyID == id);
                        if (idx >= 0) model = currencyList[idx];
                    }

                    if (model != null && model.CurrencyID > 0)
                    {
                        int idx = currencyList.FindIndex(c => c.CurrencyID == id);
                        if (idx >= 0) currencyList[idx] = model;
                        DisplayCurrency(model, idx >= 0 ? idx : 0);
                    }
                }
            }
        }

        private void TxtCurrencyCode_Leave(object sender, EventArgs e)
        {
            string code = txtCurrencyCode.Text.Trim();
            if (!string.IsNullOrEmpty(code) && selectedCurrencyId == 0)
            {
                int idx = currencyList.FindIndex(c =>
                    string.Equals(c.CurrencyCode, code, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.CurrencySymbol, code, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                {
                    DisplayCurrency(currencyList[idx], idx);
                }
            }
        }

        private void AddCurrencyImage()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        currentImageBytes = File.ReadAllBytes(dialog.FileName);
                        UpdateImageDisplay();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DeleteCurrencyImage()
        {
            currentImageBytes = null;
            UpdateImageDisplay();
        }

        // Public Action Aliases for Home Ribbon & Keyboard Shortcuts
        public void Save() => SaveCurrency();
        public void SaveData() => SaveCurrency();
        public void RibbonSave() => SaveCurrency();

        public void Clear() => ClearForm();
        public void Reset() => ClearForm();
        public void RibbonClear() => ClearForm();

        public void Delete() => DeleteCurrency();
        public void DeleteRecord() => DeleteCurrency();
        public void RibbonDeleteInvoice() => DeleteCurrency();

        public void Update() => UpdateCurrency();
        public void UpdateRecord() => UpdateCurrency();
        public void UpdateData() => UpdateCurrency();

        public void Report() => ShowReport();
        public void Print() => ShowReport();

        public void SaveCurrency()
        {
            if (selectedCurrencyId > 0)
            {
                UpdateCurrency();
                return;
            }

            if (!ValidateForm()) return;

            try
            {
                CurrencyModel model = BuildModelFromForm();
                var result = repository.SaveCurrency(model);
                MessageBox.Show("Currency saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshCurrencyList();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving currency: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateCurrency()
        {
            if (selectedCurrencyId <= 0) return;
            if (!ValidateForm()) return;

            try
            {
                CurrencyModel model = BuildModelFromForm();
                model.CurrencyID = selectedCurrencyId;
                var result = repository.UpdateCurrency(model);
                MessageBox.Show("Currency updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshCurrencyList();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating currency: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteCurrency()
        {
            if (selectedCurrencyId <= 0)
            {
                MessageBox.Show("Please select a currency to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this currency?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    repository.DeleteCurrency(selectedCurrencyId);
                    MessageBox.Show("Currency deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshCurrencyList();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting currency: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void ShowReport()
        {
            MessageBox.Show("Currency summary report functionality.", "Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtCurrencyCode.Text))
            {
                MessageBox.Show("Please enter Currency Code.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCurrencyCode.Focus();
                return false;
            }
            return true;
        }

        private CurrencyModel BuildModelFromForm()
        {
            decimal.TryParse(txtExchangeRate.Text.Trim(), out decimal rate);
            if (rate <= 0) rate = 1.0000m;

            string code = txtCurrencyCode.Text.Trim();
            string name = !string.IsNullOrWhiteSpace(txtCurrencyName.Text) ? txtCurrencyName.Text.Trim() : code;

            return new CurrencyModel
            {
                CurrencyID = selectedCurrencyId,
                CurrencyCode = code,
                CurrencyName = name,
                CurrencySymbol = code,
                CurrencyUnit = name,
                ExchangeRate = rate,
                AmntInMillions = false,
                DecimalPlace = 2,
                CountryID = 1,
                CurrencyImage = currentImageBytes
            };
        }

        private void StyleFormControls()
        {
            try
            {
                this.BackColor = Color.FromArgb(218, 234, 254);
                if (pnlMainBackground != null)
                    pnlMainBackground.BackColor = Color.FromArgb(218, 234, 254);

                // Apply Item Master Glossy Button Style to Navigation Panels & F7
                MakeControlActAndLookLikeGlossyButton(ultraPanel3);
                MakeControlActAndLookLikeGlossyButton(ultraPanel9);
                MakeControlActAndLookLikeGlossyButton(ultraPanel8);
                MakeControlActAndLookLikeGlossyButton(ultraPanel10);
                MakeControlActAndLookLikeGlossyButton(btnLookupF7);

                // Style Labels with Right-Alignment & Segoe UI Typography
                if (lblCurrencyCode != null)
                {
                    lblCurrencyCode.Appearance.FontData.Name = "Segoe UI";
                    lblCurrencyCode.Appearance.FontData.SizeInPoints = 9F;
                    lblCurrencyCode.Appearance.ForeColor = Color.FromArgb(15, 30, 55);
                    lblCurrencyCode.Appearance.TextHAlignAsString = "Right";
                    lblCurrencyCode.Appearance.TextVAlignAsString = "Middle";
                    lblCurrencyCode.Location = new Point(15, 22);
                    lblCurrencyCode.Size = new Size(105, 24);
                }

                if (lblFormulaOne != null)
                {
                    lblFormulaOne.Appearance.FontData.Name = "Segoe UI";
                    lblFormulaOne.Appearance.FontData.SizeInPoints = 9.5F;
                    lblFormulaOne.Appearance.FontData.BoldAsString = "True";
                    lblFormulaOne.Appearance.ForeColor = Color.FromArgb(15, 30, 55);
                    lblFormulaOne.Appearance.TextHAlignAsString = "Right";
                    lblFormulaOne.Appearance.TextVAlignAsString = "Middle";
                    lblFormulaOne.Location = new Point(15, 58);
                    lblFormulaOne.Size = new Size(105, 24);
                }

                if (lblCurrencyName != null)
                {
                    lblCurrencyName.Appearance.FontData.Name = "Segoe UI";
                    lblCurrencyName.Appearance.FontData.SizeInPoints = 9F;
                    lblCurrencyName.Appearance.ForeColor = Color.FromArgb(15, 30, 55);
                    lblCurrencyName.Appearance.TextHAlignAsString = "Right";
                    lblCurrencyName.Appearance.TextVAlignAsString = "Middle";
                    lblCurrencyName.Location = new Point(15, 94);
                    lblCurrencyName.Size = new Size(105, 24);
                }

                if (lblBaseCurrencySymbol != null)
                {
                    lblBaseCurrencySymbol.Appearance.FontData.Name = "Segoe UI";
                    lblBaseCurrencySymbol.Appearance.FontData.SizeInPoints = 11F;
                    lblBaseCurrencySymbol.Appearance.FontData.BoldAsString = "True";
                    lblBaseCurrencySymbol.Appearance.ForeColor = Color.FromArgb(12, 35, 75);
                    lblBaseCurrencySymbol.Appearance.TextVAlignAsString = "Middle";
                    lblBaseCurrencySymbol.Location = new Point(254, 58);
                    lblBaseCurrencySymbol.Size = new Size(40, 24);
                }

                // Style Text Editors (Signature IRS POS Peach Background)
                if (txtCurrencyCode != null)
                {
                    txtCurrencyCode.Appearance.BackColor = Color.FromArgb(255, 228, 208);
                    txtCurrencyCode.Appearance.BorderColor = Color.FromArgb(150, 180, 215);
                    txtCurrencyCode.Appearance.FontData.Name = "Segoe UI";
                    txtCurrencyCode.Appearance.FontData.SizeInPoints = 9.5F;
                    txtCurrencyCode.Appearance.FontData.BoldAsString = "True";
                    txtCurrencyCode.Location = new Point(125, 20);
                    txtCurrencyCode.Size = new Size(120, 26);
                }

                if (txtExchangeRate != null)
                {
                    txtExchangeRate.Appearance.BackColor = Color.FromArgb(255, 228, 208);
                    txtExchangeRate.Appearance.BorderColor = Color.FromArgb(150, 180, 215);
                    txtExchangeRate.Appearance.FontData.Name = "Segoe UI";
                    txtExchangeRate.Appearance.FontData.SizeInPoints = 9.5F;
                    txtExchangeRate.Appearance.FontData.BoldAsString = "True";
                    txtExchangeRate.Appearance.TextHAlignAsString = "Right";
                    txtExchangeRate.Location = new Point(125, 56);
                    txtExchangeRate.Size = new Size(120, 26);
                }

                if (txtCurrencyName != null)
                {
                    txtCurrencyName.Appearance.BackColor = Color.FromArgb(255, 228, 208);
                    txtCurrencyName.Appearance.BorderColor = Color.FromArgb(150, 180, 215);
                    txtCurrencyName.Appearance.FontData.Name = "Segoe UI";
                    txtCurrencyName.Appearance.FontData.SizeInPoints = 9.5F;
                    txtCurrencyName.Location = new Point(125, 92);
                    txtCurrencyName.Size = new Size(313, 26);
                }

                // Style PictureBox & Hint Label Alignment
                if (picCurrency != null)
                {
                    picCurrency.Location = new Point(125, 128);
                    picCurrency.Size = new Size(200, 160);
                }

                if (lblImageHint != null)
                {
                    lblImageHint.Appearance.FontData.Name = "Segoe UI";
                    lblImageHint.Appearance.FontData.SizeInPoints = 8.5F;
                    lblImageHint.Appearance.FontData.ItalicAsString = "True";
                    lblImageHint.Appearance.ForeColor = Color.FromArgb(70, 95, 130);
                    lblImageHint.Location = new Point(125, 294);
                    lblImageHint.Size = new Size(220, 20);
                }

               
            }
            catch { }
        }

        private void FrmCurrency_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    ClearForm();
                    e.Handled = true;
                    break;
                case Keys.F8:
                    SaveCurrency();
                    e.Handled = true;
                    break;
                case Keys.F12:
                    DeleteCurrency();
                    e.Handled = true;
                    break;
                case Keys.F4:
                    this.Close();
                    e.Handled = true;
                    break;
                case Keys.F7:
                    OpenCurrencyLookup();
                    e.Handled = true;
                    break;
                case Keys.F3:
                    ShowReport();
                    e.Handled = true;
                    break;
            }
        }

        private void PicCurrency_Paint(object sender, PaintEventArgs e)
        {
            // Draw a premium soft-blue rounded border over the PictureBox
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = picCurrency.Width - 1;
            int h = picCurrency.Height - 1;

            // Outer shadow
            using (var shadowPen = new Pen(Color.FromArgb(40, 100, 160, 220), 3f))
            {
                g.DrawRectangle(shadowPen, 1, 1, w - 1, h - 1);
            }

            // Inner accent line
            using (var innerPen = new Pen(Color.FromArgb(140, 175, 220), 1f))
            {
                g.DrawRectangle(innerPen, 2, 2, w - 4, h - 4);
            }

            // Main crisp border
            using (var mainPen = new Pen(Color.FromArgb(110, 150, 210), 1.5f))
            {
                g.DrawRectangle(mainPen, 0, 0, w, h);
            }
        }

        #region Glossy Button & Panel Hover/Click Styling (Matching frmItemMasterNew)

        private void MakeControlActAndLookLikeGlossyButton(Control ctrl)
        {
            if (ctrl == null) return;

            Color normalTop = Color.FromArgb(212, 232, 255);
            Color normalBottom = Color.FromArgb(172, 202, 245);
            Color border = Color.FromArgb(110, 150, 215);
            Color textNavy = Color.FromArgb(10, 35, 80);

            ctrl.Cursor = Cursors.Hand;

            if (ctrl is Infragistics.Win.Misc.UltraPanel up)
            {
                up.UseAppStyling = false;
                up.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
                up.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                up.Appearance.BackColor = normalTop;
                up.Appearance.BackColor2 = normalBottom;
                up.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                up.Appearance.BorderColor = border;
                up.Appearance.ForeColor = textNavy;

                up.MouseEnter -= Panel_MouseEnter;
                up.MouseEnter += Panel_MouseEnter;
                up.MouseLeave -= Panel_MouseLeave;
                up.MouseLeave += Panel_MouseLeave;
                up.MouseDown -= Panel_MouseDown;
                up.MouseDown += Panel_MouseDown;
                up.MouseUp -= Panel_MouseUp;
                up.MouseUp += Panel_MouseUp;

                if (up.ClientArea != null)
                {
                    foreach (Control child in up.ClientArea.Controls)
                    {
                        child.Cursor = Cursors.Hand;
                        child.MouseEnter -= Child_MouseEnter;
                        child.MouseEnter += Child_MouseEnter;
                        child.MouseLeave -= Child_MouseLeave;
                        child.MouseLeave += Child_MouseLeave;
                        child.MouseDown -= Child_MouseDown;
                        child.MouseDown += Child_MouseDown;
                        child.MouseUp -= Child_MouseUp;
                        child.MouseUp += Child_MouseUp;
                        child.Click -= Child_Click;
                        child.Click += Child_Click;
                    }
                }
            }
            else if (ctrl is Infragistics.Win.Misc.UltraButton ubtn)
            {
                ubtn.UseAppStyling = false;
                ubtn.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
                ubtn.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
                ubtn.Appearance.BackColor = normalTop;
                ubtn.Appearance.BackColor2 = normalBottom;
                ubtn.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ubtn.Appearance.BorderColor = border;
                ubtn.Appearance.ForeColor = textNavy;
                ubtn.Appearance.FontData.BoldAsString = "True";
                ubtn.Appearance.FontData.Name = "Segoe UI";
                ubtn.Appearance.FontData.SizeInPoints = 9F;
                ubtn.Appearance.TextHAlignAsString = "Center";
                ubtn.Appearance.TextVAlignAsString = "Middle";
                ubtn.Cursor = Cursors.Hand;

                ubtn.HotTrackAppearance.BackColor = Color.FromArgb(232, 244, 255);
                ubtn.HotTrackAppearance.BackColor2 = Color.FromArgb(188, 216, 255);
                ubtn.HotTrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ubtn.HotTrackAppearance.BorderColor = Color.FromArgb(80, 120, 200);

                ubtn.PressedAppearance.BackColor = Color.FromArgb(155, 190, 238);
                ubtn.PressedAppearance.BackColor2 = Color.FromArgb(185, 212, 248);
                ubtn.PressedAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ubtn.PressedAppearance.BorderColor = Color.FromArgb(70, 110, 180);
            }
        }

        private void Panel_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Appearance.BackColor = Color.FromArgb(232, 244, 255);
                up.Appearance.BackColor2 = Color.FromArgb(188, 216, 255);
            }
        }

        private void Panel_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Appearance.BackColor = Color.FromArgb(212, 232, 255);
                up.Appearance.BackColor2 = Color.FromArgb(172, 202, 245);
            }
        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Appearance.BackColor = Color.FromArgb(155, 190, 238);
                up.Appearance.BackColor2 = Color.FromArgb(185, 212, 248);
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Appearance.BackColor = Color.FromArgb(232, 244, 255);
                up.Appearance.BackColor2 = Color.FromArgb(188, 216, 255);
            }
        }

        private void Child_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                Panel_MouseEnter(up, e);
            }
        }

        private void Child_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                Panel_MouseLeave(up, e);
            }
        }

        private void Child_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                Panel_MouseDown(up, e);
            }
        }

        private void Child_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                Panel_MouseUp(up, e);
            }
        }

        private void Child_Click(object sender, EventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Focus();
            }
        }

        #endregion
    }
}
