# Introduction 
Now that I have installed the scaffolding for both ASP.NET Core WebApi backend and ReactJS frontend. I want to do a little bit of a cleanup and make sure for starters, everything is going to be running on HTTPs protocol. First going to start off with cleaning up HospitalProject.Server project and then the HospitalProject.Client project.

# HospitalProject.Server (ASP.NET Core Web API)
## Deleting the **HospitalProject.Server.http** file
First thing I'm going to is delete the **HospitalProject.Server.http**, this file works with Visual Studio but not VS code (at least I can't find a plugin for this as yet):  
```cwd
rm HospitalProject.Server/HospitalProject.Server.http
```

## Creating VS Code debug launch configuration
The next thing I'm going to do is create a VS Code launch configuration for launching ASP.NET Core in Debug Mode. And this is is pretty straight forward. In VS code, make sure that [Program.cs](../HospitalProject.Server/Program.cs) is open and then click on the Debug and Run button on the left side plane and then click on *Run and Debug*.   
![Run and Debug ASP.NET Core Application](./images/Screenshot%202026-03-06%20at%208.17.53 pm.png)

You should see a command pallette with the options for the different type of debuggers, click on the C# option.  
![Command Pallette of debugger options](./images/Screenshot%202026-03-06%20at%208.21.57 pm.png)

Next, you will get another command pallette, for which launch configuration you want to launch, select the *C#: HospitalProject.Sever [https]* option.  
![select the C#: HospitalProject.Sever \[https\] option](./images/Screenshot%202026-03-06%20at%208.22.36 pm.png)

You will get a dialog message to enter your password in order to run ASP.NET Core application:  
![Message Dialog to enter password to run app](./images/Screenshot%202026-03-06%20at%208.23.35 pm.png)

Enter your password and then application is now running in debug mode. You should see an output on the Debug Console as the following:  
```cmd

------------------------------------------------------------------------------
You may only use the Microsoft Visual Studio .NET/C/C++ Debugger (vsdbg) with
Visual Studio Code, Visual Studio or Visual Studio for Mac software to help you
develop and test your applications.
------------------------------------------------------------------------------
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7083
Microsoft.Hosting.Lifetime: Information: Now listening on: https://localhost:7083
Microsoft.Hosting.Lifetime: Information: Now listening on: http://localhost:5229
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5229
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
Microsoft.Hosting.Lifetime: Information: Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
Microsoft.Hosting.Lifetime: Information: Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/zamk/Projects/HospitalProject/HospitalProject.Server
Microsoft.Hosting.Lifetime: Information: Content root path: /Users/zamk/Projects/HospitalProject/HospitalProject.Server
The program '[30826] HospitalProject.Server' has exited with code 0 (0x0).
```

The HTTP protocol is also running, so I'm going to update and remove the HTTP URL from the `https` launch configuration from [launchSettings.json](../HospitalProject.Server/Properties/launchSettings.json) file.  
```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7083",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Now the Debug Console output only shows HTTPS.  
```cmd
------------------------------------------------------------------------------
You may only use the Microsoft Visual Studio .NET/C/C++ Debugger (vsdbg) with
Visual Studio Code, Visual Studio or Visual Studio for Mac software to help you
develop and test your applications.
------------------------------------------------------------------------------
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7083
Microsoft.Hosting.Lifetime: Information: Now listening on: https://localhost:7083
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
Microsoft.Hosting.Lifetime: Information: Application started. Press Ctrl+C to shut down.
Microsoft.Hosting.Lifetime: Information: Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/zamk/Projects/HospitalProject/HospitalProject.Server
Microsoft.Hosting.Lifetime: Information: Content root path: /Users/zamk/Projects/HospitalProject/HospitalProject.Server
```

## Adding Serilog library for logging
The next thing I want to do is add the Serilog library for my logging, to do this, I'm going to run the following command:  
```cmd
    dotnet add package Serilog.AspNetCore --project HospitalProject.Server
```

Once this is installed, I need to add some configuration to [Program.cs](../HospitalProject.Server/Program.cs), well actually it's a little more than just adding configuration.   
```csharp
    using Serilog;

    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateBootstrapLogger();

    try
    {
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
        );

        builder.Services.AddControllers();

        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseSerilogRequestLogging();
        Log.Information("Application started! Logging to both console and/or file.");

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Application start-up failed!");
    }
    finally
    {
        Log.CloseAndFlush();
    }
