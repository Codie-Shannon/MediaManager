using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Windows;
using OpenQA.Selenium;
using Media_Manager.Models;
using System.Windows.Media;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using MediaInfo.DotNetWrapper.Enumerations;
using System.Text.RegularExpressions;
using System.Globalization;
using SeleniumUndetectedChromeDriver;
using System.Threading.Tasks;
using MediaControlsLibrary.Types;

namespace Media_Manager
{
    public class Fetcher
    {
        #region Variables
        // Resources
        // =================================================
        // =================================================
        // MediaInfo
        // =================================================
        private static readonly MediaInfo.DotNetWrapper.MediaInfo mediainfo = new MediaInfo.DotNetWrapper.MediaInfo();

        // Configuraiton
        // =================================================
        private static UndetectedChromeDriver DefaultDriver;
        private static UndetectedChromeDriver IMDBDriver;

        // Paths
        // =================================================
        private static string path, baseDirectory;
        #endregion Variables



        #region Configuration
        // Configure Fetcher
        // =================================================
        // =================================================
        public static async Task<bool> ConfigureFetcherAsync(MediaType type, string filepath, string basedirectory = "", string defaultlink = "", string imdblink = "")
        {
            //Set path to filepath
            path = filepath;

            //Confgiure MediaInfo Object if type is Set to MediaType.Movies or MediaType.Null
            if (type == MediaType.Movies || type == MediaType.Episodes || type == MediaType.Null)
            {
                //Configure MediaInfo Fetcher
                mediainfo.Open(filepath);
                mediainfo.Option("ParseSpeed", "0");
            }

            //Navigate to Set URLs if the Computer System is Connected to the Internet and type is Set to MediaType.Movies or MediaType.Null
            if (Internet.Validate() && (type == MediaType.Movies || type == MediaType.TVShows || type == MediaType.Episodes || type == MediaType.Games))
            {
                //Set Active Window to Topmost
                WindowsAPI.ToggleActiveWindow(true);

                //Validate and Create Default Driver
                if (!string.IsNullOrEmpty(defaultlink)) { DefaultDriver = UndetectedChromeDriver.Create(null, null, await new ChromeDriverInstaller().Auto(), null, 0, 0, false, true, true, true); }

                //Validate and Create IMDB Driver
                if (!string.IsNullOrEmpty(imdblink)) { IMDBDriver = UndetectedChromeDriver.Create(null, null, await new ChromeDriverInstaller().Auto(), null, 1, 0, false, true, true, true); }

                //Hide Chrome Driver Windows
                WindowsAPI.HideChromeDriverWindows();

                //Unset Active Window Topmost
                WindowsAPI.ToggleActiveWindow(false);

                //Navigate Default Driver to the defaultlink Variable Value
                if (!string.IsNullOrEmpty(defaultlink)) { DefaultDriver.Navigate().GoToUrl(defaultlink); }

                //Navigate IMDB Driver to the imdblink Variable Value
                if (!string.IsNullOrEmpty(imdblink)) { IMDBDriver.Navigate().GoToUrl(imdblink); }

                //Validate and Set Base Directory
                if(!string.IsNullOrEmpty(basedirectory)) { baseDirectory = basedirectory; }

                //Return True
                return true;
            }

            //Return False
            return false;
        }


        // Close
        // =================================================
        // =================================================
        public static void Close()
        {
            //Validate Default Driver
            if (DefaultDriver != null)
            {
                //Close and Unset Default Driver
                DefaultDriver.Quit();
                DefaultDriver = null;
            }

            //Validate IMDB Driver
            if(IMDBDriver != null)
            {
                //Close and Unset IMDB Driver
                IMDBDriver.Quit();
                IMDBDriver = null;
            }
        }
        #endregion Configuration



        #region Get Methods
        // Get Movie
        // =================================================
        // =================================================
        public static Movie GetMovie(Movie movie, int id)
        {
            //Validate IMDB and Metacritic Links
            bool isIMDBValid = !string.IsNullOrEmpty(movie.IMDBLink);
            bool isMetacriticValid = !string.IsNullOrEmpty(movie.MetaCriticLink);

            //Get IMDB Release
            string[] release = isIMDBValid ? GetIMDBRelease() : new string[0];

            //Get Details IWebElements
            IReadOnlyCollection<IWebElement> details = DefaultDriver != null && IsElementPresent(DriverType.Default, By.ClassName("c-movieDetails_sectionContainer")) ? DefaultDriver.FindElements(By.ClassName("c-movieDetails_sectionContainer")) : null;

            //Get Movie Details
            Movie newmovie = new Movie()
            {
                //Database ID
                Id = id,


                //Owner's Database ID
                OwnerId = movie.OwnerId,


                //File Path
                FilePath = movie.FilePath,


                //Name
                CustomName = movie.CustomName,
                Name = isIMDBValid ? GetIMDBTitle() : GetMetacriticTitle(),


                //Standard Details
                Width = GetVideoWidth(),
                Height = GetVideoHeight(),
                Duration = GetDuration(StreamKind.Video),
                Framerate = GetFramerate(),
                Format = GetFormat(),
                FileSize = GetFileSize(),
                CreationTime = GetCreationTime(),
                CreationDate = GetCreationDate(),


                //Advance Details
                SampleRate = GetSampleRate(),
                AudioChannels = GetAudioChannels(),
                FramerateMode = GetFramerateMode(),


                //Web Details
                ReleaseDate = isIMDBValid ? release[0] : Formatter.FormatDate(GetMetacriticDetail(details, "Release Date"), "MMM d, yyyy", "MMMM d, yyyy"),
                AgeRating = isIMDBValid ? GetIMDBDetail("parentalguide") : GetMetacriticDetail(details, "Rating"),
                Genres = isIMDBValid ? GetIMDBCollectionBasedOnClass("ipc-chip__text") : GetMetacriticGenres(),
                Stars = isIMDBValid ? GetIMDBCollectionBasedOnClass("sc-bfec09a1-1") : GetMetacriticStars(),
                Directors = isIMDBValid ? GetIMDBCollectionBasedOnTitle("Director") : GetMetacriticCollection("c-productDetails_staff_directors"),
                Writers = isIMDBValid ? GetIMDBCollectionBasedOnTitle("Writer") : GetMetacriticCollection("c-productDetails_staff_writers"),
                ProductionCompanies = isIMDBValid ? GetIMDBCollectionBasedOnTitle("Production compan") : GetMetacriticProductionCompanies(details),


                //IMDB Details
                IMDBLink = movie.IMDBLink,
                Region = isIMDBValid ? release[1] : string.Empty,


                //Metacritic Details
                MetaCriticLink = movie.MetaCriticLink,
                UserScore = isMetacriticValid ? GetMetacriticScore(true) : 0,
                UserReviewCount = isMetacriticValid ? GetMetacriticReviewCount(true) : 0,
                CriticScore = isMetacriticValid ? GetMetacriticScore(false) : 0,
                CriticReviewCount = isMetacriticValid ? GetMetacriticReviewCount(false) : 0,


                //Cover Image
                CoverImage = isIMDBValid ? GetIMDBCover(id) : string.Empty
            };

            //Set Foreground Window
            WindowsAPI.SetWindowFocus();

            //Return New Movie
            return newmovie;
        }


