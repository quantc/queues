# queues
Queues comparison 

IMB MQ

.NET Framework
To test simple Producer/Consumer (Put/Get) pattern follow these steps:
- install IBM MQ server and client 
- install MQ explorer installed (separate installer)
- run both IBMMQ.Put and IBMMQ.Get projects
   - Put will be creating new messages every x seconds and putting them on the newly created queue "Queue1"
   - Get will be checking the queue every y seconds for new messagges and displaying them

.NET (.NET Standard & .NET 6)
To test simple Producer/Consumer (Put/Get) pattern:
- IBM MQ supports only remote server connection. This POC doesn't cover that part. But! You are welcome to put a name of a remote server to test that yourself.
