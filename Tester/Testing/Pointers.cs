using System;

namespace Tester.Testing
{
    public class Pointers
    {
        public IntPtr ip = new(0x44fab);
        public IntPtr ip2 = new(0x24);
        public IntPtr ip3 = new(long.MaxValue);
        public nint a = 15;
    }
}