        // Get TV Show Folder
        // =================================================
        // =================================================
        public static TVShowFolder GetTVShowFolderAsync(TVShowFolder folder, int id)
        {
            //Validate IMDB and Metacritic Links
            bool isIMDBValid = !string.IsNullOrEmpty(folder.IMDBLink);
            bool isMetacriticValid = !string.IsNullOrEmpty(folder.MetaCriticLink);

            //Get IMDB Release
            string[] release = isIMDBValid ? GetIMDBRelease() : new string[0];

            //Get Details IWebElements
            IReadOnlyCollection<IWebElement> details = DefaultDriver != null && IsElementPresent(DriverType.Default, By.ClassName("c-productionDetailsTv_sectionContainer")) ? DefaultDriver.FindElements(By.ClassName("c-productionDetailsTv_sectionContainer")) : null;

            //Get TV Show Details
            TVShowFolder newtvshowfolder = new TVShowFolder()
            {
                //Database ID
                Id = id,


                //Owner's Database ID
                OwnerId = folder.OwnerId,


                //Type
                Type = nameof(MediaType.TVShows),
                FolderType = nameof(FolderType.TVShowFolders),


                //Name
                CustomName = folder.CustomName,
                Name = isIMDBValid ? GetIMDBTitle() : GetMetacriticTitle(),


                //Custom Cover Image
                CustomCoverImage = GetCustomCoverImage(MediaType.TVShows, id, folder.CustomCoverImage, "custom"),


                //Standard Details
                CreationTime = GetCreationTime(string.Empty, true),
                CreationDate = GetCreationDate(string.Empty, true),


                //MetaCritic Details
                MetaCriticLink = folder.MetaCriticLink,
                UserScore = isMetacriticValid ? GetMetacriticScore(true, true) : 0,
                UserReviewCount = isMetacriticValid ? GetMetacriticReviewCount(true, true) : 0,
                CriticScore = isMetacriticValid ? GetMetacriticScore(false, true) : 0,
                CriticReviewCount = isMetacriticValid ? GetMetacriticReviewCount(false, true) : 0,


                //Web Details
                ReleaseDate = isIMDBValid ? release[0] : Formatter.FormatDate(GetMetacriticDetail(details, "Release Date"), "MMM d, yyyy", "MMMM d, yyyy"),
                AgeRating = isIMDBValid ? GetIMDBDetail("parentalguide") : GetMetacriticDetail(details, "Rating"),
                ProductionCompanies = isIMDBValid ? GetIMDBCollectionBasedOnTitle("Production compan") : GetMetacriticProductionCompanies(details),
                SeasonCount = isIMDBValid ? GetSeasonCount(true) : GetSeasonCount(false),
                EpisodeCount = isIMDBValid ? GetIMDBEpisodeCount() : GetMetacriticEpisodeCount(folder.MetaCriticLink),
                Genres = isIMDBValid ? GetIMDBCollectionBasedOnClass("ipc-chip__text") : GetMetacriticGenres(),
                Stars = isIMDBValid ? GetIMDBCollectionBasedOnClass("sc-bfec09a1-1") : GetMetacriticStars(),
                ReleasePeriod = isIMDBValid ? GetIMDBDetail("releaseinfo") : GetMetacriticReleasePeriod(),


                //IMDB Details
                IMDBLink = folder.IMDBLink,
                Region = isIMDBValid ? release[1] : string.Empty,
                Creators = isIMDBValid ? GetIMDBCollectionBasedOnTitle("Creator") : null,
                CoverImage = isIMDBValid ? GetIMDBCover(id, MediaType.TVShows) : string.Empty
            };

            //Set Foreground Window
            WindowsAPI.SetWindowFocus();

            //Return New TV Show Folder
            return newtvshowfolder;
        }


        // Get Season Folder
        // =================================================
        // =================================================
        public static SeasonFolder GetSeasonFolder(SeasonFolder folder, int id, string tvshowcustomcoverimage)
        {
            //Validate IMDB Link
            bool isIMDBValid = string.IsNullOrEmpty(folder.IMDBLink) ? false : true;

            //Get Season Folder
            SeasonFolder newseasonfolder = new SeasonFolder()
            {
                //Database ID
                Id = id,


                //Owner's Database ID
                OwnerId = folder.OwnerId,


                //Type
                Type = nameof(MediaType.TVShows),
                FolderType = nameof(FolderType.SeasonFolders),


                //Name
                Name = $"Season {folder.SeasonNumber}",


                //Cover Image
                CoverImage = folder.CoverImage,
                CustomCoverImage = string.IsNullOrEmpty(folder.CustomCoverImage) ? tvshowcustomcoverimage : GetCustomCoverImage(MediaType.Seasons, folder.OwnerId, folder.CustomCoverImage, $"{folder.SeasonNumber} custom"),


                //File Path
                FilePath = folder.FilePath,


                //Standard Details
                SeasonNumber = folder.SeasonNumber,
                CreationTime = GetCreationTime(string.Empty, true),
                CreationDate = GetCreationDate(string.Empty, true),


                //Web Details
                ReleaseDate = isIMDBValid ? GetIMDBSeasonReleaseDate() : GetMetacriticDate(),
                EpisodeCount = isIMDBValid ? GetIMDBSeasonEpisodeCount() : GetMetacriticSeasonEpisodeCount(),


                //IMDB Details
                IMDBLink = folder.IMDBLink,


                //MetaCritic Details
                MetaCriticLink = folder.MetaCriticLink
            };

            //Set Foreground Window
            WindowsAPI.SetWindowFocus();

            //Return newseasonfolder
            return newseasonfolder;
        }


        // Get Episode
        // =================================================
        // =================================================
        public static Episode GetEpisode(TVShowFolder folder, Episode episode, int id, int episodenumber, string imdblink, string metacritic)
        {
            //Get Width and Height of Video
            string width = GetVideoWidth(), height = GetVideoHeight();

            //Get IMDB Release
            string[] release = GetIMDBRelease();

            //Validate IMDB Link
            bool isIMDBValid = string.IsNullOrEmpty(imdblink) ? false : true;

            //Get Episode
            Episode newepisode = new Episode()
            {
                //Database ID
                Id = id,


                //Owner's Database ID
                OwnerId = episode.OwnerId,


                //Name
                CustomName = episode.CustomName,
                Name = isIMDBValid ? GetIMDBTitle() : GetMetacriticEpisodeTitle(),


                //Cover Image
                CoverImage = episode.CoverImage,


                //File Path
                FilePath = episode.FilePath,


                //Standard Details
                Season = episode.Season,
                EpisodeNumber = episodenumber,
                Width = width,
                Height = height,
                Duration = GetDuration(StreamKind.Video),
                Framerate = GetFramerate(),
                Format = GetFormat(),
                FileSize = GetFileSize(),
                CreationTime = GetCreationTime(),
                CreationDate = GetCreationDate(),


                //Advance Details
                SampleRate = GetSampleRate(),
                AudioChannels = GetAudioChannels(),
                FramerateMode = GetFramerateMode(),


                //Web Details
                AirDate = isIMDBValid ? release[0] : Formatter.FormatDate(GetMetacriticDate(), "MMM d, yyyy", "MMMM d, yyyy"),
                Region = isIMDBValid ? release[1] : string.Empty,
                AgeRating = folder.AgeRating,
                Directors = isIMDBValid ? GetIMDBCollectionBasedOnTitle("Director") : GetMetacriticCollection("c-productDetails_staff_directors"),
                Writers = isIMDBValid ? GetIMDBCollectionBasedOnTitle("Writer") : GetMetacriticCollection("c-productDetails_staff_writers"),
                ProductionCompanies = folder.ProductionCompanies,


                //IMDB Details
                IMDBLink = imdblink,
                Genres = isIMDBValid ? GetIMDBCollectionBasedOnClass("ipc-chip__text") : null,
                Stars = isIMDBValid ? GetIMDBCollectionBasedOnClass("sc-bfec09a1-1") : null,


                //Metacritic Details
                MetaCriticLink = metacritic
            };

            //Set Foreground Window
            WindowsAPI.SetWindowFocus();

            //Return newepisode
            return newepisode;
        }


        // Get Video
        // =================================================
        // =================================================
        public static Video GetVideo(int id, Video video)
        {
            //Get Width and Height of Video
            string width = GetVideoWidth(), height = GetVideoHeight();


            //Get Video Details
            return new Video()
            {
                //Database ID
                Id = id,


                //Owner's Database ID
                OwnerId = video.OwnerId,


                //File Path
                FilePath = path,


                //Cover Image
                CoverImage = GetVideoPreview(path, id, width, height),


                //Name
                CustomName = video.CustomName,
                Name = GetFileName(),


                //Standard Details
                Width = width,
                Height = height,
                Duration = GetDuration(StreamKind.Video),
                Framerate = GetFramerate(),
                Format = GetFormat(),
                FileSize = GetFileSize(),
                CreationTime = GetCreationTime(),
                CreationDate = GetCreationDate(),


                //Advance Details
                SampleRate = GetSampleRate(),
                AudioChannels = GetAudioChannels(),
                FramerateMode = GetFramerateMode()
            };
        }


