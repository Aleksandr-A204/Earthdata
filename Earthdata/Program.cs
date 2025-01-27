using Earthdata;

var earthdata = new EarthdataAccess();

// Подключение к поисковому механизму LP DAAC

// Вывод данных для Ставрополя на консоль
earthdata.PrintData();

// Получение токена для аутентификации
//string token = earthdata.GetBearerToken();
// Получение списка задач
//earthdata.GetListTasks(token);