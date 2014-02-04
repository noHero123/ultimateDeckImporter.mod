using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace deckimporter.mod
{
    class DecksearchUI
    {
        public string importme = "";

        private string deckNameFilter = "";
        private string sharerNameFilter = "";
        private bool showGrowth = true;
        private bool showEnergy = true;
        private bool showOrder = true;
        private bool showDecay = true;
        private bool showDecksICanBuild = false;
        Color dblack = new Color(1f, 1f, 1f, 0.5f);

        public bool showdecksearchUI = false;
        public bool loadList = false;
        public bool showDeleteMenu = false;
        private bool showfilter = true;
        public displayitems infodeck;
        public struct displayitems
        {
            public string timestamp;
            public string name;
            public string link;
            public string deckname;
            public bool containsGrowth;
            public bool containsEnergy;
            public bool containsOrder;
            public bool containsDecay;
            public int numberOfNeededScrolls;
            public string description;
            public string missingCards;
            public int numberOfScrollsInDeck;
            public string wholeDeckList;
        }

        Texture2D growthres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        Texture2D energyres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        Texture2D orderres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        Texture2D decayres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");

        public GUISkin cardListPopupSkin;
        public GUISkin cardListPopupGradientSkin;
        public GUISkin cardListPopupBigLabelSkin;
        public GUISkin cardListPopupLeftButtonSkin;
        private GUIStyle chatLogStyle;
        private float opacity;
        public Vector2 scrollPos, scrolll;
        List<displayitems> ahlist = new List<displayitems>();
        List<displayitems> fulllist = new List<displayitems>();
        public Rectomat recto;
        public DecksearchUI()
        {
            this.infodeck.link = "";
            this.recto = Rectomat.Instance;
            this.setskins((GUISkin)Resources.Load("_GUISkins/CardListPopup"), (GUISkin)Resources.Load("_GUISkins/CardListPopupGradient"), (GUISkin)Resources.Load("_GUISkins/CardListPopupBigLabel"), (GUISkin)Resources.Load("_GUISkins/CardListPopupLeftButton"));
            
        }
        public void setrecto(GUIStyle chatLogStyle)
        {
            recto.setupPositions(false, 6f/10f, chatLogStyle, this.cardListPopupSkin);
            //recto.setupsettingpositions(chatLogStyle, this.cardListPopupBigLabelSkin);
        }

       public void setskins(GUISkin cllps, GUISkin clpgs, GUISkin clpbls, GUISkin clplbs)
        {
            this.cardListPopupSkin = cllps;
            this.cardListPopupGradientSkin = clpgs;
            this.cardListPopupBigLabelSkin = clpbls;
            this.cardListPopupLeftButtonSkin = clplbs;

        }
       public void setChatlogStyle(GUIStyle c) 
       {
           this.chatLogStyle = c;
       }

       public void drawSearchUI()
       {
           GUI.skin = this.cardListPopupSkin;
           GUI.Box(recto.filtermenurect, string.Empty);
           // wts filter menue
           GUI.skin = this.cardListPopupBigLabelSkin;
           if (GUI.Button(recto.sbarlabelrect, "Info"))
           {
               this.showfilter = true;
           }
           Rect filterbuttonrect = recto.sbarlabelrect;
           filterbuttonrect.x = recto.sbarlabelrect.xMax + 6f;

           // dont need this at the moment
           
           if (GUI.Button(filterbuttonrect, "Filter"))
           {
               this.showfilter = false;
           }
            

           GUI.skin = this.cardListPopupSkin;
           // show infos of deck:
           if (this.showfilter && this.infodeck.link != "")
           {
               Rect showinforect = recto.filtermenurect;
               showinforect.yMin = recto.sbarlabelrect.yMax + 3f;
               showinforect.yMax = recto.sbclearrect.yMin - 3f;
               showinforect.x += 3f;
               showinforect.xMax -= 3f;

               string orgmsg = this.infodeck.deckname + " by " + this.infodeck.name + "\r\n\r\n";
               if(infodeck.description!="")orgmsg = orgmsg + "Description:\r\n" + infodeck.description + "\r\n\r\n";
               orgmsg = orgmsg + "Number of Scrolls in this deck: " + this.infodeck.numberOfScrollsInDeck + "\r\n";
               if (this.infodeck.numberOfNeededScrolls >= 1) orgmsg = orgmsg + "Number of missing Scrolls: "+this.infodeck.numberOfNeededScrolls+"\r\n";
               orgmsg = orgmsg + "\r\nDecklist:\r\n" + this.infodeck.wholeDeckList;
               if (this.infodeck.numberOfNeededScrolls >= 1) orgmsg = orgmsg + "\r\n\r\n" + "Missing Scrolls:\r\n" + infodeck.missingCards;

               GUI.skin = this.cardListPopupBigLabelSkin;
               GUI.skin.label.wordWrap = true;
               float msghigh = GUI.skin.label.CalcHeight(new GUIContent(orgmsg), showinforect.width - 30f);

               GUI.skin = this.cardListPopupSkin;
               scrolll = GUI.BeginScrollView(showinforect, scrolll, new Rect(0f, 0f, showinforect.width - 20f, msghigh));
               GUI.skin = this.cardListPopupBigLabelSkin;
               GUI.Label(new Rect(5f, 5f, showinforect.width - 30f, msghigh), orgmsg);
               GUI.skin.label.wordWrap = false;
               GUI.EndScrollView();
               if (this.infodeck.link != "" && GUI.Button(recto.sbclearrect, "Import"))
               {
                   this.importme = this.infodeck.link;
               }

           }

           if (!this.showfilter)
           { // draw filters


               GUI.skin = this.cardListPopupBigLabelSkin;
               GUI.Label(recto.sbDeckSearchLabelRect, "Deck:");
               GUI.skin = this.cardListPopupSkin;
               GUI.Box(recto.sbDeckSearchRect, string.Empty);
               string selfcopy = this.deckNameFilter;
               this.deckNameFilter = GUI.TextField(recto.sbDeckSearchRect, this.deckNameFilter, this.chatLogStyle);

               GUI.skin = this.cardListPopupBigLabelSkin;
               GUI.Label(recto.sbSharerSearchLabelRect, "Author:");
               GUI.skin = this.cardListPopupSkin;
               GUI.Box(recto.sbSharerSearchRect, string.Empty);
               string selfcopy2 = this.sharerNameFilter;
               this.sharerNameFilter = GUI.TextField(recto.sbSharerSearchRect, this.sharerNameFilter, this.chatLogStyle);


               GUI.contentColor = Color.white;
               GUI.color = Color.white;
               if (!this.showGrowth) { GUI.color = dblack; }
               bool growthclick = GUI.Button(recto.sbgrect, growthres);
               GUI.color = Color.white;
               if (!this.showOrder) { GUI.color = dblack; }
               bool orderclick = GUI.Button(recto.sborect, orderres);
               GUI.color = Color.white;
               if (!this.showEnergy) { GUI.color = dblack; }
               bool energyclick = GUI.Button(recto.sberect, energyres);
               GUI.color = Color.white;
               if (!this.showDecay) { GUI.color = dblack; }
               bool decayclick = GUI.Button(recto.sbdrect, decayres);
               GUI.color = Color.white;

               GUI.skin = this.cardListPopupBigLabelSkin;
               GUI.Label(recto.sbOnlyShowAffordableLabelRect, "only decks I can build");
               bool onlyclicked = GUI.Button(recto.sbOnlyShowAffordableRect, "");
               if (onlyclicked) this.showDecksICanBuild = !this.showDecksICanBuild;
               if (this.showDecksICanBuild)
               {
                   GUI.DrawTexture(recto.sbOnlyShowAffordableRect, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
               }
               else
               {
                   GUI.DrawTexture(recto.sbOnlyShowAffordableRect, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
               }

               if (growthclick) { this.showGrowth = !this.showGrowth; }
               if (orderclick) { this.showOrder = !this.showOrder; }
               if (energyclick) { this.showEnergy = !this.showEnergy; }
               if (decayclick) { this.showDecay = !this.showDecay; }

               if (growthclick || orderclick || energyclick || decayclick || selfcopy2 != sharerNameFilter || selfcopy != deckNameFilter || onlyclicked) this.updateFilters();

           }



       }

        public void drawUI()
        {
            bool clickableItems = true;
            GUI.depth = 22;
            this.opacity = 1f;

            this.drawSearchUI();
            // display DECKS##################################################################################
            // display DECKS##################################################################################
            // display DECKS##################################################################################
            // display DECKS##################################################################################
            // display DECKS##################################################################################

            GUI.skin = this.cardListPopupSkin;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
            GUI.Box(recto.position, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
            GUI.Box(recto.position2, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);


            this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * (float)this.ahlist.Count));
            int num = 0;
            GUI.skin = this.cardListPopupBigLabelSkin;
            this.ahlist.Reverse();
            foreach (displayitems current in this.ahlist)
            {

                GUI.skin = this.cardListPopupGradientSkin;
                //draw boxes
                Rect position7 = recto.position7offers(num);
                if (position7.yMax < this.scrollPos.y || position7.y > this.scrollPos.y + recto.position3.height)
                {
                    num++;
                    GUI.color = Color.white;
                }
                else
                {
                    if (clickableItems)
                    {
                        if (GUI.Button(position7, string.Empty))
                        {
                            this.infodeck = current;
                            this.showfilter = true;
                            //this.callback.ItemClicked(this, current);
                        }
                    }
                    else
                    {
                        GUI.Box(position7, string.Empty);
                    }
                    string name = current.deckname;

                    GUI.skin = this.cardListPopupBigLabelSkin;
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                    // draw text
                    Rect position8 = recto.position8offers(num);

                    //GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                    GUI.Label(position8, name);
                    GUI.skin = this.cardListPopupSkin;
                    // write PRICE
                    //GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    string gold = current.name;
                    GUI.skin = this.cardListPopupBigLabelSkin;
                    vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                    Rect position12 = new Rect(position8.xMax + 2f, (float)num * recto.fieldHeight, recto.labelsWidth/2f , recto.fieldHeight);
                    GUI.Label(position12, gold);
                    GUI.skin = this.cardListPopupLeftButtonSkin;
                    //draw resource
                    Texture texture = this.growthres;
                    Rect position13 = new Rect();
                    float resix = position12.xMax + 2f;
                    if (current.containsGrowth)
                    { 
                        texture = this.growthres;
                        position13 = new Rect(resix, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight);
                        GUI.DrawTexture(position13, texture);
                        resix = resix + 2f + recto.cardWidth;
                    }
                    if (current.containsOrder)
                    {
                        texture = this.orderres;
                        position13 = new Rect(resix, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight);
                        GUI.DrawTexture(position13, texture);
                        resix = resix + 2f + recto.cardWidth;
                    }
                    if (current.containsEnergy)
                    {
                        texture = this.energyres;
                        position13 = new Rect(resix, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight);
                        GUI.DrawTexture(position13, texture);
                        resix = resix + 2f + recto.cardWidth;
                    }
                    if (current.containsDecay)
                    {
                        texture = this.decayres;
                        position13 = new Rect(resix, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight);
                        GUI.DrawTexture(position13, texture);
                        resix = resix + 2f + recto.cardWidth;
                    }
                    // draw import button
                    if (current.numberOfNeededScrolls >= 1) 
                    {
                        GUI.skin.button.normal.textColor = new Color(1f, 0.5f, 0.5f, 1f);
                        GUI.skin.button.hover.textColor = new Color(1f, 0.5f, 0.5f, 1f);
                    }
                    if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth + recto.cardWidth, recto.fieldHeight), "Import"))
                    {
                        this.importme = current.link;
                    }
                    GUI.skin.button.normal.textColor = new Color(1f, 1f, 1f, 1f);
                    GUI.skin.button.hover.textColor = new Color(1f, 1f, 1f, 1f);
                    
                    num++;
                }
            }
            GUI.EndScrollView();
            this.ahlist.Reverse();
            GUI.color = Color.white;

            if (GUI.Button(recto.updatebuttonrect, "Close"))
            {
                this.showdecksearchUI = false;
            }

            if (this.infodeck.link!="" && this.infodeck.name== App.MyProfile.ProfileInfo.name && GUI.Button(recto.fillbuttonrect, "Delete"))
            {
                this.showDeleteMenu = true;
            }

        }

        public void updateFilters()
        {
            this.ahlist.Clear();
            foreach (displayitems di in this.fulllist)
            {
                if (di.name.ToLower().Contains(this.sharerNameFilter.ToLower()) && di.deckname.ToLower().Contains(this.deckNameFilter.ToLower()) && ( !(di.containsGrowth && !this.showGrowth) && !(di.containsOrder && !this.showOrder) && !(di.containsEnergy && !this.showEnergy) && !(di.containsDecay && !this.showDecay))) 
                {
                    if (this.showDecksICanBuild && di.numberOfNeededScrolls==0)
                    {
                        this.ahlist.Add(di);
                    }
                    if (!this.showDecksICanBuild)
                    { this.ahlist.Add(di); }
                        
                }
                //( di.containsGrowth && this.showGrowth || di.containsOrder && this.showOrder || di.containsEnergy && this.showEnergy || di.containsDecay && this.showDecay)
            
            }
        }

        public void setList(List<GoogleImporterExporter.sharedItem> input, Importer imp)
        {
            this.ahlist.Clear();
            foreach (GoogleImporterExporter.sharedItem element in input)
            {
                displayitems di = new displayitems();
                di.name = element.player;
                di.link = element.link;
                di.deckname = element.deckname;
                di.description = element.desc;
                di.containsGrowth = false;
                di.containsDecay = false;
                di.containsEnergy = false;
                di.containsOrder = false;
                di.timestamp = element.time;

                //calculate data
                Importer.Deckstatistics ids= imp.getdata(di.link);
                di.numberOfNeededScrolls = ids.anzunownedscrolls;

                     di.containsGrowth = ids.containsGrowth;
                     di.containsDecay = ids.containsDecay;
                     di.containsEnergy = ids.containsEnergy;
                     di.containsOrder = ids.containsOrder;
                     di.missingCards = ids.missingCards;
                     di.numberOfScrollsInDeck = ids.numberOfScrollsInDeck;
                     di.wholeDeckList = ids.deckList;

                this.ahlist.Add(di);
            }
            this.fulllist.Clear();
            this.fulllist.AddRange(this.ahlist);
            this.updateFilters();
            this.loadList = true;
        }

    }
}
