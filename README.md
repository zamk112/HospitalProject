# Motivation
Just want to give you a background for creating this project. At one of my previous jobs, the following happened:  
* 2 Weeks before go live of a massive application and infrastructure stack upgrade, I got handed a broken AngularJS and ASP.NET Core project.
* Although I managed to make it to go-live it still had a lot of problems, but not going to mention what they were.
* Because of those problems I had to rewrite the application again but I did with ReactJS with SSR and ASP.NET Core backend to make it work (there's a reason why I chose SSR, let's just say it was to make it work with their infrastructure).
* I won't mention the details unless it's a job interview 😉.
* I was close to finishing it but because working 60-80 hour weeks for almost five months (doing my support work along with web development), I ended up in the hospital with heart problems and kidney damage ☠️.

Due to the short amount of time I had and because I ended up in hospital, there's a lot of things that I could have done better and things I didn't get to finish. This project is for a next time (maybe) if I am in an situation where I need to develop an application with ReactJS and ASP.NET Core WebAPI and host it on NGINX and also going to host on Docker. So in this project I'll be mostly be focusing on:
* Security,
* Authentication and Authorisation,
* Setting up for local development, testing,
* Deployment of both the frontend and backend on a NGINX.

Initially, I was writing my documentation for building the web application stack and hosting on IIS, but I no longer have a machine that runs Windows on an X64 architecture and things started to break on Windows 11 hosted on my Parallels VM (Can't afford a new Windows 11 PC/Laptop or a MacBook Pro because I am poor and jobless at the moment 😅). So now I'm changing gears and just using my MacBook Air M1 OS for local development and Docker with an NGINX image will be like my production-like environment. 

This is going to be like a template for doing this type of development and also this is somewhat of new learning for me since I am working with NGINX and working more with Docker (usually use docker to host my Database Servers). And I'm hoping this will help you out if you get stuck as well. And let me know if I made any mistakes too (not perfect you know 😅).

# Introduction
I want to focus on the things I want to do with this project:  
* HTTPS frontend and backend endpoints,
* ODIC with Cookie integration (SSO login, going to [Auth0](https://auth0.com) for this),
* Hosting my web application stack on Docker along with SQL Server running on Docker,
* Implementing a reverse proxy on NGINX, so the backend API does not get exposed and along with some other configuration and optimisation,
* ACL and Frontend handling and integration,
* ACL with utilizing claims and roles,
* Global Logging.

My choice of editor is VS code because that's what I have been using for ReactJS and ASP.NET Core Development for a while now (even on Windows!). And Lastly, the type of application that I want to build is a Hospital CRM for managing patient data (since I was in Hospital, because why not?!).

# Table of Contents
* [Initial Commit](./Documentation/00_InitialCommit.md)
* [Setting Up For Local Development](./Documentation/01_SetupForLocalDevelopment.md)
* [Setting Up Nginx and Docker](./Documentation/02_SettingUpNginxAndDocker.md)