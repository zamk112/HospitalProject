# Introduction
I've running a whole bunch of different CMD commands with different parameters, this document is to keep track of the most relevant and update-to-date commands.

> [!IMPORTANT]
> **Note:** All commands are run from the root workspace directory.

# ASP.NET Core
## Create Web API Project
```cmd
dotnet new webapi --use-controllers -o HospitalProject.Server
```

## Run project in HTTPS mode
```cmd
dotnet run -lp "https" --project HospitalProject.Server
```

## Generate dotnet developer certificates
```cmd
dotnet dev-certs https --trust
```

Generate PEM files with `dev-certs`:
```cmd
dotnet dev-certs https -ep ~/Workspaces/Certs/dotnet/hospitalproject.SSC.pem --format pem -np
```

# NodeJS
## Create React JS Project
```cmd
npm create vite@latest
```

## Run project
```cmd
npm run dev
```

# OpenSSL
## Querying Cipher Suites
```cmd
> openssl ciphers -V 'HIGH:!aNULL:!MD5:!kRSA:!kDHE:!DSS:!PSK:!SRP:!ARIA:!CAMELLIA:!AESCCM' \
  | grep -E "TLSv1\.[0-9]"   
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
## Certificate Authority Certificate
### Generate AES256 encryption CA Key
```cmd
openssl genrsa -aes256 -out ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key 2048
```

### Generate CA PEM Certificate
```cmd
openssl req -x509 -new -nodes \
  -key ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key \
  -sha256 -days 365 \
  -config ~/Projects/HospitalProject/HospitalProject.ca.conf \
  -out ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
```

> [!TIP]
> Enter your password.

## Adding CA certificate to trust stores
### MacOS
```cmd
sudo security add-trusted-cert -d -r trustRoot \
  -k /Library/Keychains/System.keychain \
  ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
```

> [!TIP]
> Enter your password.

### Linux (Chromium based browsers) NSSDB
Add command:
```cmd
certutil -A \
  -d ~/.pki/nssdb \
  -n "HospitalProjectCA" \
  -t "CT,," \
  -i ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
```

View command:
```cmd
certutil -L -d ~/.pki/nssdb
```

Delete command:
```cmd
certutil -D -d ~/.pki/nssdb -n "HospitalProjectCA"
```

### Inspect on CA certificate
```cmd
openssl x509 -in ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem -noout -text | grep -A 3 '(X509v3|Serial Number|Issuer)'
```

> [!NOTE]
> Verifying x509 extension, Serial Number & Issuer

## Server Certificate
### Generate private key and CSR
```cmd
openssl req -new \
  -newkey rsa:2048 \
  -nodes \
  -keyout ~/Workspaces/Certs/hospital.project/hospital.project.server.key \
  -config ~/Projects/HospitalProject/HospitalProject.Server/hospitalproject.server.cert.conf \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.server.csr
```

### Generate Certificate File
```cmd
openssl x509 -req \
  -in ~/Workspaces/Certs/hospital.project/hospital.project.server.csr \
  -CA ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  -CAkey ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.key \
  -CAcreateserial \
  -days 365 \
  -sha256 \
  -extfile ~/Projects/HospitalProject/HospitalProject.Server/hospitalproject.server.cert.conf \
  -extensions v3_sign \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.server.pem
```

> [!TIP]
> Enter your password.

### Generate PFX Certificate
```cmd
openssl pkcs12 -export \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx \
  -inkey ~/Workspaces/Certs/hospital.project/hospital.project.server.key \
  -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pem \
  -certfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem
```

> [!TIP]
> Enter your password.

### Verify PEM file
```cmd
> openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  ~/Workspaces/Certs/hospital.project/hospital.project.server.pem
```

### Verify PFX Certificate
```cmd
> openssl pkcs12 -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx \
  -clcerts -nokeys -passin pass:*********** | \
```

> [!TIP]
> Enter your password.

### Viewing PEM File
```cmd
> openssl x509 -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pem -noout -text | grep -E -A 3 '(X509v3|Serial Number|Issuer)'
```

> [!NOTE]
> Verifying x509 extension, Serial Number & Issuer

### Viewing PFX Certificate
```cmd
> openssl pkcs12 -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx \
  -passin pass:******** -clcerts -nokeys | \
  openssl x509 -noout -text | \
  grep -E -A 3 '(X509v3|Serial Number|Issuer)'
```

> [!TIP]
> Enter your password.

> [!NOTE]
> Verifying x509 extension, Serial Number & Issuer

### Show Certificate Chain
PEM File:  
```cmd
> openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  -show_chain \
  ~/Workspaces/Certs/hospital.project/hospital.project.server.pem
