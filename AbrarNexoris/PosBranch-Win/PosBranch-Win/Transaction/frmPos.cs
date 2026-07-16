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
            this.GetImage();
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BorderStyle = BorderStyle.FixedSingle;

            this.LoadGrid();

        }

        private void GetImage()
        {
            ItemPictureGrid Item = Operation.GetItemPicture();
            if (Item.list.Count() > 0)
            {
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
      

                    for (int i = 0; Item.list.Count() > i; i++){
                        if(item.Photo == null)
                        {
                            
                        }
                        if (item.Photo != null && item.Photo[0] != 0)
                        {
                                if (item.Photo.Length > 2 || item.Photo[0] != 0xFF)
                                {
                                    MemoryStream ms = new MemoryStream(item.Photo);
                                    Bitmap bit = new Bitmap(ms);
                                    pic.BackgroundImage = bit;
                                    pic.Controls.Add(ItemName);
                                    flowLayoutPanel1.Controls.Add(pic);
                                }                          
                        }
                    }
                   


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

            colSlNo.DataPropertyName = "ItemName";
            colSlNo.HeaderText = "ItemName";
            colSlNo.Name = "ItemName";
            colSlNo.ReadOnly = false;
            colSlNo.Visible = true;
            colSlNo.Width = 50;
            colSlNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSlNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colItemName);


            colSlNo.DataPropertyName = "Unit";
            colSlNo.HeaderText = "Unit";
            colSlNo.Name = "Unit";
            colSlNo.ReadOnly = false;
            colSlNo.Visible = true;
            colSlNo.Width = 50;
            colSlNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSlNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colUnit);

            colSlNo.DataPropertyName = "Qty";
            colSlNo.HeaderText = "Qty";
            colSlNo.Name = "Qty";
            colSlNo.ReadOnly = false;
            colSlNo.Visible = true;
            colSlNo.Width = 50;
            colSlNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSlNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colQty);

            colSlNo.DataPropertyName = "Price";
            colSlNo.HeaderText = "Price";
            colSlNo.Name = "Price";
            colSlNo.ReadOnly = false;
            colSlNo.Visible = true;
            colSlNo.Width = 50;
            colSlNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSlNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colPrice);

            colSlNo.DataPropertyName = "Total";
            colSlNo.HeaderText = "Total";
            colSlNo.Name = "Total";
            colSlNo.ReadOnly = false;
            colSlNo.Visible = true;
            colSlNo.Width = 50;
            colSlNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSlNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItemsPos.Columns.Add(colTotal);



        }
    }
}
