using Media_Manager.Models;
using MediaControlsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Media_Manager.ViewModels
{
    public class MusicPlayerViewModel
    {
        #region Variables
        // Main
        // =============================================
        // =============================================
        // =============================================
        public static int Index { get; private set; }
        public List<Song> Songs = new List<Song>();
        public Song selectedSong;
        public string File;



        // Elements
        // =============================================
        // =============================================
        // =============================================
        private MediaElement meMusic;
        private viewIconButton btnPrevious, btnNext, btnPlay;
        private ipTitle Title;
        private ipCover Cover;



        // Base Music Function
        // =============================================
        // =============================================
        // =============================================
        public bool isPlaying = false;



        // Skip
        // =============================================
        // =============================================
        // =============================================
        public bool isSongSkipped = false;
        #endregion Variables




        // Setup
        // =============================================
        // =============================================
        // =============================================
        // =============================================
        public void Setup(ipTitle title, ipCover cover, MediaElement memusic, viewIconButton btnprevious, viewIconButton btnnext, viewIconButton btnplay)
        {
            //Set Elements
            Title = title;
            Cover = cover;
            meMusic = memusic;
            btnPrevious = btnprevious;
            btnNext = btnnext;
            btnPlay = btnplay;
        }




        #region Methods
        // Play / Pause / Stop
        // =============================================
        // =============================================
        // =============================================
        public void Play()
        {
            //Check if a song is currently playing
            if (isPlaying)
            {
                //Pause Song
                meMusic.Pause();

                //Set isPlaying Boolean to False
                isPlaying = false;
            }
            else
            {
                //Play Song
                meMusic.Play();

                //Set isPlaying Boolean to True
                isPlaying = true;
            }
        }

        public void Stop()
        {
            //Pause Song
            meMusic.Stop();

            //Set isPlaying Boolean to False
            isPlaying = false;

            //Set Play Button Icon to Play Icon
            btnPlay.SetSecondaryIcon();
        }



        // Skip Item
        // =============================================
        // =============================================
        // =============================================
        public void SkipItem(string name)
        {
            //Set Index
            if (name == "previous" && Index > 0) { Index--; }
            else if (name == "previous") { Index = Songs.Count - 1; }
            else if (name == "next" && Index < Songs.Count() - 1) { Index++; }
            else if (name == "next") { Index = 0; }

            //Set Selected Song to Songs Element at the Position of the Index Variable 
            selectedSong = Songs.ElementAt(Index);

            //Set File to Selected Song's File Path
            File = selectedSong.FilePath;

            //Reset Play Button Icon
            btnPlay.SetPrimaryIcon();

            //Execute Music View Model's Skip Item Method
            MusicViewModel.SkipItem(Index);
            
            //Set Item
            SetItem();
        }



        // Set Item
        // =============================================
        // =============================================
        // =============================================
        private void SetItem()
        {
            //Check if a Song is Currently Playing
            if (isPlaying == true)
            {
                //Pause Song
                Play();
            }

            //Set isSongSkipped to True
            isSongSkipped = true;

            //Set Music Source
            meMusic.Source = new Uri(File, UriKind.Absolute);

            //Play Song
            Play();
        }
        #endregion Methods




        #region External Methods
        // Selection
        // =============================================
        // =============================================
        // =============================================
        public void SetSelectedItem(Song song, int oldid, bool isitemsreload)
        {
            //Check if the songs list has not just been reloaded, and if the song change is valid
            if (!isitemsreload && MusicViewModel.ValidateSelection(oldid))
            {
                //Set File Path
                File = song.FilePath;

                //Set Selected Song
                selectedSong = song;

                //Set Index to Index of Selected Song
                Index = Songs.IndexOf(song);

                //Reset Icon of Music Player Play Button
                PlayIconReset();

                //Set Item
                SetItem();
            }
        }



        // Set / Update Elements
        // =============================================
        // =============================================
        // =============================================
        // Update List
        // =============================================
        // =============================================
        public void UpdateList(List<Song> songs)
        {
            //Set Songs List to songs
            Songs = songs;

            //Toggle Previous / Next Buttons
            ToggleState.UIElements(new UIElement[] { btnPrevious, btnNext }, songs.Count > 1 ? true : false);
        }


        // Set Title
        // =============================================
        // =============================================
        public void SetTitle(string name) { Title.Content = name; }


        // Set Cover
        // =============================================
        // =============================================
        public void SetCover(BitmapImage cover) { Cover.Source = cover; }



        // Reset Play Icon
        // =============================================
        // =============================================
        // =============================================
        public void PlayIconReset() { btnPlay.SetPrimaryIcon(); }



        // Clear
        // =============================================
        // =============================================
        // =============================================
        public void Clear()
        {
            //Check if a Song is Currently Playing
            if (isPlaying)
            {
                //Stop Playing Song
                Play();
            }

            //Unset Selected Item
            MusicViewModel.UnsetItem();

            //Clear Music Player Source
            meMusic.Source = null;

            //Unset Selected Song, Songs List, Index, and File
            selectedSong = null;
            Songs = new List<Song>();
            Index = default(int);
            File = default(string);
        }
        #endregion External Methods
    }
}