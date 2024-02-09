using System;
using System.Linq;
using MediaControlsLibrary;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MediaControlsLibrary.Models;
using System.IO;
using Media_Manager.Models;

namespace Media_Manager
{
    public class Validation
    {
        #region Graphical User Interface Validation
        #region Movies Validation
        // Validate Movie Panel
        // =====================================
        // =====================================
        public static void ValidateMoviePanel(OptionPanelType type, optButton button, List<Folder> folders, string[] values, KeyValuePair<int, string> savelocation, string[] checkvalues = null, KeyValuePair<int, string> checksavelocation = new KeyValuePair<int, string>())
        {
            //Check Option Panel Type
            if (type == OptionPanelType.Add)
            {
                //Validate Movie Add Panel
                ValidateMovieAddPanel(button, folders, values, savelocation);
            }
            else if (type == OptionPanelType.Edit)
            {
                //Validate Movie Edit Panel
                ValidateMovieEditPanel(button, folders, values, checkvalues, savelocation, checksavelocation);
            }
        }


        // Validate Movie Add Panel
        // =====================================
        // =====================================
        private static void ValidateMovieAddPanel(optButton button, List<Folder> folders, string[] values, KeyValuePair<int, string> savelocation)
        {
            //Initialize isValid Boolean
            bool isValid = false;

            //Validate Add Panel
            if (ValidateSaveLocation(OptionPanelType.Add, folders, savelocation) && values.Length == 3 && Process(values[0], true, true, new string[] { "https://www.imdb.com/", "https://www.metacritic.com/" }, new string[] { values[1], values[2] }, new int[] { 30, 35 }, 1))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Add Button
            ToggleState.UIElement(button, isValid);
        }


        // Validate Movie Edit Panel
        // =====================================
        // =====================================
        private static void ValidateMovieEditPanel(optButton button, List<Folder> folders, string[] values, string[] checkvalues, KeyValuePair<int, string> savelocation, KeyValuePair<int, string> checksavelocation = new KeyValuePair<int, string>())
        {
            //Initialize Booleans
            bool isCustomName = false, isPath = false, isFile = false, isLink = false, isURL = false, isSaveLocation, isValid = false;

            //Validate Custom Name
            if (checkvalues[0] == values[0]) { isCustomName = true; }

            //Validate Path
            if (checkvalues[1] == values[1]) { isPath = true; }

            //Validate File
            if (File(checkvalues[1])) { isFile = true; }

            //Validate URLs
            if (checkvalues[2] != values[2] || checkvalues[3] != values[3])
            {
                //Set isLink Boolean to True
                isLink = true;

                //Validate URLs
                isURL = URL(new string[] { "https://www.imdb.com/", "https://www.metacritic.com/" }, new string[] { checkvalues[2], checkvalues[3] }, new int[] { 37, 35 }, 1);
            }

            //Validate Save Location
            isSaveLocation = ValidateSaveLocation(OptionPanelType.Edit, folders, savelocation, checksavelocation);

            //Run Conditional Statement
            if ((isCustomName && isPath && isFile && isURL && !isSaveLocation) ||
                (!isCustomName && isPath && isFile && !isLink && !isSaveLocation) ||
                (!isCustomName && isPath && isFile && isURL && !isSaveLocation) ||
                (!isCustomName && !isPath && isFile && !isLink && !isSaveLocation) ||
                (isCustomName && !isPath && isFile && isURL && !isSaveLocation) ||
                (isCustomName && !isPath && isFile && !isLink && !isSaveLocation) ||
                (isCustomName && !isPath && isFile && !isLink && isSaveLocation) ||
                (!isCustomName && isPath && isFile && isURL && isSaveLocation) ||
                (!isCustomName && !isPath && isFile && isURL && isSaveLocation) ||
                (isCustomName && isPath && isFile && !isLink && isSaveLocation) ||
                (isCustomName && isPath && isFile && isURL && isSaveLocation) ||
                (!isCustomName && isPath && isFile && isURL && isSaveLocation) ||
                (!isCustomName && isPath && isFile && !isLink && isSaveLocation) ||
                (isCustomName && !isPath && isFile && isURL && isSaveLocation) ||
                (!isCustomName && !isPath && isFile && !isLink && isSaveLocation))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Edit Button
            ToggleState.UIElement(button, isValid);
        }
        #endregion Movies Validation



