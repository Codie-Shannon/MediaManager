namespace Media_Manager
{
    // Element Types
    public enum ElementType { Null, Files, Folders }


    // Media Types
    // =====================================================
    public enum MediaType { Null, Movies, TVShows, Seasons, Episodes, Videos, Pictures, Music, Games }


    // Driver Type
    // =====================================================
    public enum DriverType { Null, Default, IMDB }


    // Item Types
    // =====================================================
    public enum ItemType { CoverItem, ImageItem }


    // Option Panel Types
    // =====================================================
    public enum OptionPanelType { Null, Add, Edit }


    // Pane Toggle
    // =====================================================
    public enum PaneToggle { Open, Close }


    // Removal Types
    // =====================================================
    public enum RemovalType { Remove, Delete }


    // File Type
    // =====================================================
    public enum FileType { File, Folder }


    // Skip Item Types
    // =====================================================
    public enum SkipItemType { Previous, Next }


    // Slider Types
    // =====================================================
    public enum SliderType { Start, Stop }


    // Mouse Handling Mode (Zoom)
    // =====================================================
    public enum MouseHandlingMode { None, Pan, Zoom }


    // Rotation Types
    // =====================================================
    public enum RotationType { Left, Right }
}