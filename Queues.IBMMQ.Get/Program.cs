using IBM.WMQ;
using System;
using System.Threading;

namespace Queues.IBMMQ.Get
{
    internal class Program
    {
        static readonly string queueManagerName = "TEST";
        static readonly string queueName = "Queue1";

        static void Main(string[] args)
        {
            // Wait for the Producer to create a queue
            Thread.Sleep(1000);

            GetMessagesFromQueue();
        }

        static void GetMessagesFromQueue()
        {
            var queueManager = new MQQueueManager(queueManagerName);
            var queue = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING);

            Console.WriteLine("Checking for messages every 5s.");

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                Thread.Sleep(5000);
                var message = new MQMessage();

                try
                {
                    queue.Get(message);
                    Console.WriteLine("Message got = " + message.ReadString(message.MessageLength));
                    message.ClearMessage();
                }
                catch (MQException exc)
                {
                    if (exc.ReasonCode == 2033)
                    {
                        Console.WriteLine("No message available, will check again in 5s.");
                    }
                    else
                    {
                        Console.WriteLine("MQException caught: {0} - {1}", exc.ReasonCode, exc.Message);
                    }
                }
            }

            try
            {
                queue.Close();
                queueManager.Disconnect();
            }
            catch (MQException exc)
            {
            }
        }
    }
}
