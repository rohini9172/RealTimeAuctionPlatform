import api from './api';

export const getWatchlist = async () => {
  const response = await api.get('/api/watchlist');
  return response.data;
};

export const addToWatchlist = async (auctionId) => {
  const response = await api.post(`/api/watchlist/${auctionId}`);
  return response.data;
};

export const removeFromWatchlist = async (auctionId) => {
  const response = await api.delete(`/api/watchlist/${auctionId}`);
  return response.data;
};
