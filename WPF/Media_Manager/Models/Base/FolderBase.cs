using MediaControlsLibrary.Models;

namespace Media_Manager.Models.BaseModels
{
    public class FolderBase : Folder
    {
        // Standard Web Details
        // ===============================================================
        // ===============================================================
        // Release Date
        private string _releaseDate;

        public string ReleaseDate { get => _releaseDate; set { _releaseDate = value; } }


        // Creation Time
        private string _creationTime;

        public string CreationTime { get => _creationTime; set { _creationTime = value; } }


        // Creation Date
        private string _creationDate;

        public string CreationDate { get => _creationDate; set { _creationDate = value; } }



        // IMDB Details
        // ===============================================================
        // ===============================================================
        // IMDB Link
        private string _imdbLink;

        public string IMDBLink { get => _imdbLink; set { _imdbLink = value; } }



        // MetaCritic Details
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