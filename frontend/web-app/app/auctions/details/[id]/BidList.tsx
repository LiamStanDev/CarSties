"use client";

import { getBidsForAuction } from "@/app/actions/auctionActions";
import Heading from "@/app/components/Heading";
import { fetchWrapper } from "@/app/lib/fetchWraaper";
import { useBidStore } from "@/hooks/useBidStore";
import { Auction } from "@/types";
import { User } from "next-auth";
import { useEffect, useState } from "react";
import toast from "react-hot-toast";
import BidItem from "./BidItem";

type Props = {
  auction: Auction;
  user: User | null;
};

const BidList = ({ auction, user }: Props) => {
  const [loading, setLoading] = useState(true);
  const bids = useBidStore((state) => state.bids);
  const setBid = useBidStore((state) => state.setBid);

  useEffect(() => {
    getBidsForAuction(auction.id)
      .then((res) => setBid(res))
      .catch((error: any) => toast.error(error.message))
      .finally(() => setLoading(false));
  }, [auction.id, setLoading, setBid]);

  if (loading) return <span>Loading bids...</span>;

  return (
    <div className="border-2 rounded-lg p-2 bg-gray-100">
      <Heading title="Bids" />
      {bids.map((bid) => (
        <BidItem key={bid.id} bid={bid} />
      ))}
    </div>
  );
};

export default BidList;
