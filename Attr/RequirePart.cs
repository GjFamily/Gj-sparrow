using System;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequirePart : Attribute
    {
        //
        // Fields
        //
        public Type part;

        //
        // Constructors
        //
        public RequirePart(Type part) {
            this.part = part;
        }
    }
}
