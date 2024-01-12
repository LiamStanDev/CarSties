import { User, getServerSession } from "next-auth";
import { authOptions } from "../api/auth/[...nextauth]/route";

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
