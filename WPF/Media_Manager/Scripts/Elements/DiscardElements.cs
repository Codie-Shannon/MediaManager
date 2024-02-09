using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Media_Manager.Models;
using MediaControlsLibrary;
using Microsoft.VisualBasic.FileIO;

namespace Media_Manager
{
    public class DiscardElements
    {
        #region Remove Elements
        // Remove Folder
        // ========================================
        // ========================================
        public static bool RemoveFolder(MediaType type, int selectedid)
        {
            //Variables
            MessageBoxResult result;

            //Validate Media Type
            if (type == MediaType.TVShows)
            {
                //Confirm with the user if they would like to remove the folder
                result = CustomMessageBox.ShowYesNo($"Are you sure you would like to remove the folder from the application?\n\nPlease note that this will also remove any folders, tv shows, seasons, and episodes contained within the folder.", "WARNING", "Yes", "No", MessageBoxImage.Warning);
            }
            else
            {
                //Confirm with the user if they would like to remove the folder
                result = CustomMessageBox.ShowYesNo($"Are you sure you would like to remove the folder from the application?\n\nPlease note that this will also remove any folders and items contained within the folder.", "WARNING", "Yes", "No", MessageBoxImage.Warning);
            }

            //Check if the result was yes
            if (result == MessageBoxResult.Yes)
            {
                //Remove Selected Folder
                Database.RemoveFolder(type, selectedid);

                //Return True
                return true;
            }

            //Return False
            return false;
        }


        // Remove TV Show Folder
        // ========================================
        // ========================================
        public static bool RemoveTVShowFolder(RemovalType removaltype, TVShowFolder tvshowfolder)
        {
            //Initialize isRemoval Boolean
            bool isRemoval = false;

            //Check if type is Remove
            if (removaltype == RemovalType.Remove)
            {
                //Confirm with the user if they would like to remove the tv show
                MessageBoxResult result = CustomMessageBox.ShowYesNo($"Are you sure you would like to remove the tv show from the application?\n\nPlease note that this will also remove any seasons and episodes contained within the tv show.", "WARNING", "Yes", "No", MessageBoxImage.Warning);

                //Check if the result was yes
                if (result == MessageBoxResult.Yes)
                {
                    //Set isRemoval to True
                    isRemoval = true;
                }
            }

            //Check if the isRemoval boolean variable is set to true or if the type is delete
            if (isRemoval || removaltype == RemovalType.Delete)
            {
                //Remove Selected TV Show Folder
                Database.RemoveTVShowFolder(tvshowfolder);

                //Return True
                return true;
            }

            //Return False
            return false;
        }


        // Remove Season Folder
        // ========================================
        // ========================================
        public static bool RemoveSeasonFolder(RemovalType removaltype, SeasonFolder seasonfolder, bool iscustomcoverimageused, bool iscustomcoverimageparent)
        {
            //Initialize isRemoval Boolean
            bool isRemoval = false;

            //Check if type is Remove
            if (removaltype == RemovalType.Remove)
            {
                //Confirm with the user if they would like to remove the season
                MessageBoxResult result = CustomMessageBox.ShowYesNo($"Are you sure you would like to remove the season from the application?\n\nPlease note that this will also remove any episodes contained within the season.", "WARNING", "Yes", "No", MessageBoxImage.Warning);

                //Check if the result was yes
                if (result == MessageBoxResult.Yes)
                {
                    //Set isRemoval to True
                    isRemoval = true;
                }
            }

            //Check if the isRemoval boolean variable is set to true or if the type is delete
            if (isRemoval || removaltype == RemovalType.Delete)
            {
                //Remove Selected Season Folder
                Database.RemoveSeasonFolder(seasonfolder, iscustomcoverimageused, iscustomcoverimageparent);

                //Return True
                return true;
            }

            //Return False
            return false;
        }


        // Remove Item
        // ========================================
        // ========================================
        public static bool RemoveItem(MediaType type, int id, RemovalType removaltype, string coverImage = "")
        {
            //Initialize isRemoval Boolean
            bool isRemoval = false;

            //Check if type is Remove
            if (removaltype == RemovalType.Remove)
            {
                //Get Item Type
                string itemtype = GetItemType(type);

                //Confirm with the user if they would like to remove the item
                MessageBoxResult result = CustomMessageBox.ShowYesNo($"Are you sure you would like to remove the {itemtype} from the application?", "WARNING", "Yes", "No", MessageBoxImage.Warning);

                //Check if the result was yes
                if (result == MessageBoxResult.Yes)
                {
                    //Set isRemoval to True
                    isRemoval = true;
                }
            }

            //Check if the isRemoval boolean variable is set to true or if the type is delete
            if (isRemoval || removaltype == RemovalType.Delete)
            {
                //Remove Selected Item
                Database.RemoveItem(type, id, coverImage);

                //Return True
                return true;
            }

            //Return False
            return false;
        }
        #endregion Remove Elements




