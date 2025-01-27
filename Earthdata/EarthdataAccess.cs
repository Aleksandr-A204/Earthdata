using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Earthdata
{
    public class EarthdataAccess
    {
        // private string url = "https://urs.earthdata.nasa.gov";
        private string username = "aleksandr_ponomarev";
        private string password = "F";

        public EarthdataAccess() { }

        public async Task PrintDataParallel()
        {
            // Определяем ресурсы для 2023 и 2024 года
            string tempolar2023 = "2023-01-01T00:00:00Z,2023-12-31T23:59:59Z";
            string tempolar2024 = "2024-01-01T00:00:00Z,2024-12-31T23:59:59Z";

            // Запускаем загрузку данных одновременно для обоих годов
            var task2023 = PrintDataAsync(tempolar2023);
            var task2024 = PrintDataAsync(tempolar2024);

            await Task.WhenAll(task2023, task2024);
        }

        // Метод для загрузки данных гранул для заданного временного интервала
        public async Task PrintDataAsync(string tempolar)
        {
            string resource = $"https://cmr.earthdata.nasa.gov/search/granules.json?collection_concept_id=C2021957295-LPCLOUD&page_size=2000&temporal[]={tempolar}";

            try
            {
                // Execute the request
                var request = (HttpWebRequest)WebRequest.Create(resource);
                request.Method = "GET";

                var response = (HttpWebResponse)await request.GetResponseAsync();

                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string responseText = await reader.ReadToEndAsync();

                    Console.WriteLine($"Response received for tempolar {tempolar}. Length: {response.ContentLength}, Type: {response.ContentType}");

                    var jsonResponse = JObject.Parse(responseText);
                    Console.WriteLine($"Response data: {jsonResponse}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in HttpResponse: " + ex.Message);
            }
        }

        // Получает Bearer токен для аутентификации
        public string GetBearerToken()
        {
            try
            {
                // Ideally the cookie container will be persisted to/from file
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

                // Execute the request
                var request = (HttpWebRequest)WebRequest.Create("https://appeears.earthdatacloud.nasa.gov/api/login");
                request.Method = "POST";
                request.ContentLength = 0;
                request.Headers["Authorization"] = $"Basic {credentials}";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        string responseText = reader.ReadToEnd();

                        // Парсинг JSON-ответа и извлечение токена Bearer
                        var jsonResponse = JObject.Parse(responseText);
                        string bearerToken = jsonResponse["token"].ToString();
                        return bearerToken;
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public void GetListTasks(string token)
        {
            try
            {
                // Выполнение HTTP-запроса к API для получения задач
                var request = (HttpWebRequest)WebRequest.Create("https://appeears.earthdatacloud.nasa.gov/api/task");
                request.Method = "GET";
                request.Headers["Authorization"] = $"Bearer  {token}";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        string responseText = reader.ReadToEnd();
                        Console.WriteLine($"Response reveived. Length: {response.ContentLength}, Type: {response.ContentType}");
                        Console.WriteLine($"Response data: {responseText}");
                    }
                }
            }
            catch (WebException webEx)
            {
                // Обработка веб-исключений с выводом ответа об ошибке
                using (var stream = webEx.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string errorResponse = reader.ReadToEnd();
                    Console.WriteLine("Web Error: " + errorResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
