using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BooKu
{
    public partial class Dashboard : Form
    {

        private BooKuEntities _context = new BooKuEntities();
        private int maxPage = 0;
        private int currentPage = 0;
        private int currentCover = 0;

        public Dashboard()
        {
            InitializeComponent();
        }

        public void refresh()
        {
            if (button3.Text == "Cancel")
            {
                search();
                return;
            }

            var books = _context.Books
                .Select(s => new
                {
                    Title = s.Title,
                    Authors = s.BookAuthors.Select(x => x.Author.Name).ToList()
                })
                .ToList() 
                .Select(s => new
                {
                    Title = s.Title,
                    Authors = String.Join(", ", s.Authors)
                })
                .ToList(); maxPage = (int) Math.Floor((decimal) books.Count/8);
            dataGridView1.DataSource = books.Skip(8 * currentPage).Take(8).ToList();

            lbCount.Text = $"{books.ToList().Count} Books found";
            lbPage.Text = (currentPage + 1).ToString();
        }

        public void search()
        {
            var books = _context.Books.AsEnumerable()
                .Where(s => 
                (string.IsNullOrEmpty(tbTitle.Text) || s.Title.Contains(tbTitle.Text)) 
                && (string.IsNullOrEmpty(tbAuthor.Text) || s.BookAuthors.Any(x => x.Author.Name.Contains(tbAuthor.Text)))
                && (string.IsNullOrEmpty(tbSubject.Text) || s.BookSubjects.Any(x => x.Subject.Contains(tbSubject.Text))))
                .Select(s => new { Title = s.Title, Authors = String.Join(", ", s.BookAuthors.Select(x => x.Author.Name).ToList()) });
            maxPage = (int)Math.Floor((decimal)books.ToList().Count / 8);
            dataGridView1.DataSource = books.Skip(8 * currentPage).Take(8).ToList();

            lbCount.Text = $"{books.ToList().Count} Books found";
            lbPage.Text = (currentPage + 1).ToString();
            button3.Text = "Cancel";
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            valid();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.RowHeadersVisible = false;
            refresh();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentPage < maxPage) {
                currentPage += 1;
                refresh();
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage -= 1;
                refresh();
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            var title = row.Cells[0].Value.ToString();
            var book = _context.Books.FirstOrDefault(s => s.Title == title);

            if (book != null) {
                lbTitle.Text = book.Title.ToString();
                lbAuthors.Text = $"By {String.Join(",", book.BookAuthors.Select(s => s.Author.Name))}";
                lbDate.Text = $"first published: {book.FirstPublished}";
                tbDesc.Text = book.Description;
                lbSubject.Text = String.Join(",", book.BookSubjects.Select(s => s.Subject));

                var covers = book.BookCovers.Where(s => s.BookID == book.ID).Select(s => s).ToList();
                pbCovers.Tag = covers;

                currentCover = 0;
                changeCover();
            }

        }

        private void changeCover()
        {
            var image_directory = Path.Combine(Directory.GetCurrentDirectory(), "images");
            var covers = pbCovers.Tag as List<BookCover>;

            if (covers.Count > 0)
            {
                var image_path = Path.Combine(image_directory, covers[currentCover].ImagePath);

                if (image_path != null)
                {
                    pbCovers.Image = Image.FromFile(image_path);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (currentCover > 0)
            {
                currentCover -= 1;
                changeCover();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var covers = pbCovers.Tag as List<BookCover>;
            if (currentCover < covers.Count -1)
            {
                currentCover += 1;
                changeCover();
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "Search")
            {
                currentPage = 0;
                search();
            }
            else
            {
                currentPage = 0;
                button3.Text = "Search";
                refresh();
            }
        }

        private void lbAuthors_Click(object sender, EventArgs e)
        {
            if (lbTitle.Text != "Title")
            {
                FormAuthor author = new FormAuthor(lbTitle.Text);
                author.ShowDialog();
            }
        }

        private void closeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormLogin formLogin = new FormLogin();
            formLogin.Disposed += (a, b) =>
            {
                valid();
            };
            formLogin.ShowDialog();
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Session.user = null;
            valid();
        }

        private void valid()
        {
            if (Session.user != null)
            {
                loginToolStripMenuItem.Visible = false;
                logoutToolStripMenuItem.Visible = true;
                libraryToolStripMenuItem.Visible = true;
            }
            else
            {
                loginToolStripMenuItem.Visible = true;
                logoutToolStripMenuItem.Visible = false;
                libraryToolStripMenuItem.Visible = false;
            }
        }

        private void manageBooksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManageForm manageForm = new ManageForm();
            manageForm.FormClosed += (a, b) =>
            {
                refresh();
            };
            manageForm.Disposed += (a, b) =>
            {
                search();
            };
            manageForm.ShowDialog();
        }
    }
}
