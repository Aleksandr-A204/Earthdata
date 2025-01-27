using Earthdata;

// Подключение к поисковому механизму LP DAAC

var earthdata = new EarthdataAccess();

// Вывод данных для Ставрополя на консоль
await earthdata.PrintDataParallel();

// Получение токена для аутентификации
//string token = earthdata.GetBearerToken();
// Получение списка задач
//earthdata.GetListTasks(token);