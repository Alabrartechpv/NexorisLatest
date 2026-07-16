using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass.Master;
using Repository.MasterRepositry;

namespace PosBranch_Win.DialogBox
{
    public partial class view : Form
    {
        // Property to store current item ID
        public int CurrentItemId { get; private set; }

        public view()
        {
            InitializeComponent();

            // Style panels to match ultraPanel4 from frmdialForItemMaster.cs
            StyleIconPanel(ultraPanel5);
            StyleIconPanel(ultraPanel6);
            StyleIconPanel(ultraPanel3);
            StyleIconPanel(ultraPanel7);

            // Add click event handlers for OK and Close buttons
            ultraPanel5.Click += (sender, e) => this.DialogResult = DialogResult.OK;
            ultraPanel6.Click += (sender, e) => this.DialogResult = DialogResult.Cancel;

            // Initialize with empty hold details
            double initialHoldQty = LoadHoldDetails(0);

            // Add form load event handler
            this.Load += View_Load;
        }

        private void View_Load(object sender, EventArgs e)
        {
            // If no item ID is set, show empty grid
            if (CurrentItemId == 0)
            {
                double holdQty = LoadHoldDetails(0);
            }
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

        // Method to load hold details for a specific item
        public double LoadHoldDetails(int itemId)
        {
            try
            {
                // Set current item ID
                CurrentItemId = itemId;

                // If itemId is 0, show empty grid
                if (itemId == 0)
                {
                    DataTable dtEmpty = new DataTable();
                    dtEmpty.Columns.Add("BillNo", typeof(string));
                    dtEmpty.Columns.Add("LedgerID", typeof(int));
                    dtEmpty.Columns.Add("CustomerName", typeof(string));
                    dtEmpty.Columns.Add("HoldQty", typeof(double));
                    dtEmpty.Columns.Add("Unit", typeof(string));

                    ultraGrid1.DataSource = dtEmpty;
                    label1.Text = "No item selected";
                    return 0;
                }

                // Get hold details from repository
                ItemMasterRepository itemRepo = new ItemMasterRepository();
                List<HoldItemDetails> holdDetails = itemRepo.GetHoldItemDetails(itemId);

                // Create DataTable for UltraGrid1
                DataTable dtHoldDetails = new DataTable();
                dtHoldDetails.Columns.Add("BillNo", typeof(string));
                dtHoldDetails.Columns.Add("LedgerID", typeof(int));
                dtHoldDetails.Columns.Add("CustomerName", typeof(string));
                dtHoldDetails.Columns.Add("HoldQty", typeof(double));
                dtHoldDetails.Columns.Add("Unit", typeof(string));

                // Calculate total hold quantity
                double totalHoldQty = 0;

                // Add rows to the DataTable
                foreach (var detail in holdDetails)
                {
                    DataRow row = dtHoldDetails.NewRow();
                    row["BillNo"] = detail.BillNo;
                    row["LedgerID"] = detail.LedgerID;
                    row["CustomerName"] = detail.CustomerName;
                    row["HoldQty"] = detail.HoldQty;
                    row["Unit"] = detail.Unit;
                    dtHoldDetails.Rows.Add(row);

                    // Add to total
                    totalHoldQty += detail.HoldQty;
                }

                // Set the DataTable as DataSource for ultraGrid1
                ultraGrid1.DataSource = dtHoldDetails;

                // Update the label to show the count and total
                label1.Text = $"Total Hold Records: {holdDetails.Count} | Total Hold Qty: {totalHoldQty:N2}";

                return totalHoldQty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading hold details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }
    }
}
