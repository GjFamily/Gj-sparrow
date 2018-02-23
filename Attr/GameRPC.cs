using System;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class GameRPC : Attribute
    {
        //
        // Fields
        //
        public Type feature;

        //
        // Constructors
        //
        public GameRPC(Type feature)
        {
            this.feature = feature;
        }
    }
}
