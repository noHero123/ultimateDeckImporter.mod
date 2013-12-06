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
    public class deckimporter : BaseMod, ICommListener, IOkStringCancelCallback, IOkCallback
	{
        bool showJoinMessage = true;
        GUISkin lobbyskin;
        bool showImportMenu = false;
        Importer imp;
        DeckBuilder2 db;
        DeckSaveMessage copydeck;
        private MethodInfo generateDeckSaveMessage;

        public void handleMessage(Message msg)
        { // collect data for enchantments (or units who buff)

            if ( showJoinMessage && msg is RoomChatMessageMessage )
            {
                RoomChatMessageMessage rcmm = (RoomChatMessageMessage)msg;
                if (rcmm.text.StartsWith("You have joined"))
                {

                    RoomChatMessageMessage nrcmm = new RoomChatMessageMessage("[Note]", "DeckImporter was loadet");
                        nrcmm.from = "UltimateDeckImporter";
                        App.ArenaChat.handleMessage(nrcmm);
                        this.showJoinMessage = false;
                        

                }
            }

            if (msg is LibraryViewMessage)
            {
                imp.onLibraryViewReceived(msg as LibraryViewMessage);
            }


            return;
        }


        public void onConnect(OnConnectData ocd)
        {
            return; // don't care
        }


		//initialize everything here, Game is loaded at this point
        public deckimporter()
		{
            generateDeckSaveMessage = typeof(DeckBuilder2).GetMethod("generateDeckSaveMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            imp = new Importer();
            this.lobbyskin = (GUISkin)Resources.Load("_GUISkins/Lobby");
            try
            {
                App.Communicator.addListener(this);
            }
            catch { }
		}

        

		public static string GetName ()
		{
			return "deckimporter";
		}

		public static int GetVersion ()
		{
			return 1;
		}


       
		//only return MethodDefinitions you obtained through the scrollsTypes object
		//safety first! surround with try/catch and return an empty array in case it fails
		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["GlobalMessageHandler"].Methods.GetMethod("handleMessage",new Type[]{typeof(CardTypesMessage)}),
                    scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)}),
                    scrollsTypes["DeckBuilder2"].Methods.GetMethod("OnGUI")[0],
                    scrollsTypes["DeckBuilder2"].Methods.GetMethod("Start")[0],

             };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}


        public override bool WantsToReplace(InvocationInfo info)
        {
            return false;
        }

        public override void ReplaceMethod(InvocationInfo info, out object returnValue)
        {
            returnValue = null;
        
                return;
            
        }



        public override void BeforeInvoke(InvocationInfo info)
        {

            return;

        }

       

        public override void AfterInvoke (InvocationInfo info, ref object returnValue)
        //public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {
            if (info.target is GlobalMessageHandler && info.targetMethod.Equals("handleMessage") && info.arguments[0] is CardTypesMessage)
            {
                
            }
            if (info.target is DeckBuilder2 && info.targetMethod.Equals("Start"))
            {
                imp.setDeckbuilder(info.target as DeckBuilder2);
                this.db = info.target as DeckBuilder2;
            }
            if (info.target is DeckBuilder2 && info.targetMethod.Equals("OnGUI") )
            {
                GUI.depth = 21;
                GUIPositioner subMenuPositioner = App.LobbyMenu.getSubMenuPositioner(1f, 8);
                Rect guildbutton = new Rect(subMenuPositioner.getButtonRect(7f));
                guildbutton.x = guildbutton.x - 30;
                if (LobbyMenu.drawButton(guildbutton, "Import",this.lobbyskin))
                {
                    this.showImportMenu = !this.showImportMenu;
                    copydeck = (DeckSaveMessage)this.generateDeckSaveMessage.Invoke((info.target as DeckBuilder2), new object[] { "copycatt" });
                    (info.target as DeckBuilder2).clearTable();// have to delete the actual deck, or it will be buggy
                    App.Popups.ShowTextInput(this, "", "", "impdeck", "Import deck", "Insert the link to your deck:", "Import");
                }

            }

            return;//return false;
        }



        public void PopupCancel(string popupType)
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
        public void PopupOk(string popupType)
        {
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
                        RoomChatMessageMessage nrcmm = new RoomChatMessageMessage("[Note]", retu);
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
