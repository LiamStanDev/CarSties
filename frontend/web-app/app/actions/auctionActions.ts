"use server";

import { Auction, PagedResult } from "@/types";
import { getTokenWorkaround } from "./authAction";
import { fetchWrapper } from "../lib/fetchWraaper";
import { FieldValues } from "react-hook-form";
import { revalidatePath } from "next/cache";

export const getData = async (query: string): Promise<PagedResult<Auction>> => {
  return await fetchWrapper.get(`search/${query}`);
};

export const updateAuctionTest = async () => {
  const data = {
    mileage: Math.floor(Math.random() * 100000) + 1,
  };

  return await fetchWrapper.put(
    "auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c",
    data,
  );
};

export const createAuction = async (data: FieldValues) => {
  return await fetchWrapper.post("auctions", data);
};

export const getDetailedViewData = async (id: string): Promise<Auction> => {
  return await fetchWrapper.get(`auctions/${id}`);
};

export const updateAuction = async (
  data: FieldValues,
  id: string,
): Promise<Auction> => {
  const res = await fetchWrapper.put(`auctions/${id}`, data);
  // the following let this paget don't use cache
  revalidatePath(`/auctions/${id}`);
  return res;
};

export const deleteAuction = async (id: string) => {
  return await fetchWrapper.del(`auctions/${id}`);
};
