# Introduction
I have a few reasons why I want to use docker. If I was hosting my application on a Windows Server running on IIS, I would also have IIS also installed locally as well. The first reason is I would like to have the almost same configuration on my local IIS as the IIS running on a server. Secondly, once I've finished my round of development (coding, unit testing, etc..); I would like to deploy my web application stack onto docker and then run UI automation testing, once everything is good then I can deploy it onto the Windows Server. 

I would like to following the same principals with hosting my application stack on NGINX and Docker. Right now, this step is just for making sure everything is going to work as expected with my current application stack and will build upon this further later. After having a bit of reading, my initial goal of hosting both the front end and backend from one docker image with NGINX is possible but not the recommended practice, each needs to be on it's own containerised application, so I am going to follow the best practices first (Seriously, I just host database servers on Docker and nothing else, I'm still a noob with Docker 😅). Most of the places that I have worked, a web stack (usually worked with IIS) where both frontend and backend is hosted on the same server.

And since this is project is based on hospital CRM, end-to-end encryption needs to apply. I understand this will apply a significant load on the server(s) or containers, but hey nobody said working with healthcare data is going to be easy 😁.

Before I start creating a Docker file for the backend, I need to add some and make changes code to the application and these changes are related for running the application on NGINX. 

# Configuring and Adding Forward Headers 
The first thing I need to do is to add the Forward Request middleware and the reason for adding the Forward Request middleware is that NGINX will forward the request to the ASP.NET Core/Kestrel backend. I've added the Forward Request middleware just after the intialising the `app` variable step.  

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

As per [Host ASP.NET Core on Linux with Nginx | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-10.0&tabs=linux-ubuntu), this is due to proxies running loopback addresses such as 127.0.0.0/8, [::1] and the standard localhost address (127.0.0.1), are trusted by default. This will most likely change when I deploy the app onto docker and need to add a configure step in the ASP.NET Core pipeline with additional `ForwardedHeadersOptions` in a production environment. But getting back to not seeing any forward request headers, I need to add a configuration to my [vite.config.ts](../HospitalProject.Client/vite.config.ts) which will add the forward request headers flag.   
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
        agent: new https.Agent({
          ca: readFileSync(certPath)
        }),
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

So now the request with forward headers are coming through! But there is a catch to this. http-proxy-3 is sending 4 headers these are:  
* `x-forwarded-for`
* `x-forwarded-port`
* `x-forwarded-host`
* `x-forwarded-proto`

When the request comes into ASP.NET Core, the `x-forwarded-for` & `x-forwarded-proto` headers are replaced with `X-Original-For` and `X-Original-Proto` respectively with the old values of `HttpContext.Connection.RemoteIpAddress` and `HttpContext.Request.Scheme`. This means that `x-forwarded-port` & `x-forwarded-host` is not being processed, and for `x-forwarded-host` this can be processed if I add the `ForwardedHeaders.XForwardedHost` flag to the Forward Header Middleware, and for `x-forwarded-port`, I might have to do extra processing when the request is sent in. But for now this is good enough. I'll see what happens when I setting and testing the deployment to docker.

# Docker Configuration
The quickest way to get started with creating a Docker Image on VS Code is using the plugin [Container Tools](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-containers) with the command palette selecting the option "Containers: Add Docker Files to Workspace", but since I don't know Docker very well. Although, the files were generated by the command palette, after playing around with each section and step, I had to customise it so that ASP.NET Core app will run in it's own container. The command palette also generated the VS code task and launch configurations, but I don't want to use all of them and when it comes to do UI automation testing, I plan to invoke docker apps from command line. The keywords `FROM` AND `AS` are used quite a bit. `FROM` specifies the base image to start from and `AS` gives the stage a name. 

## ASP.NET Core Dockerfile And appsettings.json Configuration
The command palette generated the following docker file below:
```dockerfile
  FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
  WORKDIR /app

  USER app
  FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
  ARG configuration=Release
  WORKDIR /src
  COPY ["HospitalProject.Server/HospitalProject.Server.csproj", "HospitalProject.Server/"]
  RUN dotnet restore "HospitalProject.Server/HospitalProject.Server.csproj"
  COPY . .
  WORKDIR "/src/HospitalProject.Server"
  RUN dotnet build "HospitalProject.Server.csproj" -c $configuration -o /app/build

  FROM build AS publish
  ARG configuration=Release
  RUN dotnet publish "HospitalProject.Server.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

  FROM base AS final
  WORKDIR /app
  COPY --from=publish /app/publish .
  ENTRYPOINT ["dotnet", "HospitalProject.Server.dll"]
``` 

