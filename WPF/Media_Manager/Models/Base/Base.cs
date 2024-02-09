namespace Media_Manager.Models.BaseModels
{
    public class Base
    {
        // Database ID
        // ===============================================================
        // ===============================================================
        private int _id;

        public int Id { get => _id; set { _id = value; } }



        // Owner's Database ID
        // ===============================================================
        // ===============================================================
        private int _ownerId;

        public int OwnerId { get => _ownerId; set { _ownerId = value; } }



        // Is Favourite
        // ===============================================================
        // ===============================================================
        private int _isfavourite;

        public int isFavourite { get => _isfavourite; set { _isfavourite = value; } }



        // Name
        // ===============================================================
        // ===============================================================
        // Name
        private string _name;

        public string Name { get => _name; set { _name = value; } }


        // Custom Name
        private string _customName;

        public string CustomName { get => _customName; set { _customName = value; } }



        // Cover Image
        // ===============================================================
        // ===============================================================
        private string _coverImage;

        public string CoverImage { get => _coverImage; set { _coverImage = value; } }



        // File Path
        // ===============================================================
        // ===============================================================
        private string _filePath;

        public string FilePath { get => _filePath; set { _filePath = value; } }
    }
}
