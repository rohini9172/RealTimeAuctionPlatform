import React from 'react';
import AuctionCard from './AuctionCard';

const AuctionList = ({ auctions, renderActions }) => {
  if (!auctions || auctions.length === 0) {
    return (
      <div className="empty-state">
        <i className="bi bi-gavel fs-1 mb-3"></i>
        <h4>No auctions found.</h4>
        <p className="text-muted">Check back later for new auctions.</p>
      </div>
    );
  }

  return (
    <div className="auction-grid">
      {auctions.map((auction) => (
        <div key={auction.id}>
          <AuctionCard
            auction={auction}
            actions={renderActions?.(auction)}
          />
        </div>
      ))}
    </div>
  );
};

export default AuctionList;
