using System;
using System.Linq;
using System.Windows;
using WpfToolkit.Controls;
using MediaControlsLibrary;
using System.Windows.Input;
using Media_Manager.Models;
using System.Threading.Tasks;
using System.Windows.Controls;
using Media_Manager.Extensions;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediaControlsLibrary.Dependencies;
using static MediaControlsLibrary.NavigationBarStackTypes;
using MediaControlsLibrary.Models;
using MediaControlsLibrary.Types;

namespace Media_Manager.Views
{
    public partial class MoviesView : UserControl
    {
        #region Variables
        // Observable Collections
        // =====================================================================
        // =====================================================================
        public ObservableCollection<object> Composite { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<Folder> Folders { get; set; } = new ObservableCollection<Folder>();
        public ObservableCollection<Movie> Movies { get; set; } = new ObservableCollection<Movie>();


        // Selection
        // =====================================================================
        // =====================================================================
        private bool isSelectionChanged = false;
        private ElementType selectedType;


        // Selected Elements
        // =====================================================================
        // =====================================================================
        private Folder selectedFolder;
        private Movie selectedMovie;
        private int selectedid;
        public int selectedId { get => selectedid; set => selectedid = value; }


        // Movie View Return
        // =====================================================================
        // =====================================================================
        private bool isReturn;
        private readonly int[] SortIndexes;
        private Stack<Stack<Tuple<int, int, string, string>>> NavigationUnloadedStack;
        private Stack<Stack<Tuple<int, int, string, string>>> NavigationLoadedStack;
        private bool isFavourite;


        // Messages
        // =====================================================================
        // =====================================================================
        private readonly string str_AddFolderError = "An error occured while adding the folder. Would you like to edit the folder's details and try again?";
        private readonly string str_Exists = "The movie has already been added to the database. Are you sure you want continue with this procedure?";
        private readonly string str_ValidationError = "The File Path or URLs entered were Invalid.";
        private readonly string str_InternetError = "An internet connection is required to add a movie to the application.";
        private readonly string str_LoadError = "The file could not be found.";
        private readonly string str_EditError = "The selected element cannot be loaded for editing.";
        private readonly string str_DeletionError = "The selected item could not be deleted.";
                

        // Other
        // =====================================================================
        // =====================================================================
        private DispatcherTimer dispatcher = null;
        private int activeFolder = 0;
        private bool isEdit = false;
        #endregion Variables




        // Constructor
        // =====================================================================
        // =====================================================================
        public MoviesView () { InitializeComponent(); }

        public MoviesView(int id, int[] indexes = null, Stack<Stack<Tuple<int, int, string, string>>> navigationunloadedstack = null, Stack<Stack<Tuple<int, int, string, string>>> navigationloadedstack = null, int activefolder = 0, bool isfavourite = false)
        {
            //Initialize Components
            InitializeComponent();

            //Set isReturn to True
            isReturn = true;

            //Set selectedId
            selectedId = id;

            //Set SortIndexes
            SortIndexes = indexes;

            //Set Navigation Stacks
            NavigationUnloadedStack = navigationunloadedstack;
            NavigationLoadedStack = navigationloadedstack;

            //Set Active Folder
            activeFolder = activefolder;

            //Set isFavourite
            isFavourite = isfavourite;
        }




        // Setup
        // =====================================================================
        // =====================================================================
        private async void Setup_Tick(object sender, EventArgs e)
        {
            //Stop dispatcher
            dispatcher.Stop();

            //Disable Submenu Buttons
            ToggleSubButtons(false);

            //Set Data Context
            this.DataContext = this;

            //Set isFavourite
            if(isReturn) { tbtnShowFavourites.IsSelected = isFavourite; }

            //Load Elements
            await LoadAsync();

            //Initialize Folders
            Elements.InitializeFolders(nbNavigationBar, Folders.ToList(), NavigationLoadedStack);

            //Load Return
            if (isReturn) { await LoadReturnAsync(); }

            //Hide Loading Panel
            ToggleState.Loading(false);
        }




        #region Event Handlers
        // Page
        // =====================================================================
        // =====================================================================
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Setup dispatcher for Setup
            dispatcher = new DispatcherTimer();
            dispatcher.Tick += new EventHandler(Setup_Tick);
            dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 200);
            dispatcher.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //Clean Cover Images Folder
            CoverImages.Clean(Properties.Settings.Default.Movies);
        }



