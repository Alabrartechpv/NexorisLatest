using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using ModelClass.Master;
using Repository;

namespace PosBranch_Win.Accounts
{
    public partial class FrmCustomer : Form
    {
        public  int Ledgerid;
        Dropdowns drop = new Dropdowns();
        ClientOperations operation = new ClientOperations();
        
        public FrmCustomer()
        {
            InitializeComponent();
            RefreshCustomerGrid();
            btnUpdate.Visible = false;
        }

        private void FrmCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }

        }

        private void FrmCustomer_Load(object sender, EventArgs e)
        {
            BranchDDlGrid brnachDDl = drop.getBanchDDl();
            CmbBoxBranch.DataSource = brnachDDl.List;
            CmbBoxBranch.DisplayMember = "BranchName";
            CmbBoxBranch.ValueMember = "Id";

            PriceLevelDDlGrid payModeG = drop.GetPriceLevel();
            CmboxPriceLevel.DataSource = payModeG.List;
            CmboxPriceLevel.DisplayMember = "PriceLevel";
            CmboxPriceLevel.ValueMember = "PriceLevelId";


            
        }

        

        public void RefreshCustomerGrid()
        {
            CustomerDDLGrids GridCustomer = new CustomerDDLGrids();
            CustomerRepositoty CustRp = new CustomerRepositoty();
            GridCustomer =  CustRp.GetCustomerDDL();
            GridCustomer.List.ToString();
            dataGridViewCustomer.DataSource = GridCustomer.List;
        }

      

        private void dataGridViewCustomer_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

            ClsCustomers ObjCust = new ClsCustomers();
            CustomerAddress objCustAddress = new CustomerAddress();

            CustAddressDDLGrids ObjCustAddres = new CustAddressDDLGrids();
            CustomerRepositoty repoCust = new CustomerRepositoty();

            if (e.RowIndex > 0)
            {
                 Ledgerid = (int)dataGridViewCustomer.Rows[e.RowIndex].Cells["LedgerID"].Value;
                ObjCust.LedgerId = Ledgerid;
                ObjCustAddres = repoCust.getCustAddress(Ledgerid);
                foreach (var kk in ObjCustAddres.ListCustomer)
                {
                    txtCustomerName.Text = kk.LedgerName;
                    txtAlias.Text = kk.AliasName;
                    txtOpnDebit.Text = kk.OpenDebit.ToString();
                    txtOpnCredit.Text = kk.OpenCredit.ToString(); 

                }

               if(ObjCustAddres.ListCustAddress!=null)
                { 
                foreach (var ss in ObjCustAddres.ListCustAddress)
                {
                    txtPhone.Text = ss.Phone;
                    txtEmail.Text = ss.Email;
                }
                }

            }

            BtnSave.Visible = false;
            btnUpdate.Visible = true;

            

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            ClsCustomers ObjCustomer = new ClsCustomers();
            CustomerAddress ObjCustAddress = new CustomerAddress();
            CustomerRepositoty objRepo = new CustomerRepositoty();

            ObjCustomer.CompanyId = SessionContext.CompanyId;
            ObjCustomer.BranchId = SessionContext.BranchId;
            ObjCustomer.LedgerId = Ledgerid;
            ObjCustomer.LedgerName = txtCustomerName.Text;
            ObjCustomer.AliasName = txtAlias.Text;
            ObjCustomer.PriceLevel = CmboxPriceLevel.GetItemText(CmboxPriceLevel.SelectedItem);
            ObjCustomer.OpenDebit = Convert.ToDecimal( txtOpnDebit.Text);
            ObjCustomer.OpenCredit = Convert.ToDecimal(txtOpnCredit.Text);
            ObjCustomer.Description = "";
            ObjCustomer.Notes = "";
            ObjCustomer._Operation = "Update";

            ObjCustAddress.Email = txtEmail.Text;
            ObjCustAddress.Phone = txtPhone.Text;
            ObjCustAddress.LedgerId = Ledgerid;
            objRepo.UpdateCstomerAddress(ObjCustomer, ObjCustAddress);



        }

        private void BtnSave_Click_1(object sender, EventArgs e)
        {
            ClsCustomers ObjCustomer = new ClsCustomers();
            CustomerAddress ObjCustAddress = new CustomerAddress();
            CustomerRepositoty CustRepo = new CustomerRepositoty();

            ObjCustomer.CompanyId = SessionContext.CompanyId;
            ObjCustomer.BranchId = SessionContext.BranchId;
            ObjCustomer.LedgerId = 0;
            ObjCustomer.LedgerName = txtCustomerName.Text;
            ObjCustomer.AliasName = txtAlias.Text;
            ObjCustomer.Description = "rrrr";
            ObjCustomer.Notes = "yyyyy";
            ObjCustomer.PriceLevel = CmboxPriceLevel.GetItemText(CmboxPriceLevel.SelectedItem);
            ObjCustomer.OpenDebit = Convert.ToDecimal(txtOpnDebit.Text);
            ObjCustomer.OpenCredit = Convert.ToDecimal(txtOpnCredit.Text);
            ObjCustomer._Operation = "GENERATELEDGER";

            ObjCustAddress.Email = txtEmail.Text;
            ObjCustAddress.Phone = txtPhone.Text;
            ObjCustAddress.Address = "mahe";

            CustRepo.SaveCustomer(ObjCustomer, ObjCustAddress);

        }
    }
}
