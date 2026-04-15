# Introduction
I have a few reasons why I want to use docker. If I was hosting my application on a Windows Server running on IIS, I would also have IIS also installed locally as well. The first reason is I would like to have the almost same configuration on my local IIS as the IIS running on a server. Secondly, once I've finished my round of development (coding, unit testing, etc..); I would like to deploy my web application stack onto docker and then run UI automation testing, once everything is good then I can deploy it onto the Windows Server. 

I would like to following the same principals with hosting my application stack on NGINX and Docker. Right now, this step is just for making sure everything is going to work as expected with my current application stack and will build upon this further later. After having a bit of reading, my initial goal of hosting both the front end and backend from one docker image with NGINX is possible but not the recommended practice, each needs to be on it's own containerised application, so I am going to follow the best practices first (Seriously, I just host database servers on Docker and nothing else, I'm still a noob with Docker 😅). Most of the places that I have worked, a web stack (usually worked with IIS) where both frontend and backend is hosted on the same server.

And since this is project is based on hospital CRM, end-to-end encryption needs to apply. I understand this will apply a significant load on the server(s) or containers, but hey nobody said working with healthcare data is going to be easy 😁.

Before I start creating a Docker file for the backend, I need to add some and make changes code to the application and these changes are related for running the application on NGINX. 

