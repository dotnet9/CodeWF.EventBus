﻿using CodeWF.EventBus.Tests.Commands;
using CodeWF.EventBus.Tests.Queries;

namespace CodeWF.EventBus.Tests.Handlers
{
    internal class StaticHandler
    {
        public static int TestCount = 0;

        [EventHandler]
        public static void ReceiveAddCommand(TestAddCommand command)
        {
            TestCount++;
        }

        [EventHandler]
        public static void ReceiveSubtractCommand(TestSubtractCommand command)
        {
            TestCount--;
        }

        [EventHandler]
        public static void ReceiveStaticQuery(TestQuery query)
        {
            query.Result = TestCount;
        }
    }
}