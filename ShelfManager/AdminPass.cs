using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShelfManager
{
    public partial class AdminPass : Form
    {
        MainPage main;
        public AdminPass(MainPage mainPage)
        {
            InitializeComponent();
            this.main=mainPage;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (textBox1.Text == "Enter Password")
                textBox1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            main.Return(textBox1.Text);
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                main.Return(textBox1.Text);
                this.Close();
            }
        }
    }
}
