## Running C# API locally

1. Add the required secrets to the .NET Core secret manager. See [Variables](#Variables) below for descriptions.
    ```bash
    cd ./api/csharp
    dotnet user-secrets set "DirectLine:DirectLineSecret" "YOUR-DIRECT-LINE-SECRET-HERE"
    ```
1. (optional) Change the port specified in `./Properties/launchSettings.json`.
1. Run `dotnet run` to start the server. (Alternatively, open and run the project in Visual Studio.)

## Variables

| Variable | Description | Example value |
| -------- | ----------- | ------------- |
| `DirectLine:DirectLineSecret` | The Direct Line secret issued by Bot Framework. Can be found in the Azure Bot Channels Registration resource after enabling the Direct Line channel. |  |