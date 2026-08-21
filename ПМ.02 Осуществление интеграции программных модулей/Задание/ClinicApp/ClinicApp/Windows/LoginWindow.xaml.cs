using ClinicApp.Data;
using ClinicApp.Services;
using System.Windows;

namespace ClinicApp.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            // Проверка подключения к БД
            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте SQL Server.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "Введите логин и пароль!";
                return;
            }

            var result = AuthService.Authenticate(login, password);

            if (!result.Success)
            {
                ErrorText.Text = "Неверный логин или пароль!";
                return;
            }

            switch (result.Role)
            {
                case "Администратор":
                    new AdminWindow().Show();
                    break;
                case "Врач":
                    new DoctorWindow(result.FullName).Show();
                    break;
                case "Пациент":
                    new PatientWindow(result.FullName).Show();
                    break;
                default:
                    MessageBox.Show("Неизвестная роль!");
                    return;
            }

            this.Close();
        }
    }
}