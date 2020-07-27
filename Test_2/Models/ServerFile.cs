using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace Test_2.Models
{
    public class ServerFile
    {
        [Key]
        public int Id{get;set;}
        public string Nazwa { get; set; }
        public DateTime DataUtworzenia { get; set; }
        public string Komentarz { get; set; }
        public string NalezyDo { get; set; }
        public string Rozmiar { get; set; }
        public string Rozmiar_File { get; set; }
        public string Typ { get; set; }
    }
}