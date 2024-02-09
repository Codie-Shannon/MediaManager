using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using WpfToolkit.Controls;
using System.Windows.Input;
using Media_Manager.Models;
using MediaControlsLibrary;
using System.Threading.Tasks;
using Media_Manager.Extensions;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediaControlsLibrary.Dependencies;
using System.Windows.Controls;
using MediaControlsLibrary.Models;
using MediaControlsLibrary.Types;

namespace Media_Manager.Views
{
    public partial class GamesView : UserControl
    {
        #region Variables
        // Observable Collections
        // =====================================================================
        // =====================================================================
        public ObservableCollection<object> Composite { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<Folder> Folders { get; set; } = new ObservableCollection<Folder>();
        public ObservableCollection<Game> Games { get; set; } = new ObservableCollection<Game>();


        // Selection
        // =====================================================================
        // =====================================================================
        private bool isSelectionChanged = false;
        private ElementType selectedType;


        // Selected Elements
        // =====================================================================
        // =====================================================================
        private Folder selectedFolder;
        private Game selectedGame;
        private int selectedid;
        public int selectedId { get => this.selectedid; set => this.selectedid = value; }


        // Messages
        // =====================================================================
        // =====================================================================
        private readonly string str_AddFolderError = "An error occured while adding the folder. Would you like to edit the folder's details and try again?";
        private readonly string str_Exists = "The game has already been added to the database. Are you sure you want continue with this procedure?";
        private readonly string str_ValidationError = "The Launcher Path, Base Directory or IGDB Link Entered were Invalid.";
        private readonly string str_InternetError = "An internet connection is required to add a game to the application.";
        private readonly string str_LoadError = "The game's executable could not be found.";
        private readonly string str_EditError = "The selected item cannot be loaded for editing.";
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
        public GamesView() { InitializeComponent(); }



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

            //Load Elements
            await LoadAsync();

            //Initialize Folders
            Elements.InitializeFolders(nbNavigationBar, Folders.ToList(), null);

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
            CoverImages.Clean(Properties.Settings.Default.Games);
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
                if (sender is CoverItem i)
                {
                    //Set Selected Item to Sender
                    await ItemSelectedAsync(id);
                }
                else if (sender is CoverFolder f)
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
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedGame, ref selectedFolder, ref selectedType);

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
            UpdateFavourite(selectedGame.isFavourite == 1 ? 0 : 1);
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
                OpenPanel("Add", "Game", btnProcessItem, ItemPanel);
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
            if (element == "item" && isEdit)
            {
                //Edit Item
                EditItem();
            }
            else if (element == "item")
            {
                //Add Item
                AddItem();
            }
            else if (element == "folder" && isEdit)
            {
                //Edit Folder
                EditFolder();
            }
            else if (element == "folder")
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
            await SearchEngine.Abort(sbGame);

            //Perform Game Search
            SearchEngine.GameSearch(sbGame, ((optSearchBox)sender).Search);
        }

