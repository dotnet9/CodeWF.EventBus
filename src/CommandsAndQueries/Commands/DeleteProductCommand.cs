using System;
using CodeWF.EventBus;

namespace CommandsAndQueries.Commands
{
    public class DeleteProductCommand : Command
    {
        public Guid ProductId { get; set; }
    }
}