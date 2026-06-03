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

## Optional GitHub Variables

- `API_BASE_URL`: public prediction API URL used by Angular, for example `http://<your-vps-host>:6001`.
- `AUTH_BASE_URL`: public identity API URL used by Angular, for example `http://<your-vps-host>:5001`.

If the optional variables are not set, the frontend workflow uses:

- `http://<VPS_HOST>:6001`
- `http://<VPS_HOST>:5001`

## Server Paths

The workflows copy compose files and create `.env` files on the VPS:

- `/opt/fifa/backend`
- `/opt/fifa/frontend`

The backend workflow starts SQL Server, the prediction API, and the identity API.
The frontend workflow starts nginx on port `80`.

## Nginx Deployment

Nginx is deployed by the frontend GitHub Actions workflow as a Docker container.
The workflow builds `Frontend/Boss`, copies `deploy/nginx/frontend.conf` into
the image, pushes the image to GitHub Container Registry, and restarts it on the
VPS with `deploy/docker-compose.frontend.prod.yml`.

After a successful frontend deployment, the site should be available at:

- `http://<VPS_HOST>`
- `http://app.appzihub.fun`

The same nginx container also routes the public backend domains over the shared
Docker network `fifa-public`:

- `http://api.appzihub.fun` -> prediction API container `fifa-api:8080`
- `http://auth.appzihub.fun` -> identity API container `fifa-identity-api:8080`
- `http://app.appzihub.fun` -> Angular frontend
- `http://fifa.appzihub.fun` -> Angular frontend, supported as an alias for the current DNS record

If you want `app.appzihub.fun` specifically, add an `A` record for `app` pointing
to the VPS IP address.

Deploy the backend workflow before the frontend workflow so the backend
containers are available on the shared network.

## First Deploy Notes

The workflows install Docker on the VPS if it is missing. Make sure the VPS firewall and Hostinger firewall allow inbound traffic for:

- `80` for the frontend.
- `5001` for the identity API.
- `6001` for the prediction API.

Do not commit passwords, private keys, or passphrases into this repository.
