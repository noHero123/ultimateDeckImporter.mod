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
        string tempmessage;

        public struct Deckstatistics
        {
            public int anzunownedscrolls ;
            public List<int> unownedscrolls ;
            public bool containsGrowth;
            public bool containsEnergy;
            public bool containsOrder;
            public bool containsDecay;
            public string missingCards;
            public int numberOfScrollsInDeck;
            public string deckList;

        }


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



        private void createDeckCardsMessage(bool testonly)
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
            if (!testonly)
            {
                string meta = mtMkr.getMetaData(addedcards);
                Console.WriteLine("#meta " + meta);
                dcm.metadata = meta;
            }
            dcm.valid = false;
            dcm.deck = "";
            // add the deck!
            Console.WriteLine("set cards");

           
            //db.handleMessage(dcm); //unsave ...  was unsave, but i writed createRealDeckMessageString, so i have to use the other version (or my work would be needless)!


            string rdms = this.createRealDeckMessageString(dcm);
            Message msg = MessageFactory.create(MessageFactory.getMessageName(rdms), rdms);
            if (testonly)
            {
                this.tempmessage = rdms;
            }
            Console.WriteLine("#" + msg.getRawText());
            if (!testonly) dispatchMessages.Invoke(App.Communicator, new object[] { msg });
            this.missingCards = "";
            if (this.deckCards.Count > 0)
            {
                this.dontOwnAllCards = true;
                CardTypeManager ctm=CardTypeManager.getInstance();
                Dictionary<string, int> idNotOwnedNumber = new Dictionary<string, int>();
                foreach (int ii in deckCards)
                {
                    CardType type = ctm.get(ii);
                    if (idNotOwnedNumber.ContainsKey(type.name))
                    {
                        idNotOwnedNumber[type.name] += 1;
                    }
                    else
                    {
                        idNotOwnedNumber.Add(type.name, 1);
                    }

                }

                foreach (KeyValuePair<string, int> kvp in idNotOwnedNumber.OrderBy(i => i.Key))
                    {
                        if (this.missingCards != "") this.missingCards = this.missingCards + "\r\n";
                        CardType type = ctm.get(kvp.Key);
                        this.missingCards = this.missingCards + kvp.Value +"x "+kvp.Key;
                    }
          
            }

        }


        private string getDecklist()
        {
            string retu = "";
            CardTypeManager ctm = CardTypeManager.getInstance();
            Dictionary<string, int> idNotOwnedNumber = new Dictionary<string, int>();
            foreach (int ii in deckCards)
            {
                CardType type = ctm.get(ii);
                if (idNotOwnedNumber.ContainsKey(type.name))
                {
                    idNotOwnedNumber[type.name] += 1;
                }
                else
                {
                    idNotOwnedNumber.Add(type.name, 1);
                }

            }
            foreach (KeyValuePair<string, int> kvp in idNotOwnedNumber.OrderBy(i => i.Key))
            {
                if (retu != "") retu = retu + "\r\n";
                retu = retu + kvp.Value + "x " + kvp.Key;
            }

            return retu;
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
            foreach (string lol in stringarray)
            {
                string lolo= lol;
                if (!(lolo.Contains(","))) lolo = lolo + ",3";
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
                this.createDeckCardsMessage(false);
                if (this.noadded) return "noadded";
                if (this.dontOwnAllCards) return "You dont own: \r\n" + this.missingCards;
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
                this.createDeckCardsMessage(false);
                if (this.noadded) return "noadded";
                if (this.dontOwnAllCards) return "You dont own: " + this.missingCards;
                return "ok";
            }

            if (url.StartsWith("www.UltimateDeckImporter.com/?l="))
            {

                string ressi = url.Replace("www.UltimateDeckImporter.com/?l=", "");
                this.readDeckCardsFromScrollsPWShare(ressi);
                this.createDeckCardsMessage(false);
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
                this.createDeckCardsMessage(false);
                if (this.noadded) return "noadded";
                if (this.dontOwnAllCards) return "You dont own: " + this.missingCards;
                return "ok";
            }
            //http://scrolls.famousframes.de/?l=1,3:4,1:13,1   = ?l + scrollsid,number:nextscrollsid,number:...
            //or http://builder.scrolls.pw/?l=1,3:2,3:13,3:15,3:43,3:44,3

            if (url.StartsWith("builder.scrolls.pw/?l="))
            {

                string ressi = url.Replace("builder.scrolls.pw/?l=", "");
                this.readDeckCardsFromScrollsPWShare(ressi);
                this.createDeckCardsMessage(false);
                if (this.noadded) return "noadded";
                if (this.dontOwnAllCards) return "You dont own: " + this.missingCards;
                return "ok";

            }
            return "notok";
        }


        public Deckstatistics getdata(string link)
        {
            Deckstatistics retu= new Deckstatistics();
            retu.anzunownedscrolls = 0;
            retu.unownedscrolls = new List<int>();
            Console.WriteLine("workin on link " + link);
            string url = link;
                retu.containsDecay=false;
                retu.containsEnergy=false;
                retu.containsGrowth=false;
                retu.containsOrder=false;
                string ressi = url.Replace("http://www.UltimateDeckImporter.com/?l=", "");
                ressi = ressi.Replace("www.UltimateDeckImporter.com/?l=", "");
                ressi = ressi.Replace("http://", "");                
                this.readDeckCardsFromScrollsPWShare(ressi);
                retu.numberOfScrollsInDeck = this.deckCards.Count();
                retu.deckList = this.getDecklist();
                CardTypeManager ctm = CardTypeManager.getInstance();
                foreach(int iii in this.deckCards)
                {
                    CardType type = ctm.get(iii);
                    if (type.costDecay >= 1) retu.containsDecay=true;
                    if (type.costEnergy >= 1) retu.containsEnergy = true;
                    if (type.costOrder >= 1) retu.containsOrder = true;
                    if (type.costGrowth >= 1) retu.containsGrowth = true;
                }

                this.createDeckCardsMessage(true);//for getting missed scrolls
                retu.anzunownedscrolls = this.deckCards.Count();
                retu.missingCards = this.missingCards;
                foreach (int ii in this.deckCards) { retu.unownedscrolls.Add(ii); }
                
                

            
            return retu; 


        }


    }
}
