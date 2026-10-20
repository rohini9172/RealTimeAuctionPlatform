import React from 'react';

const BidHistory = ({ bids }) => {
  if (!bids || bids.length === 0) {
    return <div className="text-center py-3">No bids yet.</div>;
  }

  return (
    <table className="table table-striped table-hover">
      <thead>
        <tr>
          <th>Bidder</th>
          <th>Amount</th>
          <th>Time</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {bids.map((bid) => (
          <tr key={bid.id}>
            <td>{bid.bidderName}</td>
            <td>₹{bid.amount.toFixed(2)}</td>
            <td>{new Date(bid.bidTimeUtc).toLocaleString()}</td>
            <td>
              {bid.isWinningBid ? (
                <span className="badge bg-success">Winning</span>
              ) : (
                <span className="badge bg-secondary">Outbid</span>
              )}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default BidHistory;
