import NextAuth, { DefaultUser } from "next-auth";
import { JWT } from "next-auth/jwt";

// See https://next-auth.js.org/getting-started/typescript
// in Module augumentation section.
declare module "next-auth" {
  /**
   * Returned by `useSession`, `getSession` and received as a prop on the `SessionProvider` React Context
   */
  interface Session {
    user: {
      /** The user's postal address. */
    } & DefaultSession["user"];
  }

  interface Profile {
    username: string;
  }

  interface User {
    username: string;
  }
}

// See https://next-auth.js.org/getting-started/typescript
// in Submodules section.
declare module "next-auth/jwt" {
  /** Returned by the `jwt` callback and `getToken`, when using JWT sessions */
  interface JWT {
    username: string;
    assess_token?: string;
  }
}
