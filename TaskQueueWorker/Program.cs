// ====================================================
//					ВОРКЕР (Кастомер)
// ====================================================

using System.Net.Sockets;

// Бесконечный цикл для
// принятия байтов от брокера.
while (true)
{
    // Создаем сокет протокола TCP.
    using Socket tcpWorker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    // Подключаемся к брокеру.
    tcpWorker.Connect("127.0.0.1", 8888);

    // Массив байтов.
    byte[] data = new byte[1024];

    // Получаем в переменную int
    // количество байтов.
    int bytes = tcpWorker.Receive(data);

    // Отчет о полученном количестве байтов.
    Console.WriteLine("Воркер получил число байтов: " + bytes);
}