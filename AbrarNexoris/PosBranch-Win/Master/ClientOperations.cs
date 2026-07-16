using Dapper;
using ModelClass;
using ModelClass.Master;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Repository
{
   public class ClientOperations : BaseRepostitory
    {
        public string SaveSales(SalesMaster sales,SalesDetails salesDetails, DataGridView dgvItems)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                sales._Operation = "GENERATENUMBER";
                sales.FinYearId = SessionContext.FinYearId;
                 List<SalesMaster> getBillNO = DataConnection.Query<SalesMaster>(STOREDPROCEDURE._POS_Sales_Win, sales, trans,
                 commandType: CommandType.StoredProcedure).ToList<SalesMaster>();
                if (getBillNO.Count > 0)
                {
                    foreach (SalesMaster master in getBillNO)
                    {
                        sales.BillNo = master.BillNo;
                    }
                }
                sales._Operation = "CREATE";

                List<SalesMaster> listSales = DataConnection.Query<SalesMaster>(STOREDPROCEDURE._POS_Sales_Win,sales, trans,
                commandType: CommandType.StoredProcedure).ToList<SalesMaster>();
                salesDetails._Operation = "CREATE";
                for (int i = 0; i < dgvItems.RowCount; i++)
                {
                    salesDetails.ItemId = Convert.ToInt32(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.Barcode = dgvItems.Rows[i].Cells["BarCode"].Value.ToString();
                    salesDetails.CompanyId = SessionContext.CompanyId;
                    salesDetails.BranchID = SessionContext.BranchId;
                    salesDetails.FinYearId = SessionContext.FinYearId;
                    salesDetails.BillDate = DateTime.Now;
                    salesDetails.ItemName = dgvItems.Rows[i].Cells["Description"].Value.ToString();
                    salesDetails.SlNO = Convert.ToInt32(dgvItems.Rows[i].Cells["SlNo"].Value.ToString());
                    salesDetails.Unit = dgvItems.Rows[i].Cells["Unit"].Value.ToString();
                    salesDetails.UnitPrice = float.Parse(dgvItems.Rows[i].Cells["S/Price"].Value.ToString());
                    salesDetails.Cost = float.Parse(dgvItems.Rows[i].Cells["Cost"].Value.ToString());
                    salesDetails.DiscountAmount = float.Parse(dgvItems.Rows[i].Cells["DiscAmt"].Value.ToString());
                    salesDetails.DiscountPer = float.Parse(dgvItems.Rows[i].Cells["DiscPer"].Value.ToString());
                    salesDetails.Expiry = DateTime.Now;
                    salesDetails.MarginAmt = float.Parse(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.MarginPer = float.Parse(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.MRP = 0;
                    salesDetails.Packing = 1;
                    salesDetails.Qty = Convert.ToInt32(dgvItems.Rows[i].Cells["Qty"].Value.ToString());
                    salesDetails.TotalAmount = float.Parse(dgvItems.Rows[i].Cells["AmountWithTax"].Value.ToString());
                    salesDetails.BaseUnit = "";
                    salesDetails.Amount = 0;
                    salesDetails.VoucherId = 0;
                    salesDetails.CounterId = 0;
                    salesDetails.UnitId = 1;
                    salesDetails.TotalAmount = 0;
                    List<SalesDetails> ListSalesDetails = DataConnection.Query<SalesDetails>(STOREDPROCEDURE._POS_SDetails_Win, salesDetails, trans,
                    commandType: CommandType.StoredProcedure).ToList<SalesDetails>();
                }
                

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw;
            }
            return "Success";
        }



        public string SaveCategory(Category cat)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<Category> listcategory = DataConnection.Query<Category>(STOREDPROCEDURE.POS_Category, cat, trans,
                    commandType: CommandType.StoredProcedure).ToList<Category>();
                if (listcategory.Count > 0)
                {

                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw;
            }
            return "Success";
        }




       

        public Category GetByIdCategory(int selectedId)
        {
            Category item = new Category();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Category, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", selectedId);
                    cmd.Parameters.AddWithValue("@CategoryName", item.CategoryName);
                    cmd.Parameters.AddWithValue("@GroupId", item.GroupId);
                    cmd.Parameters.AddWithValue("@Photo", item.Photo);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Category>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public Category UpdateCategory(Category ct)
        {
            Category catitem = new Category();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Category, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", ct.Id);
                    cmd.Parameters.AddWithValue("@CategoryName", ct.CategoryName);
                    cmd.Parameters.AddWithValue("@GroupId", ct.GroupId);
                    cmd.Parameters.AddWithValue("@Photo", ct.Photo);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            catitem = ds.Tables[0].Rows[0].ToNullableObject<Category>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return catitem;
        }

        public string saveItem(Item itm, ItemMasterPriceSettings itmprice, DataGridView dgv_pricetab, DataGridView dgv_uomtab)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<Item> listitem = DataConnection.Query<Item>(STOREDPROCEDURE.POS_ItemMaster, itm, trans,
                    commandType: CommandType.StoredProcedure).ToList<Item>();
                if (listitem.Count > 0)
                {
                    foreach (Item master in listitem)
                    {
                        itmprice.ItemId = master.ItemId;
                    }
                }

                for (int i = 0; i < dgv_uomtab.Rows.Count; i++)
                {
                    itmprice.Unit = dgv_uomtab.Rows[i].Cells["Unit"].Value.ToString();
                    itmprice.Packing = float.Parse(dgv_uomtab.Rows[i].Cells["Packing"].Value.ToString());
                    itmprice.BarCode = dgv_uomtab.Rows[i].Cells["BarCode"].Value.ToString();
                    itmprice.ReOrder = float.Parse(dgv_uomtab.Rows[i].Cells["ReOrder"].Value.ToString());
                    itmprice.OpnStk = float.Parse(dgv_uomtab.Rows[i].Cells["OpeningStock"].Value.ToString());

                    itmprice.MarginAmt = float.Parse(dgv_pricetab.Rows[i].Cells["Margin"].Value.ToString());
                    itmprice.RetailPrice = float.Parse(dgv_pricetab.Rows[i].Cells["RetailPrice"].Value.ToString());
                    itmprice.WholeSalePrice = float.Parse(dgv_pricetab.Rows[i].Cells["WholesalePrice"].Value.ToString());
                    itmprice.CreditPrice = float.Parse(dgv_pricetab.Rows[i].Cells["CreditPrice"].Value.ToString());
                    itmprice.CardPrice = float.Parse(dgv_pricetab.Rows[i].Cells["CardPrice"].Value.ToString());

                    List<ItemMasterPriceSettings> listitemprice = DataConnection.Query<ItemMasterPriceSettings>(STOREDPROCEDURE.POS_ItemMasterPriceSettings, itmprice, trans,
                    commandType: CommandType.StoredProcedure).ToList<ItemMasterPriceSettings>();
                    if (listitemprice.Count > 0)
                    {
                        trans.Commit();
                    }
                }
                //if (listitem.Count > 0)
                //{
                //    trans.Commit();
                //}
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return "success";
        }

        public Item UpdateItem(Item item1)
        {
            Item itemup = new Item();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemId", item1.ItemId);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", 0);
                    cmd.Parameters.AddWithValue("@ItemNo", item1.ItemNo);
                    cmd.Parameters.AddWithValue("@Description", item1.Description);
                    cmd.Parameters.AddWithValue("@NameInLocalLanguage", item1.NameInLocalLanguage);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@ItemTypeId", item1.ItemTypeId);
                    cmd.Parameters.AddWithValue("@ForCustomerType", item1.ForCustomerType);
                    cmd.Parameters.AddWithValue("@BaseUnitId", item1.BaseUnitId);
                    cmd.Parameters.AddWithValue("@CategoryId", item1.CategoryId);
                    cmd.Parameters.AddWithValue("@GroupId", item1.GroupId);
                    cmd.Parameters.AddWithValue("@VendorId", 0);
                    cmd.Parameters.AddWithValue("@BrandId", item1.BrandId);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            itemup = ds.Tables[0].Rows[0].ToNullableObject<Item>();
                            string s = ds.Tables[0].Rows[0].ItemArray[0].ToString();
                            if (s == "Success")
                            {
                                DataBase.Operations = "UPDATE";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return itemup;
        }

        public string savePriceSettings(ItemMasterPriceSettings iprice)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<ItemMasterPriceSettings> listitem = DataConnection.Query<ItemMasterPriceSettings>(STOREDPROCEDURE.POS_ItemMasterPriceSettings, iprice, trans,
                    commandType: CommandType.StoredProcedure).ToList<ItemMasterPriceSettings>();
                if (listitem.Count > 0)
                {
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return "success";
        }

        public ItemMasterPriceSettings UpdatePriceSettings(ItemMasterPriceSettings price1)
        {
            ItemMasterPriceSettings pricesettings = new ItemMasterPriceSettings();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterPriceSettings, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemId", price1.ItemId);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", 0);
                    cmd.Parameters.AddWithValue("@Unit", price1.Unit);
                    cmd.Parameters.AddWithValue("@UnitId", price1.UnitId);
                    cmd.Parameters.AddWithValue("@Packing", price1.Packing);
                    cmd.Parameters.AddWithValue("@Cost", price1.Cost);
                    cmd.Parameters.AddWithValue("@MarginPer", price1.MarginPer);
                    cmd.Parameters.AddWithValue("@MarginAmt", price1.MarginAmt);
                    cmd.Parameters.AddWithValue("@RetailPrice", price1.RetailPrice);
                    cmd.Parameters.AddWithValue("@WholeSalePrice", price1.WholeSalePrice);
                    cmd.Parameters.AddWithValue("@CreditPrice", price1.CreditPrice);
                    cmd.Parameters.AddWithValue("@CardPrice", price1.CardPrice);
                    cmd.Parameters.AddWithValue("@MRP", price1.MRP);
                    cmd.Parameters.AddWithValue("@Stock", price1.Stock);
                    cmd.Parameters.AddWithValue("@OrderedStock", price1.OrderedStock);
                    cmd.Parameters.AddWithValue("@StockValue", price1.StockValue);
                    cmd.Parameters.AddWithValue("@ReOrder", price1.ReOrder);
                    cmd.Parameters.AddWithValue("@BarCode", price1.BarCode);
                    cmd.Parameters.AddWithValue("@OpnStk", price1.OpnStk);
                    cmd.Parameters.AddWithValue("@Photo", price1.Photo);
                    cmd.Parameters.AddWithValue("@_Operation", DataBase.Operations);
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            pricesettings = ds.Tables[0].Rows[0].ToNullableObject<ItemMasterPriceSettings>();
                            string s = ds.Tables[0].Rows[0].ItemArray[0].ToString();
                            if (s == "Success")
                            {
                                DataBase.Operations = "UPDATE";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return pricesettings;
        }

        public Item GetByIdItem(int selectedId)
        {
            Item item = new Item();
            ItemMasterPriceSettings itemp = new ItemMasterPriceSettings();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemId", selectedId);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", 0);
                    cmd.Parameters.AddWithValue("@ItemNo", 0);
                    cmd.Parameters.AddWithValue("@Description", "");
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@ItemTypeId", 0);
                    cmd.Parameters.AddWithValue("@VendorId", 0);
                    cmd.Parameters.AddWithValue("@BrandId", 0);
                    cmd.Parameters.AddWithValue("@GroupId", 0);
                    cmd.Parameters.AddWithValue("@CategoryId", 0);
                    cmd.Parameters.AddWithValue("@BaseUnitId", 0);
                    cmd.Parameters.AddWithValue("@ForCustomerType", "");
                    cmd.Parameters.AddWithValue("@NameInLocalLanguage", "");
                    cmd.Parameters.AddWithValue("@HSNCode", "");
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Item>();
                        }
                        //if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[1] != null) && (ds.Tables[0].Rows.Count > 0))
                        //{
                        //    itemp = ds.Tables[1].Rows[0].ToNullableObject<ItemMasterPriceSettings>();
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public ItemMasterPriceSettings GetByIdItemPrice(int selectedId)
        {
            ItemMasterPriceSettings itemp = new ItemMasterPriceSettings();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterPriceSettings, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemId", selectedId);
                    cmd.Parameters.AddWithValue("@Cost", "");
                    cmd.Parameters.AddWithValue("@OrderedStock", "");
                    cmd.Parameters.AddWithValue("@StockValue", "");
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            itemp = ds.Tables[0].Rows[0].ToNullableObject<ItemMasterPriceSettings>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return itemp;
        }

        //public Item UpdateItem(Item itmm)
        //{
        //    Item objitem = new Item();

        //    DataConnection.Open();

        //    try
        //    {
        //        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMaster, (SqlConnection)DataConnection))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@ItemId", itmm.ItemId);
        //            cmd.Parameters.AddWithValue("@Description", itmm.Description);
        //            cmd.Parameters.AddWithValue("@NameInLocalLanguage", itmm.NameInLocalLanguage);
        //            cmd.Parameters.AddWithValue("@ItemTypeId", itmm.ItemTypeId);
        //            cmd.Parameters.AddWithValue("@ForCustomerType", itmm.ForCustomerType);
        //            cmd.Parameters.AddWithValue("@BaseUnitId", itmm.BaseUnitId);
        //            cmd.Parameters.AddWithValue("@GroupId", itmm.GroupId);
        //            cmd.Parameters.AddWithValue("@CategoryId", itmm.CategoryId);
        //            cmd.Parameters.AddWithValue("@BrandId", itmm.BrandId);
        //            //cmd.Parameters.AddWithValue("@GroupId", itmm.GroupId);
        //            //cmd.Parameters.AddWithValue("@CategoryId", itmm.CategoryId);
        //            //cmd.Parameters.AddWithValue("@BrandId", itmm.BrandId);
        //            using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
        //            {
        //                DataSet ds = new DataSet();
        //                adapt.Fill(ds);
        //                if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
        //                {
        //                    objitem = ds.Tables[0].Rows[0].ToNullableObject<Item>();

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (DataConnection.State == ConnectionState.Open)
        //            DataConnection.Close();
        //    }
        //    return objitem;

        //}


        public string SaveBrand(Brand brand)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<Brand> listBrand = DataConnection.Query<Brand>(STOREDPROCEDURE.POS_Brand, brand, trans, commandType: CommandType.StoredProcedure).ToList<Brand>();
                if (listBrand.Count > 0)
                {
                    trans.Commit();
                }
                else
                {
                    trans.Rollback();

                }

            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return "Success";
        }




        public Brand GetBrandId(Brand bd)
        {
            Brand brnd = new Brand();
            try
            {
                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Brand, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", bd.Id);
                    cmd.Parameters.AddWithValue("@BrandName", bd.BrandName);
                    cmd.Parameters.AddWithValue("@Photo", bd.Photo);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            brnd = ds.Tables[0].Rows[0].ToNullableObject<Brand>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return brnd;
        }

        public Brand UpdateBrand(Brand brnd)
        {
            Brand brnds = new Brand();
            try
            {
                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Brand, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", brnd.Id);
                    cmd.Parameters.AddWithValue("@BrandName", brnd.BrandName);
                    cmd.Parameters.AddWithValue("@Photo", brnd.Photo);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "Update");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            brnds = ds.Tables[0].Rows[0].ToNullableObject<Brand>();

                        }
                    }

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

            }
            return brnds;
        }

        public Brand DeleteBrand(Brand brd)
        {
            Brand brnd = new Brand();
            try
            {
                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Brand, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", brd.Id);
                    cmd.Parameters.AddWithValue("@_Operation", "Delete");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            brnd = ds.Tables[0].Rows[0].ToNullableObject<Brand>();
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

            }
            return brnd;
        }

        public BrandDDLGrid BrandSearch(Brand brnd)
        {
            BrandDDLGrid bddlg = new BrandDDLGrid();
            try
            {
                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Brand, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BrandName", brnd.BrandName);
                    cmd.Parameters.AddWithValue("@_Operation", "Search");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables[0] != null) && (ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0))
                        {
                            bddlg.List = ds.Tables[0].ToListOfObject<BrandDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return bddlg;
        }


    }
}

