using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Media_Manager.Converters
{
    public class CoverImageMultiConverter : IMultiValueConverter
    {
        // Converter
        // =========================================================
        // =========================================================
        public object Convert(object[] values, Type targetType, object param, CultureInfo culture)
        {
            //Convert Objects to Strings
            string customcoverimage = $"{values[0]}";
            string coverimage = $"{values[1]}";
            string parameter = $"{param}";

            //Check if Cover Image Exists
            if (Validation.File(customcoverimage))
            {
                //Return Value
                return customcoverimage;
            }
            else if (Validation.File(coverimage))
            {
                //Return Value
                return coverimage;
            }

            //Return Default Cover Image
            return $"{Directory.GetCurrentDirectory()}\\Textures\\{parameter}_Cover_Default.png";
        }


        #region Not Implemented
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
        #endregion Not Implemented
    }
}
