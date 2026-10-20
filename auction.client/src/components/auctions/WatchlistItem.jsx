import React from 'react';
import { Link } from 'react-router-dom';
import { getAuctionImageUrl } from '../../services/auctionService';

const WatchlistItem = ({ item }) => {
  const statusColor = {
    Scheduled: 'warning',
    Live: 'success',
    Ended: 'secondary',
    Cancelled: 'danger',
  }[item.auctionStatus] || 'dark';

  const imageUrl = getAuctionImageUrl(item.thumbnailUrl) || 'https://via.placeholder.com/300x200';

  return (
    <div className="card h-100">
      <img src={imageUrl} className="auction-image card-img-top" alt={item.auctionTitle} />
      <div className="card-body d-flex flex-column">
        <Link to={`/auction/${item.auctionId}`}>
          <h5 className="card-title">{item.auctionTitle}</h5>
        </Link>
        <div className="d-flex justify-content-between align-items-center mb-2">
          <span className={`badge bg-${statusColor}`}>{item.auctionStatus}</span>
          <span className="fw-bold text-primary">₹{item.currentPrice.toFixed(2)}</span>
        </div>
        <div className="mb-2">
          <small className="text-muted d-flex align-items-center">
            <i className="bi bi-calendar me-1"></i>
            Ends: {new Date(item.endTime).toLocaleString()}
          </small>
        </div>
        <div className="d-grid gap-2 mt-auto">
          <Link
            to={`/auction/${item.auctionId}`}
            className="btn btn-primary w-100"
          >
            View Details
          </Link>
        </div>
      </div>
    </div>
  );
};

export default WatchlistItem;
