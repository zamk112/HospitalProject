import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'node:path';
import { readFileSync,  existsSync } from 'node:fs';
import os from "node:os";
import { env } from "node:process";
import https from "https";


const certName = 'hospitalproject.client';
const certFolder = path.join(os.homedir(), 'Workspaces', 'Certs', 'dotnet');
const certPath = path.join(certFolder, `${certName}.pem`);
const keyPath = path.join(certFolder, `${certName}.key`);

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` : 'https://localhost:7083';

if (!existsSync(certPath) || !existsSync(keyPath)) {
  throw new Error('Certificate not found.');
}

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


