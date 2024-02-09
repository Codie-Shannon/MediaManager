using System;
using System.Windows;
using System.Windows.Input;
using Media_Manager.Models;
using System.Windows.Controls;
using Media_Manager.ViewModels;
using System.Collections.Generic;

namespace Media_Manager.Views
{
    public partial class PictureGalleryView : UserControl
    {
        #region Variables
        // Viewer Object
        // ==========================================
        // ==========================================
        private ViewerViewModel ViewerModel = null;


        // View Model Object
        // ==========================================
        // ==========================================
        private PictureGalleryViewModel Model = new PictureGalleryViewModel();


        // Zoom and Pan Variables
        // ==========================================
        // ==========================================
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;
        private Point origMouseDownPoint;
        private MouseButton mouseButtonDown;
        private Cursor originalCursor;
        #endregion Variables



        // Constructor
        // ==========================================
        // ==========================================
        public PictureGalleryView() { InitializeComponent(); }

        public PictureGalleryView(int[] sortindexes, Stack<Stack<Tuple<int, int, string, string>>> navigationunloadedstack, Stack<Stack<Tuple<int, int, string, string>>> navigationloadedstack, int activeFolder, bool isfavourite, List<Picture> pictures = null, Picture picture = null)
        {
            //Initialize Components
            InitializeComponent();

            //Setup Picture Gallery
            Model.Setup(sortindexes, navigationunloadedstack, navigationloadedstack, activeFolder, isfavourite, pictures, picture, imgPicture, bmpPicture, btnPrevious, btnNext);

            //Initialize ViewerModel
            ViewerModel = new ViewerViewModel(Bar, gBarMouseMove, NavigationMenu, gNavMouseMove);
        }



        #region Event Handlers
        // Video Player View
        // ========================================
        // ========================================
        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            //Set Viewer
            ViewerModel.SetViewer();
        }

        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            //Unset Viewer
            ViewerModel.UnsetViewer();
        }


        // Panes
        // ========================================
        // ========================================
        private void View_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Run View Resized Method
            Pane.ViewResized(gBarSize, Bar, gNavSize, NavigationMenu);
        }

        private void Pane_MouseMove(object sender, MouseEventArgs e)
        {
            //Call Mouse Hover Method
            ViewerModel.MouseHover();
        }

        private void Pane_MouseLeave(object sender, MouseEventArgs e)
        {
            //Call Mouse Exit Method
            ViewerModel.MouseExit();
        }


        // Skip Item
        // ==========================================
        // ==========================================
        private void btnSkipItem_Click(object sender, RoutedEventArgs e)
        {
            //Skip Item
            Model.SkipItem(Label.Name(sender, "btn"));
        }


        // Rotation
        // ==========================================
        // ==========================================
        private void btnRotate_Click(object sender, RoutedEventArgs e)
        {
            //Rotate Picture
            Model.Rotate(Label.Name(sender, "btnrotate"));
        }


        // Zoom and Pan
        // ==========================================
        // ==========================================
        private void imgPicture_CaptureMouse(object sender, MouseButtonEventArgs e)
        {
            //Set Focus to Content
            bmpPicture.Focus();
            Keyboard.Focus(bmpPicture);

            //Get Pressed Mouse Button
            mouseButtonDown = e.ChangedButton;

            //Get Mouse Down Point
            origMouseDownPoint = e.GetPosition(bmpPicture);

            //Check if the Left Mouse Button was Pressed
            if (mouseButtonDown == MouseButton.Left)
            {
                //Set Mouse Handling Mode to Pan
                mouseHandlingMode = MouseHandlingMode.Pan;

                //Capture Mouse
                imgPicture.CaptureMouse();

                //Set Original Cursor to Current Mouse Cursor
                originalCursor = Cursor;

                //Set Cursor to Hand
                Cursor = Cursors.Hand;

                //Set Event Handled to True
                e.Handled = true;
            }
        }

        private void imgPicture_ReleaseMouseCapture(object sender, MouseButtonEventArgs e)
        {
            //Check if a Mouse Handling Mode is Set
            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                //Release Mouse Capture
                imgPicture.ReleaseMouseCapture();

                //Set Cursor to Original Cursor
                Cursor = originalCursor;

                //Set Mouse Handling Mode to None
                mouseHandlingMode = MouseHandlingMode.None;

                //Set Event Handled to True
                e.Handled = true;
            }
        }

        private void imgPicture_Pan(object sender, MouseEventArgs e)
        {
            //Check if Mouse Handling Mode is Set to Pan
            if (mouseHandlingMode == MouseHandlingMode.Pan)
            {
                //Get Cursor Position Relative to the Content
                Point curContentMousePoint = e.GetPosition(bmpPicture);

                //Calculate Drag Offset
                Vector dragOffset = curContentMousePoint - origMouseDownPoint;

                //Set Content Offset
                imgPicture.ContentOffsetX -= dragOffset.X;
                imgPicture.ContentOffsetY -= dragOffset.Y;

                //Set Event Handled to True
                e.Handled = true;
            }
        }

        private void imgPicture_Zoom(object sender, MouseWheelEventArgs e)
        {
            //Set Event Handled to True
            e.Handled = true;

            //Check Mouse Scroll Type (Up - Down)
            if (e.Delta > 0)
            {
                //Zoom In
                imgPicture.ZoomAboutPoint(imgPicture.ContentScale + .1, e.GetPosition(bmpPicture));
            }
            else if (e.Delta < 0)
            {
                //Zoom Out
                imgPicture.ZoomAboutPoint(imgPicture.ContentScale - .1, e.GetPosition(bmpPicture));
            }
        }


        // Fullscreen
        // ==========================================
        // ==========================================
        private void btnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            //Change Window Mode
            ViewerModel.Change();
        }


        // Back
        // ==========================================
        // ==========================================
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //Run Back Method
            Model.Back();
        }
        #endregion Event Handlers
    }
}