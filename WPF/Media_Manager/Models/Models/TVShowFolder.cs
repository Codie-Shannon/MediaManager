using Media_Manager.Models.BaseModels;
using System.Collections.Generic;

namespace Media_Manager.Models
{
    public class TVShowFolder : FolderBase
    {
        // Web Details
        // ===============================================================
        // ===============================================================
        // Season Count
        private int _seasonCount;

        public int SeasonCount { get => _seasonCount; set { _seasonCount = value; } }


        // Episode Count
        private int _episodeCount;

        public int EpisodeCount { get => _episodeCount; set { _episodeCount = value; } }


        // Release Date
        private string _releaseDate;

        public string ReleaseDate { get => _releaseDate; set { _releaseDate = value; } }


        // Release Period
        private string _releasePeriod;

        public string ReleasePeriod { get => _releasePeriod; set { _releasePeriod = value; } }


        // Age Rating
        private string _ageRating;

        public string AgeRating { get => _ageRating; set { _ageRating = value; } }


        // Genres
        private List<string> _genres;

        public List<string> Genres { get => _genres; set { _genres = value; } }

        public string SerializedGenres { get; set; }


        // Stars
        private List<string> _stars;

        public List<string> Stars { get => _stars; set { _stars = value; } }

        public string SerializedStars { get; set; }


        // Production Companies
        private List<string> _productionCompanies;

        public List<string> ProductionCompanies { get => _productionCompanies; set { _productionCompanies = value; } }

        public string SerializedProductionCompanies { get; set; }



        // IMDB Details
        // ===============================================================
        // ===============================================================
        // IMDB Link
        private string _imdbLink;

        public string IMDBLink { get => _imdbLink; set { _imdbLink = value; } }


        // Region
        private string _region;

        public string Region { get => _region; set { _region = value; } }


        // Creators
        private List<string> _creators;

        public List<string> Creators { get => _creators; set { _creators = value; } }

        public string SerializedCreators { get; set; }



        // Metacritic Details
        // ===============================================================
        // ===============================================================
        // MetaCritic Link
        private string _metacriticLink;

        public string MetaCriticLink { get => _metacriticLink; set { _metacriticLink = value; } }


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
    }
}