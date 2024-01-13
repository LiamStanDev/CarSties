"use client";

import { Button, TextInput } from "flowbite-react";
import { FieldValues, useForm } from "react-hook-form";
import Input from "../components/Input";
import { useEffect } from "react";
import DateInput from "../components/DateInput";

const AuctionForm = () => {
  const {
    control,
    handleSubmit,
    setFocus,
    formState: { isSubmitting, isDirty, isValid, errors },
  } = useForm({ mode: "onTouched" });

  const onSubmit = (data: FieldValues) => {
    console.log(data);
  };

  useEffect(() => {
    setFocus("make"); // set the focus to the make field
  }, [setFocus]);

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