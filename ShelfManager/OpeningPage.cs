using Newtonsoft.Json;
using RestSharp;
using ShelfManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShelfManager
{
    public partial class OpeningPage : Form
    {
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

        public OpeningPage()
        {
            createDirectory();
            InitializeComponent();

            if (isEmpty())
                getOrdersList(0);
            else
                showForm(true);

           /* int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSITED;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);*/

        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;    // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        public bool isEmpty()
        {
            string data = File.ReadAllText("Saves/Orders.json");
            Order order = JsonConvert.DeserializeObject<Order>(data);

            var x = 1;

            if (String.IsNullOrWhiteSpace(data)||order.data.Count==0)
            {
                x = 2;
                return true;
            }
            else
                return false;

        }   
        

        private void uploadCloud()
        {
            if (IsConnectedToInternet())
            {
                string result = MessageBox.Show("Are you sure you want to update local database?", "Alert", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning).ToString();

                if (result == "OK")
                {
                    showForm(false);
                    getOrdersList(1);
                }
            }
            else
            {
                MessageBox.Show("Please first connect to the internet!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

             while (order.next!=null)
                {
                    i++;
                content = null;

                while (content == null)
                    {
                        RestResponse response = await client.ExecuteAsync(request);
                        content = response.Content;
                    }
                    Order tempOrder = JsonConvert.DeserializeObject<Order>(content);
                    foreach(Datum datum in tempOrder.data)
                    {
                        order.data.Add(datum);
                    }
                    order.next = tempOrder.next;
                    request.Resource = tempOrder.next;
                }

            return order;
        }

        public async void getOrdersList(int cond)
        {
            saveOrdersList("");
            bool error = false;
            try
            {
                if(IsConnectedToInternet())
                {
                    Order picking = await callApiAsync("https://gfs.api.goflow.com/v1/orders?filters%5Bstatus%5D=in_picking&filters%5Bstatus%5D=in_packing&filters%5Bstatus%5D=ready_to_pick&filter%5Bstore.id%5D=1017");
                    saveOrdersList(JsonConvert.SerializeObject(picking));
                }
                else
                {
                    MessageBox.Show("Please connect to the internet to retrieve the orders list at least once!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }              
            }
            catch
            {
                error = true;
                MessageBox.Show("An unknown error occured, Please Try Again!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (IsConnectedToInternet() && error == false)
                {
                    if (cond == 1)
                        MessageBox.Show("The local database has been updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else if (cond == 0)
                        MessageBox.Show("The local database has been created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                showForm(true);
            }
        }

        private void showForm(bool cond) 
        {
            pictureBox2.Visible = !cond;
            panel10.Visible = cond;
            pictureBox4.Visible = cond;
            pictureBox5.Visible = !cond;
            panel9.Visible = cond;
            pictureBox3.Visible = cond;
        }

        public void saveOrdersList(string jsonData)
        {
            File.WriteAllText("Saves/Orders.json", jsonData);
        }

        private void createDirectory()
        {
            if (!Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");

            if (!File.Exists("Saves/Shelves.json"))
                File.AppendAllText("Saves/Shelves.json", "");

            if (!File.Exists("Saves/Orders.json"))
                File.AppendAllText("Saves/Orders.json", "");

            if (!File.Exists("Saves/Logs.json"))
                File.AppendAllText("Saves/Logs.json", "");

            if (!File.Exists("Saves/OrdersCompleted.json"))
                File.AppendAllText("Saves/OrdersCompleted.json", "");

            if (!File.Exists("logs.txt"))
                File.AppendAllText("logs.txt", "-----Timestamp----------Order Number------------Item Number----Number of Item------Shelf Name------Final Item------");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        
        private void startApp()
        {
            if (isEmpty())
            {
                showForm(false);
                getOrdersList(0);
            }
            else
            {
                showForm(true);
                this.Hide();
                MainPage mainPage = new MainPage(this);
                mainPage.ShowDialog();
                this.Show();
            }
        }

        private void panel6_Click(object sender, EventArgs e)
        {
            startApp();
        }

        private void panel10_Click(object sender, EventArgs e)
        {
            startApp();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            startApp();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            startApp();
        }

      

        private void label1_Click(object sender, EventArgs e)
        {
            uploadCloud();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            uploadCloud();
        }

        private void panel9_Click(object sender, EventArgs e)
        {
            uploadCloud();
        }

        private void panel3_Click(object sender, EventArgs e)
        {
            showForm(false);
            uploadCloud();
        }

        private void OpeningPage_Load(object sender, EventArgs e)
        {
           
        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel9_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
