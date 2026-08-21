using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using ClinicApp.Data;

namespace ClinicApp.Windows
{
    public partial class CompleteServiceWindow : Window
    {
        private string doctorName;

        public CompleteServiceWindow(string name)
        {
            InitializeComponent();
            doctorName = name;
            DoctorNameText.Text = doctorName;

            LoadOrders();
            OrderCombo.SelectionChanged += (s, e) => UpdateInfo();
        }

        private void LoadOrders()
        {
            DataTable orders = DatabaseHelper.ExecuteQuery($@"
                SELECT З.КодЗаказа, П.ФИО AS Пациент, У.Наименование AS Услуга,
                       FORMAT(З.ДатаОказания, 'dd.MM.yyyy HH:mm') AS Дата,
                       З.Сумма
                FROM ЗаказНаряды З
                JOIN Пациенты П ON З.КодПациента = П.КодПациента
                JOIN Услуги У ON З.КодУслуги = У.КодУслуги
                JOIN Врачи В ON З.КодВрача = В.КодВрача
                WHERE В.ФИО = '{doctorName}' AND З.СтатусОплаты = 0
                ORDER BY З.ДатаОказания");

            OrderCombo.Items.Clear();

            if (orders.Rows.Count == 0)
            {
                var item = new ComboBoxItem { Content = "Нет ожидающих приёмов", Tag = -1 };
                OrderCombo.Items.Add(item);
                OrderCombo.SelectedIndex = 0;
                InfoText.Text = "Нет приёмов, ожидающих подтверждения";
                return;
            }

            foreach (DataRow row in orders.Rows)
            {
                var item = new ComboBoxItem
                {
                    Content = $"№{row["КодЗаказа"]}: {row["Пациент"]} - {row["Услуга"]} ({row["Дата"]})",
                    Tag = row["КодЗаказа"]
                };
                OrderCombo.Items.Add(item);
            }
            OrderCombo.SelectedIndex = 0;
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            if (OrderCombo.SelectedItem == null) return;

            var item = (ComboBoxItem)OrderCombo.SelectedItem;
            int orderId = Convert.ToInt32(item.Tag);

            if (orderId == -1)
            {
                InfoText.Text = "Нет приёмов для подтверждения";
                return;
            }

            DataTable dt = DatabaseHelper.ExecuteQuery($@"
        SELECT П.ФИО AS Пациент, У.Наименование AS Услуга,
               З.ДатаОказания AS Дата,
               З.Сумма AS Сумма
        FROM ЗаказНаряды З
        JOIN Пациенты П ON З.КодПациента = П.КодПациента
        JOIN Услуги У ON З.КодУслуги = У.КодУслуги
        WHERE З.КодЗаказа = {orderId}");

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                InfoText.Text = $"Пациент: {row["Пациент"]}\n" +
                               $"Услуга: {row["Услуга"]}\n" +
                               $"Дата: {row["Дата"]}\n" +
                               $"Сумма: {row["Сумма"]} руб.";
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (OrderCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите приём!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var item = (ComboBoxItem)OrderCombo.SelectedItem;
            int orderId = Convert.ToInt32(item.Tag);

            if (orderId == -1)
            {
                MessageBox.Show("Нет приёмов для подтверждения!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Подтвердить оказание услуги?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string query = "UPDATE ЗаказНаряды SET СтатусОплаты = 1 WHERE КодЗаказа = @orderId";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@orderId", orderId)
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show($"✅ Услуга подтверждена! Заказ №{orderId}", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}