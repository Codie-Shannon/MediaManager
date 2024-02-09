using System;
using System.Windows.Data;
using System.Globalization;

namespace Media_Manager.Converters
{
    public class SelectionConverter : IMultiValueConverter
    {
        // Converter
        // =========================================================
        // =========================================================
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //Check if the selected ID is equal to the current ID and if it is, return true to select the item, else return false
            return values[0].ToString() == values[1].ToString() ? true : false;
        }


        #region Not Implemented
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion Not Implemented
    }
}