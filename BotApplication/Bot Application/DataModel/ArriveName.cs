using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.DataModel
{
    public class ArriveName
    {
        public string AreaName { get; set; }
        public string Name { get; set; }
    }

    public class Root
    {
        public List<ArriveName> AreaName { get; set; }
    }
}