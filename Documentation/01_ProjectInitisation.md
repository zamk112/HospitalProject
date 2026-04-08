
# Introduction
The first thing I want to do is to do a scaffold install of ASP.NET Core Web API and ReactJS (Typescript) version.

# ASP.NET Core Scaffold 
I'm going to start off with creating the ASP.NET Core Web API solution. So first thing is to make sure that you have the dotnet SDK installed. As of writing this documentation; I am using dotnet core 10.0. And you will also need to install certificates on your machine as well with this command:  
```cmd
    dotnet dev-certs https --trust
```

I've already done it before starting this tutorial so I don't have an output to show you at the moment. Once you have it installed, run the following the command:  
```cmd
dotnet new webapi --use-controllers -o HospitalProject.Server 
```

You're output should look like this:  
```cmd
> dotnet new webapi --use-controllers -o HospitalProject.Server
The template "ASP.NET Core Web API" was created successfully.

Processing post-creation actions...
Restoring /Users/zamk/Projects/HospitalProject/HospitalProject.Server/HospitalProject.Server.csproj:
Restore succeeded.
```

This will create an ASP.NET Core controller based web API project with the Weather Forecast endpoint. Next, I'm going to run the project to make sure everything is working with the following command:  
```cmd
dotnet run -lp "https" --project HospitalProject.Server
```

It should build successfully, and launch as well and you should see this in the output:  
```cmd
> dotnet run -lp "https" --project HospitalProject.Server
Using launch settings from HospitalProject.Server/Properties/launchSettings.json...
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7083
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5229
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/zamk/Projects/HospitalProject/HospitalProject.Server
```

Now I'm going to test the `weatherforecast` endpoint on the browser and looks like HTTPs version is working.  
![HTTPS Endpoint working and secure!](./images/Screenshot%202026-03-06%20at%207.41.42 pm.png)

Not too worried about the HTTP protocol because both the front and backends will just be HTTPS and will be using the HTTPS protocol moving forward. 

With ASP.NET Core version 8, you had Swagger UI for testing and API documentation, but in ASP.NET Core version 10 you have open API, which returns a JSON document.  
![Open API Documentation](./images/Screenshot%202026-03-06%20at%207.47.05 pm.png)

Nice so we get a JSON formatted documentation of the API endpoints.

# ReactJS Scaffold Install
For installing ReactJS scaffold, I'm going to use Vite. Make sure [nodeJS](https://nodejs.org/en) is also installed on your machine before you start running this command:  
```cmd
npm create vite@latest
```

This is the output from my console:  
```cmd
> npm create vite@latest

> npx
> "create-vite"

│
◇  Project name:
│  HospitalProject.Client
│
◇  Package name:
│  hospitalproject-client
│
◇  Select a framework:
│  React
│
◇  Select a variant:
│  TypeScript
│
◇  Install with npm and start now?
│  Yes
│
◇  Scaffolding project in /home/zamk/Projects/HospitalProject/HospitalProject.Client...
│
◇  Installing dependencies with npm...

added 172 packages, and audited 173 packages in 4s

49 packages are looking for funding
  run `npm fund` for details

found 0 vulnerabilities
│
◇  Starting dev server...

> hospitalproject-client@0.0.0 dev
> vite


  VITE v8.0.5  ready in 192 ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
  ➜  press h + enter to show help


```

Because I also started the server, it is running with HTTP. But now, as a part of my checking, I can see the website is working.  
![Launched ReactJS Website](./images/Screenshot%20from%202026-04-07%2014-17-57.png)

# Conclusion
So far everything is working, but we need to make some changes. Both frontend and backends needs to be running with HTTPS protocol not HTTP and some cleanup is required in the project is also needed as well. But will this in the next documentation.

# References
* [Tutorial: Create a controller-based web API with ASP.NET Core | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-10.0&tabs=visual-studio-code)
* [Generate OpenAPI documents | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0&tabs=net-cli%2Cvisual-studio-code)
* [Getting Started | Vite](https://vite.dev/guide/)