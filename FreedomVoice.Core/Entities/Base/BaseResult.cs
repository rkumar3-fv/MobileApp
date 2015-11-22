namespace FreedomVoice.Core.Entities.Base
{
    using Enums;

    public class BaseResult<T>
    {
        public ErrorCodes Code { get; set; }

        public T Result { get; set; }

        public string ErrorText { get; set; }
    }
}
