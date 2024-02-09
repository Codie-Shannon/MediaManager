using System.Windows;
using MediaControlsLibrary;
using System.Windows.Controls;

namespace Media_Manager
{
    public class ToggleState
    {
        // Variables
        // ==============================================
        // ==============================================
        private static Grid gOverlay;
        private static Loading LoadingPanel;



        #region Methods
        // Toggle UI Element States
        // ==============================================
        // ==============================================
        public static void UIElements(UIElement[] elements, Visibility visibility)
        {
            //Loop through each element in the elements array
            foreach (UIElement element in elements)
            {
                //Show / Hide each element in the elements array
                element.Visibility = visibility;
            }
        }

        public static void UIElements(UIElement[] elements, bool IsEnabled)
        {
            //Loop through each element in the elements array
            foreach (UIElement element in elements)
            {
                //Show / Hide each element in the elements array
                element.IsEnabled = IsEnabled;
            }
        }

        public static void UIElement(UIElement element, Visibility visibility)
        {
            //Set Element's Visibility Value to visibility Value 
            element.Visibility = visibility;
        }

        public static void UIElement(UIElement element, bool IsEnabled)
        {
            //Set Element's IsEnabled Value to IsEnabled Boolean 
            element.IsEnabled = IsEnabled;
        }


        // Toggle Panel States
        // ==============================================
        // ==============================================
        public static void Panel(UIElement element, bool IsEnabled, bool isOverlayToggle = true)
        {
            //Check if IsEnabled is set to true
            if (IsEnabled == true)
            {
                //Show Element
                element.Visibility = Visibility.Visible;

                //Check if the Panel Requires an Overlay Toggle
                if (isOverlayToggle == true)
                {
                    //Show Main Window Overlay
                    Overlay(Visibility.Visible, 0.75f);
                }
            }
            else
            {
                //Hide Element
                element.Visibility = Visibility.Collapsed;

                //Check if the Panel Requires an Overlay Toggle
                if (isOverlayToggle == true)
                {
                    //Hide Main Window Overlay
                    Overlay(Visibility.Collapsed);
                }
            }
        }


        // Toggle Main Window Overlays
        // ==============================================
        // ==============================================
        public static void InitializeOverlay(Grid overlay, Loading loadingpanel)
        {
            //Set gOverlay
            gOverlay = overlay;

            //Set Loading Panel
            LoadingPanel = loadingpanel;
        }

        public static void Overlay(Visibility toggle, float opactiy = 0)
        {
            //Check if toggle is set to Visibility.Visible
            if (toggle == Visibility.Visible)
            {
                //Toggle Opacity of gOverlay
                gOverlay.Opacity = opactiy;
            }

            //Toggle Visibility of gOverlay
            gOverlay.Visibility = toggle;
        }

        public static void Loading(bool isLoading)
        {
            //Toggle IsLoading
            LoadingPanel.IsLoading = isLoading;
        }
        #endregion Methods
    }
}