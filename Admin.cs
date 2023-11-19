using QuanLyCafe.DAO;
using QuanLyCafe.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;

namespace QuanLyCafe
{
    public partial class Admin : Form
    {
        BindingSource foodList = new BindingSource();
        BindingSource accountList = new BindingSource();
        BindingSource categoryList = new BindingSource();
        BindingSource tableList = new BindingSource();
        public Account loginAccount;
        public Admin()
        {
            InitializeComponent();
            Load();
        }

        #region method

        List<Food> SearchFoodByName(string name)
        {
            List<Food> listFood = FoodDAO.Instance.SearchFoodByName(name);

            return listFood;
        }
        void Load()
        {
            dtgvFood.DataSource = foodList;
            dtgvAccount.DataSource = accountList;
            dtgvCategory.DataSource = categoryList;
            dtgvTable.DataSource = tableList;

            LoadDateTimePickerFoodBestSeller();
            LoadListFoodBestSeller(dtpkStarDate.Value, dtpkEndDate.Value);
            LoadDateTimePickerBill();
            LoadListBillByDate(dtpkFromDate.Value, dtpkToDate.Value);
            LoadListFood();
            LoadTable();
            LoadListCateogry();
            AddFoodBinding();
            LoadCategoryIntoComboBox();
            AddAccountBinding();
            AddCategoryBinding();
            AddTableBinding();
            LoadAccount();
        }
        void LoadListBillByDate(DateTime checkIn, DateTime checkOut)
        {
            dtgvBill.DataSource = BillDAO.Instance.GetListBillByDate(checkIn, checkOut);
        }
        void LoadDateTimePickerBill()
        {
            DateTime today = DateTime.Now;
            dtpkFromDate.Value = new DateTime(today.Year,today.Month,1);
            dtpkToDate.Value = dtpkFromDate.Value.AddMonths(1).AddDays(-1);
        }
        void LoadDateTimePickerFoodBestSeller()
        {
            DateTime today = DateTime.Now;
            dtpkStarDate.Value = new DateTime(today.Year, today.Month, 1);
            dtpkEndDate.Value = dtpkFromDate.Value.AddMonths(1).AddDays(-1);
        }

        void LoadListFoodBestSeller(DateTime checkIn, DateTime checkOut)
        {
            dtgvTopFoodBestSeller.DataSource = BillDAO.Instance.GetTopSellingFoodsByDate(checkIn, checkOut);
        }

        void LoadListFood()
        {
            foodList.DataSource = FoodDAO.Instance.GetListFood();
        }
        void AddFoodBinding()
        {
            txbFoodName.DataBindings.Add(new Binding("Text", dtgvFood.DataSource, "name",true,DataSourceUpdateMode.Never));
            txbFoodID.DataBindings.Add(new Binding("Text", dtgvFood.DataSource, "id", true, DataSourceUpdateMode.Never));
            nmPriceFood.DataBindings.Add(new Binding("Value", dtgvFood.DataSource, "price", true, DataSourceUpdateMode.Never));
        }
        void AddAccountBinding()
        {
            txbUserName.DataBindings.Add(new Binding("Text", dtgvAccount.DataSource, "UserName", true, DataSourceUpdateMode.Never));
            txbDisplayName.DataBindings.Add(new Binding("Text", dtgvAccount.DataSource, "DisplayName", true, DataSourceUpdateMode.Never));
            numericUpDown1.DataBindings.Add(new Binding("Value", dtgvAccount.DataSource, "Type", true, DataSourceUpdateMode.Never));
        }
        void LoadAccount()
        {
            accountList.DataSource = AccountDAO.Instance.GetListAccounts();
        }

        void LoadTable()
        {
            tableList.DataSource = TableDAO.Instance.LoadTableList();
        }

        void AddTableBinding()
        {
            txbTableID.DataBindings.Add(new Binding("Text", dtgvTable.DataSource, "id", true, DataSourceUpdateMode.Never));
            txbTableName.DataBindings.Add(new Binding("Text", dtgvTable.DataSource, "name", true, DataSourceUpdateMode.Never));
            cbTableStatus.DataBindings.Add(new Binding("Text", dtgvTable.DataSource, "status", true, DataSourceUpdateMode.Never));
        }

