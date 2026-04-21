function Home() {
  return (
    <main>
      <section className="relative flex h-screen w-full items-center overflow-hidden">
        <div className="absolute inset-0 z-0">
          <img
            className="h-full w-full object-cover brightness-50 grayscale-[20%]"
            alt="Cinematic atmospheric city background"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuAXFfKSlBIjWgJAXE2TSgP4bQr2F4Uw4bWNSF8ujCRXUkNKzCbCAR3zDdnq_7PRBiOtaIu_yZ3tVPyZZSf_t7yhF7McQ8FCetR3i0d-2hrlWjvCaFpVDa_kuGMPNnbe5ZUmE0izxS2Gbjhqn4pbytGwVCheCOx6NCMTpdH7fT5dxlHbTmWa0GXHAMoPOzbDNwuQBhFil2L4OgKBlOMZu1MH9-AeBpKh0sl7fvz92gPhzC44s_kANtNWI1myHc6X0X-as7Rsc3VHcow"
          />
          <div className="absolute inset-0 bg-gradient-to-r from-background via-background/40 to-transparent" />
          <div className="absolute bottom-0 inset-x-0 h-1/2 bg-gradient-to-t from-background to-transparent" />
        </div>

        <div className="relative z-10 max-w-5xl px-8 md:px-24">
          <span className="mb-6 inline-block border border-secondary/20 bg-secondary/10 px-3 py-1 font-headline text-sm tracking-[0.2em] text-secondary backdrop-blur-sm">
            PREMIERE PHIM MỚI
          </span>
          <h1 className="mb-6 font-headline text-6xl font-black leading-none tracking-tighter text-white md:text-8xl">
            CHÂN TRỜI <br />
            <span className="text-primary drop-shadow-[0_0_15px_rgba(97,180,254,0.4)]">VÔ TẬN</span>
          </h1>
          <p className="mb-10 max-w-2xl text-lg font-light leading-relaxed text-slate-400 md:text-xl">
            Trải nghiệm đỉnh cao của điện ảnh với hệ thống âm thanh vòm thế hệ mới và hình ảnh sắc nét đến từng chi tiết tại hệ
            thống rạp HORIZON.
          </p>
          <div className="flex flex-wrap gap-4">
            <button
              type="button"
              className="flex items-center gap-2 bg-gradient-to-br from-primary to-primary-container px-8 py-4 font-headline font-bold tracking-wide text-on-primary transition-all hover:shadow-[0_0_25px_rgba(97,180,254,0.6)] active:scale-95"
            >
              <span>ĐẶT VÉ NGAY</span>
              <span className="material-symbols-outlined">confirmation_number</span>
            </button>
            <button
              type="button"
              className="border border-outline-variant/30 bg-surface-variant/40 px-8 py-4 font-headline font-bold tracking-wide text-white backdrop-blur-md transition-all hover:bg-surface-variant/60 active:scale-95"
            >
              XEM TRAILER
            </button>
          </div>
        </div>
      </section>

      <section className="bg-surface py-24">
        <div className="mx-auto max-w-screen-2xl px-8">
          <div className="mb-12 flex items-end justify-between border-l-4 border-secondary pl-6">
            <div>
              <h2 className="font-headline text-4xl font-black uppercase tracking-tighter text-white">Phim Đang Chiếu</h2>
              <p className="font-medium text-slate-500">Những siêu phẩm điện ảnh không thể bỏ lỡ tuần này</p>
            </div>
            <a className="group flex items-center gap-2 font-headline font-bold text-secondary" href="#">
              XEM TẤT CẢ
              <span className="material-symbols-outlined transition-transform group-hover:translate-x-1">arrow_forward</span>
            </a>
          </div>

          <div className="grid grid-cols-1 gap-0 md:grid-cols-2 lg:grid-cols-4">
            {[
              {
                title: "BÓNG MA SỐ",
                genre: "HÀNH ĐỘNG / SCI-FI",
                tag: "IMAX 2D",
                image:
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuB441TQ4zaDFJaVwak7wkoqPlYKOO9HCCAbnxwIsXxRjDLSBMppuUIKptbYkswlfezxO0m0MGLvEv3Pnin31QegTK_QqhEUISfxgtNf6cD_pAAvfyAZpafp-QUBqUIJ-TTZgGUFeWFsVcbD9sv96Oid1VIAKbWbwFkGxoZH_dxcf8YZO2IwVZt6YZTKbR1_o_S2yhsJHo4qf5yPUfF0lBuM990VFQdIRASdo5Pa0wBE9N_I-v3gw3m-JBFGHMhz1yXZ6AsqC-SQeG8",
              },
              {
                title: "THIÊN HÀ RỰC CHÁY",
                genre: "PHIÊU LƯU / KỲ ẢO",
                tag: "3D SURROUND",
                image:
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuBQu8WkkrvsAmbKuM3Hy78ae7t6EUjRQcOrSme3BjDUnqMrJC0Evtw-2pDoRE-sJSqJZ2PlTQm8b9FzzvLA-_azF0cHrcT8sirpwIWad3LeHnB7_IM4vTU-QeTVwH5DY5kd8-DkFCqsZAs2ZWrJDk31TXiXWAatcntjQFatbEwSWe_Dd6pswk8-9P9gx04R3UEa8E_1VCEoUXMQ8gQhJpfn-jQ0KXI6AXr_SL3cui8Yuj2qc3iIvpQtsYxgR-SljKv_XNObklvOpCE",
              },
              {
                title: "GIAI ĐIỆU CUỐI",
                genre: "DRAMA / TÂM LÝ",
                tag: "HORIZON EXCLUSIVE",
                image:
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuBWeUkVRXpMKvJY4QjySG_Ma03jITEA8D7EVwlKw3ob7wa0YyfUl-5MLS_0r_WKQDo-3HN8Ll-BNtDzYekGBYOM58BVeLR65DL-FhG2lfDEA2BiWAd3cmKvoSaGX25669p__lXz5FQYi3BuijjBTmWbKvT8R4XoEYLRD8UZUxAwy0rEYTbc6YyGUWEIh2U-M0BigGZyxRahJVBV-IjdLqK3INFFVU1iEnBGOTAFwsntOopbT7q86yxHRecRuxEgFMsOyYJB-Yj7byI",
              },
              {
                title: "TRÍ TUỆ NHÂN TẠO",
                genre: "THRILLER / KINH DỊ",
                tag: "PREMIUM",
                image:
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuCfZN0pSVYsdGxwrIHVKnehXbLkaXStGazsbOdCyCY-mvW3bqr15nB_lsHKPs2M23YWF8k1FvvHe2qm7edn-WUhW5GtQDU40euLrsQeqZHCHG8PmIago2hr7KmPOJDnOaZNfdwX1531aDD5gkPF2m01IxUZP5e54wqRJ9NFwZjysVffF8lh7ZyD7aNje1M4Bqxj9RHBM6XQ74K7BBnQPDXOObb7DooZcD-uGv6prNMF1GRVs8P-ja83GLQZ-XLn-F3vmuAxlis-Voc",
              },
            ].map((movie) => (
              <div
                key={movie.title}
                className="group relative aspect-[2/3] overflow-hidden border border-outline-variant/10 bg-surface-container"
              >
                <img
                  className="h-full w-full object-cover transition-transform duration-700 group-hover:scale-110"
                  alt={movie.title}
                  src={movie.image}
                />
                <div className="absolute right-4 top-4 z-20">
                  <span className="bg-surface-variant/60 px-3 py-1 font-headline text-xs font-bold uppercase tracking-widest text-secondary backdrop-blur-md">
                    {movie.tag}
                  </span>
                </div>
                <div className="absolute inset-0 bg-gradient-to-t from-background via-transparent to-transparent opacity-90 transition-opacity group-hover:opacity-100" />
                <div className="absolute bottom-0 w-full p-8">
                  <p className="mb-2 text-xs font-bold tracking-widest text-secondary">{movie.genre}</p>
                  <h3 className="mb-4 font-headline text-2xl font-black leading-none text-white">{movie.title}</h3>
                  <button
                    type="button"
                    className="w-full translate-y-12 bg-secondary py-3 font-black text-on-secondary opacity-0 transition-all duration-300 group-hover:translate-y-0 group-hover:opacity-100"
                  >
                    ĐẶT VÉ
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="bg-background py-24">
        <div className="mx-auto max-w-screen-2xl px-8">
          <div className="mb-16 flex flex-col items-center justify-between gap-4 md:flex-row">
            <h2 className="font-headline text-5xl font-black tracking-tighter text-white">SẮP KHỞI CHIẾU</h2>
            <div className="mx-8 hidden h-[2px] flex-grow bg-gradient-to-r from-secondary/50 to-transparent md:block" />
            <div className="flex gap-2">
              <button
                type="button"
                className="border border-outline-variant/30 p-2 text-outline transition-all hover:border-secondary hover:text-secondary"
              >
                <span className="material-symbols-outlined">chevron_left</span>
              </button>
              <button
                type="button"
                className="border border-outline-variant/30 p-2 text-outline transition-all hover:border-secondary hover:text-secondary"
              >
                <span className="material-symbols-outlined">chevron_right</span>
              </button>
            </div>
          </div>

          <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
            {[
              {
                date: "15.12.2024",
                title: "SAO HOẢ: NGÀY CUỐI",
                description: "Nỗ lực sinh tồn cuối cùng của nhân loại trên hành tinh đỏ đầy khắc nghiệt.",
                image:
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuAFLlZMs4cmfZw9seSCEaSKYcRsOZ0VF-iWKDEAtJfPQmbX99dd-pqQUh_pKhFbMhpGsW-TfqraOLgHR8_EZXgM7UdymXibECEEY9n2CvXSoTazsXYtSzJqHNm-q_8mWYOeNV4T3gT8R9vNZuFoif8LvaSF4BteX4myOVvkHKMGTUzf12SJU1nFEM4pG6tNBDk7yqlKFdrDidYYP4sLZlCN8Nr31eR2rQlnM4Ilhe56SYUOrF_VuVBFlN0y280MGvVCavG_f6DkT7k",
              },
              {
                date: "20.12.2024",
                title: "THÀNH PHỐ KHÔNG NGỦ",
                description: "Cuộc truy đuổi nghẹt thở trong thế giới ngầm của những kẻ đứng ngoài vòng pháp luật.",
                image:
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuChPcnpDT9D5r8ayBNHiBcGvHtN4mixoGqViVDxjNJwWgPgaa2y9NqNMo_w9DEz_8nzPq1RYg56OBGH2v3ALleNTx2zh57HTMkG2AIPLCPTcx4s8R9_838hIejqfUn0sMwfrTPgBIGUyTdwospGUe5l1YnvEil2hMYszoGxM9AjMdNf7Uf7NVlMKF-Or8ytthqliP-92N1VfngE8TFrGRU8XOlyB3BpYP9ZxO5KvhlU1VGw7pvqqZZ3KjMKbec9p0a_E39v0Wx7WWY",
              },
              {
                date: "01.01.2025",
                title: "ĐỈNH NÚI VÔ CỰC",
                description: "Hành trình khám phá bí ẩn vĩnh hằng ẩn sau những rặng núi tuyết trắng.",
                image:
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuCV3ne4VZCCEpHkgfwlLsyeQ5VtMl4wIPJ-Okss_UpyfPVL3YegJk7tx0iQRx1IQis3VsB5S8M036_RL3ljIF985F_OQqJK5loeaMNpkp8Y4zHSkoN48cBdor55pf-smuslXHiRW7BiDxfgy4Jnp2r5g3WUKm-aDQejnjFMf6GkvzwPIvRA2kDHkeJmTkmPLhg3JkeVfdg5w_0p1R-RWebdvsYTk4MofdqwP4c0CGzW04ANaXQUn_EPAvW9jx0I-FOu6e0W7vOVRpk",
              },
            ].map((movie) => (
              <div key={movie.title} className="group flex gap-6">
                <div className="aspect-[3/4] w-1/3 flex-shrink-0 overflow-hidden">
                  <img className="h-full w-full object-cover transition-transform group-hover:scale-105" alt={movie.title} src={movie.image} />
                </div>
                <div className="flex flex-col justify-center">
                  <span className="mb-1 font-headline text-sm font-black tracking-widest text-primary">{movie.date}</span>
                  <h4 className="mb-3 font-headline text-xl font-bold text-white transition-colors group-hover:text-secondary">{movie.title}</h4>
                  <p className="mb-5 text-sm text-slate-500">{movie.description}</p>
                  <button type="button" className="flex items-center gap-2 font-headline text-sm font-bold text-white transition-all hover:text-secondary">
                    <span className="material-symbols-outlined text-sm">notifications</span> NHẮC TÔI
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-screen-2xl px-8 py-24">
        <div className="relative overflow-hidden border border-outline-variant/10 bg-surface-container-high p-12 md:p-24">
          <div className="pointer-events-none absolute right-0 top-0 h-full w-1/2 opacity-30">
            <div className="absolute inset-0 bg-gradient-to-l from-secondary/20 to-transparent" />
          </div>
          <div className="relative z-10 max-w-2xl">
            <h2 className="mb-6 font-headline text-4xl font-black tracking-tighter text-white md:text-5xl">
              TRỞ THÀNH HỘI VIÊN <span className="text-secondary">VIP</span>
            </h2>
            <p className="mb-10 text-lg leading-relaxed text-slate-400">
              Nhận ngay ưu đãi giảm giá 50% cho vé xem phim đầu tiên và tích điểm đổi quà không giới hạn.
            </p>
            <div className="flex flex-col gap-4 md:flex-row">
              <input
                type="tel"
                placeholder="Số điện thoại của bạn"
                className="flex-grow border-0 border-b-2 border-outline-variant bg-surface-container-low px-2 py-4 font-medium text-white placeholder:text-slate-600 focus:border-secondary focus:ring-0"
              />
              <button
                type="button"
                className="bg-secondary px-10 py-4 font-headline font-black text-on-secondary transition-all hover:shadow-[0_0_20px_rgba(0,244,254,0.4)]"
              >
                TƯ VẤN NGAY
              </button>
            </div>
          </div>
        </div>
      </section>
    </main>
  )
}

export default Home
