import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { createAuction, toUtcAuctionDateTime } from '../services/auctionService';
import { getCategories } from '../services/categoryService';
import { useAuth } from '../context/AuthContext';
import AuctionForm from '../components/auctions/AuctionForm';
import Loading from '../components/common/Loading';
import ErrorMessage from '../components/common/ErrorMessage';

const CreateAuction = () => {
  const { user } = useAuth();
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitLoading, setSubmitLoading] = useState(false);
  const [error, setError] = useState('');
  const [auction, setAuction] = useState({});
  const navigate = useNavigate();

  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    try {
      const data = await getCategories();
      setCategories(data);
    } catch (err) {
      console.error('Failed to load categories', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSubmitLoading(true);
    try {
      const formData = new FormData(e.target);
      const auctionData = new FormData();
      ['title', 'description', 'categoryId', 'startingPrice', 'minIncrement']
        .forEach((field) => auctionData.append(field, formData.get(field)));
      ['startTime', 'endTime'].forEach((field) => {
        auctionData.append(field, toUtcAuctionDateTime(formData.get(field)));
      });
      formData.getAll('images').forEach((image) => auctionData.append('images', image));
      await createAuction(auctionData);
      navigate('/my-auctions');
    } catch (err) {
      setError(
        err.response?.data?.detail ||
          Object.values(err.response?.data?.errors || {}).flat().join(' ') ||
          'Failed to create auction.'
      );
    } finally {
      setSubmitLoading(false);
    }
  };

  if (!user || user.role !== 'Seller') {
    return <div className="text-center py-5">Access denied. Seller role required.</div>;
  }

  if (loading) return <Loading />;

  return (
    <div className="container mt-4">
      {error && <ErrorMessage message={error} />}
      <AuctionForm
        categories={categories}
        auction={auction}
        onSubmit={handleSubmit}
        loading={submitLoading}
      />
    </div>
  );
};

export default CreateAuction;
