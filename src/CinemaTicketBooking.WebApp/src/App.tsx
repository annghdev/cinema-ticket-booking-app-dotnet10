import Footer from "./components/Footer"
import Header from "./components/Header"
import Checkout from "./pages/Checkout"
import Home from "./pages/Home"
import Member from "./pages/Member"
import ShowtimeListing from "./pages/ShowtimeListing"
import ShowtimeSeatSelection from "./pages/ShowtimeSeatSelection"
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"

function App() {
  return (
    <BrowserRouter>
      <div className="min-h-screen bg-background text-on-background font-body selection:bg-secondary/30">
        <Header />
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/showtimes" element={<ShowtimeListing />} />
          <Route path="/seat-selection" element={<ShowtimeSeatSelection />} />
          <Route path="/member" element={<Member />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
        <Footer />
      </div>
    </BrowserRouter>
  )
}

export default App
