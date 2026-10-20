import React, { useState, useEffect } from 'react';
import { getAuctions } from '../services/auctionService';
import { getCategories } from '../services/categoryService';
import AuctionList from '../components/auctions/AuctionList';
import Loading from '../components/common/Loading';
import ErrorMessage from '../components/common/ErrorMessage';

const AuctionListing = () => {
  const [auctions, setAuctions] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    keyword: '',
    categoryId: '',
    status: '',
    minPrice: '',
    maxPrice: '',
    page: 1,
    pageSize: 10,
  });

  useEffect(() => {
    loadCategories();
    loadAuctions();
  }, [filters.categoryId, filters.status, filters.minPrice, filters.maxPrice, filters.keyword, filters.page]);

  const loadCategories = async () => {
    try {
      const data = await getCategories();
      setCategories(data);
    } catch (err) {
      console.error('Failed to load categories', err);
      setError('Failed to load categories.');
    }
  };

  const loadAuctions = async () => {
    try {
      setLoading(true);
      setError(null);
      const params = {
        ...filters,
        page: filters.page,
        pageSize: filters.pageSize,
      };
      // Remove empty values
      Object.keys(params).forEach(
        (key) => params[key] === '' && delete params[key]
      );
      const data = await getAuctions(params);
      setAuctions(data.items || data);
    } catch (err) {
      setError(err.response?.data?.detail || 'Failed to load auctions.');
      console.error('Failed to load auctions', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <Loading />;

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Available Auctions</h2>
      </div>
      {error && <ErrorMessage message={error} />}
      <div className="row mb-4">
        <div className="col-md-3">
          <input
            type="text"
            className="form-control"
            placeholder="Search by title"
            value={filters.keyword}
            onChange={(e) =>
              setFilters({ ...filters, keyword: e.target.value, page: 1 })
            }
          />
        </div>
        <div className="col-md-3">
          <select
            className="form-select"
            value={filters.categoryId}
            onChange={(e) =>
              setFilters({ ...filters, categoryId: e.target.value, page: 1 })
            }
          >
            <option value="">All Categories</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
        <div className="col-md-3">
          <select
            className="form-select"
            value={filters.status}
            onChange={(e) =>
              setFilters({ ...filters, status: e.target.value, page: 1 })
            }
          >
            <option value="">All Status</option>
            <option value="Scheduled">Scheduled</option>
            <option value="Live">Live</option>
            <option value="Ended">Ended</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
        <div className="col-md-3">
          <input
            type="number"
            className="form-control"
            placeholder="Min Price"
            value={filters.minPrice}
            onChange={(e) =>
              setFilters({ ...filters, minPrice: e.target.value, page: 1 })
            }
          />
        </div>
      </div>
      {auctions.length === 0 && !error ? (
        <div className="text-center py-5">
          <div className="text-muted">No auctions found.</div>
        </div>
      ) : (
        <AuctionList auctions={auctions} />
      )}
    </div>
  );
};

export default AuctionListing;
