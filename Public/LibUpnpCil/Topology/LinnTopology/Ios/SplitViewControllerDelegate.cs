using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace LinnTopology
{
    public class SplitViewControllerDelegate : UISplitViewControllerDelegate
    {
        public override void WillHideViewController(UISplitViewController aSplitViewController, UIViewController aViewController, UIBarButtonItem aBarButtonItem, UIPopoverController aPopoverController)
        {
            UINavigationController navController = aSplitViewController.ViewControllers[1] as UINavigationController;

            if(navController != null)
            {
                aBarButtonItem.Title = "Rooms";
                navController.NavigationBar.TopItem.SetLeftBarButtonItem(aBarButtonItem, false);

                SourceTableViewController source = navController.TopViewController as SourceTableViewController;

                if(source != null)
                {
                    source.SetPopOverController(aPopoverController);
                }
            }
        }

        public override void WillShowViewController(UISplitViewController aSplitViewController, UIViewController aViewController, UIBarButtonItem aButton)
        {
            UINavigationController navController = aSplitViewController.ViewControllers[1] as UINavigationController;

            if(navController != null)
            {
                navController.NavigationBar.TopItem.SetLeftBarButtonItem(null, false);

                SourceTableViewController source = navController.TopViewController as SourceTableViewController;

                if(source != null)
                {
                    source.SetPopOverController(null);
                }
            }
        }
    }
}