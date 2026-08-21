using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClinicApp.IntegrationTests
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string API_URL = "https://clinicaltables.nlm.nih.gov/api/icd10cm/v3/search";

        static async Task Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("ИНТЕГРАЦИОННОЕ ТЕСТИРОВАНИЕ API");
            Console.WriteLine("Система учёта платных медицинских услуг");
            Console.WriteLine("========================================\n");

            int passed = 0;
            int failed = 0;

            // Тест 1: Проверка валидного кода МКБ-10
            if (await TestValidIcdCode("E11.9")) passed++; else failed++;

            // Тест 2: Проверка другого валидного кода
            if (await TestValidIcdCode("I10")) passed++; else failed++;

            // Тест 3: Проверка несуществующего кода
            if (await TestInvalidIcdCode("XXXXX")) passed++; else failed++;

            // Тест 4: Проверка пустого ввода
            if (await TestEmptyCode()) passed++; else failed++;

            // Тест 5: Проверка специального символа
            if (await TestInvalidIcdCode("")) passed++; else failed++;

            Console.WriteLine("\n========================================");
            Console.WriteLine($"РЕЗУЛЬТАТЫ ТЕСТИРОВАНИЯ");
            Console.WriteLine($"✅ Пройдено: {passed}");
            Console.WriteLine($"❌ Не пройдено: {failed}");
            Console.WriteLine($"📊 Всего: {passed + failed}");
            Console.WriteLine("========================================");
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // ============================================================
        // ТЕСТ 1-2: Проверка валидного кода МКБ-10
        // ============================================================
        static async Task<bool> TestValidIcdCode(string code)
        {
            Console.Write($"🔍 Тест: Проверка кода '{code}'... ");

            try
            {
                string url = $"{API_URL}?terms={code}&maxTerms=1";
                var response = await client.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 4)
                {
                    Console.WriteLine("❌ FAIL (неверный формат ответа)");
                    return false;
                }

                int count = root[0].GetInt32();
                if (count == 0)
                {
                    Console.WriteLine("❌ FAIL (код не найден)");
                    return false;
                }

                var resultsArray = root[3];
                if (resultsArray.GetArrayLength() == 0)
                {
                    Console.WriteLine("❌ FAIL (нет результатов)");
                    return false;
                }

                var firstResult = resultsArray[0];
                if (firstResult.GetArrayLength() < 2)
                {
                    Console.WriteLine("❌ FAIL (неверный формат результата)");
                    return false;
                }

                string description = firstResult[1].GetString();
                Console.WriteLine($"✅ PASS (Описание: {description})");
                return true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ FAIL (Ошибка сети: {ex.Message})");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAIL ({ex.Message})");
                return false;
            }
        }

        // ============================================================
        // ТЕСТ 3: Проверка несуществующего кода
        // ============================================================
        static async Task<bool> TestInvalidIcdCode(string code)
        {
            Console.Write($"🔍 Тест: Проверка несуществующего кода '{code}'... ");

            try
            {
                string url = $"{API_URL}?terms={code}&maxTerms=1";
                var response = await client.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 4)
                {
                    Console.WriteLine("❌ FAIL (неверный формат ответа)");
                    return false;
                }

                int count = root[0].GetInt32();
                if (count == 0)
                {
                    Console.WriteLine("✅ PASS (код не найден)");
                    return true;
                }

                Console.WriteLine("❌ FAIL (код найден, хотя не должен был)");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAIL ({ex.Message})");
                return false;
            }
        }

        // ============================================================
        // ТЕСТ 4: Проверка пустого ввода
        // ============================================================
        static async Task<bool> TestEmptyCode()
        {
            Console.Write($"🔍 Тест: Проверка пустого ввода... ");

            try
            {
                string url = $"{API_URL}?terms=&maxTerms=1";
                var response = await client.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 4)
                {
                    Console.WriteLine("❌ FAIL (неверный формат ответа)");
                    return false;
                }

                int count = root[0].GetInt32();
                if (count == 0)
                {
                    Console.WriteLine("✅ PASS (пустой ввод обработан)");
                    return true;
                }

                Console.WriteLine("❌ FAIL (пустой ввод вернул результаты)");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAIL ({ex.Message})");
                return false;
            }
        }
    }
}