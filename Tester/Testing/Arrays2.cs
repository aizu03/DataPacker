using System.Collections.Generic;

namespace Tester.Testing
{
    public class Arrays2
    {
        public RotationPacket[] rotations;
        public string str = "Passed?";

        public Arrays2()
        {
            rotations = new[]
            {
                new RotationPacket(17, 90, 45.3),
                new RotationPacket(17, 91, 46.3),
                new RotationPacket(17, 92, 47.3),
                new RotationPacket(17, 93, 48.3),
                new RotationPacket(17, 94, 49.3),
                new RotationPacket(17, 95, 50.3),
                new RotationPacket(17, 96, 51.3),
                new RotationPacket(17, 97, 52.3),
                new RotationPacket(17, 98, 53.3),
            };
        }
    }
}