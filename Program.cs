using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SalesManagement
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Bước 1: Mở form LoginForm
            LoginForm loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Lấy vai trò người dùng từ LoginForm
                string userRole = loginForm.UserRole;

                // Bước 2: Mở MainForm sau khi đăng nhập thành công
                MainForm mainForm = new MainForm();
                mainForm.FormClosed += (s, args) => Application.Exit();

                // Bước 3: Mở ManagementForm và truyền userRole
                Management managementForm = new Management(userRole);
                managementForm.FormClosed += (s, args) =>
                {
                    if (!mainForm.IsDisposed)
                    {
                        mainForm.Show();
                    }
                };

                mainForm.Show();
                mainForm.Hide();
                managementForm.Show();

                Application.Run(mainForm);
            }
            else
            {
                Application.Exit();
            }
        }
    }
}