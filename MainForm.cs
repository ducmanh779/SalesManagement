using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SalesManagement
{
    public partial class MainForm : Form

    {
        private string strConnection = "Server=0989078698\\SQLEXPRESS;Database=SalesManagement;Integrated Security = True; Trust Server Certificate=True";
        private string TABLE = "";
        private SqlConnection conne;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            TABLE = "Products";
            LoadData("Products", "ProductID, ProductCode, ProductName, Price, ImportPrice, Quantity");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            TABLE = "Employees";
            LoadData("Employees", "EmployeeID, EmployeeCode, FullName, DateOfBirth, Gender, Position, Address");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void btnImports_Click(object sender, EventArgs e)
        {
            TABLE = "Imports";
            LoadData("Imports", "ImportID, ProductID, Quantity, ImportPrice, ImportDate, EmployeeID");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            TABLE = "Customers";
            LoadData("Customers", "CustomerID, CustomerCode, FullName, DateOfBirth, Address, Phone, RegistrationDate");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            TABLE = "Orders";
            LoadData("Orders", "OrderID, CustomerID, EmployeeID, OrderDate, TotalAmount, Status");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            TABLE = "Users";
            LoadData("Users", "UserID, Username, Password, Role, EmployeeID, CustomerID, IsFirstLogin");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void LoadData(string tableName, string columns)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    conn.Open();
                    string query = $"SELECT {columns} FROM [{tableName}]";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvMainForm.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();

            // If the search term is empty, reload the full table
            if (string.IsNullOrEmpty(searchTerm))
            {
                switch (TABLE)
                {
                    case "Products":
                        LoadData("Products", "ProductID, ProductCode, ProductName, Price, ImportPrice, Quantity");
                        break;
                    case "Employees":
                        LoadData("Employees", "EmployeeID, EmployeeCode, FullName, DateOfBirth, Gender, Position, Address");
                        break;
                    case "Imports":
                        LoadData("Imports", "ImportID, ProductID, Quantity, ImportPrice, ImportDate, EmployeeID");
                        break;
                    case "Customers":
                        LoadData("Customers", "CustomerID, CustomerCode, FullName, DateOfBirth, Address, Phone, RegistrationDate");
                        break;
                    case "Orders":
                        LoadData("Orders", "OrderID, CustomerID, EmployeeID, OrderDate, TotalAmount, Status");
                        break;
                    case "Users":
                        LoadData("Users", "UserID, Username, Password, Role, EmployeeID, CustomerID, IsFirstLogin");
                        break;
                    default:
                        MessageBox.Show("Please select a table to search.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                }
                return;
            }

            // Perform the search based on the selected table
            try
            {
                conne.Open();
                string query = "";
                string columns = "";

                switch (TABLE)
                {
                    case "Customers":
                        columns = "CustomerID, CustomerCode, FullName, DateOfBirth, Address, Phone, RegistrationDate";
                        query = $"SELECT {columns} FROM Customers " +
                                "WHERE CAST(CustomerID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(CustomerCode) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(FullName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(DateOfBirth AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Address) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Phone) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(RegistrationDate AS NVARCHAR) LIKE @keyword";
                        break;
                    case "Employees":
                        columns = "EmployeeID, EmployeeCode, FullName, DateOfBirth, Gender, Position, Address";
                        query = $"SELECT {columns} FROM Employees " +
                                "WHERE CAST(EmployeeID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(EmployeeCode) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(FullName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(DateOfBirth AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Gender) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Position) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Address) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS";
                        break;
                    case "Products":
                        columns = "ProductID, ProductCode, ProductName, Price, ImportPrice, Quantity";
                        query = $"SELECT {columns} FROM Products " +
                                "WHERE CAST(ProductID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(ProductCode) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(ProductName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(Price AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(ImportPrice AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(Quantity AS NVARCHAR) LIKE @keyword";
                        break;
                    case "Imports":
                        columns = "ImportID, ProductID, Quantity, ImportPrice, ImportDate, EmployeeID";
                        query = $"SELECT {columns} FROM Imports " +
                                "WHERE CAST(ImportID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(ProductID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(Quantity AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(ImportPrice AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(ImportDate AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(EmployeeID AS NVARCHAR) LIKE @keyword";
                        break;
                    case "Orders":
                        columns = "OrderID, CustomerID, EmployeeID, OrderDate, TotalAmount, Status";
                        query = $"SELECT {columns} FROM Orders " +
                                "WHERE CAST(OrderID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(CustomerID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(EmployeeID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(OrderDate AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(TotalAmount AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Status) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS";
                        break;
                    case "Users":
                        columns = "UserID, Username, Password, Role, EmployeeID, CustomerID, IsFirstLogin";
                        query = $"SELECT {columns} FROM Users " +
                                "WHERE CAST(UserID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Username) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Role) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(EmployeeID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(CustomerID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(IsFirstLogin AS NVARCHAR) LIKE @keyword";
                        break;
                    default:
                        MessageBox.Show("Please select a table to search.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                }

                using (SqlCommand cmd = new SqlCommand(query, conne))
                {
                    cmd.Parameters.AddWithValue("@keyword", "%" + searchTerm + "%");
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Check if any results were found
                        if (dataTable.Rows.Count == 0)
                        {
                            MessageBox.Show($"No results found for '{searchTerm}' in {TABLE}.", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        dgvMainForm.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while searching: " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conne.State == ConnectionState.Open)
                    conne.Close();
            }
        }
    }
}
