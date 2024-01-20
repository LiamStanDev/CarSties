"use client";

import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { User } from "next-auth";
import { ReactNode, useEffect, useState } from "react";
import toast from "react-hot-toast";
import { getDetailedViewData } from "../actions/auctionActions";
import AuctionCreatedToast from "../components/AuctionCreatedToast";
import AuctionFinishedToast from "../components/AuctionFinishedToast";
import { useAuctionStore } from "@/hooks/useAuctionStore";
import { useBidStore } from "@/hooks/useBidStore";
import { Auction, AuctionFinished, Bid } from "@/types";

type Props = {
  children: ReactNode;
  user: User | null;
};

const SignalRProvider = ({ children, user }: Props) => {
  const [connection, setConnection] = useState<HubConnection | null>();
  const setCurrentPrice = useAuctionStore((state) => state.setCurrentPrice);
  const addBid = useBidStore((state) => state.addBid);

  // see issue of nextjs: https://github.com/vercel/next.js/discussions/17641
  const apiUrl =
    process.env.NODE_ENV === "production"
      ? "http://api.carsties.com/notifications"
      : process.env.NEXT_PUBLIC_NOTIFY_URL;

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl(apiUrl!)
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, [apiUrl]);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          console.log("Connected to notification hub");
          connection.on("BidPlaced", (bid: Bid) => {
            // console.log("Bid placed event received");
            if (bid.bidStatus.includes("Accepted")) {
              setCurrentPrice(bid.auctionId, bid.amount);
            }
            addBid(bid);
          });

          connection.on("AuctionCreated", (auction: Auction) => {
            if (user?.username !== auction.seller) {
              return toast(<AuctionCreatedToast auction={auction} />, {
                duration: 10000,
              });
            }
          });

          connection.on(
            "AuctionFinished",
            (finishedAution: AuctionFinished) => {
              const auction = getDetailedViewData(finishedAution.auctionId);
              return toast.promise(
                auction,
                {
                  loading: "Loading",
                  success: (auction) => (
                    <AuctionFinishedToast
                      auction={auction}
                      finishedAuction={finishedAution}
                    />
                  ),
                  error: (_) => "Auction finished",
                },
                { success: { duration: 10000, icon: null } },
              );
            },
          );
        })
        .catch((error) => console.log(error));
    }

    // this callback function will called then layout componnents
    // are unmount. Note useEffect execute when layout componnents
    // are mounted.
    return () => {
      connection?.stop();
    };
  }, [connection, setCurrentPrice, addBid, user?.username]);

  return children;
};

export default SignalRProvider;
