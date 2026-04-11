using CommandLine;
using ConstanceSaveEditor.Cli.Commands;

namespace ConstanceSaveEditor.Cli;

internal sealed class Program
{
    private static async Task<int> Main(string[] args)
    {
        return await Parser.Default
            .ParseArguments<ExtractOptions, RebuildOptions, ShowOptions, ValidateOptions>(args)
            .MapResult(
                (ExtractOptions opts) => ExtractCommand.RunAsync(opts),
                (RebuildOptions opts) => RebuildCommand.RunAsync(opts),
                (ShowOptions opts) => ShowCommand.RunAsync(opts),
                (ValidateOptions opts) => ValidateCommand.RunAsync(opts),
                _ => Task.FromResult(1));
    }
}