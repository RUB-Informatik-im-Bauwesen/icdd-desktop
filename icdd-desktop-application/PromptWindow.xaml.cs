
using System;
using System.Collections.Generic;
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
    /// Interaction logic for PromptWindow.xaml
    /// </summary>

    partial class PromptWindow 
    { 
        /// <summary>
        /// Default Constructor for the Prompt Window.
        /// </summary>
        /// <param name="question">The question that shall be asked.</param>
        /// <param name="title">The title of the window.</param>
        /// <param name="defaultValue">The default text of the textbox.</param>
        public PromptWindow(string question, string title, string defaultValue = "")
        {
            InitializeComponent();
            txtQuestion.Text = question;
            this.Title = title;
            txtResponse.Text = defaultValue;
            
        }

        /// <summary>
        /// Optional Constructor for the Prompt Window with a suffix at the end.
        /// </summary>
        /// <param name="question">The question that shall be asked.</param>
        /// <param name="title">The title of the window.</param>
        /// <param name="defaultValue">The default text of the textbox.</param>
        /// <param name="suffix">The suffix that shall be displayed.</param>
        public PromptWindow(string question, string title, string suffix, string defaultValue = "")
        {
            InitializeComponent();
            txtQuestion.Text = question;
            this.Title = title;
            txtResponse.Text = defaultValue;
            suffixBox.Visibility = Visibility.Visible;
            suffixBox.Text = suffix;
            Width = this.Width + 75;
        }

        /// <summary>
        /// Optional Constructor for the Prompt Window wi
        /// </summary>
        /// <param name="question"></param>
        /// <param name="title">The title of the window</param>
        /// <param name="AdditionalBox">The additional Textbox to add</param>
        /// <param name="defaultValue"></param>
        /// <param name="AdditionalDefault"></param>
        public PromptWindow(string question, string title, TextBox AdditionalBox, string defaultValue="", string AdditionalDefault="")
        {
            InitializeComponent();
            optionalSuffix.Orientation = Orientation.Vertical;
            optionalSuffix.Children.Add(AdditionalBox);
            txtQuestion.Text = question;
            this.Title = title;
            txtResponse.Text = defaultValue;
            Height = this.Height + 50;
        }

        /// <summary>
        /// Function for the OK Button
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonSaveChanges(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Function for the Cancel Button
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonCancelChanges(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
