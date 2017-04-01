using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace DipperRestService.Controllers
{
    public class DipperController : ApiController
    {
        private string FolderPath = string.Empty;
        private string CheckinFilepath = string.Empty;

        public string[] Get()
        {
            var ctx = HttpContext.Current;
            string action = ctx.Request.Headers["action"];
            
            if (string.IsNullOrEmpty(FolderPath))
            {
                ReadConfigValues();
            }

            if (action == "GetCheckins")
            {
                using (StreamReader sr = new StreamReader(FolderPath + CheckinFilepath))
                {
                    return sr.ReadLine().Split(',');
                }
            }

            return new string[] {"There are real results returned from the service, seriously"};
        }

        private void ReadConfigValues()
        {
            FolderPath = ConfigurationManager.AppSettings["FolderPath"];
            CheckinFilepath = ConfigurationManager.AppSettings["CheckinFilepath"];
        }
    }
}
