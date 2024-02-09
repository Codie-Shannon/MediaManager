using System.Linq;
using OpenQA.Selenium;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediaControlsLibrary.Models;
using System.Threading;
using System.Windows;
using System;
using MediaControlsLibrary;
using SeleniumUndetectedChromeDriver;
using System.Diagnostics;

namespace Media_Manager
{
    public class SearchEngine
    {
        #region Variables
        // Chrome Driver
        // =================================================
        // =================================================
        private static UndetectedChromeDriver DefaultDriver;
        private static UndetectedChromeDriver IMDBDriver;


        // Search Thread
        // =================================================
        // =================================================
        private static Thread SearchThread;
        #endregion Variables



        #region Operation
        // Start
        // =================================================
        // =================================================
        public static async Task StartAsync(string defaultlink, string imdblink = "")
        {
            //Validate Internet Connection
            if (Internet.Validate())
            {
                //Set Active Window to Topmost
                WindowsAPI.ToggleActiveWindow(true);

                //Create Default Driver
                DefaultDriver = UndetectedChromeDriver.Create(null, null, await new ChromeDriverInstaller().Auto(), null, 0, 0, false, true, true, true);

                //Validate and Create IMDB Driver
                if (!string.IsNullOrEmpty(imdblink)) { IMDBDriver = UndetectedChromeDriver.Create(null, null, await new ChromeDriverInstaller().Auto(), null, 0, 0, false, true, true, true); }

                //Hide Chrome Driver Windows
                WindowsAPI.HideChromeDriverWindows();

                //Unset Active Window Topmost
                WindowsAPI.ToggleActiveWindow(false);

                //Navigate Default Driver to the defaultlink Variable Value
                DefaultDriver.Navigate().GoToUrl(defaultlink);

                //Navigate IMDB Driver to the imdblink Variable Value
                if (!string.IsNullOrEmpty(imdblink)) { IMDBDriver.Navigate().GoToUrl(imdblink); }
            }
        }


        // Abort
        // =================================================
        // =================================================
        public static async Task Abort(optSearchBox searchbox)
        {
            //Show Loading Text
            searchbox.isLoading = true;

            //Abort Any Previous Search Operation
            await Task.Run(() =>
            {
                //Validate Search Thread
                if (SearchThread != null)
                {
                    //Abort Search Thread
                    SearchThread.Abort();
                    SearchThread = null;

                    //Stop Search
                    Stop();
                }
            });

            //Hide Loading Text
            searchbox.isLoading = false;
        }


        // Stop
        // =================================================
        // =================================================
        public static void Stop()
        {
            //Validate Search Thread
            if (SearchThread != null)
            {
                //Abort Search Thread
                SearchThread.Abort();
                SearchThread = null;
            }

            //Validate Default Driver
            if (DefaultDriver != null)
            {
                //Close and Unset Default Driver
                DefaultDriver.Quit();
                DefaultDriver = null;
            }

            //Validate IMDB Driver
            if (IMDBDriver != null)
            {
                //Close and Unset IMDB Driver
                IMDBDriver.Quit();
                IMDBDriver = null;
            }
        }
        #endregion Operation



