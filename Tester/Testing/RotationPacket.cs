namespace Tester.Testing
{
    public class RotationPacket
    {
        public int playerId;
        public double yaw, pitch;

        public RotationPacket(int playerId, double yaw, double pitch)
        {
            this.playerId = playerId;
            this.yaw = yaw;
            this.pitch = pitch;
        }
    }
}