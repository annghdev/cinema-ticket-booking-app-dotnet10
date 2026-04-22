import { NavLink } from "react-router-dom"
import { useState } from "react"
import AuthModal from "./AuthModal"

const navItems = [
  { label: "Trang chủ", to: "/" },
  { label: "Phim", to: "/movies" },
  { label: "Lịch chiếu", to: "/showtimes" },
  { label: "Ưu đãi", to: "/promos" },
  { label: "Hội viên", to: "/member" },
]

function Header() {
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false)

  return (
    <>
      <header className="fixed top-0 z-50 w-full border-b border-outline-variant/20 bg-background/95 shadow-lg shadow-black/20 backdrop-blur-md">
        <div className="mx-auto flex w-full max-w-screen-2xl items-center justify-between px-8 py-4">
          <div className="font-headline text-2xl font-black tracking-tighter text-[#61b4fe]">
            ANDROMEDA CINEMA
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
              aria-label="Mở đăng nhập"
              onClick={() => setIsAuthModalOpen(true)}
              className="material-symbols-outlined text-[#61b4fe] transition-all hover:text-[#00f4fe]"
            >
              account_circle
            </button>
          </div>
        </div>
      </header>

      <AuthModal open={isAuthModalOpen} onOpenChange={setIsAuthModalOpen} />
    </>
  )
}

export default Header
