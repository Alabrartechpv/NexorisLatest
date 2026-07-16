using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;
using Repository.Accounts;
using System.Data.SqlClient; // Added for SqlConnection and SqlCommand
using ModelClass;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmViewPayment : Form
    {
        private int vendorLedgerId;
        private string billNo;
        private VendorPaymentRepository paymentRepo;

        public FrmViewPayment()
        {
            InitializeComponent();
            btnClose.Click += btnClose_Click;
            paymentRepo = new VendorPaymentRepository();
        }

        public FrmViewPayment(int vendorLedgerId, string billNo) : this()
        {
            this.vendorLedgerId = vendorLedgerId;
            this.billNo = billNo;
            lblBillNo.Text = billNo;
        }

        private void FrmViewPayment_Load(object sender, EventArgs e)
        {
            this.Text = $"Payment History - Bill #{billNo}";
            LoadPaymentHistory();
        }

        private void LoadPaymentHistory()
        {
            try
            {
                // Convert billNo to long for the stored procedure parameter
                if (!long.TryParse(billNo, out long billNoLong))
                {
                    MessageBox.Show("Invalid Bill Number format.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get payment history from repository
                DataTable paymentHistory = paymentRepo.GetPaymentHistory(vendorLedgerId, billNoLong);

                if (paymentHistory != null && paymentHistory.Rows.Count > 0)
                {
                    // Add InvoiceAmount column if it doesn't exist
                    if (!paymentHistory.Columns.Contains("InvoiceAmount"))
                    {
                        paymentHistory.Columns.Add("InvoiceAmount", typeof(decimal));
                    }
                    if (!paymentHistory.Columns.Contains("RunningBalance"))
                    {
                        paymentHistory.Columns.Add("RunningBalance", typeof(decimal));
                    }

                    decimal runningInvoiceAmount = 0;
                    decimal runningBalance = 0;
                    for (int i = 0; i < paymentHistory.Rows.Count; i++)
                    {
                        decimal paymentAmount = paymentHistory.Rows[i]["PaymentAmount"] != DBNull.Value
                            ? Convert.ToDecimal(paymentHistory.Rows[i]["PaymentAmount"])
                            : 0;

                        if (i == 0)
                        {
                            // First row: use BillAmount if exists, otherwise use original bill amount
                            if (paymentHistory.Columns.Contains("BillAmount"))
                            {
                                runningInvoiceAmount = paymentHistory.Rows[i]["BillAmount"] != DBNull.Value
                                    ? Convert.ToDecimal(paymentHistory.Rows[i]["BillAmount"])
                                    : 0;
                            }
                            else
                            {
                                // Try to get original bill amount from PMaster table
                                runningInvoiceAmount = GetOriginalBillAmount(billNoLong);
                            }
                        }
                        else
                        {
                            // Next row: use previous row's running balance
                            runningInvoiceAmount = runningBalance;
                        }

                        runningBalance = runningInvoiceAmount - paymentAmount;

                        paymentHistory.Rows[i]["InvoiceAmount"] = runningInvoiceAmount;
                        paymentHistory.Rows[i]["BalanceAmount"] = runningBalance; // Overwrite with correct running balance
                    }

                    // Set the data source
                    ultraGrid1.DataSource = paymentHistory;
                    ConfigureGridColumns();

                    // Calculate total payment amount
                    decimal totalPayment = 0;
                    foreach (DataRow row in paymentHistory.Rows)
                    {
                        if (row["PaymentAmount"] != DBNull.Value)
                        {
                            totalPayment += Convert.ToDecimal(row["PaymentAmount"]);
                        }
                    }

                    // Update the form title to show total
                    this.Text = $"Payment History - Bill #{billNo} - Total: {totalPayment:N2}";
                }
                else
                {
                    MessageBox.Show("No payment history found for this bill.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Configure columns in the correct order
                if (band.Columns.Exists("BillNo"))
                {
                    band.Columns["BillNo"].Header.Caption = "Purchase No";
                    band.Columns["BillNo"].Width = 100;
                    band.Columns["BillNo"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    band.Columns["BillNo"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                }

                if (band.Columns.Exists("BillDate"))
                {
                    band.Columns["BillDate"].Header.Caption = "Purchase Date";
                    band.Columns["BillDate"].Width = 100;
                    band.Columns["BillDate"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    // Format date to show only date part
                    band.Columns["BillDate"].Format = "dd-MM-yyyy";
                    band.Columns["BillDate"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                }

                if (band.Columns.Exists("InvoiceAmount"))
                {
                    band.Columns["InvoiceAmount"].Header.Caption = "Invoice Amount";
                    band.Columns["InvoiceAmount"].Width = 120;
                    band.Columns["InvoiceAmount"].Format = "##,##0.00";
                    band.Columns["InvoiceAmount"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    band.Columns["InvoiceAmount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                }

                if (band.Columns.Exists("PaymentAmount"))
                {
                    band.Columns["PaymentAmount"].Header.Caption = "Payment Amount";
                    band.Columns["PaymentAmount"].Width = 120;
                    band.Columns["PaymentAmount"].Format = "##,##0.00";
                    band.Columns["PaymentAmount"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    band.Columns["PaymentAmount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    band.Columns["PaymentAmount"].CellAppearance.ForeColor = Color.Green;
                }

                if (band.Columns.Exists("BalanceAmount"))
                {
                    band.Columns["BalanceAmount"].Header.Caption = "Balance";
                    band.Columns["BalanceAmount"].Width = 120;
                    band.Columns["BalanceAmount"].Format = "##,##0.00";
                    band.Columns["BalanceAmount"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    band.Columns["BalanceAmount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                }

                // Remove BillAmount column from display if it exists
                if (band.Columns.Exists("BillAmount"))
                {
                    band.Columns["BillAmount"].Hidden = true;
                }

                if (band.Columns.Exists("Type"))
                {
                    band.Columns["Type"].Header.Caption = "Type";
                    band.Columns["Type"].Width = 100;
                    band.Columns["Type"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    band.Columns["Type"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                }

                // Set column order
                band.Columns["BillNo"].Header.VisiblePosition = 0;
                band.Columns["BillDate"].Header.VisiblePosition = 1;
                if (band.Columns.Exists("Type"))
                {
                    band.Columns["Type"].Header.VisiblePosition = 2;
                }
                band.Columns["InvoiceAmount"].Header.VisiblePosition = 3;
                band.Columns["PaymentAmount"].Header.VisiblePosition = 4;
                band.Columns["BalanceAmount"].Header.VisiblePosition = 5;

                // Set alternating row colors for better readability
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(242, 242, 242);

                // Set header appearance
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private decimal GetOriginalBillAmount(long billNo)
        {
            try
            {
                // Get the original bill amount from PMaster table (for purchases)
                using (var connection = new SqlConnection(paymentRepo.DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT NetAmount FROM PMaster WHERE PurchaseNo = @BillNo AND BranchId = @BranchId", connection))
                    {
                        cmd.Parameters.AddWithValue("@BillNo", billNo);
                        cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId); // Use dynamic branch ID

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently handle the error - don't show warning message
            }

            // If we can't get the original amount from database, return 0
            // The calling method will handle this case
            return 0;
        }
    }
}
