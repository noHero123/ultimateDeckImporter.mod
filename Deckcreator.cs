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
        private MethodInfo dispatchMessages;

        public Deckcreator() 
        {
            dispatchMessages = typeof(MiniCommunicator).GetMethod("_dispatchMessageToListeners", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void setOrginalLibraryView(Message msg)
        {
            this.orginalLibraryView = msg;
        }

        public void receiveCardlist(CardTypesMessage msg)
        {   // create a libraryview message
            int id = 1000000;
            string libviw = "" ;
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
            dispatchMessages.Invoke(App.Communicator, new object[] { this.libraryview });
            imp.onLibraryViewReceived(this.libraryview as LibraryViewMessage);
        }

        public void sendOrginalLibraryView( Importer imp)
        {
            dispatchMessages.Invoke(App.Communicator, new object[] { this.orginalLibraryView });
            imp.onLibraryViewReceived(this.orginalLibraryView as LibraryViewMessage);
        }

    }
}
