using System.Net;

namespace Media_Manager
{
    public class Internet
    {
        // Validate Connection
        // ==================================================
        // ==================================================
        public static bool Validate()
        {
            //Create Web Client Object
            WebClient webClient = new WebClient();
            
            try
            {
                //Download Small File
                byte[] data = webClient.DownloadData("https://www.microsoft.com/favicon.ico");

                //Check if data has a length above 0
                if (data.Length > 0)
                {
                    //Return true
                    return true;
                }
            }
            catch { }

            //Return false
            return false;
        }
    }
}