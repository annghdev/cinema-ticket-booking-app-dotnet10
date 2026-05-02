import { Link, useLocation } from "react-router-dom"
import type { ShowTimeDto } from "../types/ShowTime"
import { format } from "date-fns"

export function ShowtimeButton({ showtime }: { showtime: ShowTimeDto }) {
  const location = useLocation()
  const returnUrl = encodeURIComponent(location.pathname + location.search)
  const timeStr = format(new Date(showtime.startAt), "HH:mm")
  const availability = showtime.availableTicketCount > 10 ? "Còn vé" : showtime.availableTicketCount > 0 ? "Sắp hết vé" : "Hết vé"
  
  return (
    <Link
      to={`/showtimes/${showtime.id}/seats?returnUrl=${returnUrl}`}
      className={`flex min-w-[108px] flex-col gap-1 rounded-lg border border-outline-variant/30 bg-surface-variant/40 px-4 py-3 text-left transition-all hover:border-primary/50 ${showtime.availableTicketCount === 0 ? "pointer-events-none opacity-50 grayscale" : ""}`}
    >
      <span className="font-headline text-xl font-semibold">{timeStr}</span>
      <span className="inline-flex w-fit rounded border border-primary/25 bg-primary/10 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wide text-primary">
        {showtime.screenCode}
      </span>
      <span className="text-xs text-on-surface-variant">{availability}</span>
    </Link>
  )
}
