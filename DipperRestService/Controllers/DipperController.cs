using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Management;
using NLog;

namespace DipperRestService.Controllers
{
    public class DipperController : ApiController
    {
        private string _folderPath = string.Empty;
        private string _checkinFilepath = string.Empty;
        private string _newImageFilepath = string.Empty;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private const string Action = "Action";
        private const string GetCheckins = "GetCheckins";
        private const string Checkin = "Checkin";
        private const string DefaultErrorResponse = "Internal Server Error";
        private const string Result = "Result";

        public string[] Get()
        {
            try
            {
                _logger.Info("Get message received at " + DateTime.UtcNow);
                string action = GetActionType(HttpContext.Current);
            
                if (string.IsNullOrEmpty(_folderPath))
                {
                    ReadConfigValues();
                }

                if (action == GetCheckins)
                {
                    _logger.Info("Getting checkins at " + DateTime.UtcNow);
                    using (StreamReader sr = new StreamReader(_folderPath + _checkinFilepath))
                    {
                        return sr.ReadLine().Split(',');
                    }
                }

                return new [] {String.Format("Please include a valid action header, the choices are: {0}", GetCheckins)};
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new []{DefaultErrorResponse};
            }
        }

        public HttpResponseMessage Post()
        {
            try
            {
                _logger.Info("Post message received at " + DateTime.UtcNow);
                if (string.IsNullOrEmpty(_folderPath))
                {
                    ReadConfigValues();
                }

                string action = GetActionType(HttpContext.Current);

                if (action == Checkin)
                {
                    string imageStatus = CheckinAndGetImageStatus();
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Headers.Add(Result, imageStatus);
                    return response;
                }

                // if no specific action is taken to return a different code, return a bad request
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        private string GetActionType(HttpContext current)
        {
            var ctx = HttpContext.Current;
            return ctx.Request.Headers[Action];
        }

        private string CheckinAndGetImageStatus()
        {
            _logger.Info("Attempting to read in {0} at {1} ", _checkinFilepath, DateTime.UtcNow);

            List<string> previousCheckins = new List<string>();

            using (StreamReader sr = new StreamReader(_folderPath + _checkinFilepath))
            {
                previousCheckins = sr.ReadLine().Split(',').ToList();
            }

            if (previousCheckins.Count > 5)
            {
                previousCheckins = previousCheckins.Skip(1).ToList();
            }

            previousCheckins.Add(DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm"));

            _logger.Info("Attempting to write out to {0} at {1} ", _checkinFilepath, DateTime.UtcNow);

            using (StreamWriter sw = new StreamWriter(_folderPath + _checkinFilepath, false))
            {
                sw.Write(string.Join(",", previousCheckins));
            }

            _logger.Info("Attempting to read in {0} at {1}", _newImageFilepath, DateTime.UtcNow);

            string responseVal = String.Empty;

            using (StreamReader sr = new StreamReader(_folderPath + _newImageFilepath))
            {
                responseVal = sr.ReadLine();
            }

            if (responseVal == true.ToString())
            {
                _logger.Info("Attempting to write out to {0} at {1}", _newImageFilepath, DateTime.UtcNow);
                using (StreamWriter sw = new StreamWriter(_folderPath + _newImageFilepath, false))
                {
                    sw.Write(false.ToString());
                }
            }

            return responseVal;
        }

        private void ReadConfigValues()
        {
            _folderPath = ConfigurationManager.AppSettings["FolderPath"];
            _checkinFilepath = ConfigurationManager.AppSettings["CheckinFilepath"];
            _newImageFilepath = ConfigurationManager.AppSettings["NewImageFilepath"];
        }
    }
}
