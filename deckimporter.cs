using System;

using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using JsonFx.Json;
using System.Text.RegularExpressions;
using System.Threading;



namespace deckimporter.mod
{
    public class deckimporter : BaseMod, ICommListener, IOkStringCancelCallback, IOkCallback, IOkCancelCallback
	{

        bool invalidDeck = false;
        DeckCardsMessage invalidDeckMessage;
        bool showMissing = false;

        int oldscreenx=0, oldscreeny=0;
        private int numberScrollsOnBoard = 0;
        private string choosenname = "";
        bool showJoinMessage = true;
        GUISkin lobbyskin;
        bool showImportMenu = false;
        Importer imp;
        DeckBuilder2 db;
        DeckSaveMessage copydeck;
        Deckcreator dckcrtr;
        GoogleImporterExporter googleie;
        DecksearchUI dcksrchui;
        private MethodInfo generateDeckSaveMessage;
        private bool buildmode = false;
        GUISkin buttonSkin = (GUISkin)Resources.Load("_GUISkins/Lobby");
        private FieldInfo chatLogStyleinfo;
        private FieldInfo scrollsBookinfo;
        private FieldInfo scrollsBookRect1info, scrollsBookRect2info;
        GUIStyle chatlogstye;

        //for copying string into buffer
        Type T = typeof(GUIUtility);
        PropertyInfo systemCopyBufferProperty;
        string deckfolder = "";


        public void handleMessage(Message msg)
        { // collect data for enchantments (or units who buff)

            if ( showJoinMessage && msg is RoomChatMessageMessage )
            {
                RoomChatMessageMessage rcmm = (RoomChatMessageMessage)msg;
                if (rcmm.text.StartsWith("You have joined"))
                {

                    RoomChatMessageMessage nrcmm = new RoomChatMessageMessage("[note]", "DeckImporter was loadet");
                        nrcmm.from = "UltimateDeckImporter";
                        App.ArenaChat.handleMessage(nrcmm);
                        this.showJoinMessage = false;
                        

                }
            }

            if (!this.dckcrtr.sendCheated && msg is LibraryViewMessage)
            {
                if ((((LibraryViewMessage)msg).profileId == App.MyProfile.ProfileInfo.id))
                {
                    imp.onLibraryViewReceived(msg as LibraryViewMessage);
                    dckcrtr.setOrginalLibraryView(msg);
                }
            }

            if (msg is CardTypesMessage)
            {
                dckcrtr.receiveCardlist(msg as CardTypesMessage);
            }

            // doesnt work, scrolls deletes the cards from the deckcardsmessage automatically
            /*if (!this.buildmode && msg is DeckCardsMessage) // repair mode
            {
                DeckCardsMessage dcm = msg as DeckCardsMessage;
                if (dcm.valid) return;
                this.invalidDeck = true;
                this.invalidDeckMessage = dcm;

            }*/

            return;
        }


        public void onConnect(OnConnectData ocd)
        {
            return; // don't care
        }


		//initialize everything here, Game is loaded at this point
        public deckimporter()
		{


            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) ? Environment.GetEnvironmentVariable("HOME") : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            deckfolder = homePath + Path.DirectorySeparatorChar + "scrollsdecks";