        // Get Picture
        // =================================================
        // =================================================
        public static Picture GetPicture(int id, Picture picture)
        {
            //Get Width and Height of Picture
            string width = GetPictureWidth(); string height = GetPictureHeight();


            //Get Picture Size
            System.Drawing.Size size = new System.Drawing.Size(Convert.ToInt32(width), Convert.ToInt32(height));


            //Get Picture Details
            return new Picture()
            {
                //Database ID
                Id = id,


                //Owner's Database ID
                OwnerId = picture.OwnerId,


                //File Path
                FilePath = path,


                //Cover Image
                CoverImage = GetPicturePreview(path, id, size),


                //Name
                CustomName = picture.CustomName,
                Name = GetFileName(),


                //Standard Details
                Width = width,
                Height = height,
                Format = GetFormat(),
                FileSize = GetFileSize(),
                CreationTime = GetCreationTime(),
                CreationDate = GetCreationDate(),


                //Advance Details
                ColourSpace = GetColourSpace(),
                BitDepth = GetBitDepth(),
                CompMode = GetCompMode(StreamKind.Image)
            };
        }


        // Get Song
        // =================================================
        // =================================================
        public static Song GetSong(int id, Song song)
        {
            //Get Song Details
            return new Song()
            {
                //Database ID
                Id = id,


                //Owner's Database ID
                OwnerId = song.OwnerId,


                //File Path
                FilePath = path,


                //Cover Image
                CoverImage = string.Empty,


                //Name
                Name = GetFileName(),


                //Standard Details
                Duration = GetDuration(StreamKind.Audio),
                Format = GetFormat(),
                FileSize = GetFileSize(),
                CreationTime = GetCreationTime(),
                CreationDate = GetCreationDate(),


                //Advance Details
                SampleRate = GetSampleRate(),
                AudioChannels = GetAudioChannels(),
                CompMode = GetCompMode(StreamKind.Audio)
            };
        }


        // Get Game
        // =================================================
        // =================================================
        public static Game GetGame(int id, Game game)
        {
            //Get Game Ratings
            List<KeyValuePair<string, int>> ratings = GetGameRatings();

            //Get and Return Game Data
            return new Game()
            {
                //Database ID
                Id = id,


                //Owner's Database ID
                OwnerId = game.OwnerId,


                //Paths
                FilePath = game.FilePath,
                BaseDirectory = game.BaseDirectory,


                //Cover Image
                CoverImage = DownloadImage(MediaType.Games, id, GetGenericGameDetail("cover_big", true, "src")),


                //Name
                CustomName = game.CustomName,
                Name = GetGenericGameDetail("banner-title", false).Replace("Edit", ""),


                //Standard Details
                Format = GetFormat(),
                FileSize = GetFileSize(baseDirectory),
                CreationTime = GetCreationTime(baseDirectory),
                CreationDate = GetCreationDate(baseDirectory),
                IGDBLink = game.IGDBLink,


                //IGDB Details
                Publisher = GetGenericGameDetail("h3"),
                ReleaseDate = GetGenericGameDetail("h2").Split('(').ElementAt(0),
                Type = game.Type,
                UserScore = ratings.Single(i => i.Key == "UserScore").Value,
                UserReviewCount = ratings.Single(i => i.Key == "UserReviewCount").Value,
                CriticScore = ratings.Single(i => i.Key == "CriticScore").Value,
                CriticReviewCount = ratings.Single(i => i.Key == "CriticReviewCount").Value,
                Genres = GetGenericGameCollection("genre"),
                AvailablePlatforms = GetGenericGameCollection("platform")
            };
        }
        #endregion Get Methods



        #region Update
        // Update Song
        // =================================================
        // =================================================
        public static Song UpdateSong(Song selectedsong, Song song)
        {
            //Update Song
            selectedsong.CustomName = song.CustomName;
            selectedsong.CoverImage = GetMusicCover(song.CoverImage, selectedsong.CoverImage, selectedsong.Id);
            selectedsong.OwnerId = song.OwnerId;

            //Return Updated Song
            return selectedsong;
        }
        #endregion Update



        #region Main
        // Download Image
        // =======================================================
        // =======================================================
        private static string DownloadImage(MediaType type, int id, string cover, string extension = "")
        {
            //Initialize Save Locations
            string tempSaveLocation = $"{Properties.Settings.Default[type.ToString()]}\\{id}_temp.png";
            string saveLocation = $"{Properties.Settings.Default[type.ToString()]}\\{id} {extension}.png".Trim();

            //Create WebClient Object
            using (WebClient client = new WebClient())
            {
                //Download Temporary Image File
                client.DownloadFile(cover, tempSaveLocation);
            }

            //Get Image
            GetImage(tempSaveLocation, saveLocation, new System.Drawing.Size(199, 291), false, System.Drawing.Size.Empty);

            //Return Save Location
            return saveLocation;
        }


        // Get Image
        // =======================================================
        // =======================================================
        private static void GetImage(string templocation, string savelocation, System.Drawing.Size size, bool isdesiredsize, System.Drawing.Size desiredsize)
        {
            //Initialize Bitmap Image
            Bitmap image = new Bitmap(templocation);

            //Resize Image
            image = ResizeImage(image, size, isdesiredsize, desiredsize);

            //Save Image
            image.Save(savelocation);

            //Dispose of Bitmap Image
            image.Dispose();
        }

        // Source 1: https://stackoverflow.com/a/10839428 || ================= || Source 2: https://stackoverflow.com/a/2001692
        private static Bitmap ResizeImage(Bitmap image, System.Drawing.Size size, bool isdesiredsize, System.Drawing.Size desiredsize)
        {
            //Check if Desired Size has been Set
            if (isdesiredsize == true)
            {
                //Get Ratio
                decimal wratio = (decimal)size.Width / desiredsize.Width;
                decimal hratio = (decimal)size.Height / desiredsize.Height;

                //Get Multiplier
                decimal ratio = wratio > hratio ? wratio : hratio;

                //Get New Size
                size = new System.Drawing.Size((int)(size.Width / ratio), (int)(size.Height / ratio));
            }
            
            //Initialize Bitmap with Set Size
            Bitmap bmp = new Bitmap(size.Width, size.Height);

            //Open Drawing Interface
            using (Graphics g = Graphics.FromImage(bmp))
            {
                //Set Interpolation Mode to High Quality
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //Draw Image to Size
                g.DrawImage(image, 0, 0, size.Width, size.Height);
            }

            //Return Result
            return bmp;
        }


        // Get Custom Cover Image
        // =======================================================
        // =======================================================
        public static string GetCustomCoverImage(MediaType type, int id, string cover, string extension = "")
        {
            //Validate Cover
            if (!string.IsNullOrEmpty(cover))
            {
                //Download Image
                return DownloadImage(type, id, cover, extension);
            }

            //Return Empty String
            return string.Empty;
        }


        // Name
        // =======================================================
        // =======================================================
        private static string GetFileName()
        {
            //Get and Return File Name
            return Path.GetFileNameWithoutExtension(path);
        }


        // Picture Resolution
        // =======================================================
        // =======================================================
        private static string GetPictureWidth()
        {
            //Get and Return Width of Picture
            return mediainfo.Get(StreamKind.Image, 0, "Width");
        }

        private static string GetPictureHeight()
        {
            //Get and Return Height of Picture
            return mediainfo.Get(StreamKind.Image, 0, "Height");
        }


        // Format
        // =======================================================
        // =======================================================
        private static string GetFormat()
        {
            //Get, Format, and Return File Format
            return Path.GetExtension(path).Replace(".", "").ToUpper();
        }


        // File Size
        // =======================================================
        // =======================================================
        private static double GetFileSize(string directory = "")
        {
            //Get File Size
            string size = string.IsNullOrEmpty(directory) ? mediainfo.Get(StreamKind.General, 0, "FileSize") : $"{new DirectoryInfo(directory).EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length)}";

            //Try Convert File Size to Double
            double.TryParse(size, out double filesize);

            //Return File Size
            return filesize;
        }


        // Creation Time and Date
        // =======================================================
        // =======================================================
        private static string GetCreationTime(string directory = "", bool ispresent = false)
        {
            //Get and Return Creation Time
            return !ispresent ? (string.IsNullOrEmpty(directory) ? File.GetCreationTime(path) : Directory.GetCreationTime(directory)).ToString("HH-mm-ss") : DateTime.Now.ToString("HH-mm-ss");
        }

