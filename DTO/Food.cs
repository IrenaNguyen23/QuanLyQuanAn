using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCafe.DTO
{
    public class Food
    {

        public Food(int id, string name, int categoryId, float price)
        {
            this.ID = id;
            this.Name = name;
            this.CategoryId = categoryId;
            this.Price = price;
        }

        public Food(DataRow row)
        {
            this.ID = (int)row["id"];
            this.Name = row["name"].ToString();
            this.CategoryId = (int)row["idCategory"];
            this.Price = (float)Convert.ToDouble(row["price"].ToString());
        }
        private int iD;

        public int ID
        {
            get { return iD; }
            set { iD = value; }
        }

        private string name;

        public string Name
        { get { return name; } set {  name = value; } }

        private int categoryId;

        public int CategoryId
        { get { return categoryId; } set { categoryId = value; } }

        private float price;
        public float Price
        { get { return price; } set {  price = value; } }
    }
}
