"use client";

import { Button, TextInput } from "flowbite-react";
import { FieldValues, useForm } from "react-hook-form";
import Input from "../components/Input";
import { useEffect } from "react";
import DateInput from "../components/DateInput";
import { createAuction, updateAuction } from "../actions/auctionActions";
import { usePathname, useRouter } from "next/navigation";
import toast from "react-hot-toast";
import { Auction } from "@/types";

type Props = {
  auction?: Auction;
};

const AuctionForm = ({ auction }: Props) => {
  const router = useRouter();
  const pathname = usePathname();

  const {
    control,
    handleSubmit,
    setFocus,
    formState: { isSubmitting, isValid },
    reset,
  } = useForm({ mode: "onTouched" });

  const onSubmit = async (data: FieldValues) => {
    try {
      let id = "";
      let res;
      if (pathname === "auctions/create") {
        res = await createAuction(data);
        id = res.id;
      } else {
        if (auction) {
          res = await updateAuction(data, auction.id);
          id = auction.id;
        }
      }

      if (res.error) {
        throw res.error;
      }
      router.push(`/auctions/details/${id}`);
    } catch (error: any) {
      toast.error(error.status + " " + error.message);
      console.log(error);
    }
  };

  useEffect(() => {
    if (auction) {
      const { make, model, color, mileage, year } = auction;
      reset({ make, model, color, mileage, year });
    }
    setFocus("make"); // set the focus to the make field
  }, [setFocus, reset, auction]);

  return (
    <form className="flex flex-col mt-3" onSubmit={handleSubmit(onSubmit)}>
      <Input
        label="Make"
        name="make"
        control={control}
        rules={{ required: "Make is require" }}
      />
      <Input
        label="Model"
        name="model"
        control={control}
        rules={{ required: "Model is require" }}
      />
      <Input
        label="Color"
        name="color"
        control={control}
        rules={{ required: "Color is require" }}
      />
      <div className="grid grid-cols-2 gap-3">
        <Input
          label="Year"
          name="year"
          type="number"
          control={control}
          rules={{ required: "Year is require" }}
        />
        <Input
          label="Mileage"
          name="mileage"
          type="number"
          control={control}
          rules={{ required: "Mileage is require" }}
        />
      </div>

      {pathname === "/auctions/create" && (
        <>
          <Input
            label="Image URL"
            name="imageUrl"
            control={control}
            rules={{ required: "Image URL is require" }}
          />

          <div className="grid grid-cols-2 gap-3">
            <Input
              label="Reserve Price (enter 0 if no reserve)"
              type="number"
              name="revervePrice"
              control={control}
              rules={{ required: "Reserve price is require" }}
            />
            <DateInput
              label="Auction end date/time"
              name="auctionEnd"
              control={control}
              dateFormat="dd MMMM yyyy h:mm a"
              showTimeSelect
              rules={{ required: "Auction end date is require" }}
            />
          </div>
        </>
      )}

      <div className="flex justify-between">
        <Button outline color="gray">
          Cancel
        </Button>
        <Button
          isProcessing={isSubmitting}
          outline
          color="success"
          type="submit"
          disabled={!isValid}
        >
          Submit
        </Button>
      </div>
    </form>
  );
};

export default AuctionForm;