        #region TV Show Validation
        // Validate TV Show Panel
        // =====================================
        // =====================================
        public static void ValidateTVShowPanel(OptionPanelType type, optButton button, List<Folder> folders, string[] values, KeyValuePair<int, string> savelocation, string[] checkvalues = null, KeyValuePair<int, string> checksavelocation = new KeyValuePair<int, string>())
        {
            //Check Option Panel Type
            if (type == OptionPanelType.Add)
            {
                //Validate TV Show Add Panel
                ValidateTVShowAddPanel(button, folders, values, savelocation);
            }
            else if (type == OptionPanelType.Edit)
            {
                //Validate TV Show Edit Panel
                ValidateTVShowEditPanel(button, folders, values, checkvalues, savelocation, checksavelocation);
            }
        }


        // Validate TV Show Add Panel
        // =====================================
        // =====================================
        private static void ValidateTVShowAddPanel(optButton button, List<Folder> folders, string[] values, KeyValuePair<int, string> savelocation)
        {
            //Initialize isValid Boolean
            bool isValid = false;

            //Validate Add Panel
            if (ValidateSaveLocation(OptionPanelType.Add, folders, savelocation) && Process(values[0], true, true, new string[] { "https://www.imdb.com/", "https://www.metacritic.com/" }, new string[] { values[1], values[2] }, new int[] { 30, 35 }, 1))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Add Button
            ToggleState.UIElement(button, isValid);
        }


        // Validate TV Show Edit Panel
        // =====================================
        // =====================================
        private static void ValidateTVShowEditPanel(optButton button, List<Folder> folders, string[] values, string[] checkvalues, KeyValuePair<int, string> savelocation, KeyValuePair<int, string> checksavelocation = new KeyValuePair<int, string>())
        {
            //Initialize Booleans
            bool isCustomName = false, isPath = false, isFile = false, isSaveLocation, isValid = false;

            //Validate Custom Name
            if (checkvalues[0] == values[0]) { isCustomName = true; }

            //Validate Path
            if (checkvalues[1] == values[1]) { isPath = true; }

            //Validate File
            if (File(checkvalues[1])) { isFile = true; }

            //Validate Save Location
            isSaveLocation = ValidateSaveLocation(OptionPanelType.Edit, folders, savelocation, checksavelocation);

            //Run Conditional Statement
            if ((isCustomName && !isPath && !isFile && isSaveLocation) ||
                (!isCustomName && !isPath && !isFile && isSaveLocation) ||
                (isCustomName && !isPath && isFile && isSaveLocation) ||
                (!isCustomName && !isPath && isFile && isSaveLocation) ||
                (isCustomName && !isPath && isFile && !isSaveLocation) ||
                (!isCustomName && !isPath && !isFile && !isSaveLocation) ||
                (!isCustomName && !isPath && isFile && !isSaveLocation) ||
                (isCustomName && !isPath && !isFile && !isSaveLocation))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Edit Button
            ToggleState.UIElement(button, isValid);
        }
        #endregion TV Show Validation



        #region Season Validation
        // Validate Season Panel
        // =====================================
        // =====================================
        public static void ValidateSeasonPanel(OptionPanelType type, optButton button, int ownerid, List<SeasonFolder> seasonfolders, string[] values, string[] checkvalues = null)
        {
            //Check Option Panel Type
            if (type == OptionPanelType.Add)
            {
                //Validate Season Add Panel
                ValidateSeasonAddPanel(button, ownerid, seasonfolders, values);
            }
            else if (type == OptionPanelType.Edit)
            {
                //Validate Season Edit Panel
                ValidateSeasonEditPanel(button, values, checkvalues);
            }
        }


        // Validate Season Add Panel
        // =====================================
        // =====================================
        private static void ValidateSeasonAddPanel(optButton button, int ownerid, List<SeasonFolder> seasonfolders, string[] values)
        {
            //Initialize Variables
            List<SeasonFolder> matchingfolders = seasonfolders.Where(i => i.OwnerId == ownerid && i.SeasonNumber == Convert.ToInt32(values[0])).ToList();
            bool isValid = false;

            //Validate Add Panel
            if (matchingfolders.Count == 0 && Directory(values[1]) && DirectoryContainFiles(new DirectoryInfo(values[1]), new string[] { ".mkv", ".mp4", ".avi", ".mov" }) && File(values[2], true))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Add Button
            ToggleState.UIElement(button, isValid);
        }


