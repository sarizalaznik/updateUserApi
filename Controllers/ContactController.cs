using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Xrm.Tooling.Connector;
using crmWebApplication.BL;
using crmWebApplication.Models;
using System.Web.Http.Cors;

namespace crmWebApplication.Controllers
{
    [EnableCors("*", "*", "*")]
    public class ContactController : ApiController
    {
        CrmBL crmBL = new CrmBL();

        public ContactController(){}
        [HttpGet]
        public HttpResponseMessage Get()
        {
            return crmBL.get();
        }

        [HttpPatch]
        public HttpResponseMessage Patch([FromBody] User user)
        {
            return crmBL.Patch(user);
        }
    }
}
