using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCafe.DTO
{
    public class BillInfor
    {

        public BillInfor(int id, int billID, int foodID, int count)
        {
            this.iD = id;
            this.BillID = billID;
            this.FoodID = foodID;
            this.Count = count;
        }

        public BillInfor(DataRow row)
        {
            this.iD = (int)row["id"];
            this.BillID = (int)row["idBill"];
            this.FoodID = (int)row["idFood"];
            this.Count = (int)row["count"];
        }
        private int iD;

        private int billId;

        private int foodId;

        private int count;

        public int ID
        {
            get { return iD; }
            set { iD = value; }
        }
        public int BillID
        {
            get { return billId; }
            set { billId = value; }
        }
        public int FoodID
        {
            get { return foodId; }
            set { foodId = value; }
        }
        public int Count
        { 
            get { return count; } 
            set { count = value; } 
        }

    }
}