A good starting point document for NGINX that I found was [How To Deploy a React Application with Nginx on Ubuntu | DigitalOcean](https://www.digitalocean.com/community/tutorials/deploy-react-application-with-nginx-on-ubuntu). It really help out a lot in terms of understanding how NGINX works.

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

So Docker is going to be my staging environment. And after that UAT (probably going to host on a VM..... haven't decided yet).

## ASP.NET Core Dockerfile And appsettings.json Configuration
But before I do build the ASP.NET Core image, I'm going to create a new **appsettings.json** file called [appsettings.Staging.json](../HospitalProject.Server/appsettings.Staging.json), it's basically the same as [appsettings.Development.json](../HospitalProject.Server/appsettings.Development.json). The command palette generated the following docker file below:
```dockerfile
  FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
  WORKDIR /app

  USER app
  FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
  ARG configuration=Release
  WORKDIR /src
  COPY ["HospitalProject.Server.csproj", "HospitalProject.Server/"]
  RUN dotnet restore "HospitalProject.Server.csproj"
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


In the first two lines `FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base` downloads and sets up the run time and `WORKDIR /app` command sets the working directory to **app** for the ASP.NET Core runtime. And lastly, `USER app` changes the user from a root account to app account for security reasons to run the entire image. And the app user is part of the ASP.NET Core image. This means the build happens with root but the application runs under app user.

The next couple of lines is the build step `FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build` targets the build to the specific OS with the `--platform=$BUILDPLATFORM`, with `ARG configuration=Release` is essentially a parameter that is available during the image building process, when you generate the the Dockerfiles with the command palette command, it also generates YAML files for production and debug release, debug release came in handy when I couldn't initially launch the app and I was able to troubleshoot and find out where I went wrong. And then working directory is changed with `WORKDIR /src` to the **src** directory where the binaries will reside. Next is the `COPY ["HospitalProject.Server.csproj", "."]`, which copies the csproj file from the host stores in it in the image. And then the `RUN dotnet restore "HospitalProject.Server.csproj"` command is ran to restore nuget packages and other dependencies as well. At this stage, Docker seems to cache from `WORKDIR /src` step to now. This is useful for when reusing the image for subsequent builds. And I copy the rest of the contents over from **HospitalProject.Server** directory locally to the **src** directory on the image and then lastly run the command `RUN dotnet build "HospitalProject.Server.csproj" -c $configuration -o /app/build`, which deploys a Release build in the **/app/build** directory in the image.

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
HospitalProject.Server.runtimeconfig.json              Serilog.Extensions.Logging.dll      web.config
HospitalProject.Server.staticwebassets.endpoints.json  Serilog.Formatting.Compact.dll 
Microsoft.AspNetCore.OpenApi.dll                       Serilog.Settings.Configuration.dll
Microsoft.Extensions.DependencyModel.dll               Serilog.Sinks.Console.dll
root@3214a0523726 /app [gifted_bhaskara]
```

Before running the build command, I excluded any kind of **appsettings.json** files from being copied over to the docker container and instead going to mount it in the **app** directory in the container from my host host machine. And going to do this with [.dockerignore](../HospitalProject.Server/.dockerignore) file.  
```
.dockerignore
Dockerfile
appsettings.json
appsettings.*.json
```

And the reason for excluding the **appsettings.json** files is because I want to be able to make changes to my configuration without deploying a new image and container. I have been running the command from the terminal `docker build -t hospital.project.server -f HospitalProject.Server/Dockerfile --pull HospitalProject.Server` to build the docker container image and also use Docker Desktop to inspect the image with the debugger console.  
![Inspecting Docker Container Image](./images/Screenshot%202026-03-10%20at%2010.42.14 am.png)

Just one thing I'd like to mention with my `docker build` command is that I added the `--pull` argument, so the build context can reference all docker related config files such as **.dockerignore** file, which I have not included for this build (but I have in the ReactJS and NGINX) build.

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

Maybe I should have coded the SSL/TLS & HTTPS configurations within the code in [Program.cs](../HospitalProject.Server/Program.cs), but no, I'm not going to do just yet (later when I build out the complete docker deployment or close to it). You can use environment variables and configurations in [appsettings.json](../HospitalProject.Server/appsettings.json) (but going to run the docker images via command line with environment variables for now, which I'll do next) and you might need to add some extra code to get some of the stuff working the way you want from reading the config file too, which I am know I will do later.

So I'm going to run docker with some environment variables and make sure it's using the HTTPS protocol. But the first thing I need to do is generate a certificate, this time a *.pfx certificate for the ASP.NET Core backend with `dotnet dev-certs https -ep ~/Workspaces/Certs/dotnet/hospitalproject.server.pfx -p ********` (replace `********` with your password).  
```cmd
> dotnet dev-certs https -ep ~/Workspaces/Certs/dotnet/hospitalproject.server.pfx -p ********
A valid HTTPS certificate is already present.
```

Now, I'm going to launch docker with all the environment variables and also mounting my file locally to docker with this command:  
```
> docker run -d -p 127.0.0.1:5229:5229 \
-e ASPNETCORE_URLS="https://+:5229" \
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/hospitalproject.server.pfx \
-e ASPNETCORE_Kestrel__Certificates__Default__Password="********" \
-e ASPNETCORE_ENVIRONMENT=Staging \
-v ~/Workspaces/Certs/dotnet/hospitalproject.server.pfx:/https/hospitalproject.server.pfx:ro \
-v ~/Projects/HospitalProject/HospitalProject.Server/appsettings.Staging.json:/app/appsettings.Staging.json:ro \
-v ~/Projects/HospitalProject/HospitalProject.Server/appsettings.json:/app/appsettings.json:ro \
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

Maybe I should have added the HTTP url and with the `app.UseHttpsRedirection()` middleware, it should redirect to HTTPS, but at the moment, decided not to, because I don't need to due to going to use my reverse proxy only to use the HTTPS port. 

But before I start fixing and creating *.yaml files, the next thing I need to do is the ReactJS deployment with NGINX and also configure NGINX work as a my reverse proxy as well and make sure everything is working!

## Node.js & NGINX Dockerfile
First thing I'm going to talk about is the [Dockerfile](../HospitalProject.Client/Dockerfile), it's really straightforward (at the moment).  
```dockerfile
ARG NODE_VERSION=current-alpine
ARG NGINX_VERSION=mainline-alpine

FROM node:${NODE_VERSION} AS builder
WORKDIR /app
COPY ["package.json", "package-lock.json", "./"]
RUN --mount=type=cache,target=/root/.npm npm ci
COPY . .
RUN npm run build

FROM nginx:${NGINX_VERSION} AS runner
COPY --chown=nginx:nginx --from=builder /app/dist /usr/share/nginx/html
USER nginx
EXPOSE 443
ENTRYPOINT ["nginx", "-c", "/etc/nginx/nginx.conf"]
CMD ["-g", "daemon off;"]
``` 

This [Dockerfile](../HospitalProject.Client/Dockerfile), I hand crafted and also added a [.dockerignore](../HospitalProject.Client/.dockerignore) file, which excludes the following content:   
```
node_modules
README.md
nginx.conf
*.nginx.conf
Dockerfile
.dockerignore
```

Based on the arguments that I have set at the beginning, I am using the current and stable version of both Node.js and NGIX respectively (Real World scenario would be to pick a specific version and stick to it until the newer version is completely tested, but I just want to see what breaks with the latest stable release that comes out).   
```dockerfile
ARG NODE_VERSION=current-alpine
ARG NGINX_VERSION=mainline-alpine
...
```

Initially `NGINX_VERSION` was `stable-alpine`, but for some reason this image does not have support for HTTP 2 for when doing reverse proxy API calls to the backend. 

The next part which is the building the ReactJS app is quite interesting:  
```dockerfile
...
FROM node:${NODE_VERSION} AS builder
WORKDIR /app
COPY ["package.json", "package-lock.json", "./"]
RUN --mount=type=cache,target=/root/.npm npm ci
COPY . .
RUN npm run build
...
```

### Node.js Configuration
The first three lines are pretty straight forward, using the node image as our builder, setting the work directory to **app** and then copying the [package.json](../HospitalProject.Client/package.json) & [package-lock.json](../HospitalProject.Client/package-lock.json) files. But the next line `RUN --mount=type=cache,target=/root/.npm npm ci` is interesting because `npm ci` uses the [package-lock.json](../HospitalProject.Client/package-lock.json) to install all the dependencies and this command caches it, which is different to how caching is done for the ASP.NET Core Docker image. 

But once the dependencies are install, everything else is copied excluding the content from [.dockerignore](../HospitalProject.Client/.dockerignore) file and then the `RUN npm run build` builds the ReactJS app which is ready for deployment.

At this stage and excluding the NGINX image setup. If I ran my command just up for the NodeJS build, it will fail. 
```cmd
> docker build -t hospital.project.client -f HospitalProject.Client/Dockerfile --pull HospitalProject.Client/
[+] Building 3.7s (11/11) FINISHED                                                                                                                                                                                                                                         docker:desktop-linux
 => [internal] load build definition from Dockerfile                                                                                                                                                                                                                                       0.0s
 => => transferring dockerfile: 501B                                                                                                                                                                                                                                                       0.0s
 => [internal] load metadata for docker.io/library/node:current-alpine                                                                                                                                                                                                                     2.0s
 => [auth] library/node:pull token for registry-1.docker.io                                                                                                                                                                                                                                0.0s
 => [internal] load .dockerignore                                                                                                                                                                                                                                                          0.0s
 => => transferring context: 93B                                                                                                                                                                                                                                                           0.0s
 => [builder 1/6] FROM docker.io/library/node:current-alpine@sha256:ad82ecad30371c43f4057aaa4800a8ed88f9446553a2d21323710c7b937177fc                                                                                                                                                       0.0s
 => => resolve docker.io/library/node:current-alpine@sha256:ad82ecad30371c43f4057aaa4800a8ed88f9446553a2d21323710c7b937177fc                                                                                                                                                               0.0s
 => [internal] load build context                                                                                                                                                                                                                                                          0.0s
 => => transferring context: 1.73kB                                                                                                                                                                                                                                                        0.0s
 => CACHED [builder 2/6] WORKDIR /app                                                                                                                                                                                                                                                      0.0s
 => CACHED [builder 3/6] COPY [package.json, package-lock.json, ./]                                                                                                                                                                                                                        0.0s
 => CACHED [builder 4/6] RUN --mount=type=cache,target=/root/.npm npm ci                                                                                                                                                                                                                   0.0s
 => [builder 5/6] COPY . .                                                                                                                                                                                                                                                                 0.0s
 => ERROR [builder 6/6] RUN npm run build                                                                                                                                                                                                                                                  1.6s
------
 > [builder 6/6] RUN npm run build:
0.315 
0.315 > hospitalproject-client@0.0.0 build
0.315 > tsc -b && vite build
0.315 
1.582 failed to load config from /app/vite.config.ts
1.585 error during build:
1.585 Error: Certificate not found.
1.585     at file:///app/node_modules/.vite-temp/vite.config.ts.timestamp-1775236837411-a2379bebc0ccb8.mjs:15:9
1.585     at ModuleJob.run (node:internal/modules/esm/module_job:437:25)
1.585     at async node:internal/modules/esm/loader:639:26
1.585     at async loadConfigFromBundledFile (file:///app/node_modules/vite/dist/node/chunks/config.js:35909:12)
1.585     at async bundleAndLoadConfigFile (file:///app/node_modules/vite/dist/node/chunks/config.js:35797:17)
1.585     at async loadConfigFromFile (file:///app/node_modules/vite/dist/node/chunks/config.js:35764:42)
1.585     at async resolveConfig (file:///app/node_modules/vite/dist/node/chunks/config.js:35413:22)
1.585     at async createBuilder (file:///app/node_modules/vite/dist/node/chunks/config.js:33875:19)
1.585     at async CAC.<anonymous> (file:///app/node_modules/vite/dist/node/cli.js:629:10)
------
Dockerfile:9
--------------------
   7 |     RUN --mount=type=cache,target=/root/.npm npm ci
   8 |     COPY . .
   9 | >>> RUN npm run build
  10 |     
  11 |     # FROM nginx:${NGINX_VERSION} AS runner
--------------------
ERROR: failed to build: failed to solve: process "/bin/sh -c npm run build" did not complete successfully: exit code: 1
```

It is failing because of how I setup SSL in [vite.config.ts](../HospitalProject.Client/vite.config.ts), so I need to tweak the code a little bit.  
```ts
...
export default defineConfig(({ command }) => {
  const certName = 'hospitalproject.client';
  const certFolder = path.join(os.homedir(), 'Workspaces', 'Certs', 'dotnet');
  const certPath = path.join(certFolder, `${certName}.pem`);
  const keyPath = path.join(certFolder, `${certName}.key`);  

  if (command === 'serve' && (!existsSync(certPath) || !existsSync(keyPath))) {
    throw new Error('Certificate not found.');
  }

  return {
    plugins: [react()],
    server: command === 'serve' ? {
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
    } : undefined,
  }
});
``` 

What I have done here passing through the `command` object in `defineConfig(...)` and checking whether the `command` object value `serve` or `build`, if it is `serve` then it should check for certificates and set the server/proxy configuration otherwise, `server` attribute value is set to `undefined` because I want `npm run build` to ignore the server/proxy configuration. If I run the build command again then the build succeeds:  
```
> docker build -t hospital.project.client -f HospitalProject.Client/Dockerfile --pull HospitalProject.Client/
[+] Building 5.6s (12/12) FINISHED                                                                                                                                                                                                                                         docker:desktop-linux
 => [internal] load build definition from Dockerfile                                                                                                                                                                                                                                       0.0s
 => => transferring dockerfile: 501B                                                                                                                                                                                                                                                       0.0s
 => [internal] load metadata for docker.io/library/node:current-alpine                                                                                                                                                                                                                     2.1s
 => [auth] library/node:pull token for registry-1.docker.io                                                                                                                                                                                                                                0.0s
 => [internal] load .dockerignore                                                                                                                                                                                                                                                          0.0s
 => => transferring context: 93B                                                                                                                                                                                                                                                           0.0s
 => [builder 1/6] FROM docker.io/library/node:current-alpine@sha256:ad82ecad30371c43f4057aaa4800a8ed88f9446553a2d21323710c7b937177fc                                                                                                                                                       0.0s
 => => resolve docker.io/library/node:current-alpine@sha256:ad82ecad30371c43f4057aaa4800a8ed88f9446553a2d21323710c7b937177fc                                                                                                                                                               0.0s
 => [internal] load build context                                                                                                                                                                                                                                                          0.0s
 => => transferring context: 1.85kB                                                                                                                                                                                                                                                        0.0s
 => CACHED [builder 2/6] WORKDIR /app                                                                                                                                                                                                                                                      0.0s
 => CACHED [builder 3/6] COPY [package.json, package-lock.json, ./]                                                                                                                                                                                                                        0.0s
 => CACHED [builder 4/6] RUN --mount=type=cache,target=/root/.npm npm ci                                                                                                                                                                                                                   0.0s
 => [builder 5/6] COPY . .                                                                                                                                                                                                                                                                 0.0s
 => [builder 6/6] RUN npm run build                                                                                                                                                                                                                                                        2.7s
 => exporting to image                                                                                                                                                                                                                                                                     0.7s
 => => exporting layers                                                                                                                                                                                                                                                                    0.1s
 => => exporting manifest sha256:2e06f73873d624f992c02e3121db116a92db0a7b959c81c6e7875292d18acba5                                                                                                                                                                                          0.0s
 => => exporting config sha256:40131840dc0a8bbb63d19db37c072e2a8a961e3eb385112495c5e91caea6471d                                                                                                                                                                                            0.0s
 => => exporting attestation manifest sha256:54683bc2596b006709f428744f689f1ba8c6fa65af714f0f462c56fc64ed868f                                                                                                                                                                              0.0s
 => => exporting manifest list sha256:d2b9df2023bf7619f232809a2ec1807a83d5ebf54c914babd9b90b6ae54ff260                                                                                                                                                                                     0.0s
 => => naming to docker.io/library/hospital.project.client:latest                                                                                                                                                                                                                          0.0s
 => => unpacking to docker.io/library/hospital.project.client:latest
```

And if you create the container from the image without passing any parameters and check the Debug tab in the container, you can see the the files being copied over excluding the files and folders mentioned in [.dockerignore](../HospitalProject.Client/.dockerignore).  
```
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
✓ distro:            Alpine Linux v3.23                                                                                   
✓ entrypoint linter: no errors (run 'entrypoint' for details)                                                             
                                                                                                                          
Note: This is a sandbox shell. All changes will not affect the actual container.                                          
                                                                                                           Version: 0.0.47
root@714d2f6d22e8 /app [sad_mccarthy]
docker > ls 
dist              index.html    package-lock.json  public  tsconfig.app.json  tsconfig.node.json
eslint.config.js  node_modules  package.json       src     tsconfig.json      vite.config.ts
root@714d2f6d22e8 /app [sad_mccarthy]
```

### NGIX configuration
The next part which is for NGINX image is pretty straight forward:  
```dockerfile
FROM nginx:${NGINX_VERSION} AS runner
COPY --chown=nginx:nginx --from=builder /app/dist /usr/share/nginx/html
USER nginx
EXPOSE 443
ENTRYPOINT ["nginx", "-c", "/etc/nginx/nginx.conf"]
CMD ["-g", "daemon off;"]
``` 

So for the NGINX image, copying the **/app/dist** directory from the NodeJS build into **/usr/share/nginx/html** with all of its content owner and group to nginx. And NGINX runs under the nginx user, with port exposed to 443 and running nginx with my config file, although I did not copy the config file and just mounting it onto the image from my machine locally when I create and run the container via command line (which I will show shortly). 

But before I start running the container, I want to discuss the **nginx.conf** or [staging.nginx.conf](../HospitalProject.Client/staging.nginx.conf) from the host directory.  
```
worker_processes auto;

pid /tmp/nginx.pid;

events {}

http {
    include /etc/nginx/mime.types;

    client_body_temp_path   /tmp/client_temp;
    proxy_temp_path         /tmp/proxy_temp;
    fastcgi_temp_path       /tmp/fastcgi_temp;
    uwsgi_temp_path         /tmp/uwsgi_temp;
    scgi_temp_path          /tmp/scgi_temp;

    server
    {
        listen 443 ssl;
        http2 on;
        server_name localhost;

        ssl_certificate         /etc/nginx/certs/hospitalproject.client.pem;
        ssl_certificate_key     /etc/nginx/certs/hospitalproject.client.key;

        root /usr/share/nginx/html;
        index index.html;

        location /weatherforecast {
            proxy_pass https://172.17.0.2:5229;

            proxy_set_header Host               $host;
            proxy_set_header X-Forwarded-Proto  $scheme;
            proxy_set_header X-Forwarded-For    $proxy_add_x_forwarded_for;

            proxy_ssl_verify              off;
            proxy_ssl_trusted_certificate /etc/nginx/certs/hospitalproject.server.pem;
        }
    }
}
```

This is actually the bare minimum that I need to run both NGIX and ReactJS, worker process (4 cores on linux) and worker connections (512 worker connections) are pretty much defaulting to the NGIX default values. The server is using SSL with HTTP2 with the server name being **localhost**. And I also had to add `include /etc/nginx/mime.types;` otherwise the server does not dish out the CSS, HTML and JavaScript files to the browser. Before adding the following configuration to the file. NGIX failed to start because it needed these directories:  
```
    client_body_temp_path   /tmp/client_temp;
    proxy_temp_path         /tmp/proxy_temp;
    fastcgi_temp_path       /tmp/fastcgi_temp;
    uwsgi_temp_path         /tmp/uwsgi_temp;
    scgi_temp_path          /tmp/scgi_temp;
    ...
```

Just removing the last entry `scgi_temp_path /tmp/scgi_temp;`, would give you this error:   
```cmd
2026/04/04 01:11:15 [emerg] 1#1: mkdir() "/var/cache/nginx/scgi_temp" failed (13: Permission denied)
nginx: [emerg] mkdir() "/var/cache/nginx/scgi_temp" failed (13: Permission denied)
```

It is trying to create a docker in **/var/cache/nginx/scgi_temp** but failing that is why I've pointed everything to the **/tmp** directory and this error message is very similar if not almost the same as the preceding configurations before it. As you can see I'm binding SSL certificates to the frontend but I have disabled SSL certification for my ASP.NET Core API endpoint with `proxy_ssl_verify off;` and the URL contains the IP address of the ASP.NET Core web API endpoint. 

I had to do this because I just wanted to make sure that my docker containers were talking to each other which they are! Next, I will need to do some networking configurations, and generating custom certificates (using the OpenSSL tool to do) and then enable SSL verification. But right now, I need to rebuild my image.  
```cmd
> docker build -t hospital.project.client -f HospitalProject.Client/Dockerfile --pull HospitalProject.Client/
[+] Building 2.2s (16/16) FINISHED                                                                                                                                                                                                                                         docker:desktop-linux
 => [internal] load build definition from Dockerfile                                                                                                                                                                                                                                       0.0s
 => => transferring dockerfile: 489B                                                                                                                                                                                                                                                       0.0s
 => [internal] load metadata for docker.io/library/node:current-alpine                                                                                                                                                                                                                     1.9s
 => [internal] load metadata for docker.io/library/nginx:stable-alpine                                                                                                                                                                                                                     1.9s
 => [auth] library/node:pull token for registry-1.docker.io                                                                                                                                                                                                                                0.0s
 => [auth] library/nginx:pull token for registry-1.docker.io                                                                                                                                                                                                                               0.0s
 => [internal] load .dockerignore                                                                                                                                                                                                                                                          0.0s
 => => transferring context: 93B                                                                                                                                                                                                                                                           0.0s
 => [builder 1/6] FROM docker.io/library/node:current-alpine@sha256:ad82ecad30371c43f4057aaa4800a8ed88f9446553a2d21323710c7b937177fc                                                                                                                                                       0.0s
 => => resolve docker.io/library/node:current-alpine@sha256:ad82ecad30371c43f4057aaa4800a8ed88f9446553a2d21323710c7b937177fc                                                                                                                                                               0.0s
 => [internal] load build context                                                                                                                                                                                                                                                          0.0s
 => => transferring context: 622B                                                                                                                                                                                                                                                          0.0s
 => [runner 1/2] FROM docker.io/library/nginx:stable-alpine@sha256:a8b39bd9cf0f83869a2162827a0caf6137ddf759d50a171451b335cecc87d236                                                                                                                                                        0.0s
 => => resolve docker.io/library/nginx:stable-alpine@sha256:a8b39bd9cf0f83869a2162827a0caf6137ddf759d50a171451b335cecc87d236                                                                                                                                                               0.0s
 => CACHED [builder 2/6] WORKDIR /app                                                                                                                                                                                                                                                      0.0s
 => CACHED [builder 3/6] COPY [package.json, package-lock.json, ./]                                                                                                                                                                                                                        0.0s
 => CACHED [builder 4/6] RUN --mount=type=cache,target=/root/.npm npm ci                                                                                                                                                                                                                   0.0s
 => CACHED [builder 5/6] COPY . .                                                                                                                                                                                                                                                          0.0s
 => CACHED [builder 6/6] RUN npm run build                                                                                                                                                                                                                                                 0.0s
 => CACHED [runner 2/2] COPY --chown=nginx:nginx --from=builder /app/dist /usr/share/nginx/html                                                                                                                                                                                            0.0s
 => exporting to image                                                                                                                                                                                                                                                                     0.1s
 => => exporting layers                                                                                                                                                                                                                                                                    0.0s
 => => exporting manifest sha256:de51bd7defe3fdbf22ffab9477b88101cf6fc94503cb5ea227ab0141c4131221                                                                                                                                                                                          0.0s
 => => exporting config sha256:86a639840d160b15458417404f6de67d0192514b79dc152fa023f5aace0dd216                                                                                                                                                                                            0.0s
 => => exporting attestation manifest sha256:a8ef689e8b613c96a26a1f038286c8c21c54ee202fc39f2b05686b56e7cf743a                                                                                                                                                                              0.0s
 => => exporting manifest list sha256:ba681b789250398ceb1fac6debb200c26e90dc7cc379145011cbed53c7d1c0a4                                                                                                                                                                                     0.0s
 => => naming to docker.io/library/hospital.project.client:latest                                                                                                                                                                                                                          0.0s
 => => unpacking to docker.io/library/hospital.project.client:latest     
```

And then going to create and run my docker image.  
```
> docker run -d -p 127.0.0.1:443:443 \                                                                     
-v ~/Projects/HospitalProject/HospitalProject.Client/staging.nginx.conf:/etc/nginx/nginx.conf:ro \
-v ~/Workspaces/Certs/dotnet/hospitalproject.client.key:/etc/nginx/certs/hospitalproject.client.key:ro \
-v ~/Workspaces/Certs/dotnet/hospitalproject.client.pem:/etc/nginx/certs/hospitalproject.client.pem:ro \
-v ~/Workspaces/Certs/dotnet/hospitalproject.server.pem:/etc/nginx/certs/hospitalproject.server.pem:ro \
hospital.project.client
878fcd247a623247d76d072d34a143c675634d05fb39e15a00b0f952b776ed50
```

Again, all the certificates and the NGINX configuration files are being mounted onto the container. And since all the certificates are generated with the dotnet tool I just copied **hospitalproject.client.pem** and renamed it to **hospitalproject.server.pem**.

In Docker desktop, in the Debug tab of the front end app. I can see the contents of my ReactJS app that `npm run build` command generated in the **/usr/share/nginx/html** directory.  
```
/ $ ls /usr/share/nginx/html
50x.html    assets      index.html  vite.svg
/ $ ls /usr/share/nginx/html/assets
index-BfOgabEj.js   index-COcDBgFa.css  react-CHdo91hT.svg
```

And then when loading the page on the browser, I can see NGIX is capturing the request.  
```
172.17.0.1 - - [04/Apr/2026:01:45:44 +0000] "GET /weatherforecast HTTP/2.0" 200 387 "https://127.0.0.1/" "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"
```

And so is the ASP.NET Core container.  
```
2026-04-04 01:46:10 [INF] Application started! Logging to both console and/or file.
2026-04-04 01:46:10 [WRN] Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'https://+:5229'.
2026-04-04 01:46:10 [INF] Now listening on: https://[::]:5229
2026-04-04 01:46:10 [INF] Application started. Press Ctrl+C to shut down.
2026-04-04 01:46:10 [INF] Hosting environment: Staging
2026-04-04 01:46:10 [INF] Content root path: /app
2026-04-04 01:46:17 [INF] Request starting HTTP/1.0 GET https://localhost/weatherforecast - null null
2026-04-04 01:46:17 [INF] Request:
Protocol: HTTP/1.0
Method: GET
Scheme: https
PathBase: 
Path: /weatherforecast
Accept: */*
Connection: close
Host: localhost
User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
Accept-Encoding: gzip, deflate, br, zstd
Accept-Language: en-GB,en-US;q=0.9,en;q=0.8
Referer: https://127.0.0.1/
X-Forwarded-Proto: [Redacted]
X-Forwarded-For: [Redacted]
sec-ch-ua-platform: "macOS"
sec-ch-ua: "Chromium";v="146", "Not-A.Brand";v="24", "Google Chrome";v="146"
sec-ch-ua-mobile: ?0
sec-fetch-site: same-origin
sec-fetch-mode: cors
sec-fetch-dest: empty
priority: u=1, i
```

In the ASP.NET Core logs, I can see `X-Forwarded-Proto` and `X-Forwarded-For` are being pass through but it is not getting processed and this is most likely due to SSL verification being disabled and the connection is HTTP 1.0 but I want HTTP 2.0, I'll tackle these next along with the networking & custom SSL certificate generation next.

# Docker Networking and Custom SSL configuration
What I want to do now is a create a subnet where both ASP.NET Core backend and the NGINX front end running in the subnet and then only expose the NGIX frontend. I will say that you might not need to do SSL bindings and verification for the ASP.NET Core backend since everything is being reversed proxied from the frontend and the backend is not exposed. 

Sure if the web stack was running on a VM not on Docker, you will need to do additional network/firewall configuration and expose the API endpoint. But I'm doing it firstly, for the sake of doing it 😜 and secondly, I guess if the company or organisation is security conscious heavy they would still want to encrypt the communication and maximise their security resilience.

## Create Docker Networking Bridge Adapter
First things first, I'm going to create a Docker networking bridge adapter with the subnet using private class A IP address range of `10.0.0.0/24`, which should give me 254 IP addresses with the following command.  
```
> docker network create \
  --driver bridge \
  --subnet 10.0.0.0/28 \
  --gateway 10.0.0.1 \
  hospital-network
08d6cde7561967a7483e8c04602dd98ec71ff0e1fba57901623560433b97517b

> docker network ls
NETWORK ID     NAME               DRIVER    SCOPE
2204768764b3   bridge             bridge    local
08d6cde75619   hospital-network   bridge    local
b05970dd2650   host               host      local
ac6cf5a7474b   none               null      local
```

With the `docker network ls` command, just confirming that the network bridge adapter is created. Running `docker network inspect hospital-network` command, I got 13 usable IP addresses, 3 of them used up due for IP Gateway, IP Broadcast and IP Network addresses.  
```
> docker network inspect hospital-network
[
    {
        "Name": "hospital-network",
        "Id": "8fd1c57688843574d9d25269d936e53bf8f38777c9791c54b66669d00757d193",
        "Created": "2026-04-04T09:13:19.587354719Z",
        "Scope": "local",
        "Driver": "bridge",
        "EnableIPv4": true,
        "EnableIPv6": false,
        "IPAM": {
            "Driver": "default",
            "Options": {},
            "Config": [
                {
                    "Subnet": "10.0.0.0/28",
                    "Gateway": "10.0.0.1"
                }
            ]
        },
        "Internal": false,
        "Attachable": false,
        "Ingress": false,
        "ConfigFrom": {
            "Network": ""
        },
        "ConfigOnly": false,
        "Options": {
            "com.docker.network.enable_ipv4": "true",
            "com.docker.network.enable_ipv6": "false"
        },
        "Labels": {},
        "Containers": {},
        "Status": {
            "IPAM": {
                "Subnets": {
                    "10.0.0.0/28": {
                        "IPsInUse": 3,
                        "DynamicIPsAvailable": 13
                    }
                }
            }
        }
    }
]
```

Before I start running the Docker containers, I'm going to generate the SSL certificates, since both backend and frontend.

## Custom Self-Signed SSL Certificates
For both front end and backend, I'm going to generating an X509 certificate, this is due the certificates being self signed.

But I still need to add the CA certificate regardless to the macOS trust store in order to get rid of the SSL error message that appears on the browser.

### Custom CA for SSL Certificates
So the first thing I need to do, is use [HospitalProject.ca.conf](../HospitalProject.ca.conf) to generate the CA certificates.  
```
[req]
distinguished_name = req_distinguished_name
x509_extensions    = v3_ca
prompt             = no

[req_distinguished_name]
CN = HospitalProjectCA

[v3_ca]
basicConstraints = critical, CA:true
keyUsage         = critical, keyCertSign, cRLSign
nameConstraints  = critical, permitted;DNS:localhost,permitted;DNS:hospitalproject.api.local,permitted;DNS:hospitalproject.local
subjectKeyIdentifier   = hash
authorityKeyIdentifier = keyid:always, issuer
```

The CA name is *HospitalProjectCA* and I'm restricting the CA to generate other certificate based on the subnet that I setup for Docker earlier and also permitting localhost and the DNS names that I will set when launching the containers (since I need to connect the React app from the browser). 

In the `req` section, the `distinguished_name` is set from the `req_distinguished_name` section which is the Common Name (`CN`), the `x509_extensions` values comes from the `v3_ca` and `prompt = no` means no interactive prompting.

In the `v3_ca` section: 
* `basicConstraints` set to `CA:true` means that the certificate is marked for Certificate Authority.
* `keyCertSign` signifies that the CA will be signing certificates and `cRLSign` signifies that it grants the CA the ability to sign the Certificate Revocation List (CRL) itself, since I don't have the CRL infrastructure setup (and I can't on my MacBook Air at least, otherwise my laptop will die 😅). I need to generate the key for my CA certificate.
* `nameConstraints` basically restricts the scope of a CA certificate to the DNS names that I have provided. 
* `subjectKeyIdentifier` is an extension that provides a means to identifying certificates that contains of the public key. Since I am generating a CA certificate OpenSSL will generate a 160-bit SHA-1 hash of the BIT STRING value of the subject public key, excluding the tag, length, and number of unused bits.
* `authorityKeyIdentifier` is the configuration that mandates that the Authority Key Identifier (AKI) extension must include the Key ID, while the Issuer name and serial number are included only if the certificate is not self-signed. And this is the case since this certificate is a certificate authority. 

Another thing to note is `basicConstraints`, `keyUsage` and `nameConstraints` has the `critical` flag, what this means that it cannot be ignored by the validator, if the certificate is not a CA.

I'm generating the CA key with -aes256 encryption and it will prompt you to enter a pass phrase.

```
> openssl genrsa -aes256 -out ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key 2048
> Enter PEM pass phrase:
> Verifying - Enter PEM pass phrase:
```

Once the key file is generated, next I need to generate the certificate using the [HospitalProject.ca.conf](../HospitalProject.ca.conf) file. And again you will have to enter a pass phrase.
```
> openssl req -x509 -new -nodes \
  -key ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key \
  -sha256 -days 365 \
  -config ~/Projects/HospitalProject/HospitalProject.ca.conf \
  -out ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
> Enter pass phrase for /Users/zamk/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key:
```

So now, both the certificate files are generated.
```
> ls ~/Workspaces/Certs/hospital.project/ca 
HospitalProject.CA.key  HospitalProject.CA.pem
```

And just to verify, everything from the config file was applied.  
```
> openssl x509 -in ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem -noout -text | grep -A 6 "X509v3"
        X509v3 extensions:
            X509v3 Basic Constraints: critical
                CA:TRUE
            X509v3 Key Usage: critical
                Certificate Sign, CRL Sign
            X509v3 Name Constraints: critical
                Permitted:
                  DNS:localhost
                  DNS:hospitalproject.api.local
                  DNS:hospitalproject.local
            X509v3 Subject Key Identifier:
                ...
            X509v3 Authority Key Identifier:
                keyid:...
                DirName:/CN=HospitalProjectCA
                serial:...
    Signature Algorithm: sha256WithRSAEncryption
    Signature Value:
        ...
```

### Adding CA Certificate macOS trust store
To add the CA certificate to the macOS trust store, run the following command.  
```
> sudo security add-trusted-cert -d -r trustRoot \
  -k /Library/Keychains/System.keychain \
  ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
Password:
```

You will have to enter your password and once again when the message dialog pops up. And I can see the certificate in the trust store now.
![](./images/Screenshot%202026-04-05%20at%2012.40.30 am.png)

### Adding CA Certificate NSSDB (Ubuntu)
On Ubuntu, I use Chromium based browsers like Google Chrome or Brave and both of them use NSSDB, which sits in **~/.pki/nssdb** and also you need **certutil** tool as well to install the certificate (with command `sudo apt update && sudo apt install libnss3-tools`). I already have **certutil** already installed and then ran this command.  
```
> certutil -A \
  -d ~/.pki/nssdb \
  -n "HospitalProjectCA" \
  -t "CT,," \
  -i ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
```

If you view your certificates in NSSDB, run this command:
```
> certutil -L -d ~/.pki/nssdb
Certificate Nickname                                         Trust Attributes
                                                             SSL,S/MIME,JAR/XPI

aspnetcore-localhost-{SOME_HASH}                             P,,  
HospitalProjectCA                                            CT,, 
```

In the parameter `-t`, `CT` means the following:
* `C` makes the certificate as a Trusted CA for the SSL server.
* `T` marks the certificate as a Trusted for client authentication. 

This configuration is needed because the certificates that I'm generating is self signed certificate.

And to remove it, run this command:
```
> certutil -D -d ~/.pki/nssdb -n "HospitalProjectCA"
```

Once installed you should be able to see it in:
* Brave Browser under **brave://certificate-manager/localcerts/platformcerts**
* Google Chrome under **chrome://certificate-manager/localcerts/platformcerts**


### ASP.NET Core Web API SSL Certificate
Firstly, [hospitalproject.server.cert.conf](../HospitalProject.Server/hospitalproject.server.cert.conf) is the config file that I will use to generate the self signed certificate for the backend.  
```
[req]
default_bits       = 2048
default_md         = sha256
distinguished_name = req_distinguished_name
req_extensions     = v3_req
prompt             = no

[req_distinguished_name]
CN = hospitalproject.api.local

[v3_req]
subjectAltName       = @alt_names
basicConstraints     = CA:false
keyUsage             = digitalSignature
extendedKeyUsage     = serverAuth
subjectKeyIdentifier = hash

[v3_sign]
subjectAltName         = @alt_names
basicConstraints       = CA:false
keyUsage               = digitalSignature
extendedKeyUsage       = serverAuth
subjectKeyIdentifier   = hash
authorityKeyIdentifier = keyid:always, issuer

[alt_names]
DNS.1 = hospitalproject.api.local
```

So `default_bits`, I left it small, the bigger the number the more processing the CPU will do. By setting the Subject Alternative Names, it ensures that the that DNS entry needs to have the name *hospitalproject.api.local*. I didn't really have to put the `distinguished_name`. But there are two sections `v3_req`, this section will be used for generating the CSR and the `v3_sign` section is going to be used for the certificate signing. `v3_req` does not have `authorityKeyIdentifier` identifier in it because when generating the CSR it will throw an error because OpenSSL does not have CA to reference. When signing the certificate then I'm able to reference the CA certificate. 

Just going onto the other attributes and values `subjectKeyIdentifier = hash` will drive the key identifier from the public key that's gets generated. And `authorityKeyIdentifier = keyid:always, issuer` will add reference to the CA with the key id and issuer CN name.

`keyUsage = digitalSignature` means the following `digitalSignature` the server will sign with DHE/ECDHE Cipher Suites and the client/browser will verify the signature with the public key in the certificate and then use it in its message exchanges. This is more secure than using `keyEncipherment` because the public that the browser/client gets is in plain text. 

`extendedKeyUsage = serverAuth`, this is just normal TLS communication between server and client/browser where `serverAuth` means it can be used to authenticate a server. If I added `clientAuth` to `extendedKeyUsage` then it will become a mTLS certificate.

`subjectAltName` is referencing the `alt_names` section with the `@alt_names` attribute.

Now I'm going to generate the command for generating the CSR and private key for the ASP.NET Core backend, with the config file [hospitalproject.server.cert.conf](../HospitalProject.Server/hospitalproject.server.cert.conf).
```
> openssl req -new \
  -newkey rsa:2048 \
  -nodes \
  -keyout ~/Workspaces/Certs/hospital.project/hospital.project.server.key \
  -config ~/Projects/HospitalProject/HospitalProject.Server/hospitalproject.server.cert.conf \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.server.csr
..+......+......+...+......+......+.+...............+...+..+.......+..+++++++++++++++++++++++++++++++++++++++*...................+.....+....+...+..+.+.....+.........+......+.........+...+.........+.+..+++++++++++++++++++++++++++++++++++++++*...+.....+.+.....+...+.+......+.....+.+.....+.........+.+...+..+...+......+....+......+.....+..........+...+.....+...+...+..........+...............+..+...+.+...+...........+....+...............+.....+..........+......+.....+.......+............+...+..+................+..+..........+.....+....++++++
..+...+.......+.....+..........+......+..+...+...+......+...+.+......+......+++++++++++++++++++++++++++++++++++++++*.+........+...+...+.+++++++++++++++++++++++++++++++++++++++*....+.+..+.............+...+...........+...............+...+.......+...+...........+....+..+.........+......+.+............+...............+...+.....+.......+..+...+...+.........+......+............+.+.....+.......+..+......+......+.........+.......+..+.......+.....+.+.................+...+.........+...+...+............+...+...+.+......+...+...............+..+...+......+......+...+....+......+..+.......+......+.........+.....+.+..............+...+.......+.....................+..+....+.....+.+.....+..........+...............+.........+........+..........+...+........+.+.....+.........+.....................+.........+..........+.........+..+...+.+.........+..+......+.......+......+............+..+.............+.....+....+...+.....+.......+.....+.+...+..+.........+...+.+.....................+...+......+..+...+.+.....+...+.+....................+....+..+...+......+.+...+...+...+.........+..+.+.....+............+.+.........+........+....+.....+.+...+...........+...+.+.........+............+.........+..+..........+...+.............................+...+..........+..+.+...........+.+..++++++
-----
```

Lastly, I'm going to self sign the certificate and generate the PEM certificate for ASP.NET Core backend. But I'm going to pass in the `v3_sign` section because then I can reference the CA.  
```
> openssl x509 -req \
  -in ~/Workspaces/Certs/hospital.project/hospital.project.server.csr \
  -CA ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  -CAkey ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key \
  -CAcreateserial \
  -days 365 \
  -sha256 \
  -extfile ~/Projects/HospitalProject/HospitalProject.Server/hospitalproject.server.cert.conf \
  -extensions v3_sign \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.server.pem
Certificate request self-signature ok
subject=CN=hospitalproject.api.local
> Enter pass phrase for /Users/zamk/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key:
```

The PEM file is not the certificate that I am going to use, I'm going to export a pfx file using the pem and key file that I generated for the server.  
```
> openssl pkcs12 -export \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx \
  -inkey ~/Workspaces/Certs/hospital.project/hospital.project.server.key \
  -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pem \
  -certfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
> Enter Export Password:
> Verifying - Enter Export Password:
```

And again, I had to enter the pass phrase twice but after this; all done, all the certificate. For the server is now generated.  
```
> ls ~/Workspaces/Certs/hospital.project 
ca                              hospital.project.server.key     hospital.project.server.pfx
hospital.project.server.csr     hospital.project.server.pem
```

And I'm going to verify everything is all good with the CA and my server certificate. Be sure to enter your pass phrase for `-passin` flag.  
```
> openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  ~/Workspaces/Certs/hospital.project/hospital.project.server.pem
/Users/zamk/Workspaces/Certs/hospital.project/hospital.project.server.pem: OK
    > openssl pkcs12 -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx \
    -clcerts -nokeys -passin pass:*********** | \
    openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
stdin: OK
```

The second command that I ran extracts the client certificate. The `-clcerts` & `-nokeys` flags isolates the end-entity certificate from the PFX file. And then pipe the output to `openssl verify` and check against the CA.

And the SANs and all the other attributes that I specified in the `v3_req` & from the `v3_sign` section the CA along with `authorityKeyIdentifier` information is also are on the certificate.  
```
> openssl x509 -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pem -noout -text | grep -E -A 3 '(X509v3|Serial Number|Issuer)'
        Serial Number:
            ...
        Signature Algorithm: sha256WithRSAEncryption
        Issuer: CN=HospitalProjectCA
        Validity
            Not Before: Apr  9 03:53:10 2026 GMT
            Not After : Apr  9 03:53:10 2027 GMT
--
        X509v3 extensions:
            X509v3 Subject Alternative Name: 
                DNS:hospitalproject.api.local
            X509v3 Basic Constraints: 
                CA:FALSE
            X509v3 Key Usage: 
                Digital Signature
            X509v3 Extended Key Usage: 
                TLS Web Server Authentication
            X509v3 Subject Key Identifier: 
                ...
            X509v3 Authority Key Identifier: 
                ...
    Signature Algorithm: sha256WithRSAEncryption
    Signature Value:
```

The output is the same when inspecting the pfx file.  
```
> openssl pkcs12 -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx \
  -passin pass:******** -clcerts -nokeys | \
  openssl x509 -noout -text | \
  grep -E -A 3 '(X509v3|Serial Number|Issuer)'
        Serial Number:
            ...
        Signature Algorithm: sha256WithRSAEncryption
        Issuer: CN=HospitalProjectCA
        Validity
            Not Before: Apr  9 03:53:10 2026 GMT
            Not After : Apr  9 03:53:10 2027 GMT
--
        X509v3 extensions:
            X509v3 Subject Alternative Name: 
                DNS:hospitalproject.api.local
            X509v3 Basic Constraints: 
                CA:FALSE
            X509v3 Key Usage: 
                Digital Signature
            X509v3 Extended Key Usage: 
                TLS Web Server Authentication
            X509v3 Subject Key Identifier: 
                ....
            X509v3 Authority Key Identifier: 
                ...
    Signature Algorithm: sha256WithRSAEncryption
    Signature Value:
```

### ReactJS and NGINX Frontend SSL Certificate
Now for the front end the [hospitalproject.client.cert.conf](../HospitalProject.Client/hospitalproject.client.cert.conf) will be used to generate the certificates. The configuration for this certificate is almost identical to [hospitalproject.server.cert.conf](../HospitalProject.Server/hospitalproject.server.cert.conf) except, I've added the localhost name and ip address in the `alt_names` section.

And I'm going to use the same commands that I used for generating the certificates for the ASP.NET Core backend.  
```
[req]
default_bits       = 2048
default_md         = sha256
distinguished_name = req_distinguished_name
req_extensions     = v3_req
prompt             = no

[req_distinguished_name]
CN = hospitalproject.local

[v3_req]
subjectAltName       = @alt_names
basicConstraints     = CA:false
keyUsage             = digitalSignature
extendedKeyUsage     = serverAuth
subjectKeyIdentifier = hash

[v3_sign]
subjectAltName         = @alt_names
basicConstraints       = CA:false
keyUsage               = digitalSignature
extendedKeyUsage       = serverAuth
subjectKeyIdentifier   = hash
authorityKeyIdentifier = keyid:always, issuer


[alt_names]
DNS.1 = localhost
DNS.2 = hospitalproject.local
IP.1 = 127.0.0.1
```


Firstly going to generate the CSR and private key.
```
> openssl req -new \
  -newkey rsa:2048 \
  -nodes \
  -keyout ~/Workspaces/Certs/hospital.project/hospital.project.client.key \
  -config ~/Projects/HospitalProject/HospitalProject.Client/hospitalproject.client.cert.conf \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.client.csr
........+.+...........+....+......+.....+.+............+++++++++++++++++++++++++++++++++++++++*.+..........+.....+...+..........+...........+.+.........+.....+++++++++++++++++++++++++++++++++++++++*............+..+...+.+...............+.....+......+...+....+...+.........+........+......+....+......+...........+..................+.......+...+...+..+............................+.........+.....+.............+.....+.+.........+..+..........+...............+...+...............+..++++++
.........+...+.............+.....+.+.....+...+...+...+....+..+...+++++++++++++++++++++++++++++++++++++++*.........+...+++++++++++++++++++++++++++++++++++++++*..............+.....+......+...+.........+.......+......+........+......+.+..+.+......+...+......+..+...+....+.....+...+..
```

Next, I'm going to self sign the certificate and generate the certificate and passing in the `v3_sign` section from my configuration file. Be sure to enter your pass phrase when prompted.
```
> openssl x509 -req \
  -in ~/Workspaces/Certs/hospital.project/hospital.project.client.csr \
  -CA ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  -CAkey ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key \
  -CAcreateserial \
  -days 365 \
  -sha256 \
  -extfile ~/Projects/HospitalProject/HospitalProject.Client/hospitalproject.client.cert.conf \
  -extensions v3_sign \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.client.pem
Certificate request self-signature ok
subject=CN=hospitalproject.local
Enter pass phrase for /Users/zamk/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key:
```

Now, both client and server certificates are now generated.  
```
> ls ~/Workspaces/Certs/hospital.project 
ca                              hospital.project.client.key     hospital.project.server.csr     hospital.project.server.pem
hospital.project.client.csr     hospital.project.client.pem     hospital.project.server.key     hospital.project.server.pfx
```

Now, I'm going to validate the certificate with my CA certificate.
```
> openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  ~/Workspaces/Certs/hospital.project/hospital.project.client.pem
/Users/zamk/Workspaces/Certs/hospital.project/hospital.project.client.pem: OK
```

The SANs and all the other attributes that I specified in the `v3_req` & from the `v3_sign` section the CA along with `authorityKeyIdentifier` information is also are on the certificate.
```
> openssl x509 -in ~/Workspaces/Certs/hospital.project/hospital.project.client.pem -noout -text | grep -E -A 3 '(X509v3|Serial Number|Issuer)'
        Serial Number:
            ...
        Signature Algorithm: sha256WithRSAEncryption
        Issuer: CN = HospitalProjectCA
        Validity
            Not Before: Apr  9 04:20:49 2026 GMT
            Not After : Apr  9 04:20:49 2027 GMT
--
        X509v3 extensions:
            X509v3 Subject Alternative Name:
                DNS:localhost, DNS:hospitalproject.local, IP Address:127.0.0.1
            X509v3 Basic Constraints:
                CA:FALSE
            X509v3 Key Usage:
                Digital Signature
            X509v3 Extended Key Usage:
                TLS Web Server Authentication
            X509v3 Subject Key Identifier:
                ...
            X509v3 Authority Key Identifier:
                ...        
            Signature Algorithm: sha256WithRSAEncryption                                   
            Signature Value:
```

And great, I can also see key usage as well.

## NGINX Config Change & Creating New Docker Container(s)
Before I create the YAML file for deployment, firstly I updated the [staging.nginx.conf](../HospitalProject.Client/staging.nginx.conf) config file and enabled SSL verification on, also pointed to the CA certificate file that I generated and also updated the URL for the ASP.NET Core backend to what I want the server name to be.  
```
...
        location /weatherforecast {
            proxy_pass https://hospitalproject.api.local:5229;

            proxy_set_header Host               $host;
            proxy_set_header X-Forwarded-Proto  $scheme;
            proxy_set_header X-Forwarded-For    $proxy_add_x_forwarded_for;

            proxy_ssl_verify              on;
            proxy_ssl_verify_depth        2;
            proxy_ssl_name                hospitalproject.api.local;
            proxy_ssl_server_name         on;
            proxy_ssl_trusted_certificate /etc/nginx/certs/HospitalProject.CA.pem;
        }
...
```

With `proxy_ssl_verify` set to on although not strictly necessary to have `proxy_ssl_verify_depth` to 2 in my case the certificate chain for the certificates that I have generated is only at level 1. 
```
> openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  -show_chain \
  ~/Workspaces/Certs/hospital.project/hospital.project.server.pem
/Users/zamk/Workspaces/Certs/hospital.project/hospital.project.server.pem: OK
Chain:
depth=0: CN=hospitalproject.api.local (untrusted)
depth=1: CN=HospitalProjectCA
```

When the above command was ran at depth 0 `CN=hospitalproject.api.local (untrusted)`, basically says that this certificate is not a trust store or CA Certificate. But the CA certificate is trusted. But the output against the server pem file is OK. In saying that NGINX doesn't care about this, the only cares if the certificate is valid. But usually on a normal certificate there would be an intermediary certificate which then the CA certificate will have a depth level of 2, so I left it at 2 which acts like a safety net for now. 

And now checking the pfx file.
```
> openssl pkcs12 -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx \
  -nokeys -passin pass:****** | grep "subject="
subject=CN=hospitalproject.api.local
subject=CN=HospitalProjectCA
```

The similar output appears for the client certificate instead of `CN=hospitalproject.api.local (untrusted)` appearing at depth 0, `CN=hospitalproject.local (untrusted)` appears instead.
```
> openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  -show_chain \
  ~/Workspaces/Certs/hospital.project/hospital.project.client.pem
/Users/zamk/Workspaces/Certs/hospital.project/hospital.project.client.pem: OK
Chain:
depth=0: CN=hospitalproject.local (untrusted)
depth=1: CN=HospitalProjectCA
```

The same for `proxy_ssl_name` and `proxy_ssl_server_name`, this is optional because I passed in the URI with a hostname or network alias when I created the docker container not an IP address it would mandatory.

Everything else remains the same. Next I created both the back and front containers with the following commands. Both contains has a network alias name, assigned to my network bridge adapter and I didn't give containers a name if I did then I'd have to delete the container and then redeploy it, IP addresses manually assign to map the subnet of my network bridge adapter and repointed the certificates to the location of where I generated the certificates with OpenSSL.
```
> docker run -d --network hospital-network --ip 10.0.0.3 --network-alias hospitalproject.api.local \
-e ASPNETCORE_URLS="https://+:5229" \
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/hospitalproject.server.pfx \
-e ASPNETCORE_Kestrel__Certificates__Default__Password="**********" \
-e ASPNETCORE_ENVIRONMENT=Staging \
-e ASPNETCORE_ForwardedHeaders_Enabled=true \
-e ASPNETCORE_KnownProxies__0=10.0.0.2 \
-v ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx:/https/hospitalproject.server.pfx:ro \
-v ~/Projects/HospitalProject/HospitalProject.Server/appsettings.Staging.json:/app/appsettings.Staging.json:ro \
hospital.project.server
a0bb6b87a96481d0aab1f487db47111a905e7af9199ccfad4508515c8981fe89
>
> docker run -d --network hospital-network --ip 10.0.0.2 --network-alias hospitalproject.local -p 443:443 \
-v ~/Projects/HospitalProject/HospitalProject.Client/staging.nginx.conf:/etc/nginx/nginx.conf:ro \
-v ~/Workspaces/Certs/hospital.project/hospital.project.client.key:/etc/nginx/certs/hospitalproject.client.key:ro \
-v ~/Workspaces/Certs/hospital.project/hospital.project.client.pem:/etc/nginx/certs/hospitalproject.client.pem:ro \
-v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/etc/nginx/certs/HospitalProject.CA.pem:ro \
hospital.project.client
470555c55c3a7daa234efbf852ea418ae5b0171f5ceb72bf20c7db034eb4644b
```

After running the above commands, I can see that the IP address that manually assigned is assigned along with the network alias that I set as well.
```
> docker ps
CONTAINER ID   IMAGE                     COMMAND                  CREATED          STATUS          PORTS                                     NAMES
ced12cdeb375   hospital.project.client   "nginx -c /etc/nginx…"   17 minutes ago   Up 17 minutes   0.0.0.0:443->443/tcp, [::]:443->443/tcp   fervent_villani
c3ebc0bc64dd   hospital.project.server   "dotnet HospitalProj…"   19 minutes ago   Up 19 minutes                                             relaxed_proskuriakova

> docker inspect --format '{{json .NetworkSettings}}' relaxed_proskuriakova
{
    "SandboxID": "c8ff2a6d9cf05f589d21a5fdf17b104a00006f8a70ea28d9204cb879435914e2",
    "SandboxKey": "/var/run/docker/netns/c8ff2a6d9cf0",
    "Ports": {},
    "Networks": {
        "hospital-network": {
            "IPAMConfig": {
                "IPv4Address": "10.0.0.3"
            },
            "Links": null,
            "Aliases": [
                "hospitalproject.api.local"
            ],
            "DriverOpts": null,
            "GwPriority": 0,
            "NetworkID": "8cc9187919c54362d59c4fb68c61338359d3f7d4ad9c2e40ab31e63919150e7f",
            "EndpointID": "6c742449a1475d99b81a43ccf440a087c4365574bd960bedcb1bfec728501f27",
            "Gateway": "10.0.0.1",
            "IPAddress": "10.0.0.3",
            "MacAddress": "2e:b8:4d:29:27:c2",
            "IPPrefixLen": 28,
            "IPv6Gateway": "",
            "GlobalIPv6Address": "",
            "GlobalIPv6PrefixLen": 0,
            "DNSNames": [
                "relaxed_proskuriakova",
                "hospitalproject.api.local",
                "c3ebc0bc64dd"
            ]
        }
    }
}

>  docker inspect --format '{{json .NetworkSettings}}' fervent_villani
{
    "SandboxID": "bf04f1c518457f7a2703db1fb3f9f815921f340ebe2b1554a3b5986efa71cd06",
    "SandboxKey": "/var/run/docker/netns/bf04f1c51845",
    "Ports": {
        "443/tcp": [
            {
                "HostIp": "0.0.0.0",
                "HostPort": "443"
            },
            {
                "HostIp": "::",
                "HostPort": "443"
            }
        ]
    },
    "Networks": {
        "hospital-network": {
            "IPAMConfig": {
                "IPv4Address": "10.0.0.2"
            },
            "Links": null,
            "Aliases": [
                "hospitalproject.local"
            ],
            "DriverOpts": null,
            "GwPriority": 0,
            "NetworkID": "8cc9187919c54362d59c4fb68c61338359d3f7d4ad9c2e40ab31e63919150e7f",
            "EndpointID": "75f03f0b8a93d5d9d2870d39733dbd06c0cce73f78a83a3de295b410152d4390",
            "Gateway": "10.0.0.1",
            "IPAddress": "10.0.0.2",
            "MacAddress": "c2:0e:fb:fb:12:03",
            "IPPrefixLen": 28,
            "IPv6Gateway": "",
            "GlobalIPv6Address": "",
            "GlobalIPv6PrefixLen": 0,
            "DNSNames": [
                "fervent_villani",
                "hospitalproject.local",
                "ced12cdeb375"
            ]
        }
    }
}
``` 

For the backend, I'm using the pem and key file certificates that I created and for the frontend I changed the certificate to point to the CA certificate that I have generated and also added two more environment variables which are `ASPNETCORE_ForwardedHeaders_Enabled=true` & `ASPNETCORE_KnownProxies__0=10.0.0.2` so that ASP.NET Core knows it's a trusted proxy and it will process the X-Forward headers like it does when running the ASP.NET core backend and frontend locally with the ViteJS proxy. 

The key difference being I did not have add these environment variables in because ASP.NET Core automatically processes the X-Forward headers when everything is running on docker. Since both containers are running. The requests are coming from browser to NGINX (And I think it's overwriting my XForward Headers configuration in my code but will sort that out later).
```
2026-04-05 14:55:57.768 | 10.0.0.1 - - [05/Apr/2026:04:55:57 +0000] "GET /weatherforecast HTTP/2.0" 200 382 "https://localhost/" "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"
```

And the API calls are getting reversed proxied to ASP.NET Core backend. With the X-Forward Headers being processed.
```
2026-04-05 14:55:57.767 | 2026-04-05 04:55:57 [INF] Request starting HTTP/1.0 GET https://localhost/weatherforecast - null null
2026-04-05 14:55:57.767 | 2026-04-05 04:55:57 [INF] Request:
2026-04-05 14:55:57.767 | Protocol: HTTP/1.0
2026-04-05 14:55:57.767 | Method: GET
2026-04-05 14:55:57.767 | Scheme: https
2026-04-05 14:55:57.767 | PathBase: 
2026-04-05 14:55:57.767 | Path: /weatherforecast
2026-04-05 14:55:57.767 | Accept: */*
2026-04-05 14:55:57.767 | Connection: close
2026-04-05 14:55:57.767 | Host: localhost
2026-04-05 14:55:57.767 | User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
2026-04-05 14:55:57.767 | Accept-Encoding: gzip, deflate, br, zstd
2026-04-05 14:55:57.767 | Accept-Language: en-GB,en-US;q=0.9,en;q=0.8
2026-04-05 14:55:57.767 | Referer: https://localhost/
2026-04-05 14:55:57.767 | X-Original-Proto: [Redacted]              <-- X-Forward-Headers being processed HERE
2026-04-05 14:55:57.767 | sec-ch-ua-platform: "Linux"
2026-04-05 14:55:57.767 | sec-ch-ua: "Chromium";v="146", "Not-A.Brand";v="24", "Google Chrome";v="146"
2026-04-05 14:55:57.767 | sec-ch-ua-mobile: ?0
2026-04-05 14:55:57.767 | sec-fetch-site: same-origin
2026-04-05 14:55:57.767 | sec-fetch-mode: cors
2026-04-05 14:55:57.767 | sec-fetch-dest: empty
2026-04-05 14:55:57.767 | priority: u=1, i
2026-04-05 14:55:57.767 | X-Original-For: [Redacted]                <-- X-Forward-Headers being processed HERE
2026-04-05 14:55:57.767 | 2026-04-05 04:55:57 [INF] Executing endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-04-05 14:55:57.767 | 2026-04-05 04:55:57 [INF] Route matched with {action = "Get", controller = "WeatherForecast"}. Executing controller action with signature System.Collections.Generic.IEnumerable`1[HospitalProject.Server.WeatherForecast] Get() on controller HospitalProject.Server.Controllers.WeatherForecastController (HospitalProject.Server).
2026-04-05 14:55:57.767 | 2026-04-05 04:55:57 [INF] Executing ObjectResult, writing value of type 'HospitalProject.Server.WeatherForecast[]'.
2026-04-05 14:55:57.767 | 2026-04-05 04:55:57 [INF] Response:
2026-04-05 14:55:57.767 | StatusCode: 200
2026-04-05 14:55:57.767 | Content-Type: application/json; charset=utf-8
2026-04-05 14:55:57.768 | 2026-04-05 04:55:57 [INF] Executed action HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server) in 0.4679ms
2026-04-05 14:55:57.768 | 2026-04-05 04:55:57 [INF] Executed endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-04-05 14:55:57.768 | 2026-04-05 04:55:57 [INF] ResponseBody: [{"date":"2026-04-06","temperatureC":29,"temperatureF":84,"summary":"Cool"},{"date":"2026-04-07","temperatureC":34,"temperatureF":93,"summary":"Chilly"},{"date":"2026-04-08","temperatureC":28,"temperatureF":82,"summary":"Balmy"},{"date":"2026-04-09","temperatureC":49,"temperatureF":120,"summary":"Mild"},{"date":"2026-04-10","temperatureC":15,"temperatureF":58,"summary":"Chilly"}]
2026-04-05 14:55:57.768 | 2026-04-05 04:55:57 [INF] Duration: 0.7618ms
2026-04-05 14:55:57.768 | 2026-04-05 04:55:57 [INF] HTTP GET /weatherforecast responded 200 in 0.7975 ms
2026-04-05 14:55:57.768 | 2026-04-05 04:55:57 [INF] Request finished HTTP/1.0 GET https://localhost/weatherforecast - 200 null application/json; charset=utf-8 1.1084ms
```

Now the good thing is the ASP.NET Core backend is no longer exposed and it is only accessible through Docker and the reverse proxy (I put **https://localhost:5173**, but ASP.NET Core ignores the port number and allows the API call to happen from port 443) and there's a reason for this, which I will discuss later.

However, for the frontend certificate I also put the another DNS name as well. So first things first. I'm going to update [staging.nginx.conf](../HospitalProject.Client/staging.nginx.conf).
```
...
http {
    include /etc/nginx/mime.types;

    server
    {
        listen 443 ssl;
        http2 on;
        server_name localhost hospitalproject.local;
        ...
    }
    ...
}
```

If I try to **https://hospitalproject.local**, it doesn't work.  
![hospitalproject.local doesn't work](./images/Screenshot%20from%202026-04-05%2015-15-17.png)

But accessing by the localhost URL does. So on my host machine, I need to add a DNS entry to resolve to the localhost IP address (127.0.0.1).

## Adding DNS entry for hospitalproject.local
The easiest way would be just to add an entry to your **/etc/hosts** file (On Ubuntu and MacOS), which I have already done.  
```
> sudo tee -a /etc/hosts << 'EOF'
# Docker Container Lookup
127.0.0.1       hospitalproject.local
# End of Section
EOF
```

Because I've exposed port 443 on Docker, a NAT transition is automatically done to docker. And now the page is loading.
![hospitalproject.local is working now](./images/Screenshot%20from%202026-04-05%2016-01-21.png)

### MacOS mDNS (much faster!)
Although the above options works, the page when not cached and also the API response is really slow. Then I realised that on MacOS, it has it's own called mDNS and handles the discovery of hostname(s) that end with `.local`. For now what I have been doing is running the following command:
```
dns-sd -P "Hospital Project" _https._tcp local 443 hospitalproject.local 127.0.0.1 &
[1] 8233
> Registering Service Hospital Project._https._tcp.local host hospitalproject.local port 443
DATE: ---Fri 10 Apr 2026---
21:22:42.762  ...STARTING...
21:22:43.505  Got a reply for record hospitalproject.local: Name now registered and active
21:22:43.506  Got a reply for service Hospital Project._https._tcp.local.: Name now registered and active
```

This binds the name `hospitalproject.local` and port 443 to `127.0.0.1`. I can see with this command that the mDNS and Browse were created (along with my printer on the network 😅).  
```
> dns-sd -B _https._tcp local & 
[6] 11005
> Browsing for _https._tcp.local
DATE: ---Fri 10 Apr 2026---
21:43:16.070  ...STARTING...
Timestamp     A/R    Flags  if Domain               Service Type         Instance Name
21:43:16.072  Add        3   1 local.               _https._tcp.          Hospital Project
21:43:16.073  Add        2  11 local.               _https._tcp.          Hospital Project
```

I want to automate this, but unfortunately, the man documents says the shell scripting this would be fragile. But there is another way on MacOS which I will do later! But the page and API call is much faster.

But now I need to do some clean up before I create YAML files for full build and deployment. 

# Clean up before proper deployment
## Logging Out the environment name
I just thought to log out the environment name because I have some ah-duh moments 😅.
```csharp
    ...
    app.UseSerilogRequestLogging();
    Log.Information("Application started! Logging to both console and/or file.");
    Log.Information($"Running Environment : {app.Environment.EnvironmentName}");
    ...
```

## Forward Headers and Known Proxies
When I created the ASP.NET Core container, I set a lot of environment variables especially these two stood out to me being a little bit problematic:  
```
...
-e ASPNETCORE_ForwardedHeaders_Enabled=true \
-e ASPNETCORE_KnownProxies__0=10.0.0.2 \
...
```

From the Reading that I have done, it seems that these parameters ignores the Forward Headers middleware that I have coded, so this and other Kestrel related configuration will be coded and/or passed in from the **appsettings.json** file(s).

As mentioned in the previous section, instead of passing environment variables for enabling forward headers and adding known proxies, I'm going to pass it through from **appsettings.json**. Before I pass it through, I need to add some code in [Program.cs](../HospitalProject.Server/Program.cs).


If it's development then use the what I originally set otherwise for everything else get it from the config file. And Within the [appsettings.Staging.json](../HospitalProject.Server/appsettings.Staging.json), this is the config that I've set for Forward Headers.
```json
...
  "ForwardedHeaders": {
    "ForwardedHeaderOptions": "XForwardedFor,XForwardedProto",
    "KnownProxies": [ "10.0.0.2" ]
  }
...
```

`ForwardedHeaderOptions` is just a comma separate string, which my code will split and trim the values and then "use" which I will discuss in a little bit and then `KnownProxies` is just an array of IP address but this is optional. I've actually created an extension class and methods for parsing in the values from **appsettings.json** file. Before I discuss the extension class, I want to discuss [Program.cs](../HospitalProject.Server/Program.cs).
```csharp
using HospitalProject.Server.Extensions;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;

...
try
{

    if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
    {
        builder.Services.AddHttpLogging(o => { 
            o.LoggingFields = HttpLoggingFields.All; 
            o.RequestHeaders.Add("Referer");
            o.RequestHeaders.Add("sec-ch-ua-platform");
            o.RequestHeaders.Add("sec-ch-ua");
            o.RequestHeaders.Add("sec-ch-ua-mobile");
            o.RequestHeaders.Add("sec-fetch-site");
            o.RequestHeaders.Add("sec-fetch-mode");
            o.RequestHeaders.Add("sec-fetch-dest");
            o.RequestHeaders.Add("priority");
            o.RequestHeaders.Add("X-Forwarded-For");
            o.RequestHeaders.Add("X-Forwarded-Proto");
            o.RequestHeaders.Add("X-Original-For");
            o.RequestHeaders.Add("X-Original-Proto");
        }); 

        builder.Services.AddOpenApi();
    }
    ...
    builder.Services.AddForwardHeaderOptionsConfiguration(builder.Configuration, builder.Environment);
    ...

    var app = builder.Build();

    app.UseForwardedHeaders();

    app.UseSerilogRequestLogging();
    Log.Information("Application started! Logging to both console and/or file.");
    app.LogForwardHeaderOptionsConfiguration();

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseHttpLogging();
        app.MapOpenApi();
    }
    ...

    app.Run();
}
...
```

First of all, I added Forward Headers for HTTP logging so that the `X-Original-Proto` and `X-Original-For` values are exposed, but only for development and staging environments. 
```csharp
    ...
    if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
    {
        builder.Services.AddHttpLogging(o => { 
            o.LoggingFields = HttpLoggingFields.All; 
            o.RequestHeaders.Add("Referer");
            o.RequestHeaders.Add("sec-ch-ua-platform");
            o.RequestHeaders.Add("sec-ch-ua");
            o.RequestHeaders.Add("sec-ch-ua-mobile");
            o.RequestHeaders.Add("sec-fetch-site");
            o.RequestHeaders.Add("sec-fetch-mode");
            o.RequestHeaders.Add("sec-fetch-dest");
            o.RequestHeaders.Add("priority");
            o.RequestHeaders.Add("X-Forwarded-For");                 <-- Here
            o.RequestHeaders.Add("X-Forwarded-Proto");               <-- Here
            o.RequestHeaders.Add("X-Original-For");                  <-- Here
            o.RequestHeaders.Add("X-Original-Proto");                <-- Here
        }); 

        builder.Services.AddOpenApi();
    }
    ...
    var app = builder.Build();
    ...
    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseHttpLogging();
        app.MapOpenApi();
    }
    ...
```

After this I've created 2 extension methods for the forward headers, which is in a static class [ForwardHeadersExtensions.cs](../HospitalProject.Server/Extensions/ForwardHeadersExtensions.cs). `AddForwardHeaderOptionsConfiguration()` takes in 2 arguments, which is the of type `IConfiguration` for reading out the configuration from the **appsettings.json** file and `IWebHostEnvironment` for getting which the value from the environment value.
```csharp
    ...
    builder.Services.AddForwardHeaderOptionsConfiguration(builder.Configuration, builder.Environment);
    ...
    var app = builder.Build();
    app.UseForwardedHeaders();
    app.UseSerilogRequestLogging();
    app.LogForwardHeaderOptionsConfiguration();
    ...
```

The main function `AddForwardHeaderOptionsConfiguration()` is checking if the environment variable `ASPNETCORE_ENVIRONMENT` is development just said the Forward Headers to `XForwardedFor` & `XForwardedProto` are set but it doesn't stop you from setting values in [appsettings.Development.json](../HospitalProject.Server/appsettings.Development.json) (Don't need to do this but hey, it's good to know 😀). Otherwise if the environment is something else then I'm going to ask you for the Forward Headers and `KnownProxies` is optional.
```csharp
namespace HospitalProject.Server.Extensions;

using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Serilog;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddForwardHeaderOptionsConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment hostEnvironment)
    {
        services.Configure<ForwardedHeadersOptions>(opts =>
        {
            var forwardHeadersSection = configuration.GetSection("ForwardedHeaders");

            if (string.IsNullOrEmpty(forwardHeadersSection.GetSection("ForwardedHeaderOptions").Get<string>()) && hostEnvironment.IsDevelopment())
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            }
            else if (!string.IsNullOrEmpty(forwardHeadersSection.GetSection("ForwardedHeaderOptions").Get<string>()))
            {
                var forwardHeaderOptionsStr = forwardHeadersSection.GetSection("ForwardedHeaderOptions").Get<string>()?.Trim();

                if (!string.IsNullOrEmpty(forwardHeaderOptionsStr))
                {
                    ForwardedHeaders forwardedHeaders = ForwardedHeaders.None;

                    if (forwardHeaderOptionsStr.Contains(','))
                    {
                        var forwardHeaderOptions = forwardHeaderOptionsStr.Split(",", StringSplitOptions.TrimEntries);

                        foreach (var option in forwardHeaderOptions)
                        {
                            forwardedHeaders |= Enum.Parse<ForwardedHeaders>(option);
                        }
                    }
                    else
                    {
                        forwardedHeaders = Enum.Parse<ForwardedHeaders>(forwardHeaderOptionsStr);
                    }

                    opts.ForwardedHeaders = forwardedHeaders;
                }
                else
                {
                    throw new Exception("Forward Header Options are missing.");
                }

                var knownProxiesSection = forwardHeadersSection.GetSection("KnownProxies").Get<string[]>();
                if (knownProxiesSection != null)
                {
                    opts.KnownProxies.Clear();
                    foreach(var proxy in knownProxiesSection)
                    {
                        opts.KnownProxies.Add(IPAddress.Parse(proxy));
                    }
                }
            }
            else
            {
                throw new Exception("Forward Header Section is missing.");
            }
        });

        return services;
    }

    public static void LogForwardHeaderOptionsConfiguration(this IApplicationBuilder app)
    {
        var fwdOptions = app.ApplicationServices.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
        Log.Information($"ForwardedHeaders config — Headers: {fwdOptions.ForwardedHeaders}, KnownProxies: {string.Join(", ", fwdOptions.KnownProxies)}");
    }
} 
```

As mentioned earlier, the `ForwardedHeaderOptions` string value is split up, trimmed and then iterating it and doing a Bitwise OR assignment comparison operation and then assigning the value to `ForwardedHeaders` or just use the whole string if it's not comma separated. 

And for known proxies, Firstly, I'm clearing out all the known proxy values and then using the `System.Net.IPAddress.Parse()` to parse out the IP address from the array and then storing them. 

The `LogForwardHeaderOptionsConfiguration()` function just logs the Forward Headers and known proxies that are set (for debugging purposes of course!). Lastly, in the app middleware pipeline, it just now `app.UseForwardHeaders()` from what it was initially before.  

## SSL Configuration
I'm going to now configure and parameterize SSL configuration both backend and frontend. On both client and server certificates, I've set the key usage to `digitalSignature`, meaning it's restricted to use ECDHE/DHE Cipher suites. This means I can use TLS version 1.2 and version 1.3. TLS version 1.3 is performant than version 1.2. But you still version 1.2 for legacy reasons. 

Just saying although, I don't think I need to do this, on Windows, SSL options are already chosen for you, so you don't need to worry about it and it will most likely be the same on Linux and MacOS where SSL cipher suites are already chosen by default (but don't know where to look at the moment), but I got curious and learnt a lot of things especially debugging with the OpenSSL tool at my last job, so I wanted to try it out 😊. And adding a disclaimer, by no shape or means I'm an expert on SSL/TLS encryption, I have read so many articles in order to understand SSL\TLS encryption that I have mushroom growing out of my head and I think I just only just scratched the surface 😅. But I will share and shout out to a few articles and websites that did help me with understanding SSL/TLS encryption more (and I've also added in the [References](#references) section as well):
* [Cipher suites. Which are safe? and which not? | LinkedIn](https://www.linkedin.com/pulse/cipher-suites-which-safe-ramkumar-nadar/)
* [What Is Perfect Forward Secrecy (PFS) in TLS?](https://deepstrike.io/blog/what-is-perfect-forward-secrecy-pfs)
* [Ciphersuite Info](https://ciphersuite.info/)

### ASP.NET Core
This is the extension class that I have created for configuring Kestrel. And I'm just configuring the default HTTPS behavior of what I want. 
```csharp
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace HospitalProject.Server.Extensions;

public static class KestrelConfigurationExtensions
{
    public static IWebHostBuilder ConfigureKestrelHttpsDefaults (this IWebHostBuilder webHostBuilder, IWebHostEnvironment hostEnvironment)
    {

        
        webHostBuilder.ConfigureKestrel((context, serverOptions) =>
        {
            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
            {
               httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
               httpsOptions.HandshakeTimeout = TimeSpan.FromSeconds(10);
               
               if (hostEnvironment.IsProduction())
                {
                    httpsOptions.CheckCertificateRevocation = true;
                }

                httpsOptions.OnAuthenticate = (connectionContext, sslOptions) =>
                {
                    sslOptions.ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        SslApplicationProtocol.Http2,
                        SslApplicationProtocol.Http11
                    };

                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        sslOptions.CipherSuitesPolicy = new CipherSuitesPolicy(new[]
                        {
                            // TLS 1.3 cipher suites
                            TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                            TlsCipherSuite.TLS_AES_128_GCM_SHA256,
                            TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256,
                            
                            // TLS 1.2 cipher suites
                            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                            TlsCipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
                        }); 
                    }
                    sslOptions.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                    sslOptions.EncryptionPolicy = EncryptionPolicy.RequireEncryption;
                };
            });

            serverOptions.ConfigureEndpointDefaults(endpointOptions =>
            {
                endpointOptions.Protocols = HttpProtocols.Http1AndHttp2;                
            });

            if (hostEnvironment.IsProduction())
            {
                serverOptions.Limits.MaxConcurrentConnections = 1000;
                serverOptions.Limits.MaxRequestBodySize = 10485760;
            }
        });

        return webHostBuilder;
    }
}
```

A few things, In `KestrelServerOptions.ConfigureHttpsDefaults()`, `httpsOptions.SslProtocols` (or `Microsoft.AspNetCore.Server.Kestrel.Https.HttpsConnectionAdapterOptions.SslProtocols`) basically allows what protocols that I want to use. Whereas `sslOptions.EnabledSslProtocols` (or `SslServerAuthenticationOptions.EnabledSslProtocols`) basically says what protocols can be matched when authentication occurs. And another thing as well `IWebHostBuilder.ConfigureKestrel()` & `KestrelServerOptions.ConfigureHttpsDefaults()` takes in `Action` delegates as parameters and to me this basically means, these will get executed when ASP.NET Core is launched and at least for `ConfigureHttpsDefaults()` each time a HTTPS endpoint is created as per [Configure endpoints for the ASP.NET Core Kestrel web server | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0#configure-https). 

However, with `httpsOptions.OnAuthenticate` (or `Microsoft.AspNetCore.Server.Kestrel.Https.HttpsConnectionAdapterOptions.OnAuthenticate`) is a variable where you assign a `Action` delegate function and it is used each time for authentication. And `sslOptions.EncryptionPolicy` (or `SslServerAuthenticationOptions.EncryptionPolicy`) and I've chosen particular cipher suites for both TLS version 1.2 and version 1.3 that supports AES encryption (since all my certificates has been encrypted with AES256). And the cipher suite is mostly based off the [TLS Cipher Suites in Windows Server 2022 - Win32 apps | Microsoft Learn](https://learn.microsoft.com/en-us/windows/win32/secauthn/tls-cipher-suites-in-windows-server-2022) in terms of order and preference.

To test this and also ASP.NET Core needs to be running on Docker since that is where I want to test this. I ran the following command:  
```
> docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -showcerts
```
This command does the following, downloads the openssl image (if not downloaded already), creates a container and attaches it to my `hospital-network` bridge adapter, runs the command, and once the command finishes running the container is deleted. This the output from running the command. 

```
> docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -showcerts
Connecting to 10.0.0.3
CONNECTED(00000003)
depth=1 CN=HospitalProjectCA
verify return:1
depth=0 CN=hospitalproject.api.local
verify return:1
DONE
---
Certificate chain
 0 s:CN=hospitalproject.api.local
   i:CN=HospitalProjectCA
   a:PKEY: RSA, 2048 (bit); sigalg: sha256WithRSAEncryption
   v:NotBefore: Apr  9 03:53:10 2026 GMT; NotAfter: Apr  9 03:53:10 2027 GMT
-----BEGIN CERTIFICATE-----
...
-----END CERTIFICATE-----
---
Server certificate
subject=CN=hospitalproject.api.local
issuer=CN=HospitalProjectCA
---
No client certificate CA names sent
Peer signing digest: SHA256
Peer signature type: rsa_pss_rsae_sha256
Peer Temp Key: X25519, 253 bits
---
SSL handshake has read 1429 bytes and written 1638 bytes
Verification: OK
---
New, TLSv1.3, Cipher is TLS_AES_256_GCM_SHA384
Protocol: TLSv1.3
Server public key is 2048 bit
This TLS version forbids renegotiation.
No ALPN negotiated
Early data was not sent
Verify return code: 0 (ok)
---
``` 

In the code, The endpoint default HTTP options are HTTP 1.X or HTTP 2 as per `endpointOptions.Protocols = HttpProtocols.Http1AndHttp2` but I've also added `SslServerAuthenticationOptions.ApplicationProtocols` options, to say what HTTP protocols you can use for authentication. However, in the above output it has `No ALPN negotiated`, so I need to add another flag to the OpenSSL command which is `-alpn h2,http/1.1` and then I should get `ALPN protocol: h2` in the output.  
```
> docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -alpn h2,http/1.1 \
  -showcerts
Connecting to 10.0.0.3
CONNECTED(00000003)
depth=1 CN=HospitalProjectCA
verify return:1
depth=0 CN=hospitalproject.api.local
verify return:1
---
Certificate chain
 0 s:CN=hospitalproject.api.local
   i:CN=HospitalProjectCA
   a:PKEY: RSA, 2048 (bit); sigalg: sha256WithRSAEncryption
   v:NotBefore: Apr  9 03:53:10 2026 GMT; NotAfter: Apr  9 03:53:10 2027 GMT
-----BEGIN CERTIFICATE-----
...
-----END CERTIFICATE-----
---
Server certificate
subject=CN=hospitalproject.api.local
issuer=CN=HospitalProjectCA
---
No client certificate CA names sent
Peer signing digest: SHA256
Peer signature type: rsa_pss_rsae_sha256
Peer Temp Key: X25519, 253 bits
---
SSL handshake has read 1438 bytes and written 1656 bytes
Verification: OK
---
New, TLSv1.3, Cipher is TLS_AES_256_GCM_SHA384
Protocol: TLSv1.3
Server public key is 2048 bit
This TLS version forbids renegotiation.
ALPN protocol: h2
Early data was not sent
Verify return code: 0 (ok)
---
DONE
```

As you can see the line `New, TLSv1.3, Cipher is TLS_AES_256_GCM_SHA384` is using the first cipher for TLS version 1.3 which is `TlsCipherSuite.TLS_AES_256_GCM_SHA384`, what happens when I say I only have TLS version 1.2 ciphers by running this command:
```
> docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -alpn h2,http/1.1 \
  -tls1_2
```

And the output for this command is the following:
```
> % docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -alpn h2,http/1.1 \
  -tls1_2
Connecting to 10.0.0.3
CONNECTED(00000003)
depth=1 CN=HospitalProjectCA
verify return:1
depth=0 CN=hospitalproject.api.local
verify return:1
DONE
---
Certificate chain
 0 s:CN=hospitalproject.api.local
   i:CN=HospitalProjectCA
   a:PKEY: RSA, 2048 (bit); sigalg: sha256WithRSAEncryption
   v:NotBefore: Apr  9 03:53:10 2026 GMT; NotAfter: Apr  9 03:53:10 2027 GMT
---
Server certificate
-----BEGIN CERTIFICATE-----
...
-----END CERTIFICATE-----
subject=CN=hospitalproject.api.local
issuer=CN=HospitalProjectCA
---
No client certificate CA names sent
Peer signing digest: SHA256
Peer signature type: rsa_pss_rsae_sha256
Peer Temp Key: X25519, 253 bits
---
SSL handshake has read 1328 bytes and written 336 bytes
Verification: OK
---
New, TLSv1.2, Cipher is ECDHE-RSA-AES256-GCM-SHA384
Protocol: TLSv1.2
Server public key is 2048 bit
Secure Renegotiation IS supported
ALPN protocol: h2
SSL-Session:
    Protocol  : TLSv1.2
    Cipher    : ECDHE-RSA-AES256-GCM-SHA384
    Session-ID: 
    Session-ID-ctx: 
    Master-Key: ...
    PSK identity: None
    PSK identity hint: None
    SRP username: None
    Start Time: 1775887676
    Timeout   : 7200 (sec)
    Verify return code: 0 (ok)
    Extended master secret: yes
---
```

As per the output `New, TLSv1.2, Cipher is ECDHE-RSA-AES256-GCM-SHA384`, it is using the first TLS version 1.2 cipher available which is `TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384`. What if I use a cipher that I did not define in `sslOptions.CipherSuitesPolicy` with the following command.  
```
> docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -tls1_2 \
  -alpn h2,http/1.1 \
  -cipher ECDHE-ECDSA-AES128-GCM-SHA256
```

The output I get is.  
```
> docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -tls1_2 \
  -alpn h2,http/1.1 \
  -cipher ECDHE-ECDSA-AES128-GCM-SHA256
Connecting to 10.0.0.3
CONNECTED(00000003)
---
no peer certificate available
---
No client certificate CA names sent
---
SSL handshake has read 7 bytes and written 191 bytes
Verification: OK
---
New, (NONE), Cipher is (NONE)
Protocol: TLSv1.2
Secure Renegotiation IS NOT supported
No ALPN negotiated
SSL-Session:
    Protocol  : TLSv1.2
    Cipher    : 0000
    Session-ID: 
    Session-ID-ctx: 
    Master-Key: 
    PSK identity: None
    PSK identity hint: None
    SRP username: None
    Start Time: 1775887744
    Timeout   : 7200 (sec)
    Verify return code: 0 (ok)
    Extended master secret: no
---
20CD169AFFFF0000:error:0A000410:SSL routines:ssl3_read_bytes:ssl/tls alert handshake failure:ssl/record/rec_layer_s3.c:918:SSL alert number 40
```

The server did not agree with the client on the cipher suite that's why no certificate string was sent back in the response. Along with the error line `20CDF29FFFFF0000:error:0A000410:SSL routines:ssl3_read_bytes:ssl/tls alert handshake failure:ssl/record/rec_layer_s3.c:918:SSL alert number 40`.

Lastly, what happens when I select a cipher that is in the list but it's not the first cipher available for version 1.3 and version 1.2, am I allowed to do that? Yes I can:

For version 1.3:
```
> docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -tls1_3 \
  -alpn h2,http/1.1 \
  -ciphersuites TLS_AES_128_GCM_SHA256
Connecting to 10.0.0.3
CONNECTED(00000003)
depth=1 CN=HospitalProjectCA
verify return:1
depth=0 CN=hospitalproject.api.local
verify return:1
DONE
---
Certificate chain
 0 s:CN=hospitalproject.api.local
   i:CN=HospitalProjectCA
   a:PKEY: RSA, 2048 (bit); sigalg: sha256WithRSAEncryption
   v:NotBefore: Apr  9 03:53:10 2026 GMT; NotAfter: Apr  9 03:53:10 2027 GMT
---
Server certificate
-----BEGIN CERTIFICATE-----
...
-----END CERTIFICATE-----
subject=CN=hospitalproject.api.local
issuer=CN=HospitalProjectCA
---
No client certificate CA names sent
Peer signing digest: SHA256
Peer signature type: rsa_pss_rsae_sha256
Peer Temp Key: X25519, 253 bits
---
SSL handshake has read 1422 bytes and written 1563 bytes
Verification: OK
---
New, TLSv1.3, Cipher is TLS_AES_128_GCM_SHA256
Protocol: TLSv1.3
Server public key is 2048 bit
This TLS version forbids renegotiation.
ALPN protocol: h2
Early data was not sent
Verify return code: 0 (ok)
---
```

For version 1.2:
```
> docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -tls1_2 \
  -alpn h2,http/1.1 \
  -cipher TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256
Connecting to 10.0.0.3
CONNECTED(00000003)
depth=1 CN=HospitalProjectCA
verify return:1
depth=0 CN=hospitalproject.api.local
verify return:1
---
Certificate chain
 0 s:CN=hospitalproject.api.local
   i:CN=HospitalProjectCA
   a:PKEY: RSA, 2048 (bit); sigalg: sha256WithRSAEncryption
   v:NotBefore: Apr  9 03:53:10 2026 GMT; NotAfter: Apr  9 03:53:10 2027 GMT
---
Server certificate
-----BEGIN CERTIFICATE-----
...
-----END CERTIFICATE-----
subject=CN=hospitalproject.api.local
issuer=CN=HospitalProjectCA
---
No client certificate CA names sent
Peer signing digest: SHA256
Peer signature type: rsa_pss_rsae_sha256
Peer Temp Key: X25519, 253 bits
---
SSL handshake has read 1320 bytes and written 276 bytes
Verification: OK
---
New, TLSv1.2, Cipher is ECDHE-RSA-CHACHA20-POLY1305
Protocol: TLSv1.2
Server public key is 2048 bit
Secure Renegotiation IS supported
ALPN protocol: h2
SSL-Session:
    Protocol  : TLSv1.2
    Cipher    : ECDHE-RSA-CHACHA20-POLY1305
    Session-ID: 
    Session-ID-ctx: 
    Master-Key: ...
    PSK identity: None
    PSK identity hint: None
    SRP username: None
    Start Time: 1775887995
    Timeout   : 7200 (sec)
    Verify return code: 0 (ok)
    Extended master secret: yes
---
DONE
```

### NGINX
### Front End SSL Configuration
Now that I've done ASP.NET Core, now it is time for NGINX. In [staging.nginx.conf](../HospitalProject.Client/staging.nginx.conf). Although, I've added a few SSL configuration items, which are below.
```
...
    http {
        ...
        ssl_protocols               TLSv1.2 TLSv1.3;
        ssl_ciphers                 HIGH:!aNULL:!MD5:!kRSA:!kDHE:!DSS:!PSK:!SRP:!ARIA:!CAMELLIA:!AESCCM;
        ssl_prefer_server_ciphers   off;
        ssl_session_tickets         off;
        ssl_session_cache           shared:SSL:10m;
        ssl_session_timeout 4h;
        ...
    }
...
```

These SSL directives are in the `http` block and they apply globally. The first line `ssl_protocols` directive specifies which TLS version that the server can use, which is TLS version 1.2 and version 1.3. And then the next one is `ssl_ciphers` with the value of `HIGH:!aNULL:!MD5:!kRSA:!kDHE:!DSS:!PSK:!SRP:!ARIA:!CAMELLIA:!AESCCM;`. To break down this string:  
* `HIGH` - any cipher suite that uses 128 bit encryption or higher. 
* `!aNULL` - means disable cipher suites that are not used for authentication. This is for stopping Man-In-The-Middle (MITM) attacks. 
* `!MD5` - don't include any ciphers that use MD5 hashing algorithms because they're weak. 
* `!kRSA` - to exclude standard RSA keys that are for exchange or authentication since they offer no perfect forward secrecy meaning if the server's private key is compromised in the future, past session keys can be decrypted.
* `!kDHE` - These type of keys are ephemeral and static it still doesn't provide forward secrecy (the ephemeral side could only verify the authenticity of the static side).
* `!DSS` - Cipher suites that include DSS (also known as DSA) for authentication is excluded they're weak because they're predictable or they have a bad entropy source used during signing (I think this is what was exploited in the Sony Playstation 3 and Android Bitcoin wallet hacks).
* `!PSK` - Cipher suites that use pre-shared keys are excluded because they're more used for IoT and VPNs, browser/server communication depends Public Key Infrastructure (PKI) certificate model (use public key certificates or Kerberos for authentication). 
* `!SRP` - Cipher suites containing Secure Remote Password (SRP) are designed to be used with passwords, and it is good protection against dictionary attacks. But it is computationally more expensive than the PSK cipher suites.
* `!ARIA` - excluded these as per [Security/Server Side TLS - MozillaWiki](https://wiki.mozilla.org/Security/Server_Side_TLS), ciphers suites containing `ARIA` have very little support.
* `!CAMELLIA` - Same reason as `ARIA`.
* `!AESCCM` - Cipher suites that contains `AESCCM` are more suited for Bluetooth (low powered) devices than web browsers. 

As a result, this is the cipher suites that the front end will be using:
```
> docker run --rm \
  --network=hospital-network \
  alpine/openssl ciphers -V 'HIGH:!aNULL:!MD5:!kRSA:!kDHE:!DSS:!PSK:!SRP:!ARIA:!CAMELLIA:!AESCCM' \
  | grep -E "TLSv1\.\d"   
          0x13,0x02 - TLS_AES_256_GCM_SHA384         TLSv1.3 Kx=any      Au=any   Enc=AESGCM(256)            Mac=AEAD
          0x13,0x03 - TLS_CHACHA20_POLY1305_SHA256   TLSv1.3 Kx=any      Au=any   Enc=CHACHA20/POLY1305(256) Mac=AEAD
          0x13,0x01 - TLS_AES_128_GCM_SHA256         TLSv1.3 Kx=any      Au=any   Enc=AESGCM(128)            Mac=AEAD
          0xC0,0x2C - ECDHE-ECDSA-AES256-GCM-SHA384  TLSv1.2 Kx=ECDH     Au=ECDSA Enc=AESGCM(256)            Mac=AEAD
          0xC0,0x30 - ECDHE-RSA-AES256-GCM-SHA384    TLSv1.2 Kx=ECDH     Au=RSA   Enc=AESGCM(256)            Mac=AEAD
          0xCC,0xA9 - ECDHE-ECDSA-CHACHA20-POLY1305  TLSv1.2 Kx=ECDH     Au=ECDSA Enc=CHACHA20/POLY1305(256) Mac=AEAD
          0xCC,0xA8 - ECDHE-RSA-CHACHA20-POLY1305    TLSv1.2 Kx=ECDH     Au=RSA   Enc=CHACHA20/POLY1305(256) Mac=AEAD
          0xC0,0x2B - ECDHE-ECDSA-AES128-GCM-SHA256  TLSv1.2 Kx=ECDH     Au=ECDSA Enc=AESGCM(128)            Mac=AEAD
          0xC0,0x2F - ECDHE-RSA-AES128-GCM-SHA256    TLSv1.2 Kx=ECDH     Au=RSA   Enc=AESGCM(128)            Mac=AEAD
          0xC0,0x24 - ECDHE-ECDSA-AES256-SHA384      TLSv1.2 Kx=ECDH     Au=ECDSA Enc=AES(256)               Mac=SHA384
          0xC0,0x28 - ECDHE-RSA-AES256-SHA384        TLSv1.2 Kx=ECDH     Au=RSA   Enc=AES(256)               Mac=SHA384
          0xC0,0x23 - ECDHE-ECDSA-AES128-SHA256      TLSv1.2 Kx=ECDH     Au=ECDSA Enc=AES(128)               Mac=SHA256
          0xC0,0x27 - ECDHE-RSA-AES128-SHA256        TLSv1.2 Kx=ECDH     Au=RSA   Enc=AES(128)               Mac=SHA256
```

In restricting the TLS/SSL ciphers that can be used when the browser is communicating to the server, the `ssl_prefer_server_ciphers` directive with the value of `off` forces the browser to use the ciphers that I have specified instead of the browser/client to choose an encryption method based on it's hardware capabilities. By restricting the list of ciphers and how ciphers will be selected. 

These two directives `ssl_session_tickets off;` & `ssl_session_cache shared:SSL:10m;` also had to be applied. `ssl_session_tickets` directive handles whether NGINX encrypts the entire SSL session state and then sends it to the client to store. And when the client reconnects, the session ticket is sent back to the server for the session to be decrypted and then resume the sessions. This is why I was trying to pick cipher suites where the session key(s) are not **static** (i.e. excluding cipher suites that use `kRSA` encryption), so once a session ticket key is generated, NGINX would hold it in memory and does not change until someone reboots the server (correct if I'm is wrong here). What I am trying to do is **Perfect Forward Secrecy (PFS) or just Forward Secrecy**, I'm trying to avoid the server's private key does not expose past session keys! 

`ssl_session_cache` directive takes in a value of `shared:SSL:10m;` which creates a shared memory cache which is available for the worker process to work with that has a named SSL with cache expiring every 10 minutes. This should hold about 40,000 sessions. Lastly, `ssl_session_timeout 4h;`, `ssl_session_timeout` define how long the `ssl_session_cache` will remain valid, for my staging environment it will be for 4 hours. 

### Reverse Proxy 
In the `location` block for reverse proxying the API call back to `/weatherforecast` endpoint, I've added a few more directives. 
```
...
        location /weatherforecast {
            proxy_pass https://hospitalproject.api.local:5229;
            
            proxy_ssl_protocols                 TLSv1.2 TLSv1.3;
            proxy_ssl_ciphers                   TLS_AES_256_GCM_SHA384:TLS_AES_128_GCM_SHA256:TLS_CHACHA20_POLY1305_SHA256:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-RSA-CHACHA20-POLY1305;
            ...
            proxy_ssl_session_reuse       on;
            proxy_ssl_trusted_certificate /etc/nginx/certs/HospitalProject.CA.pem;
        }
...
```

`proxy_ssl_protocols` sets the protocols to use, `proxy_ssl_ciphers` is the same list of ciphers set in [KestrelConfigurationExtensions.cs](../HospitalProject.Server/Extensions/KestrelConfigurationExtensions.cs). And lastly `proxy_ssl_session_reuse`, even though it is `on` by default, this helps reduce the CPU load and reuse the SSL session parameters.

## More NGINX Configuration
I still have some more NGINX configuration to do. And there is bunch of them. Before I create YAML files for deployment.

## HTTP Connection Upgrade for communication to/from ASP.NET Core
The thing I need to do is upgrade the connection to HTTP 2.0 when the API call being reversed proxied back. Currently, when the `weatherforecast` endpoint is called, it is using HTTP 1.1 as per the output of the logs.  
```
2026-04-11 15:58:03.343 | 2026-04-11 05:58:03 [INF] Application is shutting down...
2026-04-11 15:58:03.654 | 2026-04-11 05:58:03 [INF] Application started! Logging to both console and/or file.
2026-04-11 15:58:03.664 | 2026-04-11 05:58:03 [INF] ForwardedHeaders config — Headers: XForwardedFor, XForwardedProto, KnownProxies: 10.0.0.2
2026-04-11 15:58:03.707 | 2026-04-11 05:58:03 [WRN] Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'https://+:5229'.
2026-04-11 15:58:03.833 | 2026-04-11 05:58:03 [INF] Now listening on: https://[::]:5229
2026-04-11 15:58:03.833 | 2026-04-11 05:58:03 [INF] Application started. Press Ctrl+C to shut down.
2026-04-11 15:58:03.833 | 2026-04-11 05:58:03 [INF] Hosting environment: Staging
2026-04-11 15:58:03.833 | 2026-04-11 05:58:03 [INF] Content root path: /app
2026-04-11 15:58:06.053 | 2026-04-11 05:58:06 [INF] Request starting HTTP/1.1 GET https://localhost/weatherforecast - null null              <--- 👀👀👀👀 HERE
2026-04-11 15:58:06.068 | 2026-04-11 05:58:06 [INF] Request:
2026-04-11 15:58:06.068 | Protocol: HTTP/1.1                                                  <--- 👀👀👀👀 HERE
2026-04-11 15:58:06.068 | Method: GET
2026-04-11 15:58:06.068 | Scheme: https
2026-04-11 15:58:06.068 | PathBase: 
2026-04-11 15:58:06.068 | Path: /weatherforecast
2026-04-11 15:58:06.068 | Accept: */*
2026-04-11 15:58:06.068 | Host: localhost
2026-04-11 15:58:06.068 | User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
2026-04-11 15:58:06.068 | Accept-Encoding: gzip, deflate, br, zstd
2026-04-11 15:58:06.068 | Accept-Language: en-GB,en-US;q=0.9,en;q=0.8
2026-04-11 15:58:06.068 | Cache-Control: no-cache
2026-04-11 15:58:06.068 | Pragma: [Redacted]
2026-04-11 15:58:06.068 | Referer: https://localhost/
2026-04-11 15:58:06.068 | X-Original-Proto: https
2026-04-11 15:58:06.068 | sec-ch-ua-platform: "macOS"
2026-04-11 15:58:06.068 | sec-ch-ua: "Chromium";v="146", "Not-A.Brand";v="24", "Google Chrome";v="146"
2026-04-11 15:58:06.068 | sec-ch-ua-mobile: ?0
2026-04-11 15:58:06.068 | sec-fetch-site: same-origin
2026-04-11 15:58:06.068 | sec-fetch-mode: cors
2026-04-11 15:58:06.068 | sec-fetch-dest: empty
2026-04-11 15:58:06.068 | priority: u=1, i
2026-04-11 15:58:06.068 | X-Original-For: [::ffff:10.0.0.2]:55912
2026-04-11 15:58:06.069 | 2026-04-11 05:58:06 [INF] Executing endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-04-11 15:58:06.081 | 2026-04-11 05:58:06 [INF] Route matched with {action = "Get", controller = "WeatherForecast"}. Executing controller action with signature System.Collections.Generic.IEnumerable`1[HospitalProject.Server.WeatherForecast] Get() on controller HospitalProject.Server.Controllers.WeatherForecastController (HospitalProject.Server).
2026-04-11 15:58:06.084 | 2026-04-11 05:58:06 [INF] Executing ObjectResult, writing value of type 'HospitalProject.Server.WeatherForecast[]'.
2026-04-11 15:58:06.096 | 2026-04-11 05:58:06 [INF] Response:
2026-04-11 15:58:06.096 | StatusCode: 200
2026-04-11 15:58:06.096 | Content-Type: application/json; charset=utf-8
2026-04-11 15:58:06.102 | 2026-04-11 05:58:06 [INF] Executed action HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server) in 18.2892ms
2026-04-11 15:58:06.102 | 2026-04-11 05:58:06 [INF] Executed endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-04-11 15:58:06.103 | 2026-04-11 05:58:06 [INF] ResponseBody: [{"date":"2026-04-12","temperatureC":45,"temperatureF":112,"summary":"Freezing"},{"date":"2026-04-13","temperatureC":22,"temperatureF":71,"summary":"Cool"},{"date":"2026-04-14","temperatureC":48,"temperatureF":118,"summary":"Cool"},{"date":"2026-04-15","temperatureC":23,"temperatureF":73,"summary":"Freezing"},{"date":"2026-04-16","temperatureC":43,"temperatureF":109,"summary":"Sweltering"}]
2026-04-11 15:58:06.105 | 2026-04-11 05:58:06 [INF] Duration: 36.4ms
2026-04-11 15:58:06.107 | 2026-04-11 05:58:06 [INF] HTTP GET /weatherforecast responded 200 in 40.0728 ms
2026-04-11 15:58:06.109 | 2026-04-11 05:58:06 [INF] Request finished HTTP/1.1 GET https://localhost/weatherforecast - 200 null application/json; charset=utf-8 58.1173ms <--- 👀👀👀👀 HERE
```

All I have done is added one line in [staging.nginx.conf](../HospitalProject.Client/staging.nginx.conf).  
```
...
            proxy_http_version                  2;
...
```

Now the output from the ASP.NET Core logs shows the connection is HTTP 2.  
```
2026-04-11 16:19:37.991 | 2026-04-11 06:19:37 [INF] Application is shutting down...
2026-04-11 16:19:38.343 | 2026-04-11 06:19:38 [INF] Application started! Logging to both console and/or file.
2026-04-11 16:19:38.360 | 2026-04-11 06:19:38 [INF] ForwardedHeaders config — Headers: XForwardedFor, XForwardedProto, KnownProxies: 10.0.0.2
2026-04-11 16:19:38.408 | 2026-04-11 06:19:38 [WRN] Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'https://+:5229'.
2026-04-11 16:19:38.525 | 2026-04-11 06:19:38 [INF] Now listening on: https://[::]:5229
2026-04-11 16:19:38.525 | 2026-04-11 06:19:38 [INF] Application started. Press Ctrl+C to shut down.
2026-04-11 16:19:38.525 | 2026-04-11 06:19:38 [INF] Hosting environment: Staging
2026-04-11 16:19:38.525 | 2026-04-11 06:19:38 [INF] Content root path: /app
2026-04-11 16:20:32.898 | 2026-04-11 06:20:32 [INF] Request starting HTTP/2 GET https://localhost/weatherforecast - null null                <--- 👀👀👀👀 HERE
2026-04-11 16:20:32.912 | 2026-04-11 06:20:32 [INF] Request:
2026-04-11 16:20:32.912 | Protocol: HTTP/2                                                                                                   <--- 👀👀👀👀 HERE
2026-04-11 16:20:32.912 | Method: GET
2026-04-11 16:20:32.912 | Scheme: https
2026-04-11 16:20:32.912 | PathBase: 
2026-04-11 16:20:32.912 | Path: /weatherforecast
2026-04-11 16:20:32.912 | Accept: */*
2026-04-11 16:20:32.912 | Host: localhost
2026-04-11 16:20:32.912 | User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
2026-04-11 16:20:32.912 | Accept-Encoding: gzip, deflate, br, zstd
2026-04-11 16:20:32.912 | Accept-Language: en-GB,en-US;q=0.9,en;q=0.8
2026-04-11 16:20:32.912 | Cache-Control: no-cache
2026-04-11 16:20:32.912 | Pragma: [Redacted]
2026-04-11 16:20:32.912 | Referer: https://localhost/
2026-04-11 16:20:32.912 | X-Original-Proto: https
2026-04-11 16:20:32.912 | sec-ch-ua-platform: "macOS"
2026-04-11 16:20:32.912 | sec-ch-ua: "Chromium";v="146", "Not-A.Brand";v="24", "Google Chrome";v="146"
2026-04-11 16:20:32.912 | sec-ch-ua-mobile: ?0
2026-04-11 16:20:32.912 | sec-fetch-site: same-origin
2026-04-11 16:20:32.912 | sec-fetch-mode: cors
2026-04-11 16:20:32.912 | sec-fetch-dest: empty
2026-04-11 16:20:32.912 | priority: u=1, i
2026-04-11 16:20:32.912 | X-Original-For: [::ffff:10.0.0.2]:34728
2026-04-11 16:20:32.913 | 2026-04-11 06:20:32 [INF] Executing endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-04-11 16:20:32.924 | 2026-04-11 06:20:32 [INF] Route matched with {action = "Get", controller = "WeatherForecast"}. Executing controller action with signature System.Collections.Generic.IEnumerable`1[HospitalProject.Server.WeatherForecast] Get() on controller HospitalProject.Server.Controllers.WeatherForecastController (HospitalProject.Server).
2026-04-11 16:20:32.927 | 2026-04-11 06:20:32 [INF] Executing ObjectResult, writing value of type 'HospitalProject.Server.WeatherForecast[]'.
2026-04-11 16:20:32.938 | 2026-04-11 06:20:32 [INF] Response:
2026-04-11 16:20:32.938 | StatusCode: 200
2026-04-11 16:20:32.938 | Content-Type: application/json; charset=utf-8
2026-04-11 16:20:32.941 | 2026-04-11 06:20:32 [INF] Executed action HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server) in 14.8505ms
2026-04-11 16:20:32.941 | 2026-04-11 06:20:32 [INF] Executed endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-04-11 16:20:32.942 | 2026-04-11 06:20:32 [INF] ResponseBody: [{"date":"2026-04-12","temperatureC":-19,"temperatureF":-2,"summary":"Hot"},{"date":"2026-04-13","temperatureC":2,"temperatureF":35,"summary":"Scorching"},{"date":"2026-04-14","temperatureC":8,"temperatureF":46,"summary":"Freezing"},{"date":"2026-04-15","temperatureC":-4,"temperatureF":25,"summary":"Cool"},{"date":"2026-04-16","temperatureC":15,"temperatureF":58,"summary":"Cool"}]
2026-04-11 16:20:32.943 | 2026-04-11 06:20:32 [INF] Duration: 31.2729ms
2026-04-11 16:20:32.945 | 2026-04-11 06:20:32 [INF] HTTP GET /weatherforecast responded 200 in 34.3721 ms
2026-04-11 16:20:32.947 | 2026-04-11 06:20:32 [INF] Request finished HTTP/2 GET https://localhost/weatherforecast - 200 null application/json; charset=utf-8 50.0063ms             <--- 👀👀👀👀 HERE
```

And to verify that Application-Layer Protocol Negotiation (APLN) is being done. I'm going to use OpenSSL to check. 

## keepalive Connection Cache
In order to make NGINX more performant and reduce the number of SSL/TLS handshakes and making the session cache more effective, I've added the `upstream` with a `keepalive` value which caches the connections inside the `http` block.
```
...
    upstream hospitalproject_api {
        server      hospitalproject.api.local:5229;
        keepalive   16;
    }
...
```

But I also had to update the `proxy_pass` value to reference the upstream block inside the `location` block inside the `server` block.  
```
...
    server
    {
        ...

        location /weatherforecast {
            proxy_pass https://hospitalproject_api;
            ...
        }

    }
...
```

The `keepalive` directive will cache a connection then the multiplex streams over it. After the API calls are finish with serving it, the `keepalive` directive holds it in the cache pool for the next request. One thing to note is that is that you need to add a `Connection` header in the `/location` block for HTTP/1 and HTTP/1.1 protocols but it is not needed for HTTP/2.

## Adding Additional Security Headers
A good thing about setting NGINX as a reverse proxy (at the moment), no CORS is required to be setup. This is because the browser does not see two API calls being one meaning an API call for the frontend URL and another API call with a different URL for the backend. This also means that the browser does not request a pre flight check (it shouldn't now if I did not use a reverse proxy, only when I have the authentication middleware added in ASP.NET Core). Everything is one source hence the browser will put the header for `Sec-Fetch-Site` as `same-origin` and `Referrer-Policy` as `strict-origin-when-cross-origin`. This is currently the response from the server when the page is loaded:  
![Response Headers from NGINX](./images/Screenshot%202026-04-11%20at%204.48.54 pm.png)

But I am going to put some more security headers.
```
...
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header Referrer-Policy "strict-origin-when-cross-origin" always;
        add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self'; connect-src 'self'; frame-src: 'self';" always;
        add_header Strict-Transport-Security "max-age=604800" always;
...
```

The are doing the following:  
* `add_header X-Frame-Options "SAMEORIGIN" always;` -  any `iframe`, `frame`, `embed` or `object` can only be rendered on the same domain.
* `add_header X-Content-Type-Options "nosniff" always;` - prevents MIME type sniffing attacks by strictly adhering to the `Content-Type` header.
* `add_header Referrer-Policy "strict-origin-when-cross-origin" always;`, probably didn't need to add this since by default this policy is always applied but doesn't hurt if you don't have it.
* `add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self'; connect-src 'self'; frame-src 'self';" always;` - This header will have some changes or additions later on; but for now, the `Content-Security-Policy` header will provide more protection against XSS attacks. But what these directives are doing:  
   |Directive           |Comments                                                                        |
   |--------------------|--------------------------------------------------------------------------------|
   |`default-src 'self'`|Fallback for all other fetch directives.                                        |
   |`script-src 'self'` |Directive for specifying valid sources for JavaScript and WebAssembly resources.|
   |`style-src 'self'`  |Directive for CCS style and also excluding inline CSS (at the moment).          |
   |`connect-src 'self'`|Directive for restricting URL which can be loaded using script interfaces.      |
   |`frame-src 'self'`  |Directive for restricting sources for `<frame>` and `<iframe>`.                 |

   All the values after the directive is `self` so everything should have source of the domain that I specified (`localhost`, & `hospitalproject.local`).
* Lastly, `add_header Strict-Transport-Security "max-age=604800" always;` where I'm adding HTTP Strict Transport Security (HSTS) to help protect man-in-the-middle attacks, downgrading the HTTP protocol attacks and also cookie hijacking.

Once you've restarted your NGINX server with your configuration and then load the page then it appears in your response headers.  
![Additional headers now appearing](../Documentation/images/Screenshot%202026-04-13%20at%202.01.47 pm.png)

Lastly, the `always` value for `add_header` directive will always add these headers regardless of response.

### One note about HSTS
Once you've enabled HSTS, the browser will remember is based on the `max-age=604800` value. What HSTS also does, is always request the site with HTTPS when you type HTTP. If you want to remove it you it, on Chromium based browsers you will need to go to [chrome://net-internals/#hsts](chrome://net-internals/#hsts) and from there you can delete it.

## Another Server block for HTTP Direction
So since I have HSTS enabled, I added another `server` block in [staging.nginx.conf](../HospitalProject.Client/staging.nginx.conf) to perform the redirect from HTTP to HTTPS.  
```
...
    server {
        listen 80 default_server;
        server_name _;
        return 301 https://$host$request_uri;
    }   
...
```

With this new `server` block, it will capture all incoming requests that are HTTP and then redirect the users/visitors to an HTTPS connection. But I had to create another container with port 80 exposed.  
```
> docker run -d --network hospital-network --ip 10.0.0.2 --network-alias hospitalproject.local -p 443:443 -p 80:80 \
-v ~/Projects/HospitalProject/HospitalProject.Client/staging.nginx.conf:/etc/nginx/nginx.conf:ro \
-v ~/Workspaces/Certs/hospital.project/hospital.project.client.key:/etc/nginx/certs/hospitalproject.client.key:ro \
-v ~/Workspaces/Certs/hospital.project/hospital.project.client.pem:/etc/nginx/certs/hospitalproject.client.pem:ro \
-v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/etc/nginx/certs/HospitalProject.CA.pem:ro \
hospital.project.client
5abaa668ea046a0cb0ec30c338d8aac5c9d09e283bb4cdca5a0f2d8631b49909
```

And I also had to update the [Dockerfile](../HospitalProject.Client/Dockerfile) as well which also will expose port 80 when creating the container.  
```dockerfile
...
EXPOSE 80 443
...
```

## Gzip Compression
The next thing I want to apply is Gzip compression which I added to the `http` block.
```
http {
    ...
    gzip on;
    gzip_vary on;
    gzip_comp_level 5;
    gzip_min_length 1024;
    gzip_buffers 16 8k;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/javascript
        application/xml+rss
        application/json
        image/svg+xml;
    ...
}
```

`gzip_comp_level` is set to 5 and since I'm using docker I'm more for speed at the moment than best compression (maybe I should do 6 but also taking into consideration that I'm also working on my MacBook Air). `gzip_min_length` is 1024, anything less than this value won't be compressed. `gzip_buffers` I set the number attribute to 16 buffers and size of 8 Kilobytes each. `gzip_types` I've defined that types that gzip compression should apply to. 

With this current configuration the output from the weather forecast endpoint is compressed and this is not good, because it would lead to BREACH attacks since I am using HTTPS. And currently the weather forecast API call is being compressed.  
![Weather forecast response being compressed](./images/Screenshot%202026-04-15%20at%203.51.31 pm.png)

Which is what I don't want for text type responses from ASP.NET Core backend. What I have done is added `gzip off` in the `location` block for the weather forecast endpoint.  
```
...
        location /weatherforecast {
            gzip off;
            proxy_pass https://hospitalproject_api;
            ...
        }
...
```

Now everything else is compressed except for JSON responses from ASP.NET Core.  
![weather forecast endpoint no longer compressed](./images/Screenshot%202026-04-15%20at%205.35.01 pm.png)

# References {#references}
* [Writing a Dockerfile | Docker Docs](https://docs.docker.com/get-started/docker-concepts/building-images/writing-a-dockerfile/)
* [Dockerfile reference | Docker Docs](https://docs.docker.com/reference/dockerfile/)
* [Best practices | Docker Docs](https://docs.docker.com/build/building/best-practices/)
* [Part 1: Containerize an application | Docker Docs](https://docs.docker.com/get-started/workshop/02_our_app/)
* [Hosting ASP.NET Core image in container using docker compose with HTTPS | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-10.0)
* [How to Dockerize a React App: A Step-by-Step Guide for Developers | Docker](https://www.docker.com/blog/how-to-dockerize-react-app/)
* [nginx - Official Image | Docker Hub](https://hub.docker.com/_/nginx)
* [node - Official Image | Docker Hub](https://hub.docker.com/_/node)
* [How To Deploy a React Application with Nginx on Ubuntu | DigitalOcean](https://www.digitalocean.com/community/tutorials/deploy-react-application-with-nginx-on-ubuntu)
* [Optimize cache usage in builds | Docker Docs](https://docs.docker.com/build/cache/optimize/)
* [openssl-genrsa - OpenSSL Documentation](https://docs.openssl.org/master/man1/openssl-genrsa/)
* [x509v3_config - OpenSSL Documentation](https://docs.openssl.org/master/man5/x509v3_config/)
* [How to install CA certificates and PKCS12 key bundles on different platforms · GitHub](https://gist.github.com/alanbacelar/c2642c51e9a96e0cff90ef52244ef4b7#file-chromium-linux-md)
* [Configuring ASP.NET Core Forwarded Headers Middleware](https://nestenius.se/net/configuring-asp-net-core-forwarded-headers-middleware/)
* [TLS 1.2 vs TLS 1.3: Key Differences | A10 Networks](https://www.a10networks.com/glossary/key-differences-between-tls-1-2-and-tls-1-3/)
* [tls - The difference between Subject Key Identifier and sha1Fingerprint in X509 Certificates - Information Security Stack Exchange](https://security.stackexchange.com/questions/200295/the-difference-between-subject-key-identifier-and-sha1fingerprint-in-x509-certif)
* [What extensions and details are included in a SSL certificate?](https://knowledge.digicert.com/solution/what-extensions-and-details-are-included-in-a-ssl-certificate)
* [pkcs12 - OpenSSL Documentation](https://docs.openssl.org/1.1.1/man1/pkcs12/)
* [openssl verify returns ok but certificate is untrusted · Issue #21870 · openssl/openssl](https://github.com/openssl/openssl/issues/21870)
* [openssl-verification-options - OpenSSL Documentation](https://docs.openssl.org/3.0/man1/openssl-verification-options/)
* [verify - OpenSSL Documentation](https://docs.openssl.org/1.0.2/man1/verify/)
* [Ubuntu Manpage: certutil - Manage keys and certificate in both NSS databases and other NSS tokens](https://manpages.ubuntu.com/manpages/xenial/man1/certutil.1.html)
* [Configure endpoints for the ASP.NET Core Kestrel web server | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0#configure-https)
* [TLS Cipher Suites in Windows Server 2022 - Win32 apps | Microsoft Learn](https://learn.microsoft.com/en-us/windows/win32/secauthn/tls-cipher-suites-in-windows-server-2022)
* [openssl s_client commands and examples - Mister PKI](https://www.misterpki.com/openssl-s-client/)
* [openssl-s_client - OpenSSL Documentation](https://docs.openssl.org/3.0/man1/openssl-s_client/)
* [network - How do I read the output of dns-sd? - Ask Different](https://apple.stackexchange.com/questions/115674/how-do-i-read-the-output-of-dns-sd)
* [apache - SSLCipherSuite aliases - Stack Overflow](https://stackoverflow.com/questions/28737374/sslciphersuite-aliases)
* [Configuring HTTPS servers](https://nginx.org/en/docs/http/configuring_https_servers.html)
* [NGINX SSL Termination | NGINX Documentation](https://docs.nginx.com/nginx/admin-guide/security-controls/terminating-ssl-http/)
* [Strict-Transport-Security header - HTTP | MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Strict-Transport-Security)
* [Referrer-Policy header - HTTP | MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Referrer-Policy)
* [X-Frame-Options header - HTTP | MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/X-Frame-Options)
* [HTTP Strict Transport Security](https://www.chromium.org/hsts/)
* [man 1 openssl-ciphers](https://www.nevis.columbia.edu/cgi-bin/man.sh?man=1+openssl-ciphers)
* [tls - Recommended ssl_ciphers for security, compatibility - Perfect Forward secrecy - Information Security Stack Exchange](https://security.stackexchange.com/questions/54639/recommended-ssl-ciphers-for-security-compatibility-perfect-forward-secrecy)
* [Ciphersuite Info](https://ciphersuite.info/cs/?tls=all&sort=asc&security=all&singlepage=true&software=openssl)
* [Mozilla SSL Configuration Generator](https://ssl-config.mozilla.org/)
* [Security/Server Side TLS - MozillaWiki](https://wiki.mozilla.org/Security/Server_Side_TLS)
* [A quarter of major CMSs use outdated MD5 as the default password hashing scheme | ZDNET](https://www.zdnet.com/article/a-quarter-of-major-cmss-use-outdated-md5-as-the-default-password-hashing-scheme/)
* [diffie hellman - RSA Key Exchange Attack - Cryptography Stack Exchange](https://crypto.stackexchange.com/questions/103276/rsa-key-exchange-attack)
* [Why Static RSA and Diffie-Hellman cipher suites have been removed in TLS 1.3? - Cryptography Stack Exchange](https://crypto.stackexchange.com/questions/67604/why-static-rsa-and-diffie-hellman-cipher-suites-have-been-removed-in-tls-1-3)
* [Diffie–Hellman key exchange - Wikipedia](https://en.wikipedia.org/wiki/Diffie%E2%80%93Hellman_key_exchange#Ephemeral_and/or_static_keys)
* [SSL/TLS: How to choose your cipher suite - Conclusion AMIS Technology Blog](https://technology.amis.nl/architecture/security/ssltls-choose-cipher-suite/)
* [Cipher suites. Which are safe? and which not? | LinkedIn](https://www.linkedin.com/pulse/cipher-suites-which-safe-ramkumar-nadar/)
* [Pre-Shared Key Ciphersuites for Transport Layer Security (TLS)](https://datatracker.ietf.org/doc/html/rfc4279#ref-SRP)
* [RFC 5077 - Transport Layer Security (TLS) Session Resumption without Server-Side State](https://datatracker.ietf.org/doc/html/rfc5077)
* [What Is Perfect Forward Secrecy (PFS) in TLS?](https://deepstrike.io/blog/what-is-perfect-forward-secrecy-pfs)
* [Securing HTTP Traffic to Upstream Servers | NGINX Documentation](https://docs.nginx.com/nginx/admin-guide/security-controls/securing-http-traffic-upstream/)
* [Module ngx_http_upstream_module](https://nginx.org/en/docs/http/ngx_http_upstream_module.html)
* [Content-Security-Policy (CSP) header - HTTP | MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Content-Security-Policy)
* [Module ngx_http_headers_module](https://nginx.org/en/docs/http/ngx_http_headers_module.html)
* [Redirect HTTP to HTTPS in Nginx | Linuxize](https://linuxize.com/post/redirect-http-to-https-in-nginx/)
* [Module ngx_http_gzip_module](https://nginx.org/en/docs/http/ngx_http_gzip_module.html)
* [What is Cache-Control and How HTTP Cache Headers Work | CDN Guide | Imperva](https://www.imperva.com/learn/performance/cache-control/)
* [ETag header - HTTP | MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/ETag)
* [Last-Modified header - HTTP | MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Last-Modified)
* [Authorization header - HTTP | MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Authorization)
* [Understanding HTTP Caching Headers: Cache-Control, ETag, Expires | HttpStatus.com](https://httpstatus.com/learn/understanding-http-caching-headers-cache-control-etag-expires)
* [BREACH - Wikipedia](https://en.wikipedia.org/wiki/BREACH)