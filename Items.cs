using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaClientApplication
{
    public class DataGridItems
    {
        public bool Checked { get; set; }
        public string Verb { get; set; }
        public string Prep { get; set; }
        public double LogDice { get; set; }
        public double MinSen { get; set; }
        
    }

    public class DataGridItemsWithCount: DataGridItems
    {
        public int Count { get; set; }
        public int Selected { get; set; }
    }
    
}
