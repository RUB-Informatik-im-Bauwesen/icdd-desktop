using ICDDToolkitLibrary.Parser;
using MahApps.Metro.Controls;
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
using ICDDToolkitLibrary.Validator;

namespace icdd_parser_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //Set up Instances
        IcddContainerParser currContainer = null;
        CommonOpenFileDialog dlg = new CommonOpenFileDialog();

        public MainWindow()
        {
            InitializeComponent();
        }

        //Selected Item from the Treeview to visualize Container Data in the Stackpanel
        private void treeItemSelected(object sender, RoutedEventArgs e)
        {
            if ((sender as TreeView).SelectedItem != null)
            {
                DataPanel.Children.Clear();         //Clear DataGrid of Content
                TextBlock CD = new TextBlock();     //Create TextBlocks
                TextBlock FP = new TextBlock();
                TextBlock FC = new TextBlock();
                IcddContainerParser selCon = (IcddContainerParser)(sender as TreeView).SelectedItem;    //Cast TreeView Item to Container Parser
                currContainer = selCon;
                CD.Text = "Container Description: " + selCon.GetContainerDescriptionToString(); //Textbox for Container Description
                CD.VerticalAlignment = VerticalAlignment.Top;
                CD.HorizontalAlignment = HorizontalAlignment.Left;
                CD.Margin = new Thickness(0, 10, 10, 0);

                FP.Text = "Local Filepath: " + selCon.GetDocumentFolder(); //Textbox for Filepath
                FP.VerticalAlignment = VerticalAlignment.Top;
                FP.HorizontalAlignment = HorizontalAlignment.Left;
                FP.Margin = new Thickness(0, 10, 30, 0);

                DirectoryInfo conDir = new DirectoryInfo(selCon.GetDocumentFolder()); //Textbox for the Total Document Count in the Container
                int _fc = conDir.GetFiles().Length; //Getting count of total of files in Document folder
                FC.Text = "Total Documents: " +  _fc;
                FP.VerticalAlignment = VerticalAlignment.Top;
                FP.HorizontalAlignment = HorizontalAlignment.Left;
                FP.Margin = new Thickness(0, 10, 50, 0);

                //Adding the Textblocks to the Stackpanel and display the data
                DataPanel.Children.Add(CD);
                DataPanel.Children.Add(FP);
                DataPanel.Children.Add(FC);
                CreateDocuments(currContainer);
                Separator separator = new Separator();
                separator.Opacity = 0;
                separator.Height = 10;
                DataPanel.Children.Add(separator);



                DataPanelWrap.Visibility = Visibility.Visible;
            }
        }

        private void ButtonAddC_Click(object sender, RoutedEventArgs e)
        {
            var prompt = new PromptWindow("Please enter Container Name:", "Creating new Container", "");
            if (prompt.ShowDialog() == true)
            {
                string name = prompt.txtResponse.Text;
                if (name != null)
                {
                    currContainer = new IcddContainerParser(name+".icdd");
                    ContainerTree.Items.Add(currContainer);
                }
                else
                {
                    MessageBox.Show("Error: Name cannot be empty!");
                }
            }
        }

        private void ButtonOC_Click(object sender, RoutedEventArgs e)
        {
            dlg.Title = "Open Container";
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            string file = dlg.ShowDialog() == CommonFileDialogResult.Ok ? dlg.FileName : null;
            if (file == null)
            {
                MessageBox.Show("Invalid File.");
                return;
            }
            IcddValidator validator = new IcddValidator(file);
            validator.Validate();
            if (validator.IsValid())
            {
                currContainer = validator.GetValidContainer();
                ContainerTree.Items.Add(currContainer);
            }
            else
            {
                MessageBox.Show("Container Validation unsuccessfull!");
            }
        }

        private void ButtonZip_Click(object sender, RoutedEventArgs e)
        {

            if (currContainer != null)
            {
                var archive = currContainer.GetCompressedContainer();
                MessageBox.Show("Container exported to: " + archive);
            }
        }

        private void ButtonEditName_Click(object sender, RoutedEventArgs e)
        {
            PromptWindow prompt = new PromptWindow("Enter a Name", "Edit Container Name", "");
            if (prompt.ShowDialog() == true)
            {
                string newName = prompt.txtResponse.Text;
                if (newName != null)
                {
                    currContainer.SetContainerName(newName+".icdd");
                    ContainerTree.Items.Refresh();
                }
                else
                {
                    MessageBox.Show("Error: Name cannot be empty!");
                }
            }
        }

        private void ButtonAddD_Click(object sender, RoutedEventArgs e)
        {
            dlg.Title = "Open Container";
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            string file = dlg.ShowDialog() == CommonFileDialogResult.Ok ? dlg.FileName : null;
            if (file == null)
            {
                MessageBox.Show("Invalid File.");
                return;
            }
            
            DirectoryInfo docDir = new DirectoryInfo(dlg.FileName);
            currContainer.AddInternalDocument(docDir.FullName, docDir.Name, docDir.Extension, "");
            UpdateLayout();
        }

        private void ButtonDeleteC_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                currContainer.DeleteContainer();
                ContainerTree.Items.Remove(currContainer);
                DataPanelWrap.Visibility = Visibility.Hidden;
            }
            else
                return;
        }

        public void CreateDocuments(IcddContainerParser container)
        {
            DirectoryInfo docDir = new DirectoryInfo(container.GetDocumentFolder());
            ScrollViewer scrollViewer = new ScrollViewer();
            StackPanel expanderPanel = new StackPanel();

            foreach (FileInfo file in docDir.GetFiles())
            {
                Expander expander = new Expander();
                StackPanel textBlockPanel = new StackPanel();
                TextBlock title = new TextBlock();
                TextBlock lastEdited = new TextBlock();
                TextBlock extension = new TextBlock();
                TextBlock size = new TextBlock();
                expander.Header = file.Name.ToString();
                title.Text = "File Location: " + file.FullName;
                textBlockPanel.Children.Add(title);

                lastEdited.Text = "Last edited: " + file.LastWriteTime;
                textBlockPanel.Children.Add(lastEdited);

                extension.Text = "File Type: " + file.Extension;
                size.Text = "File Size: " + file.Length.ToString() + " bytes";
                textBlockPanel.Children.Add(size);
                expander.Content = textBlockPanel;
                expander.Name = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                expanderPanel.Children.Add(expander);
            }
            scrollViewer.Content = expanderPanel;
            DataPanel.Children.Add(scrollViewer);
            return;
        }
    }
}
