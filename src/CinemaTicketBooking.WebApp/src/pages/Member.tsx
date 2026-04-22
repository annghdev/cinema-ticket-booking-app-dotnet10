function Member() {
  return (
    <main className="min-h-screen bg-background text-on-background">
      <section className="relative flex min-h-[614px] items-center justify-center overflow-hidden pb-32 pt-20">
        <div className="absolute inset-0 z-0">
          <img
            className="h-full w-full object-cover opacity-30"
            alt="Cinematic futuristic theater hall"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuAlQFh-Hbn82aRRABfusv0qZB3ma6gDt6wJVS6MM2kHtrSNGyvOD-pn0zN0kZ_TQSpR1-MQh8W8eULFNtDSXp4kZUqpc_VMr3QpPb2brGDN6jcvV44MX25Ew-i92UZR-WKeyb4a49wrYor74Ewj3_9ue6HnDkthpmx7xqnUfPI0gcy4ekLpFL0m4O1QJOgtjw433DFXEBwKHhE5xvzLRW9_8iWAeoLveQt8kmcdgnmNxJRHQvFHlPNY1OtaUx-g-R42oBcjx__f3lQ"
          />
          <div className="absolute inset-0 bg-gradient-to-b from-background via-transparent to-background" />
        </div>

        <div className="relative z-10 mx-auto flex w-full max-w-screen-2xl flex-col items-center justify-between gap-12 px-8 md:flex-row">
          <div className="max-w-2xl">
            <h1 className="mb-6 font-headline text-5xl font-bold leading-tight tracking-tight md:text-7xl">
              Elevate Your <br />
              <span className="text-secondary drop-shadow-[0_0_8px_rgba(0,244,254,0.5)]">Cinema Journey</span>
            </h1>
            <p className="mb-8 max-w-lg text-lg leading-relaxed text-on-surface-variant">
              Access a world of exclusive screenings, premium comfort, and digital rewards designed for the true auteur of cinema.
            </p>
            <div className="flex gap-4">
              <button
                type="button"
                className="rounded-full bg-gradient-to-r from-primary to-primary-container px-8 py-4 font-bold text-on-primary shadow-[0_0_20px_rgba(0,244,254,0.3)] transition-transform hover:scale-105"
              >
                Upgrade to Platinum
              </button>
              <button
                type="button"
                className="rounded-full border border-outline-variant bg-surface-variant/40 px-8 py-4 font-bold text-primary backdrop-blur-xl transition-colors hover:bg-primary/10"
              >
                View Tier Benefits
              </button>
            </div>
          </div>

          <div className="w-full max-w-md rounded-[2.5rem] border border-outline-variant/20 bg-surface-variant/40 p-1 backdrop-blur-xl">
            <div className="relative aspect-[1.6/1] overflow-hidden rounded-[2rem] bg-gradient-to-br from-surface-container-highest to-surface-container-low p-8">
              <div className="absolute -right-16 -top-16 h-32 w-32 bg-secondary/10 blur-3xl" />
              <div className="relative z-10 flex h-full flex-col justify-between">
                <div className="flex items-start justify-between">
                  <div>
                    <p className="mb-1 font-headline text-xs font-bold uppercase tracking-widest text-secondary">Current Tier</p>
                    <h2 className="font-headline text-3xl font-black text-on-surface">GOLD MEMBER</h2>
                  </div>
                  <span className="material-symbols-outlined text-4xl text-secondary">military_tech</span>
                </div>
                <div>
                  <p className="mb-1 text-xs text-on-surface-variant">Card Holder</p>
                  <p className="font-headline text-lg tracking-wider">ALEXANDER VANCE</p>
                  <div className="mt-4 flex items-end justify-between">
                    <div className="font-headline text-2xl font-black tracking-widest">**** 8842</div>
                    <div className="text-right">
                      <p className="text-[10px] uppercase tracking-widest text-slate-500">Expires</p>
                      <p className="text-xs font-bold">12 / 2025</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="mx-auto mb-32 max-w-screen-2xl px-8">
        <div className="grid items-stretch gap-8 lg:grid-cols-3">
          <div className="rounded-3xl border border-outline-variant/20 bg-surface-variant/40 p-10 backdrop-blur-xl lg:col-span-2">
            <h3 className="mb-2 font-headline text-2xl font-bold">Points Momentum</h3>
            <p className="mb-12 text-sm text-on-surface-variant">Every 1,000 VND spent brings you closer to cinematic luxury.</p>
            <div className="mb-4 flex items-center justify-between">
              <span className="rounded-full bg-secondary/10 px-2 py-1 text-xs font-semibold uppercase text-secondary">Level Progress</span>
              <span className="text-sm font-bold text-secondary">8,450 / 10,000 pts</span>
            </div>
            <div className="mb-4 flex h-3 overflow-hidden rounded-full border border-outline-variant/20 bg-surface-container-high">
              <div className="w-[84.5%] bg-gradient-to-r from-primary to-secondary" />
            </div>
            <p className="text-xs italic text-on-surface-variant">1,550 points remaining until your next free Premium Ticket.</p>
          </div>

          <div className="rounded-3xl border border-primary/20 bg-primary/5 p-8">
            <h3 className="mb-6 font-headline text-xl font-bold">Active Redemption</h3>
            <div className="space-y-4">
              {[
                ["Free Large Combo", "500 PTS"],
                ["30% Ticket Discount", "300 PTS"],
                ["Bday Surprise Gift", "CLAIM"],
              ].map(([name, value]) => (
                <div key={name} className="flex items-center justify-between rounded-2xl border border-outline-variant/20 bg-background/50 p-4">
                  <span className="text-sm font-medium">{name}</span>
                  <span className="text-xs font-bold text-primary">{value}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      <section className="mb-32 bg-surface-container-low py-24">
        <div className="mx-auto max-w-screen-2xl px-8">
          <div className="flex flex-col items-center gap-16 md:flex-row">
            <div className="flex-1">
              <span className="mb-4 block font-headline text-sm font-bold uppercase tracking-[0.3em] text-secondary">Future Auteurs</span>
              <h2 className="mb-6 font-headline text-4xl font-bold md:text-5xl">
                Next-Gen <br />
                Student Pricing
              </h2>
              <p className="mb-8 text-lg leading-relaxed text-on-surface-variant">
                Students get exclusive flat-rate pricing every day of the week.
              </p>
              <button
                type="button"
                className="rounded-full bg-secondary px-10 py-4 text-sm font-black uppercase tracking-widest text-on-secondary transition-all hover:shadow-[0_0_25px_rgba(0,244,254,0.4)]"
              >
                Verify Student ID
              </button>
            </div>
            <div className="relative flex-1">
              <img
                className="relative z-10 h-[500px] w-full rounded-[3rem] object-cover shadow-2xl"
                alt="Students in cinema lobby"
                src="https://lh3.googleusercontent.com/aida-public/AB6AXuDRg5o8-EIqBFBihGPa-o2xVDZROH4Yeu8N7nHXuGY76xI2QaY6Y93lxvf3k_GIV64oEBFUONKBZMsfefitDMm52xBh1thmeOas1M8KW1K6x4_PKXiBnlKUuSwUkhQiTeT0OrXELmxYq447bZBkSHfkwLmP0k8bBOQNdcQn9VyFR4jysiwheFgeaC2b_TFxDrPRAloRPEi8cxHAmUDRu-RKsHfKincxymtT1Q7s4KzSTNi2fBbzz1ZPSHRYy_rYyXxxRVHFtePRBqY"
              />
            </div>
          </div>
        </div>
      </section>
    </main>
  )
}

export default Member
