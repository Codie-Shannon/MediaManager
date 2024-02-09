using System;
using System.IO;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace Media_Manager
{
    public class Formatter
    {
        #region Main
        // Cover Image
        // =======================================================
        // =======================================================
        public static BitmapImage FormatImage(MediaType type, string filepath, string altfilepath = "")
        {
            //Validate Cover Images
            if (!string.IsNullOrEmpty(altfilepath) && Validation.File(altfilepath))
            {
                //Format and Return Cover Image
                return new BitmapImage(new Uri(altfilepath, UriKind.Absolute));
            }
            else if (!string.IsNullOrEmpty(filepath) && Validation.File(filepath))
            {
                //Format and Return Cover Image
                return new BitmapImage(new Uri(filepath, UriKind.Absolute));
            }

            //Format and Return Default Cover Image
            return new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}\\Textures\\{type}_Cover_Default.png"));
        }


        // Name
        // =======================================================
        // =======================================================
        public static string FormatName(string name, string customname)
        {
            //Check if customname is null or empty
            if (string.IsNullOrEmpty(customname))
            {
                //Return name
                return name;
            }

            //Return customname
            return customname;
        }


        // Resolution
        // =======================================================
        // =======================================================
        public static string FormatResolution(string width, string height)
        {
            //Validate, Format and Return Resolution
            return !string.IsNullOrEmpty(width) && !string.IsNullOrEmpty(height) ? $"{width} x {height}" : string.Empty;
        }


        // Duration
        // =======================================================
        // =======================================================
        public static string FormatDuration(double value)
        {
            //Convert value to TimeSpan
            TimeSpan t = TimeSpan.FromMilliseconds(value);

            //Format TimeSpan
            string duration = string.Format("{0:D1}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds).TrimStart(' ', ':', '0');

            //Return duration
            return duration;
        }


        // File Size
        // =======================================================
        // =======================================================
        public static string FormatFileSize(double size, string sizeLabel = "")
        {
            if (size > 1024)
            {
                if (sizeLabel.Length == 0)
                    return FormatFileSize(size / 1024, " KB");
                else if (sizeLabel == " KB")
                    return FormatFileSize(size / 1024, " MB");
                else if (sizeLabel == " MB")
                    return FormatFileSize(size / 1024, " GB");
                else if (sizeLabel == " GB")
                    return FormatFileSize(size / 1024, " TB");
                else
                    return FormatFileSize(size / 1024, " PB");
            }
            else
            {
                if (sizeLabel.Length > 0)
                    return string.Concat(size.ToString("0.00"), sizeLabel);
                else
                    return string.Empty;
            }
        }


        // Creation
        // =======================================================
        // =======================================================
        public static string FormatCreation(string time, string date)
        {
            //Format Date
            date = FormatDate(date, "yyyy-MM-dd", "dddd, dd MMMM yyyy");

            //Format Time
            time = FormatDate(time, "HH-mm-ss", "t");

            //Format and Return Creation Time and Date
            return $"{time} {date}";
        }


        // Sample Rate
        // =======================================================
        // =======================================================
        public static string FormatSampleRate(double rate, string rateLabel = "")
        {
            if (rate > 1000)
            {
                if (rateLabel.Length == 0)
                    return FormatSampleRate(rate / 1000, " KHz");
                else if (rateLabel == " KHz")
                    return FormatSampleRate(rate / 1000, " MHz");
                else
                    return FormatSampleRate(rate / 1000, " GHz");
            }
            else
            {
                if (rateLabel.Length > 0)
                    return string.Concat(rate.ToString("0.0"), rateLabel);
                else
                    return string.Empty;
            }
        }
        #endregion Main



        #region Picture
        // Bit Depth
        // =======================================================
        // =======================================================
        public static string FormatBitDepth(string bitdepth)
        {
            //Format and Return BitDepth
            return $"{bitdepth} Bits";
        }
        #endregion Picture



        #region Video
        // Framerate
        // =======================================================
        // =======================================================
        public static string FormatFramerate(double framerate)
        {
            //Validate, Format and Return Framerate
            return framerate == 0 ? string.Empty : framerate.ToString("0.00") + " FPS";
        }


        // Framerate Mode
        // =======================================================
        // =======================================================
        public static string FormatFramerateMode(string mode)
        {
            //Check if framerate mode is constant (CFR) or variable (VFR)
            if (mode == "CFR")
            {
                //Format Framerate Mode
                mode = "Constant";
            }
            else if (mode == "VFR")
            {
                //Format Framerate Mode
                mode = "Variable";
            }

            //Return Framerate Mode
            return mode;
        }
        #endregion Video



        #region Virtual Entertainment
        // IMDB Release Date
        // =======================================================
        // =======================================================
        public static string FormatVirtualEntertainmentReleaseDate(string date, string region)
        {
            //Format Date
            date = FormatDate(date.Trim(), "MMMM d, yyyy", "dddd, dd MMMM yyyy");

            //Format and Return Release Date
            return string.IsNullOrEmpty(region) ? date : $"{date} ({region})";
        }


        // Numeric
        // =======================================================
        // =======================================================
        public static string FormatNumeric(int count)
        {
            //Format and Return Numeric
            return count == 0 ? string.Empty : count.ToString();
        }
        #endregion Virtual Entertainment



        #region Game
        // IGDB Release Date
        // =======================================================
        // =======================================================
        public static string FormatGameReleaseDate(string date)
        {
            //Check if date is Not Null or Empty
            if (!string.IsNullOrEmpty(date))
            {
                //Get Years Since Release Date
                int span = (DateTime.Now - Convert.ToDateTime(date, CultureInfo.InvariantCulture)).Duration().Days / 365;

                //Format Date
                date = FormatDate(date.Trim(), "MMM d, yyyy", "dddd, dd MMMM yyyy");

                //Format and Return Release Date
                return $"{date} ({span} Years)";
            }

            //Return Empty String
            return string.Empty;
        }
        #endregion Game



        #region Extensions
        // Format Date
        // =======================================================
        // =======================================================
        public static string FormatDate(string date, string originalFormat, string newFormat)
        {
            //Convert String to Date
            DateTime temp = DateTime.ParseExact(date, originalFormat, CultureInfo.InvariantCulture);

            //Format and Return Date
            return temp.ToString(newFormat);
        }
        #endregion Extensions
    }
}