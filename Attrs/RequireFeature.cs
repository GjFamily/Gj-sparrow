using System;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireFeature : Attribute
    {
        //
        // Fields
        //
        public Type feature;

        //
        // Constructors
        //
        public RequireFeature(Type feature) {
            this.feature = feature;
        }
    }
}