        private static string GetCreationDate(string directory = "", bool ispresent = false)
        {
            //Get and Return Creation Date
            return !ispresent ? (string.IsNullOrEmpty(directory) ? File.GetCreationTime(path) : Directory.GetCreationTime(directory)).ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
        }


        // Compression Mode
        // =======================================================
        // =======================================================
        private static string GetCompMode(StreamKind kind)
        {
            //Get and Return the Compression Mode
            return mediainfo.Get(kind, 0, "Compression_Mode");
        }
        #endregion Main



        #region Extensions
        // Extensions
        // =================================================
        // =================================================
        private static bool IsElementPresent(DriverType type, By by, IWebElement parent = null)
        {
            //Run Try Statement
            try
            {
                //Find Element
                _ = parent != null ? parent.FindElement(by) : (type == DriverType.Default ? DefaultDriver.FindElement(by) : IMDBDriver.FindElement(by));

                //Return Valid
                return true;
            }
            catch (NoSuchElementException)
            {
                //Return Invalid
                return false;
            }
        }
        #endregion Extensions



        #region Picture
        // Picture Preview
        // ======================================
        // ======================================
        private static string GetPicturePreview(string filepath, int id, System.Drawing.Size size)
        {
            //Initialize Save Locations
            string tempLocation = $"{Properties.Settings.Default.Pictures}{id}_temp.png";
            string saveLocation = $"{Properties.Settings.Default.Pictures}{id}.png";

            //Copy Image to Application Folder
            File.Copy(filepath, tempLocation, true);

            //Get Image
            GetImage(tempLocation, saveLocation, size, true, new System.Drawing.Size(401, 230));

            //Return Save Location
            return saveLocation;
        }



        // Advance Details
        // ======================================
        // ======================================
        // Colour Space
        private static string GetColourSpace()
        {
            //Get and Return Colour Space
            return mediainfo.Get(StreamKind.Image, 0, "ColorSpace");
        }


        // Bit Depth
        private static string GetBitDepth()
        {
            //Get and Return Bit Depth
            return mediainfo.Get(StreamKind.Image, 0, "BitDepth");
        }


        // Compression Mode
        private static string GetCompMode()
        {
            //Get and Return the Compression Mode
            return mediainfo.Get(StreamKind.Image, 0, "Compression_Mode");
        }
        #endregion Picture



        #region AV
        // Standard Details
        // ======================================
        // ======================================
        // Duration
        private static double GetDuration(StreamKind kind)
        {
            try
            {
                //Get and Return Video Duration
                return Convert.ToDouble(mediainfo.Get(kind, 0, "Duration"));
            }
            catch
            {
                //
                return 0;
            }
        }



        // Advance Details
        // ======================================
        // ======================================
        // Sample Rate
        private static double GetSampleRate()
        {
            //Get Sample Rate
            string value = mediainfo.Get(StreamKind.Audio, 0, "SamplingRate");

            //Check if Sample Rate is Extended and Get Value
            value = value.Contains(" ") ? value.Split(' ').ElementAt(0) : value;

            //Validate, Convert and Return Sample Rate
            return !string.IsNullOrEmpty(value) ? Convert.ToDouble(value) : 0;
        }

        // Audio Channels
        private static string GetAudioChannels()
        {
            //Initialize channels Variable
            string channels = mediainfo.Get(StreamKind.Audio, 0, "Channels");

            //Check if mediainfo did not locate any audio channels
            if (string.IsNullOrEmpty(channels) || channels == "0")
            {
                //Return Empty String
                return string.Empty;
            }

            //Return channels Variable
            return channels;
        }
        #endregion AV



        #region Video
        // Video Preview
        // ======================================
        // ======================================
        private static string GetVideoPreview(string filepath, int id, string _width, string _height)
        {
            //Initialize Save Locations
            string tempLocation = $"{Properties.Settings.Default.Videos}{id}_temp.png";
            string saveLocation = $"{Properties.Settings.Default.Videos}{id}.png";

            //Convert Width and Height to Integers
            int.TryParse(_width, out int width);
            int.TryParse(_height, out int height);

            //Create Media Player Object and Set Volume to 0 and ScrubbingEnabled to true
            MediaPlayer player = new MediaPlayer { Volume = 0, ScrubbingEnabled = true };

            //Open Media Player Object with filepath Variable
            player.Open(new Uri(filepath));

            //Pause Media Player Object
            player.Pause();

            //Set Position of Media Player Object to 0 Seconds
            player.Position = TimeSpan.FromSeconds(0);

            //Wait for Media Player Object to Load
            System.Threading.Thread.Sleep(1 * 1000);

            //Draw Visual
            DrawingVisual drawVisual = new DrawingVisual();

            //Set Size of Media Player Object
            DrawingContext drawingContext = drawVisual.RenderOpen();
            drawingContext.DrawVideo(player, new Rect(0, 0, width, height));
            drawingContext.Close();

            //Render Video Frame as Bitmap
            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 1 / width, 1 / height, PixelFormats.Pbgra32);
            bmp.Render(drawVisual);

            //Create PngBitmapEncodeer Object
            PngBitmapEncoder png = new PngBitmapEncoder();

            //Add bmp to the PngBitmapEncoder Frames Collection
            png.Frames.Add(BitmapFrame.Create(bmp));

            //Open Stream
            using (Stream stm = File.Create(tempLocation))
            {
                //Save Temporary Image
                png.Save(stm);
            }

            //Close Media Player Object
            player.Close();

            //Resize Image
            GetImage(tempLocation, saveLocation, new System.Drawing.Size(width, height), true, new System.Drawing.Size(401, 230));

            //Return Save Location
            return saveLocation;
        }



        // Standard Details
        // ======================================
        // ======================================
        // Resolution
        private static string GetVideoWidth()
        {
            //Get and Return Width of Video
            return mediainfo.Get(StreamKind.Video, 0, "Width");
        }

        private static string GetVideoHeight()
        {
            //Get and Return Height of Video
            return mediainfo.Get(StreamKind.Video, 0, "Height");
        }

        // Framerate
        private static double GetFramerate()
        {
            //Get Framerate
            string framerate = mediainfo.Get(StreamKind.Video, 0, "FrameRate");

            //Validate, Convert and Return Framerate
            return !string.IsNullOrEmpty(framerate) ? Convert.ToDouble(mediainfo.Get(StreamKind.Video, 0, "FrameRate")) : 0;
        }



        // Advance Details
        // ======================================
        // ======================================
        // Framerate Mode
        private static string GetFramerateMode()
        {
            //Get and Return Framerate Mode
            return mediainfo.Get(StreamKind.Video, 0, "FrameRate_Mode");
        }
        #endregion Video



        #region Music
        // Music Cover
        // ======================================
        // ======================================
        private static string GetMusicCover(string filepath, string oldfilepath, int id)
        {
            //Validate File Path
            if (filepath != oldfilepath)
            {
                //Validate Music Cover
                if (Validation.File(filepath))
                {
                    //Initialize Save Locations
                    string tempLocation = $"{Properties.Settings.Default.Music}{id}_temp.png";
                    string saveLocation = $"{Properties.Settings.Default.Music}{id}.png";

                    //Configure MediaInfo Fetcher
                    mediainfo.Open(filepath);
                    mediainfo.Option("ParseSpeed", "0");

                    //Get Width and Height of Cover
                    string width = GetPictureWidth(); string height = GetPictureHeight();

                    //Get Cover Size
                    System.Drawing.Size size = new System.Drawing.Size(Convert.ToInt32(width), Convert.ToInt32(height));

                    //Copy Image to Application Folder
                    File.Copy(filepath, tempLocation, true);

                    //Get Image
                    GetImage(tempLocation, saveLocation, size, true, new System.Drawing.Size(401, 230));

                    //Return Save Location
                    return saveLocation;
                }
                else
                {
                    //Delete Old Cover
                    File.Delete(oldfilepath);
                }
            }

            //Return Empty String
            return string.Empty;
        }
        #endregion Music



