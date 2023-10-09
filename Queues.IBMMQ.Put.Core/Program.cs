using IBM.WMQ;
using IBM.WMQ.PCF;
using System.Collections;

namespace Queues.IBMMQ.Put.Core
{
    internal class Program
    {
        static string queueManagerName = "TEST";
        static string queueName = "Queue1";

        static void Main(string[] args)
        {
            Console.WriteLine("IBM MQ for .NET Core supports connections remote servers only.");
            Console.WriteLine("Provide server name, channel and port number to proceed."); 
            // PutMessagesOnQueue();
        }

        static void PutMessagesOnQueue()
        {
            Hashtable options = new Hashtable
            {
                { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED  }
            };

            string hostName = "RemoteServerName";
            string channelName = "SYSTEM.ADMIN.SVRCONN";
            int portNumber = 1414;
            options.Add(MQC.HOST_NAME_PROPERTY, hostName);
            options.Add(MQC.CHANNEL_PROPERTY, channelName);
            options.Add(MQC.PORT_PROPERTY, portNumber);
            options.Add(MQC.CONNECT_OPTIONS_PROPERTY, MQC.MQCNO_STANDARD_BINDING);
            MQQueueManager queueManager = null;
            PCFMessageAgent agent = null;
            try
            {
                // Below fails with exception - need to connect to an actual remote server
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
            createRequest.AddParameter(MQC.MQIA_MSG_DELIVERY_SEQUENCE, MQC.MQMDS_FIFO);

            // Priority Queue
            // createRequest.AddParameter(MQC.MQIA_MSG_DELIVERY_SEQUENCE, MQC.MQMDS_PRIORITY);

            createRequest.AddParameter(MQC.MQCA_Q_DESC, "Created by " + Environment.UserName + " on " + DateTime.UtcNow.ToString("o"));
            PCFMessage[] createResponses = agent.Send(createRequest);
            Console.WriteLine(createResponses[0].ToString());
        }

        static void PutMessages()
        {
            var queueManager = new MQQueueManager(queueManagerName);

            MQQueue queue = queueManager.AccessQueue(queueName, MQC.MQOO_OUTPUT | MQC.MQOO_INPUT_SHARED | MQC.MQOO_INQUIRE);

            Console.WriteLine("Creating messages every 1s.");
            Random random = new Random();

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                Thread.Sleep(1000);

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