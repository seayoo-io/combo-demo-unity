using System;

namespace Types
{
    public class Result<T, U>
    {
        internal class Success : Result<T, U>
        {
            public T Value { get; private set; }

            public Success(T value)
            {
                Value = value;
            }
        }

        internal class Fail : Result<T, U>
        {
            public U Value { get; private set; }

            public Fail(U error)
            {
                Value = error;
            }
        }

        internal static Result<T, U> CreateSuccess(T value)
        {
            return new Success(value);
        }

        internal static Fail CreateFail(U e)
        {
            return new Fail(e);
        }

        public bool IsSuccess
        {
            get
            {
                return this is Success;
            }
        }

        public T Data
        {
            get
            {
                if (this is Success success)
                {
                    return success.Value;
                }
                return default;
            }
        }

        public U Error
        {
            get
            {
                if (this is Fail fail)
                {
                    return fail.Value;
                }
                return default;
            }
        }
    }
}