        void LoadListCateogry()
        {
            categoryList.DataSource = CategoryDAO.Instance.GetListCategory();
        }

        void AddCategoryBinding()
        {
            txbCategoryID.DataBindings.Add(new Binding("Text", dtgvCategory.DataSource, "id", true, DataSourceUpdateMode.Never));
            txbNameCategory.DataBindings.Add(new Binding("Text", dtgvCategory.DataSource, "name", true, DataSourceUpdateMode.Never));
        }
        void LoadCategoryIntoComboBox()
        {
            cbFoodCategory.DataSource = CategoryDAO.Instance.GetListCategory();
            cbFoodCategory.DisplayMember = "Name";
        }

        void AddAccount(string userName, string displayName, int type)
        {
            if (AccountDAO.Instance.InsertAccount(userName, displayName, type))
            {
                MessageBox.Show("Thêm tài khoản thành công");
            }
            else
            {
                MessageBox.Show("Thêm tài khoản thất bại");
            }

            LoadAccount();
        }

        void EditAccount(string userName, string displayName, int type)
        {
            if (AccountDAO.Instance.UpdateAccount(userName, displayName, type))
            {
                MessageBox.Show("Cập nhật tài khoản thành công");
            }
            else
            {
                MessageBox.Show("Cập nhật tài khoản thất bại");
            }

            LoadAccount();
        }

        void DeleteAccount(string userName)
        {
            if (loginAccount.UserName.Equals(userName))
            {
                MessageBox.Show("Không thể xóa tài khoản của chính mình!");
                return;
            }
            if (AccountDAO.Instance.DeleteAccount(userName))
            {
                MessageBox.Show("Xóa tài khoản thành công");
            }
            else
            {
                MessageBox.Show("Xóa tài khoản thất bại");
            }

            LoadAccount();
        }
        void ResetPass(string userName)
        {
            if (AccountDAO.Instance.ResetPassword(userName))
            {
                MessageBox.Show("Đặt lại mật khẩu thành công");
            }
            else
            {
                MessageBox.Show("Đặt lại mật khẩu thất bại");
            }
        }

        public class ExcelExporter
        {
            public ExcelExporter()
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            }

