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
using System.Diagnostics;
using System.IO;

namespace Media_Manager.Views
{
    public partial class TVShowsView : UserControl
    {
        #region Variables
        // Observable Collections
        // =====================================================================
        // =====================================================================
        public ObservableCollection<object> Composite { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<Folder> Folders { get; set; } = new ObservableCollection<Folder>();
        public ObservableCollection<TVShowFolder> TVShowFolders { get; set; } = new ObservableCollection<TVShowFolder>();
        public ObservableCollection<SeasonFolder> SeasonFolders { get; set; } = new ObservableCollection<SeasonFolder>();
        public ObservableCollection<Episode> Episodes { get; set; } = new ObservableCollection<Episode>();


        // Selection
        // =====================================================================
        // =====================================================================
        private bool isSelectionChanged = false;
        private ElementType selectedType;
        private FolderType selectedFolderType;


        // Selected Elements
        // =====================================================================
        // =====================================================================
        private Folder selectedFolder;
        private TVShowFolder selectedTVShowFolder;
        private SeasonFolder selectedSeasonFolder;
        private Episode selectedEpisode;
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
        private readonly string str_TVShowExists = "The tv show has already been added to the database. Are you sure you want continue with this procedure?";
        private readonly string str_SeasonExists = "The season has already been added to the database. Are you sure you want continue with this procedure?";
        private readonly string str_EpisodeExists = "The episode has already been added to the database. Are you sure you want continue with this procedure?";
        private readonly string str_ValidationError = "The File Path or URLs entered were Invalid.";
        private readonly string str_TVShowInternetError = "An internet connection is required to add a tv show to the application.";
        private readonly string str_SeasonInternetError = "An internet connection is required to add a season to the application.";
        private readonly string str_EpisodeInternetError = "An internet connection is required to add an episode to the application.";
        private readonly string str_LoadError = "The file could not be found.";
        private readonly string str_EditError = "The selected element cannot be loaded for editing.";
        private readonly string str_DeletionError = "The selected item could not be deleted.";


        // Other
        // =====================================================================
        // =====================================================================
        private DispatcherTimer dispatcher = null;
        private int activeFolder = 0;
        private int activeTVShowFolder = 0;
        private int activeSeasonFolder = 0;
        private bool isEdit = false;
        #endregion Variables




        // Constructor
        // =====================================================================
        // =====================================================================
        public TVShowsView() { InitializeComponent(); }

        public TVShowsView(int id, int[] indexes = null, Stack<Stack<Tuple<int, int, string, string>>> navigationunloadedstack = null, Stack<Stack<Tuple<int, int, string, string>>> navigationloadedstack = null, int activeseasonfolder = 0, bool isfavourite = false)
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

            //Set Active Season Folder
            activeSeasonFolder = activeseasonfolder;

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

            //Validate Return
            if (isReturn)
            {
                //Set isFavourite
                tbtnShowFavourites.IsSelected = isFavourite;

                //Set selectedFolderType to FolderType.SeasonFolders
                selectedFolderType = nbNavigationBar.GetFolderType();
            }

            //Load Elements
            await LoadAsync();

            //Initialize Folders Composite
            List<Folder> folders = new List<Folder>();

            //Add Folders to Composite
            folders.AddRange(Folders.ToList());
            folders.AddRange(TVShowFolders.ToList());
            folders.AddRange(SeasonFolders.ToList());

            //Initialize Folders
            Elements.InitializeFolders(nbNavigationBar, folders, NavigationLoadedStack);

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
            CoverImages.Clean(Properties.Settings.Default.TVShows);
            CoverImages.Clean(Properties.Settings.Default.Seasons);
            CoverImages.Clean(Properties.Settings.Default.Episodes);
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
            NavigateToFolder(nbNavigationBar.selectedId, nbNavigationBar.GetFolderType());
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
                if (sender is CoverItem i)
                {
                    //Set Selected Episode to Sender
                    await EpisodeSelectedAsync(id);
                }
                else if (sender is CoverFolder f)
                {
                    //Get Folder Type
                    FolderType foldertype = Label.FolderType(sender);

                    //Validate Folder Type
                    if (foldertype == FolderType.Folders)
                    {
                        //Set Selected Folder to Sender
                        await FolderSelectedAsync(id);
                    }
                    else if(foldertype == FolderType.TVShowFolders)
                    {
                        //Set Selected TV Show Folder to Sender
                        await TVShowFolderSelectedAsync(id);
                    }
                    else if(foldertype == FolderType.SeasonFolders)
                    {
                        //Set Selected Season Folder to Sender
                        await SeasonFolderSelectedAsync(id);
                    }
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
                ToggleInformationPanes(false);

                //Close Information Pane if the Parent is Clicked (The Background)
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Unset Selected Elements
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedEpisode, ref selectedSeasonFolder, ref selectedTVShowFolder, ref selectedFolder, ref selectedType, ref selectedFolderType);

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
                await EpisodeSelectedAsync(selectedid);
            }
            else if(type == "seasonfolder")
            {
                //Open Selected Season Folder
                await SeasonFolderSelectedAsync(selectedid);
            }
            else if (type == "tvshowfolder")
            {
                //Open Selected TV Show Folder
                await TVShowFolderSelectedAsync(selectedId);
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
            await LoadAsync(ElementType.Folders, null, FolderType.TVShowFolders);
        }

        private void cmiFavourite_Click(object sender, RoutedEventArgs e)
        {
            //Update Favourite
            UpdateFavourite(selectedTVShowFolder.isFavourite == 1 ? 0 : 1);
        }



        // Options Panel
        // =====================================================================
        // =====================================================================
        private void SelectPanel_Click(object sender, RoutedEventArgs e)
        {
            //Get Select Panel Type
            string type = Label.Tag(sender);

            //Initialize Folder Browser Dialog
            Elements.InitializeFolderBrowserDialog(Folders.ToList(), ref fbdTVShowSaveLocation, ref fbdFolderSaveLocation);

            //Check Select Panel Type
            if(type == "add")
            {
                //Set isEdit to False
                isEdit = false;

                //Validate Element and Folder Type
                if (nbNavigationBar.GetFolderType() == FolderType.SeasonFolders)
                {
                    //Set Selected TV Show Folder
                    selectedTVShowFolder = TVShowFolders.FirstOrDefault(i => i.Id == nbNavigationBar.selectedId && i.FolderType == nbNavigationBar.selectedFolderType);

                    //Load Add Episodes Folder
                    OpenPanel("Add", "Episodes", btnProcessEpisodes, EpisodesPanel);
                }
                else if (nbNavigationBar.GetFolderType() == FolderType.TVShowFolders)
                {
                    //Set Selected TV Show Folder
                    selectedTVShowFolder = TVShowFolders.FirstOrDefault(i => i.Id == nbNavigationBar.selectedId && i.FolderType == nbNavigationBar.selectedFolderType);

                    //Show Season Directory Open Dialog and Season Number Numeric Box
                    ToggleState.UIElements(new UIElement[] { odSeasonDirectory, nbSeason }, Visibility.Visible);

                    //Load Add Season Folder
                    OpenPanel("Add", "Season", btnProcessSeason, SeasonPanel);
                }
                else
                {
                    //Show Select Panel
                    ToggleState.Panel(SelectPanel, true);
                }
            }
            else if (type == "edit")
            {
                //Set isEdit to True
                isEdit = true;

                if (selectedType == ElementType.Folders)
                {
                    //Validate Folder Type
                    if (selectedFolderType == FolderType.Folders)
                    {
                        //Load Edit Folder
                        LoadFolderEdit();
                    }
                    else if (selectedFolderType == FolderType.TVShowFolders)
                    {
                        //Load Edit TV Show Folder
                        LoadTVShowFolderEdit();
                    }
                    else if (selectedFolderType == FolderType.SeasonFolders)
                    {
                        //Load Edit Season Folder
                        LoadSeasonFolderEdit();
                    }
                }
            }
        }

