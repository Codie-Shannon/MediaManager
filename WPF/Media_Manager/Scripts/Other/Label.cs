using MediaControlsLibrary.Dependencies;
using MediaControlsLibrary.Types;
using System.Windows;
using System.Windows.Controls;

namespace Media_Manager
{
    public class Label
    {
        // Get Prefix
        // ===========================================================
        // ===========================================================
        public static string GetPrefix(object sender, string prefix1 = "cmi", string prefix2 = "btn")
        {
            //Get and Return Prefix
            return ((Control)sender).Name.ToString().Contains(prefix1) ? prefix1 : prefix2;
        }


        // Get
        // ===========================================================
        // ===========================================================
        public static string Tag(object sender, string prefix = "")
        {
            //Check if a prefix has not been set
            if (string.IsNullOrEmpty(prefix))
            {
                //Return Tag
                return ((FrameworkElement)sender).Tag.ToString().ToLower();
            }
            else
            {
                //Format and Return Tag
                return ((FrameworkElement)sender).Tag.ToString().ToLower().Replace(prefix, "");
            }
        }

        public static string Name(object sender, string prefix = "")
        {
            //Check if a prefix has not been set
            if (string.IsNullOrEmpty(prefix))
            {
                //Return Tag
                return ((FrameworkElement)sender).Name.ToString().ToLower();
            }
            else
            {
                //Format and Return Tag
                return ((FrameworkElement)sender).Name.ToString().ToLower().Replace(prefix, "");
            }
        }

        public static int Id(object sender)
        {
            //Get and Return Id
            return ((ElementBase)sender).Id;
        }

        public static FolderType FolderType(object sender)
        {
            return ((FolderBase)sender).FolderType;
        }
    }
}