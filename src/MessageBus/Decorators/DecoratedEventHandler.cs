namespace MessageBus.Decorators
{
    internal abstract class DecoratedEventHandler<TEvent> : BaseDecoratedHandler, IMessageEventHandler<TEvent>
        where TEvent : IMessageEvent
    {
        protected IMessageEventHandler<TEvent> Inner { get; }

        protected DecoratedEventHandler(IMessageEventHandler<TEvent> inner)
            : base(inner)
        {
            Inner = inner;
        }

        public virtual void Handle(TEvent command)
        {
            Inner.Handle(command);
        }
    }
}
