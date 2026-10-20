import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  getAuctionById,
  getAuctionImageUrl,
  parseAuctionDate,
} from '../services/auctionService';
import { placeBid, getAuctionBids } from '../services/bidService';
import { addToWatchlist, removeFromWatchlist, getWatchlist } from '../services/watchlistService';
import { createConnection, joinAuctionGroup, startConnection, stopConnection } from '../services/signalRService';
import BidForm from '../components/auctions/BidForm';
import BidHistory from '../components/auctions/BidHistory';
import Loading from '../components/common/Loading';
import ErrorMessage from '../components/common/ErrorMessage';
import SuccessMessage from '../components/common/SuccessMessage';
import { useAuth } from '../context/AuthContext';

const AuctionDetails = () => {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [auction, setAuction] = useState(null);
  const [bids, setBids] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [isInWatchlist, setIsInWatchlist] = useState(false);
  const [bidLoading, setBidLoading] = useState(false);

  useEffect(() => {
    loadAuction();
    loadBids();

    if (!user) return undefined;

    const connection = createConnection(
      parseInt(id),
      (bidUpdate) => {
        setBids((prev) => [bidUpdate, ...prev]);
        setAuction((prev) =>
          prev ? { ...prev, currentPrice: bidUpdate.amount } : prev
        );
      },
      () => {
        setSuccess('Auction has ended!');
      }
    );

    let active = true;
    startConnection(connection)
      .then(() => active && joinAuctionGroup(connection, parseInt(id)))
      .catch((err) => console.error('Failed to connect to auction updates', err));

    return () => {
      active = false;
      stopConnection(connection).catch((err) =>
        console.error('Failed to stop auction updates', err)
      );
    };
  }, [id, user]);

  const loadAuction = async () => {
    try {
      const data = await getAuctionById(parseInt(id));
      setAuction(data);
      // Check if this auction is in the user's watchlist
      if (user) {
        try {
          const watchlist = await getWatchlist();
          setIsInWatchlist(watchlist.some((w) => w.auctionId === data.id));
        } catch (err) {
          console.error('Failed to check watchlist status', err);
        }
      }
    } catch (err) {
      setError('Failed to load auction details.');
    } finally {
      setLoading(false);
    }
  };

  const handleToggleWatchlist = async () => {
    if (!user) return;
    try {
      if (isInWatchlist) {
        await removeFromWatchlist(parseInt(id));
        setIsInWatchlist(false);
        setSuccess('Removed from watchlist.');
      } else {
        await addToWatchlist(parseInt(id));
        setIsInWatchlist(true);
        setSuccess('Added to watchlist.');
      }
    } catch (err) {
      setError(err.response?.data?.detail || 'Failed to update watchlist.');
    }
  };

  const loadBids = async () => {
    try {
      const data = await getAuctionBids(parseInt(id));
      setBids(data);
    } catch (err) {
      console.error('Failed to load bids', err);
    }
  };

  const handlePlaceBid = async (amount) => {
    setError('');
    setSuccess('');
    setBidLoading(true);
    try {
      await placeBid(parseInt(id), amount);
      setSuccess('Bid placed successfully!');
      await Promise.all([loadAuction(), loadBids()]);
    } catch (err) {
      setError(
        err.response?.data?.detail ||
          Object.values(err.response?.data?.errors || {}).flat().join(' ') ||
          err.response?.data?.title ||
          'Failed to place bid.'
      );
    } finally {
      setBidLoading(false);
    }
  };

  if (loading) return <Loading />;

  return (
    <div className="container mt-4">
      {error && <ErrorMessage message={error} />}
      {success && <SuccessMessage message={success} />}
      {auction ? (
        <div className="row">
          <div className="col-md-8">
            <h2>{auction.title}</h2>
            {auction.imageUrls?.length > 0 && (
              <div className="d-flex flex-wrap gap-2 mb-3">
                {auction.imageUrls.map((imageUrl) => (
                  <img
                    key={imageUrl}
                    src={getAuctionImageUrl(imageUrl)}
                    alt={auction.title}
                    className="img-fluid rounded"
                    style={{ maxHeight: '320px', objectFit: 'contain' }}
                  />
                ))}
              </div>
            )}
            <p className="text-muted">{auction.description}</p>
            <div className="mb-3">
              <span className="badge bg-primary fs-6">
                Status: {auction.status}
              </span>{' '}
              <span className="badge bg-info fs-6">
                Category: {auction.categoryName}
              </span>
            </div>
            {user && (
              <button
                className={`btn btn-${isInWatchlist ? 'outline-danger' : 'outline-primary'} w-100 mb-3`}
                onClick={handleToggleWatchlist}
              >
                {isInWatchlist ? (
                  <>
                    <i className="bi bi-heart-fill me-1"></i> Remove from Watchlist
                  </>
                ) : (
                  <>
                    <i className="bi bi-heart me-1"></i> Add to Watchlist
                  </>
                )}
              </button>
            )}
            <div className="row mb-4">
              <div className="col-md-6">
                <h4>Current Price: ₹{auction.currentPrice.toFixed(2)}</h4>
              </div>
              <div className="col-md-6">
                <p className="text-muted">
                  Ends: {parseAuctionDate(auction.endTime).toLocaleString()}
                </p>
              </div>
            </div>
            {user?.role === 'Bidder' && auction.status === 'Live' ? (
              <BidForm
                auction={auction}
                onPlaceBid={handlePlaceBid}
                loading={bidLoading}
              />
            ) : (
              <div className="alert alert-info mt-4">
                {!user
                  ? 'Log in as a bidder to place a bid.'
                  : user.role !== 'Bidder'
                    ? 'Only bidders can place bids.'
                    : auction.status === 'Scheduled'
                      ? `Bidding opens ${parseAuctionDate(auction.startTime).toLocaleString()}.`
                      : 'Bidding is closed for this auction.'}
                {!user && (
                  <button
                    type="button"
                    className="btn btn-link p-0 ms-1"
                    onClick={() => navigate('/login')}
                  >
                    Log in
                  </button>
                )}
              </div>
            )}
          </div>
          <div className="col-md-4">
            <div className="card">
              <div className="card-body">
                <h5 className="card-title">Bid History</h5>
                <BidHistory bids={bids} />
              </div>
            </div>
          </div>
        </div>
      ) : (
        <div className="text-center py-5">Auction not found.</div>
      )}
    </div>
  );
};

export default AuctionDetails;
