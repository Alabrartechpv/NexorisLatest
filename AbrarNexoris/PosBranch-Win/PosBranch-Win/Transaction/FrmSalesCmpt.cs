using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class FrmSalesCmpt : Form
    {
        //frmSalesInvoice frm = new frmSalesInvoice();

        public FrmSalesCmpt(string netAmount)
        {
            InitializeComponent();
            txtTotal.ReadOnly = true;

            txtTotal.Text = netAmount;//frm.txtNetTotal.Text;
            TxtPayedAmt.Text = netAmount; //frm.txtNetTotal.Text;
            txtBalance.Text = Convert.ToString(0);
        }

        private void FrmSalesCmpt_Load(object sender, EventArgs e)
        {
            

        }

        private void TxtPayedAmt_TextChanged(object sender, EventArgs e)
        {
            if(TxtPayedAmt.Text != "")
            {
                int numb;
                bool amt = int.TryParse(TxtPayedAmt.Text, out numb);
               if (amt == true)
                {
                    float payment = float.Parse(TxtPayedAmt.Text);
                    float Total = float.Parse(txtTotal.Text);
                    float Balance = payment - Total;
                    txtBalance.Text = Balance.ToString();
                }
            }
            
             
        }

        private void ultraButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
