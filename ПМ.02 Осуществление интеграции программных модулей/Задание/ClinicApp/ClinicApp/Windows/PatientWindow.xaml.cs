using System;
using System.Data;
using System.Windows;
using ClinicApp.Data;

namespace ClinicApp.Windows
{
    public partial class PatientWindow : Window
    {
        private string patientName;

        public PatientWindow(string name)
        {
            InitializeComponent();
            patientName = name;
            TitleText.Text = "Панель пациента";
            PatientInfoText.Text = $"👤 {patientName}";
            UpdateStatus($"Добро пожаловать, {patientName}!");
            LoadMyServices_Click(null, null);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void LoadMyServices_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(patientName))
            {
                UpdateStatus("Ошибка: имя пациента не определено");
                return;
            }

            // ДОБАВЬТЕ ЭТУ СТРОЧКУ ДЛЯ ОТЛАДКИ
            UpdateStatus($"Поиск услуг для пациента: {patientName}");

            DataTable dt = DatabaseHelper.ExecuteQuery($@"
        SELECT 
            У.Наименование AS Услуга, 
            З.ДатаОказания AS Дата,
            В.ФИО AS Врач, 
            CASE 
                WHEN З.СтатусОплаты = 1 THEN 'Оплачено' 
                ELSE 'Ожидает' 
            END AS Статус,
            З.Сумма AS Сумма
        FROM ЗаказНаряды З
        LEFT JOIN Услуги У ON З.КодУслуги = У.КодУслуги
        LEFT JOIN Врачи В ON З.КодВрача = В.КодВрача
        LEFT JOIN Пациенты П ON З.КодПациента = П.КодПациента
        WHERE П.ФИО = '{patientName}'
        ORDER BY З.ДатаОказания DESC");

            UpdateGrid(dt, "услуг");
        }

        private void BookAppointment_Click(object sender, RoutedEventArgs e)
        {
            BookAppointmentWindow bookWindow = new BookAppointmentWindow(patientName);
            bookWindow.Owner = this;
            bookWindow.ShowDialog();
            LoadMyServices_Click(null, null);
        }

        private void LoadAllServices_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(@"
        SELECT 
            КодУслуги, 
            Наименование, 
            Цена AS Стоимость,
            (SELECT COUNT(*) FROM ЗаказНаряды WHERE КодУслуги = Услуги.КодУслуги) AS КоличествоОказаний
        FROM Услуги
        ORDER BY Наименование");

            UpdateGrid(dt, "всех услуг");
        }

        private void LoadStatistics_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(patientName))
            {
                UpdateStatus("Ошибка: имя пациента не определено");
                return;
            }

            DataTable dt = DatabaseHelper.ExecuteQuery($@"
        SELECT 
            COUNT(*) AS ВсегоУслуг,
            SUM(CASE WHEN СтатусОплаты = 1 THEN 1 ELSE 0 END) AS Оплаченных,
            SUM(CASE WHEN СтатусОплаты = 0 THEN 1 ELSE 0 END) AS Ожидающих,
            ISNULL(SUM(Сумма), 0) AS ОбщаяСумма
        FROM ЗаказНаряды З
        LEFT JOIN Пациенты П ON З.КодПациента = П.КодПациента
        WHERE П.ФИО = '{patientName}'");

            if (dt.Rows.Count > 0 && dt.Rows[0]["ВсегоУслуг"] != DBNull.Value && Convert.ToInt32(dt.Rows[0]["ВсегоУслуг"]) > 0)
            {
                var row = dt.Rows[0];
                string stats = $"Статистика пациента:\n" +
                               $"Всего услуг: {row["ВсегоУслуг"]}\n" +
                               $"Оплаченных: {row["Оплаченных"]}\n" +
                               $"Ожидающих: {row["Ожидающих"]}\n" +
                               $"Общая сумма: {row["ОбщаяСумма"]} руб.";
                MessageBox.Show(stats, "Статистика", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("У вас пока нет оказанных услуг.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadMyServices_Click(null, null);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMyServices_Click(null, null);
        }

        private void UpdateGrid(DataTable dt, string type)
        {
            if (dt.Rows.Count == 0)
            {
                DataGrid.ItemsSource = null;
                UpdateStatus($"Нет {type} для отображения");
                CountText.Text = "Записей: 0";
                return;
            }

            DataGrid.ItemsSource = dt.DefaultView;
            UpdateStatus($"Загружено {type}: {dt.Rows.Count}");
            CountText.Text = $"Записей: {dt.Rows.Count}";
        }

        private void UpdateStatus(string msg) => StatusText.Text = msg;
    }
}