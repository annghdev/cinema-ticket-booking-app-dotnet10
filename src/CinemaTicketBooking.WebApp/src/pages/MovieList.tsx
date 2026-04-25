import { useEffect, useMemo, useState } from "react"
import { Link } from "react-router-dom"
import { getUpcomingAndNowShowingMovies } from "../apis/movieApi"
import { getShowTimes } from "../apis/showtimeApi"
import type { MovieDto } from "../types/Movie"
import type { ShowTimeDto } from "../types/ShowTime"

type MovieWithShowTimes = {
  movie: MovieDto
  showtimes: ShowTimeDto[]
}

function formatDuration(duration: number) {
  const hours = Math.floor(duration / 60)
  const mins = duration % 60
  return `${hours}h ${mins.toString().padStart(2, "0")}m`
}

function formatDateLabel(dateInput: string) {
  const date = new Date(dateInput)
  if (Number.isNaN(date.getTime())) {
    return dateInput
  }

  return new Intl.DateTimeFormat("vi-VN", { day: "2-digit", month: "2-digit", year: "numeric" }).format(date)
}

function formatShowTime(showtime: ShowTimeDto) {
  const date = new Date(showtime.startAt)
  if (Number.isNaN(date.getTime())) {
    return showtime.startAt
  }
  return new Intl.DateTimeFormat("vi-VN", { hour: "2-digit", minute: "2-digit" }).format(date)
}

function genreLabel(genre: MovieDto["genre"]) {
  if (genre === "SciFi") {
    return "Sci-Fi"
  }
  return genre
}

function sectionTagByStatus(status: MovieDto["status"]) {
  if (status === "NowShowing") {
    return "NOW SHOWING"
  }
  if (status === "Upcoming") {
    return "COMING SOON"
  }
  return "NO SHOW"
}

