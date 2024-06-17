namespace CodeWF.EventBus.Tests.Queries
{
    public class TestQuery : Query<int>
    {
        public override int Result { get; set; }
    }
}