        #region Virtual Entertainment
        #region IMDB
        // Methods
        // ======================================
        // ======================================
        // Title
        private static string GetIMDBTitle()
        {
            //Initialize Variable
            string title = string.Empty;

            //Validate Parent
            if (IsElementPresent(DriverType.IMDB, By.ClassName("sc-afe43def-0")))
            {
                //Get Parent IWebElement
                IWebElement parent = IMDBDriver.FindElement(By.ClassName("sc-afe43def-0"));

                //Validate and Get Title
                title = IsElementPresent(DriverType.Null, By.TagName("span"), parent) ? parent.FindElement(By.TagName("span")).Text : string.Empty;
            }

            //Return title Variable
            return title;
        }


        // Release
        private static string[] GetIMDBRelease()
        {
            //Validate List Item Titles
            if (IsElementPresent(DriverType.IMDB, By.ClassName("ipc-metadata-list__item")))
            {
                //Get List Item Titles
                ReadOnlyCollection<IWebElement> titles = IMDBDriver.FindElements(By.ClassName("ipc-metadata-list__item"));

                //Validate Release Date Parent
                if (titles.Any(i => i.GetAttribute("data-testid") == "title-details-releasedate"))
                {
                    //Get Release Date Parent
                    IWebElement parent = titles.Single(i => i.GetAttribute("data-testid") == "title-details-releasedate");

                    //Validate Release Date Elements
                    if (IsElementPresent(DriverType.Null, By.TagName("a"), parent))
                    {
                        //Get Release Date Elements
                        IEnumerable<IWebElement> elements = parent.FindElements(By.TagName("a")).Where(i => i.Text != string.Empty);

                        //Get Release
                        string[] release = elements.Last().Text.Split('(').ToArray();

                        //Format Release Region
                        release[1] = release[1].Replace(")", "");

                        //Return Release
                        return release;
                    }
                }
            }

            //Return Empty Array
            return Array.Empty<string>();
        }


        // Movie Cover
        private static string GetIMDBCover(int id, MediaType mediatype = MediaType.Movies)
        {
            //Validate Parent
            if (IsElementPresent(DriverType.IMDB, By.ClassName("ipc-poster")))
            {
                //Get Parent IWebElement
                IWebElement parent = IMDBDriver.FindElement(By.ClassName("ipc-poster"));

                //Validate and Get Cover Link
                string coverlink = IsElementPresent(DriverType.Null, By.TagName("a"), parent) ? parent.FindElements(By.TagName("a")).Last().GetAttribute("href") : string.Empty;

                //Navigate to Cover
                IMDBDriver.Navigate().GoToUrl(coverlink);

                //Get Cover
                string cover = IsElementPresent(DriverType.IMDB, By.ClassName("sc-7c0a9e7c-0")) ? IMDBDriver.FindElement(By.ClassName("sc-7c0a9e7c-0")).GetAttribute("src") : string.Empty;

                //Download Image and Return Save Location
                return DownloadImage(mediatype, id, cover);
            }

            //Return Empty String
            return string.Empty;
        }



        // Extensions
        // ======================================
        // ======================================
        // IMDB Detail
        private static string GetIMDBDetail(string contentname)
        {
            //Initialize Variable
            string detail = string.Empty;

            //Validate Parent
            if (IsElementPresent(DriverType.IMDB, By.ClassName("sc-afe43def-4")))
            {
                //Get Parent IWebElement
                IWebElement parent = IMDBDriver.FindElement(By.ClassName("sc-afe43def-4"));

                //Validate A Tag IWebElements
                if (IsElementPresent(DriverType.Null, By.TagName("a"), parent))
                {
                    //Get A Tag IWebElements within Parent
                    ReadOnlyCollection<IWebElement> elements = parent.FindElements(By.TagName("a"));

                    //Validate and Get Detail
                    detail = elements.Any(i => i.GetAttribute("href").Contains(contentname)) ? elements.Single(i => i.GetAttribute("href").Contains(contentname)).Text : string.Empty;

                    //Format Detail
                    detail = contentname == "releaseinfo" ? detail.Replace("–", " - ") : detail;
                }
            }

            //Return detail Variable
            return detail;
        }


        // Get IMDB Collection Based on Class
        private static List<string> GetIMDBCollectionBasedOnClass(string classname)
        {
            //Initialize List
            List<string> collection = new List<string>();

            //Validate Collection
            if (IsElementPresent(DriverType.IMDB, By.ClassName(classname)))
            {
                //Get Collection IWebElements
                ReadOnlyCollection<IWebElement> collectionitems = IMDBDriver.FindElements(By.ClassName(classname));

                //Loop through Collection Items IWebElements
                foreach (IWebElement item in collectionitems)
                {
                    //Add Collection Items to List
                    collection.Add(item.Text);
                }
            }

            //Remove All Empty Values from Collection and Return It
            return collection.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
        }


        // Get IMDB Collection Based on Title
        private static List<string> GetIMDBCollectionBasedOnTitle(string collectionname, string collectionamealt = "")
        {
            //Initialize List
            List<string> collection = new List<string>();

            //Validate Title IWebElements
            if (IsElementPresent(DriverType.IMDB, By.ClassName("ipc-metadata-list-item__label")))
            {
                //Get Title IWebElements
                ReadOnlyCollection<IWebElement> elements = IMDBDriver.FindElements(By.ClassName("ipc-metadata-list-item__label"));

                //Check if Title Exists within elements ReadOnlyCollection
                if (elements.Any(i => i.Text.ToLower().Contains(collectionname.ToLower())))
                {
                    //Get Parent IWebElement
                    IWebElement parent = elements.FirstOrDefault(i => i.Text.ToLower().Contains(collectionname.ToLower())).FindElement(By.XPath(".."));

                    //Validate Collection Items
                    if (IsElementPresent(DriverType.Null, By.TagName("a"), parent))
                    {
                        //Get Collection Items
                        ReadOnlyCollection<IWebElement> collectionitems = parent.FindElements(By.TagName("a"));

                        //Loop through Collection Items
                        foreach (IWebElement collectionitem in collectionitems)
                        {
                            //Validate Collection Item
                            if (!collectionitem.Text.Contains(collectionname))
                            {
                                //Add Collection Item to List
                                collection.Add(collectionitem.Text);
                            }
                        }
                    }
                }
            }

            //Remove All Empty Values from Collection and Return It
            return collection.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
        }
        #endregion IMDB



        #region Metacritic
        // Methods
        // ======================================
        // ======================================
        // Title
        private static string GetMetacriticTitle()
        {
            //Initialize Variable
            string title = string.Empty;

            //Validate Parent
            if (IsElementPresent(DriverType.Default, By.ClassName("c-productHero_title")))
            {
                //Get Parent IWebElement
                IWebElement parent = DefaultDriver.FindElement(By.ClassName("c-productHero_title"));

                //Validate and Get Title
                title = IsElementPresent(DriverType.Null, By.TagName("div"), parent) ? parent.FindElement(By.TagName("div")).Text : string.Empty;
            }

            //Return title Variable
            return title;
        }


        // Date
        private static string GetMetacriticDate()
        {
            //Initialize Variable
            string date = string.Empty;

            //Validate Parent IWebElement
            if (IsElementPresent(DriverType.Default, By.ClassName("c-productHero_score-container")))
            {
                //Get Parent IWebElement
                IWebElement parent = DefaultDriver.FindElement(By.ClassName("c-productHero_score-container"));

                //Validate Container IWebElement
                if (IsElementPresent(DriverType.Null, By.ClassName("g-text-xsmall"), parent))
                {
                    //Get Container IWebElement
                    IWebElement containerelement = parent.FindElements(By.ClassName("g-text-xsmall")).Single(i => i.Text.ToLower().Contains("premiere") || i.Text.ToLower().Contains("air"));

                    //Get Date
                    date = containerelement.FindElement(By.ClassName("u-text-uppercase")).Text;
                }
            }

            //Return date Variable
            return date;
        }


