#nullable enable

using System;
using System.ServiceProcess;
using System.Threading;

namespace PortForwardingService {

    internal static class Program {

        private static void Main(string[] args) {
            var service = new Service();

            if (Environment.UserInteractive) {
                var unblockMainThread = new ManualResetEvent(false);

                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs eventArgs) {
                    eventArgs.Cancel = true;
                    service.onStop();
                    unblockMainThread.Set();
                };

                service.onStart(args);

                unblockMainThread.WaitOne(); //block while service runs, then exit once user hits Ctrl+C
            } else {
                ServiceBase.Run(service);
            }
        }

    }

}