```

After installing the package, the first thing I had was initialise the logger with:  
```csharp
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateBootstrapLogger();
```

I had to move some stuff around, because I am using `try-catch-finally` block because if the app stops running or the user stops the app the logs needs to be closed and the buffer needs to be flushed out with `Log.CloseAndFlush();`.

I don't like to add unnecessary things for the production build. But going back to the logger configuration, I followed the two-stage initialization first by creating a bootstrap with `.CreateBootstrapLogger();` when I'm intialising the logger and then adding SerialLog as a service to replace the original logger completely once the host is loaded with:  
```csharp
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
    );
```

And lastly, I added the Serilog middleware with `app.UseSerilogRequestLogging();` just before the initialisation of the `app` variable.  


I like doing configuration from configuration files as much as possible. So in my [appsettings.Development.json](../HospitalProject.Server/appsettings.Development.json), this is the config that I have added to log stuff out to both console and to a log file:  
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/app.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 10,
          "fileSizeLimitBytes": 10485760,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

When you relaunch from the debugger, you should see a **Logs** Directory created:  
```cmd
> ls -g
total 40
drwxr-xr-x@ 3 staff   96 Mar  6 19:16 Controllers
-rw-r--r--@ 1 staff  393 Mar  6 20:42 HospitalProject.Server.csproj
drwxr-xr-x@ 3 staff   96 Mar  6 21:06 Logs                                      <-- This one!!!!
-rw-r--r--@ 1 staff  933 Mar  6 21:05 Program.cs
drwxr-xr-x@ 3 staff   96 Mar  6 20:53 Properties
-rw-r--r--@ 1 staff  259 Mar  6 19:16 WeatherForecast.cs
-rw-r--r--@ 1 staff  675 Mar  6 21:06 appsettings.Development.json
-rw-r--r--@ 1 staff  142 Mar  6 19:16 appsettings.json
drwxr-xr-x@ 3 staff   96 Mar  6 19:16 bin
drwxr-xr-x@ 8 staff  256 Mar  6 20:42 obj
```

And your log output in the Debug Console should be the same as as your log file:  
```cmd
------------------------------------------------------------------------------
You may only use the Microsoft Visual Studio .NET/C/C++ Debugger (vsdbg) with
Visual Studio Code, Visual Studio or Visual Studio for Mac software to help you
develop and test your applications.
------------------------------------------------------------------------------
2026-03-06 21:06:37 [INF] Application started! Logging to both console and/or file.
2026-03-06 21:06:37 [INF] Now listening on: https://localhost:7083
2026-03-06 21:06:37 [INF] Application started. Press Ctrl+C to shut down.
2026-03-06 21:06:37 [INF] Hosting environment: Development
2026-03-06 21:06:37 [INF] Content root path: /Users/zamk/Projects/HospitalProject/HospitalProject.Server
```

A little sparse on information from before, but I'll fix that up later. But right now I'm also going to update the [appsettings.json](../HospitalProject.Server/appsettings.json), however the logging level will be at *Warning level* and only sinking out to a file.  
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Warning"
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "Logs/app.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 10,
          "fileSizeLimitBytes": 10485760,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
```

