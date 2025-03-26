using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
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
                tabPage1.Enabled = false; // Employees tab
                tabPage5.Enabled = false; // Users tab
            }
        }

        private void LoadTabData()
        {
            if (tabControl1.SelectedTab == tabPage1) LoadTableData("Imports", dgvImports);
            else if (tabControl1.SelectedTab == tabPage2) LoadTableData("Customers", dgvCustomers);
            else if (tabControl1.SelectedTab == tabPage1) LoadTableData("Employees", dgvEmployees);
            else if (tabControl1.SelectedTab == tabPage4) LoadTableData("Products", dgvProducts);
            else if (tabControl1.SelectedTab == tabPage5) LoadTableData("Users", dgvUsers);
            else if (tabControl1.SelectedTab == tabPage6)
            {
                LoadTableData("Orders", dgvOrders);
            }
        }

        private void LoadTableData(string tableName, DataGridView dataGridView)
        {
            try
            {
                conne.Open();
                string query = "";
                switch (tableName)
                {
                    case "Employees":
                        query = "SELECT EmployeeID, EmployeeCode, FullName, DateOfBirth, Gender, Position, Address FROM Employees";
                        break;
                    case "Customers":
                        query = "SELECT CustomerID, CustomerCode, FullName, DateOfBirth, Address, Phone, RegistrationDate FROM Customers";
                        break;
                    case "Imports":
                        query = "SELECT ImportID, ProductID, Quantity, ImportPrice, ImportDate, EmployeeID FROM Imports";
                        break;
                    case "Products":
                        query = "SELECT ProductID, ProductCode, ProductName, Price, ImportPrice, Quantity FROM Products";
                        break;
                    case "Orders":
                        query = "SELECT OrderID, OrderDate, EmployeeID, CustomerID, TotalAmount, Status FROM Orders";
                        break;
                    case "Users":
                        query = "SELECT UserID, Username, Role, EmployeeID, CustomerID, IsFirstLogin FROM Users";
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
                if (conne.State == ConnectionState.Open)
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

                // Populate cbImportProductID with ProductID only
                SqlDataAdapter productAdapter = new SqlDataAdapter("SELECT ProductID, ProductName FROM Products", conne);
                DataTable productDt = new DataTable();
                productAdapter.Fill(productDt);
                cbImportProductID.DataSource = productDt;
                cbImportProductID.DisplayMember = "ProductID";
                cbImportProductID.ValueMember = "ProductID";

                // Populate cbImportEmployeeID with EmployeeID only
                SqlDataAdapter importEmployeeAdapter = new SqlDataAdapter("SELECT EmployeeID, FullName FROM Employees", conne);
                DataTable importEmployeeDt = new DataTable();
                importEmployeeAdapter.Fill(importEmployeeDt);
                cbImportEmployeeID.DataSource = importEmployeeDt;
                cbImportEmployeeID.DisplayMember = "EmployeeID";
                cbImportEmployeeID.ValueMember = "EmployeeID";

                // Populate cbOrderStatus
                cbOrderStatus.Items.AddRange(new[] { "Pending", "Completed", "Cancelled" });

                // Populate cbEmpGender
                cbEmpGender.Items.AddRange(new[] { "Male", "Female", "Other" });

                // Populate cbEmpPosition
                cbEmpPosition.Items.AddRange(new[] { "Manager", "Sales", "Staff" });

                // Populate cbRole with roles (match database CHECK constraint)
                cmbRole.Items.Clear();
                cmbRole.Items.AddRange(new[] { "ADMIN", "EMPLOYEE", "CUSTOMER" }); // Use uppercase to match database
                cmbRole.SelectedIndex = 0; // Default to "ADMIN"
                cmbRole.DropDownStyle = ComboBoxStyle.DropDownList; // Prevent free text input
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ComboBox data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conne.State == ConnectionState.Open)
                    conne.Close();
            }
        }

        private bool IsCustomerCodeUnique(string customerCode, string customerID = null)
        {
            try
            {
                conne.Open();
                string query = "SELECT COUNT(*) FROM Customers WHERE CustomerCode = @CustomerCode";
                if (!string.IsNullOrEmpty(customerID))
                {
                    query += " AND CustomerID != @CustomerID";
                }
                using (SqlCommand cmd = new SqlCommand(query, conne))
                {
                    cmd.Parameters.AddWithValue("@CustomerCode", customerCode);
                    if (!string.IsNullOrEmpty(customerID))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
                    }
                    int count = (int)cmd.ExecuteScalar();
                    return count == 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking CustomerCode uniqueness: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (conne.State == ConnectionState.Open)
                    conne.Close();
            }
        }

        private void AddData(string tableName, DataGridView dataGridView)
        {
            try
            {
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
                    string phonePattern = @"^[0-9\s\-]+$";
                    if (!Regex.IsMatch(txtCusPhone.Text, phonePattern))
                    {
                        MessageBox.Show("Phone must contain only digits (spaces or dashes are allowed).", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtCusCode.Text))
                    {
                        MessageBox.Show("Customer Code is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    string codePattern = @"^[a-zA-Z0-9]+$";
                    if (!Regex.IsMatch(txtCusCode.Text, codePattern))
                    {
                        MessageBox.Show("Customer Code must contain only letters and numbers (no spaces or special characters).", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!IsCustomerCodeUnique(txtCusCode.Text))
                    {
                        MessageBox.Show("Customer Code must be unique.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    if (string.IsNullOrWhiteSpace(txtEmpCode.Text))
                    {
                        MessageBox.Show("Employee Code is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbEmpPosition.SelectedItem == null)
                    {
                        MessageBox.Show("Position is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (tableName == "Imports")
                {
                    if (cbImportProductID.SelectedValue == null)
                    {
                        MessageBox.Show("Please select a Product.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!int.TryParse(txtImportQuantity.Text, out int quantity) || quantity <= 0)
                    {
                        MessageBox.Show("Quantity must be a valid integer greater than 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txtImportPrice.Text, out decimal importPrice) || importPrice <= 0)
                    {
                        MessageBox.Show("Import Price must be a valid number greater than 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbImportEmployeeID.SelectedValue == null)
                    {
                        MessageBox.Show("Please select an Employee.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    if (string.IsNullOrWhiteSpace(txtProductCode.Text))
                    {
                        MessageBox.Show("Product Code is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txtProPrice.Text, out decimal price) || price < 0)
                    {
                        MessageBox.Show("Price must be a valid number greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txtProImportPrice.Text, out decimal importPrice) || importPrice < 0)
                    {
                        MessageBox.Show("Import Price must be a valid number greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    if (cbOrderCustomer.SelectedValue == null)
                    {
                        MessageBox.Show("Please select a Customer.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbOrderEmployee.SelectedValue == null)
                    {
                        MessageBox.Show("Please select an Employee.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    // Validate Role value against allowed values
                    string selectedRole = cmbRole.SelectedItem.ToString();
                    string[] allowedRoles = { "ADMIN", "EMPLOYEE", "CUSTOMER" }; // Match database constraint
                    if (!allowedRoles.Contains(selectedRole))
                    {
                        MessageBox.Show("Invalid Role selected. Role must be 'ADMIN', 'EMPLOYEE', or 'CUSTOMER'.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                conne.Open();
                string query = "";
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conne;
                    switch (tableName)
                    {
                        case "Employees":
                            query = "INSERT INTO Employees (EmployeeCode, FullName, DateOfBirth, Gender, Position, Address) VALUES (@EmployeeCode, @FullName, @DateOfBirth, @Gender, @Position, @Address)";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@EmployeeCode", txtEmpCode.Text);
                            cmd.Parameters.AddWithValue("@FullName", txtEmpName.Text);
                            cmd.Parameters.AddWithValue("@DateOfBirth", dtpEmpDOB.Value);
                            cmd.Parameters.AddWithValue("@Gender", cbEmpGender.SelectedItem?.ToString() ?? "Other");
                            cmd.Parameters.AddWithValue("@Position", cbEmpPosition.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@Address", txtEmpAddress.Text);
                            break;
                        case "Customers":
                            query = "INSERT INTO Customers (CustomerCode, FullName, DateOfBirth, Address, Phone, RegistrationDate) VALUES (@CustomerCode, @FullName, @DateOfBirth, @Address, @Phone, @RegistrationDate)";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@CustomerCode", txtCusCode.Text);
                            cmd.Parameters.AddWithValue("@FullName", txtCusName.Text);
                            cmd.Parameters.AddWithValue("@DateOfBirth", dtpCuDateOfBirth.Value);
                            cmd.Parameters.AddWithValue("@Address", txtCusAddress.Text);
                            cmd.Parameters.AddWithValue("@Phone", txtCusPhone.Text);
                            cmd.Parameters.AddWithValue("@RegistrationDate", dtpCusRegistrationDate.Value);
                            break;
                        case "Imports":
                            query = "INSERT INTO Imports (ProductID, Quantity, ImportPrice, ImportDate, EmployeeID) VALUES (@ProductID, @Quantity, @ImportPrice, @ImportDate, @EmployeeID)";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@ProductID", cbImportProductID.SelectedValue);
                            cmd.Parameters.AddWithValue("@Quantity", int.Parse(txtImportQuantity.Text));
                            cmd.Parameters.AddWithValue("@ImportPrice", decimal.Parse(txtImportPrice.Text));
                            cmd.Parameters.AddWithValue("@ImportDate", dtpImportDate.Value);
                            cmd.Parameters.AddWithValue("@EmployeeID", cbImportEmployeeID.SelectedValue);
                            break;
                        case "Products":
                            query = "INSERT INTO Products (ProductCode, ProductName, Price, ImportPrice, Quantity) VALUES (@ProductCode, @ProductName, @Price, @ImportPrice, @Quantity)";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@ProductCode", txtProductCode.Text);
                            cmd.Parameters.AddWithValue("@ProductName", txtProName.Text);
                            cmd.Parameters.AddWithValue("@Price", decimal.Parse(txtProPrice.Text));
                            cmd.Parameters.AddWithValue("@ImportPrice", decimal.Parse(txtProImportPrice.Text));
                            cmd.Parameters.AddWithValue("@Quantity", int.Parse(txtProQuantity.Text));
                            break;
                        case "Orders":
                            query = "INSERT INTO Orders (OrderDate, EmployeeID, CustomerID, TotalAmount, Status) VALUES (@OrderDate, @EmployeeID, @CustomerID, @TotalAmount, @Status)";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@OrderDate", dtpOrderDate.Value);
                            cmd.Parameters.AddWithValue("@EmployeeID", cbOrderEmployee.SelectedValue);
                            cmd.Parameters.AddWithValue("@CustomerID", cbOrderCustomer.SelectedValue);
                            cmd.Parameters.AddWithValue("@TotalAmount", 0);
                            cmd.Parameters.AddWithValue("@Status", cbOrderStatus.SelectedItem?.ToString() ?? "Pending");
                            break;
                        case "Users":
                            query = "INSERT INTO Users (Username, Password, Role, EmployeeID, CustomerID, IsFirstLogin) VALUES (@Username, @Password, @Role, @EmployeeID, @CustomerID, @IsFirstLogin)";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                            cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                            cmd.Parameters.AddWithValue("@Role", cmbRole.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@EmployeeID", string.IsNullOrWhiteSpace(txtEmployeeID.Text) ? (object)DBNull.Value : txtEmployeeID.Text);
                            cmd.Parameters.AddWithValue("@CustomerID", string.IsNullOrWhiteSpace(txtCustomerID.Text) ? (object)DBNull.Value : txtCustomerID.Text);
                            cmd.Parameters.AddWithValue("@IsFirstLogin", chkIsFirstLogin.Checked);
                            break;
                        default:
                            throw new Exception("Invalid table name");
                    }
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data added successfully!", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conne.State == ConnectionState.Open)
                    conne.Close();
                LoadTableData(tableName, dataGridView);
            }
        }

        private void UpdateData(string tableName, DataGridView dataGridView, string primaryKeyValue)
        {
            try
            {
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
                    string phonePattern = @"^[0-9\s\-]+$";
                    if (!Regex.IsMatch(txtCusPhone.Text, phonePattern))
                    {
                        MessageBox.Show("Phone must contain only digits (spaces or dashes are allowed).", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
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
                    if (string.IsNullOrWhiteSpace(txtCusCode.Text))
                    {
                        MessageBox.Show("Customer Code is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    string codePattern = @"^[a-zA-Z0-9]+$";
                    if (!Regex.IsMatch(txtCusCode.Text, codePattern))
                    {
                        MessageBox.Show("Customer Code must contain only letters and numbers (no spaces or special characters).", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!IsCustomerCodeUnique(txtCusCode.Text, primaryKeyValue))
                    {
                        MessageBox.Show("Customer Code must be unique.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    if (string.IsNullOrWhiteSpace(txtEmpCode.Text))
                    {
                        MessageBox.Show("Employee Code is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbEmpPosition.SelectedItem == null)
                    {
                        MessageBox.Show("Position is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
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
                else if (tableName == "Imports")
                {
                    if (cbImportProductID.SelectedValue == null)
                    {
                        MessageBox.Show("Please select a Product.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!int.TryParse(txtImportQuantity.Text, out int quantity) || quantity <= 0)
                    {
                        MessageBox.Show("Quantity must be a valid integer greater than 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txtImportPrice.Text, out decimal importPrice) || importPrice <= 0)
                    {
                        MessageBox.Show("Import Price must be a valid number greater than 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbImportEmployeeID.SelectedValue == null)
                    {
                        MessageBox.Show("Please select an Employee.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    if (string.IsNullOrWhiteSpace(txtProductCode.Text))
                    {
                        MessageBox.Show("Product Code is required.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txtProPrice.Text, out decimal price) || price < 0)
                    {
                        MessageBox.Show("Price must be a valid number greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txtProImportPrice.Text, out decimal importPrice) || importPrice < 0)
                    {
                        MessageBox.Show("Import Price must be a valid number greater than or equal to 0.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    if (cbOrderCustomer.SelectedValue == null)
                    {
                        MessageBox.Show("Please select a Customer.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbOrderEmployee.SelectedValue == null)
                    {
                        MessageBox.Show("Please select an Employee.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    // Validate Role value against allowed values
                    string selectedRole = cmbRole.SelectedItem.ToString();
                    string[] allowedRoles = { "ADMIN", "EMPLOYEE", "CUSTOMER" }; // Match database constraint
                    if (!allowedRoles.Contains(selectedRole))
                    {
                        MessageBox.Show("Invalid Role selected. Role must be 'ADMIN', 'EMPLOYEE', or 'CUSTOMER'.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                conne.Open();
                string query = "";
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conne;
                    switch (tableName)
                    {
                        case "Employees":
                            query = "UPDATE Employees SET EmployeeCode = @EmployeeCode, FullName = @FullName, DateOfBirth = @DateOfBirth, Gender = @Gender, Position = @Position, Address = @Address WHERE EmployeeID = @EmployeeID";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@EmployeeID", primaryKeyValue);
                            cmd.Parameters.AddWithValue("@EmployeeCode", txtEmpCode.Text);
                            cmd.Parameters.AddWithValue("@FullName", txtEmpName.Text);
                            cmd.Parameters.AddWithValue("@DateOfBirth", dtpEmpDOB.Value);
                            cmd.Parameters.AddWithValue("@Gender", cbEmpGender.SelectedItem?.ToString() ?? "Other");
                            cmd.Parameters.AddWithValue("@Position", cbEmpPosition.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@Address", txtEmpAddress.Text);
                            break;
                        case "Customers":
                            query = "UPDATE Customers SET CustomerCode = @CustomerCode, FullName = @FullName, DateOfBirth = @DateOfBirth, Address = @Address, Phone = @Phone, RegistrationDate = @RegistrationDate WHERE CustomerID = @CustomerID";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@CustomerID", primaryKeyValue);
                            cmd.Parameters.AddWithValue("@CustomerCode", txtCusCode.Text);
                            cmd.Parameters.AddWithValue("@FullName", txtCusName.Text);
                            cmd.Parameters.AddWithValue("@DateOfBirth", dtpCuDateOfBirth.Value);
                            cmd.Parameters.AddWithValue("@Address", txtCusAddress.Text);
                            cmd.Parameters.AddWithValue("@Phone", txtCusPhone.Text);
                            cmd.Parameters.AddWithValue("@RegistrationDate", dtpCusRegistrationDate.Value);
                            break;
                        case "Imports":
                            query = "UPDATE Imports SET ProductID = @ProductID, Quantity = @Quantity, ImportPrice = @ImportPrice, ImportDate = @ImportDate, EmployeeID = @EmployeeID WHERE ImportID = @ImportID";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@ImportID", primaryKeyValue);
                            cmd.Parameters.AddWithValue("@ProductID", cbImportProductID.SelectedValue);
                            cmd.Parameters.AddWithValue("@Quantity", int.Parse(txtImportQuantity.Text));
                            cmd.Parameters.AddWithValue("@ImportPrice", decimal.Parse(txtImportPrice.Text));
                            cmd.Parameters.AddWithValue("@ImportDate", dtpImportDate.Value);
                            cmd.Parameters.AddWithValue("@EmployeeID", cbImportEmployeeID.SelectedValue);
                            break;
                        case "Products":
                            query = "UPDATE Products SET ProductCode = @ProductCode, ProductName = @ProductName, Price = @Price, ImportPrice = @ImportPrice, Quantity = @Quantity WHERE ProductID = @ProductID";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@ProductID", primaryKeyValue);
                            cmd.Parameters.AddWithValue("@ProductCode", txtProductCode.Text);
                            cmd.Parameters.AddWithValue("@ProductName", txtProName.Text);
                            cmd.Parameters.AddWithValue("@Price", decimal.Parse(txtProPrice.Text));
                            cmd.Parameters.AddWithValue("@ImportPrice", decimal.Parse(txtProImportPrice.Text));
                            cmd.Parameters.AddWithValue("@Quantity", int.Parse(txtProQuantity.Text));
                            break;
                        case "Orders":
                            query = "UPDATE Orders SET OrderDate = @OrderDate, EmployeeID = @EmployeeID, CustomerID = @CustomerID, TotalAmount = @TotalAmount, Status = @Status WHERE OrderID = @OrderID";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@OrderID", primaryKeyValue);
                            cmd.Parameters.AddWithValue("@OrderDate", dtpOrderDate.Value);
                            cmd.Parameters.AddWithValue("@EmployeeID", cbOrderEmployee.SelectedValue);
                            cmd.Parameters.AddWithValue("@CustomerID", cbOrderCustomer.SelectedValue);
                            cmd.Parameters.AddWithValue("@TotalAmount", string.IsNullOrWhiteSpace(txtOrderTotal.Text) ? 0 : decimal.Parse(txtOrderTotal.Text));
                            cmd.Parameters.AddWithValue("@Status", cbOrderStatus.SelectedItem?.ToString() ?? "Pending");
                            break;
                        case "Users":
                            query = "UPDATE Users SET Username = @Username, Password = @Password, Role = @Role, EmployeeID = @EmployeeID, CustomerID = @CustomerID, IsFirstLogin = @IsFirstLogin WHERE UserID = @UserID";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@UserID", primaryKeyValue);
                            cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                            cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                            cmd.Parameters.AddWithValue("@Role", cmbRole.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@EmployeeID", string.IsNullOrWhiteSpace(txtEmployeeID.Text) ? (object)DBNull.Value : txtEmployeeID.Text);
                            cmd.Parameters.AddWithValue("@CustomerID", string.IsNullOrWhiteSpace(txtCustomerID.Text) ? (object)DBNull.Value : txtCustomerID.Text);
                            cmd.Parameters.AddWithValue("@IsFirstLogin", chkIsFirstLogin.Checked);
                            break;
                        default:
                            throw new Exception("Invalid table name");
                    }
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data updated successfully!", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conne.State == ConnectionState.Open)
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
                using (SqlCommand cmd = new SqlCommand(query, conne))
                {
                    cmd.Parameters.AddWithValue("@primaryKey", primaryKeyValue);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data deleted successfully!", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE constraint"))
                {
                    MessageBox.Show($"Cannot delete this record because it is referenced by another table.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Error deleting data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (conne.State == ConnectionState.Open)
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
                    LoadTableData(tableName, dataGridView);
                    return;
                }

                conne.Open();
                string query = "";
                switch (tableName)
                {
                    case "Employees":
                        query = "SELECT EmployeeID, EmployeeCode, FullName, DateOfBirth, Gender, Position, Address FROM Employees " +
                                "WHERE CAST(EmployeeID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(EmployeeCode) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(FullName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(DateOfBirth AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Gender) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Position) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Address) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS";
                        break;
                    case "Customers":
                        query = "SELECT CustomerID, CustomerCode, FullName, DateOfBirth, Address, Phone, RegistrationDate FROM Customers " +
                                "WHERE CAST(CustomerID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(CustomerCode) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(FullName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(DateOfBirth AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Address) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Phone) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(RegistrationDate AS NVARCHAR) LIKE @keyword";
                        break;
                    case "Imports":
                        query = "SELECT ImportID, ProductID, Quantity, ImportPrice, ImportDate, EmployeeID FROM Imports " +
                                "WHERE CAST(ImportID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(ProductID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(Quantity AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(ImportPrice AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(ImportDate AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(EmployeeID AS NVARCHAR) LIKE @keyword";
                        break;
                    case "Products":
                        query = "SELECT ProductID, ProductCode, ProductName, Price, ImportPrice, Quantity FROM Products " +
                                "WHERE CAST(ProductID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(ProductCode) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(ProductName) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(Price AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(ImportPrice AS NVARCHAR) LIKE @keyword " +
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
                        query = "SELECT UserID, Username, Role, EmployeeID, CustomerID, IsFirstLogin FROM Users " +
                                "WHERE CAST(UserID AS NVARCHAR) LIKE @keyword " +
                                "OR TRIM(Username) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR TRIM(Role) LIKE @keyword COLLATE SQL_Latin1_General_CP1_CI_AS " +
                                "OR CAST(EmployeeID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(CustomerID AS NVARCHAR) LIKE @keyword " +
                                "OR CAST(IsFirstLogin AS NVARCHAR) LIKE @keyword";
                        break;
                    default:
                        throw new Exception("Invalid table name");
                }

                using (SqlCommand cmd = new SqlCommand(query, conne))
                {
                    cmd.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = "%" + searchValue + "%";
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching data: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conne.State == ConnectionState.Open)
                    conne.Close();
            }
        }

        private void ClearFields(string tableName)
        {
            switch (tableName)
            {
                case "Employees":
                    txtEmpID.Clear();
                    txtEmpCode.Clear();
                    txtEmpName.Clear();
                    txtEmpAddress.Clear();
                    dtpEmpDOB.Value = DateTime.Now;
                    cbEmpGender.SelectedIndex = -1;
                    cbEmpPosition.SelectedIndex = -1;
                    break;
                case "Customers":
                    txtCusID.Clear();
                    txtCusCode.Clear();
                    txtCusName.Clear();
                    txtCusPhone.Clear();
                    txtCusAddress.Clear();
                    dtpCuDateOfBirth.Value = DateTime.Now;
                    dtpCusRegistrationDate.Value = DateTime.Now;
                    break;
                case "Imports":
                    txtImportID.Clear();
                    cbImportProductID.SelectedIndex = -1;
                    txtImportQuantity.Clear();
                    txtImportPrice.Clear();
                    dtpImportDate.Value = DateTime.Now;
                    cbImportEmployeeID.SelectedIndex = -1;
                    break;
                case "Products":
                    txtProID.Clear();
                    txtProductCode.Clear();
                    txtProName.Clear();
                    txtProPrice.Clear();
                    txtProImportPrice.Clear();
                    txtProQuantity.Clear();
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
                    chkIsFirstLogin.Checked = false;
                    break;
            }
        }

        // Event Handlers for DataGridView SelectionChanged
        private void dgvEmployees_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvEmployees.SelectedRows[0];
                txtEmpID.Text = row.Cells["EmployeeID"].Value.ToString();
                txtEmpCode.Text = row.Cells["EmployeeCode"].Value.ToString();
                txtEmpName.Text = row.Cells["FullName"].Value.ToString();
                dtpEmpDOB.Value = Convert.ToDateTime(row.Cells["DateOfBirth"].Value);
                cbEmpGender.SelectedItem = row.Cells["Gender"].Value.ToString();
                cbEmpPosition.SelectedItem = row.Cells["Position"].Value.ToString();
                txtEmpAddress.Text = row.Cells["Address"].Value.ToString();
            }
        }

        private void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvCustomers.SelectedRows[0];
                txtCusID.Text = row.Cells["CustomerID"].Value.ToString();
                txtCusCode.Text = row.Cells["CustomerCode"].Value.ToString();
                txtCusName.Text = row.Cells["FullName"].Value.ToString();
                dtpCuDateOfBirth.Value = Convert.ToDateTime(row.Cells["DateOfBirth"].Value);
                txtCusAddress.Text = row.Cells["Address"].Value.ToString();
                txtCusPhone.Text = row.Cells["Phone"].Value.ToString();
                dtpCusRegistrationDate.Value = Convert.ToDateTime(row.Cells["RegistrationDate"].Value);
            }
        }

        private void dgvImports_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvImports.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvImports.SelectedRows[0];
                txtImportID.Text = row.Cells["ImportID"].Value.ToString();
                cbImportProductID.SelectedValue = row.Cells["ProductID"].Value;
                txtImportQuantity.Text = row.Cells["Quantity"].Value.ToString();
                txtImportPrice.Text = row.Cells["ImportPrice"].Value.ToString();
                dtpImportDate.Value = Convert.ToDateTime(row.Cells["ImportDate"].Value);
                cbImportEmployeeID.SelectedValue = row.Cells["EmployeeID"].Value;
            }
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvProducts.SelectedRows[0];
                txtProID.Text = row.Cells["ProductID"].Value.ToString();
                txtProductCode.Text = row.Cells["ProductCode"].Value.ToString();
                txtProName.Text = row.Cells["ProductName"].Value.ToString();
                txtProPrice.Text = row.Cells["Price"].Value.ToString();
                txtProImportPrice.Text = row.Cells["ImportPrice"].Value.ToString();
                txtProQuantity.Text = row.Cells["Quantity"].Value.ToString();
            }
        }

        private void dgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvOrders.SelectedRows[0];
                txtOrderID.Text = row.Cells["OrderID"].Value.ToString();
                dtpOrderDate.Value = Convert.ToDateTime(row.Cells["OrderDate"].Value);
                cbOrderEmployee.SelectedValue = row.Cells["EmployeeID"].Value;
                cbOrderCustomer.SelectedValue = row.Cells["CustomerID"].Value;
                txtOrderTotal.Text = row.Cells["TotalAmount"].Value.ToString();
                cbOrderStatus.SelectedItem = row.Cells["Status"].Value.ToString();
            }
        }

        private void dgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvUsers.SelectedRows[0];
                txtUserID.Text = row.Cells["UserID"].Value.ToString();
                txtUsername.Text = row.Cells["Username"].Value.ToString();
                txtPassword.Text = string.Empty; // Không hiển thị mật khẩu
                cmbRole.SelectedItem = row.Cells["Role"].Value.ToString();
                txtEmployeeID.Text = row.Cells["EmployeeID"].Value?.ToString() ?? string.Empty;
                txtCustomerID.Text = row.Cells["CustomerID"].Value?.ToString() ?? string.Empty;
                chkIsFirstLogin.Checked = Convert.ToBoolean(row.Cells["IsFirstLogin"].Value);
            }
        }

        // Event Handlers for Buttons
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTabData();
        }

        private void btnEmpAdd_Click(object sender, EventArgs e)
        {
            AddData("Employees", dgvEmployees);
        }

        private void btnCusAdd_Click(object sender, EventArgs e)
        {
            AddData("Customers", dgvCustomers);
        }

        private void btnImportAdd_Click(object sender, EventArgs e)
        {
            AddData("Imports", dgvImports);
        }

        private void btnProAdd_Click(object sender, EventArgs e)
        {
            AddData("Products", dgvProducts);
        }

        private void btnOrderAdd_Click(object sender, EventArgs e)
        {
            AddData("Orders", dgvOrders);
        }

        private void btnUserAdd_Click(object sender, EventArgs e)
        {
            AddData("Users", dgvUsers);
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

        private void btnImportEdit_Click(object sender, EventArgs e)
        {
            if (dgvImports.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvImports.SelectedRows[0].Cells["ImportID"].Value.ToString();
            UpdateData("Imports", dgvImports, primaryKeyValue);
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

        private void btnOrderEdit_Click(object sender, EventArgs e)
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

        private void btnImportDelete_Click(object sender, EventArgs e)
        {
            if (dgvImports.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string primaryKeyValue = dgvImports.SelectedRows[0].Cells["ImportID"].Value.ToString();
            DeleteData("Imports", dgvImports, "ImportID", primaryKeyValue);
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

        private void btnOrderDelete_Click(object sender, EventArgs e)
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

        private void btnEmpSearch_Click(object sender, EventArgs e)
        {
            SearchData("Employees", dgvEmployees, txtEmpSearch.Text);
        }

        private void btnCusSearch_Click(object sender, EventArgs e)
        {
            SearchData("Customers", dgvCustomers, txtSearch.Text);
        }

        private void btnImportSearch_Click(object sender, EventArgs e)
        {
            SearchData("Imports", dgvImports, txtSearchImports.Text);
        }

        private void btnProSearch_Click(object sender, EventArgs e)
        {
            SearchData("Products", dgvProducts, txtProSearch.Text);
        }

        private void btnOrderSearch_Click(object sender, EventArgs e)
        {
            SearchData("Orders", dgvOrders, txtOrderSearch.Text);
        }

        private void btnUserSearch_Click(object sender, EventArgs e)
        {
            SearchData("Users", dgvUsers, txtUserSearch.Text);
        }

        private void btnLogoutEmployees_Click(object sender, EventArgs e)
        {
            this.Close();
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
        }

        private void btnLogoutCustomers_Click(object sender, EventArgs e)
        {
            btnLogoutEmployees_Click(sender, e);
        }

        private void btnLogoutImports_Click(object sender, EventArgs e)
        {
            btnLogoutEmployees_Click(sender, e);
        }

        private void btnLogoutProducts_Click(object sender, EventArgs e)
        {
            btnLogoutEmployees_Click(sender, e);
        }

        private void btnLogoutOrders_Click(object sender, EventArgs e)
        {
            btnLogoutEmployees_Click(sender, e);
        }

        private void btnLogoutUsers_Click(object sender, EventArgs e)
        {
            btnLogoutEmployees_Click(sender, e);
        }

        private void Management_Load(object sender, EventArgs e)
        {
            // Có thể thêm logic khởi tạo nếu cần
        }
    }
}
