using IBM.WMQ;
using IBM.WMQ.PCF;
using System;
using System.Collections;
using System.Threading;

namespace Queues.IBMMQ.Put
{
    // Custom implementation. Includes queue creation.
    internal class Program
    {
        static readonly string queueManagerName = "QM";
        static readonly string queueName = "Queue1";

        static void Main(string[] args)
        {
            PutMessagesOnQueue();
        }

        static void PutMessagesOnQueue()
        {
            Hashtable options = new Hashtable
            {
                // Connection to a local server
                { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_BINDINGS }
            };

            MQQueueManager queueManager = null;
            PCFMessageAgent agent = null;
            try
            {
                queueManager = new MQQueueManager(queueManagerName, options);
                agent = new PCFMessageAgent(queueManager);

                try
                {
                    CreateQueue(agent);
                }
                catch (MQException exc)
                {
                    if (exc.Message == "MQRCCF_OBJECT_ALREADY_EXISTS")
                    {
                        // Queue already exists and is accessible.
                    }
                }

                PutMessages();
            }
            finally
            {
                // Disconnect the agent and queuemanager.
                if (agent != null)
                    agent.Disconnect();

                if (queueManager != null && queueManager.IsConnected)
                    queueManager.Disconnect();
            }
        }

        static void CreateQueue(PCFMessageAgent agent)
        {
            PCFMessage createRequest = new PCFMessage(CMQCFC.MQCMD_CREATE_Q);
            createRequest.AddParameter(MQC.MQCA_Q_NAME, queueName);
            createRequest.AddParameter(MQC.MQIA_Q_TYPE, MQC.MQQT_LOCAL);
            createRequest.AddParameter(MQC.MQIA_DEF_PERSISTENCE, MQC.MQPER_PERSISTENT);

            // Standard FIFO queue
            //createRequest.AddParameter(MQC.MQIA_MSG_DELIVERY_SEQUENCE, MQC.MQMDS_FIFO);

            // Priority Queue
            createRequest.AddParameter(MQC.MQIA_MSG_DELIVERY_SEQUENCE, MQC.MQMDS_PRIORITY);

            createRequest.AddParameter(MQC.MQCA_Q_DESC, "Created by " + Environment.UserName + " on " + DateTime.UtcNow.ToString("o"));
            PCFMessage[] createResponses = agent.Send(createRequest);
        }

        static void PutMessages()
        {
            var queueManager = new MQQueueManager(queueManagerName);

            MQQueue queue = queueManager.AccessQueue(queueName, MQC.MQOO_OUTPUT | MQC.MQOO_INPUT_SHARED | MQC.MQOO_INQUIRE);

            Console.WriteLine("Creating messages every 2s.");
            Random random = new Random();

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                Thread.Sleep(2000);

                var message = new MQMessage
                {
                    Format = MQC.MQFMT_STRING,
                    Priority = random.Next(9)
                };

                var text = $"Hello from the code - {random.Next(100)}, P {message.Priority}";
                message.WriteString(text);
                Console.WriteLine(text);

                var putMsgOptions = new MQPutMessageOptions();
                try
                {
                    queue.Put(message, putMsgOptions);
                }
                catch (MQException exc)
                {
                    Console.WriteLine(exc);
                }
            }
        }
    }
}
