using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelfManager.Models
{
    public class Shelf
    {
        public string Name { get; set; }
        public int Size { get; set; } 
        public int Priority { get; set; }
        public Nullable<int> open { get; set; } = 1;
        public string orderNumber { get; set; } = string.Empty;
        public int itemsCount { get; set; } = 0;
        public List<Item> items { get; set; } = new List<Item>();

    }

    public class Item
    {
        public string itemNumber { get; set; }
        public int itemQuantity { get; set; }=0;
    }
}
