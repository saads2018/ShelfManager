using Newtonsoft.Json;
using ShelfManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShelfManager
{
    public partial class ShelvesManagement : Form
    {
        public ShelvesManagement()
        {
            InitializeComponent();
            loadLists();
            this.comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void saveShelvesList(List<Shelf> shelves)
        {
            string json = JsonConvert.SerializeObject(shelves, Formatting.Indented);
            File.WriteAllText("Saves/Shelves.json", json);
        }

        private void updateShelfList()
        {
            List<Shelf> shelves = getShelfList();
            Shelf shelf = new Shelf();

            if ((textBox1.Text == "Enter Name" || textBox1.Text == "") || (textBox2.Text == "Enter Priority" || textBox2.Text == "") || (textBox3.Text == "Enter Size" || textBox3.Text == ""))
            {
                MessageBox.Show("Please enter all the necessary details!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                shelf.Name = textBox1.Text;
                shelf.Size = Int32.Parse(textBox3.Text);
                shelf.Priority = Int32.Parse(textBox2.Text);

                string message = "";

                if (button3.Text == "Add")
                {
                    shelves.Add(shelf);
                    message = "A new shelf was successfully created!";
                }
                else
                {
                    shelves[comboBox1.SelectedIndex - 1] = shelf;
                    message = "The shelf has been successfully updated!";
                }

                saveShelvesList(shelves);
                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                initShelfState();
            }
        }

        public void loadLists()
        {
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("Select Shelf");
            foreach (Shelf shelf in getShelfList())
                this.comboBox1.Items.Add(shelf.Name + ":" + (shelf.open == 1 ? " Open" : " ") + shelf.orderNumber);
        }

        private void initShelfState()
        {
            textBox1.Text = "Enter Name";
            textBox2.Text = "Enter Priority";
            textBox3.Text = "Enter Size";
            button3.Text = "Add";
            comboBox1.SelectedIndex = 0;
            loadLists();
        }

        public List<Shelf> getShelfList()
        {
            List<Shelf> shelfList = new List<Shelf>();
            string data = File.ReadAllText("Saves/Shelves.json");
            var shelves = JsonConvert.DeserializeObject<List<Shelf>>(data);

            if (String.IsNullOrWhiteSpace(data))
                return shelfList;
            else
                return shelves;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                initShelfState();
            else
            {
                textBox1.Text = getShelfList()[comboBox1.SelectedIndex - 1].Name;
                textBox2.Text = getShelfList()[comboBox1.SelectedIndex - 1].Priority.ToString();
                textBox3.Text = getShelfList()[comboBox1.SelectedIndex - 1].Size.ToString();
                button3.Text = "Save";
            }
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (textBox1.Text == "Enter Name")
                textBox1.Text = "";
        }

        private void textBox3_MouseDown(object sender, MouseEventArgs e)
        {
            if (textBox3.Text == "Enter Size")
                textBox3.Text = "";
        }

        private void textBox2_MouseEnter(object sender, EventArgs e)
        {

        }

        private void textBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (textBox2.Text == "Enter Priority")
                textBox2.Text = "";
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                updateShelfList();
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                updateShelfList();
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                updateShelfList();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            updateShelfList();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > 0)
            {
                if (getShelfList()[comboBox1.SelectedIndex - 1].open == 0)
                {
                    string result = MessageBox.Show("The selected shelf is still in use, Are you sure that you want to delete it?", "Alert", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning).ToString();
                    if (result == "Cancel")
                        return;
                }
                List<Shelf> shelves = getShelfList();
                shelves.RemoveAt(comboBox1.SelectedIndex - 1);
                saveShelvesList(shelves);
                MessageBox.Show("The shelf has been successfully deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                initShelfState();
            }
        }
    }
}
