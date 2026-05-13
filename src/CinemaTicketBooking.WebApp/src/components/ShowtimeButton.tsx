import { Link, useLocation } from "react-router-dom"
import type { ShowTimeDto } from "../types/ShowTime"
import { format } from "date-fns"

export function ShowtimeButton({ showtime }: { showtime: ShowTimeDto }) {
  const location = useLocation()
  const returnUrl = encodeURIComponent(location.pathname + location.search)
  const startTimeStr = format(new Date(showtime.startAt), "HH:mm")
  const endTimeStr = format(new Date(showtime.endAt), "HH:mm")
  const availability = showtime.availableTicketCount > 10 ? "Còn vé" : showtime.availableTicketCount > 0 ? "Sắp hết vé" : "Hết vé"
  
  return (
    <Link
      to={`/showtimes/${showtime.id}/seats?returnUrl=${returnUrl}`}
      className={`group flex min-w-[120px] flex-col gap-1.5 rounded-xl border border-outline-variant/20 bg-surface-container-high p-4 transition-all hover:border-primary/50 hover:bg-surface-container-highest hover:shadow-lg ${showtime.availableTicketCount === 0 ? "pointer-events-none opacity-50 grayscale" : "active:scale-[0.98]"}`}
    >
      <div className="flex items-center justify-between">
        <span className="rounded bg-secondary/10 px-2 py-0.5 text-[10px] font-black uppercase tracking-widest text-secondary shadow-sm">
          {showtime.format || "2D"}
        </span>
        <span className="material-symbols-outlined text-sm text-on-surface-variant transition-transform group-hover:translate-x-0.5">chevron_right</span>
      </div>
      <div className="mt-1">
        <span className="font-headline text-lg font-black tracking-tight text-white">{startTimeStr} <span className="mx-0.5 opacity-40">→</span> {endTimeStr}</span>
      </div>
      <div className="flex items-center gap-1.5 text-[11px] font-bold text-on-surface-variant">
        <span className="material-symbols-outlined text-[14px]">meeting_room</span>
        {showtime.screenCode}
      </div>
      <div className={`text-[10px] font-bold uppercase tracking-wider ${showtime.availableTicketCount > 0 ? "text-primary" : "text-slate-500"}`}>
        {availability}
      </div>
    </Link>
  )
}