One problem was the `COPY . .` command copied everything from the workspace directory into the image, which I did not want. So I modified the [Dockerfile](../HospitalProject.Server/Dockerfile) and this the code that I am running for creating the image:  
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
USER app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["HospitalProject.Server/HospitalProject.Server.csproj", "."]
RUN dotnet restore "HospitalProject.Server.csproj"
COPY HospitalProject.Server/ .
RUN dotnet build "HospitalProject.Server.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "HospitalProject.Server.csproj" -c $configuration -o /app/publish -p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalProject.Server.dll"]
```

In the first two lines `FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base` downloads and sets up the run time and `WORKDIR /app` command sets the working directory to **app** for the ASP.NET Core runtime. And lastly, `USER app` changes the user from a root account to app account for security reasons to run the entire image. And the app user is part of the ASP.NET Core image. This means the build happens with root but the application runs under app user.

The next couple of lines is the build step `FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build` targets the build to the specific OS with the `--platform=$BUILDPLATFORM`, with `ARG configuration=Release` is essentially a parameter that is available during the image building process, when you generate the the Dockerfiles with the command palette command, it also generates YAML files for production and debug release, debug release came in handy when I couldn't initially launch the app and I was able to troubleshoot and find out where I went wrong. And then working directory is changed with `WORKDIR /src` to the **src** directory where the binaries will reside. Next is the `COPY ["HospitalProject.Server/HospitalProject.Server.csproj", "."]`, which copies the csproj file from the host stores in it in the image. And then the `RUN dotnet restore "HospitalProject.Server/HospitalProject.Server.csproj"` command is ran to restore nuget packages and other dependencies as well. At this stage, Docker seems to cache from `WORKDIR /src` step to now. This is useful for when reusing the image for subsequent builds. And I copy the rest of the contents over from **HospitalProject.Server** directory locally to the **src** directory on the image and then lastly run the command `RUN dotnet build "HospitalProject.Server.csproj" -c $configuration -o /app/build`, which deploys a Release build in the **/app/build** directory in the image.

The section starts off with `FROM build AS publish` where this build called publish will be based off from the image named build. Again the command `ARG configuration=Release` sets the configuration parameter to Release. And then lastly `RUN dotnet publish "HospitalProject.Server.csproj" -c $configuration -o /app/publish -p:UseAppHost=false` command does a publish build into the **/app/publish** directory. There's a flag at the end `/p:UseAppHost=false` which basically means it will skip generating a native executable wrapper, since the container will invoke `dotnet` directly. At this point, I can still see the directories, **src**, **/app/build** and **/app/publish**, when I look through the Docker debugger.  
```cmd
         ▄                                                                                                                                       
     ▄ ▄ ▄  ▀▄▀                                                                                                                                  
   ▄ ▄ ▄ ▄ ▄▇▀  █▀▄ █▀█ █▀▀ █▄▀ █▀▀ █▀█                                                                                                          
  ▀████████▀    █▄▀ █▄█ █▄▄ █ █ ██▄ █▀▄                                                                                                          
   ▀█████▀                        DEBUG                                                                                                          
                                                                                                                                                 
Builtin commands:                                                                                                                                
- install [tool1] [tool2] ...    Add Nix packages from: https://search.nixos.org/packages                                                        
- uninstall [tool1] [tool2] ...  Uninstall NixOS package(s).                                                                                     
- entrypoint                     Print/lint/run the entrypoint.                                                                                  
- builtins                       Show builtin commands.                                                                                          
                                                                                                                                                 
Checks:                                                                                                                                          
✓ distro:            Ubuntu 24.04.4 LTS                                                                                                          
✓ entrypoint linter: no errors (run 'entrypoint' for details)                                                                                    
                                                                                                                                                 
Note: This is a sandbox shell. All changes will not affect the actual container.                                                                 
                                                                                                                                  Version: 0.0.47
root@91ff76a04c73 /src [compassionate_heisenberg]
docker > ls
Controllers  HospitalProject.Server.csproj  Program.cs  WeatherForecast.cs            appsettings.json  obj
Dockerfile   Logs                           Properties  appsettings.Development.json  bin
root@91ff76a04c73 /src [compassionate_heisenberg]
docker > cd /app 
root@91ff76a04c73 /app [compassionate_heisenberg]
docker > ls
build  publish
root@91ff76a04c73 /app [compassionate_heisenberg]
docker > 
```

The last section, uses the base image with the command `FROM base AS final`. With the working directory set to the **app** directory with the command `WORKDIR /app` as the working directory and then copies the content from the publish build with the command `COPY --from=publish /app/publish .` into **app** removes all three directories and the only directory remaining is the **app** with the binaries of the publish build.  
```cmd
         ▄                                                                                                                                       
     ▄ ▄ ▄  ▀▄▀                                                                                                                                  
   ▄ ▄ ▄ ▄ ▄▇▀  █▀▄ █▀█ █▀▀ █▄▀ █▀▀ █▀█                                                                                                          
  ▀████████▀    █▄▀ █▄█ █▄▄ █ █ ██▄ █▀▄                                                                                                          
   ▀█████▀                        DEBUG                                                                                                          
                                                                                                                                                 
