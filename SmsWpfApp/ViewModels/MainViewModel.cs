using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmsWpfApp.Models;
using SmsWpfApp.Services;

namespace SmsWpfApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly EnvironmentVariableService _service;

        private readonly Dictionary<string, string> _originalValues = new();

        public ObservableCollection<EnvironmentVariable> Variables { get; }
        public IRelayCommand CloseCommand { get; }
        public IRelayCommand RefreshCommand { get; }

        public MainViewModel()
        {
            _service = new EnvironmentVariableService();
            Variables = new ObservableCollection<EnvironmentVariable>();

            CloseCommand = new RelayCommand(CloseWindow);
            RefreshCommand = new RelayCommand(RefreshVariables);

            LoadVariables();
        }

        private void CloseWindow()
        {
            Log.Information("Пользователь инициировал закрытие окна");
            Application.Current.MainWindow?.Close();
        }

        /// <summary>
        /// Первичная загрузка / Refresh
        /// </summary>
        private void LoadVariables()
        {
            try
            {
                IConfiguration configuration = App.Configuration;

                string[] names = configuration
                    .GetSection("EnvironmentVariables")
                    .Get<string[]>() ?? Array.Empty<string>();

                Variables.Clear();

                IReadOnlyCollection<EnvironmentVariable> loaded = _service.ReadVariables(names);

                foreach (EnvironmentVariable variable in loaded)
                {
                    Variables.Add(variable);
                }

                InitializeBaseline(Variables);

                Log.Information("Переменные среды загружены: {Count}", Variables.Count);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критическая ошибка при загрузке переменных среды");

                MessageBox.Show("Произошла ошибка при загрузке переменных среды.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Инициализация baseline (используется при старте и Refresh)
        /// </summary>
        private void InitializeBaseline(IEnumerable<EnvironmentVariable> variables)
        {
            _originalValues.Clear();

            foreach (EnvironmentVariable variable in variables)
            {
                _originalValues[variable.Name] = variable.Value;
            }
        }

        /// <summary>
        /// Refresh по кнопке
        /// </summary>
        private void RefreshVariables()
        {
            Log.Information("Обновление списка переменных по запросу пользователя");
            LoadVariables();
        }

        /// <summary>
        /// Сохранение переменной (при потере фокуса)
        /// </summary>
        public void SaveVariable(EnvironmentVariable variable)
        {
            if (!_originalValues.TryGetValue(variable.Name, out string originalValue))
                return;

            string currentValue = variable.Value;

            if (string.Equals(currentValue, originalValue, StringComparison.Ordinal))
                return;

            try
            {
                _service.SaveVariable(variable.Name, currentValue);

                Log.Information(
                    "Переменная среды изменена: {Name} = {Value}",
                    variable.Name,
                    currentValue);

                _originalValues[variable.Name] = currentValue;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Не удалось сохранить переменную среды: {Name}", variable.Name);

                // TODO можно показать message box

                variable.Value = originalValue;
            }
        }
    }
}
