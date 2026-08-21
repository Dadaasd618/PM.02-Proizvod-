using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ClinicApp.Data;

namespace ClinicApp.Windows
{
    public partial class DoctorWindow : Window
    {
        private string doctorName;

        public DoctorWindow(string name)
        {
            InitializeComponent();
            doctorName = name;
            TitleText.Text = "Панель врача";
            DoctorInfoText.Text = $"👨‍⚕️ {doctorName}";
            UpdateStatus($"Добро пожаловать, {doctorName}!");
            LoadMyAppointments_Click(null, null);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void LoadMyAppointments_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(doctorName))
            {
                UpdateStatus("Ошибка: имя врача не определено");
                return;
            }

            DataTable dt = DatabaseHelper.ExecuteQuery($@"
        SELECT 
            З.КодЗаказа, 
            П.ФИО AS Пациент, 
            У.Наименование AS Услуга, 
            З.ДатаОказания AS Дата,
            CASE 
                WHEN З.СтатусОплаты = 1 THEN 'Оплачено' 
                ELSE 'Ожидает' 
            END AS Статус,
            З.Сумма AS Сумма
        FROM ЗаказНаряды З
        LEFT JOIN Пациенты П ON З.КодПациента = П.КодПациента
        LEFT JOIN Услуги У ON З.КодУслуги = У.КодУслуги
        LEFT JOIN Врачи В ON З.КодВрача = В.КодВрача
        WHERE В.ФИО = '{doctorName}'
        ORDER BY З.ДатаОказания DESC");

            UpdateGrid(dt, "приёмов");
        }

        private void CompleteService_Click(object sender, RoutedEventArgs e)
        {
            // Открываем новое окно для оказания услуги
            CompleteServiceWindow completeWindow = new CompleteServiceWindow(doctorName);
            completeWindow.Owner = this;
            completeWindow.ShowDialog();
            LoadMyAppointments_Click(null, null);
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
            if (string.IsNullOrEmpty(doctorName))
            {
                UpdateStatus("Ошибка: имя врача не определено");
                return;
            }

            DataTable dt = DatabaseHelper.ExecuteQuery($@"
        SELECT 
            COUNT(*) AS ВсегоПриёмов,
            SUM(CASE WHEN СтатусОплаты = 1 THEN 1 ELSE 0 END) AS Оплаченных,
            SUM(CASE WHEN СтатусОплаты = 0 THEN 1 ELSE 0 END) AS Ожидающих,
            ISNULL(SUM(Сумма), 0) AS ОбщаяСумма
        FROM ЗаказНаряды З
        LEFT JOIN Врачи В ON З.КодВрача = В.КодВрача
        WHERE В.ФИО = '{doctorName}'");

            if (dt.Rows.Count > 0 && dt.Rows[0]["ВсегоПриёмов"] != DBNull.Value && Convert.ToInt32(dt.Rows[0]["ВсегоПриёмов"]) > 0)
            {
                var row = dt.Rows[0];
                string stats = $"Статистика врача:\n" +
                               $"Всего приёмов: {row["ВсегоПриёмов"]}\n" +
                               $"Оплаченных: {row["Оплаченных"]}\n" +
                               $"Ожидающих: {row["Ожидающих"]}\n" +
                               $"Общая сумма: {row["ОбщаяСумма"]} руб.";
                MessageBox.Show(stats, "Статистика", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("У вас пока нет приёмов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadMyAppointments_Click(null, null);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMyAppointments_Click(null, null);
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