        // Validate Season Edit Panel
        // =====================================
        // =====================================
        private static void ValidateSeasonEditPanel(optButton button, string[] values, string[] checkvalues)
        {
            //Initialize Variables
            bool isPath = false, isFile = false, isValid = false;

            //Validate Path
            if (checkvalues[0] != values[0]) { isPath = true; }

            //Validate File
            if (File(checkvalues[0])) { isFile = true; }

            //Run Conditional Statement
            if ((isFile && isPath) || (!isFile && isPath))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Edit Button
            ToggleState.UIElement(button, isValid);
        }
        #endregion Season Validation



        #region Episodes Validation
        // Validate Season Panel
        // =====================================
        // =====================================
        public static void ValidateEpisodesPanel(OptionPanelType type, optButton button, string seasondirectory, int ownerid, List<Episode> episodes, string[] values, bool ismultiselect, int contentcounter, List<OpenDialogItem> oditems = null, string[] checkvalues = null)
        {
            //Check Option Panel Type
            if (type == OptionPanelType.Add)
            {
                //Validate Episodes Add Panel
                ValidateEpisodesAddPanel(button, seasondirectory, ownerid, episodes, values, ismultiselect, contentcounter, oditems);
            }
            else if (type == OptionPanelType.Edit)
            {
                //Validate Episodes Edit Panel
                ValidateEpisodesEditPanel(button, ownerid, episodes, values, checkvalues);
            }
        }


        // Validate Episodes Add Panel
        // =====================================
        // =====================================
        private static void ValidateEpisodesAddPanel(optButton button, string seasondirectory, int ownerid, List<Episode> episodes, string[] values, bool ismultiselect, int contentcounter, List<OpenDialogItem> oditems)
        {
            //Initialize Variables
            bool isEpisode = false, isSeasonDirectory = false, isValid = false;

            //Validate Episode
            if (!ismultiselect && File(values[0]) && !episodes.Any(i => i.OwnerId == ownerid && i.FilePath == values[0]) || ismultiselect && !episodes.Any(i => i.OwnerId == ownerid && oditems.Select(j => j.FilePath.ToLower()).Contains(i.FilePath.ToLower()))) { isEpisode = true; }

            //Validate Season Directory
            if (!ismultiselect && values[0].Contains(seasondirectory) || ismultiselect && oditems.Any(i => i.FilePath.ToLower().Contains(seasondirectory.ToLower()))) { isSeasonDirectory = true; }

            //Validate Episode(s), Season Directory, Multiselect, and Open Dialog Content Counter
            if (isEpisode && isSeasonDirectory && (!ismultiselect || ismultiselect && contentcounter > 0)) { isValid = true; }

            //Toggle Add Button
            ToggleState.UIElement(button, isValid);
        }


        // Validate Episodes Edit Panel
        // =====================================
        // =====================================
        private static void ValidateEpisodesEditPanel(optButton button, int ownerid, List<Episode> episodes, string[] values, string[] checkvalues)
        {
            //Initialize Variables
            bool isSeasonNumber = false, isPath = false, isFile = false, isValid = false;
            //List<SeasonFolder> folders = seasonfolders.Where(i => i.OwnerId == ownerid && i.SeasonNumber == Convert.ToInt32(checkvalues[0])).ToList();

            //Validate Season Number
            //if (checkvalues[0] != values[0] && folders.Count == 0) { isSeasonNumber = true; }

            //Validate Path
            if (checkvalues[1] != values[1]) { isPath = true; }

            //Validate File
            if (File(checkvalues[1])) { isFile = true; }

            //Run Conditional Statement
            if ((!isSeasonNumber && isPath && !isFile) ||
                (isSeasonNumber && isPath && !isFile) ||
                (isSeasonNumber && !isPath && isFile) ||
                (!isSeasonNumber && isPath && isFile) ||
                (isSeasonNumber && isPath && isFile) ||
                (isSeasonNumber && !isPath && !isFile))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Edit Button
            ToggleState.UIElement(button, isValid);
        }
        #endregion Episodes Validation



