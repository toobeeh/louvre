## Typo API Client Generation
On build, the typo API client will be automatically generated.  
To manually generate the client code, follow these steps:

1. Install the nswag cli tool: `dotnet tool install --global NSwag.ConsoleCore `
2. Generate client code: `nswag openapi2csclient /input:https://api.typo.rip/openapi.json /output:TypoClient.cs /namespace:tobeh.Louvre.TypoApiClient`