        #region Delete Elements
        #region Methods
        // Delete Season
        // ========================================
        // ========================================
        public static void DeleteTVShowFolder(out bool isCancel, out bool isDeleted, TVShowFolder tvshowfoler, List<SeasonFolder> seasonfolders, List<Episode> episodes)
        {
            //Initialize Variables
            bool isoverride = false;

            //Confirm Deletion with User
            MessageBoxResult result = CustomMessageBox.ShowYesNoCancel($"Are you sure you would like to delete the tv show from the computer system?\n\nPlease note for each tv show season, all episodes will be deleted before folder deletion validation can take place which will verify the season folders contents and validate deletion.", "WARNING", "Delete", "Recycle", "Cancel", MessageBoxImage.Warning);

            //Check if result is delete or recycle
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
            {
                //Get Deletion Type
                RecycleOption recycleoption = GetDeletionType(result);

                //Loop through Season Folders
                for (int i = 0; i < seasonfolders.Count; i++)
                {
                    //Get Current Looped Season's Episodes
                    List<Episode> episodestoremove = episodes.Where(j => j.OwnerId == seasonfolders[i].Id).ToList();

                    //Loop through and Delete Current Looped Seaon's Episodes 
                    foreach (Episode item in episodes) { DeletePath(recycleoption, item.FilePath, FileType.File); }

                    //Validate Season Folder Deletion
                    if (Directory.GetFiles(seasonfolders[i].FilePath, "*", System.IO.SearchOption.AllDirectories).Length == 0)
                    {
                        //Delete Season Folder
                        DeletePath(recycleoption, seasonfolders[i].FilePath, FileType.Folder);
                    }
                    else
                    {
                        //Validate isoverride Variable
                        if (!isoverride)
                        {
                            //Confirm Deletion with User
                            result = CustomMessageBox.ShowYesNoCancel($"Are you sure you would like to delete season {seasonfolders[i].SeasonNumber}'s folder from the computer system?\n\nPlease note this will also delete all files stored within the season folder.", "WARNING", "Yes", "Yes to All", "Cancel", MessageBoxImage.Warning);
                        }

                        //Check if result is yes
                        if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
                        {
                            //Delete Season Folder
                            DeletePath(recycleoption, seasonfolders[i].FilePath, FileType.Folder);

                            //Check if result is no
                            if (result == MessageBoxResult.No)
                            {
                                //Set isoverride to True
                                isoverride = true;
                            }
                        }
                        else
                        {
                            //Show Error Message
                            CustomMessageBox.ShowOK("Cannot delete season folder from computer system due to occupancy.", "ERROR", "OK", MessageBoxImage.Error);
                        }
                    }
                }

                //Set isCancel to False
                isCancel = false;

                //Set isDeleted to True
                isDeleted = true;

                //Return Method
                return;
            }
            else if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
            {
                //Set isCancel to True
                isCancel = true;

                //Set isDeleted to False
                isDeleted = false;

                //Return Method
                return;
            }

            //Set isCancel to False
            isCancel = false;

            //Set isDeleted to False
            isDeleted = false;
        }


