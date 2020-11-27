# dotnet-graphql-tye

Using ChilliCream HotChocolate GraphQL Stitching as well as Microsoft Tye for a possible Hypomos API implementation.

Based on samples and code found here:
- https://chillicream.com/docs/hotchocolate/v10/stitching
- https://github.com/ChilliCream/hotchocolate-examples/blob/master/misc/Stitching/federated-with-hot-reload/reviews/ReviewRepository.cs
- https://github.com/dotnet/tye/tree/master/samples/frontend-backend

For a nice development experience added a vscode launch config (Known Issue: it still asks if debugging should continue, just press yes and it attaches it-self to all APIs as well as the ApiGateway). Ensure that you have selected the "All" compound launch setting.

If you add further APIs, you will also have to adapt the `launch.json` and `tye.yaml` files accordingly. And configure the connection string in `appsettings.Development.json` in `ApiGateway` accordingly.

To run the solution with VisualStudio, you need to run a redis DB on port 7000 by using e.g.: `docker run --name redis-stitching -p 7000:6379 -d redis`

If running the solution via `tye run` you can access the GraphQL Banana Cake UI via: http://localhost:8080/graphql/ (and the tye dashboard with lots more interesting informations via http://localhost:8000) - otherwise if you run the solution through Visual Studio, it will be reachable via https://localhost:5001/graphql/

On the Banana Cake UI you may use following query to get started:
``` graphql
{
  me {
    storages{
      name

      metaDatas {
        name
        rating
      }
    }
  }
}
```

which shall result in:
``` json
{
  "data": {
    "me": {
      "storages": [
        {
          "name": "OneDrive",
          "metaDatas": []
        },
        {
          "name": "Azure Blob Storage",
          "metaDatas": [
            {
              "name": "MyImage.jpg",
              "rating": 3.5
            }
          ]
        }
      ]
    }
  }
}
```