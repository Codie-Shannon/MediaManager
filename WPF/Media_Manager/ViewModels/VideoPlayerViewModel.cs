using System;
using System.Linq;
using System.Windows;
using Media_Manager.Views;
using Media_Manager.Models;
using MediaControlsLibrary;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;

namespace Media_Manager.ViewModels
{
    public class VideoPlayerViewModel
    {
        #region Variables
        // Main
        // =============================================
        // =============================================
        public MediaType Type;
        private int Index;
        public List<Video> Videos = new List<Video>();
        public List<Episode> Episodes = new List<Episode>();
        public Movie selectedMovie;
        public Video selectedVideo;
        public Episode selectedEpisode;
        public string File;


        // Elements
        // =============================================
        // =============================================
        private MediaElement meVideo;
        private Slider videoSlider;


        // Base Video Function
        // =============================================
        // =============================================
        public bool isPlaying = false;
        private int sliderPosition;
        private DispatcherTimer VideoDuration;
        

        // Skip
        // =============================================
        // =============================================
        public bool isVideoSkipped = false;


        // Return
        // =============================================
        // =============================================
        public int[] SortIndexes;
        public Stack<Stack<Tuple<int, int, string, string>>> NavigationUnloadedStack;
        public Stack<Stack<Tuple<int, int, string, string>>> NavigationLoadedStack;
        public int activeFolder;
        public bool isFavourite;
        #endregion Variables



        // Constructor
        // ========================================
        // ========================================
        public VideoPlayerViewModel()
        {
            //Initialize VideoDuration Dispatcher Timer
            VideoDuration = new DispatcherTimer();
            VideoDuration.Tick += new EventHandler(VideoDuration_Tick);
            VideoDuration.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }



        #region Setup
        // Methods
        // ========================================
        // ========================================
        public void Setup(MediaType type, int[] sortindexes, Stack<Stack<Tuple<int, int, string, string>>> navigationunloadedstack, Stack<Stack<Tuple<int, int, string, string>>> navigationloadedstack, int activefolder, bool isfavourite, Movie movie, List<Video> videos, Video video, List<Episode> episodes, Episode episode, MediaElement mevideo, NavigationViewItem btnmovies, NavigationViewItem btnvideos, NavigationViewItem btnepisodes, Slider slider, Button btnprevious, Button btnnext, Button btnreverse, Button btnforward)
        {
            //Set Elements
            Type = type;
            meVideo = mevideo;
            videoSlider = slider;

            //Set Sort Indexes (For Return)
            SortIndexes = sortindexes;

            //Set Navigation Stack (For Return)
            NavigationUnloadedStack = navigationunloadedstack;
            NavigationLoadedStack = navigationloadedstack;

            //Set Active Folder (For Return)
            activeFolder = activefolder;

            //Set isFavourite (For Return)
            isFavourite = isfavourite;

            //Check what media type is to be played
            if (type == MediaType.Movies)
            {
                //Setup Movie
                SetupMovie(movie, btnmovies, btnvideos, btnepisodes, btnprevious, btnnext);
            }
            else if (type == MediaType.Videos)
            {
                //Setup Video
                SetupVideo(videos, video, btnmovies, btnvideos, btnepisodes, btnreverse, btnforward, btnprevious, btnnext);
            }
            else if (type == MediaType.TVShows)
            {
                //Setup Episodes
                SetupEpisodes(episodes, episode, btnmovies, btnvideos, btnepisodes, btnreverse, btnforward, btnprevious, btnnext);
            }

            //Set Item
            SetItem();
        }


        // Extensions
        // ========================================
        // ========================================
        private void SetupMovie(Movie movie, NavigationViewItem btnmovies, NavigationViewItem btnvideos, NavigationViewItem btnepisodes, Button btnprevious, Button btnnext)
        {
            //Set File Path
            File = movie.FilePath;

            //Set Selected Movie
            selectedMovie = movie;

            //Select Movies Navigation Button
            btnmovies.IsSelected = true;

            //Disable Videos and Episodes Navigation Button
            ToggleState.UIElements(new UIElement[] { btnvideos, btnepisodes }, false);

            //Hide Video Controls
            btnprevious.Visibility = Visibility.Collapsed;
            btnnext.Visibility = Visibility.Collapsed;
        }

