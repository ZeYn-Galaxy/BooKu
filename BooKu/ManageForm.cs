using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BooKu
{
    public partial class ManageForm : Form
    {
        private BooKuEntities _context = new BooKuEntities();

        public ManageForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void ManageForm_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;

            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.AllowUserToAddRows = false;

            dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView3.RowHeadersVisible = false;
            dataGridView3.AllowUserToAddRows = false;

            loadBooks();


            // Author
            var authors = _context.Authors.Select(s => s.Name).ToArray();
            cbAuthor.Items.AddRange(authors);

            // Subject
            var subjects = _context.BookSubjects.Select(s => s.Subject).ToArray();
            cbSubjects.Items.AddRange(subjects);

        }

        private void loadCovers()
        {
            var book = dataGridView1.Tag as Book;
            var image_directory = Path.Combine(Directory.GetCurrentDirectory(), "images");
            var covers = _context.BookCovers.Where(s => s.BookID == book.ID).Select(s => s).ToList().Select(s => new
            {
                id = s.ID,
                cover = new Bitmap(Image.FromFile(Path.Combine(image_directory, s.ImagePath))),
            }).ToList();

            dataGridView4.DataSource = covers;

        }

        private void loadBooks()
        {
            var books = _context.Books.Select(s => new
            {
                Title = s.Title,
                Description = s.Description,
                First_Published = s.FirstPublished
            });

            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = books.ToList();


            // Edit
            DataGridViewButtonColumn edit = new DataGridViewButtonColumn();
            edit.Name = "Edit";
            edit.HeaderText = "Edit";
            edit.Text = "Edit";
            edit.UseColumnTextForButtonValue = true;

            // Hapus
            DataGridViewButtonColumn hapus = new DataGridViewButtonColumn();
            hapus.Name = "Delete";
            hapus.HeaderText = "Delete";
            hapus.Text = "Delete";
            hapus.UseColumnTextForButtonValue = true;

            dataGridView1.Columns.Add(edit);
            dataGridView1.Columns.Add(hapus);
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

            if (book == null)
            {
                MessageBox.Show("Book not found");
                return;
            }

            dataGridView1.Tag = book;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Edit")
            {
                var desc = row.Cells[1].Value.ToString();
                var first = row.Cells[2].Value.ToString();

                tbTitle.Text = title;
                tbDesc.Text = desc;
                tbFirst.Value = int.Parse(first);
                button6.Text = "Edit Book";
                return;
            }


            if (dataGridView1.Columns[e.ColumnIndex].Name == "Delete")
            {
                try
                {
                    DialogResult res = MessageBox.Show("Are you sure", "delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (res == DialogResult.Yes)
                    {
                        _context.Books.Remove(book);
                        _context.SaveChanges();
                        loadBooks();
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("can't delete the book");
                }
                return;
            }

            loadAuthors();
            loadSubjects();
            loadCovers();
        }

        private void loadAuthors()
        {
            var book = dataGridView1.Tag as Book;
            // Authors
            var authors = book.BookAuthors.Select(s => new
            {
                s.Author.Name,
                s.Author.Bio,
                s.Author.BirthDate
            });

            dataGridView2.Columns.Clear();
            dataGridView2.DataSource = authors.ToList();

            DataGridViewButtonColumn hapus = new DataGridViewButtonColumn();
            hapus.Name = "Delete";
            hapus.HeaderText = "Delete";
            hapus.Text = "Delete";
            hapus.UseColumnTextForButtonValue = true;
            dataGridView2.Columns.Add(hapus);
        }

        private void loadSubjects()
        {
            var book = dataGridView1.Tag as Book;
            // Subjects
            var subjects = book.BookSubjects.Select(s => new
            {
                s.Subject
            });

            dataGridView3.Columns.Clear();
            dataGridView3.DataSource = subjects.ToList();

            DataGridViewButtonColumn hapus = new DataGridViewButtonColumn();
            hapus.Name = "Delete";
            hapus.HeaderText = "Delete";
            hapus.Text = "Delete";
            hapus.UseColumnTextForButtonValue = true;
            dataGridView3.Columns.Add(hapus);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (cbAuthor.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the author");
                return;
            }

            var author = _context.Authors.FirstOrDefault(s => s.Name == cbAuthor.Text);
            if (author == null)
            {
                MessageBox.Show("Author not found");
                return;
            }

            var book = dataGridView1.Tag as Book;

            var exist = _context.BookAuthors.FirstOrDefault(s => s.BookID == book.ID && s.AuthorID == author.ID);

            if (exist != null)
            {
                MessageBox.Show("Author already added");
                return;
            }

            var newAuthor = new BookAuthor
            {
                AuthorID = author.ID,
                BookID = book.ID
            };

            _context.BookAuthors.Add(newAuthor);
            _context.SaveChanges();
            loadAuthors();
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            if (dataGridView2.Columns[e.ColumnIndex].Name == "Delete")
            {
                DialogResult res = MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                {

                    var book = dataGridView1.Tag as Book;
                    var name = dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString();
                    var author = _context.Authors.FirstOrDefault(s => s.Name == name);

                    if (author == null)
                    {
                        MessageBox.Show("Author not found");
                        return;
                    };

                    var bookAuthor = _context.BookAuthors.FirstOrDefault(s => s.AuthorID == author.ID && s.BookID == book.ID);

                    if (bookAuthor != null)
                    {
                        _context.BookAuthors.Remove(bookAuthor);
                        _context.SaveChanges();
                        loadAuthors();
                    }

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (cbSubjects.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the subject");
                return;
            }


            var book = dataGridView1.Tag as Book;

            var exist = _context.BookSubjects.FirstOrDefault(s => s.BookID == book.ID && s.Subject == cbSubjects.Text);

            if (exist != null)
            {
                MessageBox.Show("Subject already added");
                return;
            }

            var newSubject = new BookSubject
            {
                BookID = book.ID,
                Subject = cbSubjects.Text
            };

            _context.BookSubjects.Add(newSubject);
            _context.SaveChanges();
            loadSubjects();
        }

        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            if (dataGridView3.Columns[e.ColumnIndex].Name == "Delete")
            {
                DialogResult res = MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                {

                    var book = dataGridView1.Tag as Book;
                    var subject = dataGridView3.Rows[e.RowIndex].Cells[0].Value.ToString();
                    var remove = _context.BookSubjects.FirstOrDefault(s => s.Subject == subject && s.BookID == book.ID);

                    if (remove == null)
                    {
                        MessageBox.Show("subject not found");
                        return;
                    };

                    _context.BookSubjects.Remove(remove);
                    _context.SaveChanges();
                    loadSubjects();

                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select Image",
                Filter = "Image Files | *.png; *.jpg; *.jpeg;"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                button5.Tag = ofd;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (button5.Tag == null)
            {
                return;
            }

            var ofd = button5.Tag as OpenFileDialog;
            var book = dataGridView1.Tag as Book;
            var image_directory = Path.Combine(Directory.GetCurrentDirectory(), "images");
            var filename = $"{Guid.NewGuid()}{Path.GetExtension(ofd.FileName)}";
            var image_path = Path.Combine(image_directory, filename);
            File.Copy(ofd.FileName, image_path, true);

            BookCover bookCover = new BookCover
            {
                BookID = book.ID,
                ImagePath = filename,
            };

            _context.BookCovers.Add(bookCover);
            _context.SaveChanges();
            loadCovers();
        }

        private void dataGridView4_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            if (dataGridView4.Columns[e.ColumnIndex].Name == "Delete")
            {
                DialogResult res = MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                {
                    var book = dataGridView1.Tag as Book;
                    var id = int.Parse(dataGridView4.Rows[e.RowIndex].Cells["ID"].Value.ToString());
                    var cover = _context.BookCovers.FirstOrDefault(s => s.ID == id);
                    if (cover != null)
                    {
                        var image_directory = Path.Combine(Directory.GetCurrentDirectory(), "images");
                        var image_path = Path.Combine(image_directory, cover.ImagePath);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();


                        File.Delete(image_path);
                        _context.BookCovers.Remove(cover);
                        _context.SaveChanges();
                        loadCovers();
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTitle.Text) || string.IsNullOrWhiteSpace(tbDesc.Text) || tbFirst.Value == 0) {
                MessageBox.Show("Please input the field");
                return;
            }

            if (button6.Text == "Add Book")
            {
                var newBook = new Book
                {
                    Title = tbTitle.Text,
                    Description = tbDesc.Text,
                    FirstPublished = (int) tbFirst.Value
                };


                _context.Books.Add(newBook);
                _context.SaveChanges();

                tbTitle.Clear();
                tbDesc.Clear();
                tbFirst.Value = 0;
                loadBooks();
            } else
            {
                var book = dataGridView1.Tag as Book;
                book.Title = tbTitle.Text;
                book.Description = tbDesc.Text;
                book.FirstPublished = (int) tbFirst.Value;

                _context.SaveChanges();

                tbTitle.Clear();
                tbDesc.Clear();
                tbFirst.Value = 0;
                loadBooks();
                button6.Text = "Add Book";
            }
        }
    }
}
