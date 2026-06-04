# VPS Deployment

This repo deploys with two independent GitHub Actions workflows:

- `.github/workflows/deploy-backend.yml`
- `.github/workflows/deploy-frontend.yml`

## Required GitHub Secrets

Create these in GitHub repository settings under `Secrets and variables`:

- `VPS_HOST`: VPS IP address or domain.
- `VPS_USER`: SSH user, for example `root`.
- `VPS_SSH_KEY`: private SSH key used for deployment.
- `VPS_SSH_PASSPHRASE`: SSH key passphrase, if the key has one.
- `VPS_SSH_PORT`: SSH port, if your server does not use port `22`.
- `MSSQL_SA_PASSWORD`: strong SQL Server SA password.
- `JWT_SECRET`: long random JWT signing secret.

Optional secret:

- `FIFA_DB_CONNECTION_STRING`: existing SQL Server connection string. If this is not set, the backend compose file uses the SQL Server container.
- `LETSENCRYPT_EMAIL`: email address used when issuing the HTTPS certificate. If this is not set, Certbot registers without an email.

## Optional GitHub Variables

- `API_BASE_URL`: public prediction API URL used by Angular. For production HTTPS, use `https://api.appzihub.fun`.
- `AUTH_BASE_URL`: public identity API URL used by Angular. For production HTTPS, use `https://auth.appzihub.fun`.
- `TLS_EXTRA_DOMAINS`: optional comma-separated domains to add to the Let's Encrypt certificate, for example `app.appzihub.fun`.

If the optional variables are not set, the frontend workflow uses:

- `https://api.appzihub.fun`
- `https://auth.appzihub.fun`

## Server Paths

The workflows copy compose files and create `.env` files on the VPS:

- `/opt/fifa/backend`
- `/opt/fifa/frontend`

The backend workflow starts SQL Server, the prediction API, and the identity API.
The frontend workflow starts nginx on ports `80` and `443`.

## HTTPS

The production nginx container terminates HTTPS with a Let's Encrypt certificate
stored on the VPS under `/opt/fifa/certbot/conf`. The frontend deploy workflow
does this automatically:

1. Stops the current frontend container so Certbot can temporarily bind port `80`.
2. Issues the initial certificate for `fifa.appzihub.fun`, `api.appzihub.fun`,
   and `auth.appzihub.fun`, or renews the existing certificate.
3. Starts nginx again with port `80` redirecting to `443`.

Before deploying HTTPS, make sure DNS `A` records for these domains point to the
VPS IP address:

- `fifa.appzihub.fun`
- `api.appzihub.fun`
- `auth.appzihub.fun`

If you also want `https://app.appzihub.fun`, add its DNS record and set the
GitHub variable `TLS_EXTRA_DOMAINS` to `app.appzihub.fun` before deploying the
frontend workflow.

Make sure the VPS firewall and Hostinger firewall allow inbound traffic for:

- `80` for HTTP redirects and Let's Encrypt validation.
- `443` for HTTPS.

Certificates expire after 90 days. The frontend workflow renews the certificate
when it deploys; if deployments are infrequent, run the frontend workflow at
least once every two months or add a server cron job that runs Certbot renew and
restarts the frontend container.

## Nginx Deployment

Nginx is deployed by the frontend GitHub Actions workflow as a Docker container.
The workflow builds `Frontend/Boss`, copies `deploy/nginx/frontend.conf` into
the image, pushes the image to GitHub Container Registry, and restarts it on the
VPS with `deploy/docker-compose.frontend.prod.yml`.

After a successful frontend deployment, the site should be available at:

- `https://fifa.appzihub.fun`
- `https://api.appzihub.fun`
- `https://auth.appzihub.fun`

The same nginx container also routes the public backend domains over the shared
Docker network `fifa-public`:

- `https://api.appzihub.fun` -> prediction API container `fifa-api:8080`
- `https://auth.appzihub.fun` -> identity API container `fifa-identity-api:8080`
- `https://fifa.appzihub.fun` -> Angular frontend

If you want `app.appzihub.fun` specifically, add an `A` record for `app` pointing
to the VPS IP address and include it in `TLS_EXTRA_DOMAINS`.

Deploy the backend workflow before the frontend workflow so the backend
containers are available on the shared network.

## First Deploy Notes

The workflows install Docker on the VPS if it is missing. Make sure the VPS firewall and Hostinger firewall allow inbound traffic for:

- `80` for HTTP redirects and Let's Encrypt validation.
- `443` for HTTPS.

Do not commit passwords, private keys, or passphrases into this repository.