function MovieList() {
  const [movies, setMovies] = useState<MovieDto[]>([])
  const [showtimes, setShowtimes] = useState<ShowTimeDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function loadData() {
      try {
        setLoading(true)
        const [movieData, showtimeData] = await Promise.all([getUpcomingAndNowShowingMovies(), getShowTimes()])
        const activeShowtimes = showtimeData
          .filter((item) => item.status === "Upcoming" || item.status === "Showing")
          .sort((a, b) => new Date(a.startAt).getTime() - new Date(b.startAt).getTime())
        setMovies(movieData)
        setShowtimes(activeShowtimes)
        setError(null)
      } catch {
        setError("Không tải được dữ liệu phim/lịch chiếu từ backend.")
      } finally {
        setLoading(false)
      }
    }

    void loadData()
  }, [])

  const showtimesByMovieId = useMemo(() => {
    return showtimes.reduce<Record<string, ShowTimeDto[]>>((acc, item) => {
      if (!acc[item.movieId]) {
        acc[item.movieId] = []
      }
      acc[item.movieId].push(item)
      return acc
    }, {})
  }, [showtimes])

  const moviesWithShowtimes = useMemo<MovieWithShowTimes[]>(() => {
    return movies.map((movie) => ({
      movie,
      showtimes: showtimesByMovieId[movie.id] ?? [],
    }))
  }, [movies, showtimesByMovieId])

  const hotMovies = useMemo(() => {
    return [...moviesWithShowtimes]
      .sort((a, b) => {
        if (b.showtimes.length !== a.showtimes.length) {
          return b.showtimes.length - a.showtimes.length
        }
        const aAvailable = a.showtimes.reduce((sum, item) => sum + item.availableTicketCount, 0)
        const bAvailable = b.showtimes.reduce((sum, item) => sum + item.availableTicketCount, 0)
        return bAvailable - aAvailable
      })
      .slice(0, 6)
  }, [moviesWithShowtimes])

  const nowShowingMovies = useMemo(() => {
    return moviesWithShowtimes.filter((item) => item.movie.status === "NowShowing")
  }, [moviesWithShowtimes])

  const upcomingMovies = useMemo(() => {
    return moviesWithShowtimes.filter((item) => item.movie.status === "Upcoming")
  }, [moviesWithShowtimes])

  if (loading) {
    return (
      <main className="min-h-screen bg-background pb-20 pt-24 md:pt-28">
        <div className="mx-auto w-full max-w-screen-2xl px-8 py-16">
          <div className="rounded-xl border border-outline-variant/20 bg-surface-container-low p-8 text-center text-on-surface-variant">
            Đang tải danh sách phim...
          </div>
        </div>
      </main>
    )
  }

  if (error) {
    return (
      <main className="min-h-screen bg-background pb-20 pt-24 md:pt-28">
        <div className="mx-auto w-full max-w-screen-2xl px-8 py-16">
          <div className="rounded-xl border border-red-400/30 bg-red-500/10 p-8 text-center text-red-200">{error}</div>
        </div>
      </main>
    )
  }

  return (
    <main className="min-h-screen bg-background pb-20 pt-24 md:pt-28">
      <section className="relative overflow-hidden border-b border-outline-variant/10 bg-surface-container-low/40 py-20">
        <div className="absolute inset-0">
          <img
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuAXFfKSlBIjWgJAXE2TSgP4bQr2F4Uw4bWNSF8ujCRXUkNKzCbCAR3zDdnq_7PRBiOtaIu_yZ3tVPyZZSf_t7yhF7McQ8FCetR3i0d-2hrlWjvCaFpVDa_kuGMPNnbe5ZUmE0izxS2Gbjhqn4pbytGwVCheCOx6NCMTpdH7fT5dxlHbTmWa0GXHAMoPOzbDNwuQBhFil2L4OgKBlOMZu1MH9-AeBpKh0sl7fvz92gPhzC44s_kANtNWI1myHc6X0X-as7Rsc3VHcow"
            alt="cinema background"
            className="h-full w-full object-cover opacity-20"
          />
          <div className="absolute inset-0 bg-gradient-to-r from-background via-background/85 to-background/40" />
        </div>
        <div className="relative z-10 mx-auto w-full max-w-screen-2xl px-8">
          <span className="mb-4 inline-flex items-center gap-2 border border-secondary/20 bg-secondary/10 px-3 py-1 text-sm font-semibold tracking-[0.14em] text-secondary">
            <span className="material-symbols-outlined text-base">movie</span>
            MOVIE LIBRARY
          </span>
          <h1 className="font-headline text-5xl font-black tracking-tight text-on-background md:text-7xl">
            PHIM HOT, ĐANG CHIẾU
            <br />
            <span className="text-primary drop-shadow-[0_0_15px_rgba(97,180,254,0.35)]">VÀ SẮP KHỞI CHIẾU</span>
          </h1>
          <p className="mt-4 max-w-3xl text-lg text-on-surface-variant">
            Dữ liệu phim và suất chiếu đang được lấy trực tiếp từ backend để hiển thị trạng thái phát hành theo thời gian thực.
          </p>
        </div>
      </section>

      <section className="py-16">
        <div className="mx-auto w-full max-w-screen-2xl px-8">
          <div className="mb-6 border-l-4 border-secondary pl-5">
            <h2 className="font-headline text-4xl font-black tracking-tight text-on-background">PHIM HOT</h2>
            <p className="text-on-surface-variant">Slide movie card xếp ngang</p>
          </div>

          <div className="flex gap-6 overflow-x-auto pb-2">
            {hotMovies.map(({ movie, showtimes: movieShowtimes }) => {
              const firstShowtime = movieShowtimes[0]
              return (
                <article
                  key={movie.id}
                  className="group relative h-[460px] min-w-[280px] overflow-hidden rounded-xl border border-outline-variant/20 bg-surface-container-low md:min-w-[320px]"
                >
                  <img src={movie.thumbnailUrl} alt={movie.name} className="h-full w-full object-cover transition-transform duration-700 group-hover:scale-105" />
                  <div className="absolute inset-0 bg-gradient-to-t from-background via-background/35 to-transparent" />
                  <div className="absolute right-4 top-4 rounded border border-secondary/20 bg-secondary/10 px-3 py-1 text-xs font-bold tracking-widest text-secondary">
                    {sectionTagByStatus(movie.status)}
                  </div>
                  <div className="absolute bottom-0 w-full p-6">
                    <p className="text-xs font-semibold tracking-[0.16em] text-secondary">{genreLabel(movie.genre)}</p>
                    <h3 className="mt-2 font-headline text-3xl font-black leading-tight text-on-background">{movie.name}</h3>
                    <div className="mt-3 flex items-center gap-3 text-sm text-on-surface-variant">
                      <span>{formatDuration(movie.duration)}</span>
                      <span>•</span>
                      <span>{movie.studio}</span>
                    </div>
                    <div className="mt-5 flex gap-2">
                      {firstShowtime ? (
                        <Link
                          to={`/showtimes/${firstShowtime.id}/seats`}
                          className="inline-flex items-center gap-2 rounded bg-gradient-to-r from-primary to-primary-container px-4 py-2 text-sm font-bold text-on-primary transition-all hover:shadow-[0_0_14px_rgba(0,244,254,0.45)]"
                        >
                          Chọn ghế
                          <span className="material-symbols-outlined text-base">arrow_forward</span>
                        </Link>
                      ) : (
                        <span className="inline-flex items-center rounded border border-outline-variant/30 bg-surface-variant/50 px-4 py-2 text-sm font-semibold text-on-surface-variant">
                          Chưa có suất
                        </span>
                      )}
                    </div>
                  </div>
                </article>
              )
            })}
          </div>
        </div>
      </section>

      <section className="bg-surface py-16">
        <div className="mx-auto w-full max-w-screen-2xl px-8">
          <div className="mb-8 border-l-4 border-primary pl-5">
            <h2 className="font-headline text-4xl font-black tracking-tight text-on-background">ĐANG CHIẾU</h2>
            <p className="text-on-surface-variant">List movie card dọc</p>
          </div>

          <div className="space-y-5">
            {nowShowingMovies.map(({ movie, showtimes: movieShowtimes }) => (
              <article
                key={movie.id}
                className="group rounded-xl border border-outline-variant/20 bg-surface-container-low p-4 shadow-[0_10px_28px_rgba(0,0,0,0.25)] md:p-5"
              >
                <div className="flex flex-col gap-5 md:flex-row">
                  <div className="h-56 w-full shrink-0 overflow-hidden rounded-lg bg-surface-container-highest md:h-auto md:w-44">
                    <img src={movie.thumbnailUrl} alt={movie.name} className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105" />
                  </div>

                  <div className="flex min-w-0 flex-1 flex-col">
                    <div className="flex flex-wrap items-center gap-3">
                      <h3 className="font-headline text-3xl font-black tracking-tight text-on-background">{movie.name}</h3>
                      <span className="rounded border border-secondary/25 bg-secondary/10 px-2 py-1 text-xs font-bold tracking-widest text-secondary">
                        {sectionTagByStatus(movie.status)}
                      </span>
                    </div>

                    <p className="mt-2 text-sm text-on-surface-variant">
                      {genreLabel(movie.genre)} • {formatDuration(movie.duration)} • Đạo diễn: {movie.director}
                    </p>
                    <p className="mt-3 max-w-3xl leading-relaxed text-on-surface-variant">{movie.description}</p>

                    <div className="mt-5 grid grid-cols-1 gap-3 text-sm text-on-surface-variant sm:grid-cols-3">
                      <div className="rounded border border-outline-variant/20 bg-surface-container/70 px-3 py-2">
                        <span className="text-xs uppercase tracking-wide text-secondary">Studio</span>
                        <p className="mt-1 font-medium text-on-background">{movie.studio}</p>
                      </div>
                      <div className="rounded border border-outline-variant/20 bg-surface-container/70 px-3 py-2">
                        <span className="text-xs uppercase tracking-wide text-secondary">Cập nhật</span>
                        <p className="mt-1 font-medium text-on-background">{formatDateLabel(movie.createdAt)}</p>
                      </div>
                      <div className="rounded border border-outline-variant/20 bg-surface-container/70 px-3 py-2">
                        <span className="text-xs uppercase tracking-wide text-secondary">Suất đang mở</span>
                        <p className="mt-1 font-medium text-on-background">{movieShowtimes.length}</p>
                      </div>
                    </div>

                    <div className="mt-5 flex flex-wrap gap-2">
                      {movieShowtimes.slice(0, 6).map((showtime) => (
                        <Link
                          key={showtime.id}
                          to={`/showtimes/${showtime.id}/seats`}
                          className="rounded border border-outline-variant/30 bg-surface-container/80 px-3 py-1.5 text-sm font-semibold text-on-background transition-colors hover:border-primary/50 hover:text-primary"
                        >
                          {formatShowTime(showtime)} · {showtime.cinemaName}
                        </Link>
                      ))}
                      {movieShowtimes.length === 0 && (
                        <span className="rounded border border-outline-variant/30 bg-surface-container/50 px-3 py-1.5 text-sm text-on-surface-variant">
                          Chưa có suất chiếu khả dụng
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section className="py-16">
        <div className="mx-auto w-full max-w-screen-2xl px-8">
          <div className="mb-8 border-l-4 border-secondary pl-5">
            <h2 className="font-headline text-4xl font-black tracking-tight text-on-background">SẮP KHỞI CHIẾU</h2>
            <p className="text-on-surface-variant">List movie card ngang</p>
          </div>

          <div className="space-y-5">
            {upcomingMovies.map(({ movie, showtimes: movieShowtimes }) => (
              <article
                key={movie.id}
                className="group flex flex-col gap-5 overflow-hidden rounded-xl border border-outline-variant/20 bg-surface-container-low p-4 transition-all duration-300 hover:border-secondary/40 md:flex-row md:items-stretch md:p-5"
              >
                <div className="h-52 w-full shrink-0 overflow-hidden rounded-lg bg-surface-container-highest md:h-auto md:w-60">
                  <img src={movie.thumbnailUrl} alt={movie.name} className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105" />
                </div>
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="rounded border border-secondary/20 bg-secondary/10 px-2 py-1 text-xs font-bold tracking-widest text-secondary">
                      COMING SOON
                    </span>
                    <span className="rounded border border-outline-variant/30 bg-surface-container/70 px-2 py-1 text-xs font-semibold text-on-surface-variant">
                      {genreLabel(movie.genre)}
                    </span>
                  </div>
                  <h3 className="mt-3 font-headline text-3xl font-black tracking-tight text-on-background">{movie.name}</h3>
                  <p className="mt-2 text-sm text-on-surface-variant">
                    {formatDuration(movie.duration)} • {movie.studio} • Đạo diễn: {movie.director}
                  </p>
                  <p className="mt-3 max-w-3xl leading-relaxed text-on-surface-variant">{movie.description}</p>

                  <div className="mt-5 flex flex-wrap items-center gap-4">
                    <div className="rounded border border-outline-variant/20 bg-surface-container/70 px-3 py-2 text-sm">
                      <p className="text-xs uppercase tracking-wide text-secondary">Lịch gần nhất</p>
                      <p className="font-semibold text-on-background">
                        {movieShowtimes[0] ? formatDateLabel(movieShowtimes[0].date) : "Đang cập nhật"}
                      </p>
                    </div>
                    {movieShowtimes[0] ? (
                      <Link
                        to={`/showtimes/${movieShowtimes[0].id}/seats`}
                        className="inline-flex items-center gap-2 rounded border border-outline-variant/30 bg-surface-variant/40 px-4 py-2 text-sm font-semibold text-on-background transition-colors hover:bg-surface-variant/60"
                      >
                        <span className="material-symbols-outlined text-base">event_available</span>
                        Chọn suất sớm
                      </Link>
                    ) : (
                      <button
                        type="button"
                        className="inline-flex cursor-not-allowed items-center gap-2 rounded border border-outline-variant/30 bg-surface-variant/30 px-4 py-2 text-sm font-semibold text-on-surface-variant"
                      >
                        <span className="material-symbols-outlined text-base">notifications</span>
                        Nhắc tôi khi mở bán
                      </button>
                    )}
                  </div>
                </div>
              </article>
            ))}
          </div>
        </div>
      </section>
    </main>
  )
}

export default MovieList
