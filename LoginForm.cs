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
namespace SalesManagement
{
    public partial class LoginForm : Form
    {
        SqlConnection conne;
        public string UserRole { get; private set; }  // Thêm thuộc tính UserRole

        public LoginForm()
        {
            InitializeComponent();
            Connection();
        }

        private void Connection()
        {
            try
            {
                String strConnection = "Server=0989078698\\SQLEXPRESS;Database=SalesManagement;Integrated Security=True; Trust Server Certificate=True";
                conne = new SqlConnection(strConnection);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while establishing connection: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

             string adminUser = "Duc";
             string adminPass = "123";

            if (username == adminUser && password == adminPass)
            {
                UserRole = "Admin";
                MessageBox.Show("Login successful with Admin privileges!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                return;
            }

            try
            {
                conne.Open();
                string query = "SELECT Role FROM [Users] WHERE Username = @UserName AND Password = @Password";
                using (SqlCommand cmd = new SqlCommand(query, conne))
                {
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    object role = cmd.ExecuteScalar();
                    if (role != null)
                    {
                        UserRole = role.ToString();
                        MessageBox.Show("Login successful!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show("Incorrect username or password!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ClearLogin();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conne.State == System.Data.ConnectionState.Open)
                {
                    conne.Close();
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            // Close the form and exit the application
            this.Close();
            Application.Exit();
        }

        private void ClearLogin()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtUsername.Focus();
        }

        private void linklbRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                using (Register register = new Register())
                {
                    register.ShowDialog(); // Open Register as a dialog
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while opening Register form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
