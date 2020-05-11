
namespace Countries.Service
{
    using System.Windows;

    public class DialogService
    {
        /// <summary>
        /// Show message
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }
    }
}
