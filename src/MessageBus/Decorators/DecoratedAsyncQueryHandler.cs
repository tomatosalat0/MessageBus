using System.Threading.Tasks;

namespace MessageBus.Decorators
{
    public abstract class DecoratedAsyncQueryHandler<TQuery, TQueryResult> : BaseDecoratedHandler, IAsyncMessageQueryHandler<TQuery, TQueryResult>
        where TQuery : IMessageQuery<TQueryResult>
        where TQueryResult : IMessageQueryResult
    {
        protected IAsyncMessageQueryHandler<TQuery, TQueryResult> Inner { get; }

        protected DecoratedAsyncQueryHandler(IAsyncMessageQueryHandler<TQuery, TQueryResult> inner)
            : base(inner)
        {
            Inner = inner;
        }

        public virtual Task<TQueryResult> HandleAsync(TQuery query)
        {
            return Inner.HandleAsync(query);
        }
    }
}
