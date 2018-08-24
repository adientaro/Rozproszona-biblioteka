using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RozproszoneBazyDanych
{
    public partial class AddBook : Form
    {
        public static TextBox t = new TextBox();
        public static TextBox t2 = new TextBox();
        public static TextBox t3 = new TextBox();

        public AddBook()
        {
            InitializeComponent();
            t = textBox1;
            t2 = textBox2;
            t3 = textBox3;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
