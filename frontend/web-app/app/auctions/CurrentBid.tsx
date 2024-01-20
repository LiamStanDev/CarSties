type Props = {
  reservePrice: number;
  amount?: number;
};

const CurrentBid = ({ reservePrice, amount }: Props) => {
  const text = amount ? "$" + amount : "No bids";
  const color = amount
    ? amount > reservePrice
      ? "bg-green-600"
      : "bg-amber-600"
    : "bg-red-600";

  return (
    <div
      className={`
        border-2 border-white text-white py-1 px-2 rounded-lg ${color} flex justify-center
      `}
    >
      {text}
    </div>
  );
};

export default CurrentBid;
