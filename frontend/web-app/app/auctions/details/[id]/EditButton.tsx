"use client";

import { Button } from "flowbite-react";
import Link from "next/link";

type Props = {
  id: string;
};

const EditButton = ({ id }: Props) => {
  return (
    <Button>
      <Link href={`/auctions/update/${id}`}>Update Auction</Link>
    </Button>
  );
};

export default EditButton;
