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

## First Deploy Notes

The workflows install Docker on the VPS if it is missing. Make sure the VPS firewall and Hostinger firewall allow inbound traffic for:

- `80` for the frontend.
- `5001` for the identity API.
- `6001` for the prediction API.

Do not commit passwords, private keys, or passphrases into this repository.
