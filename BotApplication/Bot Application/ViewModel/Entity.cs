using System;
using System.Collections.Generic;
using System.Text;

namespace LionTourBot.Model.ViewModel
{
    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public double score { get; set; }
    }
}
