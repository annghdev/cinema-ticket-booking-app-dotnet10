function ShowtimeListing() {
  const dates = [
    { day: "Today", date: "14", month: "Oct", active: true },
    { day: "Tue", date: "15", month: "Oct" },
    { day: "Wed", date: "16", month: "Oct" },
    { day: "Thu", date: "17", month: "Oct" },
    { day: "Fri", date: "18", month: "Oct" },
    { day: "Sat", date: "19", month: "Oct" },
    { day: "Sun", date: "20", month: "Oct" },
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
        <div className="relative z-10 mx-auto flex h-full max-w-[1920px] flex-col justify-end px-8 pb-12">
          <div className="max-w-3xl">
            <div className="mb-6 inline-flex items-center space-x-2 rounded-full border border-outline-variant/20 bg-surface-variant/40 px-3 py-1 text-sm text-on-surface-variant backdrop-blur-xl">
              <span className="material-symbols-outlined text-[16px]">stars</span>
              <span>Sci-Fi / Action</span>
              <span className="px-2">•</span>
              <span>2h 45m</span>
              <span className="px-2">•</span>
              <span className="rounded border border-outline-variant px-1.5 text-xs">PG-13</span>
            </div>
            <h1 className="mb-4 font-headline text-5xl font-bold leading-tight tracking-tighter text-on-background md:text-7xl">
              ECHOES OF ETERNITY
            </h1>
            <p className="mb-8 max-w-2xl text-lg text-on-surface-variant">
              In the distant future, humanity&apos;s last hope rests on an experimental expedition into the heart of a dying star.
            </p>
          </div>
        </div>
      </section>

      <div className="relative z-20 mx-auto -mt-8 max-w-[1920px] px-8">
        <section className="mb-12 flex flex-col gap-6 rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-4 shadow-2xl backdrop-blur-xl md:flex-row md:items-center md:justify-between">
          <div className="flex-grow overflow-hidden">
            <h3 className="mb-2 text-sm text-on-surface-variant">Select Date</h3>
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
            <h3 className="mb-2 text-sm text-on-surface-variant">Cinema Location</h3>
            <select className="w-full border-b-2 border-outline-variant bg-surface-container-low py-3 pl-3 pr-2 text-on-background focus:border-secondary focus:ring-0">
              <option>Horizon Nexus - Downtown</option>
              <option>Horizon Zenith - Uptown</option>
              <option>Horizon Echo - Suburbia</option>
            </select>
          </div>
        </section>

        <section className="space-y-12">
          <div>
            <div className="mb-6 flex items-center space-x-4">
              <h2 className="font-headline text-3xl font-bold tracking-tight">IMAX 70mm</h2>
              <div className="h-px flex-grow bg-gradient-to-r from-surface-container-high to-transparent" />
            </div>
            <div className="rounded-xl bg-surface-container-low p-6">
              <h4 className="mb-4 flex items-center text-lg font-semibold text-primary">
                <span className="material-symbols-outlined mr-2">theaters</span> Horizon Nexus - Downtown
              </h4>
              <div className="flex flex-wrap gap-4">
                {["14:30", "18:00", "21:15"].map((time) => (
                  <button
                    key={time}
                    type="button"
                    className="min-w-[100px] rounded-lg border border-outline-variant/30 bg-surface-variant/40 px-6 py-3 transition-all hover:border-primary/50"
                  >
                    <p className="font-headline text-xl">{time}</p>
                    <p className="text-xs text-on-surface-variant">{time === "21:15" ? "Few Seats Left" : "Available"}</p>
                  </button>
                ))}
              </div>
            </div>
          </div>
        </section>
      </div>
    </main>
  )
}

export default ShowtimeListing
