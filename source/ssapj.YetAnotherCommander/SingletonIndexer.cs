using System;

namespace ssapj.YetAnotherCommander
{
    internal class SingletonIndexer
    {

        private static readonly Lazy<SingletonIndexer> lazy = new Lazy<SingletonIndexer>(() => new SingletonIndexer());
        private uint _value;

        public static SingletonIndexer Instance => lazy.Value;

        private SingletonIndexer()
        {
            this._value = 0;
        }

        public uint GetValue()
        {
            var temp = this._value;

            if (this._value == uint.MaxValue)
            {
                this._value = 0;
            }
            else
            {
                this._value++;
            }

            return temp;
        }

    }
}
