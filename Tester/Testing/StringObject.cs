namespace Tester.Testing
{
    public class StringObject
    {
        private readonly string str;

        public StringObject(string str)
        {
            this.str = str;
        }

        public override bool Equals(object? obj)
        {
            return obj is StringObject so && so.GetHashCode().Equals(GetHashCode());
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }
    }
}