#nullable enable

using System;
using System.ServiceProcess;
using System.Threading;

namespace PortForwardingService;

internal static class Program {

    private static void Main(string[] args) {
        Service service = new();

        if (Environment.UserInteractive) {
            ManualResetEventSlim unblockMainThread = new(false);

            Console.CancelKeyPress += delegate(object _, ConsoleCancelEventArgs eventArgs) {
                eventArgs.Cancel = true;
                service.onStop();
                unblockMainThread.Set();
            };

            service.onStart(args);

            unblockMainThread.Wait(); //block while service runs, then exit once user hits Ctrl+C
        } else {
            ServiceBase.Run(service);
        }
    }

}