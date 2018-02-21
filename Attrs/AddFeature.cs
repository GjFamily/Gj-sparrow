using System;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class AddFeature : Attribute
    {
        //
        // Fields
        //
        public Type feature;

        //
        // Constructors
        //
        public AddFeature(Type feature) {
            this.feature = feature;
        }
    }
}
