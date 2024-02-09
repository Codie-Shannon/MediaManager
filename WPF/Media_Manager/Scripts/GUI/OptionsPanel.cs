using System;
using System.Windows;
using MediaControlsLibrary;
using System.Collections.Generic;
using Media_Manager.Models;
using MediaControlsLibrary.Types;

namespace Media_Manager
{
    public class OptionsPanel
    {
        // Get optPanel Ancestor
        // =======================================================
        // =======================================================
        public static FrameworkElement GetAncestor(FrameworkElement element, Type type, int loopcount = 20)
        {
            //Loop the Length of loopcount
            for (int i = 0; i < loopcount; i++)
            {
                //Get Parent
                element = (FrameworkElement)element.Parent;

                //Check Type
                if (element.GetType() == type)
                {
                    //Return Element
                    return element;
                }
            }

            //Return Null
            return null;
        }



        // Load Editable Values For Edit Panel
        // =======================================================
        // =======================================================
        public static void LoadEditableValues(optTextBoxLong tbName, string name, optFolderBrowserDialog fbdsavelocation, int ownerid, string folderpath, ElementType selectedType, int selectedId, List<optOpenDialog> opendialogs = null, List<string> opendialogvalues = null, MediaType type = MediaType.Null, List<optSearchBox> searchboxes = null, object selectedItem = null, FolderType selectedFolderType = FolderType.Folders, List<optNumericBox> numericboxes = null, List<int> numericboxvalues = null)
        {
            //Validate Name
            if (tbName != null)
            {
                //Set Name
                tbName.Content = name;
            }

            //Validate Save Location
            if (fbdsavelocation != null)
            {
                //Set Owner
                fbdsavelocation.Id = ownerid;
                fbdsavelocation.FolderPath = folderpath;

                //Set Selected Id
                fbdsavelocation.EditSelectedId = selectedType == ElementType.Folders && selectedFolderType == FolderType.Folders && selectedId != 0 ? selectedId : -2;
            }

            //Set Open Dialogs
            for (int i = 0; i < (opendialogs != null ? opendialogs.Count : 0); i++)
            {
                //Set Value for Current Looped Element
                opendialogs[i].Content = opendialogvalues[i];
            }

            //Set SearchBox Values
            for (int i = 0; i < (searchboxes != null ? searchboxes.Count : 0); i++)
            {
                //Set Values for Current Looped Element
                if (type == MediaType.Movies)
                {
                    //Convert selected Item Object to Movie Search Object
                    Movie movie = (Movie)selectedItem;

                    //Select Movie Item
                    searchboxes[i].Select(movie.Name, movie.CoverImage, movie.MetaCriticLink, movie.IMDBLink);
                }
                else
                {
                    //Convert selected Item Object to Game Search Object
                    Game game = (Game)selectedItem;

                    //Select Game Item
                    searchboxes[i].Select(game.Name, game.CoverImage, game.AvailablePlatforms, game.Type, game.IGDBLink);
                }
            }

            //Set Numeric Box Values
            if (numericboxes != null && numericboxes.Count > 0)
            {
                for (int i = 0; i < numericboxes.Count; i++)
                {
                    //Set Value for Current Looped Element
                    numericboxes[i].SetValue(numericboxvalues[i]);
                }
            }
        }



        // Clear Panel
        // =======================================================
        // =======================================================
        public static void ClearPanel(List<optTextBox> textboxes, List<optTextBoxLong> textboxlongs, List<optSearchBox> searchboxes, List<optOpenDialog> openFileDialogs, List<optNumericBox> numericboxes = null)
        {
            //Clear All Textboxes
            if (textboxes != null && textboxes.Count > 0)
            {
                foreach (optTextBox textbox in textboxes)
                {
                    textbox.Content = string.Empty;
                }
            }


            //Clear All Long Textboxes
            if (textboxlongs != null && textboxlongs.Count > 0)
            {
                foreach (optTextBoxLong textboxlong in textboxlongs)
                {
                    textboxlong.Content = string.Empty;
                }
            }


            //Clear All Search Boxes
            if (searchboxes != null && searchboxes.Count > 0)
            {
                foreach (optSearchBox searchbox in searchboxes)
                {
                    searchbox.Deselect();
                    searchbox.Clear();
                }
            }


            //Clear All OpenFileDialogs
            if (openFileDialogs != null && openFileDialogs.Count > 0)
            {
                foreach (optOpenDialog openFileDialog in openFileDialogs)
                {
                    openFileDialog.Content = string.Empty;
                    openFileDialog.Contents.Clear();
                }
            }


            //Clear All Numeric Boxes
            if(numericboxes != null && numericboxes.Count > 0)
            {
                foreach (optNumericBox numericbox in numericboxes)
                {
                    numericbox.Clear();
                }
            }
        }
    }
}