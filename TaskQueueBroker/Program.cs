// ====================================================
//					БРОКЕР
// ====================================================

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaskQueueLibrary.Classes;
using TaskQueueLibrary.Enums;

// Глобальный список задач для брокера.
List<UserTask> userTasksList = new List<UserTask>();

// Компаратор для сортировки задач по приоритету.
UserTaskComparer userTaskComparer = new UserTaskComparer();

// Объект блокировки доступа.
object? lockAccess = new object();



// Создаем объект синхронизации.
object? lockFlagThread = new object();
object? lockActiveClients = new object();

// Создаем флаг о состоянии потока.
ThreadActivity flag = ThreadActivity.CAN_CREATE_NEW_THREAD;


const int MAX_COUNT_CLIENTS = 3;


int countCreatedThreads = 0;
int countShutdownClients = 0;
int lastCountActiveClients = 0;

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

    lock (lockFlagThread)
    {
        localFlag = flag;
    }

    // Если поток свободен.
    if (localFlag == ThreadActivity.CAN_CREATE_NEW_THREAD)
    {
        if (countCreatedThreads < MAX_COUNT_CLIENTS)
        {
            lock (lockFlagThread)
            {
                // Занимаем поток
                flag = ThreadActivity.THREAD_CREATED_IS_WAIT_CONNECT;
            }

            countCreatedThreads++;
            Console.WriteLine("Количество созданных потоков для работы с клиентами - " + countCreatedThreads);


            // Создаем новый поток.
            Thread thread = new Thread(() => BrokerMethod(tcpServer));

            // Начинаем поток.
            thread.Start();
        }
    }



    lock (lockActiveClients)
    {
        if (lastCountActiveClients != countShutdownClients)
        {
            lastCountActiveClients = countShutdownClients;
            Console.WriteLine("Количество активных клиентов: " + countShutdownClients);
        }
        if (countShutdownClients == MAX_COUNT_CLIENTS)
        {
            tcpServer.Close();
            Console.WriteLine("Сервер завершил свою работу.");
            break;
        }
    }






    if (userTasksList.Count > 0)
    {
        // Получаем первый элемент из списка.
        UserTask firstTask = userTasksList[0];

        // Преобразовать его в байты.
        byte[] firstTaskInBytes = firstTask.getProtocolBytes();
        int bytes = firstTaskInBytes.Length;

        userTasksList.RemoveAt(0);

        // Ожидаем подключения воркера по TCP.
        using Socket worker = tcpServer.Accept();

        // Отправляем ему именно байты.
        worker.Send(firstTaskInBytes, 0, bytes, SocketFlags.None);

        Console.WriteLine("Данные из потока отправлены воркеру.");

    }










}





// Метод, который выполняется потоком.
// Принимает байты от клиента и отправляет их воркеру.
void BrokerMethod(Socket tcpServer)
{


    // Ожидаем подключение от клиента по TCP.
    using Socket client = tcpServer.Accept();


    // Как только подключился, поток свободен.
    lock (lockFlagThread)
    {
        // Флаг = 0, поток свободен.
        flag = ThreadActivity.CAN_CREATE_NEW_THREAD;
    }


    // Массив байтов для
    // принятия данных от клиента.
    byte[] receiveBytes = new byte[100];
    byte[] answerBytes = new byte[100];



    // В этом цикле подключенный клиент
    // будет обмениваться байтами
    // с брокера, пока не отключится совсем.
    while (true)
    {




        // Получаем количество байтов от клиента.
        int bytes = client.Receive(receiveBytes);

        // Получение задания.
        if (receiveBytes[1] == (byte)ClientCmdType.SEND_TASK)
        {

            UserTask t = new UserTask(receiveBytes);

            lock (lockAccess)
            {
                userTasksList.Add(t);
                userTasksList.Sort(userTaskComparer);
            }






            answerBytes[0] = (byte)ResponseCmdType.OK; // ОК
            client.Send(answerBytes);
            Console.WriteLine("Получил задание от клиента и ответил ОК.");
            continue;
        }




        // Получения запроса о состоянии.
        if (receiveBytes[1] == (byte)ClientCmdType.GET_STATE)
        {
            answerBytes[0] = (byte)ResponseCmdType.OK; // ОК
            answerBytes[1] = (byte)TaskState.COMPLETED; // Номер состояния (0/1/2) 
            client.Send(answerBytes);
            Console.WriteLine("Получил запрос состояния и ответил ОК, номер состояния.");
            continue;
        }




        // Получения запроса о результате.
        if (receiveBytes[1] == (byte)ClientCmdType.GET_RESULT)
        {
            answerBytes[0] = (byte)ResponseCmdType.OK; // ОК
            //answerBytes[1..99] -  результат
            client.Send(answerBytes);
            Console.WriteLine("Получил запрос результата, ответил ОК, байты результата.");
            continue;
        }




        // Получения запроса об отсоединении (о завершении).
        if (receiveBytes[1] == (byte)ClientCmdType.FINISH_TASK)
        {
            answerBytes[0] = (byte)ResponseCmdType.OK; // ОК
            client.Send(answerBytes);
            Console.WriteLine("Получил запрос отсоединения, ответил ОК.");
            break;
        }



    }

    client.Shutdown(SocketShutdown.Both);
    client.Close();
    lock (lockActiveClients)
    {
        countShutdownClients++;
    }
}