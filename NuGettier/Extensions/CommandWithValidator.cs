using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Reflection;

namespace NuGettier;

internal static class CommandWithValidatorExtension
{
    public static Command WithValidator(this Command command, ValidateSymbolResult<CommandResult> validationMethod)
    {
        command.AddValidator(validationMethod);
        return command;
    }

    public static Command WithValidator(this Command command, string methodName)
    {
        var method = typeof(Program).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        return command.WithValidator((commandResult) => method!.Invoke(null, [commandResult]));
    }
}
