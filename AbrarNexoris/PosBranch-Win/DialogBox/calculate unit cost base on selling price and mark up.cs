/*
 * Unit Cost Calculator
 * 
 * This form calculates the unit cost based on selling price and markup percentage.
 * 
 * Formula: Unit Cost = Price / (1 + Markup% / 100)
 * 
 * Example:
 * - Price: 20.000
 * - Markup: 10.00%
 * - Unit Cost: 20.000 / (1 + 10.00/100) = 20.000 / 1.10 = 18.18182
 * 
 * Usage:
 * 1. Enter the selling price in textBox3 (Price field)
 * 2. Enter the markup percentage in textBox2 (Mark Up % field)
 * 3. The calculated unit cost will appear automatically in textBox1 (Unit Cost field)
 * 4. Click OK to return the calculated value to the calling form
 * 
 * The Unit Cost field (textBox1) is read-only and cannot be modified by the user.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class calculate_unit_cost_base_on_selling_price_and_mark_up : Form
    {
        public string CalculatedUnitCost { get; private set; }
        public string MarkupPercentage { get; private set; }
        public string Price { get; private set; }
        public string UserEnteredMarkup { get { return userEnteredMarkup; } }

        // Property to get the selling price that should be applied to multiple price fields
        public string SellingPriceForMultipleFields { get; private set; }

        public event Action<string> MarkupChanged;

        private string originalUnitCost = "0.00000";
        private string lastPrice = "0.00000";
        private string lastMarkup = "0.00000";
        private string userEnteredMarkup = "0.00000"; // Store user's markup input
        private bool _suppressCalculation;

        public calculate_unit_cost_base_on_selling_price_and_mark_up()
        {
            InitializeComponent();

            // Style panels to match ultraPanel4 from frmdialForItemMaster.cs
            StyleIconPanel(ultraPanel5);
            StyleIconPanel(ultraPanel6);

            // Setup hover effect synchronization for ultraPanel5, label5, and ultraPictureBox1
            SetupHoverEffectSync(ultraPanel5, ultraPictureBox1, label5);

            // Make textBox1 (Unit Cost) read-only - this shows the calculated result
            textBox1.ReadOnly = true;
            textBox1.BackColor = Color.LightGray;

            // textBox2 = Mark Up % (user input)
            // textBox3 = Price/Selling Price (user input)

            // Setup calculation logic - calculate unit cost from price and markup
            textBox3.TextChanged += (s, e) => { if (!_suppressCalculation) { CalculateUnitCost(); } };
            textBox2.TextChanged += (s, e) => {
                if (!_suppressCalculation)
                {
                    CalculateUnitCost();
                    // keep MarkupPercentage in sync live so callers can poll it
                    MarkupPercentage = textBox2.Text;
                    // Store user's markup input for preservation
                    userEnteredMarkup = textBox2.Text;
                    try { MarkupChanged?.Invoke(textBox2.Text); } catch { }
                }
                else
                {
                    // During programmatic initialization, sync property but don't recalc or raise event
                    MarkupPercentage = textBox2.Text;
                }
            };

            // Setup input validation for numeric fields
            textBox3.KeyPress += (s, e) => {
                // Allow only numbers, decimal point, backspace, and delete
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true;
                }
                // Allow only one decimal point
                if (e.KeyChar == '.' && textBox3.Text.IndexOf('.') > -1)
                {
                    e.Handled = true;
                }
            };

            textBox2.KeyPress += (s, e) => {
                // Allow only numbers, decimal point, backspace, and delete
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true;
                }
                // Allow only one decimal point
                if (e.KeyChar == '.' && textBox2.Text.IndexOf('.') > -1)
                {
                    e.Handled = true;
                }
            };

            // Setup Enter key functionality for textBox2
            textBox2.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    // Visual feedback - briefly change background color
                    Color originalColor = textBox2.BackColor;
                    textBox2.BackColor = Color.LightGreen;

                    // Use a timer to restore the original color
                    Timer timer = new Timer();
                    timer.Interval = 200; // 200ms
                    timer.Tick += (timerSender, timerArgs) => {
                        textBox2.BackColor = originalColor;
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();

                    SaveAndClose();
                }
            };

            // Setup Enter key functionality for textBox3 (selling price)
            textBox3.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    // Visual feedback - briefly change background color
                    Color originalColor = textBox3.BackColor;
                    textBox3.BackColor = Color.LightGreen;

                    // Use a timer to restore the original color
                    Timer timer = new Timer();
                    timer.Interval = 200; // 200ms
                    timer.Tick += (timerSender, timerArgs) => {
                        textBox3.BackColor = originalColor;
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();

                    SaveAndClose();
                }
            };

            // Setup button click events
            ultraPanel5.Click += (s, e) => SaveAndClose();

            // Setup right-click functionality for ultraPanel5
            ultraPanel5.MouseClick += (s, e) => {
                if (e.Button == MouseButtons.Right)
                {
                    SaveAndClose();
                }
            };

            // Setup click events for label5 and ultraPictureBox1 to act as buttons
            label5.Click += (s, e) => SaveAndClose();
            ultraPictureBox1.Click += (s, e) => SaveAndClose();

            ultraPanel6.Click += (s, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // Set default values without triggering recalculation
            _suppressCalculation = true;
            try
            {
                textBox3.Text = lastPrice;
                textBox2.Text = lastMarkup;
                textBox1.Text = originalUnitCost;
            }
            finally
            {
                _suppressCalculation = false;
            }

            // Store the initial markup value as user entered markup
            userEnteredMarkup = lastMarkup;
        }

        public void SetUnitCost(string unitCost)
        {
            _suppressCalculation = true;
            try
            {
                originalUnitCost = unitCost;
                textBox1.Text = unitCost;
            }
            finally
            {
                _suppressCalculation = false;
            }
        }

        public void SetLastValues(string price, string markup, string unitCost)
        {
            _suppressCalculation = true;
            try
            {
                lastPrice = price;
                lastMarkup = markup;
                originalUnitCost = unitCost;

                textBox3.Text = lastPrice;
                // Only set textBox2 if it's empty or has default value to preserve user input
                // Also check if we have a user-entered markup to preserve
                if ((string.IsNullOrWhiteSpace(textBox2.Text) || textBox2.Text == "0.00000" || textBox2.Text == "0") &&
                    (string.IsNullOrWhiteSpace(userEnteredMarkup) || userEnteredMarkup == "0.00000" || userEnteredMarkup == "0"))
                {
                    textBox2.Text = lastMarkup;
                }
                textBox1.Text = originalUnitCost;
            }
            finally
            {
                _suppressCalculation = false;
            }
        }

        public void SetRetailPrice(string retailPrice)
        {
            // Set the retail price from Ult_Price into textBox3 (Price field)
            if (!string.IsNullOrEmpty(retailPrice))
            {
                _suppressCalculation = true;
                try
                {
                    textBox3.Text = retailPrice;
                    lastPrice = retailPrice;
                }
                finally
                {
                    _suppressCalculation = false;
                }
            }
        }

        // Method to set the selling price that will be applied to multiple fields
        public void SetSellingPriceForMultipleFields(string sellingPrice)
        {
            if (!string.IsNullOrEmpty(sellingPrice))
            {
                _suppressCalculation = true;
                try
                {
                    textBox3.Text = sellingPrice;
                    lastPrice = sellingPrice;
                }
                finally
                {
                    _suppressCalculation = false;
                }
            }
        }

        public void SetMarginPercentage(string marginPercentage)
        {
            // Set the margin percentage from Ult_Price into textBox2 (Mark Up % field)
            // Only if textBox2 is empty or has default value to preserve user input
            if (!string.IsNullOrEmpty(marginPercentage))
            {
                _suppressCalculation = true;
                try
                {
                    // Only overwrite if the current value is empty or default
                    if (string.IsNullOrWhiteSpace(textBox2.Text) || textBox2.Text == "0.00000" || textBox2.Text == "0")
                    {
                        textBox2.Text = marginPercentage;
                        lastMarkup = marginPercentage;
                        MarkupPercentage = marginPercentage;
                    }
                    else
                    {
                        // Preserve existing user input, just update the internal tracking
                        lastMarkup = textBox2.Text;
                        MarkupPercentage = textBox2.Text;
                    }
                }
                finally
                {
                    _suppressCalculation = false;
                }
            }
        }

        // Method to restore user's previously entered markup
        public void RestoreUserMarkup()
        {
            if (!string.IsNullOrWhiteSpace(userEnteredMarkup) && userEnteredMarkup != "0.00000" && userEnteredMarkup != "0")
            {
                _suppressCalculation = true;
                try
                {
                    textBox2.Text = userEnteredMarkup;
                    lastMarkup = userEnteredMarkup;
                    MarkupPercentage = userEnteredMarkup;
                }
                finally
                {
                    _suppressCalculation = false;
                }
            }
        }

        private void SaveAndClose()
        {
            if (decimal.TryParse(textBox2.Text, out decimal markupValue) && markupValue < 0)
            {
                textBox2.Text = "0.00000";
                MarkupPercentage = textBox2.Text;
                userEnteredMarkup = "0.00000";
                CalculateUnitCost();
            }
            CalculatedUnitCost = textBox1.Text;
            MarkupPercentage = textBox2.Text;
            Price = textBox3.Text;

            // Store the selling price value to be applied to multiple price fields
            SellingPriceForMultipleFields = textBox3.Text;

            // Store the user's markup input for future restoration
            if (!string.IsNullOrWhiteSpace(textBox2.Text) && textBox2.Text != "0.00000" && textBox2.Text != "0")
            {
                userEnteredMarkup = textBox2.Text;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Define consistent colors for the panel
            Color lightBlue = Color.FromArgb(127, 219, 255); // Light blue
            Color darkBlue = Color.FromArgb(0, 116, 217);    // Darker blue
            Color borderBlue = Color.FromArgb(0, 150, 220);  // Border blue
            Color borderBase = Color.FromArgb(0, 100, 180);  // Border base color

            // Create a gradient from light to dark blue with exact specified colors
            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Set highly rounded border style
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;

            // Add exact specified border color
            panel.Appearance.BorderColor = borderBlue;
            panel.Appearance.BorderColor3DBase = borderBase;

            // Ensure icons inside have transparent background
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    Infragistics.Win.UltraWinEditors.UltraPictureBox pic = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;
                }
                else if (control is Label)
                {
                    ((Label)control).ForeColor = Color.White;
                    ((Label)control).Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    ((Label)control).BackColor = Color.Transparent;
                }
            }

            // Add hover effect with consistent colors
            panel.ClientArea.MouseEnter += (sender, e) => {
                panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
            };

            panel.ClientArea.MouseLeave += (sender, e) => {
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
            };

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
        }

        // Updated method for synchronizing hover effects between panels, picture boxes, and labels
        private void SetupHoverEffectSync(Infragistics.Win.Misc.UltraPanel panel, Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox, Label label)
        {
            if (panel == null)
                return;

            // Store original colors
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;

            // Define hover colors - brighter versions of the original colors
            Color hoverBackColor = Color.FromArgb(
                Math.Min(originalBackColor.R + 30, 255),
                Math.Min(originalBackColor.G + 30, 255),
                Math.Min(originalBackColor.B + 30, 255));
            Color hoverBackColor2 = Color.FromArgb(
                Math.Min(originalBackColor2.R + 30, 255),
                Math.Min(originalBackColor2.G + 30, 255),
                Math.Min(originalBackColor2.B + 30, 255));

            // When mouse enters the picture box, change the panel appearance
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => {
                    panel.Appearance.BackColor = hoverBackColor;
                    panel.Appearance.BackColor2 = hoverBackColor2;
                    pictureBox.Cursor = Cursors.Hand;
                };

                // When mouse leaves the picture box, restore the panel appearance
                // but only if the mouse isn't still over the panel
                pictureBox.MouseLeave += (s, e) => {
                    Point mousePos = panel.PointToClient(Control.MousePosition);
                    if (!panel.ClientRectangle.Contains(mousePos))
                    {
                        panel.Appearance.BackColor = originalBackColor;
                        panel.Appearance.BackColor2 = originalBackColor2;
                    }
                };
            }

            // When mouse enters the label, change the panel appearance
            if (label != null)
            {
                label.MouseEnter += (s, e) => {
                    panel.Appearance.BackColor = hoverBackColor;
                    panel.Appearance.BackColor2 = hoverBackColor2;
                    label.Cursor = Cursors.Hand;
                };

                // When mouse leaves the label, restore the panel appearance
                // but only if the mouse isn't still over the panel
                label.MouseLeave += (s, e) => {
                    Point mousePos = panel.PointToClient(Control.MousePosition);
                    if (!panel.ClientRectangle.Contains(mousePos))
                    {
                        panel.Appearance.BackColor = originalBackColor;
                        panel.Appearance.BackColor2 = originalBackColor2;
                    }
                };
            }

            // Make sure panel hover events are still working properly
            panel.MouseEnter += (s, e) => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            panel.MouseLeave += (s, e) => {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };

            // Apply the same effect to the client area of the panel
            panel.ClientArea.MouseEnter += (s, e) => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            panel.ClientArea.MouseLeave += (s, e) => {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };
        }

        private void CalculateUnitCost()
        {
            if (_suppressCalculation)
            {
                return;
            }
            // Calculate unit cost from price and markup percentage
            // Formula: Unit Cost = Price / (1 + Markup% / 100)
            // Example: Price = 20.000, Markup = 10.00%
            // Unit Cost = 20.000 / (1 + 10.00/100) = 20.000 / 1.10 = 18.18182

            try
            {
                string priceText = textBox3.Text.Trim();
                string markupText = textBox2.Text.Trim();

                // Remove any leading spaces and validate input
                if (string.IsNullOrEmpty(priceText) || string.IsNullOrEmpty(markupText))
                {
                    // Don't change textBox1 value when inputs are empty
                    return;
                }

                if (decimal.TryParse(priceText, out decimal price) &&
                    decimal.TryParse(markupText, out decimal markupPercent))
                {
                    if (price >= 0 && markupPercent >= 0)
                    {
                        if (markupPercent == 100)
                        {
                            // Special case: 100% markup means unit cost is half the price
                            decimal unitCost = price / 2;
                            textBox1.Text = unitCost.ToString("0.00000");
                        }
                        else if (markupPercent > 100)
                        {
                            // For markup > 100%, the formula still works
                            decimal unitCost = price / (1 + markupPercent / 100);
                            textBox1.Text = unitCost.ToString("0.00000");
                        }
                        else
                        {
                            // Normal case: 0% <= markup < 100%
                            decimal unitCost = price / (1 + markupPercent / 100);
                            textBox1.Text = unitCost.ToString("0.00000");
                        }
                    }
                    else
                    {
                        // Don't change textBox1 value when inputs are invalid
                        return;
                    }
                }
                else
                {
                    // Don't change textBox1 value when parsing fails
                    return;
                }
            }
            catch (Exception)
            {
                // Don't change textBox1 value when exception occurs
                return;
            }
        }
    }
}
