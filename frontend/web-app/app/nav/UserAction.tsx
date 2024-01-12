"use client";

import { Dropdown, DropdownDivider } from "flowbite-react";
import { User } from "next-auth";
import Link from "next/link";
import { HiUser, HiCog } from "react-icons/hi";
import { AiFillCar, AiFillTrophy, AiOutlineLogout } from "react-icons/ai";
import { signOut } from "next-auth/react";

type Props = {
  user: Partial<User>;
};

const UserAction = ({ user }: Props) => {
  return (
    <Dropdown inline label={`Welcome ${user.name}`}>
      <Dropdown.Item icon={HiUser}>
        <Link href="/">My Auctions</Link>
      </Dropdown.Item>
      <Dropdown.Item icon={AiFillTrophy}>
        <Link href="/">Auctions won</Link>
      </Dropdown.Item>
      <Dropdown.Item icon={AiFillCar}>
        <Link href="/">Sell my car</Link>
      </Dropdown.Item>
      <Dropdown.Item icon={HiCog}>
        <Link href="/session">Session (dev only)</Link>
      </Dropdown.Item>
      <DropdownDivider />
      <Dropdown.Item
        icon={AiOutlineLogout}
        onClick={() => signOut({ callbackUrl: "/" })}
      >
        Sign out
      </Dropdown.Item>
    </Dropdown>
  );
};

export default UserAction;