        // Get Score
        private static float GetMetacriticScore(bool isuser, bool istvshow = false)
        {
            //Initialize Variables
            float score = 0;
            int index = istvshow ? 4 : 2;
            string xpathstart = $"/html/body/div[1]/div/div/div[2]/div[1]/div[1]/div/div/div[2]/div[3]/div[{index}]/";
            string xpath = xpathstart + (!isuser ? "div[1]/div/div[1]/div[2]/div/div/span" : "div[2]/div[1]/div[2]/div/div/span");

            //Validate Score
            if (IsElementPresent(DriverType.Default, By.XPath(xpath)))
            {
                //Get Score
                string value = DefaultDriver.FindElement(By.XPath(xpath)).Text;

                //Parse Score to Float
                float.TryParse(value, out score);
            }

            //Return score Variable
            return score;
        }


        // Get Review Count
        private static int GetMetacriticReviewCount(bool isuser, bool istvshow = false)
        {
            //Initialize Variables
            int counter = 0;
            int index = istvshow ? 4 : 2;
            string xpathstart = $"/html/body/div[1]/div/div/div[2]/div[1]/div[1]/div/div/div[2]/div[3]/div[{index}]/";
            string xpath = xpathstart + (!isuser ? "div[1]/div/div[1]/div[1]/span[3]/a/span" : "div[2]/div[1]/div[1]/span[3]/a/span");

            //Validate Parent
            if (IsElementPresent(DriverType.Default, By.XPath(xpath)))
            {
                //Get IWebElement
                IWebElement element = DefaultDriver.FindElement(By.XPath(xpath));

                //Get Review Count Value
                string value = element.Text;

                //Remove All Alphabetic Characters from Review Count and Convert to Integer
                counter = Convert.ToInt32(Regex.Replace(value, "[^0-9.]", ""));
            }

            //Return Counter
            return counter;
        }


        // Genres
        private static List<string> GetMetacriticGenres()
        {
            //Initialize Lists
            List<KeyValuePair<string, string>> genretypes = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("action", "Action"),
                new KeyValuePair<string, string>("adventure", "Adventure"),
                new KeyValuePair<string, string>("sci---fi", "Sci-Fi"),
                new KeyValuePair<string, string>("animation", "Animation"),
                new KeyValuePair<string, string>("biography", "Biography"),
                new KeyValuePair<string, string>("comedy", "Comedy"),
                new KeyValuePair<string, string>("crime", "Crime"),
                new KeyValuePair<string, string>("documentary", "Documentary"),
                new KeyValuePair<string, string>("drama", "Drama"),
                new KeyValuePair<string, string>("family", "Family"),
                new KeyValuePair<string, string>("fantasy", "Fantasy"),
                new KeyValuePair<string, string>("film---noir", "Film-Noir"),
                new KeyValuePair<string, string>("music", "Music"),
                new KeyValuePair<string, string>("game---show", "Game-Show"),
                new KeyValuePair<string, string>("history", "History"),
                new KeyValuePair<string, string>("horror", "Horror"),
                new KeyValuePair<string, string>("musical", "Musical"),
                new KeyValuePair<string, string>("mystery", "Mystery"),
                new KeyValuePair<string, string>("news", "News"),
                new KeyValuePair<string, string>("reality---tv", "Reality-TV"),
                new KeyValuePair<string, string>("romance", "Romance"),
                new KeyValuePair<string, string>("short", "Short"),
                new KeyValuePair<string, string>("sport", "Sport"),
                new KeyValuePair<string, string>("talk---show", "Talk-Show"),
                new KeyValuePair<string, string>("thriller", "Thriller"),
                new KeyValuePair<string, string>("war", "War"),
                new KeyValuePair<string, string>("western", "Western")
            };
            List<string> genres = new List<string>();

            //Validate IWebElements
            if (IsElementPresent(DriverType.Default, By.ClassName("c-globalButton_container")))
            {
                //Get IWebElements
                IReadOnlyCollection<IWebElement> collection = DefaultDriver.FindElements(By.ClassName("c-globalButton_container"));

                //Get Unique IWebElements
                List<IWebElement> elements = collection.GroupBy(i => i.GetAttribute("href")).Select(i => i.First()).ToList();

                //Loop through IWebElements
                foreach (IWebElement element in elements)
                {
                    //Validate Genre
                    if (genretypes.Any(i => string.IsNullOrEmpty(element.GetAttribute("href")) ? false : element.GetAttribute("href").Contains(i.Key)))
                    {
                        //Get Genre
                        KeyValuePair<string, string> genre = genretypes.Single(i => element.GetAttribute("href").Contains(i.Key));

                        //Add Genre to genres List
                        genres.Add(genre.Value);
                    }
                }
            }

