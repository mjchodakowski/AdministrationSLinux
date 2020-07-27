using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class FoldersChange
    {
        public Guid Id { get; set; }
        public string nazwa { get; set; }
        public string adres { get; set; }
        public string Log { get; set; }
        public string File { get; set; }
        public string nameNewFolder { get; set; }
        public string Del { get; set; }
        public string Del_new { get; set; }
    }
    

}