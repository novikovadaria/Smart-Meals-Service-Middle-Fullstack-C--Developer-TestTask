using SmsWpfApp.Models;
using SmsWpfApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmsWpfApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        /// <summary>
        /// Сохранение переменной при окончании редактирования строки (пропал фокус)
        /// </summary>
        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (DataContext is not MainViewModel vm)
                return;

            if (e.EditAction != DataGridEditAction.Commit)
                return;

            if (e.Row.Item is not EnvironmentVariable variable)
                return;

            vm.SaveVariable(variable);
        }
    }
}
