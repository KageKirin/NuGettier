using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;

namespace NuGettier;

internal static class CommandResultValidateMethods
{
    public static void ValidateEitherOf(this CommandResult commandResult, Option a, Option b)
    {
        if (
            commandResult.FindResultFor(a) is not null
            && //
            commandResult.FindResultFor(b) is not null
        )
        {
            commandResult.ErrorMessage = $"Either option '--{a.Name}' or '--{b.Name}' can be used.";
        }
    }

    static string GenerateOnlyOneOfErrorText(params Option[] options)
    {
        Debug.Assert(options.Length >= 2);

        var names = options.Select(o => $"'--{o.Name}'").ToArray();
        var list =
            names.Length == 2
                ? $"{names[0]} or {names[1]}"
                : string.Join(", ", names[0..(names.Length - 1)]) + ", or " + names[^1];
        return $"Only one of the options {list} is allowed.";
    }

    public static void ValidateOnlyOneOf(this CommandResult commandResult, params Option[] options)
    {
        Debug.Assert(options.Length >= 2);

        if (options.Count(option => commandResult.FindResultFor(option) is not null) != 1)
        {
            commandResult.ErrorMessage = GenerateOnlyOneOfErrorText(options);
        }
    }
}
