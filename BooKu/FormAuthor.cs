using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BooKu
{
    public partial class FormAuthor : Form
    {
        private BooKuEntities _context = new BooKuEntities();
        private string title = "";
        public FormAuthor(string title)
        {
            InitializeComponent();
            this.title = title;
            this.Text = title;
        }

        private void FormAuthor_Load(object sender, EventArgs e)
        {
            var book = _context.Books.FirstOrDefault(s => s.Title == this.title);

            if (book == null)
            {
                this.Dispose();
            }


            var authors = book.BookAuthors.Select(s => s.Author.Name).ToArray();
            cbAuthor.Items.AddRange(authors);
            if (cbAuthor.Items.Count > 0)
            {
                cbAuthor.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void cbAuthor_SelectedIndexChanged(object sender, EventArgs e)
        {
            var author = _context.Authors.FirstOrDefault(s => s.Name == cbAuthor.Text);

            if (author == null)
            {
                return;
            }

            lbDate.Text = author.BirthDate.ToString("d MMM yyyy");
            lbBio.Text = author.Bio;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
