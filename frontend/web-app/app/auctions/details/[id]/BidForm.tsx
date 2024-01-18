"use client";

import { placeBidForAuction } from "@/app/actions/auctionActions";
import { numberWithCommas } from "@/app/lib/numberWithComma";
import { useBidStore } from "@/hooks/useBidStore";
import { FieldValues, useForm } from "react-hook-form";
import toast from "react-hot-toast";

type Props = {
  auctionId: string;
  highBid: number;
};

const BidForm = ({ auctionId, highBid }: Props) => {
  const { register, handleSubmit, reset } = useForm();
  const addBid = useBidStore((state) => state.addBid);

  const onSubmit = (data: FieldValues) => {
    if (parseInt(data.amount) <= highBid) {
      reset();
      return toast.error(
        "Bid must be at least $" + numberWithCommas(highBid + 1),
      );
    }
    placeBidForAuction(auctionId, parseInt(data.amount))
      .then((bid) => {
        // this is from fetchWrapper's handleResponse
        if (bid.error) throw bid.error;
        // NOTE: this can remove because it will be notify by signalR
        // but when signalR server down, this will cause problem, so
        // we still live it here. Note addBid method gonna check the
        // bid to avoid same bid place.
        addBid(bid);
        reset();
      })
      .catch((err) => toast.error(err.message));
  };
  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="flex items-center border-2 
      rounded-lg py-2 
      "
    >
      <input
        type="number"
        {...register("amount")}
        className="input-custom text-sm text-gray-600"
        placeholder={`Enter your bid (minimum bid is $${numberWithCommas(
          highBid + 1,
        )})`}
      />
    </form>
  );
};

export default BidForm;
