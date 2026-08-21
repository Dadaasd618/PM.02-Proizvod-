using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using ClinicApp.Data;

namespace ClinicApp.Windows
{
    public partial class BookAppointmentWindow : Window
    {
        private string patientName;
        private int patientId;

        public BookAppointmentWindow(string name)
        {
            InitializeComponent();
            patientName = name;
            PatientNameText.Text = patientName;

            object id = DatabaseHelper.ExecuteScalar($"SELECT КодПациента FROM Пациенты WHERE ФИО = '{patientName}'");
            if (id == null || id == DBNull.Value)
            {
                MessageBox.Show("Пациент не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }
            patientId = Convert.ToInt32(id);

            LoadServices();
            LoadDoctors();
            DatePicker.SelectedDate = DateTime.Today.AddDays(1);
            if (TimeCombo.Items.Count > 0) TimeCombo.SelectedIndex = 0;
        }

        private void LoadServices()
        {
            DataTable services = DatabaseHelper.ExecuteQuery("SELECT КодУслуги, Наименование, Цена FROM Услуги");
            ServiceCombo.Items.Clear();
            foreach (DataRow row in services.Rows)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{row["Наименование"]} - {Convert.ToDecimal(row["Цена"]):0.00} руб.",
                    Tag = row["КодУслуги"]
                };
                ServiceCombo.Items.Add(item);
            }
            if (ServiceCombo.Items.Count > 0) ServiceCombo.SelectedIndex = 0;
        }

        private void LoadDoctors()
        {
            DataTable doctors = DatabaseHelper.ExecuteQuery("SELECT КодВрача, ФИО FROM Врачи");
            DoctorCombo.Items.Clear();
            foreach (DataRow row in doctors.Rows)
            {
                var item = new ComboBoxItem
                {
                    Content = row["ФИО"].ToString(),
                    Tag = row["КодВрача"]
                };
                DoctorCombo.Items.Add(item);
            }
            if (DoctorCombo.Items.Count > 0) DoctorCombo.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceCombo.SelectedItem == null || DoctorCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите услугу и врача!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int serviceId = Convert.ToInt32(((ComboBoxItem)ServiceCombo.SelectedItem).Tag);
            int doctorId = Convert.ToInt32(((ComboBoxItem)DoctorCombo.SelectedItem).Tag);

            DateTime date = DatePicker.SelectedDate ?? DateTime.Today.AddDays(1);
            string time = (TimeCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "09:00";
            DateTime dateTime = DateTime.Parse($"{date:yyyy-MM-dd} {time}");

            object priceObj = DatabaseHelper.ExecuteScalar($"SELECT Цена FROM Услуги WHERE КодУслуги = {serviceId}");
            decimal price = priceObj != null ? Convert.ToDecimal(priceObj) : 0;

            string query = @"
                INSERT INTO ЗаказНаряды (КодПациента, КодВрача, КодУслуги, ДатаОказания, СтатусОплаты, Сумма)
                VALUES (@patientId, @doctorId, @serviceId, @dateTime, 0, @price)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@patientId", patientId),
                new SqlParameter("@doctorId", doctorId),
                new SqlParameter("@serviceId", serviceId),
                new SqlParameter("@dateTime", dateTime),
                new SqlParameter("@price", price)
            };

            try
            {
                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("✅ Запись успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}