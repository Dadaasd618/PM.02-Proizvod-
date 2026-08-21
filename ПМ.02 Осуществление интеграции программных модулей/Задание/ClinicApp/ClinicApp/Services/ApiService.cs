using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClinicApp.Services
{
    public static class ApiService
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string API_URL = AppConfig.Icd10ApiUrl;

        // Константы для парсинга ответа
        private const int ResponseArrayLength = 4;
        private const int ResultsIndex = 3;
        private const int DescriptionIndex = 1;

        public static async Task<string> ValidateIcd10Code(string code)
        {
            try
            {
                string url = $"{API_URL}?terms={code}&maxTerms=1";
                var response = await client.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < ResponseArrayLength)
                    return null;

                int count = root[0].GetInt32();
                if (count == 0) return null;

                var resultsArray = root[ResultsIndex];
                if (resultsArray.GetArrayLength() == 0) return null;

                var firstResult = resultsArray[0];
                if (firstResult.GetArrayLength() < 2) return null;

                return firstResult[DescriptionIndex].GetString();
            }
            catch (HttpRequestException ex)
            {
                return $"NETWORK_ERROR: {ex.Message}";
            }
            catch (JsonException ex)
            {
                return $"JSON_ERROR: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
    }
}