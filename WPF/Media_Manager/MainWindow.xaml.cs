using System.IO;
using System.Windows;
using MediaControlsLibrary;
using System.Threading.Tasks;
using Media_Manager.ViewModels;
using System.Collections.Generic;

namespace Media_Manager
{
    public partial class MainWindow : Window
    {
        #region Variables
        // Menu
        // =========================================
        // =========================================
        private NavigationViewItem selectedNavItem = null;


        // Pages
        // =========================================
        // =========================================
        private const string NavItemPrefix = "NavItem";
        private readonly Dictionary<string, object> Pages = new Dictionary<string, object>()
        {
            { "Movies" + NavItemPrefix, new MoviesViewModel() },
            { "TVShows" + NavItemPrefix, new TVShowsViewModel() },
            { "Videos" + NavItemPrefix, new VideosViewModel() },
            { "Pictures" + NavItemPrefix, new PicturesViewModel() },
            { "Music" + NavItemPrefix, new MusicViewModel() },
            { "Games" + NavItemPrefix, new GamesViewModel() },
            { "VideoPlayer" + NavItemPrefix, new VideoPlayerViewModel() },
            { "PictureGallery" + NavItemPrefix, new PictureGalleryViewModel() }
        };
        #endregion Variables



        // Constructor
        // =========================================
        // =========================================
        public MainWindow()
        {
            InitializeComponent();

            Setup();
        }



        // Setup
        // =========================================
        // =========================================
        private void Setup()
        {
            //Initialize Overlays
            ToggleState.InitializeOverlay(Overlay, LoadingPanel);

            //Set Default Navigation Menu Item
            SelectNavItem((NavigationViewItem)NavigationMenu.FindName("Movies" + NavItemPrefix));

            //Set Default View Model
            DataContext = new MoviesViewModel();

            //Initialize Application's Local Data Path
            string localdata = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}\\{Application.Current.MainWindow.GetType().Assembly.GetName().Name}";

            //Initialize Database
            Database.Initialize(localdata);

            //Initialize Save Locations
            Properties.Settings.Default.Movies = CreateSaveLocation(localdata + @"\Images\Movie Covers\");
            Properties.Settings.Default.TVShows = CreateSaveLocation(localdata + @"\Images\TV Show Covers\");
            Properties.Settings.Default.Seasons = CreateSaveLocation(localdata + @"\Images\Season Covers\");
            Properties.Settings.Default.Episodes = CreateSaveLocation(localdata + @"\Images\Episode Covers\");
            Properties.Settings.Default.Videos = CreateSaveLocation(localdata + @"\Images\Video Preview\");
            Properties.Settings.Default.Pictures = CreateSaveLocation(localdata + @"\Images\Image Preview\");
            Properties.Settings.Default.Music = CreateSaveLocation(localdata + @"\Images\Music Covers\");
            Properties.Settings.Default.Games = CreateSaveLocation(localdata + @"\Images\Game Covers\");
        }



        #region Event Handlers
        // Navigation
        // =========================================
        // =========================================
        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            //Set Selected Navigation Item
            SelectNavItem((NavigationViewItem)sender);

            //Load Associated View
            LoadView(((NavigationViewItem)sender).Name);
        }


        // Frame
        // =========================================
        // =========================================
        private void Frame_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Show Loading Panel
            ShowLoadingPanelAsync();
        }


        // Save Locations
        // =========================================
        // =========================================
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //Show Browse Locations Panel
            BrowseLocationsPanel.Visibility = Visibility.Visible;

            //Load Browse Locations
            LoadBrowseLocations();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            //Hide Browse Locations Panel
            BrowseLocationsPanel.Visibility = Visibility.Collapsed;

            //Save Browse Locations
            SaveBrowseLocations();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            //Hide Browse Locations Panel
            BrowseLocationsPanel.Visibility = Visibility.Collapsed;
        }
        #endregion Event Handlers



        #region Methods
        // Navigation
        // =========================================
        // =========================================
        private void SelectNavItem(NavigationViewItem item)
        {
            //Check if the selectedNavItem Object is not Null
            if (selectedNavItem != null)
            {
                //Set selectedNavItem's IsSelected Value to False
                selectedNavItem.IsSelected = false;
            }

            //Set selectedNavItem to Passed Item Variable
            selectedNavItem = item;

            //Set selectedNavItem's IsSelected Value to True
            selectedNavItem.IsSelected = true;
        }

        private void LoadView(string name)
        {
            //Get Page From Pages Dictionary
            Pages.TryGetValue(name, out object page);

            //Set Page as DataContext
            DataContext = page;
        }


        // Frame
        // =========================================
        // =========================================
        private async void ShowLoadingPanelAsync()
        {
            //Delay Task by 15 Milliseconds
            await Task.Delay(15);

            //Get Frame Name
            string name = string.IsNullOrEmpty($"{Frame.Content}") ? "empty" : Frame.Content.ToString().Replace("Media_Manager.Views.", "");

            //Check if Frame Content is Not Set to PictureGalleryView and VideoPlayerView
            if (name != "PictureGalleryView" && name != "VideoPlayerView")
            {
                //Show Loading Panel
                ToggleState.Loading(true);
            }
        }


        // Browse Locations
        // =========================================
        // =========================================
        private void LoadBrowseLocations()
        {
            //Load Browse Location Values
            odMovies.Content = Properties.Settings.Default.MovieBrowse;
            odTVShows.Content = Properties.Settings.Default.TVShowBrowse;
            odVideos.Content = Properties.Settings.Default.VideoBrowse;
            odPictures.Content = Properties.Settings.Default.PictureBrowse;
            odMusic.Content = Properties.Settings.Default.MusicBrowse;
            odSongCover.Content = Properties.Settings.Default.SongCoverBrowse;
            odGames.Content = Properties.Settings.Default.GameBrowse;
        }

        private void SaveBrowseLocations()
        {
            //Save Browse Location Values
            Properties.Settings.Default.MovieBrowse = $"{odMovies.Content}";
            Properties.Settings.Default.TVShowBrowse = $"{odTVShows.Content}";
            Properties.Settings.Default.VideoBrowse = $"{odVideos.Content}";
            Properties.Settings.Default.PictureBrowse = $"{odPictures.Content}";
            Properties.Settings.Default.MusicBrowse = $"{odMusic.Content}";
            Properties.Settings.Default.SongCoverBrowse = $"{odSongCover.Content}";
            Properties.Settings.Default.GameBrowse = $"{odGames.Content}";
            Properties.Settings.Default.Save();
        }


        // Save Locations
        // =========================================
        // =========================================
        private string CreateSaveLocation(string saveLocation)
        {
            if (!Directory.Exists(saveLocation))
            {
                Directory.CreateDirectory(saveLocation);
            }

            return saveLocation;
        }
        #endregion Methods
    }
}