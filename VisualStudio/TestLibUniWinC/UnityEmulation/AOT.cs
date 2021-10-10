using System;

namespace AOT
{
    public class MonoPInvokeCallbackAttribute : Attribute
    {
        private Type type;

        public MonoPInvokeCallbackAttribute(Type type)
        {
            this.type = type;
        }
    }
}
