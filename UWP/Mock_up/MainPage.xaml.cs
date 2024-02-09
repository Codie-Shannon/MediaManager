using Mock_up.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Mock_up
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            //Go to Home Page
            MyFrame.Navigate(typeof(HomePage));

            //Select Home Page NavView Item
            NavView.SelectedItem = NavView.MenuItems.ElementAt(0);
        }

        #region Page Buttons
        private void btnHome_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Home Page
            MyFrame.Navigate(typeof(HomePage));
        }

        private void btnMovies_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Movies Page
            MyFrame.Navigate(typeof(MoviesPage));
        }

        private void btnTVShows_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to TV Shows Page
            MyFrame.Navigate(typeof(TVShowPage));
        }

        private void btnVideos_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Videos Page
            MyFrame.Navigate(typeof(VideosPage));
        }

        private void btnVideoPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Video Player Page
            MyFrame.Navigate(typeof(VideoPlayer));
        }

        private void btnPictures_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Pictures Page
            MyFrame.Navigate(typeof(PicturesPage));
        }

        private void btnPictureGallery_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Picture Gallery Page
            MyFrame.Navigate(typeof(PictureGallery));
        }

        private void btnMusic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Music Page
            MyFrame.Navigate(typeof(MusicPage));
        }

        private void btnGames_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Games Page
            MyFrame.Navigate(typeof(GamesPage));
        }

        private void btnGameEmulators_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Go to Game Emulators Page
            MyFrame.Navigate(typeof(GameEmulatorsPage));
        }
        #endregion Page Buttons
    }
}
