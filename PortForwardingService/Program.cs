#nullable enable

using NLog;
using NLog.Config;
using NLog.MessageTemplates;
using System;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace PortForwardingService;

internal static class Program {

    private static void Main(string[] args) {
        UnfuckedValueFormatter.register();
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

    /// <summary>
    /// When logging strings to NLog using structured logging, don't surround them with quotation marks, because it looks stupid
    /// </summary>
    /// <param name="parent">Built-in <see cref="ValueFormatter"/></param>
    private class UnfuckedValueFormatter(IValueFormatter parent): IValueFormatter {

        public static void register() {
            ServiceRepository services = LogManager.Configuration.LogFactory.ServiceRepository;
            services.RegisterService(typeof(IValueFormatter), new UnfuckedValueFormatter((IValueFormatter) services.GetService(typeof(IValueFormatter))));
        }

        public bool FormatValue(object value, string format, CaptureType captureType, IFormatProvider formatProvider, StringBuilder builder) {
            switch (value) {
                case string s:
                    builder.Append(s);
                    return true;
                case StringBuilder s:
                    builder.Append(s);
                    return true;
                case ReadOnlyMemory<char> s:
                    builder.Append(s);
                    return true;
                default:
                    return parent.FormatValue(value, format, captureType, formatProvider, builder);
            }
        }

    }

}