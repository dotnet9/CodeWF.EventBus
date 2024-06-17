using CodeWF.EventBus.Tests.Commands;
using CodeWF.EventBus.Tests.Queries;

namespace CodeWF.EventBus.Tests.Handlers
{
    internal class StaticHandler2
    {
        [EventHandler]
        public static void ReceiveAddCommand(TestAddCommand command)
        {
            StaticHandler.TestCount++;
        }

        [EventHandler]
        public static void ReceiveSubtractCommand(TestSubtractCommand command)
        {
            StaticHandler.TestCount--;
        }

        [EventHandler]
        public static void ReceiveStaticQuery(TestQuery query)
        {
            query.Result = StaticHandler.TestCount;
        }
    }
}