        // Delete Season
        // ========================================
        // ========================================
        public static void DeleteSeasonFolder(out bool isCancel, out bool isDeleted, SeasonFolder selectedSeasonFolder, List<Episode> episodes)
        {
            //Initialize Variables
            string yes = "Delete", no = "Recycle";

            //Confirm Deletion with User
            MessageBoxResult result = CustomMessageBox.ShowYesNoCancel($"Are you sure you would like to delete the season from the computer system?\n\nPlease note all episodes will be deleted before folder deletion validation can take place which will verify the season folders contents and validate deletion.", "WARNING", yes, no, "Cancel", MessageBoxImage.Warning);

            //Check if result is delete or recycle
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
            {
                //Get Deletion Type
                RecycleOption recycleoption = GetDeletionType(result);

                //Loop through and Delete Episodes
                foreach (Episode item in episodes) { DeletePath(recycleoption, item.FilePath, FileType.File); }

                //Validate Season Folder Deletion
                if (Directory.GetFiles(selectedSeasonFolder.FilePath, "*", System.IO.SearchOption.AllDirectories).Length == 0)
                {
                    //Delete Season Folder
                    DeletePath(recycleoption, selectedSeasonFolder.FilePath, FileType.Folder);
                }
                else
                {
                    //Get Deletion Type
                    string type = result == MessageBoxResult.Yes ? yes : no;

                    //Confirm Deletion with User
                    result = CustomMessageBox.ShowYesNo($"Are you sure you would like to delete season {selectedSeasonFolder.SeasonNumber}'s folder from the computer system?\n\nPlease note this will also delete all files stored within the season folder.", "WARNING", type, "Cancel", MessageBoxImage.Warning);

                    //Check if result is yes
                    if (result == MessageBoxResult.Yes)
                    {
                        //Delete Season Folder
                        DeletePath(recycleoption, selectedSeasonFolder.FilePath, FileType.Folder);
                    }
                }

                //Set isCancel to False
                isCancel = false;

                //Set isDeleted to True
                isDeleted = true;

                //Return Method
                return;
            }
            else if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
            {
                //Set isCancel to True
                isCancel = true;

                //Set isDeleted to False
                isDeleted = false;

                //Return Method
                return;
            }

            //Set isCancel to False
            isCancel = false;

            //Set isDeleted to False
            isDeleted = false;
        }


        // Delete Item
        // ========================================
        // ========================================
        public static void DeleteItem(out bool isCancel, out bool isDeleted, MediaType type, FileType filetype, string path)
        {
            //Get Item Type
            string itemtype = GetItemType(type);

            //Confirm Deletion with User
            MessageBoxResult result = CustomMessageBox.ShowYesNoCancel($"Are you sure you would like to delete the {itemtype} from the computer system?", "WARNING", "Delete", "Recycle", "Cancel", MessageBoxImage.Warning);

            //Check if result is delete or recycle
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
            {
                //Get Deletion Type
                RecycleOption recycleoption = GetDeletionType(result);

                //Delete Item
                DeletePath(recycleoption, path, filetype);

                //Set isCancel to False
                isCancel = false;

                //Set isDeleted to True
                isDeleted = true;

                //Return Method
                return;
            }
            else if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
            {
                //Set isCancel to True
                isCancel = true;

                //Set isDeleted to False
                isDeleted = false;

                //Return Method
                return;
            }

            //Set isCancel to False
            isCancel = false;

            //Set isDeleted to False
            isDeleted = false;
        }
        #endregion Methods



        #region Extensions
        // Delete File
        // ========================================
        // ========================================
        private static void DeletePath(RecycleOption recycleOption, string path, FileType filetype)
        {
            //Check File Type
            if (filetype == FileType.File)
            {
                //Set File's Attributes to Normal
                File.SetAttributes(path, FileAttributes.Normal);

                //Delete File
                FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
            }
            else if(filetype == FileType.Folder)
            {
                //Create DirectoryInfo Object for Game Directory
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                //Loop through Files to Set Their Attributes Value to Normal
                directoryInfo.EnumerateFiles("*", System.IO.SearchOption.AllDirectories).Sum(fi => (int)(fi.Attributes = FileAttributes.Normal));

                //Set Game Directory's Attributes to Normal
                directoryInfo.Attributes = FileAttributes.Normal;

                //Delete Directory
                FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
            }
        }


        // Get Deletion Type
        // ========================================
        // ========================================
        private static RecycleOption GetDeletionType(MessageBoxResult result)
        {
            //Check if Result is Set to MessageBoxResult Yes
            if (result == MessageBoxResult.Yes)
            {
                //Return RecycleOption DeletePermanently
                return RecycleOption.DeletePermanently;
            }

            //Return RecycleOption SendToRecycleBin
            return RecycleOption.SendToRecycleBin;
        }
        #endregion Extensions
        #endregion Delete Elements




        // Extensions
        // ========================================
        // ========================================
        // ========================================
        private static string GetItemType(MediaType type)
        {
            //Get Media Type
            string mediatype = type.ToString().TrimEnd('s').ToLower();

            //Get and Return Item Type
            return mediatype == "music" ? "song" : mediatype;
        }
    }
}