# Http Caching Demo using ETag

`DelegatingHandler` for caching Http calls based on the ETag from a provider's API.

## Configuring Application

Set the `ETagClient` settings in `appsettings.json`:

```
"ETagClient": {
    "BaseAddress": "https://localhost:7145/",
    "UrlPath": "/api/healthcheck"
}
```

Override the `Caching` defaults in `appsettings.json`:

```
"Caching": {
    "CacheDurationInSeconds": 300   // 5 minutes
}
```

## Running Application

```
dotnet run --project Cache.Demo.Api/Cache.Demo.Api.csproj
```

## Testing Application

Get request - returns `200 - OK`:

```
curl -v --location https://localhost:7230/api/test
```

Ignore caching responses - returns `200 - OK`:

```
curl -v --location https://localhost:7230/api/test --header 'X-Cache-Bust:true'
```