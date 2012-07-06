
using System;

using Linn;


namespace Linn.Toolkit
{
    public partial class HelperAutoUpdate
    {
        public HelperAutoUpdate(IHelper aHelper, IViewAutoUpdate aView, IInvoker aInvoker)
        {
            Initialise(aHelper, aView, aInvoker, this.UpdateStarted);
        }

        public HelperAutoUpdate(IHelper aHelper, IViewAutoUpdate aView, IInvoker aInvoker, EventHandler<EventArgs> aUpdateStarted)
        {
            Initialise(aHelper, aView, aInvoker, aUpdateStarted);
        }

        private void Initialise(IHelper aHelper, IViewAutoUpdate aView, IInvoker aInvoker, EventHandler<EventArgs> aUpdateStarted)
        {
            // create the auto updater
            // set it to look for stable releases to start with
            // option page will event what releases to look for once saved option has been parsed
            iAutoUpdate = new AutoUpdate(aHelper, AutoUpdate.kDefaultFeedLocation, 1000 * 60 * 60, AutoUpdate.EUpdateType.Stable, 1);

            // create the option page
            iOptionPageUpdates = new OptionPageUpdates(aHelper);
            iOptionPageUpdates.EventChanged += OptionPageUpdatesChanged;
            aHelper.AddOptionPage(iOptionPageUpdates);
            
            // create the controller
            iAutoUpdateController = new AutoUpdateController(aHelper, iAutoUpdate, iOptionPageUpdates, aView, aInvoker);
            iAutoUpdateController.EventUpdateStarted += aUpdateStarted;
        }

        public void Start()
        {
            iAutoUpdate.Start();
        }
        
        public void Dispose()
        {
            iAutoUpdate.Stop();
            iAutoUpdate.Dispose();
        }

        public void CheckForUpdates()
        {
            iAutoUpdateController.ManualCheck();
        }
        
        public OptionPageUpdates OptionPageUpdates
        {
            get { return iOptionPageUpdates; }
        }
        
        private AutoUpdate.EUpdateType OptionUpdateType
        {
            get
            {
                AutoUpdate.EUpdateType updateType = AutoUpdate.EUpdateType.Stable;

                if (iOptionPageUpdates.BetaVersions)
                {
                    updateType |= AutoUpdate.EUpdateType.Beta;
                }
                
                if (iOptionPageUpdates.DevelopmentVersions)
                {
                    updateType |= AutoUpdate.EUpdateType.Development;
                }

                if (iOptionPageUpdates.NightlyBuilds)
                {
                    updateType |= AutoUpdate.EUpdateType.Nightly;
                }

                return updateType;
            }
        }

        private void OptionPageUpdatesChanged(object sender, EventArgs e)
        {
            iAutoUpdate.UpdateTypes = OptionUpdateType;
        }

        // to be implemented in toolkit specific areas of code
        partial void UpdateStarted(object sender, EventArgs e);
        
        private OptionPageUpdates iOptionPageUpdates;
        private AutoUpdate iAutoUpdate;
        private AutoUpdateController iAutoUpdateController;
    }
}

