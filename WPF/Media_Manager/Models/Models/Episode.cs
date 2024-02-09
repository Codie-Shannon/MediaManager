using Media_Manager.Models.BaseModels;
using System.Collections.Generic;

namespace Media_Manager.Models
{
    public class Episode : Video
    {
        // Standard Details
        // ===============================================================
        // ===============================================================
        // Season
        private int _season;

        public int Season { get => _season; set { _season = value; } }


        // Episode Number
        private int _episodeNumber;

        public int EpisodeNumber { get => _episodeNumber; set { _episodeNumber = value; } }



        // Web Details
        // ===============================================================
        // ===============================================================
        // Air Date
        private string _airDate;

        public string AirDate { get => _airDate; set { _airDate = value; } }


        // Directors
        private List<string> _directors;

        public List<string> Directors { get => _directors; set { _directors = value; } }

        public string SerializedDirectors { get; set; }


        // Writers
        private List<string> _writers;

        public List<string> Writers { get => _writers; set { _writers = value; } }

        public string SerializedWriters { get; set; }


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



        // Metacritic Details
        // ===============================================================
        // ===============================================================
        // MetaCritic Link
        private string _metacriticLink;

        public string MetaCriticLink { get => _metacriticLink; set { _metacriticLink = value; } }
    }
}