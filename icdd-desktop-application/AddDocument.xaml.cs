using Microsoft.WindowsAPICodePack.Dialogs;
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
using System.Windows.Shapes;

namespace icdd_desktop_application
{
    /// <summary>
    /// Interaction logic for AddDocument.xaml
    /// </summary>
    public partial class AddDocument : Window
    {
        public string doctype = null;
        /// <summary>
        /// Initialization of the Window
        /// </summary>
        public AddDocument()
        {
            InitializeComponent();
        }
        //FileDialog for Internal Docuemnts
        CommonOpenFileDialog dlg = new CommonOpenFileDialog 
        {
            EnsureFileExists = true,
            EnsurePathExists = true,
            InitialDirectory = @"C:\\",
            Title = "Open File"
    };
        

        //Creation of List of DataObjects to access globally
        public string GetDoctype() {
            return doctype;
        }
        public TextBox filenameTextBox = new TextBox
        {
            Background = Brushes.Transparent
        };
        public TextBox filetypeTextBox = new TextBox
        {
            Background = Brushes.Transparent
        };
        public TextBox fileformatTextBox = new TextBox
        {
            Background = Brushes.Transparent
        };
        public TextBox encryptionAlgorithmTextBox = new TextBox
        {
            Background = Brushes.Transparent
        };
        public TextBox checksumTextBox = new TextBox
        {
                Background = Brushes.Transparent
        };
        public TextBox checksumAlgoTextBox = new TextBox
        {
            Background = Brushes.Transparent
        };
        public TextBox uriTextBox = new TextBox
        {
            Background = Brushes.Transparent
        };
        ObservableCollection<TextBox> textBoxCollection = new ObservableCollection<TextBox>();
        /// <summary>
        /// Function for the Event when the ComboBox DropDown Menu closes (= a Document Type is chosen)
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void OnDropDownClosed(object sender, EventArgs e)
        {
            //Clearing the list to repopulate it
            ChosenFileName.Text = null;

            //Activating the SaveButton
            SaveButton.Visibility = Visibility.Visible;

            //Clearing the Grid
            InputGrid.Children.Clear(); 
            
            RowDefinition filename = new RowDefinition();
            filename.Height = new GridLength(30, GridUnitType.Auto);
            Label filenameLabel = new Label();
            filenameLabel.Content = "File Name:";
            Grid.SetRow(filenameLabel, 0);
            Grid.SetColumn(filenameLabel, 0);
            Grid.SetRow(filenameTextBox, 0);
            Grid.SetColumn(filenameTextBox, 1);

            RowDefinition filetype = new RowDefinition();
            filetype.Height = new GridLength(30, GridUnitType.Auto);
            Label filetypeLabel = new Label();
            filetypeLabel.Content = "File Type:";
            Grid.SetRow(filetypeLabel, 1);
            Grid.SetColumn(filetypeLabel, 0);            
            Grid.SetRow(filetypeTextBox, 1);
            Grid.SetColumn(filetypeTextBox, 1);

            RowDefinition fileformat = new RowDefinition();
            fileformat.Height = new GridLength(30, GridUnitType.Auto);
            Label fileformatLabel = new Label();
            fileformatLabel.Content = "File Format:";
            Grid.SetRow(fileformatLabel, 2);
            Grid.SetColumn(fileformatLabel, 0);
            Grid.SetRow(fileformatTextBox, 2);
            Grid.SetColumn(fileformatTextBox, 1);

            InputGrid.RowDefinitions.Add(filename);
            InputGrid.Children.Add(filenameLabel);
            InputGrid.Children.Add(filenameTextBox);

            InputGrid.RowDefinitions.Add(filetype);
            InputGrid.Children.Add(filetypeLabel);
            InputGrid.Children.Add(filetypeTextBox);

            InputGrid.RowDefinitions.Add(fileformat);
            InputGrid.Children.Add(fileformatLabel);
            InputGrid.Children.Add(fileformatTextBox);

            RowDefinition encryptionAlgorithm = new RowDefinition();
            encryptionAlgorithm.Height = new GridLength(30, GridUnitType.Auto);
            Label encryptionAlgorithmLabel = new Label();
            encryptionAlgorithmLabel.Content = "Encryption Algorithm:";
            Grid.SetColumn(encryptionAlgorithmLabel, 0);
            Grid.SetColumn(encryptionAlgorithmTextBox, 1);

            RowDefinition checksum = new RowDefinition();
            checksum.Height = new GridLength(30, GridUnitType.Auto);
            Label checksumLabel = new Label();
            checksumLabel.Content = "Checksum:";
            Grid.SetColumn(checksumLabel, 0);           
            Grid.SetColumn(checksumTextBox, 1);

            RowDefinition checksumAlgo = new RowDefinition();
            checksum.Height = new GridLength(30, GridUnitType.Auto);
            Label checksumAlgoLabel = new Label();
            checksumAlgoLabel.Content = "Checksum Algorithm:";
            Grid.SetColumn(checksumAlgoLabel, 0);            
            Grid.SetColumn(checksumAlgoTextBox, 1);

            RowDefinition uri = new RowDefinition();
            uri.Height = new GridLength(30, GridUnitType.Auto);
            Label uriLabel = new Label();
            uriLabel.Content = "URI:";
            Grid.SetColumn(uriLabel, 0);
            Grid.SetColumn(uriTextBox, 1);

            textBoxCollection.Add(filenameTextBox);
            textBoxCollection.Add(fileformatTextBox);
            textBoxCollection.Add(filetypeTextBox);

            switch (DocTypeBox.SelectedIndex)
            {
                case 0:
                    doctype = "external";
                    InputGrid.Visibility = Visibility.Visible;
                    BrowseFile.Visibility = Visibility.Hidden;
                    Grid.SetRow(uriLabel, 3);
                    Grid.SetRow(uriTextBox, 3);
                    InputGrid.RowDefinitions.Add(uri);
                    InputGrid.Children.Add(uriLabel);
                    InputGrid.Children.Add(uriTextBox);
                    textBoxCollection.Add(uriTextBox);
                    break;
                case 1:
                    doctype = "internal";
                    BrowseFile.Visibility = Visibility.Visible;
                    InputGrid.Visibility = Visibility.Visible;
                    break;
                case 2:
                    doctype = "encrypted";
                    BrowseFile.Visibility = Visibility.Visible;
                    InputGrid.Visibility = Visibility.Visible;
                    Grid.SetRow(encryptionAlgorithmLabel, 3);
                    Grid.SetRow(encryptionAlgorithmTextBox, 3);
                    InputGrid.Children.Add(encryptionAlgorithmLabel);
                    InputGrid.Children.Add(encryptionAlgorithmTextBox);
                    textBoxCollection.Add(encryptionAlgorithmTextBox);
                    break;
                case 3:
                    doctype = "secure";
                    BrowseFile.Visibility = Visibility.Visible;
                    InputGrid.Visibility = Visibility.Visible;
                    Grid.SetRow(checksumLabel, 3);
                    Grid.SetRow(checksumTextBox, 3);
                    InputGrid.Children.Add(checksumLabel);
                    InputGrid.Children.Add(checksumTextBox);
                    textBoxCollection.Add(checksumTextBox);
                    Grid.SetRow(checksumAlgoLabel, 4);
                    Grid.SetRow(checksumAlgoTextBox, 4);
                    InputGrid.Children.Add(checksumAlgoLabel);
                    InputGrid.Children.Add(checksumAlgoTextBox);
                    textBoxCollection.Add(checksumAlgoTextBox);
                    break;
                case 4:                    
                    doctype = "folder";
                    InputGrid.Visibility = Visibility.Hidden;
                    BrowseFile.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
            foreach(TextBox textBox in textBoxCollection)
            {
                textBox.GotFocus += new RoutedEventHandler(TextBoxGotFocus);
                textBox.LostFocus += new RoutedEventHandler(TextBoxLostFocus);
            }
        }

        /// <summary>
        /// Function to cancel the AddDocument action.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonCancelChanges(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Function to Save the changes and add the document to the current selected Container.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonSaveChanges(object sender, RoutedEventArgs e)
        {
            bool IsGood = false;
            if (filenameTextBox.Text != null)
            {
                foreach (TextBox textBox in textBoxCollection)
                {
                    if (textBox.Text == null)
                    {
                        IsGood = false;
                    }
                    else
                        IsGood = true;
                }
            }
            else
            {
                IsGood = false;
            }
            if (!IsGood) { 
                MessageBox.Show("Warning: Emtpy Textboxes! Please fill out all boxes.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = IsGood;
            Close();
        }

        /// <summary>
        /// Function that choses a File.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonChooseFile(object sender, RoutedEventArgs e)
        {
            string filePath = dlg.ShowDialog() == CommonFileDialogResult.Ok ? dlg.FileName : null;
            ChosenFileName.Text = filePath;

        }

        /// <summary>
        /// Event Handler when the ChosenFileName Box changes it texts. It then populates the Datagrid according to this file.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TextBoxWriteFiledata(object sender, RoutedEventArgs e)
        {
            string filePath = ChosenFileName.Text;
            filenameTextBox.Text = System.IO.Path.GetFileName(filePath);
            filetypeTextBox.Text = System.IO.Path.GetExtension(filePath);
            fileformatTextBox.Text = System.IO.Path.GetExtension(filePath);
        }

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
            (sender as TextBox).BorderThickness = new Thickness(1);
            (sender as TextBox).BorderBrush = System.Windows.Media.Brushes.Gray;
        }
    }
}
