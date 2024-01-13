"use client";

import { Dropdown, DropdownDivider } from "flowbite-react";
import { User } from "next-auth";
import Link from "next/link";
import { HiUser, HiCog } from "react-icons/hi";
import { AiFillCar, AiFillTrophy, AiOutlineLogout } from "react-icons/ai";
import { signOut } from "next-auth/react";
import { usePathname, useRouter } from "next/navigation";
import { useParamsStore } from "@/hooks/useParamsStore";

type Props = {
  user: User;
};

const UserAction = ({ user }: Props) => {
  const router = useRouter();
  const pathname = usePathname();

  const setParams = useParamsStore((state) => state.setParams);

  const setWinner = () => {
    setParams({ winner: user.username, seller: undefined });
    if (pathname != "/") {
      router.push("/");
    }
  };

  const setSeller = () => {
    setParams({ seller: user.username, winner: undefined });
    if (pathname != "/") {
      router.push("/");
    }
  };

  return (
    <Dropdown inline label={`Welcome ${user.name}`}>
      <Dropdown.Item icon={HiUser} onClick={setSeller}>
        My Auctions
      </Dropdown.Item>
      <Dropdown.Item icon={AiFillTrophy} onClick={setWinner}>
        Auctions won
      </Dropdown.Item>
      <Dropdown.Item icon={AiFillCar}>
        <Link href="/auctions/create">Sell my car</Link>
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