            //Return genres List
            return genres;
        }


        // Stars
        private static List<string> GetMetacriticStars()
        {
            //Initialize List
            List<string> stars = new List<string>();

            //Validate Parent
            if (IsElementPresent(DriverType.Default, By.ClassName("c-globalPersonCard_name")))
            {
                //Get Stars IWebElement
                ReadOnlyCollection<IWebElement> elements = DefaultDriver.FindElements(By.ClassName("c-globalPersonCard_name"));

                //Loop through Star IWebElements
                foreach (IWebElement element in elements)
                {
                    //Add Star to List
                    stars.Add(element.Text);
                }
            }

            //Return Stars
            return stars;
        }


        // Production Companies
        private static List<string> GetMetacriticProductionCompanies(IReadOnlyCollection<IWebElement> details)
        {
            //Initialize List
            List<string> collection = new List<string>();

            //Validate Parent
            if (details.Any(i => i.FindElement(By.ClassName("g-text-bold")).Text.Contains("Production Company")))
            {
                //Get Parent IWebElement
                IWebElement parent = details.Single(i => i.FindElement(By.ClassName("g-text-bold")).Text.Contains("Production Company"));

                //Get Production Companies
                string companies = parent.FindElement(By.ClassName("g-outer-spacing-left-medium-fluid")).Text;

                //Split Production Companies into Array
                //Trim Whitespace of Production Company Strings
                //Convert Array to List and Items into collection List
                collection = companies.Split(',').Select(i => i != null ? i.Trim() : null).ToList();
            }

            //Return List
            return collection;
        }



        // Extensions
        // ======================================
        // ======================================
        // Metacritic Detail
        private static string GetMetacriticDetail(IReadOnlyCollection<IWebElement> details, string name)
        {
            //Initialize Variable
            string detail = string.Empty;

            //Validate Parent IWebElement
            if (details.Any(i => i.FindElement(By.ClassName("g-text-bold")).Text.ToLower().Contains(name.ToLower())))
            {
                //Get Parent IWebElement
                IWebElement parent = details.Single(i => i.FindElement(By.ClassName("g-text-bold")).Text.ToLower().Contains(name.ToLower()));

                //Get Detail
                detail = parent.FindElement(By.ClassName("g-outer-spacing-left-medium-fluid")).Text;
            }

            //Return detail Variable
            return detail;
        }
        #endregion Metacritic
        #endregion Virtual Entertainment



        #region Movie
        // Methods
        // ======================================
        // ======================================
        // Get Metacritic Collection
        private static List<string> GetMetacriticCollection(string classname)
        {
            //Inititliaze List
            List<string> collection = new List<string>();

            //Validate Parent IWebElement
            if (IsElementPresent(DriverType.Default, By.ClassName(classname)))
            {
                //Get Parent IWebElement
                IWebElement parent = DefaultDriver.FindElement(By.ClassName(classname));

                //Get IWebElements
                ReadOnlyCollection<IWebElement> elements = parent.FindElements(By.TagName("a"));

                //Loop through IWebElements
                foreach (IWebElement element in elements)
                {
                    //Get Value
                    string value = element.GetAttribute("href");

                    //Format Value
                    value = Regex.Replace(value, @"[0-9]", string.Empty);
                    value = value.Replace("https://www.metacritic.com/person/", "").Replace("/", "").Replace("-", " ").Trim();
                    value = new CultureInfo("en-US", false).TextInfo.ToTitleCase(value);

                    //Add Value to Collection
                    collection.Add(value);
                }
            }

            //Return Collection
            return collection;
        }
        #endregion Movie



        #region TV Show Folder
        #region Main
        // Methods
        // ======================================
        // ======================================
        // Season Count
        private static int GetSeasonCount(bool isimdb)
        {
            //Initialize Variables
            int seasoncount = 0;
            string classname = isimdb ? "ipc-simple-select__label" : "c-seasonsComponent_seasons";

            //Validate IWebElement
            if (IsElementPresent(isimdb ? DriverType.IMDB : DriverType.Default, By.ClassName(classname)))
            {
                //Get IWebElement
                IWebElement element = isimdb ? IMDBDriver.FindElement(By.ClassName(classname)) : DefaultDriver.FindElement(By.ClassName(classname));

                //Get Season Count
                string value = element.Text;

                //Remove All Alphabetic Characters from Season Count and Convert to Integer
                seasoncount = Convert.ToInt32(Regex.Replace(value, "[^0-9.]", ""));
            }

            //Return seasoncount Variable
            return seasoncount;
        }
        #endregion Main



        #region IMDB
        // Methods
        // ======================================
        // ======================================
        // Episode Count
        private static int GetIMDBEpisodeCount()
        {
            //Initialize Variable
            int episodecount = 0;

            //Validate IWebElement
            if (IsElementPresent(DriverType.IMDB, By.ClassName("sc-1371769f-3")))
            {
                //Get IWebElement
                IWebElement element = IMDBDriver.FindElement(By.ClassName("sc-1371769f-3"));

                //Get Episode Count
                episodecount = Convert.ToInt32(element.Text);
            }

            //Return episodecount Variable
            return episodecount;
        }
        #endregion IMDB



        #region Metacritic
        // Methods
        // ======================================
        // ======================================
        // Release Period
        private static string GetMetacriticReleasePeriod()
        {
            //Initialize Variable
            string period = string.Empty;

            //Validate View All Seasons Button
            if (IsElementPresent(DriverType.Default, By.ClassName("c-seasonsComponent_button")))
            {
                //Get View All Seasons Button
                IWebElement seasonsbtn = DefaultDriver.FindElement(By.ClassName("c-seasonsComponent_button"));

                //Click View All Seasons Button
                seasonsbtn.Click();

                //Validate Parent IWebElement
                if (IsElementPresent(DriverType.Default, By.ClassName("c-globalModal_content")))
                {
                    //Get Parent IWebElement
                    IWebElement parent = DefaultDriver.FindElement(By.ClassName("c-globalModal_content"));

                    //Validate IWebElements
                    if (IsElementPresent(DriverType.Null, By.ClassName("c-seasonsModalCard_scoreCard_details"), parent))
                    {
                        //Get IWebElements
                        IReadOnlyCollection<IWebElement> elements = parent.FindElements(By.ClassName("c-seasonsModalCard_scoreCard_details"));

                        //Get Release Period
                        string start = elements.FirstOrDefault().FindElements(By.TagName("span")).LastOrDefault().Text;
                        string end = elements.LastOrDefault().FindElements(By.TagName("span")).LastOrDefault().Text;

                        //Format Release Period
                        period = start != end ? $"{start} - {end}" : string.Empty;
                    }
                }
            }

            //Return Release Period
            return period;
        }


        // Episode Count
        private static int GetMetacriticEpisodeCount(string metacriticlink)
        {
            //Initialize Variables
            List<string> episodecounts = new List<string>();
            int totalepisodes = 0;

            //Validate Season Cards
            if (IsElementPresent(DriverType.Default, By.ClassName("c-seasonsModalCard")))
            {
                //Get Season Cards
                IReadOnlyCollection<IWebElement> seasoncards = DefaultDriver.FindElements(By.ClassName("c-seasonsModalCard"));

                //Loop through Season Cards
                foreach (IWebElement seasoncard in seasoncards)
                {
                    //Validate Episode Count Container
                    if (IsElementPresent(DriverType.Null, By.ClassName("g-text-normal"), seasoncard))
                    {
                        //Format and Get Episode Count
                        episodecounts.Add(seasoncard.FindElement(By.ClassName("g-text-normal")).FindElement(By.TagName("span")).Text.Replace("Episodes", "").Trim());
                    }
                }

                //Calculate Total Episode Count
                foreach (string item in episodecounts) { int.TryParse(item, out int result); totalepisodes += result; }
            }

            //Return Total Episodes Count
            return totalepisodes;
        }
        #endregion Metacritic
        #endregion TV Show Folder



        #region Season Folder
        #region IMDB
        // Methods
        // ======================================
        // ======================================
        // Season Link
        public static string GetIMDBSeasonLink(string imdblink, int seasonnumber)
        {
            //Split IMDB Link
            string[] linksegments = imdblink.Replace("https://", "").Split('/');

            //Format and Return IMDB Link
            return $"https://{linksegments[0]}/{linksegments[1]}/{linksegments[2]}/episodes/?season={seasonnumber}";
        }


        // Release Date
        private static string GetIMDBSeasonReleaseDate()
        {
            //Initialize Variable
            string releasedate = string.Empty;

            //Validate IWebElement
            if (IsElementPresent(DriverType.IMDB, By.ClassName("jEHgCG")))
            {
                //Get Release Date
                releasedate = IMDBDriver.FindElements(By.ClassName("jEHgCG")).FirstOrDefault().Text;
            }

            //Return releasedate Variable
            return releasedate;
        }


        // Episode Count
        private static int GetIMDBSeasonEpisodeCount()
        {
            //Initialize Variable
            int episodecount = 0;

            //Validate IWebElement
            if (IsElementPresent(DriverType.IMDB, By.ClassName("klCVxt")))
            {
                //Get Episode Count
                string value = IMDBDriver.FindElements(By.ClassName("klCVxt")).LastOrDefault().Text;

                //Format Episode Count
                value = value.Split(' ').ElementAt(0).Split('E').ElementAt(1);

                //Convert Episode Count
                int.TryParse(value, out episodecount);
            }

            //Return episodecount Variable
            return episodecount;
        }
        #endregion IMDB



        #region Metacritic
        // Methods
        // ======================================
        // ======================================
        // Season Link
        public static string GetMetacriticSeasonLink(string metacriticlink, int seasonnumber)
        {
            //Format and Return Metacritic Link
            return $"{metacriticlink}season-{seasonnumber}/";
        }


        // Episode Count
        private static int GetMetacriticSeasonEpisodeCount()
        {
            //Initialize Variable
            int episodecount = 0;

            //Validate Container IWebElement
            if (IsElementPresent(DriverType.Default, By.ClassName("c-episodesModalCard_link")))
            {
                //Get Container IWebElement
                IWebElement container = DefaultDriver.FindElements(By.ClassName("c-episodesModalCard_link")).LastOrDefault();

                //Validate IWebElement
                if (IsElementPresent(DriverType.Null, By.ClassName("g-text-bold"), container))
                {
                    //Get Episode Count
                    string value = container.FindElement(By.ClassName("g-text-bold")).Text;

                    //Format Episode Count
                    value = value.Replace("Episode", "").Trim();

                    //Convert Episode Count
                    int.TryParse(value, out episodecount);
                }

            }

            //Return episodecount Variable
            return episodecount;
        }
        #endregion Metacritic
        #endregion Season Folder



        #region Episode
        #region Main
        // Methods
        // ======================================
        // ======================================
        // Episode Number
        public static int GetEpisodeNumber(string filepath)
        {
            //Initialize Regex Object to Replace Separators with Spaces
            Regex regex = new Regex("[_:.()]|[-]|[[]|[]]");

            //Get File Name and Replace Separator Characters
            string episode = regex.Replace(Path.GetFileNameWithoutExtension(filepath).ToLower(), " ");

            //Initialize Regex Object to Locate Episode Number
            regex = new Regex(@"(?:e|x|ep|episode)\s?(?<episode>\d+)");

            //Locate Title, Season, and Episode
            Match match = regex.Match(episode);

            //Validate Match
            if (match.Success)
            {
                //Convert and Return Episode Number
                return Convert.ToInt32(match.Groups["episode"].Value.TrimStart('0'));
            }

            //Return 0
            return 0;
        }


        // Links
        public static async Task<string[]> GetEpisodeLinksAsync(string metacriticlink, string imdblink, int episodenumber)
        {
            //Initialize Variables
            string[] links = new string[2];

            //Set Active Window to Topmost
            WindowsAPI.ToggleActiveWindow(true);

            //Validate and Create Default Driver
            if (!string.IsNullOrEmpty(metacriticlink)) { DefaultDriver = UndetectedChromeDriver.Create(null, null, await new ChromeDriverInstaller().Auto(), null, 0, 0, false, true, true, true); }

            //Validate and Create IMDB Driver
            if (!string.IsNullOrEmpty(imdblink)) { IMDBDriver = UndetectedChromeDriver.Create(null, null, await new ChromeDriverInstaller().Auto(), null, 1, 0, false, true, true, true); }

            //Hide Chrome Driver Windows
            WindowsAPI.HideChromeDriverWindows();

            //Unset Active Window Topmost
            WindowsAPI.ToggleActiveWindow(false);

            //Validate Metacritic Link
            if (!string.IsNullOrEmpty(metacriticlink))
            {
                //Navigate to Metacritic Link
                DefaultDriver.Navigate().GoToUrl(metacriticlink);

                //
                await Task.Delay(100);

                //Get Metacritic Link
                links[0] = GetMetacriticEpisodeLink(episodenumber);
            }
            else
            {
                //Set Metacritic Link Array Variable to Empty String
                links[0] = string.Empty;
            }

            //Validate IMDB Link
            if (!string.IsNullOrEmpty(imdblink))
            {
                //Navigate to IMDB Link
                IMDBDriver.Navigate().GoToUrl(imdblink);

                //
                await Task.Delay(100);

                //Get IMDB Link
                links[1] = GetIMDBEpisodeLink(episodenumber);
            }
            else
            {
                //Set IMDB Link Array Variable to Empty String
                links[1] = string.Empty;
            }

            //Close Fetcher
            Close();

            //Return links Array
            return links;
        }


        // Episodes
        public static List<FileInfo> GetEpisodes(DirectoryInfo dirinfo, string[] extensions)
        {
            //Convert Extensions to HashSet
            HashSet<string> allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);

            //Get and Return Files
            return dirinfo.EnumerateFiles().Where(f => allowedExtensions.Contains(f.Extension)).ToList();
        }
        #endregion Main



        #region IMDB
        // Methods
        // ======================================
        // ======================================
        // Link
        public static string GetIMDBEpisodeLink(int episode)
        {
            //Get Season Episode Containers
            IReadOnlyCollection<IWebElement> episodes = IMDBDriver.FindElements(By.ClassName("ipc-title-link-wrapper"));

            //Get Episode Link
            string episodelink = episodes.ElementAt(episode - 1).GetAttribute("href");

            //Return Episode Link
            return episodelink;
        }
        #endregion IMDB



        #region Metacritic
        // Methods
        // ======================================
        // ======================================
        // Link
        public static string GetMetacriticEpisodeLink(int episode)
        {
            //Get Metacritic Episode Containers
            IReadOnlyCollection<IWebElement> episodes = DefaultDriver.FindElements(By.ClassName("c-episodesModalCard_link"));

            //Gwt and Return Metacritic Episode Link
            return episodes.ElementAt(episode - 1).GetAttribute("href");
        }


        // Title
        private static string GetMetacriticEpisodeTitle()
        {
            //Initialize Variable
            string title = string.Empty;

            //Validate IWebElement
            if (IsElementPresent(DriverType.Default, By.ClassName("c-productHero_seasonsComponent_episode")))
            {
                //Get Title
                title = DefaultDriver.FindElement(By.ClassName("c-productHero_seasonsComponent_episode")).Text.Trim();
            }

            //Return title Variable
            return title;
        }
        #endregion Metacritic
        #endregion Episode



        #region Game
        // Generic Game Detail (Name / Cover)
        // ===========================================================
        // ===========================================================
        private static string GetGenericGameDetail(string classname, bool isattr, string attr = "")
        {
            //Validate Element
            if(IsElementPresent(DriverType.Default, By.ClassName(classname)))
            {
                //Get Element Value
                string value = isattr ? DefaultDriver.FindElement(By.ClassName(classname)).GetAttribute(attr) : DefaultDriver.FindElement(By.ClassName(classname)).Text;

                //Return Value
                return value;
            }

            //Return Empty String
            return string.Empty;
        }


        // Generic Game Detail (Publisher / Release Date)
        // ===========================================================
        // ===========================================================
        private static string GetGenericGameDetail(string tagname)
        {
            //Validate Parent
            if(IsElementPresent(DriverType.Default, By.ClassName("gamepage-title-wrapper")))
            {
                //Get Parent
                IWebElement parent = DefaultDriver.FindElement(By.ClassName("gamepage-title-wrapper"));

                //Validate and Get Value
                string value = IsElementPresent(DriverType.Null, By.TagName(tagname), parent) ? parent.FindElement(By.TagName(tagname)).Text : string.Empty;

                //Return Value
                return value;
            }

            //Return Empty String
            return string.Empty;
        }


        // Generic Game Collection (Genres / Available Platforms)
        // ===========================================================
        // ===========================================================
        private static List<string> GetGenericGameCollection(string name, string tagname = "span", string itemstag = "a", string xpath = "/html/body/div[2]/main/div[2]/div[1]/div/div[2]/div[2]/div[2]/div[2]/p", string collectionspath = "/html/body/div[2]/main/div[2]/div[1]/div/div[2]/div[2]/div[2]/div[2]/p")
        {
            //Variables
            ReadOnlyCollection<IWebElement> collections;
            List<string> items = new List<string>();

            //Validate Collections Path
            if (IsElementPresent(DriverType.Default, By.XPath(xpath)))
            {

                //Get Collections
                collections = DefaultDriver.FindElements(By.XPath(xpath));

                //Loop through Collections
                for (int i = 0; i < collections.Count; i++)
                {
                    //Validate Collection Name
                    if (IsElementPresent(DriverType.Default, By.XPath($"{collectionspath}/{tagname}")) && collections[i].FindElement(By.TagName(tagname)).Text.ToLower().Contains(name))
                    {
                        //Get Collection Items
                        ReadOnlyCollection<IWebElement> collection = collections[i].FindElements(By.TagName(itemstag));

                        //Add Collection Items to items List
                        foreach (IWebElement item in collection)
                        {
                            items.Add(item.Text);
                        }

                        //Break For Loop
                        break;
                    }
                }
            }

            //Return items List
            return items;
        }


        // Rating (Score / Review Count)
        // ===========================================================
        // ===========================================================
        private static List<KeyValuePair<string, int>> GetGameRatings()
        {
            //Return Ratings List
            return new List<KeyValuePair<string, int>>()
            {
                GetGameReviewScore("UserScore"),
                GetGameReviewCount("UserReviewCount", "gauge-single-info", "member ratings"),
                GetGameReviewScore("CriticScore", true),
                GetGameReviewCount("CriticReviewCount", "gauge-twin-info", "critic ratings")
            };
        }


        // Extensions
        // ===========================================================
        // ===========================================================
        private static KeyValuePair<string, int> GetGameReviewCount(string label, string classname, string replacetxt)
        {
            //Variables
            int reviewcount = 0;

            //Validate IWebElement
            if (IsElementPresent(DriverType.Default, By.ClassName(classname)))
            {
                //Get Value
                string value = DefaultDriver.FindElement(By.ClassName(classname)).Text.ToLower();

                //Format Value
                value = value.Replace("based on", "").Replace(replacetxt, "");

                //Parse Value
                int.TryParse(value, out reviewcount);
            }

            //Format and Return Review Count
            return new KeyValuePair<string, int>(label, reviewcount);
        }

        private static KeyValuePair<string, int> GetGameReviewScore(string label, bool iscritic = false)
        {
            //Variables
            int score = 0;
            int index = iscritic ? 2 : 0;

            //Validate Parent
            if (IsElementPresent(DriverType.Default, By.ClassName("gamepage-gauge")))
            {
                //Get Parent
                IWebElement parent = DefaultDriver.FindElement(By.ClassName("gamepage-gauge"));

                //Validate IWebElements
                if (IsElementPresent(DriverType.Null, By.TagName("text"), parent))
                {
                    //Get IWebElements
                    ReadOnlyCollection<IWebElement> elements = parent.FindElements(By.TagName("text"));

                    //Validate Element Count
                    if (elements.Count == 3)
                    {
                        //Try Parse Review Score to Integer
                        int.TryParse(elements.ElementAt(index).Text, out score);
                    }
                }
            }

            //Format and Return Review Score
            return new KeyValuePair<string, int>(label, score);
        }
        #endregion Game
    }
}