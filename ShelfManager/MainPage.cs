using Newtonsoft.Json;
using RestSharp;
using ShelfManager.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace ShelfManager
{
    public partial class MainPage : Form
    {
        Order ordersList;
        int[] orange = { 255, 236, 228 };
        int[] green = { 217, 248, 217 };
        int[] red = { 255, 204, 203 };
        string storedBarcode = "";

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        //Creating a function that uses the API function...  
        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        OpeningPage page;


        PictureBox pic2,pic3,pic5,pic6;
        public MainPage(OpeningPage openingPage)
        {
            this.page = openingPage;
            InitializeComponent();

            pic2 = pictureBox2;
            pic3 = pictureBox3;
            pic5 = pictureBox5;
            pic6 = pictureBox6;

            pictureBox2.Paint += (sender, e) => PictureBox2_Paint(sender, e, "No Barcode Scanned Yet!",red);

            loadLists();
            getOrdersList();

            appear();
            List<string> list = new List<string>();

            foreach (Datum order in ordersList.data)
            {
               foreach(Line line in order.lines)
                {
                    list.Add(line.product.item_number);
                }
            }

            var occurrences = ordersList.data.GroupBy(x => x.status).ToDictionary(y => y.Key, z => z.Count());
            var ordes = ordersList.data.Where(x => x.status == "in_picking").ToList();

            loadInfo();
        }

        public void appear()
        {
            int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSITED;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);
        }


        private void initPictureBoxes()
        {
            pictureBox2 = pic2;
            pictureBox3 = pic3;
            pictureBox5 = pic5;
            pictureBox6 = pic6;

            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            pictureBox5.Visible = false;
            pictureBox6.Visible = false;
        }
        private void loadInfo()
        {
            List<Shelf> shelves = getShelfList();
            int count = shelves.Count;
            button4.Text = "Total Shelves: " + count.ToString();
            button7.Text = "Shelves Open: " + (count == 0 ? "0" : shelves.Where(x=>x.open==1).ToList().Count.ToString());
            button9.Text = "Shelves Being Used: " + (count == 0 ? "0" : shelves.Where(x => x.open == 0).ToList().Count.ToString());
            button8.Text = "Largest Shelf: " + (count == 0 ? "None" : largestShelf().Name+" ("+largestShelf().Size+")");

        }

        public Shelf largestShelf()
        {
            Shelf shelf = null;
            int index = 0;
            int size = 0;

            for (int i=0;i<getShelfList().Count;i++)
            {
                if (getShelfList()[i].Size>=size)
                {
                    size = getShelfList()[i].Size;
                    index = i;
                }
            }
            shelf = getShelfList()[index];

            return shelf;
        }
        private void display(int cond, string shelf,string orderNumber)
        {
            int[] color = new int[3];
            
            initPictureBoxes();

            if(cond==-1)
            {
                color = red;
                this.pictureBox2.Visible = true;
                pictureBox2.Paint += (sender, e) => PictureBox2_Paint(sender, e, "Please Scan Again!",color);
                button3.BackColor = Color.IndianRed;
                button3.Text = "ReScan";
            }
            else if (cond == 0)
            {
                color = orange;
                this.pictureBox3.Visible = true;
                pictureBox3.Paint += (sender, e) => PictureBox2_Paint(sender, e, "Move To Shelf - " + shelf, color);
                button3.BackColor = Color.FromArgb(255, 166, 131);
            }
            else if (cond == 1)
            {
                color = green;
                this.pictureBox5.Visible = true;
                pictureBox5.Paint += (sender, e) => PictureBox2_Paint(sender, e, "Move Order To Packaging !", color);
                button3.BackColor = Color.ForestGreen;
            }
            else if (cond == 10)
            {
                color = green;
                this.pictureBox5.Visible = true;
                pictureBox5.Paint += (sender, e) => PictureBox2_Paint(sender, e, "Move To Packaging - " + shelf + " !", color);
                button3.BackColor = Color.ForestGreen;
            }
            else if (cond == 100)
            {
                color = red;
                this.pictureBox6.Visible = true;
                pictureBox6.Paint += (sender, e) => PictureBox2_Paint(sender, e, "Item Already In - " + shelf + "!", color);
                button3.BackColor = Color.IndianRed;
            }
           /* else if (cond == 300)
            {
                color = red;
                this.pictureBox6.Visible = true;
                pictureBox6.Paint += (sender, e) => PictureBox2_Paint(sender, e, "Other Items In Order Are Larger!", color);
                button3.BackColor = Color.IndianRed;
            }*/
            else
            {
                color = red;
                this.pictureBox6.Visible = true;
                pictureBox6.Paint += (sender, e) => PictureBox2_Paint(sender, e, "No Shelves Available !", color);
                button3.BackColor = Color.IndianRed;
            }

            loadInfo();
            panel14.BackColor = Color.FromArgb(color[0], color[1], color[2]);
            panel12.BackColor = Color.FromArgb(color[0], color[1], color[2]);
        }

        private void PictureBox2_Paint(object sender, PaintEventArgs e,string text, int[] col)
        {
            var pic = (PictureBox)sender;

            e.Graphics.Clear(Color.FromArgb(col[0], col[1], col[2]));

            Font font = new Font(FontFamily.GenericSansSerif, 21, FontStyle.Italic);
            SizeF textSize = e.Graphics.MeasureString(text, font);

            PointF locationToDraw = new PointF();
            locationToDraw.X = (pictureBox2.Width / 2) - (textSize.Width / 2);
            locationToDraw.Y = (pictureBox2.Height / 2) - (textSize.Height / 2);

            if(pic.Image!=pic6.Image)
                e.Graphics.DrawImage(pic.Image, 140, 0, 330, 275);
            else
                e.Graphics.DrawImage(pic.Image, 150, 10, 300, 250);

            e.Graphics.DrawString(text, font, Brushes.Black, locationToDraw);
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

        public List<string> getOrderCompleteList()
        {
            List<string> compList  = new List<string>();
            string data = File.ReadAllText("Saves/OrdersCompleted.json");
            var ordersComp = JsonConvert.DeserializeObject<List<string>>(data);

            if (String.IsNullOrWhiteSpace(data))
                return compList;
            else
                return ordersComp;

        }


       
        public void saveShelvesList(List<Shelf> shelves)
        {
            string json = JsonConvert.SerializeObject(shelves, Formatting.Indented);
            File.WriteAllText("Saves/Shelves.json", json);
        }

        public void saveOrderCompList(List<string> compList)
        {
            string json = JsonConvert.SerializeObject(compList, Formatting.Indented);
            File.WriteAllText("Saves/OrdersCompleted.json", json);
        }

        public string getItem(Datum data)
        {
            List<Shelf> shelves = getShelfList();
            bool Exist = false;
            string itemNumber = "";

                foreach (Shelf shelf in shelves)
                {
                    if (data.order_number == shelf.orderNumber)
                    {
                        Exist = true;

                        foreach(Line line in data.lines)
                        {
                            foreach (Item item in shelf.items)
                            {
                                if (item.itemNumber=="None")
                                {
                                    itemNumber = line.product.item_number;
                                    goto LoopEnd;
                                }
                                else if (item.itemNumber == line.product.item_number && item.itemQuantity < line.quantity.amount)
                                {
                                    itemNumber = line.product.item_number;
                                    goto LoopEnd;
                                }
                                else if (item.itemNumber == line.product.item_number)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

        LoopEnd:
            if (Exist == false)
                return data.lines[0].product.item_number;
            else
                return itemNumber;
        }

        public void getResult(string barcode)
        {
            try
            {
                Logs log = new Logs();
                string itemSKU = barcode.Trim();
                List<string> comp = getOrderCompleteList();
                //itemSKU = itemSKU.StartsWith("SI-") ? itemSKU : throw new Exception();
               
                foreach(string item in comp)
                {
                    ordersList.data.Remove(ordersList.data.Where(x => x.order_number == item).FirstOrDefault());
                }

                Datum order = null;

                if (char.IsDigit(itemSKU[0]))
                {
                    order = ordersList.data.Where(x => x.order_number == itemSKU).FirstOrDefault();
                }
                else
                    order = ordersList.data.Where(x => x.lines.Where(y => y.product.item_number == itemSKU).FirstOrDefault() != null).FirstOrDefault();
                
                if (order == null)
                {
                    MessageBox.Show("The entered order or item does not exist!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    display(-1, "", "");
                    return;
                } 
                else if (char.IsDigit(itemSKU[0]))
                    itemSKU = getItem(order);

                string orderNumber = order.order_number;
                string shelfName = "";
                List<Shelf> shelves = getShelfList();

                log.Time_Stamp = DateTime.Now.ToString("h:mm:ss tt");
                log.Order_Number = orderNumber;


                if (!String.IsNullOrWhiteSpace(orderNumber))
                {
                    List<Line> productList = ordersList.data.Where(x => x.order_number == orderNumber).FirstOrDefault().lines;
                    log.Item_Number = itemSKU;
                    int amount = productList.Where(x => x.product.item_number == itemSKU).FirstOrDefault().quantity.amount;

                    int condition = 2;

                    if (productList.Count == 1 && productList[0].quantity.amount == 1)
                    {
                        condition = 1;
                        log.Final_Item = "Yes";
                        log.No_Of_Item = "1";
                        comp.Add(orderNumber);
                        saveOrderCompList(comp);
                    }
                    else
                    {
                        string size = extractSize(itemSKU);

                        Shelf existingShelf = shelves.Where(x => x.orderNumber == orderNumber).FirstOrDefault();

                        if (existingShelf == null && shelves.Where(x => x.open == 1).ToList().Count > 0 && shelves.Count > 0)
                        {
                            shelfName = getShelf(getLargestSize(orderNumber), orderNumber, log.Item_Number);

                            if (shelfName != "None")
                            {
                                condition = 0;
                                log.Final_Item = "No";
                                log.No_Of_Item = "1";                             
                            }
                        }
                        else
                        {
                            int index = shelves.IndexOf(shelves.Where(x => x.orderNumber == orderNumber).FirstOrDefault());

                            if (existingShelf != null && existingShelf.items.Where(x => x.itemNumber == log.Item_Number).FirstOrDefault() != null && productList.Where(x => x.product.item_number == log.Item_Number).ToList().Count == shelves[index].items.Where(x=>x.itemNumber==log.Item_Number).ToList().Count && amount == shelves[index].items.Where(x => x.itemNumber == log.Item_Number).FirstOrDefault().itemQuantity)
                            {
                                condition = 100;
                                shelfName = existingShelf.Name;
                            }
                            else if (existingShelf != null && (productList.Where(x => x.product.item_number == log.Item_Number).ToList().Count != shelves[index].items.Where(x => x.itemNumber == log.Item_Number).ToList().Count || existingShelf.items.Where(x => x.itemNumber == log.Item_Number).FirstOrDefault() == null || existingShelf.items.Where(x => x.itemNumber == log.Item_Number).FirstOrDefault().itemQuantity < amount))
                            {

                                if (countItems(productList) == existingShelf.itemsCount + 1)
                                {
                                    condition = 10;
                                    shelfName = existingShelf.Name;
                                    log.No_Of_Item = (shelves[index].itemsCount + 1).ToString();
                                    shelves[index].open = 1;
                                    shelves[index].orderNumber = String.Empty;
                                    shelves[index].itemsCount = 0;
                                    shelves[index].items.Clear();
                                    saveShelvesList(shelves);
                                    loadLists();
                                    log.Final_Item = "Yes";

                                    comp.Add(orderNumber);
                                    saveOrderCompList(comp);
                                }
                                else
                                {
                                    condition = 0;
                                    shelfName = existingShelf.Name;

                                    if (shelves[index].items.Where(x => x.itemNumber == log.Item_Number).FirstOrDefault() == null)
                                    {
                                        Item item = new Item();
                                        item.itemNumber = log.Item_Number;
                                        item.itemQuantity += 1;
                                        shelves[index].items.Add(item);
                                    }
                                    else
                                    {
                                        shelves[index].items.Where(x => x.itemNumber == log.Item_Number).FirstOrDefault().itemQuantity += 1;
                                    }

                                    shelves[index].itemsCount = getItemsCount(shelves[index]);
                                    log.No_Of_Item = (shelves[index].itemsCount).ToString();
                                    saveShelvesList(shelves);
                                    loadLists();
                                    log.Final_Item = "No";
                                }
                            }
                        }
                    }
                    log.Shelf_Name = String.IsNullOrWhiteSpace(shelfName) ? "Empty" : shelfName;
                    List<Logs> logs = getLogs();
                    logs.Add(log);

                    if (condition != 2 && condition != 100)
                    {
                        File.AppendAllText("logs.txt", "\n\n     " + log.Time_Stamp + "       " + log.Order_Number + "       " + log.Item_Number + "       " + log.No_Of_Item + "                  " + log.Shelf_Name + "               " + log.Final_Item + "       ");
                        saveLogsList(logs);
                    }

                    display(condition, shelfName,orderNumber);
                    button3.Text = "ReScan : " + storedBarcode;
                    loadLists();
                }

            }
            catch
            {
                MessageBox.Show("Please enter the correct format!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                display(-1, "", "");
            }


        }


        public void saveLogsList(List<Logs> logs)
        {
            Logs log = logs[logs.Count - 1];
            logs.RemoveAt(logs.Count - 1);
            logs.Insert(0,log);

            string json = JsonConvert.SerializeObject(logs, Formatting.Indented);
            File.WriteAllText("Saves/Logs.json", json);
        }

        private int countItems(List<Line> products)
        {
            int count = 0;

            foreach (Line product in products)
            {
                count += product.quantity.amount;
            }

            return count;
        }

        private string extractSize(string itemNumber)
        {
            string skuTrimmed = (itemNumber.Substring(itemNumber.IndexOf("-") + 1));
            string sizeAdd = (skuTrimmed.Substring(skuTrimmed.IndexOf("-") + 1));
            string size = sizeAdd.Contains("-") ? sizeAdd.Substring(0, sizeAdd.IndexOf("-")) : sizeAdd.Substring(0);

            return size;
        }

        private int getLargestSize(string orderNumber)
        {
            List<Line> products = ordersList.data.Where(x => x.order_number == orderNumber).FirstOrDefault().lines;
            int largestSize = 0;

            foreach (Line product in products)
            {
                string size = extractSize(product.product.item_number);

                if (Int32.Parse(size) > largestSize)
                {
                    largestSize = Int32.Parse(size);
                }
            }
            return largestSize;
        }

        private int getItemsCount(Shelf shelf)
        {
            int count = 0;
            foreach(Item item in shelf.items)
            {
                count+=item.itemQuantity;
            }
            return count;
        }
        private string getShelf(int size, string orderNumber, string itemNumber)
        {
            List<Shelf> shelves = getShelfList();
            List<Shelf> eligibleShelves = new List<Shelf>();

            int highestPriority = shelves[0].Priority;
            int countLarger = 0;

            foreach (Shelf shelf in shelves)
            {
                if (shelf.Size >= size && shelf.open == 1)
                {
                    if (shelf.Priority <= highestPriority)
                    {
                        eligibleShelves.Insert(0, shelf);
                        highestPriority = shelf.Priority;
                    }
                    else
                        eligibleShelves.Add(shelf);
                }
                if (ordersList.data.Where(x => x.order_number == orderNumber).FirstOrDefault().lines.Where(y => shelf.Size >= Int32.Parse(extractSize(y.product.item_number))).ToList().Count > 0)
                    countLarger++;

            }

            if (eligibleShelves.Count == 0)
            {
                    return "None";
            }
            else
            {
                eligibleShelves=eligibleShelves.Where(x => eligibleShelves[0].Priority == x.Priority).ToList();
                int index = shelves.IndexOf(shelves.Where(x=>x.Name== eligibleShelves[eligibleShelves.Count - 1].Name).FirstOrDefault());
                List<Line> products = ordersList.data.Where(x => x.order_number == orderNumber).FirstOrDefault().lines;
                shelves[index].open = 0;
                shelves[index].orderNumber = orderNumber;
                Item item = new Item();
                item.itemNumber = itemNumber;
                item.itemQuantity = 1;
                if(shelves[index].items==null || shelves[index].items.Count==0)
                {
                    shelves[index].items = new List<Item>();
                    for (int i = 0; i < products.Count; i++)
                    {
                        Item item1 = new Item();
                        item1.itemNumber = "None";
                        item1.itemQuantity = 0;
                        shelves[index].items.Add(item1);
                    }
                }
                int shelfIndex = shelves[index].items.IndexOf(shelves[index].items.Where(x => x.itemNumber == "None").FirstOrDefault());
                shelves[index].items[shelfIndex].itemNumber = item.itemNumber;
                shelves[index].items[shelfIndex].itemQuantity = item.itemQuantity;

                shelves[index].itemsCount = getItemsCount(shelves[index]);
                saveShelvesList(shelves);
                loadLists();
                return shelves[index].Name;
            }
        }

        public void loadLists()
        {
            dataGridView1.DataSource = getLogs();
        }

        public void getOrdersList()
        {
            Order temp_Orders = new Order();
            string data = File.ReadAllText("Saves/Orders.json");
            var list = JsonConvert.DeserializeObject<Order>(data);

            if (String.IsNullOrWhiteSpace(data))
                ordersList = temp_Orders;
            else
                ordersList = list;
        }

        public List<Logs> getLogs()
        {
            List<Logs> logsList = new List<Logs>();
            string data = File.ReadAllText("Saves/Logs.json");
            var logs = JsonConvert.DeserializeObject<List<Logs>>(data);

            if (String.IsNullOrWhiteSpace(data))
                return logsList;
            else
                return logs;

        }
        private void scanCode()
        {
            if ((textBox6.Text == "Scan Barcode Here" || textBox6.Text == ""))
            {
                MessageBox.Show("Please scan a barcode first!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {   
                progressBar1.Visible = true;
                timer1.Start();
                storedBarcode = textBox6.Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
      
        private void button5_Click(object sender, EventArgs e)
        {
            this.Refresh();

            int style = base.CreateParams.ExStyle;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);

            this.panel19.Refresh();
            
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                panel24.Height += 150;
                panel25.Height += 150;

                
                dataGridView1.Columns[0].Width = dataGridView1.Width/6 - 5;
                dataGridView1.Columns[1].Width = dataGridView1.Width / 6 - 5;
                dataGridView1.Columns[2].Width = dataGridView1.Width / 6 - 5;
                dataGridView1.Columns[3].Width = dataGridView1.Width / 6 - 5;
                dataGridView1.Columns[4].Width = dataGridView1.Width / 6 - 5;
                dataGridView1.Columns[5].Width = dataGridView1.Width / 6 - 5;

            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                panel24.Height -= 150;
                panel25.Height -= 150;

                dataGridView1.Columns[0].Width = 125;
                dataGridView1.Columns[1].Width = 125;
                dataGridView1.Columns[2].Width = 125;
                dataGridView1.Columns[3].Width = 125;
                dataGridView1.Columns[4].Width = 125;
                dataGridView1.Columns[5].Width = 125;
            }
            this.panel19.Refresh();
        }



        string pass;
        public void Return(string text)
        {
            pass = text;
        }
        private void label5_Click(object sender, EventArgs e)
        {
            using (Panel p = new Panel())
            {
                p.Location = new Point(0, 0);
                p.Size = this.ClientRectangle.Size;
                p.BackgroundImage = FormFade();
                this.Controls.Add(p);
                p.BringToFront();

                AdminPass adminPass = new AdminPass(this);
                adminPass.ShowDialog();
            }

            this.Refresh();
            if(pass!=null)
            {
                if (pass == "S1g0S1gns")
                {
                    using (Panel p = new Panel())
                    {
                        p.Location = new Point(0, 0);
                        p.Size = this.ClientRectangle.Size;
                        p.BackgroundImage = FormFade();
                        this.Controls.Add(p);
                        p.BringToFront();

                        try
                        {
                            ShelvesManagement shelvesManagement = new ShelvesManagement();
                            shelvesManagement.ShowDialog();
                            loadInfo();
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    MessageBox.Show("The password entered was incorrect, please try again!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                pass=null;
            }
            
        }

        private Bitmap FormFade()
        {

            Bitmap bmp = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            using (Graphics G = Graphics.FromImage(bmp))
            {
                G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                G.CopyFromScreen(this.PointToScreen(new Point(0, 0)), new Point(0, 0), this.ClientRectangle.Size);
                double percent = 0.60;
                Color darken = Color.FromArgb((int)(255 * percent), Color.Black);
                using (Brush brsh = new SolidBrush(darken))
                {
                    G.FillRectangle(brsh, this.ClientRectangle);
                }
            }
            return bmp;
        }

        private void textBox6_MouseDown(object sender, MouseEventArgs e)
        {
            if (textBox6.Text == "Scan Barcode Here")
                textBox6.Text = "";
        }

        private void textBox6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                scanCode();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            scanCode();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void barcodeResult()
        {
            getResult(textBox6.Text);
            textBox6.Text = "";
        }

        int count = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
            if (count > 5)
            {
                progressBar1.Visible = false;
                count = 0;
                timer1.Stop();
                barcodeResult();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            File.WriteAllText("Saves/Logs.json", "");
            loadLists();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(storedBarcode!="")
            {
                getResult(storedBarcode);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            File.WriteAllText("Saves/Logs.json", "");
            loadLists();
        }

        private void label7_Click(object sender, EventArgs e)
        {
           
            uploadCloud();

           
        }


        private void uploadCloud()
        {
            if (IsConnectedToInternet())
            {
                string result = MessageBox.Show("Are you sure you want to update local database?", "Alert", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning).ToString();

                if (result == "OK")
                {
                    getOrdersList(1);
                }
            }
            else
            {
                MessageBox.Show("Please first connect to the internet!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                panel2.Visible = true;
                panel3.Visible = true;
                pictureBox4.Visible = false;
            }

        }

        public async Task<Order> callApiAsync(string link)
        {
            var options = new RestClientOptions("https://gfs.api.goflow.com")
            {
                MaxTimeout = -1,
            };

            var client = new RestClient(options);
            var request = new RestRequest(link, Method.Get);
            request.AddHeader("X-Beta-Contact", "saadsultan2018@gmail.com");
            request.AddHeader("Authorization", "Bearer e7c9f88388f347108e96adf6a69af327");
            string content = null;

            Order order = new Order();
            order.data = new List<Datum>();
            order.next = link;
            var i = 0;

            while (order.next != null)
            {
                i++;
                content = null;

                while (content == null)
                {
                    RestResponse response = await client.ExecuteAsync(request);
                    content = response.Content;
                }
                Order tempOrder = JsonConvert.DeserializeObject<Order>(content);
                foreach (Datum datum in tempOrder.data)
                {
                    order.data.Add(datum);
                }
                order.next = tempOrder.next;
                request.Resource = tempOrder.next;
            }

            return order;
        }



        public void saveOrdersList(string jsonData)
        {
            File.WriteAllText("Saves/Orders.json", jsonData);
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        public async void getOrdersList(int cond)
        {
            saveOrdersList("");

            try
            {
                if (IsConnectedToInternet())
                {
                    panel2.Visible = false;
                    panel3.Visible = false;
                    pictureBox4.Visible = true;

                    Order picking = await callApiAsync("https://gfs.api.goflow.com/v1/orders?filters%5Bstatus%5D=in_picking&filters%5Bstatus%5D=in_packing&filters%5Bstatus%5D=ready_to_pick&filter%5Bstore.id%5D=1017");
                    saveOrdersList(JsonConvert.SerializeObject(picking));
                }
                else
                {
                    MessageBox.Show("Please connect to the internet to retrieve the orders list!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            finally
            {
                if (IsConnectedToInternet())
                {
                    MessageBox.Show("The local database has been updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    panel2.Visible = true;
                    panel3.Visible = true;
                    pictureBox4.Visible = false;
                    getOrdersList();
                }
            }
        }

        private void MainPage_Resize(object sender, EventArgs e)
        {
            
        }

        private void button11_Click(object sender, EventArgs e)
        {
            int style = base.CreateParams.ExStyle;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);
            this.WindowState = FormWindowState.Minimized;
        }

        
    }
}
