import api from './api';

export const placeBid = async (auctionId, bidAmount) => {
  const response = await api.post(`/api/auctions/${auctionId}/bids`, {
    amount: bidAmount,
  });
  return response.data;
};

export const getAuctionBids = async (auctionId) => {
  const response = await api.get(`/api/auctions/${auctionId}/bids`);
  return response.data;
};

export const getMyBids = async () => {
  const response = await api.get('/api/bids/my');
  return response.data;
};
