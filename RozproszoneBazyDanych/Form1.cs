using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace RozproszoneBazyDanych
{
    public partial class Form1 : Form
    {
        SqlConnection connection;
        string connectionString;
        public Form1()
        {
            InitializeComponent();
            
            connectionString = "data source = DESKTOP-EV3588L\\PAWEL; database = biblioteka; integrated security = sspi";
            connection = new SqlConnection(connectionString);

            SqlCommand cmd = new SqlCommand("select * from zbiorKsiazek", connection);
            connection.Open();
            SqlDataReader DR = cmd.ExecuteReader();

            BindingSource source = new BindingSource();
            source.DataSource = DR;

            connection.Close();
        }

        //Refresh ViewBoxes method
        private void button3_Click(object sender, EventArgs e)
        {
            using (connection = new SqlConnection(connectionString))
            using (SqlDataAdapter booksAdapter = new SqlDataAdapter("SELECT zbiorKsiazek.tytul, zbiorKsiazek.autor, iloscKsiazek.ilosc FROM zbiorKsiazek INNER JOIN iloscKsiazek ON zbiorKsiazek.id = iloscKsiazek.idZbioru", connection))
            using (SqlDataAdapter ordersAdapter = new SqlDataAdapter("SELECT klient.imie, klient.nazwisko, wypozyczenie.idKsiazka, wypozyczenie.data_wyp, wypozyczenie.data_odd, wypozyczenie.id FROM wypozyczenie INNER JOIN klient ON wypozyczenie.idKlient = klient.id",connection))
            {
                DataTable books = new DataTable();
                DataTable orders = new DataTable();

                booksAdapter.Fill(books);
                ordersAdapter.Fill(orders);

                listView1.Items.Clear();
                listView2.Items.Clear();
                for (int i = 0; i < books.Rows.Count; i++)
                {
                    DataRow drow = books.Rows[i];                  
                    if (drow.RowState != DataRowState.Deleted)
                    {     
                        ListViewItem lvi = new ListViewItem(drow[0].ToString());
                        lvi.SubItems.Add(drow[1].ToString());
                        lvi.SubItems.Add(drow[2].ToString());
                        listView1.Items.Add(lvi); 
                    }
                }
                for (int i = 0; i < orders.Rows.Count; i++)
                {
                    DataRow drow = orders.Rows[i];
                    if (drow.RowState != DataRowState.Deleted)
                    {
                        ListViewItem lvi = new ListViewItem(drow[0].ToString()+" "+drow[1].ToString());
                        lvi.SubItems.Add(drow[2].ToString());

                        DateTime date = DateTime.Parse(drow[3].ToString());
                        date = date.Date;
                        lvi.SubItems.Add(date.ToShortDateString());

                        if(drow[4] != DBNull.Value)
                        {
                            date = DateTime.Parse(drow[4].ToString());
                            date = date.Date;
                            lvi.SubItems.Add(date.ToShortDateString());
                        }
                        else
                        {
                            lvi.SubItems.Add("-");
                        }
                        lvi.SubItems.Add(drow[5].ToString());
                        listView2.Items.Add(lvi);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddOrder orderForm = new AddOrder();
            orderForm.ShowDialog();

        }

        //Add book method
        private void button2_Click(object sender, EventArgs e)
        {
            AddBook secondForm = new AddBook();
            secondForm.ShowDialog();

            bool bookExist = false;
            int idZbioru = 0;
            // MessageBox.Show("Form 1 Message : " + AddBook.t.Text + AddBook.t2.Text);
            string query = "SELECT TOP 1 zbiorKsiazek.tytul, zbiorKsiazek.id FROM zbiorKsiazek WHERE tytul = @tytulParam";
            using (connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@tytulParam", SqlDbType.VarChar, 100).Value = AddBook.t2.Text;

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            bookExist = true;
                            idZbioru = (int)reader[1];                            
                        }
                    }
                    else
                        bookExist = false;
                }
                if (bookExist)
                {
                    string increaseBookCountQuery = "UPDATE iloscKsiazek SET ilosc = ilosc + 1 WHERE idZbioru = @idZbioruParam";
                    using (SqlCommand increaseCmd = new SqlCommand(increaseBookCountQuery, connection))
                    {
                        increaseCmd.Parameters.Add("@idZbioruParam", SqlDbType.Int).Value = idZbioru;
                        increaseCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string addToBookSet = "INSERT INTO zbiorKsiazek (tytul, autor) output INSERTED.id VALUES (@tytulParam, @autorParam)";
                    string increaseBookCountQuery = "INSERT INTO iloscKsiazek (idZbioru, ilosc) VALUES (@idZbioruParam, @iloscParam)";
                    using (SqlCommand addToSetCommand = new SqlCommand(addToBookSet, connection))
                    {
                        addToSetCommand.Parameters.AddWithValue("@tytulParam", AddBook.t2.Text);
                        addToSetCommand.Parameters.AddWithValue("@autorParam", AddBook.t.Text);
                        idZbioru = (int)addToSetCommand.ExecuteScalar();
                    }
                    using (SqlCommand increaseCmd = new SqlCommand(increaseBookCountQuery, connection))
                    {
                        increaseCmd.Parameters.Add("@idZbioruParam", SqlDbType.Int).Value = idZbioru;
                        increaseCmd.Parameters.Add("@iloscParam", SqlDbType.Int).Value = 1;
                        increaseCmd.ExecuteNonQuery();
                    }
                }

                string addBook = "INSERT INTO ksiazka (idZbioru, dostepnosc, lokalizacja) VALUES (@idZbioruParam, @dostepnoscParam, @lokalizacjaParam)";
                using (SqlCommand addBookCmd = new SqlCommand(addBook, connection))
                {
                    addBookCmd.Parameters.AddWithValue("@idZbioruParam", idZbioru);
                    addBookCmd.Parameters.AddWithValue("@dostepnoscParam", true);
                    addBookCmd.Parameters.AddWithValue("@lokalizacjaParam", AddBook.t3.Text);
                    addBookCmd.ExecuteNonQuery();
                }
                MessageBox.Show("Poprawnie dodano:" +
                    "\n Tytuł: " + AddBook.t.Text +
                    "\n Autor: " + AddBook.t2.Text +
                    "\n Lokaliacja: " + AddBook.t3.Text);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int idOrder = 1;
            if (listView2.SelectedItems.Count > 0)
            {
                idOrder = Int32.Parse(listView2.SelectedItems[0].SubItems[4].Text);
                OrderDetails odForm = new OrderDetails(idOrder, connection, connectionString);
                odForm.ShowDialog();
            }
            
        }
    }
}
