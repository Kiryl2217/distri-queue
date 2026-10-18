// ====================================================
//					КЛИЕНТ (Продюсер)
// ====================================================
using System.Net.Sockets;
using System.Threading.Tasks;
using TaskQueueLibrary.Enums;


// Массив байтов для отправки.
byte[] sendBytes = new byte[100];

// Массив байтов для получения.
byte[] receiveBytes = new byte[100];

// Id клиента.
string? stringClientId = string.Empty;

// Номер задачи.
string? stringClientTaskNumber = string.Empty;

// Нужна ли подписка.
string? stringClientSubscribe = string.Empty;

byte clientId = 0;
byte clientTaskNumber = 0;
byte clientSubscribeNumber = 0;

// Флаг для проверки корректности ввода данных.
bool isDataCorrect = false;

while (!isDataCorrect)
{
    Console.Write("Введите Id клиента: ");
    stringClientId = Console.ReadLine();
    Console.WriteLine();

    Console.Write("Введите номер задачи(0-5): ");
    stringClientTaskNumber = Console.ReadLine();
    Console.WriteLine();

    Console.Write("Нужна или нет подписка на состояние " +
        "о завершении выполнения задания(1/0): ");
    stringClientSubscribe = Console.ReadLine();
    Console.WriteLine();

    isDataCorrect = CheckData(stringClientId,
        stringClientTaskNumber, stringClientSubscribe,
        out clientId, out clientTaskNumber, out clientSubscribeNumber,
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




// Создаем сокет протокола TCP.
using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

// Подключение к брокеру.
tcpListener.Connect("127.0.0.1", 8888);

// Флаг проверки успешной отправки задания.
bool isSuccessTaskSend = false;


bool isResultReceived = false;
bool isStopClient = false;

// Бесконечный цикл для
// отправки брокеру байты.
while (true)
{




    // Посылка задания.
    if (!isSuccessTaskSend)
    {
        sendBytes[0] = (byte)clientId;
        sendBytes[1] = (byte)ClientCmdType.SEND_TASK; // Команда 0 - Посылка задания.
        sendBytes[2] = (byte)clientTaskNumber;
        sendBytes[3] = (byte)clientSubscribeNumber;

        // Отправляем брокеру массив байтов.
        int sendCounts = tcpListener.Send(sendBytes);

        // Принятие ответа от брокера.
        int recieveCounts = tcpListener.Receive(receiveBytes);

        if (receiveBytes[0] == 0)
        {
            isSuccessTaskSend = true;

            // Отчет об отправке.
            Console.WriteLine("Клиент успешно отправил на брокер байты.");

        }
        else
        {
            Console.WriteLine("Ошибка отправки задания. Повторная отправка");
            // Пауза.
            Thread.Sleep(2000);
        }
    }




    // Запрос подписки (если задания успешно отправлено
    // и клиент нуждается в подписке)
    if (isSuccessTaskSend && clientSubscribeNumber == (byte)SubscriptionType.YES)
    {

        sendBytes[0] = (byte)clientId;
        sendBytes[1] = (byte)ClientCmdType.GET_STATE; // Команда 1 - Запрос подписки.

        // Отправляем брокеру массив байтов.
        int sendCounts = tcpListener.Send(sendBytes);

        // Принятие ответа от брокера.
        int recieveCounts = tcpListener.Receive(receiveBytes);

        if (receiveBytes[0] == (byte)ResponseCmdType.OK)
        {
            isSuccessTaskSend = true;

            // 1 - завершен.
            if (receiveBytes[1] == (byte)TaskState.COMPLETED)
            {
                isResultReceived = true;
            }

            // Отчет об отправке.
            Console.WriteLine("Клиент успешно отправил запрос на подписку брокеру.");
            Console.WriteLine("Состояние получил - " + receiveBytes[1]);

        }
        else
        {
            Console.WriteLine("Ошибка отправки запроса на подписку. Повторная отправка.");

            // Пауза.
            Thread.Sleep(2000);
        }
    }




    // Запрос на результат.
    if (isResultReceived)
    {

        sendBytes[0] = (byte)clientId;
        sendBytes[1] = (byte)ClientCmdType.GET_RESULT; // Команда 2 - Запрос результата.

        // Отправляем брокеру массив байтов.
        int sendCounts = tcpListener.Send(sendBytes);

        // Принятие ответа от брокера.
        int recieveCounts = tcpListener.Receive(receiveBytes);

        if (receiveBytes[0] == (byte)ResponseCmdType.OK)
        {


            // 1 - заверщен.
            if (receiveBytes[1] == (byte)TaskState.COMPLETED)
            {
                isStopClient = true;
            }

            // Отчет об отправке.
            Console.WriteLine("Клиент успешно отправил запрос на результат.");
            Console.WriteLine("Результат получил: ========Вывести результат из receiveBytes===========");

        }
        else
        {
            Console.WriteLine("Ошибка отправки запроса на подписку. Повторная отправка.");

            // Пауза.
            Thread.Sleep(2000);
        }
    }




    // Завершение соединения клиента с брокером.
    if (isStopClient)
    {

        sendBytes[0] = (byte)clientId;
        sendBytes[1] = (byte)ClientCmdType.FINISH_TASK; // Команда 3 - Завершение работы.

        // Отправляем брокеру массив байтов.
        int sendCounts = tcpListener.Send(sendBytes);

        // Принятие ответа от брокера.
        int recieveCounts = tcpListener.Receive(receiveBytes);

        if (receiveBytes[0] == (byte)ResponseCmdType.OK)
        {
            // Отчет об отправке.
            Console.WriteLine("Клиент разрывает соединение.");
            break;
        }
        else
        {
            Console.WriteLine("Ошибка отправки запроса на подписку. Повторная отправка.");

            // Пауза.
            Thread.Sleep(2000);
        }
    }
}

tcpListener.Shutdown(SocketShutdown.Both);
tcpListener.Close();
Console.ReadLine();




bool CheckData(string? id, string? task, string? subscribe,
    out byte clientId, out byte taskNumber,
    out byte subscribeNumber, out string errText)
{
    bool err = false;
    errText = string.Empty;
    clientId = 0;
    taskNumber = 0;
    subscribeNumber = 0;

    // Проверка Id.
    if (!err && string.IsNullOrWhiteSpace(id))
    {
        err = true;
        errText = "Ошибка в Id.";
    }

    if (!err && !byte.TryParse(id, out clientId))
    {
        err = true;
        errText = "Ошибка в Id.";
    }

    if (!err && clientId < 1)
    {
        err = true;
        errText = "Ошибка в Id.";
    }

    // Проверка Задачи.
    if (!err && string.IsNullOrWhiteSpace(task))
    {
        err = true;
        errText = "Ошибка в Задаче.";
    }

    if (!err && !byte.TryParse(task, out taskNumber))
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

    if (!err && !byte.TryParse(subscribe, out subscribeNumber))
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

