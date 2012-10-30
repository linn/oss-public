
using System;
using System.Collections.Generic;


namespace Linn.Wizard
{
    // Class for wizard session to know when a new session is created
    public class Session : OpenHome.Xapp.Session, OpenHome.Xapp.ITrackerSender
    {
        public Session()
            : base()
        {
            // create a page instance for this session for the first page in the wizard
            PageInstance pageInst = PageManager.Instance.CreateFirstPageInstance(this);
            pageInst.Bind(this);

            // create a navigator for this session
            iNavigator = new PageNavigator(pageInst);

            iTracker = new OpenHome.Xapp.Tracker(ModelInstance.Instance.TrackerId, this);
            iTracking = true;
        }

        public bool Tracking
        {
            get
            {
                return iTracking;
            }
            set
            {
                iTracking = value;
                iTracker.SetTracking(value);
            }
        }

        public OpenHome.Xapp.Tracker Tracker { get { return iTracker; } }

        public PageNavigator Navigator
        {
            get { return iNavigator; }
        }

        public SessionModel Model
        {
            get
            {
                // temporary code to provide access to the SessionModel for older pages - if all pages
                // used the new architecture then this property would not need to exist and this code
                // would not be tied to the setup wizard specific code
                return ModelInstance.Instance.SessionModel(this);
            }
        }

        void OpenHome.Xapp.ITrackerSender.Send(string aName, OpenHome.Xapp.JsonObject aValue)
        {
            Send(aName, aValue);
        }

        private OpenHome.Xapp.Tracker iTracker;
        private PageNavigator iNavigator;
        private bool iTracking;
    }


    // Interface for a page model - the interaction between xapp page sends and the model data is handled by reflection
    public interface IPageModel
    {
        void Opened();
        void Reopened();
        void Completed();
    }


    // Base class for all Xapp pages in the wizard.
    public class Page : OpenHome.Xapp.Page<Session>
    {
        protected Page(PageDefinitions.Page aPageDefinition)
            : base(aPageDefinition.Name, aPageDefinition.Name)
        {
            iLock = new object();
            iSessionModels = new Dictionary<Session, IPageModel>();
            iPageDefinition = aPageDefinition;
        }

        public void SetSessionModel(Session aSession, IPageModel aModel)
        {
            lock (iLock)
            {
                iSessionModels[aSession] = aModel;
            }
        }

        private IPageModel GetSessionModel(Session aSession)
        {
            lock (iLock)
            {
                return iSessionModels[aSession];
            }
        }

        protected override void OnActivated(Session aSession)
        {
            // get the page model for this session
            IPageModel model = GetSessionModel(aSession);

            // enable analytics and send page view event if tracking
            aSession.Tracker.SetTracking(aSession.Tracking);
            aSession.Tracker.TrackPageView(Id);

            // update any ui widgets that reflect tracking state
            aSession.Send("DataCollectionEnabled", aSession.Tracking);

            // render widgets
            foreach (PageDefinitions.Widget widget in iPageDefinition.Widgets)
            {
                // get the value of the data from the page model
                Assert.Check(model != null);
                System.Reflection.PropertyInfo prop = model.GetType().GetProperty(widget.DataId);
                string data = prop.GetValue(model, null) as string;

                // create the json object
                OpenHome.Xapp.JsonObject json = new OpenHome.Xapp.JsonObject();

                // add the basic required properties
                json.Add("Id", new OpenHome.Xapp.JsonValueString(widget.Id));
                json.Add("DataId", new OpenHome.Xapp.JsonValueString(widget.DataId));
                json.Add("Value", new OpenHome.Xapp.JsonValueString(data));

                // add the optional allowed values
                if (widget.AllowedValues != null || widget.AllowedValuesSource != null)
                {
                    // get the list of string allowed values from either the widget XML or an IPageModel property
                    string[] allowedValuesStr = (widget.AllowedValues != null)
                                              ? widget.AllowedValues
                                              : model.GetType().GetProperty(widget.AllowedValuesSource).GetValue(model, null) as string[];

                    // create the json array of allowed values
                    OpenHome.Xapp.JsonArray<OpenHome.Xapp.JsonValueString> allowedValues = new OpenHome.Xapp.JsonArray<OpenHome.Xapp.JsonValueString>();

                    foreach (string value in allowedValuesStr)
                    {
                        allowedValues.Add(new OpenHome.Xapp.JsonValueString(value));
                    }

                    json.Add("AllowedValues", allowedValues);
                }

                // render the widget
                aSession.Send(widget.XappEvent, json);
            }
        }

