using System.Runtime.InteropServices;
using TaskQueueLibrary.Enums;

namespace TaskQueueLibrary.Classes
{
    // Указываем упаковку - все данные идут последовательно, распределенные по байтам.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UserTask
    {

        // Набор последовательных байтов для передачи по TCP/IP.
        public byte id;
        public byte command;
        public byte subscription;
        public TaskType type;
        public byte priority;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 95)]
        public byte[] values = new byte[100 - 5];

        public UserTask(byte id, byte command, TaskType type, byte subscription, byte priority, byte[] values)
        {
            this.id = id;
            this.type = type;
            this.priority = priority;
            Array.Copy(values, this.values, 95);
        }

        public UserTask(byte[] buffer)
        {

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            UserTask record = (UserTask)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(UserTask));

            id = record.id;
            type = record.type;
            priority = record.priority;
            Array.Copy(record.values, values, 100 - 5);
            handle.Free();
        }

        public byte[] getProtocolBytes()
        {
            byte[] rawdata = new byte[100];

            // Фиксирует массив в памяти, чтобы его не удалил мусорщик, не переместил.
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);

            // Marshal Предоставляет коллекцию статических методов ири работе с неуправляемой памятью
            // Marshal.StructureToPtr — это метод из пространства имен System.Runtime.InteropServices в .NET,
            // который копирует данные из управляемого
            // объекта(обычно структуры) в заранее выделенный неуправляемый блок памяти
            Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
            handle.Free();

            return rawdata;
        }


    }
}