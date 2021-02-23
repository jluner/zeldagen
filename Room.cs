using System;

namespace zeldagen
{
    public abstract class RoomBase : Layout
    {
        private static int _counter;

        protected RoomBase() : base(_counter++)
        {
        }
    }
}