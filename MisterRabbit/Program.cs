namespace MisterRabbit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using EasyNetQ;
    using EasyNetQ.Async;

    using MisterRabbit.Messages;

    class Program
    {
        static void Main(string[] args)
        {
            var logger = new NullLogger();
//            using (var bus = RabbitHutch.CreateBus("host=localhost", x => x.Register<IEasyNetQLogger>(_ => logger)))
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                bus.Subscribe<HelloMessage>("moo", m => Console.WriteLine("Hello1-1 {0}", m.Name));
                bus.Subscribe<HelloMessage>("moo", m => Console.WriteLine("Hello1-2 {0}", m.Name));
                bus.Subscribe<HelloMessage>("moo2", m => Console.WriteLine("Hello2-1 {0}", m.Name));

                DoStuff(bus);
                //DoMoreStuff(bus);

                Console.ReadLine();
            }
        }

        // This hangs
        private static async void DoStuff(IBus bus)
        {
            var message = new HelloMessage { Name = "Bob" };
            var message2 = new HelloMessage { Name = "Jim" };
            using (var publishChannel = bus.OpenPublishChannel(x => x.WithPublisherConfirms()))
            {
                var result1 = await publishChannel.PublishAsync(message);

                var result2 = await publishChannel.PublishAsync(message2);
            }
        }

        // This works
        private static void DoMoreStuff(IBus bus)
        {
            var message = new HelloMessage { Name = "Bob" };
            var message2 = new HelloMessage { Name = "Jim" };
            var sem = new SemaphoreSlim(1);

            using (var publishChannel = bus.OpenPublishChannel(x => x.WithPublisherConfirms()))
            {
                sem.Wait();
                publishChannel.Publish(
                    message, x => x.OnSuccess(() => sem.Release()).OnFailure(() => sem.Release()));

                sem.Wait();
                publishChannel.Publish(
                    message2, x => x.OnSuccess(() => sem.Release()).OnFailure(() => sem.Release()));
            }
        }
    }
}
