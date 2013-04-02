namespace EasyNetQ.Async
{
    using System.Threading.Tasks;

    /// <summary>
    /// Async / await extensions for EasyNetQ
    /// </summary>
    public static class PublishChannelExtensions
    {
        /// <summary>
        /// Publishes a message async using publisher confirms
        /// </summary>
        /// <typeparam name="TMessage">The message type</typeparam>
        /// <param name="publishChannel">The publish channel</param>
        /// <param name="message">The message to publish</param>
        /// <returns>A Task of bool, a true result signifies successfully delivered, false if failed</returns>
        public static Task<bool> PublishAsync<TMessage>(this IPublishChannel publishChannel, TMessage message)
        {
            var tcs = new TaskCompletionSource<bool>();

            publishChannel.Publish(
                               message,
                               x =>
                                   x.OnSuccess(() => tcs.SetResult(true))
                                    .OnFailure(() => tcs.SetResult(false)));

            return tcs.Task;
        }

        /// <summary>
        /// Makes an RPC style asynchronous request.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="publishChannel">The publish channel.</param>
        /// <param name="request">The request message</param>
        /// <returns>Task of TResponse</returns>
        public static Task<TResponse> RequestAsync<TRequest, TResponse>(this IPublishChannel publishChannel, TRequest request)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            publishChannel.Request<TRequest, TResponse>(request, tcs.SetResult);

            return tcs.Task;
        }
    }
}