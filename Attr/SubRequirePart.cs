using System;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SubRequirePart : Attribute
    {
        //
        // Fields
        //
        public Type part;

        //
        // Constructors
        //
        public SubRequirePart(Type part) {
            this.part = part;
        }
    }
}
