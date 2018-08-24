using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RozproszoneBazyDanych
{
    public partial class AddOrder : Form
    {
        SqlConnection connection;
        string connectionString;
        string selectedClient;
        string pesel;

        public AddOrder()
        {
            InitializeComponent();
            connectionString = "data source = DESKTOP-EV3588L\\PAWEL; database = biblioteka; integrated security = sspi";
            connection = new SqlConnection(connectionString);
            PopulateClientList();
        }

        public void PopulateClientList()
        {
            listView1.Items.Clear();
            using (connection = new SqlConnection(connectionString))
            using (SqlDataAdapter clientsAdapter = new SqlDataAdapter("SELECT * FROM klient", connection))
            {
                DataTable clients = new DataTable();
                clientsAdapter.Fill(clients);
                for (int i = 0; i < clients.Rows.Count; i++)
                {
                    DataRow drow = clients.Rows[i];
                    if (drow.RowState != DataRowState.Deleted)
                    {
                        ListViewItem lvi = new ListViewItem(drow[1].ToString() + " " + drow[2].ToString());
                        lvi.SubItems.Add(drow[3].ToString());
                        lvi.SubItems.Add(drow[4].ToString());
                        listView1.Items.Add(lvi);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string name = textBox3.Text;
            string query = "SELECT * FROM klient WHERE klient.nazwisko = @nameParam";
            listView1.Items.Clear();

            if (string.IsNullOrWhiteSpace(name))
                PopulateClientList();
            else { 
                using (connection = new SqlConnection(connectionString))
                using(SqlCommand filtrNamesCmd = new SqlCommand(query,connection))
                {
                    connection.Open();
                    filtrNamesCmd.Parameters.AddWithValue("@nameParam", name);
                    using (SqlDataReader reader = filtrNamesCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ListViewItem lvi = new ListViewItem(reader[1].ToString() + " " + reader[2].ToString());
                                lvi.SubItems.Add(reader[3].ToString());
                                lvi.SubItems.Add(reader[4].ToString());
                                listView1.Items.Add(lvi);
                            }
                        }
                    }
                }
            }

        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                selectedClient = listView1.SelectedItems[0].SubItems[0].Text;
                pesel = listView1.SelectedItems[0].SubItems[1].Text;
            }

            textBox4.Text = selectedClient;
            textBox5.Text = pesel;
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(selectedClient + "  " + pesel + "  ");
        }

        private void button1_Click(object sender, EventArgs e)
        {    
            if (CheckFields())
            {
                bool canOrder = false, bookAvaible = false;
                int idClient = 1, avaibleOrders, idZbioru = 1;
                string queryGetClient = "SELECT TOP 1 klient.id, klient.mozliweWyp FROM klient WHERE klient.pesel = @peselParam";
                string queryGetBook = "SELECT TOP 1 ksiazka.dostepnosc, ksiazka.idZbioru FROM ksiazka WHERE ksiazka.id = @idParam";
                string queryAddOrder = "INSERT INTO wypozyczenie (idKlient, idKsiazka, data_wyp, status) VALUES (@idKlientParam, @idKsiazkaParam, @dataWypParam, @statusParam) ";
                using (connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand getClientCmd = new SqlCommand(queryGetClient, connection))
                    {
                        getClientCmd.Parameters.AddWithValue("@peselParam", pesel);
                        using (SqlDataReader reader = getClientCmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    idClient = Int32.Parse(reader[0].ToString());
                                    avaibleOrders = Int32.Parse(reader[1].ToString());
                                    if (avaibleOrders >= 1)
                                        canOrder = true;
                                }
                            }
                        }
                    }
                    using (SqlCommand getBookCmd = new SqlCommand(queryGetBook, connection))
                    {
                        getBookCmd.Parameters.AddWithValue("@idParam", textBox1.Text);
                        using (SqlDataReader reader = getBookCmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    bookAvaible = (bool)reader[0];
                                    idZbioru = (int)reader[1];
                                }
                            }
                            else
                                MessageBox.Show("Brak książki o danym id w bazie");
                        }
                    }
                    if (canOrder && bookAvaible)
                    {
                        using (SqlCommand addOrderCmd = new SqlCommand(queryAddOrder, connection))
                        {
                            addOrderCmd.Parameters.AddWithValue("@idKlientParam", idClient);
                            addOrderCmd.Parameters.AddWithValue("@idKsiazkaParam", textBox1.Text);
                            addOrderCmd.Parameters.AddWithValue("@dataWypParam", dateTimePicker1.Value.Date);
                            addOrderCmd.Parameters.AddWithValue("@statusParam", false);
                            addOrderCmd.ExecuteNonQuery();
                            MessageBox.Show("Poprawnie dodano zamówienie.");
                        }
                        string updateClientQuery = "UPDATE klient SET mozliweWyp = mozliweWyp - 1 WHERE klient.id = @idClientParam";
                        string updateBookQuery = "UPDATE ksiazka SET dostepnosc = @dostParam WHERE ksiazka.id = @idKsiazkaParam";
                        string updateBookCountQuery = "UPDATE iloscKsiazek SET ilosc = ilosc - 1 WHERE iloscKsiazek.idZbioru = @idZbioruParam";
                        using (SqlCommand updateClientCmd = new SqlCommand(updateClientQuery, connection))
                        {
                            updateClientCmd.Parameters.AddWithValue("@idClientParam", idClient);
                            updateClientCmd.ExecuteNonQuery();
                        }
                        using (SqlCommand updateBookCmd = new SqlCommand(updateBookQuery, connection))
                        {
                            updateBookCmd.Parameters.AddWithValue("@idKsiazkaParam", textBox1.Text);
                            updateBookCmd.Parameters.AddWithValue("@dostParam", false);
                            updateBookCmd.ExecuteNonQuery();
                        }
                        using (SqlCommand updateBookCountCmd = new SqlCommand(updateBookCountQuery, connection))
                        {
                            updateBookCountCmd.Parameters.AddWithValue("@idZbioruParam", idZbioru);
                            updateBookCountCmd.ExecuteNonQuery();
                        }
                    }
                    else
                        MessageBox.Show("Nie można zrealizować zamówienia. Brak możliości" +
                            "wypożyczenia przez klienta lub brak ksiażki na stanie");

                }
            }
            else
                MessageBox.Show("Należy uzupełnić wszystkie pola");
        }

        private bool CheckFields()
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox4.Text))
                return false;
            else
                return true;
        }
    }
}
