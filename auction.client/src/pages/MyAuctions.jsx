import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getMyAuctions, cancelAuction } from '../services/auctionService';
import { useAuth } from '../context/AuthContext';
import AuctionList from '../components/auctions/AuctionList';
import Loading from '../components/common/Loading';
import ErrorMessage from '../components/common/ErrorMessage';
import SuccessMessage from '../components/common/SuccessMessage';

const MyAuctions = () => {
  const { user } = useAuth();
  const [auctions, setAuctions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    loadAuctions();
  }, []);

  const loadAuctions = async () => {
    try {
      setLoading(true);
      const data = await getMyAuctions();
      setAuctions(data);
    } catch (err) {
      setError('Failed to load your auctions.');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = async (id) => {
    if (!window.confirm('Are you sure you want to cancel this auction?')) {
      return;
    }
    try {
      await cancelAuction(id);
      setSuccess('Auction cancelled successfully.');
      setError('');
      await loadAuctions();
    } catch (err) {
      setError(err.response?.data?.detail || 'Failed to cancel auction.');
    }
  };

  if (!user || user.role !== 'Seller') {
    return <div className="text-center py-5">Access denied. Seller role required.</div>;
  }

  if (loading) return <Loading />;

  return (
    <div className="container mt-4">
      {error && <ErrorMessage message={error} />}
      {success && <SuccessMessage message={success} />}
      <h2 className="mb-4">My Auctions</h2>
      <AuctionList
        auctions={auctions}
        renderActions={(auction) => auction.status === 'Scheduled' && (
          <div className="d-flex gap-2 mt-2">
            <Link
              to={`/my-auctions/${auction.id}/edit`}
              className="btn btn-outline-primary w-100"
            >
              Edit
            </Link>
            <button
              type="button"
              className="btn btn-outline-danger w-100"
              onClick={() => handleCancel(auction.id)}
            >
              Cancel
            </button>
          </div>
        )}
      />
    </div>
  );
};

export default MyAuctions;
