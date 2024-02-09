using System;
using System.Linq;
using System.Windows;
using WpfToolkit.Controls;
using MediaControlsLibrary;
using System.Windows.Input;
using Media_Manager.Models;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using Media_Manager.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediaControlsLibrary.Dependencies;
using MediaControlsLibrary.Models;
using System.IO;
using Media_Manager.ViewModels;
using MediaControlsLibrary.Types;

namespace Media_Manager.Views
{
    public partial class MusicView : UserControl
    {
        #region Variables
        // Observable Collections
        // =====================================================================
        // =====================================================================
        public ObservableCollection<object> Composite { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<Folder> Folders { get; set; } = new ObservableCollection<Folder>();
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();


        // Selection
        // =====================================================================
        // =====================================================================
        private bool isSelectionChanged = false;
        private ElementType selectedType;


        // Selected Elements
        // =====================================================================
        // =====================================================================
        private Folder selectedFolder;
        private Song selectedSong;
        private int selectedid;
        public int selectedId { get => this.selectedid; set => this.selectedid = value; }
        

        // Messages
        // =====================================================================
        // =====================================================================
        private readonly string str_AddFolderError = "An error occured while adding the folder. Would you like to edit the folder's details and try again?";
        private readonly string str_LoadError = "The file could not be found.";
        private readonly string str_EditError = "The selected item cannot be loaded for editing.";
        private readonly string str_InvalidFilePath = "The file path entered was invalid.";
        private readonly string str_DeletionError = "The selected item could not be deleted.";


        // Other
        // =====================================================================
        // =====================================================================
        private DispatcherTimer dispatcher = null;
        private int activeFolder = 0;
        private bool isEdit = false;
        #endregion Variables




        // Constructors
        // =====================================================================
        // =====================================================================
        public MusicView() { InitializeComponent(); }




        // Setup
        // =====================================================================
        // =====================================================================
        private async void Setup_Tick(object sender, EventArgs e)
        {
            //Stop dispatcher
            dispatcher.Stop();

            //Setup View Model
            MusicViewModel.Setup(ref icItems, ref tbDuration, ref tbFormat, ref tbFileSize, ref tbCreated, ref tbSampleRate, ref tbAudioChannels, ref tbCompMode);

            //Disable Submenu Buttons
            ToggleSubButtons(false);

            //Set Data Context
            this.DataContext = this;

            //Load Elements
            await LoadAsync();

            //Initialize Music Player View
            ccMusicPlayer.Content = new MusicPlayerView();

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
            CoverImages.Clean(Properties.Settings.Default.Music);
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
                if (sender is ImageItem i)
                {
                    //Set Selected Item to Sender
                    await ItemSelectedAsync(id);
                }
                else if (sender is ImageFolder f)
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
                //Clear Music Player
                MusicPlayerView.Model.Clear();

                //Hide Information Pane Data if the Parent is Clicked (The Background)
                ToggleState.Panel(informationPaneData, false, false);

                //Close Information Pane if the Parent is Clicked (The Background)
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Unset Selected Elements
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedSong, ref selectedFolder, ref selectedType);

                //Disable Submenu Buttons
                ToggleSubButtons(false);
            }
        }

        private async void cmiOpen_Click(object sender, RoutedEventArgs e)
        {
            //Get Element Type
            string type = Label.Tag(sender as ContextMenuItem);

            //Check Element Type
            if (type == "folder")
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
            //Reload Files
            await LoadAsync(ElementType.Files);

            //Reselect Song
            await ItemSelectedAsync(selectedid, true);
        }

        private void cmiFavourite_Click(object sender, RoutedEventArgs e)
        {
            //Update Favourite
            UpdateFavourite(selectedSong.isFavourite == 1 ? 0 : 1);
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
                //Load Item Add
                LoadItemAdd();
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
                //Add Items
                AddItems();
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

        private void odPath_Click(object sender, RoutedEventArgs e)
        {
            //Get Options Panel (Owner)
            optPanel owner = (optPanel)OptionsPanel.GetAncestor((FrameworkElement)sender, typeof(optPanel));

            //Perform Input Validation
            InputValidation(Label.Name(owner));
        }

        private void odCoverImage_ClearClick(object sender, RoutedEventArgs e)
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
            //Show File in File Explorer
            Elements.ShowInExplorer(FileType.File, selectedSong.FilePath);
        }



