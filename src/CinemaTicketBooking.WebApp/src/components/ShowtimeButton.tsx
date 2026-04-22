export type ShowtimeSlot = {
  time: string
  format: string
  availability: "Còn vé" | "Sắp hết vé"
}

export function ShowtimeButton({ slot }: { slot: ShowtimeSlot }) {
  return (
    <button
      type="button"
      className="flex min-w-[108px] flex-col gap-1 rounded-lg border border-outline-variant/30 bg-surface-variant/40 px-4 py-3 text-left transition-all hover:border-primary/50"
    >
      <span className="font-headline text-xl font-semibold">{slot.time}</span>
      <span className="inline-flex w-fit rounded border border-primary/25 bg-primary/10 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wide text-primary">
        {slot.format}
      </span>
      <span className="text-xs text-on-surface-variant">{slot.availability}</span>
    </button>
  )
}
