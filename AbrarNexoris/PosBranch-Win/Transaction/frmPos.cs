using ModelClass;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmPos : Form
    {
        SalesRepository Operation = new SalesRepository();


        DataGridViewTextBoxColumn colSlNo = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colItemId = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colItemName = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnit = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colQty = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colTotal   = new DataGridViewTextBoxColumn();





        public frmPos()
        {
            InitializeComponent();
        }

        private void frmPos_Load(object sender, EventArgs e)
        {
            this.GetImage(null);
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BorderStyle = BorderStyle.FixedSingle;

            this.LoadGrid();

        }

        private void GetImage(string Barcode)
        {
            ItemPictureGrid Item = Operation.GetItemPicture(Barcode);
            if (Item.list.Count() > 0 || Item.list != null)
            {
                if(flowLayoutPanel1.Controls.Count > 0)
                {
                    flowLayoutPanel1.Controls.Clear();
                }

                foreach (var item in Item.list)
                {
                    PictureBox pic = new PictureBox();
                    pic.Width = 100;
                    pic.Height = 100;
                    pic.BackgroundImageLayout = ImageLayout.Stretch;

                    Label ItemName = new Label();
                    ItemName.Text = item.ItemName;
                    ItemName.BackColor = Color.FromArgb(239, 106, 77);
                    ItemName.Dock = DockStyle.Bottom;

                    Label ItemId = new Label();
                    ItemId.Text = item.ItemId.ToString();
                    ItemId.BackColor = Color.FromArgb(239, 106, 77);
                    ItemId.Dock = DockStyle.Bottom;
                    ItemId.Visible = false;

                    Label UNit = new Label();
                    UNit.Text = item.Unit;
                    UNit.BackColor = Color.FromArgb(239, 106, 77);
                    UNit.Dock = DockStyle.Bottom;
                    UNit.Visible = Visible;

                    for (int i = 0; Item.list.Count() > i; i++){
                        Button btn = new Button
                        {
                            Text = item.ItemName + item.Unit,
                            Width = 80,
                            Height = 80,
                            Location = new System.Drawing.Point(10, 10 + (i * 35)), // Positioning buttons vertically
                            Name = item.ItemName,                         
                            
                        };
                        btn.Click += GetItemId;
                        this.Controls.Add(btn);
                        flowLayoutPanel1.Controls.Add(btn);
                        flowLayoutPanel1.Controls.Add(UNit);
                        flowLayoutPanel1.Controls.Add(ItemId);
                        //this.Controls.Add(flowLayoutPanel1);

                    }
                   


                }
            }
        }

        private void GetItemId(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            Label clickLabel = sender as Label;
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is Label label)
                {
                    // Get the text of the label
                    string labelText = label.Text;
                   // MessageBox.Show(labelText); // Display the label text
                }
            }

        }

        private void LoadGrid()
        {
            dgvItemsPos.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvItemsPos.Columns.Clear();
            dgvItemsPos.AllowUserToOrderColumns = true;
            dgvItemsPos.AllowUserToDeleteRows = false;
            dgvItemsPos.AllowUserToAddRows = false;
            dgvItemsPos.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgvItemsPos.AutoGenerateColumns = false;
            dgvItemsPos.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgvItemsPos.ColumnHeadersHeight = 40;
            dgvItemsPos.MultiSelect = false;
            dgvItemsPos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colSlNo.DataPropertyName = "SlNO";
            colSlNo.HeaderText = "SlNO";
            colSlNo.Name = "SlNO";
            colSlNo.ReadOnly = false;
            colSlNo.Visible = true;
            colSlNo.Width = 50;
            colSlNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSlNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colSlNo);

            colItemName.DataPropertyName = "ItemName";
            colItemName.HeaderText = "ItemName";
            colItemName.Name = "ItemName";
            colItemName.ReadOnly = false;
            colItemName.Visible = true;
            colItemName.Width = 250;
            colItemName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colItemName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colItemName);


            colUnit.DataPropertyName = "Unit";
            colUnit.HeaderText = "Unit";
            colUnit.Name = "Unit";
            colUnit.ReadOnly = false;
            colUnit.Visible = true;
            colUnit.Width = 100;
            colUnit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colUnit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colUnit);

            colQty.DataPropertyName = "Qty";
            colQty.HeaderText = "Qty";
            colQty.Name = "Qty";
            colQty.ReadOnly = false;
            colQty.Visible = true;
            colQty.Width = 80;
            colQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colQty);

            colPrice.DataPropertyName = "Price";
            colPrice.HeaderText = "Price";
            colPrice.Name = "Price";
            colPrice.ReadOnly = false;
            colPrice.Visible = true;
            colPrice.Width = 80;
            colPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colPrice);

            colTotal.DataPropertyName = "Total";
            colTotal.HeaderText = "Total";
            colTotal.Name = "Total";
            colTotal.ReadOnly = false;
            colTotal.Visible = true;
            colTotal.Width = 100;
            colTotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colTotal);



        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 3)
            {
                this.GetImage(textBox1.Text);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if(textBox1.Text.Length > 3)
                {
                    this.GetImage(textBox1.Text);
                }
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Click(object sender, EventArgs e)
        {
        foreach(Control  control in flowLayoutPanel1.Controls)
            {
                if(control is Label lbl)
                {
                   string Itemid = lbl.Text;
                }
            }
        }

        private void Control_Click(object sender, EventArgs e)
        {
           // MessageBox.Show("sdjfgdj");
        }

        private void flowLayoutPanel1_Enter(object sender, EventArgs e)
        {
           // MessageBox.Show("sdfgjsd");
        }
    }
}
