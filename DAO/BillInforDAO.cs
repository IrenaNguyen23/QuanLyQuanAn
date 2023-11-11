using QuanLyCafe.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCafe.DAO
{
    public class BillInforDAO
    {
        private static BillInforDAO instance;

        public static BillInforDAO Instance
        {
            get { if (instance == null) instance = new BillInforDAO(); return BillInforDAO.instance; }
            private set { BillInforDAO.instance = value; }
        }

        private BillInforDAO() { }

        public void DeleteBillInfoByFoodId(int id)
        {
            DataProvider.Instance.ExecuteQuery("delete BillInfor where idFood = " + id);
        }
        public List<BillInfor> GetListBillInfor(int id)
        {
            List<BillInfor> listBillInfor = new List<BillInfor>();

            DataTable dataTable = DataProvider.Instance.ExecuteQuery("select * from BillInfor where idBill = " + id);
            
            foreach (DataRow row in dataTable.Rows) 
            {
                BillInfor infor = new BillInfor(row);    
                listBillInfor.Add(infor);
            }
            return listBillInfor;
        }

        public void InsertBillInfor(int idBill, int idFood, int count)
        {
            DataProvider.Instance.ExecuteNonQuery("exec USP_InsertBillInfor @idBill , @idFood , @count", new object[]{ idBill, idFood, count });
        }
        
    }
}