### Adding HTTP Logging Middleware
Since I'm trying to implement a reverse proxy setup with my web application stack. For debugging purposes I also added the `AddHttpLogging()` and `UseHttpLogging()` to capture my request for debugging later. At the moment it is setup for development mode.  
```csharp
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateBootstrapLogger();

    try {
        if (builder.Environment.IsDevelopment())
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
            }); // In development mode only
        }
        ...
        var app = builder.Build();
        app.UseSerilogRequestLogging();

        Log.Information("Application started! Logging to both console and file.");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpLogging(); // In development mode only
            app.MapOpenApi();
        }
    }
    ...
```

The outputs will appear in the console and logs as well which shows something like this:  
```cmd
------------------------------------------------------------------------------
You may only use the Microsoft Visual Studio .NET/C/C++ Debugger (vsdbg) with
Visual Studio Code, Visual Studio or Visual Studio for Mac software to help you
develop and test your applications.
------------------------------------------------------------------------------
2026-03-06 21:17:45 [INF] Application started! Logging to both console and/or file.
2026-03-06 21:17:46 [INF] Now listening on: https://localhost:7083
2026-03-06 21:17:46 [INF] Application started. Press Ctrl+C to shut down.
2026-03-06 21:17:46 [INF] Hosting environment: Development
2026-03-06 21:17:46 [INF] Content root path: /Users/zamk/Projects/HospitalProject/HospitalProject.Server
2026-03-06 21:17:56 [INF] Request starting HTTP/2 GET https://localhost:7083/weatherforecast - null null
2026-03-06 21:17:56 [INF] Request:
Protocol: HTTP/2
Method: GET
Scheme: https
PathBase:
Path: /weatherforecast
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
Host: localhost:7083
User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36
Accept-Encoding: gzip, deflate, br, zstd
Accept-Language: en-GB,en-US;q=0.9,en;q=0.8
Upgrade-Insecure-Requests: [Redacted]
sec-ch-ua: "Not:A-Brand";v="99", "Google Chrome";v="145", "Chromium";v="145"
sec-ch-ua-mobile: ?0
sec-ch-ua-platform: "macOS"
sec-fetch-site: none
sec-fetch-mode: navigate
sec-fetch-user: [Redacted]
sec-fetch-dest: document
priority: u=0, i
2026-03-06 21:17:56 [INF] Executing endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-03-06 21:17:56 [INF] Route matched with {action = "Get", controller = "WeatherForecast"}. Executing controller action with signature System.Collections.Generic.IEnumerable`1[HospitalProject.Server.WeatherForecast] Get() on controller HospitalProject.Server.Controllers.WeatherForecastController (HospitalProject.Server).
2026-03-06 21:17:56 [INF] Executing ObjectResult, writing value of type 'HospitalProject.Server.WeatherForecast[]'.
2026-03-06 21:17:56 [INF] Response:
StatusCode: 200
Content-Type: application/json; charset=utf-8
2026-03-06 21:17:56 [INF] Executed action HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server) in 17.237ms
2026-03-06 21:17:56 [INF] Executed endpoint 'HospitalProject.Server.Controllers.WeatherForecastController.Get (HospitalProject.Server)'
2026-03-06 21:17:56 [INF] ResponseBody: [{"date":"2026-03-07","temperatureC":50,"temperatureF":121,"summary":"Cool"},{"date":"2026-03-08","temperatureC":1,"temperatureF":33,"summary":"Hot"},{"date":"2026-03-09","temperatureC":44,"temperatureF":111,"summary":"Mild"},{"date":"2026-03-10","temperatureC":-7,"temperatureF":20,"summary":"Scorching"},{"date":"2026-03-11","temperatureC":-20,"temperatureF":-3,"summary":"Sweltering"}]
2026-03-06 21:17:56 [INF] Duration: 31.8541ms
2026-03-06 21:17:56 [INF] HTTP GET /weatherforecast responded 200 in 34.3110 ms
2026-03-06 21:17:56 [INF] Request finished HTTP/2 GET https://localhost:7083/weatherforecast - 200 null application/json; charset=utf-8 53.9626ms
```

After you have setup your front end, you will need to add headers that you want to capture. This is after I set up my ReactJS frontend for adding additional logging for the headers that I wanted to capture in debug mode.  
```csharp
...
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
});
...
```

I might have to set some limits on the HTTP Logging later, but for now this is good enough to start off with.

# Vite SSL configuration for ReactJS Frontend
When I ran `npm run dev`, it's running with the HTTP protocol:  
```cmd
> npm run dev                                                                           

