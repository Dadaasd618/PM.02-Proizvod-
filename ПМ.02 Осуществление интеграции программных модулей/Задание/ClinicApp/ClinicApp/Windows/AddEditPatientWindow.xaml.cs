using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using ClinicApp.Data;
using ClinicApp.Helpers;

namespace ClinicApp.Windows
{
    public partial class AddEditPatientWindow : Window
    {
        private int? editId = null;

        public AddEditPatientWindow()
        {
            InitializeComponent();
            TitleText.Text = "Добавление пациента";
            BirthDatePicker.SelectedDate = DateTime.Now.AddYears(-20);
            LoginBox.Text = GenerateLogin();
            PasswordBox.Password = "user123";
            SetupInputValidation();
        }

        public AddEditPatientWindow(int id, string fullName, DateTime birthDate, string policy, string phone, string login)
        {
            InitializeComponent();
            editId = id;
            TitleText.Text = "Редактирование пациента";
            FullNameBox.Text = fullName;
            BirthDatePicker.SelectedDate = birthDate;
            PolicyBox.Text = policy;
            PhoneBox.Text = phone;
            LoginBox.Text = login;
            PasswordBox.Password = "user123";
            SetupInputValidation();
        }

        private void SetupInputValidation()
        {
            PhoneBox.TextChanged += (s, e) =>
            {
                string text = PhoneBox.Text;
                string digits = new string(text.Where(char.IsDigit).ToArray());
                if (digits.Length > 11) digits = digits.Substring(0, 11);

                string formatted = "";
                if (digits.Length > 0)
                {
                    formatted = "+7 ";
                    if (digits.Length > 1)
                        formatted += "(" + digits.Substring(1, Math.Min(3, digits.Length - 1));
                    if (digits.Length > 4)
                        formatted += ") " + digits.Substring(4, Math.Min(3, digits.Length - 4));
                    if (digits.Length > 7)
                        formatted += "-" + digits.Substring(7, Math.Min(2, digits.Length - 7));
                    if (digits.Length > 9)
                        formatted += "-" + digits.Substring(9, Math.Min(2, digits.Length - 9));
                }

                if (PhoneBox.Text != formatted)
                {
                    PhoneBox.Text = formatted;
                    PhoneBox.CaretIndex = formatted.Length;
                }
            };

            PolicyBox.TextChanged += (s, e) =>
            {
                string text = PolicyBox.Text;
                string digits = new string(text.Where(char.IsDigit).ToArray());
                if (digits.Length > 16) digits = digits.Substring(0, 16);
                if (PolicyBox.Text != digits)
                {
                    PolicyBox.Text = digits;
                    PolicyBox.CaretIndex = digits.Length;
                }
            };
        }

        private string GenerateLogin()
        {
            return "patient_" + new Random().Next(1000, 9999).ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameBox.Text.Trim();
            DateTime? birthDate = BirthDatePicker.SelectedDate;
            string policy = PolicyBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;

            if (!ValidationHelper.IsValidName(fullName))
            {
                UIService.ShowError(ErrorText, "Заполните ФИО!");
                return;
            }

            if (!birthDate.HasValue)
            {
                UIService.ShowError(ErrorText, "Выберите дату рождения!");
                return;
            }

            if (!string.IsNullOrEmpty(policy) && !ValidationHelper.IsValidPolicy(policy))
            {
                UIService.ShowError(ErrorText, "Полис должен содержать 16 цифр!");
                return;
            }

            try
            {
                if (editId.HasValue)
                {
                    string query = @"
                        UPDATE Пациенты 
                        SET ФИО = @fullName, ДатаРождения = @birthDate, 
                            Полис = @policy, Телефон = @phone
                        WHERE КодПациента = @id";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@fullName", fullName),
                        new SqlParameter("@birthDate", birthDate.Value),
                        new SqlParameter("@policy", policy ?? (object)DBNull.Value),
                        new SqlParameter("@phone", phone ?? (object)DBNull.Value),
                        new SqlParameter("@id", editId.Value)
                    };

                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    UIService.ShowSuccess("Пациент обновлён!");
                }
                else
                {
                    string insertPatient = @"
                        INSERT INTO Пациенты (ФИО, ДатаРождения, Полис, Телефон)
                        VALUES (@fullName, @birthDate, @policy, @phone);
                        SELECT SCOPE_IDENTITY();";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@fullName", fullName),
                        new SqlParameter("@birthDate", birthDate.Value),
                        new SqlParameter("@policy", policy ?? (object)DBNull.Value),
                        new SqlParameter("@phone", phone ?? (object)DBNull.Value)
                    };

                    object result = DatabaseHelper.ExecuteScalar(insertPatient, parameters);

                    if (result == null || result == DBNull.Value)
                    {
                        UIService.ShowError(ErrorText, "Не удалось получить ID пациента!");
                        return;
                    }

                    int patientId = Convert.ToInt32(result);

                    string insertUser = @"
                        INSERT INTO Пользователи (Логин, ПарольHash, Роль, КодПациента)
                        VALUES (@login, @password, 'Пациент', @patientId)";

                    SqlParameter[] userParams = new SqlParameter[]
                    {
                        new SqlParameter("@login", login),
                        new SqlParameter("@password", password),
                        new SqlParameter("@patientId", patientId)
                    };

                    DatabaseHelper.ExecuteNonQuery(insertUser, userParams);

                    MessageBox.Show($"Пациент добавлен!\nЛогин: {login}\nПароль: {password}",
                                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                UIService.ShowError(ErrorText, $"Ошибка: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}