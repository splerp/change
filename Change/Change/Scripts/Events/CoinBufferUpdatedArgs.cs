using System;

namespace Splerp.Change.Events
{
    // Used in even
    public sealed class CoinBufferUpdatedArgs : EventArgs
    {
        public byte[] updatedBuffer;

        public CoinBufferUpdatedArgs(byte[] completedBufferIn)
        {
            updatedBuffer = completedBufferIn;
        }
    }
}