﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace deckimporter.mod
{
    class Rectomat
    {

        Texture2D arrowdown = ResourceManager.LoadTexture("ChatUI/dropdown_arrow");

        public Rect ahbutton;
        public Rect genbutton;
        public Rect settingsbutton;

        private float buttonlength = 10;
        private float BOTTOM_MARGIN_EXTRA = (float)Screen.height * 0.047f;
        private Vector4 margins;
        public Rect screenRect, outerRect, innerBGRect, innerRect, buttonLeftRect, buttonRightRect, wtsbuttonrect;
        public Rect wtbbuttonrect, bothbuttonrect, ownbuttonrect, updatebuttonrect, fillbuttonrect;
        public float goldlength;
        //filterrects
        public Rect filtermenurect, sbarlabelrect, sbrect, sbrectbutton, sbgrect, sborect, sberect, sbdrect;
        public Rect sbclearrect, sbgeneratebutton, sbloadbutton, sbsavebutton, sbDeckSearchLabelRect, sbDeckSearchRect, sbSharerSearchLabelRect, sbSharerSearchRect;
        public Rect sbfilterrect, tbmessagescroll, sbOnlyShowAffordableLabelRect, sbOnlyShowAffordableRect;
        public Rect sbclrearpricesbutton, guildbutton;
        //settings
        public Rect settingRect, setsave, setreset, setload, setpreventspammlabel, setpreventspammrect, setpreventspammlabel2;
        public Rect setowncardsanzbox, setowncardsanzlabel, setsugrangebox, setsugrangelabel;
        public Rect setrowhightbox, setrowhightlabel, setrowhightlabel2, setwtslabel1, setwtslabel2, setwtsbutton1, setwtsbutton2, setwtsbox;
        public Rect setwtblabel1, setwtblabel2, setwtbbutton1, setwtbbutton2, setwtbbox;
        public Rect settakewtsgenlabel, settakewtsgenbutton, settakewtsgenlabel2, settakewtbgenlabel, settakewtbgenbutton, settakewtbgenlabel2;
        public Rect setwtsahlabel, setwtsahbutton, setwtsahlabel2, setwtsahlabel3, setwtsahlabel4, setwtbahlabel, setwtbahbutton, setwtbahlabel2, setwtbahlabel3, setwtbahlabel4, setwtsahbutton2, setwtbahbutton2;
        public Rect scrollpostbutton, scrollpostlabel1, scrollpostlabel2, repairbutton;
        public float fieldHeight;
        public float scrollBarSize = 20f;
        public float costIconSize, costIconWidth, costIconHeight, cardHeight, cardWidth, labelsWidth, labelX;

        public int maxCharsName, maxCharsRK;
        public Rect position, position2, position3;

        GUIStyle gUIStyle;
        GUISkin pulldownSkin;

        private static Rectomat instance;

        public static Rectomat Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Rectomat();
                }
                return instance;
            }
        }


        private Rectomat()
        {
            GUISkin gUISkin = (GUISkin)Resources.Load("_GUISkins/Lobby");
            gUIStyle = new GUIStyle(gUISkin.button);
            gUIStyle.normal.background = ResourceManager.LoadTexture("ChatUI/dropdown_arrow");
            GUIStyleState arg_13A_0 = gUIStyle.hover;
            Texture2D background = ResourceManager.LoadTexture("ChatUI/dropdown_arrow_mouseover");
            arg_13A_0.background = background;
            gUIStyle.active.background = background;

            GUISkin buttonSkin = (GUISkin)Resources.Load("_GUISkins/Lobby");
            this.pulldownSkin = ScriptableObject.CreateInstance<GUISkin>();
            this.pulldownSkin.button = new GUIStyle(buttonSkin.button);
            this.pulldownSkin.label = new GUIStyle(buttonSkin.label);
            this.pulldownSkin.button.normal.background = ResourceManager.LoadTexture("ChatUI/button_160a");
            GUIStyle arg_162_0 = this.pulldownSkin.button;
            TextAnchor alignment = TextAnchor.MiddleLeft;
            this.pulldownSkin.label.alignment = alignment;
            arg_162_0.alignment = alignment;
            int num8 = Screen.height / 36;
            this.pulldownSkin.label.fontSize = num8;
            this.pulldownSkin.button.fontSize = num8;

        }

        public void setupPositions(bool chatisshown, float rowscale, GUIStyle chatLogStyle, GUISkin cardListPopupSkin)
        {

            GUIPositioner subMenuPositioner = App.LobbyMenu.getSubMenuPositioner(1f, 8);
            this.guildbutton = new Rect(subMenuPositioner.getButtonRect(7f));
            guildbutton.x = guildbutton.x - 30;
            float diff = (float)Screen.width - this.guildbutton.xMax -6f;
            this.guildbutton.xMin += diff;
            this.guildbutton.xMax += diff;
            this.repairbutton = new Rect(subMenuPositioner.getButtonRect(6f));
            repairbutton.x = repairbutton.x - 30;
            diff = guildbutton.xMin - this.repairbutton.xMax - 6f;
            this.repairbutton.xMin += diff;
            this.repairbutton.xMax += diff;

            

            // set rects for menus
            this.screenRect = new Rect((float)Screen.width * 0.01f, (float)Screen.height * 0.18f, (float)Screen.width * 0.6f, (float)Screen.height * 0.57f);
            if (!chatisshown) { this.screenRect = new Rect((float)Screen.width * 0.01f, (float)Screen.height * 0.18f, (float)Screen.width * 0.6f, (float)Screen.height * 0.80f); }
            this.filtermenurect = new Rect(screenRect.xMax + (float)Screen.width * 0.01f, screenRect.y, (float)Screen.width * 0.37f, (float)Screen.height * 0.57f);

            this.margins = new Vector4(12f, 12f, 12f, 12f + this.BOTTOM_MARGIN_EXTRA);
            this.outerRect = this.screenRect;
            this.innerBGRect = new Rect(this.outerRect.x + this.margins.x, this.outerRect.y + this.margins.y, this.outerRect.width - (this.margins.x + this.margins.z), this.outerRect.height - (this.margins.y + this.margins.w));
            float num = 0.005f * (float)Screen.width;
            this.innerRect = new Rect(this.innerBGRect.x + num, this.innerBGRect.y + num, this.innerBGRect.width - 2f * num, this.innerBGRect.height - 2f * num);
            float num2 = this.BOTTOM_MARGIN_EXTRA - 0.01f * (float)Screen.height;
            this.buttonLeftRect = new Rect(this.innerRect.x + this.innerRect.width * 0.03f, this.innerBGRect.yMax + num2 * 0.28f, this.innerRect.width * 0.45f, num2);
            this.buttonRightRect = new Rect(this.innerRect.xMax - this.innerRect.width * 0.48f, this.innerBGRect.yMax + num2 * 0.28f, this.innerRect.width * 0.45f, num2);

            this.buttonlength = this.innerRect.width * 0.10f;
            this.wtsbuttonrect = new Rect(this.innerRect.x + buttonlength / 3.33f, this.innerBGRect.yMax + num2 * 0.28f, buttonlength, num2);
            this.wtbbuttonrect = new Rect(wtsbuttonrect.xMax + num, this.innerBGRect.yMax + num2 * 0.28f, buttonlength, num2);
            this.bothbuttonrect = new Rect(wtbbuttonrect.xMax + num, this.innerBGRect.yMax + num2 * 0.28f, buttonlength, num2);
            this.ownbuttonrect = new Rect(bothbuttonrect.xMax + num, this.innerBGRect.yMax + num2 * 0.28f, buttonlength, num2);
            this.updatebuttonrect = new Rect(this.innerRect.xMax - this.innerRect.width * 0.10f - this.innerRect.width * 0.03f, this.innerBGRect.yMax + num2 * 0.28f, buttonlength, num2);
            this.fillbuttonrect = new Rect(this.updatebuttonrect.x - this.innerRect.width * 0.10f - num, this.innerBGRect.yMax + num2 * 0.28f, this.innerRect.width * 0.10f, num2);

            num = (float)Screen.height / (float)Screen.width * 0.16f * rowscale;//0.16
            this.fieldHeight = (this.innerRect.width - this.scrollBarSize) / (1f / num + 1f);
            this.costIconSize = this.fieldHeight;
            this.costIconWidth = this.fieldHeight / 1.1f;
            this.costIconHeight = this.costIconWidth * 72f / 73f;
            this.cardHeight = this.fieldHeight * 0.72f;
            this.cardWidth = this.cardHeight * 100f / 75f;
            this.labelX = this.cardWidth * 1.45f;
            this.labelsWidth = this.innerRect.width - this.labelX - 2 * this.costIconSize - this.scrollBarSize - this.costIconWidth;
            this.labelsWidth = this.labelsWidth / 2.5f;
            this.maxCharsName = (int)(this.labelsWidth / 12f);
            this.maxCharsRK = (int)(this.labelsWidth / 10f);

            Vector2 vector = GUI.skin.label.CalcSize(new GUIContent("000000g"));
            goldlength = vector.x;


            float sbiconwidth = (filtermenurect.width - 2 * num2 - 6f * 4f) / 6f;
            float sbiconhight = costIconHeight;
            float chatheight = chatLogStyle.CalcHeight(new GUIContent("JScrollg"), 1000);
            float texthight = chatheight + 2;//(filtermenurect.height - 3 * sbiconhight-7*4-2*num2)/3;

            //rect for Info-button
            this.sbarlabelrect = new Rect(filtermenurect.x + num2, filtermenurect.y + num2, filtermenurect.width * 0.2f, texthight);
            this.sbrect = new Rect(sbarlabelrect.xMax + num2, sbarlabelrect.y, filtermenurect.xMax - sbarlabelrect.xMax - 2 * num2 - texthight, texthight);
            this.sbrectbutton = new Rect(sbrect.xMax, sbarlabelrect.y, texthight, texthight);

            // rect for Filter-button
            this.sbfilterrect = this.sbarlabelrect;
            this.sbfilterrect.x = this.sbarlabelrect.xMax + 6f;


            //rect for deckfilter button + box
            this.sbDeckSearchLabelRect = new Rect(sbarlabelrect.x + 4, sbarlabelrect.yMax + 4, filtermenurect.width * 0.2f, texthight);
            this.sbDeckSearchRect = new Rect(sbDeckSearchLabelRect.xMax + num2, sbDeckSearchLabelRect.y, filtermenurect.xMax - sbarlabelrect.xMax - 2 * num2 - texthight, texthight);

            //rect for sharerfilter button + box
            this.sbSharerSearchLabelRect = new Rect(sbarlabelrect.x + 4, sbDeckSearchLabelRect.yMax + 4, filtermenurect.width * 0.2f, texthight);
            this.sbSharerSearchRect = new Rect(sbSharerSearchLabelRect.xMax + num2, sbSharerSearchLabelRect.y, filtermenurect.xMax - sbarlabelrect.xMax - 2 * num2 - texthight, texthight);


            //rects for ressource buttons
            this.sbgrect = new Rect(sbarlabelrect.x, sbSharerSearchLabelRect.yMax + 4, sbiconwidth, sbiconhight);
            this.sborect = new Rect(sbgrect.xMax + 4, sbgrect.y, sbiconwidth, sbiconhight);
            this.sberect = new Rect(sborect.xMax + 4, sbgrect.y, sbiconwidth, sbiconhight);
            this.sbdrect = new Rect(sberect.xMax + 4, sbgrect.y, sbiconwidth, sbiconhight);



            this.sbclearrect = new Rect(filtermenurect.xMax - num2 - sbarlabelrect.width, filtermenurect.yMax - num2 - texthight, sbarlabelrect.width, texthight);
            this.sbclrearpricesbutton = new Rect(sbarlabelrect.x, sbclearrect.y, sbclearrect.x - sbarlabelrect.x - num2, texthight);
            this.sbgeneratebutton = new Rect(sbarlabelrect.x, sbclearrect.y - 4 - texthight, sbclearrect.x - sbarlabelrect.x - num2, texthight);
            this.sbloadbutton = new Rect(sbarlabelrect.x, sbgeneratebutton.y - 4 - texthight, (sbclearrect.x - sbarlabelrect.x - num2 - 4f) / 2f, texthight);
            this.sbsavebutton = new Rect(sbloadbutton.xMax + 4, sbgeneratebutton.y - 4 - texthight, (sbclearrect.x - sbarlabelrect.x - num2 - 4f) / 2f, texthight);


            this.sbOnlyShowAffordableRect = new Rect(sbarlabelrect.x, sbgrect.yMax + 4, texthight, texthight);
            this.sbOnlyShowAffordableLabelRect = new Rect(sbOnlyShowAffordableRect.xMax + 4, sbOnlyShowAffordableRect.y, this.labelsWidth, texthight);

            /*
            GUI.skin = cardListPopupSkin;
            float smalltexthight = GUI.skin.label.CalcHeight(new GUIContent("Jg"), 1000);
            this.sbnetworklabel = new Rect(filtermenurect.x + 4, filtermenurect.yMax - smalltexthight - 4, filtermenurect.width, smalltexthight);

            this.tradingbox = new Rect((float)Screen.width / 2f - (float)Screen.width * 0.15f, (float)Screen.height / 2f - (float)Screen.height * 0.15f, (float)Screen.width * 0.3f, (float)Screen.height * 0.3f);
            this.tradingbox = new Rect(innerRect);
            this.tradingbox.x = tradingbox.x + this.cardWidth;
            this.tradingbox.width = tradingbox.width - this.cardWidth - this.costIconWidth;

            this.tbok = new Rect(tradingbox.xMin + (float)Screen.height * 0.05f, tradingbox.yMax - (float)Screen.height * 0.05f, (float)Screen.width * 0.15f - 2 * (float)Screen.height * 0.05f, (float)Screen.height * 0.05f - 2f);
            this.tbcancel = new Rect(tradingbox.xMax - (float)Screen.width * 0.15f + (float)Screen.height * 0.05f, tradingbox.yMax - (float)Screen.height * 0.05f, (float)Screen.width * 0.15f - 2 * (float)Screen.height * 0.05f, (float)Screen.height * 0.05f - 2f);
            this.tbwhisper = new Rect(tbok.xMax + (float)Screen.height * 0.05f, tradingbox.yMax - (float)Screen.height * 0.05f, (float)Screen.width * 0.15f - 2 * (float)Screen.height * 0.05f, (float)Screen.height * 0.05f - 2f);
            this.tboffer = new Rect(tbwhisper.xMax + (float)Screen.height * 0.05f, tradingbox.yMax - (float)Screen.height * 0.05f, (float)Screen.width * 0.15f - 2 * (float)Screen.height * 0.05f, (float)Screen.height * 0.05f - 2f);

            this.tbmessage = new Rect(this.tradingbox.x, this.tradingbox.y, this.tradingbox.width, (this.tradingbox.height - (float)Screen.height * 0.05f) / 2f);
            this.tbmessagescroll = new Rect(this.tradingbox.x, this.tbmessage.yMax, this.tradingbox.width, (this.tradingbox.height - (float)Screen.height * 0.05f) / 2f);
            this.tbpriceinput = new Rect((this.tradingbox.x + this.tradingbox.xMax - goldlength) / 2f, this.tbmessage.yMax, goldlength, texthight);
            this.tbororand = new Rect((this.tradingbox.x + this.tradingbox.xMax - goldlength) / 2f, this.tbpriceinput.yMax, goldlength, texthight);
            this.tbcard = new Rect((this.tradingbox.x + this.tradingbox.xMax - 2 * this.cardWidth - 8f) / 2f, this.tbororand.yMax, 2 * this.cardWidth + 8f, 2 * this.cardHeight + 8f);
            */
            GUI.skin = (GUISkin)Resources.Load("_GUISkins/CardListPopupBigLabel");


            calcguirects();
        }

        public void calcguirects()
        {
            float offX = 0;
            position = new Rect(this.outerRect.x + offX, this.outerRect.y, this.outerRect.width, this.outerRect.height);
            position2 = new Rect(this.innerBGRect.x + offX, this.innerBGRect.y, this.innerBGRect.width, this.innerBGRect.height);
            position3 = new Rect(this.innerRect.x + offX, this.innerRect.y, this.innerRect.width, this.innerRect.height);

        }

        public Rect position7offers(int num) { return new Rect(0f, (float)num * this.fieldHeight, this.innerRect.width - this.scrollBarSize - this.cardWidth - this.costIconWidth - 12f, this.fieldHeight); }
        public Rect position8offers(int num) { return new Rect(4f, (float)num * this.fieldHeight, this.labelsWidth*1.5f, this.fieldHeight); }
        public Rect position9offers(int num) { return new Rect(4f, (float)num * this.fieldHeight - 3f + this.fieldHeight * 0.57f, this.labelsWidth, this.cardHeight); }
        public Rect position9(int num) { return new Rect(this.labelX, (float)num * this.fieldHeight - 3f + this.fieldHeight * 0.57f, this.labelsWidth, this.cardHeight); }
        public Rect restyperectOffers(int num) { return new Rect(4f + this.labelsWidth + (this.costIconSize - this.costIconWidth) / 2f - 5f, (float)num * this.fieldHeight + (this.fieldHeight - this.costIconHeight) / 2f, this.costIconWidth, this.costIconHeight); }
        public Rect restyperect(int num) { return new Rect(this.labelX + this.labelsWidth + (this.costIconSize - this.costIconWidth) / 2f - 5f, (float)num * this.fieldHeight + (this.fieldHeight - this.costIconHeight) / 2f, this.costIconWidth, this.costIconHeight); }
        public Rect position10(int num) { return new Rect(0f, (float)num * this.fieldHeight, this.cardWidth + 8f, this.fieldHeight); }
        public Rect position10offers(int num)
        {
            return new Rect(this.innerRect.width - this.scrollBarSize - this.cardWidth - this.costIconWidth - 12f + 2f, (float)num * this.fieldHeight, this.cardWidth + 8f, this.fieldHeight);
        }
        public Rect chancelbutton()
        {
            float num2 = this.BOTTOM_MARGIN_EXTRA - 0.01f * (float)Screen.height;
            return new Rect(this.innerRect.xMax - this.innerRect.width * 0.10f - this.innerRect.width * 0.03f, this.innerBGRect.yMax + num2 * 0.28f, buttonlength, num2);
        }

        public void setupsettingpositions(GUIStyle chatLogStyle, GUISkin cardListPopupBigLabelSkin)
        {
            // buttons in store:
            GUIPositioner subMenuPositioner = App.LobbyMenu.getSubMenuPositioner(1f, 5);
            ahbutton = new Rect(subMenuPositioner.getButtonRect(2f));
            genbutton = new Rect(subMenuPositioner.getButtonRect(3f));
            Rect setrecto = subMenuPositioner.getButtonRect(4f);
            setrecto.x = Screen.width - setrecto.width;// -subMenuPositioner.getButtonRect(0f).x;
            settingsbutton = new Rect(setrecto);
            // stuff in settingsmenue
            float num = 0.005f * (float)Screen.width;

            this.settingRect = new Rect((float)Screen.width * 0.01f, (float)Screen.height * 0.18f, (float)Screen.width * 0.98f, (float)Screen.height * 0.57f);
            float buttonleng = this.settingRect.width * 0.10f;
            float chatheight = chatLogStyle.CalcHeight(new GUIContent("JSllg"), 1000);
            float texthight = chatheight + 2;//(filtermenurect.height - 3 * sbiconhight-7*4-2*num2)/3;
            this.setreset = new Rect(settingRect.xMax - 4 - buttonleng, settingRect.yMax - 4 - texthight, buttonleng, texthight);
            this.setload = new Rect(setreset.x - 4 - buttonleng, settingRect.yMax - 4 - texthight, buttonleng, texthight);
            this.setsave = new Rect(setload.x - 4 - buttonleng, settingRect.yMax - 4 - texthight, buttonleng, texthight);
            GUI.skin = cardListPopupBigLabelSkin;
            float lenfactor = 1.0f;
            Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent("dont update Messages which are younger than:"));
            this.setpreventspammlabel = new Rect(settingRect.x + 4, settingRect.y + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("99999"));
            this.setpreventspammrect = new Rect(setpreventspammlabel.xMax + 4, setpreventspammlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("minutes"));
            this.setpreventspammlabel2 = new Rect(setpreventspammrect.xMax + 4, setpreventspammlabel.y, lenfactor * vector2.x, texthight);

            vector2 = GUI.skin.label.CalcSize(new GUIContent("show owned number of scrolls ahead cardname"));
            this.setowncardsanzbox = new Rect(setpreventspammlabel.x, setpreventspammlabel.yMax + 4, texthight, texthight);
            this.setowncardsanzlabel = new Rect(setowncardsanzbox.xMax + 4, setpreventspammlabel.yMax + 4, lenfactor * vector2.x, texthight);


            vector2 = GUI.skin.label.CalcSize(new GUIContent("show in WTS-AH the "));
            this.setwtsahlabel = new Rect(setowncardsanzbox.x, setowncardsanzbox.yMax + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" lower "));
            this.setwtsahbutton = new Rect(setwtsahlabel.xMax + 4, setwtsahlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" ScrollsGuide price"));
            this.setwtsahlabel2 = new Rect(setwtsahbutton.xMax + 4, setwtsahlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("and"));
            this.setwtsahlabel3 = new Rect(setwtsahbutton.xMax + 4, setwtsahlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" lower "));
            this.setwtsahbutton2 = new Rect(setwtsahlabel3.xMax + 4, setwtsahlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" ScrollsGuide prices"));
            this.setwtsahlabel4 = new Rect(setwtsahbutton2.xMax + 4, setwtsahlabel.y, lenfactor * vector2.x, texthight);



            vector2 = GUI.skin.label.CalcSize(new GUIContent("show in WTB-AH the "));
            this.setwtbahlabel = new Rect(setowncardsanzbox.x, setwtsahlabel.yMax + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" lower "));
            this.setwtbahbutton = new Rect(setwtsahlabel.xMax + 4, setwtbahlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" ScrollsGuide price"));
            this.setwtbahlabel2 = new Rect(setwtsahbutton.xMax + 4, setwtbahlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("and"));
            this.setwtbahlabel3 = new Rect(setwtsahbutton.xMax + 4, setwtbahlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" lower "));
            this.setwtbahbutton2 = new Rect(setwtbahlabel3.xMax + 4, setwtbahlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" ScrollsGuide prices"));
            this.setwtbahlabel4 = new Rect(setwtbahbutton2.xMax + 4, setwtbahlabel.y, vector2.x, texthight);

            vector2 = GUI.skin.label.CalcSize(new GUIContent("show ScrollsGuide price as range"));
            this.setsugrangebox = new Rect(setowncardsanzbox.x, setwtbahlabel.yMax + 4, texthight, texthight);
            this.setsugrangelabel = new Rect(setsugrangebox.xMax + 4, setsugrangebox.y, lenfactor * vector2.x, texthight);

            vector2 = GUI.skin.label.CalcSize(new GUIContent("scale row hight by factor"));
            this.setrowhightlabel = new Rect(setowncardsanzbox.x, setsugrangebox.yMax + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("99999"));
            this.setrowhightbox = new Rect(setrowhightlabel.xMax + 4, setsugrangebox.yMax + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("/10"));
            this.setrowhightlabel2 = new Rect(setrowhightbox.xMax + 4, setsugrangebox.yMax + 4, vector2.x, texthight);

            // take prices from

            vector2 = GUI.skin.label.CalcSize(new GUIContent("WTS-Generator takes "));
            this.settakewtsgenlabel = new Rect(setowncardsanzbox.x, setrowhightlabel.yMax + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" lower "));
            this.settakewtsgenbutton = new Rect(settakewtsgenlabel.xMax + 4, settakewtsgenlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("ScrollsGuide price"));
            this.settakewtsgenlabel2 = new Rect(settakewtsgenbutton.xMax + 4, settakewtsgenlabel.y, vector2.x, texthight);

            vector2 = GUI.skin.label.CalcSize(new GUIContent("WTB-Generator takes "));
            this.settakewtbgenlabel = new Rect(setowncardsanzbox.x, settakewtsgenlabel.yMax + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" lower "));
            this.settakewtbgenbutton = new Rect(settakewtbgenlabel.xMax + 4, settakewtbgenlabel.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("ScrollsGuide price"));
            this.settakewtbgenlabel2 = new Rect(settakewtbgenbutton.xMax + 4, settakewtbgenlabel.y, lenfactor * vector2.x, texthight);

            // rounding
            this.setwtsbox = new Rect(setowncardsanzbox.x, settakewtbgenlabel.yMax + 4, texthight, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("round ScrollsGuide prices in WTS-generator "));
            this.setwtslabel1 = new Rect(setwtsbox.xMax + 4, setwtsbox.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" down "));
            this.setwtsbutton1 = new Rect(setwtslabel1.xMax + 4, setwtsbox.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" to next "));
            this.setwtslabel2 = new Rect(setwtsbutton1.xMax + 4, setwtsbox.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" 50 "));
            this.setwtsbutton2 = new Rect(setwtslabel2.xMax + 4, setwtsbox.y, lenfactor * vector2.x, texthight);
            // rounding
            this.setwtbbox = new Rect(setowncardsanzbox.x, setwtsbox.yMax + 4, texthight, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("round ScrollsGuide prices in WTB-generator "));
            this.setwtblabel1 = new Rect(setwtbbox.xMax + 4, setwtbbox.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" down "));
            this.setwtbbutton1 = new Rect(setwtblabel1.xMax + 4, setwtbbox.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" to next "));
            this.setwtblabel2 = new Rect(setwtbbutton1.xMax + 4, setwtbbox.y, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" 50 "));
            this.setwtbbutton2 = new Rect(setwtblabel2.xMax + 4, setwtbbox.y, lenfactor * vector2.x, texthight);
            //scrollpost-day
            vector2 = GUI.skin.label.CalcSize(new GUIContent("Version of ScrollsGuide-Price: "));
            this.scrollpostlabel1 = new Rect(setowncardsanzbox.x, setwtbbox.yMax + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent("30-days"));
            this.scrollpostbutton = new Rect(scrollpostlabel1.xMax, setwtbbox.yMax + 4, lenfactor * vector2.x, texthight);
            vector2 = GUI.skin.label.CalcSize(new GUIContent(" (restart needed to take effect)"));
            this.scrollpostlabel2 = new Rect(scrollpostbutton.xMax, setwtbbox.yMax + 4, lenfactor * vector2.x, texthight);


        }



       


    }
}
