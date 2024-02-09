using Media_Manager.Models.BaseModels;

namespace Media_Manager.Models
{
    public class SeasonFolder : FolderBase
    {
        // Standard Details
        // ===============================================================
        // ===============================================================
        // Season Number
        private int _seasonNumber;

        public int SeasonNumber { get => _seasonNumber; set { _seasonNumber = value; } }



        // Web Details
        // ===============================================================
        // ===============================================================
        // Episode Count
        private int _episodeCount;

        public int EpisodeCount { get => _episodeCount; set { _episodeCount = value; } }


        // Release Date
        private string _releaseDate;

        public string ReleaseDate { get => _releaseDate; set { _releaseDate = value; } }



        // IMDB Details
        // ===============================================================
        // ===============================================================
        // IMDB Link
        private string _imdbLink;

        public string IMDBLink { get => _imdbLink; set { _imdbLink = value; } }



        // Metacritic Details
        // ===============================================================
        // ===============================================================
        // MetaCritic Link
        private string _metacriticLink;

        public string MetaCriticLink { get => _metacriticLink; set { _metacriticLink = value; } }
    }
}