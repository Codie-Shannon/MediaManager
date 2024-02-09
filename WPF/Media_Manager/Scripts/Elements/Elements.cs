using System;
using System.Windows;
using System.Diagnostics;
using Media_Manager.Models;
using System.Windows.Media;
using MediaControlsLibrary;
using System.Windows.Controls;
using System.Collections.Generic;
using MediaControlsLibrary.Dependencies;
using MediaControlsLibrary.Models;
using Media_Manager.ViewModels;
using MediaControlsLibrary.Types;

namespace Media_Manager
{
    public class Elements
    {
        #region Unset Item
        public static void UnsetElements(ref ItemsControl icitems, ref int id, ref Movie selecteditem, ref Folder selectedfolder, ref ElementType selectedtype)
        {
            //Validate Selected Element
            if ((selectedtype == ElementType.Files && id != 0 && selecteditem != null) || (selectedtype == ElementType.Folders && selectedfolder != null))
            {
                //Get Selected Element's UI Element
                ElementBase element = GetElement(icitems, selectedtype, id);

                //Validate Selected UI Element
                if (element != null)
                {
                    //Unselect Element
                    element.IsSelected = false;
                }

                //Set id to 0
                id = 0;

                //Set Selected Elements to Null
                selecteditem = null;
                selectedfolder = null;

                //Set selectedtype to ElementType.Null
                selectedtype = ElementType.Null;
            }
        }

        public static void UnsetElements(ref ItemsControl icitems, ref int id, ref Episode selecteditem, ref SeasonFolder selectedseasonfolder, ref TVShowFolder selectedtvshowfolder, ref Folder selectedfolder, ref ElementType selectedtype, ref FolderType selectedfoldertype)
        {
            //Validate Selected Element
            if ((selectedtype == ElementType.Files && id != 0 && selecteditem != null) ||

                (selectedtype == ElementType.Folders &&
                ((selectedfoldertype == FolderType.Folders && selectedfolder != null) ||
                (selectedfoldertype == FolderType.TVShowFolders && selectedtvshowfolder != null) ||
                (selectedfoldertype == FolderType.SeasonFolders && selectedseasonfolder != null))))
            {
                //Get Selected Element's UI Element
                ElementBase element = GetElement(icitems, selectedtype, id, selectedfoldertype);

                //Validate Selected UI Element
                if (element != null)
                {
                    //Unselect Element
                    element.IsSelected = false;
                }

                //Set id to 0
                id = 0;

                //Set Selected Elements to Null
                selecteditem = null;
                selectedseasonfolder = null;
                //selectedtvshowfolder = null;
                selectedfolder = null;

                //Set Selected Types to Null
                selectedtype = ElementType.Null;
            }
        }

        public static void UnsetElements(ref ItemsControl icitems, ref int id, ref Video selecteditem, ref Folder selectedfolder, ref ElementType selectedtype)
        {
            //Validate Selected Element
            if ((selectedtype == ElementType.Files && id != 0 && selecteditem != null) || (selectedtype == ElementType.Folders && selectedfolder != null))
            {
                //Get Selected Element's UI Element
                ElementBase element = GetElement(icitems, selectedtype, id);

                //Validate Selected UI Element
                if (element != null)
                {
                    //Unselect Element
                    element.IsSelected = false;
                }

                //Set id to 0
                id = 0;

                //Set Selected Elements to Null
                selecteditem = null;
                selectedfolder = null;

                //Set selectedtype to ElementType.Null
                selectedtype = ElementType.Null;
            }
        }

        public static void UnsetElements(ref ItemsControl icitems, ref int id, ref Picture selecteditem, ref Folder selectedfolder, ref ElementType selectedtype)
        {
            //Validate Selected Element
            if ((selectedtype == ElementType.Files && id != 0 && selecteditem != null) || (selectedtype == ElementType.Folders && selectedfolder != null))
            {
                //Get Selected Element's UI Element
                ElementBase element = GetElement(icitems, selectedtype, id);

                //Validate Selected UI Element
                if (element != null)
                {
                    //Unselect Element
                    element.IsSelected = false;
                }

                //Set id to 0
                id = 0;

                //Set Selected Elements to Null
                selecteditem = null;
                selectedfolder = null;

                //Set selectedtype to ElementType.Null
                selectedtype = ElementType.Null;
            }
        }
        
        public static void UnsetElements(ref ItemsControl icitems, ref int id, ref Song selecteditem, ref Folder selectedfolder, ref ElementType selectedtype)
        {
            //Validate Selected Element
            if ((selectedtype == ElementType.Files && id != 0 && selecteditem != null) || (selectedtype == ElementType.Folders && selectedfolder != null))
            {
                //Get Selected Element's UI Element
                ElementBase element = GetElement(icitems, selectedtype, id);

                //Validate Selected UI Element
                if (element != null)
                {
                    //Unselect Element
                    element.IsSelected = false;
                }

                //Set id to 0
                id = 0;

                //Set Selected Elements to Null
                selecteditem = null;
                selectedfolder = null;

                //Set selectedtype to ElementType.Null
                selectedtype = ElementType.Null;

                //Unset Selected Music View Model Element
                MusicViewModel.UnsetItem();
            }
        }