        private void OpenElementPanel_Click(object sender, RoutedEventArgs e)
        {
            //Get Element Type
            string element = Label.Name(sender, "btn");

            //Check Selected Element Type and Process Type
            if (selectedType == ElementType.Folders && selectedFolderType == FolderType.TVShowFolders && isEdit)
            {
                //Load Edit TV Show Folder
                LoadTVShowFolderEdit();
            }
            else if (element == "tvshow" && !isEdit)
            {
                //Show SearchBox
                ToggleState.UIElement(sbTVShow, Visibility.Visible);

                //Load Add Item
                OpenPanel("Add", "TV Show", btnProcessTVShow, TVShowPanel);
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

            //Validate Process Type
            if (!isEdit)
            {
                //Check Element Type
                if (element == "episode")
                {
                    //Add Episodes
                    AddEpisodes();
                }
                else if (element == "season")
                {
                    //Add Season
                    AddSeason();
                }
                else if (element == "tvshow")
                {
                    //Add TV Show
                    AddTVShow();
                }
                else if (element == "folder")
                {
                    //Add Folder
                    AddFolder();
                }
            }
            else
            {
                //Check Element Type
                if (element == "episode")
                {
                    //Edit Episode
                    Debug.WriteLine("Edit Episode");
                }
                else if (element == "season")
                {
                    //Edit Season
                    EditSeason();
                }
                else if (element == "tvshow")
                {
                    //Edit TV Show
                    EditTVShowFolder();
                }
                else if (element == "folder")
                {
                    //Edit Folder
                    EditFolder();
                }
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
            await SearchEngine.Abort(sbTVShow);

            //Perform TV Show Search
            SearchEngine.VirtualEntertainmentSearch(sbTVShow, ((optSearchBox)sender).Search, MediaType.TVShows);
        }

        private void OpenDialog_Click(object sender, RoutedEventArgs e)
        {
            //Get Options Panel (Owner)
            optPanel owner = (optPanel)OptionsPanel.GetAncestor((FrameworkElement)sender, typeof(optPanel));

            //Perform Input Validation
            InputValidation(Label.Name(owner));
        }

        private void NumericBox_Click(object sender, RoutedEventArgs e)
        {
            //Validate Type
            if (nbNavigationBar.GetFolderType() == FolderType.TVShowFolders || nbNavigationBar.GetFolderType() == FolderType.SeasonFolders)
            {
                //Get Options Panel (Owner)
                optPanel owner = (optPanel)OptionsPanel.GetAncestor(nbNavigationBar.GetFolderType() == FolderType.TVShowFolders ? nbSeason : null, typeof(optPanel));

                //Perform Input Validation
                InputValidation(Label.Name(owner));
            }
        }



        // Remove Item
        // =====================================================================
        // =====================================================================
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            //Get Removal Type
            string removal = Label.Name(sender, Label.GetPrefix(sender));

            Debug.WriteLine($"Removal: {removal}        Selected Folder Type: {selectedFolderType}");

            //Validate Selected Element Type
            if (selectedType == ElementType.Files)
            {
                //Validate Removal Name
                if (removal == "remove")
                {
                    //Remove the Item from the Application
                    RemoveItem();
                }
                else if (removal == "delete")
                {
                    //Delete the Item from the Computer System
                    RemoveItem(RemovalType.Delete);
                }
            }
            else if (selectedType == ElementType.Folders)
            {
                //Validate Selected Folder Type
                if (selectedFolderType == FolderType.Folders)
                {
                    //Remove the Folder from the Application
                    RemoveFolder();
                }
                else if (selectedFolderType == FolderType.TVShowFolders)
                {
                    //Validate Removal Name
                    if (removal == "remove")
                    {
                        //Remove the Season Folder from the Application
                        RemoveTVShowFolder();
                    }
                    else if (removal == "delete")
                    {
                        //Delete the Season Folder from the Computer System
                        RemoveTVShowFolder(RemovalType.Delete);
                    }
                }
                else if (selectedFolderType == FolderType.SeasonFolders)
                {
                    //Validate Removal Name
                    if (removal == "remove")
                    {
                        //Remove the Season Folder from the Application
                        RemoveSeasonFolder();
                    }
                    else if (removal == "delete")
                    {
                        //Delete the Season Folder from the Computer System
                        RemoveSeasonFolder(RemovalType.Delete);
                    }
                }
            }
        }



        // Show In File Explorer
        // =====================================================================
        // =====================================================================
        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            //Show File in File Explorer
            Elements.ShowInExplorer(FileType.File, selectedEpisode.FilePath);
        }



        // Sort By
        // =====================================================================
        // =====================================================================
        private async void cboxSortBy_SelectionUpdate(object sender, SelectionChangedEventArgs e)
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Set Selected Folder Type
            selectedFolderType = nbNavigationBar.GetFolderType();

            //Load Folders
            await LoadAsync();

            //Hide Loading Panel
            ToggleState.Loading(false);
        }



        // Show Unavailable
        // =====================================================================
        // =====================================================================
        private void tbtnShowNA_Click(object sender, RoutedEventArgs e)
        {
            //Validate Element Type
            if(selectedType == ElementType.Files)
            {
                //Refresh Displayed Information
                DisplayEpisodeInfo();
            }
            else if (selectedType == ElementType.Folders)
            {
                //Validate Folder Type
                if (selectedFolderType == FolderType.TVShowFolders)
                {
                    //Refresh Displayed Information
                    DisplayTVShowFolderInfo();
                }
                else if (selectedFolderType == FolderType.SeasonFolders)
                {
                    //Refresh Displayed Information
                    DisplaySeasonFolderInfo();
                }
            }
        }
        #endregion Event Handlers




        #region Methods
        // Navigation Bar
        // =====================================================================
        // =====================================================================
        private async void NavigateToFolder(int id, FolderType foldertype)
        {
            //Unset Selected Element
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedEpisode, ref selectedSeasonFolder, ref selectedTVShowFolder, ref selectedFolder, ref selectedType, ref selectedFolderType);

            //Set Selected Element
            Elements.SetElement(ref icItems, id, out selectedid, Episodes.SingleOrDefault(i => i.Id == id), out selectedEpisode, SeasonFolders.SingleOrDefault(i => i.Id == id), out selectedSeasonFolder, TVShowFolders.SingleOrDefault(i => i.Id == id), out selectedTVShowFolder, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType, foldertype, out selectedFolderType);

            //Disable Submenu Buttons
            ToggleSubButtons(true);

            //Hide Information Pane
            Pane.Toggle(PaneToggle.Close, informationPane, icItems);

            //Toggle Submenu Functions
            ToggleSubmenuFunctions(foldertype);

            //Set or Unset Active Folder
            if(selectedFolder != null) { activeFolder = selectedFolder.Id; }
            else if(selectedTVShowFolder != null) { activeTVShowFolder = selectedTVShowFolder.Id; }
            else if(selectedSeasonFolder != null) { activeSeasonFolder = selectedSeasonFolder.Id; }
            else { activeFolder = 0; activeTVShowFolder = 0; activeSeasonFolder = 0; }