        // Sort By
        // =====================================================================
        // =====================================================================
        private async void cboxSortBy_SelectionUpdate(object sender, SelectionChangedEventArgs e)
        {
            //Sort Files By Selected Data Type
            SortBy();
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
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedSong, ref selectedFolder, ref selectedType);

            //Set Selected Element
            Elements.SetElement(ref icItems, id, out selectedid, Songs.SingleOrDefault(i => i.Id == id), out selectedSong, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType, Songs.ToList(), Composite.ToList());

            //Clear Music Player
            MusicPlayerView.Model.Clear();

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
            if (sender is ImageItem && ((selectedid.ToString() != id.ToString()) || (selectedid.ToString() == id.ToString() && selectedType == ElementType.Folders)))
            {
                //Set Selected Item to Sender
                await ItemSelectedAsync(id);
            }
            else if (sender is ImageFolder && ((selectedid.ToString() != id.ToString()) || (selectedid.ToString() == id.ToString() && selectedType == ElementType.Files)))
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
                int isfavourite = selectedSong.isFavourite;

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
                vwpItems.BringIndexIntoViewPublic(selectedType == ElementType.Files ? Composite.IndexOf(selectedSong) : Composite.IndexOf(selectedFolder));
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
                Elements.UnsetElements(ref icItems, ref selectedid, ref selectedSong, ref selectedFolder, ref selectedType);

                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Songs.SingleOrDefault(i => i.Id == id), out selectedSong, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Folders, out selectedType, Songs.ToList(), Composite.ToList());

                //Clear Music Player
                MusicPlayerView.Model.Clear();

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
        private async Task ItemSelectedAsync(int id, bool isitemsreload = false)
        {
            //Get Old Song ID
            int oldid = selectedSong != null ? selectedSong.Id : -1;

            //Get Active Songs from Songs ObservableCollection
            List<Song> songs = tbtnShowFavourites.IsSelected ? Songs.Where(i => i.OwnerId == activeFolder && i.isFavourite == 1).ToList() : Songs.Where(i => i.OwnerId == activeFolder).ToList();

            //Unset Selected Element
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedSong, ref selectedFolder, ref selectedType);

            //Validate Song ID
            if (songs.Any(i => i.Id == id))
            {
                //Set Selected Element
                Elements.SetElement(ref icItems, id, out selectedid, Songs.SingleOrDefault(i => i.Id == id), out selectedSong, Folders.SingleOrDefault(i => i.Id == id), out selectedFolder, ElementType.Files, out selectedType, songs, Composite.ToList());

                //Enable Submenu Buttons
                ToggleSubButtons(true);

                //Show Information Pane
                Pane.Toggle(PaneToggle.Open, informationPane, icItems);

                //Wait for the Information Pane to Open
                await Task.Delay(250);

                //Display Item Information
                DisplayItemInfo();

                //Bring Element Into View
                BringInToView();

                //Show Information Pane Data
                ToggleState.Panel(informationPaneData, true, false);

                //Check if File is Valid
                if (Validation.File(selectedSong.FilePath))
                {
                    //Set Music Player Model's songs List
                    MusicPlayerView.Model.UpdateList(songs);

                    //Set Music Player Model's Selected Song
                    MusicPlayerView.Model.SetSelectedItem(selectedSong, oldid, isitemsreload);
                }
                else
                {
                    //Show Error Message
                    CustomMessageBox.ShowOK(str_LoadError, "ERROR", "OK", MessageBoxImage.Error);
                }
            }
            else
            {
                //Clear Music Player
                MusicPlayerView.Model.Clear();
            }
        }

        private void DisplayItemInfo()
        {
            //Display Main
            MusicPlayerView.Model.SetTitle(Formatter.FormatName(selectedSong.Name, selectedSong.CustomName));
            MusicPlayerView.Model.SetCover(Formatter.FormatImage(MediaType.Music, selectedSong.CoverImage));

            //Display Standard Details
            tbDuration.Content = Formatter.FormatDuration(selectedSong.Duration);
            tbFormat.Content = selectedSong.Format;
            tbFileSize.Content = Formatter.FormatFileSize(selectedSong.FileSize);
            tbCreated.Content = Formatter.FormatCreation(selectedSong.CreationTime, selectedSong.CreationDate);

            //Display Advance Details
            tbSampleRate.Content = Formatter.FormatSampleRate(selectedSong.SampleRate);
            tbAudioChannels.Content = selectedSong.AudioChannels;
            tbCompMode.Content = selectedSong.CompMode;
        }

        private async void UpdateFavourite(int value)
        {
            //Set selectedSong isFavourite Value
            selectedSong.isFavourite = value;

            //Set Songs Observable Collection's selectedSong isFavourite Value
            Songs.ElementAt(Songs.IndexOf(selectedSong)).isFavourite = value;

            //Update selectedSong's Database isFavourite Value
            Database.UpdateFavourite(selectedSong.Id, value, MediaType.Music);

            //Check if Favourites are being Shown at the moment and the selectedSong is being Unfavourited
            if (value == 0 && tbtnShowFavourites.IsSelected)
            {
                //Load Items
                await LoadAsync(ElementType.Files);
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
                OptionsPanel.ClearPanel(null, new List<optTextBoxLong> { tbCustomName }, null, new List<optOpenDialog> { odSongPath });
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
            if (name == "Song")
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
            Folder folder = new Folder() { OwnerId = fbdFolderSaveLocation.Id, Name = $"{tbFolderName.Content}", Type = nameof(MediaType.Music), FolderType = nameof(FolderType.Folders) };

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
        private void LoadItemAdd()
        {
            //Set Panel
            OpenPanel("Add", "Songs", btnProcessItem, ItemPanel);

            //Show Open Dialog Song Path
            ToggleState.UIElement(odSongPath, Visibility.Visible);

            //Hide UI Elements
            ToggleState.UIElements(new UIElement[] { tbCustomName, odCoverImage }, Visibility.Collapsed);
        }

        private void AddItems()
        {
            //Variables
            List<Song> baseitems = new List<Song>(), invaliditems = new List<Song>(), existingitems = new List<Song>();

            //Loop through odSongPath Selected Items
            foreach (OpenDialogItem item in odSongPath.Contents)
            {
                //Get Current Looped Items Base Data
                baseitems.Add(new Song() { OwnerId = fbdItemSaveLocation.Id, FilePath = item.FilePath });
            }

            //Disable Process Item Button
            ToggleState.UIElement(btnProcessItem, false);

            //Hide Item Panel
            ToggleState.Panel(ItemPanel, false);

            //Loop through baseitems List
            foreach (Song item in baseitems)
            {
                //Check if the Current Looped Item's File Path is Invalid
                //Else Check if the Current Looped Item has Already Been Added to the Database
                if (!Validation.Process(item.FilePath))
                {
                    //Add Current Looped Item to invaliditems List
                    invaliditems.Add(item);

                    //Remove Current Looped Item from baseitems List
                    baseitems.Remove(item);
                }
                else if(Songs.Any(i => item.FilePath == i.FilePath))
                {
                    //Add Current Looped Item to existingitems List
                    existingitems.Add(item);
                }
            }

            //Check if the baseitems List still Contains Items
            if(baseitems.Count > 0)
            {
                //Loop through invaliditems List
                foreach (Song item in invaliditems)
                {
                    //Show File Path Error Message
                    MessageBoxResult result = CustomMessageBox.ShowDialog($"The file path does not exist for {Path.GetFileName(item.FilePath)}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //Loop through existingitems List
                foreach (Song item in existingitems)
                {
                    //Show Exists Message
                    MessageBoxResult result = CustomMessageBox.ShowYesNo($"{Path.GetFileName(item.FilePath)} has already been added to the database. Are you sure you want to add this song to the application?", "WARNING", "Yes", "No", MessageBoxImage.Warning);

                    //Validate Result
                    if(result == MessageBoxResult.No)
                    {
                        //Remove Item from baseitems List
                        baseitems.Remove(item);
                    }
                }

                //Check if the baseitems List still Contains Items
                if (baseitems.Count > 0)
                {
                    //Run Process Add Items Method
                    ProcessAddItemsAsync(baseitems);
                }
            }
        }

        private async void ProcessAddItemsAsync(List<Song> items)
        {
            //Variables
            int counter = Songs.Count + 1;

            //Show Loading Panel
            ToggleState.Loading(true);

            //Run Multithreaded Task
            await Task.Run(() =>
            {
                //Loop through Songs Items
                foreach (Song item in items)
                {
                    //Configure Fetcher
                    Fetcher.ConfigureFetcherAsync(MediaType.Null, item.FilePath);

                    //Get Item Details
                    Song song = Fetcher.GetSong(counter, item);

                    //Save Item
                    Database.SaveItem(MediaType.Music, song);

                    //Increment Counter
                    counter++;
                }
            });

            //Validate Selected Song
            if (selectedSong != null)
            {
                //Get Selected Song ID
                int id = selectedSong.Id;

                //Load Items
                await LoadAsync(ElementType.Files);

                //Reselect Selected Song
                await ItemSelectedAsync(id, true);
            }
            else
            {
                //Load Items
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



        // Edit Item
        // =====================================================================
        // =====================================================================
        private void LoadItemEdit()
        {
            //Validate Selected Item
            if (Songs.Contains(selectedSong) && Validation.File(selectedSong.FilePath))
            {
                //Set Panel
                SetPanel("Edit", "Song");

                //Hide Open Dialog Song Path
                ToggleState.UIElement(odSongPath, Visibility.Collapsed);

                //Show UI Elements
                ToggleState.UIElements(new UIElement[] { tbCustomName, odCoverImage }, Visibility.Visible);

                //Load Editable Values
                OptionsPanel.LoadEditableValues(tbCustomName, selectedSong.CustomName, fbdItemSaveLocation, selectedSong.OwnerId, nbNavigationBar.GetFolderPath(), selectedType, selectedid, new List<optOpenDialog>() { odCoverImage }, new List<string>() { selectedSong.CoverImage });

                //Disable Process Item Button
                ToggleState.UIElement(btnProcessItem, false);

                //Show Item Panel
                ToggleState.Panel(ItemPanel, true);
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
            Song song = new Song() { FilePath = $"{selectedSong.FilePath}", CustomName = $"{tbCustomName.Content}", OwnerId = fbdItemSaveLocation.Id, CoverImage = $"{odCoverImage.Content}" };

            //Hide Item Panel
            ToggleState.Panel(ItemPanel, false);

            //Validate File Path
            if (Validation.ValidateSaveLocation(OptionPanelType.Null, Folders.ToList(), new KeyValuePair<int, string>(song.OwnerId, fbdItemSaveLocation.FolderPath)) && Validation.Process(song.FilePath))
            {
                //Show Loading Panel
                ToggleState.Loading(true);

                //Check if the Owner ID has Changed
                if (song.OwnerId != selectedSong.OwnerId)
                {
                    //Clear Music Player
                    MusicPlayerView.Model.Clear();
                }

                //Update Song
                song = Fetcher.UpdateSong(selectedSong, song);

                //Update Item
                Database.UpdateItem(selectedSong.Id, song, MediaType.Music);

                //Close Information Pane
                Pane.Toggle(PaneToggle.Close, informationPane, icItems);

                //Load Items
                await LoadAsync(ElementType.Files);

                //Hide Loading Panel
                ToggleState.Loading(false);

                //Set Selected Item to Edited Song
                await ItemSelectedAsync(song.Id, true);
            }
            else
            {
                //Show Invalid File Path Message
                MessageBoxResult result = CustomMessageBox.ShowYesNo(str_InvalidFilePath, "WARNING", "Edit", "Cancel", MessageBoxImage.Error);

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
            bool isRemoved = DiscardElements.RemoveFolder(MediaType.Music, selectedId);

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
            if (type == RemovalType.Delete && Validation.File(selectedSong.FilePath))
            {
                //Delete the Item from the Computer System
                DiscardElements.DeleteItem(out isCancel, out isDeleted, MediaType.Music, FileType.File, selectedSong.FilePath);
            }

            //Check if the selected item's file was deleted from the computer system or if the removal type is set to remove
            //Else check if the user cancelled the removal
            if (isDeleted == true || type == RemovalType.Remove)
            {
                //Remove Item From Application
                bool isRemoved = DiscardElements.RemoveItem(MediaType.Music, selectedSong.Id, type, selectedSong.CoverImage);

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



        // Sort By
        // =====================================================================
        // =====================================================================
        private async void SortBy()
        {
            //Show Loading Panel
            ToggleState.Loading(true);

            //Get Selection Details
            ElementType selectedtype = selectedType;
            int selectedid = selectedId;

            //Load Items
            await LoadAsync(ElementType.Files);

            //Reselect Element
            if (selectedtype == ElementType.Files)
            {
                //Rselect Song
                await ItemSelectedAsync(selectedid, true);
            }
            else
            {
                //Delay Task by 1 Millisecond
                await Task.Delay(1);

                //Reselect Folder
                await FolderSelectedAsync(selectedid);
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
            Elements.UnsetElements(ref icItems, ref selectedid, ref selectedSong, ref selectedFolder, ref selectedType);

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
            if (Songs.Count() > 0) { Composite.AddRange(tbtnShowFavourites.IsSelected ? Songs.Where(i => i.OwnerId == activeFolder && i.isFavourite == 1) : Songs.Where(i => i.OwnerId == activeFolder)); }
        }

        private async Task LoadItemsAsync()
        {
            //Clear Songs Observable Collection
            Songs.Clear();

            //Get Sort Order and Type
            string order = Sort.GetOrder(cboxSortBy);
            string type = Sort.GetType(cboxSortBy);

            //Run Task
            await Task.Run(() =>
            {
                //Loop Through Each Song in the Songs Database
                foreach (Song song in Database.LoadItems(MediaType.Music, order, type))
                {
                    //Invoke Dispatcher
                    Dispatcher.Invoke(() =>
                    {
                        //Add Each Song from the Songs Database to the Songs Observable Collection
                        Songs.Add(song);
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
                foreach (Folder folder in Database.LoadFolders(MediaType.Music))
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
            if (owner == "itempanel" && isEdit && selectedSong != null)
            {
                //Validate Panel Contents
                Validation.ValidateGenericPanel(OptionPanelType.Edit, btnProcessItem, Folders.ToList(), new string[] { selectedSong.CustomName, selectedSong.CoverImage }, true, 0, new KeyValuePair<int, string>(fbdItemSaveLocation.Id, fbdItemSaveLocation.FolderPath), new string[] { $"{tbCustomName.Content}", $"{odCoverImage.Content}" }, new KeyValuePair<int, string>(selectedSong.OwnerId, nbNavigationBar.GetFolderPath()));
            }
            else if (owner == "itempanel" && !isEdit)
            {
                //Validate Panel Contents
                Validation.ValidateGenericPanel(OptionPanelType.Add, btnProcessItem, Folders.ToList(), null, true, odSongPath.Contents.Count, new KeyValuePair<int, string>(fbdItemSaveLocation.Id, fbdItemSaveLocation.FolderPath));
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