using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StivTaskConsole
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ShowDetails : Window
    {
        public ShowDetails(TaskConsoleWCFService.Jobber JobToView)
        {
            InitializeComponent();




            string windowtitle = JobToView.Target + ":  " + JobToView.Taskname;
            ShowDetailsWindow.Title = windowtitle;
            DetailsGrid.ItemsSource = JobToView.taskdetails;

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
