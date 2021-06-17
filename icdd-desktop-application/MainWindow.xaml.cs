
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using ICDDToolkitLibrary.Model.Container;
using System.Reflection;
using icdd_desktop_application;
using ICDDToolkitLibrary.Model.Linkset;
using ICDDToolkitLibrary.Model.MMC;
using static ICDDToolkitLibrary.Model.MMC.MmcMultiModel;
using static ICDDToolkitLibrary.Model.MMC.MmcLinkModel;
using System.Data;
using ICDDToolkitLibrary.Model;
using ICDDToolkitLibrary.Validation;
using ICDDToolkitLibrary.Parsing;
using ICDDToolkitLibrary.Model.Container.Document;
using ICDDToolkitLibrary.Model.Linkset.Link;
using ICDDToolkitLibrary.Model.Linkset.Identifier;
using ICDDToolkitLibrary.Conversion;
using ICDDToolkitLibrary.Conversion.MsProjXml;
using Microsoft.WindowsAPICodePack.Shell.Interop;
using ICDDToolkitLibrary.Conversion.Ifc;
using ICDDToolkitLibrary.Conversion.Container;

namespace icdd_desktop_application
{
    public class DataObject
    {
        public string Property { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //Set up Instances
        public InformationContainer currContainer = null;
        MultiModelContainer currMContainer = null;
        bool icdd = false;
        CommonOpenFileDialog dlg = new CommonOpenFileDialog();
        ObservableCollection<DataObject> ruleFiles = new ObservableCollection<DataObject>();
        Dictionary<CtDocument, string> documents = new Dictionary<CtDocument, string>();
        /// <summary>
        /// Initialization of MainWindow.xaml
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LabelVersion.Content += System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        ObservableCollection<DataObject> list = new ObservableCollection<DataObject>();

        /// <summary>
        /// Function to create visible controls for the selected Container.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param> </param>
        private void ListItemSelected(object sender, RoutedEventArgs e)
        {
            if ((sender as ListView).SelectedItem != null)
            {
                MenuItemExportContainer.IsEnabled = true;
                MenuItemAddDocument.IsEnabled = true;
                MenuItemAddLinkset.IsEnabled = true;
                MenuItemDeleteContainer.IsEnabled = true;
                MenuItemValidateContainer.IsEnabled = true;
                MenuItemManageParties.IsEnabled = true;
                MenuItemAddOrganisation.IsEnabled = true;
                MenuItemAddPerson.IsEnabled = true;
                MenuItemAddLink.IsEnabled = true;
                MenuItemConvertContainer.IsEnabled = true;
                string[] conName = (sender as ListView).SelectedItem.ToString().Split('.');
                if (conName.Last() == "icdd")
                {
                    currContainer = (InformationContainer)(sender as ListView).SelectedItem;    //Cast ListView Item to Container Parser
                    icdd = true;
                }
                else if (conName.Last() == "mmc")
                {
                    currMContainer = (MultiModelContainer)(sender as ListView).SelectedItem;
                    icdd = false;
                }
                DataPanel.Children.Clear();         //Clear DataGrid of Content

                //Create Parent Expanders
                Expander Documents = new Expander
                {
                    Header = "Palyoad Documents",
                    IsExpanded = true
                };

                Expander Ontologies = new Expander
                {
                    Header = "Ontology Resources",
                    IsExpanded = true
                };

                Expander PayloadTriples = new Expander
                {
                    Header = "Payload Triples",
                    IsExpanded = true
                };

                //Set Color of the Expanders
                var converter = new BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#003560");
                var brushForeground = (Brush)converter.ConvertFromString("#FFFFFF");
                Documents.Background = Ontologies.Background = PayloadTriples.Background = brush;
                Documents.Foreground = Ontologies.Foreground = PayloadTriples.Foreground = brushForeground;

                //Content Generation for ICDD Containers:

                if (icdd)
                {
                    foreach (Button control in ButtonPanel.Children)
                    {
                        control.IsEnabled = true;
                    }
                    foreach (Button control in RuleFileButtons.Children)
                    {
                        control.IsEnabled = true;
                    }
                    Ontologies.Content = CreateOntologyPayload(currContainer);
                    Documents.Content = CreateDocuments(currContainer);
                    PayloadTriples.Content = CreatePayloadTriples(currContainer);
                    
                    //Create Key Value Pairs for the Property Grid
                    CtContainerDescription description = currContainer.ContainerDescription;

                    NameTextBox.Text = currContainer.ContainerName;
                    DescriptionTextBox.Text = description.Description;
                    VersionIDTextBox.Text = description.VersionId;
                    VersionDescriptionTextBox.Text = description.VersionDescription;
                    FilepathLabel.Content = currContainer.PathToContainer;
                    ConformanceIndicatorLabel.Content = description.ConformanceIndicator;
                    CreatorLabel.Content = description.Creator?.ToString();
                    CreationDateLabel.Content = description.Creation.ToString();
                    ModificatorLabel.Content = description.Modifier?.ToString();
                    ModificationDateLabel.Content = description.Modification.ToString();
                }

                else
                {
                    //Disable Buttons for the MMC View
                    foreach (Button control in ButtonPanel.Children)
                    {
                        control.IsEnabled = false;
                    }
                    foreach(Button control in RuleFileButtons.Children)
                    {
                        control.IsEnabled = false;
                    }

                    //Create Key Value Pairs for Property Grid from MMC MetaData
                    MultiModel ct = currMContainer.GetContainerDescription();
                    list.Add(new DataObject() { Property = "Name", Value = currMContainer.GetContainerName() });
                    MultiModelMetaData[] metaData = ct.MetaData;
                    List<MultiModelMetaDataMeta> metas = new List<MultiModelMetaDataMeta>();
                    foreach(MultiModelMetaData meta in metaData)
                    {
                        metas.Add(meta.Meta);
                    }
                    foreach(MultiModelMetaDataMeta meta in metas)
                    {
                        list.Add(new DataObject() { Property = meta.key, Value = meta.value.ToString() });
                    }

                    DirectoryInfo conDir = new DirectoryInfo(currMContainer.GetDocumentFolder());
                    int docCount = conDir.GetFiles().Length;
                    list.Add(new DataObject() { Property = "Document Count", Value = docCount.ToString() });

                    //Creating Content for Documents and PayloadTriples
                    Documents.Content = CreateDocuments(currMContainer);
                    PayloadTriples.Content = CreatePayloadTriples(currMContainer);
                }

                //Adding the Textblocks to the Stackpanel and display the data
                ScrollViewer boxes = new ScrollViewer
                {
                    Height = 590
                };
                StackPanel boxi = new StackPanel();
                if(icdd)
                    boxi.Children.Add(Ontologies);
                boxi.Children.Add(Documents);
                boxi.Children.Add(PayloadTriples);
                boxes.Content = boxi;
                DataPanel.Children.Add(boxes);

                Separator separator = new Separator
                {
                    Opacity = 0,
                    Height = 10
                };


                DataPanel.Children.Add(separator);
                PanelLeftProperties.Visibility = Visibility.Visible;
            }
        }

        #region Buttonfunctions

        /// <summary>
        ///  Button to Edit the selected Rulefile.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonEditRuleFile(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = null;
                if (RuleFilesList.SelectedItem != null)
                {
                    MenuItemValidateRuleFile.IsEnabled = true;
                    MenuItemExportResults.IsEnabled = true;
                    foreach (DataObject dataObject in ruleFiles)
                    {
                        path = dataObject.Property == RuleFilesList.SelectedItem.ToString() ? dataObject.Value : null;
                        if (path != null)
                            break;
                    }
                }
                Editor editor = new Editor(path);
                if (editor.ShowDialog() == true)
                {
                    File.WriteAllText(path, editor.FileTextBox.Text);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: Couldn't read file. " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Function to Load Rulefiles.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonLoadRuleFile(object sender, RoutedEventArgs e) //Button function to load Rule Files
        {
            dlg.Title = "Load Rule File";
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            string file = dlg.ShowDialog() == CommonFileDialogResult.Ok ? dlg.FileName : null;
            var ext = System.IO.Path.GetExtension(file);
            e.Handled = true;
            if (file == null)
            {
                return;
            }
            if (ext != ".ttl")
            {
                MessageBox.Show("Invalid File.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }            
            else
            {
                try
                {
                    DataObject t = new DataObject { Property = dlg.FileName.Split('\\').Last(), Value = dlg.FileName };
                    ruleFiles.Add(t);
                    RuleFilesList.Items.Add(t.Property);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error: Couldn't add Rule File. " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return;
        }

        /// <summary>
        /// Function to add Containers to the program. 
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonAddContainer(object sender, RoutedEventArgs e) //Button Function to add Containers
        {
            var prompt = new PromptWindow("Please enter Container Name:", "Creating new Container", "");
            if (prompt.ShowDialog() == true)
            {
                string name = prompt.txtResponse.Text;
                if (name != null)
                {
                    try {
                        IcddContainerBuilder builder = new IcddContainerBuilder();
                        InformationContainer container = builder.GetAssembledContainer();
                        container.ContainerName = name + ".icdd";
                        currContainer = container;
                        ContainerList.Items.Add(currContainer);
                    }
                    catch(Exception ex)
                    {
                        
                        MessageBox.Show("Error: Container couldn't be added. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Error: Name cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Function to open external Containers.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonOpenContainer(object sender, RoutedEventArgs e)
        {
            try
            {
                dlg.Title = "Open Container";
                dlg.IsFolderPicker = false;
                dlg.EnsureFileExists = true;
                dlg.EnsurePathExists = true;
                string file = dlg.ShowDialog() == CommonFileDialogResult.Ok ? dlg.FileName : null;
                var ext = System.IO.Path.GetExtension(file);
                if(file == null)
                {
                    return;
                }
                if (ext != ".icdd" && ext != ".mmc")
                {
                    MessageBox.Show("Invalid File.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                IcddContainerReader icddReader = new IcddContainerReader(file);
                List<IcddValidationResult> results = icddReader.GetValidationResults();
                if (ext == ".icdd")
                {

                    currContainer = icddReader.Read();
                    ContainerList.Items.Add(currContainer);
                }

                else if (ext == ".mmc")
                {
                    currMContainer = new MultiModelContainer(file, dlg.FileName);
                    ContainerList.Items.Add(currMContainer);
                }

                else
                {
                    MessageBox.Show("Container Validation unsuccessfull!", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    foreach (IcddValidationResult result in results)
                    {
                        ValidationResults.Items.Add(result.ToString());
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Couldn't Open Container. " + ex.ToString(), "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Function to zip the selected Container.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonZipContainer(object sender, RoutedEventArgs e)
        {
            if (currContainer != null)
            {
                dlg.Title = "Select ZIP Folder";
                dlg.IsFolderPicker = true;
                dlg.EnsurePathExists = true;
                dlg.EnsureFileExists = true;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        string folder = dlg.FileName + @"\" + currContainer.ContainerName;
                        IcddContainerWriter icddWriter = new IcddContainerWriter(folder);
                        icddWriter.Write(currContainer);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error: Couldn't retrieve compressed Container. " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Couldn't zip Container at Location " + dlg.FileName, "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }
        /// <summary>
        /// Function to add a Document to the selected Container.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonAddDocument(object sender, RoutedEventArgs e)
        {
            AddDocument addDocument = new AddDocument();
            List<string> result = new List<string>();
            if (addDocument.ShowDialog() == true)
            {
                ICollection<CtDocument> documents = currContainer.ContainerDescription.ContainsDocument;
                foreach (CtDocument doc in documents)
                {
                    if (addDocument.filenameTextBox.Text == doc.Name)
                    {
                        var fullname = addDocument.filenameTextBox.Text.Split('.');
                        var name = fullname.First();
                        var ext = fullname.Last();                       
                        Guid g = Guid.NewGuid();
                        addDocument.filenameTextBox.Text = name + g.ToString() + "." + ext;
                    }
                }
                string filePath = addDocument.ChosenFileName.Text;
                switch (addDocument.GetDoctype())
                {
                    case "external":
                        currContainer.CreateExternalDocument(addDocument.uriTextBox.Text, addDocument.filenameTextBox.Text, addDocument.filetypeTextBox.Text, addDocument.fileformatTextBox.Text);
                        break;
                    case "internal":
                        currContainer.CreateInternalDocument(filePath, addDocument.filenameTextBox.Text, addDocument.filetypeTextBox.Text, addDocument.fileformatTextBox.Text);
                        break;
                    case "encrypted":
                        currContainer.CreateEncryptedDocument(filePath, addDocument.filenameTextBox.Text, addDocument.filetypeTextBox.Text, addDocument.fileformatTextBox.Text, addDocument.encryptionAlgorithmTextBox.Text);
                        break;
                    case "secure":
                        currContainer.CreateSecuredDocument(filePath, addDocument.filenameTextBox.Text, addDocument.filetypeTextBox.Text, addDocument.fileformatTextBox.Text, addDocument.checksumTextBox.Text, addDocument.checksumAlgoTextBox.Text);
                        break;
                    case "folder":
                        currContainer.CreateFolderDocument(filePath);
                        break;
                    default:
                        break;
                }
            }
            ContainerList.Items.Refresh();            
        }


        /// <summary>
        /// Function to delete the selected Container
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonDeleteContainer(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                currContainer.Dispose();
                ContainerList.Items.Remove(currContainer);
                PanelLeftProperties.Visibility = Visibility.Hidden;
            }
            else
                return;
        }

        /// <summary>
        /// Function to edit Documents.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <param name="name">The name.</param>
        /// <param name="fdesc">The File Description. .</param>
        /// <param name="id">The identifier.</param>
        private void ButtonEditDocument(object sender, RoutedEventArgs e, string name, string fdesc, string id)
        {
            try
            {
                CtDocument doc = currContainer.ContainerDescription.GetDocument(id);
                if (doc.Name != name)
                {
                    currContainer.ContainerDescription.GetDocument(id).Name = name + "." + doc.FileType;
                }
                if (doc.Description != fdesc)
                {
                    currContainer.ContainerDescription.GetDocument(id).Description = fdesc;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: Couldn't edit the Container Description. " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }                        
        }

        /// <summary>
        /// Function to delete the selected Document.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <param name="documentId">The ID of the Document in the Container.</param>
        private void ButtonDeleteDocument(object sender, RoutedEventArgs e, string documentId)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    currContainer.DeleteDocument(documentId);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: Couldn't delete Document. " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Function to Validate the selected Rulefile.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonValidateRuleFile(object sender, RoutedEventArgs e)
        {
            string path = null;
            if (RuleFilesList.SelectedItem != null)
            {
                foreach (DataObject dataObject in ruleFiles)
                {
                    path = dataObject.Property == RuleFilesList.SelectedItem.ToString() ? dataObject.Value : null;
                }
            }
            if (path != null)
            {
                IcddValidator icddRuleValidator = null;
                try
                {
                    icddRuleValidator = new IcddValidator(currContainer);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Invalid Rulefile. Exception:\n\n" + exception.ToString(), "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                if (icddRuleValidator != null)
                {
                    try
                    {
                        icddRuleValidator.Validate();
                        List<IcddValidationResult> results = icddRuleValidator.GetResults();
                        foreach (IcddValidationResult result in results)
                        {
                            ValidationResults.Items.Add(result.ToString());
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Parsing Error during validation. Exception:\n\n" + exception.ToString(), "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }                    
                }
            }
            else
            {
                MessageBox.Show("Invalid Rulefile.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }            
        }

        /// <summary>
        /// Function to clear the RuleFile Result Textbox.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonClearRulesResultBox(object sender, RoutedEventArgs e)
        {
            ValidationResults.Items.Clear();
        }

        /// <summary>
        /// Function to add a Linkset to the selected Container.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonAddLinkset(object sender, RoutedEventArgs e)
        {
            try
            {
                PromptWindow prompt = new PromptWindow("Linkset Name", "Create new Linkset", ".rdf", "");
                if(prompt.ShowDialog() == true)
                {
                    string linksetName = prompt.txtResponse.Text;
                    if(linksetName != null)
                    {
                        currContainer.CreateLinkset(linksetName);
                    }
                    else
                    {
                        MessageBox.Show("Error: Name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Button Function to Export the Validation Results
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonExportValidationResults(object sender, RoutedEventArgs e)
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog();
            saveFileDialog.DefaultFileName = "Validation_" + currContainer.ContainerName + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_" + RuleFilesList.SelectedItem.ToString() + ".log";
            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string path = null;
                if (RuleFilesList.SelectedItem != null)
                {
                    foreach (DataObject dataObject in ruleFiles)
                    {
                        path = dataObject.Property == RuleFilesList.SelectedItem.ToString() ? dataObject.Value : null;
                    }
                }
                IcddValidator icddValidator = new IcddValidator(currContainer);
                icddValidator.Validate();
                List<IcddValidationResult> results = icddValidator.GetResults();
                StreamWriter sw = new StreamWriter(saveFileDialog.FileName);
                foreach (IcddValidationResult result in results)
                {
                    sw.WriteLine(result.ToString());
                }
                sw.Close();
            }
        }

        /// <summary>
        /// Button to check the conformity of the Container.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonValidateConformContainer(object sender, RoutedEventArgs e)
        {
            IcddContainerReader reader = new IcddContainerReader(currContainer.PathToIndex);
            IcddValidator validator = new IcddValidator(currContainer);
            if (reader.IsValid())
            {
                MessageBox.Show("Container is conform.", "Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            List<IcddValidationResult> results = reader.GetValidationResults();
            foreach (IcddValidationResult result in results)
            {
                ValidationResults.Items.Add(result.ToString());
            }
        }

        private void MenuButtonManageParties(object sender, RoutedEventArgs e)
        {
            ManageParties manageParties = new ManageParties(currContainer.ContainerDescription);
            manageParties.Show();
        }

        /// <summary>
        /// Menubutton Function to add a new Organisation to the selected Container.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MenuButtonAddOrganisation(object sender, RoutedEventArgs e)
        {
            TextBox AdditionalTextBox = new TextBox
            {
                Width = 400,
                Margin = new Thickness(5, 0, 5, 5),
                Text = "Description"
            };
            PromptWindow prompt = new PromptWindow("Details of new Organisation", "Creating new Organisation", AdditionalTextBox, "Name", "Description");
            if(prompt.ShowDialog() == true)
            {
                CtOrganisation ctOrganisation = currContainer.ContainerDescription.AddOrganisation(prompt.txtResponse.Text, AdditionalTextBox.Text);
            }
        }

        /// <summary>
        /// Menubutton Function to add a new Person to the selected Container.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MenuButtonAddPerson(object sender, RoutedEventArgs e)
        {
            TextBox AdditionalTextBox = new TextBox
            {
                Width = 400,
                Margin = new Thickness(5, 0, 5, 5),
                Text = "Description"
            };
            PromptWindow prompt = new PromptWindow("Details of new Person", "Creating new Person", AdditionalTextBox, "Name", "Description");
            if (prompt.ShowDialog() == true)
            {
                CtPerson ctPerson = currContainer.ContainerDescription.AddPerson(prompt.txtResponse.Text, AdditionalTextBox.Text);
            }
        }

        /// <summary>
        /// Menubutton Function to add a new link to the selected Container.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MenuButtonAddLink(object sender, RoutedEventArgs e)
        {
            AddLink addLink = new AddLink();
            if (addLink.ShowDialog() == true)
            {
                currContainer.SaveRdf();
            };
        }
        #endregion

        #region ContentGenerators
        #region Icdd Content Generators

        /// <summary>
        /// Function that creates the Controls to display the Ontology of the ICDD Container
        /// </summary>
        /// <param name="container">The ICDD Container for which the Ontology should be created.</param>
        /// <returns></returns>
        public object CreateOntologyPayload(InformationContainer container)
        {
            DirectoryInfo docDir = new DirectoryInfo(container.PathToContainer);
            StackPanel expanderPanel = new StackPanel();
            bool even = true;

            foreach (FileInfo file in docDir.GetFiles())
            {
                Expander expander = new Expander();
                ScrollViewer scrollViewerInner = new ScrollViewer();
                StackPanel stack = new StackPanel();
                expander.Background = even ? Brushes.LightGray : Brushes.WhiteSmoke;
                expander.Margin = new Thickness(20, 0, 0, 0);
                even = !even;
                TextBlock text = new TextBlock();
                expander.Header = file.Name.ToString();
                text.Text = "File Location:\t" + file.FullName + "\nLast edited:\t" + file.LastWriteTime + "\nFile Type:\t" + file.Extension + "\nFile Size: " + file.Length.ToString() + "bytes";
                scrollViewerInner.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                scrollViewerInner.Content = text;
                expander.Content = scrollViewerInner;
                expanderPanel.Children.Add(expander);
            }
            return expanderPanel;
        }

        /// <summary>
        /// Function to create Data and Controls for Documents of the selected ICDD Container.
        /// </summary>
        /// <param name="container">The ICDD Container for which the Documents should be created.</param>
        /// <returns></returns>
        public object CreateDocuments(InformationContainer container)
        {        
            StackPanel expanderPanel = new StackPanel(); //StackPanel for the Expanders
            CtContainerDescription description = container.ContainerDescription;
            Dictionary<CtDocument, List<LsLinkElement>> links = description.ContainsLinkElement;
            ICollection<CtDocument> docs = description.ContainsDocument;
            ContextMenu contextMenu = (ContextMenu)this.Resources["ExpanderConvertToTtlMenu"];
            bool even = true;
            foreach(CtDocument document in docs)
            {
                
                documents.Add(document, document.Name);
                Expander expander = new Expander //Expander for each Document
                {
                    Header = document.Name,
                    Background = even ? Brushes.LightGray : Brushes.WhiteSmoke,
                    Margin = new Thickness(20, 0, 0, 0)
                };
                if (document.FileType.Contains("xml") || document.FileType.Contains("XML") || document.FileType.Contains("ifc") || document.FileType.Contains("IFC"))
                {
                    expander.ContextMenu = contextMenu;
                }
                even = !even;
                ScrollViewer sv = new ScrollViewer
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                StackPanel expanderContent = new StackPanel(); //StackPanel for Document Data and Buttons
                DataGrid data = new DataGrid
                {
                    HeadersVisibility = DataGridHeadersVisibility.None,
                    IsReadOnly = true,
                    GridLinesVisibility = DataGridGridLinesVisibility.None
                }; 
                //Create DataGrid for the Document Data to display
                DataGridTextColumn property = new DataGridTextColumn //Create Columns + Binding for the DataGrid
                {
                    Binding = new Binding("Property")
                };
                DataGridColumn value = new DataGridTextColumn
                {
                    Binding = new Binding("Value")
                };
                data.Columns.Add(property);
                data.Columns.Add(value);

                //Add MetaData of the Document to DataGrid
                data.Items.Add(new DataObject() { Property = "Document Name:", Value = document.Name });
                data.Items.Add(new DataObject() { Property = "Creator:", Value = document.Creator?.ToString() });
                data.Items.Add(new DataObject() { Property = "Creation Date:", Value = document.Creation.ToString()});
                data.Items.Add(new DataObject() { Property = "Version ID:", Value = document.VersionId});
                data.Items.Add(new DataObject() { Property = "Version Description:", Value = document.VersionDescription});
                data.Items.Add(new DataObject() { Property = "File Format:", Value = document.FileFormat });
                data.Items.Add(new DataObject() { Property = "File Extension:", Value = document.Name.Split('.').Last()});                
                expanderContent.Children.Add(data);

                //Add LinkElements from the Document to the Expander
                TextBlock textBlock = new TextBlock();
                links.TryGetValue(document, out List<LsLinkElement> LinkElems);
                if (LinkElems != null)
                {
                    foreach (LsLinkElement linkelem in LinkElems)
                    {
                        textBlock.Text += " linked through Identifier " + linkelem.HasIdentifier + " in document " + linkelem.HasDocument.Name + "\n";
                    }
                    expanderContent.Children.Add(textBlock);
                }

                //Add Buttons to the Expander
                StackPanel buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    FlowDirection = FlowDirection.RightToLeft
                };
                Button editButton = new Button
                {
                    Width = 36,
                    Height = 36,
                    Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/icons/edit.png", UriKind.Relative)),
                        VerticalAlignment = VerticalAlignment.Center,
                        Stretch = Stretch.Fill,
                        Width = 18,
                        Height = 18
                    },
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                editButton.SetResourceReference(Control.StyleProperty, "MetroCircleButtonStyle");

                Button deleteButton = new Button
                {
                    Width = 36,
                    Height = 36,
                    Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/icons/delete.png", UriKind.Relative)),
                        VerticalAlignment = VerticalAlignment.Center,
                        Stretch = Stretch.Fill,
                        Width = 18,
                        Height = 18
                    },
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                deleteButton.SetResourceReference(Control.StyleProperty, "MetroCircleButtonStyle");

                //editButton.Click += (sender, EventArgs) => { ButtonEditDoc(sender, EventArgs, _Name.Text, _FDesc.Text, doc.GetID()); };
                deleteButton.Click += (sender, EventArgs) => { ButtonDeleteDocument(sender, EventArgs, document.Guid); };

                buttonPanel.Children.Add(editButton);
                buttonPanel.Children.Add(deleteButton);

                expanderContent.Children.Add(buttonPanel);

                sv.Content = expanderContent;
                expander.Content = sv;
                expanderPanel.Children.Add(expander);
            }
            return expanderPanel;
        }

        /// <summary>
        /// Function to create Controls and Data for the PayloadTriples in the selected ICDD Container.
        /// </summary>
        /// <param name="container">The ICDD Container for which the PayloadTriples should be created.</param>
        /// <returns></returns>
        public object CreatePayloadTriples(InformationContainer container)
        {
            //Create Instances of Objects
            List<CtLinkset> linksets = currContainer.Linksets;
            StackPanel expanderPanel = new StackPanel();
            bool even = true;
            
            foreach (CtLinkset linkset in linksets) //For each LinkSet in your Container:
            {
                var linkcount = 1; //counter to know which link you are in
                DataGrid linkGrid = new DataGrid //Output DataGrid
                {
                    HeadersVisibility = DataGridHeadersVisibility.None,
                    IsReadOnly = true,
                    Margin = new Thickness(20, 0, 0, 0),
                    GridLinesVisibility = DataGridGridLinesVisibility.None
                };
                DataGridTextColumn property = new DataGridTextColumn //Create Columns + Binding for the DataGrid
                {
                    Binding = new Binding("Property")
                };
                DataGridColumn value = new DataGridTextColumn
                {
                    Binding = new Binding("Value")
                };
                linkGrid.Columns.Add(property);
                linkGrid.Columns.Add(value);
                Expander expander = new Expander
                {
                    Background = even ? Brushes.LightGray : Brushes.WhiteSmoke,
                    Margin = new Thickness(20, 0, 0, 0)
                };
                even = !even;
                List<LsLink> links = linkset.HasLinks;
                foreach (LsLink link in links) //For each List of PayloadTriples inside this LinkSet
                {
                    string actualLink = "Link " + linkcount;
                    string output = null;
                    List<LsLinkElement> linkElements = link.HasLinkElements;
                    foreach (LsLinkElement element in linkElements) //For each Link inside the List of PayloadTriples
                    {
                        CtDocument doc = element.HasDocument;
                        LsIdentifier identifier = element.HasIdentifier;
                        if (doc != null && identifier != null)
                        {
                            output += element.HasDocument.Name + element.HasIdentifier.ToString() + "\n";
                        }
                    }                    
                    linkGrid.Items.Add(new DataObject { Property = actualLink, Value = output });
                    linkcount++;
                }
                expander.Content = linkGrid;
                expander.Header = linkset.FileName;
                expanderPanel.Children.Add(expander);
            }
            return expanderPanel;
        }
        #endregion
        #region MMC Content Generators

        /// <summary>
        /// Function to create Controls and Data for the Documents in the selected MMC.
        /// </summary>
        /// <param name="container">The MMC for which the PayloadTriples should be created.</param>
        /// <returns></returns>
        public object CreateDocuments(MultiModelContainer container)
        {
            StackPanel expanderPanel = new StackPanel(); //StackPanel for the Expanders
            MmcMultiModel description = container.GetContainer();
            ICollection<MultiModelApplicationModel> docs = description.ContainsDocument;
            bool even = true;
            foreach (MultiModelApplicationModel document in docs)
            {
                Expander expander = new Expander //Expander for each Document
                {
                    Header = document.ModelData.DataRessource.location,
                    Background = even ? Brushes.LightGray : Brushes.WhiteSmoke,
                    Margin = new Thickness(20, 0, 0, 0)
                };
                even = !even;
                ScrollViewer sv = new ScrollViewer
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                StackPanel expanderContent = new StackPanel(); //StackPanel for Document Data and Buttons
                DataGrid data = new DataGrid
                {
                    HeadersVisibility = DataGridHeadersVisibility.None,
                    IsReadOnly = true,
                    GridLinesVisibility = DataGridGridLinesVisibility.None
                };
                //Create DataGrid for the Document Data to display
                DataGridTextColumn property = new DataGridTextColumn //Create Columns + Binding for the DataGrid
                {
                    Binding = new Binding("Property")
                };
                DataGridColumn value = new DataGridTextColumn
                {
                    Binding = new Binding("Value")
                };
                data.Columns.Add(property);
                data.Columns.Add(value);

                //Add MetaData of the Document to DataGrid
                data.Items.Add(new DataObject() { Property = "Model Name:", Value = document.ModelData.DataRessource.location }); 
                data.Items.Add(new DataObject() { Property = "Model Format:", Value = document.modelType });
                data.Items.Add(new DataObject() { Property = "Model Type:", Value = document.ModelData.formatType });
                expanderContent.Children.Add(data);

                //Add Buttons to the Expander
                StackPanel buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    FlowDirection = FlowDirection.RightToLeft
                };
                Button editButton = new Button
                {
                    Width = 36,
                    Height = 36,
                    Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/icons/edit.png", UriKind.Relative)),
                        VerticalAlignment = VerticalAlignment.Center,
                        Stretch = Stretch.Fill,
                        Width = 18,
                        Height = 18
                    },
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                editButton.SetResourceReference(Control.StyleProperty, "MetroCircleButtonStyle");

                Button deleteButton = new Button
                {
                    Width = 36,
                    Height = 36,
                    Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/icons/delete.png", UriKind.Relative)),
                        VerticalAlignment = VerticalAlignment.Center,
                        Stretch = Stretch.Fill,
                        Width = 18,
                        Height = 18
                    },
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                deleteButton.SetResourceReference(Control.StyleProperty, "MetroCircleButtonStyle");

                //editButton.Click += (sender, EventArgs) => { ButtonEditDoc(sender, EventArgs, _Name.Text, _FDesc.Text, doc.GetID()); };
                deleteButton.Click += (sender, EventArgs) => { ButtonDeleteDocument(sender, EventArgs, document.id); };

                buttonPanel.Children.Add(editButton);
                buttonPanel.Children.Add(deleteButton);

                expanderContent.Children.Add(buttonPanel);

                sv.Content = expanderContent;
                expander.Content = sv;
                expanderPanel.Children.Add(expander);
            }
            return expanderPanel;
        }

        /// <summary>
        /// Function to create Controls and Data for the PayloadTriples in the selected MMC.
        /// </summary>
        /// <param name="container">The MMC for which the PayloadTriples should be created.</param>
        /// <returns></returns>
        public object CreatePayloadTriples(MultiModelContainer container)
        {
            //Create Instances of Objects
            MmcMultiModel description = container.GetContainer();
            var linksets = description.ContainsLinksetModel;
            StackPanel expanderPanel = new StackPanel();
            if (linksets == null)
                return expanderPanel;
            bool even = true;

            foreach (MmcLinkModel linkset in linksets) //For each LinkSet in your Container:
            {
                var linkcount = 1; //counter to know which link you are in
                DataGrid linkGrid = new DataGrid //Output DataGrid
                {
                    HeadersVisibility = DataGridHeadersVisibility.None,
                    IsReadOnly = true,
                    Margin = new Thickness(20, 0, 0, 0),
                    GridLinesVisibility = DataGridGridLinesVisibility.None
                };
                DataGridTextColumn property = new DataGridTextColumn //Create Columns + Binding for the DataGrid
                {
                    Binding = new Binding("Property")
                };
                DataGridColumn value = new DataGridTextColumn
                {
                    Binding = new Binding("Value")
                };
                linkGrid.Columns.Add(property);
                linkGrid.Columns.Add(value);
                Expander expander = new Expander
                {
                    Background = even ? Brushes.LightGray : Brushes.WhiteSmoke,
                    Margin = new Thickness(20, 0, 0, 0)
                };
                even = !even;
                LinkModel linkModel = linkset.LinkModelDescription;
                if (linkModel.Link != null) { 
                    List<LinkModelLink> links = linkModel.Link.ToList();
                
                    foreach (LinkModelLink link in links) //For each List of PayloadTriples inside this LinkSet
                    {
                        string actualLink = "Link " + linkcount;
                        string output = null;
                        List<LinkModelLinkRelatum> linkElements = link.Relatum.ToList();
                        foreach (LinkModelLinkRelatum element in linkElements) //For each Link inside the List of PayloadTriples
                        {
                            string doc = element.m;
                            string identifier = element.id;
                            if (doc != null && identifier != null)
                            {
                                output += doc + ": " + identifier + "\n";
                            }
                        }
                        linkGrid.Items.Add(new DataObject { Property = actualLink, Value = output });
                        linkcount++;
                    }
                }
                expander.Content = linkGrid;
                expander.Header = linkset.LinksetName;
                expanderPanel.Children.Add(expander);
            }
            return expanderPanel;
        }
        #endregion

        #endregion

        #region Helper Functions

        /// <summary>
        /// Helper Function to check if all Strings in a List are not empty.
        /// </summary>
        /// <param name="strings">Input String List</param>
        /// <returns>True when every String in the List is not empty. False when one of the Strings is empty</returns>
        public bool IsGood(List<string> strings)
        {
            foreach (string s in strings)
            {
                if (s == null || s == "") 
                    return false;
            }
            return true;
        }
        #endregion

        #region Eventhandler
        /// <summary>
        /// Eventhandler when a TextBox gets Focus
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).BorderThickness = new Thickness(1);
        }

        /// <summary>
        /// Eventhandler when a TextBox loses Focus
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).BorderThickness = new Thickness(0);
        }

        /// <summary>
        /// Eventhandler for when Data has changed in the PropertyGrid
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TextChangedGrid(object sender, RoutedEventArgs e)
        {
            currContainer.ContainerName = NameTextBox.Text;
            CtContainerDescription description = currContainer.ContainerDescription;
            description.Description = DescriptionTextBox.Text;
            description.VersionId = VersionIDTextBox.Text;
            description.VersionDescription = VersionDescriptionTextBox.Text;
        }

        private void ContextmenuItemConvertToTtl(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            while(item.Parent is MenuItem)
            {
                item = (MenuItem)item.Parent;
            }
            var menu = item.Parent as ContextMenu;
            if (menu != null)
            {
                var expander = menu.PlacementTarget as Expander;
                string docname = expander.Header.ToString();
                CtInternalDocument doc = (CtInternalDocument)documents.Where(p => p.Value == docname).Select(p => p.Key).FirstOrDefault();
                string path = "";
                if (doc.FileType.Contains("xml") || doc.FileType.Contains("XML"))
                {
                    MsProjConverter msProjConverter = new MsProjConverter(doc);
                     path = msProjConverter.ConvertToFile();
                }
                else if (doc.FileType.Contains("ifc") || doc.FileType.Contains("IFC"))
                {
                    IfcConverter ifcConverter = new IfcConverter(doc);
                    path = ifcConverter.ConvertToFile();
                }
                if (!String.IsNullOrEmpty(path))
                {
                    CommonOpenFileDialog commonFileDialog = new CommonOpenFileDialog();
                    commonFileDialog.IsFolderPicker = true;
                    commonFileDialog.InitialDirectory = path;
                    if(commonFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        File.Move(path, commonFileDialog.FileName);
                    }
                }
            }
        }

        public void MenuItemConvertContainerToTtl(object sender, RoutedEventArgs e)
        {
            if (currContainer != null)
            {
                indeterminateprogressbar pbWindow = new indeterminateprogressbar();
                pbWindow.Show();
                IcddConverter icddConverter = new IcddConverter(currContainer);
                string path = icddConverter.ConvertToFile();
                if (!String.IsNullOrEmpty(path))
                {
                    pbWindow.Close();
                    CommonOpenFileDialog commonFileDialog = new CommonOpenFileDialog();
                    commonFileDialog.IsFolderPicker = true;
                    commonFileDialog.InitialDirectory = path;
                    if (commonFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        if(path != commonFileDialog.FileName)
                            File.Move(path, commonFileDialog.FileName);
                    }
                }
            }

        }
        #endregion
    }
}
