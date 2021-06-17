using ICDDToolkitLibrary.Model;
using ICDDToolkitLibrary.Model.Container;
using ICDDToolkitLibrary.Model.Container.Document;
using ICDDToolkitLibrary.Model.Linkset;
using ICDDToolkitLibrary.Model.Linkset.Identifier;
using ICDDToolkitLibrary.Model.Linkset.Link;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VDS.RDF.Query.Expressions.Conditional;

namespace icdd_desktop_application
{
    /// <summary>
    /// Interaction logic for AddLink.xaml
    /// </summary>
    /// 
    public class LinksetModel
    {
        public CtLinkset CtLinkset { get; set; }
        public string Name { get; set; }
        public LinksetModel(CtLinkset CtLinkset, string name)
        {
            this.CtLinkset = CtLinkset;
            this.Name = name;
        }
    }
    public class DocumentModel
    {
        public CtDocument CtDocument { get; set; }
        public string Name { get; set; }
        public DocumentModel(CtDocument ctDocument, string name)
        {
            this.CtDocument = ctDocument;
            this.Name = name;
        }
    }

    public class LinksetViewModel : INotifyPropertyChanged
    {
        private CollectionView _linksets;
        private CollectionView _documents1;
        private CollectionView _documents2;

        public LinksetModel SlinksetModel;
        public DocumentModel SDocument1 { get; set; }
        public DocumentModel SDocument2 { get; set; }

        public LinksetViewModel(InformationContainer informationContainer)
        {
            ObservableCollection<LinksetModel> linksetModels = new ObservableCollection<LinksetModel>();
            ObservableCollection<DocumentModel> documentModels = new ObservableCollection<DocumentModel>();
            foreach (CtLinkset ctLinkset in informationContainer.Linksets)
            {
                LinksetModel LinksetModel = new LinksetModel(ctLinkset, ctLinkset.FileName);
                linksetModels.Add(LinksetModel);
            }
            foreach (CtDocument ctDocument in informationContainer.Documents)
            {
                DocumentModel documentModel = new DocumentModel(ctDocument, ctDocument.Name);
                documentModels.Add(documentModel);
            }
            _linksets = new CollectionView(linksetModels);
            _documents1 = new CollectionView(documentModels);
            _documents2 = new CollectionView(documentModels);
        }

        public CollectionView Linksets { get { return _linksets; } }
        public CollectionView Documents1 { get { return _documents1; } }
        public CollectionView Documents2 { get { return _documents2; } }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public partial class AddLink : Window
    {
        public int identifierCounter = 0;
        public InformationContainer InformationContainer;
        public ObservableCollection<LinksetModel> ctLinksets;
        public Dictionary<string, CtDocument> keyValueDocuments;
        public Dictionary<int, ComboBox> identifierBoxes;
        public Dictionary<int, ComboBox> elementBoxes;
        public Dictionary<int, List<TextBox>> identifiers;
        public string linktype;
        LinksetViewModel vm;
        public AddLink()
        {
            InitializeComponent();
            keyValueDocuments = new Dictionary<string, CtDocument>();
            InformationContainer = ((MainWindow)Application.Current.MainWindow).currContainer;
            List<CtDocument> ctDocuments = InformationContainer.Documents;
            foreach(CtDocument doc in ctDocuments)
            {
                keyValueDocuments.Add(doc.Name, doc);
            }
            vm = new LinksetViewModel(InformationContainer);
            DataContext = vm;
            identifierBoxes = new Dictionary<int, ComboBox>();
            elementBoxes = new Dictionary<int, ComboBox>();
            identifiers = new Dictionary<int, List<TextBox>>();
        }

        public StackPanel AddLinkElement()
        {
            // Link Element Panel
            StackPanel linkElement = new StackPanel();

            // Link Element Combobox Panel
            StackPanel linkElementComboboxPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5,5,0,0)
            };
            TextBlock textBlock = new TextBlock
            {
                Text = "Link Element:",
                Width = 80,
                Margin = new Thickness(5, 5, 0, 0)
            };
            ComboBox linkElementComboBox = new ComboBox
            {
                Name = "ElementBox" + identifierCounter,
                Margin = new Thickness(5,5,0,0),
                Width = 200
            };
            elementBoxes.Add(identifierCounter, linkElementComboBox);
            linkElementComboboxPanel.Children.Add(textBlock);
            linkElementComboboxPanel.Children.Add(linkElementComboBox);
            linkElement.Children.Add(linkElementComboboxPanel);
            foreach(KeyValuePair<string, CtDocument> pair in keyValueDocuments)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = pair.Key;
                linkElementComboBox.Items.Add(item);
            }