        #region Games Validation
        // Validate Panel
        // =====================================
        // =====================================
        public static void ValidateGamePanel(OptionPanelType type, optButton button, List<Folder> folders, string[] values, KeyValuePair<int, string> savelocation, string[] checkvalues = null, KeyValuePair<int, string> checksavelocation = new KeyValuePair<int, string>())
        {
            //Check Option Panel Type
            if (type == OptionPanelType.Add)
            {
                //Validate Game Add Panel
                ValidateGameAddPanel(button, folders, values, savelocation);
            }
            else if (type == OptionPanelType.Edit)
            {
                //Validate Game Edit Panel
                ValidateGameEditPanel(button, folders, values, checkvalues, savelocation, checksavelocation);
            }
        }


        // Validate Add Panel
        // =====================================
        // =====================================
        private static void ValidateGameAddPanel(optButton button, List<Folder> folders, string[] values, KeyValuePair<int, string> savelocation)
        {
            //Initialize isValid Boolean
            bool isValid = false;

            //Validate Add Panel
            if (ValidateSaveLocation(OptionPanelType.Add, folders, savelocation) && values.Length == 3 && Process(values[0], true, true, new string[] { "https://www.igdb.com/games/" }, new string[] { values[1] }, new int[] { 29 }, 1, true, values[2]))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Add Button
            ToggleState.UIElement(button, isValid);
        }


        // Validate Edit Panel
        // =====================================
        // =====================================
        private static void ValidateGameEditPanel(optButton button, List<Folder> folders, string[] values, string[] checkvalues, KeyValuePair<int, string> savelocation, KeyValuePair<int, string> checksavelocation)
        {
            //Initialize Booleans
            bool isCustomName = false, isPath = false, isFile = false, isLink = false, isURL = false, isSaveLocation = false, isValid = false;

            //Validate Custom Name
            if (checkvalues[0] == values[0]) { isCustomName = true; }

            //Validate Paths
            if (checkvalues[1] == values[1] && checkvalues[2] == checkvalues[2]) { isPath = true; }

            //Validate File and Directory
            if (File(checkvalues[1]) && Directory(checkvalues[2]) && checkvalues[1].Contains(checkvalues[2])) { isFile = true; }

            //Validate URL
            if (checkvalues[3] != values[3])
            {
                //Set IsLink Boolean to True
                isLink = true;

                //Validate URL
                isURL = URL(new string[] { "https://www.igdb.com/games/" }, new string[] { checkvalues[3] }, new int[] { 29 }, 1);
            }

            //Validate Save Location
            isSaveLocation = ValidateSaveLocation(OptionPanelType.Edit, folders, savelocation, checksavelocation);

            //Run Conditional Statement
            if ((isCustomName && isPath && isFile && isURL && !isSaveLocation) ||
                (!isCustomName && isPath && isFile && !isLink && !isSaveLocation) ||
                (!isCustomName && isPath && isFile && isURL && !isSaveLocation) ||
                (!isCustomName && !isPath && isFile && !isLink && !isSaveLocation) ||
                (isCustomName && !isPath && isFile && isURL && !isSaveLocation) ||
                (isCustomName && !isPath && isFile && !isLink && !isSaveLocation) ||
                (isCustomName && !isPath && isFile && !isLink && isSaveLocation) ||
                (!isCustomName && isPath && isFile && isURL && isSaveLocation) ||
                (!isCustomName && !isPath && isFile && isURL && isSaveLocation) ||
                (isCustomName && isPath && isFile && !isLink && isSaveLocation) ||
                (isCustomName && isPath && isFile && isURL && isSaveLocation) ||
                (!isCustomName && isPath && isFile && isURL && isSaveLocation) ||
                (!isCustomName && isPath && isFile && !isLink && isSaveLocation) ||
                (isCustomName && !isPath && isFile && isURL && isSaveLocation) ||
                (!isCustomName && !isPath && isFile && !isLink && isSaveLocation))
            {
                //Set isValid to True
                isValid = true;
            }

            //Toggle Edit Button
            ToggleState.UIElement(button, isValid);
        }
        #endregion Games Validation



        #region Generic Validation
        // Validate Panel
        // =====================================
        // =====================================
        public static void ValidateGenericPanel(OptionPanelType type, optButton button, List<Folder> folders, string[] values, bool ismultiselect, int contentcounter, KeyValuePair<int, string> savelocation, string[] checkvalues = null, KeyValuePair<int, string> checksavelocation = new KeyValuePair<int, string>())
        {
            //Check Option Panel Type
            if (type == OptionPanelType.Add)
            {
                //Validate Generic Add Panel
                ValidateGenericAddPanel(button, folders, values, ismultiselect, contentcounter, savelocation);
            }
            else if (type == OptionPanelType.Edit)
            {
                //Validate Generic Edit Panel
                ValidateGenericEditPanel(button, folders, values, checkvalues, savelocation, checksavelocation, ismultiselect);
            }
        }


