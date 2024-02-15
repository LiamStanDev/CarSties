"use client";

import { useParamsStore } from "@/app/hooks/useParamsStore";
import { usePathname, useRouter } from "next/navigation";
import { AiOutlineCar } from "react-icons/ai";

export default function Logo() {
  const router = useRouter();
  const pathname = usePathname(); // get current pathname
  const reset = useParamsStore((state) => state.reset);

  const doReset = () => {
    if (pathname !== "/") {
      router.push("/");
    }
    reset();
  };

  return (
    <div
      onClick={doReset}
      className="cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-500"
    >
      <AiOutlineCar size={34} />
      <div>CarSties Auctions</div>
    </div>
  );
}
