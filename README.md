# Background
This project has a bit of a backstory. At a previous job, two weeks before go-live of a major application and infrastructure stack upgrade, I was handed a broken AngularJS and ASP.NET Core project. I managed to get it across the line, but it still had issues — so I ended up rewriting the whole thing using ReactJS with SSR and an ASP.NET Core backend. There's a good reason I went with SSR, but I'll save that for a job interview 😉.

I was very close to completion, but after several months of long weeks juggling support work alongside the development, I ended up in hospital a week before my contract ended. This project is my way of coming back to it properly — building it the right way, at my own pace, as a reference template for anyone who needs to build a similar stack.

# What This Project Is
A Hospital CRM for managing patient data — because given the circumstances, why not 😄.
More importantly, it's a full-stack template for building and hosting a ReactJS frontend with an ASP.NET Core WebAPI backend on NGINX and Docker, with a strong focus on doing things properly.

# Goals
* HTTPS for both frontend and backend endpoints
* OIDC with Cookie-based SSO integration via Auth0
* ACL with claims and roles-based authorisation
* Reverse proxy configuration in NGINX to keep the backend API unexposed
* SQL Server running on Docker alongside the application stack
* Global logging
* Security-first approach throughout

# Development Environment
I'm developing locally on a MacBook Air M1 and an Ubuntu gaming PC, using VS Code as my editor on both. Docker with an NGINX image serves as my production-like or staging environment. This setup came about practically — but it works well (so far).

*This is a work in progress. Feedback and suggestions are welcome — I'm not perfect and happy to be corrected 😅*