        public static void UnsetElement(ref ItemsControl icitems, int id)
        {
            //Get Selected Element's UI Element
            ElementBase element = Elements.GetElement(icitems, ElementType.Files, id);

            //Validate Selected UI Element
            if (element != null)
            {
                //Unselect Element
                element.IsSelected = false;
            }
        }

        public static void UnsetElements(ref ItemsControl icitems, ref int id, ref Game selecteditem, ref Folder selectedfolder, ref ElementType selectedtype)
        {
            //Validate Selected Element
            if ((selectedtype == ElementType.Files && id != 0 && selecteditem != null) || (selectedtype == ElementType.Folders && selectedfolder != null))
            {
                //Get Selected Element's UI Element
                ElementBase element = GetElement(icitems, selectedtype, id);

                //Validate Selected UI Element
                if (element != null)
                {
                    //Unselect Element
                    element.IsSelected = false;
                }

                //Set id to 0
                id = 0;

                //Set Selected Elements to Null
                selecteditem = null;
                selectedfolder = null;

                //Set selectedtype to ElementType.Null
                selectedtype = ElementType.Null;
            }
        }
        #endregion Unset Item



        #region Set Element
        public static void SetElement(ref ItemsControl icitems, int id, out int selectedid, Movie item, out Movie selecteditem, Folder folder, out Folder selectedfolder, ElementType type, out ElementType selectedtype)
        {
            //Set Selected Elements to Null
            selecteditem = null;
            selectedfolder = null;

            //Check Element Type
            if (type == ElementType.Files)
            {
                //Set Selected Item to Item Variable
                selecteditem = item;
            }
            else if(type == ElementType.Folders)
            {
                //Set Selected Folder to Folder Variable
                selectedfolder = folder;
            }

            //Set selectedid and selectedtype
            selectedid = id;
            selectedtype = type;

            //Get Selected Element's UI Element
            ElementBase element = GetElement(icitems, type, id);

            //Validate Selected Element
            if (element != null)
            {
                //Select Element
                element.IsSelected = true;
            }
        }

        public static void SetElement(ref ItemsControl icitems, int id, out int selectedid, Episode item, out Episode selecteditem, SeasonFolder seasonfolder, out SeasonFolder selectedseasonfolder, TVShowFolder tvshowfolder, out TVShowFolder selectedtvshowfolder, Folder folder, out Folder selectedfolder, ElementType elementtype, out ElementType selectedelementtype, FolderType foldertype, out FolderType selectedfoldertype)
        {
            //Set Selected Elements to Null
            selecteditem = null;
            selectedfolder = null;
            selectedtvshowfolder = null;
            selectedseasonfolder = null;

            //Check Element Type
            if (elementtype == ElementType.Files)
            {
                //Set Selected Item to Item Variable
                selecteditem = item;
            }
            else if (elementtype == ElementType.Folders)
            {
                //Check Folder Type
                if (foldertype == FolderType.Folders)
                {
                    //Set Selected Folder to Folder Variable
                    selectedfolder = folder;
                }
                else if (foldertype == FolderType.TVShowFolders)
                {
                    //Set Selected TV Show Folder to TV Show Folder Variable
                    selectedtvshowfolder = tvshowfolder;
                }
                else if (foldertype == FolderType.SeasonFolders)
                {
                    //Set Selected Season Folder to Season Folder Variable
                    selectedseasonfolder = seasonfolder;
                }
            }

            //Set Selected Details
            selectedid = id;
            selectedelementtype = elementtype;
            selectedfoldertype = foldertype;

            //Get Selected Element's UI Element
            ElementBase element = GetElement(icitems, elementtype, id, foldertype);

            //Validate Selected Element
            if (element != null)
            {
                //Select Element
                element.IsSelected = true;
            }
        }

        public static void SetElement(ref ItemsControl icitems, int id, out int selectedid, Video item, out Video selecteditem, Folder folder, out Folder selectedfolder, ElementType type, out ElementType selectedtype)
        {
            //Set Selected Elements to Null
            selecteditem = null;
            selectedfolder = null;

            //Check Element Type
            if (type == ElementType.Files)
            {
                //Set Selected Item to Item Variable
                selecteditem = item;
            }
            else if (type == ElementType.Folders)
            {
                //Set Selected Folder to Folder Variable
                selectedfolder = folder;
            }

            //Set selectedid and selectedtype
            selectedid = id;
            selectedtype = type;

            //Get Selected Element's UI Element
            ElementBase element = GetElement(icitems, type, id);

            //Validate Selected Element
            if (element != null)
            {
                //Select Element
                element.IsSelected = true;
            }
        }

