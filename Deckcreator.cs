using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace deckimporter.mod
{
    class Deckcreator
    {
        private Message libraryview;
        private Message orginalLibraryView;
        private Message missingLibraryview;
        private MethodInfo dispatchMessages;
        public bool ownMissing = false;
        public bool sendCheated =false;

        List<int> alltypes = new List<int>();

        public Deckcreator() 
        {
            dispatchMessages = typeof(MiniCommunicator).GetMethod("_dispatchMessageToListeners", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void setOrginalLibraryView(Message msg)
        {
            this.orginalLibraryView = msg;
            LibraryViewMessage alllvm = this.libraryview as LibraryViewMessage;
            LibraryViewMessage lvm = msg as LibraryViewMessage;
           
            int id = 1000000;
            string libviw = "";
            
            List<int> owntypes = new List<int>();
            
            foreach (Card c in lvm.cards)
            {
                if (!owntypes.Contains(c.getType())) owntypes.Add(c.getType());

            }
            ownMissing = false;
            foreach (int i in alltypes)
            {

                if (!owntypes.Contains(i))
                {
                    ownMissing = true;
                    if (libviw != "") libviw = libviw + ",";
                    libviw = libviw + "{\"id\":" + id + ",\"typeId\":" +i + ",\"tradable\":false,\"isToken\":false,\"level\":0}";
                    id++;
                }

            }

            libviw = "{\"cards\":[" + libviw + "],\"profileId\":\"" + App.MyProfile.ProfileInfo.id + "\",\"msg\":\"LibraryView\"}";
            Console.WriteLine("#missing: "+libviw);
            this.missingLibraryview = MessageFactory.create(MessageFactory.getMessageName(libviw), libviw);
        }

        public void receiveCardlist(CardTypesMessage msg)
        {   // create a libraryview message
            int id = 1000000;
            string libviw = "" ;
            this.alltypes.Clear();
            foreach (CardType c in msg.cardTypes)
            {
                if (!alltypes.Contains(c.id)) alltypes.Add(c.id);

            }

            foreach (CardType ct in msg.cardTypes)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (libviw != "") libviw = libviw + ",";
                    libviw= libviw + "{\"id\":"+ id+",\"typeId\":" + ct.id + ",\"tradable\":false,\"isToken\":false,\"level\":0}";
                    id++;
                }

            }

            libviw= "{\"cards\":[" + libviw +"],\"profileId\":\""+ App.MyProfile.ProfileInfo.id + "\",\"msg\":\"LibraryView\"}";
            Console.WriteLine(libviw);
            libraryview = MessageFactory.create(MessageFactory.getMessageName(libviw),libviw);

        }

        public void sendCheatLibraryView(Importer imp)
        {
            sendCheated = true;
            dispatchMessages.Invoke(App.Communicator, new object[] { this.libraryview });
            imp.onLibraryViewReceived(this.libraryview as LibraryViewMessage);
            sendCheated = false;
        }

        public void sendOrginalLibraryView( Importer imp)
        {
            sendCheated = true;
            dispatchMessages.Invoke(App.Communicator, new object[] { this.orginalLibraryView });
            imp.onLibraryViewReceived(this.orginalLibraryView as LibraryViewMessage);
            sendCheated = false;
        }

        public void sendMissingLibraryView(Importer imp)
        {
            sendCheated = true;
            dispatchMessages.Invoke(App.Communicator, new object[] { this.missingLibraryview });
            imp.onLibraryViewReceived(this.missingLibraryview as LibraryViewMessage);
            sendCheated = false;
        }

    }
}
