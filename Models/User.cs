using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace crmWebApplication.Models
{
    public class User
    {
        public string Id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string phone { get; set; }
    }
}