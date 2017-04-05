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
            //client.BaseAddress = new Uri("http://52.41.79.4/api/Dipper");
            //client.DefaultRequestHeaders.Add("action", "GetCheckins");

            client.BaseAddress = new Uri("http://localhost:8011/api/Dipper");
            client.DefaultRequestHeaders.Add("action", "Checkin");


            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("", "login")
            });

            HttpResponseMessage response = client.PostAsync("", content).Result;

            //HttpResponseMessage response = client.GetAsync("").Result;
            //var results = response.Content.ReadAsStringAsync();

            string wtf = "wtf";
        }
    }
}
