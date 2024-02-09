using Dapper;
using System.IO;
using System.Data;
using System.Linq;
using System.Data.SQLite;
using System.Configuration;
using Media_Manager.Models;
using System.Collections.Generic;
using MediaControlsLibrary.Models;
using Media_Manager.Models.BaseModels;
using MediaControlsLibrary.Types;

namespace Media_Manager
{
    public class Database
    {
        #region Variables
        // Database Variables
        // =======================================================
        // =======================================================
        private static readonly List<string> tblFolders = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "Name TEXT NOT NULL", "Type TEXT NOT NULL", "FolderType TEXT NOT NULL" };
        private static readonly List<string> tblMovies = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "isFavourite INTEGER NOT NULL", "FilePath TEXT NOT NULL", "CoverImage TEXT NOT NULL", "CustomName TEXT", "Name TEXT NOT NULL", "Width TEXT NOT NULL", "Height TEXT NOT NULL", "Duration INTEGER NOT NULL", "Framerate REAL NOT NULL", "Format TEXT NOT NULL", "FileSize INTEGER NOT NULL", "CreationTime TEXT NOT NULL", "CreationDate TEXT NOT NULL", "SampleRate REAL", "AudioChannels TEXT", "FramerateMode TEXT", "MetaCriticLink TEXT", "UserScore REAL", "UserReviewCount INTEGER", "CriticScore REAL", "CriticReviewCount INTEGER", "IMDBLink TEXT", "ReleaseDate TEXT", "Region TEXT", "AgeRating TEXT", "SerializedGenres TEXT", "SerializedDirectors TEXT", "SerializedWriters TEXT", "SerializedStars TEXT", "SerializedProductionCompanies TEXT" };
        private static readonly List<string> tblTVShowFolders = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "isFavourite INTEGER NOT NULL", "CustomName TEXT", "Name TEXT NOT NULL", "Type TEXT NOT NULL", "FolderType TEXT NOT NULL", "CoverImage TEXT", "CustomCoverImage TEXT", "CreationTime TEXT", "CreationDate TEXT", "SeasonCount INTEGER", "EpisodeCount INTEGER", "ReleaseDate TEXT", "ReleasePeriod TEXT", "AgeRating TEXT", "SerializedGenres TEXT", "SerializedStars TEXT", "SerializedProductionCompanies TEXT", "MetaCriticLink TEXT", "UserScore REAL", "UserReviewCount INTEGER", "CriticScore REAL", "CriticReviewCount INTEGER", "IMDBLink TEXT", "Region TEXT", "SerializedCreators TEXT" };
        private static readonly List<string> tblSeasonFolders = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "FilePath TEXT NOT NULL", "CustomName TEXT", "Name TEXT NOT NULL", "Type TEXT NOT NULL", "FolderType TEXT NOT NULL", "CoverImage TEXT", "CustomCoverImage TEXT", "CreationTime TEXT", "CreationDate TEXT", "SeasonNumber INTEGER NOT NULL", "EpisodeCount TEXT", "ReleaseDate TEXT", "IMDBLink TEXT", "MetaCriticLink TEXT" }; 
        private static readonly List<string> tblEpisodes = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "FilePath TEXT NOT NULL", "CoverImage TEXT NOT NULL", "CustomName TEXT", "Name TEXT NOT NULL", "Season INTEGER NOT NULL", "EpisodeNumber INTEGER NOT NULL", "Width TEXT NOT NULL", "Height TEXT NOT NULL", "Duration INTEGER NOT NULL", "Framerate REAL NOT NULL", "Format TEXT NOT NULL", "FileSize INTEGER NOT NULL", "CreationTime TEXT NOT NULL", "CreationDate TEXT NOT NULL", "SampleRate REAL", "AudioChannels TEXT", "FramerateMode TEXT", "AirDate TEXT", "SerializedDirectors TEXT", "SerializedWriters TEXT", "SerializedProductionCompanies TEXT", "IMDBLink TEXT", "Region TEXT", "AgeRating TEXT", "SerializedGenres TEXT", "SerializedStars TEXT", "MetaCriticLink TEXT" };
        private static readonly List<string> tblVideos = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "isFavourite INTEGER NOT NULL", "FilePath TEXT NOT NULL", "CoverImage TEXT NOT NULL", "CustomName TEXT", "Name TEXT NOT NULL", "Width TEXT NOT NULL", "Height TEXT NOT NULL", "Duration INTEGER NOT NULL", "Framerate REAL NOT NULL", "Format TEXT NOT NULL", "FileSize INTEGER NOT NULL", "CreationTime TEXT NOT NULL", "CreationDate TEXT NOT NULL", "SampleRate REAL", "AudioChannels TEXT", "FramerateMode TEXT" };
        private static readonly List<string> tblPictures = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "isFavourite INTEGER NOT NULL", "FilePath TEXT NOT NULL", "CoverImage TEXT NOT NULL", "CustomName TEXT", "Name TEXT NOT NULL", "Width TEXT NOT NULL", "Height TEXT NOT NULL", "Format TEXT NOT NULL", "FileSize INTEGER NOT NULL", "CreationTime TEXT NOT NULL", "CreationDate TEXT NOT NULL", "ColourSpace TEXT", "BitDepth TEXT", "CompMode TEXT" };
        private static readonly List<string> tblMusic = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "isFavourite INTEGER NOT NULL", "FilePath TEXT NOT NULL", "CoverImage TEXT", "CustomName TEXT", "Name TEXT NOT NULL", "Duration INTEGER NOT NULL", "Format TEXT NOT NULL", "FileSize INTEGER NOT NULL", "CreationTime TEXT NOT NULL", "CreationDate TEXT NOT NULL", "SampleRate REAL", "AudioChannels TEXT", "CompMode TEXT" };
        private static readonly List<string> tblGames = new List<string> { "Id INTEGER PRIMARY KEY NOT NULL", "OwnerId INTEGER NOT NULL", "isFavourite INTEGER NOT NULL", "BaseDirectory TEXT", "FilePath TEXT", "CoverImage TEXT", "CustomName TEXT", "Name TEXT", "Format TEXT", "FileSize INTEGER", "CreationTime TEXT", "CreationDate TEXT", "IGDBLink TEXT", "Publisher TEXT", "ReleaseDate TEXT", "Type TEXT", "UserScore REAL", "UserReviewCount INTEGER", "CriticScore REAL", "CriticReviewCount INTEGER", "SerializedGenres TEXT", "SerializedAvailablePlatforms TEXT" };


        // Fields
        // =======================================================
        // =======================================================
        private static readonly List<string> folderFields = new List<string> { "OwnerId", "Name", "Type", "FolderType" };
        private static readonly List<string> movieFields = new List<string> { "OwnerId", "isFavourite", "FilePath", "CoverImage", "CustomName", "Name", "Width", "Height", "Duration", "Framerate", "Format", "FileSize", "CreationTime", "CreationDate", "SampleRate", "AudioChannels", "FramerateMode", "ReleaseDate", "Region", "AgeRating", "SerializedGenres", "SerializedDirectors", "SerializedWriters", "SerializedStars", "SerializedProductionCompanies", "IMDBLink", "MetaCriticLink", "UserScore", "UserReviewCount", "CriticScore", "CriticReviewCount" };
        private static readonly List<string> tvshowfolderFields = new List<string> { "Id", "OwnerId", "isFavourite", "CustomName", "Name", "Type", "FolderType", "CoverImage", "CustomCoverImage", "CreationTime", "CreationDate", "SeasonCount", "EpisodeCount", "ReleaseDate", "ReleasePeriod", "AgeRating", "SerializedGenres", "SerializedStars", "SerializedProductionCompanies", "MetaCriticLink", "UserScore", "UserReviewCount", "CriticScore", "CriticReviewCount", "IMDBLink", "Region", "SerializedCreators" };
        private static readonly List<string> seasonfolderFields = new List<string> { "Id", "OwnerId", "FilePath", "CustomName", "Name", "Type", "FolderType", "CoverImage", "CustomCoverImage", "CreationTime", "CreationDate", "SeasonNumber", "EpisodeCount", "ReleaseDate", "IMDBLink", "MetaCriticLink" };
        private static readonly List<string> episodeFields = new List<string> { "Id", "OwnerId", "FilePath", "CoverImage", "CustomName", "Name", "Season", "EpisodeNumber", "Width", "Height", "Duration", "Framerate", "Format", "FileSize", "CreationTime", "CreationDate", "SampleRate", "AudioChannels", "FramerateMode", "AirDate", "SerializedDirectors", "SerializedWriters", "SerializedProductionCompanies", "IMDBLink", "Region", "AgeRating", "SerializedGenres", "SerializedStars", "MetaCriticLink" };
        private static readonly List<string> videoFields = new List<string> { "OwnerId", "isFavourite", "FilePath", "CoverImage", "CustomName", "Name", "Width", "Height", "Duration", "Framerate", "Format", "FileSize", "CreationTime", "CreationDate", "SampleRate", "AudioChannels", "FramerateMode" };
        private static readonly List<string> pictureFields = new List<string> { "OwnerId", "isFavourite", "FilePath", "CoverImage", "CustomName", "Name", "Width", "Height", "Format", "FileSize", "CreationTime", "CreationDate", "ColourSpace", "BitDepth", "CompMode" };
        private static readonly List<string> musicFields = new List<string> { "OwnerId", "isFavourite", "FilePath", "CoverImage", "CustomName", "Name", "Duration", "Format", "FileSize", "CreationTime", "CreationDate", "SampleRate", "AudioChannels", "CompMode" };
        private static readonly List<string> gameFields = new List<string> { "OwnerId", "isFavourite", "BaseDirectory", "FilePath", "CoverImage", "CustomName", "Name", "Format", "FileSize", "CreationTime", "CreationDate", "IGDBLink", "Publisher", "ReleaseDate", "Type", "UserScore", "UserReviewCount", "CriticScore", "CriticReviewCount", "SerializedGenres", "SerializedAvailablePlatforms" };
        #endregion Variables



        // Connection String
        // =======================================================
        // =======================================================
        private static string LoadConnectionString(string id = "Default")
        {
            //Return app.config connection string
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

        private static void UpdateConnectionString(string id, string connectionstring)
        {
            //Create Configuration Object from App.Config
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //Get Connection Strings Section from Configuration Object
            ConnectionStringsSection connectionStringsSection = (ConnectionStringsSection)configuration.GetSection("connectionStrings");

            //Set Configuration String
            connectionStringsSection.ConnectionStrings[id].ConnectionString = connectionstring;

            //Save Configuration
            configuration.Save();

            //Refresh the Connection Strings Section of App.Config
            ConfigurationManager.RefreshSection("connectionStrings");
        }



        #region Methods
        // Initialize
        // =======================================================
        // =======================================================
        public static void Initialize(string localdata, string connectionstringId = "Default")
        {
            //Initialize Variables
            string name = "MediaManagerDB.db";
            string connectionstring = $"Data Source={localdata}\\{name};Version=3;";

            //Update Connection String
            UpdateConnectionString(connectionstringId, connectionstring);

            //Check if the Database File Does Not Exist
            if (!File.Exists($"{localdata}\\{name}"))
            {
                //Create Database File
                SQLiteConnection.CreateFile($"{localdata}\\{name}");

                //Initialize Connection String
                SQLiteConnection connection = new SQLiteConnection(connectionstring);

                //Open Connection String
                connection.Open();

                //Create Table
                CreateTable(FormatCreate(tblFolders, ElementType.Folders, MediaType.Null, FolderType.Folders), ref connection);
                CreateTable(FormatCreate(tblMovies, ElementType.Files, MediaType.Movies), ref connection);
                CreateTable(FormatCreate(tblTVShowFolders, ElementType.Folders, MediaType.Null, FolderType.TVShowFolders), ref connection);
                CreateTable(FormatCreate(tblSeasonFolders, ElementType.Folders, MediaType.Null, FolderType.SeasonFolders), ref connection);
                CreateTable(FormatCreate(tblEpisodes, ElementType.Files, MediaType.Episodes), ref connection);
                CreateTable(FormatCreate(tblVideos, ElementType.Files, MediaType.Videos), ref connection);
                CreateTable(FormatCreate(tblPictures, ElementType.Files, MediaType.Pictures), ref connection);
                CreateTable(FormatCreate(tblMusic, ElementType.Files, MediaType.Music), ref connection);
                CreateTable(FormatCreate(tblGames, ElementType.Files, MediaType.Games), ref connection);

                //Close Connection
                connection.Close();
            }
        }


        // Create
        // =======================================================
        // =======================================================
        private static void CreateTable(string table, ref SQLiteConnection connection)
        {
            //Create Command for Table String
            SQLiteCommand command = new SQLiteCommand(table, connection);

            //Execute Command for Table String
            command.ExecuteNonQuery();
        }


        // Insert
        // =======================================================
        // =======================================================
        public static void SaveFolder(object folder, FolderType foldertype = FolderType.Folders)
        {
            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Check what type of folder has to be saved
                if (foldertype == FolderType.Folders)
                {
                    //Insert Folder into SQLite Database
                    cnn.Execute(FormatInsert(folderFields, nameof(FolderType.Folders)), folder);
                }
                else if (foldertype == FolderType.TVShowFolders)
                {
                    //Convert Folder Object to TVShowFolder Object
                    TVShowFolder tvshowfolder = folder as TVShowFolder;

                    //Serialize Lists
                    tvshowfolder.SerializedGenres = SerializeList(tvshowfolder.Genres);
                    tvshowfolder.SerializedStars = SerializeList(tvshowfolder.Stars);
                    tvshowfolder.SerializedProductionCompanies = SerializeList(tvshowfolder.ProductionCompanies);
                    tvshowfolder.SerializedCreators = SerializeList(tvshowfolder.Creators);

                    //Insert TV Show Folder into SQLite Database
                    cnn.Execute(FormatInsert(tvshowfolderFields, nameof(FolderType.TVShowFolders)), tvshowfolder);
                }
                else if (foldertype == FolderType.SeasonFolders)
                {
                    //Insert Season Folder into SQLite Database
                    cnn.Execute(FormatInsert(seasonfolderFields, nameof(FolderType.SeasonFolders)), folder);
                }
            }
        }

        public static void SaveItem(MediaType mediatype, object item)
        {
            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Check what type of item has to be saved
                if(mediatype == MediaType.Movies)
                {
                    //Convert Item Object to Movie Object
                    Movie movie = item as Movie;

                    //Serialize Lists
                    movie.SerializedGenres = SerializeList(movie.Genres);
                    movie.SerializedDirectors = SerializeList(movie.Directors);
                    movie.SerializedWriters = SerializeList(movie.Writers);
                    movie.SerializedStars = SerializeList(movie.Stars);
                    movie.SerializedProductionCompanies = SerializeList(movie.ProductionCompanies);

                    //Insert Movie into SQLite Database
                    cnn.Execute(FormatInsert(movieFields, nameof(MediaType.Movies)), movie);
                }
                else if (mediatype == MediaType.Episodes)
                {
                    //Convert Item Object to Episode Object
                    Episode episode = item as Episode;

                    //Serialize Lists
                    episode.SerializedDirectors = SerializeList(episode.Directors);
                    episode.SerializedGenres = SerializeList(episode.Genres);
                    episode.SerializedProductionCompanies = SerializeList(episode.ProductionCompanies);
                    episode.SerializedStars = SerializeList(episode.Stars);
                    episode.SerializedWriters = SerializeList(episode.Writers);

                    //Insert Episode into SQLite Database
                    cnn.Execute(FormatInsert(episodeFields, nameof(MediaType.Episodes)), episode);
                }
                else if (mediatype == MediaType.Videos)
                {
                    //Insert Video into SQLite Database
                    cnn.Execute(FormatInsert(videoFields, nameof(MediaType.Videos)), item);
                }
                else if (mediatype == MediaType.Pictures)
                {
                    //Insert Picture into SQLite Database
                    cnn.Execute(FormatInsert(pictureFields, nameof(MediaType.Pictures)), item);
                }
                else if(mediatype == MediaType.Music)
                {
                    //Insert Song into Music SQLite Database
                    cnn.Execute(FormatInsert(musicFields, nameof(MediaType.Music)), item);
                }
                else if (mediatype == MediaType.Games)
                {
                    //Convert Item Object to Game Object
                    Game game = item as Game;

                    //Serialize Lists
                    game.SerializedGenres = SerializeList(game.Genres);
                    game.SerializedAvailablePlatforms = SerializeList(game.AvailablePlatforms);

                    //Insert Game into SQLite Database
                    cnn.Execute(FormatInsert(gameFields, nameof(MediaType.Games)), game);
                }
            }
        }


        // Load
        // =======================================================
        // =======================================================
        public static List<object> LoadFolders(MediaType mediatype, FolderType foldertype = FolderType.Folders, string order = "", string sorttype = "")
        {
            //Variables
            IEnumerable<object> output;

            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Check what type of dolers have to be loaded
                if (foldertype == FolderType.Folders)
                {
                    //Select all data within the Folders Database Where the Type Value is Equal to type
                    output = cnn.Query<Folder>(FormatLoad(ElementType.Folders, mediatype, foldertype), new DynamicParameters());
                }
                else if(foldertype == FolderType.TVShowFolders)
                {
                    //Select all data within the Folders Database Where the Type Value is Equal to type
                    output = cnn.Query<TVShowFolder>(FormatLoad(ElementType.Folders, mediatype, foldertype, order, sorttype), new DynamicParameters());

                    //Loop through each object in the output list
                    foreach (TVShowFolder tvshowfolder in output)
                    {
                        //Deserialize Lists
                        tvshowfolder.Creators = DeserializeList(tvshowfolder.SerializedCreators);
                        tvshowfolder.Genres = DeserializeList(tvshowfolder.SerializedGenres);
                        tvshowfolder.ProductionCompanies = DeserializeList(tvshowfolder.SerializedProductionCompanies);
                        tvshowfolder.Stars = DeserializeList(tvshowfolder.SerializedStars);
                    }
                }
                else
                {
                    //Select all data within the Season Folders Database Where the Type Value is Equal to type
                    output = cnn.Query<SeasonFolder>(FormatLoad(ElementType.Folders, mediatype, foldertype), new DynamicParameters());
                }
            }
            
            //Convert selection to list and return it
            return output.ToList();
        }

        public static List<object> LoadItems(MediaType mediatype, string order, string sorttype, bool isreverse = false)
        {
            //Variables
            IEnumerable<object> output;

            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Check what type of items have to be loaded
                if (mediatype == MediaType.Movies)
                {
                    //Select all data within the Movies Database
                    output = cnn.Query<Movie>(FormatLoad(ElementType.Files, MediaType.Movies, FolderType.Null, order, sorttype), new DynamicParameters());

                    //Loop through each object in the output list
                    foreach (Movie movie in output)
                    {
                        //Deserialize Lists
                        movie.Genres = DeserializeList(movie.SerializedGenres);
                        movie.Directors = DeserializeList(movie.SerializedDirectors);
                        movie.Writers = DeserializeList(movie.SerializedWriters);
                        movie.Stars = DeserializeList(movie.SerializedStars);
                        movie.ProductionCompanies = DeserializeList(movie.SerializedProductionCompanies);
                    }
                }
                else if (mediatype == MediaType.Episodes)
                {
                    //Select all data within the Episodes Database
                    output = cnn.Query<Episode>(FormatLoad(ElementType.Files, MediaType.Episodes, FolderType.Null, order, sorttype), new DynamicParameters());

                    //Loop through each object in the output list
                    foreach (Episode episode in output)
                    {
                        //Deserialize Lists
                        episode.Genres = DeserializeList(episode.SerializedGenres);
                        episode.Directors = DeserializeList(episode.SerializedDirectors);
                        episode.Writers = DeserializeList(episode.SerializedWriters);
                        episode.Stars = DeserializeList(episode.SerializedStars);
                        episode.ProductionCompanies = DeserializeList(episode.SerializedProductionCompanies);
                    }
                }
                else if (mediatype == MediaType.Videos)
                {
                    //Select all data within the Videos Database
                    output = cnn.Query<Video>(FormatLoad(ElementType.Files, MediaType.Videos, FolderType.Null, order, sorttype), new DynamicParameters());
                }
                else if (mediatype == MediaType.Pictures)
                {
                    //Select all data within the Pictures Database
                    output = cnn.Query<Picture>(FormatLoad(ElementType.Files, MediaType.Pictures, FolderType.Null, order, sorttype), new DynamicParameters());
                }
                else if(mediatype == MediaType.Music)
                {
                    //Select all data within the Music Database
                    output = cnn.Query<Song>(FormatLoad(ElementType.Files, MediaType.Music, FolderType.Null, order, sorttype), new DynamicParameters());
                }
                else
                {
                    //Select all data within the Games Database
                    output = cnn.Query<Game>(FormatLoad(ElementType.Files, MediaType.Games, FolderType.Null, order, sorttype, isreverse), new DynamicParameters());

                    //Loop through each object in the output list
                    foreach (Game game in output)
                    {
                        //Deserialize Lists
                        game.Genres = DeserializeList(game.SerializedGenres);
                        game.AvailablePlatforms = DeserializeList(game.SerializedAvailablePlatforms);
                    }
                }

                //Convert selection to list and return it
                return output.ToList();
            }
        }


        // Update
        // =======================================================
        // =======================================================
        public static void UpdateFolder(int id, object folder, FolderType foldertype = FolderType.Folders)
        {
            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Check what folder type has to be updated
                if (foldertype == FolderType.Folders)
                {
                    //Update Folder in SQLite Database
                    cnn.Execute(FormatUpdate(id, folderFields, nameof(FolderType.Folders)), folder);
                }
                else if (foldertype == FolderType.TVShowFolders)
                {
                    //Convert Folder Object to TVShowFolder Object
                    TVShowFolder tvshowfolder = folder as TVShowFolder;

                    //Serialize Lists
                    tvshowfolder.SerializedGenres = SerializeList(tvshowfolder.Genres);
                    tvshowfolder.SerializedStars = SerializeList(tvshowfolder.Stars);
                    tvshowfolder.SerializedProductionCompanies = SerializeList(tvshowfolder.ProductionCompanies);
                    tvshowfolder.SerializedCreators = SerializeList(tvshowfolder.Creators);

                    //Update TV Show Folder in SQLite Database
                    cnn.Execute(FormatUpdate(id, tvshowfolderFields, nameof(FolderType.TVShowFolders)), tvshowfolder);
                }
                else if (foldertype == FolderType.SeasonFolders)
                {
                    //Update Season Folder in SQLite Database
                    cnn.Execute(FormatUpdate(id, seasonfolderFields, nameof(FolderType.SeasonFolders)), folder);
                }
            }
        }

        public static void UpdateItem(int id, object item, MediaType mediatype)
        {
            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Check what media type has to be updated
                if(mediatype == MediaType.Movies)
                {
                    //Convert Item Object to Movie Object
                    Movie movie = item as Movie;

                    //Serialize Lists
                    movie.SerializedGenres = SerializeList(movie.Genres);
                    movie.SerializedDirectors = SerializeList(movie.Directors);
                    movie.SerializedWriters = SerializeList(movie.Writers);
                    movie.SerializedStars = SerializeList(movie.Stars);
                    movie.SerializedProductionCompanies = SerializeList(movie.ProductionCompanies);

                    //Update Movie in SQLite Database
                    cnn.Execute(FormatUpdate(id, movieFields, nameof(MediaType.Movies)), movie);
                }
                else if (mediatype == MediaType.Episodes)
                {
                    //Convert Item Object to Episode Object
                    Episode episode = item as Episode;

                    //Serialize Lists
                    episode.SerializedGenres = SerializeList(episode.Genres);
                    episode.SerializedDirectors = SerializeList(episode.Directors);
                    episode.SerializedWriters = SerializeList(episode.Writers);
                    episode.SerializedStars = SerializeList(episode.Stars);
                    episode.SerializedProductionCompanies = SerializeList(episode.ProductionCompanies);

                    //Update Episode in SQLite Database
                    cnn.Execute(FormatUpdate(id, episodeFields, nameof(MediaType.Episodes)), episode);
                }
                else if (mediatype == MediaType.Videos)
                {
                    //Update Video in SQLite Database
                    cnn.Execute(FormatUpdate(id, videoFields, nameof(MediaType.Videos)), item);
                }
                else if (mediatype == MediaType.Pictures)
                {
                    //Update Picture in SQLite Database
                    cnn.Execute(FormatUpdate(id, pictureFields, nameof(MediaType.Pictures)), item);
                }
                else if (mediatype == MediaType.Music)
                {
                    //Update Song in Music SQLite Database
                    cnn.Execute(FormatUpdate(id, musicFields, nameof(MediaType.Music)), item);
                }
                else if (mediatype == MediaType.Games)
                {
                    //Convert Item Object to Game Object
                    Game game = item as Game;

                    //Serialize Lists
                    game.SerializedGenres = SerializeList(game.Genres);
                    game.SerializedAvailablePlatforms = SerializeList(game.AvailablePlatforms);

                    //Update Game in SQLite Database
                    cnn.Execute(FormatUpdate(id, gameFields, nameof(MediaType.Games)), game);
                }
            }
        }

        public static void UpdateFavourite(int id, int isfavourite, MediaType mediatype, FolderType foldertype = FolderType.Null)
        {
            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Validate Favourite Type
                if (foldertype == FolderType.Null)
                {
                    //Update is Favourite Value in SQLite Database
                    cnn.Execute($"update {mediatype} set isFavourite = {isfavourite} where Id='{id}'");
                }
                else
                {
                    //Update is Favourite Value in SQLite Database
                    cnn.Execute($"update {foldertype} set isFavourite = {isfavourite} where Id='{id}'");
                }
            }
        }


        // Remove
        // =======================================================
        // =======================================================
        public static void RemoveFolder(MediaType type, int selectedid)
        {
            //Initialize Varaibles
            int id = selectedid;
            List<TVShowFolder> tvshowfolders = new List<TVShowFolder>();
            List<Base> items = new List<Base>();
            Folder folder = new Folder(), oldfolder = new Folder();

            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Check if it is the initial loop or if no elements were found on the last loop
                while (oldfolder != null)
                {
                    //Set id to selectedid
                    id = selectedid;

                    //Set oldfolder to null
                    oldfolder = null;

                    //Set folder to new folder object
                    folder = new Folder();

                    //Check if folder is not equal to null
                    while (folder != null)
                    {
                        //Select a Single Folder within the Folders Database Where the Type Value is Equal to type and the OwnerID is Equal to id 
                        folder = cnn.Query<Folder>(FormatLoad(ElementType.Folders, type, FolderType.Folders), new DynamicParameters()).Any(i => i.OwnerId == id) ? cnn.Query<Folder>(FormatLoad(ElementType.Folders, type, FolderType.Folders), new DynamicParameters()).First(i => i.OwnerId == id) : null;

                        //Check if a folder was found
                        if (folder != null)
                        {
                            //Set oldfolder to folder
                            oldfolder = folder;

                            //Set id to located folder id
                            id = folder.Id;
                        }
                    }

                    //Validate Media Type
                    if (type == MediaType.TVShows)
                    {
                        //Select All Data within the TV Shows Database Where the Type Value is Equal to type and the OwnerID is Equal to id
                        tvshowfolders = cnn.Query<TVShowFolder>(FormatLoad(ElementType.Folders, MediaType.TVShows, FolderType.TVShowFolders, "Name", "ASC"), new DynamicParameters()).Where(i => i.OwnerId == id).ToList();

                        //Loop through TV Show Folders and Remove TV Show from Application
                        foreach (TVShowFolder item in tvshowfolders) { RemoveTVShowFolder(item); }
                    }
                    else
                    {
                        //Select All Data within the Specified Items Database Where the Type Value is Equal to type and the OwnerID is Equal to id
                        items = cnn.Query<Base>(FormatLoad(ElementType.Files, type), new DynamicParameters()).Where(i => i.OwnerId == id).ToList();

                        //Loop through Selected Items and Remove Item from Database
                        foreach (Base item in items) { RemoveItem(type, item.Id, item.CoverImage); }
                    }

                    //Delete Specified Folder from SQLite Database
                    cnn.Execute($"delete from Folders where Id = '{id}' and Type = '{type}'");
                }
            }
        }

        public static void RemoveTVShowFolder(TVShowFolder tvshowfolder)
        {
            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Variables
                List<SeasonFolder> seasonfolders = cnn.Query<SeasonFolder>(FormatLoad(ElementType.Folders, MediaType.TVShows, FolderType.SeasonFolders), new DynamicParameters()).Where(i => i.OwnerId == tvshowfolder.OwnerId).ToList();

                //Validate TV Show Folder
                if (tvshowfolder != null)
                {
                    //Loop through Season Folders
                    for (int i = 0; i < seasonfolders.Count; i++)
                    {
                        //Get Current Looped Season's Episodes
                        List<Episode> episodestoremove = cnn.Query<Episode>(FormatLoad(ElementType.Files, MediaType.Episodes), new DynamicParameters()).Where(j => j.OwnerId == seasonfolders[i].Id).ToList();

                        //Loop through Current Looped Seaon's Episodes
                        foreach (Episode item in episodestoremove)
                        {
                            //Remove Episode
                            RemoveItem(MediaType.Episodes, item.Id, string.Empty);
                        }

                        //Delete Current Looped Season Folder from SQLite Database
                        cnn.Execute($"delete from {FolderType.SeasonFolders} where Id = {seasonfolders[i].Id}");
                    }

                    //Try Delete TV Show Cover Image
                    if (!string.IsNullOrEmpty(tvshowfolder.CoverImage)) { try { File.Delete(tvshowfolder.CoverImage); } catch { } }

                    //Try Delete TV Show Custom Cover Image
                    if (!string.IsNullOrEmpty(tvshowfolder.CoverImage)) { try { File.Delete(tvshowfolder.CustomCoverImage); } catch { } }

                    //Delete Specified Season Folder from SQLite Database
                    cnn.Execute($"delete from {FolderType.TVShowFolders} where Id = {tvshowfolder.Id}");
                }
            }
        }

        public static void RemoveSeasonFolder(SeasonFolder seasonfolder, bool iscustomcoverimageused, bool iscustomcoverimageparent)
        {
            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Validate Season Folder
                if (seasonfolder != null)
                {
                    //Loop through Episodes
                    foreach (Episode item in cnn.Query<Episode>(FormatLoad(ElementType.Files, MediaType.Episodes), new DynamicParameters()).Where(i => i.OwnerId == seasonfolder.Id).ToList())
                    {
                        //Remove Episode
                        RemoveItem(MediaType.Episodes, item.Id, string.Empty);
                    }

                    //Try Delete Season Custom Cover Image
                    if (!string.IsNullOrEmpty(seasonfolder.CustomCoverImage) && !iscustomcoverimageused && !iscustomcoverimageparent) { try { File.Delete(seasonfolder.CustomCoverImage); } catch { } }

                    //Delete Specified Season Folder from SQLite Database
                    cnn.Execute($"delete from {FolderType.SeasonFolders} where Id = {seasonfolder.Id}");
                }
            }
        }

        public static void RemoveItem(MediaType mediatype, int id, string coverimage = "")
        {
            //Open a Protected Connection to the SQLite Database
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Check if the Cover Image has been Set
                if (!string.IsNullOrEmpty(coverimage))
                {
                    //Try Delete Cover Image
                    try { File.Delete(coverimage); } catch { }
                }

                //Delete Specified Item from SQLite Database
                cnn.Execute($"delete from {mediatype} where Id = {id}");
            }
        }
        #endregion Methods



        #region Extensions
        // Format Create
        // =======================================================
        // =======================================================
        private static string FormatCreate(List<string> values, ElementType type, MediaType mediatype = MediaType.Null, FolderType foldertype = FolderType.Null)
        {
            //Variables
            string name = "";

            //Check Element Type
            if(type == ElementType.Files)
            {
                //Get Table Name
                name = mediatype.ToString();
            }
            else if(type == ElementType.Folders)
            {
                //Get Table Name
                name = foldertype.ToString();
            }

            //Initialize tablestring
            string tablestring = $"create table {name} (";

            //Loop Through Elements in values List
            for (int i = 0; i < values.Count(); i++)
            {
                //Check if the Current Looped Element is Not the First Element within the List
                if (i != 0)
                {
                    //Assign value to tablestring
                    tablestring += $", {values[i]}";
                }
                else
                {
                    //Assign value to tablestring
                    tablestring += values[i];
                }
            }

            //Format and Return Table String
            return tablestring += ")";
        }


        // Format Load
        // =======================================================
        // =======================================================
        private static string FormatLoad(ElementType elementtype, MediaType mediatype, FolderType foldertype = FolderType.Null, string order = "", string sorttype = "", bool isreverse = false)
        {
            //Validate Element and Folder Type
            if(elementtype == ElementType.Folders && foldertype == FolderType.TVShowFolders)
            {
                //Format and Return Load String
                return $"select * from {FolderType.TVShowFolders} where [Type] = '{mediatype}' order by {order} {sorttype}";
            }
            else if (elementtype == ElementType.Folders)
            {
                //Format and Return Load String
                return $"select * from {foldertype} where [Type] = '{mediatype}' order by Name ASC";
            }

            //Check if the Sort Order and Type have been Set
            if (!string.IsNullOrEmpty(order) && !string.IsNullOrEmpty(sorttype))
            {
                //Split order String
                string[] values = order.Split('/');

                //Check if the Length of the Values Array is Above 1
                if (values.Length > 1)
                {
                    //Initialize load String
                    string load = $"select * from {mediatype} order by ";

                    //Loop Through Elements in the values Array
                    for (int i = 0; i < values.Length; i++)
                    {
                        //Check if the Current Looped Element is the Last Element within the values Array
                        if (i == values.Length - 1)
                        {
                            //Format and Return Load String
                            return $"{load}{values[i]} {GetSortType(sorttype, isreverse)}, CustomName ASC, Name ASC";
                        }
                        else
                        {
                            //Add Value to load String
                            load += $"{values[i]} {sorttype}, ";
                        }
                    }
                }
                else
                {
                    //Format and Return Load String
                    return $"select * from {mediatype} order by {order} {GetSortType(sorttype, isreverse)}, CustomName ASC, Name ASC";
                }
            }

            //Format and Return Load String
            return $"select * from {mediatype} order by CustomName ASC, Name ASC";
        }


        // Format Insert
        // =======================================================
        // =======================================================
        private static string FormatInsert(List<string> fields, string type)
        {
            //Initialize Insert String
            string insert = $"insert into {type} (";


            //Loop through the elements in the fields list
            for (int i = 0; i < fields.Count; i++)
            {
                //Check if the current looped element is the last element within the fields list
                if (i == fields.Count - 1)
                {
                    //Assign current looped field element into insert string
                    insert += fields[i];
                }
                else
                {
                    //Assign current looped field element into insert string with the addition of a comma
                    insert += $"{fields[i]},";
                }
            }


            //Assign value into insert string
            insert += ") values (";


            //Loop through the elements in the fields list
            for (int i = 0; i < fields.Count; i++)
            {
                //Check if the current looped element is the last element within the fields list
                if (i == fields.Count - 1)
                {
                    //Assign current looped field element into insert string with the addition of @ symbol and bracket
                    insert += $"@{fields[i]})";
                }
                else
                {
                    //Assign current looped field element into insert string with the addition of a @ symbol and comma
                    insert += $"@{fields[i]},";
                }
            }


            //Return insert string
            return insert;
        }


        // Format Update
        // =======================================================
        // =======================================================
        private static string FormatUpdate(int id, List<string> fields, string type)
        {
            //Initialize Update String
            string update = $"update {type} set ";

            //Loop through fields list
            for (int i = 0; i < fields.Count(); i++)
            {
                //Check if the current looped element is the last element within the fields list
                if (i == fields.Count() - 1)
                {
                    //Add field to update variable
                    update += $"{fields[i]} = @{fields[i]} ";
                }
                else
                {
                    //Add field to update variable
                    update += $"{fields[i]} = @{fields[i]}, ";
                }
            }

            //Add id to update variable
            update += $"where Id='{id}'";

            //Return update String
            return update;
        }


        // Serialize / Deserialize
        // =======================================================
        // =======================================================
        private static string SerializeList(List<string> values)
        {
            //Variables
            string serialized = "";

            //Check if the values list contains any elements
            if (values != null && values.Count > 0)
            {
                //Loop through vaLues
                for (int i = 0; i < values.Count; i++)
                {
                    //Check if the current value is the last value within the values list
                    if (i == values.Count - 1)
                    {
                        //Add value to serialized variable
                        serialized += values[i];
                    }
                    else
                    {
                        //Add value to serialized variable with separator
                        serialized += $"{values[i]}~";
                    }
                }
            }

            //Return serialized variable
            return serialized;
        }

        private static List<string> DeserializeList(string value)
        {
            //Check if value is not null or empty
            if (!string.IsNullOrEmpty(value))
            {
                //Separate value variable
                string[] values = value.Split('~');

                //Convert values array to a list and return it
                return values.ToList();
            }

            //Return Empty List
            return new List<string>();
        }


        // Get Sort Type
        // =======================================================
        // =======================================================
        private static string GetSortType(string type, bool isreverse)
        {
            //Validate and Return Reverse Type
            if (isreverse == true && type == "ASC") { return "DESC"; }
            else if (isreverse == true && type == "DESC") { return "ASC"; }

            //Return Type
            return type;
        }
        #endregion Extensions
    }
}