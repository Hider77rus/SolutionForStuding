using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowTPL
{
    class Program
    {
        static void Main(string[] args)
        {

            //var batchDataflow = new TestDataflows();
            //batchDataflow.Test();

            //BroadcastAndBufferBlocksTestAsync().Wait();
            //ActionBlockTest();
            //BatchBlock();
            
            
            TimerTests tests = new TimerTests();
            tests.Test();
        }

        static void ActionBlockTest()
        {
            var actionBlock = new ActionBlock<int>(n =>
            {
                Console.WriteLine($"ActionBlock value: {n}");
                Console.WriteLine($"Thread Id: {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine();
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            });

            actionBlock.Post(1);
            actionBlock.Post(2);
            actionBlock.Post(3);
            actionBlock.Post(4);
            //actionBlock.Complete();

            actionBlock.Completion.ContinueWith(task => Console.WriteLine($"Task Status = {task.Status}")).Wait();
            
        }

        static async Task BroadcastAndBufferBlocksTestAsync()
        {
            /*
            Console.WriteLine("BroadcastBlock");
            Console.WriteLine();
            //Блок, который хранит и возвращает одно последнее значение значение
            var broadcastBlock = new BroadcastBlock<int>(null);

            broadcastBlock.Post(125);
            broadcastBlock.Post(126);

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(broadcastBlock.Receive());
            }

            Console.WriteLine("---------------");

            Console.WriteLine("WriteOnceBlockTest");

            //Хранит только первое значение
            var writeOnce = new WriteOnceBlock<int>(null);
            writeOnce.Post(4);
            writeOnce.Post(5);

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(writeOnce.Receive());
            }
            */
            Console.WriteLine("BufferBlockTest");

            //очередь
            var bufferBlock = new BufferBlock<int>();

            for (int i = 0; i < 3; i++)
            {
                bufferBlock.Post(i);
            }

            // for (int i = 0; i < 10; i++)
            // {
            //     Console.WriteLine(bufferBlock.Receive());
            // }

            // for (int i = 0; i < 3; i++)
            // {
            //     await bufferBlock.SendAsync(i);
            // }



            //Asynchronously receive the messages back from the block.
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine(await bufferBlock.ReceiveAsync());
            }


        }

        static void BatchBlock()
        {
            //Создаём пачки указанной размерности
            var batchBlock = new BatchBlock<int>(10);

            for (int i = 0; i < 10; i++)
            {
                batchBlock.Post(10);
            }

            for (int i = 0; i < 5; i++)
            {
                batchBlock.Post(10);
            }

            batchBlock.Complete();

            Console.WriteLine($"Сумма первого блока: {batchBlock.Receive().Sum()}");

            Console.WriteLine($"Сумма второго блока: {batchBlock.Receive().Sum()}");

        }

    }
}
