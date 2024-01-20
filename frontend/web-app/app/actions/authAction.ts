import { User, getServerSession } from "next-auth";
import { getToken } from "next-auth/jwt";
import { headers, cookies } from "next/headers"; // this should be correct
import { NextApiRequest } from "next";
import { authOptions } from "../api/auth/[...nextauth]/routeOptios";

export const getSession = async () => {
  return await getServerSession(authOptions);
};

export const getCurrentUser = async (): Promise<User | null> => {
  try {
    const session = await getSession();

    if (!session) return null;

    return session.user;
  } catch (error) {
    return null;
  }
};

// because the getToken function is not work in
// now, so we need to create
export const getTokenWorkaround = async () => {
  // create our own request object, which is like
  // Request Context in asp.net.
  const req = {
    headers: Object.fromEntries(headers()),
    cookies: Object.fromEntries(
      cookies()
        .getAll()
        .map((c) => [c.name, c.value]),
    ),
  } as NextApiRequest;

  return await getToken({ req });
};
