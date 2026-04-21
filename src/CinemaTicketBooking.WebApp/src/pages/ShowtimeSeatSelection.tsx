import { Link } from "react-router-dom"

function ShowtimeSeatSelection() {
  return (
    <main className="flex min-h-screen flex-col bg-background text-on-background md:flex-row">
      <section className="relative flex-1 overflow-y-auto bg-background md:w-[70%] lg:w-[75%]">
        <div className="mx-auto mt-12 flex w-full max-w-4xl flex-col items-center px-8">
          <div className="relative flex h-24 w-full items-center justify-center overflow-hidden rounded-t-[50%] border-t-4 border-secondary bg-gradient-to-t from-transparent to-surface-container-high/50 shadow-[0_-15px_40px_rgba(0,244,254,0.3)] sm:h-32 sm:rounded-t-full">
            <span className="mt-4 font-headline text-sm uppercase tracking-widest text-on-surface-variant">MÀN HÌNH</span>
          </div>
        </div>

        <div className="mx-auto flex max-w-5xl flex-col items-center justify-center px-4 pb-32 pt-10 sm:pb-8">
          <div className="grid grid-cols-8 gap-3">
            {Array.from({ length: 32 }).map((_, index) => {
              const number = index + 1
              const sold = [1, 6, 10, 13].includes(number)
              const selected = [3, 4].includes(number)
              return (
                <button
                  key={number}
                  type="button"
                  disabled={sold}
                  className={
                    sold
                      ? "h-10 w-10 cursor-not-allowed rounded-lg bg-surface-dim text-on-surface-variant opacity-30"
                      : selected
                        ? "h-10 w-10 rounded-lg border border-secondary bg-secondary text-on-primary shadow-[0_0_8px_rgba(0,244,254,0.5)]"
                        : "h-10 w-10 rounded-lg border border-outline-variant/20 bg-surface-container-highest text-on-background transition-colors hover:bg-primary/10 hover:text-primary"
                  }
                >
                  {sold ? <span className="material-symbols-outlined text-sm">close</span> : number}
                </button>
              )
            })}
          </div>
        </div>
      </section>

      <aside className="fixed bottom-0 z-40 w-full border-t border-outline-variant/20 bg-surface-container-low p-6 shadow-[0_-20px_40px_rgba(0,0,0,0.5)] md:relative md:h-screen md:w-[30%] md:border-l md:border-t-0 lg:w-[25%]">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="font-headline text-xl font-bold">Đã chọn</h2>
          <span className="border border-outline-variant/20 bg-surface-container-highest px-2 py-1 text-xs text-primary">2 Vé</span>
        </div>
        <div className="mb-6 space-y-3">
          <div className="rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-4">
            <div className="flex items-center justify-between">
              <span>A3</span>
              <span className="font-headline text-primary">145.000 đ</span>
            </div>
          </div>
          <div className="rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-4">
            <div className="flex items-center justify-between">
              <span>A4</span>
              <span className="font-headline text-primary">145.000 đ</span>
            </div>
          </div>
        </div>
        <div className="flex items-end justify-between pb-3">
          <div>
            <p className="text-sm text-on-surface-variant">Tổng tiền</p>
            <p className="font-headline text-3xl font-bold">290.000 đ</p>
          </div>
        </div>
        <Link
          to="/checkout"
          className="flex w-full items-center justify-center gap-2 bg-gradient-to-br from-primary to-primary-container py-4 font-headline font-bold text-on-primary transition-all hover:shadow-[0_0_20px_rgba(0,244,254,0.4)]"
        >
          Tiếp tục thanh toán
          <span className="material-symbols-outlined">arrow_forward</span>
        </Link>
      </aside>
    </main>
  )
}

export default ShowtimeSeatSelection
