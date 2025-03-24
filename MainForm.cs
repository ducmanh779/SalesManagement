using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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

        public MainForm()
        {
            InitializeComponent(); 
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            TABLE = "Products";
            LoadData("Products", "ProductID, ProductCode, ProductName, Price, Quantity"); // Added ProductCode
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            TABLE = "Employees";
            LoadData("Employees", "EmployeeID, FullName, DateOfBirth, Gender, Address");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            TABLE = "Customers";
            LoadData("Customers", "CustomerID, FullName, DateOfBirth, Address, Phone, RegistrationDate");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            TABLE = "Orders";
            LoadData("Orders", "OrderID, OrderDate, EmployeeID, CustomerID, TotalAmount, Status");
            btnSearch.Enabled = true;
            txtSearch.Clear(); ;
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            TABLE = "Users";
            LoadData("Users", "UserID, Username, Password, Role");
            btnSearch.Enabled = true;
            txtSearch.Clear();
        }

        private void LoadData(string tableName, string columns)
        {
            try
            {
                using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(strConnection))
                {
                    conn.Open();
                    string query = $"SELECT {columns} FROM [{tableName}]";
                    using (Microsoft.Data.SqlClient.SqlDataAdapter adapter = new Microsoft.Data.SqlClient.SqlDataAdapter(query, conn))
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
                        LoadData("Products", "ProductID, ProductCode, ProductName, Price, Quantity"); // Added ProductCode
                        break;
                    case "Employees":
                        LoadData("Employees", "EmployeeID, FullName, DateOfBirth, Gender, Address");
                        break;
                    case "Customers":
                        LoadData("Customers", "CustomerID, FullName, DateOfBirth, Address, Phone, RegistrationDate");
                        break;
                    case "Orders":
                        LoadData("Orders", "OrderID, OrderDate, EmployeeID, CustomerID, TotalAmount, Status");
                        break;
                    case "Users":
                        LoadData("Users", "UserID, Username, Password, Role");
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
                using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(strConnection))
                {
                    conn.Open();
                    string query = "";
                    string columns = "";

                    switch (TABLE)
                    {
                        case "Customers":
                            columns = "CustomerID, FullName, DateOfBirth, Address, Phone, RegistrationDate";
                            query = $"SELECT {columns} FROM Customers WHERE FullName LIKE @keyword OR Phone LIKE @keyword";
                            break;
                        case "Employees":
                            columns = "EmployeeID, FullName, DateOfBirth, Gender, Address";
                            query = $"SELECT {columns} FROM Employees WHERE FullName LIKE @keyword";
                            break;
                        case "Products":
                            columns = "ProductID, ProductCode, ProductName, Price, Quantity"; // Added ProductCode
                            query = $"SELECT {columns} FROM Products WHERE ProductName LIKE @keyword";
                            break;
                        case "Orders":
                            columns = "OrderID, OrderDate, EmployeeID, CustomerID, TotalAmount, Status";
                            query = $"SELECT {columns} FROM Orders WHERE CAST(EmployeeID AS NVARCHAR) LIKE @keyword OR CAST(CustomerID AS NVARCHAR) LIKE @keyword OR Status LIKE @keyword";
                            break;
                        case "Users":
                            columns = "UserID, Username, Password, Role";
                            query = $"SELECT {columns} FROM Users WHERE Username LIKE @keyword";
                            break;
                        default:
                            MessageBox.Show("Please select a table to search.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                    }

                    using (Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@keyword", "%" + searchTerm + "%");
                        using (Microsoft.Data.SqlClient.SqlDataAdapter adapter = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd))
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while searching: " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
    }
}
