using Media_Manager.Models.BaseModels;

namespace Media_Manager.Models
{
    public class Song : AVBase
    {
        // Composition Mode
        // ===============================================================
        // ===============================================================
        private string _compMode;

        public string CompMode { get => _compMode; set { _compMode = value; } }
    }
}