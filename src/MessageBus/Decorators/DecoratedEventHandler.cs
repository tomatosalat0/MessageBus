namespace MessageBus.Decorators
{
    public abstract class DecoratedEventHandler<TEvent> : BaseDecoratedHandler, IMessageEventHandler<TEvent>
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
