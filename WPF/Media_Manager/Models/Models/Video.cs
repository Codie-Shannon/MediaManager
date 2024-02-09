using Media_Manager.Models.BaseModels;

namespace Media_Manager.Models
{
    public class Video : AVBase
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


        // Framerate
        private double _framerate;

        public double Framerate { get => _framerate; set { _framerate = value; } }



        // Advance Details
        // ===============================================================
        // ===============================================================
        // Framerate Mode
        private string _framerateMode;

        public string FramerateMode { get => _framerateMode; set { _framerateMode = value; } }
    }
}