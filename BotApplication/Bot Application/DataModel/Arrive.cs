using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.DataModel
{
    public class Arrive
    {
        public string IslandID { get; set; }
        public string CountryCode { get; set; }
        public string ArriveID { get; set; }
        public string ArriveName { get; set; }
        public string CountryName { get; set; }
        public string IslandName { get; set; }

    }
    public class RootObject
    {
        public List<Arrive> ArriveID { get; set; }
    }
}