        // Validate Add Panel
        // =====================================
        // =====================================
        private static void ValidateGenericAddPanel(optButton button, List<Folder> folders, string[] values, bool ismultiselect, int contentcounter, KeyValuePair<int, string> savelocation)
        {
            //Initialize isValid Boolean
            bool isValid = false;

            //Set isValid to True (Dependent on File Path Validation)

            //Validate Save Location
            if(ValidateSaveLocation(OptionPanelType.Add, folders, savelocation))
            {
                //Validate File or Open Dialog Content Count
                if(!ismultiselect && File(values[0]) || contentcounter > 0) { isValid = true; }
            }

            //Toggle Add Button
            ToggleState.UIElement(button, isValid);
        }


        // Validate Edit Panel
        // =====================================
        // =====================================
        private static void ValidateGenericEditPanel(optButton button, List<Folder> folders, string[] values, string[] checkvalues, KeyValuePair<int, string> savelocation, KeyValuePair<int, string> checksavelocation, bool ismultiselect)
        {
            //Initialize Booleans
            bool isCustomName = false, isPath = false, isFile = false, isSaveLocation, isValid = false;

            //Validate Custom Name
            if (checkvalues[0] == values[0] || string.IsNullOrEmpty(checkvalues[0]) && string.IsNullOrEmpty(values[0])) { isCustomName = true; }

            //Validate Paths
            if (checkvalues[1] == values[1]) { isPath = true; }

            //Validate File
            if (File(checkvalues[1])) { isFile = true; }

            //Validate Save Location
            isSaveLocation = ValidateSaveLocation(OptionPanelType.Edit, folders, savelocation, checksavelocation);

            //Set isValid (Dependent on Conditional Statement)
            if ((!isCustomName && isPath && isFile && !isSaveLocation && !ismultiselect) ||
                (!isCustomName && !isPath && isFile && !isSaveLocation && !ismultiselect) ||
                (!isCustomName && isPath && isFile && isSaveLocation && !ismultiselect) ||
                (isCustomName && !isPath && isFile && isSaveLocation && !ismultiselect) ||
                (isCustomName && isPath && isFile && isSaveLocation && !ismultiselect) ||
                (isCustomName && !isPath && isFile && !isSaveLocation && !ismultiselect) ||
                (isCustomName && !isPath && isFile && !isSaveLocation && ismultiselect) ||
                (isCustomName && isPath && !isFile && isSaveLocation && ismultiselect) ||
                (isCustomName && !isPath && isFile && isSaveLocation && ismultiselect) ||
                (!isCustomName && !isPath && isFile && isSaveLocation && ismultiselect) ||
                (!isCustomName && isPath && !isFile && isSaveLocation && ismultiselect) ||
                (!isCustomName && !isPath && isFile && !isSaveLocation && ismultiselect) ||
                (isCustomName && !isPath && !isFile && !isSaveLocation && ismultiselect) ||
                (!isCustomName && isPath && !isFile && !isSaveLocation && ismultiselect) ||
                (!isCustomName && isPath && isFile && !isSaveLocation && ismultiselect))
            {
                isValid = true;
            }

            //Toggle Edit Button
            ToggleState.UIElement(button, isValid);
        }
        #endregion Generic Validation



        #region Folder Validation
        // Validate Folder Panel
        // =====================================
        // =====================================
        public static void ValidateFolderPanel(OptionPanelType type, optButton button, List<Folder> folders, Tuple<string, int, string> values, Tuple<string, int, string> checkvalues = null)
        {
            //Variables
            bool istoggle = false;

            //Validate Base Values and Set istoggle
            istoggle = (!string.IsNullOrEmpty(values.Item1) && (folders.Any(i => i.Id == values.Item2) || values.Item2 == 0) && !string.IsNullOrEmpty(values.Item3)) ? true : false;

            //Check if type is set to OptionPanelType.Edit
            if (type == OptionPanelType.Edit)
            {
                //Validate Edit and Set istoggle
                istoggle = istoggle && ((values.Item1 != checkvalues.Item1) || (values.Item2 != checkvalues.Item2) || (values.Item3 != checkvalues.Item3)) ? true : false;
            }

            //Toggle Button
            ToggleState.UIElement(button, istoggle);
        }


