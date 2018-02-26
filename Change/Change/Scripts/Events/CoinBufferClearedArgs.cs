using System;

namespace Splerp.Change.Events
{
    public sealed class CoinBufferClearedArgs : EventArgs
    {
        public byte[] updatedBuffer;

        public CoinBufferClearedArgs(byte[] completedBufferIn)
        {
            updatedBuffer = completedBufferIn;
        }
    }
}