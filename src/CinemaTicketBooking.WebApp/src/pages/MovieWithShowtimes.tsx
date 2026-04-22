import { ShowtimeButton, type ShowtimeSlot } from "../components/ShowtimeButton"

function MovieWithShowTimes() {
  const dates = [
    { day: "Hôm nay", date: "14", month: "Oct", active: true },
    { day: "Th 3", date: "15", month: "Oct" },
    { day: "Th 4", date: "16", month: "Oct" },
    { day: "Th 5", date: "17", month: "Oct" },
    { day: "Th 6", date: "18", month: "Oct" },
    { day: "Th 7", date: "19", month: "Oct" },
    { day: "CN", date: "20", month: "Oct" },
  ]

  const reviews = [
    { user: "Trần Minh K.", score: 9.2, comment: "Visual xuất sắc, âm thanh cực kỳ đã ở IMAX. Đoạn cuối rất cảm xúc." },
    { user: "Ngọc Ánh", score: 8.8, comment: "Nội dung tốt, nhịp phim ổn. Rất đáng xem ngoài rạp." },
    { user: "Huy Nguyễn", score: 8.5, comment: "Plot twist ổn, diễn xuất tròn vai. Nên xem suất tối." },
  ]

  const cinemas: {
    name: string
    address: string
    distance: string
    showtimes: ShowtimeSlot[]
  }[] = [
    {
      name: "Horizon Nexus - Downtown",
      address: "1010 Cyber Avenue, Neon District",
      distance: "2.4 km",
      showtimes: [
        { time: "14:30", format: "IMAX 70mm", availability: "Còn vé" },
        { time: "18:00", format: "IMAX 70mm", availability: "Còn vé" },
        { time: "21:15", format: "IMAX 70mm", availability: "Sắp hết vé" },
        { time: "10:00", format: "2D Phụ đề", availability: "Còn vé" },
        { time: "12:45", format: "2D Phụ đề", availability: "Còn vé" },
        { time: "20:30", format: "2D Phụ đề", availability: "Còn vé" },
      ],
    },
    {
      name: "Horizon Zenith - Uptown",
      address: "444 Director's Cut Blvd, Uptown",
      distance: "5.1 km",
      showtimes: [
        { time: "09:40", format: "Dolby Atmos 2D", availability: "Còn vé" },
        { time: "16:20", format: "Dolby Atmos 2D", availability: "Còn vé" },
        { time: "19:00", format: "Dolby Atmos 2D", availability: "Còn vé" },
        { time: "11:15", format: "4DX", availability: "Còn vé" },
        { time: "17:50", format: "4DX", availability: "Còn vé" },
      ],
    },
    {
      name: "Horizon Echo - Suburbia",
      address: "77 Indie Lane, Arts District",
      distance: "8.2 km",
      showtimes: [
        { time: "08:50", format: "2D Lồng tiếng", availability: "Còn vé" },
        { time: "13:10", format: "2D Lồng tiếng", availability: "Còn vé" },
        { time: "22:10", format: "2D Lồng tiếng", availability: "Sắp hết vé" },
      ],
    },
  ]

  return (
    <main className="flex-grow pb-12">
      <section className="relative h-[614px] min-h-[500px] w-full overflow-hidden bg-surface-container-low">
        <div className="absolute inset-0 h-full w-full">
          <img
            alt="Movie background"
            className="h-full w-full object-cover opacity-30"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuCSx01sT-iyNtkZNiRVTpcuOiJ6B1Ir-0f1vamO9K7_4bSjSFvHA_wBAzz8CAXTWzXdFV01PD3mkKiLv9F9ptoBYRDiDXyJ5Ftbdq9U9vyIcizblV6_j5xIBtewyG4g3xYVv82bbsGaL0liwYdf3VU_i7AAdQEMlWuHkIWYYuJ1jDe6OcsOFxAPKbFYL6Wfbrh8clWspc1JkRIw5bw4XRm1tuXYFBoKOuPQVHO2YxNaaxpTZPM144IapHAFazv4tnk_PUBtkV5jPtI"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-background via-background/80 to-transparent" />
        </div>
        <div className="relative z-10 mx-auto flex h-full w-full max-w-screen-2xl flex-col justify-end px-8 pb-12">
          <div className="max-w-4xl">
            <div className="mb-6 inline-flex items-center space-x-2 rounded-full border border-outline-variant/20 bg-surface-variant/40 px-3 py-1 text-sm text-on-surface-variant backdrop-blur-xl">
              <span className="material-symbols-outlined text-[16px]">stars</span>
              <span>Sci-Fi / Action</span>
              <span className="px-2">•</span>
              <span>2h 45m</span>
              <span className="px-2">•</span>
              <span className="rounded border border-outline-variant px-1.5 text-xs">PG-13</span>
            </div>
            <h1 className="mb-4 font-headline text-5xl font-bold leading-tight tracking-tighter text-on-background md:text-7xl">ECHOES OF ETERNITY</h1>
            <p className="mb-8 max-w-3xl text-lg text-on-surface-variant">
              Trong tương lai xa, hy vọng cuối cùng của nhân loại nằm trong chuyến thám hiểm vào lõi của một ngôi sao đang lụi tàn.
            </p>
            <div className="flex flex-wrap gap-4">
              <button type="button" className="flex items-center gap-2 rounded bg-gradient-to-r from-primary to-primary-container px-7 py-3 font-headline font-bold text-on-primary transition-all hover:shadow-[0_0_18px_rgba(97,180,254,0.5)]">
                <span className="material-symbols-outlined text-base">play_arrow</span>
                Xem trailer
              </button>
            </div>
          </div>
        </div>
      </section>

      <div className="relative z-20 mx-auto -mt-8 w-full max-w-screen-2xl space-y-8 px-8">
        <section className="rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-6 shadow-2xl backdrop-blur-xl">
          <h3 className="mb-5 font-headline text-2xl font-bold text-on-background">Thông tin phim</h3>
          <div className="grid gap-4 text-sm text-on-surface-variant md:grid-cols-2 lg:grid-cols-4">
            <p><span className="font-bold text-on-background">Hãng sản xuất:</span> Horizon Studios</p>
            <p><span className="font-bold text-on-background">Đạo diễn:</span> Lena Cross</p>
            <p><span className="font-bold text-on-background">Khởi chiếu:</span> 20/10/2026</p>
            <p><span className="font-bold text-on-background">Quốc gia:</span> USA</p>
          </div>
          <p className="mt-4 leading-relaxed text-on-surface-variant">
            Phim theo chân nhóm phi hành gia thực hiện nhiệm vụ cuối cùng nhằm tái khởi động nguồn năng lượng cho Trái Đất. Khi tiến sâu vào vùng hấp dẫn nguy hiểm, họ buộc phải lựa chọn giữa sự sống và sứ mệnh.
          </p>
        </section>

        <section className="rounded-xl border border-outline-variant/20 bg-surface-container-low p-6">
          <div className="mb-4 flex items-center justify-between">
            <h3 className="font-headline text-2xl font-bold text-on-background">Đánh giá người xem</h3>
            <div className="flex items-center gap-2 rounded bg-secondary/10 px-3 py-1 text-secondary">
              <span className="material-symbols-outlined text-base">grade</span>
              <span className="font-bold">8.8 / 10</span>
            </div>
          </div>
          <div className="grid gap-4 md:grid-cols-3">
            {reviews.map((review) => (
              <article key={review.user} className="rounded-lg border border-outline-variant/20 bg-surface-variant/40 p-4">
                <div className="mb-2 flex items-center justify-between">
                  <p className="font-semibold text-on-background">{review.user}</p>
                  <span className="text-sm font-bold text-secondary">{review.score}</span>
                </div>
                <p className="text-sm text-on-surface-variant">{review.comment}</p>
              </article>
            ))}
          </div>
        </section>

        <section className="mb-4 flex flex-col gap-6 rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-4 shadow-2xl backdrop-blur-xl md:flex-row md:items-center md:justify-between">
          <div className="flex-grow overflow-hidden">
            <h3 className="mb-2 text-sm text-on-surface-variant">Chọn ngày chiếu</h3>
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
          <div className="min-w-[250px]">
            <h3 className="mb-2 text-sm text-on-surface-variant">Rạp chiếu</h3>
            <select className="w-full border-b-2 border-outline-variant bg-surface-container-low py-3 pl-3 pr-2 text-on-background focus:border-secondary focus:ring-0">
              <option>Tất cả</option>
              {cinemas.map((c) => (
                <option key={c.name} value={c.name}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>
        </section>

        <section className="space-y-6">
          {cinemas.map((cinema) => (
            <div key={cinema.name} className="rounded-lg border border-outline-variant/15 bg-surface-container/60 p-4 md:p-5">
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
                <span className="flex w-fit items-center gap-1 rounded bg-secondary/10 px-2 py-1 text-sm font-bold text-secondary">
                  <span className="material-symbols-outlined text-[16px]">near_me</span>
                  {cinema.distance}
                </span>
              </div>
              <div className="flex flex-wrap gap-3">
                {cinema.showtimes.map((slot) => (
                  <ShowtimeButton key={`${cinema.name}-${slot.time}-${slot.format}`} slot={slot} />
                ))}
              </div>
            </div>
          ))}
        </section>
      </div>
    </main>
  )
}

export default MovieWithShowTimes
