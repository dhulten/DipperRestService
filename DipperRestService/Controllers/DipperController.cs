using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Management;
using System.Web.Mvc;
using DipperRestService.Models;
using Newtonsoft.Json;
using NLog;

namespace DipperRestService.Controllers
{
    public class DipperController : ApiController
    {
        private string _folderPath = string.Empty;
        private string _checkinFilepath = string.Empty;
        private string _imageStatusFilepath = string.Empty;
        private string _imageFilename = string.Empty;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private const string Action = "Action";
        private const string GetCheckins = "GetCheckins";
        private const string Checkin = "Checkin";
        private const string UploadImage = "UploadImage";
        private const string CheckinMode = "CheckinMode";
        private const string AdminMode = "AdminMode";
        private const string ImageBytes = "ImageBytes";
        private const string DefaultErrorResponse = "Internal Server Error";
        private const string Result = "Result";
        private const int MaxCheckinsLogged = 4;

        public JsonResult Get()
        {
            try
            {
                _logger.Info("Get message received");
                string action = GetHeaderByKey(Action);
            
                if (string.IsNullOrEmpty(_folderPath))
                {
                    ReadConfigValues();
                }

                if (action == GetCheckins)
                {
                    _logger.Info("Getting checkins");
                    using (StreamReader sr = new StreamReader(_folderPath + _checkinFilepath))
                    {
                        return new JsonResult {Data = sr.ReadLine()};
                    }
                }

                return new JsonResult {Data = String.Format("Please include a valid action header, the choices are: {0}", GetCheckins)}; 
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new JsonResult {Data = DefaultErrorResponse};
            }
        }

        public HttpResponseMessage Post()
        {
            try
            {
                _logger.Info("Post message received");
                if (string.IsNullOrEmpty(_folderPath))
                {
                    ReadConfigValues();
                }

                string action = GetHeaderByKey(Action);

                if (action == Checkin)
                {
                    string checkinMode = GetHeaderByKey(CheckinMode);
                    string imageStatus = CheckinAndGetImageStatus(checkinMode);

                    HttpResponseMessage response = new HttpResponseMessage();
                    if (imageStatus == DefaultErrorResponse)
                    {
                        response.StatusCode = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        response.StatusCode = HttpStatusCode.OK;
                        response.Headers.Add(Result, imageStatus);    
                    }
                    
                    return response;
                }
                
                if (action == UploadImage)
                {
                    string imageByteString;
                    HttpResponseMessage response = new HttpResponseMessage();

                    try
                    {
                        imageByteString = HttpContext.Current.Request.Params[ImageBytes];
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    
                    bool imageSaveSuccess = SaveImage(imageByteString);

                    response.StatusCode = imageSaveSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

                    return response;
                }

                // if no specific action is taken return a bad request
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        private string GetHeaderByKey(string key)
        {
            var ctx = HttpContext.Current;
            return ctx.Request.Headers[key];
        }


        private bool SaveImage(string imageByteStr)
        {
            try
            {
                Byte[] imageBytes = Convert.FromBase64String(imageByteStr);

                string currDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullImagePath = currDir + _imageFilename;

                FileInfo imageInfo = new FileInfo(fullImagePath);

                if (imageInfo.Exists)
                {
                    File.Delete(fullImagePath);
                }

                File.WriteAllBytes(fullImagePath, imageBytes);

                // update new image status
                using (StreamWriter sw = new StreamWriter(_folderPath + _imageStatusFilepath, false))
                {
                    sw.Write(true.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }

        }

        private string CheckinAndGetImageStatus(string checkinMode)
        {
            try
            {
                _logger.Info("Attempting to checkin and get image status");

                if (checkinMode != AdminMode)
                {
                    List<Checkin> previousCheckins = new List<Checkin>();

                    using (StreamReader sr = new StreamReader(_folderPath + _checkinFilepath))
                    {
                        string checkinsJson = sr.ReadLine();

                        if (!string.IsNullOrEmpty(checkinsJson))
                        {
                            previousCheckins = JsonConvert.DeserializeObject<List<Checkin>>(checkinsJson);
                        }
                    }

                    if (previousCheckins.Count > MaxCheckinsLogged)
                    {
                        previousCheckins = previousCheckins.Take(MaxCheckinsLogged).ToList();
                    }

                    previousCheckins.Insert(0, new Checkin(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")));

                    using (StreamWriter sw = new StreamWriter(_folderPath + _checkinFilepath, false))
                    {
                        sw.Write(JsonConvert.SerializeObject(previousCheckins));
                    }

                    string responseVal = String.Empty;

                    using (StreamReader sr = new StreamReader(_folderPath + _imageStatusFilepath))
                    {
                        responseVal = sr.ReadLine();
                    }

                    if (responseVal == true.ToString())
                    {
                        using (StreamWriter sw = new StreamWriter(_folderPath + _imageStatusFilepath, false))
                        {
                            sw.Write(false.ToString());
                        }
                    }

                    return responseVal;
                }
                else
                {
                    _logger.Info("Admin mode enabled, not recording checkin, sending result to force picture download.");
                    return true.ToString();
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return DefaultErrorResponse;
            }

        }

        private void ReadConfigValues()
        {
            _folderPath = ConfigurationManager.AppSettings["FolderPath"];
            _checkinFilepath = ConfigurationManager.AppSettings["CheckinFilepath"];
            _imageStatusFilepath = ConfigurationManager.AppSettings["ImageStatusFilepath"];
            _imageFilename = ConfigurationManager.AppSettings["ImageFilename"];
        }
    }
}
