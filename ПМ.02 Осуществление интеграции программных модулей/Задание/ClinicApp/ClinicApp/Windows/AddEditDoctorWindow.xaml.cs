using System;
using System.Windows;
using System.Data.SqlClient;
using ClinicApp.Data;

namespace ClinicApp.Windows
{
    public partial class AddEditDoctorWindow : Window
    {
        private int? editId = null;

        public AddEditDoctorWindow()
        {
            InitializeComponent();
            TitleText.Text = "Добавление врача";
        }

        public AddEditDoctorWindow(int id, string fullName, string specialty, string room, int departmentId)
        {
            InitializeComponent();
            editId = id;
            TitleText.Text = "Редактирование врача";
            FullNameBox.Text = fullName;
            SpecialtyBox.Text = specialty;
            RoomBox.Text = room;
            DepartmentBox.Text = departmentId > 0 ? departmentId.ToString() : "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameBox.Text.Trim();
            string specialty = SpecialtyBox.Text.Trim();
            string room = RoomBox.Text.Trim();
            string departmentText = DepartmentBox.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(specialty))
            {
                MessageBox.Show("Заполните ФИО и специальность!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (editId.HasValue)
                {
                    string query = @"
                        UPDATE Врачи 
                        SET ФИО = @fullName, Специальность = @specialty, 
                            Кабинет = @room, КодОтделения = @departmentId
                        WHERE КодВрача = @id";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@fullName", fullName),
                        new SqlParameter("@specialty", specialty),
                        new SqlParameter("@room", room),
                        new SqlParameter("@departmentId", string.IsNullOrEmpty(departmentText) ? (object)DBNull.Value : Convert.ToInt32(departmentText)),
                        new SqlParameter("@id", editId.Value)
                    };

                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("✅ Врач обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string query = @"
                        INSERT INTO Врачи (ФИО, Специальность, Кабинет, КодОтделения)
                        VALUES (@fullName, @specialty, @room, @departmentId)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@fullName", fullName),
                        new SqlParameter("@specialty", specialty),
                        new SqlParameter("@room", room),
                        new SqlParameter("@departmentId", string.IsNullOrEmpty(departmentText) ? (object)DBNull.Value : Convert.ToInt32(departmentText))
                    };

                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("✅ Врач добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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