        private void SetupVideo(List<Video> videos, Video video, NavigationViewItem btnmovies, NavigationViewItem btnvideos, NavigationViewItem btnepisodes, Button btnreverse, Button btnforward, Button btnprevious, Button btnnext)
        {
            //Set File Path
            File = video.FilePath;

            //Set Videos List to videos
            Videos = videos;

            //Set Selected Video
            selectedVideo = video;

            //Set Index to Index of Selected Video
            Index = Videos.IndexOf(video);

            //Select Videos Navigation Button
            btnvideos.IsSelected = true;

            //Disable Movies and TV Shows Navigation Buttons
            ToggleState.UIElements(new UIElement[] { btnmovies, btnepisodes }, false);

            //Hide Movie Controls
            btnreverse.Visibility = Visibility.Collapsed;
            btnforward.Visibility = Visibility.Collapsed;

            //Check if the videos list count is equal to 1
            if (videos.Count == 1)
            {
                //Disable Previous / Next Buttons 
                ToggleState.UIElements(new UIElement[] { btnprevious, btnnext }, false);
            }
        }

        private void SetupEpisodes(List<Episode> episodes, Episode episode, NavigationViewItem btnmovies, NavigationViewItem btnvideos, NavigationViewItem btnepisodes, Button btnreverse, Button btnforward, Button btnprevious, Button btnnext)
        {
            //Set File Path
            File = episode.FilePath;

            //Set Episodes List to videos
            Episodes = episodes;

            //Set Selected Episode
            selectedEpisode = episode;

            //Set Index to Index of Selected Episode
            Index = Episodes.IndexOf(episode);

            //Select TV Shows Navigation Button
            btnepisodes.IsSelected = true;

            //Disable Movies and Videos Navigation Buttons
            ToggleState.UIElements(new UIElement[] { btnmovies, btnvideos }, false);

            //Hide Movie Controls
            btnreverse.Visibility = Visibility.Collapsed;
            btnforward.Visibility = Visibility.Collapsed;

            //Check if the episodes list count is equal to 1
            if (episodes.Count == 1)
            {
                //Disable Previous / Next Buttons
                ToggleState.UIElements(new UIElement[] { btnprevious, btnnext }, false);
            }
        }
        #endregion Setup



        // Event Handlers
        // ========================================
        // ========================================
        private void VideoDuration_Tick(object sender, EventArgs e)
        {
            //Update Video Position
            UpdateVideoPosition();
        }



        #region Methods
        // Play
        // ============================================
        // ============================================
        public void Play()
        {
            //Check if video is currently playing
            if (isPlaying)
            {
                //Pause Video
                meVideo.Pause();

                //Set isPlaying Boolean to False
                isPlaying = false;

                //Set Slider Status
                SetSliderStatus(SliderType.Stop);
            }
            else
            {
                //Play Video
                meVideo.Play();

                //Set isPlaying Boolean to True
                isPlaying = true;

                //Set Slider Status
                SetSliderStatus(SliderType.Start);
            }
        }


        // Position Skip
        // ============================================
        // ============================================
        public void PositionSkip(string name)
        {
            //Get Videos Current Position in Total Seconds
            int total = (int)meVideo.Position.TotalSeconds;

            //Check if Position Skip Name is Reverse
            //Else check if Position Skip Name is Forward
            if (name == "reverse")
            {
                //Minus value From Total Seconds
                total -= 10;
            }
            else if (name == "forward")
            {
                //Add value to Total Seconds
                total += 30;
            }

            //Set Videos Position
            meVideo.Position = new TimeSpan(0, 0, total);

            //Set Slider Position
            videoSlider.Value = meVideo.Position.TotalSeconds;
        }


