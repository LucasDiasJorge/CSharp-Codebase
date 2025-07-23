# MySimpleSdk

MySimpleSdk is a simple SDK for interacting with APIs. It provides a client for making API calls, models for data representation, and services for business logic.

## Getting Started

To get started with MySimpleSdk, follow the instructions below.

### Installation

You can install MySimpleSdk via NuGet Package Manager or by using the .NET CLI.

### Usage

Here is a brief overview of how to use the SDK:

1. Create an instance of `SdkClient`.
2. Use the `GetData()` and `PostData()` methods to interact with the API.
3. Handle any exceptions using `SdkException`.

## Publishing to NuGet

To publish your SDK to NuGet, follow these steps:

1. **Create a NuGet account** at [nuget.org](https://www.nuget.org/).
2. **Add the NuGet package metadata** to your `MySimpleSdk.csproj` file. Include properties like:
   ```xml
   <PropertyGroup>
       <Id>MySimpleSdk</Id>
       <Version>1.0.0</Version>
       <Authors>Your Name</Authors>
       <Description>A simple SDK for interacting with APIs.</Description>
   </PropertyGroup>
   ```
3. **Build the project in Release mode**:
   ```
   dotnet build --configuration Release
   ```
4. **Create the NuGet package** using the command:
   ```
   dotnet pack --configuration Release
   ```
5. **Publish the package** to NuGet using the command:
   ```
   dotnet nuget push <package>.nupkg -k <your_api_key> -s https://api.nuget.org/v3/index.json
   ```

Replace `<package>` with the name of your generated `.nupkg` file and `<your_api_key>` with your NuGet API key.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue.

## License

This project is licensed under the MIT License. See the LICENSE file for details.