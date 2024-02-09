using System.IO;

namespace Media_Manager
{
    public class CoverImages
    {
        // Clean Folder
        // =====================================================================
        // =====================================================================
        public static void Clean(string directory)
        {
            //Get All Files from Folder
            string[] files = Directory.GetFiles(directory);

            //Loop through elements in files array
            foreach (string file in files)
            {
                //Check if File is a Temporary File
                if (Path.GetFileNameWithoutExtension(file).EndsWith("_temp"))
                {
                    //Try Delete File
                    try { File.Delete(file); } catch { }
                }
            }
        }
    }
}