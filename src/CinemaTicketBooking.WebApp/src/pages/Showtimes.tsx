import * as Dialog from "@radix-ui/react-dialog"
import { ShowtimeButton, type ShowtimeSlot } from "../components/ShowtimeButton"
import { useState } from "react"
import { useToast } from "../contexts/ToastContext"

type GroupMode = "cinema" | "movie"

type MovieBlock = {
  title: string
  genre: string
  duration: string
  poster: string
  showtimes: ShowtimeSlot[]
}

type CinemaBlock = {
  name: string
  address: string
  distance: string
  showtimes: ShowtimeSlot[]
}

const byCinemaData: { cinemaName: string; address: string; movies: MovieBlock[] }[] = [
  {
    cinemaName: "Horizon Nexus - Downtown",
    address: "1010 Cyber Avenue, Neon District",
    movies: [
      {
        title: "ECHOES OF ETERNITY",
        genre: "Sci-Fi / Action",
        duration: "2h 45m",
        poster:
          "https://lh3.googleusercontent.com/aida-public/AB6AXuCSx01sT-iyNtkZNiRVTpcuOiJ6B1Ir-0f1vamO9K7_4bSjSFvHA_wBAzz8CAXTWzXdFV01PD3mkKiLv9F9ptoBYRDiDXyJ5Ftbdq9U9vyIcizblV6_j5xIBtewyG4g3xYVv82bbsGaL0liwYdf3VU_i7AAdQEMlWuHkIWYYuJ1jDe6OcsOFxAPKbFYL6Wfbrh8clWspc1JkRIw5bw4XRm1tuXYFBoKOuPQVHO2YxNaaxpTZPM144IapHAFazv4tnk_PUBtkV5jPtI",
        showtimes: [
          { time: "14:30", format: "IMAX 2D", availability: "Còn vé" },
          { time: "18:00", format: "Dolby Atmos", availability: "Còn vé" },
          { time: "21:15", format: "4DX", availability: "Sắp hết vé" },
        ],
      },
      {
        title: "NEON GENESIS",
        genre: "Thriller",
        duration: "2h 10m",
        poster:
          "https://lh3.googleusercontent.com/aida-public/AB6AXuBQu8WkkrvsAmbKuM3Hy78ae7t6EUjRQcOrSme3BjDUnqMrJC0Evtw-2pDoRE-sJSqJZ2PlTQm8b9FzzvLA-_azF0cHrcT8sirpwIWad3LeHnB7_IM4vTU-QeTVwH5DY5kd8-DkFCqsZAs2ZWrJDk31TXiXWAatcntjQFatbEwSWe_Dd6pswk8-9P9gx04R3UEa8E_1VCEoUXMQ8gQhJpfn-jQ0KXI6AXr_SL3cui8Yuj2qc3iIvpQtsYxgR-SljKv_XNObklvOpCE",
        showtimes: [
          { time: "11:00", format: "2D Phụ đề", availability: "Còn vé" },
          { time: "16:40", format: "2D Lồng tiếng", availability: "Còn vé" },
        ],
      },
    ],
  },
  {
    cinemaName: "Horizon Zenith - Uptown",
    address: "444 Director's Cut Blvd, Uptown",
    movies: [
      {
        title: "ECHOES OF ETERNITY",
        genre: "Sci-Fi / Action",
        duration: "2h 45m",
        poster:
          "https://lh3.googleusercontent.com/aida-public/AB6AXuCSx01sT-iyNtkZNiRVTpcuOiJ6B1Ir-0f1vamO9K7_4bSjSFvHA_wBAzz8CAXTWzXdFV01PD3mkKiLv9F9ptoBYRDiDXyJ5Ftbdq9U9vyIcizblV6_j5xIBtewyG4g3xYVv82bbsGaL0liwYdf3VU_i7AAdQEMlWuHkIWYYuJ1jDe6OcsOFxAPKbFYL6Wfbrh8clWspc1JkRIw5bw4XRm1tuXYFBoKOuPQVHO2YxNaaxpTZPM144IapHAFazv4tnk_PUBtkV5jPtI",
        showtimes: [
          { time: "10:00", format: "Laser 2D", availability: "Còn vé" },
          { time: "13:45", format: "IMAX 2D", availability: "Còn vé" },
          { time: "19:30", format: "Dolby Atmos", availability: "Sắp hết vé" },
        ],
      },
      {
        title: "BÓNG MA SỐ",
        genre: "Hành động",
        duration: "1h 58m",
        poster:
          "https://lh3.googleusercontent.com/aida-public/AB6AXuB441TQ4zaDFJaVwak7wkoqPlYKOO9HCCAbnxwIsXxRjDLSBMppuUIKptbYkswlfezxO0m0MGLvEv3Pnin31QegTK_QqhEUISfxgtNf6cD_pAAvfyAZpafp-QUBqUIJ-TTZgGUFeWFsVcbD9sv96Oid1VIAKbWbwFkGxoZH_dxcf8YZO2IwVZt6YZTKbR1_o_S2yhsJHo4qf5yPUfF0lBuM990VFQdIRASdo5Pa0wBE9N_I-v3gw3m-JBFGHMhz1yXZ6AsqC-SQeG8",
        showtimes: [{ time: "20:50", format: "2D Phụ đề", availability: "Còn vé" }],
      },
    ],
  },
  {
    cinemaName: "Horizon Echo - Suburbia",
    address: "77 Indie Lane, Arts District",
    movies: [
      {
        title: "THÀNH PHỐ KHÔNG NGỦ",
        genre: "Tội phạm",
        duration: "2h 05m",
        poster:
          "https://lh3.googleusercontent.com/aida-public/AB6AXuChPcnpDT9D5r8ayBNHiBcGvHtN4mixoGqViVDxjNJwWgPgaa2y9NqNMo_w9DEz_8nzPq1RYg56OBGH2v3ALleNTx2zh57HTMkG2AIPLCPTcx4s8R9_838hIejqfUn0sMwfrTPgBIGUyTdwospGUe5l1YnvEil2hMYszoGxM9AjMdNf7Uf7NVlMKF-Or8ytthqliP-92N1VfngE8TFrGRU8XOlyB3BpYP9ZxO5KvhlU1VGw7pvqqZZ3KjMKbec9p0a_E39v0Wx7WWY",
        showtimes: [
          { time: "09:20", format: "2D Vietsub", availability: "Còn vé" },
          { time: "15:10", format: "VIP Recliner", availability: "Còn vé" },
        ],
      },
    ],
  },
]

