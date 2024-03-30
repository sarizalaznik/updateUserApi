using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using crmWebApplication.BL;
using crmWebApplication.Models;
using System.Web.Http;
using System.Net.Http;
using System;
using RouteAttribute = System.Web.Http.RouteAttribute;
using HttpPatchAttribute = System.Web.Http.HttpPatchAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;

namespace crmWebApplication.Controllers
{
    public class CrmController : Controller
    {
        CrmBL crmBL = new CrmBL();
        // GET: Crm
      
        //public HttpResponseMessage Patch(User user)
        //{
        //    return crmBL.UpdateContact(user);
        //}

        // GET api/Crm
        public string Get()
        {
            return "ברוך השם";
            //return crmBL.get();
        }


    }
}
