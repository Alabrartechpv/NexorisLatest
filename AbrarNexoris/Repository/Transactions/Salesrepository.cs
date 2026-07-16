using Dapper;
using ModelClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Transactions
{
 public class SalesRepository:BaseRepostitory
    {
        public string SaveSales(SalesMaster sales, SalesDetails sDetails)
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

                List<SalesMaster> listSales = DataConnection.Query<SalesMaster>(STOREDPROCEDURE._POS_Sales_Win, sales, trans,
                commandType: CommandType.StoredProcedure).ToList<SalesMaster>();
                sDetails._Operation = "CREATE";
                List<SalesDetails> ListSalesDetails = DataConnection.Query<SalesDetails>(STOREDPROCEDURE._POS_SDetails_Win, sDetails, trans,
                 commandType: CommandType.StoredProcedure).ToList<SalesDetails>();

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw;
            }
            return "Success";
        }

    }
}
