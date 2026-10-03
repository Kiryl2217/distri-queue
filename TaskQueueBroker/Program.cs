// ====================================================
//					БРОКЕР
// ====================================================

using System.Net;
using System.Net.Sockets;
using TaskQueueLibrary.Enums;

// Создаем объект синхронизации.
object? obj = new object();

// Создаем флаг о состоянии потока.
ThreadActivity flag = ThreadActivity.CAN_CREATE_NEW_THREAD;

// Задаем IP-адрес и порт для сервера.
IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);

// Создаем сокет протокола TCP.
using Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

// Связываем сокет с IP-адресом и портом.
tcpServer.Bind(ipPoint);

// Сервер начинает слушать.
tcpServer.Listen();

// Отчет о том, что сервер запущен.
Console.WriteLine("Сервер запущен.");

// Бесконечный цикл,
// где флаг ставится
// в активное состояние
// и создается новый поток.
while (true)
{
    ThreadActivity localFlag;

    lock (obj)
    {
        localFlag = flag;
    }

    // Если поток  свободен.
    if (localFlag == ThreadActivity.CAN_CREATE_NEW_THREAD)
    {
        lock (obj)
        {
            // Занимаем поток
            flag = ThreadActivity.THREAD_CREATED_IS_WAIT_CONNECT;
        }

        // Создаем новый поток.
        Thread thread = new Thread(() => BrokerMethod(tcpServer));

        // Начинаем поток.
        thread.Start();
    }
}





// Метод, который выполняется потоком.
// Принимает байты от клиента и отправляет их воркеру.
void BrokerMethod(Socket tcpServer)
{


    // Ожидаем подключение от клиента по TCP.
    using Socket client = tcpServer.Accept();


    // Как только подключился, поток свободен.
    lock (obj)
    {
        // Флаг = 0, поток свободен.
        flag = ThreadActivity.CAN_CREATE_NEW_THREAD;
    }

    // Локальный массив байтов для
    // принятия данных от клиента.
    byte[] buffer = new byte[100];



    // В этом цикле подключенный клиент
    // будет омениваться байтами
    // с брокером, пока не отключиться совсем.
    while (true)
    {
        // Получаем количество байтов от клиента.
        int bytes = client.Receive(buffer);

        // Отчет о полученных байтах.
        Console.WriteLine("Клиент: " + buffer[0] +
            " команда: " + buffer[1] + " задача: " + buffer[2] +
            " подписка: " + buffer[3] + " всего байт: " + bytes);

        client.Send(buffer);

        Console.WriteLine("Отправил ответ клиенту " + buffer[0]);
    }
}