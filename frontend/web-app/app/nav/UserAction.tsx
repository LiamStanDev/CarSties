"use client";

import { Button } from "flowbite-react";
import Link from "next/link";

const UserAction = () => {
  return (
    <Button outline>
      <Link href="/session">Session</Link>
    </Button>
  );
};

export default UserAction;
