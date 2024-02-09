using System.Windows;
using Media_Manager.Models;
using System.Windows.Controls;
using MediaControlsLibrary.Models;

namespace Media_Manager.Selectors
{
    public class ElementSelector : DataTemplateSelector
    {
        // Data Template Selector
        // ======================================================
        // ======================================================
        public override DataTemplate SelectTemplate(object item, DependencyObject owner)
        {
            //Get Owner
            FrameworkElement container = (FrameworkElement)owner;

            //Check if the Owner was Parsed Successfully
            if(container != null && item != null)
            {
                //Check if the Element's Model was Item or Folder
                if(!(item is Folder f))
                {
                    //Get and Return DataTemplate
                    return container.FindResource("ItemTemplate") as DataTemplate;
                }
                else if (item is TVShowFolder)
                {
                    //Get and Return DataTemplate
                    return container.FindResource("TVShowFolderTemplate") as DataTemplate;
                }
                else if (item is SeasonFolder)
                {
                    //Get and Return DataTemplate
                    return container.FindResource("SeasonFolderTemplate") as DataTemplate;
                }
                else
                {
                    //Get and Return DataTemplate
                    return container.FindResource("FolderTemplate") as DataTemplate;
                }
            }

            //Return Null
            return null;
        }
    }
}