```

PFX File:  
```cmd
> openssl pkcs12 -in ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx \
  -nokeys -passin pass:****** | grep "subject="
```

> [!TIP]
> Enter your password.

#### Using docker image 
Showing certificate chain:
```cmd
docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -alpn h2,http/1.1 \
  -showcerts
```

Testing cipher suites:
```
docker run --rm \
  --network=hospital-network \
  -v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/ca.pem:ro \
  alpine/openssl s_client \
  -connect hospitalproject.api.local:5229 \
  -CAfile /ca.pem \
  -tls1_2 \
  -alpn h2,http/1.1 \
  -cipher ECDHE-ECDSA-AES128-GCM-SHA256
```

## Client Certificate
### Generate private key and CSR
```cmd
> openssl req -new \
  -newkey rsa:2048 \
  -nodes \
  -keyout ~/Workspaces/Certs/hospital.project/hospital.project.client.key \
  -config ~/Projects/HospitalProject/HospitalProject.Client/hospitalproject.client.cert.conf \
  -out ~/Workspaces/Certs/hospital.project/hospital.project.client.csr
```

### Generate Certificate File
```cmd
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
```

### Verifying Certificate File
```cmd
> openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  ~/Workspaces/Certs/hospital.project/hospital.project.client.pem
```

### Viewing Certificate File
```cmd
> openssl x509 -in ~/Workspaces/Certs/hospital.project/hospital.project.client.pem -noout -text | grep -E -A 3 '(X509v3|Serial Number|Issuer)'
```

> [!NOTE]
> Verifying x509 extension, Serial Number & Issuer

### Show Certificate Chain
```cmd
> openssl verify -CAfile ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem \
  -show_chain \
  ~/Workspaces/Certs/hospital.project/hospital.project.client.pem
```

# Docker
## Build Commands
Build server image:
```cmd
docker build -t hospital.project.server -f HospitalProject.Server/Dockerfile --pull HospitalProject.Server/
```

Build client image:
```cmd
docker build -t hospital.project.client -f HospitalProject.Client/Dockerfile --pull HospitalProject.Client/
```

## Create Commands
Create Network adapter:
```cmd
docker network create \
  --driver bridge \
  --subnet 10.0.0.0/28 \
  --gateway 10.0.0.1 \
  hospital-network
```

Create Client Docker Container:
```cmd
> docker run -d --network hospital-network --ip 10.0.0.2 --network-alias hospitalproject.local -p 443:443 -p 80:80 \
-v ~/Projects/HospitalProject/HospitalProject.Client/staging.nginx.conf:/etc/nginx/nginx.conf:ro \
-v ~/Workspaces/Certs/hospital.project/hospital.project.client.key:/etc/nginx/certs/hospitalproject.client.key:ro \
-v ~/Workspaces/Certs/hospital.project/hospital.project.client.pem:/etc/nginx/certs/hospitalproject.client.pem:ro \
-v ~/Workspaces/Certs/hospital.project/ca/HospitalProject.CA.pem:/etc/nginx/certs/HospitalProject.CA.pem:ro \
hospital.project.client
```

Create Server Docker Container:
```cmd
> docker run -d --network hospital-network --ip 10.0.0.3 --network-alias hospitalproject.api.local \
-e ASPNETCORE_URLS="https://+:5229" \
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/hospitalproject.server.pfx \
-e ASPNETCORE_Kestrel__Certificates__Default__Password="************" \
-e ASPNETCORE_ENVIRONMENT=Staging \
-v ~/Workspaces/Certs/hospital.project/hospital.project.server.pfx:/https/hospitalproject.server.pfx:ro \
-v ~/Projects/HospitalProject/HospitalProject.Server/appsettings.Staging.json:/app/appsettings.Staging.json:ro \
hospital.project.server
```

> [!TIP]
> Enter your password.

## NGINX Related Commands
Query for master and worker processes:
```cmd
docker exec __CONTAINER_NAME__ ps | grep -E "nginx: \w+ process" 
```

> [!TIP]
> Replace \_\_CONTAINER_NAME\_\_ with container name

Query max open file sizes:
```cmd
> docker exec __CONTAINER_NAME__ sh -c 'pgrep -P 1 -f "nginx: worker" | while read pid; do echo "PID $pid: $(grep "open files" /proc/$pid/limits)"; done'
```

> [!TIP]
> Replace \_\_CONTAINER_NAME\_\_ with container name

Query file descriptor system wide limit:
```cmd
> docker exec __CONTAINER_NAME__ sysctl fs.file-max
```

> [!TIP]
> Replace \_\_CONTAINER_NAME\_\_ with container name