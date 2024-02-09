using System;
using System.IO;
using System.Windows.Data;
using System.Globalization;

namespace Media_Manager.Converters
{
    public class CoverImageConverter : IValueConverter
    {
        // Converter
        // =========================================================
        // =========================================================
        public object Convert(object val, Type targetType, object param, CultureInfo culture)
        {
            //Convert Objects to Strings
            string value = $"{val}";
            string parameter = $"{param}";

            //Check if Cover Image Exists
            if (Validation.File(value))
            {
                //Return Value
                return value;
            }

            //Return Default Cover Image
            return $"{Directory.GetCurrentDirectory()}\\Textures\\{parameter}_Cover_Default.png";
        }


        #region Not Implemented
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion Not Implemented
    }
}