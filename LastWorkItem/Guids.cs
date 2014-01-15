// Guids.cs
// MUST match guids.h
using System;

namespace CDuke.LastWorkItem
{
    static class GuidList
    {
        public const string guidLastWorkItemPkgString = "fcc0c8fb-b738-4516-9e06-0cfd95d828ed";
        public const string guidLastWorkItemCmdSetString = "867d07b4-8b65-4f54-8544-6c69eb907a4a";

        public static readonly Guid guidLastWorkItemCmdSet = new Guid(guidLastWorkItemCmdSetString);
    };
}