# dotnet-graphql-tye

Using ChilliCream HotChocolate GraphQL Stitching as well as Microsoft Tye for a possible Hypomos API implementation.

Based on samples and code found here:
- https://chillicream.com/docs/hotchocolate/v10/stitching
- https://github.com/ChilliCream/hotchocolate-examples/blob/master/misc/Stitching/federated-with-hot-reload/reviews/ReviewRepository.cs
- https://github.com/dotnet/tye/tree/master/samples/frontend-backend

For a nice development experience added a vscode launch config (Known Issue: it still asks if debugging should continue, just press yes and it attached it-self to all APIs as well as the ApiGateway). Ensure that you have selected the "All" compound launch setting.

If you add further APIs, you will also have to adapt the `launch.json` and `tye.yaml` files accordingly.