            public void ExportToExcel(DataGridView dataGridView)
            {
                // Kiểm tra nếu DataGridView không có dữ liệu
                if (dataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất ra Excel.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    // Tạo một package Excel
                    using (ExcelPackage package = new ExcelPackage())
                    {

                        // Tạo một worksheet mới
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                        // Ghi dòng tiêu đề
                        for (int j = 0; j < dataGridView.Columns.Count; j++)
                        {
                            worksheet.Cells[1, j + 1].Value = dataGridView.Columns[j].HeaderText;
                        }

                        // Ghi dữ liệu từ DataGridView vào worksheet
                        for (int i = 0; i < dataGridView.Rows.Count; i++)
                        {
                            for (int j = 0; j < dataGridView.Columns.Count; j++)
                            {
                                if (dataGridView.Columns[j].Name == "Ngày vào" || dataGridView.Columns[j].Name == "Ngày ra")
                                {
                                    DateTime dateValue = (DateTime)dataGridView.Rows[i].Cells[j].Value;
                                    worksheet.Cells[i + 2, j + 1].Value = dateValue.ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    worksheet.Cells[i + 2, j + 1].Value = dataGridView.Rows[i].Cells[j].Value;
                                }
                            }
                        }

                        // Lưu file Excel
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "Excel Files|*.xlsx";
                        saveFileDialog.Title = "Save Excel File";
                        saveFileDialog.FileName = "Data.xlsx";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            FileInfo fi = new FileInfo(saveFileDialog.FileName);
                            package.SaveAs(fi);
                            MessageBox.Show("Xuất dữ liệu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi khi xuất dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        #endregion

        #region event


        private void btnViewBill_Click(object sender, EventArgs e)
        {
            LoadListBillByDate(dtpkFromDate.Value, dtpkToDate.Value);
        }

        private void btnShowFood_Click(object sender, EventArgs e)
        {
            LoadListFood();
        }
        private void txbFoodID_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtgvFood.SelectedCells.Count > 0)
                {
                    int id = (int)dtgvFood.SelectedCells[0].OwningRow.Cells["idCategory"].Value;

                    Category cateogory = CategoryDAO.Instance.GetCategoryById(id);

                    cbFoodCategory.SelectedItem = cateogory;

                    int index = -1;
                    int i = 0;
                    foreach (Category item in cbFoodCategory.Items)
                    {
                        if (item.ID == cateogory.ID)
                        {
                            index = i;
                            break;
                        }
                        i++;
                    }

                    cbFoodCategory.SelectedIndex = index;
                }
            }
            catch { }
        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {
            string name = txbFoodName.Text;
            int categoryID = (cbFoodCategory.SelectedItem as Category).ID;
            float price = (float)nmPriceFood.Value;

            if (FoodDAO.Instance.InsertFood(name, categoryID, price))
            {
                MessageBox.Show("Thêm món thành công");
                LoadListFood();
                if (insertFood != null)
                    insertFood(this, new EventArgs());
            }
            else
            {
                MessageBox.Show("Có lỗi khi thêm thức ăn");
            }
        }

        private void btnEditFood_Click(object sender, EventArgs e)
        {
            string name = txbFoodName.Text;
            int categoryID = (cbFoodCategory.SelectedItem as Category).ID;
            float price = (float)nmPriceFood.Value;
            int id = Convert.ToInt32(txbFoodID.Text);

            if (FoodDAO.Instance.UpdateFood(id, name, categoryID, price))
            {
                MessageBox.Show("Sửa món thành công");
                LoadListFood();
                if (updateFood != null)
                    updateFood(this, new EventArgs());
            }
            else
            {
                MessageBox.Show("Có lỗi khi sửa thức ăn");
            }
        }
        private void btnDeleteFood_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(txbFoodID.Text);

            if (FoodDAO.Instance.DeleteFood(id))
            {
                MessageBox.Show("Xóa món thành công");
                LoadListFood();
                if (deleteFood != null)
                    deleteFood(this, new EventArgs());

            }
            else
            {
                MessageBox.Show("Có lỗi khi xóa thức ăn");
            }
        }

        private event EventHandler insertFood;
        public event EventHandler InsertFood
        {
            add { insertFood += value; }
            remove { insertFood -= value; }
        }

        private event EventHandler deleteFood;
        public event EventHandler DeleteFood
        {
            add { deleteFood += value; }
            remove { deleteFood -= value; }
        }

        private event EventHandler updateFood;
        public event EventHandler UpdateFood
        {
            add { updateFood += value; }
            remove { updateFood -= value; }
        }


        private void btnSearchFood_Click(object sender, EventArgs e)
        {
            foodList.DataSource = SearchFoodByName(txbSearchFoodName.Text);
        }

        private void btnShowAccount_Click(object sender, EventArgs e)
        {
            LoadAccount();
        }

        private void btnAddAccount_Click(object sender, EventArgs e)
        {
            string userName = txbUserName.Text;
            string displayName = txbDisplayName.Text;
            int type = (int)numericUpDown1.Value;

            AddAccount(userName, displayName, type);
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            string userName = txbUserName.Text;

            DeleteAccount(userName);
        }

        private void btnEditAccount_Click(object sender, EventArgs e)
        {
            string userName = txbUserName.Text;
            string displayName = txbDisplayName.Text;
            int type = (int)numericUpDown1.Value;

            EditAccount(userName, displayName, type);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            string userName = txbUserName.Text;

            ResetPass(userName);
        }
        private void btnFirstBillPage_Click(object sender, EventArgs e)
        {
            txbNumPageBill.Text = "1";
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            int page = Convert.ToInt32(txbNumPageBill.Text);

            if (page > 1)
                page--;

            txbNumPageBill.Text = page.ToString();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            int page = Convert.ToInt32(txbNumPageBill.Text);
            int sumRecord = BillDAO.Instance.GetNumBillListByDate(dtpkFromDate.Value, dtpkToDate.Value);

            if (page < sumRecord)
                page++;

            txbNumPageBill.Text = page.ToString();
        }

        private void btnLastBillPage_Click(object sender, EventArgs e)
        {
            int sumRecord = BillDAO.Instance.GetNumBillListByDate(dtpkFromDate.Value, dtpkToDate.Value);

            int lastPage = sumRecord / 10;

            if (sumRecord % 10 != 0)
                lastPage++;

            txbNumPageBill.Text = lastPage.ToString();
        }

        private void txbNumPageBill_TextChanged(object sender, EventArgs e)
        {
            dtgvBill.DataSource = BillDAO.Instance.GetBillListByDateAndPage(dtpkFromDate.Value, dtpkToDate.Value,Convert.ToInt32(txbNumPageBill.Text));
        }

        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            string name = txbNameCategory.Text;

            if (CategoryDAO.Instance.InsertCategory(name))
            {
                MessageBox.Show("Thêm danh mục thành công");
                LoadListCateogry();
                if (insertCategory != null)
                    insertCategory(this, new EventArgs());
            }
            else
            {
                MessageBox.Show("Có lỗi khi thêm danh mục");
            }
        }

