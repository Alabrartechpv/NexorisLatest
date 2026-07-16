using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository;
using Repository.Accounts; 
using ModelClass;
using ModelClass.Accounts;
using Infragistics.Win.UltraWinGrid;
using PosBranch_Win.Accounts;
using Infragistics.Win;


namespace PosBranch_Win.DialogBox
{
    public partial class FrmVendorDialog : Form
    {
        private TextBox txtSearchVendor;
        Dropdowns drop = new Dropdowns();
        string formName;
        private VendorDDLGrids vendorDDL;
        VendorPaymentRepository objVendPaymentRepo = new VendorPaymentRepository();
        public FrmVendorDialog(string FormNames)
        {
           
            InitializeComponent();
            formName = FormNames;
            InitializeSearchControls();
            
        }

        private void InitializeSearchControls()
        {
            Panel searchPanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.AliceBlue
            };

            txtSearchVendor = new TextBox()
            {
                Location = new Point(120, 10),
                Size = new Size(250, 23),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            Label lblSearch = new Label
            {
                Text = "Search Vendor : ",
                Location = new Point(txtSearchVendor.Left - 120, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearchVendor);
            this.Controls.Add(searchPanel);
            txtSearchVendor.TextChanged += TxtSearchVendor_TextChanged;
      

        }

     

        private void TxtSearchVendor_TextChanged(object sender, EventArgs e)
        {
            if (vendorDDL?.List == null) return;
            string searchText = txtSearchVendor.Text.Trim().ToLower();
            if(string.IsNullOrEmpty(searchText))
            {
                ultraGrid1.DataSource = vendorDDL.List;
                return;
            }

            var filteredList = vendorDDL.List.Where(c => c.LedgerName.ToString().ToLower().StartsWith(searchText)).ToList();
            ultraGrid1.DataSource = filteredList;
           
        }

        private void FrmVendorDialog_Load(object sender, EventArgs e)
        {
            this.ActiveControl = txtSearchVendor;
            txtSearchVendor.Focus();
            vendorDDL = drop.VendorDDL();
            ConfigureGrid();
            ultraGrid1.DataSource = vendorDDL.List;

            this.ultraGrid1.KeyPress += new KeyPressEventHandler(ultraGrid1_FilterKeyPress);
        }

        private void ConfigureGrid()
        {
            // Basic grid settings
            ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            // Configure appearance
            ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsVista;
            ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            ultraGrid1.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;

            // Configure selection behavior
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;

            // Configure columns
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                var band = ultraGrid1.DisplayLayout.Bands[0];

                // Configure LedgerName column
                if (band.Columns.Exists("LedgerName"))
                {
                    var ledgerColumn = band.Columns["LedgerName"];
                    ledgerColumn.Header.Caption = "Customer Name";
                    ledgerColumn.Width = 200;
                }

                // Configure LedgerID column
                if (band.Columns.Exists("LedgerID"))
                {
                    var idColumn = band.Columns["LedgerID"];
                    idColumn.Header.Caption = "ID";
                    idColumn.Width = 80;
                }
            }

        }

        private void ultraGrid1_FilterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetter(e.KeyChar))
            {
                vendorDDL = drop.VendorDDL();
                var filteredList = vendorDDL.List.Where(v =>
                    v.LedgerName.ToString().StartsWith(e.KeyChar.ToString(),
                    StringComparison.OrdinalIgnoreCase)).ToList();

                ultraGrid1.DataSource = filteredList;

                // Prevent the character from being processed further
                e.Handled = true;
            }
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (ultraGrid1.ActiveRow == null) return;



            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar==(char) Keys.Enter)
            {
                ClearUltraGrid();

                if (formName == "FrmPayment")
                {
                    //UltraGridCell AcellLedgerName =  this.ultraGrid1.ActiveRow.Cells["LedgerName"];

                    //UltraGridCell AcellLedgerId = this.ultraGrid1.ActiveRow.Cells["LedgerId"];
                    //string VendorLedgerName = AcellLedgerName.Value.ToString();
                    //int VendorLedgerId = Convert.ToInt32(AcellLedgerId.Value.ToString());
                    //FrmPayment payment = new FrmPayment();
                    //payment = (FrmPayment)Application.OpenForms["FrmPayment"];
                    //payment.txtVendorName.Text = VendorLedgerName;
                    //payment.lblVendorLedgerId.Text = VendorLedgerId.ToString();

                    ProcesspaymentPurchaseInvoiceSelection();
                }
            }
        }


        private void ProcesspaymentPurchaseInvoiceSelection()
        {
            UltraGridCell nameCell = ultraGrid1.ActiveRow.Cells["LedgerName"];
            UltraGridCell idCell = ultraGrid1.ActiveRow.Cells["LedgerID"];
            if (nameCell?.Value != null && idCell?.Value != null)
            {
                string name = nameCell.Value.ToString();
                int ledgerId = Convert.ToInt32(idCell.Value);
               var payment = (FrmPayment)Application.OpenForms["FrmPayment"];
                if(payment!=null)
                {
                    payment.SetVendorInfo(ledgerId, name);
                    this.Close();
                }
               
            }
        }

        
        private void ClearUltraGrid()
        {
            //var payments = (FrmPayment)Application.OpenForms["FrmPayment"];
            //payments.ultraGrid1.DataSource = "";
            ////payments.ultraGrid1.ResetDisplayLayout();
            //payments.ultraGrid1.Layouts.Clear();
        }
    }
}
