

namespace Countries
{
    using System.Reflection;
    using System.Windows;

    /// <summary>
    /// Interaction logic for WinInfo.xaml
    /// </summary>
    public partial class WinInfo : Window
    {
        public WinInfo()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblCompilacao.Content = $"OS Build: { Assembly.GetExecutingAssembly().GetName().Version}";
            lblVersao.Content = "Version:  1.0.1";
            lblData.Content = "Date: 28/07/2020";
            lblDireitos.Content = "Edition of the MJ-Software system. All rights reserved.";
            lblAutor.Content = "Author: Sidney Major";
        }
    }
}
