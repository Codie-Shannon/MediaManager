using Media_Manager.Models;
using Media_Manager.Views;
using MediaControlsLibrary;
using MediaControlsLibrary.Dependencies;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using WpfToolkit.Controls;

namespace Media_Manager.ViewModels
{
    public class MusicViewModel
    {
        #region Variables
        // UI Elements
        // ========================================
        // ========================================
        private static ItemsControl icItems;
        private static ipProperty tbDuration, tbFormat, tbFileSize, tbCreated, tbSampleRate, tbAudioChannels, tbCompMode;


        // Selection
        // ========================================
        // ========================================
        private static int selectedId;
        private static Song selectedSong;


        // Collections
        // ========================================
        // ========================================
        private static List<Song> Songs;
        private static List<object> Composite;
        #endregion Variables



        // Setup
        // ========================================
        // ========================================
        // ========================================
        public static void Setup(ref ItemsControl icitems, ref ipProperty tbduration, ref ipProperty tbformat, ref ipProperty tbfilesize, ref ipProperty tbcreated, ref ipProperty tbsamplerate, ref ipProperty tbaudiochannels, ref ipProperty tbcompmode)
        {
            //Set Elements
            icItems = icitems;
            tbDuration = tbduration;
            tbFormat = tbformat;
            tbFileSize = tbfilesize;
            tbCreated = tbcreated;
            tbSampleRate = tbsamplerate;
            tbAudioChannels = tbaudiochannels;
            tbCompMode = tbcompmode;
        }



        #region Methods
        // Selection
        // ========================================
        // ========================================
        public static void ItemSelected(int selectedid, Song selectedsong, List<Song> songs, List<object> composite)
        {
            //Set Elements
            selectedId = selectedid;
            selectedSong = selectedsong;
            Songs = songs;
            Composite = composite;
        }


        // Skip
        // ========================================
        // ========================================
        public static void SkipItem(int index)
        {
            //Unset Element
            Elements.UnsetElement(ref icItems, selectedSong.Id);

            //Set Element
            Elements.SetElement(ref icItems, Songs.ElementAt(index), out selectedSong);

            //Bring Element Into View
            BringInToView();

            //Display Item Information
            DisplayItemInfo();
        }


        // Unset Item
        // ========================================
        // ========================================
        public static void UnsetItem() { if (selectedSong != null) { Elements.UnsetElement(ref icItems, selectedSong.Id); } }


        // Validation (Required for Music Player View Model)
        // ========================================
        // ========================================
        public static bool ValidateSelection(int oldid)
        {
            //Validate Selection Change
            if (oldid != selectedSong.Id)
            {
                //Return True
                return true;
            }

            //Return False
            return false;
        }
        #endregion Methods



        #region Extensions
        // Bring Into View
        // ========================================
        // ========================================
        private static void BringInToView()
        {
            //Get Selected Element
            ElementBase element = Elements.GetElement(icItems, ElementType.Files, selectedId);

            //Attempt to Bring the Selected Element Into the View
            if (element != null) { element.BringIntoView(); }

            //Check if the selected element has not been found or if the selected element is not visible
            if (element == null || !element.IsVisible)
            {
                //Get Virtualizing Wrap Panel
                VirtualizingWrapPanel vwpItems = Elements.FindVisualChildren<VirtualizingWrapPanel>(icItems).ElementAt(0);

                //Bring Selected Element Index Into View
                vwpItems.BringIndexIntoViewPublic(Composite.IndexOf(selectedSong));
            }
        }


        // Display Information
        // ========================================
        // ========================================
        private static void DisplayItemInfo()
        {
            //Display Main
            MusicPlayerView.Model.SetTitle(Formatter.FormatName(selectedSong.Name, selectedSong.CustomName));
            MusicPlayerView.Model.SetCover(Formatter.FormatImage(MediaType.Music, selectedSong.CoverImage));

            //Display Standard Details
            tbDuration.Content = Formatter.FormatDuration(selectedSong.Duration);
            tbFormat.Content = selectedSong.Format;
            tbFileSize.Content = Formatter.FormatFileSize(selectedSong.FileSize);
            tbCreated.Content = Formatter.FormatCreation(selectedSong.CreationTime, selectedSong.CreationDate);

            //Display Advance Details
            tbSampleRate.Content = Formatter.FormatSampleRate(selectedSong.SampleRate);
            tbAudioChannels.Content = selectedSong.AudioChannels;
            tbCompMode.Content = selectedSong.CompMode;
        }
        #endregion Extensions
    }
}