        #region Search
        // Virtual Entertainment Search
        // ===========================================================
        // ===========================================================
        public static void VirtualEntertainmentSearch(optSearchBox searchbox, string search, MediaType mediatype = MediaType.Movies)
        {
            //Show Loading Text
            searchbox.isLoading = true;

            //Create Search Thread
            SearchThread = new Thread(async () =>
            {
                //Start Search
                await StartAsync(FormatURL(mediatype, search, "https://www.metacritic.com/search/"), FormatURL(mediatype, search, "https://www.imdb.com/find/?q=", "&s=tt&ttype=ft&ref_=fn_ft", "&s=tt&ttype=tv&ref_=fn_tv"));

                //Run Metacritic and IMDB Search on Different Threads
                List<MovieSearch> metacriticresults = await Task.Run(() => RetrieveVirtualEntertainmentResultsAsync(DriverType.Default, DefaultDriver, "c-pageSiteSearch-results-item", mediatype));
                List<MovieSearch> imdbresults = await Task.Run(() => RetrieveVirtualEntertainmentResultsAsync(DriverType.IMDB, IMDBDriver, "find-result-item", mediatype));

                //Stop Search
                Stop();

                //Combine Virtual Entertainment Results
                List<MovieSearch> results = CombineVirtualEntertainmentResults(metacriticresults, imdbresults);

                //Run Task on UI Thread
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Clear Search Box Results
                    searchbox.Clear(true);

                    //Loop through results List
                    foreach (MovieSearch movie in results)
                    {
                        //Add Current Looped Virtual Entertainment Item to Search Box Results Observable Collection
                        searchbox.Add(movie.Name, movie.CoverImage, movie.MetacriticLink, movie.IMDBLink);
                    }

                    //Hide Loading Text
                    searchbox.isLoading = false;
                }));
            });

            //Run Search Thread
            SearchThread.Start();
        }

        private static async Task<List<MovieSearch>> RetrieveVirtualEntertainmentResultsAsync(DriverType type, IWebDriver driver, string element_classname, MediaType mediatype)
        {
            //Variables
            List<MovieSearch> results = new List<MovieSearch>();

            //Validate Search Items
            if (IsElementPresent(type, By.ClassName(element_classname)))
            {
                //Check if the Driver Type is Metacritic
                if (type == DriverType.Default)
                {
                    //Get Category Index (Movies = 3, TV Shows = 4)
                    int index = mediatype == MediaType.Movies ? 3 : 4;

                    //Get Metacritic Window
                    Process metacriticwindow = WindowsAPI.RetrieveProcesses("Metacritic").FirstOrDefault();

                    //Set Foreground Window
                    WindowsAPI.SetWindowFocus(metacriticwindow.MainWindowHandle);

                    //Get Category Button
                    IWebElement categorybtn = driver.FindElement(By.XPath($"/html/body/div[1]/div/div/div[2]/div[1]/div[1]/div/div/ul/li[{index}]"));

                    //Click Category Button
                    categorybtn.Click();

                    //Wait for Category Button to be Clicked
                    await Task.Delay(500);

                    //Set Foreground Window
                    WindowsAPI.SetWindowFocus();

                    //Wait for Website to Load Category
                    await Task.Delay(1000);
                }

                //Get Search Items
                ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.ClassName(element_classname));

                //Loop through Search Items
                foreach (IWebElement element in elements)
                {
                    //Add Search Item to results List
                    results.Add(new MovieSearch()
                    {
                        Name = type == DriverType.Default ? GetMetacriticTitle(element) : GetIMDBTitle(element),
                        CoverImage = type == DriverType.Default ? GetMetacriticCover(element) : GetIMDBCover(element),
                        MetacriticLink = type == DriverType.Default ? GetMetacriticLink(element) : string.Empty,
                        IMDBLink = type == DriverType.Default ? string.Empty : GetIMDBLink(element)
                    });
                }
            }

            //Return results List
            return results;
        }

        private static List<MovieSearch> CombineVirtualEntertainmentResults(List<MovieSearch> metacriticresults, List<MovieSearch> imdbresults)
        {
            //Get Loop Type
            string type = metacriticresults.Count > imdbresults.Count ? "imdb" : "metacritic";

            //Loop through the Shortest Results List
            foreach (MovieSearch item in type == "imdb" ? imdbresults : metacriticresults)
            {
                //Convert Current Looped Virtual Entertainment Item Title to Lowercase
                string title = item.Name.ToLower();

                //Validate Current Looped Movie Match
                if (type == "imdb" && metacriticresults.Any(i => i.Name.ToLower() == title))
                {
                    //Get Match
                    MovieSearch match = metacriticresults.FirstOrDefault(i => i.Name.ToLower() == title);

                    //Assign Match's Metacritic Link Value into the Current Looped Virtual Entertainment Item's Metacritic Link Value
                    item.MetacriticLink = match.MetacriticLink;

                    //Attempt to Get Cover Image
                    item.CoverImage = string.IsNullOrEmpty(item.CoverImage) ? match.CoverImage : item.CoverImage;

                    //Remove Match from Metacritic Results List
                    metacriticresults.Remove(match);
                }
                else if (type == "metacritic" && imdbresults.Any(i => i.Name.ToLower() == title))
                {
                    //Get Match
                    MovieSearch match = imdbresults.First(i => i.Name.ToLower() == title);

                    //Assign Match's IMDB Link Value into the Current Looped Virtual Entertainment Item's IMDB Link Value
                    item.IMDBLink = match.IMDBLink;

                    //Attempt to Get Cover Image
                    item.CoverImage = string.IsNullOrEmpty(item.CoverImage) ? match.CoverImage : item.CoverImage;

                    //Remove Match from IMDB Results List 
                    imdbresults.Remove(match);
                }
            }

            //Combine Results
            imdbresults.AddRange(metacriticresults);

            //Return Results
            return imdbresults;
        }


        // Game Search
        // ===========================================================
        // ===========================================================
        public static void GameSearch(optSearchBox searchbox, string search)
        {
            //Show Loading Text
            searchbox.isLoading = true;

            //Create Search Thread
            SearchThread = new Thread(async () =>
            {
                //Perform Game Search
                List<GameSearch> result = await FindGamesAsync(search);

                //Run Task on UI Thread
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Clear Search Box Results
                    searchbox.Clear(true);

                    //Loop through Games List
                    foreach (GameSearch game in result)
                    {
                        //Add Current Looped Game to Search Box Results Observable Collection
                        searchbox.Add(game.IGDBLink, game.Name, game.CoverImage, game.Type, game.Platforms);
                    }

                    //Hide Loading Text
                    searchbox.isLoading = false;
                }));
            });

            //Run Search Thread
            SearchThread.Start();
        }

        private static async Task<List<GameSearch>> FindGamesAsync(string search)
        {
            //Variables
            List<GameSearch> results = new List<GameSearch>();

            //Start Search
            await StartAsync(FormatURL(MediaType.Games, search));

            //Get Search Results
            ReadOnlyCollection<IWebElement> elements = DefaultDriver.FindElements(By.ClassName("media"));

            //Loop through Results
            foreach (IWebElement element in elements)
            {
                //Add Search Item to results List
                results.Add(new GameSearch()
                {
                    IGDBLink = GetGameLink(element),
                    Name = GetGameTitle(element),
                    CoverImage = GetGameCover(element),
                    Type = GetGameType(element),
                    Platforms = GetGamePlatforms(element)
                });
            }

            //Return Results KeyValuePair
            return results;
        }
        #endregion Search



        #region Extensions
        // Format URL
        // =================================================
        // =================================================
        public static string FormatURL(MediaType type, string search, string start = "", string end = "", string altend = "")
        {
            //Validate Type
            if (type == MediaType.Games)
            {
                //Format Search Text
                search = search.Replace(" ", "+");

                //Return Formatted URL
                return "https://www.igdb.com/search?type=1&q=" + search;
            }
            else if (type == MediaType.Movies || type == MediaType.TVShows)
            {
                //Format Search Text
                search = search.Replace(" ", "%20");

                //Return Formatted URL
                return type == MediaType.Movies ? $"{start}{search}{end}" : $"{start}{search}{altend}";
            }

            //Return Empty String
            return string.Empty;
        }


        // Is Element Present
        // =================================================
        // =================================================
        private static bool IsElementPresent(DriverType type, By by, IWebElement parent = null)
        {
            //Run Try Statement
            try
            {
                //Find Element
                _ = parent != null ? parent.FindElement(by) : (type == DriverType.Default ? DefaultDriver.FindElement(by) : IMDBDriver.FindElement(by));

                //Return Valid
                return true;
            }
            catch (NoSuchElementException)
            {
                //Return Invalid
                return false;
            }
        }
        #endregion Extensions



        #region Virtual Entertainment
        #region Metacritic
        // Metacritic Title
        // ===========================================================
        // ===========================================================
        private static string GetMetacriticTitle(IWebElement parent) { return parent.FindElement(By.TagName("p")).Text; }


        // Metacritic Cover
        // ===========================================================
        // ===========================================================
        private static string GetMetacriticCover(IWebElement parent) { return IsElementPresent(DriverType.Null, By.TagName("img"), parent) ? parent.FindElement(By.TagName("img")).GetAttribute("src") : string.Empty; }


        // Metacritic Link
        // ===========================================================
        // ===========================================================
        private static string GetMetacriticLink(IWebElement parent) { return parent.GetAttribute("href"); }
        #endregion Metacritic



        #region IMDB
        // IMDB Title
        // ===========================================================
        // ===========================================================
        private static string GetIMDBTitle(IWebElement parent) { return GetGenericIMDBDetail(parent); }


        // IMDB Cover
        // ===========================================================
        // ===========================================================
        private static string GetIMDBCover(IWebElement parent) { return GetGenericIMDBDetail(parent, "src", "ipc-image"); }


        // IMDB Link
        // ===========================================================
        // ===========================================================
        private static string GetIMDBLink(IWebElement parent) { return GetGenericIMDBDetail(parent, "href"); }


        // Generic IMDB Detail
        // ===========================================================
        // ===========================================================
        private static string GetGenericIMDBDetail(IWebElement parent, string attribute = "", string classname = "ipc-metadata-list-summary-item__t")
        {
            //Validate Detail IWebElement
            if (IsElementPresent(DriverType.Null, By.ClassName(classname), parent))
            {
                //Get Detail IWebElement
                IWebElement element = parent.FindElement(By.ClassName(classname));

                //Get and Return Detail Value
                return string.IsNullOrEmpty(attribute) ? element.Text : element.GetAttribute(attribute);
            }

            //Return Empty String
            return string.Empty;
        }
        #endregion IMDB
        #endregion Virtual Entertainment



        #region Game
        // Game Link
        // =================================================
        // =================================================
        private static string GetGameLink(IWebElement parent) { return GetGenericGameDetail(parent, "media-body", "a", "href"); }


        // Title
        // =================================================
        // =================================================
        private static string GetGameTitle(IWebElement parent) { return GetGenericGameDetail(parent, "media-body", "a"); }


        // Cover
        // =================================================
        // =================================================
        private static string GetGameCover(IWebElement parent) { return GetGenericGameDetail(parent, "game_cover", "img", "src"); }


        // Type
        // =================================================
        // =================================================
        private static string GetGameType(IWebElement parent)
        {
            //Validate Game Type
            if (IsElementPresent(DriverType.Null, By.ClassName("game-type"), parent))
            {
                //Get and Return Game Type
                return parent.FindElement(By.ClassName("game-type")).Text;
            }

            //Return Empty String
            return string.Empty;
        }


        // Platforms
        // =================================================
        // =================================================
        private static List<string> GetGamePlatforms(IWebElement parent)
        {
            //Variables
            List<string> platforms = new List<string>();

            //Get Media Body Parent
            parent = parent.FindElement(By.ClassName("media-body"));

            //Validate Platform Parent
            if(IsElementPresent(DriverType.Null, By.TagName("div"), parent))
            {
                //Get Platform Parent
                parent = parent.FindElement(By.TagName("div"));
                
                //Validate Platforms
                if(IsElementPresent(DriverType.Null, By.TagName("a"), parent) && parent.FindElement(By.TagName("a")).GetAttribute("href").Contains("platform"))  //divs.Any(i => IsElementPresent(DriverType.Null, By.TagName("a"), i)))
                {
                    //Get Platforms
                    ReadOnlyCollection<IWebElement> elements = parent.FindElements(By.TagName("a"));

                    //Loop through Platform Elements
                    foreach (IWebElement element in elements)
                    {
                        //Add Current Looped Platform Value to List
                        platforms.Add(element.Text);
                    }
                }
            }

            //Return platforms List
            return platforms;
        }


        // Generic Game Detail
        // =================================================
        // =================================================
        private static string GetGenericGameDetail(IWebElement parent, string classname, string tagname, string attribute = "")
        {
            //Get Media Body Parent
            parent = parent.FindElement(By.ClassName(classname));

            //Validate Detail IWebElement
            if (IsElementPresent(DriverType.Null, By.TagName(tagname), parent))
            {
                //Get Detail IWebElement
                IWebElement element = parent.FindElement(By.TagName(tagname));

                //Get and Return Detail
                return string.IsNullOrEmpty(attribute) ? element.Text : element.GetAttribute(attribute);
            }

            //Return Empty String
            return string.Empty;
        }
        #endregion Game
    }
}