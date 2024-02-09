using System.Windows;
using MediaControlsLibrary;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Media_Manager
{
    public class Pane
    {
        // Variables
        // ======================================
        // ======================================
        private static double IPCWidth, NavHeight, VCHeight;
        public static bool isInformationPaneOpen = false, isViewerControlsOpen = false;



        #region View Resized
        // View Resized For Information Pane
        // ======================================
        // ======================================
        public static void ViewResized(Grid paneSize, Border pane, ItemsControl items)
        {
            //Get Information Column Size
            IPCWidth = paneSize.ActualWidth;

            //Set a New Margin for the Items ScrollViewer Element
            items.Margin = new Thickness(0, 0, -IPCWidth, 0);

            //Set Margin
            pane.Margin = new Thickness(IPCWidth, 0, 0, 0);
        }


        // View Resized For Viewer
        // ======================================
        // ======================================
        public static void ViewResized(Grid barSize, viewBar bar, Grid navSize, NavigationView navigation)
        {
            //Get Navigation Menu Height
            NavHeight = navSize.ActualHeight;

            //Set Margin of Navigation Menu
            navigation.Margin = new Thickness(0, 0, 0, NavHeight);

            //Get Viewer Controls Height
            VCHeight = barSize.ActualHeight;

            //Set Margin of Viewer Controls
            bar.Margin = new Thickness(0, VCHeight, 0, 0);
        }
        #endregion View Resized



        #region Toggle Pane
        // Toggle Pane
        // ======================================
        // ======================================
        // Toggle Information Pane
        public static void Toggle(PaneToggle toggle, Border pane, ItemsControl items = null)
        {
            //Initialize Storyboard Object
            Storyboard sb = null;

            //Set Pane Status
            sb = SetPaneStatus(toggle, ref isViewerControlsOpen);

            //Check if sb has been set
            if (sb != null)
            {
                //Start Pane Storyboard
                sb.Begin(pane);

                if (items != null)
                {
                    sb.Begin(items);
                }
            }
        }

        // Toggle Viewer Panes
        public static void Toggle(PaneToggle toggle, viewBar bar, NavigationView navigation)
        {
            //Initialize Storyboard Object
            Storyboard sb = null;

            //Set Pane Status
            sb = SetPaneStatus(toggle, ref isViewerControlsOpen);

            //Check if sb has been set
            if (sb != null)
            {
                //Start Pane Storyboard
                sb.Begin(navigation);
                sb.Begin(bar);
            }
        }


        // Extensions
        // ======================================
        // ======================================
        private static Storyboard SetPaneStatus(PaneToggle toggle, ref bool isPaneOpen)
        {
            //Initialize Variables
            string storyboard = string.Empty;
            
            //Check if isPaneOpen is set to false and if toggle is set to PaneTogle.Open
            //Else check if IsPaneOpen is set to true and if toggle is set to PaneToggle.Close
            if(!isPaneOpen && toggle == PaneToggle.Open)
            {
                //Get Open Pane Storyboard
                storyboard = "OpenPane";

                //Set Boolean to True
                isPaneOpen = true;
            }
            else if(isPaneOpen && toggle == PaneToggle.Close)
            {
                //Get Close Pane Storyboard
                storyboard = "ClosePane";

                //Set Boolean to False
                isPaneOpen = false;
            }

            //Return Storyboard
            return Application.Current.TryFindResource(storyboard) as Storyboard;
        }
        #endregion Toggle Pane
    }
}