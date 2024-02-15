/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "cdn.pixabay.com",
        port: "",
        pathname: "/photo/**",
      },
    ],
  },
  // need to add this to see the log
  logging: {
    fetches: {
      fullUrl: false,
    },
  },
  // make file more smaller
  output: "standalone",
};

module.exports = nextConfig;
