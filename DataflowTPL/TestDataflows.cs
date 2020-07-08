using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowTPL
{
    class TestDataflows
    {

        private HashSet<int> _threads = new HashSet<int>(100);

        public void Test()
        {
            //TestBatchBlock();
            TestBufferBlock().Wait();
        }
        
        private void TestBatchBlock()
        {


            var batchBlock = new BatchBlock<int>(5);
            var actionBlock = new ActionBlock<int[]>(JoinString, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 100
            });

            //передал разбитый массив
            batchBlock.LinkTo(actionBlock);

            //Помечаем, что пачка готова для обработки
            batchBlock.Completion.ContinueWith(delegate { actionBlock.Complete(); });

            //добавляем данные в пачку
             PostToBatchAddingObject(batchBlock);

            //помечаем, что пачка заполнена
            batchBlock.Complete();

            //дожидаемся окончание обработки пачек
            actionBlock.Completion.Wait();

            Console.WriteLine($"кол-во потоков {_threads.Count}");

        }

        void JoinString(int[] values)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            //Console.WriteLine(String.Join("", values));
            //Console.WriteLine($"Thread Id: {threadId}");
            
            if(!_threads.Contains(threadId))
                _threads.Add(threadId);
        }

        // Posts random Employee data to the provided target block.
        static void PostToBatchAddingObject(ITargetBlock<int> target)
        {

            for (int i = 0; i < 10000000; i++)
            {
                target.Post(i);
            }

        }

        private Task TestBufferBlock()
        {
            var bufferBlock = new BufferBlock<int>();
            
            for (int i = 0; i < 10; i++)
            {
                bufferBlock.Post(i);
            }

            var edfo = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 100
            };

            var transformeBlock = new TransformBlock<int,int>(value =>
            {
                Console.WriteLine($"Модификация значение {value}");
                return value += 1;
            },edfo);
            
            var actionBlock = new ActionBlock<int>(value =>
            {
                Console.WriteLine($"Обработка нового значение {value}");
            },edfo);

            bufferBlock.LinkTo(transformeBlock);
            transformeBlock.LinkTo(actionBlock);

            return actionBlock.Completion;
        }
    }
}