Builtin commands:                                                                                                                                
- install [tool1] [tool2] ...    Add Nix packages from: https://search.nixos.org/packages                                                        
- uninstall [tool1] [tool2] ...  Uninstall NixOS package(s).                                                                                     
- entrypoint                     Print/lint/run the entrypoint.                                                                                  
- builtins                       Show builtin commands.                                                                                          
                                                                                                                                                 
Checks:                                                                                                                                          
✓ distro:            Ubuntu 24.04.4 LTS                                                                                                          
✓ entrypoint linter: no errors (run 'entrypoint' for details)                                                                                    
                                                                                                                                                 
This is an attach shell, i.e.:                                                                                                                   
- Any changes to the container filesystem are visible to the container directly.                                                                 
- The /nix directory is invisible to the actual container.                                                                                       
                                                                                                                                  Version: 0.0.47
root@3214a0523726 /app [gifted_bhaskara]
docker > cd / 
root@3214a0523726 / [gifted_bhaskara]
docker > ls
app  bin  boot  dev  etc  home  lib  media  mnt  nix  opt  proc  root  run  sbin  srv  sys  tmp  usr  var
root@3214a0523726 / [gifted_bhaskara]
docker > cd /app
root@3214a0523726 /app [gifted_bhaskara]
docker > ls
HospitalProject.Server.deps.json                       Microsoft.OpenApi.dll               Serilog.Sinks.Debug.dll
HospitalProject.Server.dll                             Serilog.AspNetCore.dll              Serilog.Sinks.File.dll
HospitalProject.Server.pdb                             Serilog.Extensions.Hosting.dll      Serilog.dll
HospitalProject.Server.runtimeconfig.json              Serilog.Extensions.Logging.dll      appsettings.Development.json
HospitalProject.Server.staticwebassets.endpoints.json  Serilog.Formatting.Compact.dll      appsettings.json
Microsoft.AspNetCore.OpenApi.dll                       Serilog.Settings.Configuration.dll  web.config
Microsoft.Extensions.DependencyModel.dll               Serilog.Sinks.Console.dll
root@3214a0523726 /app [gifted_bhaskara]
```

I have been running the command from the terminal `docker build -t hospital.project.server -f HospitalProject.Server/Dockerfile .` to build the docker container image and also use Docker Desktop to inspect the image with the debugger console.  
![Inspecting Docker Container Image](./images/Screenshot%202026-03-10%20at%2010.42.14 am.png)

If you have a look at the container, there's no ports and you cannot hit the API endpoints either, because I have not exposed a port on docker for ASP.NET core traffic to go through.  
![Hospital Project Server running on Docker](./images/Screenshot%202026-03-10%20at%2010.51.19 am.png)

Running this command `docker run -d -p 127.0.0.1:8080:8080 hospital.project.server`, I am able to launch my Docker image and as of right now, it's running with HTTP protocol not HTTPS.  
```cmd
> docker run -d -p 127.0.0.1:8080:8080 hospital.project.server
e77a8acc3a715be04d2130ab4433a76af22c1c9f539d9707a2879deb0abb8312
> curl http://localhost:8080/weatherforecast
[{"date":"2026-03-14","temperatureC":23,"temperatureF":73,"summary":"Hot"},{"date":"2026-03-15","temperatureC":39,"temperatureF":102,"summary":"Chilly"},{"date":"2026-03-16","temperatureC":-19,"temperatureF":-2,"summary":"Warm"},{"date":"2026-03-17","temperatureC":-18,"temperatureF":0,"summary":"Balmy"},{"date":"2026-03-18","temperatureC":36,"temperatureF":96,"summary":"Cool"}]%                                                                                                                                                                     
> curl https://localhost:8080/weatherforecast
curl: (35) LibreSSL/3.3.6: error:1404B42E:SSL routines:ST_CONNECT:tlsv1 alert protocol version
```

Maybe I should have coded the SSL/TLS & HTTPS configurations within the code in [Program.cs](../HospitalProject.Server/Program.cs), but no, I'm not going to do. The main reason is deployment flexibility. You can use environment variables and configurations in [appsettings.json](../HospitalProject.Server/appsettings.json) (but going to run the docker images via command line with environment variables for now, which I'll do next) and you might need to add some extra code to get some of the stuff working the way you want from reading the config file too, which I am know I will do later.

So I'm going to run docker with some environment variables and make sure it's using the HTTPS protocol. But the first thing I need to do is generate a certificate, this time a *.pfx certificate for the ASP.NET Core backend with `dotnet dev-certs https -ep ~/Workspaces/Certs/dotnet/hospitalproject.server.pfx -p ********` (replace `********` with your password).  
```cmd
> dotnet dev-certs https -ep ~/Workspaces/Certs/dotnet/hospitalproject.server.pfx -p ********
A valid HTTPS certificate is already present.
```

Now, I'm going to launch docker with all the environment variables and also mounting my file locally to docker with this command:  
```
% docker run -d -p 127.0.0.1:5229:5229 \
-e ASPNETCORE_URLS="https://+:5229" \
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/hospitalproject.server.pfx \
-e ASPNETCORE_Kestrel__Certificates__Default__Password="********" \  
-e ASPNETCORE_ENVIRONMENT=Development \
-v ~/Workspaces/Certs/dotnet/hospitalproject.server.pfx:/https/hospitalproject.server.pfx:ro \
hospital.project.server