        // View
        // =====================================================================
        // =====================================================================
        private void View_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Run View Resized Method
            Pane.ViewResized(IPCSize, informationPane, icItems);
        }



        // Navigation Bar
        // =====================================================================
        // =====================================================================
        private void NavigationBar_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to Folder
            NavigateToFolder(nbNavigationBar.selectedId);
        }



        // Elements
        // =====================================================================
        // =====================================================================
        private async void Element_Click(object sender, RoutedEventArgs e)
        {
            //Check if a Selection Change is not Presently in Progress
            if (!isSelectionChanged)
            {
                //Get ID
                int id = Label.Id(sender);

                //Set Selection Changed to True
                isSelectionChanged = true;

                //Check if a Item or Folder was Selected
                if(sender is CoverItem i)
                {
                    //Set Selected Item to Sender
                    await ItemSelectedAsync(id);
                }
                else if(sender is CoverFolder f)
                {
                    //Set Selected Folder to Sender
                    await FolderSelectedAsync(id);
                }

                //Set isSelectionChanged to False
                isSelectionChanged = false;
            }
        }

        private void Element_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            //Set Event Handled to True
            e.Handled = true;

            //Load Selected Element's Context Menu
            LoadElementContextMenu(sender);
        }

        private void ItemsControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Check if the Original Source is the Items Control and if the Selected Element is not Presently Changing
            if (e.OriginalSource is ScrollViewer && isSelectionChanged == false)
            {
                //Hide Information Pane Data if the Parent is Clicked (The Background)
                ToggleState.Panel(informationPaneData, false, false);

                //Close Information Pane if the Parent is Clicked (The Background)
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Unset Selected Elements
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedMovie, ref selectedFolder, ref selectedType);

                //Disable Submenu Buttons
                ToggleSubButtons(false);
            }
        }

        private async void cmiOpen_Click(object sender, RoutedEventArgs e)
        {
            //Get Element Type
            string type = Label.Tag(sender as ContextMenuItem);

            //Check Element Type
            if (type == "item")
            {
                //Open Selected Video
                await ItemSelectedAsync(selectedId);
            }
            else if (type == "folder")
            {
                //Open Selected Folder
                await FolderSelectedAsync(selectedId);
            }
        }



        // Items
        // =====================================================================
        // =====================================================================
        private async void tbtnShowFavourites_Click(object sender, RoutedEventArgs e)
        {
            await LoadAsync(ElementType.Files);
        }

        private void cmiFavourite_Click(object sender, RoutedEventArgs e)
        {
            //Update Favourite
            UpdateFavourite(selectedMovie.isFavourite == 1 ? 0 : 1);
        }



        // Options Panel
        // =====================================================================
        // =====================================================================
        private void SelectPanel_Click(object sender, RoutedEventArgs e)
        {
            //Get Select Panel Type
            string type = Label.Tag(sender);

            //Initialize Folder Browser Dialog
            Elements.InitializeFolderBrowserDialog(Folders.ToList(), ref fbdItemSaveLocation, ref fbdFolderSaveLocation);

            //Check Select Panel Type
            if (type == "add")
            {
                //Set isEdit to False
                isEdit = false;

                //Show Select Panel
                ToggleState.Panel(SelectPanel, true);
            }
            else if (type == "edit")
            {
                //Set isEdit to True
                isEdit = true;

                //Check What Element Type is Selected
                if (selectedType == ElementType.Files)
                {
                    //Load Edit Item
                    LoadItemEdit();
                }
                else if (selectedType == ElementType.Folders)
                {
                    //Load Edit Folder
                    LoadFolderEdit();
                }
            }
        }

        private void OpenElementPanel_Click(object sender, RoutedEventArgs e)
        {
            //Get Element Type
            string element = Label.Name(sender, "btn");

            //Check Selected Element Type and Process Type
            if (selectedType == ElementType.Files && isEdit)
            {
                //Load Edit Item
                LoadItemEdit();
            }
            else if (element == "item" && !isEdit)
            {
                //Load Add Item
                OpenPanel("Add", "Movie", btnProcessItem, ItemPanel);
            }
            else if (selectedType == ElementType.Folders && isEdit)
            {
                //Load Edit Folder
                LoadFolderEdit();
            }
            else if (element == "folder" && !isEdit)
            {
                //Load Add Folder
                OpenPanel("Add", "Folder", btnProcessFolder, FolderPanel);
            }
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //Get Element Type
            string element = Label.Tag(sender);

            //Check Element and Process Type
            if(element == "item" && isEdit)
            {
                //Edit Item
                EditItem();
            }
            else if(element == "item")
            {
                //Add Item
                AddItem();
            }
            else if(element == "folder" && isEdit)
            {
                //Edit Folder
                EditFolder();
            }
            else if(element == "folder")
            {
                //Add Folder
                AddFolder();
            }
        }

        private void btnClosePanel_Click(object sender, RoutedEventArgs e)
        {
            //Get Options Panel (Owner)
            optPanel panel = (optPanel)OptionsPanel.GetAncestor((FrameworkElement)sender, typeof(optPanel));

            //Close Panel
            ToggleState.Panel(panel, false);
        }



        // Option Panel Elements
        // =====================================================================
        // =====================================================================
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Get Options Panel (Owner)
            optPanel owner = (optPanel)OptionsPanel.GetAncestor((FrameworkElement)sender, typeof(optPanel));

            //Perform Input Validation
            InputValidation(Label.Name(owner));
        }

        private void SearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get Options Panel (Owner)
            optPanel owner = (optPanel)OptionsPanel.GetAncestor((FrameworkElement)sender, typeof(optPanel));

            //Perform Input Validation
            InputValidation(Label.Name(owner));
        }

        private async void SearchBox_SearchChanged(object sender, RoutedEventArgs e)
        {
            //Abort Any Previous Search Operation
            await SearchEngine.Abort(sbMovie);

            //Perform Movie Search
            SearchEngine.VirtualEntertainmentSearch(sbMovie, ((optSearchBox)sender).Search);
        }

        private void odMoviePath_Click(object sender, RoutedEventArgs e)
        {
            //Get Options Panel (Owner)
            optPanel owner = (optPanel)OptionsPanel.GetAncestor((FrameworkElement)sender, typeof(optPanel));

            //Perform Input Validation
            InputValidation(Label.Name(owner));
        }



        // Remove Item
        // =====================================================================
        // =====================================================================
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            //Get Removal Type
            string removal = Label.Name(sender, Label.GetPrefix(sender));

            //Check Selected Element Type and Removal Name
            if(selectedType == ElementType.Files && removal == "remove")
            {
                //Remove the Item from the Application
                RemoveItem();
            }
            else if(selectedType == ElementType.Files && removal == "delete")
            {
                //Delete the Item from the Computer System
                RemoveItem(RemovalType.Delete);
            }
            else if(selectedType == ElementType.Folders)
            {
                //Remove the Folder from the Application
                RemoveFolder();
            }
        }



        // Show In File Explorer
        // =====================================================================
        // =====================================================================
        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            //Show File in File Explorer
            Elements.ShowInExplorer(FileType.File, selectedMovie.FilePath);
        }



        // Sort By
        // =====================================================================
        // =====================================================================
        private async void cboxSortBy_SelectionUpdate(object sender, SelectionChangedEventArgs e)
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Load Items
            await LoadAsync(ElementType.Files);

            //Hide Loading Panel
            ToggleState.Loading(false);
        }



        // Show Unavailable
        // =====================================================================
        // =====================================================================
        private void tbtnShowNA_Click(object sender, RoutedEventArgs e)
        {
            //Check if selectedType is set to ElementType.Files
            if (selectedType == ElementType.Files)
            {
                //Refresh Displayed Item Information
                DisplayItemInfo();
            }
        }
        #endregion Event Handlers




        #region Methods
        // Navigation Bar
        // =====================================================================
        // =====================================================================
        private async void NavigateToFolder(int id)
        {
            //Unset Selected Element
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedMovie, ref selectedFolder, ref selectedType);

            //Set Selected Element
            Elements.SetElement(ref icItems, id, out selectedid, Movies.SingleOrDefault(i => i.Id == id), out selectedMovie, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType);

            //Disable Submenu Buttons
            ToggleSubButtons(true);

            //Hide Information Pane
            Pane.Toggle(PaneToggle.Close, informationPane, icItems);

            //Set Active Folder
            activeFolder = selectedFolder == null ? 0 : selectedFolder.Id;

            //Load Elements for selectedFolder
            await LoadAsync();
        }



        // Elements
        // =====================================================================
        // =====================================================================
        private async void LoadElementContextMenu(object sender)
        {
            //Variables
            ContextMenuItem menuitem = null;

            //Get ID
            int id = Label.Id(sender);

            //Check if the Element is not Selected
            if (sender is CoverItem && ((selectedid.ToString() != id.ToString()) || (selectedid.ToString() == id.ToString() && selectedType == ElementType.Folders)))
            {
                //Set Selected Item to Sender
                await ItemSelectedAsync(id);
            }
            else if (sender is CoverFolder && ((selectedid.ToString() != id.ToString()) || (selectedid.ToString() == id.ToString() && selectedType == ElementType.Files)))
            {
                //Set Selected Folder to Sender
                await FolderSelectedAsync(id);
            }

            //Wait for Information Pane to Open or Close
            await Task.Delay(700);

            //Check if Element Type is ElementType.Files
            if(selectedType == ElementType.Files)
            {
                //Get isFavourite Value
                int isfavourite = selectedMovie.isFavourite;

                //Create Context Menu Favourite Item
                menuitem = new ContextMenuItem() { Header = isfavourite == 1 ? "Unfavourite" : "Favourite" };

                //Set Click Event Handler
                menuitem.Click += cmiFavourite_Click;
            }

            //Set Context Menu Position
            Elements.SetContextMenu(icItems, selectedType, selectedid, menuitem);
        }

        private void BringInToView()
        {
            //Get Selected Element
            ElementBase element = Elements.GetElement(icItems, selectedType, selectedId);

            //Attempt to Bring the Selected Element Into the View
            if (element != null) { element.BringIntoView(); }

            //Check if the selected element has not been found or if the selected element is not visible
            if (element == null || !element.IsVisible)
            {
                //Get Virtualizing Wrap Panel
                VirtualizingWrapPanel vwpItems = Elements.FindVisualChildren<VirtualizingWrapPanel>(icItems).ElementAt(0);

                //Bring Selected Element Index Into View
                vwpItems.BringIndexIntoViewPublic(selectedType == ElementType.Files ? Composite.IndexOf(selectedMovie) : Composite.IndexOf(selectedFolder));
            }
        }



        // Folders
        // =====================================================================
        // =====================================================================
        private async Task FolderSelectedAsync(int id)
        {
            //Check if the folder has been clicked twice
            if (selectedFolder != null && selectedId == id && selectedType == ElementType.Folders)
            {
                //Set Active Folder
                activeFolder = selectedFolder.Id;

                //Load Elements for selectedFolder
                await LoadAsync();

                //Load Folder in Navigation Bar
                nbNavigationBar.Load(activeFolder);
            }
            else
            {
                //Unset Selected Element
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedMovie, ref selectedFolder, ref selectedType);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Movies.SingleOrDefault(i => i.Id == id), out selectedMovie, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType);

                //Hide Item Information Pane Data
                ToggleState.Panel(informationPaneData, false, false);

                //Disable Submenu Buttons
                ToggleSubButtons(true);

                //Hide Information Pane
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Wait for the Information Pane to Close
                await Task.Delay(250);

                //Bring Element Into View
                BringInToView();
            }
        }



        // Items
        // =====================================================================
        // =====================================================================
        private async Task ItemSelectedAsync(int id)
        {
            //Check if the item has been clicked twice
            if (selectedType == ElementType.Files && selectedId != 0 && selectedMovie != null && selectedId == id)
            {
                //Check if File is Valid
                if (Validation.File(selectedMovie.FilePath))
                {
                    //Get The Main Window
                    Window mainWindow = Application.Current.MainWindow;

                    //Set Main Window's Data Context to New Video Player View
                    mainWindow.DataContext = new VideoPlayerView(MediaType.Movies, cboxSortBy.GetIndexes(), nbNavigationBar.GetStack(NavigationBarStackType.Unloaded), nbNavigationBar.GetStack(NavigationBarStackType.Loaded), activeFolder, tbtnShowFavourites.IsSelected, selectedMovie);
                }
                else
                {
                    //Show Error Message
                    CustomMessageBox.ShowOK(str_LoadError, "ERROR", "OK", MessageBoxImage.Error);
                }
            }
            else
            {
                //Unset Selected Element
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedMovie, ref selectedFolder, ref selectedType);

                //Wait for the Items to be added to the Movies Observable Collection (Required for Return)
                await Task.Delay(300);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Movies.SingleOrDefault(i => i.Id == id), out selectedMovie, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Files, out selectedType);

                //Display Item Information
                DisplayItemInfo();

                //Enable Submenu Buttons
                ToggleSubButtons(true);

                //Show Information Pane
                Pane.Toggle(PaneToggle.Open, informationPane, icItems);

                //Wait for the Information Pane to Open
                await Task.Delay(250);

                //Bring Element Into View
                BringInToView();

                //Show Information Pane Data
                ToggleState.Panel(informationPaneData, true, false);
            }
        }

        private void DisplayItemInfo()
        {
            //Display Main
            iTitle.Content = Formatter.FormatName(selectedMovie.Name, selectedMovie.CustomName);
            iCover.Source = Formatter.FormatImage(MediaType.Movies, selectedMovie.CoverImage);

            //Display Standard Details
            tbiResolution.SetValue(Formatter.FormatResolution(selectedMovie.Width, selectedMovie.Height));
            tbiDuration.SetValue(Formatter.FormatDuration(selectedMovie.Duration));
            tbiFramerate.SetValue(Formatter.FormatFramerate(selectedMovie.Framerate));
            tbiFormat.SetValue(selectedMovie.Format);
            tbiFileSize.SetValue(Formatter.FormatFileSize(selectedMovie.FileSize));
            tbiCreated.SetValue(Formatter.FormatCreation(selectedMovie.CreationTime, selectedMovie.CreationDate));

            //Display Advance Details
            tbiSampleRate.SetValue(Formatter.FormatSampleRate(selectedMovie.SampleRate));
            tbiAudioChannels.SetValue(selectedMovie.AudioChannels);
            tbiFramerateMode.SetValue(Formatter.FormatFramerateMode(selectedMovie.FramerateMode));

            //Display MetaCritic Details
            UserRating.SetRating(selectedMovie.UserScore, selectedMovie.UserReviewCount);
            CriticRating.SetRating(selectedMovie.CriticScore, selectedMovie.CriticReviewCount);

            //Display IMDB Details
            tbiReleaseDate.SetValue(Formatter.FormatVirtualEntertainmentReleaseDate(selectedMovie.ReleaseDate, selectedMovie.Region));
            tbiAgeRating.SetValue(selectedMovie.AgeRating);
            tbiGenres.SetList(selectedMovie.Genres);
            tbiStars.SetList(selectedMovie.Stars);
            tbiDirectors.SetList(selectedMovie.Directors);
            tbiWriters.SetList(selectedMovie.Writers);
            tbiProductionCompanies.SetList(selectedMovie.ProductionCompanies);
        }

        private async void UpdateFavourite(int value)
        {
            //Set selectedMovie isFavourite Value
            selectedMovie.isFavourite = value;

            //Set Movies Observable Collection's selectedMovie isFavourite Value
            Movies.ElementAt(Movies.IndexOf(selectedMovie)).isFavourite = value;

            //Update selectedMovie's Database isFavourite Value
            Database.UpdateFavourite(selectedMovie.Id, value, MediaType.Movies);

            //Check if Favourites are being Shown at the moment and the selectedMovie is being Unfavourited
            if (value == 0 && tbtnShowFavourites.IsSelected)
            {
                //Load Items
                await LoadAsync(ElementType.Files);
            }
        }

        private async Task LoadReturnAsync()
        {
            //Set isReturn to False
            isReturn = false;

            //Check if SortIndexes has been Set
            if (SortIndexes != null)
            {
                //Set Sort Indexes
                cboxSortBy.SetIndexes(SortIndexes[0], SortIndexes[1]);
            }

            //Set Navigation Bar Stacks
            nbNavigationBar.SetStack(NavigationBarStackType.Unloaded, NavigationUnloadedStack);
            nbNavigationBar.SetStack(NavigationBarStackType.Loaded, NavigationLoadedStack);

            //Wait for the Elements to Begin Being Added to the Composite Observable Collection
            await Task.Delay(2);

            //Check if the selectedid has been set
            if (selectedid > 0)
            {
                //Set Selected Item
                await ItemSelectedAsync(selectedid);
            }
        }



        // Options Panel
        // =====================================================================
        // =====================================================================
        private void OpenPanel(string process, string name, optButton button, optPanel panel)
        {
            //Clear Element Panel
            ClearPanel(Label.Name(panel));

            //Disable Button
            ToggleState.UIElement(button, false);

            //Hide Select Panel
            ToggleState.Panel(SelectPanel, false);

            //Set Panel
            SetPanel(process, name);

            //Show Element Panel
            ToggleState.Panel(panel, true);
        }

        private void ClearPanel(string name)
        {
            //Check Panel Name
            if (name == "itempanel")
            {
                //Clear Item Panel Fields
                OptionsPanel.ClearPanel(null, new List<optTextBoxLong> { tbCustomName }, new List<optSearchBox> { sbMovie }, new List<optOpenDialog> { odMoviePath });
            }
            else if (name == "folderpanel")
            {
                //Clear Folder Panel Fields
                OptionsPanel.ClearPanel(null, new List<optTextBoxLong> { tbFolderName }, null, null);
            }
        }

        private void SetPanel(string process, string name)
        {
            //Check Element Type
            if(name == "Movie")
            {
                //Set Panel Contents
                ItemPanelTitle.Content = $"{process} {name}";
                btnProcessItem.Content = process;
            }
            else if(name == "Folder")
            {
                //Set Panel Contents
                FolderPanelTitle.Content = $"{process} {name}";
                btnProcessFolder.Content = process;
            }
        }



        // Submenu Buttons
        // =====================================================================
        // =====================================================================
        private void ToggleSubButtons(bool IsEnabled)
        {
            //Check Selected Element Type or if isEnabled is Set to False
            if (selectedType == ElementType.Files || IsEnabled == false)
            {
                //Toggle Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnDelete, btnEdit, btnRemove, btnShowInExplorer }, IsEnabled);
            }
            else if(selectedType == ElementType.Folders)
            {
                //Disable the Delete and Show In Explorer Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnDelete, btnShowInExplorer }, false);

                //Enable the Edit and Remove Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnEdit, btnRemove }, true);
            }
        }



        // Add Folder
        // =====================================================================
        // =====================================================================
        private async void AddFolder()
        {
            //Get Base Folder Data
            Folder folder = new Folder() { OwnerId = fbdFolderSaveLocation.Id, Name = $"{tbFolderName.Content}", Type = nameof(MediaType.Movies), FolderType = nameof(FolderType.Folders) };

            //Disable Process Folder Button
            ToggleState.UIElement(btnProcessFolder, false);

            //Hide Folder Panel
            ToggleState.Panel(FolderPanel, false);

            //Get Boolean Value Representing if a Folder that has the Same Location (OwnerId) and Name Already Exists within the Folders Database
            bool isEmpty = !Folders.Any(i => i.OwnerId == folder.OwnerId && i.Name == folder.Name);

            //Check if a Folder that has the Same Location (OwnerId) and Name Already Exists within the Folders Database
            if (!isEmpty)
            {
                //Show Add Folder Error Message
                MessageBoxResult result = CustomMessageBox.ShowYesNo(str_AddFolderError, "WARNING", "Edit", "Cancel", MessageBoxImage.Warning);

                //Check Result
                if (result == MessageBoxResult.Yes)
                {
                    //Show Folder Panel
                    ToggleState.Panel(FolderPanel, true);
                }
            }
            else
            {
                //Show Loading Panel
                ToggleState.Loading(true);

                //Save Folder
                Database.SaveFolder(folder);

                //Load Folders
                await LoadAsync(ElementType.Folders);

                //Hide Loading Panel
                ToggleState.Loading(false);
            }
        }



        // Add Item
        // =====================================================================
        // =====================================================================
        private void AddItem()
        {
            //Variables
            bool isAddItem = false;

            //Get Base Item Data
            Movie movie = new Movie() { OwnerId = fbdItemSaveLocation.Id, FilePath = $"{odMoviePath.Content}", CustomName = $"{tbCustomName.Content}", IMDBLink = $"{sbMovie.IMDBLink}", MetaCriticLink = $"{sbMovie.DefaultLink}" };

            //Disable Process Item Button
            ToggleState.UIElement(btnProcessItem, false);

            //Hide Item Panel
            ToggleState.Panel(ItemPanel, false);

            //Get Boolean Value Representing if the Item Already Exists within the Database (Movies ObservableCollection)
            bool isEmpty = !Movies.Any(i => movie.FilePath == i.FilePath);

            //Validate File Path and URL
            bool isValid = Validation.Process(movie.FilePath, true, true, new string[] { "https://www.imdb.com/", "https://www.metacritic.com/" }, new string[] { movie.IMDBLink, movie.MetaCriticLink }, new int[] { 37, 35 });


            //Check if the item has already been added to the database, and if the filepath and urls are valid
            //Else check if the item has not been added to the database and if the filepath and urls are valid
            if (!isEmpty && isValid)
            {
                //Show Movie Exists Message
                MessageBoxResult result = CustomMessageBox.ShowYesNoCancel(str_Exists, "WARNING", "Add", "Edit", "Cancel", MessageBoxImage.Warning);

                //Check Result
                if (result == MessageBoxResult.Yes)
                {
                    //Set isAddItem boolean to true
                    isAddItem = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    //Show Item Panel
                    ToggleState.Panel(ItemPanel, true);
                }
            }
            else if (isEmpty && isValid)
            {
                //Set isAddItem boolean to true
                isAddItem = true;
            }
            else
            {
                //Show Validation Error Message
                MessageBoxResult result = CustomMessageBox.ShowYesNo(str_ValidationError, "Error", "Edit", "Cancel", MessageBoxImage.Error);

                //Check Result
                if (result == MessageBoxResult.Yes)
                {
                    //Show Item Panel
                    ToggleState.Panel(ItemPanel, true);
                }
            }


            //Check if the isAddItem boolean is set to true
            if (isAddItem)
            {
                //Run Process Add Item Method
                ProcessAddItem(movie);
            }
        }

        private async void ProcessAddItem(Movie movie)
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Stop Search Engine on Different Thread
            _ = Task.Run(() => { SearchEngine.Stop(); });

            //Process Add Item on Different Thread
            await Task.Run(async () =>
            {
                //Configure Fetcher
                bool isValid = await Fetcher.ConfigureFetcherAsync(MediaType.Movies, movie.FilePath, string.Empty, movie.MetaCriticLink, movie.IMDBLink);

                //Check if Configuration was Successful
                if (isValid)
                {
                    //Get Item Details
                    movie = Fetcher.GetMovie(movie, Movies.Count() + 1);

                    //Close Fetcher
                    Fetcher.Close();

                    //Save Item
                    Database.SaveItem(MediaType.Movies, movie);

                    //Load Items on UI Thread
                    Application.Current.Dispatcher.Invoke(new Action(async () => { await LoadAsync(ElementType.Files); }));
                }
                else
                {
                    //Show Error Message on UI Thread
                    Application.Current.Dispatcher.Invoke(new Action(() => { CustomMessageBox.ShowOK(str_InternetError, "ERROR", "OK", MessageBoxImage.Error); }));
                }
            });

            //Hide Loading Panel
            ToggleState.Loading(false);
        }



        // Edit Folder
        // =====================================================================
        // =====================================================================
        private void LoadFolderEdit()
        {
            //Validate Selected Folder
            if (Folders.Contains(selectedFolder))
            {
                //Set Panel
                SetPanel("Edit", "Folder");

                //Load Editable Values
                OptionsPanel.LoadEditableValues(tbFolderName, selectedFolder.Name, fbdFolderSaveLocation, selectedFolder.OwnerId, nbNavigationBar.GetFolderPath(), selectedType, selectedid);

                //Disable Process Folder Button
                ToggleState.UIElement(btnProcessFolder, false);

                //Show Folder Panel
                ToggleState.Panel(FolderPanel, true);
            }
            else
            {
                //Show Edit Error Message
                CustomMessageBox.ShowOK(str_EditError, "ERROR", "OK", MessageBoxImage.Error);
            }
        }

        private async void EditFolder()
        {
            //Get Base Folder Data
            Folder folder = new Folder() { Name = $"{tbFolderName.Content}", OwnerId = fbdFolderSaveLocation.Id };

            //Hide Folder Panel
            ToggleState.Panel(FolderPanel, false);

            //Check if the Folder has been Updated
            if (selectedFolder.Name != folder.Name || selectedFolder.OwnerId != folder.OwnerId)
            {
                //Show Loading Panel
                ToggleState.Loading(true);

                //Assign selectedFolder into newfolder object
                Folder newfolder = selectedFolder;

                //Clear Navigation Bar'S Navigation (Dependent on if the Folder has Changed Locations)
                if (selectedFolder.OwnerId != folder.OwnerId) { nbNavigationBar.ClearNavigation(); }

                //Set New Folder Values
                newfolder.Name = folder.Name;
                newfolder.OwnerId = folder.OwnerId;

                //Update Folder
                Database.UpdateFolder(newfolder.Id, newfolder);

                //Close Information Pane
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Load Folders
                await LoadAsync(ElementType.Folders, folder);

                //Hide Loading Panel
                ToggleState.Loading(false);
            }
        }



        // Edit Item
        // =====================================================================
        // =====================================================================
        private async void LoadItemEdit()
        {
            //Validate Selected Item
            if (Movies.Contains(selectedMovie) && Validation.File(selectedMovie.FilePath))
            {
                //Clear Panel
                ClearPanel("itempanel");

                //Set Panel
                SetPanel("Edit", "Movie");

                //Disable Process Item Button
                ToggleState.UIElement(btnProcessItem, false);

                //Show Item Panel
                ToggleState.Panel(ItemPanel, true);

                //Delay Load Editable Values by 10 Milliseconds (Required for Cover Image)
                await Task.Delay(10);

                //Load Editable Values
                OptionsPanel.LoadEditableValues(tbCustomName, selectedMovie.CustomName, fbdItemSaveLocation, selectedMovie.OwnerId, nbNavigationBar.GetFolderPath(), selectedType, selectedid, new List<optOpenDialog> { odMoviePath }, new List<string> { selectedMovie.FilePath }, MediaType.Movies, new List<optSearchBox> { sbMovie }, selectedMovie);
            }
            else
            {
                //Show Edit Error Message
                CustomMessageBox.ShowOK(str_EditError, "ERROR", "OK", MessageBoxImage.Error);
            }
        }

        private async void EditItem()
        {
            //Get Base Item Data
            Movie movie = new Movie() { FilePath = $"{odMoviePath.Content}", CustomName = $"{tbCustomName.Content}", OwnerId = fbdItemSaveLocation.Id, IMDBLink = $"{sbMovie.IMDBLink}", MetaCriticLink = $"{sbMovie.DefaultLink}" };

            //Stop Search Engine
            SearchEngine.Stop();

            //Hide Item Panel
            ToggleState.Panel(ItemPanel, false);

            //Validate File Path, IMDB Link, and MetaCritic Link
            if (Validation.ValidateSaveLocation(OptionPanelType.Null, Folders.ToList(), new KeyValuePair<int, string>(movie.OwnerId, fbdItemSaveLocation.FolderPath)) && Validation.Process(movie.FilePath, true, true, new string[] { "https://www.imdb.com/", "https://www.metacritic.com/" }, new string[] { movie.IMDBLink, movie.MetaCriticLink }, new int[] { 37, 35 }))
            {
                //Show Loading Panel
                ToggleState.Loading(true);

                //Check if the File Path, IMDB Link, or MetaCritic Link have been changed
                if (movie.FilePath != selectedMovie.FilePath || movie.IMDBLink != selectedMovie.IMDBLink || movie.MetaCriticLink != selectedMovie.MetaCriticLink)
                {
                    //Configure Fetcher
                    bool isValid = await Fetcher.ConfigureFetcherAsync(MediaType.Movies, movie.FilePath, string.Empty, movie.MetaCriticLink, movie.IMDBLink);

                    //Check if the Configuration was Successful
                    if (isValid == true)
                    {
                        //Get Movie Details
                        movie = Fetcher.GetMovie(movie, selectedMovie.Id);

                        //Close Fetcher
                        Fetcher.Close();
                    }
                    else
                    {
                        //Show Error Message
                        CustomMessageBox.ShowOK(str_InternetError, "ERROR", "OK", MessageBoxImage.Error);

                        //Hide Loading Panel
                        ToggleState.Loading(false);

                        //Return Method
                        return;
                    }
                }
                else
                {
                    //Assign selectedMovie into movie object
                    movie = selectedMovie;

                    //Update Movie Values
                    movie.CustomName = tbCustomName.Content.ToString();
                    movie.OwnerId = fbdItemSaveLocation.Id;
                }

                //Update Item
                Database.UpdateItem(movie.Id, movie, MediaType.Movies);

                //Close Information Pane
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Load Items
                await LoadAsync(ElementType.Files);

                //Hide Loading Panel
                ToggleState.Loading(false);
            }
            else
            {
                //Show Validation Error Message
                MessageBoxResult result = CustomMessageBox.ShowYesNo(str_ValidationError, "WARNING", "Edit", "Cancel", MessageBoxImage.Error);

                //Check Result
                if (result == MessageBoxResult.Yes)
                {
                    //Show Item Panel
                    ToggleState.Panel(ItemPanel, true);
                }
            }
        }



        // Remove Folder
        // =====================================================================
        // =====================================================================
        private async void RemoveFolder()
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Remove Folder From Application
            bool isRemoved = DiscardElements.RemoveFolder(MediaType.Movies, selectedId);

            //Check if the isRemoved Boolean is Set to True
            if (isRemoved)
            {
                //Remove Folder From Navigation Bar
                nbNavigationBar.Remove(selectedFolder.Id, selectedFolder.OwnerId, selectedFolder.Name, selectedFolder.FolderType);

                //Load Elements
                await LoadAsync();
            }

            //Hide Loading Panel
            ToggleState.Loading(false);
        }



        // Remove Item
        // =====================================================================
        // =====================================================================
        private async void RemoveItem(RemovalType type = RemovalType.Remove)
        {
            //Initialize Variables
            bool isDeleted = false, isCancel = false;

            //Show Loading Panel
            ToggleState.Loading(true);

            //Check if Removal Type is Delete and if the File Exists
            if (type == RemovalType.Delete && Validation.File(selectedMovie.FilePath))
            {
                //Delete the Item from the Computer System
                DiscardElements.DeleteItem(out isCancel, out isDeleted, MediaType.Movies, FileType.File, selectedMovie.FilePath);
            }

            //Check if the selected item's file was deleted from the computer system or if the removal type is set to remove
            //Else check if the user cancelled the removal
            if (isDeleted == true || type == RemovalType.Remove)
            {
                //Remove Item From Application
                bool isRemoved = DiscardElements.RemoveItem(MediaType.Movies, selectedMovie.Id, type, selectedMovie.CoverImage);

                //Check if the isRemoved Boolean is Set to True
                if (isRemoved == true)
                {
                    //Load Items
                    await LoadAsync(ElementType.Files);
                }
            }
            else if (!isCancel)
            {
                //Show Deletion Error Message
                CustomMessageBox.ShowOK(str_DeletionError, "ERROR", "OK", MessageBoxImage.Error);
            }

            //Hide Loading Panel
            ToggleState.Loading(false);
        }



        // Load
        // =====================================================================
        // =====================================================================
        private async Task LoadAsync(ElementType elementtype = ElementType.Null, Folder folder = null)
        {
            //Disable Submenu Buttons
            ToggleSubButtons(false);

            //Close Information Pane
            Pane.Toggle(PaneToggle.Close, informationPane, icItems);

            //Unset Selected Element
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedMovie, ref selectedFolder, ref selectedType);

            //Check Element Type
            if (elementtype == ElementType.Files)
            {
                //Load Items
                await LoadItemsAsync();
            }
            else if(elementtype == ElementType.Folders)
            {
                //Load Folders
                await LoadFoldersAsync();

                //Get Folder
                folder = folder == null ? Folders.OrderBy(i => i.Id).Last() : folder;

                //Add Folder to Navigation Bar
                nbNavigationBar.Add(folder.Id, folder.OwnerId, folder.Name, folder.FolderType);
            }
            else
            {
                //Load Items
                await LoadItemsAsync();

                //Load Folders
                await LoadFoldersAsync();
            }

            //Clear Composite
            Composite.Clear();

            //Add Observable Collections to Composite Collection
            if (Folders.Count() > 0) { Composite.AddRange(Folders.Where(i => i.OwnerId == activeFolder)); }
            if (Movies.Count() > 0) { Composite.AddRange(tbtnShowFavourites.IsSelected ? Movies.Where(i => i.OwnerId == activeFolder && i.isFavourite == 1) : Movies.Where(i => i.OwnerId == activeFolder)); }
        }

        private async Task LoadItemsAsync()
        {
            //Clear Movies Observable Collection
            Movies.Clear();

            //Get Sort Order and Type
            string order = Sort.GetOrder(cboxSortBy);
            string type = Sort.GetType(cboxSortBy);

            //Run Task
            await Task.Run(() =>
            {
                //Loop Through Each Movie in the Movies Database
                foreach (Movie movie in Database.LoadItems(MediaType.Movies, order, type))
                {
                    //Invoke Dispatcher
                    Dispatcher.Invoke(() =>
                    {
                        //Add Each Movie from the Movies Database to the Movies Observable Collection
                        Movies.Add(movie);
                    });
                }
            });
        }

        private async Task LoadFoldersAsync()
        {
            //Clear Folders Observable Collection
            Folders.Clear();

            //Run Task
            await Task.Run(() =>
            {
                //Loop Through Each Folder in the Folders Database Where Type is Equal to type
                foreach (Folder folder in Database.LoadFolders(MediaType.Movies))
                {
                    //Invoke Dispatcher
                    Dispatcher.Invoke(() =>
                    {
                        //Add Each Looped Folder to the Folders Observable Collection
                        Folders.Add(folder);
                    });
                }
            });
        }



        // Input Validation
        // =====================================================================
        // =====================================================================
        private void InputValidation(string owner)
        {
            //Check Owner, Prcoess Type, and Selected Element
            if (owner == "itempanel" && isEdit && selectedMovie != null)
            {
                //Validate Panel Contents
                Validation.ValidateMoviePanel(OptionPanelType.Edit, btnProcessItem, Folders.ToList(), new string[] { selectedMovie.CustomName, selectedMovie.FilePath, selectedMovie.IMDBLink, selectedMovie.MetaCriticLink }, new KeyValuePair<int, string>(fbdItemSaveLocation.Id, fbdItemSaveLocation.FolderPath), new string[] { $"{tbCustomName.Content}", $"{odMoviePath.Content}", $"{sbMovie.IMDBLink}", $"{sbMovie.DefaultLink}" }, new KeyValuePair<int, string>(selectedMovie.OwnerId, nbNavigationBar.GetFolderPath()));
            }
            else if (owner == "itempanel" && !isEdit)
            {
                //Validate Panel Contents
                Validation.ValidateMoviePanel(OptionPanelType.Add, btnProcessItem, Folders.ToList(), new string[] { $"{odMoviePath.Content}", $"{sbMovie.IMDBLink}", $"{sbMovie.DefaultLink}" }, new KeyValuePair<int, string>(fbdItemSaveLocation.Id, fbdItemSaveLocation.FolderPath));
            }
            else if (owner == "folderpanel" && isEdit && selectedFolder != null)
            {
                //Validate Panel Contents
                Validation.ValidateFolderPanel(OptionPanelType.Edit, btnProcessFolder, Folders.ToList(), Tuple.Create($"{tbFolderName.Content}", fbdFolderSaveLocation.Id, fbdFolderSaveLocation.FolderPath), Tuple.Create(selectedFolder.Name, selectedFolder.OwnerId, nbNavigationBar.GetFolderPath()));
            }
            else if (owner == "folderpanel" && !isEdit)
            {
                //Validate Panel Contents
                Validation.ValidateFolderPanel(OptionPanelType.Add, btnProcessFolder, Folders.ToList(), Tuple.Create($"{tbFolderName.Content}", fbdFolderSaveLocation.Id, fbdFolderSaveLocation.FolderPath));
            }
        }
        #endregion Methods
    }
}