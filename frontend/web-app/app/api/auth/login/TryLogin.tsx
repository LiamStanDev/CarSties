import { useParamsStore } from "@/hooks/useParamsStore";
import React from "react";

import { Button } from "flowbite-react";
import Heading from "@/app/components/Heading";
import { signIn } from "next-auth/react";

type Props = {
  title?: string;
  subtitle?: string;
  callbackUrl: string;
};

export const TryLogin = ({
  title = "You need to be logged in to do that",
  subtitle = "Please click below to sign in",
  callbackUrl,
}: Props) => {
  return (
    <div className="h-[40vh] flex flex-col gap-2 justify-center items-center shadow-lg">
      <Heading title={title} subtitle={subtitle} center />
      <div className="mt-4">
        <Button outline onClick={() => signIn("id-server", { callbackUrl })}>
          Login
        </Button>
      </div>
    </div>
  );
};

export default TryLogin;
