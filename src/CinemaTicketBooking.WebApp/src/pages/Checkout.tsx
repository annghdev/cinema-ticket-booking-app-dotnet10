import { Link, useSearchParams } from "react-router-dom"

function Checkout() {
  const [searchParams] = useSearchParams()
  const showtimeId = searchParams.get("showtimeId")
  const backToSeatSelectionPath = showtimeId ? `/showtimes/${showtimeId}/seats` : "/showtimes"

  return (
    <main className="mx-auto grid w-full max-w-screen-2xl grid-cols-1 gap-8 px-8 pb-12 pt-24 md:pt-28 lg:grid-cols-12 lg:gap-12">
      <section className="flex flex-col gap-10 lg:col-span-8">
        <div>
          <Link to={backToSeatSelectionPath} className="group mb-4 flex items-center text-sm text-on-surface-variant transition-colors hover:text-secondary">
            <span className="material-symbols-outlined mr-1 text-lg transition-transform group-hover:-translate-x-1">arrow_back</span>
            Quay lại đặt ghế
          </Link>
          <h1 className="mb-2 font-headline text-4xl font-bold tracking-tight md:text-5xl">Thanh toán</h1>
          <p className="text-on-surface-variant">Hoàn tất đặt vé để có trải nghiệm điện ảnh khó quên.</p>
        </div>

        <section className="rounded-xl bg-surface-container-low p-6 md:p-8">
          <h2 className="mb-6 flex items-center gap-3 font-headline text-2xl font-semibold">
            <span className="material-symbols-outlined text-secondary">fastfood</span>
            Đồ ăn &amp; Nước uống
          </h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {[
              { name: "Hộp bắp lớn", price: "79.000đ", qty: 1 },
              { name: "Ly coca lớn", price: "35.000đ", qty: 2 },
            ].map((item) => (
              <div key={item.name} className="flex items-center gap-4 rounded-lg border border-outline-variant/20 bg-surface-container-highest p-4">
                <div className="h-20 w-20 rounded-md bg-surface-dim" />
                <div>
                  <h3 className="font-headline text-lg font-medium">{item.name}</h3>
                  <p className="mb-2 text-sm font-bold text-secondary">{item.price}</p>
                  <p className="text-sm text-on-surface-variant">Số lượng: {item.qty}</p>
                </div>
              </div>
            ))}
          </div>
        </section>

        <section className="rounded-xl bg-surface-container-low p-6 md:p-8">
          <h2 className="mb-6 flex items-center gap-3 font-headline text-2xl font-semibold">
            <span className="material-symbols-outlined text-secondary">payment</span>
            Payment Method
          </h2>
          <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <button type="button" className="rounded-xl border border-secondary bg-surface-container-highest p-5 text-left">
              <span className="material-symbols-outlined mb-3 block text-3xl">credit_card</span>
              <h3 className="font-headline font-medium">Thẻ tín dụng</h3>
              <p className="text-xs text-on-surface-variant">Visa, Mastercard, Amex</p>
            </button>
            <button type="button" className="rounded-xl border border-outline-variant/20 bg-surface-container-highest p-5 text-left">
              <span className="material-symbols-outlined mb-3 block text-3xl text-on-surface-variant">account_balance_wallet</span>
              <h3 className="font-headline font-medium">Ví điện tử</h3>
              <p className="text-xs text-on-surface-variant">Momo, VnPay, ZaloPay</p>
            </button>
          </div>
          <div className="space-y-4">
            <input placeholder="Name on Card" className="w-full border-0 border-b-2 border-outline-variant bg-transparent px-0 py-2 focus:border-secondary focus:ring-0" />
            <input placeholder="Card Number" className="w-full border-0 border-b-2 border-outline-variant bg-transparent px-0 py-2 focus:border-secondary focus:ring-0" />
          </div>
        </section>
      </section>

      <aside className="lg:col-span-4">
        <div className="rounded-sm border border-outline-variant/10 bg-surface-container-highest p-6 shadow-[0_20px_40px_rgba(0,0,0,0.4)] lg:sticky lg:top-28">
          <h3 className="mb-1 font-headline text-2xl font-bold">Neon Genesis</h3>
          <p className="mb-4 text-sm text-on-surface-variant">Hôm nay, 8:30 PM • Phòng 4</p>
          <div className="mb-6 text-sm">
            <p>
              Ghế: <span className="font-bold text-secondary">F12, F13</span>
            </p>
          </div>
          <div className="space-y-2 border-t border-outline-variant/20 pt-4 text-sm">
            <div className="flex justify-between"><span className="text-on-surface-variant">Vé (2x)</span><span>290.000 đ</span></div>
            <div className="flex justify-between"><span className="text-on-surface-variant">Bắp + nước</span><span>149.000 đ</span></div>
          </div>
          <div className="mt-6 flex items-end justify-between border-t border-outline-variant/20 pt-4">
            <span className="font-headline text-lg text-on-surface-variant">Tổng cộng</span>
            <span className="font-headline text-3xl font-bold text-secondary">439.000 đ</span>
          </div>
          <button type="button" className="mt-6 flex w-full items-center justify-center gap-2 bg-gradient-to-br from-primary to-primary-container py-4 font-headline font-bold text-on-primary transition-all hover:shadow-[0_0_20px_rgba(0,244,254,0.6)]">
            <span className="material-symbols-outlined">lock</span>
            Thanh toán 439.000 đ
          </button>
        </div>
      </aside>
    </main>
  )
}

export default Checkout