const byMovieData: {
  movieTitle: string
  genre: string
  duration: string
  poster: string
  cinemas: CinemaBlock[]
}[] = [
  {
    movieTitle: "ECHOES OF ETERNITY",
    genre: "Sci-Fi / Action",
    duration: "2h 45m",
    poster:
      "https://lh3.googleusercontent.com/aida-public/AB6AXuCSx01sT-iyNtkZNiRVTpcuOiJ6B1Ir-0f1vamO9K7_4bSjSFvHA_wBAzz8CAXTWzXdFV01PD3mkKiLv9F9ptoBYRDiDXyJ5Ftbdq9U9vyIcizblV6_j5xIBtewyG4g3xYVv82bbsGaL0liwYdf3VU_i7AAdQEMlWuHkIWYYuJ1jDe6OcsOFxAPKbFYL6Wfbrh8clWspc1JkRIw5bw4XRm1tuXYFBoKOuPQVHO2YxNaaxpTZPM144IapHAFazv4tnk_PUBtkV5jPtI",
    cinemas: [
      {
        name: "Horizon Nexus - Downtown",
        address: "1010 Cyber Avenue, Neon District",
        distance: "2.4 km",
        showtimes: [
          { time: "14:30", format: "IMAX 2D", availability: "Còn vé" },
          { time: "18:00", format: "Dolby Atmos", availability: "Còn vé" },
        ],
      },
      {
        name: "Horizon Zenith - Uptown",
        address: "444 Director's Cut Blvd, Uptown",
        distance: "5.1 km",
        showtimes: [
          { time: "10:00", format: "Laser 2D", availability: "Còn vé" },
          { time: "19:30", format: "IMAX 2D", availability: "Sắp hết vé" },
        ],
      },
    ],
  },
  {
    movieTitle: "NEON GENESIS",
    genre: "Thriller",
    duration: "2h 10m",
    poster:
      "https://lh3.googleusercontent.com/aida-public/AB6AXuBQu8WkkrvsAmbKuM3Hy78ae7t6EUjRQcOrSme3BjDUnqMrJC0Evtw-2pDoRE-sJSqJZ2PlTQm8b9FzzvLA-_azF0cHrcT8sirpwIWad3LeHnB7_IM4vTU-QeTVwH5DY5kd8-DkFCqsZAs2ZWrJDk31TXiXWAatcntjQFatbEwSWe_Dd6pswk8-9P9gx04R3UEa8E_1VCEoUXMQ8gQhJpfn-jQ0KXI6AXr_SL3cui8Yuj2qc3iIvpQtsYxgR-SljKv_XNObklvOpCE",
    cinemas: [
      {
        name: "Horizon Nexus - Downtown",
        address: "1010 Cyber Avenue, Neon District",
        distance: "2.4 km",
        showtimes: [
          { time: "11:00", format: "2D Phụ đề", availability: "Còn vé" },
          { time: "21:00", format: "4DX", availability: "Còn vé" },
        ],
      },
    ],
  },
  {
    movieTitle: "BÓNG MA SỐ",
    genre: "Hành động",
    duration: "1h 58m",
    poster:
      "https://lh3.googleusercontent.com/aida-public/AB6AXuB441TQ4zaDFJaVwak7wkoqPlYKOO9HCCAbnxwIsXxRjDLSBMppuUIKptbYkswlfezxO0m0MGLvEv3Pnin31QegTK_QqhEUISfxgtNf6cD_pAAvfyAZpafp-QUBqUIJ-TTZgGUFeWFsVcbD9sv96Oid1VIAKbWbwFkGxoZH_dxcf8YZO2IwVZt6YZTKbR1_o_S2yhsJHo4qf5yPUfF0lBuM990VFQdIRASdo5Pa0wBE9N_I-v3gw3m-JBFGHMhz1yXZ6AsqC-SQeG8",
    cinemas: [
      {
        name: "Horizon Zenith - Uptown",
        address: "444 Director's Cut Blvd, Uptown",
        distance: "5.1 km",
        showtimes: [{ time: "20:50", format: "2D Phụ đề", availability: "Còn vé" }],
      },
    ],
  },
]

