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
    public partial class FrmVendor : Form
    {
        public int LedgerId;
        Dropdowns drop = new Dropdowns();
        public FrmVendor()
        {
            InitializeComponent();
            RefreshVendorGrid();
            btnUpdate.Visible = false;
        }

        private void FrmVendor_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Escape)
            {
                this.Close();
            }
        }

       

        private void FrmVendor_Load(object sender, EventArgs e)
        {
            BranchDDlGrid branchDDLGrid = drop.getBanchDDl();
            ComboBranch.DataSource = branchDDLGrid.List;
            ComboBranch.DisplayMember = "BranchName";
            ComboBranch.ValueMember = "Id";


        }

        public void RefreshVendorGrid()
        {
            VendorDDLGrid ObJVendorDDLG = new VendorDDLGrid();
            VendorRepository objRepoVendor = new VendorRepository();
            ObJVendorDDLG = objRepoVendor.GetVendorDDLGrid();
            dataGridViewVendor.DataSource = ObJVendorDDLG.List;
        }

    

        private void dataGridViewVendor_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {


            ClsVendors objVendor = new ClsVendors();
            VendorAddress ObjVendAddress = new VendorAddress();
            VendorAddressDDLGrid ObjVendoAddresDDlG = new VendorAddressDDLGrid();
            VendorRepository ObjVendRepo = new VendorRepository();
            if (e.RowIndex > 0)
            {
                 LedgerId = (int)dataGridViewVendor.Rows[e.RowIndex].Cells["LedgerID"].Value;
                ObjVendoAddresDDlG = ObjVendRepo.getVendorAddress(LedgerId);
                foreach (var vv in ObjVendoAddresDDlG.ListVendor)
                {
                    txtVendor.Text = vv.LedgerName;
                    txtAliasName.Text = vv.Alias;
                    txtOpenDebit.Text = vv.OpnDebit.ToString();
                    txtOpenCredit.Text = vv.OpnCredit.ToString();
                }

                if(ObjVendoAddresDDlG.ListVendorAddress!=null)
                { 
                    foreach (var va in ObjVendoAddresDDlG.ListVendorAddress)
                    {
                        txtPhone.Text = va.Phone;
                        txtEmail.Text = va.Email;
                    }
                }

            }

            btnSave.Visible = false;
            btnUpdate.Visible = true;


        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            ClsVendors ObjVendor = new ClsVendors();
            VendorAddress ObjVendAddress = new VendorAddress();
            VendorRepository ObjRepoVendor = new VendorRepository();

            ObjVendor.CompanyId = SessionContext.CompanyId;
            ObjVendor.BranchId = SessionContext.BranchId;
            ObjVendor.LedgerId = LedgerId;
            ObjVendor.LedgerName = txtVendor.Text;
            ObjVendor.Alias = txtAliasName.Text;
            ObjVendor.OpnDebit = Convert.ToDecimal(txtOpenDebit.Text);
            ObjVendor.OpnCredit = Convert.ToDecimal(txtOpenCredit.Text);
            ObjVendor.Description = "";
            ObjVendor.Notes = "";
            

            ObjVendAddress.Email = txtEmail.Text;
            ObjVendAddress.Phone = txtPhone.Text;
            ObjVendAddress.LedgerId = LedgerId;

            ObjVendor._Operation ="Update";
            ObjRepoVendor.UpdateVendorAddress(ObjVendor, ObjVendAddress);


        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            ClsVendors ObjVendor = new ClsVendors();
            VendorAddress ObjVendorAddress = new VendorAddress();
            VendorRepository RepoVendor = new VendorRepository();

            ObjVendor.CompanyId = SessionContext.CompanyId;
            ObjVendor.BranchId = SessionContext.BranchId;
            ObjVendor.LedgerId = 0;
            ObjVendor.LedgerName = txtVendor.Text;
            ObjVendor.Alias = txtAliasName.Text;
            ObjVendor.Description = "This Vendor Description";
            ObjVendor.Notes = "This Vendor Notes";
           
            ObjVendor.OpnDebit = Convert.ToDecimal(txtOpenDebit.Text);
            ObjVendor.OpnCredit = Convert.ToDecimal(txtOpenCredit.Text);
            ObjVendor._Operation = "GENERATELEDGER";

            ObjVendorAddress.Phone = txtPhone.Text;
            ObjVendorAddress.Email = txtEmail.Text;

            RepoVendor.SaveVendor(ObjVendor, ObjVendorAddress);

        }
    }
}
//21-08-24 - 03:27