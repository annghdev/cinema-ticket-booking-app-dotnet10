import { ShowtimeButton } from "../components/ShowtimeButton"
import { useEffect, useState, useMemo, useCallback } from "react"
import { useParams } from "react-router-dom"
import { useToast } from "../contexts/ToastContext"
import { getShowTimes } from "../apis/showtimeApi"
import { getMovieById } from "../apis/movieApi"
import type { ShowTimeDto } from "../types/ShowTime"
import type { MovieDto } from "../types/Movie"
import { format, addDays, startOfDay } from "date-fns"
import { vi } from "date-fns/locale"
import { MovieTrailerModal } from "../components/MovieTrailerModal"

function MovieWithShowTimes() {
  const { movieId } = useParams<{ movieId: string }>()
  const { error } = useToast()
  
  const [movieLoading, setMovieLoading] = useState(true)
  const [showtimesLoading, setShowtimesLoading] = useState(false)
  const [movie, setMovie] = useState<MovieDto | null>(null)
  const [showtimes, setShowtimes] = useState<ShowTimeDto[]>([])
  const [selectedDate, setSelectedDate] = useState<Date>(startOfDay(new Date()))
  const [isTrailerModalOpen, setIsTrailerModalOpen] = useState(false)

  const dates = useMemo(() => {
    return Array.from({ length: 7 }).map((_, i) => {
      const d = addDays(startOfDay(new Date()), i)
      return {
        date: d,
        dayLabel: i === 0 ? "Hôm nay" : format(d, "eeee", { locale: vi }),
        dateLabel: format(d, "dd"),
        monthLabel: format(d, "MMM", { locale: vi }),
      }
    })
  }, [])

  const fetchMovie = useCallback(async () => {
    if (!movieId) return
    setMovieLoading(true)
    try {
      const movieData = await getMovieById(movieId)
      setMovie(movieData)
    } catch (e) {
      console.error(e)
      error("Không thể tải thông tin phim. Vui lòng thử lại sau.")
    } finally {
      setMovieLoading(false)
    }
  }, [movieId, error])

  const fetchShowtimes = useCallback(async () => {
    if (!movieId) return
    setShowtimesLoading(true)
    try {
      const showtimesData = await getShowTimes({ 
        movieId, 
        date: format(selectedDate, "yyyy-MM-dd"), 
        status: "Upcoming" 
      })
      setShowtimes(showtimesData)
    } catch (e) {
      console.error(e)
      // Only show error toast if it's not the initial fetch (initial error handled by fetchMovie)
      if (movie) {
        error("Không thể tải lịch chiếu cho ngày này.")
      }
    } finally {
      setShowtimesLoading(false)
    }
  }, [movieId, selectedDate, movie, error])

  useEffect(() => {
    fetchMovie()
  }, [fetchMovie])

  useEffect(() => {
    fetchShowtimes()
  }, [fetchShowtimes])

  const groupedByCinema = useMemo(() => {
    const groups: {
      id: string
      name: string
      address: string
      showtimes: ShowTimeDto[]
    }[] = []
    
    showtimes.forEach((st) => {
      let group = groups.find((g) => g.id === st.cinemaId)
      if (!group) {
        group = {
          id: st.cinemaId,
          name: st.cinemaName,
          address: st.cinemaAddress,
          showtimes: [],
        }
        groups.push(group)
      }
      group.showtimes.push(st)
    })
    
    return groups
  }, [showtimes])

  if (movieLoading && !movie) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent"></div>
      </div>
    )
  }

  if (!movie) {
    return (
      <div className="flex h-screen items-center justify-center">
        <p className="text-on-surface-variant">Không tìm thấy thông tin phim.</p>
      </div>
    )
  }

  return (
    <main className="flex-grow pb-12">
      <section className="relative h-[614px] min-h-[500px] w-full overflow-hidden bg-surface-container-low">
        <div className="absolute inset-0 h-full w-full">
          <img
            alt={movie.name}
            className="h-full w-full object-cover opacity-30"
            src={movie.thumbnailUrl}
          />
          <div className="absolute inset-0 bg-gradient-to-t from-background via-background/80 to-transparent" />
        </div>
        <div className="relative z-10 mx-auto flex h-full w-full max-w-screen-2xl flex-col justify-end px-8 pb-12">
          <div className="max-w-4xl">
            <div className="mb-6 inline-flex items-center space-x-2 rounded-full border border-outline-variant/20 bg-surface-variant/40 px-3 py-1 text-sm text-on-surface-variant backdrop-blur-xl">
              <span className="material-symbols-outlined text-[16px]">stars</span>
              <span>{movie.genre}</span>
              <span className="px-2">•</span>
              <span>{movie.duration} phút</span>
              <span className="px-2">•</span>
              <span className="rounded border border-outline-variant px-1.5 text-xs">PG-13</span>
            </div>
            <h1 className="mb-4 font-headline text-5xl font-bold leading-tight tracking-tighter text-on-background md:text-7xl uppercase">{movie.name}</h1>
            <p className="mb-8 max-w-3xl text-lg text-on-surface-variant line-clamp-3">
              {movie.description}
            </p>
            <div className="flex flex-wrap gap-4">
              {movie.officialTrailerUrl && (
                <button 
                  type="button"
                  onClick={() => setIsTrailerModalOpen(true)}
                  className="flex items-center gap-2 rounded bg-gradient-to-r from-primary to-primary-container px-7 py-3 font-headline font-bold text-on-primary transition-all hover:shadow-[0_0_18px_rgba(97,180,254,0.5)]"
                >
                  <span className="material-symbols-outlined text-base">play_arrow</span>
                  Xem trailer
                </button>
              )}
            </div>
          </div>
        </div>
      </section>

      <div className="relative z-20 mx-auto -mt-8 w-full max-w-screen-2xl space-y-8 px-8">
        <section className="rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-6 shadow-2xl backdrop-blur-xl">
          <h3 className="mb-5 font-headline text-2xl font-bold text-on-background">Thông tin phim</h3>
          <div className="grid gap-4 text-sm text-on-surface-variant md:grid-cols-2 lg:grid-cols-4">
            <p><span className="font-bold text-on-background">Hãng sản xuất:</span> {movie.studio}</p>
            <p><span className="font-bold text-on-background">Đạo diễn:</span> {movie.director}</p>
            <p><span className="font-bold text-on-background">Khởi chiếu:</span> {format(new Date(movie.createdAt), "dd/MM/yyyy")}</p>
            <p><span className="font-bold text-on-background">Trạng thái:</span> {movie.status}</p>
          </div>
          <p className="mt-4 leading-relaxed text-on-surface-variant">
            {movie.description}
          </p>
        </section>

        <section className="mb-4 flex flex-col gap-6 rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-4 shadow-2xl backdrop-blur-xl md:flex-row md:items-center md:justify-between">
          <div className="flex-grow overflow-hidden">
            <h3 className="mb-2 text-sm text-on-surface-variant">Chọn ngày chiếu</h3>
            <div className="flex space-x-4 overflow-x-auto pb-2">
              {dates.map((item) => {
                const isActive = item.date.getTime() === selectedDate.getTime()
                return (
                  <button
                    key={item.date.toISOString()}
                    type="button"
                    onClick={() => setSelectedDate(item.date)}
                    className={
                      isActive
                        ? "flex h-[80px] min-w-[80px] flex-col items-center justify-center rounded-lg border border-secondary/50 bg-surface-container-highest text-secondary shadow-[0_0_8px_rgba(0,244,254,0.5)]"
                        : "flex h-[80px] min-w-[80px] flex-col items-center justify-center rounded-lg border border-outline-variant/30 bg-surface-container-low text-on-surface-variant transition-colors hover:bg-surface-container"
                    }
                  >
                    <span className="text-[10px] uppercase font-bold opacity-70">{item.dayLabel}</span>
                    <span className="font-headline text-2xl font-bold">{item.dateLabel}</span>
                    <span className="text-[10px] uppercase font-bold opacity-70">{item.monthLabel}</span>
                  </button>
                )
              })}
            </div>
          </div>
        </section>

        <section className="space-y-6">
          {showtimesLoading ? (
            <div className="flex h-32 items-center justify-center">
              <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent"></div>
            </div>
          ) : groupedByCinema.length === 0 ? (
            <div className="flex flex-col items-center justify-center rounded-xl border border-outline-variant/20 bg-surface-container-low py-12 text-center">
              <span className="material-symbols-outlined text-4xl text-on-surface-variant/40 mb-2">calendar_today</span>
              <p className="text-on-surface-variant">Không có lịch chiếu cho ngày đã chọn.</p>
            </div>
          ) : (
            groupedByCinema.map((cinema) => (
              <div key={cinema.id} className="rounded-lg border border-outline-variant/15 bg-surface-container/60 p-4 md:p-5">
                <div className="mb-4 flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <h4 className="flex items-center gap-2 font-headline text-lg font-semibold text-primary">
                      <span className="material-symbols-outlined text-[20px]">location_city</span>
                      {cinema.name}
                    </h4>
                    <p className="mt-1 flex items-start gap-2 text-sm text-on-surface-variant">
                      <span className="material-symbols-outlined mt-0.5 shrink-0 text-[18px] opacity-70">map</span>
                      {cinema.address}
                    </p>
                  </div>
                </div>
                <div className="flex flex-wrap gap-3">
                  {cinema.showtimes.map((st) => (
                    <ShowtimeButton key={st.id} showtime={st} />
                  ))}
                </div>
              </div>
            ))
          )}
        </section>
      </div>

      <MovieTrailerModal
        isOpen={isTrailerModalOpen}
        onClose={() => setIsTrailerModalOpen(false)}
        movieName={movie.name}
        trailerUrl={movie.officialTrailerUrl}
      />
    </main>
  )
}

export default MovieWithShowTimes
