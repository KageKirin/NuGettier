using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Reflection;

namespace NuGettier;

internal static class CommandWithHandlerExtension
{
    public static Command WithHandler(this Command command, ICommandHandler handler)
    {
        command.Handler = handler;
        return command;
    }

    public static Command WithHandler(this Command command, string methodName)
    {
        var method = typeof(Program).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        return command.WithHandler(CommandHandler.Create(method!));
    }
}
