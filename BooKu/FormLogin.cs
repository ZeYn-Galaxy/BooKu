using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BooKu
{
    public partial class FormLogin : Form
    {
        private BooKuEntities _context = new BooKuEntities();

        public FormLogin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbEmail.Text))
            {
                MessageBox.Show("Please input email");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                MessageBox.Show("Please input password");
                return;
            }

            var user = _context.Employees.FirstOrDefault(s => s.Email == tbEmail.Text); 
            if (user == null)
            {
                MessageBox.Show("Email not valid");
                return;
            }

            if (user.Password != hash())
            {
                MessageBox.Show("Password is wrong");
                return;
            }

            Session.user = user;
            this.Dispose();
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            tbEmail.Text = "mahdi@gmail.com";
            tbPassword.Text = "mahdi123";
        }

        private string hash()
        {
            SHA256 sHA256 = SHA256.Create();
            var bytes = sHA256.ComputeHash(Encoding.UTF8.GetBytes(tbPassword.Text));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