        protected override void OnDeactivated(Session aSession)
        {
        }

        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            switch (aName)
            {
            case "PreviousPage":
                aSession.Navigator.PreviousPage(aSession);
                return;

            case "ReturnToPage":
                aSession.Navigator.PreviousPage(aSession, aValue);
                return;

            case "LinkClicked":
                System.Diagnostics.Process.Start(aValue);
                return;

            case "DataCollectionEnabled":
                aSession.Tracking = bool.Parse(aValue);
                return;
            }

            if (aName.StartsWith("setData"))
            {
                // get the page model for this session
                IPageModel model = GetSessionModel(aSession);
                Assert.Check(model != null);

                // get the data id to set
                string dataId = aName.Substring(7);

                System.Reflection.PropertyInfo prop = model.GetType().GetProperty(dataId);
                prop.SetValue(model, aValue, null);
            }

            if (aName.StartsWith("action"))
            {
                // get the page model for this session
                IPageModel model = GetSessionModel(aSession);

                // get the action id to process
                string actionId = aName.Substring(6);

                // process all actions with the given id
                foreach (PageDefinitions.Action action in iPageDefinition.Actions)
                {
                    if (action.Id == actionId)
                    {
                        // process <Action type="navigate"> actions
                        PageDefinitions.ActionNavigate actionNav = action as PageDefinitions.ActionNavigate;
                        if (actionNav != null)
                        {
                            if (actionNav.Source != null)
                            {
                                Assert.Check(model != null);

                                // get next page name from data model
                                System.Reflection.PropertyInfo prop = model.GetType().GetProperty(actionNav.Source);
                                string pageId = prop.GetValue(model, null) as string;
                                aSession.Navigator.NextPage(aSession, pageId);
                            }
                            else
                            {
                                // jump to specified page id
                                aSession.Navigator.NextPage(aSession, actionNav.PageId);
                            }
                        }

                        // process <Action type="data"> actions
                        PageDefinitions.ActionData actionData = action as PageDefinitions.ActionData;
                        if (actionData != null)
                        {
                            Assert.Check(model != null);

                            System.Reflection.PropertyInfo prop = model.GetType().GetProperty(actionData.DataId);
                            prop.SetValue(model, (actionData.DataValue != null) ? actionData.DataValue : aValue, null);
                        }

                        // process <Action type="method"> actions
                        PageDefinitions.ActionMethod actionMethod = action as PageDefinitions.ActionMethod;
                        if (actionMethod != null)
                        {
                            Assert.Check(model != null);

                            System.Reflection.MethodInfo method = model.GetType().GetMethod(actionMethod.MethodId);
                            method.Invoke(model, null);
                        }
                    }
                }
            }
        }

