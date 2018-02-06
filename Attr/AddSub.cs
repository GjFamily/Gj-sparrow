using System;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class AddSub : Attribute
    {
        //
        // Fields
        //
        public Type sub;

        //
        // Constructors
        //
        public AddSub(Type sub) {
            this.sub = sub;
        }
    }
}
