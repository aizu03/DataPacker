namespace Tester.Testing
{
    internal class ReferenceProblem
    {
        public int num;
        public SomeChild someChild;
        public ReferenceProblem ptr;

        public ReferenceProblem()
        {
            num = 15;
            ptr = this;
            someChild = new SomeChild(this);
        }
    }

    internal class SomeChild
    {
        public readonly ReferenceProblem stackOverflow;
        public readonly SomeChild me;
        public string solved = "yes";

        public SomeChild(ReferenceProblem stackOverflow)
        {
            this.stackOverflow = stackOverflow;
            me = this;
        }
    }
}
