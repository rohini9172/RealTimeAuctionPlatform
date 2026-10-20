import React, { useState, useEffect } from 'react';
import { getWatchlist, addToWatchlist, removeFromWatchlist } from '../services/watchlistService';
import { useAuth } from '../context/AuthContext';
import WatchlistItem from '../components/auctions/WatchlistItem';
import Loading from '../components/common/Loading';
import ErrorMessage from '../components/common/ErrorMessage';

const Watchlist = () => {
  const { user } = useAuth();
  const [watchlist, setWatchlist] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadWatchlist();
  }, []);

  const loadWatchlist = async () => {
    try {
      setLoading(true);
      const data = await getWatchlist();
      setWatchlist(data);
    } catch (err) {
      setError('Failed to load watchlist.');
      console.error('Failed to load watchlist', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAdd = async (auctionId) => {
    try {
      await addToWatchlist(auctionId);
      loadWatchlist();
    } catch (err) {
      setError(err.response?.data?.detail || 'Failed to add to watchlist.');
    }
  };

  const handleRemove = async (auctionId) => {
    try {
      await removeFromWatchlist(auctionId);
      loadWatchlist();
    } catch (err) {
      setError(err.response?.data?.detail || 'Failed to remove from watchlist.');
    }
  };

  if (loading) return <Loading />;

  return (
    <div className="container mt-4">
      {error && <ErrorMessage message={error} />}
      <div className="page-header">
        <h2>My Watchlist</h2>
      </div>
      {watchlist.length === 0 ? (
        <div className="empty-state">
          <i className="bi bi-heart fs-1 mb-3"></i>
          <h4>Your watchlist is empty.</h4>
          <p className="text-muted">
            Browse auctions and add items to your watchlist to track them.
          </p>
          <a href="/" className="btn btn-primary">
            Browse Auctions
          </a>
        </div>
      ) : (
        <div className="auction-grid">
          {watchlist.map((item) => (
            <div key={item.id}>
              <WatchlistItem item={item} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Watchlist;
