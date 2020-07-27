using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class TempUser :IModel
    {
        public Guid Id { get; set; }
        public ICollection<ConnectModels> ConnectModels { get; set; }
    }
}