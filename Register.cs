using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using BCrypt.Net;
namespace SalesManagement

{
    public partial class Register : Form
    {
        SqlConnection connect;

        public Register()
        {
            InitializeComponent();
            Connection();
        }

        private string strConnection = "Server=0989078698\\SQLEXPRESS;Database=SalesManagement;Integrated Security=True; Trust Server Certificate=True";

        private void Connection()
        {
            try
            {
                connect = new SqlConnection(strConnection);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating connection: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string userName = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConPassword.Text;

            // Kiểm tra đầu vào
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please enter complete information!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Password and confirmation password do not match!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra mật khẩu không chứa ký tự đặc biệt
            if (!IsValidPassword(password))
            {
                MessageBox.Show("Password must not contain special characters! Only letters and numbers are allowed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                connect.Open();

                // Kiểm tra xem username đã tồn tại chưa
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE UserName = @UserName";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, connect))
                {
                    checkCmd.Parameters.AddWithValue("@UserName", userName);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Username already exists!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Lưu mật khẩu dưới dạng plain text (không mã hóa)
                string insertQuery = "INSERT INTO Users (UserName, Password, Role) VALUES (@UserName, @Password, @Role)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, connect))
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@Password", password); // Lưu mật khẩu dạng plain text
                    cmd.Parameters.AddWithValue("@Role", "Customer");

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Registration successful! Please log in.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during registration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connect.State == ConnectionState.Open)
                {
                    connect.Close();
                }
            }
        }

        private bool IsValidPassword(string password)
        {
            // Chỉ cho phép chữ cái (a-z, A-Z) và số (0-9)
            return System.Text.RegularExpressions.Regex.IsMatch(password, @"^[a-zA-Z0-9]+$");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
