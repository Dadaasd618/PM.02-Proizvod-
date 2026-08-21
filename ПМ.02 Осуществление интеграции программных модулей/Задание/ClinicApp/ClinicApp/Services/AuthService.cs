using System.Data.SqlClient;
using ClinicApp.Data;

namespace ClinicApp.Services
{
    public static class AuthService
    {
        public static (bool Success, string Role, string FullName) Authenticate(string login, string password)
        {
            string query = @"
                SELECT 
                    Роль,
                    CASE 
                        WHEN Роль = 'Администратор' THEN 'Администратор'
                        WHEN Роль = 'Врач' THEN (SELECT ФИО FROM Врачи WHERE КодВрача = Пользователи.КодВрача)
                        WHEN Роль = 'Пациент' THEN (SELECT ФИО FROM Пациенты WHERE КодПациента = Пользователи.КодПациента)
                        ELSE 'Пользователь'
                    END AS FullName
                FROM Пользователи
                WHERE Логин = @login AND ПарольHash = @password";

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string role = reader["Роль"].ToString();
                                string fullName = reader["FullName"]?.ToString() ?? login;

                                // Если ФИО не найдено (NULL), используем логин
                                if (string.IsNullOrEmpty(fullName) || fullName == "Пользователь")
                                {
                                    fullName = login;
                                }

                                return (true, role, fullName);
                            }
                        }
                    }
                }
            }
            catch
            {
                return (false, null, null);
            }
            return (false, null, null);
        }
    }
}