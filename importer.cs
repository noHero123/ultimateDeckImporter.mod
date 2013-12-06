using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using JsonFx.Json;
using System.Reflection;
using UnityEngine;


namespace deckimporter.mod
{
    class Importer
    {
        public List<Card> allCards = new List<Card>();
        List<int> deckCards = new List<int>();
        List<Card> level2Cards = new List<Card>();
        List<Card> level1Cards = new List<Card>();
        List<Card> level0untradeableCards = new List<Card>();
        List<Card> level0tradeableCards = new List<Card>();
        DeckBuilder2 db;
        MetaMaker mtMkr;
        private MethodInfo dispatchMessages;
        private DeckCardsMessage DeckCardsMess = new DeckCardsMessage();
        bool dontOwnAllCards = false;
        string missingCards = "";
        bool noadded = false;




        public Importer()
        {
            mtMkr = new MetaMaker();
            dispatchMessages = typeof(MiniCommunicator).GetMethod("_dispatchMessageToListeners", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void onLibraryViewReceived(LibraryViewMessage lvm)
        {
            Console.WriteLine("#Library View Received");
            allCards.Clear();
            level2Cards.Clear();
            level1Cards.Clear();
            level0untradeableCards.Clear();
            level0tradeableCards.Clear();
            allCards.AddRange(lvm.cards);
            foreach (Card c in lvm.cards)
            {
                if (c.level == 2) { level2Cards.Add(c); continue; }
                if (c.level == 1) { level1Cards.Add(c); continue; }
                if (c.level == 0 && c.tradable) { level0tradeableCards.Add(c); continue; }
                level0untradeableCards.Add(c);
            }


        }

        public void setDeckbuilder(DeckBuilder2 d)
        { 
            this.db = d;
            mtMkr.setGui3d(d);
        }

        private string createRealDeckMessageString(DeckCardsMessage dcm)
        {
            string retu = "{\"deck\":\"" + dcm.deck + "\",\"metadata\":\"" + dcm.metadata + "\",\"cards\":[";

            for (int i = 0; i < dcm.cards.Length; i++)
            { // example {"id":10364756,"typeId":117,"tradable":true,"isToken":false,"level":0}
                if (i > 0) retu = retu + ",";
                Card c= dcm.cards[i];
                retu = retu + "{\"id\":" + c.id + ",\"typeId\":" + c.getType() + ",\"tradeable\":" + c.tradable.ToString().ToLower() + ",\"isToken\":" + c.isToken.ToString().ToLower() + ",\"level\":" + dcm.cards[i].level + "}";
            }

            retu=retu+"],\"resources\":[";

            for(int i=0;i< dcm.resources.Length;i++)
            {
                if (i > 0) retu = retu + ",";
                retu = retu + "\"" + dcm.resources[i] + "\"";
            }

            retu=retu+"],\"valid\":false,\"msg\":\"DeckCards\"}";
            return retu;
        }



        private void createDeckCardsMessage()
        {
            if (this.deckCards.Count == 0) { this.noadded = true; return; }
            DeckCardsMessage dcm = new DeckCardsMessage();
            //System.Threading.Thread.Sleep(10);
            bool growthused = false, orderused = false, energyused = false, decayused = false;
            // do adding cards here
            List<Card> addedcards = new List<Card>();
            // first add the highest cardlevels

            foreach (Card c in this.level2Cards)
            {
                if (this.deckCards.Contains(c.typeId) && !(addedcards.Exists(x => x.id == c.id)))
                {
                    if (c.getResourceType() == ResourceType.GROWTH) growthused = true;
                    if (c.getResourceType() == ResourceType.ORDER) orderused = true;
                    if (c.getResourceType() == ResourceType.ENERGY) energyused = true;
                    if (c.getResourceType() == ResourceType.DECAY) decayused = true;
                    addedcards.Add(c);
                    this.deckCards.Remove(c.typeId);
                }
            }
            foreach (Card c in this.level1Cards)
            {
                if (this.deckCards.Contains(c.typeId) && !(addedcards.Exists(x => x.id == c.id)))
                {
                    if (c.getResourceType() == ResourceType.GROWTH) growthused = true;
                    if (c.getResourceType() == ResourceType.ORDER) orderused = true;
                    if (c.getResourceType() == ResourceType.ENERGY) energyused = true;
                    if (c.getResourceType() == ResourceType.DECAY) decayused = true;
                    addedcards.Add(c);
                    this.deckCards.Remove(c.typeId);
                }
            }
            // add untradeable bevor tradeable
            foreach (Card c in this.level0untradeableCards)
            {
                if (this.deckCards.Contains(c.typeId) && !(addedcards.Exists(x => x.id == c.id)))
                {
                    if (c.getResourceType() == ResourceType.GROWTH) growthused = true;
                    if (c.getResourceType() == ResourceType.ORDER) orderused = true;
                    if (c.getResourceType() == ResourceType.ENERGY) energyused = true;
                    if (c.getResourceType() == ResourceType.DECAY) decayused = true;
                    addedcards.Add(c);
                    this.deckCards.Remove(c.typeId);
                }
            }
            foreach (Card c in this.level0tradeableCards)
            {
                if (this.deckCards.Contains(c.typeId) && !(addedcards.Exists(x => x.id == c.id)))
                {
                    if (c.getResourceType() == ResourceType.GROWTH) growthused = true;
                    if (c.getResourceType() == ResourceType.ORDER) orderused = true;
                    if (c.getResourceType() == ResourceType.ENERGY) energyused = true;
                    if (c.getResourceType() == ResourceType.DECAY) decayused = true;
                    addedcards.Add(c);
                    this.deckCards.Remove(c.typeId);
                }
            }


            dcm.cards = addedcards.ToArray();

            // create resources string
            int anzressis = 0;
            if (growthused) anzressis++;
            if (orderused) anzressis++;
            if (energyused) anzressis++;
            if (decayused) anzressis++;
            ResourceType[] rt = new ResourceType[anzressis];
            int temp_res = 0;
            if (growthused) { rt[temp_res] = ResourceType.GROWTH; temp_res++; }
            if (orderused) { rt[temp_res] = ResourceType.ORDER; temp_res++; }
            if (energyused) { rt[temp_res] = ResourceType.ENERGY; temp_res++; }
            if (decayused) { rt[temp_res] = ResourceType.DECAY; temp_res++; }
            dcm.resources = rt;
            // set positions
            string meta = mtMkr.getMetaData(addedcards);
            Console.WriteLine("#meta " + meta);
            dcm.metadata = meta;

            dcm.valid = false;
            dcm.deck = "";
            // add the deck!
            Console.WriteLine("set cards");


            //db.handleMessage(dcm); //unsave ...  was unsave, but i writed createRealDeckMessageString, so i have to use the other version (or my work would be needless)!


            string rdms = this.createRealDeckMessageString(dcm);
            Message msg = MessageFactory.create(MessageFactory.getMessageName(rdms), rdms);
            Console.WriteLine("#" + msg.getRawText());
            dispatchMessages.Invoke(App.Communicator,new object[]{msg});
            if (this.deckCards.Count > 0)
            {
                this.dontOwnAllCards = true;
                CardTypeManager ctm=CardTypeManager.getInstance();
                    foreach (int ii in deckCards)
                    {
                        if (this.missingCards != "") this.missingCards = this.missingCards + ", ";
                        CardType type = ctm.get(ii);
                        this.missingCards = this.missingCards + type.name;
                    }
          
            }

        }


        private void readDeckCardsFromScrollsguide(string s)
        {
            Console.WriteLine("#read deck cards");
            this.deckCards.Clear();
            JsonReader jsonReader = new JsonReader();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(s);
            dictionary = (Dictionary<string, object>)dictionary["data"];
            Dictionary<string, object>[] scrolls = (Dictionary<string, object>[])dictionary["scrolls"];
            foreach (Dictionary<string, object> d in scrolls)
            {
                int id =(int) d["id"];
                int anz = (int)d["c"];
                for (int i = 0; i < Math.Min(anz, 3); i++)
                {
                    deckCards.Add(id);
                }

            }
            
        }

        private void readDeckCardsFromScrollsPW(string s)
        {
            Console.WriteLine("#read deck cards");
            this.deckCards.Clear();
            string ss = s;
            ss = ss.Split(new string[]{"<tbody>"}, StringSplitOptions.None)[1];
            ss = ss.Split(new string[] { "</tbody>" }, StringSplitOptions.None)[0];
            string[] stringarray= ss.Split(new string[]{"<tr data-id=\""}, StringSplitOptions.RemoveEmptyEntries);
            stringarray[0] = "#";
            foreach (string lolo in stringarray)
            {
                if (lolo == "#") continue;
                string idd = lolo.Split('"')[0];
                int id = Convert.ToInt32(idd);
                string anzz=lolo.Split(new string[]{"<span class=\"badge badge-inverse\">"}, StringSplitOptions.None)[1];
                anzz = anzz.Split(new string[] { "</span>" }, StringSplitOptions.None)[0];
                Console.WriteLine(anzz);
                int anz = Convert.ToInt32(anzz);
                for (int i = 0; i < Math.Min(anz, 3); i++)
                {
                    deckCards.Add(id);
                }

            }

        }

        private void readDeckCardsFromScrollsPWShare(string s)
        {
            Console.WriteLine("#read deck cards");
            this.deckCards.Clear();
            string[] stringarray = s.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string lolo in stringarray)
            {
                string idd = lolo.Split(',')[0];
                int id = Convert.ToInt32(idd);
                string anzz = lolo.Split(',')[1];
                int anz = Convert.ToInt32(anzz);
                for (int i = 0; i < Math.Min(anz, 3); i++)
                {
                    deckCards.Add(id);
                }

            }

        }

        private void readDeckCardsFromSeeMeScrollin(string s)
        {
            Console.WriteLine("#read deck cards");
            this.deckCards.Clear();
            string[] stringarray = s.Split(new string[] { "<div class='clearboth'>" }, StringSplitOptions.RemoveEmptyEntries);
            stringarray[0] = "#";
            foreach (string lolo in stringarray)
            {
                if (lolo == "#") continue;
                string name = lolo.Split(new string[] { "<br/></div>" }, StringSplitOptions.None)[0];
                name = name.Substring(3).ToLower();
                string anzz = lolo.Split('x')[0];
                int anz = Convert.ToInt32(anzz);
                if (this.allCards.Exists(x => x.getName().ToLower() == name))
                {
                    int id = (int)(this.allCards.Find(x => x.getName().ToLower() == name)).getType();
                    for (int i = 0; i < Math.Min(anz,3); i++)
                    {
                        deckCards.Add(id);
                    }
                }

            }

        }


        public string importFromURL(object urll)
        {
            this.dontOwnAllCards = false;
            this.missingCards = "";
            this.noadded = false;

            string url=urll as string;
            if(url.StartsWith("http://"))url=url.Replace("http://","");
            if (url.StartsWith("https://")) { url = url.Replace("https://", ""); System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true; };

            if(url.StartsWith("www.scrollsguide.com/deckbuilder/#")) 
            {
                url = url.Replace("www.scrollsguide.com/deckbuilder/#", "");
                Uri urri = new Uri("http://a.scrollsguide.com/deck/load?id=" + url);
                Console.WriteLine("#load from " + urri.AbsoluteUri);
                WebRequest myWebRequest = WebRequest.Create(urri);
                myWebRequest.Timeout = 5000;
                WebResponse myWebResponse = myWebRequest.GetResponse();
                System.IO.Stream stream = myWebResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
                string ressi = reader.ReadToEnd();
                Console.WriteLine("#get: " + ressi);
                this.readDeckCardsFromScrollsguide(ressi);
                this.createDeckCardsMessage();
                if (this.noadded) return "noadded";
                if (this.dontOwnAllCards) return "You dont own: " + this.missingCards;
                return "ok";

            }

            //theyseemescrollin.com
            if (url.StartsWith("theyseemescrollin.com/deck/"))
            {
                Uri urri = new Uri("http://" + url);
                Console.WriteLine("#load from " + urri.AbsoluteUri);
                WebRequest myWebRequest = WebRequest.Create(urri);
                myWebRequest.Timeout = 5000;
                WebResponse myWebResponse = myWebRequest.GetResponse();
                System.IO.Stream stream = myWebResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
                string ressi = reader.ReadToEnd();
                readDeckCardsFromSeeMeScrollin(ressi);
                this.createDeckCardsMessage();
                if (this.noadded) return "noadded";
                if (this.dontOwnAllCards) return "You dont own: " + this.missingCards;
                return "ok";
            }
            return "notok";
            // DAMN, it issnt up to date (decay is missing)
            if (url.StartsWith("scrolls.famousframes.de/")) //scrolls.famousframes.de = builder.scrolls.pw
            {
                url = url.Replace("scrolls.famousframes.de/", "builder.scrolls.pw/");
            }
            

            //http://builder.scrolls.pw/builds/64
            if (url.StartsWith("builder.scrolls.pw/builds/"))
            {
                Uri urri = new Uri("http://" + url);
                                Console.WriteLine("#load from " + urri.AbsoluteUri);
                WebRequest myWebRequest = WebRequest.Create(urri);
                myWebRequest.Timeout = 5000;
                WebResponse myWebResponse = myWebRequest.GetResponse();
                System.IO.Stream stream = myWebResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
                string ressi = reader.ReadToEnd();
                Console.WriteLine("#get: " + ressi);
                this.readDeckCardsFromScrollsPW(ressi);
                this.createDeckCardsMessage();
                if (this.noadded) return "noadded";
                if (this.dontOwnAllCards) return "You dont own: " + this.missingCards;
                return "ok";
            }
            //or http://builder.scrolls.pw/?l=1,3:2,3:13,3:15,3:43,3:44,3
            if (url.StartsWith("builder.scrolls.pw/?l="))
            {

                string ressi = url.Replace("builder.scrolls.pw/?l=", "");
                this.readDeckCardsFromScrollsPWShare(ressi);
                this.createDeckCardsMessage();
                if (this.noadded) return "noadded";
                if (this.dontOwnAllCards) return "You dont own: " + this.missingCards;
                return "ok";

            }
            return "notok";
        }



    }
}
