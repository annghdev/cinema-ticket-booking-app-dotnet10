import Footer from "./components/Footer"
import Header from "./components/Header"
import BookingHistory from "./pages/BookingHistory"
import Checkout from "./pages/Checkout"
import Home from "./pages/Home"
import Member from "./pages/Member"
import MovieList from "./pages/MovieList"
import MovieWithShowTimes from "./pages/MovieWithShowtimes"
import ShowtimeSeatSelection from "./pages/ShowtimeSeatSelection"
import PaymentResult from "./pages/PaymentResult"
import Showtimes from "./pages/Showtimes"
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"

function App() {
  return (
    <BrowserRouter>
      <div className="min-h-screen bg-background text-on-background font-body selection:bg-secondary/30">
        <Header />
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/movies" element={<MovieList />} />
          <Route path="/movies/:movieId/showtimes" element={<MovieWithShowTimes />} />
          <Route path="/showtimes" element={<Showtimes />} />
          <Route path="/showtimes/:showtimeId/seats" element={<ShowtimeSeatSelection />} />
          <Route path="/checkout" element={<Checkout />} />
          <Route path="/payment-result" element={<PaymentResult />} />
          <Route path="/booking-history" element={<BookingHistory />} />
          <Route path="/member" element={<Member />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
        <Footer />
      </div>
    </BrowserRouter>
  )
}

export default App
