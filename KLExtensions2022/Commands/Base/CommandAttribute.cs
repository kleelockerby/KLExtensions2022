using System;

namespace KLExtensions2022.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(int commandId) : this("00000000-0000-0000-0000-000000000000", commandId) { }

        public CommandAttribute(string commandGuid, int commandId)
        {
            Guid = new Guid(commandGuid);
            Id = commandId;
        }

        public Guid Guid { get; set; }

        public int Id { get; set; }
    }
}
