using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:8011/api/Dipper");
            client.DefaultRequestHeaders.Add("action", "GetCheckins");

            HttpResponseMessage response = client.GetAsync("").Result;
            var results = response.Content.ReadAsStringAsync();

            string wtf = "wtf";
        }
    }
}
