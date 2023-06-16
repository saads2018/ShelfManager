using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelfManager.Models
{
    public class Order
    {
        public List<Datum> data { get; set; }
        public string next { get; set; }
    }


    public class Datum
    {
        public int id { get; set; }
        public string order_number { get; set; }
        public string status { get; set; }
        public Nullable<DateTime> status_updated_at { get; set; }
        public List<Line> lines { get; set; }
    }

    public class Line
    {
        public string id { get; set; }
        public Product product { get; set; }
        public Listing listing { get; set; }
        public Quantity quantity { get; set; }
    }

    public class Listing
    {
        public string store_provided_id { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
    }

    public class Product
    {
        public int id { get; set; }
        public string item_number { get; set; }
        public string description { get; set; }
    }

    public class Quantity
    {
        public int amount { get; set; }
    }

  
}
