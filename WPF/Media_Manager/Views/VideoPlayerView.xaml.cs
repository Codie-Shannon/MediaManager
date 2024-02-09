using System;
using System.Windows;
using Media_Manager.Models;
using System.Windows.Input;
using System.Windows.Controls;
using Media_Manager.ViewModels;
using System.Collections.Generic;

namespace Media_Manager.Views
{
    public partial class VideoPlayerView : UserControl
    {
        // Variables
        // ========================================
        // ========================================
        // Viewer Object
        private ViewerViewModel ViewerModel = null;

        // View Model Object
        private VideoPlayerViewModel Model = new VideoPlayerViewModel();



        // Constructors
        // ========================================
        // ========================================
        public VideoPlayerView() { InitializeComponent(); }

        public VideoPlayerView(MediaType type, int[] sortindexes, Stack<Stack<Tuple<int, int, string, string>>> navigationunloadedstack, Stack<Stack<Tuple<int, int, string, string>>> navigationloadedstack, int activeFolder, bool isfavourite, Movie movie, List<Video> videos = null, Video video = null, List<Episode> episodes = null, Episode episode = null)
        {
            //Initialize Components
            InitializeComponent();

            //Run Setup Method
            Model.Setup(type, sortindexes, navigationunloadedstack, navigationloadedstack, activeFolder, isfavourite, movie, videos, video, episodes, episode, meVideo, btnMovies, btnVideos, btnTVShows, videoSlider, btnPrevious, btnNext, btnReverse, btnForward);

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
            //Stop Slider Movement
            Model.SetSliderStatus(SliderType.Stop);

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


        // Video Player
        // ========================================
        // ========================================
        private void meVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            //Start Slider Movement
            Model.SetSliderStatus(SliderType.Start);
        }

        private void meVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            //Stop Slider Movement
            Model.SetSliderStatus(SliderType.Stop);
        }


        // Slider
        // ========================================
        // ========================================
        private void videoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Check if Video is not Playing
            if (!Model.isPlaying)
            {
                //Update Video Position
                Model.UpdateVideoPosition();
            }
        }


        // Play
        // ========================================
        // ========================================
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //Play Video
            Model.Play();
        }


        // Position Skip
        // ========================================
        // ========================================
        private void btnPositionSkip_Click(object sender, RoutedEventArgs e)
        {
            //Skip Video
            Model.PositionSkip(Label.Name(sender, "btn"));
        }


        // Skip Item
        // ========================================
        // ========================================
        private void btnSkipItem_Click(object sender, RoutedEventArgs e)
        {
            //Skip Item
            Model.SkipItem(Label.Name(sender, "btn"));
        }


        // Volume
        // ========================================
        // ========================================
        // Mute Button Clicked
        private void VolumeBar_MuteClick(object sender, RoutedEventArgs e)
        {
            //Set Media Element IsMuted to Volume Bar IsMuted
            meVideo.IsMuted = VolumeBar.IsMuted;
        }

        // Value Updated
        private void VolumeBar_ValueUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Set Media Element Volume Value to the Value of Volume Bar
            meVideo.Volume = VolumeBar.Value;

            //Unmute Volume
            meVideo.IsMuted = false;
        }


        // Fullscreen
        // ========================================
        // ========================================
        private void btnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            //Change Window Mode
            ViewerModel.Change();
        }


        // Back
        // ========================================
        // ========================================
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //Run Back Method
            Model.Back();
        }
        #endregion Event Handlers
    }
}