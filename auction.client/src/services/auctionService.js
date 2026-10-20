import api, { API_BASE_URL } from './api';

export const getAuctions = async (params = {}) => {
  const response = await api.get('/api/auctions', { params });
  return response.data;
};

export const getAuctionById = async (id) => {
  const response = await api.get(`/api/auctions/${id}`);
  return response.data;
};

export const createAuction = async (auctionData) => {
  const response = await api.post('/api/auctions/with-images', auctionData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const getAuctionImageUrl = (imageUrl) => {
  if (!imageUrl) return null;
  if (/^https?:\/\//i.test(imageUrl)) return imageUrl;
  return `${API_BASE_URL}${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`;
};

export const parseAuctionDate = (value) => {
  if (!value) return new Date(Number.NaN);
  const utcValue = /(?:Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`;
  return new Date(utcValue);
};

export const formatAuctionDateTimeLocal = (value) => {
  if (!value) return '';
  const date = parseAuctionDate(value);
  const pad = (part) => String(part).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

export const toUtcAuctionDateTime = (value) => new Date(value).toISOString();

export const updateAuction = async (id, auctionData) => {
  const response = await api.put(`/api/auctions/${id}`, auctionData);
  return response.data;
};

export const cancelAuction = async (id) => {
  const response = await api.post(`/api/auctions/${id}/cancel`);
  return response.data;
};

export const getMyAuctions = async () => {
  const response = await api.get('/api/auctions/my');
  return response.data;
};
