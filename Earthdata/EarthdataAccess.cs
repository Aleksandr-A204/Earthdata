using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Earthdata
{
    public class EarthdataAccess
    {
        private string url = "https://urs.earthdata.nasa.gov";
        private string username = "aleksandr_ponomarev";
        private string password = "F";

        public EarthdataAccess() { }

        //Выводит данные гранул для заданного временного интервала и области
        public void PrintData()
        {
            var response = HttpResponse("https://cmr.earthdata.nasa.gov/search/granules.json?collection_concept_id=C2021957295-LPCLOUD&temporal[]=2024-07-23T00:00:00Z,2025-01-23T23:59:59Z&bounding_box[]=41.4146,42.618,44.5635,45.6569");

            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string responseText = reader.ReadToEnd();

                Console.WriteLine($"Response reveived. Length: {response.ContentLength}, Type: {response.ContentType}");

                var jsonResponse = JObject.Parse(responseText);
                Console.WriteLine($"Response data: {jsonResponse}"); // Отображение полученных данных в консоли
            }
        }

        // Выполняет HTTP-запрос к указанному ресурсу и возвращает ответ
        private HttpWebResponse HttpResponse(string? resource = null)
        {
            try
            {
                // Ideally the cookie container will be persisted to/from file
                var myContainer = new CookieContainer();

                // Create a credential cache for authenticating when redirected to Earthdata Login
                var cache = new CredentialCache();
                cache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));

                // Execute the request
                var request = (HttpWebRequest)WebRequest.Create(resource);
                request.Method = "GET";
                request.Credentials = cache;
                request.CookieContainer = myContainer;
                request.PreAuthenticate = false;
                request.AllowAutoRedirect = true;

                return (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in HttpResponse: " + ex.Message);
                return null;
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
