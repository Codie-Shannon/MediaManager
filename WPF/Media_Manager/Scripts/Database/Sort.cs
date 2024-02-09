using MediaControlsLibrary;

namespace Media_Manager
{
    public class Sort
    {
        // Get Order
        // ===================================================
        // ===================================================
        public static string GetOrder(subComboBox comboBox)
        {
            //Check if the ComboBox Order Variable has been Set
            if (comboBox.Order != null)
            {
                //Return Order Variable
                return comboBox.Order;
            }

            //Return the Default Value
            return "CustomName, Name";
        }


        // Get Type
        // ===================================================
        // ===================================================
        public static string GetType(subComboBox comboBox)
        {
            //Check if the ComboBox Type Variable has been Set
            if (comboBox.Type != null)
            {
                //Return Type Variable
                return comboBox.Type;
            }

            //Return Ascending Type
            return "ASC";
        }
    }
}