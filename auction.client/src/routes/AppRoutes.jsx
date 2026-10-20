import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from '../context/AuthContext';
import Navbar from '../components/layout/Navbar';
import PrivateRoute from '../components/common/PrivateRoute';
import Login from '../pages/Login';
import Register from '../pages/Register';
import ForgotPassword from '../pages/ForgotPassword';
import CreateCategory from '../pages/CreateCategory';
import AuctionListing from '../pages/AuctionListing';
import AuctionDetails from '../pages/AuctionDetails';
import CreateAuction from '../pages/CreateAuction';
import MyAuctions from '../pages/MyAuctions';
import EditAuction from '../pages/EditAuction';
import Watchlist from '../pages/Watchlist';
import Notifications from '../pages/Notifications';
import NotFound from '../pages/NotFound';

const AppRoutes = () => {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Navbar />
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route
            path="/create-category"
            element={
              <PrivateRoute allowedRoles={['Admin']}>
                <CreateCategory />
              </PrivateRoute>
            }
          />
          <Route path="/" element={<AuctionListing />} />
          <Route path="/auction/:id" element={<AuctionDetails />} />
          <Route
            path="/create-auction"
            element={
              <PrivateRoute allowedRoles={['Seller']}>
                <CreateAuction />
              </PrivateRoute>
            }
          />
          <Route
            path="/my-auctions"
            element={
              <PrivateRoute allowedRoles={['Seller']}>
                <MyAuctions />
              </PrivateRoute>
            }
          />
          <Route
            path="/my-auctions/:id/edit"
            element={
              <PrivateRoute allowedRoles={['Seller']}>
                <EditAuction />
              </PrivateRoute>
            }
          />
          <Route
            path="/watchlist"
            element={<PrivateRoute><Watchlist /></PrivateRoute>}
          />
          <Route
            path="/notifications"
            element={<PrivateRoute><Notifications /></PrivateRoute>}
          />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};

export default AppRoutes;