> hospitalproject-client@0.0.0 dev
> vite


  VITE v7.3.0  ready in 3121 ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
  ➜  press h + enter to show help
```

And my Chrome Browser is complaining that my website is *not secure*:  
![Chrome is saying my website is not secure](./images/Screenshot%202026-03-06%20at%209.24.34 pm.png)

So next thing is to configure ViteJS with SSL, so our connection is secure!

## SSL certificate generation
First thing I need to do is to generate the SSL pem certificate and store it somewhere. I've created a **~/Workspaces/Certs/** directory in my user account directory and I'm going to generate the pem and key files with the `dotnet` tool and create one more directory inside **~/Workspaces/Certs/** called **dotnet** and store it there.  
```cmd
mkdir -p ~/Workspaces/Certs/dotnet
dotnet dev-certs https -ep ~/Workspaces/Certs/dotnet/hospitalproject.client.pem --format pem -np
```

When you run the above command, you will get another dialog box to enter your password:  
![Dialog Box to enter password](./images/Screenshot%202026-03-06%20at%209.44.38 pm.png)

After entering the password and then clicking the *Allow* button, this will generate a pem and key file in the directory:  
```cmd
> mkdir -p ~/Workspaces/Certs/dotnet
> dotnet dev-certs https -ep ~/Workspaces/Certs/dotnet/hospitalproject.SSC.pem --format pem -np
A valid HTTPS certificate is already present.
> ls ~/Workspaces/Certs/dotnet
hospitalproject.SSC.key      hospitalproject.SSC.pem
```

At the moment the **vite.config.ts** file looks pretty barren.  
```ts
  import { defineConfig } from 'vite'
  import react from '@vitejs/plugin-react'

  // https://vite.dev/config/
  export default defineConfig({
    plugins: [react()],
  })
```

But it will get bigger in the next section (just a little bit).

## Updating **vite.config.ts** file
### Adding SSL encryption to website
Now I need to start modifying **vite.config.ts** file and make the site hosting with SSL enabled. The first thing I need to is check if the directory contains my certificates, if it does great, if it doesn't vite should throw an error not start the server.  
```ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'node:path';
import { readFileSync, existsSync } from 'node:fs';
import os from "node:os";

const certName = 'hospitalproject.SSC';
const certFolder = path.join(os.homedir(), 'Workspaces', 'Certs', 'dotnet');
const certPath = path.join(certFolder, `${certName}.pem`);
const keyPath = path.join(certFolder, `${certName}.key`);

if (!existsSync(certPath) || !existsSync(keyPath)) {
  throw new Error('Certificate not found.');
}
...
```

I kept the port the same as what you get when you create a ReactJS (TypeScript) project, but you don't have to add it because it defaults to this port anyways (I just have bad memory and forget where the port number comes from so I documented it 😅). But I had to reference my SSL pem and key files to it. When I run the `npm run dev` command, I'm getting an output like this.  
```cmd
> npm run dev

> hospitalproject-client@0.0.0 dev
> vite


  VITE v7.3.1  ready in 145 ms

  ➜  Local:   https://localhost:5173/
  ➜  Local:   https://vite.dev.localhost:5173/
  ➜  Local:   https://vite.dev.internal:5173/
  ➜  Local:   https://host.docker.internal:5173/
  ➜  Local:   https://host.containers.internal:5173/
  ➜  Network: use --host to expose
  ➜  press h + enter to show help
