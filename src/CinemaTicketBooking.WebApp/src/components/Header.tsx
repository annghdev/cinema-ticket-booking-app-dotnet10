import { NavLink } from "react-router-dom"

const navItems = [
  { label: "Trang chủ", to: "/" },
  { label: "Lịch chiếu", to: "/showtimes" },
  { label: "Chọn ghế", to: "/seat-selection" },
  { label: "Hội viên", to: "/member" },
]

function Header() {
  return (
    <header className="fixed top-0 z-50 w-full bg-[#0b0e14]/40 shadow-2xl shadow-black/50 backdrop-blur-md">
      <div className="mx-auto flex w-full max-w-screen-2xl items-center justify-between px-8 py-4">
        <div className="font-headline text-2xl font-black tracking-tighter text-[#61b4fe]">
          ENDLESS HORIZON
        </div>

        <nav className="hidden items-center space-x-8 font-headline font-bold tracking-tight md:flex">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                isActive
                  ? "active:scale-95 border-b-2 border-[#00f4fe] pb-1 text-[#00f4fe] drop-shadow-[0_0_8px_rgba(0,244,254,0.6)] transition-all duration-300 ease-out"
                  : "px-2 py-1 text-slate-400 transition-all hover:text-[#00f4fe] hover:drop-shadow-[0_0_5px_rgba(0,244,254,0.4)] active:scale-95"
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="flex items-center gap-4">
          <button
            type="button"
            aria-label="Tìm kiếm"
            className="material-symbols-outlined text-slate-400 transition-all hover:text-[#00f4fe]"
          >
            search
          </button>
          <button
            type="button"
            aria-label="Tài khoản"
            className="material-symbols-outlined text-[#61b4fe]"
          >
            account_circle
          </button>
        </div>
      </div>
    </header>
  )
}

export default Header
