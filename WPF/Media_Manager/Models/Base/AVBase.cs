namespace Media_Manager.Models.BaseModels
{
    public class AVBase : Base
    {
        // Standard Details
        // ===============================================================
        // ===============================================================
        // Duration
        private double _duration;

        public double Duration { get => _duration; set { _duration = value; } }


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
        // Sample Rate
        private double _sampleRate;

        public double SampleRate { get => _sampleRate; set { _sampleRate = value; } }


        // Audio Channels
        private string _audioChannels;

        public string AudioChannels { get => _audioChannels; set { _audioChannels = value; } }
    }
}