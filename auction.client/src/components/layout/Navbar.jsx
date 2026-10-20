import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { logout } from '../../services/authService';

const Navbar = () => {
  const { user, logout: handleLogout } = useAuth();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const navigate = useNavigate();

  const handleLogoutClick = (e) => {
    e.preventDefault();
    handleLogout();
    navigate('/login');
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
      <div className="container">
        <Link className="navbar-brand" to="/">
          RealTime Auction
        </Link>
        <button
          className="navbar-toggler"
          type="button"
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
        >
          <span className="navbar-toggler-icon"></span>
        </button>
        <div
          className={`collapse navbar-collapse ${mobileMenuOpen ? 'show' : ''}`}
          id="navbarNav"
        >
          <ul className="navbar-nav me-auto">
            <li className="nav-item">
              <Link className="nav-link" to="/">
                Auctions
              </Link>
            </li>
            {user && user.role === 'Seller' && (
              <li className="nav-item">
                <Link className="nav-link" to="/my-auctions">
                  My Auctions
                </Link>
              </li>
            )}
            {user && user.role === 'Seller' && (
              <li className="nav-item">
                <Link className="nav-link" to="/create-auction">
                  Create Auction
                </Link>
              </li>
            )}
            {user && user.role === 'Admin' && (
              <li className="nav-item">
                <Link className="nav-link" to="/create-category">
                  Create Category
                </Link>
              </li>
            )}
            <li className="nav-item">
              <Link className="nav-link" to="/watchlist">
                Watchlist
              </Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/notifications">
                Notifications
              </Link>
            </li>
          </ul>
          <ul className="navbar-nav">
            {user ? (
              <>
                <li className="nav-item">
                  <span className="nav-link text-light">
                    {user.fullName} ({user.role})
                  </span>
                </li>
                <li className="nav-item">
                  <button
                    className="nav-link text-danger bg-transparent border-0 p-0"
                    onClick={handleLogoutClick}
                  >
                    Logout
                  </button>
                </li>
              </>
            ) : (
              <>
                <li className="nav-item">
                  <Link className="nav-link" to="/login">
                    Login
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/register">
                    Register
                  </Link>
                </li>
              </>
            )}
          </ul>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
