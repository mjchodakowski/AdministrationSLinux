using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class ModelKartSieciowych
    {
        
        public string nr { get; set; }
        public string nazwa { get; set; }
        public string info { get; set; }
        public int mtu { get; set; }
        public string qdisk { get; set; }
        public string state { get; set; }
        public string group { get; set; }
        public string qlen { get; set; }
        public string inet { get; set; }
        public string inet_info { get; set; }
        public string inet6 { get; set; }
        public string inet6_info { get; set; }
        public string mac { get; set; }

    }
}