45691e19b16a1b9a0b1e044519d69e3f8a42a339cf364213208af03511c11439
```
And then when I run my `curl`, I get a response back with HTTPS and not HTTP.  
```cmd
> curl https://localhost:5229/weatherforecast
[{"date":"2026-03-14","temperatureC":-10,"temperatureF":15,"summary":"Cool"},{"date":"2026-03-15","temperatureC":-17,"temperatureF":2,"summary":"Cool"},{"date":"2026-03-16","temperatureC":6,"temperatureF":42,"summary":"Cool"},{"date":"2026-03-17","temperatureC":53,"temperatureF":127,"summary":"Scorching"},{"date":"2026-03-18","temperatureC":51,"temperatureF":123,"summary":"Mild"}]%                                                                                                                                 
> curl http://localhost:5229/weatherforecast 
curl: (52) Empty reply from server
> curl http://localhost:8080/weatherforecast
curl: (7) Failed to connect to localhost port 8080 after 0 ms: Couldn't connect to server
```

Now, I should have added the HTTP url and with the `app.UseHttpsRedirection()` middleware, it should redirect to HTTPS, but at the moment, decided not to, because I don't need to due to going to use my reverse proxy only to use the HTTPS port. Secondly, I have created another problem where I'm not logging things out to my laptop, but will fix that later. And I should create a *.yaml file for my deployment too because running the command line command is painful 😅. And maybe I shouldn't have ran Docker with the `-d` flag and create multiple containers and also add the environment variable `ASPNETCORE_ENVIRONMENT=Development` (I got lazy, just wanted ASP.NET Core to use [appsettings.Development.json](../HospitalProject.Server/appsettings.Development.json) logging configuration 😜), since this is technically this is one-off run, but I wanted to keep the containers and see what the different configurations or where I screwed up with my command (and yes, I screwed up the commands a few times) by comparing the different containers.

But before I start fixing and creating *.yaml files, the next thing I need to do is the ReactJS deployment with NGINX and also configure NGINX work as a my reverse proxy as well and make sure everything is working!

# References
* [Host ASP.NET Core on Linux with Nginx | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-10.0&tabs=linux-ubuntu)
* [Configure ASP.NET Core to work with proxy servers and load balancers | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0#forwarded-headers)
* [ForwardedHeaders Enum (Microsoft.AspNetCore.HttpOverrides) | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.httpoverrides.forwardedheaders?view=aspnetcore-10.0)
* [sagemathinc/http-proxy-3: Modern rewrite of node-proxy (the original nodejs http proxy server)](https://github.com/sagemathinc/http-proxy-3?tab=readme-ov-file#options)
* [Secure your .NET cloud apps with rootless Linux Containers - .NET Blog](https://devblogs.microsoft.com/dotnet/securing-containers-with-rootless/)
* [.NET application publishing overview - .NET | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/deploying/?pivots=visualstudio)
* [Writing a Dockerfile | Docker Docs](https://docs.docker.com/get-started/docker-concepts/building-images/writing-a-dockerfile/)
* [Dockerfile reference | Docker Docs](https://docs.docker.com/reference/dockerfile/)
* [Best practices | Docker Docs](https://docs.docker.com/build/building/best-practices/)
* [Part 1: Containerize an application | Docker Docs](https://docs.docker.com/get-started/workshop/02_our_app/)
* [Hosting ASP.NET Core image in container using docker compose with HTTPS | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-10.0)