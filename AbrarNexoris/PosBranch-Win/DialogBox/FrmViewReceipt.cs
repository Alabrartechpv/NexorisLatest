using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;
using Repository.Accounts;
using ModelClass;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmViewReceipt : Form
    {
        private int customerLedgerId;
        private string billNo;
        private decimal originalBillAmount;
        private CustomerReceiptInfoRepository receiptRepo;

        public FrmViewReceipt()
        {
            InitializeComponent();
            receiptRepo = new CustomerReceiptInfoRepository();
            btnClose.Click += btnClose_Click;
        }

        public FrmViewReceipt(int customerLedgerId, string billNo) : this()
        {
            this.customerLedgerId = customerLedgerId;
            this.billNo = billNo;
            this.originalBillAmount = 0; // Will be set later
            lblBillNo.Text = billNo;
        }

        public FrmViewReceipt(int customerLedgerId, string billNo, decimal originalBillAmount) : this()
        {
            this.customerLedgerId = customerLedgerId;
            this.billNo = billNo;
            this.originalBillAmount = originalBillAmount;
            lblBillNo.Text = billNo;
        }

        public void SetOriginalBillAmount(decimal amount)
        {
            this.originalBillAmount = amount;
        }

        private void FrmViewReceipt_Load(object sender, EventArgs e)
        {
            LoadReceiptHistory();
        }

        private void LoadReceiptHistory()
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

                // Get receipt history from repository
                DataTable receiptHistory = receiptRepo.GetReceiptHistory(customerLedgerId, billNoLong);

                if (receiptHistory != null && receiptHistory.Rows.Count > 0)
                {
                    // Add InvoiceAmount column if it doesn't exist
                    if (!receiptHistory.Columns.Contains("InvoiceAmount"))
                    {
                        receiptHistory.Columns.Add("InvoiceAmount", typeof(decimal));
                    }
                    if (!receiptHistory.Columns.Contains("RunningBalance"))
                    {
                        receiptHistory.Columns.Add("RunningBalance", typeof(decimal));
                    }

                    decimal runningInvoiceAmount = 0;
                    decimal runningBalance = 0;
                    for (int i = 0; i < receiptHistory.Rows.Count; i++)
                    {
                        decimal receiptAmount = receiptHistory.Rows[i]["ReceiptAmount"] != DBNull.Value
                            ? Convert.ToDecimal(receiptHistory.Rows[i]["ReceiptAmount"])
                            : 0;

                        if (i == 0)
                        {
                            // First row: use BillAmount if exists, otherwise use originalBillAmount
                            if (receiptHistory.Columns.Contains("BillAmount"))
                            {
                                runningInvoiceAmount = receiptHistory.Rows[i]["BillAmount"] != DBNull.Value
                                    ? Convert.ToDecimal(receiptHistory.Rows[i]["BillAmount"])
                                    : 0;
                            }
                            else
                            {
                                runningInvoiceAmount = originalBillAmount;
                            }
                        }
                        else
                        {
                            // Next row: use previous row's running balance
                            runningInvoiceAmount = runningBalance;
                        }

                        runningBalance = runningInvoiceAmount - receiptAmount;

                        receiptHistory.Rows[i]["InvoiceAmount"] = runningInvoiceAmount;
                        receiptHistory.Rows[i]["BalanceAmount"] = runningBalance; // Overwrite with correct running balance
                    }

                    // Set the data source
                    ultraGridViewReceipt.DataSource = receiptHistory;
                    ConfigureGridColumns();

                    // Calculate total receipt amount
                    decimal totalReceipt = 0;
                    foreach (DataRow row in receiptHistory.Rows)
                    {
                        if (row["ReceiptAmount"] != DBNull.Value)
                        {
                            totalReceipt += Convert.ToDecimal(row["ReceiptAmount"]);
                        }
                    }

                    // Update the form title to show total
                    this.Text = $"Receipt History - Bill #{billNo} - Total: {totalReceipt:N2}";
                }
                else
                {
                    MessageBox.Show("No receipt history found for this bill.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading receipt history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            if (ultraGridViewReceipt.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGridViewReceipt.DisplayLayout.Bands[0];

                // Configure columns in the correct order
                if (band.Columns.Exists("BillNo"))
                {
                    band.Columns["BillNo"].Header.Caption = "Bill No";
                    band.Columns["BillNo"].Width = 100;
                    band.Columns["BillNo"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    band.Columns["BillNo"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                }

                if (band.Columns.Exists("BillDate"))
                {
                    band.Columns["BillDate"].Header.Caption = "Bill Date";
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

                if (band.Columns.Exists("ReceiptAmount"))
                {
                    band.Columns["ReceiptAmount"].Header.Caption = "Receipt Amount";
                    band.Columns["ReceiptAmount"].Width = 120;
                    band.Columns["ReceiptAmount"].Format = "##,##0.00";
                    band.Columns["ReceiptAmount"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    band.Columns["ReceiptAmount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    band.Columns["ReceiptAmount"].CellAppearance.ForeColor = Color.Green;
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
                band.Columns["ReceiptAmount"].Header.VisiblePosition = 4;
                band.Columns["BalanceAmount"].Header.VisiblePosition = 5;

                // Set alternating row colors for better readability
                ultraGridViewReceipt.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(242, 242, 242);

                // Set header appearance
                ultraGridViewReceipt.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
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
                // Get the original bill amount from SMaster table
                using (var connection = new SqlConnection(receiptRepo.DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT NetAmount FROM SMaster WHERE BillNo = @BillNo AND BranchId = @BranchId", connection))
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