        public static void SetElement(ref ItemsControl icitems, int id, out int selectedid, Picture item, out Picture selecteditem, Folder folder, out Folder selectedfolder, ElementType type, out ElementType selectedtype)
        {
            //Set Selected Elements to Null
            selecteditem = null;
            selectedfolder = null;

            //Check Element Type
            if (type == ElementType.Files)
            {
                //Set Selected Item to Item Variable
                selecteditem = item;
            }
            else if (type == ElementType.Folders)
            {
                //Set Selected Folder to Folder Variable
                selectedfolder = folder;
            }

            //Set selectedid and selectedtype
            selectedid = id;
            selectedtype = type;

            //Get Selected Element's UI Element
            ElementBase element = GetElement(icitems, type, id);

            //Validate Selected Element
            if (element != null)
            {
                //Select Element
                element.IsSelected = true;
            }
        }

        public static void SetElement(ref ItemsControl icitems, int id, out int selectedid, Song item, out Song selecteditem, Folder folder, out Folder selectedfolder, ElementType type, out ElementType selectedtype, List<Song> songs, List<object> composite)
        {
            //Set Selected Elements to Null
            selecteditem = null;
            selectedfolder = null;

            //Check Element Type
            if (type == ElementType.Files)
            {
                //Set Selected Item to Item Variable
                selecteditem = item;

                //Set Selected Music View Model Element
                MusicViewModel.ItemSelected(id, selecteditem, songs, composite);
            }
            else if (type == ElementType.Folders)
            {
                //Set Selected Folder to Folder Variable
                selectedfolder = folder;
            }

            //Set selectedid and selectedtype
            selectedid = id;
            selectedtype = type;

            //Get Selected Element's UI Element
            ElementBase element = GetElement(icitems, type, id);

            //Validate Selected Element
            if (element != null)
            {
                //Select Element
                element.IsSelected = true;
            }
        }

        public static void SetElement(ref ItemsControl icitems, Song item, out Song selecteditem)
        {
            //Set Selected Item
            selecteditem = item;

            //Get Selected Element's UI Element
            ElementBase element = Elements.GetElement(icitems, ElementType.Files, selecteditem.Id);

            //Validate Selected Element
            if (element != null)
            {
                //Select Element
                element.IsSelected = true;
            }
        }

        public static void SetElement(ref ItemsControl icitems, int id, out int selectedid, Game item, out Game selecteditem, Folder folder, out Folder selectedfolder, ElementType type, out ElementType selectedtype)
        {
            //Set Selected Elements to Null
            selecteditem = null;
            selectedfolder = null;

            //Check Element Type
            if (type == ElementType.Files)
            {
                //Set Selected Item to Item Variable
                selecteditem = item;
            }
            else if (type == ElementType.Folders)
            {
                //Set Selected Folder to Folder Variable
                selectedfolder = folder;
            }

            //Set selectedid and selectedtype
            selectedid = id;
            selectedtype = type;

            //Get Selected Element's UI Element
            ElementBase element = GetElement(icitems, type, id);

            //Validate Selected Element
            if (element != null)
            {
                //Select Element
                element.IsSelected = true;
            }
        }
        #endregion Set Element



        #region Get Elements
        // Get Elements
        // =======================================================
        // =======================================================
        // =======================================================
        public static ElementBase GetElement(ItemsControl icitems, ElementType type, int id, FolderType foldertype = FolderType.Null)
        {
            //Get Visual Children from Items Control
            foreach (ElementBase element in FindVisualChildren<ElementBase>(icitems))
            {
                //Validate Element ID
                if(element != null && element.Id == id)
                {
                    //Validate Element
                    if (type == ElementType.Files && (element is CoverItem i || element is ImageItem j))
                    {
                        //Return Current Looped Element
                        return element;
                    }
                    else if (type == ElementType.Folders && (element is CoverFolder f || element is ImageFolder l) && foldertype == ((FolderBase)element).FolderType)
                    {
                        //Return Current Looped Element
                        return element;
                    }
                }
            }

            //Return Null
            return null;
        }


