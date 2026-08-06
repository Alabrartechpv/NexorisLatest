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
            AttachEvents();
            LoadBaseCurrencySymbol();
            RefreshCurrencyList();
            ClearForm();
        }

        private void AttachEvents()
        {
            btnLookupF7.Click += (s, e) => OpenCurrencyLookup();
            btnFirst.Click += (s, e) => NavigateRecord(0);
            btnPrev.Click += (s, e) => NavigateRecord(currentIndex - 1);
            btnNext.Click += (s, e) => NavigateRecord(currentIndex + 1);
            btnLast.Click += (s, e) => NavigateRecord(currencyList.Count - 1);

            menuAddImage.Click += (s, e) => AddCurrencyImage();
            menuDeleteImage.Click += (s, e) => DeleteCurrencyImage();

            txtCurrencyCode.ValueChanged += TxtCurrencyCode_ValueChanged;
            txtCurrencyCode.Leave += TxtCurrencyCode_Leave;
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

            int width = picCurrency.Width > 0 ? picCurrency.Width : 220;
            int height = picCurrency.Height > 0 ? picCurrency.Height : 175;
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                using (Font font = new Font("Segoe UI", 9F, FontStyle.Regular))
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    string text = "No image data";
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (width - textSize.Width) / 2;
                    float y = (height - textSize.Height) / 2;
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
                    int idx = currencyList.FindIndex(c => c.CurrencyID == id);
                    if (idx >= 0)
                    {
                        DisplayCurrency(currencyList[idx], idx);
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
    }
}
