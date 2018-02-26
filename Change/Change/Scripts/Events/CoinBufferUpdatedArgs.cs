using System;

namespace Splerp.Change.Events
{
    public sealed class CoinBufferUpdatedArgs : EventArgs
    {
        public int arrayLoc;

        public CoinBufferUpdatedArgs(int arrayLocIn)
        {
            arrayLoc = arrayLocIn;
        }
    }
}