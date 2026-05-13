using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using LaboratoryMD.Helpers;
namespace LaboratoryMD.Services
{
    public class ApiClient
    {
        private static readonly HttpClient _client = new HttpClient();

        public static void SetAuthToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _client.GetAsync(AppSettings.ApiBaseUrl + endpoint);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Сессия истекла. Пожалуйста, войдите заново.");
                }

                throw new Exception($"API Error: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка подключения к серверу: {ex.Message}");
            }
        }

        public static async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(AppSettings.ApiBaseUrl + endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            throw new Exception($"API Error: {response.StatusCode} - {responseContent}");
        }

        public static async Task<bool> DeleteAsync(string endpoint)
        {
            var response = await _client.DeleteAsync(AppSettings.ApiBaseUrl + endpoint);
            return response.IsSuccessStatusCode;
        }
    }
}
