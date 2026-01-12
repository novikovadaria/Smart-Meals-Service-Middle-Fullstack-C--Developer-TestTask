using Serilog;
using SmsWpfApp.Models;

namespace SmsWpfApp.Services
{
    public class EnvironmentVariableService
    {
        /// <summary>
        /// Чтение переменных среды по именам
        /// </summary>
        public IReadOnlyCollection<EnvironmentVariable> ReadVariables(IEnumerable<string> variableNames)
        {
            List<EnvironmentVariable> result = new();

            foreach (string name in variableNames)
            {
                try
                {
                    string? value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);

                    result.Add(new EnvironmentVariable
                    {
                        Name = name,
                        Value = value ?? string.Empty
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка чтения переменной среды: {Name}", name);

                    result.Add(new EnvironmentVariable
                    {
                        Name = name,
                        Value = string.Empty
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Сохранение переменной среды
        /// </summary>
        public void SaveVariable(string name, string value)
        {
            try
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка сохранения переменной среды: {Name}", name);
                throw;
            }
        }
    }
}
