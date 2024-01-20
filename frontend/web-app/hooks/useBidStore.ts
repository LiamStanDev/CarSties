import { Bid } from "@/types";
import { create } from "zustand";

type State = {
  bids: Bid[];
};

type Action = {
  setBid: (bids: Bid[]) => void;
  addBid: (bid: Bid) => void;
};

export const useBidStore = create<State & Action>((set) => ({
  bids: [],

  setBid: (bids: Bid[]) => {
    set((state) => ({ bids }));
  },
  addBid: (bid: Bid) => {
    set((state) => ({
      bids: !state.bids.find((x) => x.id === bid.id)
        ? [bid, ...state.bids]
        : [...state.bids],
    }));
  },
}));
