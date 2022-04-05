namespace Tester.Testing
{
    public class Array3
    {
        // not supported
        public object[] arr;

        public Array3()
        {
            arr = new object[] { new RotationPacket(44, 33, 22), 12, "Hello" };
        }
    }
}