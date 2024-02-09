using System.Collections.Generic;
using Media_Manager.Models.BaseModels;

namespace Media_Manager.Models
{
    public class Game : Base
    {
        // File Paths
        // ===============================================================
        // ===============================================================
        // Base Directory
        private string _baseDirectory;

        public string BaseDirectory { get => _baseDirectory; set { _baseDirectory = value; } }



        // Standard Details
        // ===============================================================
        // ===============================================================
        // Format
        private string _format;

        public string Format { get => _format; set { _format = value; } }


        // File Size
        private double _fileSize;

        public double FileSize { get => _fileSize; set { _fileSize = value; } }


        // Creation Time
        private string _creationTime;

        public string CreationTime { get => _creationTime; set { _creationTime = value; } }


        // Creation Date
        private string _creationDate;

        public string CreationDate { get => _creationDate; set { _creationDate = value; } }



        // IGDB Details
        // ===============================================================
        // ===============================================================
        // IGDB Link
        private string _igdbLink;

        public string IGDBLink { get => _igdbLink; set { _igdbLink = value; } }


        // Publisher
        private string _publisher;

        public string Publisher { get => _publisher; set { _publisher = value; } }


        // Release Date
        private string _releaseDate;

        public string ReleaseDate { get => _releaseDate; set { _releaseDate = value; } }


        // Type
        private string _type;

        public string Type { get => _type; set { _type = value; } }


        // User Score
        private float _userScore;

        public float UserScore { get => _userScore; set { _userScore = value; } }


        // User Review Count
        private int _userReviewCount;

        public int UserReviewCount { get => _userReviewCount; set { _userReviewCount = value; } }


        // Critic Score
        private float _criticScore;

        public float CriticScore { get => _criticScore; set { _criticScore = value; } }


        // Critic Review Count
        private int _criticReviewCount;

        public int CriticReviewCount { get => _criticReviewCount; set { _criticReviewCount = value; } }


        // Genres
        private List<string> _genres;

        public List<string> Genres { get => _genres; set { _genres = value; } }

        public string SerializedGenres { get; set; }


        // Available Platforms
        private List<string> _availablePlatforms;

        public List<string> AvailablePlatforms { get => _availablePlatforms; set { _availablePlatforms = value; } }

        public string SerializedAvailablePlatforms { get; set; }
    }
}