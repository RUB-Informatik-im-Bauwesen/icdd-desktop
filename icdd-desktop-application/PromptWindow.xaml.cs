using MahApps.Metro.Controls;
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

namespace icdd_parser_gui
{
    /// <summary>
    /// Interaction logic for PromptWindow.xaml
    /// </summary>

    partial class PromptWindow : MetroWindow
    {
        public PromptWindow(string question, string title, string defaultValue = "")
        {
            InitializeComponent();
            txtQuestion.Text = question;
            this.Title = title;
            txtResponse.Text = defaultValue;
            
        }

        public static string Prompt(string question, string title, string defaultValue = "")
        {
            PromptWindow inst = new PromptWindow(question, title, defaultValue);
            inst.ShowDialog();
            if (inst.DialogResult == true)
                return inst.txtResponse.Text;
            return null;
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