        private void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(txbCategoryID.Text);

            try
            {
                if (CategoryDAO.Instance.DeleteCategory(id))
                {
                    MessageBox.Show("Xóa danh mục thành công");
                    
                }
                else
                {
                    MessageBox.Show("Có lỗi khi xóa danh mục");
                }
                LoadListCateogry();
                if (deleteCategory != null)
                    deleteCategory(this, new EventArgs());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Danh mục đang có món ăn không thể xóa");
            }
        }

        private void btnEditCategory_Click(object sender, EventArgs e)
        {
            string name = txbNameCategory.Text;
            int id = Convert.ToInt32(txbCategoryID.Text);
            if (CategoryDAO.Instance.UpdateCategory(id,name))
            {
                MessageBox.Show("Cập nhật danh mục thành công");
                LoadListCateogry();
                if (updateCategory != null)
                    updateCategory(this, new EventArgs());

            }
            else
            {
                MessageBox.Show("Có lỗi khi sửa danh mục");
            }
        }

        private event EventHandler insertCategory;
        public event EventHandler InsertCategory
        {
            add { insertCategory += value; }
            remove { insertCategory -= value; }
        }

        private event EventHandler deleteCategory;
        public event EventHandler DeleteCategory
        {
            add { deleteCategory += value; }
            remove { deleteCategory -= value; }
        }

        private event EventHandler updateCategory;
        public event EventHandler UpdateCategory
        {
            add { updateCategory += value; }
            remove { updateCategory -= value; }
        }

        private void btnShowCategory_Click(object sender, EventArgs e)
        {
            LoadListCateogry();
        }

        private void btnSearchCategory_Click(object sender, EventArgs e)
        {
            categoryList.DataSource = CategoryDAO.Instance.SearchCategoryByName(txtSearchCategory.Text);
        }

        private void btnAddTable_Click(object sender, EventArgs e)
        {
            string name = txbTableName.Text;

            if (TableDAO.Instance.InsertTable(name))
            {
                MessageBox.Show("Thêm bàn thành công");
                LoadTable();
                
            }
            else
            {
                MessageBox.Show("Có lỗi khi thêm bàn");
            }
        }

        private void btnDeleteTable_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(txbTableID.Text);

            try
            {
                if (TableDAO.Instance.DeleteTable(id))
                {
                    MessageBox.Show("Xóa bàn thành công");

                }
                else
                {
                    MessageBox.Show("Có lỗi khi xóa bàn");
                }
                LoadTable();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bàn đang được sử dụng không thể xóa");
            }
        }

        private void btnEditTable_Click(object sender, EventArgs e)
        {
            string name = txbTableName.Text;
            int id = Convert.ToInt32(txbTableID.Text);
            if (TableDAO.Instance.UpdateTable(id, name))
            {
                MessageBox.Show("Cập nhật bàn thành công");
                LoadTable();
                

            }
            else
            {
                MessageBox.Show("Có lỗi khi sửa bàn");
            }
        }

        private void btnShowTable_Click(object sender, EventArgs e)
        {
            LoadTable();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExcelExporter exporter = new ExcelExporter();
            exporter.ExportToExcel(dtgvBill);
        }

        #endregion

        private void btnShowTopBestSeller_Click(object sender, EventArgs e)
        {
            LoadListFoodBestSeller(dtpkStarDate.Value,dtpkEndDate.Value);
        }

        private void btnExportBestSeller_Click(object sender, EventArgs e)
        {
            ExcelExporter exporter = new ExcelExporter();
            exporter.ExportToExcel(dtgvTopFoodBestSeller);
        }
    }
}