        // Validate Save Location
        // =====================================
        // =====================================
        public static bool ValidateSaveLocation(OptionPanelType type, List<Folder> folders, KeyValuePair<int, string> savelocation, KeyValuePair<int, string> checksavelocation = new KeyValuePair<int, string>())
        {
            //Variables
            bool isValid = false;

            //Validate Base Values and Set isValid
            isValid = ((folders.Any(i => i.Id == savelocation.Key) || savelocation.Key == 0) && (!string.IsNullOrEmpty(savelocation.Value))) ? true : false;

            //Check if type is set to OptionPanelType.Edit
            if (type == OptionPanelType.Edit)
            {
                //Validate Edit and Set istoggle
                isValid = isValid && (savelocation.Key != checksavelocation.Key) || savelocation.Value != checksavelocation.Value ? true : false;
            }

            //Return isValid
            return isValid;
        }
        #endregion Folder Validation
        #endregion Graphical User Interface Validation



        #region Process Validation
        // Process
        // =====================================
        // =====================================
        public static bool Process(string filePath, bool isFileOptional = false, bool IsLinkExpected = false, string[] baseUrls = null, string[] urls = null, int[] lengths = null, int matchcount = 0, bool isDirectoryExpected = false, string baseDirectory = "")
        {
            //Validate File Path
            bool isFileValid = File(filePath, isFileOptional);

            //Check if file exists and if a link is not expected
            //Else check if the base directory exists, if the file path is contained within the base directory, a link is expected, and the urls are valid
            if (isFileValid && !IsLinkExpected)
            {
                //Return true
                return true;
            }
            else if (isFileValid && !isDirectoryExpected && IsLinkExpected && URL(baseUrls, urls, lengths, matchcount))
            {
                //Return true
                return true;
            }
            else if(isFileValid && isDirectoryExpected && Directory(baseDirectory) && filePath.Contains(baseDirectory) && IsLinkExpected && URL(baseUrls, urls, lengths, matchcount))
            {
                //Return true
                return true;
            }

            //Return false
            return false;
        }


        // File
        // =====================================
        // =====================================
        public static bool File(string filePath, bool isFileOptional = false)
        {
            //Check if the string is not null or empty and if the file path exists
            if ((!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath)) || (isFileOptional && string.IsNullOrEmpty(filePath)))
            {
                //Return true
                return true;
            }

            //Return false
            return false;
        }


        // Directory
        // =====================================
        // =====================================
        public static bool Directory(string directory)
        {
            //Check if the string is not null or empty and if the directory path exists
            if (!string.IsNullOrEmpty(directory) && System.IO.Directory.Exists(directory))
            {
                //Return true
                return true;
            }

            //Return false
            return false;
        }

        public static bool DirectoryContainFiles(DirectoryInfo dirinfo, string[] extensions)
        {
            //Convert Extensions to HashSet
            HashSet<string> allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);

            //Check if Directory Contains Any Valid Files and Return Boolean Reperesentation
            return dirinfo.EnumerateFiles().Where(f => allowedExtensions.Contains(f.Extension)).Count() > 0 ? true : false;
        }


        // URL
        // =====================================
        // =====================================
        public static bool URL(string[] baseUrls, string[] urls, int[] lengths, int matchcount)
        {
            //Variables
            int isValid = 0;

            //Loop through values in baseUrls
            for (int i = 0; i < baseUrls.Length; i++)
            {
                //Trim white space from current looped url
                urls[i] = urls[i].Trim();

                //Run regex pattern check
                Regex pattern = new Regex(@"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=%.]+$");
                Match match = pattern.Match(urls[i]);

                //Check if the match was successful
                //If the current looped url contains the base url at the position of i
                //And if the url length is equal or longer than the length at position i
                if (match.Success == true && urls[i].Contains(baseUrls[i]) && urls[i].Length >= lengths[i])
                {
                    //Add 1 to isValid Integer
                    isValid++;
                }
            }

            //Check if urls were valid
            if (isValid >= matchcount)
            {
                //Return True
                return true;
            }

            //Return False
            return false;
        }
        #endregion Process Validation
    }
}