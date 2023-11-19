using QuanLyCafe.DAO;
using QuanLyCafe.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyCafe
{
    public partial class TableManager : Form
    {
        private Account loginAccount;

        public Account LoginAccount 
        { 
            get { return loginAccount; } 
            set { loginAccount = value; ChangeAccount(loginAccount.Type); } 
        }

        public TableManager(Account acc)
        {
            InitializeComponent();

            this.LoginAccount = acc;

            LoadTable();
            LoadCategory();
            LoadComboxTable(cbSwtich);
            LoadComboxTableGroup(cbGroupTable);
        }
        #region Method

        void ChangeAccount(int type)
        {
            adminToolStripMenuItem.Enabled = type == 1;
            thôngTinCáNhânToolStripMenuItem.Text += "(" + LoginAccount.DisplayName + ")";
        }
        void LoadCategory()
        {
            List<Category> listCategory = CategoryDAO.Instance.GetListCategory();
            cbCategory.DataSource = listCategory;
            cbCategory.DisplayMember = "Name";
        }
        void LoadFoodByCategoryId(int id)
        {
            List<Food> listFood = FoodDAO.Instance.GetFoodByCategoryId(id);
            cbFood.DataSource = listFood;
            cbFood.DisplayMember = "Name";
        }
        void LoadTable()
        {
            flpTable.Controls.Clear();

            List<Table> tableList = TableDAO.Instance.LoadTableList();

            foreach (Table item in tableList)
            {
                Button btn = new Button() { Width = TableDAO.TableWidth, Height = TableDAO.TableHeight};
                btn.Text = item.Name + Environment.NewLine + item.Status;
                btn.Click += Btn_Click;
                btn.Tag = item;

                switch(item.Status)
                {
                    case "Trống":
                        btn.BackColor = Color.LightGreen;
                        break;
                    default: btn.BackColor = Color.Pink; break;
                }
                flpTable.Controls.Add(btn);
            }
        }
        void ShowBill(int id)
        {
            lsvBill.Items.Clear();
            List<QuanLyCafe.DTO.Menu> listBillInfor = MenuDAO.Instance.GetListMenuByTable(id);
            float totalPrice = 0;

            foreach (QuanLyCafe.DTO.Menu item in listBillInfor)
            {
                ListViewItem listViewItem = new ListViewItem(item.FoodName.ToString());

                listViewItem.SubItems.Add(item.Count.ToString());
                listViewItem.SubItems.Add(item.Price.ToString());
                listViewItem.SubItems.Add(item.TotalPrice.ToString());
                totalPrice += item.TotalPrice;
                lsvBill.Items.Add(listViewItem);
            }
            CultureInfo culture = new CultureInfo("vi-VN");
            txtTotalPrice.Text = totalPrice.ToString("c", culture);
        
        }

        void LoadComboxTable(ComboBox cb)
        {
            cb.DataSource = TableDAO.Instance.LoadTableList();
            cb.DisplayMember = "Name";
        }
        void LoadComboxTableGroup(ComboBox cb)
        {
            cb.DataSource = TableDAO.Instance.LoadTableList();
            cb.DisplayMember = "Name";
        }
        #endregion

        #region Event

        private void Btn_Click(object sender, EventArgs e)
        {
            int tableID = ((sender as Button).Tag as Table).ID;
            lsvBill.Tag = (sender as Button).Tag;
            ShowBill(tableID);
        }
        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();   
        }


        void f_UpdateAccount(object sender, AccountEvent e)
        {
            thôngTinTàiToolStripMenuItem.Text = "Thông tin tài khoản (" + e.Acc.DisplayName + ")";
            thôngTinCáNhânToolStripMenuItem.Text = "Thông tin cá nhân (" + e.Acc.DisplayName + ")";
        }

        private void thôngTinCáNhânToolStripMenuItem_Click(object sender, EventArgs e)
        {
           AccountProfile f = new AccountProfile(loginAccount);
            f.UpdateAccount += f_UpdateAccount;
            f.ShowDialog();
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Admin f = new Admin();
            f.loginAccount = LoginAccount;
            f.InsertFood += F_InsertFood;
            f.DeleteFood += F_DeleteFood;
            f.UpdateFood += F_UpdateFood;
            f.InsertCategory += F_InsertCategory;
            f.DeleteCategory += F_DeleteCategory;
            f.UpdateCategory += F_UpdateCategory;
            f.ShowDialog();
        }

        private void F_UpdateCategory(object sender, EventArgs e)
        {
            
        }

        private void F_DeleteCategory(object sender, EventArgs e)
        {
            
        }

        private void F_InsertCategory(object sender, EventArgs e)
        {
            

        }

        private void F_UpdateFood(object sender, EventArgs e)
        {
            LoadFoodByCategoryId((cbCategory.SelectedItem as Category).ID);
            if (lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Table).ID);
        }

        private void F_DeleteFood(object sender, EventArgs e)
        {
            LoadFoodByCategoryId((cbCategory.SelectedItem as Category).ID);
            if (lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Table).ID);
            LoadTable();
        }

        private void F_InsertFood(object sender, EventArgs e)
        {
            LoadFoodByCategoryId((cbCategory.SelectedItem as Category).ID);
            if (lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Table).ID);
        }

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = 0;
            ComboBox cb = sender as ComboBox;

            if (cb.SelectedItem == null) 
                return;
            Category selected = cb.SelectedItem as Category;
            id = selected.ID;
            LoadFoodByCategoryId(id);
        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {
            Table table = lsvBill.Tag as Table;

            if (table == null)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi thêm món ăn", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                int idBill = BillDAO.Instance.GetUnCheckBillIdByTableId(table.ID);
                int idFood = (cbFood.SelectedItem as Food).ID;
                int count = (int)nmFoodCount.Value;

                if (idBill == -1)
                {
                    BillDAO.Instance.InsertBill(table.ID);
                    BillInforDAO.Instance.InsertBillInfor(BillDAO.Instance.GetMaxIDBill(), idFood, count);
                }
                else
                {
                    BillInforDAO.Instance.InsertBillInfor(idBill, idFood, count);
                }
                ShowBill(table.ID);

                LoadTable();
            }
        }        

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            Table table = lsvBill.Tag as Table;

            int idBill = BillDAO.Instance.GetUnCheckBillIdByTableId(table.ID);
            int discount = (int)nmDiscount.Value;

            //double totalPrice = Convert.ToDouble(txtTotalPrice.Text.Split(',')[0]);
            string totalPriceString = txtTotalPrice.Text;
            double totalPrice;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("vi-VN");

            // Xử lý chuyển đổi chuỗi thành số double
            if (double.TryParse(totalPriceString, NumberStyles.Currency, culture, out totalPrice))
            {
                // Khi chuyển đổi thành công
                // Sử dụng giá trị totalPrice ở đây
            }
            else
            {
                // Khi chuyển đổi không thành công
                // Xử lý lỗi hoặc hiển thị thông báo cho người dùng
            }
            double finalTotalPrice = totalPrice - (totalPrice / 100) * discount;

            if(totalPrice != 0)
            {
                if (idBill != -1)
                {
                    if (MessageBox.Show(string.Format("Bạn có chắc thanh toán hóa đơn cho {0}\nTổng tiền - (Tổng tiền / 100) x Giảm giá\n=> {1} - ({1} / 100) x {2} = {3}", table.Name, totalPrice, discount, finalTotalPrice), "Thông báo", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                    {
                        BillDAO.Instance.CheckOut(idBill, discount, (float)finalTotalPrice);
                        ShowBill(table.ID);

                        LoadTable();
                    }
                }
            }
            else
            {
                MessageBox.Show("Bàn trống không có gì để thanh toán");
            }
        }

        private void btnSwitchTable_Click(object sender, EventArgs e)
        {
            int id1 = (lsvBill.Tag as Table).ID;

            int id2 = (cbSwtich.SelectedItem as Table).ID;
            if(MessageBox.Show(string.Format("Bạn có muốn chuyển {0} qua {1}", (lsvBill.Tag as Table).Name, (cbSwtich.SelectedItem as Table).Name),"Thông báo",(MessageBoxButtons.OKCancel)) == System.Windows.Forms.DialogResult.OK)
            {

                TableDAO.Instance.SwitchTable(id1, id2);

                LoadTable();
            }

        }


        private void thêmMónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnAddFood_Click(this, new EventArgs());
        }

        private void thanhToánToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            btnCheckOut_Click(this, new EventArgs());
        }
        #endregion

        private void btnGroupTable_Click(object sender, EventArgs e)
        {
            int id1 = (lsvBill.Tag as Table).ID;

            int id2 = (cbGroupTable.SelectedItem as Table).ID;
            if (MessageBox.Show(string.Format("Bạn có muốn gộp {0} qua {1}", (lsvBill.Tag as Table).Name, (cbGroupTable.SelectedItem as Table).Name), "Thông báo", (MessageBoxButtons.OKCancel)) == System.Windows.Forms.DialogResult.OK)
            {

                TableDAO.Instance.MergeTable(id1, id2);

                LoadTable();
            }
        }
    }
}
