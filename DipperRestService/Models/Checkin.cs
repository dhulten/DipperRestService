using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DipperRestService.Models
{
    public class Checkin
    {
        public string DateStr { get; set; }

        public Checkin(string dateStr)
        {
            DateStr = dateStr;
        }
    }
}