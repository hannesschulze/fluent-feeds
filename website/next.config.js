/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,
  basePath: '/fluent-feeds',
  assetPrefix: '/fluent-feeds/',
  experimental: {
    images: {
      unoptimized: true,
    },
  },
}

module.exports = nextConfig
