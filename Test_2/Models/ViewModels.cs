using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class CompanyViewModel
    {
        public IEnumerable<Test_2.Models.UserMainProfileModels> UserMainProfileModel { get; set; }
        public IEnumerable<Test_2.Models.ProfileUser> ProfileUser_ { get; set; }
    }
}