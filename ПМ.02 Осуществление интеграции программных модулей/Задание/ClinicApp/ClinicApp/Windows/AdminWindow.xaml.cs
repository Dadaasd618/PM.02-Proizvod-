using ClinicApp.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace ClinicApp.Windows
{
    public partial class AdminWindow : Window
    {
        private string currentTable = "";
        private DataTable currentData;

        public AdminWindow()
        {
            InitializeComponent();
            UpdateStatus("Добро пожаловать, Администратор!");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void LoadDoctors_Click(object sender, RoutedEventArgs e)
        {
            currentTable = "Врачи";
            currentData = DatabaseHelper.ExecuteQuery("SELECT * FROM Врачи");
            UpdateGrid(currentData);
            UpdateStatus($"Загружено врачей: {currentData.Rows.Count}");
        }

        private void LoadPatients_Click(object sender, RoutedEventArgs e)
        {
            currentTable = "Пациенты";
            currentData = DatabaseHelper.ExecuteQuery("SELECT * FROM Пациенты");
            UpdateGrid(currentData);
            UpdateStatus($"Загружено пациентов: {currentData.Rows.Count}");
        }

        private void LoadServices_Click(object sender, RoutedEventArgs e)
        {
            currentTable = "Услуги";
            currentData = DatabaseHelper.ExecuteQuery("SELECT * FROM Услуги");
            UpdateGrid(currentData);
            UpdateStatus($"Загружено услуг: {currentData.Rows.Count}");
        }

        private void LoadOrders_Click(object sender, RoutedEventArgs e)
        {
            currentTable = "ЗаказНаряды";
            currentData = DatabaseHelper.ExecuteQuery(@"
        SELECT 
            З.КодЗаказа, 
            П.ФИО AS Пациент, 
            В.ФИО AS Врач, 
            У.Наименование AS Услуга, 
            З.ДатаОказания AS Дата,
            CASE 
                WHEN З.СтатусОплаты = 1 THEN 'Оплачено' 
                ELSE 'Ожидает' 
            END AS Статус,
            З.Сумма AS Сумма
        FROM ЗаказНаряды З
        LEFT JOIN Пациенты П ON З.КодПациента = П.КодПациента
        LEFT JOIN Врачи В ON З.КодВрача = В.КодВрача
        LEFT JOIN Услуги У ON З.КодУслуги = У.КодУслуги
        ORDER BY З.ДатаОказания DESC");

            UpdateGrid(currentData);
            UpdateStatus($"Загружено заказ-нарядов: {currentData.Rows.Count}");
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentTable))
            {
                MessageBox.Show("Сначала выберите таблицу!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            switch (currentTable)
            {
                case "Пациенты":
                    new AddEditPatientWindow().ShowDialog();
                    break;
                case "Врачи":
                    new AddEditDoctorWindow().ShowDialog();
                    break;
                case "Услуги":
                    new AddEditServiceWindow().ShowDialog();
                    break;
                default:
                    MessageBox.Show("Для этой таблицы нет формы добавления", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
            RefreshData();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var row = ((DataRowView)DataGrid.SelectedItem).Row;
            int id = Convert.ToInt32(row[0]);

            switch (currentTable)
            {
                case "Пациенты":
                    new AddEditPatientWindow(
                        id,
                        row[1].ToString(),
                        Convert.ToDateTime(row[2]),
                        row[3]?.ToString() ?? "",
                        row[4]?.ToString() ?? "",
                        ""
                    ).ShowDialog();
                    break;
                case "Врачи":
                    new AddEditDoctorWindow(
                        id,
                        row[1].ToString(),
                        row[2].ToString(),
                        row[3]?.ToString() ?? "",
                        row[4] != DBNull.Value ? Convert.ToInt32(row[4]) : 0
                    ).ShowDialog();
                    break;
                case "Услуги":
                    new AddEditServiceWindow(
                        id,
                        row[1].ToString(),
                        Convert.ToDecimal(row[2])
                    ).ShowDialog();
                    break;
                default:
                    MessageBox.Show("Для этой таблицы нет формы редактирования", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
            RefreshData();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var row = ((DataRowView)DataGrid.SelectedItem).Row;
            int id = Convert.ToInt32(row[0]);

            if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    string tableName = currentTable;
                    string idColumnName = "";

                    switch (tableName)
                    {
                        case "Пациенты":
                            idColumnName = "КодПациента";
                            // Сначала удаляем связанные заказ-наряды
                            DatabaseHelper.ExecuteNonQuery($"DELETE FROM ЗаказНаряды WHERE КодПациента = {id}");
                            break;
                        case "Врачи":
                            idColumnName = "КодВрача";
                            DatabaseHelper.ExecuteNonQuery($"DELETE FROM ЗаказНаряды WHERE КодВрача = {id}");
                            break;
                        case "Услуги":
                            idColumnName = "КодУслуги";
                            DatabaseHelper.ExecuteNonQuery($"DELETE FROM ЗаказНаряды WHERE КодУслуги = {id}");
                            break;
                        case "ЗаказНаряды":
                            idColumnName = "КодЗаказа";
                            break;
                        default:
                            MessageBox.Show("Неизвестная таблица!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                    }

                    string query = $"DELETE FROM {tableName} WHERE {idColumnName} = @id";
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                new SqlParameter("@id", id)
                    };

                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    RefreshData();
                    UpdateStatus("Запись удалена!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            if (!string.IsNullOrEmpty(currentTable))
            {
                if (currentTable == "ЗаказНаряды")
                {
                    LoadOrders_Click(null, null);
                }
                else
                {
                    currentData = DatabaseHelper.ExecuteQuery($"SELECT * FROM {currentTable}");
                    UpdateGrid(currentData);
                    UpdateStatus($"Обновлено: {currentData.Rows.Count} записей");
                }
            }
        }

        private void UpdateGrid(DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                DataGrid.ItemsSource = null;
                CountText.Text = "Записей: 0";
                return;
            }
            DataGrid.ItemsSource = dt.DefaultView;
            CountText.Text = $"Записей: {dt.Rows.Count}";
        }

        private void UpdateStatus(string msg) => StatusText.Text = msg;
    }
}