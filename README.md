# StreamDeck.NET
[![Latest Nuget Verison](https://img.shields.io/nuget/v/StreamDeck.Net.svg)](https://www.nuget.org/packages/StreamDeck.Net/)

> A .NET Core wrapper for the Elgato StreamDeck SDK

NOTE: This project is still in the early stages of development.

StreamDeck.Net provides an events based .NET Core wrapper around the [Elgato Stream Deck](https://www.elgato.com/en/gaming/stream-deck)

## Table of Contents

- [Sample Program](#sample-program)
- [Local Development](#local-development)
- [Install](#install)
- [Contribute](#contribute)
- [Thanks](#thanks)

## Sample program

Below shows how StreamDeck.NET can be used to create a simple counter action. It uses [natemcmaster's excellent CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) for parsing command line arguments.

```c#
namespace Counter
{
    public class Program
    {
        [Option(Description = "The port the Elgato StreamDeck software is listening on", ShortName = "port")]
        public string Port { get; set; }

        [Option(Description = "The Unique Identifier used to reference the plugin", ShortName = "pluginUUID")]
        public string PluginUuid { get; set; }

        [Option(Description = "Event type used to register the plugin", ShortName = "registerEvent")]
        public string RegisterEvent { get; set; }

        [Option(Description = "String of JSON containing appliaction and device information", ShortName = "info")]
        public string Info { get; set; }

        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);


        private async Task OnExecuteAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var streamDeck = new StreamDeckClient(Port, PluginUuid, RegisterEvent, Info);
            streamDeck.KeyDownEventAsync += UpdateCounter;
            await streamDeck.RunAsync(cancellationTokenSource.Token);
        }

        private static int _counter;

        private static async Task UpdateCounter(StreamDeckClient client, StreamDeckEventPayload args)
        {
            _counter++;
            await client.SetTitle(args.Context, _counter.ToString());
        }
    }
}
```

## Local Development

To develop locally StreamDeck.Net requires:
 - .NET Core 3.0 SDK +

Simply clone this repositry and Nuget Restore the solution.

## Install

> dotnet add package StreamDeck.Net

For ease of debuugging you can set your plugins local publish profiles to point to the StreamDeck softwares plugin folder. Then you can just re-run the software and attach to process to step through your code.

## Contribute
If a feature is missing, or you can improve the code please submit a PR or raise an Issue. For substantial chnages or additions it's best to open an Issue first to discuss the need for the feature and potential implementations.

## Thanks
A special thanks to [Jeffrey T. Fritz (@csharpfritz)](https://github.com/csharpfritz) whose [stream](https://www.youtube.com/watch?v=IOLylhVGpM8) inspired this project. The code he implemented in [StreamDeck_First](https://github.com/csharpfritz/StreamDeck_First) was initially used as the base for this project before undergoing heavy refactoring.
