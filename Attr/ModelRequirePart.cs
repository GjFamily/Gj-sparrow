using System;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ModelRequirPart : Attribute
    {
        //
        // Fields
        //
        public Type part;

        //
        // Constructors
        //
        public ModelRequirPart(Type part) {
            this.part = part;
        }
    }
}
