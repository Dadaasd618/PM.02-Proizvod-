using ClinicApp.Data;
using ClinicApp.Services;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Threading.Tasks;

namespace ClinicApp.Windows
{
    public partial class AddEditServiceWindow : Window
    {
        private int? editId = null;

        public AddEditServiceWindow()
        {
            InitializeComponent();
            TitleText.Text = "Добавление услуги";
        }

        public AddEditServiceWindow(int id, string name, decimal price)
        {
            InitializeComponent();
            editId = id;
            TitleText.Text = "Редактирование услуги";
            NameBox.Text = name;
            PriceBox.Text = price.ToString("0.00");
        }

        // Проверка кода МКБ-10 через работающий API
        private async void CheckIcdCode_Click(object sender, RoutedEventArgs e)
        {
            string code = IcdCodeBox.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                IcdResultText.Text = "Введите код!";
                IcdResultText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                return;
            }

            IcdResultText.Text = "⏳ Проверка...";
            IcdResultText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);

            string result = await ApiService.ValidateIcd10Code(code);

            if (result == null)
            {
                IcdResultText.Text = "❌ Код не найден";
                IcdResultText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else if (result.StartsWith("NETWORK_ERROR") || result.StartsWith("JSON_ERROR") || result.StartsWith("ERROR"))
            {
                IcdResultText.Text = $"⚠️ {result}";
                IcdResultText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
            }
            else
            {
                IcdResultText.Text = $"✅ {result}";
                IcdResultText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            string priceText = PriceBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                ErrorText.Text = "Введите название услуги!";
                return;
            }

            if (!decimal.TryParse(priceText.Replace('.', ','), out decimal price) || price < 0)
            {
                ErrorText.Text = "Введите корректную цену!";
                return;
            }

            try
            {
                if (editId.HasValue)
                {
                    string query = @"
                        UPDATE Услуги 
                        SET Наименование = @name, Цена = @price
                        WHERE КодУслуги = @id";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@name", name),
                        new SqlParameter("@price", price),
                        new SqlParameter("@id", editId.Value)
                    };

                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("✅ Услуга обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string query = @"
                        INSERT INTO Услуги (Наименование, Цена)
                        VALUES (@name, @price)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@name", name),
                        new SqlParameter("@price", price)
                    };

                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("✅ Услуга добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ErrorText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}