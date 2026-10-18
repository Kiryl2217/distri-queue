// ====================================================
//					ВОРКЕР (Кастомер)
// ====================================================

using System.Net.Sockets;
using System.Runtime.InteropServices;
using TaskQueueLibrary.Classes;

// Бесконечный цикл для
// принятия байтов от брокера.
while (true)
{
    // Создаем сокет протокола TCP.
    using Socket tcpWorker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    // Подключаемся к брокеру.
    tcpWorker.Connect("127.0.0.1", 8888);




    // Получаем массив байтов.
    byte[] receiveBytes = new byte[100];

    int bytes = tcpWorker.Receive(receiveBytes);

    // Преобразуем полученные байты в задание.
    UserTask receiveTask = new UserTask(receiveBytes);

    // Вывод на консоль задания.
    Console.WriteLine("Воркер получил от клиента: Id - " + receiveTask.id + ", Команду - "
        + receiveTask.command, ", Подписку - ", +receiveTask.subscription + ", Тип задачи - "
        + receiveTask.type + ", Приоритет - " + receiveTask.priority + ". Всего получено байтов: " + receiveBytes.Length);



}
