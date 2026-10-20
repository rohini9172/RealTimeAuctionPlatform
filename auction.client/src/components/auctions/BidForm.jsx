import React from 'react';

const BidForm = ({ auction, onPlaceBid, loading }) => {
  const minimumBid = auction.highestBidderName
    ? auction.currentPrice + auction.minIncrement
    : auction.startingPrice;

  return (
    <div className="card mt-4">
      <div className="card-body">
        <h5 className="card-title mb-4">Place Your Bid</h5>
        <form
          onSubmit={(event) => {
            event.preventDefault();
            const amount = Number(new FormData(event.currentTarget).get('amount'));
            if (Number.isFinite(amount) && amount >= minimumBid) {
              onPlaceBid(amount);
            }
          }}
        >
          <div className="mb-3">
            <label className="form-label" htmlFor="bidAmount">Your Bid Amount</label>
            <div className="input-group">
              <span className="input-group-text">₹</span>
              <input
                type="number"
                className="form-control"
                id="bidAmount"
                name="amount"
                step="0.01"
                min={minimumBid}
                placeholder="Enter bid amount"
                required
              />
            </div>
            <div className="form-text">
              Minimum bid: ₹{minimumBid.toFixed(2)}
            </div>
          </div>
          <button type="submit" className="btn btn-success" disabled={loading}>
            {loading ? 'Placing Bid...' : 'Place Bid'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default BidForm;
