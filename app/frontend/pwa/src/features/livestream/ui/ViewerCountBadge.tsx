interface Props {
  count: number;
}

export default function ViewerCountBadge({ count }: Props) {
  const formatted = count >= 1000 ? `${(count / 1000).toFixed(1)}k` : String(count);
  return (
    <div className="flex items-center gap-1 rounded-full bg-black/60 px-2 py-0.5 text-xs text-white">
      <span className="h-1.5 w-1.5 rounded-full bg-red-400" aria-hidden="true" />
      <span>{formatted}</span>
    </div>
  );
}