function Showtimes() {
  const { success } = useToast()
  const [groupMode, setGroupMode] = useState<GroupMode>("cinema")
  const [isFilterOpen, setIsFilterOpen] = useState(false)
  const [timeRange, setTimeRange] = useState("all")
  const [selectedCinemas, setSelectedCinemas] = useState<string[]>([])
  const [selectedMovies, setSelectedMovies] = useState<string[]>([])

  const dates = [
    { day: "Hôm nay", date: "14", month: "Oct", active: true },
    { day: "Th 3", date: "15", month: "Oct" },
    { day: "Th 4", date: "16", month: "Oct" },
    { day: "Th 5", date: "17", month: "Oct" },
    { day: "Th 6", date: "18", month: "Oct" },
    { day: "Th 7", date: "19", month: "Oct" },
    { day: "CN", date: "20", month: "Oct" },
  ]

  const cinemaOptions = ["Horizon Nexus - Downtown", "Horizon Zenith - Uptown", "Horizon Echo - Suburbia"]
  const movieOptions = ["ECHOES OF ETERNITY", "NEON GENESIS", "BÓNG MA SỐ", "THÀNH PHỐ KHÔNG NGỦ"]

  const toggleSelection = (value: string, current: string[], setter: (value: string[]) => void) => {
    if (current.includes(value)) {
      setter(current.filter((x) => x !== value))
    } else {
      setter([...current, value])
    }
  }

  return (
    <main className="min-h-screen bg-background pb-12 pt-24 md:pt-28">
      <div className="relative z-10 mx-auto w-full max-w-screen-2xl px-8">
        <section className="mb-12 flex flex-col gap-6 rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-4 shadow-2xl backdrop-blur-xl md:flex-row md:items-center md:justify-between">
          <div className="flex-grow overflow-hidden">
            <h3 className="mb-2 text-sm text-on-surface-variant">Chọn ngày</h3>
            <div className="flex space-x-4 overflow-x-auto pb-2">
              {dates.map((item) => (
                <button
                  key={item.date}
                  type="button"
                  className={
                    item.active
                      ? "flex h-[80px] min-w-[70px] flex-col items-center justify-center rounded-lg border border-secondary/50 bg-surface-container-highest text-secondary shadow-[0_0_8px_rgba(0,244,254,0.5)]"
                      : "flex h-[80px] min-w-[70px] flex-col items-center justify-center rounded-lg border border-outline-variant/30 bg-surface-container-low text-on-surface-variant transition-colors hover:bg-surface-container"
                  }
                >
                  <span className="text-xs uppercase">{item.day}</span>
                  <span className="font-headline text-2xl font-bold">{item.date}</span>
                  <span className="text-xs">{item.month}</span>
                </button>
              ))}
            </div>
          </div>

          <div className="flex min-w-[300px] flex-col gap-4">
            <div className="flex rounded-lg border border-outline-variant/30 bg-surface-container-low p-1">
              <button
                type="button"
                onClick={() => setGroupMode("cinema")}
                className={`flex-1 rounded px-4 py-2 text-sm font-semibold transition-colors ${groupMode === "cinema" ? "bg-surface-container-highest text-secondary" : "text-on-surface-variant"}`}
              >
                Group theo rạp
              </button>
              <button
                type="button"
                onClick={() => setGroupMode("movie")}
                className={`flex-1 rounded px-4 py-2 text-sm font-semibold transition-colors ${groupMode === "movie" ? "bg-surface-container-highest text-secondary" : "text-on-surface-variant"}`}
              >
                Group theo phim
              </button>
            </div>

            <button
              type="button"
              onClick={() => setIsFilterOpen(true)}
              className="flex items-center justify-center gap-2 rounded-lg border border-outline-variant/40 bg-surface-container-low py-3 font-semibold text-primary transition-colors hover:bg-primary/10"
            >
              <span className="material-symbols-outlined text-[18px]">filter_list</span>
              Filter
            </button>
          </div>
        </section>

        <section className="space-y-10">
          {groupMode === "cinema"
            ? byCinemaData.map((cinema) => (
                <div key={cinema.cinemaName} className="rounded-xl border border-outline-variant/20 bg-surface-container-low p-6 shadow-[0_10px_30px_rgba(0,0,0,0.25)]">
                  <div className="mb-6 flex flex-col gap-2 border-b border-outline-variant/20 pb-4 md:flex-row md:items-end md:justify-between">
                    <div>
                      <h2 className="font-headline text-2xl font-bold tracking-tight text-on-background">{cinema.cinemaName}</h2>
                      <p className="mt-1 flex items-center gap-2 text-sm text-on-surface-variant">
                        <span className="material-symbols-outlined text-[18px] opacity-80">map</span>
                        {cinema.address}
                      </p>
                    </div>
                    <span className="flex w-fit items-center gap-1 rounded bg-secondary/10 px-2 py-1 text-sm font-bold text-secondary">
                      <span className="material-symbols-outlined text-[16px]">theaters</span>
                      {cinema.movies.length} phim
                    </span>
                  </div>

                  <div className="space-y-8">
                    {cinema.movies.map((movie) => (
                      <div key={`${cinema.cinemaName}-${movie.title}`} className="rounded-lg border border-outline-variant/15 bg-surface-container/60 p-4 md:p-5">
                        <div className="flex flex-col gap-4 sm:flex-row sm:items-stretch">
                          <div className="h-40 w-full shrink-0 overflow-hidden rounded-lg bg-surface-container-highest sm:h-auto sm:w-28 md:w-32">
                            <img src={movie.poster} alt={movie.title} className="h-full w-full object-cover" />
                          </div>
                          <div className="min-w-0 flex-1">
                            <h3 className="font-headline text-xl font-bold text-on-background">{movie.title}</h3>
                            <p className="mt-1 text-sm text-on-surface-variant">
                              {movie.genre} · {movie.duration}
                            </p>
                            <div className="mt-4 flex flex-wrap gap-3">
                              {movie.showtimes.map((slot) => (
                                <ShowtimeButton key={`${movie.title}-${slot.time}-${slot.format}`} slot={slot} />
                              ))}
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))
            : byMovieData.map((movie) => (
                <div key={movie.movieTitle} className="rounded-xl border border-outline-variant/20 bg-surface-container-low p-6 shadow-[0_10px_30px_rgba(0,0,0,0.25)]">
                  <div className="mb-6 flex flex-col gap-4 border-b border-outline-variant/20 pb-4 sm:flex-row sm:items-start">
                    <div className="h-44 w-full shrink-0 overflow-hidden rounded-lg bg-surface-container-highest sm:h-36 sm:w-28 md:w-32">
                      <img src={movie.poster} alt={movie.movieTitle} className="h-full w-full object-cover" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <h2 className="font-headline text-2xl font-bold tracking-tight text-on-background">{movie.movieTitle}</h2>
                      <p className="mt-1 text-sm text-on-surface-variant">
                        {movie.genre} · {movie.duration}
                      </p>
                      <p className="mt-2 text-xs text-on-surface-variant">
                        Đang chiếu tại {movie.cinemas.length} rạp
                      </p>
                    </div>
                  </div>

                  <div className="space-y-8">
                    {movie.cinemas.map((c) => (
                      <div key={`${movie.movieTitle}-${c.name}`} className="rounded-lg border border-outline-variant/15 bg-surface-container/60 p-4 md:p-5">
                        <div className="mb-4 flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                          <div>
                            <h4 className="flex items-center gap-2 font-headline text-lg font-semibold text-primary">
                              <span className="material-symbols-outlined text-[20px]">location_city</span>
                              {c.name}
                            </h4>
                            <p className="mt-1 flex items-start gap-2 text-sm text-on-surface-variant">
                              <span className="material-symbols-outlined mt-0.5 shrink-0 text-[18px] opacity-70">map</span>
                              {c.address}
                            </p>
                          </div>
                          <span className="flex w-fit items-center gap-1 rounded bg-secondary/10 px-2 py-1 text-sm font-bold text-secondary">
                            <span className="material-symbols-outlined text-[16px]">near_me</span>
                            {c.distance}
                          </span>
                        </div>
                        <div className="flex flex-wrap gap-3">
                          {c.showtimes.map((slot) => (
                            <ShowtimeButton key={`${c.name}-${slot.time}-${slot.format}`} slot={slot} />
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
        </section>
      </div>

      <Dialog.Root open={isFilterOpen} onOpenChange={setIsFilterOpen}>
        <Dialog.Portal>
          <Dialog.Overlay className="auth-modal-overlay fixed inset-0 z-[90] bg-black/70 backdrop-blur-sm" />
          <Dialog.Content className="auth-modal-content fixed left-1/2 top-1/2 z-[100] w-[95vw] max-w-2xl -translate-x-1/2 -translate-y-1/2 border border-outline-variant/20 bg-surface-container-low p-6 text-on-background shadow-2xl focus:outline-none md:p-8">
            <div className="mb-6 flex items-start justify-between">
              <div>
                <Dialog.Title className="font-headline text-2xl font-bold">Bộ lọc lịch chiếu</Dialog.Title>
                <Dialog.Description className="mt-1 text-sm text-on-surface-variant">Lọc theo rạp, phim đang chiếu và khoảng thời gian.</Dialog.Description>
              </div>
              <Dialog.Close className="text-on-surface-variant hover:text-secondary">
                <span className="material-symbols-outlined">close</span>
              </Dialog.Close>
            </div>

            <div className="grid gap-6 md:grid-cols-2">
              <div>
                <h4 className="mb-3 font-headline text-lg font-bold">Danh sách rạp</h4>
                <div className="space-y-2">
                  {cinemaOptions.map((cinema) => (
                    <label key={cinema} className="flex cursor-pointer items-center gap-3 rounded border border-outline-variant/20 px-3 py-2 hover:bg-surface-variant/40">
                      <input
                        type="checkbox"
                        checked={selectedCinemas.includes(cinema)}
                        onChange={() => toggleSelection(cinema, selectedCinemas, setSelectedCinemas)}
                        className="h-4 w-4 border-outline-variant bg-surface-container-low text-secondary focus:ring-secondary"
                      />
                      <span className="text-sm">{cinema}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div>
                <h4 className="mb-3 font-headline text-lg font-bold">Phim đang chiếu</h4>
                <div className="space-y-2">
                  {movieOptions.map((m) => (
                    <label key={m} className="flex cursor-pointer items-center gap-3 rounded border border-outline-variant/20 px-3 py-2 hover:bg-surface-variant/40">
                      <input
                        type="checkbox"
                        checked={selectedMovies.includes(m)}
                        onChange={() => toggleSelection(m, selectedMovies, setSelectedMovies)}
                        className="h-4 w-4 border-outline-variant bg-surface-container-low text-secondary focus:ring-secondary"
                      />
                      <span className="text-sm">{m}</span>
                    </label>
                  ))}
                </div>
              </div>
            </div>

            <div className="mt-6">
              <h4 className="mb-3 font-headline text-lg font-bold">Khoảng thời gian</h4>
              <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
                {[
                  { id: "all", label: "Tất cả" },
                  { id: "morning", label: "Sáng (08-12)" },
                  { id: "afternoon", label: "Chiều (12-18)" },
                  { id: "evening", label: "Tối (18-24)" },
                ].map((range) => (
                  <button
                    key={range.id}
                    type="button"
                    onClick={() => setTimeRange(range.id)}
                    className={`rounded border px-3 py-2 text-xs font-semibold transition-colors ${timeRange === range.id ? "border-secondary bg-secondary/10 text-secondary" : "border-outline-variant/30 text-on-surface-variant hover:bg-surface-variant/40"}`}
                  >
                    {range.label}
                  </button>
                ))}
              </div>
            </div>

            <div className="mt-8 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => {
                  setSelectedCinemas([])
                  setSelectedMovies([])
                  setTimeRange("all")
                }}
                className="rounded border border-outline-variant/30 px-4 py-2 text-sm text-on-surface-variant transition-colors hover:bg-surface-variant/40"
              >
                Xóa lọc
              </button>
              <Dialog.Close asChild>
                <button
                  type="button"
                  onClick={() => success("Đã áp dụng bộ lọc thành công!")}
                  className="rounded bg-gradient-to-r from-primary to-primary-container px-4 py-2 text-sm font-bold text-on-primary"
                >
                  Áp dụng
                </button>
              </Dialog.Close>
            </div>
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>
    </main>
  )
}

export default Showtimes
