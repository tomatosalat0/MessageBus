namespace MessageBus.Serialization.Json
{
    public sealed class TypeCreationOptions
    {
        public bool ThrowExceptionGettingUnsetNotNullProperty { get; set; } = true;

        public bool ThrowExceptionSettingNullToNotNullProperty { get; set; } = true;
    }
}