        private void OpenDialog_TextChanged(object sender, RoutedEventArgs e)
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
            if (selectedType == ElementType.Files && removal == "remove")
            {
                //Remove the Item from the Application
                RemoveItem();
            }
            else if (selectedType == ElementType.Files && removal == "delete")
            {
                //Delete the Item from the Computer System
                RemoveItem(RemovalType.Delete);
            }
            else if (selectedType == ElementType.Folders)
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
            //Show Folder in File Explorer
            Elements.ShowInExplorer(FileType.Folder, selectedGame.BaseDirectory);
        }



        // Sort By
        // =====================================================================
        // =====================================================================
        private async void cboxSortBy_SelectionUpdate(object sender, SelectionChangedEventArgs e)
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Load Items
            await LoadItemsAsync();

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
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedGame, ref selectedFolder, ref selectedType);

            //Set Selected Element
            Elements.SetElement(ref icItems, id, out selectedid, Games.SingleOrDefault(i => i.Id == id), out selectedGame, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType);

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
            if (selectedType == ElementType.Files)
            {
                //Get isFavourite Value
                int isfavourite = selectedGame.isFavourite;

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
                vwpItems.BringIndexIntoViewPublic(selectedType == ElementType.Files ? Composite.IndexOf(selectedGame) : Composite.IndexOf(selectedFolder));
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
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedGame, ref selectedFolder, ref selectedType);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Games.SingleOrDefault(i => i.Id == id), out selectedGame, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType);

                //Hide Information Pane Data
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
            if (selectedType == ElementType.Files && selectedId != 0 && selectedGame != null && selectedId == id)
            {
                //Launch Game
                LaunchGame();
            }
            else
            {
                //Unset Selected Element
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedGame, ref selectedFolder, ref selectedType);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Games.SingleOrDefault(i => i.Id == id), out selectedGame, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Files, out selectedType);

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
            Title.Content = Formatter.FormatName(selectedGame.Name, selectedGame.CustomName);
            Cover.Source = Formatter.FormatImage(MediaType.Games, selectedGame.CoverImage);

            //Display Standard Details
            tbFormat.SetValue(selectedGame.Format);
            tbSize.SetValue(Formatter.FormatFileSize(selectedGame.FileSize));
            tbCreated.SetValue(Formatter.FormatCreation(selectedGame.CreationTime, selectedGame.CreationDate));

            //Display IGDB Details
            tbPublisher.SetValue(selectedGame.Publisher);
            tbReleaseDate.SetValue(Formatter.FormatGameReleaseDate(selectedGame.ReleaseDate));
            UserRating.SetRating(selectedGame.UserScore, selectedGame.UserReviewCount);
            CriticRating.SetRating(selectedGame.CriticScore, selectedGame.CriticReviewCount);
            tbGenre.SetList(selectedGame.Genres);
            tbAvailablePlatform.SetList(selectedGame.AvailablePlatforms);
        }

        private async void UpdateFavourite(int value)
        {
            //Set selectedGame isFavourite Value
            selectedGame.isFavourite = value;

            //Set Games Observable Collection's selectedGame isFavourite Value
            Games.ElementAt(Games.IndexOf(selectedGame)).isFavourite = value;

            //Update selectedGame's Database isFavourite Value
            Database.UpdateFavourite(selectedGame.Id, value, MediaType.Games);

            //Check if Favourites are being Shown at the moment and the selectedGame is being Unfavourited
            if (value == 0 && tbtnShowFavourites.IsSelected)
            {
                //Load Items
                await LoadAsync(ElementType.Files);
            }
        }

        private void LaunchGame()
        {
            //Validate Launcher Path
            if (Validation.File(selectedGame.FilePath))
            {
                //Get Name
                string name = Formatter.FormatName(selectedGame.Name, selectedGame.CustomName);

                //Show Confirmation MessageBox
                MessageBoxResult result = CustomMessageBox.ShowYesNo($"Are you sure you would like to launch {name}?", "QUESTION", "Yes", "No", MessageBoxImage.Question);

                //Check MessageBox Result
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        //Create and Configure New Process Object
                        Process game = new Process() { StartInfo = new ProcessStartInfo() { FileName = selectedGame.FilePath, WorkingDirectory = Path.GetDirectoryName(selectedGame.FilePath) } };

                        //Start Game Process
                        game.Start();
                    }
                    catch (Exception e)
                    {
                        //Check if the User did not Cancel the Operation
                        if (e.Message.ToLower() != "the operation was canceled by the user")
                        {
                            //Show Error Message
                            CustomMessageBox.ShowOK($"An Error Occured\n\n{e.Message}", "ERROR", "OK", MessageBoxImage.Error);

                            //Close Application
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
            else
            {
                //Show Error Message
                CustomMessageBox.ShowOK(str_LoadError, "ERROR", "OK", MessageBoxImage.Error);
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
                OptionsPanel.ClearPanel(null, new List<optTextBoxLong> { tbCustomName }, new List<optSearchBox> { sbGame }, new List<optOpenDialog> { odLauncherPath, odBaseDirectory });
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
            if (name == "Game")
            {
                //Set Panel Contents
                ItemPanelTitle.Content = $"{process} {name}";
                btnProcessItem.Content = process;
            }
            else if (name == "Folder")
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
            else if (selectedType == ElementType.Folders)
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
            Folder folder = new Folder() { OwnerId = fbdFolderSaveLocation.Id, Name = $"{tbFolderName.Content}", Type = nameof(MediaType.Games), FolderType = nameof(FolderType.Folders) };

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
            Game game = new Game() { CustomName = $"{tbCustomName.Content}", BaseDirectory = $"{odBaseDirectory.Content}", FilePath = $"{odLauncherPath.Content}", IGDBLink = $"{sbGame.DefaultLink}", Type = nameof(MediaType.Games) };

            //Disable Process Item Button
            ToggleState.UIElement(btnProcessItem, false);

            //Hide Item Panel
            ToggleState.Panel(ItemPanel, false);

            //Get Boolean Value Representing if the Item Already Exists within the Database (Games ObservableCollection)
            bool isEmpty = !Games.Any(i => game.BaseDirectory == i.BaseDirectory && game.FilePath == i.FilePath);

            //Validate File Paths and URL
            bool isValid = Validation.Process(game.FilePath, true, true, new string[] { "https://www.igdb.com/games/" }, new string[] { game.IGDBLink }, new int[] { 29 }, 1, true, game.BaseDirectory);


            //Check if the item has already been added to the database, and if the filepaths and url are valid
            //Else check if the item has not been added to the database and if the filepaths and url are valid
            if (!isEmpty && isValid)
            {
                //Show Game Exists Message
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
                ProcessAddItem(game);
            }
        }

        private async void ProcessAddItem(Game game)
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Stop Search Engine on Different Thread
            _ = Task.Run(() => { SearchEngine.Stop(); });

            //Process Add Item on Different Thread
            await Task.Run(async () =>
            {
                //Configure Fetcher
                bool isValid = await Fetcher.ConfigureFetcherAsync(MediaType.Games, game.FilePath, game.BaseDirectory, game.IGDBLink);

                //Check if Configuration was Successful
                if (isValid)
                {
                    //Get Item Details
                    game = Fetcher.GetGame(Games.Count() + 1, game);

                    //Close Fetcher
                    Fetcher.Close();

                    //Save Item
                    Database.SaveItem(MediaType.Games, game);

                    //Load Items on UI Thread
                    Application.Current.Dispatcher.Invoke(new Action(async () => { await LoadAsync(ElementType.Files); }));
                }
                else
                {
                    //Show Error Message on UI Thread
                    Application.Current.Dispatcher.Invoke(new Action(async () => { CustomMessageBox.ShowOK(str_InternetError, "ERROR", "OK", MessageBoxImage.Error); }));
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
            if (Games.Contains(selectedGame) && Validation.File(selectedGame.FilePath) && Validation.Directory(selectedGame.BaseDirectory))
            {
                //Clear Panel
                ClearPanel("itempanel");

                //Set Panel
                SetPanel("Edit", "Game");

                //Disable Process Item Button
                ToggleState.UIElement(btnProcessItem, false);

                //Show Item Panel
                ToggleState.Panel(ItemPanel, true);

                //Delay Load Editable Values by 10 Milliseconds (Required for Cover Image)
                await Task.Delay(10);

                //Load Editable Values
                OptionsPanel.LoadEditableValues(tbCustomName, selectedGame.CustomName, fbdItemSaveLocation, selectedGame.OwnerId, nbNavigationBar.GetFolderPath(), selectedType, selectedid, new List<optOpenDialog> { odLauncherPath, odBaseDirectory }, new List<string> { selectedGame.FilePath, selectedGame.BaseDirectory }, MediaType.Games, new List<optSearchBox> { sbGame }, selectedGame);
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
            Game game = new Game() { CustomName = $"{tbCustomName.Content}", IGDBLink = $"{sbGame.DefaultLink}", OwnerId = fbdItemSaveLocation.Id, BaseDirectory = $"{odBaseDirectory.Content}", FilePath = $"{odLauncherPath.Content}" };

            //Stop Search Engine on Different Thread
            _ = Task.Run(() => { SearchEngine.Stop(); });

            //Hide Item Panel
            ToggleState.Panel(ItemPanel, false);

            //Validate Launcher Path, Base Directory, and IGDB Link
            if (Validation.ValidateSaveLocation(OptionPanelType.Null, Folders.ToList(), new KeyValuePair<int, string>(game.OwnerId, fbdItemSaveLocation.FolderPath)) && Validation.Process(game.FilePath, true, true, new string[] { "https://www.igdb.com/games/" }, new string[] { game.IGDBLink }, new int[] { 29 }, 1, true, game.BaseDirectory))
            {
                //Show Loading Panel
                ToggleState.Loading(true);

                //Check if the Base Directory, Launcher Path or IGDB Link have been changed
                if (game.BaseDirectory != selectedGame.BaseDirectory || game.FilePath != selectedGame.FilePath || game.IGDBLink != selectedGame.IGDBLink)
                {
                    //Configure Fetcher
                    bool isValid = await Fetcher.ConfigureFetcherAsync(MediaType.Games, game.FilePath, game.BaseDirectory, game.IGDBLink);

                    //Check if the Configuration was Successful
                    if (isValid == true)
                    {
                        //Get Game Details
                        game = Fetcher.GetGame(selectedGame.Id, game);
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
                    //Assign selectedGame into game object
                    game = selectedGame;

                    //Update Game Details
                    game.CustomName = tbCustomName.Content.ToString();
                    game.OwnerId = fbdItemSaveLocation.Id;
                }

                //Update Item
                Database.UpdateItem(game.Id, game, MediaType.Games);

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
            bool isRemoved = DiscardElements.RemoveFolder(MediaType.Games, selectedId);

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

            //Check if Removal Type is Delete and if the Launcher Path and Base Directory Exist
            if (type == RemovalType.Delete && Validation.File(selectedGame.FilePath) && Validation.Directory(selectedGame.BaseDirectory))
            {
                //Delete the Game from the Computer System
                DiscardElements.DeleteItem(out isCancel, out isDeleted, MediaType.Games, FileType.Folder, selectedGame.BaseDirectory);
            }

            //Check if the selected item was deleted from the computer system or if the removal type is set to remove
            //Else check if the user cancelled the removal
            if (isDeleted == true || type == RemovalType.Remove)
            {
                //Remove Item From Application
                bool isRemoved = DiscardElements.RemoveItem(MediaType.Games, selectedGame.Id, type, selectedGame.CoverImage);

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



        // Load Items
        // =====================================================================
        // =====================================================================
        private async Task LoadAsync(ElementType elementtype = ElementType.Null, Folder folder = null)
        {
            //Disable Submenu Buttons
            ToggleSubButtons(false);

            //Close Information Pane
            Pane.Toggle(PaneToggle.Close, informationPane, icItems);

            //Unset Selected Element
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedGame, ref selectedFolder, ref selectedType);

            //Check Element Type
            if (elementtype == ElementType.Files)
            {
                //Load Items
                await LoadItemsAsync();
            }
            else if (elementtype == ElementType.Folders)
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
            if (Games.Count() > 0) { Composite.AddRange(tbtnShowFavourites.IsSelected ? Games.Where(i => i.OwnerId == activeFolder && i.isFavourite == 1) : Games.Where(i => i.OwnerId == activeFolder)); }
        }

        private async Task LoadItemsAsync()
        {
            //Clear Games Observable Collection
            Games.Clear();

            //Get Sort Order and Type
            string order = Sort.GetOrder(cboxSortBy);
            string type = Sort.GetType(cboxSortBy);

            //Run Task
            await Task.Run(() =>
            {
                //Loop Through Each Game in the Games Database
                foreach (Game game in Database.LoadItems(MediaType.Games, order, type))
                {
                    //Invoke Dispatcher
                    Dispatcher.Invoke(() =>
                    {
                        //Add Each Game from the Games Database to the Games Observable Collection
                        Games.Add(game);
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
                foreach (Folder folder in Database.LoadFolders(MediaType.Games))
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
            //Check Panel Type
            if (owner == "itempanel" && isEdit && selectedGame != null)
            {
                //Validate Panel Contents
                Validation.ValidateGamePanel(OptionPanelType.Edit, btnProcessItem, Folders.ToList(), new string[] { selectedGame.CustomName, selectedGame.FilePath, selectedGame.BaseDirectory, selectedGame.IGDBLink }, new KeyValuePair<int, string>(fbdItemSaveLocation.Id, fbdItemSaveLocation.FolderPath), new string[] { $"{tbCustomName.Content}", $"{odLauncherPath.Content}", $"{odBaseDirectory.Content}", $"{sbGame.DefaultLink}" }, new KeyValuePair<int, string>(selectedGame.OwnerId, nbNavigationBar.GetFolderPath()));
            }
            else if (owner == "itempanel" && !isEdit)
            {
                //Validate Panel Contents
                Validation.ValidateGamePanel(OptionPanelType.Add, btnProcessItem, Folders.ToList(), new string[] { $"{odLauncherPath.Content}", $"{sbGame.DefaultLink}", $"{odBaseDirectory.Content}" }, new KeyValuePair<int, string>(fbdItemSaveLocation.Id, fbdItemSaveLocation.FolderPath));
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