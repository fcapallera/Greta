# GretaBot
This bot is made from Bot Framework v4 core bot sample.

This bot has been created using [Bot Framework][1] and NLU services like [QnAmaker][2] and [LUIS (Europe)][2].

## Prerequisites
- [.NET Core SDK][4] version 2.1
	```bash
	# determine dotnet version
	dotnet --version
	```

# To run this bot locally
- If you want to run this Bot locally you'll have to ask for the `appsettings.json` file to the project owner. Without it you won't be able to get the configuration data. (IE. some external services).

## Visual Studio
- Open the .sln file with Visual Studio.
- Run the project (press `F5` key)

## .NET Core CLI
- Install the [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x). 
- Using the command line, navigate to your project folder.
- Type `dotnet run`.

# Testing the bot using Bot Framework Emulator 
[Bot Framework Emulator][5] is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.3.0 or greater from [here][6]

## Connect to the bot using Bot Framework Emulator
- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`


[1]: https://dev.botframework.com
[2]: https://www.qnamaker.ai/
[3]: https://eu.luis.ai/home
[4]: https://dotnet.microsoft.com/download
[5]: https://github.com/microsoft/botframework-emulator
[6]: https://github.com/Microsoft/BotFramework-Emulator/releases
[7]: https://docs.microsoft.com/cli/azure/?view=azure-cli-latest
[8]: https://docs.microsoft.com/cli/azure/install-azure-cli?view=azure-cli-latest
[9]: https://github.com/Microsoft/botbuilder-tools/tree/master/packages/MSBot
[10]: https://portal.azure.com
[11]: https://www.luis.ai
[12]: https://docs.microsoft.com/en-us/ef/#pivot=entityfmwk

