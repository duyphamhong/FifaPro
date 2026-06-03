FROM node:16-alpine AS build
WORKDIR /app

COPY Frontend/Boss/package*.json ./
RUN npm ci

COPY Frontend/Boss/angular.json Frontend/Boss/tsconfig*.json ./
COPY Frontend/Boss/src ./src

ARG API_BASE_URL
ARG AUTH_BASE_URL
RUN node -e "const fs=require('fs'); const [api, auth]=process.argv.slice(1); fs.writeFileSync('src/environments/environment.prod.ts', 'export const environment = {\n  production: true,\n  baseApiUrl: \"' + api + '\",\n  baseAuthenUrl: \"' + auth + '\"\n};\n');" "$API_BASE_URL" "$AUTH_BASE_URL"
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY deploy/nginx/frontend.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/boss /usr/share/nginx/html
EXPOSE 80
