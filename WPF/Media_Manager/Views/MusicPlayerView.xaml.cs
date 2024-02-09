using Media_Manager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Media_Manager.Views
{
    public partial class MusicPlayerView : UserControl
    {
        // Variables
        // =====================================================================
        // =====================================================================
        public static MusicPlayerViewModel Model = new MusicPlayerViewModel();



        // Constructor
        // =====================================================================
        // =====================================================================
        public MusicPlayerView()
        {
            //Initialize Components
            InitializeComponent();

            //Setup Model
            Model.Setup(Title, Cover, meMusic, btnPrevious, btnNext, btnPlay);
        }



        #region Event Handlers
        // Skip Item
        // ========================================
        // ========================================
        private void btnSkipItem_Click(object sender, RoutedEventArgs e)
        {
            //Skip Item
            Model.SkipItem(Label.Name(sender, "btn"));
        }


        // Play / Pause / Stop
        // ========================================
        // ========================================
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //Play / Pause Song
            Model.Play();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            //Stop Song
            Model.Stop();
        }


        // Volume
        // ========================================
        // ========================================
        // Mute Button Clicked
        private void VolumeBar_MuteClick(object sender, RoutedEventArgs e)
        {
            //Set Media Element IsMuted to Volume Bar IsMuted
            meMusic.IsMuted = VolumeBar.IsMuted;
        }

        // Value Updated
        private void VolumeBar_ValueUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Set Media Element Volume Value to the Value of Volume Bar
            meMusic.Volume = VolumeBar.Value;

            //Unmute Volume
            meMusic.IsMuted = false;
        }
        #endregion Event Handlers
    }
}