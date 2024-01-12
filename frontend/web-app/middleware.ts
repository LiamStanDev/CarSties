import { withAuth } from "next-auth/middleware";

// see https://next-auth.js.org/configuration/nextjs#middleware
export const config = { matcher: ["/session"] };

export default withAuth({
  pages: {
    // Matches the pages config in `[...nextauth]`
    signIn: "/api/auth/login",
  },
});