```

Everything now is running with HTTPS protocol so far good. When I click the `https://localhost:5173/` URL, I can see now that Chrome is saying the website is secure.  
![Chrome is saying the website is secure](./images/Screenshot%202026-03-06%20at%2010.06.19 pm.png)

### Setting up the Proxy to backend API calls
The next thing is to setup a proxy for the backend to be called.  
```ts
...
import { env } from "node:process"
...
const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` : 'https://localhost:7083';
...
// https://vite.dev/config/
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
        secure: true
      }
    }
  }
});
...
```

As you can see I have added a `proxy` configuration. The `target` is the URL for my ASP.NET Core backend endpoint. I set the `secure` property to `true` so it has to do SSL validation. The `'^/weatherforecast'` is a regex expression where it makes sure that the beginning starts with '/'. Going forward when I create more controllers with different endpoints. I need to map it to this `proxy` config in [vite.config.ts](../HospitalProject.Client/vite.config.ts). 

Before I go any further I need to add some code to [App.tsx](../HospitalProject.Client/src/App.tsx) to do an API calls to the *weatherforecast* endpoint. 
```tsx
import { useEffect, useRef, useState } from 'react';
...

interface Forecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
};

function App() {
  const [count, setCount] = useState(0);
  const [forecasts, setForecasts] = useState<Forecast[]>();
  const hasFetchedRef = useRef(false);

  useEffect(() => {
    if (hasFetchedRef.current) return;
    hasFetchedRef.current = true;

    const populateWeatherForecasts = async () => {
      const response = await fetch('weatherforecast');
      if (response.ok) {
        const data = await response.json();
         setForecasts(data);
      }
    };
    
    populateWeatherForecasts();
  }, []);


  return (
    <>
    ...
        <div className="weather-forecasts">
          {!forecasts ? <p>Weather Forecast Loading...</p> : 
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Temp. (C)</th>
                  <th>Temp. (F)</th>
                  <th>Summary</th>
                </tr>
              </thead>
              <tbody>
                {forecasts.map((forecast, index) => (
                  <tr key={index}>
                    <td>{new Date(forecast.date).toLocaleDateString()}</td>
                    <td>{forecast.temperatureC}</td>
                    <td>{forecast.temperatureF}</td>
                    <td>{forecast.summary}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          }
      </div>     
      ...
    </>
  )
}
...
```

So I'm ready again to fire up both the front end and back end. But when the page loads, in the console you will get this error:  
```cmd
> cd HospitalProject.Client
> npm run dev

> hospitalproject-client@0.0.0 dev
> vite


  VITE v7.3.1  ready in 183 ms

  ➜  Local:   https://localhost:5173/
  ➜  Local:   https://vite.dev.localhost:5173/
  ➜  Local:   https://vite.dev.internal:5173/
  ➜  Local:   https://host.docker.internal:5173/
  ➜  Local:   https://host.containers.internal:5173/
  ➜  Network: use --host to expose
  ➜  press h + enter to show help
1:18:11 AM [vite] http proxy error: /weatherforecast
Error: self-signed certificate; if the root CA is installed locally, try running Node.js with --use-system-ca
    at TLSSocket.onConnectSecure (node:internal/tls/wrap:1648:34)
    at TLSSocket.emit (node:events:508:20)
    at TLSSocket._finishInit (node:internal/tls/wrap:1094:8)
    at ssl.onhandshakedone (node:internal/tls/wrap:880:12)

```


And no request is made to the backend, I can in definitely confirm that if you add the environment variable `NODE_USE_SYSTEM_CA=1` on Windows, the error `Error: self-signed certificate; if the root CA is installed locally, try running Node.js with --use-system-ca` goes away, because the command `dotnet dev-certs https --trust` already installed the self-signed certificate on the Windows Certificate store and it is the same on the MacOS certificate/keychain store and it's the same certificate that you're using for your ASP.NET Core application, but on my MacOS and Ubuntu, this is not the case. 

#### Set Environment variable NODE_EXTRA_CA_CERTS
The first option to get around this issue is exporting an environment variable `NODE_EXTRA_CA_CERTS` with the file path to your certificate to get around the issue on my macOS with the following command:  
```cmd
> export NODE_EXTRA_CA_CERTS=~/Workspaces/Certs/dotnet/hospitalproject.SSC.pem
> npm run dev

> hospitalproject-client@0.0.0 dev
> vite


  VITE v7.3.1  ready in 179 ms

  ➜  Local:   https://localhost:5173/
  ➜  Local:   https://vite.dev.localhost:5173/
  ➜  Local:   https://vite.dev.internal:5173/
  ➜  Local:   https://host.docker.internal:5173/
  ➜  Local:   https://host.containers.internal:5173/
  ➜  Network: use --host to expose
  ➜  press h + enter to show help
```

This command adds the certification to the nodeJS runtime certificate store. When the page is loaded, it is secure and has loaded the weather data from the API call.  

![Page Loaded with weather data](./images/Screenshot%20from%202026-04-07%2015-20-07.png)

And then you can add the environment variable to [launch.json](../.vscode/launch.json) file or export it and add it your computers environment variable (I like adding it to the [launch.json](../.vscode/launch.json) instead of exporting to the computers environment variable). 

But I found a better option, which I will discuss next!

#### Adding agent property to proxy in ViteJS config
Since I'm using the same self-signed certificates in different formats and already bound the certificate to the ViteJS proxy, instead of adding and environment variable, I've added an `agent` object to the `proxy` object and then instantiated a new `https.Agent` object and read-in the certificate file to the `ca` attribute.

`https.Agent` is a nodeJS module, which you can use with the http-proxy-3 configuration which ViteJS uses, so now everything is in [vite.config.ts](../HospitalProject.Client/vite.config.ts), no environment variables need! This is also good since I also use my gaming PC (which is running Ubuntu) for development as well (yay for cross platform development 😃).

# Bringing it all together - Launch configurations
In my workspace, I'm going to create a [launch.json](../.vscode/launch.json) there was 2 launch configuration one for launching the ASP.NET Core web stack and the other will be for launching ViteJS/ReactJS web stack. And lastly, a compound launch configuration launching both applications at the same time. See below launch configuration for launching ASP.NET Core:  
```json
...
{
    "name": "ASP.NET Core Launch (https)",
    "type": "coreclr",
    "request": "launch",
    "preLaunchTask": "dotnet local dev build",
    "launchSettingsProfile": "https",
    "program": "${workspaceFolder}/HospitalProject.Server/bin/Debug/net10.0/HospitalProject.Server.dll",
    "args": [],
    "cwd": "${workspaceFolder}/HospitalProject.Server",
    "stopAtEntry": false,
    "console": "internalConsole",
    "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
    },
},  
...
```

For this launch configuration, I also have a `preLaunchTask`, which comes from [tasks.json](../.vscode/tasks.json). This particular task will build the ASP.NET core web application.  
```json
{
	"version": "2.0.0",
	"tasks": [
		{
			"label": "dotnet local dev build",
			"type": "dotnet",
			"task": "build ${workspaceFolder}/HospitalProject.Server/HospitalProject.Server.csproj /property:GenerateFullPaths=true /p:Configuration=Debug /p:Platform=AnyCPU /consoleloggerparameters:NoSummary",
			"file": "${workspaceFolder}/HospitalProject.Server/HospitalProject.Server.csproj",
			"group": "build",
			"problemMatcher": [],
		},
    ]
}
```

When you launch, this from the debugger, it is the same as launching from [launchSettings.json](../HospitalProject.Server/Properties/launchSettings.json) file and you are referencing the [launchSettings.json](../HospitalProject.Server/Properties/launchSettings.json) file as here too.

Next I need to create a launch configuration for ReactJS and it will need to load the page on the browser as well.  
```json
...
{
    "name": "Launch Reach App",
    "type": "node",
    "request": "launch",
    "runtimeExecutable": "npm",
    "runtimeArgs": ["run","dev"],
    "cwd": "${workspaceFolder}/HospitalProject.Client",
    "console": "integratedTerminal",
    "skipFiles": [
        "${workspaceFolder}/node_modules/**/*.js"
    ],
    "serverReadyAction": {
        "pattern": ".+Local:.+(https?:\/\/.+)",
        "uriFormat": "%s",
        "webRoot": "${workspaceFolder}/HospitalProject.Client",
        "action": "debugWithChrome"
    },
    // "env": {
    //     "NODE_EXTRA_CA_CERTS": "${userHome}/Workspaces/Certs/dotnet/hospitalproject.SSC.pem"
    // }
}, 
...
```

A few things is happening here, I'm launching ReactJS with the `npm run dev` command, `serverReadyAction` configuration looks for if viteJS is up running, once it is, the browser will open with the URL that contains *https://localhost:[portNum]*, with the debugger attached. Lastly, I added the environment variable to point to my certificate file to ensure SSL is enabled.

Now bringing it all together with a compound launch configuration.  
```json
"compounds": [
    {
        "name": "Launch Server and Client",
        "configurations": [
            "ASP.NET Core Launch (https)",
            "Launch Reach App"
        ]
    }
]
```

Once the compound launch configuration is added, you will have both the launch configuration along with the compound launch configuration appear in the Run and Debug dropdown menu.  
![Normal Launch and Compound Launch configuration are now available in Run and Debug dropdown menu](./images/Screenshot%202026-03-07%20at%202.05.56 am.png).

Now you can launch both launch configuration with the compound configuration!  
![Compound configuration launched both ASP.NET Core and ReactJS web stack](./images/Screenshot%202026-03-07%20at%202.11.36 am.png)

# References
* [Configure endpoints for the ASP.NET Core Kestrel web server | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0)
* [serilog/serilog-aspnetcore: Serilog integration for ASP.NET Core](https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file)
* [serilog/serilog-settings-configuration: A Serilog configuration provider that reads from Microsoft.Extensions.Configuration](https://github.com/serilog/serilog-settings-configuration)
* [HTTP logging in .NET and ASP.NET Core | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-10.0)
* [Server Options | Vite](https://vite.dev/config/server-options#server-https)
* [sagemathinc/http-proxy-3: Modern rewrite of node-proxy (the original nodejs http proxy server)](https://github.com/sagemathinc/http-proxy-3?tab=readme-ov-file#options)
* [Node.js — Enterprise Network Configuration](https://nodejs.org/en/learn/http/enterprise-network-configuration#adding-ca-certificates-from-the-system-store)
* [TLS (SSL) | Node.js v25.8.1 Documentation](https://nodejs.org/api/tls.html#tlssetdefaultcacertificatescerts)
* [Node.js — Node.js v23.8.0 (Current)](https://nodejs.org/en/blog/release/v23.8.0)
* [Frequently Asked Questions](https://chromium.googlesource.com/chromium/src/+/main/net/data/ssl/chrome_root_store/faq.md#how-does-the-chrome-certificate-verifier-integrate-with-platform-trust-stores-for-local-trust-decisions)
* [List of configurable options](https://code.visualstudio.com/docs/csharp/debugger-settings)
* [Browser debugging in VS Code](https://code.visualstudio.com/docs/nodejs/browser-debugging)
* [Node.js debugging in VS Code](https://code.visualstudio.com/docs/nodejs/nodejs-debugging)
* [Visual Studio Code debug configuration](https://code.visualstudio.com/docs/debugtest/debugging-configuration)
* [.net - Debugging ReactJS Components in AspNet Core - Stack Overflow](https://stackoverflow.com/questions/66012523/debugging-reactjs-components-in-aspnet-core)