        // Find Visual Children
        // =======================================================
        // =======================================================
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            //Check if the Dependency Object has been Set
            if (depObj != null)
            {
                //Loop Through Visual Tree Helper Children of Dependency Object
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    //Get Visual Tree Helper Child at Position i of the Dependency Object
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    //Check if the Child Dependency Object has been Set
                    if (child != null && child is T)
                    {
                        //Cast Child Dependency Object to Type T and Return It
                        yield return (T)child;
                    }

                    //Loop Through Visual Tree Children of Child Dependency Object
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        //Return Child of Child
                        yield return childOfChild;
                    }
                }
            }
        }
        #endregion Get Elements



        #region Context Menu
        public static void SetContextMenu(ItemsControl icitems, ElementType type, int id, ContextMenuItem favouritemenuitem, FolderType foldertype = FolderType.Null)
        {
            //Get Element
            Control element = GetElement(icitems, type, id, foldertype);

            //Set Context Menu Placement to Absolute
            element.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute;

            //Set Offset to Center of Element
            element.ContextMenu.HorizontalOffset = 0 - Convert.ToDouble(element.PointFromScreen(new Point(0, 0)).X - (element.ActualWidth / 3));
            element.ContextMenu.VerticalOffset = 0 - Convert.ToDouble(element.PointFromScreen(new Point(0, 0)).Y - (element.ActualHeight / 3));

            //Check if favouritemenuitem has been Set
            if (favouritemenuitem != null)
            {
                //Loop through Elements in Context Menu
                for (int i = 0; i < element.ContextMenu.Items.Count; i++)
                {
                    //Get Current Looped Element
                    ContextMenuItem item = element.ContextMenu.Items[i] as ContextMenuItem;

                    //Check if the Current Looped Element Matches the Criteria for a Favourite or Unfavourite ContextMenuItem
                    if ($"{item.Header}" == "Favourite" || $"{item.Header}" == "Unfavourite")
                    {
                        //Remove Favourite or Unfavourite ContextMenuItem from Context Menu
                        element.ContextMenu.Items.RemoveAt(i);
                    }
                }

                //Insert favouritemenuitem to Context Menu
                element.ContextMenu.Items.Insert(1, favouritemenuitem);
            }

            //Open Context Menu
            element.ContextMenu.IsOpen = true;
        }
        #endregion Context Menu



        #region Folders
        // Navigation Bar
        // =======================================================
        // =======================================================
        public static void InitializeFolders(NavigationBar nbnavigationbar, List<Folder> folders, Stack<Stack<Tuple<int, int, string, string>>> navigationloadedstack)
        {
            //Clear Navigation Bar Folders
            nbnavigationbar.ClearAll();

            //Add Main Folder to Navigation Bar
            nbnavigationbar.Add(0, -1, "Main", nameof(FolderType.Folders));

            //Loop through elements in folders list
            for (int i = 0; i < folders.Count; i++)
            {
                //Add Current Looped Folder to Navigation Bar
                nbnavigationbar.Add(folders[i].Id, folders[i].OwnerId, folders[i].Name, folders[i].FolderType);
            }

            //Check if Navigation Loaded Stack is Null
            if (navigationloadedstack == null)
            {
                //Load Main Folder
                nbnavigationbar.Load(0);
            }
        }


        // Folder Browser
        // =======================================================
        // =======================================================
        public static void InitializeFolderBrowserDialog(List<Folder> folders, ref optFolderBrowserDialog fbdItem, ref optFolderBrowserDialog fbdFolder)
        {
            //Clear Item Folder Browser Dialog Object Properties
            fbdItem.SetValue(FolderBrowserDialogBase.EditSelectedIdProperty, -1);
            fbdItem.SetValue(FolderBrowserDialogBase.DBNameProperty, string.Empty);
            fbdItem.SetValue(FolderBrowserDialogBase.FolderPathProperty, string.Empty);

            //Clear Folder FolderBrowser Dialog Object Properties
            fbdFolder.SetValue(FolderBrowserDialogBase.EditSelectedIdProperty, -1);
            fbdFolder.SetValue(FolderBrowserDialogBase.DBNameProperty, string.Empty);
            fbdFolder.SetValue(FolderBrowserDialogBase.FolderPathProperty, string.Empty);

            //Clear Folder Browser
            optFolderBrowserDialog.Clear();

            //Add Base and Main Folder to Folder Browser
            optFolderBrowserDialog.AddFolder(-1, -2, "Base");
            optFolderBrowserDialog.AddFolder(0, -1, "Main");

            //Loop through Folders in the Folders Observable Collection
            foreach (Folder folder in folders)
            {
                //Add Folder to Folder Browser
                optFolderBrowserDialog.AddFolder(folder.Id, folder.OwnerId, folder.Name);
            }
        }
        #endregion Folders



        #region Show In Explorer
        public static void ShowInExplorer(FileType type, string path)
        {
            //Check File Type
            if (type == FileType.Folder)
            {
                //Open Folder in File Explorer
                Process.Start("explorer.exe", path);
            }
            else if (type == FileType.File)
            {
                //Open File Explorer with the Passed Path Selected
                Process.Start("explorer.exe", "/select, \"" + path + "\"");
            }
        }
        #endregion Show In Explorer
    }
}