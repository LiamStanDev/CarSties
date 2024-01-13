import NextAuth, { NextAuthOptions } from "next-auth";
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6";

export const authOptions: NextAuthOptions = {
  session: {
    strategy: "jwt",
  },
  providers: [
    DuendeIDS6Provider({
      // this need to match Config.cs RedirectUris
      id: "id-server",
      clientId: "nextApp",
      clientSecret: "secret",
      issuer: "http://localhost:5000",
      authorization: {
        params: {
          scope: "openid profile auctionApp",
        },
      },
      idToken: true, // because Config.cs AlwaysIncludeUserClaimsInIdToken
    }),
  ],

  callbacks: {
    async jwt({ token, profile, account }) {
      console.log({ token, profile /*, account, user*/ });

      if (profile) {
        token.username = profile.username;
      }

      if (account) {
        token.assess_token = account.access_token;
      }

      return token;
    },

    async session({ session, token }) {
      if (token) {
        session.user.username = token.username;
      }

      return session;
    },
  },
};

const handler = NextAuth(authOptions);

export { handler as GET, handler as POST };
