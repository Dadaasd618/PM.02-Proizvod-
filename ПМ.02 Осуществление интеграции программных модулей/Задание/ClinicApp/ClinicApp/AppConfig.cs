namespace ClinicApp
{
    public static class AppConfig
    {
        // Строка подключения к БД 
        public const string ConnectionString = "Server=.\\SQLEXPRESS;Database=ClinicDB;Integrated Security=True;";

        // API URL для проверки МКБ-10
        public const string Icd10ApiUrl = "https://clinicaltables.nlm.nih.gov/api/icd10cm/v3/search";
    }
}