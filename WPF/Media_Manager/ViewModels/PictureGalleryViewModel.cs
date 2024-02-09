using System;
using System.Linq;
using System.Windows;
using Media_Manager.Views;
using Media_Manager.Models;
using MediaControlsLibrary;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Media_Manager.ViewModels
{
    public class PictureGalleryViewModel
    {
        #region Variables
        // Main
        // ============================================
        // ============================================
        public List<Picture> Pictures = new List<Picture>();
        public Picture selectedPicture;
        private int Index = 0;


        // Elements
        // ============================================
        // ============================================
        Image bmpPicture;
        imgPicture imgPicture;


        // Rotation
        // ============================================
        // ============================================
        private List<Rotation> rotation = new List<Rotation>() { Rotation.Rotate0, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 };
        public int width, height;
        public bool isFirstRotate = true;
        public bool isRotated = false;


        // Return
        // =============================================
        // =============================================
        public int[] SortIndexes;
        public Stack<Stack<Tuple<int, int, string, string>>> NavigationUnloadedStack;
        public Stack<Stack<Tuple<int, int, string, string>>> NavigationLoadedStack;
        public int activeFolder;
        public bool isFavourite;
        #endregion Variables



        // Setup
        // ==========================================
        // ==========================================
        public void Setup(int[] sortindexes, Stack<Stack<Tuple<int, int, string, string>>> navigationunloadedstack, Stack<Stack<Tuple<int, int, string, string>>> navigationloadedstack, int activefolder, bool isfavourite, List<Picture> pictures, Picture picture, imgPicture imgpicture, Image bmppicture, viewIconButton btnprevious, viewIconButton btnnext)
        {
            //Set Elements
            imgPicture = imgpicture;
            bmpPicture = bmppicture;

            //Set Sort Indexes (For Return)
            SortIndexes = sortindexes;

            //Set Navigation Stack (For Return)
            NavigationUnloadedStack = navigationunloadedstack;
            NavigationLoadedStack = navigationloadedstack;

            //Set Active Folder (For Return)
            activeFolder = activefolder;

            //Set isFavourite (For Return)
            isFavourite = isfavourite;

            //Set Pictures List to pictures
            Pictures = pictures;

            //Set Selected Picture
            selectedPicture = picture;

            //Set Index to Index of Selected Picture
            Index = Pictures.IndexOf(picture);

            //Set Item
            SetItem();

            //Check if the pictures list count is equal to 1
            if (pictures.Count == 1)
            {
                //Disable Previous / Next Buttons
                ToggleState.UIElements(new UIElement[] { btnprevious, btnnext }, false);
            }
        }



        #region Methods
        // Skip Item
        // ============================================
        // ============================================
        public void SkipItem(string name)
        {
            //Set Index
            if(name == "previous" && Index > 0) { Index--; }
            else if (name == "previous") { Index = Pictures.Count - 1; }
            else if(name == "next" && Index < Pictures.Count() - 1) { Index++; }
            else if(name == "next") { Index = 0; }

            //Change Picture
            ChangeItem();
        }


        // Rotation
        // ============================================
        // ============================================
        public void Rotate(string name)
        {
            //Get Picture Source
            BitmapImage image = (BitmapImage)bmpPicture.Source;

            //Get Current Rotation
            Rotation setRotation = image.Rotation;

            //Check Rotation Type
            if (name == "right")
            {
                //Rotate Right
                setRotation = RotateRight(setRotation);
            }
            else if (name == "left")
            {
                //Rotate Left
                setRotation = RotateLeft(setRotation);
            }

            //Declare Bitmap for Rotation
            BitmapImage bmpRotation = new BitmapImage();

            //Begin Initialization of bmpRotation
            bmpRotation.BeginInit();

            //Set bmpRotation's Source to the Source of the Picture Element
            bmpRotation.UriSource = image.UriSource;

            //Rotate bmpRotation
            bmpRotation.Rotation = setRotation;

            //End Initialization of bmpRotation
            bmpRotation.EndInit();

            //Set the Picture Element's Source to the Rotated Bitmap
            bmpPicture.Source = bmpRotation;

            //Update Viewport Size to Fit Image
            imgPicture.UpdateViewportSize(new Size(bmpPicture.ActualWidth, bmpPicture.ActualHeight));
        }

        private Rotation RotateLeft(Rotation setrotation)
        {
            //Variables
            bool isNext = false;

            //Loop through Rotation List
            for (int i = 0; i < rotation.Count; i++)
            {
                //Check if current looped element is equal to the current picture's rotation
                //Else check if isNext is set to true
                //Else check if it is the last elment in the loop
                if (rotation[i] == setrotation)
                {
                    //Set isNext to True
                    isNext = true;
                }
                else if (isNext == true)
                {
                    //Return New Rotation
                    return rotation[i];
                }
            }

            //Return Default Rotation
            return rotation[0];
        }

        private Rotation RotateRight(Rotation setrotation)
        {
            //Variables
            bool isNext = false;

            //Loop through Rotation List
            for (int i = rotation.Count - 1; i >= 0; i--)
            {
                //Check if current looped element is equal to the current picture's rotation
                //Else check if isNext is set to true
                //Else check if it is the last elment in the loop
                if (rotation[i] == setrotation)
                {
                    //Set isNext to True
                    isNext = true;
                }
                else if (isNext == true)
                {
                    //Return New Rotation
                    return rotation[i];
                }
            }

            //Return Default Rotation
            return rotation[rotation.Count - 1];
        }


        // Change Item
        // ============================================
        // ============================================
        public void ChangeItem()
        {
            //Set Selected Picture to Pictures Element at the Position of Index
            selectedPicture = Pictures.ElementAt(Index);

            //Set Item
            SetItem();
        }


        // Set Item
        // ============================================
        // ============================================
        public void SetItem()
        {
            //Set Scale
            SetScale(selectedPicture.FilePath);

            //Set Picture
            bmpPicture.Source = new BitmapImage(new Uri(selectedPicture.FilePath, UriKind.Absolute));
        }

        public void SetScale(string filepath)
        {
            //Create Temporary Bitmap
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(filepath);

            //Calculate Ratios
            decimal wratio = (decimal)(Application.Current.MainWindow.ActualWidth / image.Width);
            decimal hratio = (decimal)(Application.Current.MainWindow.ActualHeight / image.Height);

            //Get Scale
            decimal ratio = wratio < hratio ? wratio : hratio;

            //Set Scale
            imgPicture.MinContentScale = (double)ratio / 3;
            imgPicture.ContentScale = (double)ratio / 3;
        }


        // Back
        // ============================================
        // ============================================
        public void Back()
        {
            //Get The Main Window
            Window mainWindow = Application.Current.MainWindow;

            //Set Main Window's Data Context to New Picture View
            mainWindow.DataContext = new PicturesView(selectedPicture.Id, SortIndexes, NavigationUnloadedStack, NavigationLoadedStack, activeFolder, isFavourite);
        }
        #endregion Methods
    }
}