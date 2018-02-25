using System;

namespace Splerp.Change.Events
{
    public sealed class CoinBufferUpdatedArgs : EventArgs
    {
        public byte[] updatedBuffer;

        public CoinBufferUpdatedArgs(byte[] completedBufferIn)
        {
            updatedBuffer = completedBufferIn;
        }
    }
}