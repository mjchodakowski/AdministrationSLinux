using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class cardIP
    {
        public string Name{ get; set; }
        public string Function { get; set; }


        public cardIP(string name_,string function_)
        {
            Name = name_;
            Function = function_;
        }
    }
}