using System;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class AddPart : Attribute
    {
        //
        // Fields
        //
        public Type part;

        //
        // Constructors
        //
        public AddPart(Type part) {
            this.part = part;
        }
    }
}
