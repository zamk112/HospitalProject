# Introduction
I have a few reasons why I want to use docker. If I was hosting my application on a Windows Server running on IIS, I would also have IIS also installed locally as well. The first reason is I would like to have the almost same configuration on my local IIS as the IIS running on a server. Secondly, once I've finished my round of development (coding, unit testing, etc..); I would like to deploy my web application stack onto docker and then run UI automation testing, once everything is good then I can deploy it onto the Windows Server. 

I would like to following the same principals with hosting my application stack on NGINX and Docker. Right now, this step is just for making sure everything is going to work as expected with my current application stack and will build upon this further later.

The goal is to host one docker image where both the frontend and backend will be running from. Before I start creating a Docker file for the backend, I need to add some and make changes code to the application and these changes are related for running the application on NGINX. 

## Configuring and Adding Forward Headers 
The first thing I need to do is to add the Forward Request middleware and the reason for adding the Forward Request middleware is that NGINX will forward the request to the ASP.NET Core backend. I've added the Forward Request middleware just after the intialising the `app` variable step.  

```csharp
    ...
    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    ...
```

Let's see what happens when I launch both frontend and backend locally and see if anything is broken. At the moment the page is loading with the API call to retrieve the weather data, but I do not see the forward request headers being sent with the request.  
```
2026-03-08 14:29:40 [INF] Now listening on: https://localhost:7083
2026-03-08 14:29:40 [INF] Application started. Press Ctrl+C to shut down.
2026-03-08 14:29:40 [INF] Hosting environment: Development
2026-03-08 14:29:40 [INF] Content root path: /Users/zamk/Projects/HospitalProject/HospitalProject.Server
HospitalProject.Server.dll (24099): Loaded '/usr/local/share/dotnet/shared/Microsoft.NETCore.App/10.0.2/System.Net.WebSockets.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
2026-03-08 14:29:47 [INF] Request starting HTTP/1.1 GET https://localhost:5173/weatherforecast - null null
2026-03-08 14:29:47 [INF] Request:
Protocol: HTTP/1.1
Method: GET
Scheme: https
PathBase: 
Path: /weatherforecast
Accept: */*
Connection: close
Host: localhost:5173
User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36
Accept-Encoding: gzip, deflate, br, zstd
Accept-Language: en-GB,en-US;q=0.9,en;q=0.8
Referer: https://localhost:5173/
sec-ch-ua-platform: "macOS"
sec-ch-ua: "Not:A-Brand";v="99", "Google Chrome";v="145", "Chromium";v="145"
sec-ch-ua-mobile: ?0
sec-fetch-site: same-origin
sec-fetch-mode: cors
sec-fetch-dest: empty
priority: u=1, i
2026-03-08 14:29:47 [INF] Executing endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-03-08 14:29:47 [INF] Route matched with {action = "Get", controller = "WeatherForecast"}. Executing controller action with signature System.Collections.Generic.IEnumerable`1[HospitalProject.Server.WeatherForecast] Get() on controller HospitalProject.Server.Controllers.WeatherForecastController (HospitalProject.Server).
2026-03-08 14:29:47 [INF] Executing ObjectResult, writing value of type 'HospitalProject.Server.WeatherForecast[]'.
2026-03-08 14:29:47 [INF] Response:
StatusCode: 200
Content-Type: application/json; charset=utf-8
HospitalProject.Server.dll (24099): Loaded '/usr/local/share/dotnet/shared/Microsoft.AspNetCore.App/10.0.2/Microsoft.AspNetCore.WebUtilities.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
2026-03-08 14:29:47 [INF] Executed action HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server) in 16.8436ms
2026-03-08 14:29:47 [INF] Executed endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-03-08 14:29:47 [INF] ResponseBody: [{"date":"2026-03-09","temperatureC":0,"temperatureF":32,"summary":"Scorching"},{"date":"2026-03-10","temperatureC":-6,"temperatureF":22,"summary":"Cool"},{"date":"2026-03-11","temperatureC":33,"temperatureF":91,"summary":"Balmy"},{"date":"2026-03-12","temperatureC":-7,"temperatureF":20,"summary":"Chilly"},{"date":"2026-03-13","temperatureC":-2,"temperatureF":29,"summary":"Bracing"}]
2026-03-08 14:29:47 [INF] Duration: 30.1192ms
2026-03-08 14:29:47 [INF] HTTP GET /weatherforecast responded 200 in 33.0523 ms
2026-03-08 14:29:47 [INF] Request finished HTTP/1.1 GET https://localhost:5173/weatherforecast - 200 null application/json; charset=utf-8 47.4682ms
```

As per [Host ASP.NET Core on Linux with Nginx | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-10.0&tabs=linux-ubuntu), this is due to proxies running loopback addresses such as 127.0.0.0/8, [::1] and the standard localhost address (127.0.0.1), are trusted by default. This will most likely change when I deploy the app onto docker and need to add a configure step in the ASP.NET Core pipeline with additional `ForwardedHeadersOptions`. But getting back to not seeing any forward request headers, I need to add a configuration to my [vite.config.ts](../HospitalProject.Client/vite.config.ts) which will add the forward request headers flag.   
```ts
...
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    https: {
      key: readFileSync(keyPath),
      cert: readFileSync(certPath)
    },
    proxy: {
      '^/weatherforecast': {
        target: target,
        secure: true,
        xfwd: true
      }
    }
  }
});
```

Now that I have done is add the `xfwd` flag, the proxy will add the headers when sending the request back to the server. After restarting both the front and backends, now I can see the forward request headers in the logs even though it is redacted.  
```
2026-03-08 15:06:46 [INF] Request starting HTTP/1.1 GET https://localhost:5173/weatherforecast - null null
2026-03-08 15:06:46 [INF] Request:
Protocol: HTTP/1.1
Method: GET
Scheme: https
PathBase: 
Path: /weatherforecast
Accept: */*
Connection: close
Host: localhost:5173
User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36
Accept-Encoding: gzip, deflate, br, zstd
Accept-Language: en-GB,en-US;q=0.9,en;q=0.8
Referer: https://localhost:5173/
sec-ch-ua-platform: "macOS"
sec-ch-ua: "Not:A-Brand";v="99", "Google Chrome";v="145", "Chromium";v="145"
sec-ch-ua-mobile: ?0
sec-fetch-site: same-origin
sec-fetch-mode: cors
sec-fetch-dest: empty
priority: u=1, i
X-Original-Proto: [Redacted]                                 <---- HERE!
x-forwarded-port: [Redacted]                                 <---- HERE!
x-forwarded-host: [Redacted]                                 <---- HERE!
X-Original-For: [Redacted]                                   <---- HERE!
2026-03-08 15:06:46 [INF] Executing endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-03-08 15:06:46 [INF] Route matched with {action = "Get", controller = "WeatherForecast"}. Executing controller action with signature System.Collections.Generic.IEnumerable`1[HospitalProject.Server.WeatherForecast] Get() on controller HospitalProject.Server.Controllers.WeatherForecastController (HospitalProject.Server).
2026-03-08 15:06:46 [INF] Executing ObjectResult, writing value of type 'HospitalProject.Server.WeatherForecast[]'.
2026-03-08 15:06:46 [INF] Response:
StatusCode: 200
Content-Type: application/json; charset=utf-8
2026-03-08 15:06:46 [INF] Executed action HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server) in 1.358ms
2026-03-08 15:06:46 [INF] Executed endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-03-08 15:06:46 [INF] ResponseBody: [{"date":"2026-03-09","temperatureC":12,"temperatureF":53,"summary":"Mild"},{"date":"2026-03-10","temperatureC":39,"temperatureF":102,"summary":"Scorching"},{"date":"2026-03-11","temperatureC":34,"temperatureF":93,"summary":"Freezing"},{"date":"2026-03-12","temperatureC":23,"temperatureF":73,"summary":"Chilly"},{"date":"2026-03-13","temperatureC":44,"temperatureF":111,"summary":"Freezing"}]
2026-03-08 15:06:46 [INF] Duration: 2.3513ms
2026-03-08 15:06:46 [INF] HTTP GET /weatherforecast responded 200 in 2.9145 ms
2026-03-08 15:06:46 [INF] Request finished HTTP/1.1 GET https://localhost:5173/weatherforecast - 200 null application/json; charset=utf-8 3.5612ms
```

So now the request with forward headers are definitely coming through!

# References
* [Host ASP.NET Core on Linux with Nginx | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-10.0&tabs=linux-ubuntu)
* [sagemathinc/http-proxy-3: Modern rewrite of node-proxy (the original nodejs http proxy server)](https://github.com/sagemathinc/http-proxy-3?tab=readme-ov-file#options)