        private object iLock;
        private Dictionary<Session, IPageModel> iSessionModels;
        private PageDefinitions.Page iPageDefinition;
    }


    // Class definition for a page instance. This is essentially a wrapper for a Xapp page with some
    // particular instance of a data model
    public class PageInstance
    {
        public PageInstance(Page aPage, IPageModel aModel)
        {
            iPage = aPage;
            iModel = aModel;
        }

        public string PageId
        {
            get { return iPage.Id; }
        }

        public void Open(Session aSession)
        {
            // bind the IPageModel data to the xapp page
            iPage.SetSessionModel(aSession, iModel);

            // notify the IPageModel that the page is opened
            if (iModel != null)
            {
                iModel.Opened();
            }

            // navigate the session to this page
            aSession.Navigate(iPage.Id);
        }

        public void Reopen(Session aSession)
        {
            // bind the IPageModel data to the xapp page
            iPage.SetSessionModel(aSession, iModel);

            // notify the IPageModel that the page is reopened
            if (iModel != null)
            {
                iModel.Reopened();
            }

            // navigate the session to this page
            aSession.Navigate(iPage.Id);
        }

        public void Bind(Session aSession)
        {
            // bind the IPageModel data to the xapp page
            iPage.SetSessionModel(aSession, iModel);

            // notify the IPageModel that the page is opened
            if (iModel != null)
            {
                iModel.Opened();
            }
        }

        public void Completed()
        {
            if (iModel != null)
            {
                iModel.Completed();
            }
        }

        private Page iPage;
        private IPageModel iModel;
    }


    // Class to manage creation of xapp pages and page instances
    public class PageManager
    {
        // static methods for singleton access
        public static void Create(PageDefinitions aPageDefinitions, PageControl aPageControl)
        {
            Assert.Check(iInstance == null);
            iInstance = new PageManager(aPageDefinitions, aPageControl);
        }

        public static PageManager Instance
        {
            get
            {
                Assert.Check(iInstance != null);
                return iInstance;
            }
        }

        // instance methods
        public Entry[] Entries
        {
            get { return iEntries.ToArray(); }
        }

        public PageInstance CreatePageInstance(string aPageId, Session aSession)
        {
            foreach (Entry entry in iEntries)
            {
                if (entry.Page.Id == aPageId)
                {
                    return CreatePageInstance(entry, aSession);
                }
            }

            Assert.Check(false);
            return null;
        }

        public PageInstance CreateFirstPageInstance(Session aSession)
        {
            return CreatePageInstance(iEntries[0], aSession);
        }

        private PageManager(PageDefinitions aPageDefinitions, PageControl aPageControl)
        {
            iEntries = new List<Entry>();

            foreach (PageDefinitions.Page pageDef in aPageDefinitions.Pages)
            {
                Type pageType = Type.GetType(pageDef.Type, true);
                Page page = Activator.CreateInstance(pageType, aPageControl, pageDef) as Page;
                Assert.Check(page != null);

                iEntries.Add(new Entry(pageDef, page));
            }
        }

        private PageInstance CreatePageInstance(Entry aEntry, Session aSession)
        {
            IPageModel pageModel = null;

            if (!string.IsNullOrEmpty(aEntry.PageDefinition.Model))
            {
                Type pageModelType = Type.GetType(aEntry.PageDefinition.Model, true);
                pageModel = Activator.CreateInstance(pageModelType, aSession) as IPageModel;
                Assert.Check(pageModel != null);
            }

            return new PageInstance(aEntry.Page, pageModel);
        }

        public class Entry
        {
            public Entry(PageDefinitions.Page aPageDefinition, Page aPage)
            {
                iPageDefinition = aPageDefinition;
                iPage = aPage;
            }

            public PageDefinitions.Page PageDefinition
            {
                get { return iPageDefinition; }
            }

            public Page Page
            {
                get { return iPage; }
            }

            private PageDefinitions.Page iPageDefinition;
            private Page iPage;
        }

        private static PageManager iInstance;
        private List<Entry> iEntries;
    }


    // Class to handle page navigation through the wizard
    public class PageNavigator
    {
        public PageNavigator(PageInstance aFirstPage)
        {
            iPageStack = new List<PageInstance>();
            iPageStack.Add(aFirstPage);
        }

        public void NextPage(Session aSession, string aPageId)
        {
            // the user has moved on to the next wizard page so notify the current page that it has been completed
            iPageStack[iPageStack.Count - 1].Completed();

            // create a new instance of the specified page
            PageInstance pageInst = PageManager.Instance.CreatePageInstance(aPageId, aSession);

            // add the new page instance to the stack
            iPageStack.Add(pageInst);

            // open this new page instance for this session
            pageInst.Open(aSession);
        }

        public void NextPageNoSave(Session aSession, string aPageId)
        {
            // create a new instance of the specified page
            PageInstance pageInst = PageManager.Instance.CreatePageInstance(aPageId, aSession);

            // open this new page instance for this session
            pageInst.Open(aSession);
        }

        public void PreviousPage(Session aSession)
        {
            Assert.Check(iPageStack.Count > 1);

            // remove the last page in the stack
            PageInstance pageInst = iPageStack[iPageStack.Count - 1];
            iPageStack.Remove(pageInst);

            // open the previous page instance for this session
            pageInst = iPageStack[iPageStack.Count - 1];
            pageInst.Reopen(aSession);
        }

        public void PreviousPage(Session aSession, string aPageId)
        {
            Assert.Check(iPageStack.Count > 1);

            while (iPageStack.Count > 0)
            {
                // get page at top of the stack
                PageInstance pageInst = iPageStack[iPageStack.Count - 1];

                if (pageInst.PageId == aPageId)
                {
                    // open this page instance for this session
                    pageInst.Reopen(aSession);
                    return;
                }
                else
                {
                    // remove this page
                    iPageStack.Remove(pageInst);
                }
            }

            // this means aPageId did not exist in the page stack
            Assert.Check(false);
        }

        public string PreviousPageId
        {
            get
            {
                Assert.Check(iPageStack.Count > 1);

                return iPageStack[iPageStack.Count - 2].PageId;
            }
        }

        private List<PageInstance> iPageStack;
    }
}

