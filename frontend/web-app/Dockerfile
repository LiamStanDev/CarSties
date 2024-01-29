FROM node:alpine AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app

COPY frontend/web-app/package*.json ./
RUN npm install -g pnpm
# NODE_ENV production let pnpm install with dev dependency
ENV NODE_ENV production
RUN pnpm install

FROM node:alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY frontend/web-app ./
# 取消遙測, 可以增加性能
ENV NEXT_TELEMETRY_DISABLED 1
RUN npm install -g pnpm
RUN pnpm build

# production image, copy all files and run next
FROM node:alpine AS runner
WORKDIR /app

ENV NODE_ENV production
ENV NEXT_TELEMETRY_DISABLED 1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public
COPY --from=builder /app/package.json ./package.json
COPY --from=builder /app/next.config.js ./next.config.js

COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs

EXPOSE 3000

ENV PORT 3000

CMD ["node", "server.js"]