            //Load Elements for selectedFolder
            await LoadAsync();
        }



        // Elements
        // =====================================================================
        // =====================================================================
        private void ToggleInformationPanes(bool isEnabled, ElementType elementtype = ElementType.Null, FolderType foldertype = FolderType.Null)
        {
            //Toggle Information Pane Data
            ToggleState.Panel(informationPaneData, isEnabled, false);

            //Toggle Information Panes
            TVShowFolderInformationPane.Visibility = foldertype == FolderType.TVShowFolders ? Visibility.Visible : Visibility.Collapsed;
            SeasonFolderInformationPane.Visibility = foldertype == FolderType.SeasonFolders ? Visibility.Visible : Visibility.Collapsed;
            EpisodeInformationPane.Visibility = elementtype == ElementType.Files ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void LoadElementContextMenu(object sender)
        {
            //Variables
            ContextMenuItem menuitem = null;

            //Get ID
            int id = Label.Id(sender);

            //Get Folder Type
            FolderType foldertype = sender is CoverFolder ? ((CoverFolder)sender).FolderType : FolderType.Null;

            //Check if the Element is not Selected
            if (sender is CoverItem && ((selectedid.ToString() != id.ToString()) || (selectedid.ToString() == id.ToString() && selectedType == ElementType.Folders)))
            {
                //Set Selected Episode to Sender
                await EpisodeSelectedAsync(id);
            }
            else if (sender is CoverFolder && (selectedid.ToString() != id.ToString() || (selectedType == ElementType.Files || (selectedType == ElementType.Folders && selectedFolderType != foldertype))))
            {
                //Validate Folder Type
                if (foldertype == FolderType.Folders)
                {
                    //Set Selected Folder to Sender
                    await FolderSelectedAsync(id);
                }
                else if (foldertype == FolderType.TVShowFolders)
                {
                    //Set Selected TV Show Folder to Sender
                    await TVShowFolderSelectedAsync(id);
                }
                else if (foldertype == FolderType.SeasonFolders)
                {
                    //Set Selected Season Folder to Sender
                    await SeasonFolderSelectedAsync(id);
                }
            }

            //Wait for Information Pane to Open or Close
            await Task.Delay(700);

            //Check if Element Type is ElementType.Files
            if (selectedType == ElementType.Folders && selectedFolderType == FolderType.TVShowFolders)
            {
                //Get isFavourite Value
                int isfavourite = selectedTVShowFolder.isFavourite;

                //Create Context Menu Favourite Item
                menuitem = new ContextMenuItem() { Header = isfavourite == 1 ? "Unfavourite" : "Favourite" };

                //Set Click Event Handler
                menuitem.Click += cmiFavourite_Click;
            }

            //Set Context Menu Position
            Elements.SetContextMenu(icItems, selectedType, selectedid, menuitem, foldertype);
        }

        private void BringInToView(FolderType foldertype = FolderType.Null)
        {
            //Variables
            int index = -1;

            //Get Selected Element
            ElementBase element = Elements.GetElement(icItems, selectedType, selectedId, foldertype);

            //Attempt to Bring the Selected Element Into the View
            if (element != null) { element.BringIntoView(); }

            //Check if the selected element has not been found or if the selected element is not visible
            if (element == null || !element.IsVisible)
            {
                //Get Virtualizing Wrap Panel
                VirtualizingWrapPanel vwpItems = Elements.FindVisualChildren<VirtualizingWrapPanel>(icItems).ElementAt(0);

                //Validate Element Type
                if(selectedType == ElementType.Files)
                {
                    //Get Index
                    index = Composite.IndexOf(selectedEpisode);
                }
                else if(selectedType == ElementType.Folders)
                {
                    //Validate Folder Type
                    if(selectedFolderType == FolderType.Folders)
                    {
                        //Get Index
                        index = Composite.IndexOf(selectedFolder);
                    }
                    else if(selectedFolderType == FolderType.TVShowFolders)
                    {
                        //Get Index
                        index = Composite.IndexOf(selectedTVShowFolder);
                    }
                    else if(selectedFolderType == FolderType.SeasonFolders)
                    {
                        //Get Index
                        index = Composite.IndexOf(selectedSeasonFolder);
                    }
                }

                //Validate Index and Bring Selected Element Index Into View
                if (index != -1) { vwpItems.BringIndexIntoViewPublic(index); }
            }
        }



        // Folders
        // =====================================================================
        // =====================================================================
        private async Task FolderSelectedAsync(int id)
        {
            //Check if the folder has been clicked twice
            if (selectedFolder != null && selectedId == id && selectedType == ElementType.Folders && selectedFolderType == FolderType.Folders)
            {
                //Set Active Folder
                activeFolder = selectedFolder.Id;

                //Load Elements for selectedFolder
                await LoadAsync();

                //Load Folder in Navigation Bar
                nbNavigationBar.Load(activeFolder, nameof(FolderType.Folders));

                //Toggle Submenu Functions
                ToggleSubmenuFunctions(FolderType.Folders);
            }
            else
            {
                //Unset Selected Element
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedEpisode, ref selectedSeasonFolder, ref selectedTVShowFolder, ref selectedFolder, ref selectedType, ref selectedFolderType);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Episodes.SingleOrDefault(i => i.Id == id), out selectedEpisode, SeasonFolders.SingleOrDefault(i => i.Id == id), out selectedSeasonFolder, TVShowFolders.SingleOrDefault(i => i.Id == id), out selectedTVShowFolder, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType, FolderType.Folders, out selectedFolderType);

                //Disable Submenu Buttons
                ToggleSubButtons(true);

                //Hide Information Pane
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Wait for the Information Pane to Close
                await Task.Delay(250);

                //Bring Element Into View
                BringInToView(FolderType.Folders);
            }
        }



        // TV Show Folders
        // =====================================================================
        // =====================================================================
        private async Task TVShowFolderSelectedAsync(int id)
        {
            //Check if the folder has been clicked twice
            if (selectedTVShowFolder != null && selectedType == ElementType.Folders && selectedFolderType == FolderType.TVShowFolders && selectedId == id)
            {
                //Set Active Folder
                activeTVShowFolder = selectedTVShowFolder.Id;

                //Load Elements for selectedTVShowFolder
                await LoadAsync();

                //Load Folder in Navigation Bar
                nbNavigationBar.Load(activeTVShowFolder, nameof(FolderType.TVShowFolders));

                //Toggle Submenu Functions
                ToggleSubmenuFunctions(FolderType.TVShowFolders);
            }
            else
            {
                //Unset Selected Element
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedEpisode, ref selectedSeasonFolder, ref selectedTVShowFolder, ref selectedFolder, ref selectedType, ref selectedFolderType);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Episodes.SingleOrDefault(i => i.Id == id), out selectedEpisode, SeasonFolders.SingleOrDefault(i => i.Id == id), out selectedSeasonFolder, TVShowFolders.SingleOrDefault(i => i.Id == id), out selectedTVShowFolder, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType, FolderType.TVShowFolders, out selectedFolderType);

                //Display TV Show Information
                DisplayTVShowFolderInfo();

                //Enable Submenu Buttons
                ToggleSubButtons(true);

                //Show Information Pane
                Pane.Toggle(PaneToggle.Open, informationPane, icItems);

                //Wait for the Information Pane to Close
                await Task.Delay(250);

                //Bring Element Into View
                BringInToView(FolderType.TVShowFolders);
            }
        }

        private void DisplayTVShowFolderInfo()
        {
            //Toggle Information Panes
            ToggleInformationPanes(true, ElementType.Folders, FolderType.TVShowFolders);

            //Display Main
            Title.Content = Formatter.FormatName(selectedTVShowFolder.Name, selectedTVShowFolder.CustomName);
            Cover.Source = Formatter.FormatImage(MediaType.TVShows, selectedTVShowFolder.CoverImage, selectedTVShowFolder.CustomCoverImage);

            //Standard Details
            tbfCreated.SetValue(Formatter.FormatCreation(selectedTVShowFolder.CreationTime, selectedTVShowFolder.CreationDate));

            //Web Details
            tbfSeasons.SetValue(Formatter.FormatNumeric(selectedTVShowFolder.SeasonCount));
            tbfEpisodes.SetValue(Formatter.FormatNumeric(selectedTVShowFolder.EpisodeCount));
            tbfReleaseDate.SetValue(Formatter.FormatVirtualEntertainmentReleaseDate(selectedTVShowFolder.ReleaseDate, selectedTVShowFolder.Region));
            tbfReleasePeriod.SetValue(selectedTVShowFolder.ReleasePeriod);
            tbfAgeRating.SetValue(selectedTVShowFolder.AgeRating);
            tbfGenres.SetList(selectedTVShowFolder.Genres);
            tbfStars.SetList(selectedTVShowFolder.Stars);
            tbfProductionCompanies.SetList(selectedTVShowFolder.ProductionCompanies);

            //IMDB Details
            tbfCreators.SetList(selectedTVShowFolder.Creators);

            //Metacritic Details
            tbfUserRating.SetRating(selectedTVShowFolder.UserScore, selectedTVShowFolder.UserReviewCount);
            tbfCriticRating.SetRating(selectedTVShowFolder.CriticScore, selectedTVShowFolder.CriticReviewCount);
        }

        private async void UpdateFavourite(int value)
        {
            //Set selectedTVShowFolder isFavourite Value
            selectedTVShowFolder.isFavourite = value;

            //Set TV Show Folders Observable Collection's selectedTVShowFolder isFavourite Value
            TVShowFolders.ElementAt(TVShowFolders.IndexOf(selectedTVShowFolder)).isFavourite = value;

            //Update selectedTVShowFolder's Database isFavourite Value
            Database.UpdateFavourite(selectedTVShowFolder.Id, value, MediaType.TVShows, FolderType.TVShowFolders);

            //Check if Favourites are being Shown at the moment and the selectedTVShowFolder is being Unfavourited
            if (value == 0 && tbtnShowFavourites.IsSelected)
            {
                //Load TV Shows
                await LoadAsync(ElementType.Folders, null, FolderType.TVShowFolders);
            }
        }



        // Season Folders
        // =====================================================================
        // =====================================================================
        private async Task SeasonFolderSelectedAsync(int id)
        {
            //Check if the folder has been clicked twice
            if (selectedSeasonFolder != null && selectedType == ElementType.Folders && selectedFolderType == FolderType.SeasonFolders && selectedId == id)
            {
                //Set Active Folder
                activeSeasonFolder = selectedSeasonFolder.Id;

                //Load Elements for selectedSeasonFolder
                await LoadAsync();

                //Load Folder in Navigation Bar
                nbNavigationBar.Load(activeSeasonFolder, nameof(FolderType.SeasonFolders));
            }
            else
            {
                //Unset Selected Element
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedEpisode, ref selectedSeasonFolder, ref selectedTVShowFolder, ref selectedFolder, ref selectedType, ref selectedFolderType);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Episodes.SingleOrDefault(i => i.Id == id), out selectedEpisode, SeasonFolders.SingleOrDefault(i => i.Id == id), out selectedSeasonFolder, TVShowFolders.SingleOrDefault(i => i.Id == id), out selectedTVShowFolder, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType, FolderType.SeasonFolders, out selectedFolderType);

                //Display Season Information
                DisplaySeasonFolderInfo();

                //Enable Submenu Buttons
                ToggleSubButtons(true);

                //Show Information Pane
                Pane.Toggle(PaneToggle.Open, informationPane, icItems);

                //Wait for the Information Pane to Close
                await Task.Delay(250);

                //Bring Element Into View
                BringInToView(FolderType.SeasonFolders);
            }
        }

        private void DisplaySeasonFolderInfo()
        {
            //Toggle Information Panes
            ToggleInformationPanes(true, ElementType.Folders, FolderType.SeasonFolders);

            //Display Main
            Title.Content = selectedSeasonFolder.Name;
            Cover.Source = Formatter.FormatImage(MediaType.TVShows, selectedSeasonFolder.CoverImage, selectedSeasonFolder.CustomCoverImage);

            //Standard Details
            tbfSeasonNumber.SetValue(Formatter.FormatNumeric(selectedSeasonFolder.SeasonNumber));
            tbfsCreated.SetValue(Formatter.FormatCreation(selectedSeasonFolder.CreationTime, selectedSeasonFolder.CreationDate));

            //Web Details
            tbfSeasonEpisodes.SetValue(Formatter.FormatNumeric(selectedSeasonFolder.EpisodeCount));
        }



        // Items
        // =====================================================================
        // =====================================================================
        private async Task EpisodeSelectedAsync(int id)
        {
            //Check if the episode has been clicked twice
            if (selectedType == ElementType.Files && selectedId != 0 && selectedEpisode != null && selectedId == id)
            {
                //Check if File is Valid
                if (Validation.File(selectedEpisode.FilePath))
                {
                    //Get The Main Window
                    Window mainWindow = Application.Current.MainWindow;

                    //Set Main Window's Data Context to New Video Player View
                    mainWindow.DataContext = new VideoPlayerView(MediaType.TVShows, cboxSortBy.GetIndexes(), nbNavigationBar.GetStack(NavigationBarStackType.Unloaded), nbNavigationBar.GetStack(NavigationBarStackType.Loaded), activeSeasonFolder, tbtnShowFavourites.IsSelected, null, null, null, Episodes.Where(i => i.OwnerId == nbNavigationBar.selectedId).ToList(), selectedEpisode);
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
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedEpisode, ref selectedSeasonFolder, ref selectedTVShowFolder, ref selectedFolder, ref selectedType, ref selectedFolderType);

                //Wait for the Items to be added to the Movies Observable Collection (Required for Return)
                await Task.Delay(300);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Episodes.SingleOrDefault(i => i.Id == id), out selectedEpisode, SeasonFolders.SingleOrDefault(i => i.Id == id), out selectedSeasonFolder, TVShowFolders.SingleOrDefault(i => i.Id == id), out selectedTVShowFolder, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Files, out selectedType, FolderType.Null, out selectedFolderType);

                //Display Episode Information
                DisplayEpisodeInfo();

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

        private void DisplayEpisodeInfo()
        {
            //Toggle Information Panes
            ToggleInformationPanes(true, ElementType.Files, FolderType.Null);

            //Display Main
            Title.Content = Formatter.FormatName(selectedEpisode.Name, selectedEpisode.CustomName);
            Cover.Source = Formatter.FormatImage(MediaType.Movies, selectedEpisode.CoverImage);

            //Display Standard Details
            tbiSeason.SetValue(Formatter.FormatNumeric(selectedEpisode.Season));
            tbiEpisode.SetValue(Formatter.FormatNumeric(selectedEpisode.EpisodeNumber));
            tbiResolution.SetValue(Formatter.FormatResolution(selectedEpisode.Width, selectedEpisode.Height));
            tbiDuration.SetValue(Formatter.FormatDuration(selectedEpisode.Duration));
            tbiFramerate.SetValue(Formatter.FormatFramerate(selectedEpisode.Framerate));
            tbiFormat.SetValue(selectedEpisode.Format);
            tbiFileSize.SetValue(Formatter.FormatFileSize(selectedEpisode.FileSize));
            tbiCreated.SetValue(Formatter.FormatCreation(selectedEpisode.CreationTime, selectedEpisode.CreationDate));

            //Display Advance Details
            tbiSampleRate.SetValue(Formatter.FormatSampleRate(selectedEpisode.SampleRate));
            tbiAudioChannels.SetValue(selectedEpisode.AudioChannels);
            tbiFramerateMode.SetValue(Formatter.FormatFramerateMode(selectedEpisode.FramerateMode));

            //Display IMDB Details
            tbiAirDate.SetValue(Formatter.FormatVirtualEntertainmentReleaseDate(selectedEpisode.AirDate, selectedEpisode.Region));
            tbiAgeRating.SetValue(selectedEpisode.AgeRating);
            tbiGenres.SetList(selectedEpisode.Genres);
            tbiStars.SetList(selectedEpisode.Stars);
            tbiDirectors.SetList(selectedEpisode.Directors);
            tbiWriters.SetList(selectedEpisode.Writers);
            tbiProductionCompanies.SetList(selectedEpisode.ProductionCompanies);
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
                //Set Selected Episode
                await EpisodeSelectedAsync(selectedid);
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
            if (name == "episodespanel")
            {
                //Clear Season Panel Fields
                OptionsPanel.ClearPanel(null, null, null, new List<optOpenDialog> { odEpisodes });
            }
            if (name == "seasonpanel")
            {
                //Clear Season Panel Fields
                OptionsPanel.ClearPanel(null, null, null, new List<optOpenDialog> { odSeasonDirectory, odSeasonCoverImage }, new List<optNumericBox>() { nbSeason });
            }
            else if (name == "tvshowpanel")
            {
                //Clear TV Show Panel Fields
                OptionsPanel.ClearPanel(null, new List<optTextBoxLong> { tbTVShowCustomName }, new List<optSearchBox> { sbTVShow }, new List<optOpenDialog> { odTVShowCoverImage });
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
            if(name == "Episodes")
            {
                //Set Panel Contents
                EpisodesPanelTitle.Content = $"{process} {name}";
                btnProcessEpisodes.Content = process;
            }
            else if (name == "Season")
            {
                //Set Max Season Number
                nbSeason.Max = selectedTVShowFolder.SeasonCount;

                //Set Panel Contents
                SeasonPanelTitle.Content = $"{process} {name}";
                btnProcessSeason.Content = process;
            }
            else if (name == "TV Show")
            {
                //Set Panel Contents
                TVShowPanelTitle.Content = $"{process} {name}";
                btnProcessTVShow.Content = process;
            }
            else if (name == "Folder")
            {
                //Set Panel Contents
                FolderPanelTitle.Content = $"{process} {name}";
                btnProcessFolder.Content = process;
            }
        }



        // Submenu Functions
        // =====================================================================
        // =====================================================================
        private void ToggleSubmenuFunctions(FolderType foldertype)
        {
            //Initialize Variables
            bool isfavouritesenabled = foldertype == FolderType.Folders ? true : false;
            bool issortbyenabled = foldertype == FolderType.Folders ? true : false;

            //Toggle Sort By Dropdown and Show Favourites Toggle
            ToggleState.UIElement(cboxSortBy, issortbyenabled);
            ToggleState.UIElement(tbtnShowFavourites, isfavouritesenabled);
        }

        private void ToggleSubButtons(bool IsEnabled)
        {
            //Check Selected Element Type and Folder Type or if isEnabled is Set to False
            if (!IsEnabled)
            {
                //Toggle Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnDelete, btnEdit, btnRemove, btnShowInExplorer }, IsEnabled);
            }
            else if (selectedType == ElementType.Files)
            {
                //Toggle Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnDelete, btnRemove, btnShowInExplorer }, IsEnabled);
            }
            else if (selectedType == ElementType.Folders && selectedFolderType == FolderType.Folders)
            {
                //Disable the Delete and Show In Explorer Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnDelete, btnShowInExplorer }, false);

                //Enable the Edit and Remove Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnEdit, btnRemove }, true);
            }
            else if (selectedType == ElementType.Folders)
            {
                //Disable the Delete and Show In Explorer Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnShowInExplorer }, false);

                //Enable the Edit and Remove Submenu Buttons
                ToggleState.UIElements(new subButton[] { btnDelete, btnEdit, btnRemove }, true);
            }
        }



        // Add Folder
        // =====================================================================
        // =====================================================================
        private async void AddFolder()
        {
            //Get Base Folder Data
            Folder folder = new Folder() { OwnerId = fbdFolderSaveLocation.Id, Name = $"{tbFolderName.Content}", Type = nameof(MediaType.TVShows), FolderType = nameof(FolderType.Folders) };

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



        // Add TV Show
        // =====================================================================
        // =====================================================================
        private void AddTVShow()
        {
            //Variables
            bool isAddTVShow = false;

            //Get Base TV Show Data
            TVShowFolder tvshowfolder = new TVShowFolder() { OwnerId = fbdTVShowSaveLocation.Id, CustomName = $"{tbTVShowCustomName.Content}", CustomCoverImage = $"{odTVShowCoverImage.Content}", IMDBLink = $"{sbTVShow.IMDBLink}", MetaCriticLink = $"{sbTVShow.DefaultLink}" };

            //Disable Process TV Show Button
            ToggleState.UIElement(btnProcessTVShow, false);

            //Hide TV Show Panel
            ToggleState.Panel(TVShowPanel, false);

            //Get Boolean Value Representing if the TV Show Already Exists within the Database (TVShowFolders ObservableCollection)
            bool isEmpty = !TVShowFolders.Any(i => i.Name == tvshowfolder.Name);

            //Validate Cover Image Path and URL
            bool isValid = Validation.Process(tvshowfolder.CustomCoverImage, true, true, new string[] { "https://www.imdb.com/", "https://www.metacritic.com/" }, new string[] { tvshowfolder.IMDBLink, tvshowfolder.MetaCriticLink }, new int[] { 37, 35 });


            //Check if the tv show has already been added to the database, and if the cover image path and urls are valid
            //Else check if the tv show folder has not been added to the database and if the cover image path and urls are valid
            if (!isEmpty && isValid)
            {
                //Show TV Show Exists Message
                MessageBoxResult result = CustomMessageBox.ShowYesNoCancel(str_TVShowExists, "WARNING", "Add", "Edit", "Cancel", MessageBoxImage.Warning);

                //Check Result
                if (result == MessageBoxResult.Yes)
                {
                    //Set isAddTVShow boolean to true
                    isAddTVShow = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    //Show TV Show Panel
                    ToggleState.Panel(TVShowPanel, true);
                }
            }
            else if (isEmpty && isValid)
            {
                //Set isAddTVShow boolean to true
                isAddTVShow = true;
            }
            else
            {
                //Show Validation Error Message
                MessageBoxResult result = CustomMessageBox.ShowYesNo(str_ValidationError, "Error", "Edit", "Cancel", MessageBoxImage.Error);

                //Check Result
                if (result == MessageBoxResult.Yes)
                {
                    //Show TV Show Panel
                    ToggleState.Panel(TVShowPanel, true);
                }
            }


            //Check if the isAddTVShow boolean is set to true
            if (isAddTVShow)
            {
                //Run Process Add TV Show Method
                ProcessAddTVShow(tvshowfolder);
            }
        }

        private async void ProcessAddTVShow(TVShowFolder tvshowfolder)
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Stop Search Engine on Different Thread
            _ = Task.Run(() => { SearchEngine.Stop(); });

            //Process Add TV Show on Different Thread
            await Task.Run(async () =>
            {
                //Configure Fetcher
                bool isValid = await Fetcher.ConfigureFetcherAsync(MediaType.TVShows, string.Empty, string.Empty, tvshowfolder.MetaCriticLink, tvshowfolder.IMDBLink);

                //Check if Configuration was Successful
                if (isValid)
                {
                    //Get TV Show Details
                    tvshowfolder = Fetcher.GetTVShowFolderAsync(tvshowfolder, TVShowFolders.Count() + 1);

                    //Close Fetcher
                    Fetcher.Close();

                    //Save TV Show Folder
                    Database.SaveFolder(tvshowfolder, FolderType.TVShowFolders);

                    //Load TV Show Folders on UI Thread
                    Application.Current.Dispatcher.Invoke(new Action(async () => { await LoadAsync(ElementType.Folders, tvshowfolder, FolderType.TVShowFolders); }));
                }
                else
                {
                    //Show Error Message on UI Thread
                    Application.Current.Dispatcher.Invoke(new Action(() => { CustomMessageBox.ShowOK(str_TVShowInternetError, "ERROR", "OK", MessageBoxImage.Error); }));
                }
            });

            //Hide Loading Panel
            ToggleState.Loading(false);
        }



        // Add Season
        // =====================================================================
        // =====================================================================
        private void AddSeason()
        {
            //Variables
            bool isAddSeason = false;

            //Get Base Season Data
            SeasonFolder seasonfolder = new SeasonFolder() { OwnerId = selectedTVShowFolder.Id, FilePath = $"{odSeasonDirectory.Content}", SeasonNumber = nbSeason.Value, CustomCoverImage = $"{odSeasonCoverImage.Content}" };

            //Disable Process Season Button
            ToggleState.UIElement(btnProcessSeason, false);

            //Hide Season Panel
            ToggleState.Panel(SeasonPanel, false);

            //Get Boolean Value Representing if the Season Already Exists within the Database (SeasonFolders ObservableCollection)
            bool isEmpty = !SeasonFolders.Any(i => i.SeasonNumber == seasonfolder.SeasonNumber);

            //Validate Cover Image Path and Save Location
            bool isValid = Validation.File(seasonfolder.CustomCoverImage, true) && TVShowFolders.Any(i => i.Id == seasonfolder.OwnerId);


            //Check if the season has already been added to the database, and if the cover image path and save location are valid
            //Else check if the season folder has not been added to the database and if the cover image path and save location are valid
            if (!isEmpty && isValid)
            {
                //Show Season Exists Message
                MessageBoxResult result = CustomMessageBox.ShowYesNoCancel(str_SeasonExists, "WARNING", "Add", "Edit", "Cancel", MessageBoxImage.Warning);

                //Check Result
                if (result == MessageBoxResult.Yes)
                {
                    //Set isAddSeason boolean to true
                    isAddSeason = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    //Show Season Panel
                    ToggleState.Panel(SeasonPanel, true);
                }
            }
            else if (isEmpty && isValid)
            {
                //Set isAddSeason boolean to true
                isAddSeason = true;
            }
            else
            {
                //Show Validation Error Message
                MessageBoxResult result = CustomMessageBox.ShowYesNo(str_ValidationError, "Error", "Edit", "Cancel", MessageBoxImage.Error);

                //Check Result
                if (result == MessageBoxResult.Yes)
                {
                    //Show Season Panel
                    ToggleState.Panel(SeasonPanel, true);
                }
            }


            //Check if the isAddSeason boolean is set to true
            if (isAddSeason)
            {
                //Run Process Add Season Method
                ProcessAddSeason(seasonfolder);
            }
        }

        private async void ProcessAddSeason(SeasonFolder seasonfolder)
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Stop Search Engine on Different Thread
            _ = Task.Run(() => { SearchEngine.Stop(); });

            //Process Add Season on Different Thread
            await Task.Run(async () =>
            {
                //Get Web Links and Cover Image
                seasonfolder.IMDBLink = Fetcher.GetIMDBSeasonLink(selectedTVShowFolder.IMDBLink, seasonfolder.SeasonNumber);
                seasonfolder.MetaCriticLink = Fetcher.GetMetacriticSeasonLink(selectedTVShowFolder.MetaCriticLink, seasonfolder.SeasonNumber);
                seasonfolder.CoverImage = selectedTVShowFolder.CoverImage;

                //Configure Fetcher
                bool isValid = await Fetcher.ConfigureFetcherAsync(MediaType.TVShows, string.Empty, string.Empty, seasonfolder.MetaCriticLink, seasonfolder.IMDBLink);

                //Check if Configuration was Successful
                if (isValid)
                {
                    //Get Season Details
                    seasonfolder = Fetcher.GetSeasonFolder(seasonfolder, SeasonFolders.Count() + 1, selectedTVShowFolder.CustomCoverImage);

                    //Close Fetcher
                    Fetcher.Close();

                    //Save Season Folder
                    Database.SaveFolder(seasonfolder, FolderType.SeasonFolders);

                    //Run Tasks on UI Thread
                    Application.Current.Dispatcher.Invoke(new Action(async () =>
                    {
                        //Load Season Folders
                        await LoadAsync(ElementType.Folders, seasonfolder, FolderType.SeasonFolders);

                        //Add Episodes
                        AddEpisodes(seasonfolder.Id, Fetcher.GetEpisodes(new DirectoryInfo(seasonfolder.FilePath), new string[] { ".mkv", ".mp4", ".avi", ".mov" }));
                    }));
                }
                else
                {
                    //Show Error Message on UI Thread
                    Application.Current.Dispatcher.Invoke(new Action(() => { CustomMessageBox.ShowOK(str_SeasonInternetError, "ERROR", "OK", MessageBoxImage.Error); }));
                }
            });

            //Hide Loading Panel
            ToggleState.Loading(false);
        }



        // Add Episodes
        // =====================================================================
        // =====================================================================
        private void AddEpisodes(int seasonid = -1, List<FileInfo> episodes = null)
        {
            //Variables
            List<Episode> baseepisodes = new List<Episode>(), invalidepisodes = new List<Episode>(), existingepisodes = new List<Episode>();

            //Get Season Folder
            SeasonFolder seasonfolder = SeasonFolders.Single(i => episodes == null && i.Id == nbNavigationBar.selectedId || episodes != null && i.Id == seasonid);

            //Validate Add Type
            if (episodes == null)
            {
                //Loop through odEpisodes Selected Items
                foreach (OpenDialogItem item in odEpisodes.Contents)
                {
                    //Get Current Looped Items Base Data
                    baseepisodes.Add(new Episode() { OwnerId = nbNavigationBar.selectedId, Season = seasonfolder.SeasonNumber, CoverImage = seasonfolder.CoverImage, FilePath = item.FilePath });
                }

                //Disable Process Episodes Button
                ToggleState.UIElement(btnProcessEpisodes, false);

                //Hide Episodes Panel
                ToggleState.Panel(EpisodesPanel, false);
            }
            else
            {
                //Loop through odEpisodes Selected Items
                foreach (FileInfo item in episodes)
                {
                    //Get Current Looped Items Base Data
                    baseepisodes.Add(new Episode() { OwnerId = seasonid, Season = seasonfolder.SeasonNumber, CoverImage = seasonfolder.CoverImage, FilePath = item.FullName });
                }
            }

            //Loop through baseepisodes List
            foreach (Episode item in baseepisodes)
            {
                //Check if the Current Looped Item's File Path is Invalid
                //Else Check if the Current Looped Item has Already Been Added to the Database
                if (!Validation.Process(item.FilePath))
                {
                    //Add Current Looped Item to invalidepisodes List
                    invalidepisodes.Add(item);

                    //Remove Current Looped Item from baseepisodes List
                    baseepisodes.Remove(item);
                }
                else if (Episodes.Any(i => item.FilePath == i.FilePath))
                {
                    //Add Current Looped Item to existingepisodes List
                    existingepisodes.Add(item);
                }
            }

            //Check if the baseepisodes List still Contains Items
            if (baseepisodes.Count > 0)
            {
                //Loop through invalidepisodes List
                foreach (Episode item in invalidepisodes)
                {
                    //Show File Path Error Message
                    MessageBoxResult result = CustomMessageBox.ShowDialog($"The file path does not exist for {Path.GetFileName(item.FilePath)}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //Loop through existingepisodes List
                foreach (Episode item in existingepisodes)
                {
                    //Show Exists Message
                    MessageBoxResult result = CustomMessageBox.ShowYesNo($"{Path.GetFileName(item.FilePath)} has already been added to the database. Are you sure you want to add this episode to the application?", "WARNING", "Yes", "No", MessageBoxImage.Warning);

                    //Validate Result
                    if (result == MessageBoxResult.No)
                    {
                        //Remove Item from baseepisodes List
                        baseepisodes.Remove(item);
                    }
                }

                //Check if the baseepisodes List still Contains Items
                if (baseepisodes.Count > 0)
                {
                    //Run Process Add Episodes Method
                    ProcessAddEpisodesAsync(baseepisodes, seasonid);
                }
            }
        }

        private async void ProcessAddEpisodesAsync(List<Episode> episodes, int seasonid)
        {
            //Variables
            int counter = Episodes.Count + 1;

            //Show Loading Panel
            ToggleState.Loading(true);

            //Run Multithreaded Task
            await Task.Run(async () =>
            {
                //Get Season Folder
                SeasonFolder seasonfolder = SeasonFolders.Count > 0 && SeasonFolders.Any(i => seasonid == -1 && i.Id == nbNavigationBar.selectedId || seasonid != -1 && i.Id == seasonid) ? SeasonFolders.Single(i => seasonid == -1 && i.Id == nbNavigationBar.selectedId || seasonid != -1 && i.Id == seasonid) : null;

                //Loop through Episodes episodes
                foreach (Episode item in episodes)
                {
                    //Get Episode Number
                    int episodenumber = Fetcher.GetEpisodeNumber(item.FilePath);

                    //Get Episode Links
                    string[] links = await Fetcher.GetEpisodeLinksAsync(seasonfolder.MetaCriticLink, seasonfolder.IMDBLink, episodenumber);

                    //Configure Fetcher
                    await Fetcher.ConfigureFetcherAsync(MediaType.Episodes, item.FilePath, string.Empty, links[0], links[1]);

                    //Get Episode Details
                    Episode episode = Fetcher.GetEpisode(TVShowFolders.FirstOrDefault(i => i.Id == seasonfolder.OwnerId), item, counter, episodenumber, links[1], links[0]);

                    //Close Fetcher
                    Fetcher.Close();

                    //Save Episode
                    Database.SaveItem(MediaType.Episodes, episode);

                    //Increment Counter
                    counter++;
                }
            });

            //Set Selected Folder Type
            selectedFolderType = nbNavigationBar.GetFolderType();

            //Validate Selected Episode
            if (selectedEpisode != null)
            {
                //Get Selected Episode ID
                int id = selectedEpisode.Id;

                //Load Episodes
                await LoadAsync(ElementType.Files);

                //Reselect Selected Episode
                await EpisodeSelectedAsync(id);
            }
            else
            {
                //Load Episodes
                await LoadAsync(ElementType.Files);
            }

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



        // Edit TV Show Folder
        // =====================================================================
        // =====================================================================
        private void LoadTVShowFolderEdit()
        {
            //Validate Selected Folder
            if (TVShowFolders.Contains(selectedTVShowFolder))
            {
                //Set Panel
                SetPanel("Edit", "TV Show");

                //Hide Search Box
                ToggleState.UIElement(sbTVShow, Visibility.Collapsed);

                //Load Editable Values
                OptionsPanel.LoadEditableValues(tbTVShowCustomName, selectedTVShowFolder.CustomName, fbdTVShowSaveLocation, selectedTVShowFolder.OwnerId, nbNavigationBar.GetFolderPath(), selectedType, selectedid, new List<optOpenDialog>() { odTVShowCoverImage }, new List<string>() { selectedTVShowFolder.CustomCoverImage }, MediaType.Null, null, null, FolderType.TVShowFolders);

                //Disable Process Folder Button
                ToggleState.UIElement(btnProcessTVShow, false);

                //Show Folder Panel
                ToggleState.Panel(TVShowPanel, true);
            }
            else
            {
                //Show Edit Error Message
                CustomMessageBox.ShowOK(str_EditError, "ERROR", "OK", MessageBoxImage.Error);
            }
        }

        private async void EditTVShowFolder()
        {
            //Initialize Variables
            string oldcustomcoverimage = selectedTVShowFolder.CustomCoverImage;

            //Get Base Folder Data
            TVShowFolder folder = new TVShowFolder() { CustomName = $"{tbTVShowCustomName.Content}", CustomCoverImage = $"{odTVShowCoverImage.Content}", OwnerId = fbdTVShowSaveLocation.Id };

            //Hide TV Show Panel
            ToggleState.Panel(TVShowPanel, false);

            //Check if the Folder has been Updated
            if (selectedTVShowFolder.Name != folder.Name || selectedTVShowFolder.CustomCoverImage != folder.CustomCoverImage || selectedTVShowFolder.OwnerId != folder.OwnerId)
            {
                //Show Loading Panel
                ToggleState.Loading(true);

                //Assign selectedTVShowFolder into newtvshowfolder object
                TVShowFolder newtvshowfolder = selectedTVShowFolder;

                //Clear Navigation Bar'S Navigation (Dependent on if the TV Show Folder has Changed Locations)
                if (selectedTVShowFolder.OwnerId != folder.OwnerId) { nbNavigationBar.ClearNavigation(); }

                //Set New Folder Values
                newtvshowfolder.CustomName = folder.CustomName;
                newtvshowfolder.CustomCoverImage = Fetcher.GetCustomCoverImage(MediaType.TVShows, newtvshowfolder.Id, folder.CustomCoverImage, "custom");
                newtvshowfolder.OwnerId = folder.OwnerId;

                //Update Folder
                Database.UpdateFolder(newtvshowfolder.Id, newtvshowfolder, FolderType.TVShowFolders);

                //Close Information Pane
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Load Folders
                await LoadAsync(ElementType.Folders, newtvshowfolder, FolderType.TVShowFolders);

                //Validate Custom Cover Image Deletion
                if (newtvshowfolder.CustomCoverImage != oldcustomcoverimage && !SeasonFolders.Any(i => i.CustomCoverImage == oldcustomcoverimage)) { try { File.Delete(oldcustomcoverimage); } catch { } }

                //Hide Loading Panel
                ToggleState.Loading(false);
            }
        }



        // Edit Season Folder
        // =====================================================================
        // =====================================================================
        private void LoadSeasonFolderEdit()
        {
            //Validate Selected Folder
            if (SeasonFolders.Contains(selectedSeasonFolder))
            {
                //Set Selected TV Show Folder
                selectedTVShowFolder = TVShowFolders.FirstOrDefault(i => i.Id == nbNavigationBar.selectedId && i.FolderType == nbNavigationBar.selectedFolderType);

                //Set Panel
                SetPanel("Edit", "Season");

                //Hide Season Directory Open Dialog and Season Number Numeric Box
                ToggleState.UIElements(new UIElement[] { odSeasonDirectory, nbSeason }, Visibility.Collapsed);

                //Load Editable Values
                OptionsPanel.LoadEditableValues(null, null, null, 0, null, selectedType, selectedId, new List<optOpenDialog>() { odSeasonCoverImage }, new List<string>() { selectedSeasonFolder.CustomCoverImage }, MediaType.Null, null, null, FolderType.SeasonFolders);

                //Disable Process Folder Button
                ToggleState.UIElement(btnProcessSeason, false);

                //Show Season Panel
                ToggleState.Panel(SeasonPanel, true);

            }
            else
            {
                //Show Edit Error Message
                CustomMessageBox.ShowOK(str_EditError, "ERROR", "OK", MessageBoxImage.Error);
            }
        }

        private async void EditSeason()
        {
            //Initialize Variables
            string oldcustomcoverimage = selectedSeasonFolder.CustomCoverImage;

            //Get Base Folder Data
            SeasonFolder folder = new SeasonFolder() { CustomCoverImage = $"{odSeasonCoverImage.Content}" };

            //Hide Season Panel
            ToggleState.Panel(SeasonPanel, false);

            //Check if the Season has been Updated
            if (selectedSeasonFolder.CustomCoverImage != folder.CustomCoverImage)
            {
                //Show Loading Panel
                ToggleState.Loading(true);

                //Assign selectedSeasonFolder into newseasonfolder object
                SeasonFolder newseasonfolder = selectedSeasonFolder;

                //Set New Cover Image
                newseasonfolder.CustomCoverImage = Fetcher.GetCustomCoverImage(MediaType.Seasons, newseasonfolder.OwnerId, folder.CustomCoverImage, $"{newseasonfolder.SeasonNumber} custom");

                //Update Season Folder
                Database.UpdateFolder(newseasonfolder.Id, newseasonfolder, FolderType.SeasonFolders);

                //Close Information Pane
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Load Folders
                await LoadAsync(ElementType.Folders, newseasonfolder, FolderType.SeasonFolders);

                //Validate Custom Cover Image Deletion
                if (newseasonfolder.CustomCoverImage != oldcustomcoverimage && !TVShowFolders.Any(i => i.CustomCoverImage == oldcustomcoverimage)) { try { File.Delete(oldcustomcoverimage); } catch { } }

                //Hide Loading Panel
                ToggleState.Loading(false);
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
            bool isRemoved = DiscardElements.RemoveFolder(MediaType.TVShows, activeFolder);

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



        // Remove TV Show Folder
        // =====================================================================
        // =====================================================================
        private async void RemoveTVShowFolder(RemovalType type = RemovalType.Remove)
        {
            //Initialize Variables
            bool isDeleted = false, isCancel = false;
            List<SeasonFolder> seasonfolders = SeasonFolders.Where(i => i.OwnerId == selectedTVShowFolder.Id).ToList();

            //Show Loading Panel
            ToggleState.Loading(true);

            //Check if Removal Type is Delete
            if (type == RemovalType.Delete)
            {
                //Delete All the TV Show Season Folders from the Computer System
                DiscardElements.DeleteTVShowFolder(out isCancel, out isDeleted, selectedTVShowFolder, seasonfolders, Episodes.ToList());
            }

            //Check if the selected tv show's season folders were deleted from the computer system or if the removal type is set to remove
            //Else check if the user cancelled the removal
            if (isDeleted == true || type == RemovalType.Remove)
            {
                //Remove TV Show From Application
                bool isRemoved = DiscardElements.RemoveTVShowFolder(type, selectedTVShowFolder);

                //Check if the isRemoved Boolean is Set to True
                if (isRemoved == true)
                {
                    //Remove TV Show Folder From Navigation Bar
                    nbNavigationBar.Remove(selectedTVShowFolder.Id, selectedTVShowFolder.OwnerId, selectedTVShowFolder.Name, selectedTVShowFolder.FolderType);

                    //Set Selected Folder Type
                    selectedFolderType = nbNavigationBar.GetFolderType();

                    //Load Seasons
                    await LoadAsync();
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



        // Remove Folder
        // =====================================================================
        // =====================================================================
        private async void RemoveSeasonFolder(RemovalType type = RemovalType.Remove)
        {
            //Initialize Variables
            bool isDeleted = false, isCancel = false;
            List<Episode> episodes = Episodes.Where(i => i.OwnerId == selectedSeasonFolder.Id).ToList();

            //Show Loading Panel
            ToggleState.Loading(true);

            //Check if Removal Type is Delete and if the Season Directory Exists
            if (type == RemovalType.Delete && Validation.Directory(selectedSeasonFolder.FilePath))
            {
                //Delete the Season Folder from the Computer System
                DiscardElements.DeleteSeasonFolder(out isCancel, out isDeleted, selectedSeasonFolder, episodes);
            }

            //Check if the selected season's directory path was deleted from the computer system or if the removal type is set to remove
            //Else check if the user cancelled the removal
            if (isDeleted == true || type == RemovalType.Remove)
            {
                //Validate Custom Cover Image
                bool isCustomCoverImageUsed = SeasonFolders.Any(i => i.Id != selectedSeasonFolder.Id && i.CustomCoverImage == selectedSeasonFolder.CustomCoverImage);
                bool isCustomCoverImageParent = TVShowFolders.FirstOrDefault(i => i.Id == selectedSeasonFolder.OwnerId).CustomCoverImage == selectedSeasonFolder.CustomCoverImage;

                //Remove Season From Application
                bool isRemoved = DiscardElements.RemoveSeasonFolder(type, selectedSeasonFolder, isCustomCoverImageUsed, isCustomCoverImageParent);

                //Check if the isRemoved Boolean is Set to True
                if (isRemoved == true)
                {
                    //Remove Season Folder From Navigation Bar
                    nbNavigationBar.Remove(selectedSeasonFolder.Id, selectedSeasonFolder.OwnerId, selectedSeasonFolder.Name, selectedSeasonFolder.FolderType);

                    //Set Selected Folder Type
                    selectedFolderType = nbNavigationBar.GetFolderType();

                    //Load Seasons
                    await LoadAsync();
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
            if (type == RemovalType.Delete && Validation.File(selectedEpisode.FilePath))
            {
                //Delete the Item from the Computer System
                DiscardElements.DeleteItem(out isCancel, out isDeleted, MediaType.Episodes, FileType.File, selectedEpisode.FilePath);
            }

            //Check if the selected item's file was deleted from the computer system or if the removal type is set to remove
            //Else check if the user cancelled the removal
            if (isDeleted == true || type == RemovalType.Remove)
            {
                //Remove Item From Application
                bool isRemoved = DiscardElements.RemoveItem(MediaType.Episodes, selectedEpisode.Id, type);

                //Check if the isRemoved Boolean is Set to True
                if (isRemoved == true)
                {
                    //Set Selected Folder Type
                    selectedFolderType = nbNavigationBar.GetFolderType();

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
        private async Task LoadAsync(ElementType elementtype = ElementType.Null, Folder folder = null, FolderType foldertype = FolderType.Folders)
        {
            //Disable Submenu Buttons
            ToggleSubButtons(false);

            //Close Information Pane
            Pane.Toggle(PaneToggle.Close, informationPane, icItems);

            //Unset Selected Element
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedEpisode, ref selectedSeasonFolder, ref selectedTVShowFolder, ref selectedFolder, ref selectedType, ref selectedFolderType);

            //Check Element Type
            if (elementtype == ElementType.Files)
            {
                //Load Items
                await LoadItemsAsync();
            }
            else if (elementtype == ElementType.Folders)
            {
                //Unset Selected Folder Type
                selectedFolderType = FolderType.Null;

                //Load Folders
                await LoadFoldersAsync(foldertype);

                //Get Folder
                if (foldertype == FolderType.Folders && folder == null && Folders.Count > 0) { folder = Folders.OrderBy(i => i.Id).Last(); }
                else if (foldertype == FolderType.TVShowFolders && folder == null && TVShowFolders.Count > 0) { folder = TVShowFolders.OrderBy(i => i.Id).Last(); }
                else if (foldertype == FolderType.SeasonFolders && folder == null && SeasonFolders.Count > 0) { folder = SeasonFolders.OrderBy(i => i.Id).Last(); }

                //Add Folder to Navigation Bar
                nbNavigationBar.Add(folder.Id, folder.OwnerId, folder.Name, folder.FolderType);
            }
            else
            {
                //Load Items
                await LoadItemsAsync();

                //Load Folders
                await LoadFoldersAsync(FolderType.Folders);

                //Load TV Show Folders
                await LoadFoldersAsync(FolderType.TVShowFolders);

                //Load Season Folders
                await LoadFoldersAsync(FolderType.SeasonFolders);
            }

            //Clear Composite
            Composite.Clear();

            //Add Observable Collections to Composite Collection
            if (Folders.Count() > 0 && foldertype != FolderType.SeasonFolders && elementtype != ElementType.Files && (selectedFolderType == FolderType.Null || selectedFolderType == FolderType.Folders)) { Composite.AddRange(Folders.Where(i => i.OwnerId == activeFolder)); }
            if (TVShowFolders.Count() > 0 && foldertype != FolderType.SeasonFolders && elementtype != ElementType.Files && (selectedFolderType == FolderType.Null || selectedFolderType == FolderType.Folders)) { Composite.AddRange(tbtnShowFavourites.IsSelected ? TVShowFolders.Where(i => i.OwnerId == activeFolder && i.isFavourite == 1) : TVShowFolders.Where(i => i.OwnerId == activeFolder)); }
            if (SeasonFolders.Count() > 0 && (selectedFolderType == FolderType.TVShowFolders || foldertype == FolderType.SeasonFolders)) { Composite.AddRange(SeasonFolders.Where(i => i.OwnerId == activeTVShowFolder)); }
            if (Episodes.Count() > 0 && selectedFolderType == FolderType.SeasonFolders) { Composite.AddRange(Episodes.Where(i => i.OwnerId == activeSeasonFolder)); }
        }

        private async Task LoadItemsAsync()
        {
            //Clear Episodes Observable Collection
            Episodes.Clear();

            //Run Task
            await Task.Run(() =>
            {
                //Loop Through Each Episode in the Episodes Database
                foreach (Episode episode in Database.LoadItems(MediaType.Episodes, "EpisodeNumber", "ASC"))
                {
                    //Invoke Dispatcher
                    Dispatcher.Invoke(() =>
                    {
                        //Add Each Episode from the Episodes Database to the Episodes Observable Collection
                        Episodes.Add(episode);
                    });
                }
            });
        }

        private async Task LoadFoldersAsync(FolderType foldertype)
        {
            //Validate Folder Type
            if (foldertype != FolderType.Null)
            {
                //Validate Folder Type and Clear Observable Collection
                if (foldertype == FolderType.Folders) { Folders.Clear(); }
                else if (foldertype == FolderType.TVShowFolders) { TVShowFolders.Clear(); }
                else if (foldertype == FolderType.SeasonFolders) { SeasonFolders.Clear(); }

                //Get Sort Order and Type
                string order = Sort.GetOrder(cboxSortBy);
                string type = Sort.GetType(cboxSortBy);

                //Run Task
                await Task.Run(() =>
                {
                    //Load Folders from Database
                    List<object> folders = Database.LoadFolders(MediaType.TVShows, foldertype, order, type);

                    //Loop through Folders in folders List
                    for (int i = 0; i < folders.Count; i++)
                    {
                        //Invoke Dispatcher
                        Dispatcher.Invoke(() =>
                            {
                                //Validate Folder Type and Add Current Looped Folder to Observable Collection
                                if (foldertype == FolderType.Folders) { Folders.Add((Folder)folders[i]); }
                                else if (foldertype == FolderType.TVShowFolders) { TVShowFolders.Add((TVShowFolder)folders[i]); }
                                else if (foldertype == FolderType.SeasonFolders) { SeasonFolders.Add((SeasonFolder)folders[i]); }
                            });
                    }
                });
            }
        }



        // Input Validation
        // =====================================================================
        // =====================================================================
        private void InputValidation(string owner)
        {
            //Validate Owner
            if (owner == "episodespanel")
            {
                //Validate Process Type and Selected Element
                if (selectedEpisode != null && isEdit)
                {
                    //Validate Panel Contents
                    Debug.WriteLine("Edit Episodes Panel");
                }
                else if (!isEdit)
                {
                    //Set Selected Season Folder
                    selectedSeasonFolder = SeasonFolders.Single(i => i.Id == nbNavigationBar.selectedId);

                    //Validate Panel Contents
                    Validation.ValidateEpisodesPanel(OptionPanelType.Add, btnProcessEpisodes, selectedSeasonFolder.FilePath, nbNavigationBar.selectedId, Episodes.ToList(), null, true, odEpisodes.Contents.Count, odEpisodes.Contents.ToList());
                }
            }
            else if (owner == "seasonpanel")
            {
                //Validate Process Type and Selected Element
                if (selectedSeasonFolder != null && isEdit)
                {
                    //Validate Panel Contents
                    Validation.ValidateSeasonPanel(OptionPanelType.Edit, btnProcessSeason, -1, null, new string[] { selectedSeasonFolder.CustomCoverImage }, new string[] { $"{odSeasonCoverImage.Content}" });
                }
                else if (!isEdit)
                {
                    //Validate Panel Contents
                    Validation.ValidateSeasonPanel(OptionPanelType.Add, btnProcessSeason, nbNavigationBar.selectedId, SeasonFolders.ToList(), new string[] { $"{nbSeason.Value}", $"{odSeasonDirectory.Content}", $"{odSeasonCoverImage.Content}" });
                }
            }
            else if (owner == "tvshowpanel")
            {
                //Validate Process Type and Selected Element
                if (selectedTVShowFolder != null && isEdit)
                {
                    //Validate Panel Contents
                    Validation.ValidateTVShowPanel(OptionPanelType.Edit, btnProcessTVShow, Folders.ToList(), new string[] { selectedTVShowFolder.CustomName, selectedTVShowFolder.CoverImage, selectedTVShowFolder.IMDBLink, selectedTVShowFolder.MetaCriticLink }, new KeyValuePair<int, string>(fbdTVShowSaveLocation.Id, fbdTVShowSaveLocation.FolderPath), new string[] { $"{tbTVShowCustomName.Content}", $"{odTVShowCoverImage.Content}", $"{sbTVShow.IMDBLink}", $"{sbTVShow.DefaultLink}" }, new KeyValuePair<int, string>(selectedTVShowFolder.OwnerId, nbNavigationBar.GetFolderPath()));
                }
                else if (!isEdit)
                {
                    //Validate Panel Contents
                    Validation.ValidateTVShowPanel(OptionPanelType.Add, btnProcessTVShow, Folders.ToList(), new string[] { $"{odTVShowCoverImage.Content}", $"{sbTVShow.IMDBLink}", $"{sbTVShow.DefaultLink}" }, new KeyValuePair<int, string>(fbdTVShowSaveLocation.Id, fbdTVShowSaveLocation.FolderPath));
                }
            }
            else if (owner == "folderpanel")
            {
                //Validate Process Type and Selected Element
                if (selectedFolder != null && isEdit)
                {
                    //Validate Panel Contents
                    Validation.ValidateFolderPanel(OptionPanelType.Edit, btnProcessFolder, Folders.ToList(), Tuple.Create($"{tbFolderName.Content}", fbdFolderSaveLocation.Id, fbdFolderSaveLocation.FolderPath), Tuple.Create(selectedFolder.Name, selectedFolder.OwnerId, nbNavigationBar.GetFolderPath()));
                }
                else if (!isEdit)
                {
                    //Validate Panel Contents
                    Validation.ValidateFolderPanel(OptionPanelType.Add, btnProcessFolder, Folders.ToList(), Tuple.Create($"{tbFolderName.Content}", fbdFolderSaveLocation.Id, fbdFolderSaveLocation.FolderPath));
                }
            }
        }
        #endregion Methods
    }
}