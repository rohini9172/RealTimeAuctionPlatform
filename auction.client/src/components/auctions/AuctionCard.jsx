import React from 'react';
import { Link } from 'react-router-dom';
import { getAuctionImageUrl, parseAuctionDate } from '../../services/auctionService';

const AuctionCard = ({ auction, actions }) => {
  const statusColor = {
    Scheduled: 'warning',
    Live: 'success',
    Ended: 'secondary',
    Cancelled: 'danger',
  }[auction.status] || 'dark';

  const imageUrl = getAuctionImageUrl(auction.thumbnailUrl) || 'https://via.placeholder.com/300x200';

  return (
    <div className="card h-100">
      <img src={imageUrl} className="auction-image card-img-top" alt={auction.title} />
      <div className="card-body d-flex flex-column">
        <Link to={`/auction/${auction.id}`}>
          <h5 className="card-title">{auction.title}</h5>
        </Link>
        <p className="card-text text-truncate flex-grow-1">{auction.description}</p>
        <div className="d-flex justify-content-between align-items-center mb-2">
          <span className={`badge bg-${statusColor}`}>{auction.status}</span>
          <span className="fw-bold text-primary">₹{auction.currentPrice.toFixed(2)}</span>
        </div>
        <div className="mb-2">
          <small className="text-muted d-flex align-items-center">
            <i className="bi bi-tag me-1"></i> {auction.categoryName}
          </small>
        </div>
        <div className="mb-2">
          <small className="text-muted d-flex align-items-center">
            <i className="bi bi-calendar me-1"></i>
            Ends: {parseAuctionDate(auction.endTime).toLocaleString()}
          </small>
        </div>
        <div className="mb-2">
          <small className="text-muted d-flex align-items-center">
            <i className="bi bi-person me-1"></i> {auction.sellerName}
          </small>
        </div>
        <Link to={`/auction/${auction.id}`} className="btn btn-primary w-100 mt-auto">
          View Details
        </Link>
        {actions}
      </div>
    </div>
  );
};

export default AuctionCard;
