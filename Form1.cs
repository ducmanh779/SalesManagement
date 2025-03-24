using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;


namespace SalesManagement
{
    public partial class Management : Form
    {
        private readonly string strConnection = "Server=0989078698\\SQLEXPRESS;Database=SalesManagement;Integrated Security=True;Trust Server Certificate=True";
        private SqlConnection conne;

        public Management(string role)
        {
            InitializeComponent();
            Connection();
            SetAccessControl(role);
            LoadComboBoxes();
            LoadTabData();
        }

        private void Connection()
        {
            try
            {
                conne = new SqlConnection(strConnection);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error at createConnection: " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetAccessControl(string role)
        {
            if (role != "Admin")
            {
                tabcomtrol.Enabled = false; // Employees tab
                tabPage.Enabled = false;    // Users tab
            }
        }

        private void LoadTabData()
        {
            if (tabControl1.SelectedTab == tabPage2) LoadTableData("Customers", dgvCustomers);
            else if (tabControl1.SelectedTab == tabcomtrol) LoadTableData("Employees", dgvEmployees);
            else if (tabControl1.SelectedTab == tabPage4) LoadTableData("Products", dgvProducts);
            else if (tabControl1.SelectedTab == tabPage5) LoadTableData("Orders", dgvOrders);
            else if (tabControl1.SelectedTab == tabPage) LoadTableData("Users", dgvUsers);
        }

        private void LoadTableData(string tableName, DataGridView dataGridView)
        {
            try
            {
                conne.Open();
                string query = "";
                switch (tableName)
                {
                    case "Customers":
                        query = "SELECT CustomerID, FullName, DateOfBirth, Address, Phone, RegistrationDate FROM Customers";
                        break;
                    case "Employees":
                        query = "SELECT EmployeeID, FullName, DateOfBirth, Gender, Address FROM Employees";
                        break;
                    case "Products":
                        query = "SELECT ProductID, ProductName, Price, Quantity FROM Products";
                        break;
                    case "Orders":
                        query = "SELECT OrderID, OrderDate, EmployeeID, CustomerID, TotalAmount, Status FROM Orders";
                        break;
                    case "Users":
                        query = "SELECT UserID, Username, Role, EmployeeID, CustomerID FROM Users";
                        break;
                    default:
                        throw new Exception("Invalid table name");
                }
                SqlDataAdapter adapter = new SqlDataAdapter(query, conne);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading {tableName}: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conne.Close();
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                conne.Open();

                // Populate cbOrderCustomer with CustomerID
                SqlDataAdapter customerAdapter = new SqlDataAdapter("SELECT CustomerID, FullName FROM Customers", conne);
                DataTable customerDt = new DataTable();
                customerAdapter.Fill(customerDt);
                cbOrderCustomer.DataSource = customerDt;
                cbOrderCustomer.DisplayMember = "CustomerID";
                cbOrderCustomer.ValueMember = "CustomerID";

                // Populate cbOrderEmployee with EmployeeID
                SqlDataAdapter employeeAdapter = new SqlDataAdapter("SELECT EmployeeID, FullName FROM Employees", conne);
                DataTable employeeDt = new DataTable();
                employeeAdapter.Fill(employeeDt);
                cbOrderEmployee.DataSource = employeeDt;
                cbOrderEmployee.DisplayMember = "EmployeeID";
                cbOrderEmployee.ValueMember = "EmployeeID";

                // Populate cbOrderStatus
                cbOrderStatus.Items.AddRange(new[] { "Pending", "Completed", "Cancelled" });

                // Populate cbEmpGender
                cbEmpGender.Items.AddRange(new[] { "Male", "Female", "Other" });

                // Populate cmbRole with roles
                cmbRole.Items.AddRange(new[] { "Admin", "Employee", "Customer" });
                cmbRole.SelectedIndex = 0; // Set default selection to the first role
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ComboBox data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conne.Close();
            }
        }

        private void AddData(string tableName, DataGridView dataGridView)
        {
            try
            {
                // Input validation
                if (tableName == "Customers")
                {
                    if (string.IsNullOrWhiteSpace(txtCusName.Text))
                    {
                        MessageBox.Show("Customer Name is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtCusPhone.Text))
                    {
                        MessageBox.Show("Phone is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Employees")
                {
                    if (string.IsNullOrWhiteSpace(txtEmpName.Text))
                    {
                        MessageBox.Show("Employee Name is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Products")
                {
                    if (string.IsNullOrWhiteSpace(txtProName.Text))
                    {
                        MessageBox.Show("Product Name is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txtProPrice.Text, out decimal price) || price < 0)
                    {
                        MessageBox.Show("Price must be a valid number greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!int.TryParse(txtProQuantity.Text, out int quantity) || quantity < 0)
                    {
                        MessageBox.Show("Quantity must be a valid integer greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Orders")
                {
                    if (!decimal.TryParse(txtOrderTotal.Text, out decimal totalAmount) || totalAmount < 0)
                    {
                        MessageBox.Show("Total Amount must be a valid number greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Users")
                {
                    if (string.IsNullOrWhiteSpace(txtUsername.Text))
                    {
                        MessageBox.Show("Username is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        MessageBox.Show("Password is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cmbRole.SelectedItem == null)
                    {
                        MessageBox.Show("Please select a role.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                conne.Open();
                string query = "";
                SqlCommand cmd;
                switch (tableName)
                {
                    case "Customers":
                        query = "INSERT INTO Customers (FullName, DateOfBirth, Address, Phone, RegistrationDate) VALUES (@FullName, @DateOfBirth, @Address, @Phone, @RegistrationDate)";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@FullName", txtCusName.Text);
                        cmd.Parameters.AddWithValue("@DateOfBirth", dtpCusDOB.Value);
                        cmd.Parameters.AddWithValue("@Address", txtCusAddress.Text);
                        cmd.Parameters.AddWithValue("@Phone", txtCusPhone.Text);
                        cmd.Parameters.AddWithValue("@RegistrationDate", dtpCusDOB.Value);
                        break;
                    case "Employees":
                        query = "INSERT INTO Employees (FullName, DateOfBirth, Gender, Address) VALUES (@FullName, @DateOfBirth, @Gender, @Address)";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@FullName", txtEmpName.Text);
                        cmd.Parameters.AddWithValue("@DateOfBirth", dtpEmpDOB.Value);
                        cmd.Parameters.AddWithValue("@Gender", cbEmpGender.SelectedItem?.ToString() ?? "Other");
                        cmd.Parameters.AddWithValue("@Address", txtEmpAddress.Text);
                        break;
                    case "Products":
                        query = "INSERT INTO Products (ProductName, Price, Quantity) VALUES (@ProductName, @Price, @Quantity)";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@ProductName", txtProName.Text);
                        cmd.Parameters.AddWithValue("@Price", decimal.Parse(txtProPrice.Text));
                        cmd.Parameters.AddWithValue("@Quantity", int.Parse(txtProQuantity.Text));
                        break;
                    case "Orders":
                        query = "INSERT INTO Orders (OrderDate, EmployeeID, CustomerID, TotalAmount, Status) VALUES (@OrderDate, @EmployeeID, @CustomerID, @TotalAmount, @Status)";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@OrderDate", dtpOrderDate.Value);
                        cmd.Parameters.AddWithValue("@EmployeeID", cbOrderEmployee.SelectedValue ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CustomerID", cbOrderCustomer.SelectedValue);
                        cmd.Parameters.AddWithValue("@TotalAmount", decimal.Parse(txtOrderTotal.Text));
                        cmd.Parameters.AddWithValue("@Status", cbOrderStatus.SelectedItem?.ToString() ?? "Pending");
                        break;
                    case "Users":
                        query = "INSERT INTO Users (Username, Password, Role, EmployeeID, CustomerID) VALUES (@Username, @Password, @Role, @EmployeeID, @CustomerID)";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@Role", cmbRole.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@EmployeeID", string.IsNullOrWhiteSpace(txtEmployeeID.Text) ? (object)DBNull.Value : txtEmployeeID.Text);
                        cmd.Parameters.AddWithValue("@CustomerID", string.IsNullOrWhiteSpace(txtCustomerID.Text) ? (object)DBNull.Value : txtCustomerID.Text);
                        break;
                    default:
                        throw new Exception("Invalid table name");
                }

                cmd.ExecuteNonQuery();
                MessageBox.Show("Data added successfully!", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conne.Close();
                LoadTableData(tableName, dataGridView);
            }
        }

        private void UpdateData(string tableName, DataGridView dataGridView, string primaryKeyValue)
        {
            try
            {
                // Input validation for all tables
                if (tableName == "Customers")
                {
                    if (string.IsNullOrWhiteSpace(txtCusName.Text))
                    {
                        MessageBox.Show("Customer Name is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtCusPhone.Text))
                    {
                        MessageBox.Show("Phone is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Validate Phone: Should only contain digits (allowing optional dashes or spaces)
                    string phonePattern = @"^[0-9\s\-]+$";
                    if (!Regex.IsMatch(txtCusPhone.Text, phonePattern))
                    {
                        MessageBox.Show("Phone must contain only digits (spaces or dashes are allowed).", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Validate Address: Should not look like a phone number
                    string addressPattern = @"^[a-zA-Z0-9\s,.-]+$";
                    if (!string.IsNullOrWhiteSpace(txtCusAddress.Text) && !Regex.IsMatch(txtCusAddress.Text, addressPattern))
                    {
                        MessageBox.Show("Address contains invalid characters. Only letters, numbers, spaces, commas, periods, and dashes are allowed.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(txtCusAddress.Text) && Regex.IsMatch(txtCusAddress.Text, phonePattern))
                    {
                        MessageBox.Show("Address cannot be a phone number.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Employees")
                {
                    if (string.IsNullOrWhiteSpace(txtEmpName.Text))
                    {
                        MessageBox.Show("Employee Name is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Validate Address for Employees
                    string addressPattern = @"^[a-zA-Z0-9\s,.-]+$";
                    if (!string.IsNullOrWhiteSpace(txtEmpAddress.Text) && !Regex.IsMatch(txtEmpAddress.Text, addressPattern))
                    {
                        MessageBox.Show("Address contains invalid characters. Only letters, numbers, spaces, commas, periods, and dashes are allowed.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    string phonePattern = @"^[0-9\s\-]+$";
                    if (!string.IsNullOrWhiteSpace(txtEmpAddress.Text) && Regex.IsMatch(txtEmpAddress.Text, phonePattern))
                    {
                        MessageBox.Show("Address cannot be a phone number.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Products")
                {
                    if (string.IsNullOrWhiteSpace(txtProName.Text))
                    {
                        MessageBox.Show("Product Name is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txtProPrice.Text, out decimal price) || price < 0)
                    {
                        MessageBox.Show("Price must be a valid number greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!int.TryParse(txtProQuantity.Text, out int quantity) || quantity < 0)
                    {
                        MessageBox.Show("Quantity must be a valid integer greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Orders")
                {
                    if (!decimal.TryParse(txtOrderTotal.Text, out decimal totalAmount) || totalAmount < 0)
                    {
                        MessageBox.Show("Total Amount must be a valid number greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Users")
                {
                    if (string.IsNullOrWhiteSpace(txtUsername.Text))
                    {
                        MessageBox.Show("Username is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        MessageBox.Show("Password is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cmbRole.SelectedItem == null)
                    {
                        MessageBox.Show("Please select a role.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                conne.Open();
                string query = "";
                SqlCommand cmd;
                switch (tableName)
                {
                    case "Customers":
                        query = "UPDATE Customers SET FullName = @FullName, DateOfBirth = @DateOfBirth, Address = @Address, Phone = @Phone, RegistrationDate = @RegistrationDate WHERE CustomerID = @CustomerID";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@CustomerID", primaryKeyValue);
                        cmd.Parameters.AddWithValue("@FullName", txtCusName.Text);
                        cmd.Parameters.AddWithValue("@DateOfBirth", dtpCusDOB.Value);
                        cmd.Parameters.AddWithValue("@Address", txtCusAddress.Text);
                        cmd.Parameters.AddWithValue("@Phone", txtCusPhone.Text);
                        cmd.Parameters.AddWithValue("@RegistrationDate", dtpCusDOB.Value);
                        break;
                    case "Employees":
                        query = "UPDATE Employees SET FullName = @FullName, DateOfBirth = @DateOfBirth, Gender = @Gender, Address = @Address WHERE EmployeeID = @EmployeeID";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@EmployeeID", primaryKeyValue);
                        cmd.Parameters.AddWithValue("@FullName", txtEmpName.Text);
                        cmd.Parameters.AddWithValue("@DateOfBirth", dtpEmpDOB.Value);
                        cmd.Parameters.AddWithValue("@Gender", cbEmpGender.SelectedItem?.ToString() ?? "Other");
                        cmd.Parameters.AddWithValue("@Address", txtEmpAddress.Text);
                        break;
                    case "Products":
                        query = "UPDATE Products SET ProductName = @ProductName, Price = @Price, Quantity = @Quantity WHERE ProductID = @ProductID";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@ProductID", primaryKeyValue);
                        cmd.Parameters.AddWithValue("@ProductName", txtProName.Text);
                        cmd.Parameters.AddWithValue("@Price", decimal.Parse(txtProPrice.Text));
                        cmd.Parameters.AddWithValue("@Quantity", int.Parse(txtProQuantity.Text));
                        break;
                    case "Orders":
                        query = "UPDATE Orders SET OrderDate = @OrderDate, EmployeeID = @EmployeeID, CustomerID = @CustomerID, TotalAmount = @TotalAmount, Status = @Status WHERE OrderID = @OrderID";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@OrderID", primaryKeyValue);
                        cmd.Parameters.AddWithValue("@OrderDate", dtpOrderDate.Value);
                        cmd.Parameters.AddWithValue("@EmployeeID", cbOrderEmployee.SelectedValue ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CustomerID", cbOrderCustomer.SelectedValue);
                        cmd.Parameters.AddWithValue("@TotalAmount", decimal.Parse(txtOrderTotal.Text));
                        cmd.Parameters.AddWithValue("@Status", cbOrderStatus.SelectedItem?.ToString() ?? "Pending");
                        break;
                    case "Users":
                        query = "UPDATE Users SET Username = @Username, Password = @Password, Role = @Role, EmployeeID = @EmployeeID, CustomerID = @CustomerID WHERE UserID = @UserID";
                        cmd = new SqlCommand(query, conne);
                        cmd.Parameters.AddWithValue("@UserID", primaryKeyValue);
                        cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@Role", cmbRole.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@EmployeeID", string.IsNullOrWhiteSpace(txtEmployeeID.Text) ? (object)DBNull.Value : txtEmployeeID.Text);
                        cmd.Parameters.AddWithValue("@CustomerID", string.IsNullOrWhiteSpace(txtCustomerID.Text) ? (object)DBNull.Value : txtCustomerID.Text);
                        break;
                    default:
                        throw new Exception("Invalid table name");
                }

                cmd.ExecuteNonQuery();
                MessageBox.Show("Data updated successfully!", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conne.Close();
                LoadTableData(tableName, dataGridView);
            }
        }

            private void DeleteData(string tableName, DataGridView dataGridView, string primaryKeyColumn, string primaryKeyValue)
        {
            try
            {
                conne.Open();
                string query = $"DELETE FROM [{tableName}] WHERE {primaryKeyColumn} = @primaryKey";
                SqlCommand cmd = new SqlCommand(query, conne);
                cmd.Parameters.AddWithValue("@primaryKey", primaryKeyValue);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Data deleted successfully!", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE constraint"))
                {
                    MessageBox.Show($"Cannot delete this record because it is referenced by another table (e.g., Orders).", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Error deleting data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                conne.Close();
                LoadTableData(tableName, dataGridView);
            }
        }

        private void SearchData(string tableName, DataGridView dataGridView, string searchValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    // If the search value is empty, reload the full table data
                    LoadTableData(tableName, dataGridView);
                    return;
                }

                conne.Open();
                string query = "";
                switch (tableName)
                {
                    case "Customers":
                        query = "SELECT CustomerID, FullName, DateOfBirth, Address, Phone, RegistrationDate FROM Customers " +
                                "WHERE CAST(CustomerID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(FullName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(DateOfBirth AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Address) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Phone) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(RegistrationDate AS NVARCHAR) LIKE @keyword";
                        break;
                    case "Employees":
                        query = "SELECT EmployeeID, FullName, DateOfBirth, Gender, Address FROM Employees " +
                                "WHERE CAST(EmployeeID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(FullName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(DateOfBirth AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Gender) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Address) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS";
                        break;
                    case "Products":
                        query = "SELECT ProductID, ProductName, Price, Quantity FROM Products " +
                                "WHERE CAST(ProductID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(ProductName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(Price AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(Quantity AS NVARCHAR) LIKE @keyword";
                        break;
                    case "Orders":
                        query = "SELECT OrderID, OrderDate, EmployeeID, CustomerID, TotalAmount, Status FROM Orders " +
                                "WHERE CAST(OrderID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(OrderDate AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(EmployeeID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(CustomerID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(TotalAmount AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Status) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS";
                        break;
                    case "Users":
                        query = "SELECT UserID, Username, Role, EmployeeID, CustomerID FROM Users " +
                                "WHERE CAST(UserID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Username) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Role) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(EmployeeID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(CustomerID AS NVARCHAR) LIKE @keyword";
                        break;
                    default:
                        throw new Exception("Invalid table name");
                }

                SqlCommand cmd = new SqlCommand(query, conne);
                cmd.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = "%" + searchValue + "%"; // Explicitly set parameter type
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conne.Close();
            }
        }

        private void ClearFields(string tableName)
        {
            switch (tableName)
            {
                case "Customers":
                    txtCusID.Clear();
                    txtCusName.Clear();
                    txtCusPhone.Clear();
                    txtCusAddress.Clear();
                    dtpCusDOB.Value = DateTime.Now;
                    break;
                case "Employees":
                    txtEmpID.Clear();
                    txtEmpName.Clear();
                    txtEmpAddress.Clear();
                    dtpEmpDOB.Value = DateTime.Now;
                    cbEmpGender.SelectedIndex = -1;
                    break;
                case "Products":
                    txtProID.Clear();
                    txtProName.Clear();
                    txtProQuantity.Clear();
                    txtProPrice.Clear();
                    break;
                case "Orders":
                    txtOrderID.Clear();
                    dtpOrderDate.Value = DateTime.Now;
                    cbOrderEmployee.SelectedIndex = -1;
                    cbOrderCustomer.SelectedIndex = -1;
                    txtOrderTotal.Clear();
                    cbOrderStatus.SelectedIndex = -1;
                    break;
                case "Users":
                    txtUserID.Clear();
                    txtUsername.Clear();
                    txtPassword.Clear();
                    txtEmployeeID.Clear();
                    txtCustomerID.Clear();
                    cmbRole.SelectedIndex = 0;
                    break;
            }
        }

        // Event Handlers
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTabData();
        }

        private void btnCusAdd_Click(object sender, EventArgs e)
        {
            AddData("Customers", dgvCustomers);
        }

        private void btnEmpAdd_Click(object sender, EventArgs e)
        {
            AddData("Employees", dgvEmployees);
        }

        private void btnProAdd_Click(object sender, EventArgs e)
        {
            AddData("Products", dgvProducts);
        }

        private void btnOrdersAdd_Click(object sender, EventArgs e)
        {
            AddData("Orders", dgvOrders);
        }

        private void btnUserAdd_Click(object sender, EventArgs e)
        {
            AddData("Users", dgvUsers);
        }

        private void btnCusEdit_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvCustomers.SelectedRows[0].Cells["CustomerID"].Value.ToString();
            UpdateData("Customers", dgvCustomers, primaryKeyValue);
        }

        private void btnEmpEdit_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvEmployees.SelectedRows[0].Cells["EmployeeID"].Value.ToString();
            UpdateData("Employees", dgvEmployees, primaryKeyValue);
        }

        private void btnProEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvProducts.SelectedRows[0].Cells["ProductID"].Value.ToString();
            UpdateData("Products", dgvProducts, primaryKeyValue);
        }

        private void btnOrdersEdit_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();
            UpdateData("Orders", dgvOrders, primaryKeyValue);
        }

        private void btnUserEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvUsers.SelectedRows[0].Cells["UserID"].Value.ToString();
            UpdateData("Users", dgvUsers, primaryKeyValue);
        }

        private void btnCusDelete_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvCustomers.SelectedRows[0].Cells["CustomerID"].Value.ToString();
            DeleteData("Customers", dgvCustomers, "CustomerID", primaryKeyValue);
        }

        private void btnEmpDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvEmployees.SelectedRows[0].Cells["EmployeeID"].Value.ToString();
            DeleteData("Employees", dgvEmployees, "EmployeeID", primaryKeyValue);
        }

        private void btnProDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvProducts.SelectedRows[0].Cells["ProductID"].Value.ToString();
            DeleteData("Products", dgvProducts, "ProductID", primaryKeyValue);
        }

        private void btnOrdersDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();
            DeleteData("Orders", dgvOrders, "OrderID", primaryKeyValue);
        }

        private void btnUserDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvUsers.SelectedRows[0].Cells["UserID"].Value.ToString();
            DeleteData("Users", dgvUsers, "UserID", primaryKeyValue);
        }

        private void btnCusSearch_Click(object sender, EventArgs e)
        {
            SearchData("Customers", dgvCustomers, txtSearch.Text);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchData("Employees", dgvEmployees, txtEmpSearch.Text);
        }

        private void btnProSearch_Click(object sender, EventArgs e)
        {
            SearchData("Products", dgvProducts, txtProSearch.Text);
        }

        private void btnOrdersSearch_Click(object sender, EventArgs e)
        {
            SearchData("Orders", dgvOrders, txtOrderSearch.Text);
        }

        private void btnUserSearch_Click(object sender, EventArgs e)
        {
            SearchData("Users", dgvUsers, textBox1.Text);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
        }

        private void btnLogout2_Click(object sender, EventArgs e)
        {
            btnLogout_Click(sender, e);
        }

        private void btnLogout4_Click(object sender, EventArgs e)
        {
            btnLogout_Click(sender, e);
        }

        private void btnLogout5_Click(object sender, EventArgs e)
        {
            btnLogout_Click(sender, e);
        }

        private void btnUserLogout_Click(object sender, EventArgs e)
        {
            btnLogout_Click(sender, e);
        }

        private void Management_Load(object sender, EventArgs e)
        {

        }
    }
}
