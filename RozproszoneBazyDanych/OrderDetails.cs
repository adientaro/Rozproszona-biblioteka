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
    public partial class OrderDetails : Form
    {
        private int idOrder;
        private int idClient;
        private int idSet;
        private int idBook;
        string connectionString;
        SqlConnection connection;
        bool preStatus;

        public OrderDetails()
        {
            InitializeComponent();
        }
        public OrderDetails(int id, SqlConnection con, string conString)
        {
            InitializeComponent();
            idOrder = id;
            connection = con;
            connectionString = conString;
            GetData();
            SetRadioButtons();


        }

        private void UpdateData()
        {
            string updateBookStatus = "UPDATE ksiazka SET dostepnosc = @dostParam WHERE ksiazka.id = @idKsiazkaParam";
            string updateClientStatus = "UPDATE klient SET mozliweWyp = mozliweWyp + 1 WHERE klient.id = @idKlientParam";
            string updateCountStatus = "UPDATE iloscKsiazek SET ilosc = ilosc + 1 WHERE iloscKsiazek.idZbioru = @idZbioruParam";
            string updateOrderStatus = "UPDATE wypozyczenie SET data_odd = @dataParam, status = @statusParam WHERE wypozyczenie.id = @wypozyczenieParam";

            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand updateBookCmd = new SqlCommand(updateBookStatus, connection))
                {
                    updateBookCmd.Parameters.AddWithValue("@dostParam", true);
                    updateBookCmd.Parameters.AddWithValue("@idKsiazkaParam", idBook);
                    updateBookCmd.ExecuteNonQuery();
                }
                using (SqlCommand updateClientCmd = new SqlCommand(updateClientStatus, connection))
                {
                    updateClientCmd.Parameters.AddWithValue("@idKlientParam", idClient);
                    updateClientCmd.ExecuteNonQuery();
                }
                using (SqlCommand updateCountCmd = new SqlCommand(updateCountStatus, connection))
                {
                    updateCountCmd.Parameters.AddWithValue("@idZbioruParam", idSet);
                    updateCountCmd.ExecuteNonQuery();
                }
                using (SqlCommand updateOrderCmd = new SqlCommand(updateOrderStatus, connection))
                {
                    updateOrderCmd.Parameters.AddWithValue("@dataParam", dateTimePicker1.Value.Date);
                    updateOrderCmd.Parameters.AddWithValue("@statusParam", true);
                    updateOrderCmd.Parameters.AddWithValue("@wypozyczenieParam", idOrder);
                    updateOrderCmd.ExecuteNonQuery();
                }
            }
        }
        private void GetData()
        {
            string query = "SELECT klient.imie, klient.nazwisko, klient.pesel, wypozyczenie.id, wypozyczenie.idKsiazka, wypozyczenie.status, zbiorKsiazek.tytul, zbiorKsiazek.autor, klient.id, zbiorKsiazek.id FROM wypozyczenie INNER JOIN klient ON wypozyczenie.idKlient = klient.id INNER JOIN ksiazka ON wypozyczenie.idKsiazka = ksiazka.id INNER JOIN zbiorKsiazek ON ksiazka.idZbioru = zbiorKsiazek.id WHERE wypozyczenie.id = @idParam";
            using (connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@idParam", idOrder);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            textBox1.Text = reader[0].ToString();
                            textBox2.Text = reader[1].ToString();
                            textBox3.Text = reader[2].ToString();
                            label2.Text = "Nr zamówienia: " + reader[3].ToString();
                            textBox6.Text = reader[4].ToString();
                            textBox5.Text = reader[6].ToString();
                            textBox4.Text = reader[7].ToString();
                            preStatus = (bool)reader[5];
                            idClient = (int)reader[8];
                            idSet = (int)reader[9];
                            idBook = (int)reader[4];
                            MessageBox.Show(idClient.ToString() + "  " + idSet.ToString());
                        }
                    }
                }
            }
        }

        private void SetRadioButtons()
        {
            if (preStatus)
                radioButton2.Checked = true;
            else
                radioButton1.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (preStatus)
                MessageBox.Show("Nie można oddać ksiązki, która jest już oddana");
            else if (radioButton1.Checked)
                this.Close();
            else
            { 
                UpdateData();
                MessageBox.Show("Oddano zamówienie nr " + idOrder);
                this.Close();
            }
        }
    }
}