            // Identifier Combobox Panel
            StackPanel identifierTypeComboBoxPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 5, 0, 0)
            };
            TextBlock identifierTypeTextBlock = new TextBlock
            {
                Text = "Identifier Type:",
                Width = 80,
                Margin = new Thickness(5, 5, 0, 0)
            };
            ComboBox identifierTypeComboBox = new ComboBox();
            identifierTypeComboBox = CreateIdentifierComboBox();
            identifierTypeComboBoxPanel.Children.Add(identifierTypeTextBlock);
            identifierTypeComboBoxPanel.Children.Add(identifierTypeComboBox);
            linkElement.Children.Add(identifierTypeComboBoxPanel);

            // Delete Button
            Button deleteElementButton = new Button
            {
                Height = 10,
                Width = 10,
                Content = "x",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            deleteElementButton.Click += new RoutedEventHandler(ButtonDeleteLinkElement);
            linkElement.Children.Add(deleteElementButton);
            identifierCounter++;
            return linkElement;
        }

        public void ButtonDeleteLinkElement(object sender, RoutedEventArgs e)
        {
            var button = (FrameworkElement)sender;
            StackPanel linkElement = (StackPanel)button.Parent;
            StackPanel mainPanel = (StackPanel)linkElement.Parent;
            mainPanel.Children.Remove(linkElement);
        }

        public void ButtonAddLinkElement(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel panel = (StackPanel)button.Parent;
            StackPanel newLinkElem = AddLinkElement();
            panel.Children.Add(newLinkElem);
        }

        public void ButtonAddLink(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel parent = (StackPanel)button.Parent;
            StackPanel left = LeftLinkElemPanel;
            StackPanel right = RightLinkElemPanel;

            CtLinkset linkset = null;
            foreach (CtLinkset set in InformationContainer.Linksets)
            {
                if (set.FileName == LinksetComboBox.Text)
                {
                    linkset = set;
                }
            }
            List<StackPanel> leftLinkElementPanels = new List<StackPanel>();
            List<LsLinkElement> leftLinkElements = new List<LsLinkElement>();
            foreach(FrameworkElement elem in left.Children)
            {
                if(elem is StackPanel)
                {
                    leftLinkElementPanels.Add((StackPanel)elem);
                }
            }
            foreach(StackPanel linkElemetPanel in leftLinkElementPanels)
            {
                if(linkElemetPanel.Children[0] is StackPanel)
                { 
                    leftLinkElements.Add(CreateLinkElement((StackPanel)linkElemetPanel.Children[0], linkset));
                }
            }

            List<StackPanel> rightLinkElementPanel = new List<StackPanel>();
            List<LsLinkElement> rightLinkElements = new List<LsLinkElement>();
            foreach (FrameworkElement elem in right.Children)
            {
                if (elem is StackPanel)
                {
                    rightLinkElementPanel.Add((StackPanel)elem);
                }
            }
            foreach (StackPanel rightElemPanel in rightLinkElementPanel)
            {
                if(rightElemPanel.Children[0] is StackPanel)
                {
                    rightLinkElements.Add(CreateLinkElement((StackPanel)rightElemPanel.Children[0], linkset));
                }
            }
            switch (LinkTypeComboBox.Text)
            {
                case "Binary Link":
                    switch (LinkSubTypeComboBox.Text) {
                        case "Conflicts With":
                            linkset.CreateConflictsWith(leftLinkElements[0], rightLinkElements[0]);
                            break;
                        case "Is Alternative To":
                            linkset.CreateIsAlternativeTo(leftLinkElements[0], rightLinkElements[0]);
                            break;
                        case "Is Identical To":
                            linkset.CreateIsIdenticalTo(leftLinkElements[0], rightLinkElements[0]);
                            break;
                        default:
                            linkset.CreateBinaryLink(leftLinkElements[0], rightLinkElements[0]);
                            break;
                    }
                    break;
                case "Directed Binary Link":
                    linkset.CreateDirectedBinaryLink(leftLinkElements[0], rightLinkElements[0]);
                    break;
                case "Directed Link":
                    linkset.CreateDirectedLink(leftLinkElements, rightLinkElements);
                    break;
                case "Directed 1 To N Link":
                    switch (LinkSubTypeComboBox.Text)
                    {
                        case "Controls":
                            linkset.CreateControls(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Elaborates":
                            linkset.CreateElaborates(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Has Member":
                            linkset.CreateHasMember(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Has Part":
                            linkset.CreateHasPart(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Is Controlled By":
                            linkset.CreateIsControlledBy(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Is Elaborated By":
                            linkset.CreateIsElaboratedBy(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Is Member Of":
                            linkset.CreateIsMemberOf(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Is Part Of":
                            linkset.CreateIsPartOf(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Is Specialised As":
                            linkset.CreateIsSpecialisedAs(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Is Superseded By":
                            linkset.CreateIsSupersededBy(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Sepcialises":
                            linkset.CreateSpecialises(leftLinkElements[0], rightLinkElements);
                            break;
                        case "Supersedes":
                            linkset.CreateSupersedes(leftLinkElements[0], rightLinkElements);
                            break;
                        default:
                            linkset.CreateDirected1ToNLink(leftLinkElements[0], rightLinkElements);
                            break;
                    }
                    break;
                default:
                    break;
            }
            this.Close();
        }

        public LsLinkElement CreateLinkElement(StackPanel linkElemetPanel, CtLinkset linkset)
        {
            LsLinkElement result = null;
            int key = 0;
            foreach (FrameworkElement elem in linkElemetPanel.Children)
            {
                if (elem is ComboBox)
                {
                    StringBuilder number = new StringBuilder();
                    foreach(char ch in elem.Name)
                    {
                        if (char.IsDigit(ch))
                            number.Append(ch);
                    }
                    key = int.Parse(number.ToString());
                    string element = elementBoxes[key].Text;
                    CtDocument doc = keyValueDocuments[element];
                    string identifierType = identifierBoxes[key].Text;
                    List<TextBox> identifierInputs = new List<TextBox>();
                    identifierInputs = identifiers[key];
                    LsIdentifier lsIdentifier = null;
                    switch (identifierType)
                    {
                        case "URI Based Identifier":
                            string uristring = identifierInputs[0].Text;
                            Uri uri = new Uri(uristring);
                            lsIdentifier = linkset.CreateUriBasedIdentifier(uri);
                            break;
                        case "String Based Identifier":
                            string identifier = identifierInputs[0].Text;
                            string field = identifierInputs[1].Text;
                            lsIdentifier = linkset.CreateStringBasedIdentifier(identifier, field);
                            break;
                        case "Query Based Identifier":
                            string queryExpression = identifierInputs[0].Text;
                            string queryLang = identifierInputs[1].Text;
                            lsIdentifier = linkset.CreateQueryBasedIdentifier(queryExpression, queryLang);
                            break;
                    }
                    result = linkset.CreateLinkElement(doc, lsIdentifier);
                    break;
                }
            }
            return result;
        }
        public void LinkTypeBoxChanged(object sender, EventArgs e)
        {
            LinkSubTypeComboBox.Items.Clear();
            LeftLinkElementAdd.IsEnabled = true;
            RightLinkEelementAdd.IsEnabled = true;
            ComboBox box = (ComboBox)sender;
            linktype = box.Text;
            if (linktype == "Binary Link")
            {
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Conflicts With" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Is Alternative to" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Is Identical to" });
                LinkSubTypeComboBox.Visibility = Visibility.Visible;
            }
            else if(linktype == "Directed 1 to N Link") {
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Controls" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Elaborates" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Has Member" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Has Part" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Is Controlled By" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Is Elaborated By" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Is Member Of" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Is Part Of" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Is Specialised By" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Is Supersede By" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Specialises" });
                LinkSubTypeComboBox.Items.Add(new ComboBoxItem { Content = "Supersedes" });
                LinkSubTypeComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                LinkSubTypeComboBox.Visibility = Visibility.Hidden;
            }
        }

        public ComboBox CreateIdentifierComboBox()
        {
            ComboBox identifierTypeComboBox = new ComboBox
            {
                Name = "IdentifierBox"+ identifierCounter,
                Width = 200,
                Margin = new Thickness(5, 5, 0, 0)
            };
            identifierTypeComboBox.DropDownClosed += new EventHandler(IdentifierTypeBoxCloseDown);
            ObservableCollection<ComboBoxItem> cbItems = new ObservableCollection<ComboBoxItem>();
            cbItems.Add(new ComboBoxItem { Content = "URI Based Identifier" });
            cbItems.Add(new ComboBoxItem { Content = "String Based Identifier" });
            cbItems.Add(new ComboBoxItem { Content = "Query Based Identifier" });
            foreach (ComboBoxItem item in cbItems)
            {
                identifierTypeComboBox.Items.Add(item);
            }
            identifierBoxes.Add(identifierCounter, identifierTypeComboBox);
            return identifierTypeComboBox;
        }

        public void IdentifierTypeBoxCloseDown(object sender, EventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            int key = 0;
            foreach(var pair in identifierBoxes)
            {
                if(pair.Value == box)
                {
                    key = pair.Key;
                }
            }
            StackPanel Result = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            StackPanel Input1 = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            StackPanel Input2 = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            List<TextBox> elems = new List<TextBox>();
            string selectedItem = box.SelectedValue?.ToString();
            switch (selectedItem)
            {
                case "System.Windows.Controls.ComboBoxItem: URI Based Identifier":
                    Input1.Children.Add(new TextBlock { Width = 80, Text = "URI:" });
                    TextBox Uribox1 = new TextBox { Width = 100, Name = "URITextBox" };
                    Input1.Children.Add(Uribox1);
                    elems.Add(Uribox1);
                    break;
                case "System.Windows.Controls.ComboBoxItem: String Based Identifier":
                    Input1.Children.Add(new TextBlock { Width = 80, Text = "Identifier:" });
                    TextBox identifierBox2 = new TextBox { Width = 100, Name = "IdentigierTextBox" };
                    Input1.Children.Add(identifierBox2);
                    Input2.Children.Add(new TextBlock { Width = 80, Text = "Identifier Field:" });
                    TextBox identiferBox2 = new TextBox { Width = 100, Name = "IdentifierFieldTextBox" };
                    Input2.Children.Add(identiferBox2);
                    elems.Add(identiferBox2);
                    elems.Add(identifierBox2);
                    break;
                case "System.Windows.Controls.ComboBoxItem: Query Based Identifier":
                    Input1.Children.Add(new TextBlock { Width = 80, Text = "Query Expression:" });
                    TextBox qbBox1 = new TextBox { Width = 100, Name = "QueryExpressionTextBox" };
                    Input1.Children.Add(qbBox1);
                    Input2.Children.Add(new TextBlock { Width = 80, Text = "Query Language:" });
                    TextBox qbBox2 = new TextBox { Width = 100, Name = "QueryLanguageTextBox" };
                    Input2.Children.Add(qbBox2);
                    elems.Add(qbBox1);
                    elems.Add(qbBox2);
                    break;
                default:
                    break;
            }
            identifiers[key] = elems;
            Result.Children.Add(Input1);
            Result.Children.Add(Input2);
            StackPanel parent = new StackPanel();
            parent = (StackPanel)box.Parent;
            parent.Children.Clear();
            parent.Children.Add(box);
            parent.Children.Add(Result);
        }
    }    
}
