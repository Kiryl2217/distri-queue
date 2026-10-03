// ====================================================
//					КЛИЕНТ (Продюсер)
// ====================================================

using System.Net.Sockets;

// Массив байтов.
byte[] bytes = new byte[100];

string? stringClientId = string.Empty; // int
string? stringClientCommandNumber = string.Empty; // int 0-3
string? stringClientTaskNumber = string.Empty; // int 0-5
string? stringClientSubscribe = string.Empty; // int 0/1

int clientId = -1;
int clientCommandNumber = -1;
int clientTaskNumber = -1;
int clientSubscribeNumber = -1;

bool isDataCorrect = false;

while (!isDataCorrect)
{
    stringClientId = Console.ReadLine();
    stringClientCommandNumber = Console.ReadLine();
    stringClientTaskNumber = Console.ReadLine();
    stringClientSubscribe = Console.ReadLine();

    isDataCorrect = CheckData(stringClientId, stringClientCommandNumber,
        stringClientTaskNumber, stringClientSubscribe,
        out clientId, out clientCommandNumber,
        out clientTaskNumber, out clientSubscribeNumber,
        out string errText);

    if (!isDataCorrect)
    {
        Console.WriteLine(errText);
    }
    else
    {
        Console.WriteLine("Введенные данные корректны.");
        isDataCorrect = true;
    }
}

bool CheckData(string? id, string? command, string? task, string? subscribe,
    out int clientId, out int commandNumber, out int taskNumber,
    out int subscribeNumber, out string errText)
{
    bool err = false;
    errText = string.Empty;
    clientId = -1;
    commandNumber = -1;
    taskNumber = -1;
    subscribeNumber = -1;

    // Проверка Id.
    if (!err && string.IsNullOrWhiteSpace(id))
    {
        err = true;
        errText = "Ошибка в Id.";
    }

    if (!err && !int.TryParse(id, out clientId))
    {
        err = true;
        errText = "Ошибка в Id.";
    }

    if (!err && clientId < 1)
    {
        err = true;
        errText = "Ошибка в Id.";
    }

    // Проверка Команды.
    if (!err && string.IsNullOrWhiteSpace(command))
    {
        err = true;
        errText = "Ошибка в Команде.";
    }

    if (!err && !int.TryParse(command, out commandNumber))
    {
        err = true;
        errText = "Ошибка в Команде.";
    }

    if (!err && (commandNumber < 0 || commandNumber > 3))
    {
        err = true;
        errText = "Ошибка в Команде.";
    }

    // Проверка Задачи.
    if (!err && string.IsNullOrWhiteSpace(task))
    {
        err = true;
        errText = "Ошибка в Задаче.";
    }

    if (!err && !int.TryParse(task, out taskNumber))
    {
        err = true;
        errText = "Ошибка в Задаче.";
    }

    if (!err && (taskNumber < 0 || taskNumber > 5))
    {
        err = true;
        errText = "Ошибка в Задаче.";
    }

    // Проверка Подписки.
    if (!err && string.IsNullOrWhiteSpace(subscribe))
    {
        err = true;
        errText = "Ошибка в Подписке.";
    }

    if (!err && !int.TryParse(subscribe, out subscribeNumber))
    {
        err = true;
        errText = "Ошибка в Подписке.";
    }

    if (!err && (subscribeNumber < 0 || subscribeNumber > 1))
    {
        err = true;
        errText = "Ошибка в Подписке.";
    }

    return !err;
}



bytes[0] = (byte)clientId;
bytes[1] = (byte)clientCommandNumber;
bytes[2] = (byte)clientTaskNumber;
bytes[3] = (byte)clientSubscribeNumber;





// Создаем сокет протокола TCP.
using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

// Подключение к брокеру.
tcpListener.Connect("127.0.0.1", 8888);


// Бесконечный цикл для
// отправки брокеру байты.
while (true)
{


    // Отправляем брокеру массив байтов.
    int sendBytes = tcpListener.Send(bytes);

    // Принятие ответа от брокера.
    int recieveBytes = tcpListener.Receive(bytes);

    // Отчет об отправке.
    Console.WriteLine("Клиент успешно отправил на брокер байты.");

    // Пауза.
    Thread.Sleep(2000);
}
