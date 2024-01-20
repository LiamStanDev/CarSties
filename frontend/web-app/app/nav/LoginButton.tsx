"use client";

import { Button } from "flowbite-react";
import { signIn } from "next-auth/react";

const LoginButton = () => {
  return (
    <Button
      onClick={() =>
        // prompt login means always need to login event, the session
        // is still alive in browser
        signIn("id-server", { callbackUrl: "/" }, { prompt: "login" })
      }
    >
      Login
    </Button>
  );
};

export default LoginButton;
