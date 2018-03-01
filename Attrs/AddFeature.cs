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
        public bool prefab = false;
        //
        // Constructors
        //
        public AddFeature(Type feature) {
            this.feature = feature;
        }

        public AddFeature(Type feature, bool b)
        {
            this.feature = feature;
            prefab = b;
        }
    }
}
