using QuanLyCafe.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCafe.DAO
{
    public class MenuDAO
    {
        private static MenuDAO instance;

        public static MenuDAO Instance
        {
            get { if (instance == null) instance = new MenuDAO(); return MenuDAO.instance; }
            private set { MenuDAO.instance = value; }
        }

        private MenuDAO() { }
        
        public List<Menu> GetListMenuByTable(int id)
        {
            List<Menu> list = new List<Menu>();
            string query = "select f.name, bi.count, f.price, f.price*bi.count as total from Bill as b, BillInfor as bi, Food as f \r\nwhere bi.idBill = b.id and bi.idFood = f.id and status = 0 and b.idTable = " + id;

            DataTable data = DataProvider.Instance.ExecuteQuery(query);
            foreach (DataRow row in data.Rows) 
            {
                Menu menu = new Menu(row);
                list.Add(menu);
            }

            return list;
        }
    }
}
