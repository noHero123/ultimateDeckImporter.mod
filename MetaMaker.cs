using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace deckimporter.mod
{
    class MetaMaker
    {
        // copied the hole stuff from deckbuilder2 ( cant access alignTableCards(DeckBuilder2.TableAlignment alignment, DeckSorter sorter) )
        Rect rectTable = new Rect();
        Rect rectLeft = new Rect();
        Rect rectRight = new Rect();
        Rect subMenuRect = new Rect();
        Rect rectCard = new Rect();
        float CardSpacing = 0f;
        Gui.Gui3D gui3d;
        float currentTableCardZ = 0f;
        Vector3 _mainCameraPosition;

        public void setGui3d(DeckBuilder2 d)
        {
            this.gui3d = (Gui.Gui3D)typeof(DeckBuilder2).GetField("gui3d", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(d);
        }

        private float getTableXPos(float unit)
        {
            return this.rectTable.x + unit * this.rectTable.width;
        }

        private float getTableYPos(float unit)
        {
            return this.rectTable.y + unit * this.rectTable.height;
        }
        private float getUnitForTableXPos(float x)
        {
            return (x - this.rectTable.x) / this.rectTable.width;
        }

        private float getUnitForTableYPos(float y)
        {
            return (y - this.rectTable.y) / this.rectTable.height;
        }

        private Vector3 getPositionForStackedCard(Vector2 pos, int index, int numStacked, float zBase)
        {
            Vector3 pos2 = this.gui3d.getPosition(pos.x, pos.y + (float)(numStacked - index - 1) * this.CardSpacing).pos;
            return new Vector3(pos2.x, pos2.y, zBase - 0.1f * (float)index);
        }
        private float getNextZ()
        {
            return this.currentTableCardZ -= 0.05f;
        }

        private Vector3 worldToCamera(Vector3 w)
        {
            Vector3 mainCameraPosition = this._mainCameraPosition;
            return new Vector3(w.x + (float)(Screen.width / 2) - mainCameraPosition.x, (float)(Screen.height / 2) - w.y - mainCameraPosition.y, w.z);
        }

        public string getMetaData(List<Card> addedcards)
        {
            addedcards.Sort((x, y) => x.getName().CompareTo(y.getName()));

            this._mainCameraPosition = Camera.main.transform.position;
            this.currentTableCardZ = 800f;
            this.CardSpacing = 0.0075f * (float)Screen.height;
            string retu = "{'pos':'";
            int numTypes = DeckExtensionMethods.Collection.getNumTypes(addedcards);
            int num = (numTypes <= 5) ? 1 : 2;
            bool flag = numTypes % num != 0;
            int num2 = numTypes / num;
            int num3 = (!flag) ? num2 : (num2 + 1);
            int num4 = -1;
            int num5 = 0;

            float tnum = this.subMenuRect.y + this.subMenuRect.height;
            float tnum2 = (float)Screen.height * 0.98f - tnum;
            Vector3 vector = CardView.CardLocalScale();
            float tnum4 = vector.x / vector.z;
            float tnum5 = (float)Screen.height * 0.005f;
            float tnum6 = (float)Screen.height * 0.53f;
            float tnum7 = num4 * tnum6;
            float tnum8 = (float)Screen.width * 0.3f;
            if (tnum7 > tnum8)
            {
                tnum7 = tnum8;
                tnum6 = tnum8 / tnum4;
            }
            this.rectCard = new Rect(0f, 0f, tnum7, tnum6);
            this.rectCard.x = (float)Screen.width - this.rectCard.width - num5 * 1.7f;
            this.subMenuRect = App.LobbyMenu.getSubMenuRect(1f);

            float tnum9 = this.rectCard.x - tnum5;
            this.rectRight = new Rect(tnum9, tnum, (float)Screen.width - tnum9, tnum2);
            this.rectLeft = new Rect(0f, tnum, tnum9, tnum2 * 0.4f);
            float tnum10 = this.rectLeft.y + this.rectLeft.height;
            this.rectTable = GeomUtil.scaleCentered(new Rect(0f, tnum10, this.rectRight.x, (float)Screen.height * 0.95f - tnum10), 0.88f, 0.55f);

            for (int i = 0; i < addedcards.Count; i++)
            {
                if (i > 0) retu = retu + "|";
                int typeIndexForCardIndex = DeckExtensionMethods.Collection.getTypeIndexForCardIndex(addedcards, i);
                if (typeIndexForCardIndex != num4)
                {
                    num4 = typeIndexForCardIndex;
                    num5 = DeckExtensionMethods.Collection.getNumOf(addedcards, typeIndexForCardIndex);
                }
                int typeSubIndexForCardIndex = DeckExtensionMethods.Collection.getTypeSubIndexForCardIndex(addedcards, i);
                int num6 = (typeIndexForCardIndex < num3) ? 0 : 1;
                int num7 = (num6 != 0) ? num2 : num3;
                int num8 = (num6 != 0) ? (typeIndexForCardIndex - num3) : typeIndexForCardIndex;
                float num9 = (num7 <= 4) ? ((1f + (float)num8) / (float)(num7 + 1)) : ((float)num8 / ((float)num7 - 1f));
                float tableXPos = this.getTableXPos(num9);
                float tableYPos = this.getTableYPos(0.1f + 0.85f * (float)num6);
                Vector3 positionForStackedCard = this.getPositionForStackedCard(new Vector2(tableXPos, tableYPos), typeSubIndexForCardIndex, num5 - 3, this.getNextZ());
                Vector3 v = this.worldToCamera(positionForStackedCard);
                int xx = (int)(1000f * Mth.clamp(this.getUnitForTableXPos(v.x), 0f, 1f));
                int yy = (int)(1000f * Mth.clamp(1f - this.getUnitForTableYPos(v.y), 0f, 1f));
                retu = retu + addedcards[i].id + "," + xx + "," + yy;
            }
            retu = retu + "'}";
            return retu;
        }


    }
}
