using Media_Manager.Models.BaseModels;

namespace Media_Manager.Models
{
    public class Picture : Base
    {
        // Standard Details
        // ===============================================================
        // ===============================================================
        // Width
        private string _width;

        public string Width { get => _width; set { _width = value; } }


        // Height
        private string _height;

        public string Height { get => _height; set { _height = value; } }


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



        // Advance Details
        // ===============================================================
        // ===============================================================
        // Colour Space
        private string _colourSpace;

        public string ColourSpace { get => _colourSpace; set { _colourSpace = value; } }


        // Bit Depth
        private string _bitDepth;

        public string BitDepth { get => _bitDepth; set { _bitDepth = value; } }


        // Compression Mode
        private string _compMode;

        public string CompMode { get => _compMode; set { _compMode = value; } }
    }
}