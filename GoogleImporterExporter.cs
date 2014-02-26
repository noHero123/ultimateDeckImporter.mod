using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JsonFx.Json;

namespace deckimporter.mod
{
   public class GoogleImporterExporter
    {
        //Database https://docs.google.com/spreadsheet/ccc?key=0AhhxijYPL-BGdDVOVFhUVzN3U3RyVTlGR1FYQ1VqUGc&usp=drive_web#gid=0
        //PostSite 
        string form = "https://docs.google.com/forms/d/19gk-tjiijeIJPLlwdqvcAGVSGvcAQ2iS3zKJ3UzbIBc/";

        public bool workthreadready = true;

        public struct sharedItem
        {
            public string time;
            public string player;
            public string deckname;
            public string link;
            public string desc;
        }

        List<string> entrys = new List<string>();
        List<string> keys = new List<string>();
        public List<sharedItem> sharedDecks = new List<sharedItem>();

        public GoogleImporterExporter()
        {
            this.keys.Clear();
            this.keys.Add("entry." + "150544798"+"=");//playername
            this.keys.Add("entry." + "775296865" + "=");//link
            this.keys.Add("entry." + "904085848" + "=");//deckname
            this.keys.Add("entry." + "1932172094" + "=");//description

        }

        public string getDataFromGoogleDocs()
        {
            WebRequest myWebRequest;
            myWebRequest = WebRequest.Create("https://spreadsheets.google.com/feeds/list/" + "0AhhxijYPL-BGdDVOVFhUVzN3U3RyVTlGR1FYQ1VqUGc"+ "/od6/public/values?alt=json");
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string ressi = reader.ReadToEnd();
            return ressi;
        }


        public void postDataToGoogleForm(List<string> entrys)
        {
            string txt = "";
            int i = 0;
            foreach (string t in entrys)
            {
                txt = txt + this.keys[i] + System.Uri.EscapeDataString(t) + "&";
                i++;
            }
            string txt1 = txt;
            Console.WriteLine("##sendTogoogle: "+txt1);
            byte[] bytes = Encoding.ASCII.GetBytes(txt1 + "draftResponse=%5B%5D%0D%0A&pageHistory=0");

            HttpWebRequest webRequest = HttpWebRequest.Create(form + "formResponse") as HttpWebRequest;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            webRequest.Referer = form + "viewform";
            webRequest.ContentLength = bytes.Length;
            Stream requestStream = webRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            String data = readStream.ReadToEnd();
            Console.WriteLine(data);
            receiveStream.Close();
            readStream.Close();
            response.Close();
        }


        public void readJsonfromGoogle(string txt)
        {
            Console.WriteLine(txt);
            JsonReader jsonReader = new JsonReader();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(txt);
            dictionary = (Dictionary<string, object>)dictionary["feed"];
            Dictionary<string, object>[] entrys = (Dictionary<string, object>[])dictionary["entry"];
            sharedDecks.Clear();
            for (int i = 0; i < entrys.GetLength(0); i++)
            {
                sharedItem si = new sharedItem();
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$timestamp"];
                si.time = (string)dictionary["$t"];
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$playername"];
                si.player = (string)dictionary["$t"];
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$link"];
                si.link = (string)dictionary["$t"];
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$deckname"];
                si.deckname = (string)dictionary["$t"];
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$description"];
                si.desc = (string)dictionary["$t"];
                if (si.link.StartsWith("DELETE")) continue;
                this.sharedDecks.Add(si);
                Console.WriteLine(si.player + " " + si.deckname);
            }

           
        }

        public void workthread()
        {
            this.workthreadready = false;
            this.readJsonfromGoogle(this.getDataFromGoogleDocs());
            this.workthreadready = true;
        }


    }
}
