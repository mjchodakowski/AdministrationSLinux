using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class ProfilPolaczenia
    {
        [Key]
        public int Id { get; set; }
        public string Ip { get; set; }
        public string Port { get; set; }
        public string NazwaUser { get; set; }

        [DataType(DataType.Password)]
        public string UserKey { get; set; }

        [DataType(DataType.Password)]
        public string RootKey { get; set; }
    }
}