        // Skip Item
        // ============================================
        // ============================================
        public void SkipItem(string name)
        {
            //Set Index
            if (name == "previous" && Index > 0) { Index--; }
            else if (name == "previous") { Index = Type == MediaType.Videos ? Videos.Count - 1 : Episodes.Count - 1; }
            else if (name == "next" && (Type == MediaType.Videos && Index < Videos.Count() - 1 || Type == MediaType.TVShows && Index < Episodes.Count() - 1)) { Index++; }
            else if (name == "next") { Index = 0; }

            //Validate Media Type
            if (Type == MediaType.Videos)
            {
                //Set Selected Video to Videos Element at the Position of the Index Variable 
                selectedVideo = Videos.ElementAt(Index);

                //Set File to Selected Video File Path
                File = selectedVideo.FilePath;
            }
            else if (Type == MediaType.TVShows)
            {
                //Set Selected Episode to Episodes Element at the Position of the Index Variable 
                selectedEpisode = Episodes.ElementAt(Index);

                //Set File to Selected Episode File Path
                File = selectedEpisode.FilePath;
            }

            //Set Item
            SetItem();
        }


        // Set Item
        // ============================================
        // ============================================
        private void SetItem()
        {
            //Check if Video is Currently Playing
            if (isPlaying == true)
            {
                //Pause Video
                Play();
            }

            //Set isVideoSkipped to True
            isVideoSkipped = true;

            //Set Video Source
            meVideo.Source = new Uri(File, UriKind.Absolute);

            //Start Slider Movement
            SetSliderStatus(SliderType.Start);

            //Play Video
            Play();
        }


        // Slider
        // ============================================
        // ============================================
        // Set Slider Status
        public void SetSliderStatus(SliderType type)
        {
            //Check Slider Type
            if (type == SliderType.Start)
            {
                //Start VideoDuration Dispatcher Timer
                VideoDuration.Start();
            }
            else if (type == SliderType.Stop)
            {
                //Stop VideoDuration Dispatcher Timer
                VideoDuration.Stop();
            }
        }

        // Update Video Position
        public void UpdateVideoPosition()
        {
            //Check if meVideo NaturalDuration has Timespan
            if (meVideo.NaturalDuration.HasTimeSpan)
            {
                //Set Maximum Value of videoSlider to Video Length
                videoSlider.Maximum = meVideo.NaturalDuration.TimeSpan.TotalSeconds;

                //Check if Video was Skipped
                //Else check if the videoSlider has been moved by the user
                if (isVideoSkipped == true)
                {
                    //Set Slider Value to 0
                    videoSlider.Value = 0;

                    //Set isVideoSkipped to False
                    isVideoSkipped = false;
                }
                else if ((int)videoSlider.Value != sliderPosition)
                {
                    //Set meVideo Position to videoSlider Value
                    meVideo.Position = new TimeSpan(0, 0, (int)videoSlider.Value);
                }
                else
                {
                    //Set Value of videoSlider to Video's Current Position
                    videoSlider.Value = meVideo.Position.TotalSeconds;
                }

                //Set sliderPosition to videoSlider Value
                sliderPosition = (int)videoSlider.Value;
            }
        }


        // Back
        // ============================================
        // ============================================
        public void Back()
        {
            //Get The Main Window
            Window mainWindow = Application.Current.MainWindow;

            //Check Media Type
            if(Type == MediaType.TVShows)
            {
                //Set Main Window's Data Context to New TV Show View
                mainWindow.DataContext = new TVShowsView(selectedEpisode.Id, SortIndexes, NavigationUnloadedStack, NavigationLoadedStack, activeFolder, isFavourite);
            }
            else if (Type == MediaType.Videos)
            {
                //Set Main Window's Data Context to New Video View
                mainWindow.DataContext = new VideosView(selectedVideo.Id, SortIndexes, NavigationUnloadedStack, NavigationLoadedStack, activeFolder, isFavourite);
            }
            else if (Type == MediaType.Movies)
            {
                //Set Main Window's Data Context to New Movies View
                mainWindow.DataContext = new MoviesView(selectedMovie.Id, SortIndexes, NavigationUnloadedStack, NavigationLoadedStack, activeFolder, isFavourite);
            }
        }
        #endregion Methods
    }
}