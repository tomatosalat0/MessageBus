using System.Threading.Tasks;

namespace MessageBus.Decorators
{
    internal abstract class DecoratedAsyncEventHandler<TEvent> : BaseDecoratedHandler, IAsyncMessageEventHandler<TEvent>
        where TEvent : IMessageEvent
    {
        protected IAsyncMessageEventHandler<TEvent> Inner { get; }

        protected DecoratedAsyncEventHandler(IAsyncMessageEventHandler<TEvent> inner)
            : base(inner)
        {
            Inner = inner;
        }

        public virtual Task HandleAsync(TEvent command)
        {
            return Inner.HandleAsync(command);
        }
    }
}