            generateDeckSaveMessage = typeof(DeckBuilder2).GetMethod("generateDeckSaveMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            chatLogStyleinfo = typeof(ChatUI).GetField("chatMsgStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            scrollsBookinfo = typeof(DeckBuilder2).GetField("scrollBook", BindingFlags.Instance | BindingFlags.NonPublic);
            scrollsBookRect2info = typeof(DeckBuilder2).GetField("rectBook", BindingFlags.Instance | BindingFlags.NonPublic);
            scrollsBookRect1info = typeof(DeckBuilder2).GetField("rectLeft", BindingFlags.Instance | BindingFlags.NonPublic);
            dckcrtr = new Deckcreator();
            imp = new Importer();
            googleie = new GoogleImporterExporter();
            dcksrchui = new DecksearchUI(deckfolder,imp,googleie);
            
           
            this.lobbyskin = (GUISkin)Resources.Load("_GUISkins/Lobby");

            
            if (!Directory.Exists(deckfolder + Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(deckfolder + Path.DirectorySeparatorChar);
            }

            string[] aucfiles = Directory.GetFiles(this.deckfolder, "decks.txt");
            if (aucfiles.Contains(this.deckfolder + System.IO.Path.DirectorySeparatorChar + "decks.txt"))//File.Exists() was slower
            {
                //loadDecks();
            }
            else
            {
                System.IO.File.WriteAllText(this.deckfolder + System.IO.Path.DirectorySeparatorChar + "decks.txt", "");
            }

            try
            {
                App.Communicator.addListener(this);
            }
            catch { }
            this.systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            
		}

        

		public static string GetName ()
		{
			return "deckimporter";
		}

		public static int GetVersion ()
		{
			return 6;
		}


       
		//only return MethodDefinitions you obtained through the scrollsTypes object
		//safety first! surround with try/catch and return an empty array in case it fails
		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["DeckBuilder2"].Methods.GetMethod("handleMessage",new Type[]{typeof(Message)}),
                    scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)}),
                    scrollsTypes["DeckBuilder2"].Methods.GetMethod("OnGUI")[0],
                    scrollsTypes["DeckBuilder2"].Methods.GetMethod("Start")[0],
                    scrollsTypes["DeckBuilder2"].Methods.GetMethod("OnGUI_drawTopbarSubmenu")[0],
                    scrollsTypes["ChatUI"].Methods.GetMethod("Initiate")[0],
                    scrollsTypes["ButtonGroup"].Methods.GetMethod("render")[0],
                    scrollsTypes["ScrollBook"].Methods.GetMethod("scrollTo",new Type[]{typeof(float)}),
                    

             };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}


        public override bool WantsToReplace(InvocationInfo info)
        {
            if (this.buildmode && info.target is DeckBuilder2 && info.targetMethod.Equals("OnGUI_drawTopbarSubmenu")) return true;
            if (this.buildmode && this.dcksrchui.showdecksearchUI && info.target is ButtonGroup && info.targetMethod.Equals("render")) return true;
            if (this.buildmode && this.dcksrchui.showdecksearchUI && info.target is ScrollBook && info.targetMethod.Equals("scrollTo")) return true;
                                    
            return false;
        }

        public override void ReplaceMethod(InvocationInfo info, out object returnValue)
        {
            returnValue = null;
            if (this.buildmode && info.target is DeckBuilder2 && info.targetMethod.Equals("OnGUI_drawTopbarSubmenu"))  GUI.DrawTexture(App.LobbyMenu.getSubMenuRect(1f), ResourceManager.LoadTexture("chatUI/menu_bar_sub"));
                return;
            
        }



        public override void BeforeInvoke(InvocationInfo info)
        {
            /*if (this.dcksrchui.showdecksearchUI && this.buildmode && info.target is DeckBuilder2 && info.targetMethod.Equals("OnGUI") && !(info.target is Crafter))
            {
                this.dcksrchui.drawSearchUI(true);// because of stupid overlaying button bug in unity
            }*/

            return;

        }

       

        public override void AfterInvoke (InvocationInfo info, ref object returnValue)
        //public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {
            if (info.target is DeckBuilder2 && info.targetMethod.Equals("Start") && !(info.target is Crafter))
            {
                imp.setDeckbuilder(info.target as DeckBuilder2);
                this.db = info.target as DeckBuilder2;
                this.buildmode = false;
                this.dcksrchui.showdecksearchUI = false;
                //make scrollbook clickable
                (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setRect((Rect)this.scrollsBookRect1info.GetValue(info.target));
                (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setBoundingRect((Rect)this.scrollsBookRect2info.GetValue(info.target));
            }
            /*
            if (info.target is DeckBuilder2 && info.targetMethod.Equals("handleMessage") && this.buildmode)
            {
                if(info.arguments[0] is LibraryViewMessage)
                this.dckcrtr.sendCheatLibraryView(this.imp);
            }*/

            if (info.target is ChatUI && info.targetMethod.Equals("Initiate"))
            {
                this.chatlogstye = (GUIStyle)this.chatLogStyleinfo.GetValue(info.target);
                this.dcksrchui.setChatlogStyle(this.chatlogstye);
                this.dcksrchui.setrecto(this.chatlogstye);
            }


            if (info.target is DeckBuilder2 && info.targetMethod.Equals("OnGUI") && !(info.target is Crafter))
            {
                
                GUI.depth = 21;
                if (this.oldscreenx != Screen.width || this.oldscreeny != Screen.height)
                {
                    this.dcksrchui.setrecto(this.chatlogstye);
                    this.dcksrchui.setrecto(this.chatlogstye);
                    this.oldscreenx = Screen.width;
                    this.oldscreeny = Screen.height;
                }
                if (this.buildmode)
                {
                    if (LobbyMenu.drawButton(this.dcksrchui.recto.guildbutton, "Normal Mode", this.lobbyskin))
                    {
                        
                        (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setRect((Rect)this.scrollsBookRect1info.GetValue(info.target));
                        (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setBoundingRect((Rect)this.scrollsBookRect2info.GetValue(info.target));
            
                        string link = this.createLink();
                        (info.target as DeckBuilder2).clearTable();
                        this.buildmode = false;
                        this.dcksrchui.showdecksearchUI = false;
                        this.dckcrtr.sendOrginalLibraryView(this.imp);
                        PopupOk("impdeckbuildmode", link);
                    }
                }
                else
                {

                    /*if (this.invalidDeck && LobbyMenu.drawButton(this.dcksrchui.recto.repairbutton, "Repair", this.lobbyskin))
                    {
                        this.invalidDeck = false;
                        string link = this.createLinkFromDCM(this.invalidDeckMessage);
                        imp.importFromURL(link);

                    }*/

                    if (LobbyMenu.drawButton(this.dcksrchui.recto.guildbutton, "Build Mode", this.lobbyskin))
                    {
                        //make scrollbook clickable
                        this.showMissing = false;
                        (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setRect((Rect)this.scrollsBookRect1info.GetValue(info.target));
                        (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setBoundingRect((Rect)this.scrollsBookRect2info.GetValue(info.target));
            
                        string link = this.createLink();
                        (info.target as DeckBuilder2).clearTable();
                        this.buildmode = true;
                        this.dcksrchui.showdecksearchUI = false;
                        this.dckcrtr.sendCheatLibraryView(this.imp);
                        PopupOk("impdeckbuildmode", link);
                        
                    }
                }

                if (LobbyMenu.drawButton(new Rect((float)Screen.height * 0.04f + (float)Screen.height * 0.11f, (float)Screen.height * 0.935f, (float)Screen.height * 0.1f, (float)Screen.height * 0.035f), "Import", this.lobbyskin) && !this.dcksrchui.showdecksearchUI)
                {
                    this.showImportMenu = !this.showImportMenu;
                    copydeck = (DeckSaveMessage)this.generateDeckSaveMessage.Invoke((info.target as DeckBuilder2), new object[] { "copycatt" });
                    (info.target as DeckBuilder2).clearTable();// have to delete the actual deck, or it will be buggy
                    App.Popups.ShowTextInput(this, "", "", "impdeck", "Import deck", "Insert the link to your deck:", "Import");
                }

                if (LobbyMenu.drawButton(new Rect((float)Screen.height * 0.04f + (float)Screen.height * 0.11f * 2f, (float)Screen.height * 0.935f, (float)Screen.height * 0.1f, (float)Screen.height * 0.035f), "Export", this.lobbyskin) && !this.dcksrchui.showdecksearchUI)
                {
                    string link = this.createLink();
                    systemCopyBufferProperty.SetValue(null, link, null);
                    App.Popups.ShowOk(this, "Export Deck", "A link to your Deck was created...", "...and copied to your clipboard.", "OK");
                }

                if (this.buildmode)
                {
                    GUI.skin = this.buttonSkin;
                    GUIPositioner p = App.LobbyMenu.getSubMenuPositioner(1f, 5, 140f);
                    float xIndex = -1f;
                    float blubb = 0;
                    if (AspectRatio.now.isWider(AspectRatio._4_3) && AspectRatio.now.isNarrower(AspectRatio._16_9))
                    {
                        blubb = -0.45f;
                    }
                    Func<Rect> func = () => p.getButtonRect((xIndex += 1f) + blubb - 1f);

                    if (LobbyMenu.drawButton(func(), "Save Deck") && !this.dcksrchui.showdecksearchUI)
                    {
                        string link = this.createLink();
                        if (link != "")
                        {
                            App.Popups.ShowTextInput(this, "", "", "savedeck1", "Save deck", "Insert the name for your deck:", "Save");

                        }
                        else
                        {
                            App.Popups.ShowOk(this, "Empty Deck", "Your deck is empty", "You cant save empty decks!", "OK");
                        }

                    }
                    if (LobbyMenu.drawButton(func(), "Share Deck") && !this.dcksrchui.showdecksearchUI)
                    {
                        string link = this.createLink();
                        if (link != "")
                        {
                            if (this.numberScrollsOnBoard>=50)
                            {
                                App.Popups.ShowTextInput(this, "", "", "sharedeck1", "Share deck", "Insert the name for your deck:", "Next step");
                            }
                            else {
                                App.Popups.ShowOk(this, "Invalid Deck", "Your deck is invalid", "You cant share invalid decks!", "OK");
                            }
                            
                        }
                        else {
                            App.Popups.ShowOk(this, "Empty Deck", "Your deck is empty", "You cant share empty decks!", "OK");
                        }
                        
                    }
                    if (LobbyMenu.drawButton(func(), "Import Deck")&& googleie.workthreadready )
                    { // get data from google
                        this.dcksrchui.infodeck.link = "";
                        this.dcksrchui.loadList = false;
                        new Thread(new ThreadStart(this.googleie.workthread)).Start();
                        this.dcksrchui.showdecksearchUI = true;
                        this.dcksrchui.setrecto(this.chatlogstye);

                        // load own decks
                        this.dcksrchui.setOwnList();
                        //make scrollbook UNclickable
                        
                        (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setBoundingRect(new Rect(0,0,0,0));
                        (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setRect(new Rect(0, 0, 0, 0));
                        
                    }

                    if (LobbyMenu.drawButton(func(), "Clear Table") && !this.dcksrchui.showdecksearchUI)
                    {
                        (info.target as DeckBuilder2).clearTable();
                    }
                    if (this.dckcrtr.ownMissing)
                    {
                        if (showMissing)
                        {
                            if (LobbyMenu.drawButton(func(), "Show All") && !this.dcksrchui.showdecksearchUI)
                            {
                                this.dckcrtr.sendCheatLibraryView(this.imp);
                                this.showMissing = false;
                            }

                        }
                        else
                        {
                            if (LobbyMenu.drawButton(func(), "Show Mis") && !this.dcksrchui.showdecksearchUI)
                            {
                                this.dckcrtr.sendMissingLibraryView(this.imp);
                                this.showMissing = true;
                            }
                        }
                    }

                    if (this.dcksrchui.showdecksearchUI)
                    {
                        if (this.dcksrchui.loadList == false && this.googleie.workthreadready)
                        {
                            this.dckcrtr.sendOrginalLibraryView(this.imp);
                            dcksrchui.setGoogleList();
                            this.dckcrtr.sendCheatLibraryView(this.imp);
                        }
                        bool oldval =dcksrchui.showdecksearchUI;
                        dcksrchui.drawUI();

                        if (dcksrchui.importme != "")
                        {
                            this.dcksrchui.showdecksearchUI = false;
                            string link = "http://www.UltimateDeckImporter.com/?l=" + dcksrchui.importme;
                            (info.target as DeckBuilder2).clearTable();
                            PopupOk("impdeckbuildmode", link);
                            dcksrchui.importme = "";
                        }

                        if (dcksrchui.showdecksearchUI != oldval)
                        {
                            //make scrollbook clickable again
                            (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setInputEnabled(true);
                            (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setRect((Rect)this.scrollsBookRect1info.GetValue(info.target));
                            (this.scrollsBookinfo.GetValue(info.target) as ScrollBook).setBoundingRect((Rect)this.scrollsBookRect2info.GetValue(info.target));
                        }

                        

                        if(dcksrchui.showDeleteMenu)
                        {
                            dcksrchui.showDeleteMenu = false;
                            if (dcksrchui.viewmode == 0)
                            {
                                App.Popups.ShowTextInput(this, "", "", "deletedeck", "Delete deck", "If you want to delete your shared deck, please insert its name (" + this.dcksrchui.infodeck.deckname + "):", "Delete");
                            }
                            else
                            {
                                App.Popups.ShowOkCancel(this,"deletedeckprivate", "Delete deck", "You really want to delete your private deck: " + this.dcksrchui.infodeck.deckname, "Ok","Cancel");
                            }

                        }
                    }

                }


            }

            return;//return false;
        }


        private string createLinkFromDCM(DeckCardsMessage dcm)
        {
            string retu = "";

            List<Card> tableCards = dcm.cards.ToList();
            Dictionary<int, int> cardDic = new Dictionary<int, int>();
            foreach (Card dc in tableCards)
            {
                int type = dc.getType();
                if (cardDic.ContainsKey(type)) { cardDic[type] = cardDic[type] + 1; } else { cardDic[type] = 1; }
            }

            foreach (KeyValuePair<int, int> kvp in cardDic)
            {
                if (retu == "") { retu = kvp.Key + "," + kvp.Value; }
                else
                {
                    retu = retu + ":" + kvp.Key + "," + kvp.Value;
                }
            }
            if (retu == "") return "";
            retu = "http://www.UltimateDeckImporter.com/?l=" + retu;
            retu = retu.Replace(",3", "");
            numberScrollsOnBoard = tableCards.Count();
            return retu;
        }

        private string createLink()
        {
            string retu = "";
            List<DeckCard> tableCards = (List<DeckCard>)typeof(DeckBuilder2).GetField("tableCards", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this.db);
            Dictionary<int, int> cardDic = new Dictionary<int, int>();
            foreach(DeckCard dc in tableCards)
            {
                int type=dc.card.getCardInfo().getType();
                if (cardDic.ContainsKey(type)) { cardDic[type] = cardDic[type] + 1; } else { cardDic[type] = 1; }
            }
           
            foreach(KeyValuePair<int,int> kvp in cardDic)
            {
                if (retu == "") { retu = kvp.Key + "," + kvp.Value; }
                else
                {
                    retu = retu + ":" + kvp.Key + "," + kvp.Value;
                }
            }
            if (retu == "") return "";
            retu = "http://www.UltimateDeckImporter.com/?l=" +  retu;
            retu = retu.Replace(",3", "");
            numberScrollsOnBoard = tableCards.Count();
            return retu;
        }


        public void PopupCancel(string popupType)
        {
            if (popupType == "sharedeck2")
            {
                string link = this.createLink();
                link = link.Replace("http://www.UltimateDeckImporter.com/?l=", "");
                List<string> data = new List<string>();
                data.Add(App.MyProfile.ProfileInfo.name);
                data.Add(link);
                data.Add(this.choosenname);
                data.Add("");
                this.googleie.postDataToGoogleForm(data);
            }

            if (popupType == "impdeck")
            {
                DeckCardsMessage dcm = new DeckCardsMessage("");
                List<Card> cardes = new List<Card>();
                foreach (Card c in imp.allCards)
                {
                    if (this.copydeck.cards.Contains(c.id.ToString()))
                    {
                        cardes.Add(c);
                    }
                }
                dcm.cards = cardes.ToArray();
                dcm.metadata = this.copydeck.metadata;
                db.handleMessage(dcm);
            }
        }
        public void PopupOk(string popupType)
        {
            if (popupType == "deleteprivatedeckok")
            {
                this.dcksrchui.setOwnList();
            }
            if (popupType == "deletedeckprivate")
            {
                string text = System.IO.File.ReadAllText(this.deckfolder + System.IO.Path.DirectorySeparatorChar + "decks.txt");
                string[] data = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in data)
                {
                    if (s.Contains(this.dcksrchui.infodeck.deckname + ";"))
                    {
                        text = text.Replace(s + "\r\n", "");
                    }
                }
                System.IO.File.WriteAllText(this.deckfolder + System.IO.Path.DirectorySeparatorChar + "decks.txt", text);
                App.Popups.ShowOk(this, "deleteprivatedeckok", "Deck deleted", "Your deck was deleted.", "OK");
 
            }

            if (popupType == "savedeck2")
            {
                string text = System.IO.File.ReadAllText(this.deckfolder + System.IO.Path.DirectorySeparatorChar + "decks.txt");
                string[] data = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in data)
                {
                    if (s.Contains(this.choosenname + ";"))
                    {
                        text = text.Replace(s+"\r\n","");
                    }
                }
                string link = this.createLink();
                text = text + this.choosenname + ";" + link + "\r\n";
                System.IO.File.WriteAllText(this.deckfolder + System.IO.Path.DirectorySeparatorChar + "decks.txt", text);
                Console.WriteLine("saved own deck");
                App.Popups.ShowOk(this, "decksave", "Saved Deck", "Your deck was saved.", "OK");

            }

            if (popupType == "loadOldDeck")
            {
                DeckCardsMessage dcm = new DeckCardsMessage("");
                List<Card> cardes = new List<Card>();
                foreach (Card c in imp.allCards)
                {
                    if (this.copydeck.cards.Contains(c.id.ToString()))
                    {
                        cardes.Add(c);
                    }
                }
                dcm.cards = cardes.ToArray();
                dcm.metadata = this.copydeck.metadata;
                db.handleMessage(dcm);

            }
        }


        public void PopupOk(string popupType, string choice)
        {
            
            if (popupType == "savedeck1")
            {
                this.choosenname = choice.Replace(";","");
                string text = System.IO.File.ReadAllText(this.deckfolder + System.IO.Path.DirectorySeparatorChar + "decks.txt");
                if(text.Contains(this.choosenname+";"))
                {
                    App.Popups.ShowOkCancel(this, "savedeck2", "Override Deck", "the name is already in use, want to override existing deck?", "Ok", "Cancel");
                }
                else
                {
                string link = this.createLink();
                text = text + this.choosenname + ";" + link + "\r\n";
                System.IO.File.WriteAllText(this.deckfolder + System.IO.Path.DirectorySeparatorChar + "decks.txt", text);
                Console.WriteLine("saved own deck");
                App.Popups.ShowOk(this, "decksave", "Saved Deck", "Your deck was saved.", "OK");
                }
 
            }
            

            if (popupType == "sharedeck2")
            {

                string link = this.createLink();
                link=link.Replace("http://www.UltimateDeckImporter.com/?l=", "");
                
                List<string> data = new List<string>();
                data.Add(App.MyProfile.ProfileInfo.name);
                data.Add(link);
                data.Add(this.choosenname);
                data.Add(choice);
                this.googleie.postDataToGoogleForm(data);
            }

            if (popupType == "sharedeck1")
            {

                App.Popups.ShowTextInput(this, "", "", "sharedeck2", "Share deck", "Insert the description for your deck:", "Share");
                this.choosenname = choice;
            }

            if (popupType == "deletedeck" && choice == this.dcksrchui.infodeck.deckname)
            {
                string link = "DELETE " + this.dcksrchui.infodeck.link;
                List<string> data = new List<string>();
                data.Add(App.MyProfile.ProfileInfo.name);
                data.Add(link);
                data.Add(choice);
                data.Add(this.dcksrchui.infodeck.timestamp);
                this.googleie.postDataToGoogleForm(data);
                App.Popups.ShowOk(this, "DeletedDeck", "Deleted Deck", "You deleted your deck! \r\nPlease refresh the decklist in a few seconds.", "OK");
            }

            if (popupType == "impdeck")
            {
                try
                {
                    string retu = imp.importFromURL(choice);
                    if (retu == "notok")
                    {
                        App.Popups.ShowOk(this, "loadOldDeck", "An unknown Link appeared!", "either this is a deckbuilder the modcreator doesn't know\r\nor you are not able to copy links", "OK");
                    }

                    if (retu == "noadded")
                    {

                        App.Popups.ShowOk(this, "loadOldDeck", "An empty Deck appeared!", "You dont want to add empty decks, dont you?", "OK");
                    }

                    if (retu.StartsWith("You dont own: "))
                    {
                        RoomChatMessageMessage nrcmm = new RoomChatMessageMessage("[note]", retu);
                        nrcmm.from = "UltimateDeckImporter";
                        App.ArenaChat.handleMessage(nrcmm);
                        App.Popups.ShowOk(this, "errorMessage", "You dont own all required Scrolls!", "...buy 'em all!\r\nsee Chat for missing cards.", "OK");
                        
                    }
                }
                catch
                {
                    App.Popups.ShowOk(this, "loadOldDeck", "A wild Error appeared!", "please recheck the link and try it again", "OK");
                }
            }

            if (popupType == "impdeckbuildmode")
            {
                try
                {
                    string retu = imp.importFromURL(choice);

                    if (retu.StartsWith("You dont own: "))
                    {
                        RoomChatMessageMessage nrcmm = new RoomChatMessageMessage("[note]", retu);
                        nrcmm.from = "UltimateDeckImporter";
                        App.ArenaChat.handleMessage(nrcmm);
                        App.Popups.ShowOk(this, "errorMessage", "You dont own all required Scrolls!", "...buy 'em all!\r\nsee Chat for missing cards.", "OK");

                    }
                }
                catch
                {
                    App.Popups.ShowOk(this, "loadOldDeck", "A wild Error appeared!", "please recheck the link and try it again", "OK");
                }
            }


        }


        
	}
}
