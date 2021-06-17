using ICDDToolkitLibrary.Model.Container;
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
    public class Party
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPartySelected { get; set; }
    }
    /// <summary>
    /// Interaction logic for ManageParties.xaml
    /// </summary>
    public partial class ManageParties : Window
    {
        ObservableCollection<Party> parties = new ObservableCollection<Party>();
        List<CtParty> ctParties = new List<CtParty>();
        public ManageParties(CtContainerDescription description)
        {
            ctParties = description.Container.Parties;
            InitializeComponent(); 
            foreach (CtParty party in ctParties)
            {
                parties.Add(new Party { Type = party.GetType().Name.Replace("Ct", ""), Id = party.Guid, Name = party.Name, Description = party.Description, });
            }
            PartyDataGrid.ItemsSource = parties;
        }

        /// <summary>
        /// Button Function for editing a Party.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void ButtonEditParty(object sender, RoutedEventArgs e)
        {
            string TempId = ((Button)sender).Tag.ToString();
            Party EditParty = null;
            CtParty EditCtParty = null;
            foreach (Party party in parties)
            {
                if (TempId == party.Id)
                    EditParty = party;
                foreach (CtParty CTparty in ctParties)
                {
                    if (TempId == CTparty.Guid)
                    {
                        CTparty.Description = party.Description;
                        CTparty.Name = party.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Button Function for clicking "OK".
        /// </summary>
        /// <param name="sener">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void ButtonOK(object sener, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Function to get the active Party of a Container.
        /// </summary>
        /// <returns>Returns athe active Party if one is selected. Returns null otherwise.</returns>
        public CtParty GetSelectedParty()
        {
            CtParty SelectedParty = null;
            foreach(Party party in parties)
            {
                if(party.IsPartySelected == true)
                {
                    var temp = party.Id;
                    foreach(CtParty ctParty in ctParties)
                    {
                        if(ctParty.Guid == temp)
                        {
                            SelectedParty = ctParty;
                        } 
                    }
                }

            }
            return SelectedParty;
        }
    }
}
