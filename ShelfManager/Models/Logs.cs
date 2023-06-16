using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelfManager.Models
{
    public class Logs
    {
        public string Time_Stamp { get; set; }
        public string Order_Number { get; set; }
        public string Item_Number { get; set; }
        public string Shelf_Name { get; set; } = "Empty";
        public string No_Of_Item { get; set; } = "Empty";
        public string Final_Item { get; set; } = "Empty";


    }
}
